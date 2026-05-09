using MediatR;

namespace MiniJiraAspire.Server.Features.Tasks;

public record AssignEpicCommand(string TaskId, string EpicId) : IRequest;

public class AssignEpicHandler : IRequestHandler<AssignEpicCommand>
{
    public Task Handle(AssignEpicCommand request, CancellationToken ct)
        => Task.CompletedTask;
}
