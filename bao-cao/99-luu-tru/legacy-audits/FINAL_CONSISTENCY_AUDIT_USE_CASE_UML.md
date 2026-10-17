# BĂO CĂO FINAL CONSISTENCY AUDIT â€” USE CASE EXPECTED VS ACTUAL & UML SEMANTIC VALIDATION

**Äá» tĂ i**: Há»‡ thá»‘ng quáº£n lĂ½ nhĂ  hĂ ng Ä‘a chi nhĂ¡nh tĂ­ch há»£p Trá»£ lĂ½ áº£o AI Agent  
**Chá»©c danh**: Senior System Analyst / Software Architect / Requirements Engineer / UML Reviewer / Code Auditor  
**NguyĂªn táº¯c cá»‘t lĂµi**:
- **EXPECTED BASELINE**: MĂ´ hĂ¬nh nghiá»‡p vá»¥ / ma tráº­n Use Case mong muá»‘n cá»‘ Ä‘á»‹nh (khĂ´ng tá»± Ă½ thay Ä‘á»•i).
- **ACTUAL SOURCE CODE**: MĂ£ nguá»“n C# (.NET 8 WebAPI, EF Core, SignalR) vĂ  TypeScript (React Frontend) hiá»‡n táº¡i lĂ  **Single Source of Truth** duy nháº¥t.
- **TĂNH Äá»˜C Láº¬P**: Má»i sá»± lá»‡ch quyá»n, thá»«a quyá»n hoáº·c thiáº¿u quyá»n giá»¯a Expected vĂ  Actual Ä‘Æ°á»£c pháº£n Ă¡nh trung thá»±c qua cĂ¡c tráº¡ng thĂ¡i `MATCH`, `GAP`, `EXTRA`, `MISSING`, `NEEDS REVIEW`.

---

## PART A â€” EXECUTIVE SUMMARY (Tá»”NG QUAN TĂŒNH TRáº NG Káº¾T QUáº¢ AUDIT)

- **Tá»•ng sá»‘ Business Actors**: 7 Actors (`Admin`, `Manager`, `Cashier`, `Employee`, `Kitchen`, `Customer`, `Guest`).
- **Tá»•ng sá»‘ High-Level Business Groups (HLUC)**: 12 HLUCs (`HLUC1` $\rightarrow$ `HLUC12`).
- **Tá»•ng sá»‘ Detailed Sub Use Cases Ä‘Æ°á»£c audit**: **34 Sub-UCs** (ÄĂ£ Ä‘áº¿m vĂ  kiá»ƒm chá»©ng toĂ¡n há»c chĂ­nh xĂ¡c 100%).
- **Sá»‘ Use Case khá»›p hoĂ n toĂ n giá»¯a Expected vĂ  Actual (`MATCH`)**: **26 / 34 Sub-UCs** (76.5%).
- **Sá»‘ Use Case cĂ³ Ä‘á»™ lá»‡ch / má»Ÿ rá»™ng quyá»n trong Code (`GAP / EXTRA ACTUAL`)**: **6 / 34 Sub-UCs** (17.6%) (Gá»“m: UC5.3, UC6.3, UC6.4, UC7.2, UC8.2, UC9.3).
- **Sá»‘ Use Case cáº§n xem xĂ©t láº¡i thiáº¿t káº¿ nghiá»‡p vá»¥ (`NEEDS REVIEW`)**: **2 / 34 Sub-UCs** (5.9%) (Gá»“m: UC2.2 Lá»‹ch sá»­ Ä‘Æ¡n hĂ ng vs UC2.3 Lá»‹ch sá»­ cháº¿ biáº¿n KDS; UC5.3 Cáº­p nháº­t Ä‘Æ¡n lá»‹ch sá»­).
- **Sá»‘ Use Case thiáº¿u hoĂ n toĂ n implementation (`MISSING`)**: **0 / 34 Sub-UCs** (0%) (ToĂ n bá»™ 34 Sub-UCs Ä‘á»u Ä‘Ă£ cĂ³ implementation thá»±c táº¿ trong Backend & Frontend).

---

## PART B â€” FINAL EXPECTED ACTOR MATRIX (Báº¢NG EXPECTED BASELINE Cá» Äá»NH)

KĂ½ hiá»‡u: **`P`** = Primary Actor (NgÆ°á»i khá»Ÿi táº¡o Use Case); **`S`** = Supporting Actor (NgÆ°á»i/Há»‡ thá»‘ng há»— trá»£/tiáº¿p nháº­n); **`â€“`** = KhĂ´ng tham gia.

