using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.User.Commands;

public record ChangeUserRoleCommand(string UserId, string Role) : IRequest<ChangeUserRoleResult>;

public record ChangeUserRoleResult(UserDto? User, Dictionary<string, string[]>? ValidationErrors, bool NotFound)
{
    public static ChangeUserRoleResult Success(UserDto user) => new(user, null, false);

    public static ChangeUserRoleResult ValidationFailed(Dictionary<string, string[]> errors) => new(null, errors, false);

    public static ChangeUserRoleResult UserNotFound() => new(null, null, true);
}

public class ChangeUserRoleHandler(IUserRepository repository) : IRequestHandler<ChangeUserRoleCommand, ChangeUserRoleResult>
{
    public async Task<ChangeUserRoleResult> Handle(ChangeUserRoleCommand request, CancellationToken ct)
    {
        var role = request.Role.Trim();

        if (!UserRoles.All.Contains(role, StringComparer.OrdinalIgnoreCase))
        {
            return ChangeUserRoleResult.ValidationFailed(new Dictionary<string, string[]>
            {
                [nameof(request.Role)] = [$"Role must be one of: {string.Join(", ", UserRoles.All)}."]
            });
        }

        var normalizedRole = UserRoles.All.First(allowedRole =>
            string.Equals(allowedRole, role, StringComparison.OrdinalIgnoreCase));

        var user = await repository.ChangeRoleAsync(request.UserId, normalizedRole, ct);

        return user is null
            ? ChangeUserRoleResult.UserNotFound()
            : ChangeUserRoleResult.Success(new UserDto(user.Id.ToString(), user.Email, user.DisplayName, user.Role));
    }
}
