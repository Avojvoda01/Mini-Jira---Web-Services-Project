using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Epic;

public record GetEpicByIdQuery(Guid Id) : IRequest<EpicDto>;

public class GetEpicByIdHandler(IEpicRepository repository) : IRequestHandler<GetEpicByIdQuery, EpicDto>
{
    public Task<EpicDto> Handle(GetEpicByIdQuery request, CancellationToken ct)
        => repository.GetByIdAsync(request.Id, ct);
}
