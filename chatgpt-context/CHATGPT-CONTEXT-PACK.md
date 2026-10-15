# ChatGPT Context Pack

Updated: `2026-10-15`  
Reference branch: `main`  
Reference commit: `4abeb1bf2e646df837745713a495b164eed60488`
Tree state: clean at the start of the current static review; `main` was ahead of `origin/main` by one commit. The review's documentation changes are uncommitted.

Current session snapshot: HEAD `1ef623e94cea83f150006c9a22a10126611a171c` (`1ef623e9`) on `main`; working tree was clean before the owner employee navigation review and now contains only the documentation/checklist changes listed in that review. No application runtime or database operation was performed.

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

`QR/customer or staff Ă¢â€ â€™ OrderController Ă¢â€ â€™ OrderService Ă¢â€ â€™ persisted Order Ă¢â€ â€™ KitchenService/SignalR Ă¢â€ â€™ POS/KDS/cashier Ă¢â€ â€™ payment/invoice`.

Guest QR uses server-side QR/session context. `OrderService` is the authoritative owner of order financial recalculation. `Order` stores subtotal, VAT/service-fee snapshots, discount, total, payment method and payment timestamp. Payment code compares the submitted amount with persisted order total and handles already-paid/conflict outcomes in source. Loyalty and inventory have separate services and persistence paths.

These source paths do not prove runtime correctness. Payment, rounding, manipulated totals, duplicate requests, terminal status, loyalty idempotency, inventory concurrency and cross-branch behavior need matching runtime evidence.

## Realtime and AI

The backend maps `/kitchenHub`. `KitchenHub` authorizes staff roles and joins role/user/branch-related groups. `KitchenNotifier` publishes kitchen request events; `PaymentEventRouting` publishes payment completion to selected branch/role groups. Admin kitchen/POS/notification consumers use SignalR automatic reconnect. Event delivery and recovery are not runtime-verified in this pack.

The current DI registration uses `MockAiProvider` through `IAiProvider`, configured in the current appsettings mode. The mock is deterministic and does not call an external model. AI tools, registry, permission service and product-price confirmation code are real source paths, but end-to-end permission/write behavior is not verified here. No provider fallback should be inferred.

## Local configuration

The configured development endpoints are API `https://localhost:5000`, Admin `https://localhost:5173`, Customer `https://localhost:5174`, and SignalR at the API base plus `/kitchenHub`. LAN is opt-in through `DOAN_DEV_LAN_HOST` and the HTTPS scripts. Automatic database migration is configured off for the development launcher. These are configuration facts, not proof that services are running.

## Scope and guardrails

Read `AGENTS.md` before code changes. Preserve clean architecture, authentication, branch isolation, financial authority, migration history, API contracts and SignalR event names. Search consumers before changing a contract. Do not add dependencies or new architecture for Ă¢â‚¬Å“standardizationĂ¢â‚¬Â without an explicit scoped reason. Optional proposals must state benefit and cost.

Do not assume Docker, CI/CD, external AI, production database, automated test coverage, backup validity or deployment readiness. Old documents are historical until reconciled with current source. Static reading, build output and runtime/manual evidence are separate categories. Never record PASS without direct supporting evidence.

## Audit and work state

The audit preparation lives under `bao-cao/tai-lieu-ky-thuat/system-audit/`. The single findings register contains `BASE-001` for the dirty tree, `DOC-001` for the current-source/old-document migration-count discrepancy and `PAY-001` for remaining historical migration metadata. `EmployeeType` and `BasicSalary` are confirmed retained profile fields, not part of that finding. The findings do not prove database failure. `ba-test-frontend/` is organized as role/page folders; start at `README.md` and `00-COVERAGE-MATRIX.md`, with legacy IDs preserved in role files.

The latest preparation completed the audit documentation baseline, Mock-only documentation cleanup and checklist reorganization. Frontend builds passed for both web apps; backend compile target passed, while the full build was blocked at final output copy because `RestaurantPOS.api.exe`/DLL was in use by another process. This work did not start services, call an API, connect to PostgreSQL, run migrations/seeds, test SignalR, execute manual flows or validate backup/restore. No system-wide audit PASS is implied.

