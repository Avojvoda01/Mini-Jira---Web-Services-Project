using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace Microsoft.Extensions.Hosting.Admin.Roles;

public static class AdminRoleEndpoints
{
    private static readonly string[] AllowedRoles = ["Admin", "Project Manager", "Project Member"];

    public static void MapAdminRoleEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/admin/roles/{userId}", ChangeUserRole)
            .WithName("ChangeUserRole")
            .WithTags("Admin - Roles")
            .WithSummary("Change the role of a user (admin)");
    }

    private static async Task<Results<Ok<UserDto>, ValidationProblem, ProblemHttpResult>> ChangeUserRole(
        string userId,
        ChangeUserRoleCommand command,
        IUserRepository repository,
        CancellationToken cancellationToken)
    {
        var role = command.Role.Trim();

        if (!AllowedRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(command.Role)] = [$"Role must be one of: {string.Join(", ", AllowedRoles)}."]
            });
        }

        var normalizedRole = AllowedRoles.First(allowedRole =>
            string.Equals(allowedRole, role, StringComparison.OrdinalIgnoreCase));

        try
        {
            var user = await repository.ChangeRoleAsync(userId, normalizedRole, cancellationToken);
            return TypedResults.Ok(user);
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(ex.Message, statusCode: StatusCodes.Status404NotFound);
        }
    }
}
public record ChangeUserRoleCommand(string Role);
