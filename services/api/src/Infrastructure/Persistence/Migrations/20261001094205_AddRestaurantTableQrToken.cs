using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRestaurantTableQrToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QrToken",
                table: "Tables",
                type: "character varying(43)",
                maxLength: 43,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tables_QrToken",
                table: "Tables",
                column: "QrToken",
                unique: true,
                filter: "\"QrToken\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tables_QrToken",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "QrToken",
                table: "Tables");
        }
    }
}
