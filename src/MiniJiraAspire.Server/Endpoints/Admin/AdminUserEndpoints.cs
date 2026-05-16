using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace Microsoft.Extensions.Hosting.Admin.Users;

public static class AdminUserEndpoints
{
    public static void MapAdminUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/users")
            .WithTags("Admin - Users");

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
        IUserRepository repository,
        CancellationToken cancellationToken)
    {
        var users = await repository.GetAllAsync(cancellationToken);
        return TypedResults.Ok(users);
    }

    private static async Task<Results<Ok<UserDto>, ProblemHttpResult>> GetUserById(
        string userId,
        IUserRepository repository,
        CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return TypedResults.Problem($"User with id {userId} not found", statusCode: StatusCodes.Status404NotFound);
        }

        return TypedResults.Ok(user);
    }

    private static async Task<Results<Created<UserDto>, ValidationProblem>> CreateUser(
        CreateUserRequest request,
        IUserRepository repository,
        CancellationToken cancellationToken)
    {
        var errors = Validate(request);

        if (await repository.EmailExistsAsync(request.Email, cancellationToken))
        {
            AddError(errors, nameof(request.Email), "Email is already taken.");
        }

        if (await repository.DisplayNameExistsAsync(request.DisplayName, cancellationToken))
        {
            AddError(errors, nameof(request.DisplayName), "Display name is already taken.");
        }

        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var user = await repository.CreateAsync(request, cancellationToken);
        return TypedResults.Created($"/api/admin/users/{user.Id}", user);
    }

    private static async Task<NoContent> DeleteUser(
        string userId,
        IUserRepository repository,
        CancellationToken cancellationToken)
    {
        await repository.DeleteAsync(userId, cancellationToken);
        return TypedResults.NoContent();
    }

    private static Dictionary<string, string[]> Validate(CreateUserRequest request)
    {
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
