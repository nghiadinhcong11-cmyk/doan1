# EF Core Migration Tree Audit

Audit date: 2026-09-30  
Scope: read-only inspection of the current working tree  
Source of truth: `ApplicationDbContext`, `Program.cs`, project file, migration classes and designer metadata.

## 1. Executive conclusion

The repository currently contains two migration directories, but there is only one runtime `ApplicationDbContext`:

```text
RestaurantPOS.Infrastructure.Persistence.ApplicationDbContext
```

`Program.cs` registers that context with `UseNpgsql(...)` and does not configure `MigrationsAssembly(...)`. The project is SDK-style and has no compile exclusion for either migration directory. Therefore, in the current working tree, both directories are compiled into the same API assembly. `Database.Migrate()` does not select a tree by folder name; it discovers migrations associated with the context in the assembly.

This produces a combined and unsafe local migration set rather than two intentional runtime histories.

The second tree is not tracked by Git in the current repository state. It exists as an untracked working-tree addition, so a clean checkout would not contain it unless it is later added. That makes the current local runtime behavior different from the tracked repository baseline.

The migration chain is not safe to use for a new Supabase development database yet. The tree ownership, migration discovery, missing designer metadata, model drift, and destructive Payroll/ReceiptSettings migration behavior require human review before any database is created or migrated.

## 2. Inventory

### DbContext

| Item | Finding | Evidence |
|---|---|---|
| Runtime context | `ApplicationDbContext` | `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` |
| Namespace | `RestaurantPOS.Infrastructure.Persistence` | same file |
| Provider | PostgreSQL through Npgsql | `services/api/Program.cs` |
| Migration assembly override | Not configured | `services/api/Program.cs` only calls `UseNpgsql(...)` |
| Project inclusion | No `Compile Remove` or migration include rule | `services/api/RestaurantPOS.api.csproj` |

### Tree 1 — primary `src` tree

| Field | Value |
|---|---|
| Path | `services/api/src/Infrastructure/Persistence/Migrations/` |
| Namespace | `RestaurantPOS.api.src.Infrastructure.Persistence.Migrations` |
| DbContext metadata | `DbContext(typeof(ApplicationDbContext))`; the imported context is `RestaurantPOS.Infrastructure.Persistence.ApplicationDbContext` |
| Model snapshot | `ApplicationDbContextModelSnapshot.cs` |
| First migration | `20260721094114_InitialCreate` |
| Latest migration class | `20260910052615_AddAnalyticsIndexes` |
| Migration class files | 40 |
| Designer files | 38 |
| Model snapshot files | 1 actual EF model snapshot |
| Git status | Existing tracked tree, with current working-tree modifications |

The two migration classes without a matching designer file are:

- `20260906120000_AddReceiptSettings`
- `20260906121500_AddPaymentAtToOrder`
- `20260906143000_AddPayroll`

The count is 40 migration class files, not 40 reliably discoverable migrations. The generated `MigrationAttribute` is present in the designer partial files; the three manual classes above do not contain a `MigrationAttribute` themselves. They must not be assumed to participate in EF migration discovery without a controlled design-time/runtime verification.

### Tree 2 — `services/api/Infrastructure` tree

| Field | Value |
|---|---|
| Path | `services/api/Infrastructure/Persistence/Migrations/` |
| Namespace | `RestaurantPOS.api.Infrastructure.Persistence.Migrations` |
| DbContext metadata | `DbContext(typeof(ApplicationDbContext))` |
| Model snapshot | None |
| First migration | `20260910063419_AddBusinessInsight` |
| Latest migration | `20260919075618_RemovePayrollAndRepairReceiptSettings` |
| Migration class files | 2 |
| Designer files | 2 |
| Model snapshot files | 0 |
| Git status | Untracked; `git ls-files services/api/Infrastructure` returns no files |

Both Tree 2 migrations have generated `MigrationAttribute` metadata in their designer files and are therefore discoverable candidates when the files are present in the compiled working tree.

## 3. Runtime ownership

The relevant runtime sequence is:

```text
Program.cs
  -> AddDbContext<ApplicationDbContext>
  -> UseNpgsql(connectionString)
  -> app startup scope
  -> db.Database.Migrate()
```

There is no `MigrationsAssembly(...)` call and no explicit migration namespace/path selection. EF Core uses the assembly containing the context by default. Because the SDK-style project compiles C# files recursively, both migration directories are in that assembly when Tree 2 exists in the working tree.

Therefore:

