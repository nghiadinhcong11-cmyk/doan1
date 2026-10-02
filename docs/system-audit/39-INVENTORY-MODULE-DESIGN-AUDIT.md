# Inventory Module Design Audit

Date: 2026-10-01  
Scope: source-code audit and design only. No Inventory implementation, migration, database change, User Secret change, commit, or push was performed.

Status labels in this report:

- **CURRENT**: evidenced by the executable repository source.
- **PROPOSED**: design recommendation for a future approved implementation.
- **FUTURE**: deliberately outside the first Inventory phase.

## 1. Executive Summary

**CURRENT:** Inventory/ingredient management is not implemented. The current system has products, toppings, promotions, orders, kitchen requests, expenses, dashboards, notifications and SignalR, but no stock item, branch stock balance, stock receipt, stock issue or stock transaction entity/service/controller/page.

**PROPOSED:** Add a branch-scoped stock ledger with an independent `InventoryItem` master, immutable confirmed documents, and one authoritative confirmation transaction. `BranchInventory` is the current balance; `StockTransaction` is the audit ledger. Product sales do not deduct inventory in phase 1 because no Recipe/BOM exists.

The first safe implementation should be a new vertical slice behind the existing JWT/role/branch model. Expense integration should be one-to-one and idempotent: one confirmed `StockReceipt` may create at most one linked Expense, in the same database transaction as the stock increase.

## 2. Current System Findings

### Architecture and persistence

**CURRENT:** The repository uses ASP.NET Core 8, EF Core/Npgsql, PostgreSQL, React/Vite, JWT bearer authentication and SignalR. The main persistence boundary is `ApplicationDbContext`; controllers commonly call application services, while `ExpenseController` currently uses the context directly.

**CURRENT:** The canonical migration tree is `services/api/src/Infrastructure/Persistence/Migrations/`. The migration audit identifies 43 classes, 40 discoverable migrations, no duplicate active migration IDs, a final ReceiptSettings creation in `20260919075618_RemovePayrollAndRepairReceiptSettings`, and Payroll functionality out of scope. Inventory must use one new migration later; it must not modify historical migrations.

### Inventory existence

**CURRENT:** No `Inventory`, `Stock`, `Warehouse`, `Ingredient`, `Recipe` or `BOM` domain path was found in the current source. There is no inventory API route, navigation item, DTO, service, EF `DbSet`, migration, or frontend screen.

### Existing role reality

**CURRENT:** The employee roles actually accepted by JWT validation and employee login are `admin`, `manager`, `employee`, `cashier`, and `kitchen`. `customer` is a separate customer role. `position` is also included in JWTs but is descriptive and must not authorize Inventory actions.

### Existing tenant/branch reality

**CURRENT:** There is a `Branch` entity and `Employee.BranchId`. There is no `Restaurant` entity and no `RestaurantId` in the current domain model. JWTs carry `branchId`, `branchName`, role and identity claims. `admin` is global in the current controller patterns; non-admin staff are normally constrained to the branch claim.

**PROPOSED:** Inventory should store `BranchId` on every branch-owned record. Do not add `RestaurantId` merely because the candidate model contains it; there is no current authoritative Restaurant tenant to reference. If a true restaurant tenant model is added later, it can become a deliberate cross-cutting migration.

## 3. Expense Audit

### Current entity and schema

**CURRENT:** `Expense` contains:

- `Id: Guid`
- required `BranchId: Guid`
- `Description`, `Amount: decimal`, `ExpenseDate`, `Category`
- optional `PaymentMethod`, `Note`
- nullable `CreatedBy: Guid?`
- `CreatedAt`, `UpdatedAt`

The snapshot maps the table to `Expenses` and has an index on `(BranchId, ExpenseDate)`. The entity has no foreign-key navigation to Branch and no reference to another entity.

### Creation and permissions

**CURRENT:** `ExpenseController` exposes `GET`, `POST`, `PUT` and `DELETE` at `/api/Expense`, protected by `[Authorize(Roles = "admin,manager")]`. Creation validates positive amount, active Branch, required description/category/date, then sets `CreatedBy` from `ClaimTypes.NameIdentifier`. It does not accept the creator identity as a trusted client field.

**CURRENT:** `TryResolveBranch` lets `admin` choose any branch. Non-admin users must have a JWT `branchId`; a requested branch must equal that claim. The existing tests cover manager Branch A versus Branch B for create/read and verify the controller role declaration. This is a useful pattern to reuse, but every new Inventory endpoint must apply the rule before querying or mutating data.

