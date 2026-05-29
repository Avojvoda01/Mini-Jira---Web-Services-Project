namespace MiniJiraAspire.Server.Models;

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string ProjectManager = "Project Manager";
    public const string ProjectMember = "Project Member";

    public static readonly string[] All = [Admin, ProjectManager, ProjectMember];
}
