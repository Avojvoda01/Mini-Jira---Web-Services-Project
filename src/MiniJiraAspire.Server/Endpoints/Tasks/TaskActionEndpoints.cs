namespace Microsoft.Extensions.Hosting.Tasks.Actions;

public static class TaskActionEndpoints
{
    public static void MapTaskActionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/tasks/{taskId}/status", async (
                string taskId,
                ChangeStatusCommand command,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.Ok();
            })
            .WithName("ChangeTaskStatus")
            .WithTags("Task Actions")
            .WithSummary("Change the status of a task");

        app.MapPatch("/api/tasks/{taskId}/priority", async (
                string taskId,
                ChangePriorityCommand command,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.Ok();
            })
            .WithName("ChangeTaskPriority")
            .WithTags("Task Actions")
            .WithSummary("Change the priority of a task");

        app.MapPatch("/api/tasks/{taskId}/assign-user", async (
                string taskId,
                AssignUserCommand command,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.Ok();
            })
            .WithName("AssignUserToTask")
            .WithTags("Task Actions")
            .WithSummary("Assign a user to a task");

        app.MapPatch("/api/tasks/{taskId}/assign-epic", async (
                string taskId,
                AssignEpicCommand command,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.Ok();
            })
            .WithName("AssignEpicToTask")
            .WithTags("Task Actions")
            .WithSummary("Assign an epic to a task");
    }
}

public record ChangeStatusCommand(string Status);
public record ChangePriorityCommand(string Priority);
public record AssignUserCommand(string UserId);
public record AssignEpicCommand(string EpicId);