### Categories, frontend and dashboard

**CURRENT:** Expense categories are a hardcoded controller allowlist: raw materials, packaging, utilities, cleaning, gas, operations and other. `ExpenseManagement.tsx` uses the existing page/table/form/feedback patterns, lets only admins select all branches, and calls the existing `/api/Expense` route. The page explicitly says it records actual expenses and does not manage physical stock.

**CURRENT:** `DashboardService` and financial analysis aggregate Expenses by branch/date/category. AI financial prompts also warn about double-counting material Expenses and estimated COGS. A future StockReceipt link must preserve the existing dashboard contract and must not silently count the same purchase twice.

### Answers to the requested Expense questions

1. **How created:** direct `ExpenseController.CreateExpense`, context `Add` and `SaveChangesAsync`.
2. **BranchId:** yes, required and branch-filtered.
3. **Category:** yes, required and restricted to a hardcoded list.
4. **CreatedBy:** nullable Guid, populated from the authenticated name-identifier claim.
5. **Entity link:** none currently.
6. **Reusable transaction/service abstraction:** no Expense application service or transaction abstraction is present; `IDashboardService.InvalidateSummaryCache()` is reused after writes. Other services use EF transactions in selected flows.
7. **Branch authorization:** reasonably enforced in the current controller for Expense operations, but it is controller-local and should not be copied blindly. `BranchController` list access and some older controllers show broader inconsistencies elsewhere.
8. **Duplicate risk:** yes. There is no idempotency key, unique reference, confirmed state or transaction boundary preventing repeated manual Expense creation.

### Proposed Expense integration

**PROPOSED:** Add nullable `StockReceiptId` to `Expense`, with a unique filtered index for non-null values and a restricted foreign key to `StockReceipts`. This keeps Expense as the financial record and gives the existing dashboard a direct, simple link. `StockReceipt` remains the operational document and stores `ExpenseId` only as a response DTO convenience if needed, not as a second source of truth.

On receipt confirmation, one database transaction should:

1. lock/reload the draft receipt and branch stock rows;
2. verify the receipt is still Draft;
3. apply each stock increase;
4. insert one `StockTransaction` per line;
5. create one Expense with category `Nhập hàng` or a centrally approved Inventory category;
6. set `Expense.StockReceiptId`;
7. mark the receipt Confirmed;
8. commit.

A second confirm returns the existing confirmed result or a conflict and performs no new stock/Expense operation. The unique reference and confirmed-state check protect against both application retries and database races.

## 4. Branch/Auth Audit

### Current security model

**CURRENT:** JWT authentication validates issuer, audience, signature and lifetime. Employee tokens contain role, employee identity and branch claims. On token validation, employee existence and active status are checked. Controllers commonly use `[Authorize(Roles = ...)]` plus a branch claim guard.

`admin` is global in current patterns. `manager`, `employee`, `cashier` and `kitchen` are branch-bound through `branchId` unless a specific controller explicitly provides another rule. `customer` is not an Inventory role.

**CURRENT risk:** `BranchId` is routinely present in request DTOs. The server must treat it as a selector to validate against JWT scope, never as proof of access. Inventory must reject a Branch A token with a Branch B request before loading or writing Branch B rows.

### Proposed branch rules

- `admin`: can select any active branch and view cross-branch reports according to existing global scope.
- non-admin staff: effective branch is always the JWT `branchId`; a supplied BranchId is accepted only if equal.
- every `BranchInventory`, receipt, issue, transaction and linked Expense query includes effective BranchId or joins through a branch-owned document.
- `InventoryItem` is global catalog master data in phase 1; its stock is never global. `BranchInventory` is the isolation boundary.
- no `RestaurantId` is stored in phase 1 because the source has no Restaurant entity. Do not invent a parallel tenant claim.

## 5. Product vs Inventory Analysis

**CURRENT Product:** `Product` is a sellable menu item with code, name, category/group strings, price, cost price, sizes JSON, toppings JSON, availability and active state. `Topping` is also a sellable/catalog add-on. `Promotion` is a loyalty/discount object. There is no Category entity; category values are fields or strings.

