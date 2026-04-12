using MediatR;

namespace Microsoft.Extensions.Hosting.Tasks.Update;

public record UpdateTaskCommand(Guid TaskId, string Title, string Description);

public static class UpdateTaskEndpoint
{
    public static IEndpointRouteBuilder MapUpdateTask(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/tasks/{taskId:guid}", async (
                Guid taskId,
                UpdateTaskCommand command,
                ISender sender,
                CancellationToken ct) =>
            {
                return Results.Ok(await sender.Send(command with { TaskId = taskId }, ct));
            })
            .WithTags("Tasks")
            .WithSummary("Update task");

        return app;
    }
}