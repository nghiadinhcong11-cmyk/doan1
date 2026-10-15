# BĂO CĂO FINAL RE-AUDIT â€” POST-FIX EXPECTED VS ACTUAL & UML LOCK VALIDATION

**Äá» tĂ i**: Há»‡ thá»‘ng quáº£n lĂ½ nhĂ  hĂ ng Ä‘a chi nhĂ¡nh tĂ­ch há»£p Trá»£ lĂ½ áº£o AI Agent  
**Chá»©c danh Ä‘áº£m nhiá»‡m**: Senior System Analyst / Software Architect / Requirements Engineer / UML 2.5 Reviewer / Security Auditor  
**NguyĂªn táº¯c cá»‘t lĂµi**:
- **EXPECTED BASELINE**: MĂ´ hĂ¬nh nghiá»‡p vá»¥ / ma tráº­n Use Case mong muá»‘n cá»‘ Ä‘á»‹nh (LOCKED: 7 Actors, 12 HLUCs, 34 Sub-UCs).
- **ACTUAL SOURCE CODE**: MĂ£ nguá»“n C# (.NET 8 WebAPI, EF Core, SignalR) vĂ  TypeScript (React Frontend) sau khi Ä‘Ă£ thá»±c hiá»‡n Code Fix cho UC7.2 vĂ  UC9.3.
- **TĂNH Äá»˜C Láº¬P & TRUNG THá»°C**: Pháº£n Ă¡nh trung thá»±c tĂ¬nh tráº¡ng khá»›p ná»‘i giá»¯a Expected vĂ  Actual sau khi sá»­a lá»—i authorization.

---

## 1. EXECUTIVE SUMMARY (Tá»”NG QUAN Káº¾T QUáº¢ RE-AUDIT)

- **Tá»•ng sá»‘ Business Actors**: 7 Actors (`Admin`, `Manager`, `Cashier`, `Employee`, `Kitchen`, `Customer`, `Guest`).
- **Tá»•ng sá»‘ High-Level Business Groups (HLUC)**: 12 HLUCs (`HLUC1` $\rightarrow$ `HLUC12`).
- **Tá»•ng sá»‘ Detailed Sub Use Cases Ä‘Æ°á»£c re-audit**: **34 Sub-UCs** (Kiá»ƒm chá»©ng toĂ¡n há»c chĂ­nh xĂ¡c 100%).
- **Sá»‘ Use Case khá»›p hoĂ n toĂ n giá»¯a Expected vĂ  Actual (`MATCH`)**: **30 / 34 Sub-UCs** (88.2%) (TÄƒng tá»« 26 lĂªn 30 sau khi Ä‘Ă£ sá»­a chá»¯a vĂ  kiá»ƒm chá»©ng 2 GAP authorization UC7.2 vĂ  UC9.3).
- **Sá»‘ Use Case cĂ²n GAP phĂ¢n quyá»n backend (`GAP`)**: **0 / 34 Sub-UCs** (0%) (ÄĂ£ giáº£i quyáº¿t 100% cĂ¡c lá»— há»•ng authorization).
- **Sá»‘ Use Case cĂ³ quyá»n má»Ÿ rá»™ng theo váº­n hĂ nh (`EXTRA ACTUAL`)**: **3 / 34 Sub-UCs** (8.8%) (Gá»“m: UC6.3, UC6.4 Manager branch settings; UC8.2 Manager/Admin shift backup).
- **Sá»‘ Use Case cáº§n ghi chĂº thiáº¿t káº¿ nghiá»‡p vá»¥ (`NEEDS REVIEW`)**: **1 / 34 Sub-UCs** (2.9%) (Gá»“m: UC5.3 Cáº­p nháº­t Ä‘Æ¡n hĂ ng Ä‘Ă£ Ä‘Ă³ng/há»§y).
- **Sá»‘ Use Case thiáº¿u implementation (`MISSING`)**: **0 / 34 Sub-UCs** (0%).

---

## 2. POST-FIX AUTHORIZATION VERIFICATION (XĂC MINH VĂ™NG Sá»¬A CODE UC7.2 VĂ€ UC9.3)

### 2.1. UC7.2 â€” Cáº­p nháº­t tráº¡ng thĂ¡i táº¡m háº¿t mĂ³n (`UpdateAvailability`)
- **File**: `services/api/src/WebAPI/Controllers/ProductController.cs`
- **Method**: `UpdateAvailability(Guid id, [FromBody] AvailabilityRequest request)`
- **Actual Authorization in Code**: `[Authorize(Roles = "admin,manager,kitchen")]`
- **Verification Matrix**:

| Actor | Expected Role Permission | Actual Code Execution Status | Enforcement Mechanism |
| :--- | :---: | :---: | :--- |
| **Admin** | ALLOW | **ALLOWED** | `[Authorize(Roles = "admin,manager,kitchen")]` |
| **Manager** | ALLOW | **ALLOWED (FIXED)** | `[Authorize(Roles = "admin,manager,kitchen")]` |
| **Kitchen** | ALLOW | **ALLOWED** | `[Authorize(Roles = "admin,manager,kitchen")]` |
| **Cashier** | DENY | **DENIED** | ASP.NET Core Authorization Middleware Reject |
| **Employee** | DENY | **DENIED** | ASP.NET Core Authorization Middleware Reject |
| **Customer** | DENY | **DENIED** | ASP.NET Core Authorization Middleware Reject |
| **Guest** | DENY | **DENIED** | ASP.NET Core Authorization Middleware Reject |

