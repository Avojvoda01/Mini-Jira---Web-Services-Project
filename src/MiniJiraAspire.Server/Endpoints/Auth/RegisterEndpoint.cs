using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace Microsoft.Extensions.Hosting.Auth.Register;

public static class RegisterEndpoint
{
    public static void MapRegister(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", RegisterUser)
            .WithName("Register")
            .WithTags("Auth")
            .WithSummary("Register a new user");
    }

    private static async Task<Created<UserDto>> RegisterUser(
        RegisterCommand command,
        IUserRepository repository,
        CancellationToken cancellationToken)
    {
        var request = new CreateUserRequest(
            command.Email,
            command.Password,
            command.DisplayName);

        var user = await repository.CreateAsync(request, cancellationToken);

        return TypedResults.Created($"/api/users/{user.Id}", user);
    }
}

public record RegisterCommand(string Email, string Password, string DisplayName);
