using MediatR;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Comment;

public record UpdateCommentCommand(string TaskId, Guid CommentId, string Content) : IRequest;

public class UpdateCommentHandler(ICommentRepository repository) : IRequestHandler<UpdateCommentCommand>
{
    public Task Handle(UpdateCommentCommand request, CancellationToken ct)
        => repository.UpdateAsync(request.TaskId, request.CommentId, request.Content, ct);
}
