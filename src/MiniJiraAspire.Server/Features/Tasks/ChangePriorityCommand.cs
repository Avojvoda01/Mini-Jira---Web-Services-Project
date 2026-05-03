using MediatR;

namespace MiniJiraAspire.Server.Features.Tasks;

public record ChangePriorityCommand(string TaskId, string Priority) : IRequest;

public class ChangePriorityHandler : IRequestHandler<ChangePriorityCommand>
{
    public Task Handle(ChangePriorityCommand request, CancellationToken ct)
        => Task.CompletedTask;
}
