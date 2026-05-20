using System.ComponentModel.DataAnnotations;

namespace MiniJiraAspire.Server.Models;

public record CreateUserRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(6)] string Password,
    [property: Required, StringLength(100, MinimumLength = 2)] string DisplayName);

public record CreateUserData(string Email, string PasswordHash, string DisplayName);

public record UserDto(string Id, string Email, string DisplayName, string Role);

public record LoginRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required] string Password);

public record LoginResponse(string Token, UserDto User);

public record EpicDto(Guid Id, string Name, string Description, Guid ProjectId, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);

public record CreateEpicRequest(
    [property: Required, StringLength(100, MinimumLength = 3)] string Name,
    [property: StringLength(2000)] string? Description,
    [property: Required] Guid ProjectId);

public record UpdateEpicRequest(
    [property: StringLength(100, MinimumLength = 3)] string Name,
    [property: StringLength(2000)] string? Description);

public record ProjectDto(Guid Id, string Name, string Description, string[] MemberIds, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);

public record CreateProjectRequest(
    [property: Required, StringLength(100, MinimumLength = 3)] string Name,
    [property: StringLength(2000)] string Description);

public record UpdateProjectRequest(
    [property: StringLength(100, MinimumLength = 3)] string Name,
    [property: StringLength(2000)] string Description);

public record TaskItemDto(Guid Id, string Title, string? Description, string Status, string Priority, Guid ProjectId, Guid? AssigneeId, Guid? EpicId, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);

public record CreateTaskRequest(
    [property: Required, StringLength(200, MinimumLength = 3)] string Title,
    [property: StringLength(2000)] string? Description,
    [property: Required] Guid ProjectId);

public record UpdateTaskRequest(
    [property: StringLength(200, MinimumLength = 3)] string Title,
    [property: StringLength(2000)] string? Description);
