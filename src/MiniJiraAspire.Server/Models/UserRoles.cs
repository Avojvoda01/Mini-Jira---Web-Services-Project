namespace MiniJiraAspire.Server.Models;

public enum UserRole
{
    Admin,
    ProjectManager,
    ProjectMember
}

public static class UserRoleExtensions
{
    public static readonly UserRole[] All = [UserRole.Admin, UserRole.ProjectManager, UserRole.ProjectMember];

    public static string ToRoleString(this UserRole role) => role switch
    {
        UserRole.Admin          => "Admin",
        UserRole.ProjectManager => "Project Manager",
        UserRole.ProjectMember  => "Project Member",
        _ => throw new ArgumentOutOfRangeException(nameof(role))
    };

    public static bool TryParse(string value, out UserRole role)
    {
        var trimmed = value.Trim();
        if (string.Equals(trimmed, "Admin", StringComparison.OrdinalIgnoreCase))
            { role = UserRole.Admin; return true; }
        if (string.Equals(trimmed, "Project Manager", StringComparison.OrdinalIgnoreCase))
            { role = UserRole.ProjectManager; return true; }
        if (string.Equals(trimmed, "Project Member", StringComparison.OrdinalIgnoreCase))
            { role = UserRole.ProjectMember; return true; }
        role = default;
        return false;
    }
}
