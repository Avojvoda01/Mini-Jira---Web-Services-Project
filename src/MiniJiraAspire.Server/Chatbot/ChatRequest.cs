using System.ComponentModel.DataAnnotations;

namespace MiniJiraAspire.Server.Chatbot;

public record ChatRequest(
    [property: Required, MinLength(1)] string Message);
