using System;
using System.Collections.Generic;
using MediatR;

namespace MiniJiraAspire.Server.Models;

// Tasks
public record GetTaskQuery(string TaskId) : IRequest<TaskItemDto?>;

public record GetTasksQuery(
    string? Search,
    string? Status,
    string? Priority,
    string? AssigneeId,
    string? EpicId,
    string? ProjectId) : IRequest<TaskItemDto[]>;

// Comments
public record GetCommentsForTaskQuery(string TaskId) : IRequest<CommentDto[]>;

public record GetCommentByIdQuery(string TaskId, Guid CommentId) : IRequest<CommentDto?>;

// Epics
public record GetAllEpicsQuery(Guid? ProjectId = null) : IRequest<List<EpicDto>>;

public record GetEpicByIdQuery(Guid Id) : IRequest<EpicDto?>;

// Projects
public record GetAllProjectsQuery : IRequest<List<ProjectDto>>;

public record GetProjectByIdQuery(Guid Id) : IRequest<ProjectDto?>;

// Users
public record GetAllUsersQuery : IRequest<List<UserDto>>;

public record GetUserByIdQuery(string UserId) : IRequest<UserDto?>;