The current static security review is documented in `bao-cao/tai-lieu-ky-thuat/system-audit/02-static-review/SECURITY-AND-BRANCH-REVIEW.md`. It inventories controller actions, `/kitchenHub` groups/producers and Mock AI tools. Findings `SEC-001..SEC-005` are open and distinguish source-confirmed behavior from policy ambiguity and runtime risk. Customer profile/loyalty scope, Product availability global-vs-branch behavior and missing-order-branch SignalR fallback need an explicit decision or isolated runtime evidence.

## Verification rule

When context conflicts with source, inspect the affected source and record the difference. Source establishes implementation; runtime evidence establishes observed behavior in a named environment. If evidence is missing, say `chĂ†Â°a xÄ‚Â¡c minh` or `bĂ¡Â»â€¹ chĂ¡ÂºÂ·n`. Do not expose secrets, tokens, passwords, personal data, full connection strings or private certificates.

For a new session, attach this file alone to ChatGPT when repository access is unavailable. For Codex, open the repository and read `00-START-HERE.md` first, then inspect the current working tree and relevant source before acting. `PAY-001` remains open and does not claim database cleanup.
## Employee profile and checklist decision

EmployeeType and BasicSalary are retained in the employee entity, persistence/API and management UI. They are only employee profile data: the current scope does not include salary calculation, salary periods, deductions or payment. Manual cases are in ba-test-frontend/chu-quan/employees.md and ba-test-frontend/quan-ly/employees.md.

Legacy cases are classified into ba-test-frontend/<vai-tro>/00-CASES-BY-PAGE.md, lien-vai-tro/order-to-payment.md and chung/refresh-session.md. Legacy files are now ID lookup tables. The current coverage matrix is ba-test-frontend/00-COVERAGE-MATRIX.md.

PAY-001 tracks only the unresolved historical migration-chain decision and must not be used as a reason to delete EmployeeType or BasicSalary. The database was not inspected and no runtime test was run; source/build evidence does not replace runtime evidence.
## Snapshot versus migration history

The current EF model snapshot contains EmployeeType and BasicSalary on Employee, but no Payroll entity, payroll table mapping or payroll relationship. Payroll references that remain in historical migration source/designer files are not evidence that the current model or database still has those entities. PAY-001 remains open because migration-chain replay/baseline consequences have not been decided. No migration, snapshot or database was changed.

Detailed route/function coverage is in ba-test-frontend/00-ROUTE-FUNCTION-MAP.md. NET-01..NET-05 and TECH-01..TECH-05 remain separate manual/technical cases, all ChĂ†Â°a test.
Coverage hiĂ¡Â»â€¡n Ă„â€˜Ă†Â°Ă¡Â»Â£c Ă„â€˜Ă¡Â»â€˜i soÄ‚Â¡t theo route thĂ¡Â»Â±c tĂ¡ÂºÂ¿ trong ba-test-frontend/00-ROUTE-FUNCTION-MAP.md. TĂ¡Â»â€¢ng ID duy nhĂ¡ÂºÂ¥t lÄ‚Â  183: 143 theo vai trÄ‚Â², 17 FLOW, 7 REF, 5 NET, 5 TECH vÄ‚Â  6 case EMP-PROFILE. ViĂ¡Â»â€¡c phÄ‚Â¢n loĂ¡ÂºÂ¡i case cĂ…Â© khÄ‚Â´ng cÄ‚Â³ nghĂ„Â©a mĂ¡Â»Âi route/chĂ¡Â»Â©c nĂ„Æ’ng Ă„â€˜Ä‚Â£ cÄ‚Â³ case; route map ghi rÄ‚Âµ khoĂ¡ÂºÂ£ng trĂ¡Â»â€˜ng.
Checklist gap completion added explicit cases for /customers, /support, /kitchen/tables, inventory/HR/settings subpages and guest direct /reservation/profile routes. New IDs are ADM-CUS-01..09, ADM-SUP-01..03, KIT-TBL-01..04, INV-SUB-01..08, HR-SUB-01..08, SET-SUB-01..08 and KQR-01..06. The unique-ID total is now 229; shared profile cases are counted once.

COV-001 records that source does not establish whether a QR guest may use reservation/profile routes. The checklist records both unauthenticated and guest-session behavior as questions to verify; it does not choose a product policy. All new cases remain ChĂ†Â°a test.
## Static security review handoff

