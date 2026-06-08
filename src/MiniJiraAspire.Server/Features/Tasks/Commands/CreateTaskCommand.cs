using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Tasks.Commands;

public class CreateTaskHandler(ITaskRepository repository) : IRequestHandler<CreateTaskCommand, TaskItemDto>
{
    public async Task<TaskItemDto> Handle(CreateTaskCommand request, CancellationToken ct)
    {
        var task = await repository.CreateAsync(new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            ProjectId = Guid.Parse(request.ProjectId),
            CreatedById = request.CreatedById,
            EstimateMinutes = request.EstimateMinutes > 0 ? request.EstimateMinutes : null,
        }, ct);

        return new TaskItemDto(task.Id, task.Title, task.Description, task.Status, task.Priority, task.ProjectId, task.AssigneeId, task.EpicId, task.CreatedById, task.UpdatedById, task.CreatedAtUtc, task.UpdatedAtUtc, task.EstimateMinutes);
    }
}
