using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Epic.Commands;

public class UpdateEpicHandler(IEpicRepository repository) : IRequestHandler<UpdateEpicCommand, EpicDto?>
{
    public async Task<EpicDto?> Handle(UpdateEpicCommand request, CancellationToken ct)
    {
        var epic = await repository.UpdateAsync(request.Id, request.Name, request.Description, ct);
        return epic is null
            ? null
            : new EpicDto(epic.Id, epic.Name, epic.Description ?? string.Empty, epic.ProjectId, epic.CreatedAtUtc, epic.UpdatedAtUtc);
    }
}
