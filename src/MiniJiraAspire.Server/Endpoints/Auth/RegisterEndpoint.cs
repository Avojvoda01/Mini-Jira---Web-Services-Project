using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Features.Auth.Commands;
using MiniJiraAspire.Server.Models;

namespace MiniJiraAspire.Server.Endpoints.Auth;

public static class RegisterEndpoint
{
    public static void MapRegister(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", RegisterUser)
            .WithName("Register")
            .WithTags("Auth")
            .WithSummary("Register a new user");
    }

    private static async Task<Results<Created<UserDto>, ValidationProblem>> RegisterUser(
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