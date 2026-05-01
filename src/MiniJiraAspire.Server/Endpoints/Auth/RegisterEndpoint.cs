using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace Microsoft.Extensions.Hosting.Auth.Register;

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
        IUserRepository repository,
        CancellationToken cancellationToken)
    {
        var errors = Validate(command);

        if (await repository.EmailExistsAsync(command.Email, cancellationToken))
        {
            AddError(errors, nameof(command.Email), "Email is already taken.");
        }

        if (await repository.DisplayNameExistsAsync(command.DisplayName, cancellationToken))
        {
            AddError(errors, nameof(command.DisplayName), "Display name is already taken.");
        }

        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var request = new CreateUserRequest(
            command.Email,
            command.Password,
            command.DisplayName);

        var user = await repository.CreateAsync(request, cancellationToken);

        return TypedResults.Created($"/api/users/{user.Id}", user);
    }

    private static Dictionary<string, string[]> Validate(RegisterCommand command)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(command);

        Validator.TryValidateObject(command, validationContext, validationResults, validateAllProperties: true);

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
