# Role × Function × API × Frontend Matrix

## 1. Audit scope and actors

This is a source-only audit of the current repository. No runtime credentials, database connection, migration, or business-data mutation was used.

Actors are kept separate:

- `admin`: employee JWT role with global-management semantics in services that support global scope.
- `manager`: employee JWT role intended for assigned-branch management.
- `employee`: domain role retained in `Employee.Role`; the current employee login UI/API has no accepted login mode that produces this role.
- `cashier`: employee JWT role using cashier login mode.
- `kitchen`: employee JWT role using kitchen login mode.
- `registered customer`: customer JWT with `customerSessionType=registered`.
- `guest QR customer`: customer JWT with `customerSessionType=guest`, normally entered through the customer web guest flow.
- `anonymous/public`: no JWT; used by selected catalog, branch, table, customer-exists, customer-token, and AI endpoints.

The thesis source of truth names the same roles and distinguishes guest access. It also describes AI as permissioned tool-calling rather than an autonomous database agent. Current source now contains Inventory, so the older thesis statement that Inventory is not evidenced is stale and is recorded as a documentation discrepancy.

## 2. Source-derived authentication and authorization rules

`AuthController.Login` accepts `Mode=admin`, `cashier`, or `kitchen` only. `admin` mode accepts effective roles `admin` and `manager`; cashier mode accepts `admin`, `manager`, `cashier`; kitchen mode accepts `admin`, `manager`, `kitchen`. An effective `employee` role is rejected by all three branches. `JwtService` writes role, identity, and optional `branchId` claims.

`AuthController.GetCustomerToken` accepts a phone identifier without a password and emits either a registered or guest customer JWT. Inactive registered customers are rejected.

For Inventory, `InventoryController` allows operational roles at class level, limits item master data, minimum-stock writes, receipts, and confirmations to admin/manager, and delegates branch filtering to `InventoryService`. Non-admin branch access is compared to the JWT `branchId` claim.

Notation used below:

- `PUBLIC`: no authentication required by the current action/controller path.
- `AUTH`: any authenticated principal accepted by the action.
- `R(...)`: role attribute list.
- `OWN`: service checks customer identity/resource ownership.
- `BR`: service checks the requested resource branch against JWT branch context; admin may be global where the service permits it.
- `LOGIC`: no attribute gate; action contains its own identity/ownership logic.
- `MUT`: write/mutation; `READ`: read/query.

## 3. Complete backend controller inventory

There are 23 controllers: 22 under `services/api/src/WebAPI/Controllers` and `AiController` under `services/api/src/AI/Controllers`. There are 117 HTTP actions.

The following inventory lists every current action. Controller-level attributes are included where they apply to all actions; action-specific attributes override or further restrict them.

### AuthController — `api/Auth` — 4 actions

| Method and route | Action | Auth / ownership | Kind |
|---|---|---|---|
| POST `api/Auth/login` | Login | PUBLIC endpoint; validates active employee/password and accepted mode | MUT/auth |
| POST `api/Auth/customer-token` | GetCustomerToken | PUBLIC; registered/guest customer token flow | MUT/auth |
| GET `api/Auth/me` | GetCurrentUser | AUTH | READ |
| POST `api/Auth/change-password` | ChangePassword | AUTH; JWT identity must equal request ID; customer/employee type must match role | MUT |

### AreaController — `api/Area` — 4 actions

| Method and route | Action | Auth | Kind |
|---|---|---|---|
| GET `api/Area` | GetAreas | PUBLIC | READ |
| POST `api/Area` | CreateArea | R(admin) | MUT |
| PUT `api/Area/{id}` | UpdateArea | R(admin) | MUT |
| DELETE `api/Area/{id}` | DeleteArea | R(admin) | MUT |

### AttendanceController — `api/Attendance` — 3 actions

Controller gate: `R(admin,manager,cashier,kitchen,employee)`.

| Method and route | Action | Branch/identity behavior | Kind |
|---|---|---|---|
| GET `api/Attendance` | GetAttendances | query filters include employee/branch; service/controller logic applies current scope | READ |
| POST `api/Attendance/check-in` | CheckIn | authenticated employee attendance mutation | MUT |
| PATCH `api/Attendance/{id}/check-out` | CheckOut | authenticated employee attendance mutation | MUT |

### BranchController — `api/Branch` — 5 actions

| Method and route | Action | Auth | Kind |
|---|---|---|---|
| GET `api/Branch` | GetBranches | PUBLIC | READ |
| POST `api/Branch` | CreateBranch | R(admin) | MUT |
| PUT `api/Branch/{id}` | UpdateBranch | R(admin) | MUT |
| PATCH `api/Branch/{id}/toggle-status` | ToggleStatus | R(admin) | MUT |
| DELETE `api/Branch/{id}` | DeleteBranch | R(admin) | MUT |

### BusinessInsightController — `api/BusinessInsight` — 5 actions

Controller gate: `R(admin,manager)`.

| Method and route | Action | Kind |
|---|---|---|
| GET `api/BusinessInsight` | Get | READ |
| GET `api/BusinessInsight/{id}` | GetById | READ |
| GET `api/BusinessInsight/unread-count` | GetUnreadCount | READ |
| POST `api/BusinessInsight/{id}/read` | MarkRead | MUT |
| POST `api/BusinessInsight/{id}/resolve` | Resolve | MUT |

### CustomerController — `api/Customer` — 6 actions

