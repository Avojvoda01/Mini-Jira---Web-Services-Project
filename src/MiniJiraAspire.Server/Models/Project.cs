namespace MiniJiraAspire.Server.Models;

public class Project : BaseEntity
{
    public required string Name { get; set; }
    public required string Description { get; set; }

    public List<ProjectMember> Members { get; set; } = [];
}