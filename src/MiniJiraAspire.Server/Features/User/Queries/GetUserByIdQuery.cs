using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.User.Queries;

public class GetUserByIdHandler(IUserRepository repository) : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        var user = await repository.GetByIdAsync(request.UserId, ct);
        return user is null
            ? null
            : new UserDto(user.Id.ToString(), user.Email, user.DisplayName, user.Role);
    }
}
