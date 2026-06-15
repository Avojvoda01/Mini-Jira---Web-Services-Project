namespace MiniJiraAspire.Server.Features.Chatbot;

public record ChatbotIntent(
    string Intent,
    string? ProjectName = null,
    string? EpicName = null,
    string? Search = null,
    string? Status = null,
    string? Priority = null);
