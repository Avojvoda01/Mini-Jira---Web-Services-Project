namespace MiniJiraAspire.Server.Models;

public record UserDto(string Id, string Email, string DisplayName, string Role);

public record EpicDto(Guid Id, string Name, string Description, Guid ProjectId, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);

public record ProjectDto(Guid Id, string Name, string Description, string[] MemberIds, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);

public record TaskItemDto(Guid Id, string Title, string? Description, string Status, string Priority, Guid ProjectId, Guid? AssigneeId, Guid? EpicId, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);

public record CommentDto(
    Guid Id,
    string TaskId,
    Guid? UserId,
    string Content,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);