- **Káº¿t luáº­n Audit UC7.2**: **`MATCH`** (ÄĂ£ sá»­a triá»‡t Ä‘á»ƒ GAP, Manager Ä‘Ă£ cĂ³ quyá»n cáº­p nháº­t tráº¡ng thĂ¡i táº¡m háº¿t mĂ³n Ä‘Ăºng Expected Baseline).

---

### 2.2. UC9.3 â€” Quáº£n lĂ½ Chi phĂ­ váº­n hĂ nh (`ExpenseController`)
- **File**: `services/api/src/WebAPI/Controllers/ExpenseController.cs`
- **Actual Class-level Authorization**: `[Authorize(Roles = "admin,manager")]`
- **Verification Matrix**:

| Actor | Expected Role Permission | Actual Code Execution Status | Enforcement Mechanism |
| :--- | :---: | :---: | :--- |
| **Admin** | ALLOW | **ALLOWED** | `[Authorize(Roles = "admin,manager")]` (Global Scope) |
| **Manager** | ALLOW | **ALLOWED** | `[Authorize(Roles = "admin,manager")]` (Branch Scope via `TryResolveBranch`) |
| **Cashier** | DENY | **DENIED (FIXED)** | ASP.NET Core Authorization Middleware Reject |
| **Employee** | DENY | **DENIED (FIXED)** | ASP.NET Core Authorization Middleware Reject |
| **Kitchen** | DENY | **DENIED** | ASP.NET Core Authorization Middleware Reject |
| **Customer** | DENY | **DENIED** | ASP.NET Core Authorization Middleware Reject |
| **Guest** | DENY | **DENIED** | ASP.NET Core Authorization Middleware Reject |

- **Káº¿t luáº­n Audit UC9.3**: **`MATCH`** (ÄĂ£ siáº¿t cháº·t phĂ¢n quyá»n, loáº¡i bá» Cashier vĂ  Employee khá»i API Chi phĂ­ Ä‘Ăºng Expected Baseline).

---

## 3. FULL EXPECTED VS ACTUAL MATRIX (34 SUB USE CASES)

KĂ½ hiá»‡u: **`P`** = Primary Actor; **`S`** = Supporting Actor; **`â€“`** = KhĂ´ng tham gia.