| Method and route | Action | Auth / ownership | Kind |
|---|---|---|---|
| GET `api/Customer` | GetCustomers | R(admin,manager,employee,cashier) | READ |
| GET `api/Customer/exists/{phoneNumber}` | CheckExists | PUBLIC; rate-limited | READ |
| GET `api/Customer/{phoneNumber}` | GetByPhone | AUTH; customer must request own phone (`OWN`); staff may serve lookup | READ |
| POST `api/Customer` | CreateOrUpdate | PUBLIC route; creating is public, existing-customer update requires authenticated/own logic | MUT |
| GET `api/Customer/{id}/loyalty-history` | GetLoyaltyHistory | R(admin,manager,cashier,employee,customer); customer ID must equal JWT ID (`OWN`) | READ |
| PATCH `api/Customer/{phoneNumber}/profile` | UpdateProfile | R(admin,manager,customer); customer must own phone/customer ID (`OWN`) | MUT |

### DashboardController — `api/Dashboard` — 1 action

Controller gate: `R(admin,manager)`.

| Method and route | Action | Branch behavior | Kind |
|---|---|---|---|
| GET `api/Dashboard/summary` | GetSummary | admin may select/global; manager query is overridden by a valid JWT BranchId and fails closed without one | READ |

### EmployeeController — `api/Employee` — 6 actions

Controller gate: `AUTH`; action-level roles distinguish management from operational self-profile access.

| Method and route | Action | Kind |
|---|---|---|
| GET `api/Employee` | GetEmployees | R(admin,manager); manager JWT branch only | READ |
| GET `api/Employee/{id}` | GetEmployee | employee roles; admin global, manager own branch, operational roles self only | READ |
| POST `api/Employee` | CreateEmployee | R(admin,manager); manager branch derived from JWT and management-role creation prevented | MUT |
| PUT `api/Employee/{id}` | UpdateEmployee | R(admin,manager); manager own-branch non-management target only | MUT |
| PATCH `api/Employee/{id}/toggle-status` | ToggleStatus | R(admin,manager); manager own-branch non-management target only | MUT |
| DELETE `api/Employee/{id}` | DeleteEmployee | R(admin,manager); manager own-branch non-management target only | MUT |

Task 47E and Task 47F hardened this controller. A manager without a valid JWT BranchId fails closed.

### ExpenseController — `api/Expense` — 4 actions

Controller gate: `R(admin,manager)`.

| Method and route | Action | Kind |
|---|---|---|
| GET `api/Expense` | GetExpenses | READ |
| POST `api/Expense` | CreateExpense | MUT |
| PUT `api/Expense/{id}` | UpdateExpense | MUT; Inventory-linked records are protected in current source |
| DELETE `api/Expense/{id}` | DeleteExpense | MUT; Inventory-linked records are protected in current source |

### InventoryController — `api/inventory` — 21 actions

Controller gate: `R(admin,manager,employee,cashier,kitchen)`.

| Method and route | Action | Auth / service condition | Kind |
|---|---|---|---|
| GET `api/inventory/items` | GetItems | R(admin,manager) | READ |
| GET `api/inventory/items/{id}` | GetItem | R(admin,manager) | READ |
| POST `api/inventory/items` | CreateItem | R(admin,manager); normalized-name/unit validation | MUT |
| PUT `api/inventory/items/{id}` | UpdateItem | R(admin,manager) | MUT |
| GET `api/inventory/branches/{branchId}` | GetBranchInventory | operational role; BR | READ |
| GET `api/inventory/overview/{branchId}` | GetOverview | operational role; BR | READ |
| PUT `api/inventory/branches/{branchId}/items/{inventoryItemId}/minimum-stock` | UpdateMinimumStock | R(admin,manager); BR | MUT |
| GET `api/inventory/receipts` | GetReceipts | operational role; branch query scoped by service | READ |
| GET `api/inventory/receipts/{id}` | GetReceipt | operational role; document branch checked | READ |
| POST `api/inventory/receipts` | CreateReceipt | R(admin,manager); branch checked | MUT |
| PUT `api/inventory/receipts/{id}` | UpdateReceipt | R(admin,manager); Draft only; branch checked | MUT |
| POST `api/inventory/receipts/{id}/confirm` | ConfirmReceipt | R(admin,manager); Draft, BR, atomic stock/ledger/Expense | MUT |
| POST `api/inventory/receipts/{id}/cancel` | CancelReceipt | R(admin,manager); Draft only; BR | MUT |
| GET `api/inventory/issues` | GetIssues | operational role; branch query scoped by service | READ |
| GET `api/inventory/issues/{id}` | GetIssue | operational role; document branch checked | READ |
| POST `api/inventory/issues` | CreateIssue | operational role; Draft creation; BR | MUT |
| PUT `api/inventory/issues/{id}` | UpdateIssue | operational role; Draft only; BR | MUT |
| POST `api/inventory/issues/{id}/confirm` | ConfirmIssue | R(admin,manager); BR, row locks, stock check | MUT |
| POST `api/inventory/issues/{id}/cancel` | CancelIssue | operational role; Draft only; BR | MUT |
| GET `api/inventory/transactions` | GetTransactions | operational role; branch-scoped query | READ |

### InvoiceController — `api/Invoice` — 2 actions

Controller gate: `R(admin,manager,cashier,employee)`.

| Method and route | Action | Kind |
|---|---|---|
| GET `api/Invoice` | Get | READ |
| GET `api/Invoice/{id}` | Detail | READ |

### NotificationController — `api/Notification` — 3 actions

Controller gate: `AUTH`.

