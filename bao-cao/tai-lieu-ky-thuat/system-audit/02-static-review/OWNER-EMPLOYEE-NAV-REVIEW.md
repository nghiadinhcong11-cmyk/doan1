# Static Review — Owner “Nhân viên” Navigation

## Snapshot and limits

- Snapshot: branch `main`, HEAD `1ef623e94cea83f150006c9a22a10126611a171c` (`1ef623e9`).
- Working tree at review start: clean (`git status --short --branch` reported `## main...origin/main`).
- Review type: static source/document review only.
- No API, browser, database, migration, seed, SignalR or business transaction was run.
- No application code, migration or model snapshot was changed.
- `MockAiProvider`, `EmployeeType` and `BasicSalary` were retained. `EmployeeType` and `BasicSalary` are treated only as employee profile fields.

The report distinguishes source-confirmed behavior, source gaps, runtime risks and policy questions. It does not certify runtime behavior.

## Scope from actual navigation

The admin/manager `Navbar` contains a “Nhân viên” dropdown. Its desktop and mobile variants link to:

| Route | Page/component | Scope in this review |
|---|---|---|
| `/employees` | `EmployeeManagement.tsx` | Employee profile list, search, status/branch/department filters, add/edit, toggle active state, permanent delete, EmployeeType and BasicSalary |
| `/attendance` | `AttendanceManagement.tsx` | Attendance list/filter and admin QR activation/selection for a branch |
| `/schedule` | `WorkSchedulePage.tsx` | Weekly schedule, branch/name filters, create and delete schedule |
| `/shifts` | `ShiftManagement.tsx` | Shift history, date/branch filters and cash/revenue display |

The nav is rendered by `Navbar.tsx` and the four routes are registered by `ManagementRoutes.tsx`. `ManagementRoutes` is selected for both `admin` and `manager`; therefore the same frontend URL surface is not admin-only. Staff-specific routes such as `/staff/attendance`, `/pos/attendance`, `/kitchen/attendance` and their profile/schedule routes are outside this owner-nav review.

## Navigation and direct URL behavior

- `Navbar.tsx` marks the “Nhân viên” group active for `/employees`, `/attendance`, `/schedule` and `/shifts`.
- Desktop dropdown and mobile menu expose all four links.
- `ManagementRoutes.tsx` registers all four paths without a per-route frontend role wrapper. The application sends `admin` and `manager` sessions into this route set; other roles are sent to their separate route modules.
- Unknown URLs in the management route set redirect to `/dashboard`.
- Frontend visibility is not the security boundary. The API controllers enforce role and branch checks.

Source does not prove direct-URL behavior in a running browser. Existing `ADM-11` and `ADM-17` remain the reusable manual cases for direct URL and main-navigation checks.

## Trace by page

### `/employees`

UI: `EmployeeManagement.tsx`

- Loads branches from `GET /api/Branch` and employees from `GET /api/Employee`.
- Sends search, active-state, department and optional position query values. The branch filter is applied client-side after the unscoped response; no `branchId` query is sent from this page.
- Has active/inactive/all status radio filters, branch and department selects, debounced search, expandable detail rows, add/edit modal, status toggle and permanent delete.
- `filterPos` exists in state and is appended to the API query if populated, but no position-filter control is rendered in the inspected component.
- No pagination is implemented.
- `fetchEmployees` does not check `response.ok`; exceptions are logged and the page has no visible list error state. The empty state can therefore be indistinguishable from a failed/invalid response.
- Save closes the modal and refetches after success. Delete/toggle refetch after a successful response but do not show a visible failure response when the server returns a non-success status.
- The update payload is an entity-shaped object merged from the existing row and form state. It includes role, branch, active state, profile fields and password fields. The backend remains the authority for allowed role/branch changes.

API: `EmployeeController.cs`, `Employee.cs`, `ApplicationDbContext.cs`