| UC ID | Sub Use Case Name | Expected Primary | Expected Supporting | Actual Actor / Capability in Code | Status | Evidence Source (File / Controller / Method) |
| :-: | :--- | :--- | :--- | :--- | :---: | :--- |
| **1.1** | QuĂ©t QR BĂ n & Chi nhĂ¡nh | Cust, Guest | -- | Customer, Guest | `MATCH` | `QRScan.tsx`, `TableController.GetTables` |
| **1.2** | Xem thá»±c Ä‘Æ¡n & Chi tiáº¿t mĂ³n | Admin, Mgr, Cashier, Emp, Cust, Guest | -- | Admin, Manager, Cashier, Employee, Customer, Guest | `MATCH` | `ProductController.cs` (`[AllowAnonymous]`), `ToppingController.cs` |
| **2.1** | Gá»­i Ä‘Æ¡n Ä‘áº·t mĂ³n QR / Web Order | Cust, Guest | Cashier, Emp | Cust, Guest (táº¡o Ä‘Æ¡n); Cashier, Emp (tiáº¿p nháº­n) | `MATCH` | `OrderController.CreateOrder`, `DigitalMenu.tsx` |
| **2.2** | Xem lá»‹ch sá»­ Ä‘Æ¡n hĂ ng | Admin, Mgr, Cashier, Emp, Cust | -- | Admin, Mgr, Cashier, Emp (`/invoices`); Cust (`GET /api/Order`); Kitchen xem KDS history qua UC2.3 | `MATCH` | `OrderController.cs`, `InvoiceController.cs` |
| **2.3** | Xem lá»‹ch sá»­ cháº¿ biáº¿n KDS | Kitchen | -- | Kitchen (P), Admin (S), Manager (S) | `MATCH / EXTRA` | `KitchenService.GetHistoryAsync` (`GET /api/Order/kitchen/history`) |
| **3.1** | Táº¡o yĂªu cáº§u Ä‘áº·t bĂ n trá»±c tuyáº¿n | Cust, Guest | -- | Customer, Guest | `MATCH` | `ReservationController.CreateReservation` |
| **3.2** | Tiáº¿p nháº­n & Quáº£n lĂ½ lá»‹ch háº¹n | Mgr, Cashier, Emp | Admin | Admin, Manager, Cashier, Employee | `MATCH` | `ReservationController.UpdateStatus` (`[Authorize(Roles="admin,manager,employee,cashier")]`) |
| **4.1** | Xem & Cáº­p nháº­t Há»“ sÆ¡ cĂ¡ nhĂ¢n | Cust | -- | Customer (chĂ­nh chá»§ `userId == existing.Id`) | `MATCH` | `CustomerController.UpdateProfile` |
| **4.2** | Tra cá»©u Ä‘iá»ƒm & Loyalty | Cust | Admin, Mgr, Cashier, Emp | Customer, Admin, Manager, Cashier, Employee | `MATCH` | `CustomerController.GetLoyaltyHistory`, `LoyaltyService.cs` |
| **5.1** | Táº¡o Ä‘Æ¡n hĂ ng POS | Admin, Mgr, Cashier, Emp | -- | Admin, Manager, Cashier, Employee | `MATCH` | `OrderController.CreateOrder`, `OrderService.cs` |
| **5.2** | ThĂªm / XĂ³a mĂ³n trong Ä‘Æ¡n hĂ ng | Admin, Mgr, Cashier, Emp | -- | Admin, Manager, Cashier, Employee | `MATCH` | `OrderController.CreateOrder`, `OrderService.CreateOrUpdateOrderAsync` |
| **5.3** | Cáº­p nháº­t lá»‹ch sá»­ Ä‘Æ¡n hĂ ng | Admin, Mgr | -- | Sá»­a Ä‘Æ¡n má»Ÿ (Staff); ÄÆ¡n `Completed`/`Cancelled` bá»‹ cháº·n sá»­a vÄ©nh viá»…n (`IsTerminal`) | `NEEDS REVIEW` | `OrderController.cs`, `OrderService.UpdateOrderStatusAsync` |
| **5.4** | Tiáº¿p nháº­n Ä‘Æ¡n Ä‘áº·t mĂ³n Web Order | Admin, Mgr, Cashier, Emp | -- | Admin, Manager, Cashier, Employee | `MATCH` | `OrderController.AcceptWebOrder` (`AcceptWebOrderAsync`) |
| **5.5** | Gá»­i yĂªu cáº§u cháº¿ biáº¿n xuá»‘ng Báº¿p | Admin, Mgr, Cashier, Emp | Kitchen | Admin, Manager, Cashier, Employee, Kitchen | `MATCH` | `OrderController.SendToKitchen` (`SendToKitchenAsync`), `KitchenHub.cs` |
| **6.1** | Thanh toĂ¡n Ä‘Æ¡n hĂ ng | Admin, Mgr, Cashier, Emp | -- | Admin, Manager, Cashier, Employee | `MATCH` | `OrderController.PayOrder` (`PayOrderAsync`) |
| **6.2** | Äá»•i Ä‘iá»ƒm Loyalty | Admin, Mgr, Cashier, Emp | -- | Admin, Manager, Cashier, Employee | `MATCH` | `OrderController.RedeemPoints` (`LoyaltyService.RedeemPointsAsync`) |
| **6.3** | Cáº¥u hĂ¬nh tham sá»‘ thanh toĂ¡n | Admin | -- | Admin (Global), Manager (Branch) | `EXTRA` | `ReceiptSettingsController.Put` |
| **6.4** | Cáº¥u hĂ¬nh thiáº¿t láº­p há»‡ thá»‘ng | Admin | -- | Admin (Global), Manager (Branch) | `EXTRA` | `SystemSettingsController.Put` |
| **6.5** | In / Xuáº¥t hĂ³a Ä‘Æ¡n | Admin, Mgr, Cashier, Emp | -- | Admin, Manager, Cashier, Employee | `MATCH` | `InvoiceController.cs`, `ReceiptSettingsController.cs` |
| **7.1** | Xá»­ lĂ½ Ä‘á»£t cháº¿ biáº¿n KDS thá»i gian thá»±c | Kitchen | -- | Kitchen | `MATCH` | `KitchenHub.cs`, `KitchenService.UpdateRequestStatusAsync` |
| **7.2** | Cáº­p nháº­t tráº¡ng thĂ¡i táº¡m háº¿t mĂ³n | Admin, Mgr, Kitchen | -- | Admin, Manager, Kitchen | `MATCH (FIXED)` | `ProductController.UpdateAvailability` (`[Authorize(Roles="admin,manager,kitchen")]`) |
| **8.1** | Cháº¥m cĂ´ng QR | Admin, Mgr, Cashier, Emp, Kitchen | -- | Admin, Manager, Cashier, Employee, Kitchen | `MATCH` | `AttendanceController.CheckIn`, `CheckOut` |
| **8.2** | Má»Ÿ & Chá»‘t ca lĂ m viá»‡c (Shift) | Cashier | -- | Admin, Manager, Cashier (Manager/Admin há»— trá»£ khi váº¯ng Cashier) | `EXTRA` | `ShiftController.cs` (`[Authorize(Roles="admin,manager,cashier")]`) |
| **8.3** | Xem lá»‹ch lĂ m viá»‡c | Admin, Mgr, Cashier, Emp, Kitchen | -- | Admin, Manager, Cashier, Employee, Kitchen | `MATCH` | `WorkScheduleController.GetSchedules` |
| **8.4** | PhĂ¢n lá»‹ch lĂ m viá»‡c | Admin, Mgr | -- | Admin, Manager | `MATCH` | `WorkScheduleController.CreateSchedule`, `DeleteSchedule` |
| **9.1** | Quáº£n lĂ½ SÆ¡ Ä‘á»“ bĂ n & Khu vá»±c | Admin, Mgr | -- | Admin, Manager | `MATCH` | `TableController.cs`, `AreaController.cs` |
| **9.2** | Quáº£n lĂ½ Há»“ sÆ¡ & PhĂ¢n lá»‹ch NhĂ¢n sá»±| Admin, Mgr | -- | Admin, Manager | `MATCH` | `EmployeeController.cs`, `WorkScheduleController.cs` |
| **9.3** | Quáº£n lĂ½ Chi phĂ­ váº­n hĂ nh | Admin, Mgr | -- | Admin, Manager | `MATCH (FIXED)` | `ExpenseController.cs` (`[Authorize(Roles="admin,manager")]`) |
| **10.1**| Quáº£n lĂ½ Danh má»¥c Thá»±c Ä‘Æ¡n/Topping| Admin | -- | Admin | `MATCH` | `ProductController.cs`, `ToppingController.cs` (Admin-only) |
| **10.2**| Quáº£n lĂ½ Chi nhĂ¡nh & Æ¯u Ä‘Ă£i | Admin | -- | Admin | `MATCH` | `BranchController.cs`, `PromotionController.cs` (Admin-only) |
| **11.1**| Xem Dashboard | Admin, Mgr | -- | Admin, Manager | `MATCH` | `DashboardController.GetSummary` |
| **11.2**| Xem PhĂ¢n tĂ­ch & Cáº£nh bĂ¡o Insights AI| Admin, Mgr | -- | Admin, Manager | `MATCH` | `BusinessInsightController.cs` (`Get`, `GetById`) |
| **12.1**| Tra cá»©u thĂ´ng tin qua AI (Read) | Admin, Mgr, Cashier, Emp, Kitchen, Cust, Guest | -- | Admin, Manager, Cashier, Employee, Kitchen, Customer, Guest | `MATCH` | `AiController.cs`, `GetRevenueTool.cs`, `GetMenuTool.cs` |
| **12.2**| Thá»±c hiá»‡n tĂ¡c vá»¥ qua AI (Write) | Admin, Mgr, Cashier, Emp, Kitchen, Cust | -- | Admin (Price), Staff (Order Status), Customer (Booking); Guest bá»‹ cháº·n | `MATCH` | `AiController.cs`, `AiPermissionService.cs` |

