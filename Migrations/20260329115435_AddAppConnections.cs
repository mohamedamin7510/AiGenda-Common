using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_genda_API.Migrations
{
    /// <inheritdoc />
    public partial class AddAppConnections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppConnections",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WorkSpaceId = table.Column<int>(type: "int", nullable: true),
                    Provider = table.Column<int>(type: "int", nullable: false),
                    ExternalAccountId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SyncFrequency = table.Column<int>(type: "int", nullable: false),
                    LastSyncAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    LastSyncError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GrantedScopes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RemovedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppConnections_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppConnections_AspNetUsers_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppConnections_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppConnections_WorkSpaces_WorkSpaceId",
                        column: x => x.WorkSpaceId,
                        principalTable: "WorkSpaces",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LinkedData",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WorkSpaceId = table.Column<int>(type: "int", nullable: true),
                    AppConnectionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Provider = table.Column<int>(type: "int", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RawData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SyncedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RemovedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkedData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LinkedData_AppConnections_AppConnectionId",
                        column: x => x.AppConnectionId,
                        principalTable: "AppConnections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LinkedData_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LinkedData_AspNetUsers_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LinkedData_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LinkedData_WorkSpaces_WorkSpaceId",
                        column: x => x.WorkSpaceId,
                        principalTable: "WorkSpaces",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ce7b85e7-0058-40a4-b306-8974504ac779",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEMWSqISbkz5MJ3rUTYPZ2qFXGTSfQmtj40ACojZFyOXQ6GsEZS3S3UGDQaioYR40Sw==");

            migrationBuilder.CreateIndex(
                name: "IX_AppConnections_CreatedById",
                table: "AppConnections",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_AppConnections_UpdatedById",
                table: "AppConnections",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_AppConnections_UserId",
                table: "AppConnections",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppConnections_WorkSpaceId",
                table: "AppConnections",
                column: "WorkSpaceId");

            migrationBuilder.CreateIndex(
                name: "IX_LinkedData_AppConnectionId",
                table: "LinkedData",
                column: "AppConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_LinkedData_CreatedById",
                table: "LinkedData",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_LinkedData_UpdatedById",
                table: "LinkedData",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_LinkedData_UserId",
                table: "LinkedData",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LinkedData_WorkSpaceId",
                table: "LinkedData",
                column: "WorkSpaceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LinkedData");

            migrationBuilder.DropTable(
                name: "AppConnections");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ce7b85e7-0058-40a4-b306-8974504ac779",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEAmeuZnmVQNw+PV3BycAkds2w/Yy4u8M0USBsM+jjBwwfbaflBjAkKcwwxEsbbMLqQ==");
        }
    }
}
