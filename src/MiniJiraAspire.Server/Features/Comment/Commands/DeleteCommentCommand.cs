using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Comment.Commands;

public class DeleteCommentHandler(ICommentRepository repository) : IRequestHandler<DeleteCommentCommand>
{
    public Task Handle(DeleteCommentCommand request, CancellationToken ct)
        => repository.DeleteAsync(request.TaskId, request.CommentId, ct);
}
