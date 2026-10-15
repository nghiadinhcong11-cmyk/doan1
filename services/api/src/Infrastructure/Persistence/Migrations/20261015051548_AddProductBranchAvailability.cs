using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

using Microsoft.EntityFrameworkCore.Infrastructure;
using RestaurantPOS.Infrastructure.Persistence;

namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261015051548_AddProductBranchAvailability")]
    public partial class AddProductBranchAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductBranchAvailabilities",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductBranchAvailabilities", x => new { x.ProductId, x.BranchId });
                    table.ForeignKey(
                        name: "FK_ProductBranchAvailabilities_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductBranchAvailabilities_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductBranchAvailabilities_BranchId_Status",
                table: "ProductBranchAvailabilities",
                columns: new[] { "BranchId", "Status" });

            // Preserve the legacy product-level availability as the initial value
            // for every existing product/branch pair. Existing rows remain the
            // source of truth until this migration is applied to the target DB.
            migrationBuilder.Sql("""
                INSERT INTO "ProductBranchAvailabilities" ("ProductId", "BranchId", "Status", "UpdatedAtUtc")
                SELECT p."Id", b."Id", COALESCE(NULLIF(p."AvailabilityStatus", ''), 'Available'), CURRENT_TIMESTAMP
                FROM "Products" p
                CROSS JOIN "Branches" b
                ON CONFLICT ("ProductId", "BranchId") DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductBranchAvailabilities");
        }
    }
}
