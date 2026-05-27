using Microsoft.EntityFrameworkCore;
using MiniJiraAspire.Server.Migrations;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Persistence.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
        => db.Users
            .AsNoTracking()
            .OrderBy(user => user.DisplayName)
            .ToListAsync(cancellationToken);

    public Task<User?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userId, out var id))
        {
            return Task.FromResult<User?>(null);
        }

        return db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLower();

        return db.Users.FirstOrDefaultAsync(
            user => user.Email.ToLower() == normalizedEmail,
            cancellationToken);
    }

    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<bool> DeleteAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userId, out var id))
        {
            return false;
        }

        var user = await db.Users.FindAsync([id], cancellationToken);

        if (user is null)
        {
            return false;
        }

        db.Users.Remove(user);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<User?> ChangeRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userId, out var id))
        {
            return null;
        }

        var user = await db.Users.FindAsync([id], cancellationToken);

        if (user is null)
        {
            return null;
        }

        user.Role = role;
        await db.SaveChangesAsync(cancellationToken);
        return user;
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLower();
        return db.Users.AnyAsync(user => user.Email.ToLower() == normalizedEmail, cancellationToken);
    }

    public Task<bool> DisplayNameExistsAsync(string displayName, CancellationToken cancellationToken = default)
    {
        var normalizedDisplayName = displayName.Trim().ToLower();
        return db.Users.AnyAsync(user => user.DisplayName.ToLower() == normalizedDisplayName, cancellationToken);
    }
}
