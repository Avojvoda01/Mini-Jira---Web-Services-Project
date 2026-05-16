using MediatR;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Tasks.Commands;

/// <summary>
/// Command to assign or unassign an Epic from a Task.
/// </summary>
/// <param name="TaskId">The ID of the Task to update.</param>
/// <param name="EpicId">The ID of the Epic to assign. Pass null to unassign the Epic from the Task.</param>
public record AssignEpicCommand(string TaskId, string? EpicId) : IRequest;

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
