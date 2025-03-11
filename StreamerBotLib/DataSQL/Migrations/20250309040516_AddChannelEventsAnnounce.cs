using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamerBotLib.DataSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddChannelEventsAnnounce : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Announce",
                table: "ChannelEvents",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Announce",
                table: "ChannelEvents");
        }
    }
}
