# EF Core Migration Reconciliation — Phase A

Audit date: 2026-09-30  
Mode: Phase A decision gate only  
Database access: none  
Source changes: none

## 0. Updated domain scope clarification

The current system scope excludes the Payroll module and payroll functionality. This means:

- no payroll calculation;
- no payroll records/API/page/navigation;
- no allowances, overtime calculation, deductions, net-salary calculation or payroll reports;
- historical Payroll migrations remain historical database history and must not be rewritten only to remove their names.

`Employee.BasicSalary` is intentionally retained as employee-management/profile data for future extensibility. Its presence does not mean Payroll is implemented. No migration should drop `BasicSalary`.

`EmployeeType` is audited separately. Source/UI/API inspection found no non-migration usage of `EmployeeType`. It appears in historical Payroll-related migration metadata and old schema repair operations, but there is not enough current domain evidence to decide whether it is an independent employee classification such as full-time, part-time or intern. Therefore it is **HUMAN DECISION REQUIRED** and must not be dropped in this task.

## 1. Decision gate

The requested reconciliation cannot safely proceed to Phase B without database-history evidence.

Reasons:

1. Tree A is the only tracked migration history, but the working tree contains an untracked Tree B that is compiled by the SDK-style project when present.
2. `__EFMigrationsHistory` was not inspected, by design. Therefore it is unknown whether Tree B migration IDs, backup migration IDs, or equivalent schema operations were applied to the current Supabase database.
3. The current model snapshot is already modified in the working tree and differs from `HEAD`; it is not possible to use the committed snapshot alone as the current intended model.
4. Three Tree A migration classes have no designer file and no local `MigrationAttribute`, so their EF discoverability cannot be treated as equivalent to the generated migrations without a controlled tooling check.
5. Tree B contains destructive Payroll drops and a ReceiptSettings recreation. Moving, removing or reordering those operations without database history could break an existing database.

Decision:

```text
RECONCILIATION: HUMAN REVIEW REQUIRED
PHASE B: NOT EXECUTED
```

## 2. Git provenance

### Tree A

Path:

```text
services/api/src/Infrastructure/Persistence/Migrations/
```

Evidence:

- `git ls-files` reports 78 tracked files in this directory, including the migration classes, designer files and `ApplicationDbContextModelSnapshot.cs`.
- Git history shows the tree in the initial project history and in the security baseline commit `8b1012ec`.
- The latest tracked migration in the path is `20260910052615_AddAnalyticsIndexes`.
- The current working tree has a pre-existing modification to `ApplicationDbContextModelSnapshot.cs` and untracked `20260910052615_AddAnalyticsIndexes.cs/.Designer.cs`; those changes were not created by this audit.

### Tree B

Path:

```text
services/api/Infrastructure/Persistence/Migrations/
```

Evidence:

- `git ls-files services/api/Infrastructure/Persistence/Migrations` returns no files.
- `git log --all` shows no commit containing this path.
- Both Tree B migrations are untracked working-tree files.
- No Git evidence proves whether Tree B was generated from a valid branch, copied from a backup, or created as a local corrective attempt.

Tree B origin is therefore:

```text
UNKNOWN — no Git provenance available
```

### Additional migration-like backup

The working tree also contains:

```text
services/Migrations_backup_20260919/
```

It is untracked, outside the API project directory, and is not part of the normal SDK compile path. It contains another Payroll-removal migration (`20260919072925_RemovePayrollFeature`) and `20260913103915_HardenEmployeeLifecycle`. It is historical/local backup material, not an approved active migration location. Its presence reinforces that Tree B provenance cannot be inferred from filenames alone.

## 3. Canonical history identification

Tree A has the strongest canonical evidence:

- tracked in Git;
- long chronological chain beginning with `20260721094114_InitialCreate`;
- one model snapshot;
- consistent project location under `src/Infrastructure/Persistence`;
- generated designer files for most migrations;
- same `ApplicationDbContext` as the runtime registration.

Tree A is therefore the **canonical historical tree for investigation**.

This does not mean Tree A is internally reconciled with the current model. Its snapshot has pre-existing working-tree edits that add `BusinessInsight`, remove Payroll entity metadata and add analytics indexes, while Tree B contains the actual `AddBusinessInsight` migration and a later Payroll/ReceiptSettings migration.

