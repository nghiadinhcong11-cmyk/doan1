# EF Migration Baseline Reconciliation Audit

## Purpose and safety

Task 47H.1A.1 inspected the current source migration baseline before introducing `RestaurantTable.QrToken`. This was a source-only audit. No QrToken field or migration was created, no migration was applied, and no database was contacted.

## Migration inventory

There are 41 source migrations. Each migration except the three older handwritten migrations listed below has a designer. The current snapshot is `ApplicationDbContextModelSnapshot.cs`; its logical position is after `20261001044428_AddInventoryManagement`.

| ID | Migration | Designer |
|---|---|---|
| 20260721094114 | InitialCreate | Yes |
| 20260722105813 | AddBankInfoToBranch | Yes |
| 20260723095409 | AddAttendanceSystem | Yes |
| 20260723100041 | FixWorkScheduleBranchId | Yes |
| 20260723103527 | AddBranchIdToEmployee | Yes |
| 20260723104422 | AddInfoToOrder | Yes |
| 20260723105007 | AddCustomerTable | Yes |
| 20260723145243 | AddBranchNameToSchedule | Yes |
| 20260723150626 | AddCustomerPhoneToOrder | Yes |
| 20260812051057 | AddForeignKeysAndSync | Yes |
| 20260822153125 | AddShiftTable | Yes |
| 20260822153908 | AddReservationTable | Yes |
| 20260822155325 | UpdateShiftDetails | Yes |
| 20260822162819 | AddCustomerPassword | Yes |
| 20260825144406 | UpdateCustomerProfile | Yes |
| 20260825162216 | AddCustomerEmailToOrder | Yes |
| 20260825165006 | AddProductOptionsSupport | Yes |
| 20260829062053 | AddToppingTable | Yes |
| 20260829063304 | UpdateToppingSchema | Yes |
| 20260829142528 | AddToppingSupportToOrderDetails | Yes |
| 20260829152705 | AddPromotionTable | Yes |
| 20260902072328 | AddImageUrlToBranch | Yes |
| 20260902133018 | AddKitchenRequests | Yes |
| 20260905050736 | AddExpenseBranchAndAudit | Yes |
| 20260906120000 | AddReceiptSettings | No â€” handwritten |
| 20260906121500 | AddPaymentAtToOrder | No â€” handwritten |
| 20260906143000 | Addremoved staff-finance module | No â€” handwritten |
| 20260907044417 | AddEmployeeTypeColumn | Yes |
| 20260907060113 | AddPaymentAtToOrderCorrective | Yes |
| 20260907060600 | AddMissingremoved staff-finance modulesAndSettings | Yes |
| 20260907063921 | Corrective_FixMissingColumns | Yes |
| 20260907155909 | RepairMissingDatabaseSchema | Yes |
| 20260907164814 | AddSystemSettings | Yes |
| 20260908032924 | AddOrderFinancialSnapshot | Yes |
| 20260908041144 | AddBranchStoreProfile | Yes |
| 20260908043852 | AddLoyaltyFoundation | Yes |
| 20260908045205 | AddLoyaltyIdempotencyIndex | Yes |
| 20260908094237 | AddRoleToEmployee | Yes |
| 20260908100201 | BackfillEmployeeRoles | Yes |
| 20260910052615 | AddAnalyticsIndexes | Yes |
| 20260910063419 | AddBusinessInsight | Yes |
| 20260919075618 | Removeremoved staff-finance moduleAndRepairReceiptSettings | Yes |
| 20261001044428 | AddInventoryManagement | Yes |

The dates embedded in migration names are identifiers, not evidence that an arbitrary database has applied them. Applied-state verification is deliberately outside this audit.

## Three-state comparison

