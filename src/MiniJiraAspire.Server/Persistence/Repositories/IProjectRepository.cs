using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Persistence.Repositories;

public interface IProjectRepository
{
    Task<List<Project>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default);

    Task<Project> UpdateAsync(Guid id, string name, string description, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
