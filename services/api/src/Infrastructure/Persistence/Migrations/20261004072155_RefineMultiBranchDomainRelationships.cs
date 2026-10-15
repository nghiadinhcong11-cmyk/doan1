using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefineMultiBranchDomainRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SystemSettings_BranchId_Key",
                table: "SystemSettings");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptSettings_BranchId",
                table: "ReceiptSettings");

            migrationBuilder.AddColumn<Guid>(
                name: "AreaId",
                table: "Tables",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Areas",
                type: "uuid",
                nullable: true);

            // Safe phase-1 backfill: infer an Area branch only when every
            // existing table using that name belongs to exactly one branch.
            migrationBuilder.Sql("""
                UPDATE "Areas" AS a
                SET "BranchId" = candidates."BranchId"
                FROM (
                    SELECT "AreaName", (ARRAY_AGG("BranchId" ORDER BY "BranchId"))[1] AS "BranchId"
                    FROM "Tables"
                    WHERE "BranchId" IS NOT NULL AND "AreaName" IS NOT NULL
                    GROUP BY "AreaName"
                    HAVING COUNT(DISTINCT "BranchId") = 1
                ) AS candidates
                WHERE a."BranchId" IS NULL
                  AND a."Name" = candidates."AreaName";
                """);

            // Safe phase-1 backfill: map a table only when exactly one Area
            // with the same name exists in the table's branch.
            migrationBuilder.Sql("""
                WITH candidates AS (
                    SELECT t."Id" AS "TableId",
                           (ARRAY_AGG(a."Id" ORDER BY a."Id"))[1] AS "AreaId",
                           COUNT(a."Id") AS "MatchCount"
                    FROM "Tables" AS t
                    JOIN "Areas" AS a
                      ON a."Name" = t."AreaName"
                     AND a."BranchId" = t."BranchId"
                    WHERE t."AreaId" IS NULL
                    GROUP BY t."Id"
                )
                UPDATE "Tables" AS t
                SET "AreaId" = candidates."AreaId"
                FROM candidates
                WHERE t."Id" = candidates."TableId"
                  AND candidates."MatchCount" = 1;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Tables_AreaId",
                table: "Tables",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_BranchId_Key",
                table: "SystemSettings",
                columns: new[] { "BranchId", "Key" },
                unique: true,
                filter: "\"BranchId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_BranchId",
                table: "Shifts",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_EmployeeId",
                table: "Shifts",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_BranchId",
                table: "Reservations",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_TableId",
                table: "Reservations",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptSettings_BranchId",
                table: "ReceiptSettings",
                column: "BranchId",
                unique: true,
                filter: "\"BranchId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Areas_BranchId",
                table: "Areas",
                column: "BranchId");

            // PostgreSQL UNIQUE indexes treat NULL values as distinct. These
            // expression/partial indexes enforce one global row explicitly.
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX "UX_ReceiptSettings_Global"
                ON "ReceiptSettings" ((1))
                WHERE "BranchId" IS NULL;
                """);
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX "UX_SystemSettings_GlobalKey"
                ON "SystemSettings" ("Key")
                WHERE "BranchId" IS NULL;
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_Areas_Branches_BranchId",
                table: "Areas",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessInsights_Branches_BranchId",
                table: "BusinessInsights",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Branches_BranchId",
                table: "Orders",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptSettings_Branches_BranchId",
                table: "ReceiptSettings",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Branches_BranchId",
                table: "Reservations",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Tables_TableId",
                table: "Reservations",
                column: "TableId",
                principalTable: "Tables",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Branches_BranchId",
                table: "Shifts",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Employees_EmployeeId",
                table: "Shifts",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SystemSettings_Branches_BranchId",
                table: "SystemSettings",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tables_Areas_AreaId",
                table: "Tables",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Areas_Branches_BranchId",
                table: "Areas");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessInsights_Branches_BranchId",
                table: "BusinessInsights");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Branches_BranchId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptSettings_Branches_BranchId",
                table: "ReceiptSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Branches_BranchId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Tables_TableId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Branches_BranchId",
                table: "Shifts");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Employees_EmployeeId",
                table: "Shifts");

            migrationBuilder.DropForeignKey(
                name: "FK_SystemSettings_Branches_BranchId",
                table: "SystemSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_Tables_Areas_AreaId",
                table: "Tables");

            migrationBuilder.DropIndex(
                name: "IX_Tables_AreaId",
                table: "Tables");

            migrationBuilder.DropIndex(
                name: "IX_SystemSettings_BranchId_Key",
                table: "SystemSettings");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_BranchId",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_EmployeeId",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_BranchId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_TableId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptSettings_BranchId",
                table: "ReceiptSettings");

            migrationBuilder.DropIndex(
                name: "IX_Areas_BranchId",
                table: "Areas");

            migrationBuilder.Sql("DROP INDEX IF EXISTS \"UX_ReceiptSettings_Global\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"UX_SystemSettings_GlobalKey\";");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Areas");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_BranchId_Key",
                table: "SystemSettings",
                columns: new[] { "BranchId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptSettings_BranchId",
                table: "ReceiptSettings",
                column: "BranchId",
                unique: true);
        }
    }
}
