using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Persistence.Repositories;

public interface IProjectRepository
{
    Task<List<Project>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default);

    Task<Project?> UpdateAsync(Guid id, string name, string description, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> AddMemberAsync(Guid projectId, Guid userId, string role, CancellationToken cancellationToken = default);

    Task<bool> RemoveMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);

    Task<bool> ChangeOwnerAsync(Guid projectId, Guid newOwnerId, CancellationToken cancellationToken = default);
}
