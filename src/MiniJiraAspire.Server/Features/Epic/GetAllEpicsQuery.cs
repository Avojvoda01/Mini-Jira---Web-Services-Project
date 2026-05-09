using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Epic;

public record GetAllEpicsQuery : IRequest<List<EpicDto>>;

public class GetAllEpicsHandler(IEpicRepository repository) : IRequestHandler<GetAllEpicsQuery, List<EpicDto>>
{
    public Task<List<EpicDto>> Handle(GetAllEpicsQuery request, CancellationToken ct)
        => repository.GetAllAsync(ct);
}
