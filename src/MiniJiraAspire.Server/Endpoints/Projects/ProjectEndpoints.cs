namespace Microsoft.Extensions.Hosting.Projects;

public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/projects", async (
                CreateProjectCommand command,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.Created("/api/projects/new-id", new { Id = "new-id" });
            })
            .WithName("CreateProject")
            .WithTags("Projects")
            .WithSummary("Create a new project");

        app.MapDelete("/api/projects/{projectId}", async (
                string projectId,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.NoContent();
            })
            .WithName("DeleteProject")
            .WithTags("Projects")
            .WithSummary("Delete a project");

        app.MapPost("/api/projects/{projectId}/members", async (
                string projectId,
                AddProjectMemberCommand command,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.Created($"/api/projects/{projectId}/members", new { });
            })
            .WithName("AddProjectMember")
            .WithTags("Projects")
            .WithSummary("Assign a member to a project");

        app.MapDelete("/api/projects/{projectId}/members/{userId}", async (
                string projectId,
                string userId,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.NoContent();
            })
            .WithName("RemoveProjectMember")
            .WithTags("Projects")
            .WithSummary("Remove a member from a project");
    }
}

public record CreateProjectCommand(string Name, string? Description);
public record AddProjectMemberCommand(string UserId, string Role);