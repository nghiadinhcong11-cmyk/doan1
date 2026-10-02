# Inventory Phase 1A — Domain Model and EF Configuration

Date: 2026-10-01  
Scope: domain entities, centralized domain validation/constants, EF `DbSet` registration and `OnModelCreating` configuration only.

No controller, application service, DTO, UI, migration, database connection, database update, API startup, SignalR event or AI tool was added.

## 1. Executive Summary

Phase 1A is complete at the source/model level. The approved Inventory model is now represented by exactly seven new domain entities/tables, plus a nullable `Expense.StockReceiptId` relationship:

1. `InventoryItems`
2. `BranchInventories`
3. `StockReceipts`
4. `StockReceiptItems`
5. `StockIssues`
6. `StockIssueItems`
7. `StockTransactions`

`InventoryItem` is independent from `Product`. Stock is branch-scoped through `BranchInventory`; `MinimumStock` is correctly stored on `BranchInventory`, not the master item. No Product sales behavior, Recipe/BOM, automatic deduction, Payroll or Order/OrderDetails schema was changed.

The EF model is intentionally ahead of the existing migration snapshot. Phase 1B must generate and review one new migration from the canonical migration tree. No migration was generated in Phase 1A.

## 2. Files Added

### Domain

- `services/api/src/Domain/Entities/InventoryItem.cs`
- `services/api/src/Domain/Entities/BranchInventory.cs`
- `services/api/src/Domain/Entities/StockReceipt.cs`
- `services/api/src/Domain/Entities/StockReceiptItem.cs`
- `services/api/src/Domain/Entities/StockIssue.cs`
- `services/api/src/Domain/Entities/StockIssueItem.cs`
- `services/api/src/Domain/Entities/StockTransaction.cs`
- `services/api/src/Domain/Inventory/InventoryUnitCatalog.cs`
- `services/api/src/Domain/Inventory/InventoryEnums.cs`

### Tests

- `tests/RestaurantPOS.Tests/InventoryDomainValidationTests.cs`

## 3. Files Modified

- `services/api/src/Domain/Entities/Expense.cs`
  - Added nullable `StockReceiptId` and one navigation to `StockReceipt`.
- `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs`
  - Added seven Inventory `DbSet` properties.
  - Added Inventory and Expense-link EF configuration in `OnModelCreating`.

No DTOs were added because Phase 1A is explicitly domain/EF-only and controllers/services do not exist yet. DTOs belong to Phase 2 when the API behavior is implemented.

## 4. Final Entity Model

### InventoryItem

`Id`, `Name`, `NormalizedName`, `UnitCode`, `IsActive`, `CreatedAtUtc`, `UpdatedAtUtc?`.

It has no `MinimumStock`, `CurrentQuantity`, `BranchId`, `ProductId` or `RestaurantId`. `NormalizedName` supports duplicate protection using trimmed/collapsed whitespace and uppercase normalization. The unique active-item index is configured on `NormalizedName` with a PostgreSQL filtered-index expression; Phase 1B must review the generated SQL.

### BranchInventory

`Id`, `BranchId`, `InventoryItemId`, `CurrentQuantity`, `MinimumStock`, `UpdatedAtUtc`.

The balance and reorder threshold are branch-specific. `(BranchId, InventoryItemId)` is unique. Both quantities use `decimal(18,3)` and have non-negative check constraints.

### StockReceipt and StockReceiptItem

`StockReceipt` contains branch, supplier, optional purchase date, total amount, enum status, note, creator/confirmation identity and nullable idempotency key.

`StockReceiptItem` contains receipt/item IDs, `Quantity` and `UnitPrice`. No `TotalPrice` or client-supplied unit snapshot is persisted. Future confirmation recalculates line and receipt totals from authoritative values.

### StockIssue and StockIssueItem

`StockIssue` contains branch, reason, note, enum status, creator/confirmation identity and nullable idempotency key.

`StockIssueItem` contains issue/item IDs and quantity. Unit is derived from `InventoryItem`; it is not an authoritative line input.

### StockTransaction

`Id`, `BranchId`, `InventoryItemId`, `Type`, `Quantity`, `BeforeQuantity`, `AfterQuantity`, `ReferenceType`, `ReferenceId`, `CreatedBy`, `CreatedAtUtc`, `Note`.

`StockTransactionType` reserves `IN`, `OUT` and `ADJUSTMENT`. `ADJUSTMENT` has no workflow or table in Phase 1A.

## 5. EF Relationships

