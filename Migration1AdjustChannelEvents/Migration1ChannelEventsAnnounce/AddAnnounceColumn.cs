using Microsoft.EntityFrameworkCore.Migrations;

namespace EFMigrations.Migration1ChannelEventsAnnounce
{
    public partial class AddAnnounceColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable("ChannelEvents");
            migrationBuilder.AddColumn<int>(name: "Announce", table: "ChannelEvents", type: "integer", nullable: false);
        }
    }
}
