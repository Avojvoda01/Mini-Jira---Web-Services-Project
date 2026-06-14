using System;

namespace MiniJiraAspire.Server.Models;

public class TaskItem : BaseEntity
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = "Ready";
    public string Priority { get; set; } = "Medium";
    public Guid ProjectId { get; set; }
    public Guid? AssigneeId { get; set; }
    public Guid? EpicId { get; set; }
    public Guid? CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }
    public int? EstimateMinutes { get; set; }
}
