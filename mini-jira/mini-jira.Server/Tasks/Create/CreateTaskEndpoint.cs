using MediatR;

namespace Microsoft.Extensions.Hosting.Tasks.Create;

public static class CreateTaskEndpoint
{

    public static void MapCreateTask(this IEndpointRouteBuilder app)
    {
        app.MapPost("/tasks", async (
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
    }
    
}


public record CreateTaskCommand(string Title, string Description);