- `BranchInventory -> Branch`: required, `Restrict` delete.
- `BranchInventory -> InventoryItem`: required, `Restrict` delete.
- `StockReceipt -> Branch`: required, `Restrict` delete.
- `StockReceiptItem -> StockReceipt`: required, `Restrict` delete.
- `StockReceiptItem -> InventoryItem`: required, `Restrict` delete.
- `StockIssue -> Branch`: required, `Restrict` delete.
- `StockIssueItem -> StockIssue`: required, `Restrict` delete.
- `StockIssueItem -> InventoryItem`: required, `Restrict` delete.
- `StockTransaction -> Branch`: required, `Restrict` delete.
- `StockTransaction -> InventoryItem`: required, `Restrict` delete.
- `Expense.StockReceiptId -> StockReceipt.Id`: optional one-to-one, `Restrict` delete.

Child rows use `Restrict` rather than database cascade. This preserves confirmed-document history; a future service may explicitly delete Draft children before deleting a Draft document. Confirmed documents must eventually be immutable.

`CreatedBy` and `ConfirmedBy` are Guid identity snapshots without invented User/Employee foreign keys. The current system has Employee identity claims but no single safe User entity relationship shared by all authenticated identities. Phase 2 must derive these values from JWT claims, never client input.

`StockTransaction.ReferenceType + ReferenceId` is a controlled polymorphic reference. It intentionally has no multi-table foreign key because PostgreSQL cannot enforce one FK against both receipts and issues. The centralized values are `StockReceipt`, `StockIssue` and reserved `StockAdjustment`; service validation and future tests must preserve integrity.

## 6. Constraints and Index Intent

Configured in `ApplicationDbContext.OnModelCreating`:

- unique active `InventoryItem.NormalizedName`;
- unique `(BranchId, InventoryItemId)` for `BranchInventories`;
- `CurrentQuantity >= 0`;
- `MinimumStock >= 0`;
- receipt `TotalAmount >= 0`;
- receipt/issue statuses limited to `Draft`, `Confirmed`, `Cancelled`;
- receipt quantity `> 0`;
- receipt unit price `>= 0`;
- issue quantity `> 0`;
- transaction quantity `> 0`;
- transaction before/after quantities `>= 0`;
- transaction type limited to `IN`, `OUT`, `ADJUSTMENT`;
- transaction reference type limited to the centralized reference values;
- branch/date, document/idempotency, transaction history and reference indexes;
- unique filtered Expense index on non-null `StockReceiptId`.

PostgreSQL-specific filtered-index expressions and check-constraint SQL must be reviewed in the Phase 1B generated migration. They were not executed or applied here.

## 7. Unit Design

`InventoryUnitCatalog` is the single centralized definition for:

`kg`, `g`, `litre`, `ml`, `piece`, `box`, `bottle`, `pack`.

`IsValid` and `IsDiscrete` provide pure validation helpers. No Unit table was introduced and no arbitrary unit is accepted by the domain helper. Receipt/issue lines do not own a unit; they derive it from the InventoryItem master.

## 8. Quantity/Money Precision

- Quantity, current balance, minimum stock, before and after snapshots: `decimal(18,3)`.
- Unit price and receipt total amount: `decimal(18,2)`.
- No floating-point monetary or quantity field was added.
- Discrete units require integral quantities through `InventoryQuantityRules`; continuous units accept decimal quantities.
- Non-negative balance validation is represented by pure helper coverage and EF check constraints. Phase 2 must repeat validation at the service boundary.

## 9. Expense Relationship

`Expense.StockReceiptId` is the only persisted link. No `StockReceipt.ExpenseId` was added. EF config maps an optional one-to-one relationship with a unique filtered index for non-null values and restricted deletion.

No Expense is created automatically in Phase 1A. Future receipt confirmation must create at most one Expense in the same transaction as stock application, using the approved `Nhập hàng` category and receipt confirmation/purchase date rules.

## 10. Idempotency Decision

The current source has no general idempotency convention. Nullable `IdempotencyKey` fields were added only to `StockReceipt` and `StockIssue`, with intended unique `(BranchId, IdempotencyKey)` filtered indexes. This is deliberately conservative and not a general idempotency subsystem.

Phase 2 must combine the key with Draft/Confirmed state and a transaction. A repeated confirmation must not apply stock, ledger or Expense twice.

## 11. CreatedBy/ConfirmedBy Identity Decision

The fields are required/optional Guid snapshots, matching current `Expense.CreatedBy` style without adding a new User entity or unsafe FK. Phase 2 must populate them from authenticated JWT identity claims. Client requests must not control them.