A Product such as “Cà phê sữa” is not the same record as stock materials such as coffee, milk and sugar. Products are not consistently branch-owned in the current catalog. Reusing Product for inventory would mix retail price, menu availability, toppings and raw-material units and would make future recipes difficult.

**PROPOSED:** `InventoryItem` is independent from Product. It represents a purchasable/consumable stock item and its canonical unit. No Recipe/BOM relationship is required in phase 1, but a future `Recipe`/`RecipeLine` can reference `ProductId` and `InventoryItemId` without changing the stock ledger.

**FUTURE:** Recipe/BOM, automatic deduction on sale, cost-layering and COGS. Until approved, product sales must not mutate inventory.

## 6. Proposed Business Flow

### Receipt / inbound

`Create Draft -> add lines -> validate unit/quantity/price -> Confirm -> transactionally increase BranchInventory -> append StockTransactions -> create/link one Expense -> Confirmed`.

Drafts do not change stock or Expense. Confirmed quantities and unit prices are immutable. Corrections use a reversal/adjustment document, not an edit of history.

### Issue / outbound

`Create Draft -> choose branch/item -> show current balance -> enter quantity/reason -> Confirm -> lock BranchInventory -> verify sufficient quantity -> decrease balance -> append StockTransaction -> Confirmed`.

An issue for more than current stock is rejected. The client preview is not trusted.

### Adjustment

An adjustment is a separately permissioned document with a reason and before/after snapshot. It should be used for count corrections, damage, expiry or opening-balance correction. It is not a silent update to `CurrentQuantity`.

## 7. Proposed Data Model

The following is design only; no entities or migrations were added.

### InventoryItem

`Id Guid`, `Name`, `UnitCode`, `MinimumStock decimal(18,3)`, `IsActive`, `CreatedAtUtc`, `UpdatedAtUtc?`.

The item master is independent of Product. `UnitCode` is normalized and controlled. Once the item has stock history, changing its unit is forbidden; create a replacement item or use an approved conversion migration.

### BranchInventory

`Id Guid`, `BranchId Guid`, `InventoryItemId Guid`, `CurrentQuantity decimal(18,3)`, `UpdatedAtUtc`, optional concurrency token.

Unique constraint: `(BranchId, InventoryItemId)`. No RestaurantId in the current domain.

### StockReceipt / StockReceiptItem

`StockReceipt`: `Id Guid`, `BranchId`, optional `SupplierName`, `TotalAmount decimal(18,2)`, `Status`, `Note`, `CreatedBy`, `CreatedAtUtc`, `ConfirmedBy`, `ConfirmedAtUtc`, optional idempotency key.

`StockReceiptItem`: `Id Guid`, `StockReceiptId`, `InventoryItemId`, `Quantity decimal(18,3)`, `UnitPrice decimal(18,2)`, `TotalPrice decimal(18,2)`, and a snapshot `UnitCode` only if the implementation needs historical display protection. The authoritative unit must be checked against the item at draft time and confirmation.

### StockIssue / StockIssueItem

`StockIssue`: `Id Guid`, `BranchId`, `Reason`, `Note`, `Status`, `CreatedBy`, `CreatedAtUtc`, `ConfirmedBy`, `ConfirmedAtUtc`, optional idempotency key.

`StockIssueItem`: `Id Guid`, `StockIssueId`, `InventoryItemId`, `Quantity decimal(18,3)`.

### StockTransaction

`Id Guid`, `BranchId`, `InventoryItemId`, `Type` (`IN`, `OUT`, `ADJUSTMENT`), `Quantity decimal(18,3)`, `BeforeQuantity`, `AfterQuantity`, `ReferenceType`, `ReferenceId`, `CreatedBy`, `CreatedAtUtc`, optional `Note`.

`Quantity` is positive and `Type` determines direction. A transaction is append-only. `ReferenceType + ReferenceId + InventoryItemId + Type` should be protected against duplicate application where appropriate; the document confirmation/idempotency key remains the primary guard.

## 8. Entity Relationships and Conventions

**PROPOSED:** All IDs are `Guid`, matching the current domain. Money uses `decimal(18,2)`; quantities and minimum stock use `decimal(18,3)`. Date fields use UTC `DateTime` with `CreatedAtUtc`/`ConfirmedAtUtc` naming in new entities, while existing `CreatedAt` fields are not renamed.

Relationships:

