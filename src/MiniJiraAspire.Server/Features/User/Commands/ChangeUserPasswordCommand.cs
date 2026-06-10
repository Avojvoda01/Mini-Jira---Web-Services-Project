using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;
using UserEntity = MiniJiraAspire.Server.Models.User;

namespace MiniJiraAspire.Server.Features.User.Commands;

public class ChangeUserPasswordHandler(
    IUserRepository repository,
    IPasswordHasher<UserEntity> passwordHasher) : IRequestHandler<ChangeUserPasswordCommand, ChangeUserPasswordResponse>
{
    public async Task<ChangeUserPasswordResponse> Handle(ChangeUserPasswordCommand request, CancellationToken ct)
    {
        var errors = ValidateAnnotations(request);
        if (errors.Count > 0)
            return ChangeUserPasswordResponse.ValidationFailed(errors);

        var user = await repository.GetByIdAsync(request.UserId, ct);
        if (user is null)
            return ChangeUserPasswordResponse.UserNotFound();

        var verifyResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (verifyResult == PasswordVerificationResult.Failed)
            return ChangeUserPasswordResponse.InvalidPassword();

        var newHash = passwordHasher.HashPassword(user, request.NewPassword);
        await repository.UpdatePasswordHashAsync(request.UserId, newHash, ct);

        return ChangeUserPasswordResponse.Success();
    }

    private static Dictionary<string, string[]> ValidateAnnotations(ChangeUserPasswordCommand request)
    {
        var dto = new ChangeUserPasswordRequest(request.CurrentPassword, request.NewPassword);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(dto, new ValidationContext(dto), results, validateAllProperties: true);

        var errors = new Dictionary<string, string[]>();
        foreach (var result in results)
        {
            foreach (var member in result.MemberNames)
            {
                errors[member] = errors.TryGetValue(member, out var existing)
                    ? [.. existing, result.ErrorMessage ?? "Invalid value."]
                    : [result.ErrorMessage ?? "Invalid value."];
            }
        }

        return errors;
    }
}
