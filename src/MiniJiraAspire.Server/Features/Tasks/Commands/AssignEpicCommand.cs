using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Tasks.Commands;

public class AssignEpicHandler(ITaskRepository repository) : IRequestHandler<AssignEpicCommand>
{
    public async Task Handle(AssignEpicCommand request, CancellationToken ct)
    {
        Guid? epicId = null;
        if (!string.IsNullOrWhiteSpace(request.EpicId))
        {
            epicId = Guid.Parse(request.EpicId);
        }

        await repository.AssignEpicAsync(Guid.Parse(request.TaskId), epicId, ct);
    }
}
