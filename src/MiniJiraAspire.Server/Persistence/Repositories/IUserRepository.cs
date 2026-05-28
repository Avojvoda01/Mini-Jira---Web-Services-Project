using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Persistence.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(string userId, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string userId, CancellationToken cancellationToken = default);

    Task<User?> ChangeRoleAsync(string userId, string role, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> DisplayNameExistsAsync(string displayName, CancellationToken cancellationToken = default);
}
