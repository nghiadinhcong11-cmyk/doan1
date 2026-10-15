using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeTypeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmployeeType",
                table: "Employees",
                type: "text",
                nullable: false,
                defaultValue: "FullTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeeType",
                table: "Employees");
        }
    }
}
