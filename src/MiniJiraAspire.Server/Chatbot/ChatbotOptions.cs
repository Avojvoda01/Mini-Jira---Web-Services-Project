namespace MiniJiraAspire.Server.Chatbot;

public class ChatbotOptions
{
    public const string SectionName = "LmStudio";

    public bool Enabled { get; set; } = true;

    public string BaseUrl { get; set; } = "http://localhost:1234/v1";

    public string Model { get; set; } = "local-model";

    public int MaxTokens { get; set; } = 900;
}
