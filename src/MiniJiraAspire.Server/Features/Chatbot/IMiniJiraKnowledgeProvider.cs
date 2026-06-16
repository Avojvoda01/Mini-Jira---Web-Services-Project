namespace MiniJiraAspire.Server.Features.Chatbot;

public interface IMiniJiraKnowledgeProvider
{
    Task<string> GetKnowledgeAsync(string question, CancellationToken cancellationToken = default);
}