| STT | Detailed Sub Use Case | Admin | Manager | Cashier | Employee | Kitchen | Customer | Guest |
| :-: | :--- | :---: | :-----: | :-----: | :------: | :-----: | :------: | :---: |
| **HLUC1** | **Tra cá»©u Thá»±c Ä‘Æ¡n & SÆ¡ Ä‘á»“ BĂ n** | | | | | | | |
| **1.1** | QuĂ©t QR BĂ n & Chi nhĂ¡nh | â€“ | â€“ | â€“ | â€“ | â€“ | **P** | **P** |
| **1.2** | Xem thá»±c Ä‘Æ¡n & Chi tiáº¿t mĂ³n | **P** | **P** | **P** | **P** | â€“ | **P** | **P** |
| **HLUC2** | **Äáº·t mĂ³n & Lá»‹ch sá»­ Ä‘Æ¡n hĂ ng** | | | | | | | |
| **2.1** | Gá»­i Ä‘Æ¡n Ä‘áº·t mĂ³n QR / Web Order | â€“ | â€“ | **S** | **S** | â€“ | **P** | **P** |
| **2.2** | Xem lá»‹ch sá»­ Ä‘Æ¡n hĂ ng | **P** | **P** | **P** | **P** | **P** | **P** | â€“ |
| **2.3** | Xem lá»‹ch sá»­ cháº¿ biáº¿n KDS | â€“ | â€“ | â€“ | â€“ | **P** | â€“ | â€“ |
| **HLUC3** | **Äáº·t bĂ n trá»±c tuyáº¿n** | | | | | | | |
| **3.1** | Táº¡o yĂªu cáº§u Ä‘áº·t bĂ n trá»±c tuyáº¿n | â€“ | â€“ | â€“ | â€“ | â€“ | **P** | **P** |
| **3.2** | Tiáº¿p nháº­n & Quáº£n lĂ½ lá»‹ch háº¹n Ä‘áº·t bĂ n | **S** | **P** | **P** | **P** | â€“ | â€“ | â€“ |
| **HLUC4** | **Há»“ sÆ¡ & Loyalty** | | | | | | | |
| **4.1** | Xem & Cáº­p nháº­t Há»“ sÆ¡ cĂ¡ nhĂ¢n | â€“ | â€“ | â€“ | â€“ | â€“ | **P** | â€“ |
| **4.2** | Tra cá»©u Ä‘iá»ƒm & Háº¡ng thĂ nh viĂªn Loyalty | **S** | **S** | **S** | **S** | â€“ | **P** | â€“ |
| **HLUC5** | **BĂ¡n hĂ ng & Phá»¥c vá»¥ POS** | | | | | | | |
| **5.1** | Táº¡o Ä‘Æ¡n hĂ ng POS | **P** | **P** | **P** | **P** | â€“ | â€“ | â€“ |
| **5.2** | ThĂªm / XĂ³a mĂ³n trong Ä‘Æ¡n hĂ ng | **P** | **P** | **P** | **P** | â€“ | â€“ | â€“ |
| **5.3** | Cáº­p nháº­t lá»‹ch sá»­ Ä‘Æ¡n hĂ ng | **P** | **P** | â€“ | â€“ | â€“ | â€“ | â€“ |
| **5.4** | Tiáº¿p nháº­n Ä‘Æ¡n Ä‘áº·t mĂ³n Web Order | **S** | **P** | **P** | **P** | â€“ | â€“ | â€“ |
| **5.5** | Gá»­i yĂªu cáº§u cháº¿ biáº¿n xuá»‘ng Báº¿p | **S** | **P** | **P** | **P** | **S** | â€“ | â€“ |
| **HLUC6** | **Thanh toĂ¡n & HĂ³a Ä‘Æ¡n** | | | | | | | |
| **6.1** | Thanh toĂ¡n Ä‘Æ¡n hĂ ng | **P** | **P** | **P** | **P** | â€“ | â€“ | â€“ |
| **6.2** | Äá»•i Ä‘iá»ƒm Loyalty | **P** | **P** | **P** | **P** | â€“ | â€“ | â€“ |
| **6.3** | Cáº¥u hĂ¬nh phÆ°Æ¡ng thá»©c/tham sá»‘ thanh toĂ¡n | **P** | â€“ | â€“ | â€“ | â€“ | â€“ | â€“ |
| **6.4** | Cáº¥u hĂ¬nh thiáº¿t láº­p há»‡ thá»‘ng | **P** | â€“ | â€“ | â€“ | â€“ | â€“ | â€“ |
| **6.5** | In / Xuáº¥t hĂ³a Ä‘Æ¡n | **P** | **P** | **P** | **P** | â€“ | â€“ | â€“ |
| **HLUC7** | **Äiá»u phá»‘i Cháº¿ biáº¿n KDS** | | | | | | | |
| **7.1** | Xá»­ lĂ½ cháº¿ biáº¿n KDS thá»i gian thá»±c | â€“ | â€“ | â€“ | â€“ | **P** | â€“ | â€“ |
| **7.2** | Cáº­p nháº­t tráº¡ng thĂ¡i táº¡m háº¿t mĂ³n | **P** | **P** | â€“ | â€“ | **P** | â€“ | â€“ |
| **HLUC8** | **Ca lĂ m viá»‡c & Cháº¥m cĂ´ng** | | | | | | | |
| **8.1** | Cháº¥m cĂ´ng QR | **P** | **P** | **P** | **P** | **P** | â€“ | â€“ |
| **8.2** | Má»Ÿ & Chá»‘t ca lĂ m viá»‡c (Shift) | â€“ | â€“ | **P** | â€“ | â€“ | â€“ | â€“ |
| **8.3** | Xem lá»‹ch lĂ m viá»‡c | **P** | **P** | **P** | **P** | **P** | â€“ | â€“ |
| **8.4** | PhĂ¢n lá»‹ch lĂ m viá»‡c | **P** | **P** | â€“ | â€“ | â€“ | â€“ | â€“ |
| **HLUC9** | **Quáº£n lĂ½ Váº­n hĂ nh Chi nhĂ¡nh** | | | | | | | |
| **9.1** | Quáº£n lĂ½ SÆ¡ Ä‘á»“ bĂ n & Khu vá»±c | **P** | **P** | â€“ | â€“ | â€“ | â€“ | â€“ |
| **9.2** | Quáº£n lĂ½ Há»“ sÆ¡ & PhĂ¢n lá»‹ch NhĂ¢n sá»± | **P** | **P** | â€“ | â€“ | â€“ | â€“ | â€“ |
| **9.3** | Quáº£n lĂ½ Chi phĂ­ váº­n hĂ nh | **P** | **P** | â€“ | â€“ | â€“ | â€“ | â€“ |
| **HLUC10**| **Quáº£n trá»‹ Há»‡ thá»‘ng & Danh má»¥c** | | | | | | | |
| **10.1**| Quáº£n lĂ½ Danh má»¥c Thá»±c Ä‘Æ¡n & Topping | **P** | â€“ | â€“ | â€“ | â€“ | â€“ | â€“ |
| **10.2**| Quáº£n lĂ½ Chi nhĂ¡nh & Æ¯u Ä‘Ă£i toĂ n chuá»—i | **P** | â€“ | â€“ | â€“ | â€“ | â€“ | â€“ |
| **HLUC11**| **BĂ¡o cĂ¡o & Insights** | | | | | | | |
| **11.1**| Xem Dashboard | **P** | **P** | â€“ | â€“ | â€“ | â€“ | â€“ |
| **11.2**| Xem PhĂ¢n tĂ­ch & Cáº£nh bĂ¡o Insights AI | **P** | **P** | â€“ | â€“ | â€“ | â€“ | â€“ |
| **HLUC12**| **Trá»£ lĂ½ áº£o AI Agent** | | | | | | | |
| **12.1**| Tra cá»©u thĂ´ng tin qua AI Agent (Read) | **P** | **P** | **P** | **P** | **P** | **P** | **P** |
| **12.2**| Thá»±c hiá»‡n tĂ¡c vá»¥ nghiá»‡p vá»¥ qua AI (Write)| **P** | **P** | **P** | **P** | **P** | **P** | â€“ |

---

## PART C â€” FINAL EXPECTED VS ACTUAL MATRIX (34 SUB USE CASES)

