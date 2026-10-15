# Inventory Phase 2 — Backend Core

## 1. Executive Summary

Implemented the Inventory backend core on top of the validated Phase 1B schema. The implementation provides item management, branch-scoped inventory reads, receipt/issue Draft workflows, confirmation, cancellation of Draft documents, Expense integration for confirmed receipts, append-only stock transactions, pagination, authorization, branch isolation, negative-stock protection, and PostgreSQL row locking.

No migration was created, no DEV database was connected, and no business data was inserted.

## 2. Scope

Implemented:

- InventoryItem management and controlled units.
- BranchInventory overview and branch-specific MinimumStock.
- StockReceipt Draft/edit/confirm/cancel.
- StockIssue Draft/edit/confirm/cancel.
- StockTransaction history with pagination and filters.
- Receipt Expense creation and duplicate protection.
- Role and JWT branch-scope enforcement.

Deferred: frontend, Recipe/BOM, automatic deduction from Orders, adjustments, transfers, suppliers, purchase orders, barcode, unit conversion, SignalR, AI, and forecasting.

## 3. Architecture

The implementation follows the existing Domain → Application → Infrastructure → WebAPI structure:

- DTO contracts: `Application/DTOs/Inventory/InventoryDtos.cs`.
- Application service: `Application/Services/InventoryService.cs` and `IInventoryService.cs`.
- Web API surface: `WebAPI/Controllers/InventoryController.cs`.
- Persistence: existing `ApplicationDbContext` and Phase 1A entities/configuration.
- DI registration: existing `Program.cs` service registration.

No second DbContext or new framework was introduced.

## 4. Endpoints

```text
GET    /api/inventory/items
GET    /api/inventory/items/{id}
POST   /api/inventory/items
PUT    /api/inventory/items/{id}
GET    /api/inventory/branches/{branchId}
PUT    /api/inventory/branches/{branchId}/items/{inventoryItemId}/minimum-stock
GET    /api/inventory/receipts
GET    /api/inventory/receipts/{id}
POST   /api/inventory/receipts
PUT    /api/inventory/receipts/{id}
POST   /api/inventory/receipts/{id}/confirm
POST   /api/inventory/receipts/{id}/cancel
GET    /api/inventory/issues
GET    /api/inventory/issues/{id}
POST   /api/inventory/issues
PUT    /api/inventory/issues/{id}
POST   /api/inventory/issues/{id}/confirm
POST   /api/inventory/issues/{id}/cancel
GET    /api/inventory/transactions
```

All writes use request DTOs. Server-controlled quantities, totals, status, audit identity, timestamps, transaction snapshots, and Expense links are not accepted from clients.

## 5. Authorization Matrix

| Operation | admin | manager | employee | cashier | kitchen | customer |
|---|---:|---:|---:|---:|---:|---:|
| Item management | Yes | Yes | No | No | No | No |
| Branch inventory/minimum stock | Global | Own branch | Own branch read | Own branch read | Own branch read | No |
| Receipt Draft/edit/confirm | Yes | Own branch | No | No | No | No |
| Issue Draft/edit/cancel | Yes | Own branch | Own branch | Own branch | Own branch | No |
| Issue confirm | Yes | Own branch | No | No | No | No |
| History/documents read | Scoped | Own branch | Own branch | Own branch | Own branch | No |

Role values come from the authenticated JWT. Request BranchId is always checked against the JWT branch claim for non-admin users.

## 6. Branch Isolation

Every branch-scoped service operation calls the same branch authorization path. Admin may select a branch; manager and operational roles must have a matching `branchId` claim. A request body BranchId cannot expand access.

## 7. Inventory Item Rules

- `Name` is trimmed and `NormalizedName` is generated server-side.
- `UnitCode` is normalized and checked against the Phase 1A catalog.
- Active normalized-name duplicates are rejected before persistence; the database unique filtered index remains the final boundary.
- No hard-delete endpoint exists. Deactivation uses `IsActive`.
- No Product relationship or automatic sales deduction was added.

## 8. Receipt State Machine

```text
Draft ──confirm──> Confirmed
Draft ──cancel───> Cancelled
Confirmed/Cancelled ──business edit──> forbidden
```

Draft creation/edit does not affect stock, ledger, or Expense. Confirmation recalculates totals and applies all effects atomically. Confirmed receipts are idempotent: a repeated confirmation returns without repeating effects.

## 9. Issue State Machine

```text
Draft ──confirm──> Confirmed
Draft ──cancel───> Cancelled
Confirmed/Cancelled ──business edit──> forbidden
```

