using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Features.Project.GetProjectByIdQuery;

public interface IGetProjectByIdQuery
{
    Task<ProjectDto?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default);
}
