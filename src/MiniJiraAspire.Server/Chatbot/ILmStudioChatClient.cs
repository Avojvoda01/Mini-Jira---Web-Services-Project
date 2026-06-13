namespace MiniJiraAspire.Server.Chatbot;

public interface ILmStudioChatClient
{
    Task<string> AskAsync(string miniJiraContext, string question, CancellationToken cancellationToken = default);

    Task<string> AnswerFromLiveDataAsync(string question, string liveDataContext, CancellationToken cancellationToken = default);

    Task<ChatbotIntent> ClassifyIntentAsync(
        string question,
        IReadOnlyCollection<string>? visibleProjectNames = null,
        IReadOnlyCollection<string>? visibleEpicNames = null,
        CancellationToken cancellationToken = default);
}
