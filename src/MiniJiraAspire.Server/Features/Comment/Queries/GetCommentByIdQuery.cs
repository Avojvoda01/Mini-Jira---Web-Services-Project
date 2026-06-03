using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Comment.Queries;

public class GetCommentByIdHandler(ICommentRepository repository) : IRequestHandler<GetCommentByIdQuery, CommentDto?>
{
    public async Task<CommentDto?> Handle(GetCommentByIdQuery request, CancellationToken ct)
    {
        var comment = await repository.GetByIdAsync(request.TaskId, request.CommentId, ct);
        return comment is null
            ? null
            : new CommentDto(comment.Id, comment.TaskId, comment.UserId, comment.Content, comment.CreatedAtUtc, comment.UpdatedAtUtc);
    }
}