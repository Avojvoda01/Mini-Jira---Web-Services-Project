using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniJiraAspire.Server.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260516104930_AddUsersTableIdempotent")]
public partial class AddUsersTableIdempotent : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS "Users" (
                "Id" uuid NOT NULL,
                "Email" text NOT NULL,
                "PasswordHash" text NOT NULL,
                "DisplayName" text NOT NULL,
                "Role" text NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone,
                CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
            );
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TABLE IF EXISTS \"Users\";");
    }
}
