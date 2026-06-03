using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Models;

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
        IMediator mediator,
        CancellationToken ct)
    {
        var users = await mediator.Send(new GetAllUsersQuery(), ct);
        return TypedResults.Ok(users);
    }

    private static async Task<Results<Ok<UserDto>, NotFound>> GetUser(
        string userId,
        IMediator mediator,
        CancellationToken ct)
    {
        var user = await mediator.Send(new GetUserByIdQuery(userId), ct);

        return user is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(user);
    }
}