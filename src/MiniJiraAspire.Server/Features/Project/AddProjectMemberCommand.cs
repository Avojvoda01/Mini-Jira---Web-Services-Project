using MediatR;

namespace MiniJiraAspire.Server.Features.Project;

public record AddProjectMemberCommand(string ProjectId, string UserId, string Role) : IRequest;

public class AddProjectMemberHandler : IRequestHandler<AddProjectMemberCommand>
{
    public Task Handle(AddProjectMemberCommand request, CancellationToken ct)
        => Task.CompletedTask;
}
