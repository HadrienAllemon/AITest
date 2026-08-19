using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Elara1.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemoryFacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemoryFacts", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "MemoryFacts",
                columns: new[] { "Id", "Content" },
                values: new object[,]
                {
                    { 1, "User works as a software engineer at a tech firm." },
                    { 2, "User's partner is named Alex." },
                    { 3, "User tends to feel overwhelmed when deadlines pile up on Fridays." },
                    { 4, "User's favorite way to relax is going for evening walks." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemoryFacts");
        }
    }
}