| Method and route | Action | Scope | Kind |
|---|---|---|---|
| GET `api/Notification` | Get | service receives role/user/branch context | READ |
| PATCH `api/Notification/{id}/read` | MarkRead | user notification ownership handled by service | MUT |
| POST `api/Notification/read-all` | MarkAllRead | role/user/branch context | MUT |

### OrderController — `api/Order` — 15 actions

The controller has no global role attribute; action attributes and service logic determine access. Customer ownership and branch status are handled in service/controller paths where applicable.

| Method and route | Action | Auth / condition | Kind |
|---|---|---|---|
| GET `api/Order` | GetOrders | AUTH; query supports customer/branch/status filters | READ |
| POST `api/Order` | CreateOrder | public/customer/POS path is handled by request/service logic | MUT |
| POST `api/Order/{id}/accept` | AcceptWebOrder | R(admin,manager,employee,cashier) | MUT |
| POST `api/Order/{id}/send-to-kitchen` | SendToKitchen | R(admin,manager,employee,cashier,kitchen) | MUT |
| GET `api/Order/{id}/kitchen-status` | GetKitchenStatus | R(admin,manager,employee,cashier,kitchen) | READ |
| POST `api/Order/{id}/payment` | PayOrder | R(admin,manager,employee,cashier) | MUT |
| GET `api/Order/{id}/loyalty` | GetOrderLoyalty | AUTH; customer ownership/service rules | READ |
| POST `api/Order/{id}/redeem` | RedeemPoints | R(admin,manager,employee,cashier) | MUT |
| GET `api/Order/{id}` | GetOrder | AUTH; service ownership/scope required | READ |
| PATCH `api/Order/{id}/status` | UpdateStatus | R(admin,manager,employee,cashier,kitchen) | MUT |
| DELETE `api/Order/{id}` | DeleteOrder | R(admin,manager,employee,cashier,kitchen) | MUT |
| GET `api/Order/kitchen/active-requests` | GetActiveKitchenRequests | R(admin,manager,kitchen) | READ |
| PATCH `api/Order/kitchen/requests/{id}/status` | UpdateRequestStatus | R(admin,manager,kitchen) | MUT |
| GET `api/Order/kitchen/history` | GetKitchenHistory | R(admin,manager,kitchen) | READ |
| GET `api/Order/kitchen/requests/{id}` | GetKitchenRequestDetail | R(admin,manager,kitchen) | READ |

### ProductController — `api/Product` — 6 actions

| Method and route | Action | Auth | Kind |
|---|---|---|---|
| GET `api/Product` | GetProducts | PUBLIC | READ |
| POST `api/Product` | CreateProduct | R(admin) | MUT |
| DELETE `api/Product/{id}` | DeleteProduct | R(admin) | MUT |
| PUT `api/Product/{id}` | UpdateProduct | R(admin) | MUT |
| PATCH `api/Product/{id}/toggle-status` | ToggleStatus | R(admin) | MUT |
| PATCH `api/Product/{id}/availability` | UpdateAvailability | R(admin,manager,kitchen) | MUT |

### PromotionController — `api/Promotion` — 5 actions

| Method and route | Action | Auth | Kind |
|---|---|---|---|
| GET `api/Promotion` | GetPromotions | PUBLIC | READ |
| POST `api/Promotion` | CreatePromotion | R(admin) | MUT |
| PUT `api/Promotion/{id}` | UpdatePromotion | R(admin) | MUT |
| DELETE `api/Promotion/{id}` | DeletePromotion | R(admin) | MUT |
| PATCH `api/Promotion/{id}/toggle` | ToggleStatus | R(admin) | MUT |

### ReceiptSettingsController — `api/ReceiptSettings` — 2 actions

Controller gate: `R(admin,manager,cashier,employee)`; `TryBranch` checks the requested branch against the JWT context except permitted admin/global behavior.

| Method and route | Action | Kind |
|---|---|---|
| GET `api/ReceiptSettings` | Get | READ/BR |
| PUT `api/ReceiptSettings` | Put | MUT/BR |

### ReservationController — `api/Reservation` — 6 actions

| Method and route | Action | Auth / condition | Kind |
|---|---|---|---|
| GET `api/Reservation` | GetReservations | AUTH; customer phone and staff scope handled by service | READ |
| GET `api/Reservation/{id}` | GetReservation | AUTH; ownership/scope handled by service | READ |
| POST `api/Reservation` | CreateReservation | AUTH attribute; service supports guest behavior documented in source/tests | MUT |
| PUT `api/Reservation/{id}` | UpdateReservation | AUTH; ownership/branch checks | MUT |
| PATCH `api/Reservation/{id}/status` | UpdateStatus | R(admin,manager,employee,cashier) | MUT |
| DELETE `api/Reservation/{id}` | DeleteReservation | AUTH; ownership/scope handled by service | MUT |

### ShiftController — `api/Shift` — 4 actions

Controller gate: `R(admin,manager,cashier)`.

| Method and route | Action | Kind |
|---|---|---|
| GET `api/Shift/current/{employeeId}` | GetCurrentShift | READ; manager target shift must be in JWT branch |
| POST `api/Shift/open` | OpenShift | MUT; target employee must belong to requested/authorized branch |
| POST `api/Shift/close/{id}` | CloseShift | MUT; target branch checked and revenue query constrained to shift branch |
| GET `api/Shift` | GetShifts | READ; manager query resolved to JWT branch |

### SystemSettingsController — `api/SystemSettings` — 2 actions

Controller gate: `AUTH`; service uses caller role and branch context.

| Method and route | Action | Kind |
|---|---|---|
| GET `api/SystemSettings` | Get | READ/BR |
| PUT `api/SystemSettings` | Put | MUT/BR |