Tree B must not be made canonical merely because its IDs are newer or its latest target model is closer to the current domain.

## 4. EF discoverability inventory

Project facts:

- `services/api/RestaurantPOS.api.csproj` is SDK-style and has no compile exclusion for either migration directory.
- `Program.cs` calls `UseNpgsql(...)` without `MigrationsAssembly(...)`.
- The runtime context is `RestaurantPOS.Infrastructure.Persistence.ApplicationDbContext`.
- Generated designer files carry `DbContext(typeof(ApplicationDbContext))` and `Migration("...")` metadata.
- The three manual Tree A migration classes have no matching designer and no local `MigrationAttribute`.

### Tree A migration inventory

The following classes are tracked in Tree A. `Designer=Yes` means a matching generated designer file exists. `Attribute=Yes` means the migration ID is present in that designer metadata. `Expected discoverable=Yes*` means discoverable through the generated designer metadata; the class source itself does not contain the attribute.

| Migration ID | Tracked | Designer | Attribute | Expected discoverable | Purpose |
|---|---:|---:|---:|---:|---|
| 20260721094114_InitialCreate | Yes | Yes | Yes* | Yes* | Initial schema |
| 20260722105813_AddBankInfoToBranch | Yes | Yes | Yes* | Yes* | Branch bank fields |
| 20260723095409_AddAttendanceSystem | Yes | Yes | Yes* | Yes* | Attendance schema |
| 20260723100041_FixWorkScheduleBranchId | Yes | Yes | Yes* | Yes* | Schedule branch fix |
| 20260723103527_AddBranchIdToEmployee | Yes | Yes | Yes* | Yes* | Employee branch |
| 20260723104422_AddInfoToOrder | Yes | Yes | Yes* | Yes* | Order information |
| 20260723105007_AddCustomerTable | Yes | Yes | Yes* | Yes* | Customer table |
| 20260723145243_AddBranchNameToSchedule | Yes | Yes | Yes* | Yes* | Schedule branch name |
| 20260723150626_AddCustomerPhoneToOrder | Yes | Yes | Yes* | Yes* | Order customer phone |
| 20260812051057_AddForeignKeysAndSync | Yes | Yes | Yes* | Yes* | Foreign keys/sync |
| 20260822153125_AddShiftTable | Yes | Yes | Yes* | Yes* | Shift table |
| 20260822153908_AddReservationTable | Yes | Yes | Yes* | Yes* | Reservation table |
| 20260822155325_UpdateShiftDetails | Yes | Yes | Yes* | Yes* | Shift details |
| 20260822162819_AddCustomerPassword | Yes | Yes | Yes* | Yes* | Customer password field |
| 20260825144406_UpdateCustomerProfile | Yes | Yes | Yes* | Yes* | Customer profile |
| 20260825162216_AddCustomerEmailToOrder | Yes | Yes | Yes* | Yes* | Order customer email |
| 20260825165006_AddProductOptionsSupport | Yes | Yes | Yes* | Yes* | Product options |
| 20260829062053_AddToppingTable | Yes | Yes | Yes* | Yes* | Topping table |
| 20260829063304_UpdateToppingSchema | Yes | Yes | Yes* | Yes* | Topping schema |
| 20260829142528_AddToppingSupportToOrderDetails | Yes | Yes | Yes* | Yes* | Order topping support |
| 20260829152705_AddPromotionTable | Yes | Yes | Yes* | Yes* | Promotion table |
| 20260902072328_AddImageUrlToBranch | Yes | Yes | Yes* | Yes* | Branch image |
| 20260902133018_AddKitchenRequests | Yes | Yes | Yes* | Yes* | Kitchen request tables |
| 20260905050736_AddExpenseBranchAndAudit | Yes | Yes | Yes* | Yes* | Expense branch/audit |
| 20260906040844_AddKitchenHistoryAndNotifications | Yes | Yes | Yes* | Yes* | Kitchen history/notifications |
| 20260906120000_AddReceiptSettings | Yes | No | No | Uncertain | Initial ReceiptSettings |
| 20260906121500_AddPaymentAtToOrder | Yes | No | No | Uncertain | Payment timestamp |
| 20260906143000_AddPayroll | Yes | No | No | Uncertain | Historical Payroll schema |
| 20260907044417_AddEmployeeTypeColumn | Yes | Yes | Yes* | Yes* | Employee type |
| 20260907060113_AddPaymentAtToOrderCorrective | Yes | Yes | Yes* | Yes* | Empty corrective migration |
| 20260907060600_AddMissingPayrollsAndSettings | Yes | Yes | Yes* | Yes* | Payroll repair |
| 20260907063921_Corrective_FixMissingColumns | Yes | Yes | Yes* | Yes* | Corrective schema |
| 20260907155909_RepairMissingDatabaseSchema | Yes | Yes | Yes* | Yes* | Raw SQL schema repair |
| 20260907164814_AddSystemSettings | Yes | Yes | Yes* | Yes* | System settings |
| 20260908032924_AddOrderFinancialSnapshot | Yes | Yes | Yes* | Yes* | Order financial fields |
| 20260908041144_AddBranchStoreProfile | Yes | Yes | Yes* | Yes* | Branch profile |
| 20260908043852_AddLoyaltyFoundation | Yes | Yes | Yes* | Yes* | Loyalty schema |
| 20260908045205_AddLoyaltyIdempotencyIndex | Yes | Yes | Yes* | Yes* | Loyalty index |
| 20260908094237_AddRoleToEmployee | Yes | Yes | Yes* | Yes* | Employee role |
| 20260908100201_BackfillEmployeeRoles | Yes | Yes | Yes* | Yes* | Role backfill |
| 20260910052615_AddAnalyticsIndexes | Yes | Yes | Yes* | Yes* | Analytics indexes |

