using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_genda_API.Migrations
{
    /// <inheritdoc />
    public partial class improvementsintherelationshipsandaddingthesomecolumnsintheworkspace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceUser_AspNetUsers_UserID",
                table: "WorkspaceUser");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceUser_WorkSpaces_WrokSpaceID",
                table: "WorkspaceUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkspaceUser",
                table: "WorkspaceUser");

            migrationBuilder.DropColumn(
                name: "WorkSpaceUserId",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "WorkspaceUser",
                newName: "workspaceUsers");

            migrationBuilder.RenameColumn(
                name: "IconPath",
                table: "WorkSpaces",
                newName: "IconCode");

            migrationBuilder.RenameIndex(
                name: "IX_WorkspaceUser_UserID",
                table: "workspaceUsers",
                newName: "IX_workspaceUsers_UserID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_workspaceUsers",
                table: "workspaceUsers",
                columns: new[] { "WrokSpaceID", "UserID" });

            migrationBuilder.AddForeignKey(
                name: "FK_workspaceUsers_AspNetUsers_UserID",
                table: "workspaceUsers",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_workspaceUsers_WorkSpaces_WrokSpaceID",
                table: "workspaceUsers",
                column: "WrokSpaceID",
                principalTable: "WorkSpaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_workspaceUsers_AspNetUsers_UserID",
                table: "workspaceUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_workspaceUsers_WorkSpaces_WrokSpaceID",
                table: "workspaceUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_workspaceUsers",
                table: "workspaceUsers");

            migrationBuilder.RenameTable(
                name: "workspaceUsers",
                newName: "WorkspaceUser");

            migrationBuilder.RenameColumn(
                name: "IconCode",
                table: "WorkSpaces",
                newName: "IconPath");

            migrationBuilder.RenameIndex(
                name: "IX_workspaceUsers_UserID",
                table: "WorkspaceUser",
                newName: "IX_WorkspaceUser_UserID");

            migrationBuilder.AddColumn<int>(
                name: "WorkSpaceUserId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkspaceUser",
                table: "WorkspaceUser",
                columns: new[] { "WrokSpaceID", "UserID" });

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceUser_AspNetUsers_UserID",
                table: "WorkspaceUser",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceUser_WorkSpaces_WrokSpaceID",
                table: "WorkspaceUser",
                column: "WrokSpaceID",
                principalTable: "WorkSpaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
