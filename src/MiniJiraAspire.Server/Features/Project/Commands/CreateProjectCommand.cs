using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.Project.Commands;

public class CreateProjectHandler(IProjectRepository repository) : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    public async Task<ProjectDto> Handle(CreateProjectCommand request, CancellationToken ct)
    {
        var project = new Models.Project
        {
            Name = request.Name,
            Description = request.Description,
            CreatedById = request.CreatedById
        };

        if (request.CreatedById is { } creatorId && creatorId != Guid.Empty)
        {
            project.Members.Add(new ProjectMember
            {
                UserId = creatorId
            });
        }

        var created = await repository.CreateAsync(project, ct);
        return new ProjectDto(
            created.Id,
            created.Name,
            created.Description,
            [.. created.Members.Select(member => member.UserId.ToString())],
            created.CreatedById,
            created.CreatedAtUtc,
            created.UpdatedAtUtc);
    }
}
