using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Tasks.Commands;

public class AssignEpicHandler(ITaskRepository repository) : IRequestHandler<AssignEpicCommand, TaskItemDto?>
{
    public async Task<TaskItemDto?> Handle(AssignEpicCommand request, CancellationToken ct)
    {
        Guid? epicId = null;
        if (!string.IsNullOrWhiteSpace(request.EpicId))
        {
            epicId = Guid.Parse(request.EpicId);
        }

        var task = await repository.AssignEpicAsync(Guid.Parse(request.TaskId), epicId, ct);
        return task is null
            ? null
            : new TaskItemDto(task.Id, task.Title, task.Description, task.Status, task.Priority, task.ProjectId, task.AssigneeId, task.EpicId, task.CreatedAtUtc, task.UpdatedAtUtc);
    }
}
