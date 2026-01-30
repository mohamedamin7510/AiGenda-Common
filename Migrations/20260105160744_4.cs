using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_genda_API.Migrations
{
    /// <inheritdoc />
    public partial class _4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Note_Folders_FolderId",
                table: "Note");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Folders_FolderId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_FolderId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Note_FolderId",
                table: "Note");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "Note");

            migrationBuilder.DropColumn(
                name: "IsTaskFinished",
                table: "Note");

            migrationBuilder.AddColumn<int>(
                name: "ParentFolderId",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ParentFolderId",
                table: "Note",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ParentFolderId",
                table: "Tasks",
                column: "ParentFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Note_ParentFolderId",
                table: "Note",
                column: "ParentFolderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Note_Folders_ParentFolderId",
                table: "Note",
                column: "ParentFolderId",
                principalTable: "Folders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Folders_ParentFolderId",
                table: "Tasks",
                column: "ParentFolderId",
                principalTable: "Folders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Note_Folders_ParentFolderId",
                table: "Note");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Folders_ParentFolderId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_ParentFolderId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Note_ParentFolderId",
                table: "Note");

            migrationBuilder.DropColumn(
                name: "ParentFolderId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ParentFolderId",
                table: "Note");

            migrationBuilder.AddColumn<int>(
                name: "FolderId",
                table: "Tasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FolderId",
                table: "Note",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTaskFinished",
                table: "Note",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_FolderId",
                table: "Tasks",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Note_FolderId",
                table: "Note",
                column: "FolderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Note_Folders_FolderId",
                table: "Note",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Folders_FolderId",
                table: "Tasks",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "Id");
        }
    }
}
