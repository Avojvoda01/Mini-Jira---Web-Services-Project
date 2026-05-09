using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Features.Project.CreateProjectCommand;
using MiniJiraAspire.Server.Features.Project.DeleteProjectCommand;
using MiniJiraAspire.Server.Features.Project.GetAllProjectsQuery;
using MiniJiraAspire.Server.Features.Project.GetProjectByIdQuery;
using MiniJiraAspire.Server.Features.Project.UpdateProjectCommand;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Endpoints.Projects;

public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects")
            .WithTags("Projects");

        group.MapPost("/", CreateProject)
            .WithName("CreateProject")
            .WithSummary("Create a new project");

        group.MapPut("/{id:guid}", UpdateProject)
            .WithName("UpdateProject")
            .WithSummary("Edit an existing project");

        group.MapDelete("/{id:int}", DeleteProject)
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
    }

    private static async Task<Ok<List<ProjectDto>>> GetAllProjects(
        IGetAllProjectsQuery query,
        CancellationToken cancellationToken)
    {
        var projects = await query.ExecuteAsync(cancellationToken);
        return TypedResults.Ok(projects);
    }

    private static async Task<Results<Ok<ProjectDto>, ProblemHttpResult>> GetProjectById(
        Guid id,
        IGetProjectByIdQuery query,
        CancellationToken cancellationToken)
    {
        var project = await query.ExecuteAsync(id, cancellationToken);
        if (project is null)
            return TypedResults.Problem($"Project with id {id} not found", statusCode: StatusCodes.Status404NotFound);

        return TypedResults.Ok(project);
    }

    private static async Task<Created<ProjectDto>> CreateProject(
        CreateProjectRequest request,
        ICreateProjectCommand command,
        CancellationToken cancellationToken)
    {
        var project = await command.ExecuteAsync(request, cancellationToken);
        return TypedResults.Created($"/api/projects/{project.Id}", project);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> UpdateProject(
        Guid id,
        UpdateProjectRequest request,
        IUpdateProjectCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            await command.ExecuteAsync(id, request, cancellationToken);
            return TypedResults.NoContent();
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(ex.Message, statusCode: StatusCodes.Status404NotFound);
        }
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteProject(
        Guid id,
        IDeleteProjectCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            await command.ExecuteAsync(id, cancellationToken);
            return TypedResults.NoContent();
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(ex.Message, statusCode: StatusCodes.Status404NotFound);
        }
    }
}
