using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Admin.Users;
using MiniJiraAspire.Server.Migrations;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Persistence.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<UserDto> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = new User
        {
            Email = command.Email,
            // only temporary, should be hashed in production
            PasswordHash = command.Password,
            DisplayName = command.DisplayName,
            Role = "Project Member"
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
}
