using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Endpoints.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users");
        
        group.MapGet("/", GetUsers)
            .WithName("GetUsers")
            .WithSummary("Get users");
        
        group.MapGet("/{userId}", GetUser)
            .WithName("GetUser")
            .WithSummary("Get user by id");
        
    }

    private static async Task<Ok<List<UserDto>>> GetUsers(
        IUserRepository repository,
        CancellationToken cancellationToken)
    {
        var users = await repository.GetAllAsync(cancellationToken);
        return TypedResults.Ok(users);
    }


    private static async Task<Results<Ok<UserDto>, NotFound>> GetUser(
        string userId,
        IUserRepository repository,
        CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(userId, cancellationToken);

        return user is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(user);
    }
    
    
}