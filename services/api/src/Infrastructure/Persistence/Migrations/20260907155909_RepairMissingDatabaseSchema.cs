using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantPOS.api.src.Infrastructure.Persistence.Migrations
{
    public partial class RepairMissingDatabaseSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Employees.EmployeeType
            migrationBuilder.Sql("""
                ALTER TABLE "Employees"
                ADD COLUMN IF NOT EXISTS "EmployeeType" text NOT NULL DEFAULT 'FullTime';
                """);

            // 2. Orders.PaymentAt
            migrationBuilder.Sql("""
                ALTER TABLE "Orders"
                ADD COLUMN IF NOT EXISTS "PaymentAt" timestamp with time zone NULL;
                """);

            // 3. EmployeeSalaryProfiles
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "EmployeeSalaryProfiles"
                (
                    "Id" uuid NOT NULL PRIMARY KEY,
                    "EmployeeId" uuid NOT NULL,
                    "BranchId" uuid NOT NULL,
                    "EmployeeType" text NOT NULL,
                    "MonthlySalary" numeric NOT NULL,
                    "HourlyRate" numeric NOT NULL,
                    "StandardWorkDays" numeric NOT NULL,
                    "StandardWorkHours" numeric NOT NULL,
                    "OvertimeRate" numeric NOT NULL,
                    "Allowance" numeric NOT NULL,
                    "EffectiveFrom" timestamp with time zone NOT NULL,
                    "EffectiveTo" timestamp with time zone NULL,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    "UpdatedAt" timestamp with time zone NOT NULL
                );
                """);

            // 4. PayrollSettings
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "PayrollSettings"
                (
                    "Id" uuid NOT NULL PRIMARY KEY,
                    "BranchId" uuid NULL,
                    "StandardWorkDays" numeric NOT NULL,
                    "StandardWorkHoursPerDay" numeric NOT NULL,
                    "FullTimeDeductionPerMissingDay" numeric NOT NULL,
                    "DefaultPartTimeHourlyRate" numeric NOT NULL,
                    "OvertimeEnabled" boolean NOT NULL,
                    "OvertimeMultiplier" numeric NOT NULL,
                    "UpdatedAt" timestamp with time zone NOT NULL
                );
                """);

            // 5. Payrolls
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "Payrolls"
                (
                    "Id" uuid NOT NULL PRIMARY KEY,
                    "EmployeeId" uuid NOT NULL,
                    "BranchId" uuid NOT NULL,
                    "Month" integer NOT NULL,
                    "Year" integer NOT NULL,
                    "EmployeeType" text NOT NULL,
                    "StandardWorkDays" numeric NOT NULL,
                    "ActualWorkDays" numeric NOT NULL,
                    "TotalHours" numeric NOT NULL,
                    "RegularHours" numeric NOT NULL,
                    "OvertimeHours" numeric NOT NULL,
                    "BaseSalary" numeric NOT NULL,
                    "HourlyRate" numeric NOT NULL,
                    "RegularPay" numeric NOT NULL,
                    "OvertimePay" numeric NOT NULL,
                    "Allowance" numeric NOT NULL,
                    "Bonus" numeric NOT NULL,
                    "Deduction" numeric NOT NULL,
                    "GrossSalary" numeric NOT NULL,
                    "NetSalary" numeric NOT NULL,
                    "Status" text NOT NULL,
                    "CalculatedAt" timestamp with time zone NULL,
                    "LockedAt" timestamp with time zone NULL,
                    "PaidAt" timestamp with time zone NULL,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    "UpdatedAt" timestamp with time zone NOT NULL
                );
                """);

            // 6. PayrollAdjustments
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "PayrollAdjustments"
                (
                    "Id" uuid NOT NULL PRIMARY KEY,
                    "PayrollId" uuid NOT NULL,
                    "Type" text NOT NULL,
                    "Amount" numeric NOT NULL,
                    "Reason" text NOT NULL,
                    "CreatedBy" uuid NULL,
                    "CreatedAt" timestamp with time zone NOT NULL
                );
                """);

            // 7. Indexes
            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS
                "IX_EmployeeSalaryProfiles_EmployeeId_BranchId_EffectiveFrom"
                ON "EmployeeSalaryProfiles"
                ("EmployeeId", "BranchId", "EffectiveFrom");

                CREATE UNIQUE INDEX IF NOT EXISTS
                "IX_PayrollSettings_BranchId"
                ON "PayrollSettings"
                ("BranchId");

                CREATE UNIQUE INDEX IF NOT EXISTS
                "IX_Payrolls_EmployeeId_BranchId_Month_Year"
                ON "Payrolls"
                ("EmployeeId", "BranchId", "Month", "Year");

                CREATE INDEX IF NOT EXISTS
                "IX_PayrollAdjustments_PayrollId_CreatedAt"
                ON "PayrollAdjustments"
                ("PayrollId", "CreatedAt");
                """);

            // 8. PayrollAdjustment -> Payroll FK
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'FK_PayrollAdjustments_Payrolls_PayrollId'
                    ) THEN
                        ALTER TABLE "PayrollAdjustments"
                        ADD CONSTRAINT "FK_PayrollAdjustments_Payrolls_PayrollId"
                        FOREIGN KEY ("PayrollId")
                        REFERENCES "Payrolls" ("Id")
                        ON DELETE CASCADE;
                    END IF;
                END $$;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Không rollback để tránh xóa dữ liệu hiện có.
        }
    }
}