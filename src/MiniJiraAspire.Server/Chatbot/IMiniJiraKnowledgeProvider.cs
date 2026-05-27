namespace MiniJiraAspire.Server.Chatbot;

public interface IMiniJiraKnowledgeProvider
{
    Task<string> GetKnowledgeAsync(CancellationToken cancellationToken = default);
}
