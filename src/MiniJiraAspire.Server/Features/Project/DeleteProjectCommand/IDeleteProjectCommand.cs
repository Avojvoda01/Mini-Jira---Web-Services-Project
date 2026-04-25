namespace MiniJiraAspire.Server.Features.Project.DeleteProjectCommand;

public interface IDeleteProjectCommand
{
    Task ExecuteAsync(int id, CancellationToken cancellationToken = default);
}
