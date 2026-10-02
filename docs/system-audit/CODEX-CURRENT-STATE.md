# Codex Current State

> **Resume here:** Read this file first. Then read only the detailed audit documents relevant to the assigned task. Current executable source is authoritative if it conflicts with documentation. Do not reconstruct completed audit history unless necessary.

> A source/test PASS is not browser/device runtime or E2E evidence unless that evidence is explicitly recorded.

## 1. Current baseline

- Latest backend suite after Task 47H.1B: **267/267 PASS** (5 new Guest QR tests).
- Prior QrToken persistence baseline: **262/262 PASS**.
- Focused QrToken foundation tests: **8/8 PASS**.
- Backend build: **PASS**.
- Admin Web build: **PASS**. Customer Web build: **PASS**.
- EF canonical migrations: **43**. EF-recognized migrations: **43**. Pending model changes: **NONE**.
- Latest migration: `20261001155855_FinalizeRestaurantTableQrToken`.

## 2. Architecture and security baseline

- Backend: ASP.NET Core 8, EF Core 8, PostgreSQL/Supabase, JWT, SignalR. Frontends: React/Vite.
- Canonical schema facts: `OrderDetails` (not `OrderItems`); no Restaurant/`RestaurantId` architecture; Payroll subsystem removed; `BasicSalary` and `EmployeeType` retained; BusinessInsight persisted.
- Expense `PaymentMethod` and `Note` are required.
- Management Login accepts `admin` and `manager`; Staff Login accepts `employee`, `cashier`, and `kitchen`. JWT role and branch scope derive from the database account, never client input.
- Manager branch-owned operations fail closed without a valid trusted `BranchId`. EmployeeController hardening, manager branch isolation, unified Management/Staff authentication, and Kitchen cash-shift route consistency are complete.
- Anonymous AI and Guest QR AI are denied. Registered Customer AI remains separate and unchanged.

## 3. Inventory baseline

Inventory Phases 1–4 implementation is substantially complete.

- Stock is branch-scoped.
- Receipt confirmation creates exactly one linked Expense; issue confirmation creates none.
- PostgreSQL transaction/locking prevents negative stock.
- Admin frontend and overview are implemented; previous PostgreSQL E2E/concurrency validation passed.

Do not reconstruct inventory implementation details unless assigned an Inventory task.

## 4. Migration baseline and history reconciliation

- The prior migration-count discrepancy is resolved. The raw canonical count now equals EF discovery: **43 == 43**.
- The following handwritten migration-like files were not EF-discoverable and were proven unnecessary to the canonical fresh migration chain because later generated migrations provide the final schema:
  - `20260906120000_AddReceiptSettings`
  - `20260906121500_AddPaymentAtToOrder`
  - `20260906143000_AddPayroll`
- They are preserved outside compiled migration source in `docs/archive/migrations/`.
- **Never restore them to** `services/api/src/Infrastructure/Persistence/Migrations/`.

## 5. QrToken persistence foundation — COMPLETE

`RestaurantTable.QrToken` is required, maximum length 43, unique, server-generated, and not client-authoritative.

`QrTokenGenerator.Generate()` uses `RandomNumberGenerator` with 32 random bytes (256-bit entropy), unpadded Base64Url, and emits exactly 43 characters.

### Migration A — applied

`20261001094205_AddRestaurantTableQrToken` introduced nullable `QrToken` and a unique filtered index. It was explicitly applied to the authorized isolated DEV project.

### Controlled backfill — complete

`scripts/diagnostics/QrTokenBackfill` remains retained for controlled maintenance only:

- dry-run by default;
- explicit `--execute` required;
- `DEV_SUPABASE_PROJECT_REF` target guard;
- reuses the canonical generator;
- transactional and idempotent; never regenerates existing tokens.

On isolated DEV: 7 Tables, 0 NULL tokens, 7 non-null, 7 distinct, 0 invalid length, and 0 invalid Base64Url. A second execution modified 0 rows. No token values were logged.

### Migration B — applied

`20261001155855_FinalizeRestaurantTableQrToken` is applied to isolated DEV. Final schema:

- `Tables.QrToken character varying(43) NOT NULL`;
- no default and no empty-string fallback;
- `IX_Tables_QrToken` is unique and unfiltered on `QrToken`.

