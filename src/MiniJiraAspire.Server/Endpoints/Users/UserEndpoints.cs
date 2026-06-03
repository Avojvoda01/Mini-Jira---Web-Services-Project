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

        var adminGroup = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization(policy => policy.RequireRole(UserRole.Admin.ToRoleString()));

        adminGroup.MapPost("/", CreateUser)
            .WithName("CreateUser")
            .WithSummary("Create a new user (admin)");

        adminGroup.MapDelete("/{userId}", DeleteUser)
            .WithName("DeleteUser")
            .WithSummary("Delete a user (admin)");

        adminGroup.MapPatch("/{userId}/role", ChangeUserRole)
            .WithName("ChangeUserRole")
            .WithSummary("Change the role of a user (admin)");
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

    private static async Task<Results<Created<UserDto>, ValidationProblem>> CreateUser(
        CreateUserRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new CreateUserCommand(request.Email, request.Password, request.DisplayName), ct);

        return result is { Succeeded: true, User: not null }
            ? TypedResults.Created($"/api/users/{result.User.Id}", result.User)
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

    private static async Task<Results<Ok<UserDto>, ValidationProblem, ProblemHttpResult>> ChangeUserRole(
        string userId,
        ChangeUserRoleRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new ChangeUserRoleCommand(userId, request.Role), ct);

        if (result.ValidationErrors is not null)
            return TypedResults.ValidationProblem(result.ValidationErrors);

        if (result.NotFound)
            return TypedResults.Problem($"User with id {userId} not found", statusCode: StatusCodes.Status404NotFound);

        return TypedResults.Ok(result.User!);
    }
}