- `GET /api/Employee`: `admin,manager`; managers are constrained to their JWT branch; admins may pass `branchId`, although this page does not pass it. The list projection omits `Password` while retaining `EmployeeType`, `BasicSalary`, role, branch and profile fields.
- `GET /api/Employee/{id}`: `admin,manager,employee,cashier,kitchen`; non-admin access is restricted to self, or manager same-branch access. The response sets `Password = null` before returning.
- `POST /api/Employee`: `admin,manager`; manager branch is forced from the JWT and management roles are downgraded to `employee`. Admin role/branch values are accepted from the request without an explicit branch existence/active check.
- `PUT /api/Employee/{id}`: `admin,manager`; managers cannot cross branches or edit management principals. Admin can update role/branch/profile/password fields from the entity-shaped payload. No explicit branch existence/active check is present before save.
- `PATCH /api/Employee/{id}/toggle-status`: `admin,manager`; manager is same-branch and non-management-only. No current-user/self check is present.
- `DELETE /api/Employee/{id}`: `admin,manager`; manager is same-branch and non-management-only. No current-user/self check is present.
- `Employee` has `EmployeeType` and `BasicSalary` as persisted profile fields. No payroll calculation path was included in this review.

Persistence/history: employee, attendance and schedule relationships are represented in `ApplicationDbContext.cs`; shifts use `ClientSetNull` for the employee relationship. Attendance and schedule use the default relationship behavior in the inspected configuration. Permanent employee deletion therefore needs runtime/database verification against existing attendance, schedule, shift and other historical references before it can be treated as safe.

### `/attendance`

UI: `AttendanceManagement.tsx`

- Calls `GET /api/Attendance` and `GET /api/Branch`.
- Provides employee-name search, branch filter, status filter, refresh, CSV-like export button UI and QR branch selection/activation modal.
- Shows loading and empty states. Fetch errors are logged and do not produce a visible error state.
- Branch/status filtering is performed in the browser after the API response.

API: `AttendanceController.cs`

- Controller authorization includes `admin,manager,cashier,kitchen,employee`.
- Admin may filter by requested `branchId`; manager is forced to the JWT branch. Non-management roles are forced to their own employee ID.
- Check-in/check-out validates active employee, QR type/date/branch and duplicate open attendance. Check-out also checks record ownership for non-management roles and manager branch scope.

### `/schedule`

UI: `WorkSchedulePage.tsx`

- Calls `GET /api/WorkSchedule`, `GET /api/Employee` and `GET /api/Branch` for the selected week.
- Provides week navigation, employee-name search, branch filter, create modal and delete action.
- Has loading and empty states. Fetch failures are logged without a visible page error state. Save/delete refetches but does not surface non-success response bodies consistently.
- The create payload copies `employeeName`, `branchId` and `branchName` from the selected employee in frontend state; the backend is expected to enforce branch ownership.

API: `WorkScheduleController.cs`

- Read authorization includes management and operational roles; non-management users are constrained to their own employee ID.
- Managers are constrained to their JWT branch for reads, create and delete. Admins may request a branch filter and may create/delete without the manager restriction.
- Manager create overwrites branch/name from the employee record. Admin create accepts the entity-shaped schedule payload without an explicit employee/branch consistency check visible in this controller.

### `/shifts`

UI: `ShiftManagement.tsx`

- Calls `GET /api/Shift?fromDate=...&toDate=...`; admins additionally call `GET /api/Branch`.
- Provides date range, branch filter for admins, refresh and empty/loading states.
- The page calculates displayed cash difference from persisted shift values; it does not mutate shift data.
- Fetch errors are logged and do not produce a visible error state.

API: `ShiftController.cs`

- Authorization is `admin,manager,cashier`.
- Admin may query all branches or a requested branch; manager requests are forced to the JWT branch; cashier is forced to own employee ID.
- Open/close operations verify employee/branch access and reject duplicate open or already-closed shifts. The admin/manager UI under `/shifts` is read-only; open/close UI is elsewhere in POS hooks.

## Findings

### HR-001 — Admin can self-disable or self-delete through the employee management API

- Severity: High
- Priority: P1
- Confidence: source behavior confirmed; effect on already-issued JWT requires runtime/auth verification.
- Evidence: `EmployeeController.ToggleStatus` and `DeleteEmployee` authorize `admin,manager`, load the target employee and mutate/delete it without comparing the target ID with `User.FindFirst(ClaimTypes.NameIdentifier)`. `EmployeeManagement.tsx` exposes both “Cho nghỉ/Kích hoạt” and “Xóa vĩnh viễn” for expanded rows.
- Impact: an admin can lock or remove the account represented by the current session. Login filters `IsActive`, but the inspected authentication path does not establish that an already-issued JWT is revoked or revalidated on every request. Permanent deletion may also affect historical employee-linked data.
- Classification: source-confirmed missing self-protection; existing-session behavior and referential outcome are runtime/database questions.
- Minimum follow-up: test the current admin account against toggle/delete using a disposable test identity, then decide whether self-disable/delete must be rejected or require another admin/confirmation workflow.

