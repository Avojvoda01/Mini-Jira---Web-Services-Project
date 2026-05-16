using Microsoft.EntityFrameworkCore;
using MiniJiraAspire.Server.Migrations;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Persistence.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<List<UserDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        return db.Users
            .AsNoTracking()
            .OrderBy(user => user.DisplayName)
            .Select(user => new UserDto(
                user.Id.ToString(),
                user.Email,
                user.DisplayName,
                user.Role))
            .ToListAsync(cancellationToken);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = new User
        {
            Email = request.Email,
            // only temporary, should be hashed in production
            PasswordHash = request.Password,
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
