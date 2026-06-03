using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Project.Commands;

public class AddProjectMemberHandler(IProjectRepository repository) : IRequestHandler<AddProjectMemberCommand>
{
    public async Task Handle(AddProjectMemberCommand request, CancellationToken ct)
    {
        await repository.AddMemberAsync(
            Guid.Parse(request.ProjectId),
            Guid.Parse(request.UserId),
            request.Role,
            ct);
    }
}
