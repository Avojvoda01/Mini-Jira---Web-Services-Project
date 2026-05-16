using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Tasks.Queries;

public record GetTasksQuery(
    string? Search,
    string? Status,
    string? Priority,
    string? AssigneeId,
    string? EpicId,
    string? ProjectId) : IRequest<TaskItemDto[]>;

public class GetTasksHandler(ITaskRepository repository) : IRequestHandler<GetTasksQuery, TaskItemDto[]>
{
    public async Task<TaskItemDto[]> Handle(GetTasksQuery request, CancellationToken ct)
    {
        var tasks = await repository.GetAllAsync(request.Search, request.Status, request.Priority, request.AssigneeId, request.EpicId, request.ProjectId, ct);
        return tasks.Select(t => new TaskItemDto(t.Id, t.Title, t.Description, t.Status, t.Priority, t.ProjectId, t.AssigneeId, t.EpicId, t.CreatedAtUtc, t.UpdatedAtUtc)).ToArray();
    }
}
