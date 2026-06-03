using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Tasks.Commands;

public class AssignUserHandler(ITaskRepository repository) : IRequestHandler<AssignUserCommand, TaskItemDto?>
{
    public async Task<TaskItemDto?> Handle(AssignUserCommand request, CancellationToken ct)
    {
        var userId = string.IsNullOrWhiteSpace(request.UserId)
            ? (Guid?)null
            : Guid.Parse(request.UserId);

        var task = await repository.AssignUserAsync(Guid.Parse(request.TaskId), userId, ct);
        return task is null
            ? null
            : new TaskItemDto(task.Id, task.Title, task.Description, task.Status, task.Priority, task.ProjectId, task.AssigneeId, task.EpicId, task.CreatedAtUtc, task.UpdatedAtUtc);
    }
}
