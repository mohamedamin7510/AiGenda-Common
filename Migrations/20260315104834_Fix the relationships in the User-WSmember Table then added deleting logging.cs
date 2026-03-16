using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_genda_API.Migrations
{
    /// <inheritdoc />
    public partial class FixtherelationshipsintheUserWSmemberTablethenaddeddeletinglogging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkspaceMembers_UserID",
                table: "WorkspaceMembers");

            migrationBuilder.AddColumn<DateTime>(
                name: "RemovedAt",
                table: "WorkSpaces",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RemovedById",
                table: "WorkSpaces",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Visibility",
                table: "WorkSpaces",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RemovedAt",
                table: "Tasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RemovedById",
                table: "Tasks",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RemovedAt",
                table: "Spaces",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RemovedById",
                table: "Spaces",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkSpaces_RemovedById",
                table: "WorkSpaces",
                column: "RemovedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceMembers_UserID",
                table: "WorkspaceMembers",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_RemovedById",
                table: "Tasks",
                column: "RemovedById");

            migrationBuilder.CreateIndex(
                name: "IX_Spaces_RemovedById",
                table: "Spaces",
                column: "RemovedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Spaces_AspNetUsers_RemovedById",
                table: "Spaces",
                column: "RemovedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_AspNetUsers_RemovedById",
                table: "Tasks",
                column: "RemovedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkSpaces_AspNetUsers_RemovedById",
                table: "WorkSpaces",
                column: "RemovedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Spaces_AspNetUsers_RemovedById",
                table: "Spaces");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_AspNetUsers_RemovedById",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkSpaces_AspNetUsers_RemovedById",
                table: "WorkSpaces");

            migrationBuilder.DropIndex(
                name: "IX_WorkSpaces_RemovedById",
                table: "WorkSpaces");

            migrationBuilder.DropIndex(
                name: "IX_WorkspaceMembers_UserID",
                table: "WorkspaceMembers");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_RemovedById",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Spaces_RemovedById",
                table: "Spaces");

            migrationBuilder.DropColumn(
                name: "RemovedAt",
                table: "WorkSpaces");

            migrationBuilder.DropColumn(
                name: "RemovedById",
                table: "WorkSpaces");

            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "WorkSpaces");

            migrationBuilder.DropColumn(
                name: "RemovedAt",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "RemovedById",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "RemovedAt",
                table: "Spaces");

            migrationBuilder.DropColumn(
                name: "RemovedById",
                table: "Spaces");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceMembers_UserID",
                table: "WorkspaceMembers",
                column: "UserID",
                unique: true);
        }
    }
}
