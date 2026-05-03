using Microsoft.EntityFrameworkCore;
using MiniJiraAspire.Server.Migrations;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Models.CommentDTO.Request;
using MiniJiraAspire.Server.Persistence.Repositories.Interfaces;

namespace MiniJiraAspire.Server.Persistence.Repositories
{
    public class CommentRepository(AppDbContext db) : ICommentRepository
    {
        public async Task<CommentDTO> CreateAsync(string taskId, CreateCommentRequest request, CancellationToken cancellationToken = default)
        {
            var comment = new Comment
            {
                TaskId = taskId,
                UserId = request.UserId,
                Content = request.Content,
                CreatedAtUtc = DateTime.UtcNow
            };

            db.Comments.Add(comment);
            await db.SaveChangesAsync(cancellationToken);

            return MapToDto(comment);
        }

        public async Task DeleteAsync(string taskId, Guid id, CancellationToken cancellationToken = default)
        {
            var comment = await db.Comments
                .FirstOrDefaultAsync(x => x.Id == id && x.TaskId == taskId, cancellationToken);

            if (comment is null)
            {
                throw new Exception("Comment delete");
            }

            db.Comments.Remove(comment);
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<CommentDTO> GetByIdAsync(string taskId, Guid id, CancellationToken cancellationToken = default)
        {
            var comment = await db.Comments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.TaskId == taskId, cancellationToken);

            if (comment is null)
            {
                throw new Exception("Comment get by id");
            }

            return MapToDto(comment);
        }

        public async Task UpdateAsync(string taskId, Guid id, UpdateCommentRequest request, CancellationToken cancellationToken = default)
        {
            var comment = await db.Comments
                .FirstOrDefaultAsync(x => x.Id == id && x.TaskId == taskId, cancellationToken);

            if (comment is null)
            {
                throw new Exception("Comment update");
            }

            comment.Content = request.Content;
            comment.UpdatedAtUtc = DateTime.UtcNow;

            await db.SaveChangesAsync(cancellationToken);
        }

        private static CommentDTO MapToDto(Comment comment) =>
            new(
                comment.Id,
                comment.TaskId,
                comment.UserId,
                comment.Content,
                comment.CreatedAtUtc,
                comment.UpdatedAtUtc);
    }
}