### HR-002 — HR pages do not consistently expose API errors or non-success responses

- Severity: Medium
- Priority: P1
- Confidence: source behavior confirmed.
- Evidence: `EmployeeManagement.fetchEmployees`, `AttendanceManagement.fetchAttendances`, `WorkSchedulePage.fetchSchedules` and `ShiftManagement.fetchData` log errors or fall back to empty data without a user-visible error state. `EmployeeManagement.fetchEmployees` parses the body without checking `response.ok`; save/delete/toggle handlers also do not consistently surface non-success response bodies.
- Impact: API failure, expired session, empty data and an invalid response can appear identical. Users may retry without understanding whether a mutation was accepted, and repeated operations need runtime verification.
- Minimum follow-up: add manual cases for API error/401/403, retry and repeated save/delete/toggle. Any code fix should be scoped to the affected page patterns only.

### HR-003 — Admin employee branch assignment lacks explicit active-branch validation

- Severity: Medium
- Priority: P1
- Confidence: source behavior confirmed; final business policy for inactive-branch assignment is not documented here.
- Evidence: `EmployeeController.CreateEmployee` and `UpdateEmployee` accept `BranchId` for admin without an explicit lookup requiring an existing active branch. `EmployeeManagement` loads branch choices from `GET /api/Branch`, whose public list filters active branches, but a direct request can submit another value. A database FK may reject an unknown ID; the controller does not provide a deterministic application-level validation response, and an existing inactive branch is not rejected by source.
- Impact: employee records may be assigned to an inactive or invalid branch, weakening branch lifecycle invariants and causing later HR/schedule/auth behavior to diverge from the UI selection model.
- Minimum follow-up: test valid, unknown and inactive branch IDs in an isolated API environment. If inactive assignment is not intended, add a server-side validation rule after that policy is explicitly approved.

## Observations and non-findings

- No pagination exists on the employee, attendance, schedule or shift pages inspected. This is a capability observation, not automatically a defect without a product requirement.
- Employee list has department/status/branch/search controls, but no role filter control. `filterPos` is present in state/query construction without a rendered position filter control.
- `EmployeeType` is editable as a text field and `BasicSalary` as a numeric input. The source does not define a domain range/format rule for either field beyond the input type and decimal persistence. No new validation rule is inferred here.
- The employee controller uses an entity-shaped request/response for admin/manager operations. The source strips password from read/list responses and nulls it before create/update response, but broader DTO minimization was not treated as a new finding in this review.
- No payroll calculation, payroll period, deduction or payment behavior was inferred from `EmployeeType` or `BasicSalary`.

## Coverage and limitations

Covered statically:

- Actual owner navigation and route registration.
- `/employees`, `/attendance`, `/schedule`, `/shifts` UI behavior visible in source.
- Corresponding controller authorization, query/mutation paths, entities and persistence relationships.
- `EmployeeType` and `BasicSalary` binding/persistence/display paths.
- Direct-URL route composition at the React source level.

Not covered:

- Browser rendering, responsive layout, actual HTTP status/body, JWT expiry/revocation, runtime branch isolation, database FK behavior, duplicate submissions, concurrency, QR scanning, export output or persisted historical deletion effects.
- Routes outside the owner “Nhân viên” navigation, including staff-specific HR routes.

## Implementation follow-up — 2026-10-15

The requested minimal source fixes were applied after the original static review:

- HR-001: `EmployeeController.ToggleStatus` and `DeleteEmployee` compare the target with the authenticated `ClaimTypes.NameIdentifier` and return `409 Conflict` for self-disable/self-delete. The employee page disables those actions for the current employee but keeps profile editing available.
- HR-002: `EmployeeManagement`, `AttendanceManagement`, `WorkSchedulePage` and `ShiftManagement` check response status, preserve failed form state, show safe error feedback, finish busy states, and avoid duplicate pending writes or refreshes.
- HR-003: `EmployeeController.CreateEmployee` checks supplied branch IDs for existence and active state. `UpdateEmployee` checks only an actual non-null branch transfer; profile edits retain the existing branch even if that branch is inactive. Manager JWT branch enforcement remains unchanged.

