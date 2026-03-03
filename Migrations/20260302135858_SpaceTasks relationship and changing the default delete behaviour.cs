using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_genda_API.Migrations
{
    /// <inheritdoc />
    public partial class SpaceTasksrelationshipandchangingthedefaultdeletebehaviour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Spaces_AspNetUsers_CreatedById",
                table: "Spaces");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkSpaces_AspNetUsers_CreatedById",
                table: "WorkSpaces");

            migrationBuilder.AddColumn<string>(
                name: "SpaceId",
                table: "Tasks",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_SpaceId",
                table: "Tasks",
                column: "SpaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Spaces_AspNetUsers_CreatedById",
                table: "Spaces",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Spaces_SpaceId",
                table: "Tasks",
                column: "SpaceId",
                principalTable: "Spaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkSpaces_AspNetUsers_CreatedById",
                table: "WorkSpaces",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Spaces_AspNetUsers_CreatedById",
                table: "Spaces");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Spaces_SpaceId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkSpaces_AspNetUsers_CreatedById",
                table: "WorkSpaces");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_SpaceId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "SpaceId",
                table: "Tasks");

            migrationBuilder.AddForeignKey(
                name: "FK_Spaces_AspNetUsers_CreatedById",
                table: "Spaces",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkSpaces_AspNetUsers_CreatedById",
                table: "WorkSpaces",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
