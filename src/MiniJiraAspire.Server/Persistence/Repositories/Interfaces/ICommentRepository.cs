using MiniJiraAspire.Server.Models.CommentDTO.Request;

namespace MiniJiraAspire.Server.Persistence.Repositories.Interfaces
{
    public interface ICommentRepository
    {
        Task<CommentDTO[]> GetAllAsync(string taskId, CancellationToken cancellationToken = default);
        Task<CommentDTO> GetByIdAsync(string taskId, Guid id, CancellationToken cancellationToken = default);
        Task<CommentDTO> CreateAsync(string taskId, CreateCommentRequest request, CancellationToken cancellationToken = default);
        Task UpdateAsync(string taskId, Guid id, UpdateCommentRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(string taskId, Guid id, CancellationToken cancellationToken = default);
    }
}
