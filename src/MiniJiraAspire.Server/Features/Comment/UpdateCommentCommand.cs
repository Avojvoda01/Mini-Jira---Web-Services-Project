using MediatR;
using MiniJiraAspire.Server.Models.CommentDTO.Request;
using MiniJiraAspire.Server.Persistence.Repositories.Interfaces;

namespace MiniJiraAspire.Server.Features.Comment;

public record UpdateCommentCommand(string TaskId, int CommentId, string Content) : IRequest;

public class UpdateCommentHandler(ICommentRepository repository) : IRequestHandler<UpdateCommentCommand>
{
    public Task Handle(UpdateCommentCommand request, CancellationToken ct)
        => repository.UpdateAsync(request.TaskId, request.CommentId, new UpdateCommentRequest(request.Content), ct);
}
