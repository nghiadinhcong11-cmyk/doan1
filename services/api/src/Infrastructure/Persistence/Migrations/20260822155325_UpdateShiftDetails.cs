using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateShiftDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BranchName",
                table: "Shifts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CashRevenue",
                table: "Shifts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransferRevenue",
                table: "Shifts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BranchName",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "CashRevenue",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "TransferRevenue",
                table: "Shifts");
        }
    }
}
