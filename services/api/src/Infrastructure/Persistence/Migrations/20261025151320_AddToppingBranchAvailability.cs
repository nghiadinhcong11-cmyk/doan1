using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddToppingBranchAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ToppingBranchAvailabilities",
                columns: table => new
                {
                    ToppingId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToppingBranchAvailabilities", x => new { x.ToppingId, x.BranchId });
                    table.ForeignKey(
                        name: "FK_ToppingBranchAvailabilities_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ToppingBranchAvailabilities_Toppings_ToppingId",
                        column: x => x.ToppingId,
                        principalTable: "Toppings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToppingBranchAvailabilities_BranchId_Status",
                table: "ToppingBranchAvailabilities",
                columns: new[] { "BranchId", "Status" });

            migrationBuilder.Sql("""
                INSERT INTO "ToppingBranchAvailabilities" ("ToppingId", "BranchId", "Status", "UpdatedAtUtc")
                SELECT topping."Id", branch."Id", 'Available', NOW()
                FROM "Toppings" topping
                CROSS JOIN "Branches" branch
                ON CONFLICT ("ToppingId", "BranchId") DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToppingBranchAvailabilities");

        }
    }
}
