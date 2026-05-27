using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Features.User.Commands;
using MiniJiraAspire.Server.Features.User.Queries;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Endpoints.Admin;

public static class AdminUserEndpoints
{
    public static void MapAdminUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/users")
            .WithTags("Admin - Users")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        group.MapGet("/", GetUsers)
            .WithName("AdminGetUsers")
            .WithSummary("Get all users (admin)");

        group.MapGet("/{userId}", GetUserById)
            .WithName("AdminGetUserById")
            .WithSummary("Get a user by id (admin)");

        group.MapPost("/", CreateUser)
            .WithName("AdminCreateUser")
            .WithSummary("Create a new user (admin)");

        group.MapDelete("/{userId}", DeleteUser)
            .WithName("AdminDeleteUser")
            .WithSummary("Delete a user (admin)");
    }

    private static async Task<Ok<List<UserDto>>> GetUsers(
        IMediator mediator,
        CancellationToken ct)
    {
        var users = await mediator.Send(new GetAllUsersQuery(), ct);
        return TypedResults.Ok(users);
    }

    private static async Task<Results<Ok<UserDto>, ProblemHttpResult>> GetUserById(
        string userId,
        IMediator mediator,
        CancellationToken ct)
    {
        var user = await mediator.Send(new GetUserByIdQuery(userId), ct);

        return user is null
            ? TypedResults.Problem($"User with id {userId} not found", statusCode: StatusCodes.Status404NotFound)
            : TypedResults.Ok(user);
    }

    private static async Task<Results<Created<UserDto>, ValidationProblem>> CreateUser(
        CreateUserRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new CreateUserCommand(request.Email, request.Password, request.DisplayName), ct);

        return result is { Succeeded: true, User: not null }
            ? TypedResults.Created($"/api/admin/users/{result.User.Id}", result.User)
            : TypedResults.ValidationProblem(result.Errors);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteUser(
        string userId,
        IMediator mediator,
        CancellationToken ct)
    {
        var deleted = await mediator.Send(new DeleteUserCommand(userId), ct);

        return deleted
            ? TypedResults.NoContent()
            : TypedResults.Problem($"User with id {userId} not found", statusCode: StatusCodes.Status404NotFound);
    }
}