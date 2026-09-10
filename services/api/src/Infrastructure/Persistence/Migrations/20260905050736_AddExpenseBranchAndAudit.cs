using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations;

public partial class AddExpenseBranchAndAudit : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>("BranchId", "Expenses", nullable: true);
        migrationBuilder.AddColumn<Guid>("CreatedBy", "Expenses", nullable: true);
        migrationBuilder.AddColumn<DateTime>("UpdatedAt", "Expenses", nullable: true);

        // Preserve existing records by assigning them to the current main/first branch.
        migrationBuilder.Sql("UPDATE \"Expenses\" SET \"BranchId\" = (SELECT \"Id\" FROM \"Branches\" ORDER BY \"IsMain\" DESC, \"CreatedAt\" LIMIT 1) WHERE \"BranchId\" IS NULL;");
        migrationBuilder.AlterColumn<Guid>("BranchId", "Expenses", type: "uuid", nullable: false, oldNullable: true);
        migrationBuilder.CreateIndex("IX_Expenses_BranchId_ExpenseDate", "Expenses", new[] { "BranchId", "ExpenseDate" });
        migrationBuilder.AddForeignKey(
            name: "FK_Expenses_Branches_BranchId",
            table: "Expenses",
            column: "BranchId",
            principalTable: "Branches",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey("FK_Expenses_Branches_BranchId", "Expenses");
        migrationBuilder.DropIndex("IX_Expenses_BranchId_ExpenseDate", "Expenses");
        migrationBuilder.DropColumn("BranchId", "Expenses");
        migrationBuilder.DropColumn("CreatedBy", "Expenses");
        migrationBuilder.DropColumn("UpdatedAt", "Expenses");
    }
}
