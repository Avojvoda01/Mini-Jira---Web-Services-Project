using MediatR;
using MiniJiraAspire.Server.Features.Tasks.Commands;
using MiniJiraAspire.Server.Features.Tasks.Queries;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Endpoints.Tasks;

public static class TaskEndpoints
{
    public static void MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var tasks = app.MapGroup("/api/tasks")
            .WithTags("Tasks");

        tasks.MapPost("/", async (
                CreateTaskCommand command,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(command, ct);
                return Results.Created($"/api/tasks/{result.Id}", result);
            })
            .WithName("CreateTask")
            .WithSummary("Create a new task");

        tasks.MapPut("/{taskId}", async (
                string taskId,
                UpdateTaskCommand command,
                IMediator mediator,
                CancellationToken ct) =>
            {
                await mediator.Send(command with { TaskId = taskId }, ct);
                return Results.Ok();
            })
            .WithName("UpdateTask")
            .WithSummary("Edit an existing task");

        tasks.MapDelete("/{taskId}", async (
                string taskId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                await mediator.Send(new DeleteTaskCommand(taskId), ct);
                return Results.NoContent();
            })
            .WithName("DeleteTask")
            .WithSummary("Delete a task");

        tasks.MapGet("/{taskId}", async (
                string taskId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(new GetTaskQuery(taskId), ct);
                if (result is null)
                    return Results.Problem($"Task with id {taskId} not found", statusCode: StatusCodes.Status404NotFound);
                return Results.Ok(result);
            })
            .Produces<TaskItemDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("GetTask")
            .WithSummary("Get a single task by ID");

        tasks.MapGet("/", async (
                string? search,
                string? status,
                string? priority,
                string? assigneeId,
                string? epicId,
                string? projectId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(new GetTasksQuery(search, status, priority, assigneeId, epicId, projectId), ct);
                return Results.Ok(result);
            })
            .Produces<TaskItemDto[]>(StatusCodes.Status200OK)
            .WithName("GetTasks")
            .WithSummary("Search and filter tasks");

        var actions = app.MapGroup("/api/tasks")
            .WithTags("Task Actions");

        actions.MapPatch("/{taskId}/status", async (
                string taskId,
                ChangeStatusCommand command,
                IMediator mediator,
                CancellationToken ct) =>
            {
                await mediator.Send(command with { TaskId = taskId }, ct);
                return Results.Ok();
            })
            .WithName("ChangeTaskStatus")
            .WithSummary("Change the status of a task");

        actions.MapPatch("/{taskId}/priority", async (
                string taskId,
                ChangePriorityCommand command,
                IMediator mediator,
                CancellationToken ct) =>
            {
                await mediator.Send(command with { TaskId = taskId }, ct);
                return Results.Ok();
            })
            .WithName("ChangeTaskPriority")
            .WithSummary("Change the priority of a task");

        actions.MapPatch("/{taskId}/assign-user", async (
                string taskId,
                AssignUserCommand command,
                IMediator mediator,
                CancellationToken ct) =>
            {
                await mediator.Send(command with { TaskId = taskId }, ct);
                return Results.Ok();
            })
            .WithName("AssignUserToTask")
            .WithSummary("Assign a user to a task");

        actions.MapPatch("/{taskId}/assign-epic", async (
                string taskId,
                AssignEpicCommand command,
                IMediator mediator,
                CancellationToken ct) =>
            {
                await mediator.Send(command with { TaskId = taskId }, ct);
                return Results.Ok();
            })
            .WithName("AssignEpicToTask")
            .WithSummary("Assign an epic to a task");
    }
}
