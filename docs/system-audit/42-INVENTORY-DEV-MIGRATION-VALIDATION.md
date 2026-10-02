# Task 42 — Inventory DEV Migration Validation

## 1. Executive Summary

Task 42 is prepared for human execution only. The approved migration `20261001044428_AddInventoryManagement` passed the offline static review and local build/test checks. Codex did not run `dotnet ef database update`, did not start the API, and did not connect to Supabase.

Host application and post-migration validation remain pending until the human runs them from normal Windows PowerShell against the authorized isolated DEV project.

## 2. Authorized DEV Target

```text
Project ref: qfkgjxwbshjgsxsvkpkp
Host: aws-0-ap-southeast-2.pooler.supabase.com
Port: 5432
Database: postgres
Username identity: postgres.qfkgjxwbshjgsxsvkpkp
```

The previous reference `vyphnxzunqthjfypxcab` is unauthorized and is rejected by the diagnostic. No credentials are recorded here.

## 3. Host Execution Context

The migration must be run by the human from normal Windows PowerShell. Previous evidence established that the normal host has TCP, TLS, PostgreSQL authentication, and `SELECT 1` working. The Codex execution context must not run the migration because of its Schannel TLS limitation.

## 4. Pre-Apply Verification

Static inspection of `20261001044428_AddInventoryManagement` confirms:

- exactly seven Inventory tables are created;
- `Expenses.StockReceiptId` is nullable with a unique index and FK;
- no operation references Employees, EmployeeType, BasicSalary, Payroll, Orders, OrderDetails, Products, Customers, ReceiptSettings, or BusinessInsights;
- no unrelated destructive operation is present in `Up()`;
- `Down()` reverses only the Expense link and seven Inventory tables;
- the designer contains normal EF migration metadata.

## 5. Migration Command

From `D:\Dev\doan`, after the target check passes, the human should run:

```powershell
dotnet ef database update 20261001044428_AddInventoryManagement --project .\services\api\RestaurantPOS.api.csproj --startup-project .\services\api\RestaurantPOS.api.csproj --context RestaurantPOS.Infrastructure.Persistence.ApplicationDbContext --no-build
```

EF may apply earlier pending canonical migrations first if the isolated database does not yet contain them. The human must capture the complete non-secret output and report every migration EF applies. No non-canonical migration should be applied.

## 6. Migration Application Result

```text
HOST MIGRATION: NOT RUN
MIGRATION APPLIED: NO
```

## 7. Migration History Validation

After successful application, the human must run the read-only diagnostic and return output proving `20261001044428_AddInventoryManagement` is present exactly once, together with the important preceding canonical migrations.

## 8. Seven-Table Validation

The diagnostic checks for exactly these tables and reports the count:

`InventoryItems`, `BranchInventories`, `StockReceipts`, `StockReceiptItems`, `StockIssues`, `StockIssueItems`, `StockTransactions`.

Expected result: `INVENTORY TABLES: 7/7`.

## 9. Expense Relationship Validation

The diagnostic checks nullable UUID `Expenses.StockReceiptId`, the `StockReceiptId` FK to `StockReceipts.Id`, and the unique non-null index.

## 10. BranchInventory Constraints

The diagnostic checks required columns, unique `(BranchId, InventoryItemId)`, Branch and InventoryItem FKs, and the non-negative CurrentQuantity/MinimumStock checks.

## 11. Precision Validation

It checks:

- quantities and balance snapshots: numeric(18,3);
- receipt total and unit price: numeric(18,2).

## 12. FK / Index Validation

It verifies the generated named FKs and indexes without inspecting business rows. It also confirms that `StockTransactions.ReferenceId` has no polymorphic database FK.

## 13. EmployeeType Preservation

The diagnostic reads only metadata and verifies `Employees.EmployeeType`, `Employees.BasicSalary`, and `Employees.Role` remain present.

## 14. BasicSalary Preservation

`Employees.BasicSalary` must be present. No employee values are selected or printed.

## 15. Payroll Regression Check

The diagnostic checks that `Payrolls`, `PayrollSettings`, `PayrollAdjustments`, and `EmployeeSalaryProfiles` are absent. Historical migration source is not treated as active schema.

## 16. Build/Test

```text
Backend build: PASS
Backend tests: 207/207 PASS
PostMigrationInventoryValidation build: PASS
```

The diagnostic build reports the known Npgsql 8.0.0 NU1903 warning and one nullable warning; Npgsql was not upgraded.

## 17. Database Mutation Scope

Codex performed no database operation. The diagnostic is metadata-only and does not insert InventoryItems, receipts, issues, transactions, Expenses, or seed data.

## 18. Readiness for Phase 2

Phase 2 remains blocked until the human returns:

1. successful host migration output;
2. read-only diagnostic output with migration history and schema results;
3. confirmation that no business data was inserted.

The migration is statically safe for the authorized isolated DEV target, but Task 42 is not complete until those host-side results are supplied.

