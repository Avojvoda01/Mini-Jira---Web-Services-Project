using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;
using EpicEntity = MiniJiraAspire.Server.Models.Epic;

namespace MiniJiraAspire.Server.Features.Epic;

public record CreateEpicCommand(string Name, string? Description, Guid ProjectId) : IRequest<EpicDto>;

public class CreateEpicHandler(IEpicRepository repository) : IRequestHandler<CreateEpicCommand, EpicDto>
{
    public async Task<EpicDto> Handle(CreateEpicCommand request, CancellationToken ct)
    {
        var epic = await repository.CreateAsync(new EpicEntity
        {
            Name = request.Name,
            Description = request.Description,
            ProjectId = request.ProjectId
        }, ct);

        return new EpicDto(epic.Id, epic.Name, epic.Description ?? string.Empty, epic.ProjectId, epic.CreatedAtUtc, epic.UpdatedAtUtc);
    }
}