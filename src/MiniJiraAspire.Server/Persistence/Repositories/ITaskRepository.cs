using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Persistence.Repositories;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllAsync(string? search, string? status, string? priority, string? assigneeId, string? epicId, string? projectId, CancellationToken cancellationToken = default);
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TaskItem> CreateAsync(TaskItem task, CancellationToken cancellationToken = default);
    Task<TaskItem?> UpdateAsync(Guid id, string title, string? description, Guid? updatedById = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TaskItem?> ChangeStatusAsync(Guid id, string status, Guid? updatedById = null, CancellationToken cancellationToken = default);
    Task<TaskItem?> ChangePriorityAsync(Guid id, string priority, Guid? updatedById = null, CancellationToken cancellationToken = default);
    Task<TaskItem?> AssignUserAsync(Guid id, Guid? userId, Guid? updatedById = null, CancellationToken cancellationToken = default);
    Task<TaskItem?> AssignEpicAsync(Guid id, Guid? epicId, Guid? updatedById = null, CancellationToken cancellationToken = default);
}
