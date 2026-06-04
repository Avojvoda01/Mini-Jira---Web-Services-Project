using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniJiraAspire.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectCreator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "Projects",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CreatedById",
                table: "Projects",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_CreatedById",
                table: "Projects",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_CreatedById",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_CreatedById",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Projects");
        }
    }
}