- `BranchInventory.BranchId -> Branch.Id`, restrict delete.
- `BranchInventory.InventoryItemId -> InventoryItem.Id`, restrict delete.
- Receipt/issue documents -> Branch, restrict delete.
- Receipt/issue lines -> parent document, cascade only while the document is Draft; in implementation, deleting confirmed history must be forbidden, so a restricted FK or application rule is safer.
- Lines -> InventoryItem, restrict delete.
- `StockTransaction` -> Branch and InventoryItem, restrict delete.
- `Expense.StockReceiptId -> StockReceipt.Id`, nullable, unique when present, restrict delete.

Indexes:

- unique `(BranchId, InventoryItemId)` on BranchInventory;
- `(BranchId, CreatedAtUtc)` on receipts/issues;
- `(BranchId, InventoryItemId, CreatedAtUtc)` on StockTransaction;
- `(ReferenceType, ReferenceId)` on StockTransaction;
- unique idempotency key per document type/branch if an API idempotency key is adopted;
- unique filtered Expense link for `StockReceiptId IS NOT NULL`.

Statuses are strings or a centrally defined enum mapped to stable strings, consistent with current status-string usage. The choice must be made once before implementation; clients must not invent values.

## 9. Unit and Quantity Design

### Unit options

- **Free string:** rejected; it permits `kg`, `KG`, `kilogram` and per-document changes.
- **Enum:** strong validation but requires code/migration changes for every new unit and is less friendly to future localization.
- **Unit table:** extensible, but additional master-data lifecycle is unnecessary for phase 1.

**RECOMMENDED:** store a controlled `UnitCode` string on `InventoryItem`, validated by one application-level unit catalog and optionally protected by a PostgreSQL check constraint for the initial allowlist: `kg`, `g`, `litre`, `ml`, `piece`, `box`, `bottle`, `pack`. Receipt/issue DTOs do not accept a unit; they derive it from InventoryItem. A future Unit table can replace the allowlist without changing transaction semantics.

No automatic `kg <-> g` or `litre <-> ml` conversion is proposed for phase 1. An item has one canonical unit.

### Precision

**RECOMMENDED:** `decimal(18,3)` for quantities. It supports `2.5 kg` and `0.25 kg` while remaining bounded. Discrete units (`piece`, `box`, `bottle`, `pack`) require a mathematically integral quantity greater than zero. Continuous units may use up to three decimal places. PostgreSQL numeric/EF decimal is required; never use `double` or `float`.

Money uses `decimal(18,2)` as a design default, while existing VND financial rules remain owned by their current services.

## 10. Stock Consistency and Concurrency

**CURRENT:** Existing services use EF transactions in selected workflows and some atomic `ExecuteUpdateAsync` operations, but there is no Inventory row-locking or concurrency-token pattern to reuse directly. `BranchInventory` does not yet exist.

**PROPOSED confirmation algorithm:**

1. Begin a PostgreSQL database transaction.
2. Load the document by ID and effective branch; reject missing, unauthorized or non-Draft documents.
3. Lock each affected `BranchInventory` row with PostgreSQL `FOR UPDATE`, in deterministic `InventoryItemId` order.
4. If a row is missing for an inbound item, create it under the unique constraint and retry/reload safely; for an outbound item, reject.
5. Validate all quantities and reject any issue where `CurrentQuantity < requested quantity`.
6. Calculate `BeforeQuantity` and `AfterQuantity` in decimal arithmetic.
7. Update the balance, insert ledger rows, and mark the document Confirmed in the same transaction.
8. For receipts, insert the linked Expense once under the unique `StockReceiptId` constraint.
9. Commit; publish notifications only after commit.

This protects the example of concurrent issues of 4 kg and 3 kg from a 5 kg balance. A simple read-then-write or EF optimistic check alone is insufficient. An optional `xmin`/row-version strategy can detect stale edits, but row locking plus a database transaction is the authoritative confirm mechanism.

Double submit protection requires a unique client idempotency key or document ID plus a confirmed-state check. Confirmed documents cannot be edited or confirmed again.

## 11. Document Status and History

**PROPOSED statuses:** `Draft`, `Confirmed`, `Cancelled`.

- Draft: editable; no stock or Expense effect.
- Confirmed: applies exactly once; quantities and lines immutable.
- Cancelled before confirmation: no stock effect.
- Cancelled after confirmation: do not erase history; create a reversal/adjustment document if business approval allows it.

There is no direct delete for confirmed documents or StockTransactions. Corrections are explicit and auditable.

