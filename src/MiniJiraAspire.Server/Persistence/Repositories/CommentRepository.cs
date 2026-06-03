using Microsoft.EntityFrameworkCore;
using MiniJiraAspire.Server.Migrations;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Persistence.Repositories;

public class CommentRepository(AppDbContext db) : ICommentRepository
{
    public async Task<List<Comment>> GetAllAsync(string taskId, CancellationToken cancellationToken = default)
        => await db.Comments
            .AsNoTracking()
            .Where(x => x.TaskId == taskId)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public Task<Comment?> GetByIdAsync(string taskId, Guid id, CancellationToken cancellationToken = default)
        => db.Comments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TaskId == taskId, cancellationToken);

    public async Task<Comment> CreateAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        db.Comments.Add(comment);
        await db.SaveChangesAsync(cancellationToken);
        return comment;
    }

    public async Task<Comment?> UpdateAsync(string taskId, Guid id, string content, CancellationToken cancellationToken = default)
    {
        var comment = await db.Comments
            .FirstOrDefaultAsync(x => x.Id == id && x.TaskId == taskId, cancellationToken);
        if (comment is null) return null;

        comment.Content = content;
        comment.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return comment;
    }

    public async Task<bool> DeleteAsync(string taskId, Guid id, CancellationToken cancellationToken = default)
    {
        var comment = await db.Comments
            .FirstOrDefaultAsync(x => x.Id == id && x.TaskId == taskId, cancellationToken);
        if (comment is null) return false;

        db.Comments.Remove(comment);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}