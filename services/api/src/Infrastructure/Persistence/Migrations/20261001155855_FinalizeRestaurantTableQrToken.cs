using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FinalizeRestaurantTableQrToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tables_QrToken",
                table: "Tables");

            migrationBuilder.AlterColumn<string>(
                name: "QrToken",
                table: "Tables",
                type: "character varying(43)",
                maxLength: 43,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(43)",
                oldMaxLength: 43,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tables_QrToken",
                table: "Tables",
                column: "QrToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tables_QrToken",
                table: "Tables");

            migrationBuilder.AlterColumn<string>(
                name: "QrToken",
                table: "Tables",
                type: "character varying(43)",
                maxLength: 43,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(43)",
                oldMaxLength: 43);

            migrationBuilder.CreateIndex(
                name: "IX_Tables_QrToken",
                table: "Tables",
                column: "QrToken",
                unique: true,
                filter: "\"QrToken\" IS NOT NULL");
        }
    }
}
