using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations;

public partial class AddStockReceiptPrintOptions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>("ShowApprovedBy", "ReceiptSettings", type: "boolean", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<bool>("ShowPreparedBy", "ReceiptSettings", type: "boolean", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<bool>("ShowSignatures", "ReceiptSettings", type: "boolean", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<bool>("ShowSupplier", "ReceiptSettings", type: "boolean", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<bool>("ShowUnit", "ReceiptSettings", type: "boolean", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<bool>("ShowUnitPrice", "ReceiptSettings", type: "boolean", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<bool>("ShowWarehouse", "ReceiptSettings", type: "boolean", nullable: false, defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("ShowApprovedBy", "ReceiptSettings");
        migrationBuilder.DropColumn("ShowPreparedBy", "ReceiptSettings");
        migrationBuilder.DropColumn("ShowSignatures", "ReceiptSettings");
        migrationBuilder.DropColumn("ShowSupplier", "ReceiptSettings");
        migrationBuilder.DropColumn("ShowUnit", "ReceiptSettings");
        migrationBuilder.DropColumn("ShowUnitPrice", "ReceiptSettings");
        migrationBuilder.DropColumn("ShowWarehouse", "ReceiptSettings");
    }
}
