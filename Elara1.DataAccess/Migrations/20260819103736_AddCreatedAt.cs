using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elara1.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MemoryFacts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "MemoryFacts",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 19, 10, 37, 35, 879, DateTimeKind.Utc).AddTicks(1848));

            migrationBuilder.UpdateData(
                table: "MemoryFacts",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 19, 10, 37, 35, 879, DateTimeKind.Utc).AddTicks(1849));

            migrationBuilder.UpdateData(
                table: "MemoryFacts",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 19, 10, 37, 35, 879, DateTimeKind.Utc).AddTicks(1849));

            migrationBuilder.UpdateData(
                table: "MemoryFacts",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 19, 10, 37, 35, 879, DateTimeKind.Utc).AddTicks(1850));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MemoryFacts");
        }
    }
}
