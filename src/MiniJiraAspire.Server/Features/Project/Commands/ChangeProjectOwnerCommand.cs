using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Project.Commands;

public class ChangeProjectOwnerHandler(IProjectRepository repository) : IRequestHandler<ChangeProjectOwnerCommand, bool>
{
    public Task<bool> Handle(ChangeProjectOwnerCommand request, CancellationToken ct)
        => repository.ChangeOwnerAsync(request.ProjectId, request.NewOwnerId, ct);
}