## 12. Validation Added

Pure domain helpers now cover:

- approved units;
- arbitrary unit rejection;
- discrete-unit detection;
- continuous decimal quantities;
- integral discrete quantities;
- non-positive and negative quantity rejection;
- non-negative balance values;
- duplicate-name normalization;
- Draft default status for receipt and issue.

These helpers do not replace Phase 2 backend authorization/confirmation validation or PostgreSQL concurrency controls.

## 13. Tests Added

`InventoryDomainValidationTests` adds 20 unit cases covering unit allowlisting, discrete detection, quantity rules, name normalization and default status. They use no database and do not claim to prove PostgreSQL row locking or `FOR UPDATE` behavior.

## 14. Build/Test Results

```text
BUILD: PASS
TESTS: 206/206 PASS
```

The build retains existing nullable warnings in unrelated/current source files. No new migration or database command was run.

## 15. Migration Status

```text
MIGRATION CREATED: NO
MIGRATION COMMANDS RUN: NO
MODEL SNAPSHOT MODIFIED: NO
CANONICAL MIGRATION TREE MODIFIED: NO
DATABASE CONNECTION: NO
DATABASE MUTATED: NO
```

The EF model and existing snapshot intentionally differ until Phase 1B generates a reviewed migration. `ApplicationDbContextModelSnapshot.cs` was not manually edited.

## 16. Deviations From Audit 39

- `MinimumStock` was moved from the Audit 39 `InventoryItem` candidate to `BranchInventory`, per the approved branch-specific decision.
- No `Unit` table was added; the approved controlled string catalog is used.
- No `TotalPrice` or `UnitCodeSnapshot` was added to receipt lines; totals are future server-derived values and the canonical item unit remains the source.
- No DTOs were added because the requested phase is domain/EF configuration only and there are no Inventory controllers/services to consume them yet.
- Child relationships use `Restrict` rather than cascade to protect confirmed history; future Draft deletion must be explicit in a service.
- Stock transaction enum values are persisted as stable uppercase strings `IN`, `OUT`, `ADJUSTMENT`.

## 17. Risks / Open Technical Questions

- The active filtered unique index for normalized names depends on PostgreSQL migration generation and must be reviewed in Phase 1B.
- PostgreSQL check constraints cannot enforce discrete-unit integrality without joining InventoryItems; Phase 2 must validate unit/quantity in the service transaction.
- Polymorphic `ReferenceId` has no database FK; service validation and immutable ledger tests are required.
- No optimistic concurrency token was added in Phase 1A. Phase 2 must use PostgreSQL row locking in deterministic item order.
- The existing Expense controller category allowlist does not yet include `Nhập hàng`; updating that behavior belongs to Expense/receipt integration, not Phase 1A.
- EF enum/status values and existing migration naming should be reviewed before migration generation.

## 18. Phase 1B Readiness

The model is ready for a human-reviewed Phase 1B migration design/generation step, subject to:

1. review the generated PostgreSQL SQL for seven tables and the Expense column/index;
2. verify filtered indexes and check constraints;
3. verify no Payroll/Order/Product/Recipe tables are changed;
4. verify the snapshot is generated by EF tooling, not edited manually;
5. generate exactly one new migration from the canonical migration tree;
6. review and test it only against the authorized isolated DEV database under the existing safety procedure.

## Final Phase 1A Status

```text
PHASE 1A: PASS
INVENTORY ENTITIES: 7
EXPECTED: 7
MINIMUM STOCK LOCATION: BranchInventory
INVENTORYITEM BRANCH-SCOPED: NO (shared master)
BRANCHINVENTORY BRANCH-SCOPED: YES
PRODUCT LINK ADDED: NO
RECIPE/BOM ADDED: NO
STOCK ADJUSTMENT TABLE ADDED: NO
EXPENSE.STOCKRECEIPTID: ADDED
EXPENSE LINK UNIQUE INTENT: YES
UNIT DESIGN: controlled UnitCode string/catalog; no Unit table or conversion
QUANTITY PRECISION: decimal(18,3)
MONEY PRECISION: decimal(18,2)
MIGRATION CREATED: NO
MODEL SNAPSHOT MODIFIED: NO
DATABASE CONNECTION: NO
DATABASE MUTATED: NO
BUILD: PASS
TESTS: 206/206 PASS
SAFE FOR PHASE 1B MIGRATION REVIEW: YES
UNRELATED EXISTING WORKTREE CHANGES PRESERVED: YES
```

No commit or push was performed.