### TableController — `api/Table` — 6 actions

| Method and route | Action | Auth | Kind |
|---|---|---|---|
| GET `api/Table` | GetTables | PUBLIC | READ |
| GET `api/Table/{id}` | GetTable | PUBLIC | READ |
| POST `api/Table` | CreateTable | R(admin,manager) | MUT |
| PUT `api/Table/{id}` | UpdateTable | R(admin,manager) | MUT |
| PATCH `api/Table/{id}/status` | UpdateStatus | R(admin,manager,employee,cashier) | MUT |
| DELETE `api/Table/{id}` | DeleteTable | R(admin,manager) | MUT |

### ToppingController — `api/Topping` — 4 actions

| Method and route | Action | Auth | Kind |
|---|---|---|---|
| GET `api/Topping` | GetToppings | PUBLIC | READ |
| POST `api/Topping` | CreateTopping | R(admin) | MUT |
| PUT `api/Topping/{id}` | UpdateTopping | R(admin) | MUT |
| DELETE `api/Topping/{id}` | DeleteTopping | R(admin) | MUT |

### WorkScheduleController — `api/WorkSchedule` — 3 actions

Controller gate: `R(admin,manager,cashier,kitchen,employee)`.

| Method and route | Action | Kind |
|---|---|---|
| GET `api/WorkSchedule` | GetSchedules | READ; filters employee/branch/date | READ |
| POST `api/WorkSchedule` | CreateSchedule | MUT |
| DELETE `api/WorkSchedule/{id}` | DeleteSchedule | MUT |

### AiController — `api/Ai` — 1 action

| Method and route | Action | Auth / context | Kind |
|---|---|---|---|
| POST `api/Ai/chat` | Chat | PUBLIC endpoint; anonymous defaults to customer context; authenticated JWT role/branch is read | MUT-like orchestration; tool-specific |

The AI endpoint itself is not a role gate. `AiToolRegistry`, `AiAuthorization`, `AiPermissionService`, risk checks, and tool `AllowedRoles` are the effective function-level authorization layer.

## 4. AI tool function inventory

Current registered tool families are:

| Tool/function | Allowed contexts | Read/write and scope |
|---|---|---|
| `get_active_staff` | admin, manager | READ; employee service; manager scope requires review |
| `get_best_sellers` | admin, manager, employee, cashier, kitchen | READ; business/order data |
| `get_business_summary` | admin, manager | READ |
| `get_financial_analysis` | admin, manager | READ |
| `get_order_list` | admin, manager, employee, cashier, kitchen | READ; branch context for non-admin; customer rejected |
| `get_revenue_comparison` | admin, manager | READ |
| `get_revenue` | admin, manager | READ |
| `update_product_price` | admin | WRITE; product service |
| `create_booking` | customer | WRITE; reservation service |
| `customer_get_menu` | admin, manager, employee, cashier, kitchen, customer | READ |
| `get_my_orders` | customer | READ/OWN |
| `get_my_shift` | employee, cashier, kitchen | READ/OWN |
| `get_table_summary` | admin, manager, employee, cashier, kitchen | READ; branch context for non-admin |
| `update_order_status` | admin, manager, employee, cashier, kitchen | WRITE; order service with role/branch context |

The source contains no Inventory AI tool and no Inventory SignalR event.

## 5. Frontend route and function inventory

### Admin Web route structure

`App.tsx` has three shells: unauthenticated login, cashier shell, kitchen shell, and a shared admin/manager shell. There is no employee shell. It declares 56 route entries including redirects and duplicate feature routes.

