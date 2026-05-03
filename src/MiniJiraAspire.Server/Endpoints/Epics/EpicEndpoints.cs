using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Features.Epic;
using MiniJiraAspire.Server.Models;

namespace Microsoft.Extensions.Hosting.Epics;

public static class EpicEndpoints
{
    public static void MapEpicEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/epics")
            .WithTags("Epics");

        group.MapPost("/", CreateEpic)
            .WithName("CreateEpic")
            .WithSummary("Create a new epic");

        group.MapPut("/{id:int}", UpdateEpic)
            .WithName("UpdateEpic")
            .WithSummary("Edit an existing epic");

        group.MapDelete("/{id:int}", DeleteEpic)
            .WithName("DeleteEpic")
            .WithSummary("Delete an epic");

        group.MapGet("/", GetAllEpics)
            .Produces<List<EpicDto>>(StatusCodes.Status200OK)
            .WithName("GetEpics")
            .WithSummary("Get all epics");

        group.MapGet("/{id:int}", GetEpicById)
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

    private static async Task<Ok<EpicDto>> GetEpicById(
        int id,
        IMediator mediator,
        CancellationToken ct)
    {
        var epic = await mediator.Send(new GetEpicByIdQuery(id), ct);
        return TypedResults.Ok(epic);
    }

    private static async Task<Created<EpicDto>> CreateEpic(
        CreateEpicRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var epic = await mediator.Send(new CreateEpicCommand(request.Name, request.Description), ct);
        return TypedResults.Created($"/api/epics/{epic.Id}", epic);
    }

    private static async Task<NoContent> UpdateEpic(
        int id,
        UpdateEpicRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        await mediator.Send(new UpdateEpicCommand(id, request.Name, request.Description), ct);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteEpic(
        int id,
        IMediator mediator,
        CancellationToken ct)
    {
        await mediator.Send(new DeleteEpicCommand(id), ct);
        return TypedResults.NoContent();
    }
}
