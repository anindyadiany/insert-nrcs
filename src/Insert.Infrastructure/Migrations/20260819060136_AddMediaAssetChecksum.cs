using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Insert.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaAssetChecksum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Checksum",
                table: "MediaAssets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Checksum",
                table: "MediaAssets");
        }
    }
}
