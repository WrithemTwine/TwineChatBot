using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamerBotLib.Migrations
{
    /// <inheritdoc />
    public partial class AddCommandPlatformMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommandPlatformMessages",
                columns: table => new
                {
                    CmdName = table.Column<string>(type: "TEXT", nullable: false),
                    Platform = table.Column<int>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandPlatformMessages", x => new { x.CmdName, x.Platform });
                    table.ForeignKey(
                        name: "FK_CommandPlatformMessages_CommandsBase_CmdName",
                        column: x => x.CmdName,
                        principalTable: "CommandsBase",
                        principalColumn: "CmdName",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommandPlatformMessages");
        }
    }
}
