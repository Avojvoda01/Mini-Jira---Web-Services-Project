using MediatR;
using MiniJiraAspire.Server.Features.Tasks.Commands;
using MiniJiraAspire.Server.Features.Tasks.Queries;
using MiniJiraAspire.Server.Models;

namespace Microsoft.Extensions.Hosting.Tasks;

public static class TaskEndpoints
{
    public static void MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/tasks", async (
                CreateTaskCommand command,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(command, ct);
                return Results.Created($"/api/tasks/{result.Id}", result);
            })
            .WithName("CreateTask")
            .WithTags("Tasks")
            .WithSummary("Create a new task");

        app.MapPut("/api/tasks/{taskId}", async (
                string taskId,
                UpdateTaskCommand command,
                IMediator mediator,
                CancellationToken ct) =>
            {
                await mediator.Send(command with { TaskId = taskId }, ct);
                return Results.Ok();
            })
            .WithName("UpdateTask")
            .WithTags("Tasks")
            .WithSummary("Edit an existing task");

        app.MapDelete("/api/tasks/{taskId}", async (
                string taskId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                await mediator.Send(new DeleteTaskCommand(taskId), ct);
                return Results.NoContent();
            })
            .WithName("DeleteTask")
            .WithTags("Tasks")
            .WithSummary("Delete a task");

        app.MapGet("/api/tasks/{taskId}", async (
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
            .WithTags("Tasks")
            .WithSummary("Get a single task by ID");

        app.MapGet("/api/tasks", async (
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
            .WithTags("Tasks")
            .WithSummary("Search and filter tasks");
    }
}
