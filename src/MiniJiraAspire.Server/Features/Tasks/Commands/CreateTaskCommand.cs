using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Tasks.Commands;

public record CreateTaskCommand(string Title, string? Description, string ProjectId) : IRequest<TaskItemDto>;

public class CreateTaskHandler(ITaskRepository repository) : IRequestHandler<CreateTaskCommand, TaskItemDto>
{
    public async Task<TaskItemDto> Handle(CreateTaskCommand request, CancellationToken ct)
    {
        var task = await repository.CreateAsync(new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            ProjectId = Guid.Parse(request.ProjectId)
        }, ct);

        return new TaskItemDto(task.Id, task.Title, task.Description, task.Status, task.Priority, task.ProjectId, task.AssigneeId, task.EpicId, task.CreatedAtUtc, task.UpdatedAtUtc);
    }
}
