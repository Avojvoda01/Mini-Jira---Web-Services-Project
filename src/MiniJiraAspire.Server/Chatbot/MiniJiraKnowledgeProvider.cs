using System.Text;

namespace MiniJiraAspire.Server.Chatbot;

public class MiniJiraKnowledgeProvider(IWebHostEnvironment environment) : IMiniJiraKnowledgeProvider
{
    public async Task<string> GetKnowledgeAsync(CancellationToken cancellationToken = default)
    {
        var knowledgeDirectory = Path.Combine(environment.ContentRootPath, "Chatbot", "Knowledge");

        if (!Directory.Exists(knowledgeDirectory))
        {
            return "No Mini Jira knowledge files were found.";
        }

        var knowledge = new StringBuilder();
        var files = Directory.GetFiles(knowledgeDirectory, "*.txt")
            .OrderBy(file => file);

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
}
