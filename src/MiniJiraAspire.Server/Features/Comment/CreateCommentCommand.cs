using MediatR;
using MiniJiraAspire.Server.Models.CommentDTO.Request;
using MiniJiraAspire.Server.Persistence.Repositories;
using CommentEntity = MiniJiraAspire.Server.Models.Comment;

namespace MiniJiraAspire.Server.Features.Comment;

public record CreateCommentCommand(string TaskId, string Content, Guid? UserId) : IRequest<CommentDTO>;

public class CreateCommentHandler(ICommentRepository repository) : IRequestHandler<CreateCommentCommand, CommentDTO>
{
    public async Task<CommentDTO> Handle(CreateCommentCommand request, CancellationToken ct)
    {
        var comment = await repository.CreateAsync(new CommentEntity
        {
            TaskId = request.TaskId,
            UserId = request.UserId,
            Content = request.Content
        }, ct);

        return new CommentDTO(comment.Id, comment.TaskId, comment.UserId, comment.Content, comment.CreatedAtUtc, comment.UpdatedAtUtc);
    }
}