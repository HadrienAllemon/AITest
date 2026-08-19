using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elara1.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddHistoryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Conversation, Roles, and Messages (with their PK/FK constraints) already existed
            // in the database before EF was involved -- see the manual schema-fix script that
            // added the missing primary keys and made Messages.Id an identity column. The
            // generated CreateTable calls for those three were removed here so this migration
            // only adds what was genuinely missing (the FK-column indexes) and baselines EF's
            // migration history against the real schema, instead of re-creating existing tables.

            migrationBuilder.UpdateData(
                table: "MemoryFacts",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "MemoryFacts",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "MemoryFacts",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "MemoryFacts",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId",
                table: "Messages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_RoleId",
                table: "Messages",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_ConversationId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_RoleId",
                table: "Messages");

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
    }
}
