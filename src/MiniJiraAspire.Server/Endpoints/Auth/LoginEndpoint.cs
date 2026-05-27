using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Features.Auth.Commands;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Endpoints.Auth;

public static class LoginEndpoint
{
    public static void MapLogin(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", Login)
            .WithName("Login")
            .WithTags("Auth")
            .WithSummary("Login user");
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
}
