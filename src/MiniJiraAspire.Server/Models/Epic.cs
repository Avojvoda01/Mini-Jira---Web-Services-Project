namespace MiniJiraAspire.Server.Models;

public class Epic : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    // TODO: Add references for Tasks

}