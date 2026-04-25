using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Project.GetAllProjectsQuery;

public class GetAllProjectsQuery(IProjectRepository repository) : IGetAllProjectsQuery
{
    public async Task<List<ProjectDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var projects = await repository.GetAllAsync(cancellationToken);
        return projects.Select(p => new ProjectDto(
            p.Id, 
            p.Name, 
            p.Description)
        ).ToList();
    }
}
