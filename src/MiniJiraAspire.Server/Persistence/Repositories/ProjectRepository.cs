using Microsoft.EntityFrameworkCore;
using MiniJiraAspire.Server.Migrations;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Persistence.Repositories;

public class ProjectRepository(AppDbContext db) : IProjectRepository
{
    public Task<List<Project>> GetAllAsync(CancellationToken cancellationToken = default)
        => db.Projects
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

    public Task<Project?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default)
    {
        db.Projects.Add(project);
        await db.SaveChangesAsync(cancellationToken);
        return project;
    }

    public async Task<Project> UpdateAsync(int id, string name, string description, CancellationToken cancellationToken = default)
    {
        var project = await db.Projects.FindAsync([id], cancellationToken);
        if (project is null)
            throw new Exception($"Project with id {id} not found");

        project.Name = name;
        project.Description = description;
        await db.SaveChangesAsync(cancellationToken);
        return project;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var project = await db.Projects.FindAsync([id], cancellationToken);
        if (project is null)
            throw new Exception($"Project with id {id} not found");

        db.Projects.Remove(project);
        await db.SaveChangesAsync(cancellationToken);
    }
}
