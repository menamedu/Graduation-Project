using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradProject.Migrations
{
    /// <inheritdoc />
    public partial class editLabTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Flag",
                table: "Labs",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Labs",
                keyColumn: "Id",
                keyValue: 1,
                column: "Flag",
                value: "LowPassFlag");

            migrationBuilder.UpdateData(
                table: "Vulnerabilities",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 1, 7, 54, 18, 332, DateTimeKind.Utc).AddTicks(5851));

            migrationBuilder.UpdateData(
                table: "Vulnerabilities",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 1, 7, 54, 18, 332, DateTimeKind.Utc).AddTicks(5855));

            migrationBuilder.UpdateData(
                table: "Vulnerabilities",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 1, 7, 54, 18, 332, DateTimeKind.Utc).AddTicks(5856));

            migrationBuilder.UpdateData(
                table: "Vulnerabilities",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 1, 7, 54, 18, 332, DateTimeKind.Utc).AddTicks(5858));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Flag",
                table: "Labs");

            migrationBuilder.UpdateData(
                table: "Vulnerabilities",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 30, 18, 37, 11, 578, DateTimeKind.Utc).AddTicks(6408));

            migrationBuilder.UpdateData(
                table: "Vulnerabilities",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 30, 18, 37, 11, 578, DateTimeKind.Utc).AddTicks(6413));

            migrationBuilder.UpdateData(
                table: "Vulnerabilities",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 30, 18, 37, 11, 578, DateTimeKind.Utc).AddTicks(6415));

            migrationBuilder.UpdateData(
                table: "Vulnerabilities",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 30, 18, 37, 11, 578, DateTimeKind.Utc).AddTicks(6417));
        }
    }
}
