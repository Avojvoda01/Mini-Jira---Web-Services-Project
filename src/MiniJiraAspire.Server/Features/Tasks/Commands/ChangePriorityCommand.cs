using MediatR;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Tasks.Commands;

public record ChangePriorityCommand(string TaskId, string Priority) : IRequest;

public class ChangePriorityHandler(ITaskRepository repository) : IRequestHandler<ChangePriorityCommand>
{
    public async Task Handle(ChangePriorityCommand request, CancellationToken ct)
        => await repository.ChangePriorityAsync(Guid.Parse(request.TaskId), request.Priority, ct);
}
