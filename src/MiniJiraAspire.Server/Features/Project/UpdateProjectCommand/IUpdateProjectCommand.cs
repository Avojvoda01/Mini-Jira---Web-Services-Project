using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Features.Project.UpdateProjectCommand;

public interface IUpdateProjectCommand
{
    Task ExecuteAsync(int id, UpdateProjectRequest request, CancellationToken cancellationToken = default);
}
