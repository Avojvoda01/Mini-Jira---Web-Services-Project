using System.Text;

namespace MiniJiraAspire.Server.Features.Chatbot;

public class MiniJiraKnowledgeProvider(IWebHostEnvironment environment) : IMiniJiraKnowledgeProvider
{
    public async Task<string> GetKnowledgeAsync(string question, CancellationToken cancellationToken = default)
    {
        var knowledgeDirectory = Path.Combine(environment.ContentRootPath, "Features", "Chatbot", "Knowledge");

        if (!Directory.Exists(knowledgeDirectory))
        {
            return "No Mini Jira knowledge files were found.";
        }

        var knowledge = new StringBuilder();
        var files = SelectKnowledgeFiles(knowledgeDirectory, question);

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var content = await File.ReadAllTextAsync(file, cancellationToken);

            knowledge.AppendLine($"# {fileName}");
            knowledge.AppendLine(content.Trim());
            knowledge.AppendLine();
        }

        return knowledge.ToString();
    }

    private static IEnumerable<string> SelectKnowledgeFiles(string knowledgeDirectory, string question)
    {
        var selectedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Path.Combine(knowledgeDirectory, "overview.txt"),
            Path.Combine(knowledgeDirectory, "ui-navigation.txt")
        };

        var normalizedQuestion = question.ToLowerInvariant();

        AddIfMatches(normalizedQuestion, selectedFiles, knowledgeDirectory, "projects.txt", "project", "projects", "member", "members");
        AddIfMatches(normalizedQuestion, selectedFiles, knowledgeDirectory, "epics.txt", "epic", "epics");
        AddIfMatches(normalizedQuestion, selectedFiles, knowledgeDirectory, "tasks.txt", "task", "tasks", "ticket", "tickets", "status", "priority", "assignee", "assign");
        AddIfMatches(normalizedQuestion, selectedFiles, knowledgeDirectory, "comments.txt", "comment", "comments", "discussion", "note", "notes");
        AddIfMatches(normalizedQuestion, selectedFiles, knowledgeDirectory, "users-admin.txt", "user", "users", "admin", "role", "roles", "login", "register", "auth");

        if (selectedFiles.Count == 2)
        {
            selectedFiles.Add(Path.Combine(knowledgeDirectory, "projects.txt"));
            selectedFiles.Add(Path.Combine(knowledgeDirectory, "tasks.txt"));
        }

        return selectedFiles
            .Where(File.Exists)
            .OrderBy(file => file);
    }

    private static void AddIfMatches(
        string normalizedQuestion,
        ISet<string> selectedFiles,
        string knowledgeDirectory,
        string fileName,
        params string[] keywords)
    {
        if (keywords.Any(normalizedQuestion.Contains))
        {
            selectedFiles.Add(Path.Combine(knowledgeDirectory, fileName));
        }
    }
}
