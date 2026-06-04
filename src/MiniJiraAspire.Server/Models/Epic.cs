using System;

namespace MiniJiraAspire.Server.Models;

public class Epic : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
}