using Microsoft.EntityFrameworkCore;
using MiniJiraAspire.Server.Migrations;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Persistence.Repositories;

public class EpicRepository(AppDbContext db) : IEpicRepository
{
    public Task<List<Epic>> GetAllAsync(Guid? projectId = null, CancellationToken cancellationToken = default)
    {
        var query = db.Epics.AsNoTracking().AsQueryable();
        if (projectId.HasValue)
            query = query.Where(e => e.ProjectId == projectId.Value);
        return query.OrderBy(e => e.Name).ToListAsync(cancellationToken);
    }

    public Task<Epic?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => db.Epics
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<Epic> CreateAsync(Epic epic, CancellationToken cancellationToken = default)
    {
        db.Epics.Add(epic);
        await db.SaveChangesAsync(cancellationToken);
        return epic;
    }

    public async Task<Epic?> UpdateAsync(Guid id, string name, string? description, Guid? updatedById = null, CancellationToken cancellationToken = default)
    {
        var epic = await db.Epics.FindAsync([id], cancellationToken);
        if (epic is null) return null;

        epic.Name = name;
        epic.Description = description;
        epic.UpdatedById = updatedById;
        await db.SaveChangesAsync(cancellationToken);
        return epic;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var epic = await db.Epics.FindAsync([id], cancellationToken);
        if (epic is null) return false;

        var assignedTasks = await db.TaskItems
            .Where(task => task.EpicId == id)
            .ToListAsync(cancellationToken);

        foreach (var task in assignedTasks)
        {
            task.EpicId = null;
        }

        db.Epics.Remove(epic);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}