The three `Uncertain` entries are the manual classes without designer metadata. They exist in the tracked tree, but their actual runtime/design-time discovery must be verified in a controlled environment before any cleanup decision.

### Tree B inventory

| Migration ID | Path | Tracked | Designer | MigrationAttribute | Expected discoverable | Purpose |
|---|---|---:|---:|---:|---:|---|
| 20260910063419_AddBusinessInsight | Tree B | No | Yes | Yes | Yes in current working tree | Creates BusinessInsights and indexes |
| 20260919075618_RemovePayrollAndRepairReceiptSettings | Tree B | No | Yes | Yes | Yes in current working tree | Drops Payroll tables and recreates ReceiptSettings shape |

## 5. Tree B analysis

### `20260910063419_AddBusinessInsight`

This migration creates the `BusinessInsights` table and indexes. The current `ApplicationDbContext` contains `DbSet<BusinessInsight>` and matching Fluent API indexes. The current working-tree Tree A snapshot also contains the BusinessInsight model, but Tree A has no tracked migration with this ID. This is a split ownership problem: the model snapshot edit and the migration implementation are in different working-tree locations.

Required future action is not to copy the folder blindly. The BusinessInsight schema intent must be represented by one approved migration in the canonical history, with a reviewed snapshot and stable migration ID.

### `20260919075618_RemovePayrollAndRepairReceiptSettings`

The migration:

- drops `EmployeeSalaryProfiles`;
- drops `PayrollAdjustments`;
- drops `PayrollSettings`;
- drops `Payrolls`;
- creates `ReceiptSettings`;
- creates a unique `BranchId` index;
- adds `FontFamily` and `FontSize` in the generated model target;
- has a `Down` path that recreates Payroll tables.

It is not a simple current-model migration. It combines two independent concerns: Payroll removal and ReceiptSettings repair. Its direct `DropTable` operations are not guarded by `IF EXISTS`, while earlier raw SQL repair migrations use `IF NOT EXISTS` and can create Payroll tables. This combination requires existing-history verification before it can be retained or moved.

## 6. Payroll reconciliation

### Historical creation/repair

Payroll-related schema is introduced or reconstructed by:

- `20260906143000_AddPayroll` — manual SQL creation of Payroll tables;
- `20260907060600_AddMissingPayrollsAndSettings` — Payroll repair history;
- `20260907155909_RepairMissingDatabaseSchema` — raw SQL `IF NOT EXISTS` creation of Payroll tables, indexes and FK;
- related designer target models.

### Historical removal

There are two untracked removal variants in the working tree:

- Tree B: `20260919075618_RemovePayrollAndRepairReceiptSettings`;
- backup: `20260919072925_RemovePayrollFeature`.

The Git history does not prove which one, if either, was applied to the current Supabase database.

### Current model

Current `ApplicationDbContext` has no Payroll DbSets and no current Payroll entity types. The current `Employee` entity does not declare `BasicSalary` or `EmployeeType`, but the domain decision is that `BasicSalary` is intentionally retained employee-management data. Therefore the current model is missing an intended domain field; this is not evidence that the database column is stale and is not a reason to drop it.

```text
Payroll module/functionality: absent and out of scope
Payroll historical migration content: present and must remain immutable
Employee.BasicSalary: intentionally retained; current model/API mapping requires separate review
EmployeeType: no current non-migration usage found; human domain decision required
```

The final intended schema should not contain Payroll tables. That intended state cannot safely be enforced on an unknown existing database by deleting or rewriting historical migrations.

## 7. ReceiptSettings reconciliation

ReceiptSettings operations found:

1. `20260906120000_AddReceiptSettings.cs` creates the table, but has no designer file or local migration attribute.
2. Tree B `20260919075618_RemovePayrollAndRepairReceiptSettings` creates `ReceiptSettings` again and adds the latest fields/index shape.
3. Backup `20260913103915_HardenEmployeeLifecycle` adds `FontFamily` and `FontSize` to the existing table.

This creates two possible interpretations:

- If the manual initial ReceiptSettings class is not discovered, Tree B's create may be the first discovered create.
- If the manual class is discovered through some external/custom mechanism, Tree B's create is a duplicate and can fail on a clean database.

Because no EF discovery command or database was used, the exact applied path remains unverified. ReceiptSettings requires a single canonical create and a separate additive repair migration if the existing table needs new columns.

## 8. Current model drift

| Area | Current model | Canonical tracked snapshot / history | Classification |
|---|---|---|---|
| BusinessInsight | Entity, DbSet and indexes present | Snapshot modified to include it; create migration only in untracked Tree B | MISSING FROM TRACKED CANONICAL HISTORY |
| Payroll entity types | Absent | Removed from modified snapshot; historical migrations remain | EXPECTED HISTORICAL |
| Payroll tables | Not represented by current model | Created/repaired historically; removal variants untracked | FINAL DATABASE STATE UNKNOWN |
| Employee.BasicSalary | Absent from current entity, but intentionally required by domain scope | Present in snapshot and Tree B target | CURRENT MODEL MISSING INTENDED DOMAIN FIELD; do not drop |
| Employee.EmployeeType | Absent from current entity; no non-migration source/UI/API usage found | Present in snapshot and historical Payroll-related repair migrations | HUMAN DECISION REQUIRED; do not drop in this task |
| Order.PaymentAt | Present in current entity | Historical manual migration plus corrective history | PARTIAL / DISCOVERABILITY UNCERTAIN |
| ReceiptSettings | Present with current fields | Initial manual create plus later repair variants | PARTIAL / ORDERING UNCERTAIN |
| Analytics indexes | Present in current Fluent API | Snapshot modified and tracked migration exists | REPRESENTED, BUT SNAPSHOT HAS PRE-EXISTING CHANGES |

The current model is therefore:

```text
CURRENT MODEL REPRESENTED: PARTIAL
Employee.BasicSalary: INTENDED DOMAIN FIELD MISSING FROM CURRENT ENTITY MODEL
EmployeeType: HUMAN DECISION REQUIRED; not safe to classify as Payroll-only from current source evidence
```

## 9. Actions performed

Only read-only inspection was performed:

- `git status --short`;
- `git ls-files` for both migration trees;
- `git log --all` for migration paths and relevant commits;
- inspection of project file, `Program.cs`, DbContext, snapshots, migration classes and designers;
- static comparison of current model and migration metadata.

No migration reconciliation was applied.

## 10. Files changed, deleted or created

By this task:

- Created: `docs/system-audit/32-EF-MIGRATION-RECONCILIATION.md`.
- Changed migration files: none.
- Deleted migration files: none.
- Moved/renamed migration files: none.
- DbContext/configuration changes: none.