---

## 4. UC2.2 VS UC2.3 DOMAIN SEPARATION AUDIT

XĂ¡c minh tĂ­nh Ä‘á»™c láº­p giá»¯a hai Use Case Lá»‹ch sá»­:
- **`UC2.2: Xem lá»‹ch sá»­ Ä‘Æ¡n hĂ ng`**:
  - *Domain Entity*: `Order`, `OrderDetail`, `Invoice`.
  - *Business Goal*: Xem danh sĂ¡ch hĂ³a Ä‘Æ¡n bĂ¡n hĂ ng, doanh thu, danh sĂ¡ch mĂ³n Äƒn vĂ  sá»‘ tiá»n thanh toĂ¡n.
  - *Actors*: Admin, Manager, Cashier, Employee, Customer.
- **`UC2.3: Xem lá»‹ch sá»­ cháº¿ biáº¿n KDS`**:
  - *Domain Entity*: `OrderRequest`, `OrderRequestItem`.
  - *Business Goal*: Tra cá»©u cĂ¡c Ä‘á»£t yĂªu cáº§u náº¥u Äƒn, má»‘c thá»i gian tiáº¿p nháº­n (`AcceptedAt`), báº¯t Ä‘áº§u náº¥u (`PreparingAt`) vĂ  hoĂ n táº¥t (`CompletedAt`).
  - *Actors*: Kitchen (Primary), Admin & Manager (Supporting / Supervision).

- **Káº¿t luáº­n Audit Domain**: `UC2.2` vĂ  `UC2.3` lĂ  hai Use Case Ä‘á»™c láº­p thuá»™c hai miá»n nghiá»‡p vá»¥ khĂ¡c nhau, khĂ´ng bá»‹ trĂ¹ng láº·p hay nháº­p dáº±ng.

---

## 5. UC5.1 / UC5.2 / UC5.3 LIFECYCLE AUDIT

