using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Endpoints.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("Auth");

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Login user");

        group.MapPost("/register", Register)
            .WithName("Register")
            .WithSummary("Register a new user");
    }

    private static async Task<Results<Ok<LoginResponse>, ProblemHttpResult>> Login(
        LoginRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new LoginUserCommand(request.Email, request.Password), ct);

        return result is null
            ? TypedResults.Problem(
                title: "Unable to sign in.",
                detail: "This user does not exist or the password is incorrect.",
                statusCode: StatusCodes.Status401Unauthorized)
            : TypedResults.Ok(result);
    }

    private static async Task<Results<Created<UserDto>, ProblemHttpResult>> Register(
        CreateUserRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new RegisterUserCommand(request.Email, request.Password, request.DisplayName),
            ct);

        if (result.DisplayNameConflict)
            return TypedResults.Problem("Display name is already taken.", statusCode: StatusCodes.Status409Conflict);

        if (result.EmailConflict)
            return TypedResults.Problem("Email is already taken.", statusCode: StatusCodes.Status409Conflict);

        return result.Succeeded && result.User is not null
            ? TypedResults.Created($"/api/v1/users/{result.User.Id}", result.User)
            : TypedResults.Problem(new HttpValidationProblemDetails(result.Errors)
            {
                Status = StatusCodes.Status422UnprocessableEntity
            });
    }
}