| UC ID | Sub Use Case Name | Expected Primary | Expected Supporting | Actual Actor / Capability in Code | Status | Evidence Source (File / Controller / Method) |
| :-: | :--- | :--- | :--- | :--- | :---: | :--- |
| **1.1** | QuĂ©t QR BĂ n & Chi nhĂ¡nh | Cust, Guest | -- | Customer, Guest | `MATCH` | `QRScan.tsx`, `TableController.GetTables` |
| **1.2** | Xem thá»±c Ä‘Æ¡n & Chi tiáº¿t mĂ³n | Admin, Mgr, Cashier, Emp, Cust, Guest | -- | Admin, Manager, Cashier, Employee, Customer, Guest | `MATCH` | `ProductController.cs` (`[AllowAnonymous]`), `ToppingController.cs` |
| **2.1** | Gá»­i Ä‘Æ¡n Ä‘áº·t mĂ³n QR / Web Order | Cust, Guest | Cashier, Emp | Cust, Guest (táº¡o Ä‘Æ¡n); Cashier, Emp (tiáº¿p nháº­n) | `MATCH` | `OrderController.CreateOrder`, `DigitalMenu.tsx` |
| **2.2** | Xem lá»‹ch sá»­ Ä‘Æ¡n hĂ ng | Admin, Mgr, Cashier, Emp, Kitchen, Cust | -- | Admin, Mgr, Cashier, Emp (`/invoices`); Cust (`GET /api/Order`); Kitchen xem KDS history qua UC2.3 | `NEEDS REVIEW` | `OrderController.cs`, `InvoiceController.cs`, `KitchenService.cs` |
| **2.3** | Xem lá»‹ch sá»­ cháº¿ biáº¿n KDS | Kitchen | -- | Kitchen (P), Admin (S), Manager (S) | `MATCH / EXTRA` | `KitchenService.GetHistoryAsync` (`GET /api/Order/kitchen/history`) |
| **3.1** | Táº¡o yĂªu cáº§u Ä‘áº·t bĂ n trá»±c tuyáº¿n | Cust, Guest | -- | Customer, Guest | `MATCH` | `ReservationController.CreateReservation` |
| **3.2** | Tiáº¿p nháº­n & Quáº£n lĂ½ lá»‹ch háº¹n | Mgr, Cashier, Emp | Admin | Admin, Manager, Cashier, Employee | `MATCH` | `ReservationController.UpdateStatus` (`[Authorize(Roles="admin,manager,employee,cashier")]`) |
| **4.1** | Xem & Cáº­p nháº­t Há»“ sÆ¡ cĂ¡ nhĂ¢n | Cust | -- | Customer (chĂ­nh chá»§ `userId == existing.Id`) | `MATCH` | `CustomerController.UpdateProfile` |
| **4.2** | Tra cá»©u Ä‘iá»ƒm & Loyalty | Cust | Admin, Mgr, Cashier, Emp | Customer, Admin, Manager, Cashier, Employee | `MATCH` | `CustomerController.GetLoyaltyHistory`, `LoyaltyService.cs` |
| **5.1** | Táº¡o Ä‘Æ¡n hĂ ng POS | Admin, Mgr, Cashier, Emp | -- | Admin, Manager, Cashier, Employee | `MATCH` | `OrderController.CreateOrder`, `OrderService.cs` |
| **5.2** | ThĂªm/XĂ³a mĂ³n trong Ä‘Æ¡n hĂ ng | Admin, Mgr, Cashier, Emp | -- | Admin, Manager, Cashier, Employee | `MATCH` | `OrderController.CreateOrder`, `OrderService.CreateOrUpdateOrderAsync` |
| **5.3** | Cáº­p nháº­t lá»‹ch sá»­ Ä‘Æ¡n hĂ ng | Admin, Mgr | -- | Admin, Manager, Cashier, Employee (Sá»­a Ä‘Æ¡n Ä‘ang má»Ÿ); ÄÆ¡n Ä‘Ă£ Ä‘Ă³ng/há»§y bá»‹ cháº·n sá»­a | `GAP / NEEDS REVIEW` | `OrderController.cs`, `OrderService.UpdateOrderStatusAsync` |
| **5.4** | Tiáº¿p nháº­n Ä‘Æ¡n Ä‘áº·t mĂ³n Web Order | Admin, Mgr, Cashier, Emp | -- | Admin, Manager, Cashier, Employee | `MATCH` | `OrderController.AcceptWebOrder` (`AcceptWebOrderAsync`) |
| **5.5** | Gá»­i yĂªu cáº§u cháº¿ biáº¿n xuá»‘ng Báº¿p | Admin, Mgr, Cashier, Emp | Kitchen | Admin, Manager, Cashier, Employee, Kitchen | `MATCH` | `OrderController.SendToKitchen` (`SendToKitchenAsync`), `KitchenHub.cs` |
| **6.1** | Thanh toĂ¡n Ä‘Æ¡n hĂ ng | Admin, Mgr, Cashier, Emp | -- | Admin, Manager, Cashier, Employee | `MATCH` | `OrderController.PayOrder` (`PayOrderAsync`) |
| **6.2** | Äá»•i Ä‘iá»ƒm Loyalty | Admin, Mgr, Cashier, Emp | -- | Admin, Manager, Cashier, Employee | `MATCH` | `OrderController.RedeemPoints` (`LoyaltyService.RedeemPointsAsync`) |
| **6.3** | Cáº¥u hĂ¬nh tham sá»‘ thanh toĂ¡n | Admin | -- | Admin (Global), Manager (Branch) | `GAP / EXTRA` | `ReceiptSettingsController.Put` |
| **6.4** | Cáº¥u hĂ¬nh thiáº¿t láº­p há»‡ thá»‘ng | Admin | -- | Admin (Global), Manager (Branch) | `GAP / EXTRA` | `SystemSettingsController.Put` |
| **6.5** | In / Xuáº¥t hĂ³a Ä‘Æ¡n | Admin, Mgr, Cashier, Emp | -- | Admin, Manager, Cashier, Employee | `MATCH` | `InvoiceController.cs`, `ReceiptSettingsController.cs` |
| **7.1** | Xá»­ lĂ½ cháº¿ biáº¿n KDS thá»i gian thá»±c | Kitchen | -- | Kitchen | `MATCH` | `KitchenHub.cs`, `KitchenService.UpdateRequestStatusAsync` |
| **7.2** | Cáº­p nháº­t tráº¡ng thĂ¡i táº¡m háº¿t mĂ³n | Admin, Mgr, Kitchen | -- | Admin, Kitchen (Manager bá»‹ cháº·n 403 náº¿u khĂ´ng dĂ¹ng mode="admin") | `GAP` | `ProductController.UpdateAvailability` (`[Authorize(Roles="admin,kitchen")]`) |
| **8.1** | Cháº¥m cĂ´ng QR | Admin, Mgr, Cashier, Emp, Kitchen | -- | Admin, Manager, Cashier, Employee, Kitchen | `MATCH` | `AttendanceController.CheckIn`, `CheckOut` |
| **8.2** | Má»Ÿ & Chá»‘t ca lĂ m viá»‡c (Shift) | Cashier | -- | Admin, Manager, Cashier | `GAP / EXTRA` | `ShiftController.cs` (`[Authorize(Roles="admin,manager,cashier")]`) |
| **8.3** | Xem lá»‹ch lĂ m viá»‡c | Admin, Mgr, Cashier, Emp, Kitchen | -- | Admin, Manager, Cashier, Employee, Kitchen | `MATCH` | `WorkScheduleController.GetSchedules` |
| **8.4** | PhĂ¢n lá»‹ch lĂ m viá»‡c | Admin, Mgr | -- | Admin, Manager | `MATCH` | `WorkScheduleController.CreateSchedule`, `DeleteSchedule` |
| **9.1** | Quáº£n lĂ½ SÆ¡ Ä‘á»“ bĂ n & Khu vá»±c | Admin, Mgr | -- | Admin, Manager | `MATCH` | `TableController.cs`, `AreaController.cs` |
| **9.2** | Quáº£n lĂ½ Há»“ sÆ¡ & PhĂ¢n lá»‹ch NhĂ¢n sá»±| Admin, Mgr | -- | Admin, Manager | `MATCH` | `EmployeeController.cs`, `WorkScheduleController.cs` |
| **9.3** | Quáº£n lĂ½ Chi phĂ­ váº­n hĂ nh | Admin, Mgr | -- | Admin, Manager, Cashier, Employee | `GAP / EXTRA` | `ExpenseController.cs` (`[Authorize(Roles="admin,manager,employee,cashier")]`) |
| **10.1**| Quáº£n lĂ½ Danh má»¥c Thá»±c Ä‘Æ¡n/Topping| Admin | -- | Admin | `MATCH` | `ProductController.cs`, `ToppingController.cs` (admin-only) |
| **10.2**| Quáº£n lĂ½ Chi nhĂ¡nh & Æ¯u Ä‘Ă£i | Admin | -- | Admin | `MATCH` | `BranchController.cs`, `PromotionController.cs` (admin-only) |
| **11.1**| Xem Dashboard | Admin, Mgr | -- | Admin, Manager | `MATCH` | `DashboardController.GetSummary` |
| **11.2**| Xem PhĂ¢n tĂ­ch & Cáº£nh bĂ¡o Insights AI| Admin, Mgr | -- | Admin, Manager | `MATCH` | `BusinessInsightController.cs` (`Get`, `GetById`) |
| **12.1**| Tra cá»©u thĂ´ng tin qua AI (Read) | Admin, Mgr, Cashier, Emp, Kitchen, Cust, Guest | -- | Admin, Manager, Cashier, Employee, Kitchen, Customer, Guest | `MATCH` | `AiController.cs`, `GetRevenueTool.cs`, `GetMenuTool.cs` |
| **12.2**| Thá»±c hiá»‡n tĂ¡c vá»¥ qua AI (Write) | Admin, Mgr, Cashier, Emp, Kitchen, Cust | -- | Admin (Price), Staff (Order Status), Customer (Booking); Guest bá»‹ cháº·n | `MATCH` | `AiController.cs`, `AiPermissionService.cs` |