| Area | Migration chain | Current snapshot | Current entity/DbContext | Classification |
|---|---|---|---|---|
| Expense `PaymentMethod` | `InitialCreate` creates `NOT NULL`; no later Expense alteration found | nullable | Non-null CLR default and explicit `IsRequired()` | STALE SNAPSHOT |
| Expense `Note` | `InitialCreate` creates `NOT NULL`; no later Expense alteration found | nullable | Non-null CLR default and explicit `IsRequired()` | STALE SNAPSHOT |
| Inventory | `AddInventoryManagement` creates seven inventory tables, Expense FK/index, checks and indexes | all seven entities, FK/index and restrictions present | same entities/DbSets/configuration | no pending difference |
| removed staff-finance module subsystem | historical removed staff-finance module migrations create tables; `Removeremoved staff-finance moduleAndRepairReceiptSettings` drops them | no removed staff-finance module model entities | no removed staff-finance module entities or DbSets | intentional removed subsystem |
| Employee `BasicSalary`, `EmployeeType` | initial salary column; `AddEmployeeTypeColumn` adds retained type | both present | both present | retained employee profile data |
| Analytics | `AddAnalyticsIndexes` adds Orders index; `AddBusinessInsight` creates table/indexes | index and `BusinessInsight` present | `BusinessInsights` DbSet and matching indexes | no pending difference |
| Orders | historical/current model uses `OrderDetails` | `OrderDetails` table present | `DbSet<OrderDetail> OrderDetails` | no pending difference |
| RestaurantTable | `QrCodeUrl` only | `QrCodeUrl`, no token | `QrCodeUrl`, no token | QrToken intentionally not started |

## Exact EF diagnostic

A disposable migration scaffold was generated into an isolated non-canonical folder, inspected, and deleted without being applied. It contained exactly two `Up` operations:

1. `AlterColumn` `Expenses.PaymentMethod` from nullable `text` to required `text`, with an EF-generated empty-string default.
2. `AlterColumn` `Expenses.Note` from nullable `text` to required `text`, with an EF-generated empty-string default.

It contained no Inventory, removed staff-finance module, BusinessInsight, Orders/OrderDetails, Restaurant, or analytics operations. The warning about possible data loss was caused by the two nullable-to-required alterations.

The operation is not a legitimate new database change: the initial migration already declares both columns `NOT NULL`, and the authorized Inventory PostgreSQL E2E evidence independently recorded the same runtime constraint. The snapshot lost the required metadata while the runtime model was corrected for the receipt Expense contract.

## Area findings

### Inventory

| Difference | Cause | Classification | Required action |
|---|---|---|---|
| None proposed by EF | Snapshot, `AddInventoryManagement`, and current `ConfigureInventory` agree on the inventory model | N/A | Do not regenerate or alter Inventory migration |

### removed staff-finance module

| Difference | Cause | Classification | Required action |
|---|---|---|---|
| No EF operation | removed staff-finance module entities/DbSets are absent after the removal migration | INTENTIONAL LEGACY DB SCHEMA | Do not restore removed staff-finance module source; later deployment verification must confirm the removal migration state |
| `BasicSalary` and `EmployeeType` remain on Employee | They are retained employee-profile fields, not the removed removed staff-finance module subsystem | N/A | Preserve them |

### Analytics

| Difference | Cause | Classification | Required action |
|---|---|---|---|
| None proposed by EF | `BusinessInsight` and the Orders analytics index appear in migration, snapshot, and current DbContext | N/A | No change |

### Expense contract

| Difference | Cause | Classification | Required action |
|---|---|---|---|
| Snapshot marks `PaymentMethod` nullable | Snapshot was not reconciled after current required model/runtime contract was established | STALE SNAPSHOT | Source-only snapshot repair |
| Snapshot marks `Note` nullable | Same defect | STALE SNAPSHOT | Source-only snapshot repair |

## Canonical baseline decision

The intended canonical state before QR work is:

* Current `ApplicationDbContext` and the `Expense` entity are authoritative for the required Expense persistence contract.
* The reviewed migration chain and PostgreSQL runtime evidence confirm `Expenses.PaymentMethod` and `Expenses.Note` are `NOT NULL`.
* The current snapshot must be repaired only to represent that already-existing contract; it must not generate a new schema migration.
* Current Inventory, analytics, `OrderDetails`, retained `EmployeeType`/`BasicSalary`, and removed staff-finance module removal remain unchanged.

## Reconciliation plan (not executed)

