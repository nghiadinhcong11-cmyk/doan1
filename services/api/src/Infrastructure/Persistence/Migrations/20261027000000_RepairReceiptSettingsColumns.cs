using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations;

public partial class RepairReceiptSettingsColumns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // The earlier receipt-options migration can be present in migration
        // history while these columns are absent in a restored/local database.
        // IF NOT EXISTS repairs that drift without touching existing values.
        migrationBuilder.Sql("""
            ALTER TABLE public."ReceiptSettings"
                ADD COLUMN IF NOT EXISTS "ShowApprovedBy" boolean NOT NULL DEFAULT FALSE,
                ADD COLUMN IF NOT EXISTS "ShowPreparedBy" boolean NOT NULL DEFAULT FALSE,
                ADD COLUMN IF NOT EXISTS "ShowSignatures" boolean NOT NULL DEFAULT FALSE,
                ADD COLUMN IF NOT EXISTS "ShowSupplier" boolean NOT NULL DEFAULT FALSE,
                ADD COLUMN IF NOT EXISTS "ShowUnit" boolean NOT NULL DEFAULT FALSE,
                ADD COLUMN IF NOT EXISTS "ShowUnitPrice" boolean NOT NULL DEFAULT FALSE,
                ADD COLUMN IF NOT EXISTS "ShowWarehouse" boolean NOT NULL DEFAULT FALSE;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            ALTER TABLE public."ReceiptSettings"
                DROP COLUMN IF EXISTS "ShowApprovedBy",
                DROP COLUMN IF EXISTS "ShowPreparedBy",
                DROP COLUMN IF EXISTS "ShowSignatures",
                DROP COLUMN IF EXISTS "ShowSupplier",
                DROP COLUMN IF EXISTS "ShowUnit",
                DROP COLUMN IF EXISTS "ShowUnitPrice",
                DROP COLUMN IF EXISTS "ShowWarehouse";
            """);
    }
}
