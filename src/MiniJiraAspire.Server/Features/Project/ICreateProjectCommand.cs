using MediatR;

namespace MiniJiraAspire.Server.Features.Project;

public record DeleteProjectCommand(string ProjectId) : IRequest;

public class DeleteProjectHandler : IRequestHandler<DeleteProjectCommand>
{
    public Task Handle(DeleteProjectCommand request, CancellationToken ct)
        => Task.CompletedTask;
}