1. **SOURCE ONLY** â€” Update the two `Expense` properties in `ApplicationDbContextModelSnapshot.cs` to `IsRequired()`.
2. **SOURCE ONLY** â€” Run EF pending-model diagnostic again. It must report no pending model changes before QrToken work.
3. **DB VERIFICATION REQUIRED** â€” On the authorized DEV host, confirm the migration history includes the reviewed chain and that `Expenses.PaymentMethod`/`Note` remain `NOT NULL`. Do not use a cleanup migration to perform this confirmation.
4. **DB MUTATION REQUIRED only when separately approved** â€” Apply any existing reviewed migrations only according to the established DEV migration process; this audit does not authorize it.
5. **MIGRATION REQUIRED (future QR work)** â€” Only after steps 1â€“3 are clean, begin the QrToken foundation in a dedicated migration sequence.

## Future QrToken migration/backfill strategy

Do not use PostgreSQL `random()`, MD5, table/branch-derived values, timestamp values, a shared default, or undocumented extensions.

The recommended safe sequence is two migrations plus a controlled application-side backfill tool:

1. **Migration A** â€” Add nullable `QrToken` with a unique filtered index for non-null values. New table creation generates tokens with `.NET RandomNumberGenerator`, 32 random bytes, Base64Url encoding.
2. **Controlled one-time backfill** â€” An explicit, authenticated maintenance command/tool reads only null-token tables and writes independently generated tokens. It handles the unique-index collision exception with a small bounded retry. It must be run against the approved target only, report counts without tokens, and never log token values.
3. **Migration B** â€” After a read-only verification proves no null/empty values and no duplicates, make `QrToken` `NOT NULL` and replace the filtered index with a final unique index if needed.

This avoids relying on unknown PostgreSQL extension availability while preserving cryptographic randomness. Rollback before Migration B can remove the nullable column only if no dependent QR bootstrap is deployed; after issuance, token revocation/rotation must be treated as an operational policy rather than erased silently.

## Outcome

The migration baseline is not yet repaired, but the only source-level EF drift is now identified and bounded to the two Expense snapshot metadata entries. The next task should reconcile that snapshot and prove EF reports no pending model changes before QrToken implementation begins.

## Task 47H.1A.2 â€” completed source-only reconciliation

The canonical snapshot was repaired only at the two stale Expense properties:

* `Expenses.PaymentMethod` is marked required.
* `Expenses.Note` is marked required.

This is a snapshot correction, not a fabricated database migration. The initial migration already creates both columns `NOT NULL`, no subsequent source migration changes that contract, current EF configuration requires them, and prior authorized PostgreSQL runtime evidence confirms it.

An isolated EF diagnostic migration was scaffolded after the repair. Its `Up` and `Down` methods were empty, proving that EF proposed zero operations for Inventory, removed staff-finance module, analytics, Orders, Expense, RestaurantTable, or any other area. The diagnostic migration/designer files were removed. The canonical migration count remains 41 and no database was contacted.

The full backend suite passed: **254/254**. The backend also built successfully using a temporary isolated output path because the active local API held its standard binaries open. Temporary MSBuild files and output directories were removed after validation.

The EF migration baseline is now ready for the separate QrToken foundation work.

## Intentional QR schema work after reconciliation

Task 47H.1A.3 began intentional schema work only after the zero-operation baseline check. Migration `20261001094205_AddRestaurantTableQrToken` contains only the nullable `Tables.QrToken` column and its filtered unique index; it does not alter the reconciled Expense contract or any unrelated model area. It remains unapplied pending human migration review.

### Migration inventory count note

During the Task 47H.1A.3 final source enumeration, the migrations directory contained 44 existing migration classes before the new QrToken migration and 45 afterward, rather than the previously documented 41/42. The three-count discrepancy predates `AddRestaurantTableQrToken`; that migration adds one class only and its generated operations are clean. No existing migration was removed or rewritten. Human migration review must reconcile the documented count with the actual source inventory before any new migration is applied.

## Task 47H.1A.3R â€” migration-count reconciliation (read-only)

### Result: CASE B â€” EF DISCOVERY PROBLEM

