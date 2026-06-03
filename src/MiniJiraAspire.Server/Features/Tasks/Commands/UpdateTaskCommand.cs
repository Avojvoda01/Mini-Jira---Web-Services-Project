using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Tasks.Commands;

public class UpdateTaskHandler(ITaskRepository repository) : IRequestHandler<UpdateTaskCommand>
{
    public async Task Handle(UpdateTaskCommand request, CancellationToken ct)
        => await repository.UpdateAsync(Guid.Parse(request.TaskId), request.Title, request.Description, ct);
}
