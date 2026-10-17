# THESIS DIAGRAM SOURCE AUDIT

## 1. Executive Summary

PhĂ¡ÂºÂ¡m vi lÄ‚Â  audit read-only cho bĂ¡Â»â€˜n biĂ¡Â»Æ’u Ă„â€˜Ă¡Â»â€œ ChĂ†Â°Ă†Â¡ng 3: Domain Class Diagram (3.2.5), ERD (3.3.1), System Architecture Diagram (3.4.1) vÄ‚Â  AI Agent Architecture Diagram (3.6.1).

NguĂ¡Â»â€œn Ă†Â°u tiÄ‚Âªn: current Domain entities, ApplicationDbContext, OnModelCreating, current model snapshot, Program.cs, Application Services, Controllers/Hubs, AI runtime vÄ‚Â  frontend source. Historical migrations/docs chĂ¡Â»â€° dÄ‚Â¹ng Ă„â€˜Ă¡Â»Æ’ nhĂ¡ÂºÂ­n diĂ¡Â»â€¡n khÄ‚Â¡c biĂ¡Â»â€¡t; current model thĂ¡ÂºÂ¯ng.

KĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n chÄ‚Â­nh:

- Current runtime cÄ‚Â³ 29 DbSet/entity persisted.
- KhÄ‚Â´ng cÄ‚Â³ current Restaurant, Category entity, ProductTopping, OrderDetailTopping, Invoice, removed staff-finance module hoĂ¡ÂºÂ·c OrderItem.
- Runtime dÄ‚Â¹ng RestaurantTable (file Table.cs), OrderDetail (nested trong Order.cs), OrderRequest vÄ‚Â  OrderRequestItem.
- Product.Category lÄ‚Â  string vÄ‚Â  Product.ToppingsJson lÄ‚Â  string; khÄ‚Â´ng cÄ‚Â³ ProductTopping join entity.
- OrderDetail cÄ‚Â³ ToppingId nullable trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p; khÄ‚Â´ng cÄ‚Â³ OrderDetailTopping.
- KDS status nĂ¡ÂºÂ±m trÄ‚Âªn OrderRequest; SentQuantity nĂ¡ÂºÂ±m trÄ‚Âªn OrderDetail; OrderRequestItem khÄ‚Â´ng cÄ‚Â³ OrderDetailId.
- CÄ‚Â³ 2 frontend app: apps/admin-web vÄ‚Â  apps/customer-web. POS/KDS lÄ‚Â  logical modules trong admin-web, khÄ‚Â´ng phĂ¡ÂºÂ£i app riÄ‚Âªng.
- Backend lÄ‚Â  mĂ¡Â»â„¢t ASP.NET Core 8 project chĂ¡Â»Â©a cÄ‚Â¡c logical layers. MÄ‚Â´ tĂ¡ÂºÂ£ chÄ‚Â­nh xÄ‚Â¡c: layered architecture inspired by Clean Architecture; Application Services hiĂ¡Â»â€¡n dÄ‚Â¹ng ApplicationDbContext trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p.
- PostgreSQL/Npgsql vÄ‚Â  Supabase connection configuration Ă„â€˜Ă†Â°Ă¡Â»Â£c source chĂ¡Â»Â©ng minh. KhÄ‚Â´ng cÄ‚Â³ Docker, payment gateway server-side hoĂ¡ÂºÂ·c webhook Ă„â€˜Ă†Â°Ă¡Â»Â£c xÄ‚Â¡c minh.
- SignalR hub lÄ‚Â  /kitchenHub. Events: NewOrderRequest, RequestStatusUpdated, PaymentCompleted. kitchen-admin vÄ‚Â  Clients.All tĂ¡ÂºÂ¡o global behavior.
- AI cÄ‚Â³ Ă„â€˜Ä‚Âºng 14 runtime tools; definitions lĂ¡Â»Âc theo role trĂ†Â°Ă¡Â»â€ºc AI provider, permission kiĂ¡Â»Æ’m tra lĂ¡ÂºÂ¡i sau function call, loop tĂ¡Â»â€˜i Ă„â€˜a 5 lĂ†Â°Ă¡Â»Â£t, xĂ¡Â»Â­ lÄ‚Â½ tuĂ¡ÂºÂ§n tĂ¡Â»Â±.

TrĂ¡ÂºÂ¡ng thÄ‚Â¡i:

- VERIFIED: evidence trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p tĂ¡Â»Â« current executable source/runtime model.
- PARTIALLY VERIFIED: chĂ¡Â»â€° mĂ¡Â»â„¢t phĂ¡ÂºÂ§n claim Ă„â€˜Ă†Â°Ă¡Â»Â£c chĂ¡Â»Â©ng minh.
- NOT VERIFIED: khÄ‚Â´ng Ă„â€˜Ă¡Â»Â§ evidence hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i.
- FALSE / MUST CHANGE: proposal mÄ‚Â¢u thuĂ¡ÂºÂ«n current source.

KhÄ‚Â´ng tĂ¡ÂºÂ¡o PlantUML, khÄ‚Â´ng sĂ¡Â»Â­a source/database/migration/config/test/frontend/thesis. Sole intentional output lÄ‚Â  file audit nÄ‚Â y.

## 2. Current Domain Entity Inventory

| Entity | File | Runtime | DbSet | Main purpose | Diagram |
|---|---|---|---|---|---|
| Area | Domain/Entities/Area.cs | YES | Areas | area label/order | SHOULD INCLUDE; khÄ‚Â´ng cÄ‚Â³ BranchId/FK |
| Attendance | Domain/Entities/Attendance.cs | YES | Attendances | employee check-in/out | SHOULD INCLUDE |
| Branch | Domain/Entities/Branch.cs | YES | Branches | branch/store | MUST INCLUDE |
| BranchInventory | Domain/Entities/BranchInventory.cs | YES | BranchInventories | branch stock quantity | MUST INCLUDE inventory |
| BusinessInsight | Domain/Entities/BusinessInsight.cs | YES | BusinessInsights | persisted insight | SHOULD INCLUDE |
| Customer | Domain/Entities/Customer.cs | YES | Customers | customer/loyalty owner | MUST INCLUDE |
| Employee | Domain/Entities/Employee.cs | YES | Employees | staff identity/role | MUST INCLUDE |
| Expense | Domain/Entities/Expense.cs | YES | Expenses | branch expense | SHOULD INCLUDE |
| InventoryItem | Domain/Entities/InventoryItem.cs | YES | InventoryItems | global inventory catalog | MUST INCLUDE inventory |
| LoyaltyTransaction | Domain/Entities/LoyaltyTransaction.cs | YES | LoyaltyTransactions | earn/redeem ledger | MUST INCLUDE |
| Notification | Domain/Entities/Notification.cs | YES | Notifications | targeted notification | OPTIONAL |
| Order | Domain/Entities/Order.cs | YES | Orders | order/payment | MUST INCLUDE |
| OrderDetail | nested in Domain/Entities/Order.cs | YES | OrderDetails | order line | MUST INCLUDE |
| OrderRequest | Domain/Entities/OrderRequest.cs | YES | OrderRequests | KDS request batch | MUST INCLUDE |
| OrderRequestItem | nested in Domain/Entities/OrderRequest.cs | YES | OrderRequestItems | KDS item snapshot | MUST INCLUDE |
| Product | Domain/Entities/Product.cs | YES | Products | menu product | MUST INCLUDE |
| Promotion | Domain/Entities/Promotion.cs | YES | Promotions | promotion/points | OPTIONAL |
| ReceiptSettings | Domain/Entities/ReceiptSettings.cs | YES | ReceiptSettings | receipt settings | OPTIONAL |
| Reservation | Domain/Entities/Reservation.cs | YES | Reservations | reservation | SHOULD INCLUDE |
| RestaurantTable | Domain/Entities/Table.cs | YES | Tables | table/QR token | MUST INCLUDE |
| Shift | Domain/Entities/Shift.cs | YES | Shifts | shift/cash session | SHOULD INCLUDE |
| StockIssue | Domain/Entities/StockIssue.cs | YES | StockIssues | stock-out document | MUST INCLUDE inventory |
| StockIssueItem | Domain/Entities/StockIssueItem.cs | YES | StockIssueItems | stock-out line | MUST INCLUDE inventory |
| StockReceipt | Domain/Entities/StockReceipt.cs | YES | StockReceipts | stock-in document | MUST INCLUDE inventory |
| StockReceiptItem | Domain/Entities/StockReceiptItem.cs | YES | StockReceiptItems | stock-in line | MUST INCLUDE inventory |
| StockTransaction | Domain/Entities/StockTransaction.cs | YES | StockTransactions | stock movement/audit | MUST INCLUDE inventory |
| SystemSetting | Domain/Entities/SystemSetting.cs | YES | SystemSettings | branch/key/value setting | OPTIONAL |
| Topping | Domain/Entities/Topping.cs | YES | Toppings | topping catalog | MUST INCLUDE |
| WorkSchedule | Domain/Entities/WorkSchedule.cs | YES | WorkSchedules | employee schedule | SHOULD INCLUDE |

Explicit absence:

| Proposed name | Current result | Status |
|---|---|---|
| Restaurant / RestaurantId | no current class/property; root is Branch/BranchId | FALSE / MUST CHANGE |
| Category entity | no class/DbSet; Product.Category is string | FALSE / MUST CHANGE |
| ProductTopping | no class/DbSet/configuration | FALSE / MUST CHANGE |
| OrderDetailTopping | no class/DbSet/configuration | FALSE / MUST CHANGE |
| Invoice | no class/DbSet; Order.InvoiceCode exists | FALSE / MUST CHANGE |
| removed staff-finance module | no current Domain/DbContext; only historical migration evidence | FALSE / MUST CHANGE |
| OrderItem | current runtime name is OrderDetail | FALSE / MUST CHANGE |

## 3. Canonical Domain Class Model

### A. MUST INCLUDE

Branch, Employee, Customer, RestaurantTable, Product, Topping, Order, OrderDetail, OrderRequest, OrderRequestItem and LoyaltyTransaction.

For a complete current-system diagram also include Attendance, WorkSchedule, Shift, Reservation, Expense, BusinessInsight, InventoryItem, BranchInventory, StockReceipt, StockReceiptItem, StockIssue, StockIssueItem and StockTransaction.

### B. SHOULD INCLUDE

Area, Promotion, ReceiptSettings, SystemSetting and Notification when the diagram scope includes those modules.

### C. OPTIONAL

Inventory enums and finance enums may be shown as types/constraints, not as database entities.

### D. MUST NOT INCLUDE

Restaurant, RestaurantId, Category entity, ProductTopping, OrderDetailTopping, Invoice, removed staff-finance module, OrderItem, Ingredient, Recipe, ProductIngredient, Supplier, StockMovement or invented Adjustment entity.

Canonical text model:

    Branch 1 -> * Employee
    Branch 1 -> * Attendance
    Branch 1 -> * WorkSchedule
    Branch 1 -> * Expense
    Branch 1 -> * RestaurantTable
    Branch 1 -> * BranchInventory
    Branch 1 -> * StockReceipt
    Branch 1 -> * StockIssue
    Branch 1 -> * StockTransaction

    Customer 0..1 -> * Order
    Customer 1 -> * LoyaltyTransaction
    Order 1 -> * OrderDetail
    Order 1 -> * OrderRequest
    Order 0..1 -> * LoyaltyTransaction
    OrderRequest 1 -> * OrderRequestItem

    Product 0..1 -> * OrderDetail
    Topping 0..1 -> * OrderDetail
    InventoryItem 1 -> * BranchInventory
    InventoryItem 1 -> * StockReceiptItem
    InventoryItem 1 -> * StockIssueItem
    InventoryItem 1 -> * StockTransaction
    StockReceipt 1 -> * StockReceiptItem
    StockIssue 1 -> * StockIssueItem
    Expense 0..1 -> 1 StockReceipt

The model intentionally does not draw Area-Table, Table-Order, Customer-Reservation, Employee-Shift, OrderDetail-OrderRequestItem or Product-Topping as formal FK relations because current source does not configure those relationships.

## 4. Entity Field Matrix

| Entity | PK | Important scalar/FK properties | Navigation | Status | Diagram fields |
|---|---|---|---|---|---|
| Branch | Id | Name, Address?, PhoneNumber?, IsMain, IsActive, TaxCode, RepresentativeName? | none | IsActive | Id, Name, IsActive |
| Employee | Id | EmployeeCode?, FullName, Position?, Department?, BasicSalary, BranchId?, Role, IsActive | none | Role, IsActive | Id, FullName, Role, BranchId, IsActive |
| Customer | Id | FullName?, PhoneNumber?, Email?, TotalSpending, TotalOrders, LoyaltyPoints, IsActive | none | IsActive | Id, FullName, PhoneNumber, LoyaltyPoints |
| Area | Id | Name, DisplayOrder | none | none | Id, Name |
| RestaurantTable | Id | Name, AreaName, SeatCount, IsActive, Status, QrToken, BranchId? | none | Status, IsActive | Id, Name, BranchId, Status, QrToken |
| Product | Id | Code?, Name?, Category?, Group?, Price, CostPrice, SizesJson?, ToppingsJson?, IsActive, AvailabilityStatus | none | AvailabilityStatus, IsActive | Id, Name, Price, Category, IsActive |
| Topping | Id | Name, Price, CostPrice, ToppingType?, Category?, IsActive | none | IsActive | Id, Name, Price, IsActive |
| Order | Id | InvoiceCode?, CreatedAt, PaymentAt?, CustomerId?, SubTotal?, VatAmount?, VatPercent?, ServiceFeeAmount?, ServiceFeePercent?, TotalAmount, Discount, PaidAmount, PaymentMethod?, TableName?, BranchId? | Details: List<OrderDetail> | Status | Id, BranchId, CustomerId, Status, TotalAmount, PaidAmount |
| OrderDetail | Id | ProductId?, ToppingId?, ProductName, Quantity, SentQuantity, UnitPrice, Options? | none | none | Id, ProductId, ToppingId, Quantity, SentQuantity, UnitPrice |
| OrderRequest | Id | OrderId, RequestNumber, CreatedAt, AcceptedAt?, PreparingAt?, CompletedAt?, ProcessedBy? | Items | Status | Id, OrderId, RequestNumber, Status |
| OrderRequestItem | Id | OrderRequestId, ProductId?, ProductName, Quantity, Note?, Options? | none | none | Id, OrderRequestId, ProductId, Quantity |
| LoyaltyTransaction | Id | CustomerId, OrderId?, Type, Points, BalanceAfter, Description? | none | Type | Id, CustomerId, OrderId, Type, Points |
| Reservation | Id | CustomerName?, CustomerPhone?, ReservationTime, NumberOfGuests, BranchId?, TableId?, Status | none | Status | Id, BranchId, TableId, ReservationTime, Status |
| Attendance | Id | EmployeeId, CheckInTime, CheckOutTime?, BranchId, Status? | none | Status | Id, EmployeeId, BranchId, CheckInTime |
| WorkSchedule | Id | EmployeeId, Date, ShiftName?, StartTime?, EndTime?, BranchId? | none | none | Id, EmployeeId, BranchId, Date |
| Shift | Id | EmployeeId, BranchId, StartTime, EndTime?, StartingCash, EndingCash?, TotalRevenue, CashRevenue, TransferRevenue, Status | none | Status | Id, EmployeeId, BranchId, Status |
| Expense | Id | BranchId, Description, Amount, ExpenseDate, Category, PaymentMethod, CreatedBy, StockReceiptId? | StockReceipt? | none | Id, BranchId, Amount, ExpenseDate, StockReceiptId |
| BusinessInsight | Id | BranchId?, Type, Severity, Title, Summary, EvidenceJson, PeriodStart, PeriodEnd, DeduplicationKey | none | Status | Id, BranchId, Type, Severity, Status |
| InventoryItem | Id | Name, NormalizedName, UnitCode, IsActive | BranchInventories | IsActive | Id, Name, UnitCode, IsActive |
| BranchInventory | Id | BranchId, InventoryItemId, CurrentQuantity, MinimumStock | Branch?, InventoryItem? | none | Id, BranchId, InventoryItemId, CurrentQuantity |
| StockReceipt | Id | BranchId, SupplierName?, PurchaseDate?, TotalAmount, Status, CreatedBy, IdempotencyKey? | Branch?, Items | Status | Id, BranchId, TotalAmount, Status |
| StockReceiptItem | Id | StockReceiptId, InventoryItemId, Quantity, UnitPrice | StockReceipt?, InventoryItem? | none | Id, StockReceiptId, InventoryItemId, Quantity, UnitPrice |
| StockIssue | Id | BranchId, Reason, Status, CreatedBy, IdempotencyKey? | Branch?, Items | Status | Id, BranchId, Reason, Status |
| StockIssueItem | Id | StockIssueId, InventoryItemId, Quantity | StockIssue?, InventoryItem? | none | Id, StockIssueId, InventoryItemId, Quantity |
| StockTransaction | Id | BranchId, InventoryItemId, Type, Quantity, BeforeQuantity, AfterQuantity, ReferenceType, ReferenceId, CreatedBy | Branch?, InventoryItem? | Type | Id, BranchId, InventoryItemId, Type, Quantity |
| ReceiptSettings | Id | BranchId?, PaperWidth, display flags, FontFamily, FontSize | none | none | Id, BranchId |
| SystemSetting | Id | BranchId?, Key, Value, Type, UpdatedAt | none | none | Id, BranchId, Key, Type |
| Notification | Id | Type, EntityType?, EntityId?, TargetRole?, TargetUserId?, BranchId?, IsRead | none | IsRead | Id, TargetRole, TargetUserId, BranchId |
| Promotion | Id | Name, RequiredPoints, DiscountValue, PromotionType, IsActive, EndDate? | none | IsActive | Id, Name, DiscountValue |

## 5. Entity Relationship Matrix

Evidence: ApplicationDbContext.OnModelCreating, entity property declarations and current ApplicationDbContextModelSnapshot.

| Entity A | Relation | Entity B | FK | Required/optional | Evidence/behavior |
|---|---|---|---|---|---|
| Branch | 1-* configured | Employee | Employee.BranchId | nullable | HasOne Branch / HasForeignKey |
| Branch | 1-* configured | RestaurantTable | RestaurantTable.BranchId | nullable | HasOne Branch / HasForeignKey |
| Branch | 1-* configured | Attendance | Attendance.BranchId | required | HasOne Branch / HasForeignKey |
| Branch | 1-* configured | WorkSchedule | WorkSchedule.BranchId | nullable | HasOne Branch / HasForeignKey |
| Branch | 1-* configured | Expense | Expense.BranchId | required | Restrict |
| Branch | 1-* configured | BranchInventory | BranchInventory.BranchId | required | Restrict |
| Branch | 1-* configured | StockReceipt | StockReceipt.BranchId | required | Restrict |
| Branch | 1-* configured | StockIssue | StockIssue.BranchId | required | Restrict |
| Branch | 1-* configured | StockTransaction | StockTransaction.BranchId | required | Restrict |
| Branch | scalar ownership | Order | Order.BranchId | nullable | no explicit fluent relation |
| Branch | scalar ownership | Reservation | Reservation.BranchId | nullable | no explicit fluent relation |
| Branch | scalar/index scope | BusinessInsight | BusinessInsight.BranchId | nullable | index, no navigation |
| Area | no relation | RestaurantTable | none | N/A | Table has AreaName, no AreaId |
| Product | 1-* optional principal | OrderDetail | OrderDetail.ProductId | nullable | IsRequired(false) |
| Topping | 1-* optional principal | OrderDetail | OrderDetail.ToppingId | nullable | IsRequired(false) |
| Product/Topping | no many-to-many | Ă¢â‚¬â€ | Ă¢â‚¬â€ | N/A | Product.ToppingsJson string; no join entity |
| Order | 1-* | OrderDetail | shadow OrderId | required | Cascade |
| Order | 1-* | OrderRequest | OrderRequest.OrderId | required | no cascade explicit |
| Customer | 0..1-* | Order | Order.CustomerId | nullable | explicit optional FK |
| Customer | 1-* | LoyaltyTransaction | LoyaltyTransaction.CustomerId | required | configured FK |
| Order | 0..1-* | LoyaltyTransaction | LoyaltyTransaction.OrderId | nullable | filtered unique OrderId+Type |
| Customer | no relation | Reservation | none | N/A | no CustomerId |
| RestaurantTable | no FK relation | Order | none | N/A | Order has TableName, not TableId |
| OrderRequest | 1-* | OrderRequestItem | OrderRequestItem.OrderRequestId | required | Cascade |
| OrderDetail | no relation | OrderRequestItem | no OrderDetailId | N/A | snapshot ProductId/ProductName/Quantity |
| InventoryItem | 1-* | BranchInventory | BranchInventory.InventoryItemId | required | Restrict; unique pair with Branch |
| InventoryItem | 1-* | StockReceiptItem | StockReceiptItem.InventoryItemId | required | Restrict |
| InventoryItem | 1-* | StockIssueItem | StockIssueItem.InventoryItemId | required | Restrict |
| InventoryItem | 1-* | StockTransaction | StockTransaction.InventoryItemId | required | Restrict |
| StockReceipt | 1-* | StockReceiptItem | StockReceiptItem.StockReceiptId | required | Restrict |
| StockIssue | 1-* | StockIssueItem | StockIssueItem.StockIssueId | required | Restrict |
| Expense | 0..1-1 | StockReceipt | Expense.StockReceiptId | nullable | unique filtered index, Restrict |
| Employee | configured *-1 | Attendance | Attendance.EmployeeId | required | HasOne Employee |
| Employee | configured *-1 | WorkSchedule | WorkSchedule.EmployeeId | required | HasOne Employee |
| Employee | scalar only | Shift | Shift.EmployeeId | required | no fluent relation |

Do not convert scalar fields into formal relationship arrows in the ERD.

## 6. Inventory Domain Deep Audit

### Current Inventory Domain Model exactly

InventoryItem, BranchInventory, StockReceipt, StockReceiptItem, StockIssue, StockIssueItem and StockTransaction.

No current Ingredient, StockMovement, InventoryTransaction, Recipe, ProductIngredient, Unit, Supplier, Import, Export or Adjustment entity was found.

