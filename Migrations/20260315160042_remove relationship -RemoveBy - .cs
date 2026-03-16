using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_genda_API.Migrations
{
    /// <inheritdoc />
    public partial class removerelationshipRemoveBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "IX_Tasks_RemovedById",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Spaces_RemovedById",
                table: "Spaces");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WorkSpaces_RemovedById",
                table: "WorkSpaces",
                column: "RemovedById");

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
    }
}
