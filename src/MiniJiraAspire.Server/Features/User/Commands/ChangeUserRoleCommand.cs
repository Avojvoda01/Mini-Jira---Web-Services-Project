using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.User.Commands;

public class ChangeUserRoleHandler(IUserRepository repository) : IRequestHandler<ChangeUserRoleCommand, ChangeUserRoleResponse>
{
    public async Task<ChangeUserRoleResponse> Handle(ChangeUserRoleCommand request, CancellationToken ct)
    {
        if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role) || !Enum.IsDefined(role))
        {
            return ChangeUserRoleResponse.ValidationFailed(new Dictionary<string, string[]>
            {
                [nameof(request.Role)] = [$"Role must be one of: {string.Join(", ", Enum.GetNames<UserRole>())}."]
            });
        }

        var user = await repository.ChangeRoleAsync(request.UserId, role, ct);

        return user is null
            ? ChangeUserRoleResponse.UserNotFound()
            : ChangeUserRoleResponse.Success(new UserDto(user.Id.ToString(), user.Email, user.DisplayName, user.Role, user.CreatedAtUtc));
    }
}
