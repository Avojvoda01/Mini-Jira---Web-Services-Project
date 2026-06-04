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

    public async Task<Project?> UpdateAsync(Guid id, string name, string description, CancellationToken cancellationToken = default)
    {
        var project = await db.Projects.FindAsync([id], cancellationToken);
        if (project is null) return null;

        project.Name = name;
        project.Description = description;
        await db.SaveChangesAsync(cancellationToken);
        return project;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await db.Projects.FindAsync([id], cancellationToken);
        if (project is null) return false;

        db.Projects.Remove(project);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AddMemberAsync(Guid projectId, Guid userId, string role, CancellationToken cancellationToken = default)
    {
        var projectExists = await db.Projects.AnyAsync(project => project.Id == projectId, cancellationToken);
        if (!projectExists)
            return false;

        var userExists = await db.Users.AnyAsync(user => user.Id == userId, cancellationToken);
        if (!userExists)
            return false;

        var alreadyMember = await db.ProjectMembers.AnyAsync(
            member => member.ProjectId == projectId && member.UserId == userId,
            cancellationToken);

        if (alreadyMember)
            return true;

        db.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = projectId,
            UserId = userId,
        });

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        var membership = await db.ProjectMembers.FirstOrDefaultAsync(
            member => member.ProjectId == projectId && member.UserId == userId,
            cancellationToken);

        if (membership is null)
            return false;

        db.ProjectMembers.Remove(membership);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ChangeOwnerAsync(Guid projectId, Guid newOwnerId, CancellationToken cancellationToken = default)
    {
        var project = await db.Projects.FindAsync([projectId], cancellationToken);
        if (project is null) return false;

        var userExists = await db.Users.AnyAsync(u => u.Id == newOwnerId, cancellationToken);
        if (!userExists) return false;

        project.CreatedById = newOwnerId;
        project.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
