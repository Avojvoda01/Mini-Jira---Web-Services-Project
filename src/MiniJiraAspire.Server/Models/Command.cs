using System;
using MediatR;

namespace MiniJiraAspire.Server.Models;

// Auth
public record LoginUserCommand(string Email, string Password) : IRequest<LoginResponse?>;

public record RegisterUserCommand(string Email, string Password, string DisplayName) : IRequest<RegisterUserResponse>;

// Tasks
public record CreateTaskCommand(string Title, string? Description, string ProjectId) : IRequest<TaskItemDto>;

public record UpdateTaskCommand(string TaskId, string Title, string? Description) : IRequest<TaskItemDto?>;

public record DeleteTaskCommand(string TaskId) : IRequest<bool>;

public record ChangeStatusCommand(string TaskId, string Status) : IRequest<TaskItemDto?>;

public record ChangePriorityCommand(string TaskId, string Priority) : IRequest<TaskItemDto?>;

public record AssignUserCommand(string TaskId, string? UserId) : IRequest<TaskItemDto?>;

public record AssignEpicCommand(string TaskId, string? EpicId) : IRequest<TaskItemDto?>;

// Comments
public record CreateCommentCommand(string TaskId, string Content, Guid? UserId) : IRequest<CommentDto?>;

public record UpdateCommentCommand(string TaskId, Guid CommentId, string Content) : IRequest<CommentDto?>;

public record DeleteCommentCommand(string TaskId, Guid CommentId) : IRequest<bool>;

// Epics
public record CreateEpicCommand(string Name, string? Description, Guid ProjectId) : IRequest<EpicDto?>;

public record UpdateEpicCommand(Guid Id, string Name, string? Description) : IRequest<EpicDto?>;

public record DeleteEpicCommand(Guid Id) : IRequest<bool>;

// Projects
public record CreateProjectCommand(string Name, string? Description) : IRequest<ProjectDto>;

public record UpdateProjectCommand(Guid Id, string Name, string? Description) : IRequest<ProjectDto?>;

public record DeleteProjectCommand(Guid ProjectId) : IRequest<bool>;

public record AddProjectMemberCommand(string ProjectId, string UserId, string Role) : IRequest<bool>;

public record RemoveProjectMemberCommand(string ProjectId, string UserId) : IRequest<bool>;

// Users
public record CreateUserCommand(string Email, string Password, string DisplayName) : IRequest<CreateUserResponse>;

public record DeleteUserCommand(string UserId) : IRequest<bool>;

public record ChangeUserRoleCommand(string UserId, string Role) : IRequest<ChangeUserRoleResponse>;
