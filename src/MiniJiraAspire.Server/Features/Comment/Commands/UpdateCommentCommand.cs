using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Comment.Commands;

public class UpdateCommentHandler(ICommentRepository repository) : IRequestHandler<UpdateCommentCommand>
{
    public Task Handle(UpdateCommentCommand request, CancellationToken ct)
        => repository.UpdateAsync(request.TaskId, request.CommentId, request.Content, ct);
}