The security review is source-only at reference HEAD `4abeb1bf2e646df837745713a495b164eed60488`. It found layered JWT/claims, role, resource and branch checks, but no runtime proof. Open findings are `SEC-001..SEC-005` in the single findings register. They cover customer lookup/privacy scope, customer loyalty/profile branch policy, global Product availability/broadcast, missing-order-branch kitchen notification fallback and non-centralized AI branch validation. Do not call these runtime vulnerabilities or PASS without the requested isolated tests.

Manual/API cases are in `ba-test-frontend/ky-thuat-runtime/README.md`: `AUTH-01..04`, `BRANCH-01..04`, `SIG-01..02`, `AI-01..03`, all `ChĂ†Â°a test`. Guest direct-route cases `KQR-01..06` remain linked to `COV-001`. No API, browser, database, migration, seed, SignalR or business transaction was run.



## Current implementation and verification update â€” 2026-10-15

Customer records and loyalty are chain-wide by decision. Staff lookup responses are minimized in CustomerController; operational staff are not granted full profile mutation. Product catalog remains shared while availability is per branch through ProductBranchAvailability; a reviewed but unapplied migration seeds legacy availability into each product/branch pair. Order creation, acceptance, payment, status processing and kitchen submission now require an active branch, and invalid-branch realtime business notifications are suppressed. These are source changes awaiting runtime/database evidence.

The guest /profile spinner was caused by CustomerProfile starting with loading=true and never clearing it when a guest had no customerInfo; the guest CTA also used logout behavior. App.tsx and CustomerProfile.tsx now detect guest sessions, finish loading, show login invitation and preserve guest QR/session/cart state. Customer profile API calls are skipped for guest sessions. Direct route and login-return cases remain ChÆ°a test.

New source paths: ProductBranchAvailability.cs, ApplicationDbContext.cs, ProductController.cs, ProductService.cs, OrderService.cs, KitchenService.cs, OrderController.cs, CustomerController.cs, customer App.tsx, CustomerProfile.tsx, POS/Kitchen/DigitalMenu callers, and 20261015051548_AddProductBranchAvailability.cs.

No database, migration apply, runtime/API/browser/SignalR test or transaction was executed. The migration must be applied only in an approved isolated database, then availability transition and two-branch behavior must be verified. SEC-001..SEC-004 are source-updated and awaiting retest; SEC-005 remains open as defense-in-depth; COV-001 is split between fixed profile policy and open reservation policy. MockAiProvider, EmployeeType and BasicSalary remain unchanged in scope.




## Static diff review update — 2026-10-15

The follow-up static review is recorded in bao-cao/tai-lieu-ky-thuat/system-audit/02-static-review/GUEST-CUSTOMER-AVAILABILITY-ORDER-DIFF-REVIEW.md. The actual HEAD remains 4abeb1bf2e646df837745713a495b164eed60488 and the working tree is dirty; not every modified file belongs to the feature diff.

Latest source-fix update: public registration now uses `CustomerRegistrationDto` with only full name, phone and password; kitchen request status validates request/order, active branch and legal transition before mutation; branch-aware availability reads use the branch row or explicit `Available` when missing; and KitchenPage filters availability events by selected branch and refetches after reconnect. The migration has explicit EF context/migration metadata but has not been applied. SEC-003, SEC-004 and SEC-006 remain open pending runtime or isolated-migration evidence. Inactive-branch open-order recovery remains a business decision. No database, API, browser, SignalR or transaction was run.

EF handoff: against a rebuilt isolated assembly, `dotnet-ef 8.0.31 migrations list --no-connect` discovered `20261015051548_AddProductBranchAvailability`; `has-pending-model-changes` reported no pending model changes; and offline SQL generation succeeded. The missing conventional `BuildTargetModel` designer did not block discovery/script generation because direct migration metadata and `Up`/`Down` are present, but it remains a tooling-pattern difference for future scaffolding review. POS and DigitalMenu have no availability SignalR consumer; only KitchenPage does, so live coverage is incomplete. The `Available` missing-row fallback remains pending user confirmation.

Important residual findings:

