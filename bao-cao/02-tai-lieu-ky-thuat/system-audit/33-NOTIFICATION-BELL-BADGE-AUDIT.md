# Notification Bell Badge Audit

Audit date: 2026-09-30  
Mode: source-code audit only  
Database/API runtime: not used

## 1. Executive finding

The reported “virtual order” is not a separate virtual-order entity in the current source.

There are two different concepts:

1. **Pending web/customer order:** an `Order` created through `POST /api/Order` with status `Đang xử lý` and normally an empty `CreatedBy`. Staff claims it through `POST /api/Order/{id}/accept`. The same `Order` is updated with `CreatedBy` and a real table/takeaway context; a second official `Order` is not created.
2. **Kitchen request:** an `OrderRequest` created by `KitchenService.SendToKitchenAsync` from an existing `Order`. It is a batch of dishes sent to the kitchen and has statuses `Pending`, `Preparing`, `Ready`, `Completed`, `Cancelled`. It is not converted into an official `Order`.

The bell badge is currently based on unread persistent `Notification` records, not on actionable pending web `Order` records and not on active `OrderRequest` records. The accept flow does not update or mark related `Notification` records as read, and it emits no notification-count synchronization event. This is the primary reason the badge can remain unchanged after staff accepts a pending web order.

## 2. Exact domain flow

### Customer QR/web order

```text
Customer DigitalMenu
  -> POST /api/Order
  -> OrderService.CreateOrUpdateOrderAsync
  -> Order.Status = "Đang xử lý"
  -> Order.CreatedBy remains empty for a guest/web order
  -> POS fetches pending web orders
```

Evidence:

- `apps/customer-web/src/pages/DigitalMenu.tsx:316-355` sends the order.
- `services/api/src/WebAPI/Controllers/OrderController.cs:89-150` accepts the order request.
- `services/api/src/Application/Services/OrderService.cs:218-244` forces new orders to `Đang xử lý`.
- `services/api/src/Application/Services/OrderService.cs:404-406` defines a pending web order as the order with status `Đang xử lý` and empty `CreatedBy`.

### Staff acceptance

```text
POS “Xử lý đơn”
  -> POST /api/Order/{id}/accept
  -> OrderController.AcceptWebOrder
  -> OrderService.AcceptWebOrderAsync
  -> conditional claim of the same Order
  -> sets TableName and CreatedBy
  -> POS removes it from local pendingOrders
```

Evidence:

- `apps/admin-web/src/features/pos/pages/POSPage.tsx:997-1056`.
- `services/api/src/WebAPI/Controllers/OrderController.cs:158-187`.
- `services/api/src/Application/Services/OrderService.cs:387-450`.

The conversion does not:

- delete an `OrderRequest`;
- update an `OrderRequest` status;
- mark a `Notification` as read;
- delete a `Notification`;
- emit `RequestAccepted`, `OrderClaimed`, `NotificationRead` or equivalent;
- create a second official `Order`.

### Kitchen request

```text
Existing Order
  -> POST /api/Order/{id}/send-to-kitchen
  -> KitchenService.SendToKitchenAsync
  -> creates OrderRequest + OrderRequestItem
  -> emits NewOrderRequest
  -> creates persistent ORDER_CREATED Notification for kitchen/admin
```

Evidence:

- `services/api/src/WebAPI/Controllers/OrderController.cs:194-205`.
- `services/api/src/Application/Services/KitchenService.cs:26-101`.
- `services/api/src/Domain/Entities/OrderRequest.cs:7-35`.

Kitchen status changes use:

```text
PATCH /api/Order/kitchen/requests/{id}/status
```

and update only `OrderRequest.Status`, timestamps and `ProcessedBy`; they do not update related persistent notifications.

## 3. Bell components and state ownership

### Primary management bell