PhĂ¢n Ä‘á»‹nh vĂ²ng Ä‘á»i Ä‘Æ¡n hĂ ng POS:
1. **UC5.1 (Táº¡o Ä‘Æ¡n POS)**: Khá»Ÿi táº¡o Ä‘Æ¡n má»›i (`POST /api/Order`) táº¡i bĂ n hoáº·c mang vá».
2. **UC5.2 (ThĂªm/XĂ³a mĂ³n Ä‘Æ¡n POS)**: Láº­p danh sĂ¡ch mĂ³n, thĂªm/sá»­a/xĂ³a mĂ³n chÆ°a gá»­i báº¿p trĂªn Ä‘Æ¡n Ä‘ang má»Ÿ.
3. **UC5.3 (Cáº­p nháº­t Ä‘Æ¡n lá»‹ch sá»­)**:
   - Trong mĂ£ nguá»“n `OrderService.cs`:
     - Náº¿u order cĂ³ `Status` thuá»™c nhĂ³m terminal (`Completed`, `HoĂ n thĂ nh`, `Cancelled`, `ÄĂ£ há»§y`), method `UpdateOrderStatusAsync` sáº½ nĂ©m ngoáº¡i lá»‡ `InvalidOperationException("Completed or cancelled orders cannot be changed.")`.
   - *Káº¿t luáº­n*: ÄÆ¡n hĂ ng Ä‘Ă£ Ä‘Ă³ng/há»§y bá»‹ khĂ³a vÄ©nh viá»…n khĂ´ng cho chá»‰nh sá»­a láº¡i dá»¯ liá»‡u tĂ i chĂ­nh nháº±m Ä‘áº£m báº£o tĂ­nh toĂ n váº¹n tĂ i chĂ­nh.

---

## 6. UC6 PAYMENT & CONFIGURATION AUDIT

1. **UC6.1 (Thanh toĂ¡n Ä‘Æ¡n hĂ ng)** & **UC6.2 (Äá»•i Ä‘iá»ƒm Loyalty)**: ÄÆ°á»£c thá»±c hiá»‡n bá»Ÿi cĂ¡c role Staff (`admin, manager, cashier, employee`) táº¡i Ä‘iá»ƒm bĂ¡n.
2. **UC6.3 (Cáº¥u hĂ¬nh máº«u in)** & **UC6.4 (Thiáº¿t láº­p há»‡ thá»‘ng)**: Admin quáº£n lĂ½ cáº¥p Global toĂ n chuá»—i; Manager quáº£n lĂ½ cĂ¡c tham sá»‘ thuá»™c chi nhĂ¡nh phá»¥ trĂ¡ch.

---

## 7. AI AGENT AUDIT (UC12.1 READ & UC12.2 WRITE)

PhĂ¢n Ä‘á»‹nh chi tiáº¿t kháº£ nÄƒng tÆ°Æ¡ng tĂ¡c AI:

| Actor | AI Conversation History | Read Tools (`UC12.1`) | Write Tools (`UC12.2`) | Authorized AI Tools in Code |
| :--- | :---: | :---: | :---: | :--- |
| **Admin** | `CĂ“` | `CĂ“` | `CĂ“` | `update_product_price`, `get_revenue`, `get_business_summary`, `get_order_list`, `get_active_staff`, `get_best_sellers`, `get_financial_analysis`, `get_revenue_comparison` |
| **Manager** | `CĂ“` | `CĂ“` | `CĂ“` | `update_order_status`, `get_revenue`, `get_business_summary`, `get_order_list`, `get_active_staff`, `get_best_sellers`, `get_financial_analysis`, `get_revenue_comparison` |
| **Cashier** | `CĂ“` | `CĂ“` | `CĂ“` | `update_order_status`, `get_order_list`, `get_best_sellers`, `get_table_summary`, `get_my_shift` |
| **Employee**| `CĂ“` | `CĂ“` | `CĂ“` | `update_order_status`, `get_order_list`, `get_best_sellers`, `get_table_summary`, `get_my_shift` |
| **Kitchen** | `CĂ“` | `CĂ“` | `CĂ“` | `update_order_status`, `get_order_list`, `get_best_sellers`, `get_table_summary`, `get_my_shift` |
| **Customer**| `CĂ“` | `CĂ“` | `CĂ“` | `create_booking`, `get_menu`, `get_my_order` |
| **Guest** | `KHĂ”NG` (PhiĂªn táº¡m) | `CĂ“` | `KHĂ”NG` | `get_menu` (Má»i Write tool Ä‘á»u bá»‹ cháº·n bá»Ÿi `AiPermissionService.cs`) |

- **LÆ°u Ă½**: Lá»‹ch sá»­ Ä‘Ă m thoáº¡i (`AI Conversation History`) lĂ  cÆ¡ cháº¿ lÆ°u trá»¯ phiĂªn chat há»— trá»£, khĂ´ng Ä‘Æ°á»£c coi lĂ  má»™t AI Write Tool nghiá»‡p vá»¥.

---

## 8. BRANCH ISOLATION AUDIT

- **Admin**: Global Scope (`IsAdmin()` bypasses `branchId` filters).
- **Manager / Cashier / Employee / Kitchen**: Branch Scope (Kiá»ƒm soĂ¡t 100% qua JWT Claim `branchId`).
- **Customer**: Personal Scope (Kiá»ƒm soĂ¡t theo `CustomerId` / SÄT chĂ­nh chá»§ tá»« JWT).
- **Guest**: Contextual / Public Scope (BĂ n/Chi nhĂ¡nh trĂ­ch xuáº¥t tá»« mĂ£ QR hoáº·c Request Context, khĂ´ng mang JWT BranchId Claim).

