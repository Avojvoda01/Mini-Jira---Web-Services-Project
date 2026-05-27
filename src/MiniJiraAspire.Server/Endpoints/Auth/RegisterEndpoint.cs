using System.ComponentModel.DataAnnotations;
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
        RegisterCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var errors = Validate(command);

        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var result = await mediator.Send(
            new RegisterUserCommand(command.Email, command.Password, command.DisplayName),
            cancellationToken);

        return result.Succeeded && result.User is not null
            ? TypedResults.Created($"/api/users/{result.User.Id}", result.User)
            : TypedResults.ValidationProblem(result.Errors);
    }

    private static Dictionary<string, string[]> Validate(RegisterCommand command)
    {
        var request = new CreateUserRequest(
            command.Email,
            command.Password,
            command.DisplayName);

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(request);

        Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true);

        var errors = new Dictionary<string, string[]>();

        foreach (var validationResult in validationResults)
        {
            foreach (var memberName in validationResult.MemberNames)
            {
                AddError(errors, memberName, validationResult.ErrorMessage ?? "Invalid value.");
            }
        }

        return errors;
    }

    private static void AddError(Dictionary<string, string[]> errors, string key, string error)
    {
        errors[key] = errors.TryGetValue(key, out var existingErrors)
            ? [.. existingErrors, error]
            : [error];
    }
}

public record RegisterCommand(
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(6)] string Password,
    [property: Required, StringLength(100, MinimumLength = 2)] string DisplayName);
