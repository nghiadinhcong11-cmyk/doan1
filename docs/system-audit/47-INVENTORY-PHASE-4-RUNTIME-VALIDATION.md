# Inventory Phase 4 — Runtime UI/API Validation

## 1. Status

**PARTIAL — AWAITING HUMAN NORMAL WINDOWS POWERSHELL RUN**

The repository is ready for runtime validation against the isolated DEV database. Codex did not connect to Supabase, start a database-backed API workflow, create Inventory records, or run the UI flow.

## 2. DEV safety gate

Authorized project:

`qfkgjxwbshjgsxsvkpkp`

Required host-side command:

```powershell
$env:DEV_SUPABASE_PROJECT_REF = "qfkgjxwbshjgsxsvkpkp"
dotnet run --project .\scripts\diagnostics\HostSelect1\HostSelect1.csproj --framework net8.0
```

Required output: `TARGET CHECK: PASS`, `CONNECTION OPEN: PASS`, and `SELECT 1: PASS`.

## 3. Host execution commands

From `D:\Dev\doan`, start the API in one normal Windows PowerShell terminal with automatic database mutation explicitly disabled:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:DatabaseSafety__AllowAutomaticMigration = "false"
dotnet run --project .\services\api\RestaurantPOS.api.csproj --launch-profile RestaurantPOS.api
```

The expected API endpoint is `https://localhost:5000`. In a second terminal start Admin Web:

```powershell
npm run dev --prefix .\apps\admin-web -- --host localhost --port 5173
```

Use the existing authorized DEV account without placing credentials in logs or this report. The full UI checklist is defined by Task 47: item creation, duplicate validation, minimum stock, receipt/expense, issue, ledger/history, overview, branch/role checks, responsive/accessibility checks, and browser/network inspection.

## 4. Read-only post-runtime validation

After the UI run, execute from normal Windows PowerShell:

```powershell
$env:DEV_SUPABASE_PROJECT_REF = "qfkgjxwbshjgsxsvkpkp"
dotnet run --project .\scripts\diagnostics\PostMigrationInventoryValidation\PostMigrationInventoryValidation.csproj --framework net8.0
```

No migration or seed operation belongs in this validation command.

## 5. Current results

| Area | Result |
|---|---|
| DEV target | AWAITING HUMAN HOST CHECK |
| API startup | NOT EXECUTED BY CODEX |
| Admin startup/login/UI workflow | AWAITING HUMAN HOST RUN |
| Database mutation | NONE BY CODEX |
| New migration | NO |
| Manager/operational role runtime | AWAITING FIXTURE/RUN |
| Browser console/network | AWAITING HUMAN HOST RUN |
| Post-runtime schema validation | AWAITING HUMAN HOST RUN |

## 6. Local non-database verification

Phase 3A baseline remains available:

- Backend build: PASS
- Backend tests: 216/216 PASS after rebuilding the test assembly.
- Admin build: PASS
- Customer build: PASS

## 7. Human return evidence

Return sanitized output for the safety gate, API startup, browser console/network errors, each first failing workflow step, and the post-runtime validator. Include only test prefixes such as `UI_INV_...`; never include passwords, JWTs, full connection strings, or personal data.

## 8. Verdict

Phase 4 cannot be marked PASS until the human host completes the real UI/API workflow and returns runtime evidence. No production database was touched by Codex.

## 9. Task 47A — Admin HTTPS API Connectivity

Runtime evidence showed Admin login targeting `http://localhost:5000/api/Auth/login` while the API listens on `https://localhost:5000`. The Admin development fallback in `src/config.ts` used the Vite page protocol, so an HTTP Vite page produced an HTTP API URL.

The fallback now uses `https:` during Vite development, while `VITE_API_URL` remains the deployment override. The backend HTTPS configuration was not changed.

