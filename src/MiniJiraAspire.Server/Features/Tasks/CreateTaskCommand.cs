using MediatR;

namespace MiniJiraAspire.Server.Features.Tasks;

public record CreateTaskCommand(string Title, string? Description, string ProjectId) : IRequest<object>;

public class CreateTaskHandler : IRequestHandler<CreateTaskCommand, object>
{
    public Task<object> Handle(CreateTaskCommand request, CancellationToken ct)
        => Task.FromResult<object>(new { Id = "new-id" });
}
