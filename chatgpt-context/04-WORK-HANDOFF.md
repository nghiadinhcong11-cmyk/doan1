# Work Handoff

## Current objective

Perform the static authentication, authorization and branch-isolation review and preserve the evidence boundary for later runtime tests.

## Recently completed

- Surveyed the repository and existing `ba-test-frontend/`, audit, evidence and diagnostic material.
- Established the audit preparation index and source-of-truth rule under `bao-cao/tai-lieu-ky-thuat/system-audit/`.
- Added the audit baseline, scope/environment, evidence index and single findings register.
- Recorded `BASE-001` for the dirty working tree and `DOC-001` for the source/document migration-count discrepancy.
- Created this `chatgpt-context/` pack for future sessions.
- Confirmed current AI registration is `IAiProvider` Ă¢â€ â€™ `MockAiProvider` and updated current context/audit guidance to the Mock-only boundary.
- Reorganized `ba-test-frontend/` into role/page folders, copied legacy case content into role-indexed files, added coverage/runtime/cross-role guides and replaced old root checklists with links.
- Removed an obsolete audit/archive document that could be deleted without touching canonical migration history.

These actions are documentation preparation and static review only. They do not mean the system passed an audit, build or runtime test.

## Not completed

- No runtime API/browser/database session, migration, seed, SignalR test, performance test or backup/restore was run.
- Canonical EF migration files and model snapshot were not rewritten because several migration/designer files contain mixed historical schema steps; database state is not claimed to be cleaned.
- No finding was closed and no feature was certified.
- At the start of the current static review the working tree was clean; the documentation changes from this review are now uncommitted user changes.

## Open issues

- `BASE-001`: current working tree is not a reproducible clean snapshot. See `bao-cao/tai-lieu-ky-thuat/system-audit/06-findings/FINDINGS-REGISTER.md`.
- `DOC-001`: current source migration count differs from an older state document. This is a verified documentation/source-count discrepancy, not a database failure conclusion.
- Existing evidence has not been revalidated against the current working tree.
- `PAY-001`: employee profile fields are a confirmed retained decision; only historical migration metadata remains under separate review.

## Decisions already made

- Keep one active checklist location under `ba-test-frontend/`; legacy copies are preserved only for ID/evidence mapping.
- Use current executable source as the first reference when documents conflict.
- Keep findings in one register and distinguish execution status from conclusion.
- Treat current AI registration as Mock AI unless source/configuration proves otherwise.
- Keep runtime and database claims unverified until evidence is generated in an approved test environment.
- Keep customer lookup/loyalty scope, product availability scope and missing-branch SignalR fallback as explicit policy/runtime questions; do not infer a manager global scope.

## Suggested next steps

The following are suggestions, not new user instructions:

1. Freeze or name a review snapshot for the audit.
2. Perform static review of payment/order state and migration/model alignment after this security review.
3. Run only the approved build/typecheck/codegen checks and record their evidence.
4. Prepare an isolated runtime plan before any API/database/SignalR verification.

## Handoff metadata

Last updated: `2026-10-15`  
Reference branch: `main`  
Reference commit: `4abeb1bf2e646df837745713a495b164eed60488`
Reference tree: clean at review start; `main` was ahead of `origin/main` by one commit. The current review changes are intentionally not committed.

## Source-fix handoff — 2026-10-15

- SEC-006: public customer registration now uses `CustomerRegistrationDto` with only full name, phone and password. Server owns group/rank, loyalty, spending, order count, activity and timestamps; frontend registration sends the allowlisted contract.
- SEC-004: kitchen request status validates status, request/order, active branch and legal transition before mutation. Invalid transitions are returned as 400; no runtime proof yet.
- SEC-003: branch-aware availability reads use the `(ProductId, BranchId)` row and explicit `Available` fallback when missing. Legacy `Products.AvailabilityStatus` remains migration/legacy data, not branch operational state. Kitchen consumes branch-scoped availability events and refetches after reconnect.
- Migration `20261015051548_AddProductBranchAvailability.cs` has explicit EF context/migration metadata. It has not been applied; no database or migration history was changed.
- Inactive-branch open-order recovery remains a business decision. Current source blocks create/accept/payment/status/send-to-kitchen when the branch is inactive, so an existing open order may require reactivation or an approved administrative recovery path.

All related findings remain open as “Đã sửa source, chờ kiểm chứng runtime/migration”; build success is not a PASS. Manual cases remain `Chưa test`.

## Migration/tooling handoff — 2026-10-15

- EF tooling was run against a rebuilt isolated assembly, with `--no-connect`; no database, seed or application was run.
- `migrations list` discovered `20261015051548_AddProductBranchAvailability`.
- `has-pending-model-changes` reported no model changes pending relative to the current snapshot.
- Offline SQL generation succeeded and was inspected without committing the generated output. It contains the new table, composite key, cascade FKs, index, cross-join seed and migration-history insert.
- Direct `DbContext`/`Migration` metadata is sufficient for discovery and script generation. The migration still has no conventional generated `BuildTargetModel` designer; this did not block current tooling, but future migration scaffolding should use the normal project tooling and review the target model.
- Branch-missing availability currently returns `Available`; this is implementation behavior pending user confirmation, not a finalized business policy.
- POS and DigitalMenu do not subscribe to `ProductAvailabilityChanged`; they refresh through existing fetch/reload flows. Kitchen realtime does not cover all frontend consumers.