The Admin global fetch interceptor previously read the shared `localStorage` key `token` and attached it to every API request. This allowed a stale Customer JWT to appear on login requests. Admin and Customer tokens now use separate keys (`adminToken` and `customerToken`), and Admin authentication endpoints under `/api/Auth/` are excluded from bearer injection.

Because the current Vite development page is HTTP, the development CORS policy now explicitly permits localhost/127.0.0.1 HTTP origins on ports 5173/5174. LAN origins remain HTTPS-only. No database, migration, or schema change was made.

Human retest must verify:

- `POST https://localhost:5000/api/Auth/login`;
- no `Authorization: Bearer` header on that login request;
- successful Admin login and Inventory navigation;
- no certificate or CORS error after accepting/trusting the local development certificate as appropriate.

## 10. Task 47A validation status

- Root cause: Admin development API fallback inherited the HTTP Vite page protocol; the backend remains HTTPS-only.
- Admin API fallback after fix: `https://localhost:5000`.
- Login bearer handling: `/api/Auth/*` is excluded from the Admin fetch interceptor, and Admin/Customer token storage is namespaced separately.
- CORS: localhost HTTP Vite origins were added explicitly; LAN origins remain HTTPS-only.
- Backend HTTPS configuration: unchanged.
- Database/migration: no connection, mutation, or migration.
- Admin build: PASS.
- Customer build: PASS.
- Backend build: PASS in isolated output; the normal output directory was locked by the active API process.
- Backend tests: 216/216 PASS.

The human login retest is still required to confirm the browser request is exactly `https://localhost:5000/api/Auth/login` and has no stale Customer bearer header.

## 11. Task 47B — Inventory Overview PostgreSQL Query Fix

### Runtime symptom

The human Phase 4 run reached `GET /api/inventory/overview/{branchId}`, but PostgreSQL request execution failed in `InventoryService.GetQuantityByUnitAsync`. The original query grouped a joined anonymous projection and directly constructed `InventoryQuantityByUnitResponse` inside the aggregate projection. EF Core 8/Npgsql could not translate that complete expression.

### Fix

The query now keeps filtering, joining, grouping, summing, and deterministic ordering in the database, projecting only scalar anonymous values (`UnitCode` and `Quantity`). The application maps those already-aggregated rows to response DTOs after `ToListAsync()`. No transaction history is materialized before aggregation.

### Preserved behavior

- branch and transaction-type filters remain in SQL;
- the UTC half-open month range remains unchanged;
- quantities remain grouped by `UnitCode`, so mixed units are never combined;
- ordering remains by `UnitCode`;
- empty matches return an empty collection.

### Regression coverage and validation

The overview tests cover same-unit aggregation, mixed units, IN/OUT separation, ADJUSTMENT exclusion, branch isolation, month start/end boundaries, and empty-result behavior. After rebuilding the test assembly, the complete suite passed `216/216`.

No migration was created, no schema was changed, no database was connected to or mutated, and human runtime retest of the overview endpoint remains required.

## 12. Task 47C — Runtime Role & Authorization Survey

### Scope and safety result

This task performed source inspection and attempted only local, read-only runtime checks. No business mutation endpoint was called. No migration, schema change, seed, reset, or production connection was performed.

| Check | Result |
|---|---|
| Local API listener available to this Codex session | NOT AVAILABLE (no listener on ports 5000/5173) |
| Credential environment variables | MISSING in this Codex session for all six surveyed roles |
| DEV database target changed | NO |
| Automatic migration | NOT RUN |
| Business-data mutation | NO |
| Browser UI survey | NOT EXECUTED — browser runtime bootstrap was unavailable |

Because the credentials were not present and the local listeners were not available, authentication and HTTP authorization results are **BLOCKED**, not inferred as pass/fail. A human host run is required for runtime evidence.

### Source-derived authentication model

