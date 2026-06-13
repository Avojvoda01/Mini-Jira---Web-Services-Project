using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using MiniJiraAspire.Server.Features.Project.Queries;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Endpoints.Projects;

public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/projects")
            .WithTags("Projects")
            .RequireAuthorization();

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
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AddProjectMember")
            .WithSummary("Assign a member to a project (owner or admin only)");

        group.MapDelete("/{projectId:guid}/members/{userId}", RemoveProjectMember)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("RemoveProjectMember")
            .WithSummary("Remove a member from a project (owner or admin only)");

        group.MapPatch("/{id:guid}/owner", ChangeProjectOwner)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("ChangeProjectOwner")
            .WithSummary("Change project owner (Admin only)");
    }

    private static async Task<Created<ProjectDto>> CreateProject(
        CreateProjectRequest request,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken ct)
    {
        var creatorId = GetUserId(user);
        var result = await mediator.Send(new CreateProjectCommand(request.Name, request.Description, creatorId), ct);
        return TypedResults.Created($"/api/v1/projects/{result.Id}", result);
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out var id) ? id : null;
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

    private static async Task<Results<NoContent, ProblemHttpResult>> AddProjectMember(
        Guid projectId,
        AddProjectMemberCommand command,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken ct)
    {
        var project = await mediator.Send(new GetProjectByIdQuery(projectId), ct);
        if (project is null)
            return TypedResults.Problem("Project not found", statusCode: StatusCodes.Status404NotFound);

        var callerId = GetUserId(user);
        if (!user.IsInRole("Admin") && project.CreatedById != callerId)
            return TypedResults.Problem("Only the project owner or an admin can manage members.", statusCode: StatusCodes.Status403Forbidden);

        var added = await mediator.Send(command with { ProjectId = projectId.ToString() }, ct);
        return added
            ? TypedResults.NoContent()
            : TypedResults.Problem("User not found", statusCode: StatusCodes.Status404NotFound);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> RemoveProjectMember(
        Guid projectId,
        string userId,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken ct)
    {
        var project = await mediator.Send(new GetProjectByIdQuery(projectId), ct);
        if (project is null)
            return TypedResults.Problem("Project not found", statusCode: StatusCodes.Status404NotFound);

        var callerId = GetUserId(user);
        if (!user.IsInRole("Admin") && project.CreatedById != callerId)
            return TypedResults.Problem("Only the project owner or an admin can manage members.", statusCode: StatusCodes.Status403Forbidden);

        var removed = await mediator.Send(new RemoveProjectMemberCommand(projectId.ToString(), userId), ct);
        return removed
            ? TypedResults.NoContent()
            : TypedResults.Problem("Project membership not found", statusCode: StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> ChangeProjectOwner(
        Guid id,
        ChangeProjectOwnerRequest request,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken ct)
    {
        if (!user.IsInRole("Admin"))
            return Results.Problem("Only admins can change the project owner.", statusCode: StatusCodes.Status403Forbidden);

        var changed = await mediator.Send(new ChangeProjectOwnerCommand(id, request.OwnerId), ct);
        return changed
            ? Results.NoContent()
            : Results.Problem("Project or user not found.", statusCode: StatusCodes.Status404NotFound);
    }
}