| Item | Finding |
|---|---|
| Component | `apps/admin-web/src/components/Navbar.tsx` |
| Used by | Admin/manager/default management shell in `apps/admin-web/src/App.tsx:154-187` |
| State | Local `notifications: any[]` state |
| Initial source | `GET /api/Notification` at `Navbar.tsx:105-110` |
| Refresh | Initial load, every 30 seconds, `refresh-notifications` window event |
| Realtime | SignalR `/kitchenHub`, event `NotificationCreated` |
| Badge expression | `notifications.filter(n => !n.isRead).length` at `Navbar.tsx:375-378` |
| Persistence | No localStorage/sessionStorage for count or notification list |
| Cleanup | `connection.off('NotificationCreated', onNotification)` and `window.removeEventListener(...)` are present |

`Navbar.fetchNotifications()` at `Navbar.tsx:74-103` derives notifications from pending `/api/Order` rows, but the mounted effect calls `fetchPersistentNotifications()` instead. The derived function is therefore not the normal initial badge source.

### Cashier bell

| Item | Finding |
|---|---|
| Component | `apps/admin-web/src/components/CashierNavbar.tsx` |
| Used by | Cashier shell in `apps/admin-web/src/App.tsx:107-127` |
| State | Independent local `notifications: any[]` state |
| Initial source | `GET /api/Notification` at `CashierNavbar.tsx:86-93` |
| Refresh | Initial load and every 30 seconds |
| Realtime | SignalR `NotificationCreated` |
| Badge expression | `notifications.filter(n => !n.isRead).length` at `CashierNavbar.tsx:134-137` |
| Event refresh behavior | `refresh-notifications` calls the different `fetchNotifications()` function, not the persistent notification fetch |
| Cleanup | SignalR and window event cleanup are present |

This creates a duplicated and inconsistent state model: management Navbar and CashierNavbar both own separate notification lists and interpret the same bell differently after a refresh event.

### Kitchen role

`KitchenNavbar.tsx` has no bell or notification badge. Kitchen uses `KitchenPage.tsx` request cards and the `NewOrderRequest`/`RequestStatusUpdated` events directly.

### POS notification tab

`POSPage.tsx:1461-1515` displays `pendingOrders.length` for “Đơn hàng chờ duyệt”. This is a separate local count from the Navbar bell and is based on pending web orders plus reservation state. It is not the persistent notification count.

## 4. Current badge semantics

### Backend query

`NotificationController.Get` calls:

```text
NotificationService.GetForUserAsync(role, userId, branchId, unreadOnly: false)
```

`NotificationService.GetForUserAsync`:

- filters target role;
- filters target user;
- filters `BranchId` using `(n.BranchId == null || n.BranchId == branchId)`;
- does not filter `EntityType`, `EntityId`, notification type or actionable status;
- returns up to 100 records;
- returns read and unread records.

The frontend then counts only records where `IsRead` is false.

### Actual meaning

```text
BELL BADGE SEMANTICS:
Number of unread persistent Notification rows returned for the current role/user/branch scope, capped at 100 records before the frontend count.
```

It is not:

- the number of pending web/customer `Order` rows;
- the number of active `OrderRequest` rows;
- the number of unprocessed customer orders;
- the number of requests that still require POS acceptance.

### Expected business semantics for the reported bug

```text
EXPECTED BUSINESS SEMANTICS:
Number of actionable pending web/customer orders awaiting staff acceptance, or a clearly separate unread-notification count whose lifecycle is explicitly marked read when the corresponding action is completed.
```

The actual and expected semantics differ. This is primarily a notification-semantics/design bug, exposed by a missing synchronization/lifecycle update.

## 5. Notification creation and lifecycle

### `OrderRequest` notification creation

`KitchenService.SendToKitchenAsync` creates two persistent notifications for each request:

```text
Type: ORDER_CREATED
EntityType: OrderRequest
EntityId: orderRequest.Id
TargetRole: kitchen and admin
BranchId: order.BranchId
Route: /kitchen
IsRead: default false
```

`NotificationService.CreateAsync` persists the record, then emits `NotificationCreated` to SignalR groups.

### Notification read lifecycle

Notifications are marked read only when a user clicks a bell item:

```text
PATCH /api/Notification/{id}/read
```