Next session: decide the missing-row default and inactive-branch order recovery policy, run migration/runtime tests in an approved isolated environment, and test POS/customer availability live-vs-reload behavior.
## Latest handoff update

- Confirmed decision: retain EmployeeType and BasicSalary as employee profile fields in source/API/frontend; no salary-processing module is implied.
- Added employee profile checklist cases for create, edit, save, reload, display, current form validation and branch authorization.
- Moved role legacy case tables into active role files 00-CASES-BY-PAGE.md; legacy files now provide ID lookup. Rebuilt the cross-role and refresh tables in their active files.
- Updated the coverage matrix with route/page/function/case/file/gap columns.
- PAY-001 remains open for historical migration-chain review only. No database was connected or modified.
- No runtime test was executed. The next static review should validate links, residual historical migration references and the role/page map before manual runtime work.

## Authentication and branch-isolation review update

- Added `bao-cao/tai-lieu-ky-thuat/system-audit/02-static-review/SECURITY-AND-BRANCH-REVIEW.md` with controller/action, SignalR and AI tool inventories, visible controls, evidence limits and policy questions.
- Added findings `SEC-001` through `SEC-005`. They remain open; adding manual cases does not close them.
- Added technical/API cases `AUTH-01..04`, `BRANCH-01..04`, `SIG-01..02` and `AI-01..03` in `ba-test-frontend/ky-thuat-runtime/README.md`. All remain `ChĂ†Â°a test`.
- Static review did not run API, browser, database, migration, seed, SignalR or business transactions. MockAiProvider remains primary; EmployeeType and BasicSalary remain profile fields.
## Reconciliation update

- The current model snapshot contains EmployeeType and BasicSalary only for Employee; no Payroll entity/table/relationship mapping was found in the snapshot.
- Historical migration source/designer references remain and are tracked by PAY-001. No migration, snapshot, database or EF history was changed.
- Route/function coverage is now maintained in ba-test-frontend/00-ROUTE-FUNCTION-MAP.md; the coverage matrix remains the summary index.
- NET-01..NET-05 and TECH-01..TECH-05 remain separate, unchanged, and ChĂ†Â°a test.
## Checklist gap-completion update

- Added page checklists for admin /customers and /support, kitchen /kitchen/tables, inventory/HR/settings subpages, and guest direct /reservation and /profile.
- Added COV-001 because source renders guest/customer shell behavior but does not establish the intended guest policy for reservation/profile data.
- Added case IDs ADM-CUS-01..09, ADM-SUP-01..03, KIT-TBL-01..04, INV-SUB-01..08, HR-SUB-01..08, SET-SUB-01..08 and KQR-01..06.
- Updated route-function map and coverage matrix. Cases remain ChĂ†Â°a test; this is checklist preparation, not runtime verification.
- EmployeeType, BasicSalary, MockAiProvider and migration history remain unchanged.



## Implementation handoff update â€” 2026-10-15

### Completed in source

- Fixed guest profile infinite loading by recognizing guest session modes, ending profile loading immediately, showing a login invitation, and avoiding customer profile API calls for guests.
- Applied source-level customer/loyalty least-privilege response changes, per-branch product availability model/controller callers, and active-branch order processing checks.
- Added source migration 20261015051548_AddProductBranchAvailability.cs; it is not applied and the database was not connected.
- Added manual/runtime follow-up cases KQR-07..08, AUTH-05, BRANCH-05..07, and SIG-03; all remain ChÆ°a test.

### Not completed

- No API, browser, SignalR, database, migration apply, seed or business transaction was run.
- Customer reservation guest policy remains open. Runtime confirmation of customer scope, availability, branch invariant and guest profile behavior remains required.
- Migration history/database state was not cleaned or changed. PAY-001 remains historical migration-chain review only.

### Current verification

- Isolated backend build succeeded with no compile errors in .artifacts/api; normal output was previously blocked by a locked running API output. Customer and admin frontend production builds also completed; none of these builds is runtime evidence.
- Working tree contains pre-existing and current uncommitted changes; no commit/push was performed.




## Static diff review follow-up — 2026-10-15

- Reviewed the actual HEAD and dirty diff; no application code was changed in this review.
- New static report: bao-cao/tai-lieu-ky-thuat/system-audit/02-static-review/GUEST-CUSTOMER-AVAILABILITY-ORDER-DIFF-REVIEW.md.
- Customer profile DTO ownership checks are present, but public Customer entity binding leaves CustomerGroup client-controlled on new registration. This is SEC-006 and needs a DTO/allowlist fix before acceptance.
- Availability is branch-scoped in the new model, but missing rows fall back to global Product.AvailabilityStatus; Product/Branch creation does not initialize rows, and no ProductAvailabilityChanged frontend consumer was found. This remains SEC-003 residual risk.
- Kitchen request status suppresses invalid-branch broadcast but mutates OrderRequest before validating the joined order branch. SEC-004 remains open until ordering is corrected.
- Migration source was reviewed only; explicit `DbContext`/`Migration` metadata was added to the uncommitted migration, but no generated `BuildTargetModel` designer or database/schema application was performed.
- Added AUTH-06, BRANCH-08 and SIG-04. All remain Chưa test. Guest reservation policy remains open.
## Owner employee navigation static review — 2026-10-15

