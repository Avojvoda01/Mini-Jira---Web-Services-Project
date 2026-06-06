using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniJiraAspire.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByToTasksAndEpics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "TaskItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "Epics",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_CreatedById",
                table: "TaskItems",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Epics_CreatedById",
                table: "Epics",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Epics_Users_CreatedById",
                table: "Epics",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_Users_CreatedById",
                table: "TaskItems",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Epics_Users_CreatedById",
                table: "Epics");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_Users_CreatedById",
                table: "TaskItems");

            migrationBuilder.DropIndex(
                name: "IX_TaskItems_CreatedById",
                table: "TaskItems");

            migrationBuilder.DropIndex(
                name: "IX_Epics_CreatedById",
                table: "Epics");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Epics");
        }
    }
}
