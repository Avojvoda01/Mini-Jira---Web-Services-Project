using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;
using CommentEntity = MiniJiraAspire.Server.Models.Comment;

namespace MiniJiraAspire.Server.Features.Comment.Commands;

public class CreateCommentHandler(ICommentRepository repository, ITaskRepository taskRepository) : IRequestHandler<CreateCommentCommand, CommentDto?>
{
    public async Task<CommentDto?> Handle(CreateCommentCommand request, CancellationToken ct)
    {
        if (!Guid.TryParse(request.TaskId, out var taskId)
            || await taskRepository.GetByIdAsync(taskId, ct) is null)
        {
            return null;
        }

        var comment = await repository.CreateAsync(new CommentEntity
        {
            TaskId = request.TaskId,
            UserId = request.UserId,
            Content = request.Content
        }, ct);

        return new CommentDto(comment.Id, comment.TaskId, comment.UserId, comment.Content, comment.CreatedAtUtc, comment.UpdatedAtUtc);
    }
}