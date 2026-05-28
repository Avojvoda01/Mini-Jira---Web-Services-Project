namespace MiniJiraAspire.Server.Models;

// Operation results / responses returned from endpoints.
// Feature-specific results (e.g. RegisterUserResult, CreateUserResult,
// ChangeUserRoleResult) live next to their handlers in Features/**.
public record LoginResponse(string Token, UserDto User);