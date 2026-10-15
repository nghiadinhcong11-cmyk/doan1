# ChatGPT Context Pack

Updated: `2026-10-15`  
Reference branch: `main`  
Reference commit: `0a480965f5a582c6da5c30fb70d9a5a2d1cd3d4e`  
Tree state: dirty; the latest context update observed 142 modified, 149 deleted and 19,517 untracked entries with `git status --porcelain=v1 -uall`. The audit baseline remains the earlier survey snapshot.

This self-contained summary is generated from the other files in `chatgpt-context/`, the current source and the audit preparation documents. Update it together with those files when shared assumptions materially change. It is a compact orientation document, not a replacement for source or `AGENTS.md`.

## System

This is a multi-branch restaurant POS and management system. Current source covers branch and catalog management, tables/reservations, staff operations, orders, kitchen workflow, payment/invoice/receipt, expenses/reporting, customer and QR ordering, loyalty, inventory, notifications, system settings, SignalR kitchen communication and a local AI assistant.

The stack is ASP.NET Core 8/C#, EF Core 8, PostgreSQL/Npgsql, JWT Bearer, SignalR, React/Vite/TypeScript/Tailwind. The applications actually present are:

- `apps/admin-web`: management, POS, staff and kitchen display routes.
- `apps/customer-web`: customer login/profile/reservation and guest/customer QR menu.
- `services/api`: layered backend with Domain, Application, Infrastructure, WebAPI and AI areas.
- `packages/shared`: Swagger snapshot and generated API types.

Current source covers staff profiles, attendance, schedules and shifts as separate staff-operation concerns. Do not infer an additional financial staff-processing module from those fields.

## Roles and security boundary

Current source uses `admin`, `manager`, `employee`, `cashier`, `kitchen` and `customer`. Admin is the global management role in the documented model. Manager and operational staff are normally branch-scoped. Customer access is limited to its own identity/session scope. Role claims and server-side checks are authoritative; frontend guards, position strings, URL parameters, local storage and client-supplied branch IDs are not security boundaries.

Relevant paths are `services/api/Program.cs`, `services/api/src/Infrastructure/Services/JwtService.cs`, `services/api/src/Application/Common/Security/RoleNames.cs`, the WebAPI controllers/services and `apps/admin-web/src/App.tsx`.

## Business flow and financial authority

The core flow is:

`QR/customer or staff → OrderController → OrderService → persisted Order → KitchenService/SignalR → POS/KDS/cashier → payment/invoice`.

Guest QR uses server-side QR/session context. `OrderService` is the authoritative owner of order financial recalculation. `Order` stores subtotal, VAT/service-fee snapshots, discount, total, payment method and payment timestamp. Payment code compares the submitted amount with persisted order total and handles already-paid/conflict outcomes in source. Loyalty and inventory have separate services and persistence paths.

These source paths do not prove runtime correctness. Payment, rounding, manipulated totals, duplicate requests, terminal status, loyalty idempotency, inventory concurrency and cross-branch behavior need matching runtime evidence.

## Realtime and AI

The backend maps `/kitchenHub`. `KitchenHub` authorizes staff roles and joins role/user/branch-related groups. `KitchenNotifier` publishes kitchen request events; `PaymentEventRouting` publishes payment completion to selected branch/role groups. Admin kitchen/POS/notification consumers use SignalR automatic reconnect. Event delivery and recovery are not runtime-verified in this pack.

The current DI registration uses `MockAiProvider` through `IAiProvider`, configured in the current appsettings mode. The mock is deterministic and does not call an external model. AI tools, registry, permission service and product-price confirmation code are real source paths, but end-to-end permission/write behavior is not verified here. No provider fallback should be inferred.

## Local configuration

The configured development endpoints are API `https://localhost:5000`, Admin `https://localhost:5173`, Customer `https://localhost:5174`, and SignalR at the API base plus `/kitchenHub`. LAN is opt-in through `DOAN_DEV_LAN_HOST` and the HTTPS scripts. Automatic database migration is configured off for the development launcher. These are configuration facts, not proof that services are running.

## Scope and guardrails

Read `AGENTS.md` before code changes. Preserve clean architecture, authentication, branch isolation, financial authority, migration history, API contracts and SignalR event names. Search consumers before changing a contract. Do not add dependencies or new architecture for “standardization” without an explicit scoped reason. Optional proposals must state benefit and cost.