| Entity | DbSet/table | PK/FK | Navigation | BranchId | Constraints/indexes | Service/API |
|---|---|---|---|---|---|---|
| InventoryItem | InventoryItems | Id | BranchInventories | no | unique active NormalizedName | InventoryService; /api/inventory/items |
| BranchInventory | BranchInventories | Id; BranchId; InventoryItemId | Branch, InventoryItem | direct required | unique branch/item; quantities nonnegative; Restrict | InventoryService; /api/inventory/branches |
| StockReceipt | StockReceipts | Id; BranchId | Branch, Items | direct required | status/total checks; idempotency unique | InventoryService; /api/inventory/receipts |
| StockReceiptItem | StockReceiptItems | Id; StockReceiptId; InventoryItemId | StockReceipt, InventoryItem | no direct | quantity/price checks | InventoryService |
| StockIssue | StockIssues | Id; BranchId | Branch, Items | direct required | status/idempotency checks | InventoryService; /api/inventory/issues |
| StockIssueItem | StockIssueItems | Id; StockIssueId; InventoryItemId | StockIssue, InventoryItem | no direct | quantity positive | InventoryService |
| StockTransaction | StockTransactions | Id; BranchId; InventoryItemId | Branch, InventoryItem | direct required | type/reference/quantity checks | InventoryService; /api/inventory/transactions |

InventoryService uses InventoryUserContext from role, NameIdentifier and branchId claims. Non-admin users cannot access another branch. PostgreSQL path locks BranchInventory rows using FOR UPDATE. Receipt/issue idempotency keys are supported.

Inventory class diagram must use the seven exact entities. ERD must show BranchInventory as the branch quantity table and StockTransaction as the movement/audit table; do not invent ProductIngredient or Supplier.

## 7. Current DbSet Inventory

There are 29 current DbSets in ApplicationDbContext:

| DbSet | Entity | Current table |
|---|---|---|
| Products | Product | Products |
| Orders | Order | Orders |
| OrderDetails | OrderDetail | OrderDetails |
| Expenses | Expense | Expenses |
| Employees | Employee | Employees |
| Tables | RestaurantTable | Tables |
| Areas | Area | Areas |
| Branches | Branch | Branches |
| Attendances | Attendance | Attendances |
| WorkSchedules | WorkSchedule | WorkSchedules |
| Customers | Customer | Customers |
| Shifts | Shift | Shifts |
| Reservations | Reservation | Reservations |
| Toppings | Topping | Toppings |
| Promotions | Promotion | Promotions |
| OrderRequests | OrderRequest | OrderRequests |
| OrderRequestItems | OrderRequestItem | OrderRequestItems |
| Notifications | Notification | Notifications |
| ReceiptSettings | ReceiptSettings | ReceiptSettings |
| SystemSettings | SystemSetting | SystemSettings |
| LoyaltyTransactions | LoyaltyTransaction | LoyaltyTransactions |
| BusinessInsights | BusinessInsight | BusinessInsights |
| InventoryItems | InventoryItem | InventoryItems |
| BranchInventories | BranchInventory | BranchInventories |
| StockReceipts | StockReceipt | StockReceipts |
| StockReceiptItems | StockReceiptItem | StockReceiptItems |
| StockIssues | StockIssue | StockIssues |
| StockIssueItems | StockIssueItem | StockIssueItems |
| StockTransactions | StockTransaction | StockTransactions |

Table names are verified from current ApplicationDbContextModelSnapshot. Inventory tables additionally use explicit ToTable configuration.

## 8. ERD Primary/Foreign Key Matrix

All current primary keys are Guid Id. Current key/constraint summary:

| Table | PK | FK | Nullable | Important behavior |
|---|---|---|---|---|
| Branches | Id | Ă¢â‚¬â€ | Ă¢â‚¬â€ | root |
| Employees | Id | BranchId Ă¢â€ â€™ Branches | yes | configured |
| Attendances | Id | EmployeeId, BranchId | no | configured |
| WorkSchedules | Id | EmployeeId; BranchId | Employee no; Branch yes | configured |
| Shifts | Id | EmployeeId, BranchId scalar | no | no fluent relationship |
| Tables | Id | BranchId Ă¢â€ â€™ Branches | yes | QrToken required/max 43/unique |
| Products | Id | Ă¢â‚¬â€ | Ă¢â‚¬â€ | Category string |
| Toppings | Id | Ă¢â‚¬â€ | Ă¢â‚¬â€ | no Product join |
| Customers | Id | Ă¢â‚¬â€ | Ă¢â‚¬â€ | unique PhoneNumber |
| Orders | Id | CustomerId Ă¢â€ â€™ Customers; BranchId scalar | Customer yes | index BranchId/CreatedAt/Status |
| OrderDetails | Id | shadow OrderId; ProductId?; ToppingId? | product/topping yes | Order cascade; product/topping optional |
| OrderRequests | Id | OrderId Ă¢â€ â€™ Orders | no | one order-many requests |
| OrderRequestItems | Id | OrderRequestId; ProductId scalar? | Product yes | request cascade |
| LoyaltyTransactions | Id | CustomerId; OrderId? | Order yes | filtered unique OrderId+Type |
| Reservations | Id | BranchId?, TableId? scalar | yes | no Customer FK |
| Expenses | Id | BranchId; StockReceiptId? | receipt yes | Branch Restrict; receipt unique one-to-one |
| BusinessInsights | Id | BranchId? scalar | yes | unique DeduplicationKey, indexes |
| Notifications | Id | BranchId?, TargetUserId? | yes | target index |
| ReceiptSettings | Id | BranchId? scalar | yes | unique BranchId |
| SystemSettings | Id | BranchId? scalar | yes | unique BranchId+Key |
| InventoryItems | Id | Ă¢â‚¬â€ | Ă¢â‚¬â€ | unique active NormalizedName |
| BranchInventories | Id | BranchId; InventoryItemId | no | unique pair, nonnegative checks, Restrict |
| StockReceipts | Id | BranchId | no | status/total/idempotency checks |
| StockReceiptItems | Id | StockReceiptId; InventoryItemId | no | quantity/price checks |
| StockIssues | Id | BranchId | no | status/idempotency checks |
| StockIssueItems | Id | StockIssueId; InventoryItemId | no | quantity positive |
| StockTransactions | Id | BranchId; InventoryItemId | no | type/reference/quantity checks |

## 9. Canonical ERD Relationship List

    Branches 1 -- * Employees
    Branches 1 -- * Attendances
    Employees 1 -- * Attendances
    Branches 1 -- * WorkSchedules
    Employees 1 -- * WorkSchedules
    Branches 1 -- * Expenses
    Branches 1 -- * Tables

    Customers 0..1 -- * Orders
    Orders 1 -- * OrderDetails
    Products 0..1 -- * OrderDetails
    Toppings 0..1 -- * OrderDetails
    Orders 1 -- * OrderRequests
    OrderRequests 1 -- * OrderRequestItems
    Customers 1 -- * LoyaltyTransactions
    Orders 0..1 -- * LoyaltyTransactions

    Branches 1 -- * BranchInventories
    InventoryItems 1 -- * BranchInventories
    Branches 1 -- * StockReceipts
    StockReceipts 1 -- * StockReceiptItems
    InventoryItems 1 -- * StockReceiptItems
    Branches 1 -- * StockIssues
    StockIssues 1 -- * StockIssueItems
    InventoryItems 1 -- * StockIssueItems
    Branches 1 -- * StockTransactions
    InventoryItems 1 -- * StockTransactions
    Expenses 0..1 -- 1 StockReceipts

The arrows above mean configured/current FK paths. Area-Table, Table-Order, Customer-Reservation, OrderDetail-OrderRequestItem, Product-Topping and Employee-Shift are not included because no current FK/navigation/configuration proves them.

## 10. Direct vs Indirect Branch-Scoped Entities

### DIRECT BRANCH-SCOPED ENTITIES

Employee (nullable), Attendance (required), WorkSchedule (nullable), RestaurantTable (nullable), Order (nullable), Reservation (nullable), Expense (required), Shift (required scalar), Notification (nullable), ReceiptSettings (nullable), SystemSetting (nullable), BusinessInsight (nullable), BranchInventory (required), StockReceipt (required), StockIssue (required), StockTransaction (required).

### INDIRECT BRANCH-SCOPED ENTITIES

| Entity | Route | Status |
|---|---|---|
| OrderDetail | OrderDetail Ă¢â€ â€™ shadow OrderId Ă¢â€ â€™ Order.BranchId | VERIFIED data path |
| OrderRequest | OrderRequest.OrderId Ă¢â€ â€™ Order.BranchId | VERIFIED data path |
| OrderRequestItem | OrderRequestItem.OrderRequestId Ă¢â€ â€™ OrderRequest Ă¢â€ â€™ Order | VERIFIED data path |
| StockReceiptItem | StockReceiptItem.StockReceiptId Ă¢â€ â€™ StockReceipt.BranchId | VERIFIED |
| StockIssueItem | StockIssueItem.StockIssueId Ă¢â€ â€™ StockIssue.BranchId | VERIFIED |
| LoyaltyTransaction | OrderId? Ă¢â€ â€™ Order branch or Customer context | PARTIALLY VERIFIED |
| InventoryItem | global catalog, branch relation through BranchInventory | VERIFIED global catalog |

Area, Product, Topping and Customer do not have direct BranchId. Their branch scope must not be invented.

## 11. Frontend Architecture Audit

| App | Directory | Framework | Purpose |
|---|---|---|---|
| Admin Web | apps/admin-web | React 18, TypeScript, Vite, Tailwind | Admin, management, POS, KDS |
| Customer Web | apps/customer-web | React 18, TypeScript, Vite, Tailwind | Registered customer, Guest QR, reservation |

Required conclusions:

1. admin-web contains POS: VERIFIED, features/pos/pages/POSPage.tsx.
2. admin-web contains KDS: VERIFIED, features/kitchen/pages/KitchenPage.tsx and KitchenHistoryPage.tsx.
3. POS is separate app: FALSE / MUST CHANGE.
4. KDS is separate app: FALSE / MUST CHANGE.
5. customer-web serves registered customers: VERIFIED.
6. customer-web serves Guest QR: VERIFIED, QRScan and DigitalMenu.

Draw one Admin Web boundary containing logical POS UI, KDS UI and management screens. Draw Customer Web as the second application boundary.

## 12. Backend Architecture Audit

Current logical components:

