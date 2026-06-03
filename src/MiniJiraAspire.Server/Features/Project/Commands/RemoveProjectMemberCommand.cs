using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Project.Commands;

public class RemoveProjectMemberHandler(IProjectRepository repository) : IRequestHandler<RemoveProjectMemberCommand, bool>
{
    public Task<bool> Handle(RemoveProjectMemberCommand request, CancellationToken ct)
        => repository.RemoveMemberAsync(
            Guid.Parse(request.ProjectId),
            Guid.Parse(request.UserId),
            ct);
}
