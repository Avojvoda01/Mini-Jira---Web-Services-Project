using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;
using EpicEntity = MiniJiraAspire.Server.Models.Epic;

namespace MiniJiraAspire.Server.Features.Epic.Commands;

public class CreateEpicHandler(IEpicRepository repository, IProjectRepository projectRepository) : IRequestHandler<CreateEpicCommand, EpicDto?>
{
    public async Task<EpicDto?> Handle(CreateEpicCommand request, CancellationToken ct)
    {
        if (await projectRepository.GetByIdAsync(request.ProjectId, ct) is null)
        {
            return null;
        }

        var epic = await repository.CreateAsync(new EpicEntity
        {
            Name = request.Name,
            Description = request.Description,
            ProjectId = request.ProjectId,
            CreatedById = request.CreatedById,
        }, ct);

        return new EpicDto(epic.Id, epic.Name, epic.Description ?? string.Empty, epic.ProjectId, epic.CreatedById, epic.UpdatedById, epic.CreatedAtUtc, epic.UpdatedAtUtc);
    }
}