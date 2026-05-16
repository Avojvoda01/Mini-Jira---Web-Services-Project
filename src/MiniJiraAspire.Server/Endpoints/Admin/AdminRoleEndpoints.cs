namespace Microsoft.Extensions.Hosting.Admin.Roles;

public static class AdminRoleEndpoints
{
    public static void MapAdminRoleEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/admin/roles/{userId}", async (
                string userId,
                ChangeUserRoleCommand command,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.Ok();
            })
            .WithName("ChangeUserRole")
            .WithTags("Admin - Roles")
            .WithSummary("Change the role of a user (admin)");
    }
}

// TODO: replace Role with an enum once the RoleEndpoint is implemented.
public record ChangeUserRoleCommand(string Role);
