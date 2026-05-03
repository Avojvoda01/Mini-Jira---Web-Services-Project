using MediatR;
using MiniJiraAspire.Server.Persistence.Repositories.Interfaces;

namespace MiniJiraAspire.Server.Features.Comment;

public record DeleteCommentCommand(string TaskId, int CommentId) : IRequest;

public class DeleteCommentHandler(ICommentRepository repository) : IRequestHandler<DeleteCommentCommand>
{
    public Task Handle(DeleteCommentCommand request, CancellationToken ct)
        => repository.DeleteAsync(request.TaskId, request.CommentId, ct);
}