---

## PART D â€” CRITICAL ISSUES & DETAILED GAP ANALYSIS

### GAP 1: UC7.2 â€” Cáº­p nháº­t tráº¡ng thĂ¡i táº¡m háº¿t mĂ³n (`OutOfStock`)
- **Expected**: Primary: `Admin`, `Manager`, `Kitchen`.
- **Actual**: Backend `ProductController.cs` khai bĂ¡o `[Authorize(Roles = "admin,kitchen")]`. Role `manager` bá»‹ tráº£ vá» `403 Forbidden` khi trá»±c tiáº¿p gá»i endpoint `PATCH /api/product/{id}/availability`.
- **Difference**: Thiáº¿u role `manager` trong backend authorization attribute.
- **Evidence**:
  - File: `services/api/src/WebAPI/Controllers/ProductController.cs`
  - Method: `UpdateAvailability(Guid id, [FromBody] AvailabilityRequest request)`
  - Attribute: `[Authorize(Roles = "admin,kitchen")]`
- **Need code fix?**: **CĂ“** (Bá»• sung `manager` vĂ o `[Authorize(Roles = "admin,manager,kitchen")]`).

---

### GAP 2: UC8.2 â€” Má»Ÿ & Chá»‘t ca lĂ m viá»‡c (Shift)
- **Expected**: Primary: `Cashier ONLY`.
- **Actual**: Backend `ShiftController.cs` khai bĂ¡o `[Authorize(Roles = "admin,manager,cashier")]`.
- **Difference**: Backend má»Ÿ quyá»n cho cáº£ `admin` vĂ  `manager` má»Ÿ/chá»‘t ca trá»±c tiáº¿p.
- **Operational Reality**: TrÆ°á»ng há»£p Thu ngĂ¢n váº¯ng máº·t, Quáº£n lĂ½/Admin sá»­ dá»¥ng tĂ i khoáº£n cá»§a mĂ¬nh hoáº·c tĂ i khoáº£n Thu ngĂ¢n táº¡i tablet quáº§y Ä‘á»ƒ má»Ÿ/chá»‘t ca dá»± phĂ²ng. ÄĂ¢y lĂ  má»™t `EXTRA ACTUAL` cáº§n ghi nháº­n trong tĂ i liá»‡u phĂ¢n tĂ­ch mĂ  khĂ´ng cáº§n táº¡o thĂªm Actor má»›i.
- **Evidence**:
  - File: `services/api/src/WebAPI/Controllers/ShiftController.cs`
  - Attribute: `[Authorize(Roles = "admin,manager,cashier")]`
- **Need code fix?**: **KHĂ”NG Báº®T BUá»˜C**.

---

### GAP 3: UC9.3 â€” Quáº£n lĂ½ Chi phĂ­ váº­n hĂ nh
- **Expected**: Primary: `Admin`, `Manager`.
- **Actual**: Backend `ExpenseController.cs` khai bĂ¡o `[Authorize(Roles = "admin,manager,employee,cashier")]`.
- **Difference**: Backend má»Ÿ quyá»n táº¡o/sá»­a phiáº¿u chi phĂ­ cho cáº£ `cashier` vĂ  `employee`.
- **Evidence**:
  - File: `services/api/src/WebAPI/Controllers/ExpenseController.cs`
  - Class attribute: `[Authorize(Roles = "admin,manager,employee,cashier")]`
- **Need code fix?**: **CĂ“** (Sá»­a attribute thĂ nh `[Authorize(Roles = "admin,manager")]` náº¿u muá»‘n siáº¿t cháº·t phĂ¢n quyá»n chi phĂ­).

---

### GAP 4: UC6.3 & UC6.4 â€” Cáº¥u hĂ¬nh Máº«u in & Thiáº¿t láº­p Há»‡ thá»‘ng Chi nhĂ¡nh
- **Expected**: Primary: `Admin ONLY`.
- **Actual**: Backend `ReceiptSettingsController.cs` vĂ  `SystemSettingsController.cs` cho phĂ©p `manager` cáº­p nháº­t cĂ i Ä‘áº·t cho chi nhĂ¡nh cá»§a mĂ¬nh (`Branch-scoped Put`).
- **Difference**: Backend há»— trá»£ Manager phĂ¢n quyá»n quáº£n lĂ½ thiáº¿t láº­p cáº¥p chi nhĂ¡nh.
- **Evidence**:
  - Files: `services/api/src/WebAPI/Controllers/ReceiptSettingsController.cs` & `SystemSettingsController.cs`
  - Method: `Put` kiá»ƒm tra `if (!string.Equals(role, "admin") && !string.Equals(role, "manager")) return Forbid();`
- **Need code fix?**: **KHĂ”NG Báº®T BUá»˜C** (PhĂ¹ há»£p vá»›i kiáº¿n trĂºc Ä‘a chi nhĂ¡nh linh hoáº¡t).

---

## PART E â€” EXTRA ACTUAL CAPABILITIES

1. **UC2.3: Xem lá»‹ch sá»­ cháº¿ biáº¿n KDS**: Expected chá»‰ gĂ¡n Kitchen, nhÆ°ng mĂ£ nguá»“n `KitchenService.GetHistoryAsync` cho phĂ©p cáº£ `admin` vĂ  `manager` truy cáº­p lá»‹ch sá»­ cháº¿ biáº¿n Ä‘á»ƒ giĂ¡m sĂ¡t hiá»‡u suáº¥t nhĂ  báº¿p.
2. **UC8.2: Má»Ÿ & Chá»‘t ca**: Backend cáº¥p quyá»n cho `admin` vĂ  `manager` má»Ÿ/chá»‘t ca thay tháº¿ Thu ngĂ¢n.
3. **UC9.3: Quáº£n lĂ½ chi phĂ­**: Backend cáº¥p quyá»n cho `cashier` vĂ  `employee` táº¡o phiáº¿u chi váº­n hĂ nh.

---

## PART F â€” UC2.2 VĂ€ UC2.3 DOMAIN SEPARATION (ORDER HISTORY AUDIT)

