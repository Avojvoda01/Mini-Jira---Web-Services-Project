using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Endpoints.Projects;

public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects")
            .WithTags("Projects");

        group.MapPost("/", async (
                CreateProjectRequest request,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(new CreateProjectCommand(request.Name, request.Description), ct);
                return TypedResults.Created($"/api/projects/{result.Id}", result);
            })
            .WithName("CreateProject")
            .WithSummary("Create a new project");

        group.MapPut("/{id:guid}", async (
                Guid id,
                UpdateProjectRequest request,
                IMediator mediator,
                CancellationToken ct) =>
            {
                try
                {
                    var result = await mediator.Send(new UpdateProjectCommand(id, request.Name, request.Description), ct);
                    return Results.NoContent();
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: StatusCodes.Status404NotFound);
                }
            })
            .WithName("UpdateProject")
            .WithSummary("Edit an existing project");

        group.MapDelete("/{id:guid}", async (
                Guid id,
                IMediator mediator,
                CancellationToken ct) =>
            {
                try
                {
                    await mediator.Send(new DeleteProjectCommand(id), ct);
                    return Results.NoContent();
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: StatusCodes.Status404NotFound);
                }
            })
            .WithName("DeleteProject")
            .WithSummary("Delete a project");

        group.MapGet("/", async (
                IMediator mediator,
                CancellationToken ct) =>
            {
                var projects = await mediator.Send(new GetAllProjectsQuery(), ct);
                return TypedResults.Ok(projects);
            })
            .Produces<List<ProjectDto>>(StatusCodes.Status200OK)
            .WithName("GetProjects")
            .WithSummary("Get all projects");

        group.MapGet("/{id:guid}", async (
                Guid id,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var project = await mediator.Send(new GetProjectByIdQuery(id), ct);
                if (project is null)
                    return Results.Problem($"Project with id {id} not found", statusCode: StatusCodes.Status404NotFound);

                return Results.Ok(project);
            })
            .Produces<ProjectDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get project by id");

        group.MapPost("/{projectId:guid}/members", async (
                Guid projectId,
                AddProjectMemberCommand command,
                IMediator mediator,
                CancellationToken ct) =>
            {
                await mediator.Send(command with { ProjectId = projectId.ToString() }, ct);
                return Results.Created($"/api/projects/{projectId}/members", new { });
            })
            .WithName("AddProjectMember")
            .WithSummary("Assign a member to a project");

        group.MapDelete("/{projectId:guid}/members/{userId}", async (
                Guid projectId,
                string userId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                await mediator.Send(new RemoveProjectMemberCommand(projectId.ToString(), userId), ct);
                return Results.NoContent();
            })
            .WithName("RemoveProjectMember")
            .WithSummary("Remove a member from a project");
    }
}
