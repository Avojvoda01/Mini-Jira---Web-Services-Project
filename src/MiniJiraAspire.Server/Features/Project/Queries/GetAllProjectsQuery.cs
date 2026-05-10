using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Project.Queries;

public record GetAllProjectsQuery : IRequest<List<ProjectDto>>;

public class GetAllProjectsHandler(IProjectRepository repository) : IRequestHandler<GetAllProjectsQuery, List<ProjectDto>>
{
    public async Task<List<ProjectDto>> Handle(GetAllProjectsQuery request, CancellationToken ct)
    {
        var projects = await repository.GetAllAsync(ct);
        return projects.Select(p => new ProjectDto(p.Id, p.Name, p.Description, p.CreatedAtUtc, p.UpdatedAtUtc)).ToList();
    }
}