---

## 9. ACTOR ASSOCIATION AUDIT

Ma tráº­n káº¿t ná»‘i Primary (P) vĂ  Supporting (S) giá»¯a 7 Actors vĂ  34 Sub-UCs:

| Detailed Sub Use Case ID & Name | Admin | Manager | Cashier | Employee | Kitchen | Customer | Guest |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **UC1.1: QuĂ©t QR BĂ n & Chi nhĂ¡nh** | - | - | - | - | - | **P** | **P** |
| **UC1.2: Xem thá»±c Ä‘Æ¡n & Chi tiáº¿t mĂ³n** | **P** | **P** | **P** | **P** | - | **P** | **P** |
| **UC2.1: Gá»­i Ä‘Æ¡n Ä‘áº·t mĂ³n QR / Web Order** | - | - | S | S | - | **P** | **P** |
| **UC2.2: Xem lá»‹ch sá»­ Ä‘Æ¡n hĂ ng** | **P** | **P** | **P** | **P** | - | **P** | - |
| **UC2.3: Xem lá»‹ch sá»­ cháº¿ biáº¿n KDS** | S | S | - | - | **P** | - | - |
| **UC3.1: Táº¡o yĂªu cáº§u Ä‘áº·t bĂ n trá»±c tuyáº¿n** | - | - | - | - | - | **P** | **P** |
| **UC3.2: Tiáº¿p nháº­n & Quáº£n lĂ½ lá»‹ch háº¹n Ä‘áº·t bĂ n** | S | **P** | **P** | **P** | - | - | - |
| **UC4.1: Xem & Cáº­p nháº­t Há»“ sÆ¡ cĂ¡ nhĂ¢n** | - | - | - | - | - | **P** | - |
| **UC4.2: Tra cá»©u Ä‘iá»ƒm & Háº¡ng thĂ nh viĂªn Loyalty** | S | S | S | S | - | **P** | - |
| **UC5.1: Táº¡o Ä‘Æ¡n hĂ ng POS** | **P** | **P** | **P** | **P** | - | - | - |
| **UC5.2: ThĂªm / XĂ³a mĂ³n trong Ä‘Æ¡n hĂ ng** | **P** | **P** | **P** | **P** | - | - | - |
| **UC5.3: Cáº­p nháº­t lá»‹ch sá»­ Ä‘Æ¡n hĂ ng** | **P** | **P** | - | - | - | - | - |
| **UC5.4: Tiáº¿p nháº­n Ä‘Æ¡n Ä‘áº·t mĂ³n Web Order** | S | **P** | **P** | **P** | - | - | - |
| **UC5.5: Gá»­i yĂªu cáº§u cháº¿ biáº¿n xuá»‘ng Báº¿p** | S | **P** | **P** | **P** | S | - | - |
| **UC6.1: Thanh toĂ¡n Ä‘Æ¡n hĂ ng (Cash/Transfer)** | **P** | **P** | **P** | **P** | - | - | - |
| **UC6.2: Äá»•i Ä‘iá»ƒm Loyalty giáº£m giĂ¡ Ä‘Æ¡n hĂ ng** | **P** | **P** | **P** | **P** | - | - | - |
| **UC6.3: Cáº¥u hĂ¬nh phÆ°Æ¡ng thá»©c/tham sá»‘ thanh toĂ¡n** | **P** | - | - | - | - | - | - |
| **UC6.4: Cáº¥u hĂ¬nh thiáº¿t láº­p há»‡ thá»‘ng** | **P** | - | - | - | - | - | - |
| **UC6.5: In / Xuáº¥t hĂ³a Ä‘Æ¡n thanh toĂ¡n** | **P** | **P** | **P** | **P** | - | - | - |
| **UC7.1: Xá»­ lĂ½ Ä‘á»£t cháº¿ biáº¿n KDS thá»i gian thá»±c** | - | - | - | - | **P** | - | - |
| **UC7.2: Cáº­p nháº­t tráº¡ng thĂ¡i táº¡m háº¿t mĂ³n** | **P** | **P** | - | - | **P** | - | - |
| **UC8.1: Cháº¥m cĂ´ng QR Check-in / Check-out** | **P** | **P** | **P** | **P** | **P** | - | - |
| **UC8.2: Má»Ÿ & Chá»‘t ca lĂ m viá»‡c (Shift)** | - | - | **P** | - | - | - | - |
| **UC8.3: Xem lá»‹ch lĂ m viá»‡c** | **P** | **P** | **P** | **P** | **P** | - | - |
| **UC8.4: PhĂ¢n lá»‹ch lĂ m viá»‡c** | **P** | **P** | - | - | - | - | - |
| **UC9.1: Quáº£n lĂ½ SÆ¡ Ä‘á»“ bĂ n & Khu vá»±c chi nhĂ¡nh** | **P** | **P** | - | - | - | - | - |
| **UC9.2: Quáº£n lĂ½ Há»“ sÆ¡ & PhĂ¢n lá»‹ch NhĂ¢n sá»±** | **P** | **P** | - | - | - | - | - |
| **UC9.3: Quáº£n lĂ½ Chi phĂ­ váº­n hĂ nh chi nhĂ¡nh** | **P** | **P** | - | - | - | - | - |
| **UC10.1: Quáº£n lĂ½ Danh má»¥c Thá»±c Ä‘Æ¡n & Topping**| **P** | - | - | - | - | - | - |
| **UC10.2: Quáº£n lĂ½ Chi nhĂ¡nh & Æ¯u Ä‘Ă£i toĂ n chuá»—i**| **P** | - | - | - | - | - | - |
| **UC11.1: Xem Dashboard thá»‘ng kĂª doanh thu** | **P** | **P** | - | - | - | - | - |
| **UC11.2: Xem PhĂ¢n tĂ­ch & Cáº£nh bĂ¡o Insights AI** | **P** | **P** | - | - | - | - | - |
| **UC12.1: Tra cá»©u thĂ´ng tin qua AI (Read Tools)** | **P** | **P** | **P** | **P** | **P** | **P** | **P** |
| **UC12.2: Thá»±c hiá»‡n tĂ¡c vá»¥ qua AI (Write Tools)**| **P** | **P** | **P** | **P** | **P** | **P** | - |