| Component | Evidence | Diagram position |
|---|---|---|
| WebAPI Controllers | WebAPI/Controllers/*.cs | HTTP boundary |
| AI Controller | AI/Controllers/AiController.cs | AI HTTP boundary |
| Application Services | Application/Services/*.cs | business/application layer |
| Domain | Domain/Entities/*.cs | current model layer |
| Infrastructure | ApplicationDbContext, migrations, DbInitializer | persistence layer |
| Authentication | JWT Bearer in Program.cs, JwtService | security middleware |
| Authorization | Authorize attributes, branch helpers, AI permission | security boundary |
| SignalR | KitchenHub, KitchenNotifier, AddSignalR | realtime component |
| AI | Orchestrator, registry, tools, AI provider implementation | AI logical module |

Classification: **VERIFIED as layered architecture inspired by Clean Architecture.** Folder/dependency intent is present, but Application Services directly reference ApplicationDbContext. Do not claim full dependency inversion/pure Clean Architecture without qualification. All layers are inside one API process, not separate deployable processes.

## 13. SignalR Architecture Audit

| Item | Exact value | Evidence |
|---|---|---|
| Hub class | KitchenHub | WebAPI/Hubs/KitchenHub.cs |
| Route | /kitchenHub | Program.cs MapHub |
| Auth roles | admin,manager,employee,cashier,kitchen | Authorize on KitchenHub |
| Event | NewOrderRequest | KitchenNotifier |
| Event | RequestStatusUpdated | KitchenNotifier |
| Event | PaymentCompleted | OrderController.PayOrder |

Exact groups:

    kitchen-admin
    role:{role}
    kitchen-branch:{branchId}
    branch:{branchId}
    branch:{branchId}:role:{role}

Event behavior:

| Event | Branch-specific | Role-specific | Global |
|---|---|---|---|
| NewOrderRequest | kitchen-branch:{branchId} | not direct role group | kitchen-admin |
| RequestStatusUpdated | branch-aware overload | not direct role group | kitchen-admin |
| PaymentCompleted | no | no | HubContext.Clients.All |

SignalR is realtime VERIFIED. Universal branch isolation is FALSE / MUST CHANGE because of global admin groups and Clients.All.

## 14. External Services & Deployment Audit

### PostgreSQL/Supabase

Program.cs reads ConnectionStrings:DefaultConnection or SUPABASE_CONNECTION_STRING, normalizes PostgreSQL URI/keyword format and configures UseNpgsql. PostgreSQL/Npgsql is VERIFIED. Supabase as a live deployment is PARTIALLY VERIFIED: configuration target exists, live deployment is not proven.

### AI provider

AI provider implementation uses HttpClient and configuration keys AI provider:ApiKey and AI provider:Model, with default model fallback MockAiProvider. external AI provider is outside the backend process boundary.

### Payment

Backend validates cash/transfer and updates persisted Order. POS constructs an img.vietqr.io image URL using branch bank fields. No payment gateway client, settlement callback or webhook controller was found.

Status: payment gateway/webhook NOT VERIFIED / FALSE if claimed. VietQR server integration FALSE; UI-level VietQR representation VERIFIED.

### Deployment

API Kestrel listens on HTTPS port 5000. Frontends use Vite packages and runtime API host/port configuration. No Dockerfile or docker-compose file was found. Current source proves Development/demo configuration, not production deployment or cloud application hosting.

## 15. Canonical System Architecture

    Actors
      Guest QR / Registered Customer
      Admin / Manager / Employee / Cashier / Kitchen
          Ă¢â€ â€œ
    Frontend
      Customer Web
      Admin Web
        Ă¢â€Å“Ă¢â€â‚¬ POS UI logical module
        Ă¢â€Å“Ă¢â€â‚¬ KDS UI logical module
        Ă¢â€â€Ă¢â€â‚¬ Management/Analytics/Inventory/Settings
          Ă¢â€ â€œ REST and SignalR
    ASP.NET Core 8 API process
      WebAPI Controllers
      JWT Authentication/Authorization
      Application Services
      Domain entities
      Infrastructure EF Core/Npgsql
      SignalR KitchenHub/KitchenNotifier
      AI Controller/Orchestrator/Registry/Tools
          Ă¢â€ â€œ
    PostgreSQL database (Supabase connection target)

    AI module -- external HTTP --> external AI provider

POS and KDS are logical Admin Web modules. SignalR is inside the API process. AI provider is outside the backend boundary. PostgreSQL/Supabase is outside the API process boundary. AI Tool Registry is inside the backend AI module.

## 16. AI Component Inventory

| Component | Exact class/file | Responsibility |
|---|---|---|
| AiController | AI/Controllers/AiController.cs | POST /api/Ai/chat, claims, guest block |
| AiOrchestrator | AI/Services/AiOrchestrator.cs | prompt/context, loop, tool calls, response |
| AiContextService | AI/Services/AiContextService.cs | DB-backed role context |
| PromptBuilder | AI/Services/PromptBuilder.cs | role prompt selection |
| AI provider implementation | AI/Services/AI provider implementation.cs | external AI provider HTTP call |
| AiToolRegistry | AI/Tools/AiToolRegistry.cs | role-filtered schemas/tool lookup |
| AiPermissionService | AI/Services/AiPermissionService.cs | role/risk/manager branch precondition |
| AiAuthorization | AI/Security/AiAuthorization.cs | role/risk/branch helper |
| AiToolValidator | AI/Utils/AiToolValidator.cs | required/type/range/branch argument checks |
| IAiTool | AI/Tools/IAiTool.cs | tool contract |
| ToolRiskLevel | AI/Security/ToolRiskLevel.cs | Read, Write, Destructive |

Current runtime tool classes are the 14 classes listed in section 18. No AI component is a separate deployment process.

## 17. Exact AI Runtime Pipeline

    AI UI
      Ă¢â€ â€™ POST /api/Ai/chat
    AiController.Chat
      Ă¢â€ â€™ extract JWT role/user/branch/customer claims
      Ă¢â€ â€™ Forbid guest customer session
    AiOrchestrator.ProcessAsync
      Ă¢â€ â€™ AiContextService.GetContextAsync
      Ă¢â€ â€™ PromptBuilder.BuildSystemInstruction
      Ă¢â€ â€™ AiToolRegistry.GetAI providerToolsDefinition(role)
      Ă¢â€ â€™ AI provider implementation.GenerateContentAsync
      Ă¢â€ â€™ external AI provider
      Ă¢â€ â€™ text OR functionCall
      Ă¢â€ â€™ AiToolRegistry.GetTool(name)
      Ă¢â€ â€™ AiPermissionService.CanExecuteAsync
      Ă¢â€ â€™ IAiTool.ExecuteAsync
      Ă¢â€ â€™ Application Service
      Ă¢â€ â€™ ApplicationDbContext/PostgreSQL
      Ă¢â€ â€™ AiToolResult
      Ă¢â€ â€™ next AI provider iteration, max 5
      Ă¢â€ â€™ final AiResponse

AiOrchestrator has no direct DbContext field. AiContextService reads DbContext for context. Current tools use Application Service interfaces, not direct DbContext.

## 18. Complete 14 AI Tool Matrix

Registration is in Program.cs; runtime name comes from each class Name property.

| # | Runtime name | Class | R/W | Allowed roles | Risk | Scope | Application Service |
|---:|---|---|---|---|---|---|---|
| 1 | get_active_staff | GetActiveStaffTool | Read | admin, manager | Read | user BranchId | IEmployeeService.GetActiveStaffAsync |
| 2 | get_best_sellers | GetBestSellersTool | Read | admin, manager, employee, cashier, kitchen | Read | branch context | IDashboardService.GetBestSellersAsync |
| 3 | get_business_summary | GetBusinessSummaryTool | Read | admin, manager | Read | branch context | IDashboardService.GetBusinessSummaryAsync |
| 4 | get_financial_analysis | GetFinancialAnalysisTool | Read | admin, manager | Read | branch context | IFinancialAnalysisService.GetFinancialAnalysisAsync |
| 5 | get_order_list | GetOrderListTool | Read | admin, manager, employee, cashier, kitchen | Read | OrderQueryFilter.BranchId | IOrderService.GetOrdersAsync |
| 6 | get_revenue_comparison | GetRevenueComparisonTool | Read | admin, manager | Read | branch context | IDashboardService.GetRevenueReportAsync |
| 7 | get_revenue | GetRevenueTool | Read | admin, manager | Read | branch context | IDashboardService.GetRevenueByPeriodAsync |
| 8 | update_product_price | UpdateProductPriceTool | Write | admin | Write | product/price validation | IProductService.UpdatePriceByNameAsync |
| 9 | employee_get_table_summary | GetTableSummaryTool | Read | admin, manager, employee, cashier, kitchen | Read | admin may omit; others BranchId | ITableService.GetTableStatusCountsAsync |
| 10 | get_my_shift | GetMyShiftTool | Read | employee, cashier, kitchen | Read | own UserId/date | IEmployeeService.GetShiftsByEmployeeIdAsync |
| 11 | update_order_status | UpdateOrderStatusTool | Write | admin, manager, employee, cashier, kitchen | Write | BranchId + role passed to service | IOrderService.UpdateOrderStatusByCodeAsync |
| 12 | customer_get_menu | GetMenuTool | Read | admin, manager, employee, cashier, kitchen, customer | Read | category/menu query | IProductService.GetAllProductsAsync |
| 13 | get_my_orders | GetMyOrderTool | Read | customer | Read | customer phone/email/identity | IOrderService.GetOrdersByCustomerAsync |
| 14 | create_booking | CreateBookingTool | Write | customer | Write | customer context and reservation branch | IReservationService.CreateBookingFromAiAsync |

No runtime get_menu or singular get_my_order name exists. No registered Destructive tool was found.

## 19. AI Role/Risk/Scope Matrix

| Question | Status | Evidence |
|---|---|---|
| Definitions filtered by role before AI provider? | VERIFIED | AiToolRegistry.GetAI providerToolsDefinition |
| Permission checked after function call? | VERIFIED | AiOrchestrator calls CanExecuteAsync before ExecuteAsync |
| Risk representation? | VERIFIED | IAiTool.RiskLevel and ToolRiskLevel |
| Manager missing branch blocked? | VERIFIED | AiPermissionService |
| Actual branch data filtering? | PARTIALLY VERIFIED | tool/service-specific BranchId paths |
| Guest blocked? | VERIFIED | AiController returns Forbid for customerSessionType guest |
| Admin branch bypass? | VERIFIED/PARTIAL | AiAuthorization treats admin as global; tool/service behavior remains endpoint-specific |
| Customer write allowed? | VERIFIED | create_booking is Write and customer-allowed |
| Product price update role? | VERIFIED | update_product_price AllowedRoles admin only |

Tool definitions are role-filtered before AI provider. Permission is rechecked after function call. Risk is Read/Write/Destructive. Current tools call Application Services; manager branch precondition is centralized but operation-level branch/ownership checks remain in the service/tool.

## 20. Sequential Tool Calling Audit

Maximum AI tool loop = **5**. Evidence: AiOrchestrator.ProcessAsync defines MAX_ITERATIONS = 5 and loops while iterations < MAX_ITERATIONS.

| Behavior | Current implementation | Status |
|---|---|---|
| Sequential/parallel | sequential; no parallel execution | VERIFIED |
| Multiple function calls | enumerate each functionCall part and collect responses | VERIFIED |
| Text-only response | no function call returns first text part | VERIFIED |
| Unknown tool | error AiToolResult | VERIFIED |
| Denied permission | error AiToolResult | VERIFIED |
| Invalid arguments | validator/tool parsing error path | VERIFIED/PARTIAL by tool |
| Empty AI provider candidates | unsuccessful AiResponse | VERIFIED |
| AI provider HTTP error | sanitized exception/error response | VERIFIED |
| Max loop | unsuccessful max-tool error | VERIFIED |

Use a UML loop guard iterations < 5. Do not use a parallel fragment.

## 21. Business Insight Audit

BusinessInsight is a persisted current entity/DbSet. It contains Id, nullable BranchId, Type, Severity, Title, Summary, AiExplanation, AiRecommendation, EvidenceJson, period fields, Status, DeduplicationKey and timestamps. OnModelCreating adds unique DeduplicationKey and indexes on BranchId, CreatedAt and Status.

ProactiveInsightService and InsightBackgroundService generate/store insights; BusinessInsightController and Admin Web BusinessInsights page expose read/resolve actions. This is a current stored feature.

DashboardService computes revenue, comparison, business summary and best sellers. FinancialAnalysisService computes financial analysis. AI tools expose selected computations to AI provider. These computed responses are not the same object as persisted BusinessInsight.

BusinessInsight.BranchId is direct and nullable. Status: VERIFIED.

## 22. Canonical AI Architecture

    User
      Ă¢â€ â€™ AI UI
      Ă¢â€ â€™ AiController
      Ă¢â€ â€™ AiUserContext
      Ă¢â€ â€™ AiOrchestrator
        Ă¢â€Å“Ă¢â€ â€™ AiContextService Ă¢â€ â€™ ApplicationDbContext (context read)
        Ă¢â€Å“Ă¢â€ â€™ PromptBuilder
        Ă¢â€Å“Ă¢â€ â€™ AiToolRegistry (role-filtered schema)
        Ă¢â€â€Ă¢â€ â€™ AI provider implementation -- external HTTP --> external AI provider
                                  Ă¢â€ â€œ functionCall
                           AiToolRegistry.GetTool
                                  Ă¢â€ â€œ
                           AiPermissionService/AiAuthorization
                                  Ă¢â€ â€œ
                           IAiTool.ExecuteAsync
                                  Ă¢â€ â€œ
                           Application Service
                                  Ă¢â€ â€œ
                           EF Core/ApplicationDbContext
                                  Ă¢â€ â€œ
                           AiToolResult Ă¢â€ â€™ AI provider next iteration
                                  Ă¢â€ â€œ loop guard < 5
                           Final response Ă¢â€ â€™ UI

Components to draw: AI UI, AiController, AiOrchestrator, context/prompt services, registry, permission, tool abstraction/implementations, AI provider implementation, external AI provider, Application Services, EF Core/DbContext and PostgreSQL.

Components not to draw: direct AiOrchestratorĂ¢â€ â€™DbContext, parallel tool executor, Invoice/removed staff-finance module/Restaurant components, payment gateway/webhook, separate POS/KDS processes.

Optional: collapse individual prompt files and 14 tools into grouped components for high-level architecture; keep exact matrix separately.

## 23. Proposed Domain Class Diagram Verification

| Proposed item | Status | Correction |
|---|---|---|
| Branch, Employee, Attendance, WorkSchedule | VERIFIED | current entities |
| Shift | VERIFIED/PARTIAL | scalar EmployeeId/BranchId; no fluent relation |
| Area | VERIFIED entity | no Table FK; Table.AreaName |
| Table | RENAME | exact class RestaurantTable |
| Category | FALSE / MUST CHANGE | Product.Category string |
| Product, Topping, Promotion | VERIFIED | current entities; no ProductTopping |
| ProductTopping | FALSE / MUST CHANGE | absent |
| Customer, LoyaltyTransaction, Reservation | VERIFIED/PARTIAL | Reservation has no CustomerId |
| Order, OrderDetail | VERIFIED | exact current names |
| OrderDetailTopping | FALSE / MUST CHANGE | direct OrderDetail.ToppingId only |
| OrderRequest, OrderRequestItem | VERIFIED | KDS current entities |
| Expense, BusinessInsight | VERIFIED | current persisted entities |
| Inventory entities | VERIFIED | exact seven entities in section 6 |

## 24. Proposed ERD Verification

| Proposed relation | Status | Correct source interpretation |
|---|---|---|
| Branch 1-* Employee | VERIFIED/PARTIAL | configured; BranchId nullable |
| Branch 1-* Area | FALSE / MUST CHANGE | no Area.BranchId |
| Area 1-* Table | FALSE / MUST CHANGE | AreaName string, no AreaId |
| Branch 1-* Table | VERIFIED/PARTIAL | configured; BranchId nullable |
| Category 1-* Product | FALSE / MUST CHANGE | no Category entity |
| Product *-* Topping join | FALSE / MUST CHANGE | no join entity |
| Branch 1-* Order | PARTIALLY VERIFIED | BranchId scalar/nullable, no fluent relation |
| Table 1-* Order | FALSE / MUST CHANGE | Order.TableName, no TableId |
| Customer 0..1-* Order | VERIFIED | Order.CustomerId optional FK |
| Order 1-* OrderDetail | VERIFIED | shadow OrderId required, Cascade |
| Product/Topping to OrderDetail | VERIFIED/PARTIAL | optional direct FKs |
| Order 1-* OrderRequest | VERIFIED | OrderRequest.OrderId required |
| OrderRequest 1-* OrderRequestItem | VERIFIED | required FK, Cascade |
| OrderDetail 1-* OrderRequestItem | FALSE / MUST CHANGE | no OrderDetailId |
| Customer 1-* LoyaltyTransaction | VERIFIED | CustomerId required |
| Order ?-* LoyaltyTransaction | VERIFIED | nullable OrderId, unique by type |
| Customer ?-* Reservation | FALSE / MUST CHANGE | no CustomerId |
| Branch 1-* Reservation | PARTIALLY VERIFIED | nullable scalar only |
| Employee 1-* Attendance/WorkSchedule | VERIFIED | configured FKs |
| Employee/Branch 1-* Shift | PARTIALLY VERIFIED | scalar fields, no fluent relation |
| Branch 1-* Expense | VERIFIED | Restrict FK |
| Inventory relations | VERIFIED | exact seven-entity model |
| BusinessInsight relation | PARTIALLY VERIFIED | direct optional BranchId/index, no nav/FK config |

## 25. Proposed System Architecture Verification

The proposal is VERIFIED with these corrections:

    Guest QR/Customer Ă¢â€ â€™ Customer Web
    Staff/Admin Ă¢â€ â€™ Admin Web
    Kitchen Ă¢â€ â€™ KDS logical UI in Admin Web
    Customer Web/Admin Web Ă¢â€ â€™ ASP.NET Core API
    API Ă¢â€ â€™ Controllers/Auth/Application Services/Domain/Infrastructure/AI
    Application Services Ă¢â€ â€™ EF Core/Npgsql Ă¢â€ â€™ PostgreSQL
    Application Services/KitchenNotifier Ă¢â€ â€™ /kitchenHub
    AI module Ă¢â€ â€™ external AI provider

Put AI provider outside the API process. Put PostgreSQL/Supabase outside the API process. Put AI Tool Registry inside the API. Do not draw POS or KDS as separate applications. Do not add Docker or production deployment without evidence. Show global Admin SignalR behavior as a limitation.

## 26. Proposed AI Architecture Verification

The proposal is VERIFIED with exact corrections:

1. UI calls POST /api/Ai/chat.
2. AiController reads JWT claims and blocks guest before orchestrator.
3. Orchestrator calls context, prompt builder, registry and AI provider implementation.
4. Function call returns to orchestrator, then registry lookup and permission check.
5. Tool calls Application Service, then database.
6. Tool result returns to AI provider for another sequential iteration.
7. Maximum iterations is 5.
8. Unknown tool, denied permission, invalid argument, AI provider error and max-loop are error branches.
9. No direct AiOrchestratorĂ¢â€ â€™DbContext and no parallel fragment.
10. Exact customer names are customer_get_menu and get_my_orders.

## 27. PlantUML Correction Checklist

### Domain Class

- [ ] Use RestaurantTable, not invented Table entity.
- [ ] Use OrderDetail, not OrderItem.
- [ ] Remove Restaurant, RestaurantId, Category entity, ProductTopping, OrderDetailTopping, Invoice and removed staff-finance module.
- [ ] Include OrderRequest/OrderRequestItem for KDS.
- [ ] Use the seven exact inventory entities.
- [ ] Do not draw absent navigations as formal associations.

### ERD

- [ ] Use all 29 current DbSets/table names.
- [ ] Mark nullable BranchId, CustomerId and OrderId fields.
- [ ] Draw OrderĂ¢â€ â€™OrderDetail through shadow OrderId with Cascade.
- [ ] Draw OrderĂ¢â€ â€™OrderRequest and OrderRequestĂ¢â€ â€™OrderRequestItem.
- [ ] Remove OrderRequestItemĂ¢â€ â€™OrderDetail.
- [ ] Remove Product/Topping join tables.
- [ ] Remove CustomerĂ¢â€ â€™Reservation and AreaĂ¢â€ â€™Table formal FK edges.
- [ ] Include inventory unique/check constraints in notes.

### System Architecture

- [ ] Exactly two frontend app boundaries.
- [ ] POS/KDS as logical Admin Web modules.
- [ ] One ASP.NET Core API process with logical layers.
- [ ] SignalR /kitchenHub inside API.
- [ ] PostgreSQL/Supabase database boundary.
- [ ] AI provider external service boundary.
- [ ] No Docker/production/payment gateway component without evidence.

### AI Architecture

- [ ] Draw ControllerĂ¢â€ â€™OrchestratorĂ¢â€ â€™Context/Prompt/Registry/AI provider.
- [ ] Draw role-filtered schemas before AI provider.
- [ ] Draw permission re-check after Function Call.
- [ ] Draw ToolĂ¢â€ â€™Application ServiceĂ¢â€ â€™EF Core/DB.
- [ ] Add loop iterations < 5.
- [ ] Use sequential, not parallel.
- [ ] Add guest-forbidden path.
- [ ] Use exact 14 runtime tool names.

## 28. Final Diagram Verdict

### 1. Domain Class Diagram classes?

**VERIFIED.** Core classes are Branch, Employee, Customer, RestaurantTable, Product, Topping, Order, OrderDetail, OrderRequest, OrderRequestItem and LoyaltyTransaction. Complete current model adds Attendance, WorkSchedule, Shift, Reservation, Expense, BusinessInsight and seven inventory entities.

Evidence: current Domain/Entities and 29 DbSets.

### 2. Classes to remove?

**FALSE / MUST CHANGE.** Restaurant, Category entity, ProductTopping, OrderDetailTopping, Invoice, removed staff-finance module, OrderItem and invented inventory classes.

Evidence: no current class/DbSet.

### 3. Classes to rename?

**VERIFIED.** Table proposal Ă¢â€ â€™ RestaurantTable; OrderItem proposal Ă¢â€ â€™ OrderDetail.

### 4. Exact inventory entities?

**VERIFIED.** InventoryItem, BranchInventory, StockReceipt, StockReceiptItem, StockIssue, StockIssueItem, StockTransaction.

### 5. Exact inventory relations?

**VERIFIED.** Branch to BranchInventory/StockReceipt/StockIssue/StockTransaction; InventoryItem to BranchInventory/StockReceiptItem/StockIssueItem/StockTransaction; StockReceipt to StockReceiptItem; StockIssue to StockIssueItem; optional Expense to StockReceipt one-to-one.

### 6. Restaurant entity?

**FALSE / MUST CHANGE.** Absent; Branch is root.

### 7. removed staff-finance module entity?

**FALSE / MUST CHANGE.** Absent from current Domain/DbContext; historical backup does not count.

### 8. Invoice entity?

**FALSE / MUST CHANGE.** Absent; InvoiceController/InvoiceCode do not prove entity.

### 9. OrderItem or OrderDetail?

**VERIFIED.** OrderDetail.

### 10. Exact KDS entities?

**VERIFIED.** OrderRequest and OrderRequestItem, related to Order/OrderDetail.

### 11. KDS status location?

**VERIFIED.** OrderRequest.Status. OrderDetail.SentQuantity is not item status.

### 12. OrderĂ¢â€ â€™OrderDetail cardinality?

**VERIFIED.** 1-to-many; required shadow OrderId; Cascade.

### 13. OrderĂ¢â€ â€™OrderRequest cardinality?

**VERIFIED.** 1-to-many; required OrderRequest.OrderId; supports multiple send batches.

### 14. OrderRequestItemĂ¢â€ â€™OrderDetail?

**FALSE / MUST CHANGE.** No FK/OrderDetailId; snapshot ProductId/ProductName/Quantity.

### 15. CustomerĂ¢â€ â€™Order optionality?

**VERIFIED.** Order.CustomerId nullable; one Customer can be referenced by many Orders.

### 16. CustomerĂ¢â€ â€™Reservation optionality?

**FALSE / MUST CHANGE.** No CustomerId or configured relationship; name/phone snapshot only.

### 17. LoyaltyTransactionĂ¢â€ â€™Order?

**VERIFIED.** nullable OrderId; filtered unique OrderId+Type for non-null OrderId; CustomerId required.

### 18. BusinessInsight persisted?

**VERIFIED.** DbSet and current entity; service/controller/background usage.

### 19. BusinessInsight direct BranchId?

**VERIFIED.** Guid? BranchId exists directly; indexed, no explicit navigation.

### 20. POS separate or Admin logical UI?

**VERIFIED.** Logical UI in apps/admin-web/features/pos.

### 21. KDS separate or Admin logical UI?

**VERIFIED.** Logical UI in apps/admin-web/features/kitchen.

### 22. Number of frontend apps?

**VERIFIED.** Two: admin-web and customer-web.

### 23. Exact SignalR route?

**VERIFIED.** /kitchenHub.

### 24. Exact SignalR events?

**VERIFIED.** NewOrderRequest, RequestStatusUpdated, PaymentCompleted.

### 25. Global event/group?

**VERIFIED.** kitchen-admin is global Admin group; NewOrderRequest and RequestStatusUpdated include it; PaymentCompleted uses Clients.All.

### 26. Payment gateway/webhook?

**NOT VERIFIED / FALSE if claimed.** No current server-side gateway/webhook path.

### 27. VietQR UI or server integration?

**VERIFIED UI-only.** img.vietqr.io representation, not settlement integration.

### 28. Exact AI components?

**VERIFIED.** AiController, AiOrchestrator, AiContextService, PromptBuilder, AI provider implementation, AiToolRegistry, AiPermissionService, AiAuthorization, AiToolValidator, IAiTool, 14 tools, AiUserContext and AiToolResult.

### 29. Exact 14 runtime tool names?

**VERIFIED.** get_active_staff, get_best_sellers, get_business_summary, get_financial_analysis, get_order_list, get_revenue_comparison, get_revenue, update_product_price, employee_get_table_summary, get_my_shift, update_order_status, customer_get_menu, get_my_orders, create_booking.

### 30. Exact allowed roles?

**VERIFIED.** Full evidence matrix is section 18 and comes from each AllowedRoles property.

### 31. Exact risks?

**VERIFIED.** Read for all except update_product_price, update_order_status and create_booking, which are Write; no Destructive registration.

### 32. Guest AI blocked where?

**VERIFIED.** AiController.Chat returns Forbid for customerSessionType guest before orchestration.

### 33. Definitions filtered before AI provider?

**VERIFIED.** AiToolRegistry filters AllowedRoles by current role.

### 34. Permission recheck after Function Call?

**VERIFIED.** AiOrchestrator calls CanExecuteAsync before ExecuteAsync.

### 35. Maximum loop?

**VERIFIED.** 5.

### 36. Sequential or parallel?

**VERIFIED.** Sequential; function-call parts enumerated in order.

### 37. AI provider direct DB?

**VERIFIED no.** AI provider is external HTTP through AI provider implementation.

### 38. AI Tools through Application Service?

**VERIFIED for current 14 tools.** Tool constructors use Application Service interfaces; no direct DbContext injection in current tools.

### 39. System Architecture changes?

**VERIFIED correction.** Two frontend apps, logical POS/KDS in admin-web, one API process/layers, SignalR inside API, PostgreSQL and AI provider external boundaries, no Docker/production/payment gateway claims.

### 40. AI Architecture changes?

**VERIFIED correction.** Exact ControllerĂ¢â€ â€™OrchestratorĂ¢â€ â€™Context/Prompt/RegistryĂ¢â€ â€™AI providerĂ¢â€ â€™Function CallĂ¢â€ â€™PermissionĂ¢â€ â€™ToolĂ¢â€ â€™Application ServiceĂ¢â€ â€™DBĂ¢â€ â€™Tool ResultĂ¢â€ â€™AI provider chain, guest block and max-5 sequential loop.

### 41. Domain Class PlantUML ready?

**PARTIALLY VERIFIED / READY WITH CORRECTIONS.** Inventory/fields/relations are identified, but absent entities must be removed and scalar-vs-FK distinctions preserved.

### 42. ERD PlantUML ready?

**PARTIALLY VERIFIED / READY WITH CORRECTIONS.** Current 29-table model is identified; absent joins/FKs and nullable fields must be represented accurately.

### 43. Architecture PlantUML ready?

**VERIFIED / READY WITH CORRECTIONS.** Component boundaries are source-grounded; omit production/Docker/gateway claims.

### 44. AI Architecture PlantUML ready?

**VERIFIED / READY WITH CORRECTIONS.** Exact pipeline, 14 tools, role/risk, guest block and loop are source-grounded.

## Audit completion record

- Audit type: READ-ONLY.
- Source code modified: NO.
- Thesis modified: NO.
- Database/migration/config/test/frontend modified: NO.
- PlantUML created: NO.
- Sole intentional output: baocaokhoaluan/THESIS-DIAGRAM-SOURCE-AUDIT.md.
- Existing working-tree changes were preserved.

## DIAGRAM SOURCE AUDIT COMPLETE Ă¢â‚¬â€ READY FOR CONTROLLED PLANTUML DESIGN

---

# AUDIT EVIDENCE COMPLETION ADDENDUM

This addendum completes the evidence gaps without changing the prior source-of-truth conclusions.

## A. Exact 29-Entity Field, FK and Navigation Completion

The existing field matrix is expanded here with exact FK ownership and navigation evidence. All names below are current C# property names.

| Entity | PK | FK fields | Navigation properties | Important exact fields |
|---|---|---|---|---|
| Branch | Id: Guid | none | none declared | Name:string, IsMain:bool, IsActive:bool |
| Employee | Id: Guid | BranchId: Guid? | none declared | EmployeeCode:string?, FullName:string, BasicSalary:decimal, Role:string, IsActive:bool |
| Customer | Id: Guid | none | none declared | FullName:string?, PhoneNumber:string?, TotalSpending:decimal, TotalOrders:int, LoyaltyPoints:int, IsActive:bool |
| RestaurantTable | Id: Guid | BranchId: Guid? | none declared | Name:string, AreaName:string, Status:string, QrToken:string, IsActive:bool |
| Area | Id: Guid | none | none declared | Name:string, DisplayOrder:int |
| Product | Id: Guid | none | none declared | Name:string?, Category:string?, ToppingsJson:string?, Price:decimal, CostPrice:decimal, IsActive:bool, AvailabilityStatus:string |
| Topping | Id: Guid | none | none declared | Name:string, Price:decimal, CostPrice:decimal, ToppingType:string?, Category:string?, IsActive:bool |
| Promotion | Id: Guid | none | none declared | Name:string, RequiredPoints:int, DiscountValue:decimal, PromotionType:string, IsActive:bool |
| Order | Id: Guid | CustomerId: Guid?, BranchId: Guid? | Details:List<OrderDetail> | InvoiceCode:string?, SubTotal:decimal?, VatAmount:decimal?, VatPercent:decimal?, ServiceFeeAmount:decimal?, ServiceFeePercent:decimal?, TotalAmount:decimal, Discount:decimal, PaidAmount:decimal, PaymentMethod:string?, Status:string, TableName:string? |
| OrderDetail | Id: Guid | ProductId: Guid?, ToppingId: Guid?, shadow OrderId: Guid | none declared | ProductName:string, Quantity:int, SentQuantity:int, UnitPrice:decimal, Options:string? |
| OrderRequest | Id: Guid | OrderId: Guid | Items:List<OrderRequestItem> | RequestNumber:int, Status:string, CreatedAt:DateTime, AcceptedAt:DateTime?, PreparingAt:DateTime?, CompletedAt:DateTime? |
| OrderRequestItem | Id: Guid | OrderRequestId: Guid, ProductId: Guid? scalar | none declared | ProductName:string, Quantity:int, Note:string?, Options:string?; no OrderDetailId |
| LoyaltyTransaction | Id: Guid | CustomerId: Guid, OrderId: Guid? | none declared | Type:string, Points:int, BalanceAfter:int, Description:string? |
| Reservation | Id: Guid | BranchId: Guid?, TableId: Guid? scalar | none declared | CustomerName:string?, CustomerPhone:string?, ReservationTime:DateTime, NumberOfGuests:int, Status:string |
| Attendance | Id: Guid | EmployeeId: Guid, BranchId: Guid | none declared | CheckInTime:DateTime, CheckOutTime:DateTime?, Status:string? |
| WorkSchedule | Id: Guid | EmployeeId: Guid, BranchId: Guid? | none declared | Date:DateTime, ShiftName:string?, StartTime:string?, EndTime:string? |
| Shift | Id: Guid | EmployeeId: Guid, BranchId: Guid | none declared | StartTime:DateTime, EndTime:DateTime?, StartingCash:decimal, EndingCash:decimal?, Status:string |
| Expense | Id: Guid | BranchId: Guid, StockReceiptId: Guid? | StockReceipt:StockReceipt? | Description:string, Amount:decimal, ExpenseDate:DateTime, Category:string, PaymentMethod:string |
| BusinessInsight | Id: Guid | BranchId: Guid? scalar | none declared | Type:string, Severity:string, Title:string, Summary:string, EvidenceJson:string, Status:string, DeduplicationKey:string |
| InventoryItem | Id: Guid | none | BranchInventories:ICollection<BranchInventory> | Name:string, NormalizedName:string, UnitCode:string, IsActive:bool |
| BranchInventory | Id: Guid | BranchId: Guid, InventoryItemId: Guid | Branch:Branch?, InventoryItem:InventoryItem? | CurrentQuantity:decimal, MinimumStock:decimal |
| StockReceipt | Id: Guid | BranchId: Guid | Branch:Branch?, Items:ICollection<StockReceiptItem> | SupplierName:string?, PurchaseDate:DateTime?, TotalAmount:decimal, Status:StockDocumentStatus, CreatedBy:Guid, IdempotencyKey:string? |
| StockReceiptItem | Id: Guid | StockReceiptId: Guid, InventoryItemId: Guid | StockReceipt:StockReceipt?, InventoryItem:InventoryItem? | Quantity:decimal, UnitPrice:decimal |
| StockIssue | Id: Guid | BranchId: Guid | Branch:Branch?, Items:ICollection<StockIssueItem> | Reason:string, Status:StockDocumentStatus, CreatedBy:Guid, IdempotencyKey:string? |
| StockIssueItem | Id: Guid | StockIssueId: Guid, InventoryItemId: Guid | StockIssue:StockIssue?, InventoryItem:InventoryItem? | Quantity:decimal |
| StockTransaction | Id: Guid | BranchId: Guid, InventoryItemId: Guid | Branch:Branch?, InventoryItem:InventoryItem? | Type:StockTransactionType, Quantity:decimal, BeforeQuantity:decimal, AfterQuantity:decimal, ReferenceType:string, ReferenceId:Guid |
| ReceiptSettings | Id: Guid | BranchId: Guid? scalar | none declared | PaperWidth:int, ShowLogo:bool, ShowAddress:bool, ShowPhone:bool, FontFamily:string, FontSize:int |
| SystemSetting | Id: Guid | BranchId: Guid? scalar | none declared | Key:string, Value:string, Type:string, UpdatedAt:DateTime |
| Notification | Id: Guid | BranchId: Guid? scalar; EntityId/TargetUserId are not configured FKs | none declared | Type:string, Title:string, Message:string, TargetRole:string?, IsRead:bool |

Product.Category exact type is string?. Product.ToppingsJson exact type is string?. Product has no BranchId, Branch navigation or EF Branch relation. Product.AvailabilityStatus exact type is string.

OrderDetail.ToppingId exact type is Guid?. OrderDetail.SentQuantity exact type is int. OrderDetail.ProductId exact type is Guid?. There is no OrderDetail.OrderId CLR property; the OrderId relation is a shadow FK configured in ApplicationDbContext.

OrderRequestItem has exactly OrderRequestId: Guid and ProductId: Guid? as possible relation-like fields. It has no OrderDetailId, no ToppingId and no navigation property. ProductId is not explicitly configured with HasOne<Product>() in current OnModelCreating.

Evidence: Domain/Entities/*.cs and ApplicationDbContext.cs; current model snapshot confirms table names and the shadow OrderId relation.

## B. Exact Relationship Matrix Completion

| Entity A | A cardinality | Entity B | B cardinality | FK owner/property | Required? | Navigation/EF evidence |
|---|---:|---|---:|---|---|---|
| Branch | 1 | Employee | 0..* | Employee.BranchId | no | HasOne Branch, WithMany |
| Branch | 1 | RestaurantTable | 0..* | RestaurantTable.BranchId | no | HasOne Branch, WithMany |
| Area | none | RestaurantTable | none | none | N/A | FALSE - DO NOT DRAW; Table.AreaName only |
| Branch | none | Area | none | none | N/A | FALSE - DO NOT DRAW; no BranchId |
| Branch | none | Product | none | none | N/A | FALSE - DO NOT DRAW; no BranchId |
| Product | none | Topping | none | none | N/A | FALSE - DO NOT DRAW; ToppingsJson is string |
| Branch | scalar scope only | Order | scalar scope only | Order.BranchId | nullable | no configured EF Branch relation |
| RestaurantTable | none | Order | none | no TableId | N/A | FALSE - DO NOT DRAW; Order.TableName only |
| Customer | 0..1 | Order | 0..* | Order.CustomerId | no | HasOne Customer, WithMany |
| Order | 1 | OrderDetail | 0..* | shadow OrderDetail.OrderId | yes | Order.Details, Cascade |
| Product | 1 | OrderDetail | 0..* | OrderDetail.ProductId | no | HasOne Product, optional |
| Topping | 1 | OrderDetail | 0..* | OrderDetail.ToppingId | no | HasOne Topping, optional |
| Order | 1 | OrderRequest | 0..* | OrderRequest.OrderId | yes | HasOne Order, WithMany |
| OrderRequest | 1 | OrderRequestItem | 0..* | OrderRequestItem.OrderRequestId | yes | OrderRequest.Items, Cascade |
| OrderDetail | none | OrderRequestItem | none | no OrderDetailId | N/A | FALSE - DO NOT DRAW |
| Customer | 1 | LoyaltyTransaction | 0..* | LoyaltyTransaction.CustomerId | yes | HasOne Customer, WithMany |
| Order | 0..1 | LoyaltyTransaction | 0..* | LoyaltyTransaction.OrderId | no | optional FK; filtered unique OrderId+Type |
| Customer | none | Reservation | none | no CustomerId | N/A | FALSE - DO NOT DRAW |
| Branch | scalar scope only | Reservation | scalar scope only | Reservation.BranchId | nullable | no configured FK |
| Employee | 1 | Attendance | 0..* | Attendance.EmployeeId | yes | HasOne Employee |
| Branch | 1 | Attendance | 0..* | Attendance.BranchId | yes | HasOne Branch |
| Employee | 1 | WorkSchedule | 0..* | WorkSchedule.EmployeeId | yes | HasOne Employee |
| Branch | 1 | WorkSchedule | 0..* | WorkSchedule.BranchId | no | HasOne Branch |
| Employee | scalar only | Shift | scalar only | Shift.EmployeeId | yes scalar | no EF relation; FALSE as formal association |
| Branch | scalar only | Shift | scalar only | Shift.BranchId | yes scalar | no EF relation; FALSE as formal association |
| Branch | 1 | Expense | 0..* | Expense.BranchId | yes | Restrict |
| Branch | scalar scope only | BusinessInsight | scalar scope only | BusinessInsight.BranchId | nullable | index only, no navigation |
| Branch | scalar scope only | ReceiptSettings | scalar scope only | ReceiptSettings.BranchId | nullable | unique index only |
| Branch | scalar scope only | SystemSetting | scalar scope only | SystemSetting.BranchId | nullable | unique BranchId+Key index only |
| Branch | scalar scope only | Notification | scalar scope only | Notification.BranchId | nullable | target index only |
| Branch | 1 | BranchInventory | 0..* | BranchInventory.BranchId | yes | ConfigureInventory, Restrict |
| InventoryItem | 1 | BranchInventory | 0..* | BranchInventory.InventoryItemId | yes | unique pair, Restrict |
| Branch | 1 | StockReceipt | 0..* | StockReceipt.BranchId | yes | ConfigureInventory, Restrict |
| StockReceipt | 1 | StockReceiptItem | 0..* | StockReceiptItem.StockReceiptId | yes | collection, Restrict |
| InventoryItem | 1 | StockReceiptItem | 0..* | StockReceiptItem.InventoryItemId | yes | Restrict |
| Branch | 1 | StockIssue | 0..* | StockIssue.BranchId | yes | ConfigureInventory, Restrict |
| StockIssue | 1 | StockIssueItem | 0..* | StockIssueItem.StockIssueId | yes | collection, Restrict |
| InventoryItem | 1 | StockIssueItem | 0..* | StockIssueItem.InventoryItemId | yes | Restrict |
| Branch | 1 | StockTransaction | 0..* | StockTransaction.BranchId | yes | ConfigureInventory, Restrict |
| InventoryItem | 1 | StockTransaction | 0..* | StockTransaction.InventoryItemId | yes | ConfigureInventory, Restrict |
| Expense | 0..1 | StockReceipt | 0..1 | Expense.StockReceiptId | no | one-to-one, unique filtered, Restrict |

## C. Inventory Deep Audit Completion

### InventoryItem

Exact fields: Id, Name, NormalizedName, UnitCode, IsActive, CreatedAtUtc, UpdatedAtUtc?, BranchInventories. PK is Id. No direct FK. Navigation is BranchInventories. NormalizedName has a unique filtered index for active items. One InventoryItem can have many BranchInventory, StockReceiptItem, StockIssueItem and StockTransaction rows.

### BranchInventory

Exact fields: Id, BranchId, InventoryItemId, CurrentQuantity, MinimumStock, UpdatedAtUtc, Branch, InventoryItem. BranchId and InventoryItemId are required Guid FKs. CurrentQuantity and MinimumStock are decimal and have nonnegative checks. BranchId + InventoryItemId is unique. It is an association/quantity entity between Branch and InventoryItem, not a pure join table because it has its own PK and quantity state.

### StockReceipt and StockReceiptItem

StockReceipt has BranchId, CreatedBy, Status, timestamps, IdempotencyKey, Branch and Items. StockReceiptItem has StockReceiptId, InventoryItemId, Quantity, UnitPrice, StockReceipt and InventoryItem. Cardinalities: Branch 1 to 0..* StockReceipt; StockReceipt 1 to 0..* StockReceiptItem; InventoryItem 1 to 0..* StockReceiptItem. Receipt status is Draft/Confirmed/Cancelled; total is nonnegative; item quantity is positive and unit price is nonnegative.

### StockIssue and StockIssueItem

StockIssue has BranchId, Reason, Status, CreatedBy, timestamps, IdempotencyKey, Branch and Items. StockIssueItem has StockIssueId, InventoryItemId, Quantity, StockIssue and InventoryItem. Cardinalities: Branch 1 to 0..* StockIssue; StockIssue 1 to 0..* StockIssueItem; InventoryItem 1 to 0..* StockIssueItem.

### StockTransaction

Exact fields: Id, BranchId, InventoryItemId, Type, Quantity, BeforeQuantity, AfterQuantity, ReferenceType, ReferenceId, CreatedBy, CreatedAtUtc, Note. Formal FKs are BranchId and InventoryItemId only. ReferenceType is constrained to StockReceipt, StockIssue or StockAdjustment, but ReferenceId is not a configured FK and has no navigation. Therefore StockTransaction to StockReceipt and StockTransaction to StockIssue must not be drawn.

### CANONICAL INVENTORY MODEL

    Branch 1 -> 0..* BranchInventory
    InventoryItem 1 -> 0..* BranchInventory
    Branch 1 -> 0..* StockReceipt
    StockReceipt 1 -> 0..* StockReceiptItem
    InventoryItem 1 -> 0..* StockReceiptItem
    Branch 1 -> 0..* StockIssue
    StockIssue 1 -> 0..* StockIssueItem
    InventoryItem 1 -> 0..* StockIssueItem
    Branch 1 -> 0..* StockTransaction
    InventoryItem 1 -> 0..* StockTransaction
    Expense 0..1 -> 0..1 StockReceipt

All lines above have current FK/navigation/configuration evidence.

## D. ERD PK/FK/Constraint Completion

| Table | PK | FK and nullability | Unique constraints | Important indexes/checks | Delete behavior |
|---|---|---|---|---|---|
| Branches | Id | none | none | none | root |
| Employees | Id | BranchId nullable | none | none | unspecified |
| Tables | Id | BranchId nullable | QrToken unique | QrToken required/max 43 | unspecified |
| Areas | Id | none | none | none | none |
| Products | Id | none | none | none | none |
| Toppings | Id | none | none | none | none |
| Promotions | Id | none | none | none | none |
| Customers | Id | none | PhoneNumber unique | PhoneNumber index | none |
| Orders | Id | CustomerId nullable; BranchId scalar nullable | none | BranchId+CreatedAt+Status | Customer delete unspecified |
| OrderDetails | Id | shadow OrderId required; ProductId/ToppingId nullable | none | optional Product/Topping FKs | Order Cascade |
| OrderRequests | Id | OrderId required | none | none | no explicit cascade |
| OrderRequestItems | Id | OrderRequestId required; ProductId nullable scalar | none | none | OrderRequest Cascade |
| LoyaltyTransactions | Id | CustomerId required; OrderId nullable | filtered OrderId+Type unique | CustomerId+CreatedAt | unspecified |
| Reservations | Id | BranchId/TableId nullable scalar | none | none | none |
| Attendances | Id | EmployeeId/BranchId required | none | none | unspecified |
| WorkSchedules | Id | EmployeeId required; BranchId nullable | none | none | unspecified |
| Shifts | Id | EmployeeId/BranchId required scalar | none | none | none |
| Expenses | Id | BranchId required; StockReceiptId nullable | filtered StockReceiptId unique | BranchId+ExpenseDate | Branch/receipt Restrict |
| BusinessInsights | Id | BranchId nullable scalar | DeduplicationKey unique | BranchId, CreatedAt, Status | none |
| InventoryItems | Id | none | filtered NormalizedName unique | IsActive filter | none |
| BranchInventories | Id | BranchId/InventoryItemId required | composite unique pair | quantity nonnegative | Restrict |
| StockReceipts | Id | BranchId required | filtered BranchId+IdempotencyKey | status/total checks; branch/date | Restrict |
| StockReceiptItems | Id | StockReceiptId/InventoryItemId required | none | quantity/price checks | Restrict |
| StockIssues | Id | BranchId required | filtered BranchId+IdempotencyKey | status check; branch/date | Restrict |
| StockIssueItems | Id | StockIssueId/InventoryItemId required | none | quantity positive | Restrict |
| StockTransactions | Id | BranchId/InventoryItemId required | none | type/reference/quantity checks | Restrict |
| ReceiptSettings | Id | BranchId nullable scalar | BranchId unique | none | none |
| SystemSettings | Id | BranchId nullable scalar | BranchId+Key unique | none | none |
| Notifications | Id | BranchId/TargetUserId/EntityId nullable scalar | none | TargetRole+TargetUserId+CreatedAt | none |

No composite primary key exists. BranchId+InventoryItemId and BranchId+Key are unique indexes, not composite PKs.

## E. Canonical ERD Relationship List

    Branch [1] -- Employee [0..*]
    FK = Employee.BranchId
    VERIFIED

    Branch [1] -- RestaurantTable [0..*]
    FK = RestaurantTable.BranchId
    VERIFIED

    Customer [0..1] -- Order [0..*]
    FK = Order.CustomerId
    VERIFIED

    Order [1] -- OrderDetail [0..*]
    FK = shadow OrderDetail.OrderId
    Delete = Cascade
    VERIFIED

    Product [1] -- OrderDetail [0..*]
    FK = OrderDetail.ProductId nullable
    VERIFIED

    Topping [1] -- OrderDetail [0..*]
    FK = OrderDetail.ToppingId nullable
    VERIFIED

    Order [1] -- OrderRequest [0..*]
    FK = OrderRequest.OrderId
    VERIFIED

    OrderRequest [1] -- OrderRequestItem [0..*]
    FK = OrderRequestItem.OrderRequestId
    Delete = Cascade
    VERIFIED

    Customer [1] -- LoyaltyTransaction [0..*]
    FK = LoyaltyTransaction.CustomerId
    VERIFIED

    Order [0..1] -- LoyaltyTransaction [0..*]
    FK = LoyaltyTransaction.OrderId nullable
    VERIFIED

    Employee [1] -- Attendance [0..*]
    FK = Attendance.EmployeeId
    VERIFIED

    Branch [1] -- Attendance [0..*]
    FK = Attendance.BranchId
    VERIFIED

    Employee [1] -- WorkSchedule [0..*]
    FK = WorkSchedule.EmployeeId
    VERIFIED

    Branch [1] -- WorkSchedule [0..*]
    FK = WorkSchedule.BranchId nullable
    VERIFIED

    Branch [1] -- Expense [0..*]
    FK = Expense.BranchId
    Delete = Restrict
    VERIFIED

    Expense [0..1] -- StockReceipt [0..1]
    FK = Expense.StockReceiptId nullable, filtered unique
    VERIFIED

    Branch [1] -- BranchInventory [0..*]
    FK = BranchInventory.BranchId
    VERIFIED

    InventoryItem [1] -- BranchInventory [0..*]
    FK = BranchInventory.InventoryItemId
    VERIFIED

    Branch [1] -- StockReceipt [0..*]
    FK = StockReceipt.BranchId
    VERIFIED

    StockReceipt [1] -- StockReceiptItem [0..*]
    FK = StockReceiptItem.StockReceiptId
    VERIFIED

    InventoryItem [1] -- StockReceiptItem [0..*]
    FK = StockReceiptItem.InventoryItemId
    VERIFIED

    Branch [1] -- StockIssue [0..*]
    FK = StockIssue.BranchId
    VERIFIED

    StockIssue [1] -- StockIssueItem [0..*]
    FK = StockIssueItem.StockIssueId
    VERIFIED

    InventoryItem [1] -- StockIssueItem [0..*]
    FK = StockIssueItem.InventoryItemId
    VERIFIED

    Branch [1] -- StockTransaction [0..*]
    FK = StockTransaction.BranchId
    VERIFIED

    InventoryItem [1] -- StockTransaction [0..*]
    FK = StockTransaction.InventoryItemId
    VERIFIED

### RELATIONSHIPS THAT MUST NOT BE DRAWN

Branch-Area; Area-RestaurantTable; Branch-Product; Product-Topping join; Order-RestaurantTable; Customer-Reservation; OrderDetail-OrderRequestItem; OrderRequestItem-OrderDetail; Employee-Shift as formal FK; Branch-Shift as formal FK; Branch-Reservation as formal FK; Branch-BusinessInsight as formal FK; Branch-ReceiptSettings; Branch-SystemSetting; Branch-Notification; StockTransaction-StockReceipt; StockTransaction-StockIssue; ProductTopping; OrderDetailTopping.

## F. Direct and Indirect Branch Scope Completion

### DIRECT BRANCH-SCOPED

| Entity | BranchId property | Required? | Evidence |
|---|---|---|---|
| Employee | Guid? BranchId | no | Employee.cs |
| Attendance | Guid BranchId | yes | Attendance.cs and HasForeignKey |
| WorkSchedule | Guid? BranchId | no | WorkSchedule.cs and HasForeignKey |
| RestaurantTable | Guid? BranchId | no | Table.cs and HasForeignKey |
| Order | Guid? BranchId | no | Order.cs; scalar scope, no fluent Branch relation |
| Reservation | Guid? BranchId | no | Reservation.cs; scalar scope |
| Shift | Guid BranchId | yes | Shift.cs; scalar field, no fluent relation |
| Expense | Guid BranchId | yes | Expense.cs and Restrict relation |
| BusinessInsight | Guid? BranchId | no | BusinessInsight.cs and index only |
| ReceiptSettings | Guid? BranchId | no | unique BranchId index |
| SystemSetting | Guid? BranchId | no | unique BranchId+Key index |
| Notification | Guid? BranchId | no | target index only |
| BranchInventory | Guid BranchId | yes | ConfigureInventory |
| StockReceipt | Guid BranchId | yes | ConfigureInventory |
| StockIssue | Guid BranchId | yes | ConfigureInventory |
| StockTransaction | Guid BranchId | yes | ConfigureInventory |

### INDIRECT BRANCH-SCOPED

| Entity | Path to Branch | Evidence |
|---|---|---|
| OrderDetail | shadow OrderId Ă¢â€ â€™ Order.BranchId | Order.Details configured with shadow FK |
| OrderRequest | OrderRequest.OrderId Ă¢â€ â€™ Order.BranchId | required OrderId |
| OrderRequestItem | OrderRequestItem.OrderRequestId Ă¢â€ â€™ OrderRequest.OrderId Ă¢â€ â€™ Order.BranchId | request-item cascade relation |
| StockReceiptItem | StockReceiptItem.StockReceiptId Ă¢â€ â€™ StockReceipt.BranchId | receipt FK |
| StockIssueItem | StockIssueItem.StockIssueId Ă¢â€ â€™ StockIssue.BranchId | issue FK |
| LoyaltyTransaction | optional OrderId Ă¢â€ â€™ Order.BranchId, or Customer context | OrderId nullable; Customer has no BranchId |

Area, Product, Topping, Customer and InventoryItem do not have BranchId. InventoryItem is a global catalog; branch quantity is BranchInventory.

## G. System Architecture Exact Current Model

### Frontend

| App | Exact directory | Roles/purpose | Conclusion |
|---|---|---|---|
| Admin Web | apps/admin-web | Admin, Manager, Employee, Cashier UI; POS; KDS; management | one frontend app |
| Customer Web | apps/customer-web | registered customer and Guest QR | one frontend app |

admin-web contains features/pos and features/kitchen. POS and KDS are logical modules, not independent deployment applications. customer-web contains QRScan, DigitalMenu, CustomerLogin, CustomerProfile and ReservationPage.

### Backend

Current logical components are WebAPI Controllers, AI Controller, Application Services, Domain entities, Infrastructure Persistence, EF Core/Npgsql, JWT Authentication/Authorization, SignalR KitchenHub/KitchenNotifier and the AI module. They are inside one ASP.NET Core API project/process.

Application Services do use ApplicationDbContext directly. Exact classification: **B. Clean Architecture-inspired layered architecture.** Strict Clean Architecture is not accurate because OrderService, KitchenService, LoyaltyService and InventoryService directly depend on ApplicationDbContext.

### Runtime connection matrix

| Source | Destination | Connection | Status |
|---|---|---|---|
| Guest QR/Registered Customer | Customer Web | browser/HTTP frontend | VERIFIED |
| Staff/Admin/Kitchen | Admin Web | browser/HTTP frontend | VERIFIED |
| Customer Web | WebAPI | fetch HTTP via runtime API URL | VERIFIED |
| Admin Web/POS/KDS | WebAPI | fetch HTTP via runtime API URL | VERIFIED |
| Admin Web POS/KDS | KitchenHub | SignalR at /kitchenHub | VERIFIED |
| Customer Web | KitchenHub | no current client connection | NOT VERIFIED; DO NOT DRAW |
| Controllers | Application Services | DI interface/method calls | VERIFIED |
| Application Services | ApplicationDbContext | direct EF calls | VERIFIED |
| ApplicationDbContext | PostgreSQL | EF Core/Npgsql | VERIFIED |
| AiController | AiOrchestrator | IAiOrchestrator.ProcessAsync | VERIFIED |
| AiOrchestrator | AiContextService | method call | VERIFIED |
| AiOrchestrator | PromptBuilder | method call | VERIFIED |
| AiOrchestrator | AiToolRegistry | schema/tool lookup | VERIFIED |
| AiOrchestrator | AI provider implementation | method call | VERIFIED |
| AI provider implementation | external AI provider | external HTTPS POST | VERIFIED |
| AI tools | Application Services | injected service interfaces | VERIFIED for all 14 |
| Application Services | KitchenNotifier | IKitchenNotifier in kitchen flow | VERIFIED |
| OrderController | HubContext<KitchenHub> | PaymentCompleted Clients.All | VERIFIED |

### CANONICAL SYSTEM ARCHITECTURE

    Actors
      Ă¢â€ â€™ Customer Web
      Ă¢â€ â€™ Admin Web
        Ă¢â€ â€™ POS UI logical module
        Ă¢â€ â€™ KDS UI logical module
      Ă¢â€ â€™ ASP.NET Core 8 API
        Ă¢â€ â€™ WebAPI Controllers
        Ă¢â€ â€™ JWT Authentication/Authorization
        Ă¢â€ â€™ Application Services
        Ă¢â€ â€™ Domain Entities
        Ă¢â€ â€™ Infrastructure / EF Core / Npgsql
        Ă¢â€ â€™ SignalR KitchenHub
        Ă¢â€ â€™ AI module
      Ă¢â€ â€™ PostgreSQL database, configured for Supabase
      Ă¢â€ â€™ external AI provider as external AI service

Components to draw: the two frontend apps, logical POS/KDS modules, one API process, logical layers, SignalR, database and AI provider external service.

Optional: individual controller groups, tool group, PromptBuilder/Context subcomponents.

Components not to draw: Customer Web to SignalR, separate POS/KDS applications, payment gateway, server-side VietQR settlement, webhook, Docker deployment or production cloud app deployment.

## H. SignalR Exact Topology

### Connected groups

| Group | Join condition in KitchenHub.OnConnectedAsync | Status |
|---|---|---|
| kitchen-admin | role equals admin | CONNECTED GROUP VERIFIED |
| role:{role} | non-empty role | CONNECTED GROUP VERIFIED |
| kitchen-branch:{branchId} | valid branchId claim | CONNECTED GROUP VERIFIED |
| branch:{branchId} | valid branchId claim | CONNECTED GROUP VERIFIED |
| branch:{branchId}:role:{role} | valid branchId and role | CONNECTED GROUP VERIFIED |

### Actual event targets

| Event | Publisher | Actual target | Consumer |
|---|---|---|---|
| NewOrderRequest | KitchenService Ă¢â€ â€™ KitchenNotifier.NotifyNewOrderRequestAsync | kitchen-admin plus kitchen-branch:{branchId}; fallback kitchen-admin | KitchenPage SignalR listener |
| RequestStatusUpdated | KitchenService Ă¢â€ â€™ KitchenNotifier.NotifyRequestStatusUpdatedAsync | branch-aware overload targets kitchen-admin plus kitchen-branch:{branchId}; other overload targets kitchen-admin | KDS status flow |
| PaymentCompleted | OrderController.PayOrder | Clients.All; payload includes branchId | Admin/POS listeners and read-refresh paths |

role:{role}, branch:{branchId} and branch:{branchId}:role:{role} are joined groups but are not actual targets for these three publishers. They must not be presented as if every event is sent to every joined group.

## I. AI Exact Current Pipeline

### Component matrix

| Component | Exact class/interface | File | Responsibility |
|---|---|---|---|
| Entry | AiController | AI/Controllers/AiController.cs | Authorize, POST /api/Ai/chat, claims, guest block |
| Orchestrator | AiOrchestrator / IAiOrchestrator | AI/Services/AiOrchestrator.cs | context, prompt, AI provider loop, tool calls |
| Context | AiContextService | AI/Services/AiContextService.cs | DB-backed role context |
| Prompt | PromptBuilder | AI/Services/PromptBuilder.cs | role prompt selection |
| AI provider adapter | AI provider implementation / IAI provider implementation | AI/Services/AI provider implementation.cs | external API request |
| Registry | AiToolRegistry | AI/Tools/AiToolRegistry.cs | role-filtered schema and lookup |
| Permission | AiPermissionService | AI/Services/AiPermissionService.cs | role/risk/manager branch check |
| Authorization | AiAuthorization | AI/Security/AiAuthorization.cs | role/risk/branch helper |
| Validator | AiToolValidator | AI/Utils/AiToolValidator.cs | required/type/range/branch validation |
| Contract | IAiTool | AI/Tools/IAiTool.cs | Name, AllowedRoles, RiskLevel, schema, execute |
| Risk | ToolRiskLevel | AI/Security/ToolRiskLevel.cs | Read, Write, Destructive |

### Exact numbered call chain

1. AI frontend sends HTTP POST /api/Ai/chat.
2. AiController reads Role, NameIdentifier, fullName/Name, branchId and customerSessionType claims.
3. Guest customer session is rejected with Forbid before orchestration.
4. Controller creates AiUserContext.
5. Controller calls IAiOrchestrator.ProcessAsync.
6. Orchestrator calls AiContextService.GetContextAsync.
7. AiContextService reads ApplicationDbContext for role context.
8. Orchestrator calls PromptBuilder.BuildSystemInstruction.
9. Orchestrator calls AiToolRegistry.GetAI providerToolsDefinition(role).
10. Orchestrator builds contents, system instruction and role-filtered tools.
11. Orchestrator calls AI provider implementation.GenerateContentAsync.
12. AI provider implementation sends HTTPS generateContent request to external AI provider.
13. Text-only response terminates successfully with AiResponse.
14. Function-call response goes to AiToolRegistry.GetTool.
15. AiPermissionService.CanExecuteAsync rechecks manager branch, role and risk.
16. Tool-specific argument validation runs through AiToolValidator or tool parsing.
17. IAiTool.ExecuteAsync calls its Application Service.
18. Application Service uses EF Core/ApplicationDbContext and PostgreSQL.
19. Tool result becomes functionResponse.
20. Orchestrator appends response and calls AI provider again while iterations < 5.
21. Final text terminates; unknown tool, denied permission, invalid args, AI provider error or max loop returns error.

### Complete 14-tool data-access matrix

| Tool | Direct DbContext? | Application Service? | Other dependency |
|---|---|---|---|
| get_active_staff | NO | IEmployeeService | none |
| get_best_sellers | NO | IDashboardService | none |
| get_business_summary | NO | IDashboardService | none |
| get_financial_analysis | NO | IFinancialAnalysisService | none |
| get_order_list | NO | IOrderService | none |
| get_revenue_comparison | NO | IDashboardService | none |
| get_revenue | NO | IDashboardService | none |
| update_product_price | NO | IProductService | none |
| employee_get_table_summary | NO | ITableService | none |
| get_my_shift | NO | IEmployeeService | none |
| update_order_status | NO | IOrderService | none |
| customer_get_menu | NO | IProductService | none |
| get_my_orders | NO | IOrderService | none |
| create_booking | NO | IReservationService | none |

Conclusion: AI provider to DB direct is FALSE. Tool to DbContext direct is FALSE for all current 14 tools. Tool to Application Service is VERIFIED for all current 14 tools. AiContextService is a separate context component that directly reads DbContext.

### BusinessInsight exact status

BusinessInsight is persisted through DbSet<BusinessInsight>, has direct nullable BranchId, unique DeduplicationKey and indexes on BranchId, CreatedAt and Status. ProactiveInsightService and InsightBackgroundService create/store insights; BusinessInsightController and Admin Web consume them. DashboardService and FinancialAnalysisService compute revenue, comparisons, business summary, best sellers and financial analysis; AI tools expose selected computed analysis. Stored BusinessInsight, computed analysis response and AI tool wrapper are distinct.

## J. Four Diagram Direct Verification

### Domain Class Diagram

All current entities in the original inventory and completion matrix are VERIFIED. Rename Table to RestaurantTable and OrderItem to OrderDetail. Remove Restaurant, Category entity, ProductTopping, OrderDetailTopping, Invoice and removed staff-finance module. Area, Shift, Reservation, BusinessInsight, ReceiptSettings, SystemSetting and Notification are current entities, but their scalar fields must not become absent formal relationships.

### ERD

Final formal cardinalities are the relations in section E. In particular:

    Order [1] Ă¢â€ â€™ OrderDetail [0..*]
    FK = shadow OrderDetail.OrderId, required, Cascade

    Order [1] Ă¢â€ â€™ OrderRequest [0..*]
    FK = OrderRequest.OrderId, required

    OrderRequest [1] Ă¢â€ â€™ OrderRequestItem [0..*]
    FK = OrderRequestItem.OrderRequestId, required, Cascade

OrderRequestItem has no relation to OrderDetail. Customer to Reservation, Branch to Area, Area to RestaurantTable, Product to Topping and Table to Order are FALSE - DO NOT DRAW.

### System Architecture

Include actors, Customer Web, Admin Web with POS/KDS logical modules, one ASP.NET Core API, Controllers/Auth/Application/Domain/Infrastructure/AI layers, SignalR /kitchenHub, PostgreSQL/Supabase database and AI provider external service. Do not draw Customer Web to SignalR, separate POS/KDS deployments, payment gateway, server-side VietQR, webhook, Docker or production cloud app.

### AI Architecture

Include AI UI, AiController, AiUserContext, AiOrchestrator, AiContextService, PromptBuilder, AiToolRegistry, AiPermissionService/AiAuthorization, AiToolValidator, IAiTool/14 tools, AI provider implementation, external AI provider, Application Services, EF Core/DbContext, PostgreSQL, AiToolResult and AiResponse. Draw role filtering before AI provider, permission recheck after Function Call, tool-to-service-to-database path, guest block and sequential loop max 5.

## K. Final PlantUML Input Specification

### K1. Domain Class Specification

Classes to display:

    Branch(Id, Name, IsActive)
    Employee(Id, FullName, Role, BranchId?, IsActive)
    Customer(Id, FullName?, PhoneNumber?, LoyaltyPoints, IsActive)
    RestaurantTable(Id, Name, BranchId?, Status, QrToken)
    Area(Id, Name)
    Product(Id, Name?, Category?, Price, IsActive, AvailabilityStatus)
    Topping(Id, Name, Price, IsActive)
    Promotion(Id, Name, RequiredPoints, DiscountValue, IsActive)
    Order(Id, BranchId?, CustomerId?, Status, SubTotal?, TotalAmount, PaidAmount, PaymentMethod?)
    OrderDetail(Id, ProductId?, ToppingId?, Quantity, SentQuantity, UnitPrice)
    OrderRequest(Id, OrderId, RequestNumber, Status)
    OrderRequestItem(Id, OrderRequestId, ProductId?, Quantity, ProductName)
    LoyaltyTransaction(Id, CustomerId, OrderId?, Type, Points)
    Reservation(Id, BranchId?, TableId?, ReservationTime, Status)
    Attendance(Id, EmployeeId, BranchId, CheckInTime, CheckOutTime?)
    WorkSchedule(Id, EmployeeId, BranchId?, Date, ShiftName?)
    Shift(Id, EmployeeId, BranchId, StartTime, EndTime?, Status)
    Expense(Id, BranchId, Amount, ExpenseDate, StockReceiptId?)
    BusinessInsight(Id, BranchId?, Type, Severity, Status)
    InventoryItem(Id, Name, UnitCode, IsActive)
    BranchInventory(Id, BranchId, InventoryItemId, CurrentQuantity, MinimumStock)
    StockReceipt(Id, BranchId, TotalAmount, Status)
    StockReceiptItem(Id, StockReceiptId, InventoryItemId, Quantity, UnitPrice)
    StockIssue(Id, BranchId, Reason, Status)
    StockIssueItem(Id, StockIssueId, InventoryItemId, Quantity)
    StockTransaction(Id, BranchId, InventoryItemId, Type, Quantity, BeforeQuantity, AfterQuantity)

Draw only formal relations from section E. Composition is justified for Order to OrderDetail and OrderRequest to OrderRequestItem by cascade evidence; use ordinary associations for other relations.

### K2. ERD Specification

Use the 29 current tables in section 7. Every current primary key is Id. Use exact nullable FK, unique index, check and delete values from section D. Do not add a composite primary key. BranchId+InventoryItemId and BranchId+Key are unique indexes, not PKs. Do not add any relationship from the MUST NOT DRAW list.

### K3. System Architecture Specification

Nodes:

    Guest QR / Registered Customer
    Admin / Manager / Employee / Cashier / Kitchen
    Customer Web
    Admin Web
    POS UI logical module
    KDS UI logical module
    ASP.NET Core 8 API
    WebAPI Controllers
    JWT Authentication/Authorization
    Application Services
    Domain Entities
    EF Core / Npgsql
    SignalR KitchenHub
    AI Module
    PostgreSQL
    external AI provider

Edges:

    Customer Web Ă¢â€ â€™ API: HTTP REST
    Admin Web Ă¢â€ â€™ API: HTTP REST
    POS UI Ă¢â€ â€™ API: HTTP REST
    KDS UI Ă¢â€ â€™ API: HTTP REST
    POS UI Ă¢â€ â€™ KitchenHub: SignalR /kitchenHub
    KDS UI Ă¢â€ â€™ KitchenHub: SignalR /kitchenHub
    Controllers Ă¢â€ â€™ Application Services: DI method calls
    Application Services Ă¢â€ â€™ Domain Entities: business model use
    Application Services Ă¢â€ â€™ ApplicationDbContext: EF data access
    ApplicationDbContext Ă¢â€ â€™ PostgreSQL: Npgsql
    Application Services Ă¢â€ â€™ KitchenNotifier: realtime notification
    AI Module Ă¢â€ â€™ external AI provider: external HTTPS

### K4. AI Architecture Specification

Nodes:

    AI UI, AiController, AiUserContext, AiOrchestrator, AiContextService,
    PromptBuilder, AiToolRegistry, AiPermissionService, AiAuthorization,
    AiToolValidator, IAiTool/14 implementations, AI provider implementation, external AI provider,
    Application Services, ApplicationDbContext/EF Core, PostgreSQL,
    AiToolResult, AiResponse

Edges:

    AI UI Ă¢â€ â€™ AiController: HTTP POST /api/Ai/chat
    AiController Ă¢â€ â€™ AiUserContext: claims construction
    AiController Ă¢â€ â€™ AiOrchestrator: ProcessAsync
    AiOrchestrator Ă¢â€ â€™ AiContextService: context method call
    AiOrchestrator Ă¢â€ â€™ PromptBuilder: role prompt method call
    AiOrchestrator Ă¢â€ â€™ AiToolRegistry: role-filtered schema lookup
    AiOrchestrator Ă¢â€ â€™ AI provider implementation: model request method call
    AI provider implementation Ă¢â€ â€™ external AI provider: external HTTPS generateContent
    FunctionCall Ă¢â€ â€™ AiToolRegistry: runtime name lookup
    Registry Ă¢â€ â€™ AiPermissionService: permission check
    Permission Ă¢â€ â€™ Tool: execution gate
    Tool Ă¢â€ â€™ Application Service: tool execution
    Application Service Ă¢â€ â€™ ApplicationDbContext: EF read/write
    ApplicationDbContext Ă¢â€ â€™ PostgreSQL: database access
    Tool Ă¢â€ â€™ AiToolResult: result
    AiToolResult Ă¢â€ â€™ AiOrchestrator: function response
    AiOrchestrator Ă¢â€ â€™ AI provider implementation: next iteration
    Final text Ă¢â€ â€™ AiResponse/UI: final response

Loop guard: iterations < 5. Behavior: sequential. Permission location: after Function Call and before tool execution. Guest rejection: AiController before orchestrator. Unknown tool, denied permission, invalid argument and provider error are alt error branches.

## L. Evidence Completion Status

| Area | Complete |
|---|---|
| Complete 29-entity exact field matrix | YES |
| Exact PK/FK/navigation matrix | YES |
| Exact relationship/cardinality matrix | YES |
| Inventory model and relationships | YES |
| ERD PK/FK/unique/index matrix | YES |
| Canonical ERD relationship list | YES |
| Direct vs indirect branch scope | YES |
| Exact architecture connections | YES |
| SignalR connected groups vs actual targets | YES |
| AI components and numbered pipeline | YES |
| Exact 14 AI tools | YES |
| Per-tool direct DB/Application Service matrix | YES |
| Four diagram verification | YES |
| Four PlantUML input specifications | YES |

## Final completion record

- Diagram evidence completion: YES
- Output: baocaokhoaluan/THESIS-DIAGRAM-SOURCE-AUDIT.md
- Entity field matrix complete: YES
- Relationship matrix complete: YES
- Inventory model complete: YES
- ERD PK/FK complete: YES
- System architecture complete: YES
- 14 AI tools complete: YES
- AI pipeline complete: YES
- Domain Class PlantUML input ready: YES, after applying MUST NOT DRAW corrections
- ERD PlantUML input ready: YES, after applying exact FK/cardinality list
- System Architecture PlantUML input ready: YES
- AI Architecture PlantUML input ready: YES
- Current persisted entities: 29
- Current DbSets: 29
- Runtime AI tools: 14
- Remaining blockers: none for source-grounded design; absent relationships/components listed above must not be drawn
- Source code modified: NO
- Thesis modified: NO
- PlantUML generated: NO
- Sole intentional output: baocaokhoaluan/THESIS-DIAGRAM-SOURCE-AUDIT.md

## DIAGRAM EVIDENCE COMPLETION Ă¢â‚¬â€ READY FOR CONTROLLED PLANTUML GENERATION

# BACKEND DOMAIN CLASS DIAGRAM FINAL AUDIT

PhĂ¡ÂºÂ§n nÄ‚Â y lÄ‚Â  kĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ audit read-only Ă„â€˜Ă¡Â»â€˜i vĂ¡Â»â€ºi backend hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i, dÄ‚Â¹ng lÄ‚Â m source-of-truth cho Domain Class Diagram. Quan hĂ¡Â»â€¡ chĂ¡Â»â€° Ă„â€˜Ă†Â°Ă¡Â»Â£c xÄ‚Â¡c nhĂ¡ÂºÂ­n khi cÄ‚Â³ FK/navigation vÄ‚Â /hoĂ¡ÂºÂ·c cĂ¡ÂºÂ¥u hÄ‚Â¬nh EF Core tĂ†Â°Ă†Â¡ng Ă¡Â»Â©ng. CÄ‚Â¡c trĂ†Â°Ă¡Â»Âng cÄ‚Â³ tÄ‚Âªn giĂ¡Â»â€˜ng FK nhĂ†Â°ng chĂ¡Â»â€° lÄ‚Â  scalar khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c chuyĂ¡Â»Æ’n thÄ‚Â nh association trong UML.

## PHĂ¡ÂºÂ¦N A Ă¢â‚¬â€ Danh sÄ‚Â¡ch toÄ‚Â n bĂ¡Â»â„¢ Entity vÄ‚Â  thuĂ¡Â»â„¢c tÄ‚Â­nh

Backend hiĂ¡Â»â€¡n cÄ‚Â³ 29 entity/DbSet:

| Entity | ThuĂ¡Â»â„¢c tÄ‚Â­nh hiĂ¡Â»â€¡n cÄ‚Â³ (tÄ‚Âªn vÄ‚Â  kiĂ¡Â»Æ’u) |
|---|---|
| Area | Id: Guid; Name: string; DisplayOrder: int; CreatedAt: DateTime |
| Attendance | Id: Guid; EmployeeId: Guid; EmployeeName: string?; CheckInTime: DateTime; CheckOutTime: DateTime?; BranchId: Guid; Status: string?; Note: string?; ImagePath: string? |
| Branch | Id: Guid; Name: string; Address: string?; PhoneNumber: string?; IsMain: bool; IsActive: bool; BankName: string?; AccountNumber: string?; AccountHolder: string?; ImageUrl: string?; TaxCode: string?; RepresentativeName: string?; RepresentativeEmail: string?; Industry: string?; BusinessType: string?; CreatedAt: DateTime |
| BranchInventory | Id: Guid; BranchId: Guid; InventoryItemId: Guid; CurrentQuantity: decimal; MinimumStock: decimal; UpdatedAtUtc: DateTime; Branch: Branch?; InventoryItem: InventoryItem? |
| BusinessInsight | Id: Guid; BranchId: Guid?; Type: string; Severity: string; Title: string; Summary: string; AiExplanation: string?; AiRecommendation: string?; EvidenceJson: string; PeriodStart: DateTime; PeriodEnd: DateTime; ComparisonPeriodStart: DateTime?; ComparisonPeriodEnd: DateTime?; DetectedAt: DateTime; Status: string; DeduplicationKey: string; CreatedAt: DateTime |
| Customer | Id: Guid; FullName: string?; PhoneNumber: string?; Email: string?; Address: string?; Birthday: DateTime?; Gender: string?; CustomerGroup: string?; TotalSpending: decimal; TotalOrders: int; LoyaltyPoints: int; IsActive: bool; Password: string?; CreatedAt: DateTime; UpdatedAt: DateTime; LastOrderDate: DateTime? |
| Employee | Id: Guid; EmployeeCode: string?; EmployeeType: string; FullName: string; PhoneNumber: string?; Position: string?; Department: string?; BasicSalary: decimal; BranchId: Guid?; BranchName: string?; CitizenId: string?; BirthDate: DateTime?; Gender: string?; Address: string?; StartDate: DateTime; IsActive: bool; Role: string; Note: string?; Username: string?; Password: string?; CreatedAt: DateTime; Branch: Branch? |
| Expense | Id: Guid; BranchId: Guid; Description: string; Amount: decimal; ExpenseDate: DateTime; Category: string; PaymentMethod: string; Note: string; CreatedBy: Guid?; StockReceiptId: Guid?; CreatedAt: DateTime; UpdatedAt: DateTime?; StockReceipt: StockReceipt? |
| InventoryItem | Id: Guid; Name: string; NormalizedName: string; UnitCode: string; IsActive: bool; CreatedAtUtc: DateTime; UpdatedAtUtc: DateTime?; BranchInventories: ICollection<BranchInventory> |
| LoyaltyTransaction | Id: Guid; CustomerId: Guid; OrderId: Guid?; Type: string; Points: int; BalanceAfter: int; Description: string?; CreatedAt: DateTime |
| Notification | Id: Guid; Type: string; Title: string; Message: string; EntityType: string?; EntityId: Guid?; Route: string?; TargetRole: string?; TargetUserId: Guid?; BranchId: Guid?; IsRead: bool; CreatedAt: DateTime |
| Order | Id: Guid; InvoiceCode: string?; CreatedAt: DateTime; PaymentAt: DateTime?; CustomerId: Guid?; CustomerName: string?; CustomerPhone: string?; CustomerEmail: string?; SubTotal: decimal?; VatAmount: decimal?; VatPercent: decimal?; ServiceFeeAmount: decimal?; ServiceFeePercent: decimal?; TotalAmount: decimal; Discount: decimal; PaidAmount: decimal; PaymentMethod: string?; Status: string; Note: string?; TableName: string?; CreatedBy: string?; BranchId: Guid?; BranchName: string?; Details: List<OrderDetail> |
| OrderDetail | Id: Guid; ProductId: Guid?; ToppingId: Guid?; ProductName: string; Quantity: int; SentQuantity: int; UnitPrice: decimal; Options: string?; SubTotal: decimal (computed) |
| OrderRequest | Id: Guid; OrderId: Guid; RequestNumber: int; Status: string; CreatedAt: DateTime; AcceptedAt: DateTime?; PreparingAt: DateTime?; CompletedAt: DateTime?; ProcessedBy: string?; Note: string?; Items: List<OrderRequestItem> |
| OrderRequestItem | Id: Guid; OrderRequestId: Guid; ProductId: Guid?; ProductName: string; Quantity: int; Note: string?; Options: string? |
| Product | Id: Guid; Code: string?; Name: string?; Category: string?; Group: string?; Price: decimal; CostPrice: decimal; ImageUrl: string?; SizesJson: string?; ToppingsJson: string?; IsActive: bool; AvailabilityStatus: string; CreatedAt: DateTime |
| Promotion | Id: Guid; Name: string; Description: string?; RequiredPoints: int; DiscountValue: decimal; PromotionType: string; IsActive: bool; EndDate: DateTime?; CreatedAt: DateTime |
| ReceiptSettings | Id: Guid; BranchId: Guid?; PaperWidth: int; ShowLogo: bool; ShowAddress: bool; ShowPhone: bool; ShowStaff: bool; ShowPaymentMethod: bool; ShowOrderNote: bool; ShowThankYou: bool; ThankYouText: string; FontFamily: string; FontSize: int; UpdatedAt: DateTime |
| Reservation | Id: Guid; CustomerName: string?; CustomerPhone: string?; ReservationTime: DateTime; NumberOfGuests: int; BranchId: Guid?; BranchName: string?; Note: string?; Status: string; CreatedAt: DateTime; TableId: Guid?; TableName: string? |
| RestaurantTable | Id: Guid; Name: string; AreaName: string; SeatCount: int; Description: string?; IsActive: bool; Status: string; QrCodeUrl: string?; QrToken: string; BranchId: Guid?; BranchName: string?; CreatedAt: DateTime |
| Shift | Id: Guid; EmployeeId: Guid; EmployeeName: string; BranchId: Guid; BranchName: string?; StartTime: DateTime; EndTime: DateTime?; StartingCash: decimal; EndingCash: decimal?; TotalRevenue: decimal; CashRevenue: decimal; TransferRevenue: decimal; Status: string; Note: string |
| StockIssue | Id: Guid; BranchId: Guid; Reason: string; Note: string?; Status: StockDocumentStatus; CreatedBy: Guid; CreatedAtUtc: DateTime; ConfirmedBy: Guid?; ConfirmedAtUtc: DateTime?; IdempotencyKey: string?; Branch: Branch?; Items: ICollection<StockIssueItem> |
| StockIssueItem | Id: Guid; StockIssueId: Guid; InventoryItemId: Guid; Quantity: decimal; StockIssue: StockIssue?; InventoryItem: InventoryItem? |
| StockReceipt | Id: Guid; BranchId: Guid; SupplierName: string?; PurchaseDate: DateTime?; TotalAmount: decimal; Status: StockDocumentStatus; Note: string?; CreatedBy: Guid; CreatedAtUtc: DateTime; ConfirmedBy: Guid?; ConfirmedAtUtc: DateTime?; IdempotencyKey: string?; Branch: Branch?; Items: ICollection<StockReceiptItem> |
| StockReceiptItem | Id: Guid; StockReceiptId: Guid; InventoryItemId: Guid; Quantity: decimal; UnitPrice: decimal; StockReceipt: StockReceipt?; InventoryItem: InventoryItem? |
| StockTransaction | Id: Guid; BranchId: Guid; InventoryItemId: Guid; Type: StockTransactionType; Quantity: decimal; BeforeQuantity: decimal; AfterQuantity: decimal; ReferenceType: string; ReferenceId: Guid; CreatedBy: Guid; CreatedAtUtc: DateTime; Note: string?; Branch: Branch?; InventoryItem: InventoryItem? |
| SystemSetting | Id: Guid; BranchId: Guid?; Key: string; Value: string; Type: string; UpdatedAt: DateTime |
| Topping | Id: Guid; Name: string; Price: decimal; CostPrice: decimal; ToppingType: string?; Category: string?; Description: string?; ImageUrl: string?; IsActive: bool; CreatedAt: DateTime |
| WorkSchedule | Id: Guid; EmployeeId: Guid; EmployeeName: string?; Date: DateTime; ShiftName: string?; StartTime: string?; EndTime: string?; BranchId: Guid?; BranchName: string? |

EF evidence summary: cÄ‚Â¡c quan hĂ¡Â»â€¡ Ă„â€˜Ă†Â°Ă¡Â»Â£c xÄ‚Â¡c nhĂ¡ÂºÂ­n Ă„â€˜Ă¡Â»Âu nĂ¡ÂºÂ±m trong OnModelCreating cĂ¡Â»Â§a ApplicationDbContext. KhÄ‚Â´ng tÄ‚Â¬m thĂ¡ÂºÂ¥y data annotation nÄ‚Â o khai bÄ‚Â¡o quan hĂ¡Â»â€¡. KhÄ‚Â´ng cÄ‚Â³ cĂ¡ÂºÂ¥u hÄ‚Â¬nh quan hĂ¡Â»â€¡ Ă„â€˜Ă¡Â»â„¢c lĂ¡ÂºÂ­p nÄ‚Â o thay thĂ¡ÂºÂ¿ cho cÄ‚Â¡c cĂ¡ÂºÂ¥u hÄ‚Â¬nh Ă„â€˜Ä‚Â£ nÄ‚Âªu. CÄ‚Â¡c enum dÄ‚Â¹ng trong domain gĂ¡Â»â€œm StockDocumentStatus vÄ‚Â  StockTransactionType; enum khÄ‚Â´ng phĂ¡ÂºÂ£i entity vÄ‚Â  khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c vĂ¡ÂºÂ½ nhĂ†Â° class persistence.

## PHĂ¡ÂºÂ¦N B Ă¢â‚¬â€ Relationship Matrix

KÄ‚Â½ hiĂ¡Â»â€¡u: RELATION = cÄ‚Â³ association EF Ă„â€˜Ä‚Â£ xÄ‚Â¡c minh; SCALAR REFERENCE = chĂ¡Â»â€° cÄ‚Â³ trĂ†Â°Ă¡Â»Âng Ă„â€˜Ă¡Â»â€¹nh danh, khÄ‚Â´ng cÄ‚Â³ association EF; NO RELATION = khÄ‚Â´ng Ă„â€˜Ă¡Â»Â§ bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng hoĂ¡ÂºÂ·c khÄ‚Â´ng tĂ¡Â»â€œn tĂ¡ÂºÂ¡i FK.

| # | CĂ¡ÂºÂ·p entity | FK/property evidence | Navigation/configuration evidence | Cardinality vÄ‚Â  delete behavior | KĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n |
|---:|---|---|---|---|---|
| 1 | Branch Ă¢â‚¬â€ Employee | Employee.BranchId, nullable | Employee Ă¢â€ â€™ Branch; HasOne<Branch>().WithMany() | Branch 1 Ă¢â‚¬â€ Employee 0..*; FK optional; behavior mĂ¡ÂºÂ·c Ă„â€˜Ă¡Â»â€¹nh EF | RELATION |
| 2 | Branch Ă¢â‚¬â€ RestaurantTable | RestaurantTable.BranchId, nullable | RestaurantTable Ă¢â€ â€™ Branch; HasOne<Branch>().WithMany() | Branch 1 Ă¢â‚¬â€ Table 0..*; FK optional; behavior mĂ¡ÂºÂ·c Ă„â€˜Ă¡Â»â€¹nh EF | RELATION |
| 3 | Branch Ă¢â‚¬â€ Area | KhÄ‚Â´ng cÄ‚Â³ Area.BranchId | KhÄ‚Â´ng navigation/configuration | KhÄ‚Â´ng xÄ‚Â¡c Ă„â€˜Ă¡Â»â€¹nh | NO RELATION |
| 4 | Area Ă¢â‚¬â€ RestaurantTable | Table chĂ¡Â»â€° cÄ‚Â³ AreaName: string | KhÄ‚Â´ng AreaId/navigation/configuration | KhÄ‚Â´ng xÄ‚Â¡c Ă„â€˜Ă¡Â»â€¹nh | NO RELATION |
| 5 | Branch Ă¢â‚¬â€ Product | KhÄ‚Â´ng cÄ‚Â³ Product.BranchId | KhÄ‚Â´ng navigation/configuration | KhÄ‚Â´ng xÄ‚Â¡c Ă„â€˜Ă¡Â»â€¹nh | NO RELATION |
| 6 | Product Ă¢â‚¬â€ Topping | Product.ToppingsJson chĂ¡Â»â€° lÄ‚Â  JSON scalar | KhÄ‚Â´ng Product-Topping navigation/configuration | KhÄ‚Â´ng xÄ‚Â¡c Ă„â€˜Ă¡Â»â€¹nh; khÄ‚Â´ng suy diĂ¡Â»â€¦n many-to-many | NO RELATION |
| 7 | Customer Ă¢â‚¬â€ Order | Order.CustomerId, nullable | Order Ă¢â€ â€™ Customer; HasOne<Customer>().WithMany() | Customer 0..1 Ă¢â‚¬â€ Order 0..*; FK Order optional; behavior mĂ¡ÂºÂ·c Ă„â€˜Ă¡Â»â€¹nh EF | RELATION |
| 8 | Branch Ă¢â‚¬â€ Order | Order.BranchId, nullable; index branch/date/status | KhÄ‚Â´ng Branch navigation vÄ‚Â  khÄ‚Â´ng HasOne<Branch> | ChĂ¡Â»â€° scalar branch scope | SCALAR REFERENCE |
| 9 | RestaurantTable Ă¢â‚¬â€ Order | Order khÄ‚Â´ng cÄ‚Â³ TableId; chĂ¡Â»â€° TableName | KhÄ‚Â´ng navigation/configuration | KhÄ‚Â´ng tĂ¡Â»â€œn tĂ¡ÂºÂ¡i FK | NO RELATION |
| 10 | Order Ă¢â‚¬â€ OrderDetail | Shadow FK OrderId no OrderDetail class property | Order.Details; HasMany().WithOne().HasForeignKey(OrderId).OnDelete(Cascade) | Order 1 Ă¢â‚¬â€ Detail 0..*; required shadow FK; cascade; composition candidate | RELATION |
| 11 | Product Ă¢â‚¬â€ OrderDetail | OrderDetail.ProductId, nullable | HasOne<Product>().WithMany().HasForeignKey(ProductId).IsRequired(false) | Product 1 Ă¢â‚¬â€ Detail 0..*; FK optional; behavior mĂ¡ÂºÂ·c Ă„â€˜Ă¡Â»â€¹nh EF | RELATION |
| 12 | Topping Ă¢â‚¬â€ OrderDetail | OrderDetail.ToppingId, nullable | HasOne<Topping>().WithMany().HasForeignKey(ToppingId).IsRequired(false) | Topping 1 Ă¢â‚¬â€ Detail 0..*; FK optional; behavior mĂ¡ÂºÂ·c Ă„â€˜Ă¡Â»â€¹nh EF | RELATION |
| 13 | Order Ă¢â‚¬â€ OrderRequest | OrderRequest.OrderId, required | HasOne<Order>().WithMany().HasForeignKey(OrderId) | Order 1 Ă¢â‚¬â€ Request 0..*; required FK; behavior mĂ¡ÂºÂ·c Ă„â€˜Ă¡Â»â€¹nh EF | RELATION |
| 14 | OrderRequest Ă¢â‚¬â€ OrderRequestItem | OrderRequestItem.OrderRequestId, required | OrderRequest.Items; HasMany().WithOne().HasForeignKey().OnDelete(Cascade) | Request 1 Ă¢â‚¬â€ Item 0..*; cascade; composition candidate | RELATION |
| 15 | OrderDetail Ă¢â‚¬â€ OrderRequestItem | KhÄ‚Â´ng cÄ‚Â³ OrderDetailId | KhÄ‚Â´ng navigation/configuration | KhÄ‚Â´ng xÄ‚Â¡c Ă„â€˜Ă¡Â»â€¹nh | NO RELATION |
| 16 | Customer Ă¢â‚¬â€ LoyaltyTransaction | LoyaltyTransaction.CustomerId, required | HasOne<Customer>().WithMany().HasForeignKey(CustomerId) | Customer 1 Ă¢â‚¬â€ Transaction 0..*; required; behavior mĂ¡ÂºÂ·c Ă„â€˜Ă¡Â»â€¹nh EF | RELATION |
| 17 | Order Ă¢â‚¬â€ LoyaltyTransaction | LoyaltyTransaction.OrderId, nullable | HasOne<Order>().WithMany().HasForeignKey(OrderId).IsRequired(false) | Order 0..1 Ă¢â‚¬â€ Transaction 0..*; FK optional; behavior mĂ¡ÂºÂ·c Ă„â€˜Ă¡Â»â€¹nh EF | RELATION |
| 18 | Branch Ă¢â‚¬â€ Reservation | Reservation.BranchId, nullable | KhÄ‚Â´ng navigation/configuration | ChĂ¡Â»â€° scalar reference | SCALAR REFERENCE |
| 19 | RestaurantTable Ă¢â‚¬â€ Reservation | Reservation.TableId, nullable | KhÄ‚Â´ng navigation/configuration | ChĂ¡Â»â€° scalar reference | SCALAR REFERENCE |
| 20 | Customer Ă¢â‚¬â€ Reservation | KhÄ‚Â´ng cÄ‚Â³ Reservation.CustomerId | KhÄ‚Â´ng navigation/configuration | KhÄ‚Â´ng xÄ‚Â¡c Ă„â€˜Ă¡Â»â€¹nh | NO RELATION |
| 21 | Employee Ă¢â‚¬â€ Attendance | Attendance.EmployeeId, required | HasOne<Employee>().WithMany().HasForeignKey(EmployeeId) | Employee 1 Ă¢â‚¬â€ Attendance 0..*; required; behavior mĂ¡ÂºÂ·c Ă„â€˜Ă¡Â»â€¹nh EF | RELATION |
| 22 | Branch Ă¢â‚¬â€ Attendance | Attendance.BranchId, required | HasOne<Branch>().WithMany().HasForeignKey(BranchId) | Branch 1 Ă¢â‚¬â€ Attendance 0..*; required; behavior mĂ¡ÂºÂ·c Ă„â€˜Ă¡Â»â€¹nh EF | RELATION |
| 23 | Employee Ă¢â‚¬â€ WorkSchedule | WorkSchedule.EmployeeId, required | HasOne<Employee>().WithMany().HasForeignKey(EmployeeId) | Employee 1 Ă¢â‚¬â€ Schedule 0..*; required; behavior mĂ¡ÂºÂ·c Ă„â€˜Ă¡Â»â€¹nh EF | RELATION |
| 24 | Branch Ă¢â‚¬â€ WorkSchedule | WorkSchedule.BranchId, nullable | HasOne<Branch>().WithMany().HasForeignKey(BranchId) | Branch 1 Ă¢â‚¬â€ Schedule 0..*; optional; behavior mĂ¡ÂºÂ·c Ă„â€˜Ă¡Â»â€¹nh EF | RELATION |
| 25 | Employee Ă¢â‚¬â€ Shift | Shift.EmployeeId, required | KhÄ‚Â´ng navigation/configuration | ChĂ¡Â»â€° scalar reference | SCALAR REFERENCE |
| 26 | Branch Ă¢â‚¬â€ Shift | Shift.BranchId, required | KhÄ‚Â´ng navigation/configuration | ChĂ¡Â»â€° scalar reference | SCALAR REFERENCE |
| 27 | Branch Ă¢â‚¬â€ BranchInventory | BranchInventory.BranchId, required | BranchInventory.Branch; HasOne<Branch>().WithMany().OnDelete(Restrict) | Branch 1 Ă¢â‚¬â€ Inventory row 0..*; required; Restrict | RELATION |
| 28 | InventoryItem Ă¢â‚¬â€ BranchInventory | BranchInventory.InventoryItemId, required | BranchInventory.InventoryItem; HasOne<InventoryItem>().WithMany().OnDelete(Restrict) | Item 1 Ă¢â‚¬â€ Branch inventory row 0..*; required; Restrict | RELATION |
| 29 | Branch Ă¢â‚¬â€ StockReceipt | StockReceipt.BranchId, required | StockReceipt.Branch; HasOne<Branch>().WithMany().OnDelete(Restrict) | Branch 1 Ă¢â‚¬â€ Receipt 0..*; required; Restrict | RELATION |
| 30 | StockReceipt Ă¢â‚¬â€ StockReceiptItem | StockReceiptItem.StockReceiptId, required | StockReceipt.Items; HasMany().WithOne().OnDelete(Restrict) | Receipt 1 Ă¢â‚¬â€ Item 0..*; required; Restrict; not composition | RELATION |
| 31 | InventoryItem Ă¢â‚¬â€ StockReceiptItem | StockReceiptItem.InventoryItemId, required | StockReceiptItem.InventoryItem; FK Restrict | Item 1 Ă¢â‚¬â€ Receipt item 0..*; required; Restrict | RELATION |
| 32 | Branch Ă¢â‚¬â€ StockIssue | StockIssue.BranchId, required | StockIssue.Branch; HasOne<Branch>().WithMany().OnDelete(Restrict) | Branch 1 Ă¢â‚¬â€ Issue 0..*; required; Restrict | RELATION |
| 33 | StockIssue Ă¢â‚¬â€ StockIssueItem | StockIssueItem.StockIssueId, required | StockIssue.Items; HasMany().WithOne().OnDelete(Restrict) | Issue 1 Ă¢â‚¬â€ Item 0..*; required; Restrict; not composition | RELATION |
| 34 | InventoryItem Ă¢â‚¬â€ StockIssueItem | StockIssueItem.InventoryItemId, required | StockIssueItem.InventoryItem; FK Restrict | Item 1 Ă¢â‚¬â€ Issue item 0..*; required; Restrict | RELATION |
| 35 | Branch Ă¢â‚¬â€ StockTransaction | StockTransaction.BranchId, required | StockTransaction.Branch; FK Restrict | Branch 1 Ă¢â‚¬â€ Transaction 0..*; required; Restrict | RELATION |
| 36 | InventoryItem Ă¢â‚¬â€ StockTransaction | StockTransaction.InventoryItemId, required | StockTransaction.InventoryItem; FK Restrict | Item 1 Ă¢â‚¬â€ Transaction 0..*; required; Restrict | RELATION |
| 37 | Branch Ă¢â‚¬â€ Expense | Expense.BranchId, required | HasOne<Branch>().WithMany().OnDelete(Restrict) | Branch 1 Ă¢â‚¬â€ Expense 0..*; required; Restrict | RELATION |
| 38 | StockReceipt Ă¢â‚¬â€ Expense | Expense.StockReceiptId, nullable; filtered unique index | Expense.StockReceipt; HasOne<StockReceipt>().WithOne().OnDelete(Restrict) | Receipt 0..1 Ă¢â‚¬â€ Expense 0..1; optional one-to-one; Restrict | RELATION |
| 39 | Branch Ă¢â‚¬â€ BusinessInsight | BusinessInsight.BranchId, nullable; index only | KhÄ‚Â´ng navigation/configuration | ChĂ¡Â»â€° scalar reference | SCALAR REFERENCE |
| 40 | Branch Ă¢â‚¬â€ ReceiptSettings | ReceiptSettings.BranchId, nullable; unique index only | KhÄ‚Â´ng navigation/configuration | ChĂ¡Â»â€° scalar reference | SCALAR REFERENCE |
| 41 | Branch Ă¢â‚¬â€ SystemSetting | SystemSetting.BranchId, nullable; unique composite index only | KhÄ‚Â´ng navigation/configuration | ChĂ¡Â»â€° scalar reference | SCALAR REFERENCE |
| 42 | Branch Ă¢â‚¬â€ Notification | Notification.BranchId, nullable; index only | KhÄ‚Â´ng navigation/configuration | ChĂ¡Â»â€° scalar reference | SCALAR REFERENCE |

Composition/association conclusion: chĂ¡Â»â€° OrderĂ¢â‚¬â€œOrderDetail vÄ‚Â  OrderRequestĂ¢â‚¬â€œOrderRequestItem cÄ‚Â³ collection navigation cÄ‚Â¹ng cascade delete rÄ‚Âµ rÄ‚Â ng, nÄ‚Âªn cÄ‚Â³ thĂ¡Â»Æ’ biĂ¡Â»Æ’u diĂ¡Â»â€¦n bĂ¡ÂºÂ±ng composition nĂ¡ÂºÂ¿u mĂ¡Â»Â¥c tiÄ‚Âªu diagram lÄ‚Â  vÄ‚Â²ng Ă„â€˜Ă¡Â»Âi aggregate. StockReceiptĂ¢â‚¬â€œStockReceiptItem vÄ‚Â  StockIssueĂ¢â‚¬â€œStockIssueItem cÄ‚Â³ collection navigation nhĂ†Â°ng delete behavior lÄ‚Â  Restrict; vÄ‚Â¬ vĂ¡ÂºÂ­y phĂ¡ÂºÂ£i vĂ¡ÂºÂ½ association thÄ‚Â´ng thĂ†Â°Ă¡Â»Âng, khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c gĂ¡ÂºÂ¯n composition. CÄ‚Â¡c quan hĂ¡Â»â€¡ cÄ‚Â²n lĂ¡ÂºÂ¡i lÄ‚Â  association thÄ‚Â´ng thĂ†Â°Ă¡Â»Âng.

## PHĂ¡ÂºÂ¦N C Ă¢â‚¬â€ Scalar References khÄ‚Â´ng phĂ¡ÂºÂ£i association

CÄ‚Â¡c trĂ†Â°Ă¡Â»Âng dĂ†Â°Ă¡Â»â€ºi Ă„â€˜Ä‚Â¢y cÄ‚Â³ thĂ¡Â»Æ’ cÄ‚Â³ Ä‚Â½ nghĂ„Â©a tham chiĂ¡ÂºÂ¿u nghiĂ¡Â»â€¡p vĂ¡Â»Â¥ hoĂ¡ÂºÂ·c branch scope, nhĂ†Â°ng khÄ‚Â´ng cÄ‚Â³ navigation/FK constraint/fluent relationship tĂ†Â°Ă†Â¡ng Ă¡Â»Â©ng trong audit hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i:

| Entity | Scalar reference | Ă„ÂĂ¡Â»â€˜i tĂ†Â°Ă¡Â»Â£ng Ă„â€˜Ă†Â°Ă¡Â»Â£c gĂ¡Â»Â£i Ä‚Â½ | KhÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c kĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n thÄ‚Â nh |
|---|---|---|---|
| Order | BranchId: Guid? | Branch | BranchĂ¢â‚¬â€œOrder association |
| Reservation | BranchId: Guid? | Branch | BranchĂ¢â‚¬â€œReservation association |
| Reservation | TableId: Guid? | RestaurantTable | RestaurantTableĂ¢â‚¬â€œReservation association |
| Shift | EmployeeId: Guid | Employee | EmployeeĂ¢â‚¬â€œShift association |
| Shift | BranchId: Guid | Branch | BranchĂ¢â‚¬â€œShift association |
| BusinessInsight | BranchId: Guid? | Branch | BranchĂ¢â‚¬â€œBusinessInsight association |
| ReceiptSettings | BranchId: Guid? | Branch | BranchĂ¢â‚¬â€œReceiptSettings association |
| SystemSetting | BranchId: Guid? | Branch | BranchĂ¢â‚¬â€œSystemSetting association |
| Notification | BranchId: Guid? | Branch | BranchĂ¢â‚¬â€œNotification association |
| StockTransaction | ReferenceType: string + ReferenceId: Guid | StockReceipt/StockIssue/StockAdjustment | polymorphic EF association |
| StockTransaction | CreatedBy: Guid | User/Employee | Employee/User association |
| Expense | CreatedBy: Guid? | User/Employee | Employee/User association |
| StockReceipt | CreatedBy: Guid; ConfirmedBy: Guid? | User/Employee | Employee/User association |
| StockIssue | CreatedBy: Guid; ConfirmedBy: Guid? | User/Employee | Employee/User association |
| Notification | TargetUserId: Guid? | User/Employee | User/Employee association |
| Order | CreatedBy: string? | User/Employee | User/Employee association |
| Order | TableName: string? | RestaurantTable | RestaurantTableĂ¢â‚¬â€œOrder association |
| Reservation | BranchName/TableName: string? | Branch/RestaurantTable | association |
| Employee | BranchName: string? | Branch | additional relation beyond BranchId |
| Product | ToppingsJson: string? | Topping collection | ProductĂ¢â‚¬â€œTopping many-to-many |
| OrderRequestItem | ProductId: Guid? | Product | ProductĂ¢â‚¬â€œOrderRequestItem association |

Branch-scoped entities phĂ¡ÂºÂ£i vĂ¡ÂºÂ«n Ă„â€˜Ă†Â°Ă¡Â»Â£c ghi chÄ‚Âº trong thesis khi cĂ¡ÂºÂ§n mÄ‚Â´ tĂ¡ÂºÂ£ isolation, nhĂ†Â°ng ghi chÄ‚Âº Ă„â€˜Ä‚Â³ khÄ‚Â´ng biĂ¡ÂºÂ¿n scalar field thÄ‚Â nh association trong Domain Class Diagram. StockTransaction.ReferenceType/ReferenceId lÄ‚Â  mĂ¡Â»â„¢t convention tham chiĂ¡ÂºÂ¿u Ă„â€˜a hÄ‚Â¬nh, khÄ‚Â´ng phĂ¡ÂºÂ£i FK Ă„â€˜Ä‚Â£ Ă„â€˜Ă†Â°Ă¡Â»Â£c EF xÄ‚Â¡c minh.

## PHĂ¡ÂºÂ¦N D Ă¢â‚¬â€ CÄ‚Â¡c quan hĂ¡Â»â€¡ TUYĂ¡Â»â€ T Ă„ÂĂ¡Â»ÂI KHÄ‚â€NG Ă„ÂĂ†Â¯Ă¡Â»Â¢C VĂ¡ÂºÂ¼

KhÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c vĂ¡ÂºÂ½ cÄ‚Â¡c quan hĂ¡Â»â€¡ sau trong Domain Class Diagram hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i:

1. Branch Ă¢â‚¬â€ Area: Area khÄ‚Â´ng cÄ‚Â³ BranchId, navigation hoĂ¡ÂºÂ·c EF configuration.
2. Area Ă¢â‚¬â€ RestaurantTable: RestaurantTable chĂ¡Â»â€° lĂ†Â°u AreaName, khÄ‚Â´ng cÄ‚Â³ AreaId/FK/navigation.
3. Branch Ă¢â‚¬â€ Product: Product khÄ‚Â´ng cÄ‚Â³ BranchId vÄ‚Â  khÄ‚Â´ng cÄ‚Â³ relation config.
4. Product Ă¢â‚¬â€ Topping: ToppingsJson lÄ‚Â  scalar JSON; khÄ‚Â´ng cÄ‚Â³ join entity hay many-to-many config.
5. RestaurantTable Ă¢â‚¬â€ Order: Order khÄ‚Â´ng cÄ‚Â³ TableId, chĂ¡Â»â€° cÄ‚Â³ TableName.
6. OrderDetail Ă¢â‚¬â€ OrderRequestItem: OrderRequestItem khÄ‚Â´ng cÄ‚Â³ OrderDetailId/navigation.
7. Customer Ă¢â‚¬â€ Reservation: Reservation khÄ‚Â´ng cÄ‚Â³ CustomerId/navigation.
8. Employee Ă¢â‚¬â€ Shift vÄ‚Â  Branch Ă¢â‚¬â€ Shift: Shift chĂ¡Â»â€° cÄ‚Â³ scalar EmployeeId/BranchId, khÄ‚Â´ng cÄ‚Â³ navigation/configuration.
9. Branch Ă¢â‚¬â€ Order, Branch Ă¢â‚¬â€ Reservation, Branch Ă¢â‚¬â€ BusinessInsight, Branch Ă¢â‚¬â€ ReceiptSettings, Branch Ă¢â‚¬â€ SystemSetting, Branch Ă¢â‚¬â€ Notification: cÄ‚Â¡c trĂ†Â°Ă¡Â»Âng BranchId chĂ¡Â»â€° lÄ‚Â  scalar/index evidence, khÄ‚Â´ng phĂ¡ÂºÂ£i EF association.
10. Branch Ă¢â‚¬â€ Shift: scalar reference, khÄ‚Â´ng phĂ¡ÂºÂ£i EF association.
11. Employee/User Ă¢â‚¬â€ CreatedBy, ConfirmedBy, TargetUserId: chĂ¡Â»â€° lÄ‚Â  Guid scalar; khÄ‚Â´ng cÄ‚Â³ FK/navigation.
12. StockTransaction Ă¢â‚¬â€ StockReceipt/StockIssue/StockAdjustment: ReferenceType + ReferenceId lÄ‚Â  convention, khÄ‚Â´ng cÄ‚Â³ FK/navigation.
13. Product Ă¢â‚¬â€ OrderRequestItem: ProductId lÄ‚Â  scalar nullable, khÄ‚Â´ng cÄ‚Â³ EF relationship.
14. Restaurant/Category/Invoice/removed staff-finance module/Payment/OrderItem/ProductTopping/OrderDetailTopping: khÄ‚Â´ng phĂ¡ÂºÂ£i cÄ‚Â¡c entity hiĂ¡Â»â€¡n cÄ‚Â³ trong backend audit nÄ‚Â y; khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c tĂ¡Â»Â± thÄ‚Âªm vÄ‚Â o diagram.
15. CÄ‚Â¡c quan hĂ¡Â»â€¡ many-to-many hoĂ¡ÂºÂ·c composition khÄ‚Â¡c khÄ‚Â´ng cÄ‚Â³ trong Relationship Matrix: khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c suy diĂ¡Â»â€¦n tĂ¡Â»Â« tÄ‚Âªn property, JSON, tÄ‚Âªn hiĂ¡Â»Æ’n thĂ¡Â»â€¹, endpoint hoĂ¡ÂºÂ·c nghiĂ¡Â»â€¡p vĂ¡Â»Â¥ dĂ¡Â»Â± kiĂ¡ÂºÂ¿n.

Domain Class Diagram vÄ‚Â  ERD khÄ‚Â´ng Ă„â€˜Ă¡Â»â€œng nhĂ¡ÂºÂ¥t: ERD cÄ‚Â³ thĂ¡Â»Æ’ nhĂ¡ÂºÂ¥n mĂ¡ÂºÂ¡nh PK/FK/index/unique constraint; Domain Class Diagram phĂ¡ÂºÂ£i Ă†Â°u tiÄ‚Âªn entity, thuĂ¡Â»â„¢c tÄ‚Â­nh domain, navigation vÄ‚Â  association Ă„â€˜Ä‚Â£ Ă„â€˜Ă†Â°Ă¡Â»Â£c code xÄ‚Â¡c nhĂ¡ÂºÂ­n. KhÄ‚Â´ng Ă„â€˜Ă†Â°a index hoĂ¡ÂºÂ·c tÄ‚Âªn cĂ¡Â»â„¢t scalar vÄ‚Â o UML nhĂ†Â° thĂ¡Â»Æ’ Ă„â€˜Ä‚Â³ lÄ‚Â  quan hĂ¡Â»â€¡.

## PHĂ¡ÂºÂ¦N E Ă¢â‚¬â€ PlantUML Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t hoÄ‚Â n chĂ¡Â»â€°nh cho Domain Class Diagram

Ă„ÂoĂ¡ÂºÂ¡n dĂ†Â°Ă¡Â»â€ºi Ă„â€˜Ä‚Â¢y chĂ¡Â»â€° dÄ‚Â¹ng cÄ‚Â¡c association RELATION trong PhĂ¡ÂºÂ§n B. CÄ‚Â¡c entity khÄ‚Â´ng cÄ‚Â³ relation vĂ¡ÂºÂ«n Ă„â€˜Ă†Â°Ă¡Â»Â£c khai bÄ‚Â¡o Ă„â€˜Ă¡Â»Æ’ phĂ¡ÂºÂ£n Ä‚Â¡nh Ă„â€˜Ă¡ÂºÂ§y Ă„â€˜Ă¡Â»Â§ 29 entity hiĂ¡Â»â€¡n cÄ‚Â³. CÄ‚Â¡c field Ă„â€˜Ă†Â°Ă¡Â»Â£c chĂ¡Â»Ân lÄ‚Â  field domain/Ă„â€˜Ă¡Â»â€¹nh danh/quan trĂ¡Â»Âng, khÄ‚Â´ng phĂ¡ÂºÂ£i toÄ‚Â n bĂ¡Â»â„¢ cĂ¡Â»â„¢t persistence.

    @startuml
    hide methods
    skinparam classAttributeIconSize 0
    skinparam linetype ortho

    class Area {
      +Id : Guid
      +Name : string
      +DisplayOrder : int
    }
    class Attendance {
      +Id : Guid
      +EmployeeId : Guid
      +BranchId : Guid
      +CheckInTime : DateTime
      +CheckOutTime : DateTime?
    }
    class Branch {
      +Id : Guid
      +Name : string
      +IsActive : bool
    }
    class BranchInventory {
      +Id : Guid
      +BranchId : Guid
      +InventoryItemId : Guid
      +CurrentQuantity : decimal
      +MinimumStock : decimal
    }
    class BusinessInsight {
      +Id : Guid
      +BranchId : Guid?
      +Type : string
      +Severity : string
      +Status : string
    }
    class Customer {
      +Id : Guid
      +FullName : string?
      +PhoneNumber : string?
      +LoyaltyPoints : int
    }
    class Employee {
      +Id : Guid
      +FullName : string
      +BranchId : Guid?
      +Role : string
    }
    class Expense {
      +Id : Guid
      +BranchId : Guid
      +Amount : decimal
      +StockReceiptId : Guid?
    }
    class InventoryItem {
      +Id : Guid
      +Name : string
      +UnitCode : string
      +IsActive : bool
    }
    class LoyaltyTransaction {
      +Id : Guid
      +CustomerId : Guid
      +OrderId : Guid?
      +Points : int
      +Type : string
    }
    class Notification {
      +Id : Guid
      +Type : string
      +TargetRole : string?
      +TargetUserId : Guid?
      +BranchId : Guid?
    }
    class Order {
      +Id : Guid
      +CustomerId : Guid?
      +BranchId : Guid?
      +TotalAmount : decimal
      +Status : string
      +Details : List<OrderDetail>
    }
    class OrderDetail {
      +Id : Guid
      +ProductId : Guid?
      +ToppingId : Guid?
      +Quantity : int
      +UnitPrice : decimal
    }
    class OrderRequest {
      +Id : Guid
      +OrderId : Guid
      +Status : string
      +Items : List<OrderRequestItem>
    }
    class OrderRequestItem {
      +Id : Guid
      +OrderRequestId : Guid
      +ProductId : Guid?
      +Quantity : int
    }
    class Product {
      +Id : Guid
      +Name : string?
      +Price : decimal
      +ToppingsJson : string?
    }
    class Promotion {
      +Id : Guid
      +Name : string
      +RequiredPoints : int
      +DiscountValue : decimal
    }
    class ReceiptSettings {
      +Id : Guid
      +BranchId : Guid?
      +PaperWidth : int
      +UpdatedAt : DateTime
    }
    class Reservation {
      +Id : Guid
      +BranchId : Guid?
      +TableId : Guid?
      +ReservationTime : DateTime
      +Status : string
    }
    class RestaurantTable {
      +Id : Guid
      +Name : string
      +BranchId : Guid?
      +QrToken : string
      +Status : string
    }
    class Shift {
      +Id : Guid
      +EmployeeId : Guid
      +BranchId : Guid
      +StartTime : DateTime
      +Status : string
    }
    class StockIssue {
      +Id : Guid
      +BranchId : Guid
      +Status : StockDocumentStatus
      +Items : ICollection<StockIssueItem>
    }
    class StockIssueItem {
      +Id : Guid
      +StockIssueId : Guid
      +InventoryItemId : Guid
      +Quantity : decimal
    }
    class StockReceipt {
      +Id : Guid
      +BranchId : Guid
      +Status : StockDocumentStatus
      +TotalAmount : decimal
      +Items : ICollection<StockReceiptItem>
    }
    class StockReceiptItem {
      +Id : Guid
      +StockReceiptId : Guid
      +InventoryItemId : Guid
      +Quantity : decimal
      +UnitPrice : decimal
    }
    class StockTransaction {
      +Id : Guid
      +BranchId : Guid
      +InventoryItemId : Guid
      +Type : StockTransactionType
      +ReferenceId : Guid
    }
    class SystemSetting {
      +Id : Guid
      +BranchId : Guid?
      +Key : string
      +Value : string
    }
    class Topping {
      +Id : Guid
      +Name : string
      +Price : decimal
      +ToppingType : string?
    }
    class WorkSchedule {
      +Id : Guid
      +EmployeeId : Guid
      +BranchId : Guid?
      +Date : DateTime
      +ShiftName : string?
    }

    Branch "1" -- "0..*" Employee : BranchId
    Branch "1" -- "0..*" RestaurantTable : BranchId
    Customer "0..1" -- "0..*" Order : CustomerId
    Order "1" *-- "0..*" OrderDetail : shadow OrderId / Cascade
    Product "0..1" -- "0..*" OrderDetail : ProductId
    Topping "0..1" -- "0..*" OrderDetail : ToppingId
    Order "1" -- "0..*" OrderRequest : OrderId
    OrderRequest "1" *-- "0..*" OrderRequestItem : OrderRequestId / Cascade
    Customer "1" -- "0..*" LoyaltyTransaction : CustomerId
    Order "0..1" -- "0..*" LoyaltyTransaction : OrderId
    Employee "1" -- "0..*" Attendance : EmployeeId
    Branch "1" -- "0..*" Attendance : BranchId
    Employee "1" -- "0..*" WorkSchedule : EmployeeId
    Branch "0..1" -- "0..*" WorkSchedule : BranchId
    Branch "1" -- "0..*" BranchInventory : BranchId / Restrict
    InventoryItem "1" -- "0..*" BranchInventory : InventoryItemId / Restrict
    Branch "1" -- "0..*" StockReceipt : BranchId / Restrict
    StockReceipt "1" -- "0..*" StockReceiptItem : StockReceiptId / Restrict
    InventoryItem "1" -- "0..*" StockReceiptItem : InventoryItemId / Restrict
    Branch "1" -- "0..*" StockIssue : BranchId / Restrict
    StockIssue "1" -- "0..*" StockIssueItem : StockIssueId / Restrict
    InventoryItem "1" -- "0..*" StockIssueItem : InventoryItemId / Restrict
    Branch "1" -- "0..*" StockTransaction : BranchId / Restrict
    InventoryItem "1" -- "0..*" StockTransaction : InventoryItemId / Restrict
    Branch "1" -- "0..*" Expense : BranchId / Restrict
    StockReceipt "0..1" -- "0..1" Expense : StockReceiptId / Restrict

    note right of Order
      BranchId and TableName are scalar data.
      No Branch-Order or Table-Order edge.
    end note
    note right of StockTransaction
      ReferenceType + ReferenceId are
      scalar polymorphic convention only.
    end note
    @enduml

PlantUML nÄ‚Â y khÄ‚Â´ng vĂ¡ÂºÂ½ cÄ‚Â¡c scalar references cĂ¡Â»Â§a PhĂ¡ÂºÂ§n C vÄ‚Â  khÄ‚Â´ng vĂ¡ÂºÂ½ cÄ‚Â¡c quan hĂ¡Â»â€¡ bĂ¡Â»â€¹ cĂ¡ÂºÂ¥m Ă¡Â»Å¸ PhĂ¡ÂºÂ§n D. TrĂ†Â°Ă¡Â»â€ºc khi Ă„â€˜Ă†Â°a vÄ‚Â o thesis, cĂ¡ÂºÂ§n giĂ¡Â»Â¯ nguyÄ‚Âªn nhÄ‚Â£n FK, optionality vÄ‚Â  delete behavior trong chÄ‚Âº thÄ‚Â­ch/matrix nĂ¡ÂºÂ¿u diagram Ă„â€˜Ă†Â°Ă¡Â»Â£c giĂ¡ÂºÂ£n lĂ†Â°Ă¡Â»Â£c vĂ¡Â»Â layout.

# ERD AUDIT Ă¢â‚¬â€ HÄ‚Å’NH 3.10

PhĂ¡ÂºÂ¡m vi bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng: ApplicationDbContext.cs, ApplicationDbContextModelSnapshot.cs, cÄ‚Â¡c migration/designer hiĂ¡Â»â€¡n cÄ‚Â³ vÄ‚Â  entity classes. KhÄ‚Â´ng tÄ‚Â¬m thĂ¡ÂºÂ¥y file database local .db, .sqlite hoĂ¡ÂºÂ·c .sql trong workspace; do Ă„â€˜Ä‚Â³ schema production/runtime chĂ†Â°a Ă„â€˜Ă†Â°Ă¡Â»Â£c query trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p. Schema dĂ†Â°Ă¡Â»â€ºi Ă„â€˜Ä‚Â¢y lÄ‚Â  schema EF/migration hiĂ¡Â»â€¡n hÄ‚Â nh, vĂ¡Â»â€ºi ModelSnapshot mĂ¡Â»â€ºi nhĂ¡ÂºÂ¥t lÄ‚Â m bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng canonical. KhÄ‚Â´ng suy FK chĂ¡Â»â€° tĂ¡Â»Â« hĂ¡ÂºÂ­u tĂ¡Â»â€˜ Id.

## PHĂ¡ÂºÂ¦N A Ă¢â‚¬â€ Danh sÄ‚Â¡ch bĂ¡ÂºÂ£ng vÄ‚Â  columns thĂ¡Â»Â±c tĂ¡ÂºÂ¿

DbContext hiĂ¡Â»â€¡n cÄ‚Â³ 29 DbSet/bĂ¡ÂºÂ£ng. TĂ¡ÂºÂ¥t cĂ¡ÂºÂ£ cÄ‚Â³ PK Ă„â€˜Ă†Â¡n Id theo ModelSnapshot; khÄ‚Â´ng cÄ‚Â³ composite PK Ă„â€˜Ă†Â°Ă¡Â»Â£c xÄ‚Â¡c minh.

| BĂ¡ÂºÂ£ng | Entity | Columns vÄ‚Â  nullability quan trĂ¡Â»Âng |
|---|---|---|
| Areas | Area | Id Guid PK; Name string NOT NULL; DisplayOrder int NOT NULL; CreatedAt DateTime NOT NULL |
| Attendances | Attendance | Id PK; EmployeeId Guid NOT NULL; EmployeeName string NULL; CheckInTime DateTime NOT NULL; CheckOutTime DateTime NULL; BranchId Guid NOT NULL; Status/Note/ImagePath string NULL |
| Branches | Branch | Id PK; Name string NOT NULL; Address/PhoneNumber/BankName/AccountNumber/AccountHolder/ImageUrl/TaxCode/RepresentativeName/RepresentativeEmail/Industry/BusinessType string NULL; IsMain/IsActive bool NOT NULL; CreatedAt DateTime NOT NULL |
| BranchInventories | BranchInventory | Id PK; BranchId/InventoryItemId Guid NOT NULL; CurrentQuantity/MinimumStock decimal(18,3) NOT NULL; UpdatedAtUtc DateTime NOT NULL |
| BusinessInsights | BusinessInsight | Id PK; BranchId Guid NULL; Type/Severity/Title/Summary/EvidenceJson/Status/DeduplicationKey string NOT NULL; AiExplanation/AiRecommendation string NULL; period/detected/created timestamps |
| Customers | Customer | Id PK; FullName/PhoneNumber/Email/Address/Birthday/Gender/CustomerGroup/Password/LastOrderDate nullable; TotalSpending decimal; TotalOrders/LoyaltyPoints int; IsActive bool; CreatedAt/UpdatedAt |
| Employees | Employee | Id PK; EmployeeCode/PhoneNumber/Position/Department/BranchId/BranchName/CitizenId/BirthDate/Gender/Address/Note/Username/Password nullable; EmployeeType/FullName/BasicSalary/StartDate/IsActive/Role required |
| Expenses | Expense | Id PK; BranchId Guid NOT NULL; Description/Amount/ExpenseDate/Category/PaymentMethod/Note required; CreatedBy/StockReceiptId/UpdatedAt nullable |
| InventoryItems | InventoryItem | Id PK; Name/NormalizedName/UnitCode required; IsActive; CreatedAtUtc; UpdatedAtUtc nullable |
| LoyaltyTransactions | LoyaltyTransaction | Id PK; CustomerId Guid NOT NULL; OrderId Guid NULL; Type/Points/BalanceAfter/CreatedAt required; Description NULL |
| Notifications | Notification | Id PK; Type/Title/Message required; EntityType/EntityId/Route/TargetRole/TargetUserId/BranchId nullable; IsRead/CreatedAt required |
| Orders | Order | Id PK; CustomerId/BranchId and invoice/customer/snapshot/display fields nullable; financial/status fields required or nullable exactly as current entity; no TableId |
| OrderDetails | OrderDetail | Id PK; shadow OrderId Guid NOT NULL; ProductId/ToppingId Guid NULL; ProductName/Quantity/SentQuantity/UnitPrice required; Options NULL |
| OrderRequests | OrderRequest | Id PK; OrderId Guid NOT NULL; RequestNumber/Status/CreatedAt required; AcceptedAt/PreparingAt/CompletedAt/ProcessedBy/Note nullable |
| OrderRequestItems | OrderRequestItem | Id PK; OrderRequestId Guid NOT NULL; ProductId Guid NULL; ProductName/Quantity required; Note/Options nullable |
| Products | Product | Id PK; Code/Name/Category/Group/ImageUrl/SizesJson/ToppingsJson nullable; Price/CostPrice/IsActive/AvailabilityStatus/CreatedAt required |
| Promotions | Promotion | Id PK; Name/RequiredPoints/DiscountValue/PromotionType/IsActive/CreatedAt required; Description/EndDate nullable |
| ReceiptSettings | ReceiptSettings | Id PK; BranchId Guid NULL; display booleans, PaperWidth, FontSize, ThankYouText, FontFamily, UpdatedAt |
| Reservations | Reservation | Id PK; CustomerName/CustomerPhone/BranchId/BranchName/Note/TableId/TableName nullable; ReservationTime/NumberOfGuests/Status/CreatedAt required |
| Tables | RestaurantTable | Id PK; Name/AreaName/SeatCount/IsActive/Status/QrToken/CreatedAt required; Description/QrCodeUrl/BranchId/BranchName nullable |
| Shifts | Shift | Id PK; EmployeeId/BranchId Guid NOT NULL; EmployeeName/StartTime/StartingCash/TotalRevenue/CashRevenue/TransferRevenue/Status required; EndTime/EndingCash/BranchName/Note nullable |
| StockIssues | StockIssue | Id PK; BranchId/CreatedBy required; Reason/Status/CreatedAtUtc required; Note/ConfirmedBy/ConfirmedAtUtc/IdempotencyKey nullable |
| StockIssueItems | StockIssueItem | Id PK; StockIssueId/InventoryItemId required; Quantity decimal(18,3) required |
| StockReceipts | StockReceipt | Id PK; BranchId/CreatedBy/TotalAmount/Status/CreatedAtUtc required; SupplierName/PurchaseDate/Note/ConfirmedBy/ConfirmedAtUtc/IdempotencyKey nullable |
| StockReceiptItems | StockReceiptItem | Id PK; StockReceiptId/InventoryItemId/Quantity/UnitPrice required; Quantity decimal(18,3), UnitPrice decimal(18,2) |
| StockTransactions | StockTransaction | Id PK; BranchId/InventoryItemId/Type/Quantity/BeforeQuantity/AfterQuantity/ReferenceType/ReferenceId/CreatedBy/CreatedAtUtc required; Note nullable |
| SystemSettings | SystemSetting | Id PK; BranchId Guid NULL; Key/Value/Type/UpdatedAt required |
| Toppings | Topping | Id PK; Name/Price/CostPrice/IsActive/CreatedAt required; ToppingType/Category/Description/ImageUrl nullable |
| WorkSchedules | WorkSchedule | Id PK; EmployeeId required; BranchId nullable; EmployeeName/ShiftName/StartTime/EndTime/BranchName nullable; Date required |

KiĂ¡Â»Æ’u database cĂ¡Â»Â¥ thĂ¡Â»Æ’ chĂ¡Â»â€° Ă„â€˜Ă†Â°Ă¡Â»Â£c xÄ‚Â¡c Ă„â€˜Ă¡Â»â€¹nh khi snapshot/configuration cÄ‚Â³ evidence; decimal precision, enum string conversion, length, checks vÄ‚Â  indexes Ă„â€˜Ă†Â°Ă¡Â»Â£c ghi tĂ¡ÂºÂ¡i PhĂ¡ÂºÂ§n E.

## PHĂ¡ÂºÂ¦N B Ă¢â‚¬â€ Danh sÄ‚Â¡ch PK / FK / Shadow FK

| Child table | FK column | Parent table | Parent PK | Nullable | FK constraint | DeleteBehavior | Source | KĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n |
|---|---|---|---|---|---|---|---|---|
| Employees | BranchId | Branches | Id | Yes | Yes | default/ClientSetNull | Fluent + snapshot | REAL FK |
| Tables | BranchId | Branches | Id | Yes | Yes | default/ClientSetNull | Fluent + snapshot | REAL FK |
| Attendances | EmployeeId, BranchId | Employees, Branches | Id | No | Yes | Cascade | Fluent + snapshot | REAL FK |
| WorkSchedules | EmployeeId | Employees | Id | No | Yes | Cascade | Fluent + snapshot | REAL FK |
| WorkSchedules | BranchId | Branches | Id | Yes | Yes | default/ClientSetNull | Fluent + snapshot | REAL FK |
| Orders | CustomerId | Customers | Id | Yes | Yes | default/ClientSetNull | Fluent + snapshot | REAL FK |
| OrderDetails | OrderId | Orders | Id | No | Yes | Cascade | Fluent shadow FK + snapshot | SHADOW FK |
| OrderDetails | ProductId, ToppingId | Products, Toppings | Id | Yes | Yes | default/ClientSetNull | Fluent + snapshot | REAL FK |
| OrderRequests | OrderId | Orders | Id | No | Yes | Cascade | Fluent + snapshot | REAL FK |
| OrderRequestItems | OrderRequestId | OrderRequests | Id | No | Yes | Cascade | Fluent + snapshot | REAL FK |
| LoyaltyTransactions | CustomerId | Customers | Id | No | Yes | Cascade | Fluent + snapshot | REAL FK |
| LoyaltyTransactions | OrderId | Orders | Id | Yes | Yes | default/ClientSetNull | Fluent + snapshot | REAL FK |
| BranchInventories | BranchId, InventoryItemId | Branches, InventoryItems | Id | No | Yes | Restrict | Fluent + snapshot | REAL FK |
| StockReceipts | BranchId | Branches | Id | No | Yes | Restrict | Fluent + snapshot | REAL FK |
| StockReceiptItems | StockReceiptId, InventoryItemId | StockReceipts, InventoryItems | Id | No | Yes | Restrict | Fluent + snapshot | REAL FK |
| StockIssues | BranchId | Branches | Id | No | Yes | Restrict | Fluent + snapshot | REAL FK |
| StockIssueItems | StockIssueId, InventoryItemId | StockIssues, InventoryItems | Id | No | Yes | Restrict | Fluent + snapshot | REAL FK |
| StockTransactions | BranchId, InventoryItemId | Branches, InventoryItems | Id | No | Yes | Restrict | Fluent + snapshot | REAL FK |
| Expenses | BranchId | Branches | Id | No | Yes | Restrict | Fluent + snapshot | REAL FK |
| Expenses | StockReceiptId | StockReceipts | Id | Yes | Yes | Restrict | Fluent one-to-one + snapshot | REAL FK |

OrderDetail khÄ‚Â´ng khai bÄ‚Â¡o OrderId trong C#; Order.Details Ă„â€˜Ă†Â°Ă¡Â»Â£c cĂ¡ÂºÂ¥u hÄ‚Â¬nh bĂ¡ÂºÂ±ng HasForeignKey("OrderId"), snapshot vÄ‚Â  migration tĂ¡ÂºÂ¡o column/index/FK nÄ‚Â y. VÄ‚Â¬ vĂ¡ÂºÂ­y phĂ¡ÂºÂ£i ghi chÄ‚Â­nh xÄ‚Â¡c OrderDetail.OrderId = SHADOW FK, khÄ‚Â´ng phĂ¡ÂºÂ£i property C#.

## PHĂ¡ÂºÂ¦N C Ă¢â‚¬â€ Relationship Matrix

| Child table | FK column | Parent table | Cardinality | KĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n |
|---|---|---|---|---|
| Employees | BranchId | Branches | Branch 1 Ă¢â‚¬â€ Employee 0..*, nullable | CORRECT |
| Tables | BranchId | Branches | Branch 1 Ă¢â‚¬â€ Table 0..*, nullable | CORRECT |
| Attendances | EmployeeId, BranchId | Employees, Branches | mĂ¡Â»â€”i FK required; 1 Ă¢â‚¬â€ 0..* | CORRECT |
| WorkSchedules | EmployeeId, BranchId | Employees, Branches | Employee required; Branch nullable | CORRECT |
| Orders | CustomerId | Customers | Customer 0..1 Ă¢â‚¬â€ Order 0..* | CORRECT |
| OrderDetails | shadow OrderId | Orders | Order 1 Ă¢â‚¬â€ Detail 0..* | CORRECT |
| OrderDetails | ProductId, ToppingId | Products, Toppings | mĂ¡Â»â€”i FK nullable; principal 0..1 Ă¢â‚¬â€ child 0..* | CORRECT |
| OrderRequests | OrderId | Orders | Order 1 Ă¢â‚¬â€ Request 0..* | CORRECT |
| OrderRequestItems | OrderRequestId | OrderRequests | Request 1 Ă¢â‚¬â€ Item 0..* | CORRECT |
| LoyaltyTransactions | CustomerId, OrderId | Customers, Orders | Customer required; Order nullable | CORRECT |
| BranchInventories | BranchId, InventoryItemId | Branches, InventoryItems | required; 1 Ă¢â‚¬â€ 0..* | CORRECT |
| StockReceipts/Items | BranchId, StockReceiptId, InventoryItemId | Branches, StockReceipts, InventoryItems | required; Restrict | CORRECT |
| StockIssues/Items | BranchId, StockIssueId, InventoryItemId | Branches, StockIssues, InventoryItems | required; Restrict | CORRECT |
| StockTransactions | BranchId, InventoryItemId | Branches, InventoryItems | required; Restrict | CORRECT |
| Expenses | BranchId, StockReceiptId | Branches, StockReceipts | Branch required; receipt nullable unique one-to-one | CORRECT |

KhÄ‚Â´ng cÄ‚Â³ quan hĂ¡Â»â€¡ EF Ă„â€˜Ă†Â°Ă¡Â»Â£c xÄ‚Â¡c nhĂ¡ÂºÂ­n cho BranchĂ¢â‚¬â€œArea, AreaĂ¢â‚¬â€œTable, BranchĂ¢â‚¬â€œProduct, ProductĂ¢â‚¬â€œTopping, TableĂ¢â‚¬â€œOrder, ProductĂ¢â‚¬â€œOrderRequestItem, OrderDetailĂ¢â‚¬â€œOrderRequestItem, CustomerĂ¢â‚¬â€œReservation, EmployeeĂ¢â‚¬â€œShift, BranchĂ¢â‚¬â€œShift, BranchĂ¢â‚¬â€œBusinessInsight, BranchĂ¢â‚¬â€œReceiptSettings, BranchĂ¢â‚¬â€œSystemSetting hoĂ¡ÂºÂ·c BranchĂ¢â‚¬â€œNotification.

## PHĂ¡ÂºÂ¦N D Ă¢â‚¬â€ Scalar ID fields KHÄ‚â€NG phĂ¡ÂºÂ£i FK

| Table | Column | CÄ‚Â³ thĂ¡Â»Æ’ tham chiĂ¡ÂºÂ¿u tĂ¡Â»â€ºi | FK constraint | KĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n |
|---|---|---|---|---|
| Orders | BranchId | Branches | No | SCALAR ID ONLY |
| Reservations | BranchId, TableId | Branches, Tables | No | SCALAR ID ONLY |
| OrderRequestItems | ProductId | Products | No | SCALAR ID ONLY |
| Shifts | EmployeeId, BranchId | Employees, Branches | No | SCALAR ID ONLY |
| BusinessInsights | BranchId | Branches | No | SCALAR ID ONLY |
| ReceiptSettings | BranchId | Branches | No | SCALAR ID ONLY |
| SystemSettings | BranchId | Branches | No | SCALAR ID ONLY |
| Notifications | BranchId, TargetUserId | Branch/User | No | SCALAR ID ONLY |
| StockTransactions | ReferenceType + ReferenceId | Receipt/Issue/Adjustment | No polymorphic FK | SCALAR REFERENCE ONLY |
| Expenses/StockReceipts/StockIssues | CreatedBy/ConfirmedBy | User/Employee | No | SCALAR ID ONLY |

Order.TableName, OrderDetail.ProductName, OrderRequestItem.ProductName, RestaurantTable.AreaName vÄ‚Â  Product.ToppingsJson lÄ‚Â  snapshot/denormalized data; khÄ‚Â´ng thay thĂ¡ÂºÂ¿ FK vÄ‚Â  khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c ghi <<FK>>.

## PHĂ¡ÂºÂ¦N E Ă¢â‚¬â€ Unique constraints vÄ‚Â  indexes quan trĂ¡Â»Âng

| Table | Index/constraint | Evidence |
|---|---|---|
| BranchInventories | BranchId + InventoryItemId unique | HasIndex(...).IsUnique |
| LoyaltyTransactions | OrderId + Type unique khi OrderId IS NOT NULL | filtered unique |
| ReceiptSettings | BranchId unique | unique index, khÄ‚Â´ng phĂ¡ÂºÂ£i FK |
| SystemSettings | BranchId + Key unique | composite unique |
| StockReceipts | BranchId + IdempotencyKey unique khi key non-null | filtered unique |
| StockIssues | BranchId + IdempotencyKey unique khi key non-null | filtered unique |
| BusinessInsights | DeduplicationKey unique | unique index |
| Customers | PhoneNumber unique | unique index |
| Tables | QrToken unique | unique index |
| InventoryItems | NormalizedName unique khi IsActive = TRUE | filtered unique |
| Orders | BranchId + CreatedAt + Status | non-unique query index |
| Expenses | BranchId + ExpenseDate | non-unique query index |
| Notifications | TargetRole + TargetUserId + CreatedAt | non-unique query index |
| StockTransactions | ReferenceType + ReferenceId; BranchId + InventoryItemId + CreatedAtUtc | non-unique indexes |
| Receipt/Issue items | parent id + InventoryItemId | non-unique indexes |

## PHĂ¡ÂºÂ¦N F Ă¢â‚¬â€ CÄ‚Â¡c lĂ¡Â»â€”i cĂ¡Â»Â§a PlantUML ERD hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i

CÄ‚Â¡c quan hĂ¡Â»â€¡ Ă„â€˜ang vĂ¡ÂºÂ½ trong ERD hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i tĂ¡Â»Â« BranchĂ¢â‚¬â€œEmployee Ă„â€˜Ă¡ÂºÂ¿n toÄ‚Â n bĂ¡Â»â„¢ nhÄ‚Â³m Order, Loyalty, Attendance, WorkSchedule, Expense vÄ‚Â  Inventory Ă„â€˜Ă¡Â»Âu lÄ‚Â  CORRECT khi giĂ¡Â»Â¯ Ă„â€˜Ä‚Âºng FK/nullability/delete behavior cĂ¡Â»Â§a PhĂ¡ÂºÂ§n BĂ¢â‚¬â€œC.

CÄ‚Â¡c correction bĂ¡ÂºÂ¯t buĂ¡Â»â„¢c:

1. OrderDetail.OrderId phĂ¡ÂºÂ£i ghi SHADOW FK.
2. KhÄ‚Â´ng ghi <<FK>> cho Orders.BranchId, Reservations.BranchId/TableId, OrderRequestItems.ProductId, Shifts.EmployeeId/BranchId, BusinessInsights.BranchId, ReceiptSettings.BranchId, SystemSettings.BranchId vÄ‚Â  Notifications.BranchId.
3. KhÄ‚Â´ng vĂ¡ÂºÂ½ cÄ‚Â¡c scalar/snapshot relationship Ă¡Â»Å¸ PhĂ¡ÂºÂ§n D.
4. PhÄ‚Â¢n biĂ¡Â»â€¡t nullable FK cĂ¡Â»Â§a Employees.BranchId, Tables.BranchId, WorkSchedules.BranchId, Orders.CustomerId, OrderDetails.ProductId/ToppingId, LoyaltyTransactions.OrderId vÄ‚Â  Expenses.StockReceiptId.
5. Delete behavior phĂ¡ÂºÂ£i lÄ‚Â  Cascade cho OrderĂ¢â‚¬â€œOrderDetail vÄ‚Â  OrderRequestĂ¢â‚¬â€œOrderRequestItem; Restrict cho Branch/Inventory/StockReceipt/StockIssue relations nÄ‚Âªu trÄ‚Âªn.
6. NĂ¡ÂºÂ¿u ERD hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i vĂ¡ÂºÂ½ PromotionĂ¢â‚¬â€œCustomer/Order, AreaĂ¢â‚¬â€œTable, ProductĂ¢â‚¬â€œTopping hoĂ¡ÂºÂ·c TableĂ¢â‚¬â€œOrder thÄ‚Â¬ Ă„â€˜Ä‚Â³ lÄ‚Â  quan hĂ¡Â»â€¡ thĂ¡Â»Â«a/INCORRECT.

CÄ‚Â¡c entity Area, Promotion, Reservation, Shift, BusinessInsight, ReceiptSettings, SystemSetting vÄ‚Â  Notification khÄ‚Â´ng cÄ‚Â³ configured FK relationship Ă„â€˜Ă¡ÂºÂ¿n cÄ‚Â¡c entity nghiĂ¡Â»â€¡p vĂ¡Â»Â¥ lÄ‚Â¢n cĂ¡ÂºÂ­n theo model hiĂ¡Â»â€¡n hÄ‚Â nh; ghi ISOLATED TABLE / NO CONFIGURED FK RELATIONSHIP khi cĂ¡ÂºÂ§n.

Migration history note: mĂ¡Â»â„¢t sĂ¡Â»â€˜ designer migration cĂ…Â© cÄ‚Â³ thĂ¡Â»Æ’ thĂ¡Â»Æ’ hiĂ¡Â»â€¡n Cascade cho quan hĂ¡Â»â€¡ required trĂ†Â°Ă¡Â»â€ºc khi cĂ¡ÂºÂ¥u hÄ‚Â¬nh hiĂ¡Â»â€¡n hÄ‚Â nh Ă„â€˜Ă†Â°Ă¡Â»Â£c hoÄ‚Â n thiĂ¡Â»â€¡n. ModelSnapshot mĂ¡Â»â€ºi nhĂ¡ÂºÂ¥t vÄ‚Â  migration 20261002140936_AddToppingType.Designer.cs lÄ‚Â  canonical cho model hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i. ChĂ†Â°a cÄ‚Â³ bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng schema runtime Ă„â€˜Ä‚Â£ apply migration cuĂ¡Â»â€˜i vÄ‚Â¬ khÄ‚Â´ng cÄ‚Â³ database local/runtime introspection.

## PHĂ¡ÂºÂ¦N G Ă¢â‚¬â€ PlantUML ERD hoÄ‚Â n chĂ¡Â»â€°nh Ă„â€˜Ä‚Â£ sĂ¡Â»Â­a

    @startuml
    hide methods
    skinparam linetype ortho
    entity Branches { * Id : uuid <<PK>>; Name : text }
    entity Employees { * Id : uuid <<PK>>; BranchId : uuid? <<FK>> }
    entity Tables { * Id : uuid <<PK>>; BranchId : uuid? <<FK>>; QrToken : text <<UQ>> }
    entity Customers { * Id : uuid <<PK>> }
    entity Orders { * Id : uuid <<PK>>; CustomerId : uuid? <<FK>>; BranchId : uuid? <<SCALAR ID ONLY>>; TableName : text? }
    entity OrderDetails { * Id : uuid <<PK>>; OrderId : uuid <<SHADOW FK>>; ProductId : uuid? <<FK>>; ToppingId : uuid? <<FK>> }
    entity Products { * Id : uuid <<PK>>; ToppingsJson : text? }
    entity Toppings { * Id : uuid <<PK>> }
    entity OrderRequests { * Id : uuid <<PK>>; OrderId : uuid <<FK>> }
    entity OrderRequestItems { * Id : uuid <<PK>>; OrderRequestId : uuid <<FK>>; ProductId : uuid? <<SCALAR ID ONLY>> }
    entity LoyaltyTransactions { * Id : uuid <<PK>>; CustomerId : uuid <<FK>>; OrderId : uuid? <<FK>> }
    entity Attendance { * Id : uuid <<PK>>; EmployeeId : uuid <<FK>>; BranchId : uuid <<FK>> }
    entity WorkSchedules { * Id : uuid <<PK>>; EmployeeId : uuid <<FK>>; BranchId : uuid? <<FK>> }
    entity BranchInventories { * Id : uuid <<PK>>; BranchId : uuid <<FK>>; InventoryItemId : uuid <<FK>> }
    entity InventoryItems { * Id : uuid <<PK>> }
    entity StockReceipts { * Id : uuid <<PK>>; BranchId : uuid <<FK>> }
    entity StockReceiptItems { * Id : uuid <<PK>>; StockReceiptId : uuid <<FK>>; InventoryItemId : uuid <<FK>> }
    entity StockIssues { * Id : uuid <<PK>>; BranchId : uuid <<FK>> }
    entity StockIssueItems { * Id : uuid <<PK>>; StockIssueId : uuid <<FK>>; InventoryItemId : uuid <<FK>> }
    entity StockTransactions { * Id : uuid <<PK>>; BranchId : uuid <<FK>>; InventoryItemId : uuid <<FK>>; ReferenceId : uuid }
    entity Expenses { * Id : uuid <<PK>>; BranchId : uuid <<FK>>; StockReceiptId : uuid? <<FK UQ>> }
    entity Areas { * Id : uuid <<PK>> }
    entity Promotions { * Id : uuid <<PK>> }
    entity Reservations { * Id : uuid <<PK>>; BranchId : uuid? <<SCALAR ID ONLY>>; TableId : uuid? <<SCALAR ID ONLY>> }
    entity Shifts { * Id : uuid <<PK>>; EmployeeId : uuid <<SCALAR ID ONLY>>; BranchId : uuid <<SCALAR ID ONLY>> }
    entity BusinessInsights { * Id : uuid <<PK>>; BranchId : uuid? <<SCALAR ID ONLY>> }
    entity ReceiptSettings { * Id : uuid <<PK>>; BranchId : uuid? <<SCALAR ID ONLY UQ>> }
    entity SystemSettings { * Id : uuid <<PK>>; BranchId : uuid? <<SCALAR ID ONLY>> }
    entity Notifications { * Id : uuid <<PK>>; BranchId : uuid? <<SCALAR ID ONLY>> }

    Branches ||--o{ Employees : BranchId nullable
    Branches ||--o{ Tables : BranchId nullable
    Customers ||--o{ Orders : CustomerId nullable
    Orders ||--o{ OrderDetails : OrderId shadow / Cascade
    Products ||--o{ OrderDetails : ProductId nullable
    Toppings ||--o{ OrderDetails : ToppingId nullable
    Orders ||--o{ OrderRequests : OrderId / Cascade
    OrderRequests ||--o{ OrderRequestItems : OrderRequestId / Cascade
    Customers ||--o{ LoyaltyTransactions : CustomerId / Cascade
    Orders ||--o{ LoyaltyTransactions : OrderId nullable
    Employees ||--o{ Attendances : EmployeeId / Cascade
    Branches ||--o{ Attendances : BranchId / Cascade
    Employees ||--o{ WorkSchedules : EmployeeId / Cascade
    Branches ||--o{ WorkSchedules : BranchId nullable
    Branches ||--o{ BranchInventories : BranchId / Restrict
    InventoryItems ||--o{ BranchInventories : InventoryItemId / Restrict
    Branches ||--o{ StockReceipts : BranchId / Restrict
    StockReceipts ||--o{ StockReceiptItems : StockReceiptId / Restrict
    InventoryItems ||--o{ StockReceiptItems : InventoryItemId / Restrict
    Branches ||--o{ StockIssues : BranchId / Restrict
    StockIssues ||--o{ StockIssueItems : StockIssueId / Restrict
    InventoryItems ||--o{ StockIssueItems : InventoryItemId / Restrict
    Branches ||--o{ StockTransactions : BranchId / Restrict
    InventoryItems ||--o{ StockTransactions : InventoryItemId / Restrict
    Branches ||--o{ Expenses : BranchId / Restrict
    StockReceipts ||--o| Expenses : StockReceiptId nullable unique / Restrict
    @enduml

KĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n: 26 REAL FK columns/relationships Ă„â€˜Ă†Â°Ă¡Â»Â£c xÄ‚Â¡c nhĂ¡ÂºÂ­n trong model hiĂ¡Â»â€¡n hÄ‚Â nh, trong Ă„â€˜Ä‚Â³ OrderDetail.OrderId lÄ‚Â  shadow FK. CÄ‚Â¡c ID Ă¡Â»Å¸ PhĂ¡ÂºÂ§n D lÄ‚Â  SCALAR ID ONLY. KhÄ‚Â´ng Ă„â€˜Ă¡Â»Â§ bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng Ă„â€˜Ă¡Â»Æ’ khĂ¡ÂºÂ³ng Ă„â€˜Ă¡Â»â€¹nh database runtime Ă„â€˜Ä‚Â£ Ă„â€˜Ă¡Â»â€œng bĂ¡Â»â„¢ migration cuĂ¡Â»â€˜i.




