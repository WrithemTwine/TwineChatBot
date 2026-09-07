using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamerBotLib.Migrations
{
    /// <inheritdoc />
    public partial class PlatformMessages_UserStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomWelcomeMessages",
                table: "UserStats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GiveawaysEntered",
                table: "UserStats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GiveawaysWon",
                table: "UserStats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RaidsReceived",
                table: "UserStats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShoutOutsGiven",
                table: "UserStats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomWelcomeMessages",
                table: "UserStats");

            migrationBuilder.DropColumn(
                name: "GiveawaysEntered",
                table: "UserStats");

            migrationBuilder.DropColumn(
                name: "GiveawaysWon",
                table: "UserStats");

            migrationBuilder.DropColumn(
                name: "RaidsReceived",
                table: "UserStats");

            migrationBuilder.DropColumn(
                name: "ShoutOutsGiven",
                table: "UserStats");
        }
    }
}
