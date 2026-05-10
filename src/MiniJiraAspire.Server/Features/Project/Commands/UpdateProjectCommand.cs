using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Project.Commands;

public record UpdateProjectCommand(Guid Id, string Name, string? Description) : IRequest<ProjectDto>;

public class UpdateProjectHandler(IProjectRepository repository) : IRequestHandler<UpdateProjectCommand, ProjectDto>
{
    public async Task<ProjectDto> Handle(UpdateProjectCommand request, CancellationToken ct)
    {
        var updated = await repository.UpdateAsync(request.Id, request.Name, request.Description, ct);
        return new ProjectDto(updated.Id, updated.Name, updated.Description, updated.CreatedAtUtc, updated.UpdatedAtUtc);
    }
}
