using System;
using System.ComponentModel.DataAnnotations;

namespace MiniJiraAspire.Server.Models;

// Auth & Users
public record LoginRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required] string Password);

public record CreateUserRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required]
    [property: MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    [property: RegularExpression(
        @"^(?=.*[A-Za-z])(?=.*\d).+$",
        ErrorMessage = "Password must contain at least one letter and one number.")]
    string Password,
    [property: Required, StringLength(100, MinimumLength = 2)] string DisplayName);

public record ChangeUserRoleRequest(string Role);

public record UpdateUserProfileRequest(
    [property: Required, StringLength(100, MinimumLength = 2)] string DisplayName,
    [property: Required, EmailAddress] string Email);

public record ChangeUserPasswordRequest(
    [property: Required] string CurrentPassword,
    [property: Required]
    [property: MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    [property: RegularExpression(
        @"^(?=.*[A-Za-z])(?=.*\d).+$",
        ErrorMessage = "Password must contain at least one letter and one number.")]
    string NewPassword);

// Epics
public record CreateEpicRequest(
    [property: Required, StringLength(100, MinimumLength = 3)] string Name,
    [property: StringLength(2000)] string? Description,
    [property: Required] Guid ProjectId);

public record UpdateEpicRequest(
    [property: StringLength(100, MinimumLength = 3)] string Name,
    [property: StringLength(2000)] string? Description);

// Projects
public record CreateProjectRequest(
    [property: Required, StringLength(100, MinimumLength = 3)] string Name,
    [property: StringLength(2000)] string Description);

public record UpdateProjectRequest(
    [property: StringLength(100, MinimumLength = 3)] string Name,
    [property: StringLength(2000)] string Description);

public record ChangeProjectOwnerRequest([property: Required] Guid OwnerId);

// Tasks
public record CreateTaskRequest(
    [property: Required, StringLength(200, MinimumLength = 3)] string Title,
    [property: StringLength(2000)] string? Description,
    [property: Required] Guid ProjectId);

public record UpdateTaskRequest(
    [property: StringLength(200, MinimumLength = 3)] string Title,
    [property: StringLength(2000)] string? Description);

// Comments
public record CreateCommentRequest(string Content);

public record UpdateCommentRequest(string Content);
