using MediatR;

namespace MiniJiraAspire.Server.Features.Tasks;

public record UpdateTaskCommand(string TaskId, string Title, string? Description) : IRequest;

public class UpdateTaskHandler : IRequestHandler<UpdateTaskCommand>
{
    public Task Handle(UpdateTaskCommand request, CancellationToken ct)
        => Task.CompletedTask;
}
