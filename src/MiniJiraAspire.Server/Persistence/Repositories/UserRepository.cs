using Microsoft.EntityFrameworkCore;
using MiniJiraAspire.Server.Migrations;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Persistence.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<List<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await db.Users
            .AsNoTracking()
            .OrderBy(user => user.DisplayName)
            .Select(user => new UserDto(
                user.Id.ToString(),
                user.Email,
                user.DisplayName,
                user.Role))
            .ToListAsync(cancellationToken);
    }

    public async Task<UserDto?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userId, out var id))
        {
            return null;
        }

        return await db.Users
            .AsNoTracking()
            .Where(user => user.Id == id)
            .Select(user => new UserDto(
                user.Id.ToString(),
                user.Email,
                user.DisplayName,
                user.Role))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLower();

        return db.Users.FirstOrDefaultAsync(
            user => user.Email.ToLower() == normalizedEmail,
            cancellationToken);
    }

    public async Task<UserDto> CreateAsync(CreateUserData request, CancellationToken cancellationToken = default)
    {
        var user = new User
        {
            Email = request.Email,
            PasswordHash = request.PasswordHash,
            DisplayName = request.DisplayName,
            // TODO: replace the string role with an enum once the RoleEndpoint is implemented.
            Role = "User"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return new UserDto(
            user.Id.ToString(),
            user.Email,
            user.DisplayName,
            user.Role);
    }

    public async Task DeleteAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userId, out var id))
        {
            throw new Exception("Invalid user id");
        }

        var user = await db.Users.FindAsync([id], cancellationToken);

        if (user is null)
        {
            throw new Exception("User delete");
        }

        db.Users.Remove(user);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserDto> ChangeRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userId, out var id))
        {
            throw new Exception("Invalid user id");
        }

        var user = await db.Users.FindAsync([id], cancellationToken);

        if (user is null)
        {
            throw new Exception("User role update");
        }

        user.Role = role;
        await db.SaveChangesAsync(cancellationToken);

        return new UserDto(
            user.Id.ToString(),
            user.Email,
            user.DisplayName,
            user.Role);
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
