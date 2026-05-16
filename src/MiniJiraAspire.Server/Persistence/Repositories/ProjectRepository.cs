using Microsoft.EntityFrameworkCore;
using MiniJiraAspire.Server.Migrations;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Persistence.Repositories;

public class ProjectRepository(AppDbContext db) : IProjectRepository
{
    public Task<List<Project>> GetAllAsync(CancellationToken cancellationToken = default)
        => db.Projects
            .Include(project => project.Members)
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

    public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => db.Projects
            .Include(project => project.Members)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default)
    {
        db.Projects.Add(project);
        await db.SaveChangesAsync(cancellationToken);
        return project;
    }

    public async Task<Project> UpdateAsync(Guid id, string name, string description, CancellationToken cancellationToken = default)
    {
        var project = await db.Projects.FindAsync([id], cancellationToken);
        if (project is null)
            throw new Exception($"Project with id {id} not found");

        project.Name = name;
        project.Description = description;
        await db.SaveChangesAsync(cancellationToken);
        return project;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await db.Projects.FindAsync([id], cancellationToken);
        if (project is null)
            throw new Exception($"Project with id {id} not found");

        db.Projects.Remove(project);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddMemberAsync(Guid projectId, Guid userId, string role, CancellationToken cancellationToken = default)
    {
        var projectExists = await db.Projects.AnyAsync(project => project.Id == projectId, cancellationToken);
        if (!projectExists)
            throw new Exception($"Project with id {projectId} not found");

        var userExists = await db.Users.AnyAsync(user => user.Id == userId, cancellationToken);
        if (!userExists)
            throw new Exception($"User with id {userId} not found");

        var alreadyMember = await db.ProjectMembers.AnyAsync(
            member => member.ProjectId == projectId && member.UserId == userId,
            cancellationToken);

        if (alreadyMember)
            return;

        db.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = projectId,
            UserId = userId,
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        var membership = await db.ProjectMembers.FirstOrDefaultAsync(
            member => member.ProjectId == projectId && member.UserId == userId,
            cancellationToken);

        if (membership is null)
            return;

        db.ProjectMembers.Remove(membership);
        await db.SaveChangesAsync(cancellationToken);
    }
}
