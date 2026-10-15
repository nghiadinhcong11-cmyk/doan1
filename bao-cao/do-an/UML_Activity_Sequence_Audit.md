# SOURCE CODE AUDIT â€“ ACTIVITY DIAGRAM & SEQUENCE DIAGRAM

Pháº¡m vi audit lĂ  read-only. KhĂ´ng sá»­a source, test, database, migration hoáº·c documentation.

---

# I. ACTIVITY DIAGRAM SOURCE AUDIT

## FLOW 1 â€” QR Ordering

### Swimlane

| Lane | TrĂ¡ch nhiá»‡m |
|---|---|
| Guest/Customer | Má»Ÿ Web, quĂ©t QR, xem menu, chá»n mĂ³n, gá»­i yĂªu cáº§u |
| Customer Web | Äá»c `?qr`, gá»i bootstrap, hiá»ƒn thá»‹ menu, quáº£n lĂ½ cart, gá»­i order |
| Backend/API | XĂ¡c thá»±c Guest session, kiá»ƒm tra request, ghi nháº­n order |
| OrderService | Táº¡o hoáº·c merge order, tĂ­nh láº¡i giĂ¡ vĂ  tá»•ng tiá»n |
| Database | LÆ°u table, branch, product, topping, order |
| Staff/POS | Tiáº¿p nháº­n Web Order sau khi khĂ¡ch gá»­i |

### Initial node

- Guest má»Ÿ Customer Web táº¡i `/`.
- Hoáº·c má»Ÿ URL `/?qr=<QrToken>`.
- `apps/customer-web/src/App.tsx`
- `apps/customer-web/src/pages/DigitalMenu.tsx`

### Main flow

| Step | Swimlane | Action | Source Evidence |
|---|---|---|---|
| 1 | Guest/Customer | Má»Ÿ Web hoáº·c quĂ©t QR | `App.tsx`, `DigitalMenu.tsx` |
| 2 | Customer Web | Äá»c query `qr` | `DigitalMenu.tsx` |
| 3 | Customer Web | Gá»i bootstrap | `POST /api/guest/bootstrap` |
| 4 | Backend/API | Resolve active Table tá»« QR token | `GuestController.Bootstrap` |
| 5 | Backend/API | XĂ¡c Ä‘á»‹nh Branch tá»« Table | `GuestController`, Guest session context |
| 6 | Backend/API | Táº¡o Guest JWT chá»©a Table/Branch claims | Guest bootstrap implementation |
| 7 | Customer Web | LÆ°u Guest token trong `sessionStorage` | `DigitalMenu.tsx`, `App.tsx` |
| 8 | Customer Web | Gá»i Product/Topping/Promotion APIs | `DigitalMenu.tsx` |
| 9 | Customer Web | Lá»c Product active vĂ  `Available` | `DigitalMenu.tsx:fetchProducts` |
| 10 | Guest/Customer | Chá»n mĂ³n, size, topping | `DigitalMenu.tsx` |
| 11 | Customer Web | Quáº£n lĂ½ cart táº¡m thá»i | `DigitalMenu.tsx` |
| 12 | Customer Web | Gá»­i `POST /api/Order` | `DigitalMenu.tsx:confirmOrder` |
| 13 | Backend/API | Ghi Ä‘Ă¨ Table/Branch báº±ng Guest context | `OrderController.CreateOrder` |
| 14 | Backend/API | Validate details, quantity, ProductId, ToppingId | `OrderController.CreateOrder` |
| 15 | OrderService | TĂ¬m order Ä‘ang xá»­ lĂ½ cĂ¹ng bĂ n/chi nhĂ¡nh | `OrderService.CreateOrUpdateOrderAsync` |
| 16 | OrderService | Merge order hoáº·c táº¡o order má»›i | `OrderService.CreateOrUpdateOrderAsync` |
| 17 | OrderService | TĂ­nh láº¡i giĂ¡, VAT, service fee, total tá»« database | `OrderService.RecalculateOrderFinancials` |
| 18 | Database | LÆ°u Order vĂ  OrderDetails | EF Core `SaveChangesAsync` |
| 19 | Backend/API | Tráº£ Order thĂ nh cĂ´ng | `OrderController.CreateOrder` |
| 20 | Customer Web | XĂ³a cart vĂ  hiá»ƒn thá»‹ thĂ nh cĂ´ng | `DigitalMenu.tsx` |

### Decisions

**Decision: QR token cĂ³ tá»“n táº¡i trĂªn URL?**

- Owner: Customer Web
- `[CĂ³]` â†’ gá»i `/api/guest/bootstrap`
- `[KhĂ´ng]` â†’ tiáº¿p tá»¥c Web menu thĂ´ng thÆ°á»ng
- Evidence: `App.tsx`, `DigitalMenu.tsx`

**Decision: QR token há»£p lá»‡?**

- Owner: Backend
- `[Há»£p lá»‡]` â†’ resolve Table/Branch vĂ  táº¡o Guest JWT
- `[KhĂ´ng há»£p lá»‡]` â†’ tráº£ lá»—i bootstrap; khĂ´ng táº¡o Guest session
- Evidence: `GuestController.Bootstrap`

**Decision: Product/Topping API thĂ nh cĂ´ng?**

- Owner: Customer Web
- `[ThĂ nh cĂ´ng]` â†’ hiá»ƒn thá»‹ menu
- `[Tháº¥t báº¡i]` â†’ hiá»ƒn thá»‹ â€œKhĂ´ng thá»ƒ táº£i thá»±c Ä‘Æ¡nâ€
- Evidence: `DigitalMenu.tsx:fetchProducts`

**Decision: Product cĂ³ bĂ¡n Ä‘Æ°á»£c khĂ´ng?**

- Owner: Customer Web
- `[Active + Available]` â†’ hiá»ƒn thá»‹
- `[Inactive/Unavailable]` â†’ lá»c khá»i menu
- Evidence: `DigitalMenu.tsx:fetchProducts`

Backend hiá»‡n kiá»ƒm tra Product/Topping tá»“n táº¡i nhÆ°ng khĂ´ng kiá»ƒm tra Ä‘áº§y Ä‘á»§ `AvailabilityStatus` khi submit order.

**Decision: CĂ³ order Ä‘ang xá»­ lĂ½ cĂ¹ng bĂ n/chi nhĂ¡nh khĂ´ng?**

- Owner: `OrderService`
- `[CĂ³]` â†’ merge/update existing order
- `[KhĂ´ng]` â†’ táº¡o order má»›i
- Evidence: `OrderService.CreateOrUpdateOrderAsync`

**Decision: Order request há»£p lá»‡ khĂ´ng?**

- Owner: `OrderController`
- `[Há»£p lá»‡]` â†’ gá»i OrderService
- `[KhĂ´ng]` â†’ HTTP 400/403/404
- Evidence: `OrderController.CreateOrder`

### Loop

- Customer cĂ³ thá»ƒ láº·p láº¡i thao tĂ¡c thĂªm/xĂ³a/tÄƒng/giáº£m mĂ³n trong cart.
- Customer Web reload menu má»—i 10 giĂ¢y.
- ÄĂ¢y lĂ  UI polling, khĂ´ng pháº£i backend business loop.
- Evidence: `DigitalMenu.tsx`, `setInterval(fetchProducts, 10000)`.

