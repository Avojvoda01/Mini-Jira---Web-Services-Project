namespace MiniJiraAspire.Server.Features.Project.DeleteProjectCommand;

public interface IDeleteProjectCommand
{
    Task ExecuteAsync(Guid id, CancellationToken cancellationToken = default);
}
