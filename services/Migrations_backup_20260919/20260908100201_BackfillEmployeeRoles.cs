using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BackfillEmployeeRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Mặc định tất cả là employee
            migrationBuilder.Sql("UPDATE \"Employees\" SET \"Role\" = 'employee'");

            // 2. Map Quản lý -> manager (trừ admin)
            migrationBuilder.Sql("UPDATE \"Employees\" SET \"Role\" = 'manager' WHERE \"Position\" = 'Quản lý'");

            // 3. Map Thu ngân -> cashier
            migrationBuilder.Sql("UPDATE \"Employees\" SET \"Role\" = 'cashier' WHERE \"Position\" = 'Thu ngân'");

            // 4. Map Bếp/Pha chế -> kitchen
            migrationBuilder.Sql("UPDATE \"Employees\" SET \"Role\" = 'kitchen' WHERE \"Position\" IN ('Đầu bếp', 'Pha chế')");

            // 5. Hard-code admin account if exists by username
            migrationBuilder.Sql("UPDATE \"Employees\" SET \"Role\" = 'admin' WHERE \"Username\" = 'admin'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
