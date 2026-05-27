using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Epic;

public record GetAllEpicsQuery : IRequest<List<EpicDto>>;

public class GetAllEpicsHandler(IEpicRepository repository) : IRequestHandler<GetAllEpicsQuery, List<EpicDto>>
{
    public async Task<List<EpicDto>> Handle(GetAllEpicsQuery request, CancellationToken ct)
    {
        var epics = await repository.GetAllAsync(ct);
        return epics
            .Select(epic => new EpicDto(epic.Id, epic.Name, epic.Description ?? string.Empty, epic.ProjectId, epic.CreatedAtUtc, epic.UpdatedAtUtc))
            .ToList();
    }
}