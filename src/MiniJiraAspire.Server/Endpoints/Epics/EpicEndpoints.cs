using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Endpoints.Epics;

public static class EpicEndpoints
{
    public static void MapEpicEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/epics")
            .WithTags("Epics");

        group.MapPost("/", CreateEpic)
            .WithName("CreateEpic")
            .WithSummary("Create a new epic");

        group.MapPut("/{id:guid}", UpdateEpic)
            .Produces<EpicDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("UpdateEpic")
            .WithSummary("Edit an existing epic");

        group.MapDelete("/{id:guid}", DeleteEpic)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("DeleteEpic")
            .WithSummary("Delete an epic");

        group.MapGet("/", GetAllEpics)
            .Produces<List<EpicDto>>(StatusCodes.Status200OK)
            .WithName("GetEpics")
            .WithSummary("Get all epics");

        group.MapGet("/{id:guid}", GetEpicById)
            .Produces<EpicDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get epic by id");
    }

    private static async Task<Ok<List<EpicDto>>> GetAllEpics(
        IMediator mediator,
        CancellationToken ct)
    {
        var epics = await mediator.Send(new GetAllEpicsQuery(), ct);
        return TypedResults.Ok(epics);
    }

    private static async Task<Results<Ok<EpicDto>, ProblemHttpResult>> GetEpicById(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var epic = await mediator.Send(new GetEpicByIdQuery(id), ct);
        return epic is null
            ? TypedResults.Problem($"Epic with id {id} not found", statusCode: StatusCodes.Status404NotFound)
            : TypedResults.Ok(epic);
    }

    private static async Task<Created<EpicDto>> CreateEpic(
        CreateEpicRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var epic = await mediator.Send(new CreateEpicCommand(request.Name, request.Description, request.ProjectId), ct);
        return TypedResults.Created($"/api/v1/epics/{epic.Id}", epic);
    }

    private static async Task<Results<Ok<EpicDto>, ProblemHttpResult>> UpdateEpic(
        Guid id,
        UpdateEpicRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var epic = await mediator.Send(new UpdateEpicCommand(id, request.Name, request.Description), ct);
        return epic is null
            ? TypedResults.Problem($"Epic with id {id} not found", statusCode: StatusCodes.Status404NotFound)
            : TypedResults.Ok(epic);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteEpic(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var deleted = await mediator.Send(new DeleteEpicCommand(id), ct);
        return deleted
            ? TypedResults.NoContent()
            : TypedResults.Problem($"Epic with id {id} not found", statusCode: StatusCodes.Status404NotFound);
    }
}
