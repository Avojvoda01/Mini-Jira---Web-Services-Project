using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Tasks.Commands;

public class DeleteTaskHandler(ITaskRepository repository) : IRequestHandler<DeleteTaskCommand, bool>
{
    public Task<bool> Handle(DeleteTaskCommand request, CancellationToken ct)
        => repository.DeleteAsync(Guid.Parse(request.TaskId), ct);
}
