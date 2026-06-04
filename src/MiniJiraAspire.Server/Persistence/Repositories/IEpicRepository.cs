using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Persistence.Repositories;

public interface IEpicRepository
{
    Task<List<Epic>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Epic?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Epic> CreateAsync(Epic epic, CancellationToken cancellationToken = default);
    Task<Epic?> UpdateAsync(Guid id, string name, string? description, Guid? updatedById = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}