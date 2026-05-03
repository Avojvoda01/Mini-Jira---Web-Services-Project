using MediatR;

namespace MiniJiraAspire.Server.Features.Project;

public record CreateProjectCommand(string Name, string? Description) : IRequest<object>;

public class CreateProjectHandler : IRequestHandler<CreateProjectCommand, object>
{
    public Task<object> Handle(CreateProjectCommand request, CancellationToken ct)
        => Task.FromResult<object>(new { Id = "new-id" });
}
