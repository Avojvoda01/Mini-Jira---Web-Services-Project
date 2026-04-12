using MediatR;

namespace Microsoft.Extensions.Hosting.Tasks.Create;

public static class CreateTaskEndpoint
{

    public static IEndpointRouteBuilder MapCreateTask(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/tasks", async (
                CreateTaskCommand command,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(command, ct);
                return Results.Ok(result);
            })
            .WithName("CreateTask")
            .WithTags("Tasks")
            .WithSummary("Create a new task");

        return app;
    }
    
}


public record CreateTaskCommand(string Title, string Description);