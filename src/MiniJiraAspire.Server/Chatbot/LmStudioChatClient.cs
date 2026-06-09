using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace MiniJiraAspire.Server.Chatbot;

public class LmStudioChatClient(HttpClient httpClient, IOptions<ChatbotOptions> options) : ILmStudioChatClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

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
                    content = "You are a friendly Mini Jira in-app assistant for normal website users. Speak as the assistant using first person, for example \"I can help you...\" not \"You can...\". Use only the provided context for Mini Jira questions. Return only the final answer. Give helpful answers in 1 to 3 short sentences. If the user asks what you can help with, say \"I can explain Mini Jira, help you find projects, tasks, epics, and comments, show your assigned tasks, summarize projects or epics, and suggest what to work on first.\" If the user greets you or chats casually, respond naturally and briefly, then offer help with Mini Jira. Explain where to click and what each page does. Never mention API endpoints, backend routes, HTTP methods, database fields, ids, tokens, DTOs, code, or server implementation details. Do not include reasoning. If you do not know, say so."
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

    public async Task<string> AnswerFromLiveDataAsync(string question, string liveDataContext, CancellationToken cancellationToken = default)
    {
        var chatbotOptions = options.Value;

        if (!chatbotOptions.Enabled)
        {
            return liveDataContext;
        }

        var requestUrl = $"{chatbotOptions.BaseUrl.TrimEnd('/')}/chat/completions";
        var request = new
        {
            model = chatbotOptions.Model,
            temperature = 0.25,
            max_tokens = chatbotOptions.MaxTokens,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = """
                    You are a friendly Mini Jira in-app assistant.
                    Speak as the assistant using first person, for example "I found..." or "I can help...", not "You can...".
                    The backend has already checked authentication and permissions.
                    Use only the safe live data provided. Do not invent projects, tasks, epics, users, counts, status, or priority.
                    Answer naturally in 1 to 4 short sentences.
                    If the data says nothing was found, say that clearly and, if useful, ask a short follow-up.
                    Never mention API endpoints, backend routes, database fields, ids, tokens, DTOs, code, or server implementation details.
                    Do not include reasoning.
                    """
                },
                new
                {
                    role = "user",
                    content = $"""
                    User question:
                    {question}

                    Safe live Mini Jira data:
                    {liveDataContext}
                    """
                }
            }
        };

        var response = await httpClient.PostAsJsonAsync(requestUrl, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var completion = await response.Content.ReadFromJsonAsync<LmStudioChatCompletionResponse>(cancellationToken);
        var message = completion?.Choices?.FirstOrDefault()?.Message;
        var answer = ExtractFinalAnswer(message?.Content)
            ?? ExtractFinalAnswer(message?.ReasoningContent);

        return string.IsNullOrWhiteSpace(answer)
            ? liveDataContext
            : answer;
    }

    public async Task<ChatbotIntent> ClassifyIntentAsync(
        string question,
        IReadOnlyCollection<string>? visibleProjectNames = null,
        IReadOnlyCollection<string>? visibleEpicNames = null,
        CancellationToken cancellationToken = default)
    {
        var chatbotOptions = options.Value;

        if (!chatbotOptions.Enabled || IsGreeting(question))
        {
            return new ChatbotIntent("general_help");
        }

        var requestUrl = $"{chatbotOptions.BaseUrl.TrimEnd('/')}/chat/completions";
        var request = new
        {
            model = chatbotOptions.Model,
            temperature = 0,
            max_tokens = 180,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = """
                    You convert a Mini Jira user's message into one structured action.
                    Return only JSON. No markdown. No explanation.

                    Allowed intents:
                    general_help
                    get_my_projects
                    get_my_tasks
                    newest_tasks
                    search_tasks
                    project_task_filter
                    project_people
                    project_changes
                    prioritize_my_tasks
                    list_project_epics
                    summarize_project
                    summarize_epic_status

                    JSON shape:
                    {"intent":"general_help","projectName":null,"epicName":null,"search":null,"status":null,"priority":null}

                    Rules:
                    - Understand the user's meaning, not exact wording.
                    - Choose general_help for conceptual or navigation questions that do not need live database data.
                    - Choose a live-data intent only when the user asks about their actual projects, tasks, epics, members, summaries, changes, or priorities.
                    - If the user refers to a visible project, set projectName to the closest exact name from the visible project list.
                    - If the user refers to a visible epic, set epicName to the closest exact name from the visible epic list.
                    - If no visible project or epic is clearly referenced, use null.
                    - status must be one of Open, In Progress, Review, Completed, or null.
                    - priority must be one of Low, Medium, High, or null.
                    - search is only for the search_tasks intent. For get_my_tasks, newest_tasks, summaries, epics, members, or changes, search must be null.
                    """
                },
                new
                {
                    role = "user",
                    content = $"""
                    Visible project names:
                    {FormatNamesForPrompt(visibleProjectNames)}

                    Visible epic names:
                    {FormatNamesForPrompt(visibleEpicNames)}

                    User message:
                    {question}
                    """
                }
            }
        };

        var response = await httpClient.PostAsJsonAsync(requestUrl, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var completion = await response.Content.ReadFromJsonAsync<LmStudioChatCompletionResponse>(cancellationToken);
        var rawAnswer = ExtractFinalAnswer(completion?.Choices?.FirstOrDefault()?.Message?.Content)
            ?? ExtractFinalAnswer(completion?.Choices?.FirstOrDefault()?.Message?.ReasoningContent);

        return ParseIntent(rawAnswer);
    }

    private static ChatbotIntent ParseIntent(string? rawAnswer)
    {
        if (string.IsNullOrWhiteSpace(rawAnswer))
        {
            return new ChatbotIntent("general_help");
        }

        var json = ExtractJsonObject(rawAnswer);
        if (json is null)
        {
            return new ChatbotIntent("general_help");
        }

        try
        {
            var intent = JsonSerializer.Deserialize<ChatbotIntent>(json, JsonOptions);
            return IsAllowedIntent(intent?.Intent)
                ? intent! with
                {
                    Intent = intent.Intent.Trim(),
                    Status = NormalizeStatus(intent.Status),
                    Priority = NormalizePriority(intent.Priority)
                }
                : new ChatbotIntent("general_help");
        }
        catch (JsonException)
        {
            return new ChatbotIntent("general_help");
        }
    }

    private static string? ExtractJsonObject(string rawAnswer)
    {
        var start = rawAnswer.IndexOf('{');
        var end = rawAnswer.LastIndexOf('}');

        return start >= 0 && end > start
            ? rawAnswer[start..(end + 1)]
            : null;
    }

    private static bool IsAllowedIntent(string? intent)
    {
        var allowedIntents = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "general_help",
            "get_my_projects",
            "get_my_tasks",
            "newest_tasks",
            "search_tasks",
            "project_task_filter",
            "project_people",
            "project_changes",
            "prioritize_my_tasks",
            "list_project_epics",
            "summarize_project",
            "summarize_epic_status"
        };

        return !string.IsNullOrWhiteSpace(intent) && allowedIntents.Contains(intent);
    }

    private static string FormatNamesForPrompt(IReadOnlyCollection<string>? names)
        => names is null || names.Count == 0
            ? "(none)"
            : string.Join("\n", names.Select(name => $"- {name}"));

    private static string? NormalizeStatus(string? status)
        => status?.Trim().ToLowerInvariant() switch
        {
            "open" => "Open",
            "in progress" => "In Progress",
            "review" => "Review",
            "completed" => "Completed",
            "done" => "Completed",
            _ => null
        };

    private static string? NormalizePriority(string? priority)
        => priority?.Trim().ToLowerInvariant() switch
        {
            "low" => "Low",
            "medium" => "Medium",
            "high" => "High",
            _ => null
        };

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
