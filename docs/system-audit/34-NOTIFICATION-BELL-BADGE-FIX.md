# Notification Bell Badge Fix

Date: 2026-09-30

## Scope

The POS/Admin bell badge now represents actionable web/QR orders awaiting staff acceptance. Persistent notifications remain available in the notification dropdown and are not used as the pending-order badge source.

## Implementation

### Files changed

- `apps/admin-web/src/features/notifications/pendingOrderQuery.ts`
- `apps/admin-web/src/components/Navbar.tsx`
- `apps/admin-web/src/components/CashierNavbar.tsx`
- `apps/admin-web/src/features/pos/pages/POSPage.tsx`
- `services/api/src/WebAPI/Controllers/OrderController.cs`

The listed files already contained unrelated working-tree changes before this task; those changes were preserved.

### Badge source

Old source:

```text
GET /api/Notification
→ local notifications state
→ notifications.filter(n => !n.isRead).length
```

New source:

```text
GET /api/Order?status=Đang xử lý&branchId=<selected branch when applicable>
→ filter status === Đang xử lý
→ filter CreatedBy is null/empty
→ count
```

The query is shared by `Navbar` and `CashierNavbar` through `pendingOrderQuery.ts`. It reuses the existing `GET /api/Order` endpoint and does not introduce a second server-side business definition.

### Pending order definition

```text
Order.Status == "Đang xử lý"
AND Order.CreatedBy is null or empty
AND the existing OrderController branch authorization/filter applies
```

This matches the existing POS acceptance flow. Accepting an order updates that same `Order` through `POST /api/Order/{id}/accept`; it does not create another order.

### Branch scoping

- Branch-scoped users continue to be constrained by the JWT `branchId` through `OrderController.TryResolveBranch`.
- The selected branch is sent for the global admin UI when present, preserving the existing selected-branch behavior.
- Backend realtime notifications are sent to `kitchen-admin` for admin users and `branch:{branchId}:role:{role}` groups for manager, employee, and cashier users.
- Kitchen groups are not included; Kitchen KDS behavior is unchanged.

### Navbar shared logic

Both `Navbar` and `CashierNavbar` now use `fetchPendingWebOrderCount`. `CashierNavbar` no longer replaces its persistent notification list with a separate combined order/reservation count implementation.

The bell badge renders only when the pending count is greater than zero. Its accessible label also describes the pending-order count. The notification dropdown/history remains backed by `/api/Notification`.

### SignalR and refresh strategy

`OrderController` emits `PendingOrderChanged` after:

- a newly created order is in pending status with no `CreatedBy`;
- a web order is successfully accepted.

Clients treat this event as invalidation only and refetch the canonical pending count. The POS also dispatches a local pending-state event after successful accept and cancel, so the current page updates immediately even without relying on the server event.

There is no event payload count and no blind `count++` or `count--` logic. A 30-second refetch remains as a fallback.

### Notification history

Preserved. Notification records, unread state, notification dropdown content, and read actions were not removed. The badge and the notification history intentionally have separate semantics.

### Kitchen and database impact

- Kitchen `OrderRequest` lifecycle: unchanged.
- Kitchen SignalR events/protocol: unchanged.
- Order state machine: unchanged.
- Database schema: unchanged.
- Migration added: none.
- Database update/seed/runtime E2E: not run.

## Verification

- Admin build: PASS (`npm run build`).
- Customer build: PASS (`npm run build`).
- Backend isolated build: PASS (`dotnet build` using a temporary output path; 0 warnings, 0 errors).
- Backend tests: PASS, `177/177`.
- `git diff --check`: no whitespace error in the task changes; pre-existing unrelated EOF warning remains in `baocaodoan/06_Quan_ly_don_hang_POS.md`.
- Runtime E2E: NOT RUN, as required. No current Supabase/database was used.

The normal backend build output was also attempted but could not replace the running `RestaurantPOS.api.exe`/DLL because process `RestaurantPOS.api (11428)` held the files. Rebuilding to an isolated temporary output path verified compilation successfully without stopping or altering that process.

## Static regression

The implementation does not change customer QR ordering, POS acceptance semantics, Kitchen processing, payment rules, authentication, or branch authorization. It only adds an invalidation event and changes the frontend badge source to the existing pending web-order query.

## Remaining risks

1. Browser runtime verification of SignalR group delivery and badge transitions remains pending.
2. The admin selected-branch behavior still depends on the existing `selectedBranchId` local-storage convention.
3. No frontend unit-test harness exists for Navbar state behavior; verification was by TypeScript/build, source tracing, and backend tests.

## Result

```text
FILES CHANGED:
shared pending-order query, Navbar, CashierNavbar, POSPage, OrderController, this report

BADGE OLD SOURCE:
unread persistent Notification count

BADGE NEW SOURCE:
server pending web/QR Order count

BADGE SEMANTICS:
PENDING ACTIONABLE WEB/QR ORDERS

PENDING ORDER DEFINITION:
Status = Đang xử lý AND CreatedBy empty/null, with existing branch scope

BRANCH SCOPING:
existing OrderController authorization/filter plus branch-scoped SignalR groups

NAVBAR SHARED LOGIC:
fetchPendingWebOrderCount in pendingOrderQuery.ts

SIGNALR STRATEGY:
PendingOrderChanged → refetch canonical count

BLIND COUNT +/-:
NO

NOTIFICATION HISTORY PRESERVED:
YES

KITCHEN LOGIC CHANGED:
NO

DATABASE CHANGED:
NO

MIGRATION ADDED:
NO

BACKEND BUILD:
PASS (isolated output path; normal output was file-locked by running API)

BACKEND TESTS:
177/177

ADMIN BUILD:
PASS

CUSTOMER BUILD:
PASS

STATIC REGRESSION:
PASS

RUNTIME E2E:
NOT RUN
```
