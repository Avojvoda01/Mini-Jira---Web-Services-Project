namespace MiniJiraAspire.Server.Models;

public record LoginResponse(string Token, UserDto User);

public record RegisterUserResponse(UserDto? User, Dictionary<string, string[]> Errors)
{
    public bool Succeeded => User is not null;

    public static RegisterUserResponse Success(UserDto user) => new(user, []);

    public static RegisterUserResponse ValidationFailed(Dictionary<string, string[]> errors) => new(null, errors);
}

public record CreateUserResponse(UserDto? User, Dictionary<string, string[]> Errors)
{
    public bool Succeeded => User is not null;

    public static CreateUserResponse Success(UserDto user) => new(user, []);

    public static CreateUserResponse ValidationFailed(Dictionary<string, string[]> errors) => new(null, errors);
}

public record ChangeUserRoleResponse(UserDto? User, Dictionary<string, string[]>? ValidationErrors, bool NotFound)
{
    public static ChangeUserRoleResponse Success(UserDto user) => new(user, null, false);

    public static ChangeUserRoleResponse ValidationFailed(Dictionary<string, string[]> errors) => new(null, errors, false);

    public static ChangeUserRoleResponse UserNotFound() => new(null, null, true);
}