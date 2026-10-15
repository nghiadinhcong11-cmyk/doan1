# Code and Documents Map

Only paths observed in the repository are listed here. Read the relevant source before making a claim.

## Backend and shared contract

- Entry point/configuration: `services/api/Program.cs`.
- Domain entities/enums: `services/api/src/Domain/`.
- Application services/DTOs/security helpers: `services/api/src/Application/`.
- Persistence/configuration: `services/api/src/Infrastructure/Persistence/`.
- Controllers/middleware: `services/api/src/WebAPI/`.
- API snapshot/types: `packages/shared/swagger.json`, `packages/shared/types/`.

## Frontend

- Admin/POS/KDS: `apps/admin-web/src/`.
- Admin route composition: `apps/admin-web/src/app/routes/`.
- Customer ordering/profile/QR: `apps/customer-web/src/`.
- Shared visual components/assets: `apps/shared/`.
- Frontend package/build configuration: `apps/admin-web/package.json`, `apps/customer-web/package.json`, Vite and TypeScript configs.

## High-risk implementation paths

| Area | Source paths |
|---|---|
| Auth and roles | `Program.cs`, `AuthController.cs`, `JwtService.cs`, `RoleNames.cs`, `apps/admin-web/src/features/auth/` |
| Branch isolation | Controllers/services using `BranchId`, `ApplicationDbContext.cs`, `GuestSessionContext.cs`, `apps/admin-web/src/App.tsx` and branch-aware pages |
| Order/payment | `Order.cs`, `OrderService.cs`, `OrderController.cs`, `LoyaltyService.cs`, `InvoiceController.cs`, admin POS pages |
| Kitchen/realtime | `KitchenHub.cs`, `KitchenNotifier.cs`, `PaymentEventRouting.cs`, `KitchenService.cs`, admin kitchen/POS/notification consumers |
| Database | `ApplicationDbContext.cs`, `DbInitializer.cs`, `services/api/src/Infrastructure/Persistence/Migrations/` |
| AI | `services/api/src/AI/`, `AI_RULES.md`, AI registration in `Program.cs` |

## Scripts and operational entry points

- Local HTTPS/LAN launcher: `scripts/start-dev.ps1`, `scripts/setup-https.ps1`.
- Manual-test backend: `scripts/diagnostics/StartManualTestBackend.ps1`.
- Read-only/guarded diagnostics: `scripts/diagnostics/` and `scripts/README.md`.
- Build/codegen commands: root `package.json`, application package files and `services/api/RestaurantPOS.api.csproj`.

Read diagnostic scripts before running them. `--apply`, `--execute`, migration and seed operations require separate target verification and approval.

## Manual testing

- Entry: `ba-test-frontend/README.md`.
- Role/page checklists: `ba-test-frontend/00-COVERAGE-MATRIX.md`, the seven role folders, `chung/`, `lien-vai-tro/` and `ky-thuat-runtime/`.
- Legacy IDs are preserved in each role's `00-LEGACY-CASES.md` and mapped in the coverage matrix; do not duplicate cases in context files.

## Audit and thesis documents

- Audit index: `bao-cao/tai-lieu-ky-thuat/system-audit/00-README.md`.
- Baseline: `bao-cao/tai-lieu-ky-thuat/system-audit/01-baseline/`.
- Findings: `bao-cao/tai-lieu-ky-thuat/system-audit/06-findings/FINDINGS-REGISTER.md`.
- Evidence catalog: `bao-cao/tai-lieu-ky-thuat/system-audit/05-operations-and-thesis/EVIDENCE-INDEX.md`.
- Existing audit reports: `bao-cao/tai-lieu-ky-thuat/system-audit/01-SYSTEM-OVERVIEW.md` through `54-TOPPING-SCHEMA-DRIFT-REPAIR.md` and `CODEX-CURRENT-STATE.md`.
- Historical material: `bao-cao/archive/`, including legacy audits and archived migrations.
- General report/thesis area: `bao-cao/README.md` and its current subfolders. Reconcile claims with source before reuse.
## Current audit/checklist locations

- Employee profile source: services/api/src/Domain/Entities/Employee.cs, services/api/src/WebAPI/Controllers/EmployeeController.cs, apps/admin-web/src/features/hrm/pages/EmployeeManagement.tsx.
- Role/page checklist: ba-test-frontend/<vai-tro>/; active classified tables are in each role's 00-CASES-BY-PAGE.md; legacy files are ID lookup only.
- Employee profile cases: ba-test-frontend/chu-quan/employees.md and ba-test-frontend/quan-ly/employees.md.
- Cross-role and refresh cases: ba-test-frontend/lien-vai-tro/order-to-payment.md and ba-test-frontend/chung/refresh-session.md.
- Coverage matrix: ba-test-frontend/00-COVERAGE-MATRIX.md.
- Audit findings/evidence: bao-cao/tai-lieu-ky-thuat/system-audit/06-findings/FINDINGS-REGISTER.md and 05-operations-and-thesis/EVIDENCE-INDEX.md.
- Route/function coverage đối soát trực tiếp: ba-test-frontend/00-ROUTE-FUNCTION-MAP.md; đây là mục lục route và khoảng trống, còn các file page/00-CASES-BY-PAGE.md mới chứa bước test.