Do not assume Docker, CI/CD, external AI, production database, automated test coverage, backup validity or deployment readiness. Old documents are historical until reconciled with current source. Static reading, build output and runtime/manual evidence are separate categories. Never record PASS without direct supporting evidence.

## Audit and work state

The audit preparation lives under `bao-cao/tai-lieu-ky-thuat/system-audit/`. The single findings register contains `BASE-001` for the dirty tree, `DOC-001` for the current-source/old-document migration-count discrepancy and `PAY-001` for remaining historical migration metadata. `EmployeeType` and `BasicSalary` are confirmed retained profile fields, not part of that finding. The findings do not prove database failure. `ba-test-frontend/` is organized as role/page folders; start at `README.md` and `00-COVERAGE-MATRIX.md`, with legacy IDs preserved in role files.

The latest preparation completed the audit documentation baseline, Mock-only documentation cleanup and checklist reorganization. Frontend builds passed for both web apps; backend compile target passed, while the full build was blocked at final output copy because `RestaurantPOS.api.exe`/DLL was in use by another process. This work did not start services, call an API, connect to PostgreSQL, run migrations/seeds, test SignalR, execute manual flows or validate backup/restore. No system-wide audit PASS is implied.

## Verification rule

When context conflicts with source, inspect the affected source and record the difference. Source establishes implementation; runtime evidence establishes observed behavior in a named environment. If evidence is missing, say `chưa xác minh` or `bị chặn`. Do not expose secrets, tokens, passwords, personal data, full connection strings or private certificates.

For a new session, attach this file alone to ChatGPT when repository access is unavailable. For Codex, open the repository and read `00-START-HERE.md` first, then inspect the current working tree and relevant source before acting. `PAY-001` remains open and does not claim database cleanup.
## Employee profile and checklist decision

EmployeeType and BasicSalary are retained in the employee entity, persistence/API and management UI. They are only employee profile data: the current scope does not include salary calculation, salary periods, deductions or payment. Manual cases are in ba-test-frontend/chu-quan/employees.md and ba-test-frontend/quan-ly/employees.md.

Legacy cases are classified into ba-test-frontend/<vai-tro>/00-CASES-BY-PAGE.md, lien-vai-tro/order-to-payment.md and chung/refresh-session.md. Legacy files are now ID lookup tables. The current coverage matrix is ba-test-frontend/00-COVERAGE-MATRIX.md.

PAY-001 tracks only the unresolved historical migration-chain decision and must not be used as a reason to delete EmployeeType or BasicSalary. The database was not inspected and no runtime test was run; source/build evidence does not replace runtime evidence.
## Snapshot versus migration history

The current EF model snapshot contains EmployeeType and BasicSalary on Employee, but no Payroll entity, payroll table mapping or payroll relationship. Payroll references that remain in historical migration source/designer files are not evidence that the current model or database still has those entities. PAY-001 remains open because migration-chain replay/baseline consequences have not been decided. No migration, snapshot or database was changed.

Detailed route/function coverage is in ba-test-frontend/00-ROUTE-FUNCTION-MAP.md. NET-01..NET-05 and TECH-01..TECH-05 remain separate manual/technical cases, all Chưa test.
Coverage hiện được đối soát theo route thực tế trong ba-test-frontend/00-ROUTE-FUNCTION-MAP.md. Tổng ID duy nhất là 183: 143 theo vai trò, 17 FLOW, 7 REF, 5 NET, 5 TECH và 6 case EMP-PROFILE. Việc phân loại case cũ không có nghĩa mọi route/chức năng đã có case; route map ghi rõ khoảng trống.
Checklist gap completion added explicit cases for /customers, /support, /kitchen/tables, inventory/HR/settings subpages and guest direct /reservation/profile routes. New IDs are ADM-CUS-01..09, ADM-SUP-01..03, KIT-TBL-01..04, INV-SUB-01..08, HR-SUB-01..08, SET-SUB-01..08 and KQR-01..06. The unique-ID total is now 229; shared profile cases are counted once.

COV-001 records that source does not establish whether a QR guest may use reservation/profile routes. The checklist records both unauthenticated and guest-session behavior as questions to verify; it does not choose a product policy. All new cases remain Chưa test.
