using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MiniJiraAspire.Server.Models;
using MiniJiraAspire.Server.Persistence.Repositories;

namespace MiniJiraAspire.Server.Features.User.Commands;

public class UpdateUserProfileHandler(IUserRepository repository) : IRequestHandler<UpdateUserProfileCommand, UpdateUserProfileResponse>
{
    public async Task<UpdateUserProfileResponse> Handle(UpdateUserProfileCommand request, CancellationToken ct)
    {
        var errors = ValidateAnnotations(request);
        if (errors.Count > 0)
            return UpdateUserProfileResponse.ValidationFailed(errors);

        var current = await repository.GetByIdAsync(request.UserId, ct);
        if (current is null)
            return UpdateUserProfileResponse.UserNotFound();

        var displayNameChanged = !string.Equals(current.DisplayName, request.DisplayName.Trim(), System.StringComparison.OrdinalIgnoreCase);
        var emailChanged = !string.Equals(current.Email, request.Email.Trim(), System.StringComparison.OrdinalIgnoreCase);

        if (displayNameChanged && await repository.DisplayNameExistsAsync(request.DisplayName, ct))
            return UpdateUserProfileResponse.DisplayNameAlreadyTaken();

        if (emailChanged && await repository.EmailExistsAsync(request.Email, ct))
            return UpdateUserProfileResponse.EmailAlreadyTaken();

        var updated = await repository.UpdateProfileAsync(request.UserId, request.DisplayName.Trim(), request.Email.Trim(), ct);
        if (updated is null)
            return UpdateUserProfileResponse.UserNotFound();

        return UpdateUserProfileResponse.Success(new UserDto(updated.Id.ToString(), updated.Email, updated.DisplayName, updated.Role));
    }

    private static Dictionary<string, string[]> ValidateAnnotations(UpdateUserProfileCommand request)
    {
        var dto = new UpdateUserProfileRequest(request.DisplayName, request.Email);
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
