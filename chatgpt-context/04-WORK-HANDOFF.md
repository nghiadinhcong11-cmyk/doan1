# Work Handoff

## Current objective

Consolidate the current AI boundary, remove obsolete provider/staff-processing documentation where safe, and organize manual verification by role/page while preserving user changes and migration history.

## Recently completed

- Surveyed the repository and existing `ba-test-frontend/`, audit, evidence and diagnostic material.
- Established the audit preparation index and source-of-truth rule under `bao-cao/tai-lieu-ky-thuat/system-audit/`.
- Added the audit baseline, scope/environment, evidence index and single findings register.
- Recorded `BASE-001` for the dirty working tree and `DOC-001` for the source/document migration-count discrepancy.
- Created this `chatgpt-context/` pack for future sessions.
- Confirmed current AI registration is `IAiProvider` → `MockAiProvider` and updated current context/audit guidance to the Mock-only boundary.
- Reorganized `ba-test-frontend/` into role/page folders, copied legacy case content into role-indexed files, added coverage/runtime/cross-role guides and replaced old root checklists with links.
- Removed an obsolete audit/archive document that could be deleted without touching canonical migration history.

These actions are documentation preparation only. They do not mean the system passed an audit, build or runtime test.

## Not completed

- No runtime API/browser/database session, migration, seed, SignalR test, performance test or backup/restore was run.
- Canonical EF migration files and model snapshot were not rewritten because several migration/designer files contain mixed historical schema steps; database state is not claimed to be cleaned.
- No finding was closed and no feature was certified.
- The working tree remains user-owned and not clean.

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

## Suggested next steps

The following are suggestions, not new user instructions:

1. Freeze or name a review snapshot for the audit.
2. Perform static review of authorization/branch isolation, payment/order state, migration/model alignment and AI permission paths.
3. Run only the approved build/typecheck/codegen checks and record their evidence.
4. Prepare an isolated runtime plan before any API/database/SignalR verification.

## Handoff metadata

Last updated: `2026-10-15`  
Reference branch: `main`  
Reference commit: `0a480965f5a582c6da5c30fb70d9a5a2d1cd3d4e`  
Reference tree: dirty; the latest context update observed 142 modified, 149 deleted and 19,517 untracked entries; see `01-baseline/REPOSITORY-SNAPSHOT.md` for the earlier audit snapshot.
## Latest handoff update

- Confirmed decision: retain EmployeeType and BasicSalary as employee profile fields in source/API/frontend; no salary-processing module is implied.
- Added employee profile checklist cases for create, edit, save, reload, display, current form validation and branch authorization.
- Moved role legacy case tables into active role files 00-CASES-BY-PAGE.md; legacy files now provide ID lookup. Rebuilt the cross-role and refresh tables in their active files.
- Updated the coverage matrix with route/page/function/case/file/gap columns.
- PAY-001 remains open for historical migration-chain review only. No database was connected or modified.
- No runtime test was executed. The next static review should validate links, residual historical migration references and the role/page map before manual runtime work.
## Reconciliation update

- The current model snapshot contains EmployeeType and BasicSalary only for Employee; no Payroll entity/table/relationship mapping was found in the snapshot.
- Historical migration source/designer references remain and are tracked by PAY-001. No migration, snapshot, database or EF history was changed.
- Route/function coverage is now maintained in ba-test-frontend/00-ROUTE-FUNCTION-MAP.md; the coverage matrix remains the summary index.
- NET-01..NET-05 and TECH-01..TECH-05 remain separate, unchanged, and Chưa test.
## Checklist gap-completion update

- Added page checklists for admin /customers and /support, kitchen /kitchen/tables, inventory/HR/settings subpages, and guest direct /reservation and /profile.
- Added COV-001 because source renders guest/customer shell behavior but does not establish the intended guest policy for reservation/profile data.
- Added case IDs ADM-CUS-01..09, ADM-SUP-01..03, KIT-TBL-01..04, INV-SUB-01..08, HR-SUB-01..08, SET-SUB-01..08 and KQR-01..06.
- Updated route-function map and coverage matrix. Cases remain Chưa test; this is checklist preparation, not runtime verification.
- EmployeeType, BasicSalary, MockAiProvider and migration history remain unchanged.