### Fork / Join

**NO VERIFIED PARALLEL BUSINESS FLOW**

Customer Web dĂ¹ng `Promise.all` Ä‘á»ƒ táº£i Product vĂ  Topping song song, nhÆ°ng Ä‘Ă¢y lĂ  implementation detail cá»§a frontend, khĂ´ng nĂªn biá»ƒu diá»…n nhÆ° fork nghiá»‡p vá»¥.

### Final/error paths

- QR khĂ´ng há»£p lá»‡: bootstrap failure.
- Menu API failure: káº¿t thĂºc vá»›i lá»—i táº£i menu.
- Cart rá»—ng hoáº·c thiáº¿u branch: khĂ´ng submit.
- Guest context khĂ´ng há»£p lá»‡: HTTP 403.
- Product/Topping khĂ´ng tá»“n táº¡i: HTTP 400.
- Quantity khĂ´ng há»£p lá»‡: HTTP 400.
- Database/order error: HTTP 500.
- ThĂ nh cĂ´ng: cart reset, hiá»ƒn thá»‹ â€œÄáº·t mĂ³n thĂ nh cĂ´ngâ€.

### Recommended Activity Flow â€“ QR Ordering

`Guest má»Ÿ Web/QR â†’ [QR?] â†’ Bootstrap Guest session â†’ [Valid?] â†’ Load menu â†’ Filter available products â†’ Customer chá»n mĂ³n â†’ Quáº£n lĂ½ cart â†’ Submit Order â†’ Validate Guest context/items â†’ [Existing pending order?] â†’ Merge hoáº·c Create â†’ Recalculate totals â†’ Save â†’ Success`

---

## FLOW 2 â€” Web Order Acceptance

### Swimlane

| Lane | TrĂ¡ch nhiá»‡m |
|---|---|
| Customer Web | Gá»­i order Ä‘ang xá»­ lĂ½ |
| POS Web | Hiá»ƒn thá»‹ pending Web Orders vĂ  gá»­i accept |
| Staff | Chá»n bĂ n vĂ  tiáº¿p nháº­n |
| OrderController | Kiá»ƒm tra role/branch vĂ  map result thĂ nh HTTP |
| OrderService | Claim pending order báº±ng transaction/conditional update |
| Database | Atomic update Order/Table |

### Initial node

Customer Web gá»­i:

`POST /api/Order`

Order Ä‘Æ°á»£c xem lĂ  Web Order khi:

- `Status == "Äang xá»­ lĂ½"`
- `CreatedBy` rá»—ng

KhĂ´ng cĂ³ `OrderType == "Web"` riĂªng Ä‘Æ°á»£c dĂ¹ng trong acceptance logic.

### Main flow

| Step | Swimlane | Action | Evidence |
|---|---|---|---|
| 1 | Customer Web | Gá»­i order | `DigitalMenu.tsx`, `POST /api/Order` |
| 2 | Backend | LÆ°u order tráº¡ng thĂ¡i `Äang xá»­ lĂ½` | `OrderService.CreateOrUpdateOrderAsync` |
| 3 | POS Web | Query pending orders | `POSPage.tsx`, `GET /api/Order?status=Äang xá»­ lĂ½` |
| 4 | Staff | Chá»n Web Order vĂ  bĂ n Ä‘Ă­ch | `POSPage.tsx` |
| 5 | POS Web | Gá»i accept endpoint | `POST /api/Order/{id}/accept` |
| 6 | Controller | Kiá»ƒm tra staff role vĂ  branch claim | `OrderController.AcceptWebOrder` |
| 7 | OrderService | Load existing order | `AcceptWebOrderAsync` |
| 8 | OrderService | Kiá»ƒm tra tráº¡ng thĂ¡i vĂ  `CreatedBy` | `AcceptWebOrderAsync` |
| 9 | OrderService | Validate target table náº¿u cĂ³ | `AcceptWebOrderAsync` |
| 10 | Database | Claim table báº±ng conditional update | `Tables.ExecuteUpdateAsync` |
| 11 | Database | Claim Order báº±ng conditional update | `Orders.ExecuteUpdateAsync` |
| 12 | OrderService | Commit transaction | `AcceptWebOrderAsync` |
| 13 | Backend | Tráº£ accepted Order | `OrderController` |

### Decisions

- Order tá»“n táº¡i?
  - KhĂ´ng â†’ `404 NotFound`
  - CĂ³ â†’ tiáº¿p tá»¥c

- Staff cĂ³ Ä‘Ăºng branch khĂ´ng?
  - KhĂ´ng â†’ `403 Forbidden`
  - CĂ³ â†’ tiáº¿p tá»¥c

- Order cĂ²n pending vĂ  chÆ°a cĂ³ `CreatedBy`?
  - KhĂ´ng â†’ `409 Conflict`
  - CĂ³ â†’ tiáº¿p tá»¥c

- Target table há»£p lá»‡/active?
  - KhĂ´ng â†’ `400 InvalidTable`
  - CĂ³ â†’ tiáº¿p tá»¥c

- Table claim thĂ nh cĂ´ng?
  - KhĂ´ng â†’ `409 Conflict`
  - CĂ³ â†’ tiáº¿p tá»¥c

- Order conditional claim cáº­p nháº­t Ä‘Ăºng má»™t row?
  - KhĂ´ng â†’ `409 Conflict`
  - CĂ³ â†’ commit vĂ  tráº£ thĂ nh cĂ´ng

### Merge/Create/Update

Acceptance **khĂ´ng táº¡o Order má»›i vĂ  khĂ´ng merge item**.

`AcceptWebOrderAsync`:

- cáº­p nháº­t `Order.TableName`;
- cáº­p nháº­t `Order.CreatedBy`;
- claim table;
- khĂ´ng gá»i `CreateOrUpdateOrderAsync`;
- khĂ´ng tĂ­nh láº¡i total;
- khĂ´ng merge OrderDetails.

Merge/create xáº£y ra á»Ÿ bÆ°á»›c Customer Web submit trong `OrderService.CreateOrUpdateOrderAsync`, trÆ°á»›c acceptance.

### Loop

- POS cĂ³ thá»ƒ refresh/query pending orders.
- KhĂ´ng cĂ³ backend acceptance loop.
- KhĂ´ng cĂ³ retry nghiá»‡p vá»¥ riĂªng ngoĂ i EF execution strategy cho transient database retry.

### Fork / Join

**NO VERIFIED PARALLEL FLOW**

Transaction vĂ  conditional update lĂ  concurrency control, khĂ´ng pháº£i UML parallel business flow.

### Final/error paths

- `404`: Order khĂ´ng tá»“n táº¡i.
- `403`: khĂ¡c branch hoáº·c thiáº¿u branch authority.
- `400`: table khĂ´ng há»£p lá»‡.
- `409`: accepted láº§n hai, table Ä‘Ă£ bá»‹ claim, hoáº·c conditional Order claim tháº¥t báº¡i.
- ThĂ nh cĂ´ng: Order Ä‘Æ°á»£c claim vĂ  tráº£ vá» POS.