The previous `41` exactly matches both the pre-QR `.Designer.cs` count and EF Core's recognized migration count. It does **not** match the raw migration-class count. The earlier inventory is internally inconsistent: it calls the result `41 source migrations`, includes the three handwritten files, and has 43 rows because it omits generated migration `20260906040844_AddKitchenHistoryAndNotifications`. It records no raw-file counting command. The source-supported explanation is therefore that the count used the generated/designer (equivalently, EF-discoverable) population and was incorrectly documented as a raw source count.

Raw inventory: 45 current migration classes, 44 before QrToken; 42 current designer files, 41 before QrToken. `ApplicationDbContextModelSnapshot.cs` and designer partials are not migrations. The latest raw class is `20261001094205_AddRestaurantTableQrToken`, with both source and designer files.

| Raw class | Timestamp | Designer | EF discoverable | Classification | Why omitted from old 41 |
|---|---|---:|---:|---|---|
| AddReceiptSettings | 20260906120000 | No | No | NOT AN EF MIGRATION / orphaned handwritten class | No generated partial supplies `[DbContext]` and `[Migration]` metadata |
| AddPaymentAtToOrder | 20260906121500 | No | No | NOT AN EF MIGRATION / orphaned handwritten class | No generated partial supplies `[DbContext]` and `[Migration]` metadata |
| Addremoved staff-finance module | 20260906143000 | No | No | NOT AN EF MIGRATION / orphaned handwritten class | No generated partial supplies `[DbContext]` and `[Migration]` metadata |

Each listed file has the expected `RestaurantPOS.api.src.Infrastructure.Persistence.Migrations` namespace and derives from `Migration`, but contains no `[DbContext]`/`[Migration]` attributes and has no designer partial where generated migrations declare them. EF consequently excludes all three.

Source inspection confirms their historical operations are represented in later model metadata but not repaired as EF history: `AddPaymentAtToOrderCorrective` has an empty `Up` while its generated model contains `PaymentAt`; `AddEmployeeTypeColumn` overlaps the handwritten removed staff-finance module migration's `EmployeeType` column; and `Removeremoved staff-finance moduleAndRepairReceiptSettings` removes removed staff-finance module tables and creates `ReceiptSettings`. The current snapshot contains `PaymentAt`, `ReceiptSettings`, retained `EmployeeType`, no removed staff-finance module tables, and QrToken. Removing or renaming the files would not repair the discovery defect and would erase forensic evidence.

`dotnet ef migrations list --no-connect --json --context ApplicationDbContext` was run after local compilation. It explicitly prohibited a database connection and returned `applied: null` for all recognized migrations:

* EF-recognized pre-QR count: **41**.
* EF-recognized current count: **42**.
* `20261001094205_AddRestaurantTableQrToken` is recognized and latest.
* No duplicate raw migration IDs or ordering/timestamp conflicts were found; raw filenames are chronological.

The three handwritten operations create overlap/conflict risk if treated as an independently executable raw chain. Do not apply QrToken Migration A. A dedicated migration-history repair proposal is required. No migration source, snapshot, executable source, database, or production system was changed; no migration was applied.

## Task 47H.1A.3R.1 â€” source-only repair

### Operation and coverage matrix

