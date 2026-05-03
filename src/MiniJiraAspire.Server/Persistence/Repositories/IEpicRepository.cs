using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Persistence.Repositories;

public interface IEpicRepository
{
    Task<List<EpicDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EpicDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EpicDto> CreateAsync(CreateEpicRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateEpicRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

}