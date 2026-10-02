# Task 47F — Manager Branch Isolation Audit

## 1. Executive summary

Task 47F audited all 23 current HTTP controllers and all 99 actions a manager can reach, including public actions, authenticated common actions, and manager-specific actions. The audit traced controller claims, route/query/body identifiers, service filters, direct EF queries, entity relationships, Admin Web branch selectors, and all registered AI tools.

The starting automated baseline was 227/227. Seventeen endpoint or tool paths required hardening. The corrected suite passes 239/239 with zero failures and zero skips. No database connection, schema mutation, migration, or production access occurred.

This is source and automated-test validation. It is not real API or PostgreSQL runtime validation.

## 2. Security invariant and trust model

For branch-owned data, a manager's only authorization-grade branch source is the signed JWT `branchId` claim. Route, query, body, localStorage, frontend state, and related entity IDs are untrusted inputs.

Final fail-closed rules:

- a manager with a valid branch claim can operate only on that branch's resources;
- a manager without a valid branch claim is forbidden from branch-owned operations;
- request `BranchId` cannot widen manager authority;
- an entity ID is resolved and its stored branch is checked before read or mutation;
- admin retains global or explicitly selected-branch behavior where current architecture supports it;
- intentionally global master data is not mechanically forced into branch ownership.

## 3. Coverage

- Controllers audited: 23 (22 WebAPI controllers plus `AiController`).
- Manager-reachable actions audited: 99 of 117 total HTTP actions.
- AI tools audited: 14 registered tool functions.
- Backend paths inspected: controllers, application services, direct EF queries, authorization helpers, JWT claim readers, and entity relationships.
- Frontend selectors inspected: global Navbar branch selector, Dashboard, Expenses, Inventory, Work Schedule, Tables, Receipt/System Settings, and branch-management routes.

## 4. Data scope model

| Domain | Scope | Reason |
|---|---|---|
| Authentication/session | USER | JWT identity, role, and optional branch are tied to the authenticated principal. |
| Dashboard/revenue analytics | MIXED | Admin can query global/selected data; manager is restricted to JWT branch. |
| Orders/POS/kitchen requests/invoices | BRANCH | Orders and kitchen requests carry or resolve a branch; IDs cannot bypass that scope. |
| Reservations | BRANCH + USER | Staff access is branch-scoped; customer access is ownership-scoped. |
| Customer registry/loyalty | MIXED | Customer master data is shared; customer-facing detail/history is identity-owned. |
| Employees | BRANCH + USER | Management is branch-scoped for managers; operational self-profile is user-owned. |
| Branch metadata | GLOBAL MASTER | Public branch listing currently supports customer/QR selection; branch mutation is admin-only. |
| Areas | GLOBAL MASTER | Current model has no BranchId and mutations are admin-only. |
| Tables | BRANCH | Tables carry BranchId and service mutations/detail accept an authorized branch. |
| Products/toppings/promotions | GLOBAL MASTER | Current entities are shared catalog data; manager only has the explicitly allowed product-availability mutation. |
| Expenses | BRANCH | Expense carries BranchId; Inventory-linked expenses also have immutable-source protection. |
| Work schedules | BRANCH | Schedule and target employee must both belong to the manager's branch. |
| Attendance | BRANCH + USER | Managers view/manage their branch; operational roles act on their own records. |
| Cash shifts | BRANCH + USER | Shift belongs to branch and employee; reconciliation only includes same-branch orders. |
| Inventory items | GLOBAL MASTER | Item definitions are shared across branches. |
| Inventory balances/documents/ledger | BRANCH | BranchInventory, receipts, issues, and transactions are branch-owned. |
| Notifications | USER/BRANCH/MIXED | Service filters by recipient, role, branch, or intentional global notification. |
| Business insights | BRANCH; global for admin | Branch insights are manager-scoped; null/global insights are admin-only. |
| Receipt/system settings | BRANCH | Existing helpers resolve manager requests to the JWT branch. |
| AI | MIXED/CONTEXTUAL | Tool role and risk gates apply; manager tools require a valid branch context and pass it to services. |

## 5. Manager-accessible endpoint inventory and disposition

The complete 117-action route inventory remains in `48-ROLE-FUNCTION-MATRIX.md`. The 99 manager-reachable paths were rechecked against current source and classified as follows:

| Module | Manager paths | Final classification | Branch behavior |
|---|---:|---|---|
| Auth | 4 | SAFE / PUBLIC OR USER | Auth actions do not confer branch authority. |
| Area | 1 | GLOBAL BY DESIGN | Public shared master read. |
| Attendance | 3 | HARDENED | JWT branch for management; own identity for operational actions. |
| Branch | 1 | GLOBAL BY DESIGN | Public metadata read; mutations remain admin-only. |
| Business Insights | 5 | HARDENED | All manager paths require a valid branch; global/null insights are admin-only. |
| Customers | 6 | GLOBAL/USER BY DESIGN | Shared registry with customer ownership checks where applicable. |
| Dashboard | 1 | HARDENED | Manager query branch is replaced by parsed JWT branch. |
| Employees | 6 | HARDENED | List/mutations own branch; detail own branch or self; missing branch fails closed. |
| Expenses | 4 | SAFE | JWT branch constrains list and target IDs; Inventory-linked records remain protected. |
| Inventory | 20 | SAFE | `InventoryService` consistently resolves JWT branch and validates document/entity IDs. |
| Invoices | 2 | SAFE | Service/controller order branch constraints apply. |
| Notifications | 3 | SAFE | User/role/branch context is passed to notification service. |
| Orders/POS/Kitchen | 15 | HARDENED | Existing order paths were scoped; missing-branch kitchen request detail now fails closed. |
| Products | 2 | GLOBAL BY DESIGN | Public catalog and explicit shared availability operation. |
| Promotions | 1 | GLOBAL BY DESIGN | Public shared catalog read. |
| Receipt Settings | 2 | SAFE | Existing branch resolver rejects cross-branch manager input. |
| Reservations | 6 | SAFE | Service receives authorized branch/customer ownership context. |
| Cash Shift | 4 | HARDENED | Employee branch validated; close reconciliation includes shift branch. |
| System Settings | 2 | SAFE | Existing service resolves manager to JWT branch. |
| Tables | 6 | HARDENED | Existing mutations/list were scoped; detail now fails closed without branch claim. |
| Toppings | 1 | GLOBAL BY DESIGN | Public shared catalog read. |
| Work Schedule | 3 | HARDENED | Target employee and schedule must belong to manager JWT branch. |
| AI | 1 endpoint / 14 tools | HARDENED | Manager without BranchId cannot execute tools; tool services receive JWT branch. |

Totals: 82 paths were already safe or intentionally global; 17 endpoint/tool paths were hardened.

## 6. Findings matrix before fixes

| Severity | Endpoint/function | Problem | Exploit path | Resolution |
|---|---|---|---|---|
| High | Employee detail and mutations | Missing manager branch could compare equal to branchless records or create unscoped employees. | Manager JWT without BranchId plus employee ID/body. | Require valid JWT branch for manager detail and every mutation. |
| High | WorkSchedule create | Body BranchId was checked, but EmployeeId ownership was not. | Manager A submits Branch A plus EmployeeId from Branch B. | Resolve employee; require employee Branch A; derive names and branch server-side. |
| High | Shift open | Request EmployeeId and EmployeeName could reference/spoof another branch. | Manager A opens Branch A shift for Branch B employee. | Resolve employee; require matching branch; use stored full name. |
| High | Shift close revenue | Revenue query omitted BranchId. | Same employee-name string on another branch contaminates reconciliation. | Filter completed orders by `shift.BranchId`. |
| High | AI manager context | Manager without BranchId could invoke otherwise allowed tools with null/global scope. | Incomplete/malformed manager JWT invokes analytics/order/staff tools. | Central `AiPermissionService` fail-closed gate. |
| High | Kitchen request detail | Missing branch claim was passed as null, interpreted as unscoped. | Manager JWT without BranchId plus request ID. | Forbid before service call. |
| High | Business Insight detail/read/resolve | Null manager branch matched null/global insight scope. | Manager without BranchId accesses global insight ID. | Require manager branch before entity lookup/mutation. |
| Medium | Dashboard summary | Non-Guid or absent manager claim could reach service without a trustworthy branch. | Malformed manager JWT with arbitrary query branch. | Parse JWT claim and forbid on failure; always override query branch. |
| Medium | Table detail | Authenticated non-admin missing branch passed null authorized scope. | Manager JWT without BranchId plus table ID. | Forbid before service call. |
| Medium | Attendance checkout | Manager branch comparison did not explicitly fail closed before target comparison. | Missing manager BranchId with malformed/branchless context. | Require valid manager branch and matching attendance branch. |
| Medium | Admin Web branch UX | Manager saw global branch selector and branch-management links/routes. | Local branch selection advertised authority backend correctly denied. | Lock selector to session branch; branch management route/actions are admin-only. |

