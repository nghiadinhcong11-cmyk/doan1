using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemovePayrollAndRepairReceiptSettings : Migration
    {
        /// <inheritdoc />
       protected override void Up(MigrationBuilder migrationBuilder)
       {
           migrationBuilder.DropTable(
               name: "EmployeeSalaryProfiles");

           migrationBuilder.DropTable(
               name: "PayrollAdjustments");

           migrationBuilder.DropTable(
               name: "PayrollSettings");

           migrationBuilder.DropTable(
               name: "Payrolls");

           migrationBuilder.CreateTable(
               name: "ReceiptSettings",
               columns: table => new
               {
                   Id = table.Column<Guid>(
                       type: "uuid",
                       nullable: false),

                   BranchId = table.Column<Guid?>(
                       type: "uuid",
                       nullable: true),

                   PaperWidth = table.Column<int>(
                       type: "integer",
                       nullable: false,
                       defaultValue: 80),

                   ShowLogo = table.Column<bool>(
                       type: "boolean",
                       nullable: false,
                       defaultValue: true),

                   ShowAddress = table.Column<bool>(
                       type: "boolean",
                       nullable: false,
                       defaultValue: true),

                   ShowPhone = table.Column<bool>(
                       type: "boolean",
                       nullable: false,
                       defaultValue: true),

                   ShowStaff = table.Column<bool>(
                       type: "boolean",
                       nullable: false,
                       defaultValue: true),

                   ShowPaymentMethod = table.Column<bool>(
                       type: "boolean",
                       nullable: false,
                       defaultValue: true),

                   ShowOrderNote = table.Column<bool>(
                       type: "boolean",
                       nullable: false,
                       defaultValue: true),

                   ShowThankYou = table.Column<bool>(
                       type: "boolean",
                       nullable: false,
                       defaultValue: true),

                   ThankYouText = table.Column<string>(
                       type: "text",
                       nullable: false,
                       defaultValue: "Thank you!"),

                   UpdatedAt = table.Column<DateTime>(
                       type: "timestamp with time zone",
                       nullable: false),

                   FontFamily = table.Column<string>(
                       type: "text",
                       nullable: false,
                       defaultValue: ""),

                   FontSize = table.Column<int>(
                       type: "integer",
                       nullable: false,
                       defaultValue: 0)
               },
               constraints: table =>
               {
                   table.PrimaryKey("PK_ReceiptSettings", x => x.Id);
               });

           migrationBuilder.CreateIndex(
               name: "IX_ReceiptSettings_BranchId",
               table: "ReceiptSettings",
               column: "BranchId",
               unique: true);
       }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FontFamily",
                table: "ReceiptSettings");

            migrationBuilder.DropColumn(
                name: "FontSize",
                table: "ReceiptSettings");

            migrationBuilder.CreateTable(
                name: "EmployeeSalaryProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Allowance = table.Column<decimal>(type: "numeric", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeType = table.Column<string>(type: "text", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "numeric", nullable: false),
                    MonthlySalary = table.Column<decimal>(type: "numeric", nullable: false),
                    OvertimeRate = table.Column<decimal>(type: "numeric", nullable: false),
                    StandardWorkDays = table.Column<decimal>(type: "numeric", nullable: false),
                    StandardWorkHours = table.Column<decimal>(type: "numeric", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeSalaryProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Payrolls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActualWorkDays = table.Column<decimal>(type: "numeric", nullable: false),
                    Allowance = table.Column<decimal>(type: "numeric", nullable: false),
                    BaseSalary = table.Column<decimal>(type: "numeric", nullable: false),
                    Bonus = table.Column<decimal>(type: "numeric", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Deduction = table.Column<decimal>(type: "numeric", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeType = table.Column<string>(type: "text", nullable: false),
                    GrossSalary = table.Column<decimal>(type: "numeric", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "numeric", nullable: false),
                    LockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    NetSalary = table.Column<decimal>(type: "numeric", nullable: false),
                    OvertimeHours = table.Column<decimal>(type: "numeric", nullable: false),
                    OvertimePay = table.Column<decimal>(type: "numeric", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RegularHours = table.Column<decimal>(type: "numeric", nullable: false),
                    RegularPay = table.Column<decimal>(type: "numeric", nullable: false),
                    StandardWorkDays = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    TotalHours = table.Column<decimal>(type: "numeric", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payrolls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    DefaultPartTimeHourlyRate = table.Column<decimal>(type: "numeric", nullable: false),
                    FullTimeDeductionPerMissingDay = table.Column<decimal>(type: "numeric", nullable: false),
                    OvertimeEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    OvertimeMultiplier = table.Column<decimal>(type: "numeric", nullable: false),
                    StandardWorkDays = table.Column<decimal>(type: "numeric", nullable: false),
                    StandardWorkHoursPerDay = table.Column<decimal>(type: "numeric", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollAdjustments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    PayrollId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollAdjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollAdjustments_Payrolls_PayrollId",
                        column: x => x.PayrollId,
                        principalTable: "Payrolls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalaryProfiles_EmployeeId_BranchId_EffectiveFrom",
                table: "EmployeeSalaryProfiles",
                columns: new[] { "EmployeeId", "BranchId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_PayrollAdjustments_PayrollId_CreatedAt",
                table: "PayrollAdjustments",
                columns: new[] { "PayrollId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Payrolls_EmployeeId_BranchId_Month_Year",
                table: "Payrolls",
                columns: new[] { "EmployeeId", "BranchId", "Month", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollSettings_BranchId",
                table: "PayrollSettings",
                column: "BranchId",
                unique: true);
        }
    }
}
