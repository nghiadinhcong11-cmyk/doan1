# Inventory Phase 1B — Migration Review

## 1. Executive Summary

Exactly one EF Core migration was generated for the Phase 1A Inventory model. The generated Inventory schema is structurally consistent with the approved seven-table design, including quantity/money precision, non-negative checks, branch isolation foreign keys, and the nullable one-to-one Expense link.

The migration is **not safe to apply** because EF also generated an unrelated `DropColumn("EmployeeType", "Employees")`. This is caused by the pre-existing model/snapshot divergence and must be resolved through the repository's migration-history process before DEV application. The migration and snapshot were not manually repaired.

No database connection, database update, API startup, or SQL execution was performed.

## 2. Pre-Generation Safety Check

- Baseline build: PASS.
- Baseline tests: 206/206 PASS.
- Seven Inventory entities present.
- `Expense.StockReceiptId` present.
- No Inventory migration existed before generation.
- No Recipe/BOM, StockAdjustment, Unit table, Product/Inventory FK, or invented `RestaurantId` was added.
- BasicSalary and EmployeeType source changes were not made by this task.
- No database operation was executed.

## 3. Migration ID / Name

Generated migration:

`20261001043012_AddInventoryManagement`

The generated class is `AddInventoryManagement` and its designer contains EF migration metadata: `[Migration("20261001043012_AddInventoryManagement")]`.

## 4. Files Generated

- `services/api/src/Infrastructure/Persistence/Migrations/20261001043012_AddInventoryManagement.cs`
- `services/api/src/Infrastructure/Persistence/Migrations/20261001043012_AddInventoryManagement.Designer.cs`
- `services/api/src/Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs` was updated by EF.

## 5. Tables Created

The `Up()` method creates exactly these seven Inventory tables:

1. `InventoryItems`
2. `BranchInventories`
3. `StockReceipts`
4. `StockReceiptItems`
5. `StockIssues`
6. `StockIssueItems`
7. `StockTransactions`

No Recipe, BOM, StockAdjustment, or Unit table is created.

## 6. Expense Alteration

`Expenses` receives nullable `StockReceiptId` with a foreign key to `StockReceipts.Id`. `StockReceipt` does not receive an `ExpenseId`, so there is one persisted relationship source.

## 7. Columns and Precision Review

- `InventoryItems` has name, normalized name, controlled `UnitCode`, active flag, and UTC timestamps. It has no `MinimumStock`, `BranchId`, `ProductId`, or `RestaurantId`.
- `BranchInventories` owns branch-specific `MinimumStock` and `CurrentQuantity`.
- Quantity fields use `numeric(18,3)`.
- `TotalAmount` and `UnitPrice` use `numeric(18,2)`.
- Receipt and issue lines contain no client-authoritative stock balance fields.
- Stock transactions contain immutable balance snapshots and polymorphic reference fields without a multiple-table foreign key.

## 8. Constraints Review

Generated checks include:

- `CurrentQuantity >= 0`.
- `MinimumStock >= 0`.
- Receipt and issue quantities `> 0`.
- Receipt `UnitPrice >= 0`.
- Receipt `TotalAmount >= 0`.
- Transaction quantity `> 0`.
- Transaction `BeforeQuantity >= 0` and `AfterQuantity >= 0`.
- Approved receipt/issue statuses only.
- Approved transaction types and reference types only.

## 9. Index Review

- Unique `(BranchId, InventoryItemId)` on `BranchInventories`.
- Filtered unique `Expenses.StockReceiptId` index for non-null values.
- Branch/date and branch/idempotency indexes on receipt and issue documents.
- Filtered unique `(BranchId, IdempotencyKey)` indexes for receipts and issues.
- Transaction branch/item/date and reference indexes.
- Active normalized item-name uniqueness is represented in the model snapshot/configuration; no additional raw SQL was introduced.

## 10. Foreign Keys / Delete Behavior

Branch and InventoryItem relationships use `Restrict`. Expense-to-StockReceipt uses `Restrict`. Receipt/issue line ownership also uses `Restrict` rather than cascade; later services must explicitly manage Draft deletion. No cascade path can erase StockTransaction history.

## 11. Unit Validation

The approved codes remain application-controlled: `kg`, `g`, `litre`, `ml`, `piece`, `box`, `bottle`, `pack`.

No database UnitCode check constraint was generated. Status and transaction/reference type checks are database-enforced; unit validation is currently application-only.

## 12. Status Validation

`Draft`, `Confirmed`, and `Cancelled` are stored as bounded strings and have database check constraints on receipts and issues.

## 13. Idempotency Indexes

Nullable idempotency keys use branch-scoped filtered unique indexes. PostgreSQL therefore permits multiple null keys while preventing duplicate non-null keys within each document table and branch.

## 14. Expense One-to-One Enforcement

YES. A unique nullable index on `Expenses.StockReceiptId` permits multiple nulls but permits at most one Expense for each non-null StockReceiptId.

## 15. Destructive Operation Scan

**BLOCKER FOUND.** `Up()` contains:

```csharp
migrationBuilder.DropColumn(
    name: "EmployeeType",
    table: "Employees");
```

This is an unrelated existing-table change and violates the Phase 1B scope. No other unrelated `DropTable`, `DropColumn`, rename, update, delete, or raw SQL operation was found in the generated migration.

## 16. Down() Review

The Inventory reversal order is FK-safe and removes the Expense relationship and seven Inventory tables. However, `Down()` also re-adds `Employees.EmployeeType`, confirming that the generated migration is not safe as a complete migration until the snapshot/model divergence is resolved. It was not manually edited.

## 17. Snapshot Review

The generated snapshot contains the seven Inventory entities and `Expense.StockReceiptId`.

