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

        if (IsGreeting(question))
        {
            return "Hey! I am here to help with Mini Jira. You can ask me where to find projects, epics, tasks, comments, or anything else in the app.";
        }

        var requestUrl = $"{chatbotOptions.BaseUrl.TrimEnd('/')}/chat/completions";
        var request = new
        {
            model = chatbotOptions.Model,
            temperature = 0.2,
            max_tokens = chatbotOptions.MaxTokens,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = "You are a friendly Mini Jira in-app assistant for normal website users. Use only the provided context for Mini Jira questions. Return only the final answer. Give helpful answers in 1 to 3 short sentences. If the user greets you or chats casually, respond naturally and briefly, then offer help with Mini Jira. Explain where to click and what each page does. Never mention API endpoints, backend routes, HTTP methods, database fields, ids, tokens, DTOs, code, or server implementation details. Do not include reasoning. If you do not know, say so."
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
        var message = completion?.Choices?.FirstOrDefault()?.Message;
        var answer = ExtractFinalAnswer(message?.Content);

        if (!string.IsNullOrWhiteSpace(answer))
        {
            return answer;
        }

        var fallbackAnswer = ExtractFinalAnswer(message?.ReasoningContent);

        return string.IsNullOrWhiteSpace(fallbackAnswer)
            ? "I could not generate a clear answer right now. Please try asking again in a shorter way."
            : fallbackAnswer;
    }

    private static string? ExtractFinalAnswer(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var answer = text.Trim();
        var markers = new[]
        {
            "Final Answer Construction:",
            "Final Answer:",
            "Draft Response:"
        };

        var foundReasoningMarker = false;
        foreach (var marker in markers)
        {
            var markerIndex = answer.LastIndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex >= 0)
            {
                answer = answer[(markerIndex + marker.Length)..].Trim();
                foundReasoningMarker = true;
                break;
            }
        }

        if (foundReasoningMarker)
        {
            answer = RemoveNumberedReasoningLines(answer);
        }

        var firstEmptyLine = answer.IndexOf("\n\n", StringComparison.Ordinal);
        if (firstEmptyLine >= 0)
        {
            answer = answer[..firstEmptyLine].Trim();
        }

        return string.IsNullOrWhiteSpace(answer) ? null : answer;
    }

    private static string RemoveNumberedReasoningLines(string answer)
    {
        var lines = answer
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Where(line => !char.IsDigit(line[0]))
            .ToArray();

        return lines.Length == 0 ? answer : string.Join(' ', lines);
    }

    private static bool IsGreeting(string question)
    {
        var normalizedQuestion = question.Trim().ToLowerInvariant();
        var greetings = new[]
        {
            "hi",
            "hey",
            "hello",
            "yo",
            "whats up",
            "what's up",
            "sup",
            "hey whats up",
            "hey what's up"
        };

        return greetings.Contains(normalizedQuestion);
    }

    private sealed record LmStudioChatCompletionResponse(LmStudioChoice[]? Choices);

    private sealed record LmStudioChoice(LmStudioMessage? Message);

    private sealed record LmStudioMessage(string? Content, string? ReasoningContent);
}
