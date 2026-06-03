using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Project.Commands;

public class DeleteProjectHandler(IProjectRepository repository) : IRequestHandler<DeleteProjectCommand, bool>
{
    public Task<bool> Handle(DeleteProjectCommand request, CancellationToken ct)
        => repository.DeleteAsync(request.ProjectId, ct);
}
