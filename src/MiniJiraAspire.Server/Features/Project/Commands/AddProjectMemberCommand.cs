using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Project.Commands;

public class AddProjectMemberHandler(IProjectRepository repository) : IRequestHandler<AddProjectMemberCommand, bool>
{
    public Task<bool> Handle(AddProjectMemberCommand request, CancellationToken ct)
        => repository.AddMemberAsync(
            Guid.Parse(request.ProjectId),
            Guid.Parse(request.UserId),
            request.Role,
            ct);
}
