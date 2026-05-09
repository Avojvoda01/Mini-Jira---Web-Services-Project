using MediatR;

namespace MiniJiraAspire.Server.Features.Tasks;

public record GetTaskQuery(string TaskId) : IRequest<object>;

public class GetTaskHandler : IRequestHandler<GetTaskQuery, object>
{
    public Task<object> Handle(GetTaskQuery request, CancellationToken ct)
        => Task.FromResult<object>(new { Id = request.TaskId });
}