## 12. Expense Integration

The recommended link is `Expense.StockReceiptId`, nullable and unique when non-null. It avoids two competing links and fits the existing Expense table, which already owns BranchId, Amount, Category, CreatedBy and CreatedAt.

Receipt confirmation and Expense creation share one transaction. The Expense amount equals the persisted receipt `TotalAmount`; no client-calculated amount is trusted at confirmation. Category should be a backend-owned Inventory/receiving category, with a clear decision whether to add it to the current Expense allowlist or use an existing approved category. The current hardcoded category list means implementation must update controller/service validation together.

If the receipt is confirmed twice, the second request must return the already-confirmed receipt or a conflict without adding stock, transaction rows or Expense. The unique link is a database backstop, not the only protection.

The existing dashboard can include linked purchase expenses once, but the COGS/estimated-profit calculation must be reviewed before enabling both purchase Expense and inventory valuation in the same metric. This is a known double-counting risk already documented in AI prompts.

## 13. Permission Matrix

The matrix below is **PROPOSED** using current roles; `Position` is never used for permission.

| Action | admin | manager | employee | cashier | kitchen |
|---|---:|---:|---:|---:|---:|
| ViewInventory | all authorized branches | own branch | own branch read | own branch read | own branch read |
| CreateInventoryItem | yes | yes | no | no | no |
| UpdateInventoryItem | yes | yes | no | no | no |
| CreateStockReceipt | yes | own branch | no | no | no |
| ConfirmStockReceipt | yes | own branch | no | no | no |
| CreateStockIssue | yes | own branch | own branch draft | own branch draft | own branch draft |
| ConfirmStockIssue | yes | own branch | no by default | no by default | no by default |
| AdjustStock | yes | own branch with reason | no | no | no |
| ViewStockHistory | all authorized branches | own branch | own branch | own branch | own branch |
| ViewInventoryDashboard | allowed branches | own branch | own branch read | own branch read | own branch read |

The employee/kitchen/cashier confirm decision is an open business decision. The conservative default is draft creation by operational staff and confirmation by manager/admin to prevent unreviewed stock movement. If the business requires immediate issue confirmation, it needs an explicit permission and stronger audit/approval rules, not a role inferred from Position.

## 14. API Design

Routes should follow the existing controller convention and remain branch-safe:

| Endpoint | Proposed roles/scope | Request/response | Main validation and side effects |
|---|---|---|---|
| `GET /api/inventory/items` | authenticated staff, branch-independent master read | filters + paged item DTOs | active/unit filter; no stock mutation |
| `POST /api/inventory/items` | admin/manager | create DTO -> item DTO | canonical name/unit; no duplicate active item; no branch spoofing |
| `PUT /api/inventory/items/{id}` | admin/manager | update DTO | unit change restricted after history; no mass assignment |
| `GET /api/inventory/branches/{branchId}` | staff with branch access | branch balance DTOs | server resolves branch scope; includes low-stock status |
| `GET /api/inventory/dashboard` | staff with branch access | branch/month/item/status filters | read-only aggregate query |
| `POST /api/inventory/receipts` | admin/manager | draft receipt + lines | branch access, item/unit/quantity/price validation; no stock effect |
| `POST /api/inventory/receipts/{id}/confirm` | admin/manager | confirm command/idempotency key -> receipt result | transaction, stock IN, ledger, one Expense |
| `POST /api/inventory/issues` | staff in branch | draft issue + lines | branch access; no stock effect |
| `POST /api/inventory/issues/{id}/confirm` | admin/manager by default | confirm command -> issue result | row lock, sufficient stock, stock OUT, ledger |
| `POST /api/inventory/adjustments` | admin/manager | adjustment DTO -> result | reason required; explicit ledger entry |
| `GET /api/inventory/transactions` | staff with branch access | paged history filters | read-only; branch and date/item filters |

Request DTOs should contain item IDs, quantities, prices, branch/document IDs and notes only. They must not accept `BeforeQuantity`, `AfterQuantity`, `CurrentQuantity`, `CreatedBy`, `ConfirmedBy`, Expense amount overrides or arbitrary Unit values as authoritative fields. Response DTOs may expose server snapshots.

## 15. Service Architecture

**PROPOSED:** A small application boundary is preferable to putting the ledger in controllers:

