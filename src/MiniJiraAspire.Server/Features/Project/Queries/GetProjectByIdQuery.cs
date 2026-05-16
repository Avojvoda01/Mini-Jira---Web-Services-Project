using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Project.Queries;

public record GetProjectByIdQuery(Guid Id) : IRequest<ProjectDto?>;

public class GetProjectByIdHandler(IProjectRepository repository) : IRequestHandler<GetProjectByIdQuery, ProjectDto?>
{
    public async Task<ProjectDto?> Handle(GetProjectByIdQuery request, CancellationToken ct)
    {
        var project = await repository.GetByIdAsync(request.Id, ct);
        return project is null
            ? null
            : new ProjectDto(
                project.Id,
                project.Name,
                project.Description,
                [.. project.Members.Select(member => member.UserId.ToString())],
                project.CreatedAtUtc,
                project.UpdatedAtUtc);
    }
}