### Reject

**REJECT WEB ORDER = UNSUPPORTED**

KhĂ´ng tĂ¬m tháº¥y endpoint/service chĂ­nh thá»©c cho Staff tá»« chá»‘i Web Order. KDS cancellation lĂ  cancellation cá»§a Kitchen Request, khĂ´ng pháº£i Web Order rejection.

### Automatic send-to-kitchen

**NOT VERIFIED / FALSE FOR CURRENT ACCEPT FLOW**

Acceptance khĂ´ng tá»± Ä‘á»™ng gá»i `SendToKitchenAsync`. POS pháº£i gá»i riĂªng:

`POST /api/Order/{id}/send-to-kitchen`

### Recommended Activity Flow â€“ Web Order Acceptance

`POS query pending orders â†’ Staff chá»n Web Order â†’ Accept endpoint â†’ [Order tá»“n táº¡i?] â†’ [ÄĂºng branch?] â†’ [Pending + CreatedBy rá»—ng?] â†’ [Table há»£p lá»‡?] â†’ Claim table â†’ [Claim thĂ nh cĂ´ng?] â†’ Claim Order â†’ [Affected rows = 1?] â†’ Commit â†’ Accepted`

---

## FLOW 3 â€” Payment & Loyalty

### Swimlane

| Lane | TrĂ¡ch nhiá»‡m |
|---|---|
| Cashier/Staff | Initiate payment hoáº·c redeem points |
| POS Web | Gá»­i payment/redeem request |
| OrderController | Validate input, branch, map result |
| OrderService | Conditional payment update vĂ  transaction |
| LoyaltyService | Earn/redeem/refund points |
| Database | Atomic Order/Customer/LoyaltyTransaction updates |
| SignalR clients | Nháº­n `PaymentCompleted` |

### Payment initiation

Endpoint:

`POST /api/Order/{id}/payment`

Actor roles:

- `admin`
- `manager`
- `employee`
- `cashier`

Request DTO:

- `Amount`
- `PaymentMethod`

Payment methods thá»±c sá»± Ä‘Æ°á»£c há»— trá»£:

- `Tiá»n máº·t`
- `Chuyá»ƒn khoáº£n`

KhĂ´ng tĂ¬m tháº¥y:

- VietQR gateway;
- payment gateway;
- webhook;
- external payment confirmation.

### Exact payment flow

| Step | Status | Source |
|---|---|---|
| Staff má»Ÿ payment trĂªn POS | `[VERIFIED]` | `POSPage.tsx` |
| POS gá»­i amount/payment method | `[VERIFIED]` | `POST /api/Order/{id}/payment` |
| Kiá»ƒm tra amount > 0 | `[VERIFIED]` | `OrderController.PayOrder` |
| Kiá»ƒm tra payment method | `[VERIFIED]` | `OrderController.PayOrder` |
| Load Order | `[VERIFIED]` | `OrderController.PayOrder` |
| Kiá»ƒm tra branch access | `[VERIFIED]` | `OrderController.HasBranchAccess` |
| So sĂ¡nh request amount vá»›i `Order.TotalAmount` | `[VERIFIED]` | `OrderController.PayOrder` |
| Má»Ÿ execution strategy vĂ  database transaction | `[VERIFIED]` | `OrderService.PayOrderAsync` |
| Conditional update Order | `[VERIFIED]` | `Orders.ExecuteUpdateAsync` |
| Äiá»u kiá»‡n update | `[VERIFIED]` | `Id == id`, `Status == "Äang xá»­ lĂ½"`, `PaidAmount <= 0`, `TotalAmount == amount` |
| Set PaidAmount, PaymentMethod, PaymentAt, Status | `[VERIFIED]` | `OrderService.PayOrderAsync` |
| Kiá»ƒm tra affected rows | `[VERIFIED]` | `affected == 1` |
| Gá»i EarnPointsAsync | `[VERIFIED]` | `OrderService.PayOrderAsync` |
| Commit transaction | `[VERIFIED]` | `OrderService.PayOrderAsync` |
| Query láº¡i Order | `[VERIFIED]` | `GetOrderByIdAsync` |
| Broadcast `PaymentCompleted` | `[VERIFIED]` | `OrderController`, `_hub.Clients.All.SendAsync` |
| HTTP 200 | `[VERIFIED]` | `PaymentAttemptResult.Paid` |

### Concurrency

CÆ¡ cháº¿ chá»‘ng double payment:

```text
UPDATE Orders
WHERE Id = @id
  AND Status = 'Äang xá»­ lĂ½'
  AND PaidAmount <= 0
  AND TotalAmount = @amount
```

- Conditional SQL update: **VERIFIED**
- Transaction: **VERIFIED**
- Affected-row check: **VERIFIED**
- RowVersion/optimistic concurrency token: **NOT VERIFIED**
- Separate distributed lock: **NOT VERIFIED**
- Second concurrent request:
  - conditional update affects zero rows;
  - reloads Order;
  - sees `PaidAmount > 0` or completed status;
  - returns `AlreadyPaid`;
  - controller returns HTTP 409.

### Payment failure paths

- Amount <= 0 â†’ HTTP 400.
- Unsupported method â†’ HTTP 400.
- Order missing â†’ HTTP 404.
- Wrong branch â†’ HTTP 403.
- Amount differs from persisted total â†’ HTTP 400.
- Already paid â†’ HTTP 409.
- Order not payable for another reason â†’ HTTP 400.
- Unexpected exception â†’ handled by outer API pipeline; exact public status depends on global middleware.

### After payment

- Order status becomes `"HoĂ n thĂ nh"`.
- `PaidAmount`, `PaymentMethod`, `PaymentAt` are persisted.
- No separate Invoice entity creation is performed.
- Invoice history reads persisted Orders through `InvoiceController`.
- Table status is not updated by payment code.
- `PaymentCompleted` is broadcast through SignalR.
- Broadcast uses `Clients.All`, not branch-specific groups.

### Loyalty earn

`PayOrderAsync` calls `EarnPointsAsync` after the conditional Order update but before the payment transaction commits.

`EarnPointsAsync`:

- requires Order exists;
- requires `CustomerId`;
- requires completed status;
- checks existing `"Earn"` transaction;
- calculates points from `loyaltyPointsPerAmount`, default 10,000 VND per point;
- atomically increments customer loyalty/spending/order counters;
- creates `LoyaltyTransaction`;
- uses the existing payment transaction when one already exists.

Therefore:

- Loyalty is called before payment commit: **VERIFIED**
- Loyalty is inside the same transaction when invoked from payment: **VERIFIED**
- Loyalty failure causes payment transaction not to commit: **INFERRED from transaction flow; no dedicated compensation branch**
- Earn idempotency check exists: **VERIFIED**
- Full concurrent duplicate-earn guarantee: **NOT VERIFIED**, because source shows a check followed by insert but no confirmed unique constraint in this path.

### Redeem points

Redeem is separate from payment:

`POST /api/Order/{id}/redeem`

