using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace Microsoft.Extensions.Hosting.Epics;

public static class EpicEndpoints
{
    public static void MapEpicEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/epics", CreateEpic)
            .WithName("CreateEpic")
            .WithTags("Epics")
            .WithSummary("Create a new epic");

        app.MapPut("/api/epics/{id:int}", UpdateEpic)
            .WithName("UpdateEpic")
            .WithTags("Epics")
            .WithSummary("Edit an existing epic");

        app.MapDelete("/api/epics/{id:int}",DeleteEpic)
            .WithName("DeleteEpic")
            .WithTags("Epics")
            .WithSummary("Delete an epic");

        app.MapGet("/api/epics", GetAllEpics)
            .Produces<List<EpicDto>>(StatusCodes.Status200OK)
            .WithName("GetEpics")
            .WithTags("Epics")
            .WithSummary("Get all epics");
        
        app.MapGet("/api/epics/{id:int}", GetEpicById)
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
    
    private static async Task<Ok<EpicDto>>GetEpicById(int id,
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
        int id,
        IEpicRepository repository,
        CancellationToken cancellationToken)
    {
        await repository.DeleteAsync(id, cancellationToken);
        return TypedResults.NoContent();
    }
    
    private static async Task<NoContent> UpdateEpic(
        int id,
        UpdateEpicRequest request,
        IEpicRepository repository,
        CancellationToken cancellationToken)
    {
        await repository.UpdateAsync(id, request, cancellationToken);
        return TypedResults.NoContent();
    }

}
