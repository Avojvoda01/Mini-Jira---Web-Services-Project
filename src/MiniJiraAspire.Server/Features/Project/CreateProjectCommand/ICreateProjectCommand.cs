using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Features.Project.CreateProjectCommand;

public interface ICreateProjectCommand
{
    Task<ProjectDto> ExecuteAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);
}
