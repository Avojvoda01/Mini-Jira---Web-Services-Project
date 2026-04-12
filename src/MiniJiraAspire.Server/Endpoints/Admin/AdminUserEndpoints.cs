namespace Microsoft.Extensions.Hosting.Admin.Users;

public static class AdminUserEndpoints
{
    public static void MapAdminUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/users", async (
                CreateUserCommand command,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.Created("/api/admin/users/new-id", new { Id = "new-id" });
            })
            .WithName("AdminCreateUser")
            .WithTags("Admin - Users")
            .WithSummary("Create a new user (admin)");

        app.MapDelete("/api/admin/users/{userId}", async (
                string userId,
                CancellationToken ct) =>
            {
                // TODO: implement logic
                return Results.NoContent();
            })
            .WithName("AdminDeleteUser")
            .WithTags("Admin - Users")
            .WithSummary("Delete a user (admin)");
    }
}

public record CreateUserCommand(string Email, string Password, string DisplayName);