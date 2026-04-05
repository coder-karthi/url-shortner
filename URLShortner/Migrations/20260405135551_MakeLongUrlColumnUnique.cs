using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URLShortner.Migrations
{
    /// <inheritdoc />
    public partial class MakeLongUrlColumnUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_url_mappings_LongUrl",
                table: "url_mappings",
                column: "LongUrl",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_url_mappings_LongUrl",
                table: "url_mappings");
        }
    }
}
