using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Tasks.Commands;

public class ChangeStatusHandler(ITaskRepository repository) : IRequestHandler<ChangeStatusCommand>
{
    public async Task Handle(ChangeStatusCommand request, CancellationToken ct)
        => await repository.ChangeStatusAsync(Guid.Parse(request.TaskId), request.Status, ct);
}
