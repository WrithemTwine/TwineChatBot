using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamerBotLib.DataSQL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuoteTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoryName",
                table: "Quotes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "QuoteDate",
                table: "Quotes",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryName",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "QuoteDate",
                table: "Quotes");
        }
    }
}
