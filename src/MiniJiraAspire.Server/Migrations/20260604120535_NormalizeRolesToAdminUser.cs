using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniJiraAspire.Server.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeRolesToAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Users\" SET \"Role\" = 'User' WHERE \"Role\" IN ('ProjectManager', 'ProjectMember')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Users\" SET \"Role\" = 'User' WHERE \"Role\" NOT IN ('Admin', 'User')");
        }
    }
}
