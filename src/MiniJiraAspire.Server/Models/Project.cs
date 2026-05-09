namespace MiniJiraAspire.Server.Models;

public class Project
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
}