| Route | Page/component | Shell/visibility | Major functions and API usage |
|---|---|---|---|
| `/` | LoginPage | PUBLIC | employee login modes admin/cashier/kitchen; branch selector for cashier/kitchen |
| `/dashboard` | Dashboard | shared admin/manager shell | KPI summary, branch filter, branch list; Dashboard API |
| `/business-insights` | BusinessInsights | shared shell | list/filter insights, detail, mark read, resolve |
| `/products` | ProductManagement | shared shell | list/search, create/edit, status toggle, delete, topping lookup |
| `/tables` | TableManagement | shared shell | table/area/branch list, create/edit/delete, table status |
| `/invoices` and `/pos/invoices` | InvoiceHistory | shared/cashier | invoice search/date/payment filters, detail, receipt settings |
| `/expenses` | ExpenseManagement | shared shell | list/filter, create/edit/delete Expense; linked inventory protection is backend |
| `/inventory` | InventoryPage overview | shared shell only | KPI overview, low stock, branch inventory, item/receipt/issue navigation |
| `/inventory/items` | InventoryPage items | shared shell only | item list, create/edit, active state |
| `/inventory/receipts` | InventoryPage receipts | shared shell only | list/filter, create Draft, edit Draft, cancel/confirm |
| `/inventory/receipts/:id` | InventoryPage detail | shared shell only | receipt detail, Draft actions, confirmed read-only |
| `/inventory/issues` | InventoryPage issues | shared shell only | list/filter, create/edit Draft, cancel/confirm |
| `/inventory/issues/:id` | InventoryPage detail | shared shell only | issue detail, Draft actions, confirmed read-only |
| `/inventory/history` | InventoryPage history | shared shell only | transaction history, filters, pagination |
| `/employees` | EmployeeManagement | shared shell | list/filter, create/edit, active toggle, delete |
| `/attendance` | AttendanceManagement | shared shell | attendance list, branch/employee filters |
| `/schedule` | WorkSchedulePage | shared shell | schedule list, create/delete, employee/branch selection |
| `/customers` | CustomerManagement | shared shell | search/list, create/update, loyalty history |
| `/promotions` | PromotionManagement | shared shell | list, create/edit/delete, toggle |
| `/branches` | BranchManagement | admin-only element check | branch CRUD, status toggle |
| `/shifts` | ShiftManagement | shared shell | shift list; admin branch loading |
| `/reservations` | ReservationManagement | shared shell | list/filter, status update, delete |
| `/print-templates` | PrintTemplates | shared shell | print template UI; backend endpoint not present in current WebAPI inventory |
| `/settings/receipt` | ReceiptSettingsPage | shared/cashier conditional menu | branch receipt settings read/update |
| `/settings` | SystemSettings | shared shell with explicit admin/manager element check | branch settings read/update |
| `/toppings` | ToppingManagement | shared shell | topping CRUD |
| `/kitchen` | KitchenPage | shared shell or kitchen shell | active kitchen queue, product availability toggle, request status |
| `/kitchen/history` | KitchenHistoryPage | shared or kitchen shell | kitchen history/detail/status view |
| `/pos` | POSPage | shared/cashier | POS order creation/update, table selection, customer lookup, order accept, kitchen send, payment, loyalty redeem, shifts, reservations |
| `/pos/attendance` | EmployeeAttendance | cashier shell | personal attendance read/check-in/check-out |
| `/pos/schedule` | EmployeeSchedule | cashier shell | personal schedule read |
| `/pos/shifts` | ShiftManagement | cashier shell | shift read/open/close |
| `/pos/reservations` | ReservationManagement | cashier shell | reservation read/status/delete according to component |
| `/pos/profile` | EmployeeProfile | cashier shell | employee profile/read/logout |
| `/kitchen/tables` | TableStatusPage | kitchen shell | active table status read |
| `/kitchen/attendance` | EmployeeAttendance | kitchen shell | attendance operations |
| `/kitchen/schedule` | EmployeeSchedule | kitchen shell | schedule read |
| `/kitchen/reservations` | ReservationManagement readOnly | kitchen shell | reservation read-only UI |
| `/kitchen/profile` | EmployeeProfile | kitchen shell | employee profile/read/logout |
| `/profile` | ProfilePage | shared shell | `/api/Auth/me`, branch display, password change, profile/settings actions |
| `/support` | SupportPage | shared shell | support UI |
| `/forbidden` | ForbiddenPage | authenticated shells | access-denied display |

Navigation visibility is not a security boundary. `Navbar` shows Inventory only to admin/manager; CashierNavbar and KitchenNavbar do not show Inventory, although backend class-level Inventory reads permit cashier/kitchen. The shared shell has many routes without a role element guard, so direct navigation must be treated separately from menu visibility.

### Customer Web route structure

| Route | Page | Auth context | Major functions and APIs |
|---|---|---|---|
| `/` | DigitalMenu | registered or guest customer state | branch/table context, product/topping/promotion/menu read, cart, order creation, table status updates, customer profile refresh |
| `/reservation` | ReservationPage | customer web session | branch/table selection, reservation creation |
| `/profile` | CustomerProfile | registered or guest token state; profile functions require registered identity for protected APIs | profile read/update, order history, loyalty history, reservation history |
| `/scan` | QRScan | customer web | camera QR scan; navigates to table/menu context; browser camera permission required |
| `*` | redirect to `/` | current web session | route fallback |

Customer Web uses `customerToken`; guest login calls `/api/Auth/customer-token` with no phone/password, while registered login uses `/api/Customer/exists/{phoneNumber}`, `/api/Customer`, then customer-token. The current App route condition treats both as logged in, so registered-vs-guest restrictions remain backend-owned.

## 6. Functional catalog

The following catalog decomposes the current feature surface into 164 audited function/sub-function entries. Counts are catalog entries, not HTTP action counts.

| Module | Function/sub-function groups | Count |
|---|---|---:|
| Authentication/profile | employee login modes, customer token, current user, logout, password change, inactive-account rejection, legacy hash upgrade, profile read/update | 9 |
| Dashboard/analytics | summary, branch filter, invoice search/detail, business insight list/detail/unread/read/resolve | 10 |
| Orders/POS | list/search, detail, create, edit, quantity/item selection, customer lookup, accept web order, send kitchen, status, kitchen status, payment, payment method, loyalty read/redeem, delete, table status | 15 |
| Kitchen | active queue, detail, history/filter, request status, product availability, notifications, table view | 7 |
| Reservations | list/filter, detail, create, update, status, delete, customer ownership | 7 |
| Customers/loyalty | list/search, exists check, profile read/update, create/update, loyalty history, ownership restriction | 7 |
| Employees/HR | list/filter, detail, create, update, active toggle, delete, attendance list/check-in/out, schedules list/create/delete, shifts list/current/open/close | 18 |
| Branch/areas/tables | branch list/CRUD/status, area list/CRUD, table list/detail/CRUD/status | 16 |
| Catalog/promotions | products list/search/status, product CRUD, availability, toppings CRUD, promotions CRUD/status | 14 |
| Expenses/settings | Expense list/create/update/delete, receipt settings read/update, system settings read/update, print-template UI | 11 |
| Inventory | overview KPIs/list, item list/detail/create/update, branch stock, minimum stock, receipt list/detail/create/edit/confirm/cancel, issue list/detail/create/edit/confirm/cancel, transaction history/filters | 24 |
| Notifications | list/unread, mark one, mark all, pending-order badge query | 4 |
| AI | chat, menu, orders, booking, revenue, business summary, financial analysis, comparison, best sellers, staff, shifts, table summary, order status, product price | 14 |
| Customer QR | scan, table validation, branch/table context, guest token, menu/cart/order, reservation | 8 |

