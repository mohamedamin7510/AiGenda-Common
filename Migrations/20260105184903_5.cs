using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_genda_API.Migrations
{
    /// <inheritdoc />
    public partial class _5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Folders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Folders",
                type: "nvarchar(450)",
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Folders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                table: "Folders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Folders_CreatedById",
                table: "Folders",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_UpdatedById",
                table: "Folders",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Folders_AspNetUsers_CreatedById",
                table: "Folders",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Folders_AspNetUsers_UpdatedById",
                table: "Folders",
                column: "UpdatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Folders_AspNetUsers_CreatedById",
                table: "Folders");

            migrationBuilder.DropForeignKey(
                name: "FK_Folders_AspNetUsers_UpdatedById",
                table: "Folders");

            migrationBuilder.DropIndex(
                name: "IX_Folders_CreatedById",
                table: "Folders");

            migrationBuilder.DropIndex(
                name: "IX_Folders_UpdatedById",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "Folders");
        }
    }
}
