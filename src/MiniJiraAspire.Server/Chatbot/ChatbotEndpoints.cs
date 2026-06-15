using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Chatbot;

public static class ChatbotEndpoints
{
    public static void MapChatbotEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/chats", AskChatbot)
            .WithName("AskChatbot")
            .WithTags("Chatbot")
            .WithSummary("Ask the Mini Jira chatbot a question")
            .RequireAuthorization();
    }

    private static async Task<Results<Ok<ChatResponse>, BadRequest<ChatResponse>, ProblemHttpResult>> AskChatbot(
        ChatRequest request,
        HttpContext httpContext,
        IProjectRepository projectRepository,
        ITaskRepository taskRepository,
        IEpicRepository epicRepository,
        IUserRepository userRepository,
        IMiniJiraKnowledgeProvider knowledgeProvider,
        ILmStudioChatClient lmStudioChatClient,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return TypedResults.BadRequest(new ChatResponse("Please enter a question."));
        }

        try
        {
            var question = request.Message.Trim();
            var userId = GetUserId(httpContext.User);
            if (userId is null)
            {
                return TypedResults.Ok(new ChatResponse("Please sign in first, then I can help with Mini Jira."));
            }

            var visibleProjects = await GetVisibleProjectsAsync(userId.Value, httpContext.User, projectRepository, cancellationToken);
            var visibleProjectIds = visibleProjects.Select(project => project.Id).ToHashSet();
            var visibleEpics = await GetVisibleEpicsAsync(visibleProjectIds, epicRepository, cancellationToken);

            var intent = await lmStudioChatClient.ClassifyIntentAsync(
                question,
                visibleProjects.Select(project => project.Name).ToArray(),
                visibleEpics.Select(epic => epic.Name).ToArray(),
                cancellationToken);

            if (intent.Intent.Equals("general_help", StringComparison.OrdinalIgnoreCase))
            {
                var knowledge = await knowledgeProvider.GetKnowledgeAsync(question, cancellationToken);
                var knowledgeAnswer = await lmStudioChatClient.AskAsync(knowledge, question, cancellationToken);
                return TypedResults.Ok(new ChatResponse(knowledgeAnswer));
            }

            var liveDataContext = await AnswerFromIntentAsync(
                intent,
                userId.Value,
                visibleProjects,
                visibleProjectIds,
                visibleEpics,
                projectRepository,
                taskRepository,
                epicRepository,
                userRepository,
                cancellationToken);
            var answer = await lmStudioChatClient.AnswerFromLiveDataAsync(question, liveDataContext, cancellationToken);

            return TypedResults.Ok(new ChatResponse(answer));
        }
        catch (HttpRequestException)
        {
            return TypedResults.Problem(
                "The chatbot could not reach LM Studio. Make sure LM Studio is running with the local server enabled.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    private static async Task<string> AnswerFromIntentAsync(
        ChatbotIntent intent,
        Guid userId,
        List<Project> visibleProjects,
        HashSet<Guid> visibleProjectIds,
        List<Epic> visibleEpics,
        IProjectRepository projectRepository,
        ITaskRepository taskRepository,
        IEpicRepository epicRepository,
        IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        return intent.Intent.Trim().ToLowerInvariant() switch
        {
            "get_my_projects" => AnswerProjects(visibleProjects),
            "get_my_tasks" => await AnswerMyTasksAsync(intent, userId, projectRepository, taskRepository, cancellationToken),
            "newest_tasks" => await AnswerNewestTasksAsync(visibleProjectIds, taskRepository, cancellationToken),
            "search_tasks" => await AnswerTaskSearchAsync(intent, visibleProjectIds, taskRepository, cancellationToken),
            "project_task_filter" => await AnswerProjectTasksAsync(intent, visibleProjects, taskRepository, cancellationToken),
            "project_people" => await AnswerProjectPeopleAsync(intent, visibleProjects, taskRepository, userRepository, cancellationToken),
            "project_changes" => await AnswerProjectChangesAsync(intent, visibleProjects, taskRepository, cancellationToken),
            "prioritize_my_tasks" => await AnswerPrioritySuggestionAsync(userId, taskRepository, cancellationToken),
            "list_project_epics" => AnswerProjectEpics(intent, visibleProjects, visibleEpics),
            "summarize_project" => await AnswerProjectSummaryAsync(intent, visibleProjects, taskRepository, epicRepository, cancellationToken),
            "summarize_epic_status" => await AnswerEpicSummaryAsync(intent, visibleEpics, taskRepository, cancellationToken),
            _ => "I am not sure which Mini Jira action to use for that. Try asking about your projects, tasks, epics, or a project summary."
        };
    }

    private static string AnswerProjects(List<Project> projects)
    {
        if (projects.Count == 0)
        {
            return "You do not have any projects assigned right now.";
        }

        return projects.Count == 1
            ? $"You have 1 project: {projects[0].Name}."
            : $"You have {projects.Count} projects: {string.Join(", ", projects.Select(project => project.Name))}.";
    }

    private static async Task<string> AnswerMyTasksAsync(
        ChatbotIntent intent,
        Guid userId,
        IProjectRepository projectRepository,
        ITaskRepository taskRepository,
        CancellationToken cancellationToken)
    {
        var tasks = await taskRepository.GetAllAsync(null, intent.Status, intent.Priority, userId.ToString(), null, null, cancellationToken);
        var projects = await projectRepository.GetAllAsync(cancellationToken);
        var projectsById = projects.ToDictionary(project => project.Id);

        return tasks.Count == 0
            ? "You do not have any matching tasks assigned to you right now."
            : $"You have {tasks.Count} matching task{Plural(tasks.Count)}: {FormatAssignedTaskList(tasks.Take(8), projectsById)}.";
    }

    private static async Task<string> AnswerNewestTasksAsync(
        HashSet<Guid> visibleProjectIds,
        ITaskRepository taskRepository,
        CancellationToken cancellationToken)
    {
        var tasks = await taskRepository.GetAllAsync(null, null, null, null, null, null, cancellationToken);
        tasks = tasks
            .Where(task => visibleProjectIds.Contains(task.ProjectId))
            .OrderByDescending(task => task.CreatedAtUtc)
            .Take(8)
            .ToList();

        return tasks.Count == 0
            ? "I could not find any tasks in your projects."
            : $"Newest tasks in your projects: {FormatTaskList(tasks)}.";
    }

    private static async Task<string> AnswerTaskSearchAsync(
        ChatbotIntent intent,
        HashSet<Guid> visibleProjectIds,
        ITaskRepository taskRepository,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(intent.Search))
        {
            return "What task keyword should I search for?";
        }

        var tasks = await taskRepository.GetAllAsync(intent.Search, null, null, null, null, null, cancellationToken);
        tasks = tasks.Where(task => visibleProjectIds.Contains(task.ProjectId)).ToList();

        return tasks.Count == 0
            ? $"I could not find tasks related to {intent.Search} in your projects."
            : $"I found {tasks.Count} task{Plural(tasks.Count)} related to {intent.Search}: {FormatTaskList(tasks.Take(8))}.";
    }

    private static async Task<string> AnswerProjectTasksAsync(
        ChatbotIntent intent,
        List<Project> visibleProjects,
        ITaskRepository taskRepository,
        CancellationToken cancellationToken)
    {
        var project = FindProject(intent.ProjectName, visibleProjects);
        if (project is null)
        {
            return MissingProjectAnswer(intent.ProjectName, visibleProjects);
        }

        var tasks = await taskRepository.GetAllAsync(intent.Search, intent.Status, intent.Priority, null, null, project.Id.ToString(), cancellationToken);
        return tasks.Count == 0
            ? $"I could not find matching tasks in {project.Name}."
            : $"{project.Name} has {tasks.Count} matching task{Plural(tasks.Count)}: {FormatTaskList(tasks.Take(8))}.";
    }

    private static async Task<string> AnswerProjectPeopleAsync(
        ChatbotIntent intent,
        List<Project> visibleProjects,
        ITaskRepository taskRepository,
        IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        var project = FindProject(intent.ProjectName, visibleProjects);
        if (project is null)
        {
            return MissingProjectAnswer(intent.ProjectName, visibleProjects);
        }

        var users = await userRepository.GetAllAsync(cancellationToken);
        var usersById = users.ToDictionary(projectUser => projectUser.Id);
        var tasks = await taskRepository.GetAllAsync(null, null, null, null, null, project.Id.ToString(), cancellationToken);
        var memberSummaries = project.Members.Select(member =>
        {
            var displayName = usersById.TryGetValue(member.UserId, out var projectUser)
                ? projectUser.DisplayName
                : "Unknown member";
            var count = tasks.Count(task => task.AssigneeId == member.UserId);
            return $"{displayName}: {count} task{Plural(count)}";
        });

        return $"{project.Name} has {project.Members.Count} member{Plural(project.Members.Count)}. {string.Join("; ", memberSummaries)}.";
    }

    private static async Task<string> AnswerProjectChangesAsync(
        ChatbotIntent intent,
        List<Project> visibleProjects,
        ITaskRepository taskRepository,
        CancellationToken cancellationToken)
    {
        var project = FindProject(intent.ProjectName, visibleProjects);
        if (project is null)
        {
            return MissingProjectAnswer(intent.ProjectName, visibleProjects);
        }

        var sinceUtc = DateTime.UtcNow.AddDays(-7);
        var tasks = await taskRepository.GetAllAsync(null, null, null, null, null, project.Id.ToString(), cancellationToken);
        var changedTasks = tasks
            .Where(task => (task.UpdatedAtUtc ?? task.CreatedAtUtc) >= sinceUtc)
            .OrderByDescending(task => task.UpdatedAtUtc ?? task.CreatedAtUtc)
            .Take(8)
            .ToList();

        return changedTasks.Count == 0
            ? $"I could not find task changes in {project.Name} during the last 7 days."
            : $"Recent changes in {project.Name}: {FormatTaskList(changedTasks)}.";
    }

    private static async Task<string> AnswerPrioritySuggestionAsync(
        Guid userId,
        ITaskRepository taskRepository,
        CancellationToken cancellationToken)
    {
        var tasks = await taskRepository.GetAllAsync(null, null, null, userId.ToString(), null, null, cancellationToken);
        var candidates = tasks
            .Where(task => !task.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            .OrderBy(task => PriorityRank(task.Priority))
            .ThenBy(task => StatusRank(task.Status))
            .ThenBy(task => task.CreatedAtUtc)
            .Take(3)
            .ToList();

        return candidates.Count == 0
            ? "You do not have any unfinished assigned tasks right now."
            : $"I would start with: {FormatTaskList(candidates)}.";
    }

    private static string AnswerProjectEpics(
        ChatbotIntent intent,
        List<Project> visibleProjects,
        List<Epic> visibleEpics)
    {
        var project = FindProject(intent.ProjectName, visibleProjects);
        if (project is null)
        {
            return MissingProjectAnswer(intent.ProjectName, visibleProjects);
        }

        var epics = visibleEpics.Where(epic => epic.ProjectId == project.Id).ToList();
        return epics.Count == 0
            ? $"{project.Name} does not have any epics yet."
            : $"{project.Name} has {epics.Count} epic{Plural(epics.Count)}: {string.Join(", ", epics.Select(epic => epic.Name))}.";
    }

    private static async Task<string> AnswerProjectSummaryAsync(
        ChatbotIntent intent,
        List<Project> visibleProjects,
        ITaskRepository taskRepository,
        IEpicRepository epicRepository,
        CancellationToken cancellationToken)
    {
        var project = FindProject(intent.ProjectName, visibleProjects);
        if (project is null)
        {
            return MissingProjectAnswer(intent.ProjectName, visibleProjects);
        }

        var tasks = await taskRepository.GetAllAsync(null, null, null, null, null, project.Id.ToString(), cancellationToken);
        var epics = await epicRepository.GetAllAsync(project.Id, cancellationToken);
        var highPriorityCount = tasks.Count(task => task.Priority.Equals("High", StringComparison.OrdinalIgnoreCase));
        var highPriorityText = highPriorityCount > 0
            ? $" It has {highPriorityCount} high-priority task{Plural(highPriorityCount)}."
            : string.Empty;

        return $"{project.Name}: {project.Description} It has {project.Members.Count} member{Plural(project.Members.Count)}, {epics.Count} epic{Plural(epics.Count)}, and {tasks.Count} task{Plural(tasks.Count)}{FormatStatusSummary(tasks)}.{highPriorityText}";
    }

    private static async Task<string> AnswerEpicSummaryAsync(
        ChatbotIntent intent,
        List<Epic> visibleEpics,
        ITaskRepository taskRepository,
        CancellationToken cancellationToken)
    {
        var epic = FindEpic(intent.EpicName, visibleEpics);
        if (epic is null)
        {
            return MissingEpicAnswer(intent.EpicName, visibleEpics);
        }

        var tasks = await taskRepository.GetAllAsync(null, null, null, null, epic.Id.ToString(), null, cancellationToken);
        var highPriorityCount = tasks.Count(task => task.Priority.Equals("High", StringComparison.OrdinalIgnoreCase));
        var highPriorityText = highPriorityCount > 0
            ? $" It also has {highPriorityCount} high-priority task{Plural(highPriorityCount)}."
            : string.Empty;

        return $"{epic.Name} has {tasks.Count} task{Plural(tasks.Count)}{FormatStatusSummary(tasks)}.{highPriorityText}";
    }

    private static async Task<List<Project>> GetVisibleProjectsAsync(
        Guid userId,
        ClaimsPrincipal user,
        IProjectRepository projectRepository,
        CancellationToken cancellationToken)
    {
        var projects = await projectRepository.GetAllAsync(cancellationToken);

        return user.IsInRole("Admin")
            ? projects
            : projects
                .Where(project => project.CreatedById == userId || project.Members.Any(member => member.UserId == userId))
                .ToList();
    }

    private static async Task<List<Epic>> GetVisibleEpicsAsync(
        HashSet<Guid> visibleProjectIds,
        IEpicRepository epicRepository,
        CancellationToken cancellationToken)
    {
        var epics = await epicRepository.GetAllAsync(null, cancellationToken);
        return epics.Where(epic => visibleProjectIds.Contains(epic.ProjectId)).ToList();
    }

    private static Project? FindProject(string? projectName, List<Project> visibleProjects)
    {
        if (visibleProjects.Count == 1 && string.IsNullOrWhiteSpace(projectName))
        {
            return visibleProjects[0];
        }

        if (string.IsNullOrWhiteSpace(projectName))
        {
            return null;
        }

        var normalizedProjectName = NormalizeName(projectName);
        return visibleProjects.FirstOrDefault(project => NormalizeName(project.Name) == normalizedProjectName)
            ?? visibleProjects.FirstOrDefault(project => NormalizeName(project.Name).Contains(normalizedProjectName))
            ?? visibleProjects.FirstOrDefault(project => normalizedProjectName.Contains(NormalizeName(project.Name)));
    }

    private static Epic? FindEpic(string? epicName, List<Epic> visibleEpics)
    {
        if (visibleEpics.Count == 1 && string.IsNullOrWhiteSpace(epicName))
        {
            return visibleEpics[0];
        }

        if (string.IsNullOrWhiteSpace(epicName))
        {
            return null;
        }

        var normalizedEpicName = NormalizeName(epicName);
        return visibleEpics.FirstOrDefault(epic => NormalizeName(epic.Name) == normalizedEpicName)
            ?? visibleEpics.FirstOrDefault(epic => NormalizeName(epic.Name).Contains(normalizedEpicName))
            ?? visibleEpics.FirstOrDefault(epic => normalizedEpicName.Contains(NormalizeName(epic.Name)));
    }

    private static string MissingProjectAnswer(string? requestedProjectName, List<Project> visibleProjects)
    {
        if (visibleProjects.Count == 0)
        {
            return "You do not have any visible projects right now.";
        }

        var visibleNames = string.Join(", ", visibleProjects.Select(project => project.Name));
        return string.IsNullOrWhiteSpace(requestedProjectName)
            ? $"Which project should I use? Your visible projects are: {visibleNames}."
            : $"I could not find {requestedProjectName} in your visible projects. Your visible projects are: {visibleNames}.";
    }

    private static string MissingEpicAnswer(string? requestedEpicName, List<Epic> visibleEpics)
    {
        if (visibleEpics.Count == 0)
        {
            return "I could not find any epics in your visible projects right now.";
        }

        var visibleNames = string.Join(", ", visibleEpics.Select(epic => epic.Name));
        return string.IsNullOrWhiteSpace(requestedEpicName)
            ? $"Which epic should I use? Your visible epics are: {visibleNames}."
            : $"I could not find {requestedEpicName} in your visible epics. Your visible epics are: {visibleNames}.";
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
    }

    private static string NormalizeName(string value)
        => value
            .Replace("?", string.Empty)
            .Replace(".", string.Empty)
            .Replace(",", string.Empty)
            .Replace("\"", string.Empty)
            .Replace("'", string.Empty)
            .Replace(" project", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim()
            .ToLowerInvariant();

    private static string FormatStatusSummary(List<TaskItem> tasks)
    {
        if (tasks.Count == 0)
        {
            return string.Empty;
        }

        var byStatus = tasks
            .GroupBy(task => task.Status)
            .OrderBy(group => group.Key)
            .Select(group => $"{group.Count()} {group.Key}");

        return $" ({string.Join(", ", byStatus)})";
    }

    private static string FormatTaskList(IEnumerable<TaskItem> tasks)
        => string.Join("; ", tasks.Select(task => $"{task.Title} ({task.Status}, {task.Priority})"));

    private static string FormatAssignedTaskList(IEnumerable<TaskItem> tasks, Dictionary<Guid, Project> projectsById)
        => string.Join("; ", tasks.Select(task =>
        {
            var projectName = projectsById.TryGetValue(task.ProjectId, out var project)
                ? project.Name
                : "Unknown project";

            return $"Project: {projectName}, Task: {task.Title}, Status: {task.Status}, Priority: {task.Priority}";
        }));

    private static int PriorityRank(string priority)
        => priority.ToLowerInvariant() switch
        {
            "high" => 0,
            "medium" => 1,
            "low" => 2,
            _ => 3
        };

    private static int StatusRank(string status)
        => status.ToLowerInvariant() switch
        {
            "ready" => 0,
            "open" => 0,
            "in progress" => 1,
            "review" => 2,
            "completed" => 3,
            _ => 4
        };

    private static string Plural(int count)
        => count == 1 ? string.Empty : "s";
}