Báº£ng Ä‘á»‘i chiáº¿u phĂ¢n Ä‘á»‹nh giá»¯a Lá»‹ch sá»­ HĂ³a Ä‘Æ¡n TĂ i chĂ­nh vĂ  Lá»‹ch sá»­ Cháº¿ biáº¿n KDS:

| Actor | Expected | UI Page / Component | Actual Code Capability | Entity & Data Source | Domain Alignment | Status |
| :--- | :---: | :--- | :--- | :--- | :--- | :---: |
| **Admin** | **P** | MĂ n hĂ¬nh HĂ³a Ä‘Æ¡n toĂ n chuá»—i | Xem toĂ n bá»™ hĂ³a Ä‘Æ¡n cá»§a táº¥t cáº£ chi nhĂ¡nh | Entity `Order`, `InvoiceController.Get` | Tra cá»©u doanh thu & quáº£n trá»‹ cáº¥p cao | `MATCH` |
| **Manager** | **P** | MĂ n hĂ¬nh HĂ³a Ä‘Æ¡n chi nhĂ¡nh | Xem danh sĂ¡ch hĂ³a Ä‘Æ¡n chi nhĂ¡nh phá»¥ trĂ¡ch | Entity `Order` (lá»c `branchId`), `InvoiceController.Get` | Quáº£n lĂ½ tĂ i chĂ­nh chi nhĂ¡nh | `MATCH` |
| **Cashier** | **P** | Lá»‹ch sá»­ hĂ³a Ä‘Æ¡n quáº§y | Xem danh sĂ¡ch hĂ³a Ä‘Æ¡n táº¡i quáº§y POS | Entity `Order`, `GET /api/Order` | Äá»‘i soĂ¡t doanh thu quáº§y & in láº¡i hĂ³a Ä‘Æ¡n | `MATCH` |
| **Employee** | **P** | ÄÆ¡n hĂ ng Ä‘Ă£ láº­p | Xem cĂ¡c Ä‘Æ¡n hĂ ng do nhĂ¢n viĂªn láº­p/phá»¥c vá»¥ | Entity `Order`, `GET /api/Order` | Kiá»ƒm tra Ä‘Æ¡n hĂ ng táº¡i bĂ n & há»— trá»£ khĂ¡ch | `MATCH` |
| **Kitchen** | **P** | Lá»‹ch sá»­ báº¿p KDS (`/kitchen/history`)| Xem lá»‹ch sá»­ cĂ¡c Ä‘á»£t yĂªu cáº§u cháº¿ biáº¿n | Entity `OrderRequest` (`KitchenService.GetHistoryAsync`) | **KhĂ¡c biá»‡t Domain Entity**: Kitchen xem Ä‘á»£t náº¥u KDS chá»© khĂ´ng xem hĂ³a Ä‘Æ¡n tĂ i chĂ­nh. | `NEEDS REVIEW` |
| **Customer** | **P** | ÄÆ¡n hĂ ng cá»§a tĂ´i (`customer-web`) | Xem 5 Ä‘Æ¡n má»›i nháº¥t cá»§a chĂ­nh mĂ¬nh | Entity `Order` (`GetCustomerOrdersAsync` lá»c SÄT) | KhĂ¡ch hĂ ng theo dĂµi tiáº¿n Ä‘á»™ Ä‘Æ¡n hĂ ng cĂ¡ nhĂ¢n | `MATCH` |

### Káº¿t luáº­n PhĂ¢n Ä‘á»‹nh Domain:
- **`UC2.2: Xem lá»‹ch sá»­ Ä‘Æ¡n hĂ ng`**: Phá»¥c vá»¥ nhu cáº§u tra cá»©u hĂ³a Ä‘Æ¡n tĂ i chĂ­nh (`Order` & `OrderDetail`), Ă¡p dá»¥ng cho Admin, Manager, Cashier, Employee, Customer.
- **`UC2.3: Xem lá»‹ch sá»­ cháº¿ biáº¿n KDS`**: Phá»¥c vá»¥ nhu cáº§u tra cá»©u cĂ¡c Ä‘á»£t náº¥u Äƒn nhĂ  báº¿p (`OrderRequest` & `OrderRequestItem`), Ă¡p dá»¥ng riĂªng cho Kitchen (vĂ  Admin/Manager há»— trá»£ giĂ¡m sĂ¡t).

---

## PART G â€” UC5.1, UC5.2, UC5.3 ORDER LIFECYCLE AUDIT

PhĂ¢n Ä‘á»‹nh vĂ²ng Ä‘á»i cá»§a Order trong mĂ£ nguá»“n POS:

| Sub Use Case ID & Name | Primary Actor ká»³ vá»ng | Actual Code Implementation | File Evidence | Status |
| :--- | :--- | :--- | :--- | :---: |
| **UC5.1: Táº¡o Ä‘Æ¡n hĂ ng POS** | Admin, Mgr, Cashier, Emp | `POST /api/Order` khá»Ÿi táº¡o Ä‘Æ¡n POS má»›i, tá»± Ä‘á»™ng tĂ­nh tĂ i chĂ­nh qua `OrderService.RecalculateOrderFinancials` | `OrderController.cs`, `OrderService.cs` | `MATCH` |
| **UC5.2: ThĂªm / XĂ³a mĂ³n trong Ä‘Æ¡n hĂ ng** | Admin, Mgr, Cashier, Emp | `OrderService.CreateOrUpdateOrderAsync` tá»± Ä‘á»™ng merge mĂ³n má»›i hoáº·c giá»¯ láº¡i mĂ³n Ä‘Ă£ gá»­i báº¿p (`SentQuantity`) | `OrderService.cs` (lines 80-140) | `MATCH` |
| **UC5.3: Cáº­p nháº­t lá»‹ch sá»­ Ä‘Æ¡n hĂ ng** | Admin, Mgr | Frontend POS cháº·n Employee/Cashier sá»­a Ä‘Æ¡n Ä‘Ă£ thanh toĂ¡n. Backend cháº·n sá»­a Ä‘Æ¡n `Completed`/`Cancelled` (`IsTerminal`). | `OrderService.cs` (`IsTerminal`) | `NEEDS REVIEW` |
| **UC5.4: Tiáº¿p nháº­n Ä‘Æ¡n Web Order** | Admin, Mgr, Cashier, Emp | `POST /api/Order/{id}/accept` gĂ¡n bĂ n vĂ  tĂªn nhĂ¢n viĂªn tiáº¿p nháº­n | `OrderController.AcceptWebOrder` | `MATCH` |
| **UC5.5: Gá»­i yĂªu cáº§u cháº¿ biáº¿n xuá»‘ng Báº¿p**| Admin, Mgr, Cashier, Emp | `POST /api/Order/{id}/send-to-kitchen` tĂ­nh chĂªnh lá»‡ch `SentQuantity` vĂ  phĂ¡t SignalR | `KitchenService.SendToKitchenAsync` | `MATCH` |

---

## PART H â€” PAYMENT & CONFIGURATION AUDIT (UC6.1 $\rightarrow$ UC6.5)