## Host Commands

First set the expected project reference and run the safe target/connectivity check:

```powershell
$env:DEV_SUPABASE_PROJECT_REF = "qfkgjxwbshjgsxsvkpkp"
dotnet run --project .\scripts\diagnostics\HostSelect1\HostSelect1.csproj --framework net8.0
```

Only if it reports the authorized target and `TARGET CHECK: PASS`, run the explicit migration command above.

After migration succeeds, run the read-only schema validator:

```powershell
dotnet run --project .\scripts\diagnostics\PostMigrationInventoryValidation\PostMigrationInventoryValidation.csproj --framework net8.0
```

## Current Final Status

```text
TASK 42: PARTIAL — PREPARED FOR HUMAN HOST EXECUTION
AUTHORIZED DEV REF: qfkgjxwbshjgsxsvkpkp
TARGET CHECK: PENDING HUMAN HOST RUN
HOST MIGRATION: NOT RUN
MIGRATION APPLIED: NO
MIGRATION HISTORY: PENDING
INVENTORY TABLES: PENDING
EXPENSE.STOCKRECEIPTID: STATIC PASS / DATABASE PENDING
EXPENSE UNIQUE LINK: STATIC PASS / DATABASE PENDING
BRANCHINVENTORY UNIQUE: STATIC PASS / DATABASE PENDING
CURRENTQUANTITY CHECK: STATIC PASS / DATABASE PENDING
MINIMUMSTOCK CHECK: STATIC PASS / DATABASE PENDING
QUANTITY PRECISION: STATIC PASS / DATABASE PENDING
MONEY PRECISION: STATIC PASS / DATABASE PENDING
EMPLOYEETYPE PRESERVED: STATIC YES / DATABASE PENDING
BASICSALARY PRESERVED: STATIC YES / DATABASE PENDING
UNEXPECTED PAYROLL SCHEMA: STATIC NO / DATABASE PENDING
UNEXPECTED SCHEMA CHANGES: STATIC NO / DATABASE PENDING
BUSINESS DATA INSERTED: NO
BUILD: PASS
TESTS: 207/207 PASS
SAFE FOR INVENTORY PHASE 2: NO — awaiting host validation
```

No commit or push was performed.

## CHECK Constraint Diagnostic Correction

### Original validator failure

The applied migration log showed PostgreSQL successfully executing the Inventory CHECK constraints, but the first validator reported the BranchInventory and receipt/issue/transaction checks as failures. No schema correction was authorized or performed.

### Root cause

The validator queried `pg_get_constraintdef(c.oid)` correctly, but then required raw operator/value text such as `">= 0"` and `"> 0"` to appear unchanged. PostgreSQL may normalize those expressions with casts and additional parentheses, for example `>= (0)::numeric`. This caused false negatives even when the named CHECK constraint existed.

### Corrected PostgreSQL metadata query

The validator now uses PostgreSQL-native metadata:

```sql
SELECT t.relname, c.conname, pg_get_constraintdef(c.oid)
FROM pg_constraint c
JOIN pg_class t ON t.oid = c.conrelid
JOIN pg_namespace n ON n.oid = t.relnamespace
WHERE c.contype = 'c'
  AND n.nspname = 'public'
  AND t.relname IN (...)
ORDER BY t.relname, c.conname;
```

Constraint validation now requires the exact expected constraint name on the expected table and `contype = 'c'`. The normalized PostgreSQL definition is printed for review but is not compared by brittle raw-text equality.

### Diagnostic output improvements

The validator now prints all discovered CHECK definitions grouped by table and reports each required constraint independently, including both document-status constraints and all five StockTransaction constraints. It does not print secrets or business rows and remains read-only.

### Local verification

```text
PostMigrationInventoryValidation build: PASS
Backend build: PASS
Backend tests: 207/207 PASS
Database connection by Codex: NO
Database/schema mutation: NO
```

### Human rerun

The human must rerun:

```powershell
$env:DEV_SUPABASE_PROJECT_REF = "qfkgjxwbshjgsxsvkpkp"
dotnet run --project .\scripts\diagnostics\PostMigrationInventoryValidation\PostMigrationInventoryValidation.csproj --framework net8.0
```

Expected result after the corrected metadata matching is `POST-MIGRATION VALIDATION: PASS`, assuming the constraints shown in the migration execution log are present. The human output is still pending.

## Task 42A Status

```text
TASK 42A: PARTIAL — VALIDATOR CORRECTED; HUMAN RERUN PENDING
ROOT CAUSE: brittle raw-text comparison of PostgreSQL-normalized CHECK definitions
DATABASE SCHEMA MODIFIED: NO
MIGRATION CREATED: NO
CHECK CONSTRAINTS DISCOVERED: AWAITING HUMAN RERUN
POST-MIGRATION VALIDATION: AWAITING HUMAN RERUN
SAFE FOR INVENTORY PHASE 2: NO — awaiting human validator output
```
