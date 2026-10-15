using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HardenEmployeeLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FontFamily",
                table: "ReceiptSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FontSize",
                table: "ReceiptSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeSalaryProfiles_Employees_EmployeeId",
                table: "EmployeeSalaryProfiles",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payrolls_Employees_EmployeeId",
                table: "Payrolls",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeSalaryProfiles_Employees_EmployeeId",
                table: "EmployeeSalaryProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Payrolls_Employees_EmployeeId",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "FontFamily",
                table: "ReceiptSettings");

            migrationBuilder.DropColumn(
                name: "FontSize",
                table: "ReceiptSettings");
        }
    }
}