- Runtime does not use only `src/Infrastructure/Persistence/Migrations` by folder convention.
- Runtime does not use only `Infrastructure/Persistence/Migrations` either.
- Current working-tree runtime ownership is **combined assembly discovery**.
- A clean checkout that does not contain the untracked Tree 2 would have a different migration set.

This is the primary reason the current migration state must be resolved before runtime E2E or a new Supabase database.

## 4. Chronological comparison

### Tree 1 sequence

Tree 1 begins with the initial schema on 2026-07-21 and continues through branch, attendance, scheduling, customer authentication/profile, product options, toppings, promotions, kitchen requests/history, expenses, payment/receipt-related changes, Payroll-related history, system settings, loyalty, roles and analytics indexes.

Its latest named migration is:

```text
20260910052615_AddAnalyticsIndexes
```

The Tree 1 model snapshot has already been modified to contain later/current-looking concepts such as `BusinessInsight`, even though the corresponding `AddBusinessInsight` migration is in Tree 2. This means the snapshot is not a reliable proof that Tree 1 alone contains the complete chronological history.

### Tree 2 sequence

```text
20260910063419_AddBusinessInsight
20260919075618_RemovePayrollAndRepairReceiptSettings
```

Tree 2 continues chronologically after Tree 1's latest migration ID. It is not an independent old baseline: its migrations reference the same `ApplicationDbContext`, the same tables, and the same schema concepts.

### Duplicate and conflict assessment

| Topic | Finding |
|---|---|
| Duplicate migration IDs | No identical migration IDs found between the two trees |
| Duplicate schema responsibility | Yes; Tree 1 snapshot already includes BusinessInsight while Tree 2 owns its creation migration |
| Independent DbContexts | No evidence; both designer sets point to the same `ApplicationDbContext` |
| Independent runtime histories | No; no separate assembly/provider configuration exists |
| Obsolete/legacy tree | Tree 2 is at least an untracked working-tree residue; intentionality is unproven |
| Conflicting snapshots | Tree 1 has the only actual snapshot; Tree 2 has no snapshot and uses migration designer target models |
| Safe to merge now | No |

## 5. Current model versus migration metadata

### Current model entities

The current `ApplicationDbContext` contains:

```text
Product
Order
OrderDetail
Expense
Employee
RestaurantTable
Area
Branch
Attendance
WorkSchedule
Customer
Shift
Reservation
Topping
Promotion
OrderRequest
OrderRequestItem
Notification
ReceiptSettings
SystemSetting
LoyaltyTransaction
BusinessInsight
```

There are no current `DbSet` declarations for `Payroll`, `PayrollAdjustment`, `PayrollSettings`, or `EmployeeSalaryProfile`.

### Important drift found

The current `Employee` entity contains no `BasicSalary` and no `EmployeeType`, but both the Tree 1 model snapshot and Tree 2 latest designer target model still contain those employee properties. The current `Order` entity contains `PaymentAt`, while the migration history contains both a manual `AddPaymentAtToOrder` class and a later empty corrective migration; this requires migration-history verification rather than assumptions.

The latest Tree 2 designer target model is closer to the current domain than the older Payroll model because it no longer declares Payroll entity types, but it is still not an exact current-model snapshot due to stale employee properties and other history artifacts.

### Module comparison

| Area | Current model | Tree 1 snapshot | Tree 2 latest target model | Assessment |
|---|---|---|---|---|
| Restaurant/Branch/Table/Area | Present | Present | Present | broadly aligned |
| Order/OrderDetail | Present | Present | Present | broadly aligned; verify PaymentAt history |
| OrderRequest/Item | Present | Present | Present | aligned |
| Customer | Present | Present | Present | aligned at entity level |
| Product/options/toppings | Present | Present | Present | aligned at entity level |
| Kitchen/notifications | Present | Present | Present | aligned at entity level |
| ReceiptSettings | Present | Present | Present | migration ordering risk exists |
| BusinessInsight | Present | Present in snapshot | Present | migration ownership split |
| Employee.BasicSalary | Absent | Present | Present | stale migration metadata |
| Employee.EmployeeType | Absent | Present | Present | stale migration metadata |
| Payroll entity types | Absent | Absent from snapshot | Absent from latest target | historical migration only in final target |

## 6. Payroll history

Payroll is historical migration content, not current EF domain model content.

Historical Payroll operations appear in:

- `20260906143000_AddPayroll.cs`
- `20260907060600_AddMissingPayrollsAndSettings`
- `20260907155909_RepairMissingDatabaseSchema`
- `20260919075618_RemovePayrollAndRepairReceiptSettings`
- related designer target models

