using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_genda_API.Migrations
{
    /// <inheritdoc />
    public partial class FixAppConnectionShadowProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppConnections_AspNetUsers_UpdatedById",
                table: "AppConnections");

            // تم تعليق إنشاء جدول LinkedData والـ Indexes بتاعته لأنه موجود بالفعل في سيرفر الـ localhost الحقيقي
            /*
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
            */

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ce7b85e7-0058-40a4-b306-8974504ac779",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEFj4tX4+yRh3OU2vRNAd2e7YPXxgv7ie2XvFalW2sX9xEjfFHyAobEVVbYb4lI9vuw==");

            migrationBuilder.AddForeignKey(
                name: "FK_AppConnections_AspNetUsers_UpdatedById",
                table: "AppConnections",
                column: "UpdatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppConnections_AspNetUsers_UpdatedById",
                table: "AppConnections");

            // تم تعليق حذف الجدول هنا أيضاً لتتوافق مع دالة Up
            // migrationBuilder.DropTable(name: "LinkedData");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ce7b85e7-0058-40a4-b306-8974504ac779",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAELoC3Ftr7L/ETjjZyLUxdqudicv4FkYoy7kjq8JONDzz1URghftfhl4WkiDDSJje9Q==");

            migrationBuilder.AddForeignKey(
                name: "FK_AppConnections_AspNetUsers_UpdatedById",
                table: "AppConnections",
                column: "UpdatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}