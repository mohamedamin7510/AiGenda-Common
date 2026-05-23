using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_genda_API.Migrations
{
    /// <inheritdoc />
    public partial class AlignOAuthWithValueConverter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ce7b85e7-0058-40a4-b306-8974504ac779",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEDkBEXtaA/Uz1ryzPXBJyltBWAnu8iOakLWuip/BlIU310BeNUKW9lMrqGG4To20Bg==");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ce7b85e7-0058-40a4-b306-8974504ac779",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAELoC3Ftr7L/ETjjZyLUxdqudicv4FkYoy7kjq8JONDzz1URghftfhl4WkiDDSJje9Q==");
        }
    }
}