- Audited the actual admin/management “Nhân viên” navigation scope: `/employees`, `/attendance`, `/schedule`, `/shifts`.
- Static report: `bao-cao/tai-lieu-ky-thuat/system-audit/02-static-review/OWNER-EMPLOYEE-NAV-REVIEW.md`.
- Added findings `HR-001..HR-003` to the single findings register. They remain open; no runtime, API, database or migration test was run.
- Added owner manual cases `ADM-EMP-01..05` in `ba-test-frontend/chu-quan/employees.md`; reused existing `ADM-11`, `ADM-17` and shared `HR-SUB-03..05`. All remain `Chưa test`.
- `EmployeeType` and `BasicSalary` remain profile-only fields. No payroll behavior was added or inferred.
- No application code, migration, snapshot, `AGENTS.md` or `AI_RULES.md` was changed.
- Owner navigation HR source-fix update (2026-10-15): HR-001 now blocks self-disable/self-delete from authenticated identity in `EmployeeController` and disables only those UI actions; self-edit remains available.
- HR-002 now checks response status and exposes safe error/loading feedback across `/employees`, `/attendance`, `/schedule`, `/shifts`, with pending guards for writes/refreshes.
- HR-003 now validates active branch on create and actual non-null branch transfer while allowing profile edits on an existing inactive branch.
- HR-001..003 are `Đã sửa source, chờ kiểm chứng runtime`; `ADM-EMP-01..05` remain `Chưa test`. Admin build passed; isolated backend build passed; no API/browser/database/migration/seed operation ran.

## Owner navigation HR static fix review — 2026-10-15

- Reviewed the actual HR application diff separately from pre-existing documentation changes.
- Confirmed JWT source mapping: `AuthController` passes `employee.Id` to `JwtService`, which writes `ClaimTypes.NameIdentifier`; backend self checks compare the employee key, while localStorage is UX-only.
- Residual findings: HR-004 self-disable through entity-shaped `PUT`; HR-005 omitted/null `BranchId` ambiguity; HR-006 handler-level duplicate-submit and mutation-success/refetch-failure distinction.
- Added page-specific owner cases: `ADM-ATT-01..03`, `ADM-SCH-01..04`, `ADM-SHF-01..03`; all remain `Chưa test`. `ADM-EMP-06..07` cover the HR-004/005 paths.
- HR handoff is not closed. No source, API, browser, runtime, database, migration or build operation was performed in this review.
- HR residuals fixed in source: `UpdateEmployee` now uses allow-listed `EmployeeUpdateRequest`, blocks self active→inactive PUT, distinguishes omitted/null/ID `BranchId`, preserves manager branch rules and checks active transfer before mutation. Employee/schedule mutations and attendance/shifts refreshes now have ref-backed in-flight guards; successful mutations report refetch failure separately.
- HR-004 is merged into HR-001, HR-005 into HR-003, HR-006 into HR-002. All three parent findings are `Đã sửa source, chờ kiểm chứng runtime`; no runtime/API/browser/database/migration/seed was run.
- Added `ADM-EMP-06..08`, `ADM-ATT-01..03`, `ADM-SCH-01..04`, `ADM-SHF-01..03`; all remain `Chưa test`. HR handoff is pending runtime verification.

## HR handoff clarification and closeout preparation - 2026-10-15

- Actor and target roles are separate in branch validation. Manager actors remain constrained to the authenticated JWT branch; admin actors may currently assign a branchless normalized staff target, including `manager`, because no existing admin target-role branch-required rule is present. This is preserved policy, not a new source defect.
- BranchId semantics are explicit: omitted preserves, same ID preserves including an inactive current branch, different non-null ID requires an active existing branch, and explicit null removes the branch under current admin behavior. Role and final branch are checked before mutation; historical links are untouched.
- Owner `/attendance` and `/shifts` are read/refresh-only. Attendance check-in/out and shift open/close endpoints exist in staff/POS controllers outside this owner-nav scope and were not runtime-tested as owner-page mutations.
- Owner writes reviewed are employee create/update/status-toggle/delete and schedule create/delete. Their source handlers include response checks, ref-backed in-flight protection and distinct mutation-success/refetch-failure feedback. Manual cases remain `Chưa test`.
- HR-001..003 are `Đã sửa source, chờ kiểm chứng runtime`; HR-004/005/006 are merged into those parent findings. Admin frontend and isolated backend builds passed; no API, browser, database, migration, seed or runtime operation was run. This is not runtime PASS or finding closure.