The backend method `MarkReadAsync` sets `IsRead = true`. There is also a `POST /api/Notification/read-all` endpoint, but the bell components shown here do not use it for order acceptance.

### Missing conversion lifecycle

`AcceptWebOrderAsync` changes the pending web `Order` but has no dependency on `INotificationService`, no notification lookup by order/entity, and no notification event. Therefore any existing unread persistent notification remains unread after acceptance.

`UpdateRequestStatusAsync` changes `OrderRequest.Status` and emits `RequestStatusUpdated`, but similarly does not mark or resolve `Notification` records associated with that request.

## 6. SignalR audit

### Existing events

| Event | Sender | Receiver | Trigger | Payload | Badge effect |
|---|---|---|---|---|---|
| `NotificationCreated` | `NotificationService.CreateAsync` | Navbar/CashierNavbar | Persistent notification creation | Notification entity | Prepends local item; no count reconciliation |
| `NewOrderRequest` | `KitchenNotifier.NotifyNewOrderRequestAsync` | Kitchen clients | New `OrderRequest` created | request payload | Kitchen request list only |
| `RequestStatusUpdated` | `KitchenNotifier.NotifyRequestStatusUpdatedAsync` | Kitchen/POS clients | OrderRequest status update | `{ id, status }` | No Navbar badge handling |
| `PaymentCompleted` | Order controller/payment path | POS/invoice consumers | Payment completion | payment payload | Not related to request badge |
| conversion event | None found | None | `AcceptWebOrderAsync` success | None | No bell update |

### Event flow comparison

```text
CUSTOMER SUBMITS WEB ORDER
  -> Order created with pending status
  -> no NotificationCreated event from CreateOrder path
  -> POS discovers it by polling/fetching /api/Order

STAFF ACCEPTS WEB ORDER
  -> same Order claimed by setting CreatedBy/TableName
  -> POS local pendingOrders removes it and refetches orders
  -> refresh-notifications window event is dispatched
  -> Navbar refetches persistent notifications only
  -> persistent Notification rows are unchanged
  -> badge remains based on old unread rows
```

For kitchen requests:

```text
ORDER -> OrderRequest
  -> NotificationCreated + NewOrderRequest
OrderRequest status update
  -> RequestStatusUpdated
  -> Notification row remains unchanged
```

## 7. Branch isolation

### API query

`NotificationService.GetForUserAsync` does apply a branch predicate from the authenticated claim passed by `NotificationController`. For branch-scoped roles, this is a branch-aware query.

### Frontend/API mismatch risks

- `Navbar.fetchPersistentNotifications()` does not send the selected branch from `localStorage`; it relies entirely on the JWT `branchId` claim.
- The Navbar allows local branch selection and reloads the page, but the badge source is not explicitly tied to that selected branch.
- Admin users may have no branch claim and are therefore not equivalent to a branch-scoped manager/cashier query. Their notification visibility depends on the notification target/group rules.
- `NotificationService.CreateAsync` sends branch-role notifications to `branch:{branchId}:role:{role}`. `KitchenHub.OnConnectedAsync` puts admins in `kitchen-admin` and `role:admin`, but not in every branch-role group unless their token has a branch ID.

Conclusion:

```text
BRANCH FILTER: PARTIAL / ROLE-DEPENDENT
```

The database query itself is branch-aware for a branch claim, but the overall bell behavior is not consistently aligned with the selected UI branch and admin global scope.

## 8. Duplicate state/listener audit

### Listener lifecycle

`Navbar.tsx` and `CashierNavbar.tsx` each register one `NotificationCreated` handler and remove it on cleanup. No direct evidence of an unbounded duplicate listener was found in those components.

`POSPage.tsx` and `KitchenPage.tsx` also clean up their SignalR handlers. The listener duplication risk is therefore not the primary cause.

```text
DUPLICATE LISTENER RISK: LOW / NOT PRIMARY
```

### Duplicate state ownership

There are multiple count/list sources:

- persistent notifications in `Navbar`;
- persistent notifications in `CashierNavbar`;
- derived pending orders/reservations in the unused/refresh path;
- POS `pendingOrders.length`;
- Kitchen `requests.length`/status statistics;
- server-side `Notification.IsRead`.

The same window event has different handlers in the two navbar implementations. This is a high-confidence duplicate-state/synchronization risk.

## 9. Reload behavior analysis

### Case A — Reload fixes the badge

This is possible only if the server-side notification query returns a changed set between requests, for example because another action marked notifications read or because a different navbar refresh path is used. The acceptance endpoint itself does not cause that change.

### Case B — Reload does not fix the badge

This is the expected result when the badge represents persistent unread notifications created for `OrderRequest` and no read/resolve operation occurred. Reload calls `GET /api/Notification`, which returns the same unread rows.

```text
RELOAD EXPECTED TO FIX: NO for the reported conversion lifecycle
```

This is a source-based conclusion, not runtime verification.

## 10. Root-cause classification

### Primary root cause — notification semantics mismatch

**Classification:** `NOTIFICATION SEMANTICS BUG`  
**Confidence:** HIGH  
**Evidence:**

- `Navbar.tsx:375-378` counts unread `Notification` rows.
- `NotificationService.GetForUserAsync` returns persistent notifications, not pending orders.
- `KitchenService.cs:90-99` creates `ORDER_CREATED` notifications tied to `OrderRequest`.
- `OrderService.AcceptWebOrderAsync` changes the `Order` claim state but never resolves the related notification concept.

The badge is being interpreted as “pending order requests” by the user, while the code implements “unread notification records”.

### Secondary root cause — missing conversion synchronization/lifecycle update

**Classification:** `FRONTEND STATE BUG` + `CACHE INVALIDATION BUG`  
**Confidence:** HIGH  
**Evidence:**

- POS removes the accepted order locally at `POSPage.tsx:1051` and refetches POS pending orders.
- POS dispatches `refresh-notifications` at `POSPage.tsx:1055`.
- `Navbar` responds by refetching persistent notifications, but the backend rows are unchanged.
- No accept/processed SignalR event or notification mark-read call exists.

The local POS list and Navbar badge have separate sources of truth and are not invalidated together.

### Additional root cause — inconsistent navbar refresh paths

**Classification:** `DUPLICATE STATE BUG`  
**Confidence:** HIGH  
**Evidence:**

- `Navbar` refresh event calls `fetchPersistentNotifications()`.
- `CashierNavbar` refresh event calls derived `fetchNotifications()`.
- Derived notification objects do not set `isRead`, so the badge treats every derived item as unread.

This can make admin/manager and cashier badges behave differently after the same POS action.

### Not identified as primary

- `BACKEND STATUS BUG`: not proven for web-order acceptance; the accepted `Order` is intentionally still `Đang xử lý` and is claimed by `CreatedBy`.
- `SIGNALR EVENT BUG`: there is no conversion event, but the core failure is the missing lifecycle/semantic contract rather than a broken existing event.
- `DUPLICATE LISTENER BUG`: cleanup exists; no high-confidence leak found.
- `LOCALSTORAGE BUG`: no badge persistence found.

## 11. Recommended minimal fix — not implemented

### Preferred semantic design

Choose and document one of these explicitly:

1. **Actionable-order badge:** bell count is a branch-scoped query of pending web/customer `Order` rows (`Status = Đang xử lý` and empty `CreatedBy`). Acceptance removes the row from the count.
2. **Unread-notification badge:** keep `Notification.IsRead` as the count source, but conversion/status actions must resolve the related notification by `EntityType`/`EntityId` and emit a synchronization event or trigger a refetch.

For the reported UX (“orders waiting for staff acceptance”), option 1 is the more accurate semantic model. Existing system notifications such as Business Insights should remain a separate notification category rather than being mixed into the pending-order badge.

### Minimal safe implementation direction

