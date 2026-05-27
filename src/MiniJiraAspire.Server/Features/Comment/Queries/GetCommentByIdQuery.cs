using MediatR;
using MiniJiraAspire.Server.Models.CommentDTO.Request;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Comment.Queries;

public record GetCommentByIdQuery(string TaskId, Guid CommentId) : IRequest<CommentDTO?>;

public class GetCommentByIdHandler(ICommentRepository repository) : IRequestHandler<GetCommentByIdQuery, CommentDTO?>
{
    public async Task<CommentDTO?> Handle(GetCommentByIdQuery request, CancellationToken ct)
    {
        var comment = await repository.GetByIdAsync(request.TaskId, request.CommentId, ct);
        return comment is null
            ? null
            : new CommentDTO(comment.Id, comment.TaskId, comment.UserId, comment.Content, comment.CreatedAtUtc, comment.UpdatedAtUtc);
    }
}