using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Project.CreateProjectCommand;

public class CreateProjectCommand(IProjectRepository repository) : ICreateProjectCommand
{
    public async Task<ProjectDto> ExecuteAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var project = new Models.Project
        {
            Name = request.Name,
            Description = request.Description
        };

        var created = await repository.CreateAsync(project, cancellationToken);
        return new ProjectDto(created.Id, created.Name, created.Description);
    }
}
