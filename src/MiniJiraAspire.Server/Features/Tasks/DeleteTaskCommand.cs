using MediatR;

namespace MiniJiraAspire.Server.Features.Tasks;

public record DeleteTaskCommand(string TaskId) : IRequest;

public class DeleteTaskHandler : IRequestHandler<DeleteTaskCommand>
{
    public Task Handle(DeleteTaskCommand request, CancellationToken ct)
        => Task.CompletedTask;
}
