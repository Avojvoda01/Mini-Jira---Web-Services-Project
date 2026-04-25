using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Features.Project.GetAllProjectsQuery;

public interface IGetAllProjectsQuery
{
    Task<List<ProjectDto>> ExecuteAsync(CancellationToken cancellationToken = default);
}