The working tree already contained extensive unrelated/pre-existing changes. Those changes were preserved.

## 11. Offline script findings

No EF tooling command was run. This avoids any possibility of runtime configuration resolving the current connection string.

An offline SQL script was therefore **not generated**. A future `dotnet ef migrations script` review may be used only after a human approves the migration assembly and confirms that design-time tooling does not open a database connection in this project configuration.

The static source review already identifies these script risks:

- duplicate/conditional ReceiptSettings creation;
- unguarded Payroll drops;
- Payroll recreation through raw SQL repair migration;
- stale employee columns;
- provider-specific SQL and existing-data assumptions.

## 12. Required human review before Phase B

1. Inspect the current Supabase `__EFMigrationsHistory` through an approved, read-only database process outside this task.
2. Confirm whether the current database contains any of the following migration IDs: Tree B IDs, backup removal ID, manual ReceiptSettings/PaymentAt/Payroll IDs, or their equivalent schema changes.
3. Decide whether Tree B represents an applied historical migration, an untracked local attempt, or a discarded design.
4. Decide whether BusinessInsight must be preserved as a new canonical migration ID or already exists in an approved database history.
5. Confirm whether `EmployeeType` is an independent employee classification. Keep `BasicSalary`; do not create a drop migration. If `BasicSalary` must be restored to the current EF model/API mapping, handle that as a separate approved domain-model/API task.
6. Approve one canonical active migration location and an explicit migration assembly configuration if needed.

## 13. Future Phase B strategy after approval

The preferred strategy is:

```text
Keep canonical tracked history
  + preserve every migration ID that may already be applied
  + add one reviewed reconciliation migration for current-model drift
  + make the active migration location explicit
  + keep historical Payroll migrations immutable
```

For a new disposable development database, a separate reviewed baseline may be safer than replaying repair-era Payroll migrations, but it must be clearly marked development-only and must not replace the existing database history.

No migration should be deleted, renamed, moved or rewritten until the database-history check is complete.

## PHASE B — CANONICALIZATION RESULT

Phase B was performed using the verified read-only Supabase history supplied in the task. No database connection was made and no migration was executed.

### 1. Files inspected

- `services/api/src/Infrastructure/Persistence/Migrations/`
- `services/api/Infrastructure/Persistence/Migrations/`
- `services/Migrations_backup_20260919/`
- `services/api/src/Domain/Entities/Employee.cs`
- `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs`
- `services/api/src/Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs`
- Employee controller/service/UI consumers
- Git status, tracked-file inventory and Git history

### 2. Canonicalization performed

The two Tree B migration IDs confirmed as applied were moved into the canonical Tree A directory without changing their migration IDs or migration operations:

- `20260910063419_AddBusinessInsight`
- `20260919075618_RemovePayrollAndRepairReceiptSettings`

Their namespaces were normalized to the existing canonical migration namespace:

```text
RestaurantPOS.api.src.Infrastructure.Persistence.Migrations
```

The untracked `20260910052615_AddAnalyticsIndexes` source already existed under Tree A and was retained there. It was not recreated or assigned a replacement ID.

Tree B was removed from the API source tree after the four files were moved. There is now one active migration location in the source layout.

### 3. Applied migration reconciliation

The supplied database evidence confirms that the canonical source now contains all later applied IDs that were previously only in the untracked Tree B/local Tree A material:

```text
20260910052615_AddAnalyticsIndexes
20260910063419_AddBusinessInsight
20260919075618_RemovePayrollAndRepairReceiptSettings
```

Historical IDs were not renamed, replaced, squashed or rewritten. The future commit that makes this state shareable must include these currently untracked canonical files; otherwise a clean Git checkout will still omit them.

### 4. BusinessInsight

`20260910063419_AddBusinessInsight` is now located under the canonical Tree A path with its original generated designer and `Migration` attribute. `BusinessInsights` remains represented by the current `DbContext` and snapshot. No replacement migration and no duplicate `CreateTable` operation were created.

### 5. Payroll history

`20260919075618_RemovePayrollAndRepairReceiptSettings` is now part of the canonical source history with its original ID and operations. Historical Payroll creation/repair migrations remain untouched. Current Payroll functionality and current Payroll model types remain absent. The supplied database evidence confirms Payroll is absent in the current database.

