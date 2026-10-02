# Development Database Isolation

Date: 2026-09-30  
Scope: configuration/startup preparation only

## Strategy

Use a separate Supabase/PostgreSQL development project and configure it through .NET User Secrets. The application now has an explicit Development-only opt-in for automatic migration and seed:

```text
ASPNETCORE_ENVIRONMENT = Development
DatabaseSafety:AllowAutomaticMigration = true
```

The default value is `false`. The current/main Supabase database must never be used for this workflow.

## Changes made

### `Program.cs`

The previous unconditional startup block called:

```text
Database.Migrate()
DbInitializer.SeedAsync(...)
```

on every environment. It now runs only when both conditions are true:

```text
environment is Development
AND DatabaseSafety:AllowAutomaticMigration is true
```

Otherwise the API skips schema/data mutation and continues normal startup. Production/non-Development startup cannot automatically migrate or seed, even if the flag is accidentally set.

Realtime application behavior and business logic were not changed.

### `appsettings.json`

Added a non-secret default:

```json
"DatabaseSafety": {
  "AllowAutomaticMigration": false
}
```

The connection string remains empty in tracked configuration.

## Configuration inventory

| Item | Current behavior |
|---|---|
| Environment | `launchSettings.json` uses `Development` for the project profile |
| Connection key | `ConnectionStrings:DefaultConnection` |
| Fallback connection key | `SUPABASE_CONNECTION_STRING` environment variable |
| JWT secret | Required at startup from `Jwt:Secret` or equivalent environment configuration |
| Seed admin password | `Seed:AdminPassword`, with `RESTAURANTPOS_SEED_ADMIN_PASSWORD` fallback |
| Gemini key | Not required for migration/seed or the core QR/POS/Kitchen migration test; required only when AI functionality is used |
| Automatic migration | Development + explicit opt-in only |
| Automatic seed | Runs in the same guarded block as migration |
| User Secrets support | Enabled by `UserSecretsId=RestaurantPOS-api-local` |
| Runtime provider | PostgreSQL via Npgsql; Supabase connection string is normalized by existing code |

Configuration precedence follows the standard .NET configuration pipeline: appsettings, environment-specific appsettings when present, User Secrets in Development, environment variables, and command-line configuration. No `appsettings.Development.json` exists in the project currently.

## User Secrets template

Run these templates only after confirming the target is the disposable development project. Values are intentionally placeholders and were not executed:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<DEV_CONNECTION_STRING>" --project .\services\api\RestaurantPOS.api.csproj
dotnet user-secrets set "Jwt:Secret" "<DEV_ONLY_JWT_SECRET>" --project .\services\api\RestaurantPOS.api.csproj
dotnet user-secrets set "Seed:AdminPassword" "<DEV_ONLY_ADMIN_PASSWORD>" --project .\services\api\RestaurantPOS.api.csproj
dotnet user-secrets set "DatabaseSafety:AllowAutomaticMigration" "true" --project .\services\api\RestaurantPOS.api.csproj
```

Optional AI-only configuration:

```powershell
dotnet user-secrets set "Gemini:ApiKey" "<GEMINI_KEY>" --project .\services\api\RestaurantPOS.api.csproj
```

The connection string, JWT secret, seed password, and Gemini key must not be placed in tracked files or frontend configuration.

## Main database protection

The minimum protection is now explicit opt-in plus `Development` environment. A normal launch with the default `false` setting does not call `Migrate` or the seeder. Production/non-Development environments never enter the migration/seed branch.

This is a configuration safety gate, not proof that a connection string points to a disposable database. Human verification of the Supabase project/host is still required before enabling the flag.

No hostname, password, marker, or current database identifier was hardcoded.

## Seeder audit

`DbInitializer.SeedAsync` is guarded by the same opt-in startup block. On an empty database it can create:

- one main Branch;
- Areas;
- restaurant Tables;
- one development admin employee;
- development Product records.

The admin password is read from `Seed:AdminPassword` or its environment fallback and is hashed with the existing ASP.NET password hasher. The seeder refuses to create a default admin outside Development.

Seeding is partially idempotent: it uses existence checks, but the outer `Branches.AnyAsync()` gate means a partially populated database is not fully reconciled by a later seed run. It does not seed Customer, Order, OrderItem, OrderRequest, Payment, or Kitchen history data. Those records can be created through the application flow after the base dataset is available.

The base seed is sufficient to provide branch/table/product prerequisites, but it is not a complete QR-to-payment E2E fixture. Therefore seed sufficiency for full E2E is **PARTIAL**.

## Safe execution plan — not executed

1. Create or select a disposable Supabase development project.
2. Confirm its host/project is not the current/main Supabase project.
3. Configure the User Secrets templates above with development-only values.
4. Confirm `ASPNETCORE_ENVIRONMENT=Development`.
5. Confirm `DatabaseSafety:AllowAutomaticMigration=true` is present only in the development profile.
6. Apply/test the canonical migrations against that isolated target using the approved execution method.
7. Verify `__EFMigrationsHistory` and final schema with the read-only queries below.
8. Seed the base branch/table/product/admin dataset.
9. Start the API only after the target and safety flag have been verified.
10. Run the QR → menu → cart → order → POS → Kitchen → payment verification.

No step above was executed in this task.

## Post-migration read-only validation queries

Run these only against the isolated development database after migration. They return metadata/counts and do not mutate data.

### Applied migrations

```sql
SELECT "MigrationId", "ProductVersion"
FROM "__EFMigrationsHistory"
ORDER BY "MigrationId";
```

### Required table presence

```sql
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name IN (
    '__EFMigrationsHistory', 'Branches', 'Areas', 'Tables', 'Employees',
    'Products', 'Categories', 'Orders', 'OrderDetails', 'OrderItems',
    'OrderRequests', 'OrderRequestItems', 'Customers', 'ReceiptSettings',
    'BusinessInsights', 'Notifications', 'LoyaltyTransactions',
    'SystemSettings', 'Toppings', 'Reservations', 'Expenses'
  )
