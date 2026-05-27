using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;
using UserEntity = MiniJiraAspire.Server.Models.User;

namespace MiniJiraAspire.Server.Features.User.Commands;

public record CreateUserCommand(string Email, string Password, string DisplayName) : IRequest<CreateUserResult>;

public record CreateUserResult(UserDto? User, Dictionary<string, string[]> Errors)
{
    public bool Succeeded => User is not null;

    public static CreateUserResult Success(UserDto user) => new(user, []);

    public static CreateUserResult ValidationFailed(Dictionary<string, string[]> errors) => new(null, errors);
}

public class CreateUserHandler(
    IUserRepository repository,
    IPasswordHasher<UserEntity> passwordHasher) : IRequestHandler<CreateUserCommand, CreateUserResult>
{
    public async Task<CreateUserResult> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var errors = ValidateAnnotations(request);

        if (await repository.EmailExistsAsync(request.Email, ct))
        {
            AddError(errors, nameof(request.Email), "Email is already taken.");
        }

        if (await repository.DisplayNameExistsAsync(request.DisplayName, ct))
        {
            AddError(errors, nameof(request.DisplayName), "Display name is already taken.");
        }

        if (errors.Count > 0)
        {
            return CreateUserResult.ValidationFailed(errors);
        }

        var user = new UserEntity
        {
            Email = request.Email,
            DisplayName = request.DisplayName,
            PasswordHash = string.Empty
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        var created = await repository.CreateAsync(user, ct);

        return CreateUserResult.Success(new UserDto(created.Id.ToString(), created.Email, created.DisplayName, created.Role));
    }

    private static Dictionary<string, string[]> ValidateAnnotations(CreateUserCommand request)
    {
        var dto = new CreateUserRequest(request.Email, request.Password, request.DisplayName);
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, validateAllProperties: true);

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