- Staff roles: `admin`, `manager`, `employee`, `cashier`.
- Order must be processing and unpaid.
- Existing Redeem transaction blocks a second redemption.
- Points must satisfy minimum setting.
- Discount is `points * loyaltyRedemptionValue`, default 1,000 VND/point.
- Discount is capped at order total.
- Customer points are decremented atomically with balance condition.
- Order `Discount` increases.
- Order `TotalAmount` decreases.
- Payment later validates against the updated persisted total.

Redeem before payment: **VERIFIED**  
Redeem optional: **VERIFIED**  
Payment and redeem share one transaction: **FALSE** â€” they are separate requests and separate transactions.

### Payment decisions/fragments

Valid `alt`:

- `[Invalid amount/method]` â†’ HTTP 400.
- `[Order missing]` â†’ HTTP 404.
- `[Branch unauthorized]` â†’ HTTP 403.
- `[Amount mismatch]` â†’ HTTP 400.
- `[Conditional update affected 1 row]` â†’ loyalty, commit, success.
- `[Conditional update affected 0 rows + already paid]` â†’ HTTP 409.
- `[Conditional update affected 0 rows + other state]` â†’ HTTP 400.

Valid `opt`:

- Loyalty earn is invoked conditionally based on customer/order eligibility.
- Redeem points is optional but occurs through a separate pre-payment flow.

No verified `par` fragment.

### Recommended Payment Activity Flow

`Cashier/POS â†’ Validate amount/method â†’ Load Order â†’ Check branch â†’ Compare amount with persisted TotalAmount â†’ Begin transaction â†’ Conditional payment update â†’ [Affected rows = 1?] â†’ [Customer eligible for loyalty?] â†’ Earn points â†’ Commit â†’ Broadcast PaymentCompleted â†’ Return success`

The loyalty eligibility branch is implemented inside `EarnPointsAsync`, not as a separate controller decision.

---

## FLOW 4 â€” KDS / Kitchen

### Swimlane

| Lane | Responsibility |
|---|---|
| POS/Staff | Initiates send-to-kitchen |
| OrderController | Authorizes and delegates |
| KitchenService | Creates request, calculates unsent quantity, updates status |
| Database | Stores OrderRequest and OrderRequestItems |
| KitchenNotifier | Sends SignalR events |
| KitchenHub | Authenticates connection and assigns groups |
| KDS Client | Displays requests and sends status updates |
| Kitchen staff | Changes request status |

### Main flow

| Step | Swimlane | Action | Evidence |
|---|---|---|---|
| 1 | POS/Staff | Click send-to-kitchen | `POSPage.tsx` |
| 2 | POS | Call endpoint | `POST /api/Order/{id}/send-to-kitchen` |
| 3 | Controller | Check Order branch access | `OrderController.SendToKitchen` |
| 4 | KitchenService | Load Order with Details | `KitchenService.SendToKitchenAsync` |
| 5 | KitchenService | Require Order status `Äang xá»­ lĂ½` | `KitchenService` |
| 6 | KitchenService | Select `Quantity > SentQuantity` | `KitchenService` |
| 7 | KitchenService | Calculate `unsentQuantity` | `KitchenService` |
| 8 | KitchenService | Create `OrderRequest` with `Pending` | `KitchenService` |
| 9 | KitchenService | Create OrderRequestItems | `KitchenService` |
| 10 | KitchenService | Set each detail `SentQuantity = Quantity` | `KitchenService` |
| 11 | Database | Save and commit transaction | EF Core |
| 12 | KitchenNotifier | Send `NewOrderRequest` | `KitchenNotifier` |
| 13 | KDS Client | Receive event and add/update request | `KitchenPage.tsx` |
| 14 | Kitchen staff | Change Pending â†’ Preparing â†’ Ready â†’ Completed | KDS UI |
| 15 | KDS Client | Send status PATCH | `PATCH /api/Order/kitchen/requests/{id}/status` |
| 16 | KitchenService | Persist request status/timestamps | `UpdateRequestStatusAsync` |
| 17 | KitchenNotifier | Send `RequestStatusUpdated` | `KitchenNotifier` |
| 18 | POS/KDS | Update UI or reload active requests | `POSPage.tsx`, `KitchenPage.tsx` |

### SentQuantity and duplicate prevention

`SentQuantity` prevents resending already dispatched quantities:

```text
itemsToSend = Details.Where(Quantity > SentQuantity)
unsentQuantity = Quantity - SentQuantity
SentQuantity = Quantity
```

If no item satisfies the condition, the service throws:

`KhĂ´ng cĂ³ mĂ³n má»›i cáº§n gá»­i báº¿p.`

### Statuses

Frontend declares:

- `Pending`
- `Preparing`
- `Ready`
- `Completed`
- `Cancelled`

Backend explicitly timestamps:

- `Preparing`: `AcceptedAt`, `PreparingAt`
- `Completed`: `CompletedAt`

Backend does not visibly validate the allowed status set. It assigns the received string.

There is no verified per-item status endpoint or per-item status entity.

### Product availability

KDS can update product availability through:

`PATCH /api/Product/{id}/availability`

Statuses exposed by UI:

- `Available`
- `TemporarilyUnavailable`
- `OutOfStock`

This is product catalog availability, not an OrderRequest item status.

### SignalR

Hub:

`/kitchenHub`

Connection:

- `KitchenHub`
- `[Authorize(Roles = "admin,manager,employee,cashier,kitchen")]`

Groups assigned by `KitchenHub.OnConnectedAsync`:

- `kitchen-admin`
- `role:{role}`
- `kitchen-branch:{branchId}`
- `branch:{branchId}`
- `branch:{branchId}:role:{role}`

Notifier events:

- `NewOrderRequest`
- `RequestStatusUpdated`

Important security nuance:

- Branch-specific groups exist.
- `kitchen-admin` is global for all Admin connections.
- `KitchenNotifier` sends to both `kitchen-admin` and `kitchen-branch:{branchId}`.
- Therefore branch filtering is verified for branch-scoped kitchen users, but Admin receives global kitchen notifications by design.

### Loop

- KDS polls active requests every 8 seconds.
- KDS reloads after SignalR reconnect.
- POS reloads order kitchen status after `RequestStatusUpdated`.
- This is recovery/synchronization behavior, not a business-process loop.

### Fork / Join

**NO VERIFIED BUSINESS FORK/JOIN**

The database transaction commits before the notifier call. SignalR notification and in-app notifications are awaited sequentially after commit.

### Final/error paths

- Order missing â†’ failure.
- Order not processing â†’ failure.
- No unsent items â†’ failure.
- Database transaction error â†’ rollback.
- Unknown request ID on status update â†’ `NotFound`.
- Status update success â†’ save and broadcast.
- KDS cancellation removes the request from the active UI but does not represent Web Order rejection.

### Recommended Activity Flow â€“ KDS

`POS send-to-kitchen â†’ Validate Order â†’ Select unsent quantities â†’ Create Kitchen Request/Items â†’ Update SentQuantity â†’ Commit â†’ Notify SignalR â†’ KDS displays request â†’ Kitchen changes status â†’ PATCH backend â†’ Save status/timestamps â†’ Notify status event â†’ KDS/POS refresh`

