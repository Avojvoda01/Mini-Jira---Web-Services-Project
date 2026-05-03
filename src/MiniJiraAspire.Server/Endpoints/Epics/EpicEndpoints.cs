using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

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

        group.MapPut("/{id:guid}", UpdateEpic)
            .WithName("UpdateEpic")
            .WithSummary("Edit an existing epic");

        group.MapDelete("/{id:guid}", DeleteEpic)
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
        IEpicRepository repository,
        CancellationToken cancellationToken)
    {
        var epics = await repository.GetAllAsync(cancellationToken);
        return TypedResults.Ok(epics);
    }
    
    private static async Task<Ok<EpicDto>>GetEpicById(Guid id,
        IEpicRepository repository,
        CancellationToken cancellationToken)
    {
        var epic = await repository.GetByIdAsync(id, cancellationToken);
        return TypedResults.Ok(epic);
    }
    
    private static async Task<Created<EpicDto>> CreateEpic(
        CreateEpicRequest request,
        IEpicRepository repository,
        CancellationToken cancellationToken)
    {
        var epic = await repository.CreateAsync(request, cancellationToken);
        return TypedResults.Created($"/api/epics/{epic.Id}", epic);
    }

    private static async Task<NoContent> DeleteEpic(
        Guid id,
        IEpicRepository repository,
        CancellationToken cancellationToken)
    {
        await repository.DeleteAsync(id, cancellationToken);
        return TypedResults.NoContent();
    }
    
    private static async Task<NoContent> UpdateEpic(
        Guid id,
        UpdateEpicRequest request,
        IEpicRepository repository,
        CancellationToken cancellationToken)
    {
        await repository.UpdateAsync(id, request, cancellationToken);
        return TypedResults.NoContent();
    }

}
