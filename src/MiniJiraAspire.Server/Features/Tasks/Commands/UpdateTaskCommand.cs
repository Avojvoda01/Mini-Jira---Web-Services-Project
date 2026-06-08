using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Tasks.Commands;

public class UpdateTaskHandler(ITaskRepository repository) : IRequestHandler<UpdateTaskCommand, TaskItemDto?>
{
    public async Task<TaskItemDto?> Handle(UpdateTaskCommand request, CancellationToken ct)
    {
        var estimate = request.EstimateMinutes > 0 ? request.EstimateMinutes : null;
        var task = await repository.UpdateAsync(Guid.Parse(request.TaskId), request.Title, request.Description, estimate, request.UpdatedById, ct);
        return task is null
            ? null
            : new TaskItemDto(task.Id, task.Title, task.Description, task.Status, task.Priority, task.ProjectId, task.AssigneeId, task.EpicId, task.CreatedById, task.UpdatedById, task.CreatedAtUtc, task.UpdatedAtUtc, task.EstimateMinutes);
    }
}
