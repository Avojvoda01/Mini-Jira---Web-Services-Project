using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Endpoints.Projects;

public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/projects")
            .WithTags("Projects");

        group.MapPost("/", CreateProject)
            .WithName("CreateProject")
            .WithSummary("Create a new project");

        group.MapPut("/{id:guid}", UpdateProject)
            .WithName("UpdateProject")
            .WithSummary("Edit an existing project");

        group.MapDelete("/{id:guid}", DeleteProject)
            .WithName("DeleteProject")
            .WithSummary("Delete a project");

        group.MapGet("/", GetAllProjects)
            .Produces<List<ProjectDto>>(StatusCodes.Status200OK)
            .WithName("GetProjects")
            .WithSummary("Get all projects");

        group.MapGet("/{id:guid}", GetProjectById)
            .Produces<ProjectDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get project by id");

        group.MapPost("/{projectId:guid}/members", AddProjectMember)
            .WithName("AddProjectMember")
            .WithSummary("Assign a member to a project");

        group.MapDelete("/{projectId:guid}/members/{userId}", RemoveProjectMember)
            .WithName("RemoveProjectMember")
            .WithSummary("Remove a member from a project");
    }

    private static async Task<Created<ProjectDto>> CreateProject(
        CreateProjectRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new CreateProjectCommand(request.Name, request.Description), ct);
        return TypedResults.Created($"/api/v1/projects/{result.Id}", result);
    }

    private static async Task<IResult> UpdateProject(
        Guid id,
        UpdateProjectRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var project = await mediator.Send(new UpdateProjectCommand(id, request.Name, request.Description), ct);
        return project is null
            ? Results.Problem($"Project with id {id} not found", statusCode: StatusCodes.Status404NotFound)
            : Results.Ok(project);
    }

    private static async Task<IResult> DeleteProject(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var deleted = await mediator.Send(new DeleteProjectCommand(id), ct);
        return deleted
            ? Results.NoContent()
            : Results.Problem($"Project with id {id} not found", statusCode: StatusCodes.Status404NotFound);
    }

    private static async Task<Ok<List<ProjectDto>>> GetAllProjects(
        IMediator mediator,
        CancellationToken ct)
    {
        var projects = await mediator.Send(new GetAllProjectsQuery(), ct);
        return TypedResults.Ok(projects);
    }

    private static async Task<IResult> GetProjectById(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var project = await mediator.Send(new GetProjectByIdQuery(id), ct);
        return project is null
            ? Results.Problem($"Project with id {id} not found", statusCode: StatusCodes.Status404NotFound)
            : Results.Ok(project);
    }

    private static async Task<IResult> AddProjectMember(
        Guid projectId,
        AddProjectMemberCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        var added = await mediator.Send(command with { ProjectId = projectId.ToString() }, ct);
        return added
            ? Results.Created($"/api/projects/{projectId}/members", new { })
            : Results.Problem("Project or user not found", statusCode: StatusCodes.Status404NotFound);
    }

    private static async Task<NoContent> RemoveProjectMember(
        Guid projectId,
        string userId,
        IMediator mediator,
        CancellationToken ct)
    {
        await mediator.Send(new RemoveProjectMemberCommand(projectId.ToString(), userId), ct);
        return TypedResults.NoContent();
    }
}
