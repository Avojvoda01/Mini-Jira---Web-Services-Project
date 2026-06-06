using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Tasks.Commands;

public class ChangePriorityHandler(ITaskRepository repository) : IRequestHandler<ChangePriorityCommand, TaskItemDto?>
{
    public async Task<TaskItemDto?> Handle(ChangePriorityCommand request, CancellationToken ct)
    {
        var task = await repository.ChangePriorityAsync(Guid.Parse(request.TaskId), request.Priority, request.UpdatedById, ct);
        return task is null
            ? null
            : new TaskItemDto(task.Id, task.Title, task.Description, task.Status, task.Priority, task.ProjectId, task.AssigneeId, task.EpicId, task.CreatedById, task.UpdatedById, task.CreatedAtUtc, task.UpdatedAtUtc);
    }
}
