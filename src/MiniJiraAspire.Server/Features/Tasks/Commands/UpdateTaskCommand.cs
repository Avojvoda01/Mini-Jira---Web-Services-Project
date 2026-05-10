using MediatR;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Tasks.Commands;

public record UpdateTaskCommand(string TaskId, string Title, string? Description) : IRequest;

public class UpdateTaskHandler(ITaskRepository repository) : IRequestHandler<UpdateTaskCommand>
{
    public async Task Handle(UpdateTaskCommand request, CancellationToken ct)
        => await repository.UpdateAsync(Guid.Parse(request.TaskId), request.Title, request.Description, ct);
}
