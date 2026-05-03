namespace MiniJiraAspire.Server.Models;

public class Epic
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    // TODO: Add references for Tasks
    
}