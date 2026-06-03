using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.User.Commands;

public class ChangeUserRoleHandler(IUserRepository repository) : IRequestHandler<ChangeUserRoleCommand, ChangeUserRoleResponse>
{
    public async Task<ChangeUserRoleResponse> Handle(ChangeUserRoleCommand request, CancellationToken ct)
    {
        if (!UserRoleExtensions.TryParse(request.Role, out var role))
        {
            return ChangeUserRoleResponse.ValidationFailed(new Dictionary<string, string[]>
            {
                [nameof(request.Role)] = [$"Role must be one of: {string.Join(", ", UserRoleExtensions.All.Select(r => r.ToRoleString()))}."]
            });
        }

        var user = await repository.ChangeRoleAsync(request.UserId, role.ToRoleString(), ct);

        return user is null
            ? ChangeUserRoleResponse.UserNotFound()
            : ChangeUserRoleResponse.Success(new UserDto(user.Id.ToString(), user.Email, user.DisplayName, user.Role));
    }
}
