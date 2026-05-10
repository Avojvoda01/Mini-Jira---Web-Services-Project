using MediatR;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Tasks.Commands;

public record AssignUserCommand(string TaskId, string UserId) : IRequest;

public class AssignUserHandler(ITaskRepository repository) : IRequestHandler<AssignUserCommand>
{
    public async Task Handle(AssignUserCommand request, CancellationToken ct)
        => await repository.AssignUserAsync(Guid.Parse(request.TaskId), Guid.Parse(request.UserId), ct);
}
