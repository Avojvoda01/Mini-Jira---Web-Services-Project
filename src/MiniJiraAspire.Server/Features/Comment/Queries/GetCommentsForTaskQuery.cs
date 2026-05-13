using MediatR;
using MiniJiraAspire.Server.Models.CommentDTO.Request;
using MiniJiraAspire.Server.Persistence.Repositories.Interfaces;

namespace MiniJiraAspire.Server.Features.Comment.Queries;

public record GetCommentsForTaskQuery(string TaskId) : IRequest<CommentDTO[]>;

public class GetCommentsForTaskHandler(ICommentRepository repository) : IRequestHandler<GetCommentsForTaskQuery, CommentDTO[]>
{
    public Task<CommentDTO[]> Handle(GetCommentsForTaskQuery request, CancellationToken ct)
        => repository.GetAllAsync(request.TaskId, ct);
}