| Handwritten migration | Operation | Object | Intended effect | Discoverable coverage | Class |
|---|---|---|---|---|---|
| AddReceiptSettings | CreateTable + PK | `ReceiptSettings` | Original receipt settings columns | `Removeremoved staff-finance moduleAndRepairReceiptSettings` creates the final table/PK | A â€” fully superseded |
| AddReceiptSettings | CreateIndex unique | `ReceiptSettings.BranchId` | One settings row per branch | `Removeremoved staff-finance moduleAndRepairReceiptSettings` creates the final unique index | D â€” still required and covered |
| AddReceiptSettings.Down | DropTable | `ReceiptSettings` | Roll back original table | Not relevant to forward chain | C â€” current model no longer needs it |
| AddPaymentAtToOrder | AddColumn | `Orders.PaymentAt` nullable timestamptz | Payment timestamp | `RepairMissingDatabaseSchema` adds it with `IF NOT EXISTS` | D â€” still required and covered |
| AddPaymentAtToOrder.Down | DropColumn | `Orders.PaymentAt` | Roll back timestamp | Not relevant to forward chain | C â€” current model no longer needs it |
| Addremoved staff-finance module | AddColumn | `Employees.EmployeeType` | Employee category | `AddEmployeeTypeColumn`, reinforced by repair migration | D â€” still required and covered |
| Addremoved staff-finance module | CreateTable | `EmployeeSalaryProfiles` | removed staff-finance module salary profile | `RepairMissingDatabaseSchema` creates it; removal migration drops it | B â€” superseded then removed |
| Addremoved staff-finance module | CreateTable | `removed staff-finance moduleSettings` | removed staff-finance module settings | `RepairMissingDatabaseSchema` creates it; removal migration drops it | B â€” superseded then removed |
| Addremoved staff-finance module | CreateTable | `removed staff-finance modules` | removed staff-finance module records | `RepairMissingDatabaseSchema` creates it; removal migration drops it | B â€” superseded then removed |
| Addremoved staff-finance module | CreateTable | `removed staff-finance moduleAdjustments` | removed staff-finance module adjustments | `RepairMissingDatabaseSchema` creates it; removal migration drops it | B â€” superseded then removed |
| Addremoved staff-finance module | CreateIndex | Four removed staff-finance module indexes | Query/uniqueness indexes | `RepairMissingDatabaseSchema` creates all four; removal drops their tables | B â€” superseded then removed |
| Addremoved staff-finance module.Down | DropTables + DropColumn | removed staff-finance module tables, `Employees.EmployeeType` | Roll back handwritten schema | Not relevant to forward chain | C â€” current model no longer needs it |

`BasicSalary` is not a handwritten operation: `InitialCreate` creates it and the current `Employee` model retains it. `EmployeeType` remains employee profile data, not removed staff-finance module functionality.

### Fresh-chain analysis and repair

The unexecuted, no-connect command `dotnet ef migrations script --idempotent --no-build --context ApplicationDbContext` was inspected. An empty PostgreSQL database using only discoverable migrations receives `EmployeeType` from `AddEmployeeTypeColumn`; `PaymentAt` and temporary removed staff-finance module tables from `RepairMissingDatabaseSchema`; final `ReceiptSettings` and removed staff-finance module removal from `Removeremoved staff-finance moduleAndRepairReceiptSettings`; and `QrToken` plus its filtered unique index from Migration A. The snapshot contains `PaymentAt`, final receipt settings, retained `BasicSalary`/`EmployeeType`, no removed staff-finance module subsystem, and QrToken.

`AddPaymentAtToOrderCorrective`, `AddMissingremoved staff-finance modulesAndSettings`, and `Corrective_FixMissingColumns` have empty `Up`/`Down`; `RepairMissingDatabaseSchema` is the discoverable forward repair. No handwritten class is required for a fresh database to reach the current intended model.

**STRATEGY 1 â€” ARCHIVE NON-EF HISTORICAL FILES** was safe and performed. The files were moved unchanged from the canonical compiled directory to `bao-cao/99-luu-tru/migrations/`:

* `20260906120000_AddReceiptSettings.cs`
* `20260906121500_AddPaymentAtToOrder.cs`
* `20260906143000_Addremoved staff-finance module.cs`

The accompanying archive README prohibits restoring them to the canonical migration directory. No migration was deleted or converted. Post-repair canonical raw count is **42** and EF-recognized count is **42**; QrToken remains recognized and latest. `dotnet ef migrations has-pending-model-changes --no-build --context ApplicationDbContext` reported no model changes since the last migration. No database connection, SQL execution, migration application, or production action occurred.

## Task 47H.1A.4 â€” Migration A runtime evidence

After the source-history repair, isolated DEV history was verified as 41 canonical predecessor migrations with no unknown IDs. Explicit EF application of `20261001094205_AddRestaurantTableQrToken` then succeeded. DEV history is now 42 migrations with Migration A latest exactly once. Direct PostgreSQL verification confirmed nullable `Tables.QrToken varchar(43)` with no default and unique filtered index `IX_Tables_QrToken` (`"QrToken" IS NOT NULL`). The seven pre-existing table rows remain null-tokened; no backfill was performed. The source model remains clean.