- `IInventoryService`: item master and branch balance reads;
- `IStockReceiptService`: draft/confirm receipt and Expense integration;
- `IStockIssueService`: draft/confirm issue and adjustment;
- `IInventoryDashboardService`: read-only aggregates and history.

These can be implemented as fewer concrete services if the codebase prefers, but confirmation transaction ownership must be explicit. Controllers should authenticate/authorize and delegate. Services should use `ApplicationDbContext`, one transaction boundary, and server-derived identity.

No service should call DbContext from an AI orchestrator. Future AI tools must call read-only application services.

## 16. Frontend UX and Design System

**CURRENT:** Admin routing uses React Router with pages such as `/expenses`; ExpenseManagement already uses `PageHeader`, `Button`, `FormField`, `Feedback`, table primitives, loading and empty states. No Inventory route exists.

**PROPOSED routes:** `/inventory`, `/inventory/items`, `/inventory/receipts`, `/inventory/issues`, `/inventory/history`. Use the existing navbar/role guard conventions; do not add a new state-management library or design system.

### Main screen wireframe

```text
PageHeader: Kho hàng                         [Nhập hàng] [Xuất hàng]
[Branch selector if authorized] [Month] [Item] [Status]
KPI: Tổng nhập | Tổng xuất | Sắp hết | Hết hàng | Chi phí nhập
Table: Item | Unit | Opening | In | Out | Adjusted | Current | Minimum | Status | Actions
Mobile: filters collapse; KPI cards stack; table becomes horizontally scrollable/card rows.
```

### Receipt UX

Branch -> optional supplier -> lines with InventoryItem, readonly unit, quantity, unit price -> calculated subtotal/total preview -> note -> Save Draft/Confirm. The UI may preview totals, but the backend recalculates and persists stock/Expense.

### Issue UX

Branch -> item -> server-provided current balance/unit -> quantity -> reason/note -> Confirm. Quantity over current balance is rejected by the backend. The client check is only a usability aid.

### History UX

Date/time, type, quantity, before, after, branch, actor display, reference and reason. Do not show passwords, hashes or secret claims.

## 17. Dashboard and Monthly Calculation

**PROPOSED formulas:**

- `OpeningStock = SUM(IN) - SUM(OUT) + SUM(ADJUSTMENT_SIGNED)` before the month start, or the first trusted ledger balance when bootstrapping.
- `ReceivedThisMonth = SUM(IN)` in the branch/month.
- `IssuedThisMonth = SUM(OUT)` in the branch/month.
- `AdjustedThisMonth = SUM(ADJUSTMENT_SIGNED)` in the branch/month.
- `CurrentStock = BranchInventory.CurrentQuantity`.

All aggregates filter by effective BranchId and UTC month boundaries. A transaction should carry immutable `CreatedAtUtc` and signed interpretation by Type; do not recalculate history from current settings.

Recommended indexes are `(BranchId, InventoryItemId, CreatedAtUtc)`, `(BranchId, CreatedAtUtc, Type)`, and the unique balance key. For large histories, pre-aggregated monthly snapshots can be a later optimization; correctness comes first.

Status:

- `OUT_OF_STOCK` when `CurrentQuantity <= 0`;
- `LOW_STOCK` when `CurrentQuantity <= MinimumStock` and above zero;
- `IN_STOCK` otherwise.

The existing NotificationService and branch SignalR groups could support low-stock notifications later. Notification integration is deferred from phase 1.

## 18. Audit Trail

`StockTransaction` must answer who, what, when, where, before, after, reason and reference. `CreatedBy`/`ConfirmedBy` come from the authenticated `ClaimTypes.NameIdentifier` and are resolved server-side to Employee display data for read DTOs. Client-supplied usernames are not authoritative.

Confirmed documents and ledger rows are append-only. Do not delete history to correct an error. The branch ID on each ledger row is stored explicitly for safe filtering and forensic traceability even though it can be related through the document.

## 19. SignalR and AI

**SignalR:** Current `/kitchenHub` is authenticated and groups clients by role/branch, but some order/payment broadcasts are broader than ideal. Inventory realtime is **DEFERRED** in phase 1. If later added, emit a minimal `InventoryChanged`/`LowStockDetected` event to `branch:{branchId}` groups only, after transaction commit.

**AI:** No Inventory AI tool is proposed for phase 1. Future read-only tools can call `IInventoryDashboardService`/`IInventoryService`, accept verified item/branch identifiers, derive branch scope from the authenticated AI context, and pass through `AiPermissionService`. The AI must never access DbContext or invent IDs/quantities.

