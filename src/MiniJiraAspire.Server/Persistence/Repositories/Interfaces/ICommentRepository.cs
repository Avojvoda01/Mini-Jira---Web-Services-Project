using MiniJiraAspire.Server.Models.CommentDTO.Request;

namespace MiniJiraAspire.Server.Persistence.Repositories.Interfaces
{
    public interface ICommentRepository
    {
        Task<CommentDTO> GetByIdAsync(string taskId, int id, CancellationToken cancellationToken = default);
        Task<CommentDTO> CreateAsync(string taskId, CreateCommentRequest request, CancellationToken cancellationToken = default);
        Task UpdateAsync(string taskId, int id, UpdateCommentRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(string taskId, int id, CancellationToken cancellationToken = default);
    }
}