- Employee authentication is `POST /api/Auth/login` with `Mode` values `admin`, `cashier`, or `kitchen`.
- `admin` mode accepts only effective roles `admin` and `manager`.
- `cashier` mode accepts `admin`, `manager`, and `cashier` and may validate a supplied branch.
- `kitchen` mode accepts `admin`, `manager`, and `kitchen` and may validate a supplied branch.
- Customer authentication is `POST /api/Auth/customer-token` with a phone identifier and no password.
- JWTs contain a role claim and, for employee accounts assigned to a branch, a `branchId` claim.
- `GET /api/Auth/me` is protected by `[Authorize]`.

### Source-derived authorization matrix

The matrix below describes controller attributes and the current Admin Web route structure; it is not a runtime PASS claim.

| Module / representative API | admin | manager | employee | cashier | kitchen | customer |
|---|---|---|---|---|---|---|
| Dashboard summary | ALLOWED | ALLOWED | DENIED | DENIED | DENIED | DENIED |
| Orders read / detail | ALLOWED | ALLOWED | ALLOWED | ALLOWED | ALLOWED | CONDITIONAL |
| Kitchen active/history | ALLOWED | ALLOWED | DENIED | DENIED | ALLOWED | DENIED |
| Customers list | ALLOWED | ALLOWED | ALLOWED | ALLOWED | DENIED | DENIED |
| Customer own profile/history | CONDITIONAL | CONDITIONAL | DENIED | DENIED | DENIED | CONDITIONAL |
| Expenses | ALLOWED | ALLOWED | DENIED | DENIED | DENIED | DENIED |
| Employees | AUTHENTICATED CONTROLLER; ACTION POLICY REQUIRES REVIEW | same | same | same | same | same |
| Branches read | ALLOWED by route source | ALLOWED by route source | ALLOWED by route source | ALLOWED by route source | ALLOWED by route source | ALLOWED by route source |
| Branches mutation | ALLOWED | DENIED | DENIED | DENIED | DENIED | DENIED |
| Products/categories/toppings read | ALLOWED/public depending endpoint | same | same | same | same | same |
| Product/menu management | ALLOWED | DENIED except availability path | DENIED | DENIED | CONDITIONAL availability | DENIED |
| Promotions read | public route | public route | public route | public route | public route | public route |
| Work schedule / attendance read | ALLOWED | ALLOWED | ALLOWED | ALLOWED | ALLOWED | DENIED |
| Shift read | ALLOWED | ALLOWED | ALLOWED by controller default? | ALLOWED | DENIED | DENIED |
| Reservations read | AUTHENTICATED | AUTHENTICATED | AUTHENTICATED | AUTHENTICATED | AUTHENTICATED/read-only UI | DENIED by controller auth |
| Business insights | ALLOWED | ALLOWED | DENIED | DENIED | DENIED | DENIED |
| Notifications | any authenticated role | any authenticated role | any authenticated role | any authenticated role | any authenticated role | any authenticated role |
| Inventory overview/branch/receipts/issues/history reads | ALLOWED | ALLOWED branch-scoped | ALLOWED branch-scoped | ALLOWED branch-scoped | ALLOWED branch-scoped | DENIED |
| Inventory item master reads | ALLOWED | ALLOWED | DENIED | DENIED | DENIED | DENIED |
| Inventory item/minimum-stock management | ALLOWED | ALLOWED branch-scoped where applicable | DENIED | DENIED | DENIED | DENIED |
| Inventory receipt create/edit/confirm/cancel | ALLOWED | ALLOWED branch-scoped | DENIED | DENIED | DENIED | DENIED |
| Inventory issue draft/read/edit/cancel | ALLOWED | ALLOWED branch-scoped | CONDITIONAL draft/read/edit/cancel by service policy | same | same | DENIED |
| Inventory issue confirm | ALLOWED | ALLOWED branch-scoped | DENIED | DENIED | DENIED | DENIED |

The Inventory controller explicitly authorizes `admin,manager,employee,cashier,kitchen` at class level, restricts master data and confirmations to `admin,manager`, and delegates branch filtering to `InventoryService`. The service rejects non-admin access when the requested branch differs from the JWT branch claim.

