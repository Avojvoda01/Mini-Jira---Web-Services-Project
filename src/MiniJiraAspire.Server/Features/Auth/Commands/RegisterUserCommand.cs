using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;
using UserEntity = MiniJiraAspire.Server.Models.User;

namespace MiniJiraAspire.Server.Features.Auth.Commands;

public class RegisterUserHandler(
    IUserRepository repository,
    IPasswordHasher<UserEntity> passwordHasher) : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    public async Task<RegisterUserResponse> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        var errors = ValidateAnnotations(request);

        if (errors.Count > 0)
        {
            return RegisterUserResponse.ValidationFailed(errors);
        }

        if (await repository.DisplayNameExistsAsync(request.DisplayName, ct))
        {
            return RegisterUserResponse.DisplayNameAlreadyTaken();
        }

        if (await repository.EmailExistsAsync(request.Email, ct))
        {
            return RegisterUserResponse.EmailAlreadyTaken();
        }

        var user = new UserEntity
        {
            Email = request.Email,
            DisplayName = request.DisplayName,
            PasswordHash = string.Empty
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        var createdUser = await repository.CreateAsync(user, ct);

        return RegisterUserResponse.Success(
            new UserDto(createdUser.Id.ToString(), createdUser.Email, createdUser.DisplayName, createdUser.Role, createdUser.CreatedAtUtc));
    }

    private static Dictionary<string, string[]> ValidateAnnotations(RegisterUserCommand request)
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
