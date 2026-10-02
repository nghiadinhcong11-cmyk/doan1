# Docker and Payroll Removal Report

Audit date: 2026-09-30

## 1. Docker files removed

Removed from the current repository:

- `docker-compose.yml`
- `services/api/Dockerfile`
- `services/api/.dockerignore`

Docker was not a runtime dependency. The compose file only launched the API and expected an external PostgreSQL/Supabase connection; it did not provide PostgreSQL itself.

## 2. Docker references

Current deployment documentation now states Docker is removed/not used. Remaining references are intentional:

- `AGENTS.md`: generic tooling/configuration guidance; requires human review if repository-wide instruction cleanup is desired.
- `docs/system-audit/02-TECH-STACK.md`, `12-DEPLOYMENT.md`, `17-DOCUMENTATION-CONFLICTS.md`, `18-THESIS-CONSISTENCY-AUDIT.md`: explain the removal or classify old claims.
- `baocaodoan/`: updated current thesis material explains Docker is outside the current deployment; this is not a Docker dependency.
- `services/api/src/Program.cs`: “container” is a generic ASP.NET hosting comment, not a Docker reference.
- `docs/archive/`: historical records were not rewritten.

No GitHub Actions workflow, GitHub deployment manifest, test container, or PostgreSQL container reference was found. Build pipelines use direct `dotnet`/`npm` commands.

## 3. Payroll residuals discovered

The active source contained obsolete one-line stubs, not working Payroll behavior:

- `PayrollController.cs`
- `PayrollService.cs`
- `IPayrollService.cs`
- `Payroll.cs`
- `PayrollAdjustment.cs`
- `PayrollSettings.cs`
- `EmployeeSalaryProfile.cs`
- `apps/admin-web/src/features/hrm/pages/PayrollPage.tsx`
- `tests/RestaurantPOS.Tests/SalaryConsistencyTests.cs`

These files had no implementation and no active consumers. Attendance, Shift and WorkSchedule were checked separately and retained.

## 4. Payroll source/UI removal

Removed the obsolete source/UI/test stubs above. Removed the Payroll-only `BasicSalary` and `EmployeeType` fields from the current `Employee` entity and corresponding Employee Management UI/form/API contract artifacts. No Payroll endpoint, route, sidebar item, or current frontend import remains.

The current `ApplicationDbContext` has no Payroll DbSet or Payroll model configuration. DI and controller registration have no active Payroll registration.

## 5. Payroll database residuals

- Current EF model: Payroll entities/DbSets ABSENT.
- Migration history: Payroll references PRESENT in historical migration files, designers/snapshots and backup migration sets.
- Live database: UNKNOWN; this task did not connect to or inspect the live PostgreSQL schema.
- No migration was created, changed, applied, or rolled back.
- If old migrations were applied to a database, Payroll tables may still exist. A separately reviewed future migration/drop plan is required; this report does not drop tables.

Migration history was deliberately preserved and is not treated as active source code.

## 6. Tests

`SalaryConsistencyTests.cs` was a one-line obsolete Payroll test stub and was removed. No Attendance/Shift or unrelated test was removed. The final test run reports 173 passed, 0 failed, 0 skipped.

## 7. Documentation updated

Updated current documentation in `README.md`, `docs/system-audit/01`, `02`, `05`, `06`, `07`, `08`, `12`, `13`, `15`, `17`, `18`, and this report. Updated current `baocaodoan/` documents that described Payroll or Docker as active. Historical `docs/archive/` material was preserved.

## 8. Remaining historical/instruction references

Payroll names remain in migration history and archived historical documents by design. Payroll-specific policy text remains in `AGENTS.md` and `AI_RULES.md`; these are instruction files with pre-existing changes and require human confirmation before editing. They do not create runtime/UI/API functionality.

## 9. Verification before and after

Baseline before cleanup:

- Root `dotnet build` and `dotnet test`: failed because the repository root has no solution/project file.
- Correct API project build: passed.
- Correct test project: 173 passed in the earlier baseline run.
- Admin and Customer `npm run build`: passed.

After cleanup:

- `dotnet build services/api/RestaurantPOS.api.csproj --no-restore`: PASS, 0 warnings, 0 errors.
- `dotnet test tests/RestaurantPOS.Tests/RestaurantPOS.Tests.csproj --no-restore`: PASS, 173 passed, 0 failed, 0 skipped.
- Admin `npm run build`: PASS; Vite warns about a JavaScript chunk larger than 500 kB.
- Customer `npm run build`: PASS.

## 10. Files changed by this cleanup

Docker: the three files listed in section 1.

Payroll/source/UI/contracts: the obsolete stubs, `Employee.cs`, `EmployeeManagement.tsx`, `packages/shared/types/api.ts`, `packages/shared/swagger.json`, and the obsolete salary test.

Documentation: current README, system-audit files listed above, and current `baocaodoan/` files containing removed-scope claims.

Other dirty files shown by `git status` pre-date this cleanup and were preserved. No commit or push was performed.

## 11. Human review required

1. Inspect the live PostgreSQL migration table/schema and decide whether a future reviewed DROP migration is needed for old Payroll tables.
2. Decide whether to remove obsolete Payroll policy sections from `AGENTS.md` and `AI_RULES.md`; they were not changed automatically.
3. Confirm whether historical archive and migration references should remain permanently for traceability.

## Final status

- DOCKER: REMOVED; no runtime residual.
- PAYROLL SOURCE: REMOVED; migration/history residual only.
- PAYROLL UI: REMOVED.
- PAYROLL CURRENT EF MODEL: ABSENT.
- PAYROLL MIGRATION HISTORY: PRESENT.
- CURRENT DOCUMENTATION: CLEAN for product/system docs; RESIDUAL EXISTS in `AGENTS.md` and `AI_RULES.md` as unmodified Payroll policy text requiring human review.
