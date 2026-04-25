using Microsoft.Extensions.Hosting.Admin.Users;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Persistence.Repositories;

public interface IUserRepository
{
    Task<UserDto> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken = default);

    Task DeleteAsync(string userId, CancellationToken cancellationToken = default);
}