---

## FLOW 5 â€” AI Tool Execution

### Swimlane

| Lane | Responsibility |
|---|---|
| User | Sends natural-language request |
| AI UI | Sends message and history |
| AiController | Requires authentication, builds user context |
| AiOrchestrator | Builds context, selects tool definitions, loops AI provider/tool calls |
| AiContextService | Reads contextual system data |
| PromptBuilder | Builds system instruction |
| AI provider | Returns text or function call |
| AiToolRegistry | Resolves tool and schemas |
| AiPermissionService | Checks role, branch condition for manager, and risk |
| AI Tool | Validates arguments and executes |
| Application Service | Performs business query/mutation |
| Database | Persists or reads data |

### Main flow

| Step | Swimlane | Action | Evidence |
|---|---|---|---|
| 1 | User | Enters AI message | `ChatBot.tsx` |
| 2 | AI UI | Sends `POST /api/Ai/chat` | `ChatBot.tsx` |
| 3 | AiController | Requires `[Authorize]` | `AiController` |
| 4 | AiController | Reads role, user ID, name, branch claims | `AiController.Chat` |
| 5 | AiController | Rejects Guest QR customer | `AiController.Chat` |
| 6 | AiOrchestrator | Builds context | `AiContextService.GetContextAsync` |
| 7 | AiOrchestrator | Builds system prompt | `PromptBuilder` |
| 8 | AiOrchestrator | Builds role-filtered tool definitions | `AiToolRegistry.GetAI providerToolsDefinition` |
| 9 | AI provider implementation | Sends request to external AI provider | `AI provider implementation.GenerateContentAsync` |
| 10 | AI provider | Returns text or function call | AI provider response parsing |
| 11 | AiOrchestrator | If text only, returns final response | `AiOrchestrator.ProcessAsync` |
| 12 | AiOrchestrator | If function call, resolve tool | `AiToolRegistry.GetTool` |
| 13 | AiPermissionService | Checks permission | `CanExecuteAsync` |
| 14 | AI Tool | Validates arguments | `AiToolValidator` |
| 15 | Application Service | Executes business operation | Tool constructor dependencies |
| 16 | AiOrchestrator | Builds function response | `functionResponses` |
| 17 | AI provider implementation | Sends tool result to AI provider | `GenerateContentAsync` |
| 18 | AI provider | Produces final text or another tool call | Orchestrator loop |
| 19 | AiController | Returns `AiResponse` | `Ok(result)` |

### Decisions

- Authenticated user?
  - No â†’ rejected by `[Authorize]`.
- Guest QR customer?
  - Yes â†’ HTTP 403.
- AI provider returned function call?
  - No â†’ final text returned.
  - Yes â†’ resolve and authorize tool.
- Tool exists?
  - No â†’ error function result sent back to AI provider.
- Permission allowed?
  - No â†’ denial function result sent back to AI provider.
  - Yes â†’ execute tool.
- Maximum five iterations reached?
  - Yes â†’ failure response.
  - No â†’ continue loop.

### Loop

Verified sequential tool loop:

```text
AI provider â†’ Function Call â†’ Tool â†’ Function Response â†’ AI provider
```

- Maximum iterations: 5.
- No parallel tool execution was verified.
- Multiple function calls in a single AI provider response are iterated sequentially within the same tool-response construction.

### Fork / Join

**NO VERIFIED PARALLEL FLOW**

All tool execution is awaited sequentially.

### Final/error paths

- Missing/invalid authentication â†’ authorization failure.
- Guest QR AI â†’ HTTP 403.
- AI provider invalid response â†’ `Success=false`.
- Unknown tool â†’ error result.
- Permission denied â†’ error result.
- Tool validation failure â†’ tool error result.
- external AI provider failure â†’ orchestrator failure response.
- More than five tool rounds â†’ `Success=false`.
- Normal final text â†’ `Success=true`.

### Recommended Activity Flow â€“ AI Tool Execution

`Authenticated User â†’ AI UI â†’ AiController â†’ Build user context â†’ Build context/prompt/tool definitions â†’ AI provider â†’ [Function call?] â†’ [Permission allowed?] â†’ Validate arguments â†’ Execute Application Service â†’ Return tool result to AI provider â†’ [More calls?] â†’ Final text â†’ User`

---

# II. SEQUENCE DIAGRAM SOURCE OF TRUTH

## Sequence 1 â€” Payment Concurrency

### Recommended lifelines

| Lifeline | Classification |
|---|---|
| Cashier/Staff | Actor |
| POS Web | Boundary |
| `OrderController` | Controller |
| `OrderService` | Application Service |
| `LoyaltyService` | Application Service |
| `ApplicationDbContext` / EF Core | Infrastructure |
| PostgreSQL Orders/Customers/Loyalty tables | Entity/Database |
| SignalR `IHubContext<KitchenHub>` | Infrastructure |
| Invoice History/POS listeners | Boundary |

### Exact message order

| # | Sender | Receiver | Message/Method | Sync/Async | Return | Condition | Evidence |
|---:|---|---|---|---|---|---|---|
| 1 | Cashier | POS Web | Enter payment | Sync UI | UI state | â€” | `POSPage.tsx` |
| 2 | POS Web | `OrderController` | `POST /api/Order/{id}/payment` | Async HTTP | HTTP request | â€” | `OrderController.PayOrder` |
| 3 | Controller | Controller | Validate amount/method | Sync | Continue/error | Invalid input â†’ 400 | `PayOrder` |
| 4 | Controller | `OrderService` | `GetOrderByIdAsync(id)` | Async | Order/null | Missing â†’ 404 | `PayOrder` |
| 5 | Controller | Controller | `HasBranchAccess(order.BranchId)` | Sync | Boolean | False â†’ 403 | `OrderController` |
| 6 | Controller | `OrderService` | `PayOrderAsync(id, amount, method)` | Async | Payment result | â€” | `IOrderService` |
| 7 | OrderService | EF/PostgreSQL | Begin execution strategy/transaction | Async | Transaction | â€” | `PayOrderAsync` |
| 8 | OrderService | PostgreSQL | Conditional `ExecuteUpdateAsync` | Async | affected rows | Pending, unpaid, amount exact | `Orders.ExecuteUpdateAsync` |
| 9 | PostgreSQL | OrderService | affected count | Async | `0` or `1` | â€” | `PayOrderAsync` |
| 10 | OrderService | `LoyaltyService` | `EarnPointsAsync(id)` | Async | Loyalty transaction/null | affected == 1 | `PayOrderAsync` |
| 11 | LoyaltyService | PostgreSQL | Read Order/customer/transactions | Async | Data | Customer eligible | `LoyaltyService` |
| 12 | LoyaltyService | PostgreSQL | Atomic customer update + insert transaction | Async | Result | â€” | `EarnPointsAsync` |
| 13 | OrderService | PostgreSQL | `CommitAsync()` | Async | Commit | Payment success | `PayOrderAsync` |
| 14 | OrderService | EF/PostgreSQL | `GetOrderByIdAsync(id)` | Async | Paid Order | â€” | `PayOrderAsync` |
| 15 | Controller | SignalR HubContext | `Clients.All.SendAsync("PaymentCompleted", payload)` | Awaited async push | Task | Payment success | `OrderController` |
| 16 | Controller | POS Web | HTTP 200 Order | Async HTTP return | Paid Order | Paid | `PayOrder` |
| 17 | Controller | POS Web | HTTP 409 | Async HTTP return | Conflict | Already paid | `PaymentAttemptResult.AlreadyPaid` |

