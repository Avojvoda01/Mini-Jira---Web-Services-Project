using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;


namespace Microsoft.Extensions.Hosting.Admin.Users;

public static class AdminUserEndpoints
{
    public static void MapAdminUserEndpoints(this IEndpointRouteBuilder app)
    {

        var group = app.MapGroup("/api/admin/users")
            .WithTags("Admin - Users");

        group.MapPost("/", CreateUser)
            .WithName("AdminCreateUser")
            .WithSummary("Create a new user (admin)");

        group.MapDelete("/{userId}", DeleteUser)
            .WithName("AdminDeleteUser")
            .WithSummary("Delete a user (admin)");
    
    }

    private static async Task<Created<UserDto>> CreateUser(
        CreateUserCommand command,
        IUserRepository repository,
        CancellationToken cancellationToken)
    {
        var user = await repository.CreateAsync(command, cancellationToken);
        return TypedResults.Created($"/api/users/{user.Id}", user);
    }

    private static async Task<NoContent> DeleteUser(
        string userId,
        IUserRepository repository,
        CancellationToken cancellationToken)
    {
        await repository.DeleteAsync(userId, cancellationToken);
        return TypedResults.NoContent();
    }
}






public record CreateUserCommand(string Email, string Password, string DisplayName);