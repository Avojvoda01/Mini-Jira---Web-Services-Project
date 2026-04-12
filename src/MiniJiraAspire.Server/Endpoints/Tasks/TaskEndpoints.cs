namespace Microsoft.Extensions.Hosting.Tasks;

public static class TaskEndpoints
{
    public static void MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/tasks", async (
                CreateTaskCommand command,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.Created($"/api/tasks/new-id", new { Id = "new-id" });
            })
            .WithName("CreateTask")
            .WithTags("Tasks")
            .WithSummary("Create a new task");

        app.MapPut("/api/tasks/{taskId}", async (
                string taskId,
                UpdateTaskCommand command,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.Ok();
            })
            .WithName("UpdateTask")
            .WithTags("Tasks")
            .WithSummary("Edit an existing task");

        app.MapDelete("/api/tasks/{taskId}", async (
                string taskId,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.NoContent();
            })
            .WithName("DeleteTask")
            .WithTags("Tasks")
            .WithSummary("Delete a task");

        app.MapGet("/api/tasks/{taskId}", async (
                string taskId,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.Ok(new { Id = taskId });
            })
            .WithName("GetTask")
            .WithTags("Tasks")
            .WithSummary("Get a single task by ID");

        app.MapGet("/api/tasks", async (
                string? search,
                string? status,
                string? priority,
                string? assigneeId,
                string? epicId,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.Ok(Array.Empty<object>());
            })
            .WithName("GetTasks")
            .WithTags("Tasks")
            .WithSummary("Search and filter tasks");
    }
}

public record CreateTaskCommand(string Title, string? Description, string ProjectId);
public record UpdateTaskCommand(string Title, string? Description);