### Combined fragments

#### `alt`

Verified:

- invalid amount/method;
- order missing;
- unauthorized branch;
- amount mismatch;
- payment conditional update succeeds;
- already paid/concurrency conflict;
- other non-payable status.

#### `opt`

- Loyalty earn is invoked conditionally based on customer/order eligibility.
- Redeem points is an optional separate request before payment, not an optional branch inside `PayOrderAsync`.

#### `break`

A validation failure terminates the payment interaction immediately:

- 400;
- 403;
- 404.

A AI provider/AI-style generic break is not relevant here.

#### `par`

**NO VERIFIED PAR fragment.**

---

## Sequence 2 â€” KDS SignalR

### Recommended lifelines

| Lifeline | Classification |
|---|---|
| POS Staff | Actor |
| POS Web | Boundary |
| `OrderController` | Controller |
| `KitchenService` | Application Service |
| `ApplicationDbContext` | Infrastructure |
| PostgreSQL Order/OrderRequest tables | Entity/Database |
| `KitchenNotifier` | Infrastructure Service |
| `IHubContext<KitchenHub>` | Infrastructure Service |
| `KitchenHub` | Boundary/Infrastructure |
| KDS Web | Boundary |
| Kitchen Staff | Actor |

### Exact message order â€” Send to kitchen

| # | Sender | Receiver | Message/Method | Sync/Async | Return | Condition | Evidence |
|---:|---|---|---|---|---|---|---|
| 1 | Staff | POS Web | Click Send to Kitchen | Sync UI | â€” | â€” | `POSPage.tsx` |
| 2 | POS Web | `OrderController` | `POST /api/Order/{id}/send-to-kitchen` | Async HTTP | â€” | â€” | `OrderController` |
| 3 | Controller | `OrderService` | `GetOrderByIdAsync(id)` | Async | Order | Branch check | `OrderController` |
| 4 | Controller | `KitchenService` | `SendToKitchenAsync(id)` | Async | `OrderRequest` | â€” | `IKitchenService` |
| 5 | KitchenService | PostgreSQL | Load Order + Details | Async | Order graph | Order processing | `KitchenService` |
| 6 | KitchenService | KitchenService | Select `Quantity > SentQuantity` | Sync | Item list | No items â†’ error | `KitchenService` |
| 7 | KitchenService | PostgreSQL | Insert OrderRequest/Items and update SentQuantity | Async | Save result | â€” | `SendToKitchenAsync` |
| 8 | KitchenService | PostgreSQL | Commit transaction | Async | Commit | Save successful | `SendToKitchenAsync` |
| 9 | KitchenService | `KitchenNotifier` | `NotifyNewOrderRequestAsync(payload)` | Awaited async | Task | After commit | `KitchenService` |
| 10 | Notifier | `IHubContext<KitchenHub>` | `Clients.Groups(...).SendAsync("NewOrderRequest")` | Awaited async push | Task | Branch payload | `KitchenNotifier` |
| 11 | SignalR | KDS Web | `NewOrderRequest` event | Async push | Event payload | Connected client | `KitchenPage.tsx` |
| 12 | KDS Web | KDS Web | Add/update request state and play sound | Sync UI | â€” | Active status | `KitchenPage.tsx` |

### Exact message order â€” Status update

| # | Sender | Receiver | Message/Method | Sync/Async | Return | Evidence |
|---:|---|---|---|---|---|---|
| 1 | Kitchen Staff | KDS Web | Click status button | Sync UI | â€” | `KitchenPage.tsx` |
| 2 | KDS Web | `OrderController` | `PATCH /api/Order/kitchen/requests/{id}/status` | Async HTTP | â€” | `KitchenPage.tsx` |
| 3 | Controller | `KitchenService` | `GetOrderRequestOrderAsync(id)` | Async | Order/null | Branch check |
| 4 | Controller | `KitchenService` | `UpdateRequestStatusAsync(id,status,processedBy)` | Async | Request/null | `OrderController` |
| 5 | KitchenService | PostgreSQL | Update request status/timestamps | Async | Save | â€” |
| 6 | KitchenService | `KitchenNotifier` | `NotifyRequestStatusUpdatedAsync` | Awaited async | Task | After save |
| 7 | Notifier | SignalR groups | `RequestStatusUpdated` | Async push | Event payload | Branch/admin groups |
| 8 | KDS Web | KDS Web | Update/remove active request | Sync UI | â€” | `KitchenPage.tsx` |
| 9 | POS Web | POS Web | Reload order kitchen status | Async HTTP | Updated statuses | `POSPage.tsx` |

### Combined fragments

- `alt`: request not found vs request found.
- `alt`: active status vs Completed/Cancelled status.
- `loop`: KDS polling every 8 seconds and reconnect reload.
- `loop`: repeated Kitchen status transitions.
- `opt`: SignalR push is optional from the userâ€™s perspective because polling provides recovery.
- `par`: not verified.

### Branch warning

Branch-specific groups are used, but `kitchen-admin` is global. Admin clients receive notifications for all branches. Do not draw the system as perfectly branch-isolated for Admin SignalR.

---

## Sequence 3 â€” AI Agent

### Recommended lifelines

| Lifeline | Classification |
|---|---|
| User | Actor |
| Customer/Admin Web ChatBot | Boundary |
| `AiController` | Controller |
| `AiOrchestrator` | Application Service |
| `AiContextService` | Application Service |
| `PromptBuilder` | Application Service |
| `AiToolRegistry` | Application Service |
| `AiPermissionService` | Application Service |
| `IAiTool` implementation | Application Service |
| Domain Application Service | Application Service |
| `ApplicationDbContext` | Infrastructure |
| PostgreSQL | Entity/Database |
| `AI provider implementation` | Infrastructure Service |
| external AI provider | External System |

### Exact message order

