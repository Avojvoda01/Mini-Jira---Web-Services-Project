using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Features.Project.GetProjectByIdQuery;

public interface IGetProjectByIdQuery
{
    Task<ProjectDto?> ExecuteAsync(int id, CancellationToken cancellationToken = default);
}