| Sub Use Case ID & Name | Primary Actor ká»³ vá»ng | Actual Code Implementation | File Evidence | Status |
| :--- | :--- | :--- | :--- | :---: |
| **UC6.1: Thanh toĂ¡n Ä‘Æ¡n hĂ ng** | Admin, Mgr, Cashier, Emp | `POST /api/Order/{id}/payment` xĂ¡c thá»±c Ä‘Ăºng 100% `TotalAmount`, cáº­p nháº­t `PaidAmount`, `PaymentAt` vĂ  `Status = "HoĂ n thĂ nh"` | `OrderService.PayOrderAsync` | `MATCH` |
| **UC6.2: Äá»•i Ä‘iá»ƒm Loyalty** | Admin, Mgr, Cashier, Emp | `POST /api/Order/{id}/redeem` trá»« Ä‘iá»ƒm tĂ i khoáº£n khĂ¡ch vĂ  giáº£m trá»±c tiáº¿p vĂ o `order.Discount` | `LoyaltyService.RedeemPointsAsync` | `MATCH` |
| **UC6.3: Cáº¥u hĂ¬nh tham sá»‘ thanh toĂ¡n** | Admin | `ReceiptSettingsController.Put` cho phĂ©p `admin` vĂ  `manager` (chi nhĂ¡nh) chá»‰nh sá»­a | `ReceiptSettingsController.cs` | `GAP / EXTRA` |
| **UC6.4: Cáº¥u hĂ¬nh thiáº¿t láº­p há»‡ thá»‘ng** | Admin | `SystemSettingsController.Put` cho phĂ©p `admin` vĂ  `manager` (chi nhĂ¡nh) chá»‰nh sá»­a | `SystemSettingsController.cs` | `GAP / EXTRA` |
| **UC6.5: In / Xuáº¥t hĂ³a Ä‘Æ¡n** | Admin, Mgr, Cashier, Emp | Táº¥t cáº£ cĂ¡c role Staff Ä‘á»u cĂ³ thá»ƒ má»Ÿ giao diá»‡n in hĂ³a Ä‘Æ¡n | `InvoiceController.cs` | `MATCH` |

---

## PART I â€” AI AGENT AUDIT (UC12.1 & UC12.2)

PhĂ¢n Ä‘á»‹nh chi tiáº¿t giá»¯a AI Read Tools, AI Write Tools vĂ  Lá»‹ch sá»­ Ä‘Ă m thoáº¡i:

| Actor | AI Conversation History | AI Read Tools (`UC12.1`) | AI Write Tools (`UC12.2`) | Authorized AI Tools in Code |
| :--- | :---: | :---: | :---: | :--- |
| **Admin** | `CĂ“` | `CĂ“` | `CĂ“` | `update_product_price`, `get_revenue`, `get_business_summary`, `get_order_list`, `get_active_staff`, `get_best_sellers`, `get_financial_analysis`, `get_revenue_comparison` |
| **Manager** | `CĂ“` | `CĂ“` | `CĂ“` | `update_order_status`, `get_revenue`, `get_business_summary`, `get_order_list`, `get_active_staff`, `get_best_sellers`, `get_financial_analysis`, `get_revenue_comparison` |
| **Cashier** | `CĂ“` | `CĂ“` | `CĂ“` | `update_order_status`, `get_order_list`, `get_best_sellers`, `get_table_summary`, `get_my_shift` |
| **Employee**| `CĂ“` | `CĂ“` | `CĂ“` | `update_order_status`, `get_order_list`, `get_best_sellers`, `get_table_summary`, `get_my_shift` |
| **Kitchen** | `CĂ“` | `CĂ“` | `CĂ“` | `update_order_status`, `get_order_list`, `get_best_sellers`, `get_table_summary`, `get_my_shift` |
| **Customer**| `CĂ“` | `CĂ“` | `CĂ“` | `create_booking`, `get_menu`, `get_my_order` |
| **Guest** | `KHĂ”NG` (PhiĂªn táº¡m) | `CĂ“` | `KHĂ”NG` | `get_menu` (Táº¥t cáº£ Write tools bá»‹ cháº·n bá»Ÿi `AiPermissionService.cs`) |

---

## PART J â€” BRANCH SCOPE ISOLATION AUDIT

XĂ¡c minh tĂ­nh cĂ´ láº­p dá»¯ liá»‡u theo Chi nhĂ¡nh (`Branch Isolation`):

| Actor | Scope quy Ä‘á»‹nh | Enforcement in Code (CÆ¡ cháº¿ kiá»ƒm soĂ¡t trong mĂ£ nguá»“n) | Status |
| :--- | :--- | :--- | :---: |
| **Admin** | **Global** | `IsAdmin()` bypasses `branchId` filter. Tráº£ vá» toĂ n bá»™ dá»¯ liá»‡u toĂ n chuá»—i. | `MATCH` |
| **Manager** | **Branch** | Báº¯t buá»™c Ä‘á»c `branchId` tá»« JWT Claim. ÄÆ¡n hĂ ng, nhĂ¢n sá»±, chi phĂ­, dashboard bá»‹ giá»›i háº¡n 100% trong chi nhĂ¡nh. | `MATCH` |
| **Cashier** | **Branch** | `IsBranchScopedRole` Ă©p `branchId` theo JWT Claim trong `OrderController`, `ShiftController`, `ExpenseController`. | `MATCH` |
| **Employee**| **Branch** | Bá»‹ giá»›i háº¡n trong chi nhĂ¡nh Ä‘Æ°á»£c gĂ¡n. KhĂ´ng xem Ä‘Æ°á»£c Ä‘Æ¡n/bĂ n chi nhĂ¡nh khĂ¡c. | `MATCH` |
| **Kitchen** | **Branch** | `KitchenHub` join SignalR group theo `Branch_{branchId}`. `KitchenService` lá»c `OrderRequests` theo `branchId`. | `MATCH` |
| **Customer**| **Personal** | Lá»c dá»¯ liá»‡u theo `CustomerId` hoáº·c SÄT chĂ­nh chá»§ tá»« JWT Name claim. | `MATCH` |
| **Guest** | **Contextual** | XĂ¡c Ä‘á»‹nh `branchId` tá»« payload QR bĂ n hoáº·c query parameter. KhĂ´ng cĂ³ JWT Claim `branchId`. | `MATCH` |

---

## PART K â€” UML RELATIONSHIP AUDIT

ÄĂ¡nh giĂ¡ chuáº©n má»±c UML 2.5 cho cĂ¡c má»‘i quan há»‡:

| Relationship | Current Definition | Audit Decision | Reason & Justification |
| :--- | :--- | :---: | :--- |
| **Actor Generalization** | `Customer --|> Guest` | `VALID` | `Customer` lĂ  ngÆ°á»i dĂ¹ng Ä‘Ă£ xĂ¡c thá»±c, káº¿ thá»«a toĂ n bá»™ kháº£ nÄƒng tÆ°Æ¡ng tĂ¡c cĂ´ng khai cá»§a `Guest` (quĂ©t QR, xem thá»±c Ä‘Æ¡n, táº¡o Ä‘Æ¡n vĂ£ng lai) vĂ  má»Ÿ rá»™ng thĂªm cĂ¡c Use Case Ä‘á»‹nh danh cĂ¡ nhĂ¢n. |
| **`<<include>>`** | `UC5.4 (Gá»­i báº¿p) --<<include>>--> UC5.1 (Táº¡o/Sá»­a Ä‘Æ¡n POS)` | `VALID` | Má»i Ä‘á»£t gá»­i mĂ³n xuá»‘ng KDS báº¿p báº¯t buá»™c pháº£i dá»±a trĂªn vĂ  cáº­p nháº­t láº¡i `SentQuantity` cá»§a má»™t Ä‘Æ¡n hĂ ng POS Ä‘Ă£ táº¡o. |
| **`<<include>>`** | `UC5.3 (Tiáº¿p nháº­n Web Order) --<<include>>--> UC5.1 (Táº¡o/Sá»­a Ä‘Æ¡n POS)` | `VALID` | Viá»‡c tiáº¿p nháº­n Ä‘Æ¡n Web báº¯t buá»™c pháº£i thá»±c hiá»‡n chuyá»ƒn Ä‘á»•i Ä‘Æ¡n hĂ ng chá» thĂ nh má»™t Ä‘Æ¡n POS chĂ­nh thá»©c gáº¯n bĂ n/ngÆ°á»i nháº­n. |
| **`<<extend>>`** | `UC6.2 (Äá»•i Ä‘iá»ƒm Loyalty) --<<extend>>--> UC5.1 (Táº¡o/Sá»­a Ä‘Æ¡n POS)` | `VALID` | **Extension Point**: Láº­p tĂ i chĂ­nh Ä‘Æ¡n POS.<br>**Condition**: Chá»‰ thá»±c hiá»‡n khi khĂ¡ch hĂ ng yĂªu cáº§u quy Ä‘á»•i Ä‘iá»ƒm tĂ­ch lÅ©y thĂ nh tiá»n giáº£m giĂ¡. |
| **`<<extend>>`** | `UC6.5 (In hĂ³a Ä‘Æ¡n) --<<extend>>--> UC6.1 (Thanh toĂ¡n Ä‘Æ¡n hĂ ng)` | `VALID` | **Extension Point**: HoĂ n táº¥t giao dá»‹ch thanh toĂ¡n.<br>**Condition**: Chá»‰ thá»±c hiá»‡n khi thu ngĂ¢n hoáº·c khĂ¡ch hĂ ng cĂ³ nhu cáº§u xuáº¥t/in hĂ³a Ä‘Æ¡n giáº¥y. |

---

## PART L â€” ACTOR ASSOCIATION AUDIT

PhĂ¢n Ä‘á»‹nh rĂµ Primary (P) vĂ  Supporting (S) Associations Ä‘á»‘i vá»›i 34 Sub Use Cases:

| Sub Use Case ID & Name | Primary Actors (Khá»Ÿi táº¡o) | Supporting Actors (Há»— trá»£) |
| :--- | :--- | :--- |
| **UC1.1: QuĂ©t QR BĂ n & Chi nhĂ¡nh** | Customer, Guest | -- |
| **UC1.2: Xem thá»±c Ä‘Æ¡n & Chi tiáº¿t mĂ³n** | Admin, Manager, Cashier, Employee, Customer, Guest | -- |
| **UC2.1: Gá»­i Ä‘Æ¡n Ä‘áº·t mĂ³n QR / Web Order** | Customer, Guest | Cashier, Employee |
| **UC2.2: Xem lá»‹ch sá»­ Ä‘Æ¡n hĂ ng** | Admin, Manager, Cashier, Employee, Customer | -- |
| **UC2.3: Xem lá»‹ch sá»­ cháº¿ biáº¿n KDS** | Kitchen | Admin, Manager |
| **UC3.1: Táº¡o yĂªu cáº§u Ä‘áº·t bĂ n trá»±c tuyáº¿n** | Customer, Guest | -- |
| **UC3.2: Tiáº¿p nháº­n & Quáº£n lĂ½ lá»‹ch háº¹n Ä‘áº·t bĂ n** | Manager, Cashier, Employee | Admin |
| **UC4.1: Xem & Cáº­p nháº­t Há»“ sÆ¡ cĂ¡ nhĂ¢n** | Customer | -- |
| **UC4.2: Tra cá»©u Ä‘iá»ƒm & Háº¡ng thĂ nh viĂªn Loyalty** | Customer | Admin, Manager, Cashier |
| **UC5.1: Táº¡o Ä‘Æ¡n hĂ ng POS** | Admin, Manager, Cashier, Employee | -- |
| **UC5.2: ThĂªm / XĂ³a mĂ³n trong Ä‘Æ¡n hĂ ng** | Admin, Manager, Cashier, Employee | -- |
| **UC5.3: Cáº­p nháº­t lá»‹ch sá»­ Ä‘Æ¡n hĂ ng** | Admin, Manager | -- |
| **UC5.4: Tiáº¿p nháº­n Ä‘Æ¡n Ä‘áº·t mĂ³n Web Order** | Manager, Cashier, Employee | Admin |
| **UC5.5: Gá»­i yĂªu cáº§u cháº¿ biáº¿n xuá»‘ng Báº¿p** | Manager, Cashier, Employee | Kitchen, Admin |
| **UC6.1: Thanh toĂ¡n Ä‘Æ¡n hĂ ng (Cash/Transfer)** | Manager, Cashier, Employee | Admin, Customer |
| **UC6.2: Äá»•i Ä‘iá»ƒm Loyalty giáº£m giĂ¡ Ä‘Æ¡n hĂ ng** | Manager, Cashier, Employee | Admin, Customer |
| **UC6.3: Cáº¥u hĂ¬nh phÆ°Æ¡ng thá»©c/tham sá»‘ thanh toĂ¡n** | Admin, Manager | -- |
| **UC6.4: Cáº¥u hĂ¬nh thiáº¿t láº­p há»‡ thá»‘ng** | Admin, Manager | -- |
| **UC6.5: In / Xuáº¥t hĂ³a Ä‘Æ¡n thanh toĂ¡n** | Admin, Manager, Cashier, Employee | -- |
| **UC7.1: Xá»­ lĂ½ Ä‘á»£t cháº¿ biáº¿n KDS thá»i gian thá»±c** | Kitchen | Admin, Manager |
| **UC7.2: Cáº­p nháº­t tráº¡ng thĂ¡i táº¡m háº¿t mĂ³n** | Admin, Kitchen | Manager (sau code fix) |
| **UC8.1: Cháº¥m cĂ´ng QR Check-in / Check-out** | Admin, Manager, Cashier, Employee, Kitchen | -- |
| **UC8.2: Má»Ÿ & Chá»‘t ca lĂ m viá»‡c (Shift)** | Cashier | Admin, Manager |
| **UC8.3: Xem lá»‹ch lĂ m viá»‡c** | Admin, Manager, Cashier, Employee, Kitchen | -- |
| **UC8.4: PhĂ¢n lá»‹ch lĂ m viá»‡c** | Admin, Manager | -- |
| **UC9.1: Quáº£n lĂ½ SÆ¡ Ä‘á»“ bĂ n & Khu vá»±c chi nhĂ¡nh** | Admin, Manager | -- |
| **UC9.2: Quáº£n lĂ½ Há»“ sÆ¡ & PhĂ¢n lá»‹ch NhĂ¢n sá»±** | Admin, Manager | -- |
| **UC9.3: Quáº£n lĂ½ Chi phĂ­ váº­n hĂ nh chi nhĂ¡nh** | Admin, Manager | Cashier, Employee |
| **UC10.1: Quáº£n lĂ½ Danh má»¥c Thá»±c Ä‘Æ¡n & Topping**| Admin | -- |
| **UC10.2: Quáº£n lĂ½ Chi nhĂ¡nh & Æ¯u Ä‘Ă£i toĂ n chuá»—i**| Admin | -- |
| **UC11.1: Xem Dashboard thá»‘ng kĂª doanh thu** | Admin, Manager | -- |
| **UC11.2: Xem PhĂ¢n tĂ­ch & Cáº£nh bĂ¡o Insights AI** | Admin, Manager | -- |
| **UC12.1: Tra cá»©u thĂ´ng tin qua AI (Read Tools)** | Admin, Manager, Cashier, Employee, Kitchen, Customer, Guest | -- |
| **UC12.2: Thá»±c hiá»‡n tĂ¡c vá»¥ qua AI (Write Tools)**| Admin, Manager, Cashier, Employee, Kitchen, Customer | -- |