- Establish one canonical pending-action query/API for the bell.
- Keep server/database state authoritative.
- On successful `AcceptWebOrderAsync`, refresh or invalidate the canonical pending-order query.
- If persistent notifications remain in the same bell, resolve only the notification linked to the accepted entity; do not mark unrelated notifications read.
- Use SignalR only to trigger a refetch or deliver a typed order-accepted event; do not blindly execute `count--`.
- Keep branch scope server-enforced.
- Render no badge when the authoritative count is zero.
- Make `Navbar` and `CashierNavbar` use the same semantic source and refresh behavior.

### Regression risks

- Customer QR order submission must continue creating pending `Order` rows.
- POS acceptance must remain concurrency-safe and branch-scoped.
- Kitchen `OrderRequest` flow must not be confused with web-order acceptance.
- Existing BusinessInsight/Product notifications must not disappear when an order is accepted.
- Admin global notification visibility and manager/cashier branch isolation need separate tests.
- SignalR group routing must not expose another branch's notification.

## 12. Proposed test plan

| Test | Expected result |
|---|---|
| 1. Zero pending web orders | No actionable-order badge |
| 2. One QR/web order | Badge = 1 |
| 3. Four QR/web orders | Badge = 4 |
| 4. Accept one | Badge 4 → 3 |
| 5. Accept all | Badge disappears / count 0 |
| 6. Reload after all accepted | Badge remains absent |
| 7. New order after zero | Badge = 1 |
| 8. Branch A order | Branch B unaffected |
| 9. SignalR reconnect | No duplicate increment |
| 10. Acceptance API failure | Badge does not decrease |
| 11. Kitchen request created | Kitchen request state and any notification are distinct from pending web-order count |
| 12. Mark one notification read | Only that notification changes; unrelated BusinessInsight/order notifications remain |

## 13. Final status

```text
VIRTUAL ORDER ENTITY:
Pending web Order (Status = "Đang xử lý", CreatedBy empty); OrderRequest is a separate kitchen-batch entity, not the conversion target.

BELL COMPONENT:
apps/admin-web/src/components/Navbar.tsx; cashier variant: apps/admin-web/src/components/CashierNavbar.tsx

BADGE SOURCE:
Combination of GET /api/Notification, local unread filter, NotificationCreated SignalR prepend, and inconsistent refresh-notifications handlers

BADGE CURRENT SEMANTICS:
Unread persistent Notification rows returned for the authenticated role/user/branch, capped at 100

BADGE EXPECTED SEMANTICS:
Actionable pending web/customer orders, or a separately defined unread notification lifecycle that resolves on the corresponding action

CONVERSION ENDPOINT:
POST /api/Order/{id}/accept

REQUEST STATE AFTER CONVERSION:
No OrderRequest conversion; the same Order remains "Đang xử lý" and is claimed by setting TableName and CreatedBy

SIGNALR CREATE EVENT:
NotificationCreated for persistent notifications; NewOrderRequest for kitchen OrderRequest creation

SIGNALR CONVERT/PROCESSED EVENT:
NONE for web-order acceptance; RequestStatusUpdated exists only for OrderRequest status changes

PRIMARY ROOT CAUSE:
Notification semantics mismatch: unread Notification count is used as a pending-order badge

SECONDARY ROOT CAUSE:
Missing notification/query invalidation after acceptance, plus inconsistent Navbar/CashierNavbar refresh state

BRANCH FILTER:
PARTIAL / ROLE-DEPENDENT

DUPLICATE LISTENER RISK:
NO high-confidence listener leak; YES duplicate state/source risk

RELOAD EXPECTED TO FIX:
NO for persistent unread notifications left unchanged by conversion

RECOMMENDED FIX:
Define a canonical branch-scoped actionable pending-order count, refetch it after successful acceptance, and keep unrelated persistent notifications separate or explicitly resolve only the linked notification.

FIX COMPLEXITY:
MEDIUM

SAFE TO FIX:
YES, after confirming the desired bell semantics with the product owner; implementation was not performed in this audit.
```

## 14. Audit boundaries

- No source code was modified.
- No database was queried or changed.
- No API was started.
- No migration was run.
- No runtime/E2E claim was made.
- No commit or push was performed.