This preserves the historical chain:

```text
historical Payroll create/repair migrations
  → 20260919075618_RemovePayrollAndRepairReceiptSettings
  → intended Payroll absence
```

No Payroll feature was reintroduced.

### 6. ReceiptSettings

The applied removal/repair migration was preserved unchanged in operation semantics. It remains a known clean-database risk because its `Up` path creates `ReceiptSettings` while the older manual `20260906120000_AddReceiptSettings` class has no generated designer and is not in the supplied applied-history list. No historical migration was rewritten and no duplicate corrective migration was created.

The backup-only `20260913103915_HardenEmployeeLifecycle` contains a separate ReceiptSettings additive change and remains outside the active API migration path pending provenance review.

### 7. BasicSalary reconciliation

`Employee.BasicSalary` was restored to the current `Employee` entity as a non-nullable `decimal`, matching the existing database/snapshot intent (`numeric NOT NULL`). This is a model/property restoration only:

- no Payroll calculation was added;
- no Payroll API/service/UI was added;
- no `AddColumn` migration was created;
- no database was changed.

The existing employee controller binds the entity directly, so the restored property is available to the existing employee-management contract without introducing a new endpoint or business rule. The frontend currently does not expose a salary calculation workflow; UI follow-up is not part of this reconciliation.

### 8. EmployeeType decision

`EmployeeType` is retained in the database/history intent and is not dropped. Current non-migration source usage was not found, so it remains a future-domain-review field rather than an active Payroll feature. No model drop and no migration were created.

### 9. Missing migration metadata

The three manual Tree A classes remain source history without generated designers:

| Migration ID | In supplied applied list | Action |
|---|---:|---|
| `20260906120000_AddReceiptSettings` | No | Preserved as historical/manual source; not promoted into active generated discovery |
| `20260906121500_AddPaymentAtToOrder` | No | Preserved as historical/manual source; no ID or operation rewrite |
| `20260906143000_AddPayroll` | No | Preserved as historical Payroll source; no ID or operation rewrite |

No metadata was fabricated for these classes because doing so could activate duplicate ReceiptSettings creation or unverified Payroll operations on a clean database.

### 10. Backup tree

`services/Migrations_backup_20260919/` remains outside the API compile path and was not deleted. Most files duplicate canonical historical migration names. Its unique files are:

- `20260913103915_HardenEmployeeLifecycle`
- `20260919072925_RemovePayrollFeature`

These remain historical/local backup evidence only. They were not promoted because their provenance and application status were not established by Git or the supplied database list.

### 11. Model versus snapshot

| Area | Result |
|---|---|
| BasicSalary | Restored to current Employee model; aligns with snapshot/database intent |
| EmployeeType | Intentionally retained database/history field; current model does not use it; no drop permitted |
| Payroll entities | Absent from current model and intended final model |
| BusinessInsight | Current model and canonical migration represented |
| ReceiptSettings | Current model represented; historical clean-chain ordering remains risky |
| Applied later migration IDs | Present in canonical source location |

Overall current-model representation is **PARTIAL** because EmployeeType remains database/history-only and the snapshot contains historical compatibility metadata that is not an active domain feature.

### 12. Clean database risk

Canonical source ownership is now deterministic at the folder level, but a clean database replay is still **STILL RISKY** without an offline migration-script review or isolated test database. Remaining risks include:

- manual migrations without generated metadata;
- direct Payroll drops in the final removal migration;
- ReceiptSettings create-order ambiguity;
- provider-specific raw SQL and assumptions in historical repair migrations.

This task did not run EF migration commands because runtime configuration could resolve a live connection. It did not create or mutate a development database.

### 13. Build and test

After canonicalization and BasicSalary model restoration:

- Backend isolated build: PASS, 0 errors.
- Backend tests: PASS, `177/177`.
- Admin build: NOT REQUIRED by scope; BasicSalary is a backend model restoration only and no frontend code was changed.
- Customer build: NOT REQUIRED.

No API was started. The normal output path was not used for database/runtime verification.

### 14. Remaining human actions

