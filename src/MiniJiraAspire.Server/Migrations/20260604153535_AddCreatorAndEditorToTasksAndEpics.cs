using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniJiraAspire.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatorAndEditorToTasksAndEpics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedById",
                table: "TaskItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedById",
                table: "Epics",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_UpdatedById",
                table: "TaskItems",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Epics_UpdatedById",
                table: "Epics",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Epics_Users_UpdatedById",
                table: "Epics",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_Users_UpdatedById",
                table: "TaskItems",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Epics_Users_UpdatedById",
                table: "Epics");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_Users_UpdatedById",
                table: "TaskItems");

            migrationBuilder.DropIndex(
                name: "IX_TaskItems_UpdatedById",
                table: "TaskItems");

            migrationBuilder.DropIndex(
                name: "IX_Epics_UpdatedById",
                table: "Epics");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "Epics");
        }
    }
}
