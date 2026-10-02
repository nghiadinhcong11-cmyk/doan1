# Clean Database Migration Chain Finalization

Date: 2026-09-30  
Scope: static/offline source validation only

## Decision

The canonical migration chain is statically ready for a separate disposable development/test database execution test. No API was started, no EF database command was run, and no database connection was made.

This is not production migration verification.

## Canonical source

The only active migration location is:

```text
services/api/src/Infrastructure/Persistence/Migrations/
```

The former untracked Tree B was removed from the API compile path during Phase B. The canonicalized files for the later applied IDs remain in Tree A and must be included in a future approved commit; they were intentionally not deleted or committed in this task.

## Migration inventory and discoverability

Static inspection found 43 migration classes in the canonical directory:

- 40 generated/discoverable migrations with `.Designer.cs` and `MigrationAttribute` metadata;
- 3 manual historical classes without designer/attribute metadata.

The 15 supplied applied IDs were all present exactly once with source, designer and migration metadata:

| Migration ID | Applied evidence | Designer | MigrationAttribute | Result |
|---|---:|---:|---:|---|
| `20260907044417_AddEmployeeTypeColumn` | Yes | Yes | Yes | Discoverable |
| `20260907060113_AddPaymentAtToOrderCorrective` | Yes | Yes | Yes | Discoverable |
| `20260907060600_AddMissingPayrollsAndSettings` | Yes | Yes | Yes | Discoverable |
| `20260907063921_Corrective_FixMissingColumns` | Yes | Yes | Yes | Discoverable |
| `20260907155909_RepairMissingDatabaseSchema` | Yes | Yes | Yes | Discoverable |
| `20260907164814_AddSystemSettings` | Yes | Yes | Yes | Discoverable |
| `20260908032924_AddOrderFinancialSnapshot` | Yes | Yes | Yes | Discoverable |
| `20260908041144_AddBranchStoreProfile` | Yes | Yes | Yes | Discoverable |
| `20260908043852_AddLoyaltyFoundation` | Yes | Yes | Yes | Discoverable |
| `20260908045205_AddLoyaltyIdempotencyIndex` | Yes | Yes | Yes | Discoverable |
| `20260908094237_AddRoleToEmployee` | Yes | Yes | Yes | Discoverable |
| `20260908100201_BackfillEmployeeRoles` | Yes | Yes | Yes | Discoverable |
| `20260910052615_AddAnalyticsIndexes` | Yes | Yes | Yes | Discoverable |
| `20260910063419_AddBusinessInsight` | Yes | Yes | Yes | Discoverable |
| `20260919075618_RemovePayrollAndRepairReceiptSettings` | Yes | Yes | Yes | Discoverable |

The three manual classes are retained as historical source but are not promoted into active EF discovery because their applied status was not part of the supplied verified applied-ID set:

| Migration ID | Purpose | Designer | Attribute | Decision |
|---|---|---:|---:|---|
| `20260906120000_AddReceiptSettings` | Early ReceiptSettings create | No | No | Remain non-discoverable historical source |
| `20260906121500_AddPaymentAtToOrder` | Early PaymentAt add | No | No | Remain non-discoverable historical source |
| `20260906143000_AddPayroll` | Historical Payroll creation | No | No | Remain non-discoverable historical source |

Adding fabricated metadata to these classes would change clean-database semantics: it could create ReceiptSettings twice or introduce an unverified duplicate Payroll path. No metadata repair was therefore necessary or safe for them.

No duplicate migration IDs were found among the canonical migration attributes.

## ReceiptSettings chain

| Migration | Operation | Guard/condition | Clean-chain result |
|---|---|---|---|
| `20260906120000_AddReceiptSettings` | `CreateTable` and unique branch index | No guard; class has no EF migration metadata | Not part of active discovered chain |
| `20260907060600_AddMissingPayrollsAndSettings` | Empty `Up` | N/A | No ReceiptSettings operation |
| `20260907155909_RepairMissingDatabaseSchema` | Raw SQL for EmployeeType, PaymentAt and Payroll tables/indexes/FK | `IF NOT EXISTS` / `DO $$` guards; no ReceiptSettings create | No ReceiptSettings operation |
| `20260919075618_RemovePayrollAndRepairReceiptSettings` | Drops historical Payroll tables, then creates ReceiptSettings and unique branch index with current fields | No `IF NOT EXISTS` guard | First and only active ReceiptSettings create |

