using MediatR;
using MiniJiraAspire.Server.Features.Project;

namespace Microsoft.Extensions.Hosting.Projects;

public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/projects", async (
                CreateProjectCommand command,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(command, ct);
                return Results.Created($"/api/projects/{((dynamic)result).Id}", result);
            })
            .WithName("CreateProject")
            .WithTags("Projects")
            .WithSummary("Create a new project");

        app.MapDelete("/api/projects/{projectId}", async (
                string projectId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                await mediator.Send(new DeleteProjectCommand(projectId), ct);
                return Results.NoContent();
            })
            .WithName("DeleteProject")
            .WithTags("Projects")
            .WithSummary("Delete a project");

        app.MapPost("/api/projects/{projectId}/members", async (
                string projectId,
                AddProjectMemberCommand command,
                IMediator mediator,
                CancellationToken ct) =>
            {
                await mediator.Send(command with { ProjectId = projectId }, ct);
                return Results.Created($"/api/projects/{projectId}/members", new { });
            })
            .WithName("AddProjectMember")
            .WithTags("Projects")
            .WithSummary("Assign a member to a project");

        app.MapDelete("/api/projects/{projectId}/members/{userId}", async (
                string projectId,
                string userId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                await mediator.Send(new RemoveProjectMemberCommand(projectId, userId), ct);
                return Results.NoContent();
            })
            .WithName("RemoveProjectMember")
            .WithTags("Projects")
            .WithSummary("Remove a member from a project");
    }
}