## 7. Role × function matrix

The matrix below uses the decomposed functions rather than only module names. `COND` always means the condition in the Notes column.

| Function | admin | manager | employee | cashier | kitchen | registered customer | guest QR |
|---|---|---|---|---|---|---|---|
| Employee login | ALLOWED | ALLOWED | DENIED by current modes | ALLOWED cashier mode | ALLOWED kitchen mode | N/A | N/A |
| Customer token | PUBLIC | PUBLIC | PUBLIC | PUBLIC | PUBLIC | PUBLIC | PUBLIC |
| Current-user claims | ALLOWED after JWT | ALLOWED after JWT | N/A until login exists | ALLOWED | ALLOWED | ALLOWED | ALLOWED |
| Change own password | ALLOWED | ALLOWED | COND if token exists | ALLOWED | ALLOWED | REGISTERED ONLY | DENIED guest |
| Dashboard summary | ALLOWED global/selected | ALLOWED branch condition | DENIED | DENIED | DENIED | DENIED | DENIED |
| Business insights/read/resolve | ALLOWED | ALLOWED | DENIED | DENIED | DENIED | DENIED | DENIED |
| Invoice list/detail | ALLOWED | ALLOWED | ALLOWED | ALLOWED | DENIED | DENIED | DENIED |
| POS create/update order | ALLOWED | ALLOWED | ALLOWED if authenticated path | ALLOWED | DENIED UI/API for payment/POS | PUBLIC/customer path | GUEST QR customer path |
| Accept web order | ALLOWED | ALLOWED | ALLOWED | ALLOWED | DENIED | DENIED | DENIED |
| Send order to kitchen | ALLOWED | ALLOWED | ALLOWED | ALLOWED | ALLOWED | customer create path only | guest create path only |
| Update kitchen/order status | ALLOWED | ALLOWED | ALLOWED | ALLOWED | ALLOWED | DENIED | DENIED |
| Pay order | ALLOWED | ALLOWED | ALLOWED | ALLOWED | DENIED | DENIED | DENIED |
| Loyalty read/redeem | staff by action / customer own read | same | read/use where action permits | read/use where action permits | DENIED | OWN registered | guest unsupported |
| Kitchen queue/history | ALLOWED | ALLOWED | DENIED | DENIED | ALLOWED | DENIED | DENIED |
| Product availability toggle | ALLOWED | ALLOWED | DENIED | DENIED | ALLOWED | DENIED | DENIED |
| Reservation read/create/update | AUTH/service condition | AUTH/service branch | AUTH per controller/service | AUTH per controller/service | read-only UI | OWN/registered or service guest path | guest create supported by tests/source |
| Customer list | ALLOWED | ALLOWED | ALLOWED | ALLOWED | DENIED | DENIED | DENIED |
| Customer own profile | CONDITIONAL | CONDITIONAL | DENIED | DENIED | DENIED | REGISTERED OWN | guest read/update denied |
| Customer loyalty history | ALLOWED | ALLOWED | ALLOWED | ALLOWED | DENIED | REGISTERED OWN | guest unsupported |
| Employee management | ALLOWED globally | ALLOWED, own branch only; cannot target management roles or elevate roles | DENIED except own profile read | DENIED except own profile read | DENIED except own profile read | DENIED | DENIED |
| Branch CRUD | ALLOWED | DENIED | DENIED | DENIED | DENIED | DENIED | DENIED |
| Area CRUD | ALLOWED | DENIED | DENIED | DENIED | DENIED | DENIED | DENIED |
| Table CRUD | ALLOWED | ALLOWED | DENIED except status | DENIED except status | DENIED | PUBLIC read | PUBLIC read |
| Catalog read | PUBLIC | PUBLIC | PUBLIC | PUBLIC | PUBLIC | PUBLIC | PUBLIC |
| Product CRUD | ALLOWED | DENIED | DENIED | DENIED | DENIED | DENIED | DENIED |
| Topping CRUD | ALLOWED | DENIED | DENIED | DENIED | DENIED | DENIED | DENIED |
| Promotion CRUD | ALLOWED | DENIED | DENIED | DENIED | DENIED | PUBLIC read | PUBLIC read |
| Expense CRUD | ALLOWED | ALLOWED | DENIED | DENIED | DENIED | DENIED | DENIED |
| Receipt settings | ALLOWED | ALLOWED branch | ALLOWED branch | ALLOWED branch | DENIED | DENIED | DENIED |
| System settings | ALLOWED | ALLOWED branch/service | AUTH attribute but action scope needs review | same | same | DENIED | DENIED |
| Attendance | ALLOWED | ALLOWED | ALLOWED | ALLOWED | ALLOWED | DENIED | DENIED |
| Work schedule | ALLOWED | ALLOWED | ALLOWED | ALLOWED | ALLOWED | DENIED | DENIED |
| Shift | ALLOWED | ALLOWED | N/A/role not accepted | ALLOWED | DENIED; no Kitchen route | DENIED | DENIED |
| Notifications | own role/user/branch context | own context | own context if token | own context | own context | own context | own guest context if emitted |
| Inventory overview | ALLOWED | ALLOWED own/selected branch | ALLOWED own branch if token | ALLOWED own branch | ALLOWED own branch | DENIED | DENIED |
| Inventory item master | ALLOWED | ALLOWED | DENIED | DENIED | DENIED | DENIED | DENIED |
| Branch stock/minimum | ALLOWED | ALLOWED own branch | READ own branch; minimum DENIED | READ own branch; minimum DENIED | READ own branch | DENIED | DENIED |
| Receipt Draft create/edit | ALLOWED | ALLOWED own branch | DENIED | DENIED | DENIED | DENIED | DENIED |
| Receipt confirm/cancel | ALLOWED | ALLOWED own branch | DENIED | DENIED | DENIED | DENIED | DENIED |
| Issue Draft create/edit/cancel | ALLOWED | ALLOWED own branch | CONDITIONAL own branch | CONDITIONAL own branch | CONDITIONAL own branch | DENIED | DENIED |
| Issue confirm | ALLOWED | ALLOWED own branch | DENIED | DENIED | DENIED | DENIED | DENIED |
| Inventory transaction history | ALLOWED | ALLOWED own/selected branch | ALLOWED own branch | ALLOWED own branch | ALLOWED own branch | DENIED | DENIED |
| AI chat | PUBLIC; tools filtered by admin | PUBLIC; manager tools | PUBLIC/role from JWT if role exists | PUBLIC/role from JWT | PUBLIC/role from JWT | customer tool set | anonymous defaults customer |
| AI price update | ALLOWED | DENIED | DENIED | DENIED | DENIED | DENIED | DENIED |
| AI order status | ALLOWED | ALLOWED | ALLOWED | ALLOWED | ALLOWED | DENIED | DENIED |
| AI customer booking/menu/orders | menu only; customer tools not all applicable | menu | menu/order list | menu/order list | menu/order list | menu, own orders, booking | menu; booking/order ownership limited |

