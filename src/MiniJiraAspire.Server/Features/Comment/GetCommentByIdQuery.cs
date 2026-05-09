using MediatR;
using MiniJiraAspire.Server.Models.CommentDTO.Request;
using MiniJiraAspire.Server.Persistence.Repositories.Interfaces;

namespace MiniJiraAspire.Server.Features.Comment;

public record GetCommentByIdQuery(string TaskId, Guid CommentId) : IRequest<CommentDTO>;

public class GetCommentByIdHandler(ICommentRepository repository) : IRequestHandler<GetCommentByIdQuery, CommentDTO>
{
    public Task<CommentDTO> Handle(GetCommentByIdQuery request, CancellationToken ct)
        => repository.GetByIdAsync(request.TaskId, request.CommentId, ct);
}
