using MediatR;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Project.Commands;

public record RemoveProjectMemberCommand(string ProjectId, string UserId) : IRequest;

public class RemoveProjectMemberHandler(IProjectRepository repository) : IRequestHandler<RemoveProjectMemberCommand>
{
    public async Task Handle(RemoveProjectMemberCommand request, CancellationToken ct)
    {
        await repository.RemoveMemberAsync(
            Guid.Parse(request.ProjectId),
            Guid.Parse(request.UserId),
            ct);
    }
}
