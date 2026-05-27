namespace MiniJiraAspire.Server.Chatbot;

public interface ILmStudioChatClient
{
    Task<string> AskAsync(string miniJiraContext, string question, CancellationToken cancellationToken = default);
}