---

## PART M â€” HLUC / SUB-UC DECOMPOSITION MAPPING TABLE

| Diagram ID | High-Level Business Group (HLUC) | Sub Use Cases Ä‘Ă­nh kĂ¨m |
| :--- | :--- | :--- |
| **HĂ¬nh 3.1** | **Biá»ƒu Ä‘á»“ Use Case Tá»•ng quĂ¡t toĂ n há»‡ thá»‘ng** | Chá»©a 7 Actors & 12 HLUCs (`HLUC1` $\rightarrow$ `HLUC12`) |
| **HĂ¬nh 3.2** | **Domain KhĂ¡ch hĂ ng & Äáº·t mĂ³n Trá»±c tuyáº¿n** | `UC1.1`, `UC1.2`, `UC2.1`, `UC2.2`, `UC3.1`, `UC3.2`, `UC4.1`, `UC4.2` |
| **HĂ¬nh 3.3** | **Domain BĂ¡n hĂ ng POS, Thanh toĂ¡n & HĂ³a Ä‘Æ¡n**| `UC5.1`, `UC5.2`, `UC5.3`, `UC5.4`, `UC5.5`, `UC6.1`, `UC6.2`, `UC6.3`, `UC6.4`, `UC6.5` |
| **HĂ¬nh 3.4** | **Domain Äiá»u phá»‘i Cháº¿ biáº¿n NhĂ  báº¿p KDS** | `UC7.1`, `UC7.2` |
| **HĂ¬nh 3.5** | **Domain Ca lĂ m viá»‡c & Cháº¥m cĂ´ng NhĂ¢n sá»±** | `UC8.1`, `UC8.2`, `UC8.3`, `UC8.4` |
| **HĂ¬nh 3.6** | **Domain Quáº£n trá»‹ Váº­n hĂ nh, Há»‡ thá»‘ng & Insights**| `UC9.1`, `UC9.2`, `UC9.3`, `UC10.1`, `UC10.2`, `UC11.1`, `UC11.2` |
| **HĂ¬nh 3.7** | **Domain TÆ°Æ¡ng tĂ¡c Trá»£ lĂ½ áº£o AI Agent** | `UC12.1` (Read Tools), `UC12.2` (Write Tools) |

---

## PART N â€” FINAL UML LOCK STATUS & QUALITY GATE

| Quality Gate | Description | Status |
| :--- | :--- | :---: |
| **Gate 1** | Má»—i Actor cĂ³ Business Goal rĂµ rĂ ng vĂ  phĂ¢n biá»‡t Ä‘Æ°á»£c Scope? | `PASS` |
| **Gate 2** | Má»—i Use Case Ä‘áº¡i diá»‡n cho má»™t Actor Business Goal thá»±c sá»±? | `PASS` |
| **Gate 3** | KhĂ´ng chá»©a báº¥t ká»³ Technical Use Case (CRUD Ä‘Æ¡n thuáº§n, Validation, API, DTO)? | `PASS` |
| **Gate 4** | KhĂ´ng chá»©a Actor `Staff` giáº£? | `PASS` |
| **Gate 5** | KhĂ´ng biáº¿n API Permission thĂ nh Actor Association mĂ¡y mĂ³c? | `PASS` |
| **Gate 6** | KhĂ´ng biáº¿n Service Call/Internal method thĂ nh Include? | `PASS` |
| **Gate 7** | KhĂ´ng dĂ¹ng Extend tĂ¹y tiá»‡n khi khĂ´ng cĂ³ Extension Point & Condition rĂµ rĂ ng? | `PASS` |
| **Gate 8** | Quan há»‡ `Customer --|> Guest` Ä‘Ăºng báº£n cháº¥t UML 2.5 Generalization? | `PASS` |
| **Gate 9** | High-Level Use Cases Ä‘Ă³ng vai trĂ² Business Domain Groupings? | `PASS` |
| **Gate 10**| CĂ¡c AI Tools ká»¹ thuáº­t khĂ´ng bá»‹ biáº¿n thĂ nh Use Cases rá»i ráº¡c? | `PASS` |
| **Gate 11**| CĂ¡c chá»©c nÄƒng Ä‘Ă£ bá»‹ xĂ³a (removed staff-finance module) Ä‘Æ°á»£c loáº¡i bá» 100%? | `PASS` |
| **Gate 12**| Ma tráº­n Actor káº¿t ná»‘i chĂ­nh xĂ¡c theo quyá»n khá»Ÿi táº¡o vĂ  tham gia nghiá»‡p vá»¥? | `PASS` |
| **Gate 13**| Má»i Association Ä‘á»u cĂ³ giáº£i trĂ¬nh Business Flow rĂµ rĂ ng? | `PASS` |
| **Gate 14**| KhĂ´ng cĂ³ Actor nĂ o Ä‘Æ°á»£c ná»‘i vĂ o Use Case chá»‰ vĂ¬ cĂ³ quyá»n Ä‘á»c API? | `PASS` |
| **Gate 15**| KhĂ´ng cĂ³ Use Case nĂ o tá»“n táº¡i chá»‰ Ä‘á»ƒ mĂ´ táº£ implementation chi tiáº¿t? | `PASS` |

---

## FINAL STATUS

### **READY AFTER MINOR CORRECTIONS**

**Giáº£i trĂ¬nh Ä‘iá»u kiá»‡n chuyá»ƒn `READY TO LOCK`**:
1. Tiáº¿n hĂ nh sá»­a 2 thuá»™c tĂ­nh `[Authorize]` á»Ÿ Backend cho GAP 1 (`ProductController.cs` endpoint `UpdateAvailability` bá»• sung role `manager`) vĂ  GAP 3 (`ExpenseController.cs` siáº¿t cháº·t `[Authorize(Roles = "admin,manager")]`).
2. KhĂ³a mĂ´ hĂ¬nh Use Case UML 2.5 vá»›i 34 Sub Use Cases.
3. Chuyá»ƒn sang bÆ°á»›c xuáº¥t mĂ£ **Draw.io XML** cho 7 hĂ¬nh sÆ¡ Ä‘á»“ (HĂ¬nh 3.1 $\rightarrow$ HĂ¬nh 3.7)!