The snapshot also continues to contain unrelated pre-existing working-tree changes. The snapshot was not restored or manually repaired.

## 18. Pre-existing Snapshot Diff Separation

The pre-generation snapshot diff recorded before migration generation included unrelated changes for BusinessInsights, removal of EmployeeSalaryProfile/Payroll model entries, removal of EmployeeType from the snapshot, an Orders analytics index, and ReceiptSettings font fields.

The new Inventory-related snapshot additions are the seven Inventory entities, their relationships/constraints/indexes, and the Expense-to-StockReceipt relationship/index. The generated migration nevertheless interpreted the EmployeeType divergence as a schema operation, producing the blocker described above.

## 19. Migration Discoverability

YES by static metadata inspection. The generated designer has the normal EF `[Migration(...)]` attribute and `BuildTargetModel`. `dotnet ef migrations list` was not run because this review did not require risking configured database access.

## 20. Build/Test Results

- Pre-generation build: PASS.
- Pre-generation tests: 206/206 PASS.
- Post-generation build: PASS, with the repository's existing nullable-reference warnings.
- Post-generation tests: 206/206 PASS.

## 21. Database Connectivity Confirmation

- Migration generation used EF design-time startup only.
- No `dotnet ef database update` was run.
- The API was not started.
- No Supabase or other database connection was intentionally opened.
- No database was mutated.

## 22. Deviations / Risks

The migration must not be applied because of the unrelated `Employees.EmployeeType` drop. Do not manually delete the operation or hand-edit the snapshot. The pre-existing snapshot/model divergence needs a controlled migration-history decision before a safe Inventory migration can be generated or applied.

## 23. Recommendation Before DEV Apply

The original `20261001043012_AddInventoryManagement` migration was removed because it contained an unrelated `Employees.EmployeeType` drop. Do not apply that removed migration. The replacement migration below has passed the destructive-operation scan and remains unapplied pending normal isolated-DEV review.

## EmployeeType Reconciliation

- Root cause: `Employee.cs` did not contain `EmployeeType`, while historical schema evidence and migration `20260907044417_AddEmployeeTypeColumn` define a required `text` column with default `FullTime`.
- Before repair, the current Employee entity and working snapshot omitted the property, so EF interpreted the existing column as removed and generated `DropColumn("EmployeeType", "Employees")`.
- Model fix: restored non-nullable `string EmployeeType` with compatible default `FullTime`; no Payroll behavior was added.
- The old unapplied Inventory migration was removed using `dotnet ef migrations remove --force`. EF could not complete its optional applied-state SSL check, but the task evidence established that the migration had not been applied; no database connection succeeded.
- EF restored the prior snapshot, which contains `EmployeeType`, before the replacement migration was generated.
- Replacement migration: `20261001044428_AddInventoryManagement`.
- The replacement `Up()` and `Down()` contain no EmployeeType, Employees, BasicSalary, or Payroll operations.

## Repaired Final Safety Verdict

```text
PHASE 1B: PASS — CLEAN MIGRATION GENERATED; NOT APPLIED
EMPLOYEETYPE ROOT CAUSE: Employee.cs and the working snapshot omitted a historically required EmployeeType property, so EF detected a drop.
EMPLOYEE MODEL EMPLOYEETYPE: PRESENT
SNAPSHOT EMPLOYEETYPE: PRESENT
OLD INVENTORY MIGRATION: REMOVED
OLD MIGRATION APPLIED: NO
NEW MIGRATION: 20261001044428_AddInventoryManagement
NEW MIGRATION COUNT: 1
INVENTORY TABLES: 7
EXPENSE.STOCKRECEIPTID: PRESENT
EMPLOYEETYPE OPERATION IN NEW MIGRATION: NO
BASICSALARY OPERATION: NO
PAYROLL OPERATION: NO
UNRELATED EXISTING TABLE CHANGES: NO
DESTRUCTIVE UNRELATED OPERATIONS: NO
DOWN() SAFE: YES
BUILD: PASS
TESTS: 207/207 PASS
DATABASE CONNECTION: NO
DATABASE MUTATED: NO
SAFE TO APPLY TO ISOLATED DEV: YES — subject to normal reviewed DEV procedure
APPLIED TO DEV: NO
```

## Final Safety Verdict

```text
PHASE 1B: PARTIAL — GENERATED BUT BLOCKED BY UNRELATED EMPLOYEETYPE DROP
MIGRATION NAME: 20261001043012_AddInventoryManagement
NEW MIGRATION COUNT: 1
EXPECTED: 1
TABLES CREATED: 7
EXPECTED: 7
UNEXPECTED EXISTING TABLE CHANGES: YES
EXPENSE.STOCKRECEIPTID: PRESENT
EXPENSE ONE-TO-ONE DB ENFORCEMENT: YES
BRANCHINVENTORY UNIQUE KEY: YES
QUANTITY PRECISION: decimal(18,3)
MONEY PRECISION: decimal(18,2)
NEGATIVE STOCK CHECK: YES
MINIMUM STOCK CHECK: YES
UNIT DB CHECK: NO — APPLICATION VALIDATION ONLY
STATUS DB CHECK: YES
DESTRUCTIVE OPERATIONS: YES — Employees.EmployeeType drop
UNEXPECTED CASCADE PATH: NO
DOWN MIGRATION SAFE: NO — unrelated EmployeeType re-add
MIGRATION DISCOVERABLE: YES
PRE-EXISTING SNAPSHOT CHANGES PRESERVED: YES
BUILD: PASS
TESTS: 206/206 PASS
DATABASE CONNECTION: NO
DATABASE MUTATED: NO
SAFE TO APPLY TO ISOLATED DEV: NO
APPLIED TO DEV: NO
```
