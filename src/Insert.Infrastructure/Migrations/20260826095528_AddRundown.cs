using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Insert.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRundown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RundownItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RundownId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    ItemType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StoryId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    SegmentLabel = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SegmentDurationSeconds = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RundownItems", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Rundowns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Program = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AirDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rundowns", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RundownItems");

            migrationBuilder.DropTable(
                name: "Rundowns");
        }
    }
}