### Runtime survey results

The safe runtime harness checked credential-variable presence without printing values. All required variables were missing in this session, so the following were not executed:

- employee-role authentication;
- customer phone authentication;
- authenticated GET requests for Dashboard, Customers, Expenses, Inventory, Orders, Kitchen, schedules, attendance, shifts, reservations, notifications, and reports;
- manager same-branch and cross-branch read checks;
- unauthenticated protected-endpoint status verification;
- browser login, navigation, direct-route, console, and network checks.

The attempted unauthenticated Inventory request could not produce an HTTP status because no local API listener was available to the session. It is therefore `BLOCKED`, not classified as an authorization result.

### Source findings requiring follow-up

1. **[High, source mismatch] employee authentication** — `AuthController.Login` has no mode that accepts effective role `employee`; `admin` rejects it, while `cashier` and `kitchen` reject it. `apps/admin-web/src/features/auth/pages/LoginPage.tsx` also exposes only `admin`, `cashier`, and `kitchen` modes and does not model `employee` in its login callback. Runtime confirmation is pending credentials/host, but the source mismatch is concrete. Proposed follow-up: `TASK 47D — employee authentication and staff-shell contract review`.
2. **[Medium, UI/API mismatch risk] Admin Web role shells** — `App.tsx` has dedicated shells for cashier and kitchen and a shared admin/manager shell, but does not provide an employee shell. Backend route attributes and frontend route visibility are not equivalent. Proposed follow-up: include employee and direct-route checks in Task 47D.

### Task 47D — Unified management/staff authentication (source and automated validation)

The historical employee-login mismatch is corrected in source: management login accepts only stored admin/manager accounts, while staff login accepts only stored employee/cashier/kitchen accounts. The server derives JWT role and branch from the authenticated employee record. Employee now routes to `/staff/profile` with profile, own attendance, own schedule, and inventory-overview navigation. Human Phase 4 runtime retest must use **Đăng nhập quản lý** for admin/manager and **Đăng nhập nhân viên** for employee/cashier/kitchen; no browser or DEV database login was performed by Codex.
3. **[Audit limitation] broad controller authorization** — some controllers use `[Authorize]` without role restrictions (for example parts of Employee, Reservation, System Settings, and Notification flows). Runtime status and branch ownership must be verified before treating these as intended permissions.

No issue above was fixed in Task 47C.

### Deferred mutation tests

The following were intentionally not executed: creating/updating/deleting inventory items, minimum-stock changes, receipt or issue creation/edit/confirmation/cancellation, expense mutations, order/payment mutations, employee changes, password changes, and any stock movement. They belong to dedicated Phase 4 E2E tasks.

### Validation

- Backend build: PASS (`dotnet build .\\services\\api\\RestaurantPOS.api.csproj --no-restore`).
- Backend tests: PASS, `215/215` in the available local test assembly. The task-provided baseline said `216/216`; the observed repository run did not contain that additional test count, so no higher count is claimed.
- Source files modified by Task 47C: none.
- Documentation updated: this report only.
- Migration created/applied: NO/NO.
- Database mutation: NO.

### Verdict

`TASK 47C: PARTIAL — SURVEY COMPLETE WITH BLOCKERS/UNEXECUTED UI TESTS`

Human host rerun is required with the credential environment variables available, API/Admin Web running, and sanitized role-level HTTP evidence. Inventory Phase 4 is not complete.

## 13. Task 47C.1 — Exhaustive source role/function inventory

The source-only audit is documented in [`48-ROLE-FUNCTION-MATRIX.md`](48-ROLE-FUNCTION-MATRIX.md). It inventories 23 controllers, 117 backend actions, Admin Web/Customer Web routes, AI tools, Inventory role mapping, and frontend/backend authorization mismatches. It does not claim runtime authorization validation and does not change application behavior.
