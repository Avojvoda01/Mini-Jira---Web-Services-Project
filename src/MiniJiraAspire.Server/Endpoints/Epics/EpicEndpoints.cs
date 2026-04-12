namespace Microsoft.Extensions.Hosting.Epics;

public static class EpicEndpoints
{
    public static void MapEpicEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/epics", async (
                CreateEpicCommand command,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.Created("/api/epics/new-id", new { Id = "new-id" });
            })
            .WithName("CreateEpic")
            .WithTags("Epics")
            .WithSummary("Create a new epic");

        app.MapPut("/api/epics/{epicId}", async (
                string epicId,
                UpdateEpicCommand command,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.Ok();
            })
            .WithName("UpdateEpic")
            .WithTags("Epics")
            .WithSummary("Edit an existing epic");

        app.MapDelete("/api/epics/{epicId}", async (
                string epicId,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.NoContent();
            })
            .WithName("DeleteEpic")
            .WithTags("Epics")
            .WithSummary("Delete an epic");

        app.MapGet("/api/epics", async (
                string? projectId,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.Ok(Array.Empty<object>());
            })
            .WithName("GetEpics")
            .WithTags("Epics")
            .WithSummary("Get all epics");
    }
}

public record CreateEpicCommand(string Title, string? Description, string ProjectId);
public record UpdateEpicCommand(string Title, string? Description);