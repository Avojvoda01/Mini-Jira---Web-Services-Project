using MediatR;

namespace MiniJiraAspire.Server.Features.Tasks;

public record ChangeStatusCommand(string TaskId, string Status) : IRequest;

public class ChangeStatusHandler : IRequestHandler<ChangeStatusCommand>
{
    public Task Handle(ChangeStatusCommand request, CancellationToken ct)
        => Task.CompletedTask;
}
