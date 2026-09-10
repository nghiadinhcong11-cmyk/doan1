using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations;

public partial class AddReceiptSettings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ReceiptSettings",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                PaperWidth = table.Column<int>(type: "integer", nullable: false, defaultValue: 80),
                ShowLogo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                ShowAddress = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                ShowPhone = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                ShowStaff = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                ShowPaymentMethod = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                ShowOrderNote = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                ShowThankYou = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                ThankYouText = table.Column<string>(type: "text", nullable: false, defaultValue: "Cảm ơn quý khách và hẹn gặp lại!"),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ReceiptSettings", x => x.Id));
        migrationBuilder.CreateIndex("IX_ReceiptSettings_BranchId", "ReceiptSettings", "BranchId", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("ReceiptSettings");
}
