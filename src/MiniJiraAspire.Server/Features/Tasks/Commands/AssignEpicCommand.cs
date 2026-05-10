using MediatR;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Tasks.Commands;

public record AssignEpicCommand(string TaskId, string EpicId) : IRequest;

public class AssignEpicHandler(ITaskRepository repository) : IRequestHandler<AssignEpicCommand>
{
    public async Task Handle(AssignEpicCommand request, CancellationToken ct)
        => await repository.AssignEpicAsync(Guid.Parse(request.TaskId), Guid.Parse(request.EpicId), ct);
}
