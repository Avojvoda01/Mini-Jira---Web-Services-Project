using MediatR;

namespace MiniJiraAspire.Server.Features.Tasks;

public record AssignUserCommand(string TaskId, string UserId) : IRequest;

public class AssignUserHandler : IRequestHandler<AssignUserCommand>
{
    public Task Handle(AssignUserCommand request, CancellationToken ct)
        => Task.CompletedTask;
}