No vulnerability was found in the Inventory service branch model, Expense ID checks, Reservation service authorization, Receipt/System Settings branch helpers, or Notification filtering.

## 7. Hardening changes

### Backend

- `DashboardController`: parses and requires manager JWT branch, overriding request branch.
- `EmployeeController`: fails closed for branchless managers on detail and every management mutation.
- `WorkScheduleController`: validates target employee branch and derives persisted names/branch from trusted data.
- `AttendanceController`: requires valid manager branch for checkout management.
- `ShiftController`: validates employee-to-branch membership and scopes revenue reconciliation to the shift branch.
- `TableController`: prevents authenticated branch-bound callers from issuing an unscoped detail lookup.
- `OrderController`: prevents unscoped kitchen request detail lookup.
- `BusinessInsightController`: excludes managers without branch from global/null insights.
- `AiPermissionService`: denies all manager tool execution when trusted branch context is absent.

### Admin Web

- manager branch selector is non-switching and displays only the assigned session branch;
- `/branches` and every branch-management navigation entry are admin-only;
- System Settings hides branch-management entry from managers;
- Work Schedule branch choices are restricted to the manager's assigned session branch.

Frontend controls are UX alignment only. The backend remains the security boundary.

## 8. AI branch isolation

`AiController` derives role and BranchId from authenticated JWT context; it does not accept a client role or branch override for authorization. Tool arguments do not expose a generic BranchId selector. Branch-aware tools pass `AiUserContext.BranchId` into Dashboard, Employee, Table, Order, and financial services. Entity mutation through `update_order_status` also reaches OrderService branch checks.

The new permission gate rejects every manager tool invocation if `AiUserContext.BranchId` is absent. Admin remains eligible for global tools according to each tool's role/risk contract. There is no Inventory AI tool.

Anonymous access to `/api/Ai/chat` remains a separate Task 47H policy question.

## 9. Automated tests

Added regression coverage proves:

- Dashboard ignores manager-requested Branch B and uses JWT Branch A;
- Dashboard fails closed without a manager branch;
- manager cannot schedule a Branch B employee into Branch A;
- schedule names and branch values are derived from trusted employee data;
- manager cannot open a Branch A shift for a Branch B employee;
- shift reconciliation excludes another branch's orders;
- table and kitchen-request detail do not issue unscoped lookups;
- manager without branch cannot read global Business Insight detail;
- AI manager without branch is denied while a manager with branch retains permitted read tools;
- manager without branch cannot create an employee.

Focused authorization tests: 23/23 passed. Full backend suite: 239 discovered, 239 passed, 0 failed, 0 skipped.

## 10. Build results

- Backend isolated build: PASS, 0 errors.
- Admin Web production build: PASS. The existing Vite large-chunk warning remains.
- Customer Web: N/A; no Customer Web or shared frontend code changed.

## 11. Database safety

- Database connection: NO.
- Database mutation: NO.
- Migration created/applied: NO.
- Schema change: NO.
- Production touched: NO.

## 12. Intentional global exceptions

Current source intentionally models Areas, Products, Toppings, and Promotions as shared/global master data. Customers are a shared registry with ownership restrictions on customer-facing operations. Branch list is public metadata used by customer/QR flows. InventoryItem is global, while balances and documents are branch-owned. These resources were not given artificial BranchId rules.

## 13. Remaining/deferred findings

- Task 47D: `employee` login and frontend shell remain unresolved and unchanged.
- Task 47H: review public Branch/Area/Table/catalog surfaces and anonymous AI policy.
- Human runtime follow-up: exercise manager Branch A against an existing Branch B through the real API/UI. This audit did not connect to PostgreSQL.

## 14. Verdict

All current manager-accessible branch-owned paths were inspected. Confirmed vulnerabilities were minimally hardened with automated coverage while preserving admin global behavior and intentional global master data.

**TASK 47F: PASS — MANAGER BRANCH ISOLATION AUDITED AND HARDENED**
