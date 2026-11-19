using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderEmployeeAuditForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedByEmployeeId",
                table: "Orders",
                column: "CreatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PaidByEmployeeId",
                table: "Orders",
                column: "PaidByEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Employees_CreatedByEmployeeId",
                table: "Orders",
                column: "CreatedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Employees_PaidByEmployeeId",
                table: "Orders",
                column: "PaidByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Employees_CreatedByEmployeeId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Employees_PaidByEmployeeId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CreatedByEmployeeId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PaidByEmployeeId",
                table: "Orders");
        }
    }
}