Therefore, in the discoverable clean chain, ReceiptSettings is created once by `20260919075618...`. The earlier manual class is preserved but not activated. The applied current Supabase history for the final repair migration is preserved without rewriting its semantics.

## BusinessInsight and analytics

- `20260910063419_AddBusinessInsight` has one canonical source/designer pair and one `CreateTable("BusinessInsights")` operation.
- Its indexes are created after the table in the same migration.
- `20260910052615_AddAnalyticsIndexes` creates the order analytics index after the Orders table and required columns already exist.
- No duplicate migration IDs or duplicate BusinessInsights create operation were found in the canonical active chain.

## Payroll chronology

The active discoverable chronology is:

```text
20260907044417_AddEmployeeTypeColumn
  → 20260907060600_AddMissingPayrollsAndSettings (empty corrective migration)
  → 20260907155909_RepairMissingDatabaseSchema
       - guarded Payroll table/index/FK creation
  → 20260919075618_RemovePayrollAndRepairReceiptSettings
       - drops Payroll tables
       - creates current ReceiptSettings
```

The final intended schema has no Payroll tables or current Payroll entities. Historical Payroll migration source remains preserved. Payroll functionality remains out of scope.

## BasicSalary and EmployeeType

- `Employee.BasicSalary` is present in the current entity and in the latest snapshot.
- `BasicSalary` was already created by the initial migration as `numeric NOT NULL`; no `AddColumn` migration is needed or created.
- `EmployeeType` is retained in the historical/database intent and snapshot, but is not active application functionality. No drop operation or migration was added.
- No payroll calculation, API, service, page or navigation was introduced.

## Snapshot consistency

| Area | Result |
|---|---|
| BasicSalary | Present and aligned |
| Payroll entities | Absent from current model and snapshot final intent |
| BusinessInsight | Present in current model and snapshot; canonical create migration present |
| ReceiptSettings | Present in current model and snapshot; active clean create occurs once |
| PaymentAt | Present in current `Order` model and snapshot; guarded repair migration supplies the column if needed |
| Loyalty entities/indexes | Present and represented |
| Kitchen entities | Present and represented |
| Financial snapshots | Present and represented |
| EmployeeType | Snapshot/database-only retained field; intentional mismatch with current active model |

Overall snapshot status: **PARTIAL**, with the remaining EmployeeType difference intentional and documented, not an accidental drop decision.

## Offline validation limits

No `dotnet ef migrations list` or `dotnet ef migrations script` command was run. In this project, design-time tooling can resolve runtime configuration, so those commands were not treated as guaranteed connection-free without a dedicated design-time factory/configuration boundary.

Instead, validation used:

- canonical source and namespace inspection;
- `MigrationAttribute`/designer inventory;
- duplicate-ID scan;
- chronological operation inspection;
- current model/snapshot comparison;
- backend compilation and tests.

No SQL was generated or executed.

## Verification

- Database connection used: NO.
- Database mutated: NO.
- API started: NO.
- Backend isolated build: PASS.
- Backend tests: `177/177` PASS.
- Frontend builds: not required for this backend/migration-only task.

## Final status

```text
CANONICAL TREE:
services/api/src/Infrastructure/Persistence/Migrations/

CANONICAL MIGRATION COUNT:
43 migration classes

DISCOVERABLE MIGRATION COUNT:
40

MISSING APPLIED MIGRATIONS:
0 from the supplied verified applied-ID set

DUPLICATE MIGRATION IDS:
0

RECEIPT SETTINGS CHAIN:
SAFE

BUSINESS INSIGHT CHAIN:
SAFE

ANALYTICS INDEX CHAIN:
SAFE

PAYROLL FINAL SCHEMA:
ABSENT

BASIC SALARY FINAL MODEL:
PRESENT

EMPLOYEE TYPE:
RETAINED

MODEL SNAPSHOT:
PARTIAL — EmployeeType is intentional database/history retention

DATABASE CONNECTION USED:
NO

DATABASE MUTATED:
NO

BACKEND BUILD:
PASS

BACKEND TESTS:
177/177

CLEAN MIGRATION STATIC VALIDATION:
PASS

SAFE TO CREATE SUPABASE DEV/TEST:
YES — execution test must target a separate disposable project/database

SAFE TO MIGRATE CURRENT SUPABASE:
NO

NEXT ACTION:
Create/select an isolated development PostgreSQL/Supabase database, review the generated SQL there, then run the canonical migrations only against that isolated target.
```

This report does not authorize production migration, current Supabase migration, commit, or push.
