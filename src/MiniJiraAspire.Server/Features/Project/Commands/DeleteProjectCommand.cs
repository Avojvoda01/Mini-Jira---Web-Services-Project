using MediatR;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Project.Commands;

public record DeleteProjectCommand(Guid ProjectId) : IRequest;

public class DeleteProjectHandler(IProjectRepository repository) : IRequestHandler<DeleteProjectCommand>
{
    public async Task Handle(DeleteProjectCommand request, CancellationToken ct)
    {
        await repository.DeleteAsync(request.ProjectId, ct);
    }
}
