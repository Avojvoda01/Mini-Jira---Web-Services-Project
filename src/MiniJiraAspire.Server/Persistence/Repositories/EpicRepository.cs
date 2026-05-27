using Microsoft.EntityFrameworkCore;
using MiniJiraAspire.Server.Migrations;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Persistence.Repositories;

public class EpicRepository(AppDbContext db) : IEpicRepository
{
    public Task<List<Epic>> GetAllAsync(CancellationToken cancellationToken = default)
        => db.Epics
            .AsNoTracking()
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);

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

    public async Task<Epic> UpdateAsync(Guid id, string name, string? description, CancellationToken cancellationToken = default)
    {
        var epic = await db.Epics.FindAsync([id], cancellationToken)
            ?? throw new Exception($"Epic with id {id} not found");

        epic.Name = name;
        epic.Description = description;
        await db.SaveChangesAsync(cancellationToken);
        return epic;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var epic = await db.Epics.FindAsync([id], cancellationToken)
            ?? throw new Exception($"Epic with id {id} not found");

        var assignedTasks = await db.TaskItems
            .Where(task => task.EpicId == id)
            .ToListAsync(cancellationToken);

        foreach (var task in assignedTasks)
        {
            task.EpicId = null;
        }

        db.Epics.Remove(epic);
        await db.SaveChangesAsync(cancellationToken);
    }
}