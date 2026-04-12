using MediatR;
using Microsoft.Extensions.Hosting.Tasks.Create;
using Microsoft.Extensions.Hosting.Tasks.Get;
using Microsoft.Extensions.Hosting.Tasks.Update;

namespace Microsoft.Extensions.Hosting.Tasks;

public static class TaskEndpoints
{
    public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks").WithTags("Tasks");

        group.MapPost("/", async (CreateTaskCommand cmd, ISender sender, CancellationToken ct)
                => Results.Ok(await sender.Send(cmd, ct)))
            .WithSummary("Create task");

        group.MapPut("/{taskId:guid}", async (Guid taskId, UpdateTaskCommand cmd, ISender sender, CancellationToken ct)
                => Results.Ok(await sender.Send(cmd with { TaskId = taskId }, ct)))
            .WithSummary("Update task");
        /*
        group.MapDelete("/{taskId:guid}", async (Guid taskId, ISender sender, CancellationToken ct)
                => Results.Ok(await sender.Send(new DeleteTaskCommand(taskId), ct)))
            .WithSummary("Delete task");
        */
        group.MapGet("/{taskId:guid}", async (Guid taskId, ISender sender, CancellationToken ct)
                => Results.Ok(await sender.Send(new GetTaskByIdQuery(taskId), ct)))
            .WithSummary("Get task");

        /*
        group.MapGet("/", async ([AsParameters] SearchTasksQuery query, ISender sender, CancellationToken ct)
                => Results.Ok(await sender.Send(query, ct)))
            .WithSummary("Search tasks");
        */
        return app;
    }
}