- Customer profile ownership and restricted profile DTO fields are source-protected, but public Customer registration still binds the domain entity and does not reset CustomerGroup. SEC-006 is open; do not claim the registration boundary is safe until a registration DTO/allowlist is implemented.
- Branch availability uses ProductBranchAvailability with composite key and branch-scoped mutation. Missing rows still expose legacy global Product.AvailabilityStatus, and no frontend ProductAvailabilityChanged consumer was found. SEC-003 remains open for fallback/lifecycle/realtime verification.
- Order branch checks cover create, accept, status, payment and send-to-kitchen, but KitchenService updates request status before validating the joined order branch. SEC-004 remains open for validation-before-mutation.
- The migration creates and seeds product/branch rows but has not been applied. It includes inactive branches, does not normalize invalid legacy status strings and has no generated designer file in the current uncommitted source. Schema/data behavior remains unverified.
- Guest profile static flow is now bounded and avoids profile API calls for guest; direct route, login transition and 401/403/network behavior remain untested. Guest reservation policy remains undecided.

New manual/runtime cases: AUTH-06, BRANCH-08 and SIG-04, all Chưa test. MockAiProvider, EmployeeType and BasicSalary remain in scope unchanged.

## Owner employee navigation static review — 2026-10-15

The owner/management “Nhân viên” navigation was statically audited at HEAD `1ef623e9`. Its actual management scope is `/employees`, `/attendance`, `/schedule` and `/shifts`; staff-specific HR routes are separate. The report is `bao-cao/tai-lieu-ky-thuat/system-audit/02-static-review/OWNER-EMPLOYEE-NAV-REVIEW.md`.

New open findings are `HR-001` (no admin self-toggle/self-delete protection), `HR-002` (HR page API errors are not consistently exposed or response-checked) and `HR-003` (admin employee branch assignment lacks explicit active-branch validation). Manual cases `ADM-EMP-01..05` were added to `ba-test-frontend/chu-quan/employees.md`; existing navigation/direct-URL and shared HR cases were reused. All cases remain `Chưa test`. No application code, migration, snapshot, runtime or database operation was performed. `EmployeeType`, `BasicSalary` and `MockAiProvider` remain unchanged.
- Owner navigation HR source-fix update (2026-10-15): HR-001 self-disable/self-delete is blocked server-side from authenticated identity and disabled in the owner UI; HR-002 status/error/loading/retry handling is fixed across `/employees`, `/attendance`, `/schedule`, `/shifts`; HR-003 validates active branch on create and actual branch transfer while allowing profile edits on an existing inactive branch. Findings remain runtime-pending and manual cases remain `Chưa test`. Admin build passed; backend isolated-output build passed. No runtime/API/browser/database/migration/seed was run.

Static review of that HR diff found residual source gaps HR-004 (self-disable via entity-shaped PUT), HR-005 (omitted versus null BranchId cannot be distinguished), and HR-006 (handler-level duplicate-submit and mutation/refetch outcome distinction). Page-specific cases `ADM-ATT-01..03`, `ADM-SCH-01..04`, `ADM-SHF-01..03` and employee cases `ADM-EMP-06..07` were added; all remain `Chưa test`. HR handoff is not closed.
- HR residual-fix update: `EmployeeUpdateRequest` preserves omitted BranchId, distinguishes explicit null/ID, validates active transfers before mutation, and prevents self active→inactive PUT. Ref-backed in-flight guards and explicit refetch-failure feedback cover owner HR pages. HR-004/005/006 are merged into HR-001/003/002 respectively; all remain runtime-pending. New page-specific cases remain `Chưa test`.

HR handoff clarification (2026-10-15): actor and target roles are separate in branch validation. Manager actors remain constrained to the authenticated JWT branch; admin actors may currently assign a branchless normalized staff target, including `manager`, because the source has no existing admin target-role branch-required rule. Omitted BranchId preserves, same ID preserves even when inactive, different non-null ID requires an active existing branch, and explicit null removes the branch under current admin behavior. No historical attendance, schedule, shift or order links are changed. Owner `/attendance` and `/shifts` are read/refresh-only; attendance check-in/out and shift open/close writes are separate staff/POS endpoints outside this page scope. HR-001..003 remain source-fixed/runtime-pending; all manual cases remain `Chưa test`.