The current model has no Payroll DbSets or Payroll entity declarations. This is consistent with Payroll being outside the current system scope.

### Expected final-schema intent

If the complete chronological chain were valid and all required operations were applied, the intended final state of the latest Tree 2 migration is:

- Payroll tables dropped by `RemovePayrollAndRepairReceiptSettings`;
- Payroll entity types absent from the final target model;
- ReceiptSettings recreated with the latest shape;
- historical Payroll migration records retained in `__EFMigrationsHistory`;
- possible stale `Employees.EmployeeType`/`Employees.BasicSalary` columns unless an explicit later drop exists.

This is an intended migration-code reading, not a statement about any actual database. No database was inspected.

## 7. Clean-database simulation from migration code

This is static analysis only.

### Main risks

1. **Migration discovery gap:** three manual Tree 1 migration classes have no matching designer file and no local `MigrationAttribute`. They cannot be treated as reliably discovered migrations without controlled EF design-time verification.
2. **Payroll reconstruction by repair migration:** `RepairMissingDatabaseSchema` uses PostgreSQL `IF NOT EXISTS` SQL to recreate Payroll tables and related indexes, even though Payroll is no longer in current scope. A clean chain can therefore create Payroll temporarily before the later removal migration.
3. **Destructive Payroll removal:** Tree 2 uses direct `DropTable` operations without `IF EXISTS`. If the preceding migration history did not create those tables, the clean chain can fail.
4. **ReceiptSettings duplication risk:** Tree 2's latest migration creates `ReceiptSettings`. If the manual Tree 1 `AddReceiptSettings` is included by a future discovery change, the table may be created twice.
5. **Stale model metadata:** snapshots/target models contain properties no longer present in the current entity model.
6. **Raw SQL assumptions:** migrations directly reference PostgreSQL objects, columns, tables, constraints and `pg_constraint`; this is provider-specific and assumes a particular prior schema state.
7. **Data-dependent backfill:** `AddExpenseBranchAndAudit` updates existing expense rows by selecting a main/first branch before making `BranchId` non-null. This is not a pure empty-schema operation and must be reviewed for existing databases.

### Expected clean database result

The final schema cannot be declared `SAFE` from source inspection alone. The result depends on which migrations EF actually discovers, especially the manual classes and the untracked Tree 2.

The safest conclusion is:

```text
Clean database migration: RISK / NOT APPROVED
```

No migration was executed.

## 8. Existing database risk

EF Core records applied migrations in:

```text
__EFMigrationsHistory
```

This table is the database's record of which migration IDs were applied. EF compares that history with the migrations discovered from the compiled assembly. It does not safely infer that two migrations with similar schema effects are interchangeable.

Changing, deleting, renaming or replacing migration files can cause:

- EF to attempt operations already applied;
- EF to skip operations that were never applied;
- migration ID/history mismatch;
- destructive rollback or schema drift;
- inability to upgrade an existing Supabase database;
- clean databases and existing databases ending with different schemas.

Because the current database was not inspected, no conclusion can be made about which migration IDs are already present or whether Payroll/ReceiptSettings tables exist there.

## 9. Tree classification

| Tree | Classification | Evidence | Decision |
|---|---|---|---|
| `services/api/src/Infrastructure/Persistence/Migrations` | ACTIVE, but inconsistent | tracked primary tree, snapshot, long chronological history, referenced by current project layout | Do not delete or rewrite without database history review |
| `services/api/Infrastructure/Persistence/Migrations` | UNCERTAIN / untracked residue | same DbContext, generated designers, newer IDs, no snapshot, absent from `git ls-files` | Do not add to a shared/production migration path until provenance and database history are confirmed |

Tree 2 cannot be classified as a safe intentional second runtime history. It may represent a later migration patch generated in a different folder, but source evidence does not prove that it was deliberately configured as a separate assembly.

## 10. Cleanup options

### Option A — Keep Tree 1 as active and archive/remove Tree 2 from active source

**Benefit:** one visible history and one snapshot owner.  
**Risk:** unsafe if Tree 2 migrations were already applied to an existing database.  
**Existing DB effect:** requires checking `__EFMigrationsHistory` first; may require preserving equivalent migration IDs.  
**Clean DB effect:** could omit BusinessInsight/Payroll-removal changes unless they are represented safely in the active chain.  
**Thesis/project effect:** clearer architecture after reconciliation.

### Option B — Keep both trees intentionally