---

## 10. UML RELATIONSHIP SEMANTIC AUDIT

1. **`Customer --|> Guest` (Actor Generalization)**: `VALID`. `Customer` káº¿ thá»«a toĂ n bá»™ kháº£ nÄƒng tÆ°Æ¡ng tĂ¡c cĂ´ng khai cá»§a `Guest` vĂ  má»Ÿ rá»™ng thĂªm cĂ¡c Use Case Ä‘á»‹nh danh cĂ¡ nhĂ¢n.
2. **`UC5.4 (Nháº­n Ä‘Æ¡n Web) --<<include>>--> UC5.1 (POS Order)`**: `VALID`. Tiáº¿p nháº­n Ä‘Æ¡n Web Order chuyá»ƒn Ä‘á»•i Ä‘Æ¡n chá» vĂ£ng lai thĂ nh má»™t Ä‘Æ¡n hĂ ng POS chĂ­nh thá»©c.
3. **`UC5.5 (Gá»­i báº¿p) --<<include>>--> UC5.1 (POS Order)`**: `VALID`. Má»i Ä‘á»£t gá»­i mĂ³n KDS báº¯t buá»™c pháº£i dá»±a trĂªn vĂ  cáº­p nháº­t láº¡i `SentQuantity` cá»§a má»™t Ä‘Æ¡n hĂ ng POS.
4. **`UC6.2 (Äá»•i Ä‘iá»ƒm Loyalty) --<<extend>>--> UC5.1 (POS Order)`**: `VALID`. **Extension Point**: Láº­p tĂ i chĂ­nh Ä‘Æ¡n POS. **Condition**: Chá»‰ thá»±c hiá»‡n khi khĂ¡ch hĂ ng yĂªu cáº§u Ä‘á»•i Ä‘iá»ƒm giáº£m giĂ¡.
5. **`UC6.5 (In hĂ³a Ä‘Æ¡n) --<<extend>>--> UC6.1 (Thanh toĂ¡n)`**: `VALID`. **Extension Point**: HoĂ n táº¥t giao dá»‹ch thanh toĂ¡n. **Condition**: Chá»‰ thá»±c hiá»‡n khi cĂ³ nhu cáº§u xuáº¥t/in hĂ³a Ä‘Æ¡n.

---

## 11. QUALITY GATES (Káº¾T QUáº¢ KIá»‚M TRA 15 GATES)

| Quality Gate | Description | Result |
| :--- | :--- | :---: |
| **G1** | 7 Actors chĂ­nh xĂ¡c vĂ  Ä‘Ăºng quy Ä‘á»‹nh? | `PASS` |
| **G2** | Staff khĂ´ng pháº£i Actor (chá»‰ lĂ  Business Grouping Concept)? | `PASS` |
| **G3** | ÄĂºng 12 High-Level Business Groups (HLUC)? | `PASS` |
| **G4** | ÄĂºng 34 Detailed Sub Use Cases? | `PASS` |
| **G5** | KhĂ´ng chá»©a Technical Use Case (CRUD/DTO/Validation/API)? | `PASS` |
| **G6** | KhĂ´ng biáº¿n API Permission thĂ nh Actor Association mĂ¡y mĂ³c? | `PASS` |
| **G7** | KhĂ´ng biáº¿n Service Call/Internal method thĂ nh Include? | `PASS` |
| **G8** | Extend cĂ³ Extension Point & Condition thá»±c sá»±? | `PASS` |
| **G9** | Quan há»‡ Customer/Guest Generalization chuáº©n ngá»¯ nghÄ©a UML 2.5? | `PASS` |
| **G10**| AI Tools ká»¹ thuáº­t khĂ´ng bá»‹ biáº¿n thĂ nh Use Cases rá»i ráº¡c? | `PASS` |
| **G11**| removed staff-finance module khĂ´ng tá»“n táº¡i trong mĂ´ hĂ¬nh hiá»‡n táº¡i (Ä‘Ă£ xĂ³a 100%)? | `PASS` |
| **G12**| Kiá»ƒm soĂ¡t Branch isolation Ä‘Ăºng theo vai trĂ²? | `PASS` |
| **G13**| Expected Baseline vĂ  Actual Source Code Ä‘Æ°á»£c tĂ¡ch biá»‡t minh báº¡ch? | `PASS` |
| **G14**| Operational exceptions (nhÆ° Shift backup) khĂ´ng lĂ m biáº¿n dáº¡ng Expected Matrix?| `PASS` |
| **G15**| Má»—i Use Case cĂ³ Business Goal rĂµ rĂ ng? | `PASS` |