ORDER BY table_name;
```

### Payroll absence

```sql
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name IN ('Payrolls', 'PayrollSettings', 'PayrollAdjustments', 'EmployeeSalaryProfiles');
```

Expected result: zero rows.

### Employee columns

```sql
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_schema = 'public'
  AND table_name = 'Employees'
  AND column_name IN ('BasicSalary', 'EmployeeType', 'Role')
ORDER BY column_name;
```

Expected: `BasicSalary`, `EmployeeType`, and `Role` are present according to the intended schema.

### ReceiptSettings columns

```sql
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_schema = 'public'
  AND table_name = 'ReceiptSettings'
ORDER BY ordinal_position;
```

### BusinessInsights and operational data

```sql
SELECT COUNT(*) AS business_insight_count FROM "BusinessInsights";
SELECT COUNT(*) AS branch_count FROM "Branches";
SELECT COUNT(*) AS table_count FROM "Tables";
SELECT COUNT(*) AS product_count FROM "Products";
SELECT COUNT(*) AS customer_count FROM "Customers";
SELECT COUNT(*) AS order_count FROM "Orders";
SELECT COUNT(*) AS order_request_count FROM "OrderRequests";
```

### Order/payment-related columns

```sql
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_schema = 'public'
  AND table_name IN ('Orders', 'OrderDetails', 'OrderItems')
ORDER BY table_name, ordinal_position;
```

If a separate `Payments` table is expected by a later environment, verify it explicitly:

```sql
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name ILIKE '%payment%';
```

## Verification

- Database connection used: NO.
- Database mutated: NO.
- API started: NO.
- Backend build: PASS using an isolated output path.
- Backend tests: `177/177` PASS.
- Frontend builds: not required for this backend startup/configuration task.

## Final status

```text
DEVELOPMENT DB STRATEGY:
separate Supabase/PostgreSQL project via .NET User Secrets

MAIN DB AUTO MIGRATION:
DISABLED

DEVELOPMENT AUTO MIGRATION:
EXPLICIT OPT-IN

DEVELOPMENT AUTO SEED:
EXPLICIT OPT-IN

CONNECTION STRING STORAGE:
USER SECRETS

CONNECTION STRING COMMITTED:
NO

DATABASE CONNECTION USED:
NO

DATABASE MUTATED:
NO

BACKEND BUILD:
PASS

BACKEND TESTS:
177/177

SEED SUFFICIENT FOR FULL E2E:
PARTIAL

SAFE TO CONFIGURE DEV CONNECTION:
YES, after human confirms the separate Supabase target

SAFE TO RUN FIRST DEV MIGRATION:
YES, only after target/configuration verification and with explicit opt-in

NEXT HUMAN ACTION:
Set the four required development User Secrets using placeholders replaced with development-only values, verify the Supabase project is isolated, then execute the approved migration plan against that target.
```

No commit or push was performed.
