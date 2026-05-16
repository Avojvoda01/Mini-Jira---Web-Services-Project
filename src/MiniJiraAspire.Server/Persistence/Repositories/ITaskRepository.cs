using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Persistence.Repositories;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllAsync(string? search, string? status, string? priority, string? assigneeId, string? epicId, string? projectId, CancellationToken cancellationToken = default);
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TaskItem> CreateAsync(TaskItem task, CancellationToken cancellationToken = default);
    Task<TaskItem> UpdateAsync(Guid id, string title, string? description, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task ChangeStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);
    Task ChangePriorityAsync(Guid id, string priority, CancellationToken cancellationToken = default);
    Task AssignUserAsync(Guid id, Guid? userId, CancellationToken cancellationToken = default);
    Task AssignEpicAsync(Guid id, Guid? epicId, CancellationToken cancellationToken = default);
}
