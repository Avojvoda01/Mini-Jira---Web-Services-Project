using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Project.UpdateProjectCommand;

public class UpdateProjectCommand(IProjectRepository repository) : IUpdateProjectCommand
{
    public async Task ExecuteAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default)
    {
        await repository.UpdateAsync(id, request.Name, request.Description, cancellationToken);
    }
}