| # | Sender | Receiver | Message/Method | Sync/Async | Return | Condition | Evidence |
|---:|---|---|---|---|---|---|---|
| 1 | User | ChatBot | Enter message | Sync UI | â€” | â€” | `ChatBot.tsx` |
| 2 | ChatBot | `AiController` | `POST /api/Ai/chat` | Async HTTP | â€” | Auth required | `ChatBot.tsx` |
| 3 | `AiController` | `AiController` | Read role/user/branch claims | Sync | `AiUserContext` | â€” | `AiController.Chat` |
| 4 | `AiController` | `AiController` | Reject Guest QR if applicable | Sync | HTTP 403 | Guest role/session | `AiController` |
| 5 | `AiController` | `AiOrchestrator` | `ProcessAsync(request,userContext)` | Async | `AiResponse` | â€” | `IAiOrchestrator` |
| 6 | Orchestrator | `AiContextService` | `GetContextAsync` | Async | Context text | â€” | `AiOrchestrator` |
| 7 | Orchestrator | `PromptBuilder` | `BuildSystemInstruction` | Sync | Prompt | â€” | `AiOrchestrator` |
| 8 | Orchestrator | `AiToolRegistry` | `GetAI providerToolsDefinition(role)` | Sync | Schemas | â€” | `AiToolRegistry` |
| 9 | Orchestrator | `AI provider implementation` | `GenerateContentAsync` | Async HTTP | AI provider JSON | â€” | `AI provider implementation` |
| 10 | external AI provider | Orchestrator | Text/function call response | Async | Candidate | â€” | `AiOrchestrator` |
| 11 | Orchestrator | Orchestrator | Parse function call/text | Sync | Decision | â€” | `AiOrchestrator` |
| 12 | Orchestrator | ToolRegistry | `GetTool(name)` | Sync | Tool/null | Function call | `AiToolRegistry` |
| 13 | Orchestrator | PermissionService | `CanExecuteAsync(tool,context)` | Async | Boolean | Tool exists | `AiPermissionService` |
| 14 | Orchestrator | AI Tool | `ExecuteAsync(args,context)` | Async | Tool result | Allowed | `IAiTool` |
| 15 | AI Tool | Validator | Validate arguments | Sync | Valid/exception | Tool-specific | `AiToolValidator` |
| 16 | AI Tool | Application Service | Execute domain operation | Async | Domain result | Valid args | Tool implementations |
| 17 | Application Service | Database | Query/update | Async | Data/result | â€” | Application services |
| 18 | Orchestrator | AI provider implementation | Send function response | Async HTTP | AI provider response | Function call | `AiOrchestrator` |
| 19 | AI provider | Orchestrator | Final text or another call | Async | Candidate | Max 5 iterations | `AiOrchestrator` |
| 20 | Orchestrator | `AiController` | `AiResponse` | Async | Response | Final text/error | `AiController` |
| 21 | `AiController` | ChatBot | HTTP 200/403/error | Async HTTP | JSON | â€” | `AiController` |

### Combined fragments

#### `alt`

Verified:

- `[No function call]` â†’ return final AI provider text.
- `[Function call]` â†’ resolve, authorize, execute, return function response.
- `[Permission allowed]` vs `[Permission denied]`.
- `[Tool exists]` vs `[Unknown tool]`.

#### `loop`

Verified:

```text
AI provider â†’ Function Call â†’ Tool â†’ Function Response â†’ AI provider
```

Maximum: 5 iterations.

#### `opt`

- Tool execution is optional.
- Context/tool definitions are included before AI provider invocation.
- Customer/Staff tool availability depends on role.

#### `break`

- Authentication failure.
- Guest QR denial.
- AI provider invalid response.
- Tool validation failure.
- Tool execution exception.
- Maximum iteration exceeded.

No `par` fragment is verified.

---

# III. ACTIVITY â†” SEQUENCE CONSISTENCY

## Payment

Consistent mapping:

- Activity â€œValidate paymentâ€ â†’ `OrderController.PayOrder`
- Activity â€œConditional updateâ€ â†’ `OrderService.PayOrderAsync`
- Activity â€œEarn loyaltyâ€ â†’ `LoyaltyService.EarnPointsAsync`
- Activity â€œCommitâ€ â†’ `PayOrderAsync`
- Activity â€œPaymentCompletedâ€ â†’ `OrderController` SignalR broadcast

Important correction:

- Invoice is not created as a separate entity during payment.
- Table status is not updated during payment.
- Loyalty is executed before the payment transaction commits.
- Redeem is a separate pre-payment API flow.

## KDS

Consistent mapping:

- Activity â€œCreate Kitchen Requestâ€ â†’ `KitchenService.SendToKitchenAsync`
- Activity â€œCommit before notifyâ€ â†’ database transaction in `KitchenService`
- Activity â€œPush eventâ€ â†’ `KitchenNotifier`
- Activity â€œDisplay/update KDSâ€ â†’ `KitchenPage.tsx`
- Activity â€œUpdate statusâ€ â†’ `PATCH /api/Order/kitchen/requests/{id}/status`

SignalR is infrastructure and should appear in the Sequence Diagram. It should not be mistaken for a synchronous HTTP return.

## AI

Consistent mapping:

- AI provider does not access the database directly.
- `AiOrchestrator` controls the loop.
- `AiPermissionService` checks role/risk/manager branch prerequisite.
- AI Tool validates arguments and calls an Application Service.
- Application Service accesses the database.
- Tool results return to AI provider before the final response.

---

# IV. VERIFIED DECISIONS

1. QR token present?
2. QR token valid?
3. Product/Topping API successful?
4. Product active and available?
5. Cart contains items?
6. Guest context valid?
7. Existing pending Order exists?
8. Order request valid?
9. Web Order exists?
10. Staff branch authorized?
11. Web Order still pending and unclaimed?
12. Target table valid?
13. Table claim succeeds?
14. Conditional Web Order claim updates one row?
15. Payment amount valid?
16. Payment method supported?
17. Order exists?
18. Staff has branch access?
19. Request amount equals persisted `TotalAmount`?
20. Conditional payment update affects one row?
21. Order already paid?
22. Customer eligible for Loyalty?
23. Unsent kitchen items exist?
24. Kitchen request exists?
25. AI request authenticated?
26. Guest QR AI?
27. AI provider returned function call?
28. Tool exists?
29. Tool permission allowed?
30. Tool execution succeeded?
31. Maximum five AI iterations reached?

---

# V. VERIFIED COMBINED FRAGMENTS

## `alt`

- QR valid/invalid.
- Web Order pending/claimed.
- Table claim success/conflict.
- Payment accepted/already paid/invalid.
- AI text response/tool call.
- AI permission allowed/denied.
- KDS request found/not found.
- KDS active/completed/cancelled.

## `loop`

- AI sequential tool calling, maximum five iterations.
- KDS polling every eight seconds.
- KDS reconnect and refresh.
- Repeated kitchen status transitions.
- Customer cart manipulation.

## `opt`

- QR bootstrap when entering through QR.
- Loyalty earning when payment belongs to an eligible registered customer.
- Redeem points before payment.
- SignalR push, with polling as recovery.

## `break`

- Authentication failure.
- Guest AI denial.
- Payment validation failure.
- Web Order conflict.
- Invalid table.
- Invalid kitchen request.
- Tool validation/permission failure.
- AI provider failure or iteration limit.

## `par`

**No verified business-level parallel processing.**

`Promise.all` for menu loading and concurrent HTTP/SignalR infrastructure should not be drawn as business parallelism.

---

# VI. ASYNC / REALTIME INTERACTIONS

Verified asynchronous interactions:

- Customer Web â†’ API HTTP calls.
- POS Web â†’ API HTTP calls.
- KDS Web â†’ API HTTP calls.
- `KitchenService` â†’ `KitchenNotifier`.
- `KitchenNotifier` â†’ SignalR `IHubContext`.
- `NewOrderRequest` SignalR push.
- `RequestStatusUpdated` SignalR push.
- `PaymentCompleted` global SignalR push.
- KDS polling every eight seconds.
- KDS reload after SignalR reconnect.
- AI provider HTTP call.
- Sequential AI provider tool-call loop.
- EF Core async database operations.
- Execution strategy retries for transient database failures.

