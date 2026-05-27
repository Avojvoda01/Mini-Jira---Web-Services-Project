using MediatR;
using MiniJiraAspire.Server.Features.Tasks.Commands;

namespace MiniJiraAspire.Server.Endpoints.Tasks;

public static class TaskActionEndpoints
{
    public static void MapTaskActionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/tasks/{taskId}/status", async (
                string taskId,
                ChangeStatusCommand command,
                IMediator mediator,
                CancellationToken ct) =>
            {
                await mediator.Send(command with { TaskId = taskId }, ct);
                return Results.Ok();
            })
            .WithName("ChangeTaskStatus")
            .WithTags("Task Actions")
            .WithSummary("Change the status of a task");

        app.MapPatch("/api/tasks/{taskId}/priority", async (
                string taskId,
                ChangePriorityCommand command,
                IMediator mediator,
                CancellationToken ct) =>
            {
                await mediator.Send(command with { TaskId = taskId }, ct);
                return Results.Ok();
            })
            .WithName("ChangeTaskPriority")
            .WithTags("Task Actions")
            .WithSummary("Change the priority of a task");

        app.MapPatch("/api/tasks/{taskId}/assign-user", async (
                string taskId,
                AssignUserCommand command,
                IMediator mediator,
                CancellationToken ct) =>
            {
                await mediator.Send(command with { TaskId = taskId }, ct);
                return Results.Ok();
            })
            .WithName("AssignUserToTask")
            .WithTags("Task Actions")
            .WithSummary("Assign a user to a task");

        app.MapPatch("/api/tasks/{taskId}/assign-epic", async (
                string taskId,
                AssignEpicCommand command,
                IMediator mediator,
                CancellationToken ct) =>
            {
                await mediator.Send(command with { TaskId = taskId }, ct);
                return Results.Ok();
            })
            .WithName("AssignEpicToTask")
            .WithTags("Task Actions")
            .WithSummary("Assign an epic to a task");
    }
}