Static build evidence: admin frontend `npm run build` passed; backend source build passed with isolated output at `.artifacts/hr-employee` because the normal API executable was locked by another process. No API, browser, database, migration or seed verification was run; findings remain open for runtime confirmation.

## Static review of the HR fix diff — 2026-10-15

Compared with HEAD `1ef623e9`, the HR application-code delta is limited to `EmployeeController.cs` and the four owner-nav pages. The other dirty files were pre-existing audit/context/checklist changes from the earlier review and are not attributed to the HR source fix.

### Residual findings

#### HR-004 — Self-disable remains possible through `PUT /api/Employee/{id}`

- Evidence: `EmployeeController.UpdateEmployee` accepts an entity-shaped `Employee`, then marks the bound entity `EntityState.Modified` without rejecting `CurrentUserId() == id` when `employee.IsActive` is false. The new self-checks exist only in `ToggleStatus` and `DeleteEmployee`.
- Path: `PUT /api/Employee/{id}` → `UpdateEmployee` → `_context.Entry(employee).State = Modified` → `SaveChangesAsync`.
- Impact: a direct payload can still disable the authenticated employee even though the UI toggle is disabled. The controller must preserve valid self-profile edits while preventing a self status transition.
- Classification: source-confirmed residual HR-001 gap. No runtime claim.
- Minimum follow-up: compare the persisted status with the requested status after loading `existing`; reject only a self transition to inactive. Do not infer a last-admin policy.

#### HR-005 — `PUT` binding conflates omitted `BranchId` and explicit `null`

- Evidence: `UpdateEmployee` binds the whole `Employee`; both an omitted nullable `BranchId` and an explicit `branchId: null` arrive as `null`. The condition `employee.BranchId != existing.BranchId` therefore treats either as a branch change/removal. A non-null target is checked for active existence, but null removal is not distinguished or governed by a separate rule.
- Path: `PUT /api/Employee/{id}` → model binding → `employee.BranchId` comparison → `_context.Entry(employee).State = Modified`.
- Impact: a profile-only request that omits branch can clear the branch; the requested “giữ nguyên branch” behavior is not provable for partial/entity-shaped payloads. This is especially relevant for inactive-branch profile edits and role–branch policy.
- Classification: source-confirmed contract/branch-policy gap; intended null-transfer policy still needs confirmation.
- Minimum follow-up: use an update contract that distinguishes field absence from explicit null, or explicitly preserve `existing.BranchId` for profile edits while validating an intentional transfer. Preserve manager JWT branch enforcement.

#### HR-006 — Pending guards and mutation/refetch outcomes are not fully distinct

- Evidence: `EmployeeManagement.handleSaveEmployee` and `WorkSchedulePage.handleSave` disable the submit button through state but do not guard at handler entry. `EmployeeManagement` closes/resets after mutation success and calls `fetchEmployees()` without awaiting it; delete/toggle and schedule delete await refetch, but their fetch helpers swallow refetch failures and only set a generic page/list error. No success-vs-refetch-failed feedback is emitted.
- Path: employee save/toggle/delete and schedule save/delete handlers → mutation response → fetch helper → UI state.
- Impact: rapid programmatic/re-entrant submits are not prevented at the handler boundary, and a successful mutation followed by failed refetch can look like an unresolved operation, encouraging an unsafe retry. This is not a claim that the mutation is duplicated in normal browser use; it is a source-level state distinction gap.
- Classification: source-confirmed residual HR-002 gap. Attendance and shifts are read-only in this navigation; their refresh guards were reviewed separately.
- Minimum follow-up: add handler-level in-flight guards and a result path that records mutation success separately from refetch failure. Keep the form closed only after mutation success, not after refetch success.

### Scope and non-findings

