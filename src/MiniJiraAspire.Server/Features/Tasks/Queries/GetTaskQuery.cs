using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Tasks.Queries;

public record GetTaskQuery(string TaskId) : IRequest<TaskItemDto?>;

public class GetTaskHandler(ITaskRepository repository) : IRequestHandler<GetTaskQuery, TaskItemDto?>
{
    public async Task<TaskItemDto?> Handle(GetTaskQuery request, CancellationToken ct)
    {
        var task = await repository.GetByIdAsync(Guid.Parse(request.TaskId), ct);
        if (task is null) return null;
        return new TaskItemDto(task.Id, task.Title, task.Description, task.Status, task.Priority, task.ProjectId, task.AssigneeId, task.EpicId);
    }
}
