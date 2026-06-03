using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Tasks.Commands;

public class UpdateTaskHandler(ITaskRepository repository) : IRequestHandler<UpdateTaskCommand, TaskItemDto?>
{
    public async Task<TaskItemDto?> Handle(UpdateTaskCommand request, CancellationToken ct)
    {
        var task = await repository.UpdateAsync(Guid.Parse(request.TaskId), request.Title, request.Description, ct);
        return task is null
            ? null
            : new TaskItemDto(task.Id, task.Title, task.Description, task.Status, task.Priority, task.ProjectId, task.AssigneeId, task.EpicId, task.CreatedAtUtc, task.UpdatedAtUtc);
    }
}
