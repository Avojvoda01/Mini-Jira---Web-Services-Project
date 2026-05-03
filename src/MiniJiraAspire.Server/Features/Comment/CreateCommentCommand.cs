using MediatR;
using MiniJiraAspire.Server.Models.CommentDTO.Request;
using MiniJiraAspire.Server.Persistence.Repositories.Interfaces;

namespace MiniJiraAspire.Server.Features.Comment;

public record CreateCommentCommand(string TaskId, string Content, int? UserId) : IRequest<CommentDTO>;

public class CreateCommentHandler(ICommentRepository repository) : IRequestHandler<CreateCommentCommand, CommentDTO>
{
    public Task<CommentDTO> Handle(CreateCommentCommand request, CancellationToken ct)
        => repository.CreateAsync(request.TaskId, new CreateCommentRequest(request.Content, request.UserId), ct);
}
