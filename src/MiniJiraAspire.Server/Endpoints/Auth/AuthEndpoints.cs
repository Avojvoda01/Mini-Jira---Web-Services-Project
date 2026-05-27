using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Features.Auth.Commands;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Endpoints.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Login user");

        group.MapPost("/register", Register)
            .WithName("Register")
            .WithSummary("Register a new user");
    }

    private static async Task<Results<Ok<LoginResponse>, UnauthorizedHttpResult>> Login(
        LoginRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new LoginUserCommand(request.Email, request.Password), ct);

        return result is null
            ? TypedResults.Unauthorized()
            : TypedResults.Ok(result);
    }

    private static async Task<Results<Created<UserDto>, ValidationProblem>> Register(
        CreateUserRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new RegisterUserCommand(request.Email, request.Password, request.DisplayName),
            ct);

        return result.Succeeded && result.User is not null
            ? TypedResults.Created($"/api/users/{result.User.Id}", result.User)
            : TypedResults.ValidationProblem(result.Errors);
    }
}