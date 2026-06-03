using Microsoft.EntityFrameworkCore;
using MiniJiraAspire.Server.Migrations;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Persistence.Repositories;

public class TaskRepository(AppDbContext db) : ITaskRepository
{
    public async Task<List<TaskItem>> GetAllAsync(string? search, string? status, string? priority, string? assigneeId, string? epicId, string? projectId, CancellationToken cancellationToken = default)
    {
        var query = db.TaskItems.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.Title.Contains(search) || (t.Description != null && t.Description.Contains(search)));

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(t => t.Status == status);

        if (!string.IsNullOrWhiteSpace(priority))
            query = query.Where(t => t.Priority == priority);

        if (!string.IsNullOrWhiteSpace(assigneeId) && Guid.TryParse(assigneeId, out var assigneeGuid))
            query = query.Where(t => t.AssigneeId == assigneeGuid);

        if (!string.IsNullOrWhiteSpace(epicId) && Guid.TryParse(epicId, out var epicGuid))
            query = query.Where(t => t.EpicId == epicGuid);

        if (!string.IsNullOrWhiteSpace(projectId) && Guid.TryParse(projectId, out var projectGuid))
            query = query.Where(t => t.ProjectId == projectGuid);

        return await query.OrderByDescending(t => t.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => db.TaskItems.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<TaskItem> CreateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        db.TaskItems.Add(task);
        await db.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<TaskItem?> UpdateAsync(Guid id, string title, string? description, CancellationToken cancellationToken = default)
    {
        var task = await db.TaskItems.FindAsync([id], cancellationToken);
        if (task is null) return null;

        task.Title = title;
        task.Description = description;
        await db.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await db.TaskItems.FindAsync([id], cancellationToken);
        if (task is null) return false;

        db.TaskItems.Remove(task);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<TaskItem?> ChangeStatusAsync(Guid id, string status, CancellationToken cancellationToken = default)
    {
        var task = await db.TaskItems.FindAsync([id], cancellationToken);
        if (task is null) return null;

        task.Status = status;
        await db.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<TaskItem?> ChangePriorityAsync(Guid id, string priority, CancellationToken cancellationToken = default)
    {
        var task = await db.TaskItems.FindAsync([id], cancellationToken);
        if (task is null) return null;

        task.Priority = priority;
        await db.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<TaskItem?> AssignUserAsync(Guid id, Guid? userId, CancellationToken cancellationToken = default)
    {
        var task = await db.TaskItems.FindAsync([id], cancellationToken);
        if (task is null) return null;

        task.AssigneeId = userId;
        await db.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<TaskItem?> AssignEpicAsync(Guid id, Guid? epicId, CancellationToken cancellationToken = default)
    {
        var task = await db.TaskItems.FindAsync([id], cancellationToken);
        if (task is null) return null;

        task.EpicId = epicId;
        await db.SaveChangesAsync(cancellationToken);
        return task;
    }
}
