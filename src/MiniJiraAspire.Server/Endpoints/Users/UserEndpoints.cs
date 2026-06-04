using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Endpoints.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/", GetUsers)
            .WithName("GetUsers")
            .WithSummary("Get users");

        group.MapGet("/{userId}", GetUser)
            .Produces<UserDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("GetUser")
            .WithSummary("Get user by id");

        var adminGroup = app.MapGroup("/users")
            .WithTags("Users")
            .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Admin)));

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

    private static async Task<Results<Ok<UserDto>, ProblemHttpResult>> GetUser(
        string userId,
        IMediator mediator,
        CancellationToken ct)
    {
        var user = await mediator.Send(new GetUserByIdQuery(userId), ct);

        return user is null
            ? TypedResults.Problem($"User with id {userId} not found", statusCode: StatusCodes.Status404NotFound)
            : TypedResults.Ok(user);
    }

    private static async Task<Results<Created<UserDto>, ProblemHttpResult>> CreateUser(
        CreateUserRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new CreateUserCommand(request.Email, request.Password, request.DisplayName), ct);

        if (result.EmailConflict)
            return TypedResults.Problem("Email is already taken.", statusCode: StatusCodes.Status409Conflict);

        return result is { Succeeded: true, User: not null }
            ? TypedResults.Created($"/api/v1/users/{result.User.Id}", result.User)
            : TypedResults.Problem(new HttpValidationProblemDetails(result.Errors)
            {
                Status = StatusCodes.Status422UnprocessableEntity
            });
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

    private static async Task<Results<Ok<UserDto>, ProblemHttpResult>> ChangeUserRole(
        string userId,
        ChangeUserRoleRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new ChangeUserRoleCommand(userId, request.Role), ct);

        if (result.ValidationErrors is not null)
            return TypedResults.Problem(new HttpValidationProblemDetails(result.ValidationErrors)
            {
                Status = StatusCodes.Status422UnprocessableEntity
            });

        if (result.NotFound)
            return TypedResults.Problem($"User with id {userId} not found", statusCode: StatusCodes.Status404NotFound);

        return TypedResults.Ok(result.User!);
    }
}