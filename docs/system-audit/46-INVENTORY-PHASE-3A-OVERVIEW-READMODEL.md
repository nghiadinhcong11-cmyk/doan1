# Inventory Phase 3A — Overview Read Model

## 1. Problem

The Admin inventory overview previously displayed branch balances but could not safely display monthly received quantity, issued quantity, or import cost. Calculating those values in the browser would require loading unbounded transaction history and would incorrectly add heterogeneous units together.

## 2. Endpoint

Added the read-only endpoint:

`GET /api/inventory/overview/{branchId}`

The existing `InventoryService` branch authorization is applied before any query. Managers are restricted to their assigned branch; admins may select an authorized branch according to the existing conventions.

## 3. Response and semantics

`InventoryOverviewResponse` returns active-item, low-stock, and out-of-stock counts; received and issued quantities grouped by the stored `UnitCode`; import cost; the UTC period; and a low-stock list.

The dashboard uses mutually exclusive counts:

- out of stock: `CurrentQuantity == 0`;
- low stock: `CurrentQuantity > 0`, `MinimumStock > 0`, and `CurrentQuantity <= MinimumStock`.

The low-stock list is limited to 10 rows, ordered by current quantity and then name. It contains only non-zero low-stock rows; the out-of-stock KPI remains separate.

## 4. Monthly period and sources

The period is the current UTC calendar month represented as `[PeriodStartUtc, PeriodEndUtc)`. Received and issued quantities come from `StockTransactions` and use `CreatedAtUtc`, grouped by the related `InventoryItem.UnitCode`. `ADJUSTMENT` transactions are excluded.

Import cost comes only from confirmed `StockReceipts` whose `ConfirmedAtUtc` is in the same period. This avoids double-counting linked Expenses and follows the operational confirmation event.

## 5. Query strategy

Production PostgreSQL queries use `AsNoTracking`, server-side grouping, joins, and aggregate `SUM` operations. The overview does not load full transaction or receipt history. Active item balances are projected with a left join so an active item without a branch balance is represented as zero stock.

The SQLite test provider cannot translate `SUM(decimal)` consistently, so tests use a bounded current-month fallback only in the service's SQLite provider branch. Production PostgreSQL remains server-aggregated.

## 6. Frontend integration

The existing Admin overview now requests the branch balances and overview read model together. KPI cards use backend counts, monthly quantities render one line per unit, and import cost uses the existing Vietnamese currency formatting. No Expense API call was added; receipt confirmation remains the sole inventory confirmation call.

## 7. Tests and safety

Added a service regression test covering active counts, mutually exclusive low/out-of-stock cards, mixed `kg` and `piece` quantities, OUT separation, ADJUSTMENT exclusion, import cost, and the low-stock list.

No migration was created, no schema was changed, and no database connection or mutation was performed by Codex.

## 8. Phase 4 readiness

The monthly KPI read-model gap is resolved for the current overview. Frontend end-to-end verification against the authorized DEV host remains a later human-controlled activity.
