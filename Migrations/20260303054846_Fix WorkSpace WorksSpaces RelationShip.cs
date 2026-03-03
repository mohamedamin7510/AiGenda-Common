using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_genda_API.Migrations
{
    /// <inheritdoc />
    public partial class FixWorkSpaceWorksSpacesRelationShip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_WorkspaceUsers_WorkspaceUsersWrokSpaceID_WorkspaceUsersUserID",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkSpaces_WorkspaceUsers_WorkspaceUsersWrokSpaceID_WorkspaceUsersUserID",
                table: "WorkSpaces");

            migrationBuilder.DropTable(
                name: "WorkspaceUsers");

            migrationBuilder.DropIndex(
                name: "IX_WorkSpaces_WorkspaceUsersWrokSpaceID_WorkspaceUsersUserID",
                table: "WorkSpaces");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_WorkspaceUsersWrokSpaceID_WorkspaceUsersUserID",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "WorkspaceUsersUserID",
                table: "WorkSpaces");

            migrationBuilder.DropColumn(
                name: "WorkspaceUsersWrokSpaceID",
                table: "WorkSpaces");

            migrationBuilder.DropColumn(
                name: "WorkspaceUsersUserID",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "WorkspaceUsersWrokSpaceID",
                table: "AspNetUsers",
                newName: "WorkSpaceUserId");

            migrationBuilder.CreateTable(
                name: "WorkspaceUser",
                columns: table => new
                {
                    WrokSpaceID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceUser", x => new { x.WrokSpaceID, x.UserID });
                    table.ForeignKey(
                        name: "FK_WorkspaceUser_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkspaceUser_WorkSpaces_WrokSpaceID",
                        column: x => x.WrokSpaceID,
                        principalTable: "WorkSpaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceUser_UserID",
                table: "WorkspaceUser",
                column: "UserID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkspaceUser");

            migrationBuilder.RenameColumn(
                name: "WorkSpaceUserId",
                table: "AspNetUsers",
                newName: "WorkspaceUsersWrokSpaceID");

            migrationBuilder.AddColumn<string>(
                name: "WorkspaceUsersUserID",
                table: "WorkSpaces",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkspaceUsersWrokSpaceID",
                table: "WorkSpaces",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkspaceUsersUserID",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkspaceUsers",
                columns: table => new
                {
                    WrokSpaceID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceUsers", x => new { x.WrokSpaceID, x.UserID });
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkSpaces_WorkspaceUsersWrokSpaceID_WorkspaceUsersUserID",
                table: "WorkSpaces",
                columns: new[] { "WorkspaceUsersWrokSpaceID", "WorkspaceUsersUserID" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_WorkspaceUsersWrokSpaceID_WorkspaceUsersUserID",
                table: "AspNetUsers",
                columns: new[] { "WorkspaceUsersWrokSpaceID", "WorkspaceUsersUserID" });

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_WorkspaceUsers_WorkspaceUsersWrokSpaceID_WorkspaceUsersUserID",
                table: "AspNetUsers",
                columns: new[] { "WorkspaceUsersWrokSpaceID", "WorkspaceUsersUserID" },
                principalTable: "WorkspaceUsers",
                principalColumns: new[] { "WrokSpaceID", "UserID" });

            migrationBuilder.AddForeignKey(
                name: "FK_WorkSpaces_WorkspaceUsers_WorkspaceUsersWrokSpaceID_WorkspaceUsersUserID",
                table: "WorkSpaces",
                columns: new[] { "WorkspaceUsersWrokSpaceID", "WorkspaceUsersUserID" },
                principalTable: "WorkspaceUsers",
                principalColumns: new[] { "WrokSpaceID", "UserID" });
        }
    }
}
