using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddScopedReceiptSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReceiptSettings_BranchId",
                table: "ReceiptSettings");

            migrationBuilder.AddColumn<string>(
                name: "DocumentType",
                table: "ReceiptSettings",
                type: "text",
                nullable: false,
                defaultValue: "Invoice");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptSettings_BranchId_DocumentType",
                table: "ReceiptSettings",
                columns: new[] { "BranchId", "DocumentType" },
                unique: true,
                filter: "\"BranchId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptSettings_DocumentType",
                table: "ReceiptSettings",
                column: "DocumentType",
                unique: true,
                filter: "\"BranchId\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReceiptSettings_BranchId_DocumentType",
                table: "ReceiptSettings");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptSettings_DocumentType",
                table: "ReceiptSettings");

            migrationBuilder.DropColumn(
                name: "DocumentType",
                table: "ReceiptSettings");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptSettings_BranchId",
                table: "ReceiptSettings",
                column: "BranchId",
                unique: true,
                filter: "\"BranchId\" IS NOT NULL");
        }
    }
}
