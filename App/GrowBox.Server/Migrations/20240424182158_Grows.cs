using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrowBox.Server.Migrations
{
    /// <inheritdoc />
    public partial class Grows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiaryNotes");

            migrationBuilder.CreateTable(
                name: "Grows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    GrowBoxId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Grows_GrowBoxes_GrowBoxId",
                        column: x => x.GrowBoxId,
                        principalTable: "GrowBoxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrowDiaryNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GrowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Text = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrowDiaryNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrowDiaryNotes_Grows_GrowId",
                        column: x => x.GrowId,
                        principalTable: "Grows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrowDiaryNoteMedias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GrowDiaryNoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    MimeType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Data = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrowDiaryNoteMedias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrowDiaryNoteMedias_GrowDiaryNotes_GrowDiaryNoteId",
                        column: x => x.GrowDiaryNoteId,
                        principalTable: "GrowDiaryNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GrowDiaryNoteMedias_GrowDiaryNoteId",
                table: "GrowDiaryNoteMedias",
                column: "GrowDiaryNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_GrowDiaryNotes_GrowId",
                table: "GrowDiaryNotes",
                column: "GrowId");

            migrationBuilder.CreateIndex(
                name: "IX_Grows_GrowBoxId",
                table: "Grows",
                column: "GrowBoxId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GrowDiaryNoteMedias");

            migrationBuilder.DropTable(
                name: "GrowDiaryNotes");

            migrationBuilder.DropTable(
                name: "Grows");

            migrationBuilder.CreateTable(
                name: "DiaryNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GrowBoxId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiaryNotes", x => x.Id);
                });
        }
    }
}
