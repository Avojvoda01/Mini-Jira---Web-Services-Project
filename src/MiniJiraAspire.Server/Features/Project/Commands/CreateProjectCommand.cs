using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Project.Commands;

public record CreateProjectCommand(string Name, string? Description) : IRequest<ProjectDto>;

public class CreateProjectHandler(IProjectRepository repository) : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    public async Task<ProjectDto> Handle(CreateProjectCommand request, CancellationToken ct)
    {
        var project = new Models.Project
        {
            Name = request.Name,
            Description = request.Description
        };
        var created = await repository.CreateAsync(project, ct);
        return new ProjectDto(created.Id, created.Name, created.Description, [], created.CreatedAtUtc, created.UpdatedAtUtc);
    }
}