1. Ensure the six canonical files for the three later applied IDs are included in the next approved commit; they are currently untracked because this task does not commit.
2. Review the canonical migration script against an isolated development PostgreSQL/Supabase project before applying it anywhere.
3. Decide separately whether `EmployeeType` should become an active employee-management property or remain database-only.
4. Review the two unique backup migrations before deciding whether they are obsolete historical artifacts.
5. Do not run the canonical chain against the current Supabase database without an explicit, separate approval.

## PHASE B FINAL STATUS

```text
CANONICAL MIGRATION TREE:
services/api/src/Infrastructure/Persistence/Migrations/

ACTIVE MIGRATION LOCATIONS:
1

TREE B:
CANONICALIZED / REMOVED

UNTRACKED ACTIVE MIGRATIONS:
3 migration IDs (6 source/designer files) until future commit

ADD BUSINESS INSIGHT HISTORY:
PRESERVED

REMOVE PAYROLL HISTORY:
PRESERVED

RECEIPT SETTINGS HISTORY:
PRESERVED / CLEAN-CHAIN RISK REMAINS

PAYROLL CURRENT MODEL:
ABSENT

PAYROLL FUNCTIONALITY:
OUT OF SCOPE

BASIC SALARY DOMAIN STATUS:
INTENTIONALLY RETAINED

BASIC SALARY EF MODEL:
RESTORED

BASIC SALARY DATABASE MIGRATION:
NOT REQUIRED

EMPLOYEE TYPE:
RETAINED FOR NOW

CURRENT MODEL REPRESENTED:
PARTIAL

DATABASE HISTORY REPRESENTED:
YES (supplied applied IDs preserved in canonical source)

CLEAN CHECKOUT DETERMINISTIC:
YES after the currently untracked canonical files are included in a future commit; NO if they remain uncommitted/untracked

CLEAN DATABASE MIGRATION:
STILL RISKY

DATABASE CONNECTION USED:
NO

DATABASE MUTATED:
NO

BACKEND BUILD:
PASS

BACKEND TESTS:
177/177

ADMIN BUILD:
NOT REQUIRED

SAFE TO CREATE SUPABASE DEV/TEST:
NO — isolated migration-script review remains required

SAFE TO RUN MIGRATIONS ON CURRENT SUPABASE:
NO

NEXT ACTION:
Commit the canonical untracked migration files only after review, then validate a generated migration script against a separate disposable PostgreSQL/Supabase development project.
```

## 14. Final status

```text
CANONICAL MIGRATION TREE:
services/api/src/Infrastructure/Persistence/Migrations/

TREE B ORIGIN:
Unknown; untracked and absent from Git history

TREE B FINAL STATUS:
NEEDS REVIEW

ACTIVE MIGRATION LOCATIONS:
1 tracked canonical location; 1 additional untracked API migration location currently affecting the working-tree build

CURRENT MODEL REPRESENTED:
PARTIAL

PAYROLL FINAL INTENT:
ABSENT / OUT OF SCOPE; historical migrations remain immutable

EMPLOYEE BASIC SALARY:
INTENTIONALLY RETAINED; no drop migration permitted

EMPLOYEE TYPE:
HUMAN REVIEW REQUIRED; no current non-migration usage found, do not drop

RECEIPTSETTINGS:
ISSUE / ORDERING AND DISCOVERABILITY UNCERTAIN

CLEAN CHECKOUT DETERMINISTIC:
NO for the current intended application state; HEAD itself contains Tree A only, while the working tree depends on untracked Tree B for newer migration IDs/schema intent

CLEAN DATABASE MIGRATION:
STILL RISKY

DATABASE CONNECTION USED:
NO

DATABASE MUTATED:
NO

BACKEND BUILD:
NOT RUN — Phase A gate stopped before reconciliation

BACKEND TESTS:
NOT RUN — Phase A gate stopped before reconciliation

SAFE TO CREATE SUPABASE DEV/TEST:
NO

NEXT ACTION:
Human review of __EFMigrationsHistory and Tree B provenance, then approve the canonical migration strategy before Phase B
```

## 15. Safety boundary

- No API was started.
- No database connection was made.
- No `dotnet ef database update` was run.
- No migration was added, removed, edited, moved or renamed.
- No User Secrets, connection string or environment variable was changed.
- No database was migrated, seeded, dropped or otherwise mutated.
- No commit or push was performed.
