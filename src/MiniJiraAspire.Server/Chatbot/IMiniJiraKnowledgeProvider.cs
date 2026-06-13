namespace MiniJiraAspire.Server.Chatbot;

public interface IMiniJiraKnowledgeProvider
{
    Task<string> GetKnowledgeAsync(string question, CancellationToken cancellationToken = default);
}
