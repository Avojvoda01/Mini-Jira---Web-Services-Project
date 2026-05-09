using MediatR;

namespace MiniJiraAspire.Server.Features.Tasks;

public record GetTasksQuery(
    string? Search,
    string? Status,
    string? Priority,
    string? AssigneeId,
    string? EpicId) : IRequest<object[]>;

public class GetTasksHandler : IRequestHandler<GetTasksQuery, object[]>
{
    public Task<object[]> Handle(GetTasksQuery request, CancellationToken ct)
        => Task.FromResult(Array.Empty<object>());
}
