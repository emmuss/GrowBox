using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrowBox.Server.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.CreateTable(
            //     name: "GrowBoxes",
            //     columns: table => new
            //     {
            //         Id = table.Column<Guid>(type: "uuid", nullable: false),
            //         Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            //         Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            //         Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
            //         Icon = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
            //         Image = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
            //         GrowBoxUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
            //         WebCamStreamUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
            //         WebCamSnapshotUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_GrowBoxes", x => x.Id);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "SensorReadings",
            //     columns: table => new
            //     {
            //         Id = table.Column<Guid>(type: "uuid", nullable: false),
            //         GrowBoxId = table.Column<Guid>(type: "uuid", nullable: false),
            //         Type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
            //         Value = table.Column<double>(type: "double precision", nullable: false),
            //         Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_SensorReadings", x => x.Id);
            //         table.ForeignKey(
            //             name: "FK_SensorReadings_GrowBoxes_GrowBoxId",
            //             column: x => x.GrowBoxId,
            //             principalTable: "GrowBoxes",
            //             principalColumn: "Id",
            //             onDelete: ReferentialAction.Cascade);
            //     });
            //
            // migrationBuilder.CreateIndex(
            //     name: "IX_SensorReadings_GrowBoxId",
            //     table: "SensorReadings",
            //     column: "GrowBoxId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SensorReadings");

            migrationBuilder.DropTable(
                name: "GrowBoxes");
        }
    }
}