## 20. Security Threat Model

| Risk | Severity | Related current pattern | Required mitigation |
|---|---|---|---|
| IDOR / BranchId tampering | Critical | controllers use request branch filters | resolve branch from JWT; admin-only cross-branch; test every endpoint |
| RestaurantId spoofing | High | no current Restaurant model | do not accept/store invented tenant IDs; use BranchId until tenant model exists |
| Role escalation via Position | High | JWT contains both role and position | authorize only ClaimTypes.Role; never Position |
| Negative/zero/decimal abuse | High | Expense validates amount, no stock model | validate positive quantity, unit scale and server decimal bounds |
| Double confirm | Critical | Expense has no idempotency | Draft/Confirmed state, idempotency key, unique references and transaction |
| Negative stock/race | Critical | no current stock locking | PostgreSQL row lock plus transaction and sufficient-stock check |
| Lost update | High | no Inventory concurrency token | `FOR UPDATE`; optional row version for draft edits |
| Expense duplication | Critical | direct Expense POST, no link uniqueness | unique `Expense.StockReceiptId` and same transaction as receipt confirm |
| Confirmed document edit/delete | High | Expense supports PUT/DELETE | Inventory confirmed docs/ledger are immutable; reversal/adjustment only |
| Client audit identity | High | current Expense derives CreatedBy correctly | derive all Inventory actors from JWT; ignore client actor fields |
| Cross-branch dashboard/history | Critical | branch guards vary by controller | branch predicate in service queries and integration tests |
| Mass assignment | High | some controllers bind entities directly | use narrow DTOs; never bind balance/status/audit fields |
| Secret exposure | Critical | secure JWT/User Secret pattern exists | do not return password/hash/JWT secret; redact errors/logs |

## 21. Migration Impact

No migration was created. A future approved implementation would most likely add **7 tables**:

1. `InventoryItems`
2. `BranchInventories`
3. `StockReceipts`
4. `StockReceiptItems`
5. `StockIssues`
6. `StockIssueItems`
7. `StockTransactions`

It would also alter `Expenses` with nullable `StockReceiptId`, a foreign key and a unique filtered index. It would need indexes and constraints described above, including decimal precision, positive quantity checks, status/unit checks, unique `(BranchId, InventoryItemId)`, and restricted delete behavior.

**NEW MIGRATION REQUIRED:** yes, later, from the canonical migration tree. Never modify an applied historical migration. Before generating it, inspect the dirty worktree and current snapshot; review for destructive operations.

No existing table should be repurposed as stock. No Payroll table/entity should be reintroduced.

## 22. Test Plan

### Backend unit/integration

- create InventoryItem;
- duplicate normalized item handling;
- invalid/unsupported unit;
- unit change after history rejected;
- draft receipt leaves stock and Expense unchanged;
- confirmed receipt increases stock exactly once;
- receipt double confirm is idempotent/conflict-safe;
- Expense created exactly once and linked;
- issue confirm decreases stock;
- issue above stock rejected;
- cross-branch receipt/issue/history/dashboard rejected;
- employee cannot access an unauthorized branch;
- concurrent issues cannot produce negative stock;
- adjustment creates before/after ledger evidence;
- monthly opening/in/out/adjustment calculations;
- low-stock and out-of-stock thresholds;
- confirmed document cannot be edited/deleted;
- inactive item behavior;
- history actor/reference traceability;
- Payroll remains absent and BasicSalary/EmployeeType remain unrelated employee fields.

### Frontend/manual

Create item, receive draft, confirm receipt, confirm issue, over-issue error, low-stock display, branch switching, month/item/status filters, history detail, loading/empty/error states, responsive mobile layout, double-click confirm, and role-specific visibility.

## 23. Phased Implementation Plan

### Phase 1 — domain/data model and migration

Likely files: `Domain/Entities`, `ApplicationDbContext`, entity configuration, one new migration, DTO contracts. Risk: schema/data integrity and Expense link. Verification: migration SQL review, isolated database, model snapshot, constraint tests. Rollback: do not edit history; use a reviewed down plan only before application or a forward correction migration.

### Phase 2 — backend stock core

Likely files: Inventory/Stock services, controllers, authorization tests. Risk: branch isolation and concurrency. Verification: transaction/row-lock tests, authorization matrix, negative stock tests.

