using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrowBox.Server.Migrations
{
    /// <inheritdoc />
    public partial class WaterPumps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WaterPumpsUrl",
                table: "GrowBoxes",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WaterPumpsUrl",
                table: "GrowBoxes");
        }
    }
}
