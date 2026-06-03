using MediatR;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Endpoints.Tasks;

public static class TaskEndpoints
{
    public static void MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var tasks = app.MapGroup("/tasks")
            .WithTags("Tasks");

        tasks.MapPost("/", CreateTask)
            .WithName("CreateTask")
            .WithSummary("Create a new task");

        tasks.MapPut("/{taskId}", UpdateTask)
            .WithName("UpdateTask")
            .WithSummary("Edit an existing task");

        tasks.MapDelete("/{taskId}", DeleteTask)
            .WithName("DeleteTask")
            .WithSummary("Delete a task");

        tasks.MapGet("/{taskId}", GetTask)
            .Produces<TaskItemDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("GetTask")
            .WithSummary("Get a single task by ID");

        tasks.MapGet("/", GetTasks)
            .Produces<TaskItemDto[]>(StatusCodes.Status200OK)
            .WithName("GetTasks")
            .WithSummary("Search and filter tasks");

        var actions = app.MapGroup("/tasks")
            .WithTags("Task Actions");

        actions.MapPatch("/{taskId}/status", ChangeStatus)
            .WithName("ChangeTaskStatus")
            .WithSummary("Change the status of a task");

        actions.MapPatch("/{taskId}/priority", ChangePriority)
            .WithName("ChangeTaskPriority")
            .WithSummary("Change the priority of a task");

        actions.MapPatch("/{taskId}/assign-user", AssignUser)
            .WithName("AssignUserToTask")
            .WithSummary("Assign a user to a task");

        actions.MapPatch("/{taskId}/assign-epic", AssignEpic)
            .WithName("AssignEpicToTask")
            .WithSummary("Assign an epic to a task");
    }

    private static async Task<IResult> CreateTask(
        CreateTaskCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Results.Created($"/api/v1/tasks/{result.Id}", result);
    }

    private static async Task<IResult> UpdateTask(
        string taskId,
        UpdateTaskCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        var task = await mediator.Send(command with { TaskId = taskId }, ct);
        return task is null
            ? Results.Problem($"Task with id {taskId} not found", statusCode: StatusCodes.Status404NotFound)
            : Results.Ok(task);
    }

    private static async Task<IResult> DeleteTask(
        string taskId,
        IMediator mediator,
        CancellationToken ct)
    {
        var deleted = await mediator.Send(new DeleteTaskCommand(taskId), ct);
        return deleted
            ? Results.NoContent()
            : Results.Problem($"Task with id {taskId} not found", statusCode: StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetTask(
        string taskId,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetTaskQuery(taskId), ct);
        return result is null
            ? Results.Problem($"Task with id {taskId} not found", statusCode: StatusCodes.Status404NotFound)
            : Results.Ok(result);
    }

    private static async Task<IResult> GetTasks(
        string? search,
        string? status,
        string? priority,
        string? assigneeId,
        string? epicId,
        string? projectId,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetTasksQuery(search, status, priority, assigneeId, epicId, projectId), ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> ChangeStatus(
        string taskId,
        ChangeStatusCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        var task = await mediator.Send(command with { TaskId = taskId }, ct);
        return task is null
            ? Results.Problem($"Task with id {taskId} not found", statusCode: StatusCodes.Status404NotFound)
            : Results.Ok(task);
    }

    private static async Task<IResult> ChangePriority(
        string taskId,
        ChangePriorityCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        var task = await mediator.Send(command with { TaskId = taskId }, ct);
        return task is null
            ? Results.Problem($"Task with id {taskId} not found", statusCode: StatusCodes.Status404NotFound)
            : Results.Ok(task);
    }

    private static async Task<IResult> AssignUser(
        string taskId,
        AssignUserCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        var task = await mediator.Send(command with { TaskId = taskId }, ct);
        return task is null
            ? Results.Problem($"Task with id {taskId} not found", statusCode: StatusCodes.Status404NotFound)
            : Results.Ok(task);
    }

    private static async Task<IResult> AssignEpic(
        string taskId,
        AssignEpicCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        var task = await mediator.Send(command with { TaskId = taskId }, ct);
        return task is null
            ? Results.Problem($"Task with id {taskId} not found", statusCode: StatusCodes.Status404NotFound)
            : Results.Ok(task);
    }
}
