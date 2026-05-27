using MediatR;
using MiniJiraAspire.Server.Models.CommentDTO.Request;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Comment.Queries;

public record GetCommentsForTaskQuery(string TaskId) : IRequest<CommentDTO[]>;

public class GetCommentsForTaskHandler(ICommentRepository repository) : IRequestHandler<GetCommentsForTaskQuery, CommentDTO[]>
{
    public async Task<CommentDTO[]> Handle(GetCommentsForTaskQuery request, CancellationToken ct)
    {
        var comments = await repository.GetAllAsync(request.TaskId, ct);
        return comments
            .Select(comment => new CommentDTO(comment.Id, comment.TaskId, comment.UserId, comment.Content, comment.CreatedAtUtc, comment.UpdatedAtUtc))
            .ToArray();
    }
}