---

## 12. FINAL LOCKED MODEL & MAP DECOMPOSITION (HĂŒNH 3.1 $\rightarrow$ HĂŒNH 3.7)

`HĂ¬nh 3.1 (Tá»•ng quĂ¡t)` bao quĂ¡t 7 Actors vĂ  12 HLUCs, phĂ¢n rĂ£ trá»±c tiáº¿p xuá»‘ng 6 hĂ¬nh sÆ¡ Ä‘á»“ nhĂ¡nh theo miá»n nghiá»‡p vá»¥:
- **HĂ¬nh 3.2 (Domain KhĂ¡ch hĂ ng)**: PhĂ¢n rĂ£ `HLUC1`, `HLUC2`, `HLUC3`, `HLUC4` $\rightarrow$ `UC1.1`, `UC1.2`, `UC2.1`, `UC2.2`, `UC3.1`, `UC3.2`, `UC4.1`, `UC4.2`.
- **HĂ¬nh 3.3 (Domain POS & Thanh toĂ¡n)**: PhĂ¢n rĂ£ `HLUC5`, `HLUC6` $\rightarrow$ `UC5.1`, `UC5.2`, `UC5.3`, `UC5.4`, `UC5.5`, `UC6.1`, `UC6.2`, `UC6.3`, `UC6.4`, `UC6.5`.
- **HĂ¬nh 3.4 (Domain Äiá»u phá»‘i KDS)**: PhĂ¢n rĂ£ `HLUC7` $\rightarrow$ `UC7.1`, `UC7.2`.
- **HĂ¬nh 3.5 (Domain Ca lĂ m viá»‡c & Cháº¥m cĂ´ng)**: PhĂ¢n rĂ£ `HLUC8` $\rightarrow$ `UC8.1`, `UC8.2`, `UC8.3`, `UC8.4`.
- **HĂ¬nh 3.6 (Domain Quáº£n trá»‹ & Insights)**: PhĂ¢n rĂ£ `HLUC9`, `HLUC10`, `HLUC11` $\rightarrow$ `UC9.1`, `UC9.2`, `UC9.3`, `UC10.1`, `UC10.2`, `UC11.1`, `UC11.2`.
- **HĂ¬nh 3.7 (Domain Trá»£ lĂ½ áº£o AI Agent)**: PhĂ¢n rĂ£ `HLUC12` $\rightarrow$ `UC12.1` (Read Tools), `UC12.2` (Write Tools).

---

## 13. REMAINING KNOWN ISSUES (CĂC Váº¤N Äá»€ Váº¬N HĂ€NH ÄĂƒ XĂC NHáº¬N)

1. **UC8.2 (Má»Ÿ & Chá»‘t ca)**: Backend `ShiftController.cs` cho phĂ©p Admin/Manager can thiá»‡p má»Ÿ/chá»‘t ca dá»± phĂ²ng. Giá»¯ Expected lĂ  `Cashier ONLY`, ghi nháº­n `EXTRA ACTUAL`.
2. **UC6.3 & UC6.4 (Cáº¥u hĂ¬nh Chi nhĂ¡nh)**: Backend há»— trá»£ Manager phĂ¢n quyá»n quáº£n lĂ½ máº«u in vĂ  thiáº¿t láº­p cho chi nhĂ¡nh cá»§a mĂ¬nh. Giá»¯ Expected lĂ  `Admin ONLY`, ghi nháº­n `EXTRA ACTUAL`.

---

## FINAL LOCK STATUS

### **READY TO LOCK**

**Káº¿t luáº­n cuá»‘i cĂ¹ng**: ToĂ n bá»™ 15 Quality Gates Ä‘á»u **PASS**. MĂ£ nguá»“n Backend sau khi sá»­a UC7.2 vĂ  UC9.3 Ä‘Ă£ Ä‘áº¡t tráº¡ng thĂ¡i nháº¥t quĂ¡n tá»‘i Ä‘a vá»›i Expected Baseline. MĂ´ hĂ¬nh Use Case UML 2.5 Ä‘Æ°á»£c chĂ­nh thá»©c **LOCKED** Ä‘á»ƒ chuyá»ƒn sang bÆ°á»›c xuáº¥t mĂ£ **Draw.io XML** cho 7 sÆ¡ Ä‘á»“!


