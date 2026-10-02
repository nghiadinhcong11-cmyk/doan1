# Inventory Phase 3 — Admin Web Frontend

## 1. Executive Summary

Implemented the Admin Web Inventory workflow against the existing Phase 2 API. The frontend does not alter stock locally, create Expenses, or duplicate backend business rules. Receipt/issue confirmation refetches backend state and uses an in-app confirmation modal.

## 2. Scope and Routes

- `/inventory` — Tổng quan kho
- `/inventory/items` — Danh mục nguyên liệu
- `/inventory/receipts` and `/inventory/receipts/:id` — Phiếu nhập
- `/inventory/issues` and `/inventory/issues/:id` — Phiếu xuất
- `/inventory/history` — Lịch sử kho

The navigation item `Kho hàng` is shown for the existing admin/manager management shell. Backend authorization remains authoritative.

## 3. Implemented UX

The overview shows item-type count, low-stock count, out-of-stock count, branch inventory, status filters, and minimum-stock editing. Inventory items support create, edit, activation/deactivation, controlled unit selection, and search without hard delete.

Receipt and issue forms support multiple lines, unit display, quantity/price input, client-side preview, duplicate-line prevention, draft creation, detail views, confirmation, cancellation, loading states, and backend error feedback. Confirmed/cancelled documents are read-only in the UI.

History uses the paginated transaction endpoint and displays IN/OUT/ADJUSTMENT labels, before/after balances, reference, actor, and filters. No adjustment action was added.

## 4. API Integration

`apps/admin-web/src/features/inventory/inventoryApi.ts` is the centralized typed API layer. It mirrors the Phase 2 DTOs, uses the existing authenticated `fetch` interceptor, and extracts safe backend validation/conflict messages. No second Expense API call is made after receipt confirmation; the UI reports that the backend recorded the inventory expense after a successful confirmation.

## 5. Authorization and Branch Context

The UI uses the existing `selectedBranchId` context and existing Navbar branch selector. Admin/manager management routes are exposed in the management shell; confirmation actions are only rendered for admin/manager. Employees/cashiers/kitchen are not granted management UI actions by this change. The server still validates every branch and role request.

## 6. KPI Read-Model Gap

Resolved in Phase 3A. The overview now consumes the bounded branch-scoped read model at `GET /api/inventory/overview/{branchId}`. Monthly movement quantities are displayed by unit, and import cost comes from confirmed receipts without a second Expense API call.

## 7. Loading, Error, Empty, Responsive, Accessibility

Pages use existing `PageHeader`, `Button`, `FormField`, `Feedback`, `Spinner`, `StatusBadge`, and table primitives. Empty/error/loading states are provided for each main list. Forms have labels, required fields, keyboard-accessible buttons, status text, and responsive grid/table layouts. Confirmation is an application modal rather than a browser alert.

## 8. Build/Test Results

- Admin Web build: PASS (`npm run build`); existing Vite chunk-size warning remains.
- Customer Web build: PASS.
- Backend build: PASS.
- Backend tests: PASS after Phase 3A, 215/215.
- Database connection/mutation: NONE from Codex.
- Migration created: NO.

## 9. Deferred Features

Monthly KPI read model, adjustment workflow, recipe/BOM, automatic deduction from sales, transfers, supplier management, barcode, SignalR inventory events, AI tools, and frontend automated tests remain deferred. The repository has no established Admin frontend test runner, so verification was static plus production build.

## 10. Phase 4 Readiness

The Admin Web workflow is ready for human-host API/E2E verification. Runtime DEV validation was not performed by Codex and remains subject to the existing PostgreSQL/host execution procedure.