### Phase 3 — Expense integration

Likely files: Expense entity/configuration/service/controller, receipt confirmation service, dashboard queries. Risk: double counting and duplicate Expense. Verification: one-to-one link, repeated confirm, dashboard reconciliation.

### Phase 4 — frontend master/receipts/issues

Likely files: admin routes, Inventory pages, typed API client/types, navbar entry. Risk: trusting client totals or branch selector. Verification: backend-authoritative responses, role/branch UX, responsive manual tests.

### Phase 5 — dashboard/history/low-stock

Likely files: dashboard service/controller/page, history table, optional notification boundary. Risk: opening-balance and time-zone errors. Verification: seeded ledger fixtures and UTC month tests.

### Phase 6 — security/concurrency hardening

Likely files: tests and service transaction code. Risk: race behavior only visible under parallel load. Verification: concurrent confirmation tests against PostgreSQL, IDOR tests, duplicate-submit tests.

### Phase 7 — isolated DEV E2E

Run only after migration/schema review and the existing DEV target safety procedure. Verify receiving, issuing, Expense link, dashboard, branch isolation and recovery behavior. No production/main database.

## 24. Out of Scope / Future

Recipe/BOM, automatic ingredient deduction from Product sales, full supplier management, purchase orders, barcode scanning, branch warehouse transfers, lot/batch/expiry tracking, FIFO/LIFO costing, accounting ledger, Payroll, AI write actions and advanced forecasting are **FUTURE**. No Payroll functionality is added or implied by Inventory.

## 25. Open Decisions

1. Should operational employees confirm their own StockIssues, or should manager/admin confirmation remain mandatory?
2. Should the initial unit allowlist be a code constant/check constraint or be introduced as a Unit master table?
3. What is the approved Expense category label for stock receiving, and should the existing category allowlist be changed centrally?
4. Should `ExpenseDate` equal receipt confirmation time or allow a separately entered invoice date?
5. Should one receipt permit multiple Expenses, or is the recommended strict one-to-one link final? The safe default is one-to-one.
6. Is a global InventoryItem master acceptable across branches, or must item activation be branch-specific?
7. Should a future stock adjustment require manager approval and an explicit reason code?
8. Is the requested `OrderItems` name a business contract, or should the implementation follow the current EF table `OrderDetails`?

## Final Summary

```text
INVENTORY MODULE NEEDED: YES
INVENTORYITEM SEPARATE FROM PRODUCT: YES
BRANCH-SCOPED STOCK: YES
STOCK HISTORY REQUIRED: YES
EXPENSE INTEGRATION: Expense.StockReceiptId nullable unique link; create once in the receipt-confirm transaction
RECOMMENDED UNIT DESIGN: controlled UnitCode string/catalog; unit derived from InventoryItem, no per-document free text
RECOMMENDED QUANTITY PRECISION: decimal(18,3); discrete units require integral quantities
NEGATIVE STOCK: FORBIDDEN
CONFIRMED DOCUMENT EDIT: FORBIDDEN
DOUBLE CONFIRM PROTECTION: Draft/Confirmed state + idempotency key/document guard + unique references + database transaction
CONCURRENCY STRATEGY: PostgreSQL transaction with deterministic BranchInventory FOR UPDATE row locks and sufficient-stock check
NEW TABLE COUNT: 7
NEW MIGRATION REQUIRED: YES, future single migration from canonical tree
SIGNALR PHASE 1: NO
AI INTEGRATION PHASE 1: NO
SAFE TO IMPLEMENT: NO - open business decisions and design approval remain
FIRST IMPLEMENTATION PHASE: Phase 1, after human approval of model/unit/permission/Expense decisions
HUMAN DECISIONS REQUIRED: issue confirmation role; unit catalog strategy; Inventory Expense category/date; one-to-one Expense rule; global vs branch item master; OrderItems vs current OrderDetails naming
```

## Verification and Not Changed

- Source audit performed against current entities, `ApplicationDbContext`, controllers, services, JWT configuration, migrations, frontend routes/components, SignalR and tests.
- No Inventory feature was implemented.
- No migration was created.
- No database, Supabase target, User Secret, production/demo configuration or historical migration was changed.
- No API with automatic migration was started and no database connection was made by this audit.
- Existing backend tests/build state was not changed by this documentation-only report.
