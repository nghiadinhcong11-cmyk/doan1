# Business Flows

## Login

Employee login looks up an active employee by username, verifies a hashed password, retains a plaintext comparison for legacy rows, and upgrades legacy passwords to a hash. `mode` selects admin, cashier or kitchen access; manager is returned as a role when appropriate. Customer token issuance is phone-based and creates a guest token when no customer record exists.

## POS/order

The POS sends item/product/topping/option identity. `OrderService` retrieves authoritative product/topping data and recalculates item prices and totals. Existing unpaid processing orders can be merged by branch/table; sent quantities cannot be reduced. New/updated financial snapshots include subtotal, VAT/service percentages and amounts. Discount is a persisted legacy field but current order creation does not trust a client discount and no verified promotion-to-order discount path was found.

Calculation order is subtotal -> rounded service fee -> VAT on subtotal plus service fee -> discount -> total. Money uses `decimal` and `AwayFromZero` rounding in `OrderService`.

## Kitchen

`POST /api/Order/{id}/send-to-kitchen` delegates to `KitchenService.SendToKitchenAsync`. It compares each detail's `Quantity` and `SentQuantity`, creates a new `OrderRequest` containing only the delta, increments `SentQuantity`, and notifies SignalR. This is how a later addition to an existing order is distinguished from already sent items.

## QR customer ordering

`QRScan` uses camera/jsQR and navigates with `tableId` from a scanned URL or a GUID-like payload. `DigitalMenu` then calls the table endpoint, obtains branch information from the table, loads products/toppings/promotions and posts an order. Customer login is optional; the customer app can use phone/customer-token flows. QR security depends on the backend table lookup and order branch/table validation; the client-provided identifier must not be considered trusted by itself.

## Payment

The verified payment methods are exactly `Tiền mặt` and `Chuyển khoản` in `OrderController`. The controller compares the submitted amount to persisted `Order.TotalAmount`; `OrderService.PayOrderAsync` performs an atomic conditional update for an unpaid processing order, stores PaidAmount/PaymentMethod/PaymentAt and marks it complete. No bank provider settlement or webhook callback is evidenced. A `PaymentCompleted` SignalR broadcast is sent using `Clients.All`, which is a security/visibility concern.

## HRM, expense and reporting

Attendance, schedules, shifts and expenses have controllers, pages and persistence. Payroll is outside the current business scope and has no active flow. Dashboard and BusinessInsight services/controllers provide summaries and proactive insight records. Historical migration references remain for database review only.
