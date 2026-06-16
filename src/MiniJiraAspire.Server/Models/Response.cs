namespace MiniJiraAspire.Server.Models;

public record LoginResponse(string Token, UserDto User);

public record RegisterUserResponse(UserDto? User, Dictionary<string, string[]> Errors, bool EmailConflict = false, bool DisplayNameConflict = false)
{
    public bool Succeeded => User is not null;

    public static RegisterUserResponse Success(UserDto user) => new(user, []);

    public static RegisterUserResponse ValidationFailed(Dictionary<string, string[]> errors) => new(null, errors);

    public static RegisterUserResponse EmailAlreadyTaken() => new(null, [], EmailConflict: true);

    public static RegisterUserResponse DisplayNameAlreadyTaken() => new(null, [], DisplayNameConflict: true);
}

public record CreateUserResponse(UserDto? User, Dictionary<string, string[]> Errors, bool EmailConflict = false)
{
    public bool Succeeded => User is not null;

    public static CreateUserResponse Success(UserDto user) => new(user, []);

    public static CreateUserResponse ValidationFailed(Dictionary<string, string[]> errors) => new(null, errors);

    public static CreateUserResponse EmailAlreadyTaken() => new(null, [], EmailConflict: true);
}

public record ChangeUserRoleResponse(UserDto? User, Dictionary<string, string[]>? ValidationErrors, bool NotFound)
{
    public static ChangeUserRoleResponse Success(UserDto user) => new(user, null, false);

    public static ChangeUserRoleResponse ValidationFailed(Dictionary<string, string[]> errors) => new(null, errors, false);

    public static ChangeUserRoleResponse UserNotFound() => new(null, null, true);
}

public record UpdateUserProfileResponse(UserDto? User, Dictionary<string, string[]>? Errors, bool EmailConflict, bool DisplayNameConflict, bool NotFound)
{
    public bool Succeeded => User is not null;

    public static UpdateUserProfileResponse Success(UserDto user) => new(user, null, false, false, false);

    public static UpdateUserProfileResponse ValidationFailed(Dictionary<string, string[]> errors) => new(null, errors, false, false, false);

    public static UpdateUserProfileResponse EmailAlreadyTaken() => new(null, null, true, false, false);

    public static UpdateUserProfileResponse DisplayNameAlreadyTaken() => new(null, null, false, true, false);

    public static UpdateUserProfileResponse UserNotFound() => new(null, null, false, false, true);
}

public record ChangeUserPasswordResponse(bool Succeeded, bool InvalidCurrentPassword, bool NotFound, Dictionary<string, string[]>? ValidationErrors = null)
{
    public static ChangeUserPasswordResponse Success() => new(true, false, false);

    public static ChangeUserPasswordResponse InvalidPassword() => new(false, true, false);

    public static ChangeUserPasswordResponse UserNotFound() => new(false, false, true);

    public static ChangeUserPasswordResponse ValidationFailed(Dictionary<string, string[]> errors) => new(false, false, false, errors);
}

// Chatbot
public record ChatResponse(string Answer);