- JWT identity mapping was checked: `AuthController` passes `employee.Id` to `JwtService.GenerateToken`, which writes it to `ClaimTypes.NameIdentifier`; the new backend comparison therefore uses the employee primary key in this source path. The browser `localStorage.employeeId` is UX-only and is not trusted by the backend.
- `POST /api/Employee` validates the effective branch after manager branch override and before add/save. Manager role/branch restrictions remain in place. Update validation occurs before `EntityState.Modified`; no code path here mutates attendance, work schedules or shifts during a branch transfer.
- `AttendanceManagement` and `ShiftManagement` have no owner-nav mutation handler; their read/refresh error paths were reviewed. Existing UI-only attendance export/download/print controls were not introduced by this HR diff and remain a separate functionality observation.
- EmployeeType and BasicSalary, MockAiProvider, migrations, snapshots and historical links were not changed.

The HR patch is therefore only partially ready for handoff: HR-001, HR-002 and HR-003 have meaningful source fixes, but HR-004/005/006 require source decisions/fixes before the HR handoff can be closed. All new manual cases remain `Chưa test`.

## Second implementation follow-up — 2026-10-15

The three residual gaps from the preceding review were addressed in source:

- HR-004 is merged into HR-001. `UpdateEmployee` now rejects an authenticated employee payload with `IsActive=false` before constructing a modified entity or calling `SaveChangesAsync`. `ToggleStatus` and `DeleteEmployee` retain the same protection.
- HR-005 is merged into HR-003. `EmployeeUpdateRequest` is an allow-listed update contract; its `JsonElement BranchId` distinguishes omitted, `null`, and an ID. Omitted preserves the current branch, a different non-null ID is checked for active existence before mutation, and explicit null removes the branch only under the existing admin behavior. Manager branch enforcement is retained. No history entity is touched.
- HR-006 is merged into HR-002. Employee and schedule mutation handlers now use ref-backed in-flight guards; independent employee rows and schedule rows do not share one global lock. Mutation success is reported separately when the subsequent refetch fails. Attendance and shifts have ref-backed refresh guards because they are read-only in this navigation.

Review result: no new standalone HR-004/005/006 remains after this change; they are residual paths covered by HR-001/002/003. The findings are source-fixed and await runtime only. The previous paragraph is retained as historical review context; this section supersedes its handoff conclusion.

Role–branch note: the inspected source already lets an admin create/update a branchless employee and does not define a separate target-role requirement for an admin assigning `manager`. This change preserves that behavior rather than inventing a new rule. If a `manager` target must always have a branch, that remains a policy clarification before tightening the contract.

## Final handoff clarification - 2026-10-15

### BranchId null and actor/target roles

- Actor authorization and target employee role are separate. A manager actor is constrained by the authenticated JWT branch and the controller preserves the existing target branch even when BranchId is omitted or null. An admin actor is chain-wide under the existing authorization path.
- The target role is the normalized requested Role, or the existing role when omitted. For an admin actor, the current source permits a branchless target for normalized staff roles, including manager; there is no existing admin-side target-role rule requiring a manager branch. This behavior is preserved rather than changed into a new policy.
- For an admin update: omitted BranchId preserves the current branch; the same non-null ID preserves it without requiring an inactive branch to become active; a different non-null ID requires an existing active branch before mutation; explicit null removes the branch under the existing admin behavior. Role and final branch are calculated before mutation, and no attendance, schedule, shift or order-history link is changed.
- No new source blocker was found. Whether an admin-assigned manager must always have a branch remains a policy clarification outside this fix.

### Attendance and shifts write surface

- Owner `/attendance` is read/refresh-only: it performs GET requests for attendance and branch data; QR activation is local UI state and no owner-page attendance mutation handler exists.
- Owner `/shifts` is read/refresh-only: it performs GET requests for shifts and optional branch data; no open/close mutation handler exists on this page.
- Backend write endpoints exist outside these owner-page handlers: `AttendanceController.CheckIn` (`POST /api/Attendance/check-in`), `CheckOut` (`PATCH /api/Attendance/{id}/check-out`), `ShiftController.OpenShift` (`POST /api/Shift/open`) and `CloseShift` (`POST /api/Shift/close/{id}`). They are staff/POS flows outside this owner navigation page scope and were not runtime-tested here.
- Owner-page writes are employee create/update/status-toggle/delete and schedule create/delete. Those reviewed handlers include response checks, in-flight protection and separate mutation-success/refetch-failure feedback.

HR-001, HR-002 and HR-003 are `Đã sửa source, chờ kiểm chứng runtime`; HR-004/005/006 are merged residual paths, not independent fixes. All manual cases remain `Chưa test`; no runtime PASS is claimed.

