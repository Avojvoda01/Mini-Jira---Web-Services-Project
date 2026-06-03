using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Persistence.Repositories;

public interface ICommentRepository
{
    Task<List<Comment>> GetAllAsync(string taskId, CancellationToken cancellationToken = default);
    Task<Comment?> GetByIdAsync(string taskId, Guid id, CancellationToken cancellationToken = default);
    Task<Comment> CreateAsync(Comment comment, CancellationToken cancellationToken = default);
    Task<Comment?> UpdateAsync(string taskId, Guid id, string content, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string taskId, Guid id, CancellationToken cancellationToken = default);
}