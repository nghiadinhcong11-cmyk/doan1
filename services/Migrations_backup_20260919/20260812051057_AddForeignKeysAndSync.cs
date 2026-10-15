using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKeysAndSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Orders_OrderId",
                table: "OrderDetails");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSchedules_BranchId",
                table: "WorkSchedules",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSchedules_EmployeeId",
                table: "WorkSchedules",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Tables_BranchId",
                table: "Tables",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ProductId",
                table: "OrderDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_BranchId",
                table: "Employees",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_BranchId",
                table: "Attendances",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_EmployeeId",
                table: "Attendances",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Branches_BranchId",
                table: "Attendances",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Employees_EmployeeId",
                table: "Attendances",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Branches_BranchId",
                table: "Employees",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Orders_OrderId",
                table: "OrderDetails",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Products_ProductId",
                table: "OrderDetails",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tables_Branches_BranchId",
                table: "Tables",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkSchedules_Branches_BranchId",
                table: "WorkSchedules",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkSchedules_Employees_EmployeeId",
                table: "WorkSchedules",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Branches_BranchId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Employees_EmployeeId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Branches_BranchId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Orders_OrderId",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Products_ProductId",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Tables_Branches_BranchId",
                table: "Tables");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkSchedules_Branches_BranchId",
                table: "WorkSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkSchedules_Employees_EmployeeId",
                table: "WorkSchedules");

            migrationBuilder.DropIndex(
                name: "IX_WorkSchedules_BranchId",
                table: "WorkSchedules");

            migrationBuilder.DropIndex(
                name: "IX_WorkSchedules_EmployeeId",
                table: "WorkSchedules");

            migrationBuilder.DropIndex(
                name: "IX_Tables_BranchId",
                table: "Tables");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_ProductId",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_Employees_BranchId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_BranchId",
                table: "Attendances");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_EmployeeId",
                table: "Attendances");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Orders_OrderId",
                table: "OrderDetails",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");
        }
    }
}
