using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Comment.Commands;

public class UpdateCommentHandler(ICommentRepository repository) : IRequestHandler<UpdateCommentCommand, CommentDto?>
{
    public async Task<CommentDto?> Handle(UpdateCommentCommand request, CancellationToken ct)
    {
        var comment = await repository.UpdateAsync(request.TaskId, request.CommentId, request.Content, ct);
        return comment is null
            ? null
            : new CommentDto(comment.Id, comment.TaskId, comment.UserId, comment.Content, comment.CreatedAtUtc, comment.UpdatedAtUtc);
    }
}
