using System;

namespace MiniJiraAspire.Server.Models;

public record UserDto(string Id, string Email, string DisplayName, UserRole Role);

public record EpicDto(Guid Id, string Name, string Description, Guid ProjectId, Guid? CreatedById, Guid? UpdatedById, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);

public record ProjectDto(Guid Id, string Name, string Description, string[] MemberIds, Guid? CreatedById, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);

public record TaskItemDto(Guid Id, string Title, string? Description, string Status, string Priority, Guid ProjectId, Guid? AssigneeId, Guid? EpicId, Guid? CreatedById, Guid? UpdatedById, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc, int? EstimateMinutes);

public record CommentDto(
    Guid Id,
    string TaskId,
    Guid? UserId,
    string Content,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);