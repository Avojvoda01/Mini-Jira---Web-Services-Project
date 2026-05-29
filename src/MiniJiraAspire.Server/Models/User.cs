namespace MiniJiraAspire.Server.Models;

public class User : BaseEntity
{
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string DisplayName { get; set; }
    public string Role { get; set; } = UserRoles.ProjectMember;
}
