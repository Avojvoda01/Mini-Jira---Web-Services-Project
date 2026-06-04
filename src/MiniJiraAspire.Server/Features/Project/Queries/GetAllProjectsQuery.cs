using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Project.Queries;

public class GetAllProjectsHandler(IProjectRepository repository) : IRequestHandler<GetAllProjectsQuery, List<ProjectDto>>
{
    public async Task<List<ProjectDto>> Handle(GetAllProjectsQuery request, CancellationToken ct)
    {
        var projects = await repository.GetAllAsync(ct);
        return projects
            .Select(project => new ProjectDto(
                project.Id,
                project.Name,
                project.Description,
                [.. project.Members.Select(member => member.UserId.ToString())],
                project.CreatedById,
                project.CreatedAtUtc,
                project.UpdatedAtUtc))
            .ToList();
    }
}
