namespace MiniJiraAspire.Server.Models;

public class User
{
    public Guid Id { get; set; }

    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string DisplayName { get; set; }
    // TODO: replace Role with an enum once the RoleEndpoint is implemented.
    public string Role { get; set; } = "Project Member";



}
