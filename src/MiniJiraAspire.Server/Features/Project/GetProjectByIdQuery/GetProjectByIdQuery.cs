using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Project.GetProjectByIdQuery;

public class GetProjectByIdQuery(IProjectRepository repository) : IGetProjectByIdQuery
{
    public async Task<ProjectDto?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await repository.GetByIdAsync(id, cancellationToken);
        if (project is null)
            return null;

        return new ProjectDto(
            project.Id, 
            project.Name, 
            project.Description);
    }
}