Issue confirmation rejects the entire operation when any requested item exceeds available stock. No Expense is created for an issue.

## 10. Expense Integration

Receipt confirmation creates exactly one Expense with:

- category `Nhập hàng`;
- amount equal to server-calculated receipt total;
- `StockReceiptId` equal to the receipt ID;
- purchase date when supplied, otherwise confirmation time;
- deterministic human-readable description.

The unique database link is supplemented by an application check. Linked Inventory Expenses are now immutable through the existing Expense update/delete endpoints so the receipt-to-Expense invariant cannot be broken by ordinary CRUD.

## 11. Ledger Design

Receipt lines create `IN` transactions. Issue lines create `OUT` transactions. Each transaction stores branch/item, positive quantity, before/after balance, reference type/ID, authenticated creator, timestamp, and an optional note. No ledger update/delete endpoint exists. `ADJUSTMENT` remains reserved.

## 12. Negative Stock Prevention

Issue confirmation validates every item before changing any balance. Missing `BranchInventory` is treated as zero stock. Insufficient stock returns a conflict with item name, available quantity, and requested quantity. The database non-negative check remains a second boundary.

Low-stock response values are derived:

- `IsOutOfStock`: `CurrentQuantity <= 0`;
- `IsLowStock`: `MinimumStock > 0 && CurrentQuantity <= MinimumStock`.

## 13. PostgreSQL Locking Strategy

Confirmations use one EF database transaction. On Npgsql, affected rows are processed in ascending `InventoryItemId` order and selected with PostgreSQL `FOR UPDATE`. Receipt confirmation safely creates missing branch rows using `ON CONFLICT DO NOTHING` before locking. Issue confirmation never creates a missing stock row.

SQLite unit tests use a provider fallback for business-rule testing only. They do not claim to prove PostgreSQL row-lock or concurrent-transaction behavior. A PostgreSQL integration test remains required for the strongest concurrency guarantee.

## 14. Idempotency

Receipt and issue creation honor the existing nullable branch-scoped `IdempotencyKey` indexes. A repeated create with the same branch/key returns the existing document. Confirmation is status/idempotency safe and never reapplies stock, ledger, or Expense after `Confirmed`.

## 15. Transaction Atomicity

Receipt confirmation wraps row locks, balance changes, transactions, Expense creation, status, and audit fields in one database transaction. Issue confirmation wraps row locks, all-stock validation, balance changes, transactions, and status in one transaction. Failure before commit rolls back all persisted effects.

## 16. Error Semantics

The controller maps service errors to existing HTTP conventions:

- validation → 400;
- missing resource → 404;
- branch/role denial → 403;
- state, duplicate, and insufficient-stock conflicts → 409.

Database exception details are not returned to clients.

## 17. Test Coverage

Added service-level SQLite tests for:

- valid/invalid units and normalized duplicate items;
- manager cross-branch denial;
- receipt Draft side-effect isolation;
- receipt confirmation, server total, Expense and ledger creation;
- repeated receipt confirmation idempotency;
- receipt atomicity when the Expense link already exists;
- issue atomic rejection when one line is insufficient;
- successful issue balance/ledger/no-Expense behavior;
- employee Draft creation without confirmation permission.

The test suite does not fake PostgreSQL `FOR UPDATE` concurrency. That belongs in a PostgreSQL integration test against disposable infrastructure.

## 18. Deferred Features

Frontend, dashboards, SignalR, AI tools, Recipe/BOM, automatic Product deduction, adjustments/reversals, supplier management, transfers, conversion, barcode, lots/expiry, costing, and advanced reporting remain out of scope.

## 19. Known Limitations

- No PostgreSQL concurrency integration test was run in this task.
- No host DEV API/E2E run was performed.
- Receipt/issue list access is available to operational roles within their JWT branch scope; a future policy decision may narrow document visibility further.
- Item duplicate race handling relies on the validated database unique index in addition to the application pre-check.
- Existing Expense dashboard/category behavior was not broadly refactored.

## 20. Phase 3 Readiness

Backend core is ready for human-controlled DEV E2E review. Frontend Phase 3 can consume the DTO/API contracts after confirming host-side flows with authenticated admin/manager and operational-role scenarios. No migration is required for Phase 3.

## Verification

```text
NEW MIGRATION: NO
DATABASE CONNECTION: NO
DATABASE MUTATED: NO
BACKEND BUILD: PASS
BACKEND TESTS: 214/214 PASS
```

No commit or push was performed.
