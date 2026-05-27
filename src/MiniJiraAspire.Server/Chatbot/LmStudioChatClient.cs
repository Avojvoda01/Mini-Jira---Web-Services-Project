using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace MiniJiraAspire.Server.Chatbot;

public class LmStudioChatClient(HttpClient httpClient, IOptions<ChatbotOptions> options) : ILmStudioChatClient
{
    public async Task<string> AskAsync(string miniJiraContext, string question, CancellationToken cancellationToken = default)
    {
        var chatbotOptions = options.Value;

        if (!chatbotOptions.Enabled)
        {
            return "The Mini Jira chatbot is currently disabled.";
        }

        var requestUrl = $"{chatbotOptions.BaseUrl.TrimEnd('/')}/chat/completions";
        var request = new
        {
            model = chatbotOptions.Model,
            temperature = 0.2,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = """
                    You are a helpful chatbot for the Mini Jira web application.
                    Answer questions about Mini Jira projects, epics, tasks, comments, users, and admin features.
                    Use only the Mini Jira context provided by the application.
                    If the context does not contain the answer, say that you do not know.
                    Keep answers simple, short, and clear.
                    Do not invent project names, user data, task data, ids, or database records.
                    """
                },
                new
                {
                    role = "user",
                    content = $"""
                    Mini Jira context:
                    {miniJiraContext}

                    User question:
                    {question}
                    """
                }
            }
        };

        var response = await httpClient.PostAsJsonAsync(requestUrl, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var completion = await response.Content.ReadFromJsonAsync<LmStudioChatCompletionResponse>(cancellationToken);
        var answer = completion?.Choices?.FirstOrDefault()?.Message?.Content;

        return string.IsNullOrWhiteSpace(answer)
            ? "I could not generate an answer from LM Studio."
            : answer.Trim();
    }

    private sealed record LmStudioChatCompletionResponse(LmStudioChoice[]? Choices);

    private sealed record LmStudioChoice(LmStudioMessage? Message);

    private sealed record LmStudioMessage(string? Content);
}
