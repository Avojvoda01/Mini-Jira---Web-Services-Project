using MediatR;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Tasks.Commands;

public record DeleteTaskCommand(string TaskId) : IRequest;

public class DeleteTaskHandler(ITaskRepository repository) : IRequestHandler<DeleteTaskCommand>
{
    public async Task Handle(DeleteTaskCommand request, CancellationToken ct)
        => await repository.DeleteAsync(Guid.Parse(request.TaskId), ct);
}
