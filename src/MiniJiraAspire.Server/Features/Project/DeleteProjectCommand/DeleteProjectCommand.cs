using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Project.DeleteProjectCommand;

public class DeleteProjectCommand(IProjectRepository repository) : IDeleteProjectCommand
{
    public Task ExecuteAsync(int id, CancellationToken cancellationToken = default)
        => repository.DeleteAsync(id, cancellationToken);
}