**Benefit:** preserves all current migration IDs.  
**Risk:** still one assembly, no separate runtime boundary, no Tree 2 snapshot, and local-vs-clean-checkout behavior differs.  
**Existing DB effect:** combined ordering must match applied history.  
**Clean DB effect:** remains exposed to missing designer/duplicate-operation risks.  
**Thesis/project effect:** difficult to explain and maintain.

### Option C — Re-baseline for a new development/test database

**Benefit:** can produce a clean, current-schema baseline without replaying historical Payroll repair operations.  
**Risk:** must not replace the production/history chain; requires an explicitly isolated development path and review of all current tables/constraints.  
**Existing DB effect:** none if isolated.  
**Clean DB effect:** potentially safest for a disposable development database after approval.  
**Thesis/project effect:** document as a development-only baseline, not the production migration history.

### Option D — Explicitly configure a single migration assembly/path

**Benefit:** removes accidental folder-based discovery ambiguity.  
**Risk:** changing configuration alone does not reconcile missing migration IDs or existing database history.  
**Existing DB effect:** requires history compatibility review.  
**Clean DB effect:** deterministic only after the selected tree is internally valid.  
**Thesis/project effect:** improves architecture documentation.

## 11. Recommendation

### SAFE NOW

- Do not run API startup against a new or existing Supabase database.
- Preserve both trees until database history and provenance are reviewed.
- Treat `src/Infrastructure/Persistence/Migrations` as the primary historical tree for investigation.
- Treat `services/api/Infrastructure/Persistence/Migrations` as uncertain untracked material, not an approved active migration source.

### REQUIRES DATABASE HISTORY CHECK

- Any decision to archive/remove Tree 2.
- Any attempt to run migrations against an existing Supabase database.
- Any migration re-baseline or clean development baseline.
- Any decision about whether the three manual migrations were ever applied.

### DO NOT DO

- Do not delete or rename migration IDs.
- Do not edit `__EFMigrationsHistory` manually.
- Do not modify already-applied migrations.
- Do not run `Database.Migrate()` on the current remote target until it is proven isolated and disposable.
- Do not create the development database using the current ambiguous migration set.

The safest project approach is:

1. Confirm the provenance of Tree 2 and whether its migration IDs exist in any database history.
2. Select one authoritative migration assembly/path.
3. Reconcile the selected chain and current model through a separate approved migration task.
4. For a disposable development database, use a separately documented baseline only after the selected schema is reviewed.
5. Keep production/existing-database history immutable.

## 12. Future command plan — not executed

These are planning commands only. They were not run during this audit and must target an explicitly isolated database/configuration.

```powershell
# Inspect discovered migrations after tree ownership is approved.
dotnet ef migrations list `
  --project .\services\api\RestaurantPOS.api.csproj

# Generate a SQL review artifact without applying it.
dotnet ef migrations script `
  --project .\services\api\RestaurantPOS.api.csproj `
  --output .\artifacts\migration-review.sql
```

Before either command, human approval is required for the selected tree, migration assembly, and target environment. `dotnet ef database update` is intentionally not part of this audit plan.

## 13. Final status

```text
DBCONTEXT:
ApplicationDbContext

MIGRATION TREE COUNT:
2

TREE 1:
ACTIVE (primary tracked tree, but internally inconsistent)

TREE 2:
UNCERTAIN (untracked working-tree tree; not proven intentional)

RUNTIME TREE:
Combined migration types from the API assembly in the current working tree; no explicit MigrationsAssembly. Clean checkout may differ because Tree 2 is untracked.

CURRENT MODEL MATCH:
Tree 2 latest target model is closer in Payroll entity removal, but neither tree is an exact current-model match. Tree 1 snapshot and Tree 2 target both retain stale Employee.BasicSalary/EmployeeType metadata.

CLEAN DATABASE MIGRATION:
RISK

PAYROLL IN FINAL SCHEMA:
NO by intended latest migration target, but UNKNOWN for any actual database and unsafe to assert without execution/history inspection.

EXISTING DATABASE RISK:
HIGH

MIGRATION CLEANUP REQUIRED BEFORE DEV DB:
YES — HUMAN REVIEW

RECOMMENDED ACTION:
Confirm migration provenance/history, select one authoritative assembly, reconcile the chain/current model in a separate approved task, then use an isolated development baseline.

SAFE TO CREATE DEV SUPABASE NOW:
NO
```

## 14. Audit boundaries

- No API was started.
- No database connection was made.
- No migration was listed through EF tooling or applied.
- No migration/database update was run.
- No migration, DbContext, configuration or source file was modified.
- No migration was deleted, moved or renamed.
- No commit or push was performed.
