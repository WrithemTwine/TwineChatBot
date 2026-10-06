using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamerBotLib.DataSQL.Migrations
{
    /// <inheritdoc />
    public partial class OverlayServiceSelections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OverlayServicesSelection",
                columns: table => new
                {
                    OverlayType = table.Column<int>(type: "INTEGER", nullable: false),
                    OverlayAction = table.Column<string>(type: "TEXT", nullable: false),
                    SelectionType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OverlayServicesSelection", x => new { x.OverlayType, x.OverlayAction });
                });

            migrationBuilder.CreateIndex(
                name: "IX_OverlayServicesSelection_OverlayType_OverlayAction",
                table: "OverlayServicesSelection",
                columns: new[] { "OverlayType", "OverlayAction" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OverlayServicesSelection");
        }
    }
}
