using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Persistence.Repositories;

public interface IEpicRepository
{
    Task<List<Epic>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Epic?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Epic> CreateAsync(Epic epic, CancellationToken cancellationToken = default);
    Task<Epic> UpdateAsync(Guid id, string name, string? description, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}