using MediatR;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Tasks.Commands;

public record ChangeStatusCommand(string TaskId, string Status) : IRequest;

public class ChangeStatusHandler(ITaskRepository repository) : IRequestHandler<ChangeStatusCommand>
{
    public async Task Handle(ChangeStatusCommand request, CancellationToken ct)
        => await repository.ChangeStatusAsync(Guid.Parse(request.TaskId), request.Status, ct);
}
