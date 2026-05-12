using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniJiraAspire.Server.Migrations
{
    /// <inheritdoc />
    public partial class epicprojectref : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "Epics",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Epics_ProjectId",
                table: "Epics",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Epics_Projects_ProjectId",
                table: "Epics",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Epics_Projects_ProjectId",
                table: "Epics");

            migrationBuilder.DropIndex(
                name: "IX_Epics_ProjectId",
                table: "Epics");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Epics");
        }
    }
}
