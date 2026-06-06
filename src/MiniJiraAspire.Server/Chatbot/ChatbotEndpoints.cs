using Microsoft.AspNetCore.Http.HttpResults;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Chatbot;

public static class ChatbotEndpoints
{
    public static void MapChatbotEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/chat", AskChatbot)
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
            var liveAnswer = await TryAnswerLiveDataQuestionAsync(
                request.Message.Trim(),
                httpContext.User,
                projectRepository,
                taskRepository,
                epicRepository,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(liveAnswer))
            {
                return TypedResults.Ok(new ChatResponse(liveAnswer));
            }

            var knowledge = await knowledgeProvider.GetKnowledgeAsync(request.Message, cancellationToken);
            var answer = await lmStudioChatClient.AskAsync(knowledge, request.Message.Trim(), cancellationToken);

            return TypedResults.Ok(new ChatResponse(answer));
        }
        catch (HttpRequestException)
        {
            return TypedResults.Problem(
                "The chatbot could not reach LM Studio. Make sure LM Studio is running with the local server enabled.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    private static async Task<string?> TryAnswerLiveDataQuestionAsync(
        string question,
        ClaimsPrincipal user,
        IProjectRepository projectRepository,
        ITaskRepository taskRepository,
        IEpicRepository epicRepository,
        CancellationToken cancellationToken)
    {
        var normalizedQuestion = Normalize(question);

        if (IsMyProjectsQuestion(normalizedQuestion))
        {
            var userId = GetUserId(user);
            if (userId is null)
            {
                return "Please sign in first, then I can show you your projects.";
            }

            var projects = await GetVisibleProjectsAsync(userId.Value, user, projectRepository, cancellationToken);

            if (projects.Count == 0)
            {
                return "You do not have any projects assigned right now.";
            }

            var projectNames = string.Join(", ", projects.Select(project => project.Name));
            return projects.Count == 1
                ? $"You have 1 project: {projectNames}."
                : $"You have {projects.Count} projects: {projectNames}.";
        }

        if (IsAssignedTicketsQuestion(normalizedQuestion))
        {
            var userId = GetUserId(user);
            if (userId is null)
            {
                return "Please sign in first, then I can show the tickets assigned to you.";
            }

            var tasks = await taskRepository.GetAllAsync(
                search: null,
                status: null,
                priority: null,
                assigneeId: userId.Value.ToString(),
                epicId: null,
                projectId: null,
                cancellationToken);

            if (tasks.Count == 0)
            {
                return "You do not have any tickets assigned to you right now.";
            }

            var taskSummary = string.Join("; ", tasks.Take(8).Select(task => $"{task.Title} ({task.Status}, {task.Priority})"));
            var extraCount = tasks.Count > 8 ? $" There are {tasks.Count - 8} more tickets too." : string.Empty;
            return $"You have {tasks.Count} assigned ticket{Plural(tasks.Count)}: {taskSummary}.{extraCount}";
        }

        if (IsEpicStatusSummaryQuestion(normalizedQuestion))
        {
            var userId = GetUserId(user);
            if (userId is null)
            {
                return "Please sign in first, then I can summarize epic status for your projects.";
            }

            var epicName = ExtractEpicName(question);
            if (string.IsNullOrWhiteSpace(epicName))
            {
                return "Which epic should I summarize?";
            }

            var visibleProjects = await GetVisibleProjectsAsync(userId.Value, user, projectRepository, cancellationToken);
            var visibleProjectIds = visibleProjects.Select(project => project.Id).ToHashSet();
            var epics = await epicRepository.GetAllAsync(cancellationToken);
            var epic = epics
                .Where(epic => visibleProjectIds.Contains(epic.ProjectId))
                .FirstOrDefault(epic => epic.Name.Contains(epicName, StringComparison.OrdinalIgnoreCase));

            if (epic is null)
            {
                return $"I could not find an epic named {epicName} in your projects.";
            }

            var epicTasks = await taskRepository.GetAllAsync(
                search: null,
                status: null,
                priority: null,
                assigneeId: null,
                epicId: epic.Id.ToString(),
                projectId: null,
                cancellationToken);

            if (epicTasks.Count == 0)
            {
                return $"{epic.Name} does not have any tickets assigned to it yet.";
            }

            var byStatus = epicTasks
                .GroupBy(task => task.Status)
                .OrderBy(group => group.Key)
                .Select(group => $"{group.Count()} {group.Key}");

            var highPriorityCount = epicTasks.Count(task => task.Priority.Equals("High", StringComparison.OrdinalIgnoreCase));
            var statusSummary = string.Join(", ", byStatus);
            var prioritySentence = highPriorityCount > 0
                ? $" It also has {highPriorityCount} high-priority ticket{Plural(highPriorityCount)}."
                : string.Empty;

            return $"{epic.Name} has {epicTasks.Count} ticket{Plural(epicTasks.Count)}: {statusSummary}.{prioritySentence}";
        }

        return null;
    }

    private static async Task<List<Project>> GetVisibleProjectsAsync(
        Guid userId,
        ClaimsPrincipal user,
        IProjectRepository projectRepository,
        CancellationToken cancellationToken)
    {
        var projects = await projectRepository.GetAllAsync(cancellationToken);

        if (user.IsInRole("Admin"))
        {
            return projects;
        }

        return projects
            .Where(project => project.Members.Any(member => member.UserId == userId))
            .ToList();
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userId, out var parsedUserId)
            ? parsedUserId
            : null;
    }

    private static bool IsMyProjectsQuestion(string normalizedQuestion)
        => normalizedQuestion.Contains("project")
           && (normalizedQuestion.Contains("my")
               || normalizedQuestion.Contains("i have")
               || normalizedQuestion.Contains("i am in")
               || normalizedQuestion.Contains("i'm in")
               || normalizedQuestion.Contains("assigned to me"))
           && (normalizedQuestion.Contains("what")
               || normalizedQuestion.Contains("which")
               || normalizedQuestion.Contains("list")
               || normalizedQuestion.Contains("show")
               || normalizedQuestion.Contains("how many"));

    private static bool IsAssignedTicketsQuestion(string normalizedQuestion)
        => (normalizedQuestion.Contains("ticket") || normalizedQuestion.Contains("task"))
           && (normalizedQuestion.Contains("assigned to me") || normalizedQuestion.Contains("my assigned") || normalizedQuestion.Contains("assigned tickets") || normalizedQuestion.Contains("assigned tasks"));

    private static bool IsEpicStatusSummaryQuestion(string normalizedQuestion)
        => normalizedQuestion.Contains("summar")
           && normalizedQuestion.Contains("status")
           && normalizedQuestion.Contains("epic");

    private static string ExtractEpicName(string question)
    {
        var cleanedQuestion = question
            .Replace("?", string.Empty)
            .Replace(".", string.Empty)
            .Trim();
        var lowerQuestion = cleanedQuestion.ToLowerInvariant();
        var epicIndex = lowerQuestion.LastIndexOf(" epic", StringComparison.OrdinalIgnoreCase);

        if (epicIndex <= 0)
        {
            return string.Empty;
        }

        var beforeEpic = cleanedQuestion[..epicIndex].Trim();
        var statusOfIndex = beforeEpic.LastIndexOf("status of ", StringComparison.OrdinalIgnoreCase);

        if (statusOfIndex >= 0)
        {
            return beforeEpic[(statusOfIndex + "status of ".Length)..].Trim();
        }

        var words = beforeEpic.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Length == 0 ? string.Empty : words[^1];
    }

    private static string Normalize(string value)
        => value.ToLowerInvariant()
            .Replace("?", string.Empty)
            .Replace(".", string.Empty)
            .Replace(",", string.Empty)
            .Trim();

    private static string Plural(int count)
        => count == 1 ? string.Empty : "s";
}
