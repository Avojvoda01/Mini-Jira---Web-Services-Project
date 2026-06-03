using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.User.Queries;

public class GetAllUsersHandler(IUserRepository repository) : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken ct)
    {
        var users = await repository.GetAllAsync(ct);
        return users
            .Select(user => new UserDto(user.Id.ToString(), user.Email, user.DisplayName, user.Role))
            .ToList();
    }
}