Pre/post token-mapping fingerprints matched; existing tokens were preserved without regeneration.

### Authorized DEV

- Project reference: `qfkgjxwbshjgsxsvkpkp`.
- Applied migrations: **43**; latest applied: `20261001155855_FinalizeRestaurantTableQrToken`.
- Production was not touched. Do not record or print credentials.

## 6. Guest QR security — implementation complete; runtime validation partial

QrToken persistence and source implementation are complete. Overall Guest QR security must **not** be marked complete until browser/device runtime and E2E validation are performed.

Implemented trust chain:

`Physical QR (?qr=<opaque QrToken>)` → `POST /api/guest/bootstrap` → server resolves active `RestaurantTable` → server derives `BranchId` → signed Guest JWT → guest order authorization from claims.

- Bootstrap accepts only a 43-character opaque Base64Url `QrToken`; it does not accept client table or branch authority.
- Guest JWT is compatibility-role `customer`, but is authoritatively distinguished by `customerSessionType=guest`, signed `tableId`, and signed `branchId`.
- `GuestSessionContext` centralizes claim parsing and fails closed for absent or malformed guest claims.
- Guest order writes overwrite request table/branch values from trusted context. Guest list/detail reads require the same trusted table and branch.
- Anonymous `GET /api/Table/{id}` no longer establishes authority; it is authenticated-only. The phone token endpoint no longer mints unbound guest sessions.
- Customer Web uses `?qr=<QrToken>`, secure bootstrap, a separate `guestToken`, and a session-storage guest marker; `customerToken` remains for registered customers. Guest AI UI/access is denied.
- Public table responses do not expose `QrToken`. Authorized admin/manager QR generation uses `GET /api/Table/{id}/qr-token`. Legacy persisted `QrCodeUrl` values were not mass-updated.
- Guest and Registered Customer remain distinct. Phone-only Registered Customer identity is still outside this work.

Task 47H.1C runtime validation is **PARTIAL**. Authorized isolated DEV API validation completed successfully for valid and invalid bootstrap, signed table/branch claims, normal guest order creation, client table/branch injection resistance, cross-table order-read denial, arbitrary OrderId protection, Guest versus Registered Customer separation, Guest AI denial, and legacy GUID authority denial. The test data had no second branch, so cross-branch runtime validation was blocked.

Remaining validation is Customer Web browser E2E (QR URL, bootstrap, menu/order UI, refresh, and storage separation) and cross-branch runtime validation when suitable isolated DEV data exists. No Guest JWT, QrToken, or secret values were recorded.

Untrusted input for Guest authorization includes request/query/body `TableId`, `BranchId`, and `OrderId`, local storage context, arbitrary GUIDs, frontend role values, and AI arguments.

## 7. Immediate next task

`47H.1C.1 — CUSTOMER WEB BROWSER AND CROSS-BRANCH GUEST QR VALIDATION`

Complete the remaining Customer Web browser E2E and cross-branch runtime validation. Do not claim Guest QR runtime validation is complete until both are evidenced.

## 8. Deferred task

`47H.2 — REGISTERED CUSTOMER IDENTITY VERIFICATION POLICY`

Registered Customer phone-only identity remains a separate product/security decision. Do not mix it into Guest QR work and do not automatically introduce OTP.

## 9. Permanent migration/database safety rules

- `DatabaseSafety:AllowAutomaticMigration=false`; startup automatic migration remains disabled.
- Apply migrations only explicitly and only after positively identifying the authorized target.
- Never manually edit `__EFMigrationsHistory`.
- Never restore archived handwritten migration files into the canonical migration directory.
- Never print database, JWT, API, or certificate secrets, or QrToken values.
- Do not rotate QrTokens or mutate persisted legacy `QrCodeUrl` values without explicit authorization.

## 10. Detailed references

- [Guest QR session security](52-GUEST-QR-SESSION-SECURITY.md)
- [EF migration baseline reconciliation](53-EF-MIGRATION-BASELINE-RECONCILIATION.md)
- [Public/Guest/Customer/AI security](51-PUBLIC-GUEST-CUSTOMER-AI-SECURITY.md)
- [Role/function matrix](48-ROLE-FUNCTION-MATRIX.md)
