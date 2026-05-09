using MediatR;

namespace MiniJiraAspire.Server.Features.Project.Commands;

public record RemoveProjectMemberCommand(string ProjectId, string UserId) : IRequest;

public class RemoveProjectMemberHandler : IRequestHandler<RemoveProjectMemberCommand>
{
    public Task Handle(RemoveProjectMemberCommand request, CancellationToken ct)
        => Task.CompletedTask;
}