No verified fire-and-forget business operation was found. Frontend audio playback and cleanup operations are UI-level asynchronous behavior.

---

# VII. STATEMENT VERIFICATION

| # | Statement | Result | Evidence |
|---:|---|---|---|
| 1 | QR token is sent to `/api/guest/bootstrap` | VERIFIED | `DigitalMenu.tsx`, `GuestController.Bootstrap` |
| 2 | Backend resolves trusted Table and Branch | VERIFIED | `GuestController`, Guest session context |
| 3 | Backend creates Guest JWT/context bound to Table/Branch | VERIFIED | Guest bootstrap implementation |
| 4 | Guest can order without Registered Customer login | VERIFIED | Guest JWT and `OrderController.CreateOrder` |
| 5 | Cart is frontend temporary state | VERIFIED | `DigitalMenu.tsx` React state |
| 6 | Submit can create or update/merge pending Order | VERIFIED | `OrderService.CreateOrUpdateOrderAsync` |
| 7 | Staff has a dedicated Web Order accept endpoint | VERIFIED | `POST /api/Order/{id}/accept` |
| 8 | Second accept is prevented | VERIFIED | `CreatedBy`, status, conditional update |
| 9 | Conflict can return HTTP 409 | VERIFIED | `WebOrderAcceptResult.Conflict` |
| 10 | Accept can merge into an existing Order | FALSE | Acceptance claims an existing Order; merge occurs during submit |
| 11 | Full Web Order reject function is absent | VERIFIED | No reject endpoint/service found |
| 12 | Accept does not automatically send to kitchen | VERIFIED | Separate `send-to-kitchen` endpoint |
| 13 | Backend checks persisted TotalAmount | VERIFIED | Controller and conditional update |
| 14 | Conditional database update prevents double payment | VERIFIED | `ExecuteUpdateAsync` predicates |
| 15 | Only one concurrent payment can mark Order paid | VERIFIED | affected-row conditional update |
| 16 | Remaining payment receives conflict/failure | VERIFIED | `AlreadyPaid` â†’ HTTP 409 |
| 17 | Loyalty is processed after payment update and before commit | VERIFIED | `PayOrderAsync` |
| 18 | Redeem points is optional | VERIFIED | Separate endpoint |
| 19 | Cash and Bank Transfer are supported | VERIFIED | `OrderController.PayOrder` |
| 20 | Automatic VietQR webhook is absent | VERIFIED | No gateway/webhook source found |
| 21 | Send-to-kitchen creates Kitchen Request | VERIFIED | `KitchenService.SendToKitchenAsync` |
| 22 | KitchenNotifier broadcasts realtime event | VERIFIED | `KitchenNotifier` |
| 23 | KitchenHub/SignalR distributes by branch | PARTIALLY VERIFIED | Branch groups exist; global `kitchen-admin` also receives events |
| 24 | KDS client receives realtime events | VERIFIED | `KitchenPage.tsx` |
| 25 | Kitchen updates status per item | FALSE | Current endpoint updates request status, not item status |
| 26 | Kitchen history exists | VERIFIED | `KitchenService.GetHistoryAsync`, `KitchenHistoryPage.tsx` |
| 27 | Product availability can be updated from KDS | VERIFIED | Product availability UI and endpoint |
| 28 | Sales do not automatically deduct inventory | VERIFIED | No sales-to-inventory deduction in audited flow |
| 29 | Guest QR cannot use AI | VERIFIED | `AiController.Chat` |
| 30 | Registered Customer can use AI | VERIFIED | `[Authorize]`, customer tools |
| 31 | AI provider selects tools based on request | VERIFIED | Function-call parsing in `AiOrchestrator` |
| 32 | Permission is checked before tool execution | PARTIALLY VERIFIED | Role/risk check precedes tool; deeper ownership/branch checks occur in tool/service |
| 33 | Tools call Application Services, not AI provider-direct DB access | VERIFIED | Tool constructor dependencies |
| 34 | Admin-only tools exist | VERIFIED | `UpdateProductPriceTool` and admin tools |
| 35 | Admin can update product price through AI | VERIFIED | `UpdateProductPriceTool` |
| 36 | Customer can create booking through AI | VERIFIED | `CreateBookingTool` |
| 37 | AI is not an unrestricted autonomous agent | VERIFIED | Explicit registry and max-five loop |
| 38 | Tool calling can loop multiple times | VERIFIED | `MAX_ITERATIONS = 5` |

---

# VIII. DIAGRAM WARNINGS

- Do not turn chronological steps into `<<include>>` or `<<extend>>`.
- Do not place DTO mapping or claim parsing as separate business activities.
- Do not show AI provider directly accessing PostgreSQL.
- Do not show SignalR push as a synchronous HTTP return.
- Do not show Web Order acceptance as creating a new Order.
- Do not add Web Order rejection; it is unsupported.
- Do not add VietQR gateway/webhook behavior.
- Do not add automatic inventory consumption after sales.
- Do not treat `SentQuantity` as an item-status workflow.
- Do not treat Product availability as KitchenRequest status.
- Do not claim perfect branch isolation for Admin SignalR because `kitchen-admin` is global.
- Do not combine Cashier Shift with Attendance.
- Do not show Manager as global administrator.
- Do not show Invoice creation during payment; invoice history reads persisted Orders.
- Do not show Table status changing automatically after payment.
- Do not show Loyalty as a separate committed transaction after payment; `EarnPointsAsync` runs before the payment transaction commits.
- Do not show Redeem Points as part of the payment endpoint; it is a separate pre-payment endpoint.
- Do not show parallel branches unless explicitly modeling frontend menu loading or infrastructure behavior.
- Do not show Guest QR Customer using AI.
- Do not omit the five-iteration maximum in the AI sequence.
- Do not show all AI tools as mandatory; AI provider selects them conditionally.

---

# IX. FINAL VERDICT

## Activity Diagrams

| Diagram | Verdict |
|---|---|
| QR Ordering | `READY FOR PLANTUML` |
| Web Order Acceptance | `READY FOR PLANTUML` |
| Payment & Loyalty | `READY FOR PLANTUML` â€” include loyalty/concurrency annotations |
| KDS | `READY FOR PLANTUML` â€” include global Admin SignalR caveat |
| AI Tool Execution | `READY FOR PLANTUML` |

## Sequence Diagrams

| Diagram | Verdict |
|---|---|
| Payment concurrency | `READY FOR PLANTUML` |
| KDS SignalR | `READY FOR PLANTUML` â€” show branch groups and global Admin group accurately |
| AI Agent | `READY FOR PLANTUML` |

No additional source verification is required for the core diagrams. The only items that must remain explicitly marked as unsupported or partial are:

- Web Order rejection;
- item-level Kitchen status;
- automatic payment gateway/webhook;
- complete voucher application;
- perfect branch isolation for Admin SignalR;
- concurrent Loyalty duplicate protection beyond the existing idempotency check.