## 8. Branch-scope matrix

| Area | admin | manager | employee | cashier | kitchen | customer/guest |
|---|---|---|---|---|---|---|
| JWT branch claim | optional | required for branch-owned operations; fail closed when missing/invalid | expected if authenticated | expected | expected | absent for customer JWT |
| Inventory | global or selected branch | assigned branch only | assigned branch only | assigned branch only | assigned branch only | denied |
| Receipt/system settings | admin/global or requested permitted branch | assigned branch | controller accepts role but branch helper applies | assigned branch | denied receipt settings | denied |
| Orders/tables/reservations | global or selected branch where supported | assigned branch; route/query/entity IDs cannot widen scope | operational branch | operational branch | kitchen branch | customer-owned/table context |
| AI context | admin may have null branch/global | valid branch required before any manager tool execution | branch context | branch context | branch context | no branch unless table/order context is passed |

Task 47F completed the service-level review. Inventory and settings retain their existing branch helpers; Employee, Dashboard, schedules, Shift, Table detail, Kitchen request detail, Business Insights, and AI now fail closed for managers without trusted branch context. Intentional global master data remains documented separately.

## 9. Registered customer versus guest QR matrix

| Function | Registered customer | Guest QR customer |
|---|---|---|
| Token acquisition | phone exists, customer-token returns registered session | Customer Web guest action requests customer-token without phone/password |
| Customer identity | persistent customer ID and phone | generated guest identity; no registered customer ID |
| Menu/catalog | PUBLIC | PUBLIC |
| Table/QR context | may use table scan/context | primary intended flow |
| Create order | customer web order path | guest QR order path |
| Profile read/update | own phone/ID only | no registered profile ownership |
| Order history | own data where service recognizes identity | guest history not equivalent to registered history |
| Reservation | customer flow; authentication/service condition | source/tests support guest creation path, ownership remains limited |
| Loyalty history/redeem | own registered ID; protected | not supported as registered loyalty |
| Password change | customer type only, requires persisted password and own ID | denied/no persisted customer identity/password |
| AI | customer tools including own orders/booking/menu | anonymous defaults to customer role; no authenticated ownership claim |

## 10. Frontend/backend consistency findings

1. **[High] employee login/shell mismatch.** Domain defaults `Employee.Role` to `employee`, but `AuthController` has no mode that emits employee JWT; `LoginPage` has no employee mode; `App.tsx` has no employee shell. Backend attributes nevertheless reference employee on Orders, Attendance, Invoice, WorkSchedule, Inventory, and AI tools.
2. **[Resolved by Task 47E] EmployeeController action-level authorization.** Management actions now require `admin` or `manager`; employee detail remains available only to employee-role contexts, with admin/global, manager same-branch, and operational self-profile rules. Manager mutations are branch-scoped and cannot target management roles or elevate a role.
3. **[High] direct-route/API mismatch for Inventory.** Admin Web only shows Inventory in Navbar for admin/manager, but the shared admin/manager shell route declarations are not independently role-guarded and Inventory backend reads permit employee/cashier/kitchen. Backend remains authoritative, but UI visibility does not describe all API-allowed operational reads.
4. **[Resolved by Task 47G] Kitchen shift route mismatch.** `ShiftController` remains restricted to admin/manager/cashier because it implements cashier cash-shift opening, closing, reconciliation, and revenue history. The Kitchen navbar and `/kitchen/shifts` route were removed; direct navigation now falls through to the Kitchen home route.
5. **[Medium] shared-shell route exposure.** Many shared routes (employees, customers, promotions, toppings, kitchen, POS, reservations, schedules) are declared without per-role route guards; only navigation visibility and selected element checks restrict UX. Direct navigation is not equivalent to backend authorization.
6. **[Medium] public route breadth.** Product, Promotion, Topping, Table, Area, and Branch list endpoints are public in current controller source. This may be intentional for customer/QR flows but should be explicitly accepted as public surface.
7. **[Medium] AI controller public surface.** `/api/Ai/chat` has no `[Authorize]`; anonymous context defaults to customer and tools are filtered later. This is not equivalent to an authenticated-only assistant.
8. **[Documentation]** Thesis source says Inventory is not evidenced, while current source includes the full Inventory module. The thesis must be updated in a future documentation task, not silently treated as current implementation evidence.
9. **[Resolved by Task 47F] Manager branch isolation.** All 99 manager-reachable actions were audited. Seventeen paths were hardened, including missing-branch fail-closed behavior, schedule employee ownership, shift reconciliation, entity-ID detail access, Business Insights, and AI tools. Admin Web no longer advertises branch switching or branch management to managers.

