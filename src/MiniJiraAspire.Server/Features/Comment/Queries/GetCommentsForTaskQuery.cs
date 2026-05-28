using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Comment.Queries;

public record GetCommentsForTaskQuery(string TaskId) : IRequest<CommentDto[]>;

public class GetCommentsForTaskHandler(ICommentRepository repository) : IRequestHandler<GetCommentsForTaskQuery, CommentDto[]>
{
    public async Task<CommentDto[]> Handle(GetCommentsForTaskQuery request, CancellationToken ct)
    {
        var comments = await repository.GetAllAsync(request.TaskId, ct);
        return comments
            .Select(comment => new CommentDto(comment.Id, comment.TaskId, comment.UserId, comment.Content, comment.CreatedAtUtc, comment.UpdatedAtUtc))
            .ToArray();
    }
}