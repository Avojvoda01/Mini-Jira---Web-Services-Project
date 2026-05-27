using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Epic;

public record GetEpicByIdQuery(Guid Id) : IRequest<EpicDto?>;

public class GetEpicByIdHandler(IEpicRepository repository) : IRequestHandler<GetEpicByIdQuery, EpicDto?>
{
    public async Task<EpicDto?> Handle(GetEpicByIdQuery request, CancellationToken ct)
    {
        var epic = await repository.GetByIdAsync(request.Id, ct);
        return epic is null
            ? null
            : new EpicDto(epic.Id, epic.Name, epic.Description ?? string.Empty, epic.ProjectId, epic.CreatedAtUtc, epic.UpdatedAtUtc);
    }
}