## 11. Inventory Phase 4 role map

| Runtime scenario | Required test role | Reason |
|---|---|---|
| Overview / branch stock / history | manager own branch; admin global/selected branch | validates branch scope and admin scope |
| Item creation | admin and manager | item master API restriction |
| Duplicate item | admin and manager | normalized-name conflict |
| MinimumStock | manager own branch; admin second branch if safe | branch-specific management |
| Receipt Draft/edit | manager own branch; admin optional | R(admin,manager), Draft-only |
| Receipt confirmation/Expense | manager own branch; admin optional | financial and stock mutation authorization |
| Confirmed receipt immutability | manager | state transition and read-only contract |
| Issue Draft/edit | employee/cashier/kitchen own branch, if valid JWT fixture exists; manager comparison | operational draft policy |
| Issue confirmation | manager/admin | confirmation role gate |
| Transaction history | all operational roles in own branch | read branch scope |
| Low/out-of-stock transitions | manager/admin | requires mutation; dedicated E2E only |
| Insufficient-stock rejection | manager/admin | requires mutation; dedicated E2E only |
| Draft cancellation | manager/admin; operational issue-draft roles | mutation/state policy |
| Filters/pagination | manager/admin and operational read roles | query scope |
| Cross-branch authorization | manager against another existing branch | read-only branch denial; admin comparison |

No mutation was executed for this audit.

## 12. Test-count discrepancy: 216 versus 215

The earlier `215/215` result came from a stale test assembly. Current source already contains `[Fact]` immediately before `Overview_returns_empty_quantity_groups_when_branch_has_no_transactions`; no test attribute was missing and no duplicate equivalent test was found.

After rebuilding the test project:

- the specific regression test was discovered and passed: `1/1`;
- the Inventory test class passed: `9/9`;
- the complete backend suite passed: `216/216`, with 0 failed and 0 skipped.

No test source or production source was changed for this correction. The authoritative current baseline is `216/216`.

Task 47E and Task 47G raised the baseline to `227/227`. Task 47F added 12 focused manager branch-isolation tests; the authoritative current suite is `239/239`, with 0 failed and 0 skipped.

## Task 47D update — unified employee-system login

The historical role-specific `admin`/`cashier`/`kitchen` login modes have been replaced by semantic `management` and `staff` groups. `management` accepts only stored `admin`/`manager` roles; `staff` accepts only stored `employee`/`cashier`/`kitchen` roles. The database role, not a request value, is emitted into the JWT. Employee now has the `/staff` shell for profile, own attendance, own schedule, and the existing inventory overview read model. Customer authentication remains separate.

## Task 47H update — public and AI boundary

Anonymous access is now limited to deliberate customer bootstrap/menu/QR-reservation surfaces. Public branches use a customer-safe active-only DTO and public/customer product reads exclude inactive products. AI chat is authenticated-only; guest QR sessions are denied and registered customer identity is derived from JWT claims before customer tools run. The QR table GUID and phone-only customer-token bootstrap remain explicit policy gaps documented in `51-PUBLIC-GUEST-CUSTOMER-AI-SECURITY.md`; they are not equivalent to staff or registered-customer authorization proof.

## 13. Unresolved authorization questions

- Should employee authentication receive an explicit login mode and Admin/Staff shell?
- Employee response data still uses the existing entity-shaped contract (with `Password` nulled); a separate data-minimization review may be warranted for sensitive fields such as salary/citizen data.
- Manager branch scope was audited and hardened system-wide in Task 47F. Real API/PostgreSQL cross-branch runtime confirmation remains a human follow-up.
- Kitchen cash-shift access was reconciled in Task 47G by removing `/kitchen/shifts`; Kitchen uses the existing attendance and schedule routes instead.
- Should public catalog/table/branch routes remain public or use a distinct guest/table token policy?
- Should `/api/Ai/chat` remain public with anonymous customer context, or require authenticated customer/staff sessions for selected tools?
- Should route guards be centralized instead of relying on shell selection/navigation visibility?

## 14. Recommended follow-up tasks

- `TASK 47D` — Employee authentication mode, JWT role issuance, and employee Admin Web shell.
- Task 47E is complete: EmployeeController action-level role gates, manager branch checks, and privilege-elevation regression tests were added without changing employee authentication.
- Task 47F is complete: 99 manager-reachable actions were audited, 17 paths were hardened, and the full backend suite passes 239/239.
- `TASK 47G` — Kitchen route/API consistency for shifts and other shared-shell pages.
- `TASK 47H` — Public endpoint and anonymous AI policy review.
- `TASK 47I` — Add the missing `[Fact]` only after confirming the intended test baseline and update the documented count.

## 15. Audit conclusion

This document is a source-derived authorization inventory, not runtime authorization validation. Runtime role testing remains a separate task requiring running API/frontend and credentials. No source behavior was changed by this audit.
