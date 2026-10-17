# BĂO CĂO AUDIT EXPECTED VS ACTUAL â€” FINAL USE CASE & ACTOR ALIGNMENT

**Äá» tĂ i**: Há»‡ thá»‘ng quáº£n lĂ½ nhĂ  hĂ ng Ä‘a chi nhĂ¡nh tĂ­ch há»£p Trá»£ lĂ½ áº£o AI Agent  
**ÄÆ¡n vá»‹ thá»±c hiá»‡n**: Senior System Analyst / Software Architect  
**PhÆ°Æ¡ng phĂ¡p**: Äá»‘i chiáº¿u **Expected Business Model (MĂ´ hĂ¬nh Nghiá»‡p vá»¥ mong muá»‘n)** vá»›i **Actual Implementation (MĂ£ nguá»“n thá»±c táº¿)** Ä‘á»ƒ xĂ¡c Ä‘á»‹nh cĂ¡c Ä‘iá»ƒm MATCH, GAP, MISSING vĂ  NEEDS REVIEW.

---

## PART A â€” EXECUTIVE SUMMARY (Tá»”NG QUAN AUDIT)

- **Tá»•ng sá»‘ High-Level Use Cases (HLUC)**: 12 HLUCs
- **Tá»•ng sá»‘ Sub Use Cases Ä‘Æ°á»£c audit**: 29 Sub-UCs
- **Tá»•ng sá»‘ Business Actors**: 7 Actors (`Admin`, `Manager`, `Cashier`, `Employee`, `Kitchen`, `Customer`, `Guest`)
- **Sá»‘ Use Case khá»›p hoĂ n toĂ n (`MATCH`)**: **20 / 29 Sub-UCs**
- **Sá»‘ Use Case cĂ³ Ä‘á»™ lá»‡ch phĂ¢n quyá»n backend (`GAP`)**: **7 / 29 Sub-UCs** (Chá»§ yáº¿u do Backend cáº¥p quyá»n rá»™ng hÆ¡n Expected Ä‘á»‘i vá»›i Role `employee` vĂ  `manager`)
- **Sá»‘ Use Case cáº§n xem xĂ©t láº¡i nghiá»‡p vá»¥ (`NEEDS REVIEW`)**: **2 / 29 Sub-UCs** (Gá»“m: PhĂ¢n Ä‘á»‹nh "Xem lá»‹ch sá»­ Ä‘Æ¡n hĂ ng" giá»¯a cĂ¡c Actor vĂ  PhĂ¢n Ä‘á»‹nh "Cáº­p nháº­t Ä‘Æ¡n hĂ ng" trĂªn POS)
- **Sá»‘ tĂ­nh nÄƒng bá»‹ thiáº¿u/chÆ°a triá»ƒn khai (`MISSING`)**: **0 / 29 Sub-UCs** (Táº¥t cáº£ 29 Sub-UCs trong Expected Model Ä‘á»u Ä‘Ă£ cĂ³ implementation thá»±c táº¿ trong Backend & Frontend)

---

## PART B â€” EXPECTED VS ACTUAL ACTOR MATRIX

Báº£ng Ä‘á»‘i chiáº¿u tá»•ng quan giá»¯a Expected Business Model vĂ  Actual Implementation trong mĂ£ nguá»“n:

| STT | Detailed Sub Use Case | Expected Actors | Actual Code Authorization | Status | Evidence Source (Source Code) |
| :-: | :--- | :--- | :--- | :---: | :--- |
| **1.1** | QuĂ©t QR BĂ n & Chi nhĂ¡nh | Cust (P), Guest (P) | Customer, Guest | `MATCH` | `QRScan.tsx`, `TableController.cs` |
| **1.2** | Xem thá»±c Ä‘Æ¡n & Chi tiáº¿t mĂ³n | Admin (P), Mgr (P), Cashier (P), Emp (P), Cust (P), Guest (P) | Admin, Manager, Cashier, Employee, Kitchen, Customer, Guest | `MATCH` | `ProductController.cs` (`[AllowAnonymous]`) |
| **2.1** | Gá»­i Ä‘Æ¡n Ä‘áº·t mĂ³n QR / Web Order | Cust (P), Guest (P), Cashier (S), Emp (S) | Customer, Guest, Cashier (S), Employee (S) | `MATCH` | `OrderController.cs` (`CreateOrder`) |
| **2.2** | Xem lá»‹ch sá»­ Ä‘Æ¡n hĂ ng | Admin (P), Mgr (P), Cashier (P), Emp (P), Kitchen (P) | Cust (P), Admin (P), Mgr (P), Cashier (P), Emp (P), Kitchen (P - KDS History) | `NEEDS REVIEW` | `OrderController.cs`, `KitchenService.cs` (`GetHistoryAsync`) |
| **3.1** | Táº¡o yĂªu cáº§u Ä‘áº·t bĂ n trá»±c tuyáº¿n | Cust (P), Guest (P) | Customer, Guest, Staff | `MATCH` | `ReservationController.cs` (`CreateReservation`) |
| **3.2** | Tiáº¿p nháº­n & Quáº£n lĂ½ lá»‹ch háº¹n Ä‘áº·t bĂ n | Mgr (P), Cashier (P), Emp (P), Admin (S) | Admin, Manager, Cashier, Employee | `MATCH` | `ReservationController.cs` (`[Authorize(Roles="admin,manager,employee,cashier")]`) |
| **4.1** | Xem & Cáº­p nháº­t Há»“ sÆ¡ cĂ¡ nhĂ¢n | Cust (P) | Customer (chĂ­nh chá»§) | `MATCH` | `CustomerController.cs` (`UpdateProfile`) |
| **4.2** | Tra cá»©u Ä‘iá»ƒm & Háº¡ng thĂ nh viĂªn Loyalty | Cust (P), Admin (S), Mgr (S), Cashier (S) | Customer, Admin, Manager, Cashier, Employee | `MATCH` | `CustomerController.cs` (`GetLoyaltyHistory`), `LoyaltyService.cs` |
| **5.1** | Táº¡o Ä‘Æ¡n hĂ ng POS | Admin (P), Mgr (P), Cashier (P), Emp (P) | Admin, Manager, Cashier, Employee | `MATCH` | `OrderController.cs` (`CreateOrder`), `OrderService.cs` |
| **5.2** | Cáº­p nháº­t Ä‘Æ¡n hĂ ng | Admin (P), Mgr (P) | Admin, Manager, Cashier, Employee | `GAP` | `OrderController.cs` (`CreateOrder` cho phĂ©p sá»­a order) |
| **5.3** | Tiáº¿p nháº­n Ä‘Æ¡n Ä‘áº·t mĂ³n Web Order | Mgr (P), Cashier (P), Emp (P), Admin (S) | Admin, Manager, Cashier, Employee | `MATCH` | `OrderController.cs` (`AcceptWebOrder`) |
| **5.4** | Gá»­i yĂªu cáº§u cháº¿ biáº¿n xuá»‘ng Báº¿p | Mgr (P), Cashier (P), Emp (P), Admin (S), Kitchen (S) | Admin, Manager, Cashier, Employee, Kitchen | `MATCH` | `OrderController.cs` (`SendToKitchen`) |
| **6.1** | Thanh toĂ¡n Ä‘Æ¡n hĂ ng | Mgr (P), Cashier (P), Admin (S), Cust (S) | Admin, Manager, Cashier, Employee | `GAP` | `OrderController.cs` (`PayOrder` cho phĂ©p cáº£ `employee`) |
| **6.2** | Äá»•i Ä‘iá»ƒm Loyalty | Mgr (P), Cashier (P), Admin (S), Cust (S) | Admin, Manager, Cashier, Employee | `GAP` | `OrderController.cs` (`RedeemPoints` cho phĂ©p cáº£ `employee`) |
| **6.3** | Cáº¥u hĂ¬nh thanh toĂ¡n | Admin (P) | Admin, Manager (Branch-scoped) | `GAP` | `ReceiptSettingsController.cs`, `SystemSettingsController.cs` |
| **6.4** | In / Xuáº¥t hĂ³a Ä‘Æ¡n | Admin (P), Mgr (P), Cashier (P), Emp (P) | Admin, Manager, Cashier, Employee | `MATCH` | `InvoiceController.cs`, `ReceiptSettingsController.cs` |
| **7.1** | Xá»­ lĂ½ Ä‘á»£t cháº¿ biáº¿n KDS thá»i gian thá»±c | Kitchen (P) | Kitchen, Manager, Admin | `MATCH` | `KitchenHub.cs`, `KitchenService.cs` |
| **7.2** | Cáº­p nháº­t tráº¡ng thĂ¡i táº¡m háº¿t mĂ³n | Admin (P), Mgr (P), Kitchen (P) | Admin, Kitchen | `GAP` | `ProductController.cs` (`[Authorize(Roles="admin,kitchen")]`) |
| **8.1** | Cháº¥m cĂ´ng QR | Admin (P), Mgr (P), Cashier (P), Emp (P), Kitchen (P) | Admin, Manager, Cashier, Employee, Kitchen | `MATCH` | `AttendanceController.cs` (`CheckIn`, `CheckOut`) |
| **8.2** | Má»Ÿ & Chá»‘t ca lĂ m viá»‡c | Cashier (P) | Admin, Manager, Cashier | `GAP` | `ShiftController.cs` (`[Authorize(Roles="admin,manager,cashier")]`) |
| **8.3** | Xem lá»‹ch lĂ m viá»‡c | Admin (P), Mgr (P), Cashier (P), Emp (P), Kitchen (P) | Admin, Manager, Cashier, Employee, Kitchen | `MATCH` | `WorkScheduleController.cs` (`GetSchedules`) |
| **8.4** | PhĂ¢n lá»‹ch lĂ m viá»‡c | Admin (P), Mgr (P) | Admin, Manager | `MATCH` | `WorkScheduleController.cs` (`CreateSchedule`, `DeleteSchedule`) |
| **9.1** | Quáº£n lĂ½ SÆ¡ Ä‘á»“ bĂ n & Khu vá»±c | Admin (P), Mgr (P) | Admin, Manager | `MATCH` | `TableController.cs`, `AreaController.cs` |
| **9.2** | Quáº£n lĂ½ Há»“ sÆ¡ & PhĂ¢n lá»‹ch NhĂ¢n sá»± | Admin (P), Mgr (P) | Admin, Manager | `MATCH` | `EmployeeController.cs`, `WorkScheduleController.cs` |
| **9.3** | Quáº£n lĂ½ Chi phĂ­ váº­n hĂ nh | Admin (P), Mgr (P) | Admin, Manager, Cashier, Employee | `GAP` | `ExpenseController.cs` (`[Authorize(Roles="admin,manager,employee,cashier")]`) |
| **10.1**| Quáº£n lĂ½ Danh má»¥c Thá»±c Ä‘Æ¡n & Topping | Admin (P) | Admin | `MATCH` | `ProductController.cs`, `ToppingController.cs` (Admin-only) |
| **10.2**| Quáº£n lĂ½ Chi nhĂ¡nh & Æ¯u Ä‘Ă£i toĂ n chuá»—i | Admin (P) | Admin | `MATCH` | `BranchController.cs`, `PromotionController.cs` (Admin-only) |
| **10.3**| Cáº¥u hĂ¬nh Thiáº¿t láº­p Há»‡ thá»‘ng (VAT/Fee)| Admin (P) | Admin, Manager (Branch-scoped) | `GAP` | `SystemSettingsController.cs` (`Put`) |
| **11.1**| Xem Dashboard | Admin (P), Mgr (P) | Admin, Manager | `MATCH` | `DashboardController.cs` (`GetSummary`) |
| **11.2**| Xem PhĂ¢n tĂ­ch & Cáº£nh bĂ¡o Insights AI | Admin (P), Mgr (P) | Admin, Manager | `MATCH` | `BusinessInsightController.cs` (`Get`, `GetById`) |
| **12.1**| Tra cá»©u thĂ´ng tin qua AI â€“ Read Tools | Admin (P), Mgr (P), Cashier (P), Emp (P), Kitchen (P), Cust (P), Guest (P) | Admin, Manager, Cashier, Employee, Kitchen, Customer, Guest | `MATCH` | `AiController.cs`, `AiPermissionService.cs` |
| **12.2**| Thá»±c hiá»‡n tĂ¡c vá»¥ nghiá»‡p vá»¥ qua AI â€“ Write Tools | Admin (P), Mgr (P), Cashier (P), Emp (P), Kitchen (P), Cust (P) | Admin, Manager, Cashier, Employee, Kitchen, Customer | `MATCH` | `AiController.cs`, `AiPermissionService.cs` |

---

## PART C â€” FULL USE CASE AUDIT (CHI TIáº¾T 12 HIGH-LEVEL USE CASES)

### HLUC1: Tra cá»©u Thá»±c Ä‘Æ¡n & SÆ¡ Ä‘á»“ BĂ n
- **UC 1.1: QuĂ©t QR BĂ n & Chi nhĂ¡nh**
  - **Business Goal**: Nháº­n diá»‡n chi nhĂ¡nh vĂ  sá»‘ bĂ n phá»¥c vá»¥ Ä‘á»ƒ báº¯t Ä‘áº§u tráº£i nghiá»‡m gá»i mĂ³n/Ä‘áº·t bĂ n.
  - **Expected Actors**: Customer (P), Guest (P).
  - **Actual Implementation**: Frontend `QRScan.tsx` quĂ©t QR chá»©a payload `{ type, branchId, tableId, tableName }`. Backend `TableController.GetTables` cháº¥p nháº­n anonymous request vá»›i `branchId`.
  - **Status**: `MATCH`.

- **UC 1.2: Xem thá»±c Ä‘Æ¡n & Chi tiáº¿t mĂ³n**
  - **Business Goal**: Xem danh sĂ¡ch mĂ³n Äƒn, thĂ´ng tin giĂ¡, kĂ­ch thÆ°á»›c (Size) vĂ  danh sĂ¡ch Topping kĂ¨m theo.
  - **Expected Actors**: Admin (P), Manager (P), Cashier (P), Employee (P), Customer (P), Guest (P).
  - **Actual Implementation**: `ProductController.GetProducts` vĂ  `ToppingController.GetToppings` má»Ÿ public `[AllowAnonymous]`. Táº¥t cáº£ cĂ¡c Actor Ä‘á»u xem Ä‘Æ°á»£c thá»±c Ä‘Æ¡n trĂªn UI Admin-Web hoáº·c Customer-Web.
  - **Status**: `MATCH`.

---

### HLUC2: Äáº·t mĂ³n qua QR / Web Order
- **UC 2.1: Gá»­i Ä‘Æ¡n Ä‘áº·t mĂ³n QR / Web Order**
  - **Business Goal**: KhĂ¡ch hĂ ng chá»n mĂ³n, chá»n Size/Topping vĂ  táº¡o Ä‘Æ¡n Ä‘áº·t mĂ³n trá»±c tuyáº¿n tá»« thiáº¿t bá»‹ di Ä‘á»™ng.
  - **Expected Actors**: Customer (P), Guest (P), Cashier (S), Employee (S).
  - **Actual Implementation**: `OrderController.CreateOrder` cho phĂ©p táº¡o Ä‘Æ¡n vĂ£ng lai (`CustomerId = null`) hoáº·c gáº¯n `CustomerId` náº¿u Ä‘Ă£ Ä‘Äƒng nháº­p. NhĂ¢n viĂªn POS (Cashier/Employee) tiáº¿p nháº­n Ä‘Æ¡n trĂªn mĂ n hĂ¬nh POS.
  - **Status**: `MATCH`.

- **UC 2.2: Xem lá»‹ch sá»­ Ä‘Æ¡n hĂ ng**
  - **Business Goal**: Tra cá»©u cĂ¡c Ä‘Æ¡n hĂ ng/hĂ³a Ä‘Æ¡n Ä‘Ă£ vĂ  Ä‘ang xá»­ lĂ½ trong há»‡ thá»‘ng.
  - **Expected Actors**: Admin (P), Manager (P), Cashier (P), Employee (P), Kitchen (P).
  - **Actual Implementation**: 
    - Staff/Admin (Admin, Manager, Cashier, Employee) xem lá»‹ch sá»­ hĂ³a Ä‘Æ¡n táº¡i `GET /api/Order` hoáº·c `/invoices`.
    - Customer xem lá»‹ch sá»­ Ä‘Æ¡n cá»§a chĂ­nh mĂ¬nh qua `GET /api/Order` (lá»c theo SÄT tá»« JWT Name claim) hoáº·c `DigitalMenu.tsx`.
    - Kitchen tra cá»©u lá»‹ch sá»­ Ä‘á»£t cháº¿ biáº¿n KDS qua `GET /api/Order/kitchen/history` (`KitchenService.GetHistoryAsync`).
  - **Status**: `NEEDS REVIEW` (Xem chi tiáº¿t táº¡i Part D).

---

### HLUC3: Äáº·t bĂ n trá»±c tuyáº¿n
- **UC 3.1: Táº¡o yĂªu cáº§u Ä‘áº·t bĂ n trá»±c tuyáº¿n**
  - **Business Goal**: Äáº·t trÆ°á»›c lá»‹ch háº¹n giá»¯ bĂ n táº¡i nhĂ  hĂ ng.
  - **Expected Actors**: Customer (P), Guest (P).
  - **Actual Implementation**: `ReservationController.CreateReservation` cho phĂ©p Guest táº¡o Ä‘Æ¡n vá»›i `Status = "Pending"` hoáº·c Customer tá»± Ä‘á»™ng Ä‘iá»n SÄT chĂ­nh chá»§.
  - **Status**: `MATCH`.

- **UC 3.2: Tiáº¿p nháº­n & Quáº£n lĂ½ lá»‹ch háº¹n Ä‘áº·t bĂ n**
  - **Business Goal**: NhĂ¢n viĂªn/Quáº£n lĂ½ duyá»‡t, xáº¿p bĂ n hoáº·c há»§y lá»‹ch háº¹n Ä‘áº·t bĂ n.
  - **Expected Actors**: Manager (P), Cashier (P), Employee (P), Admin (S).
  - **Actual Implementation**: `ReservationController.UpdateStatus` cĂ³ attribute `[Authorize(Roles = "admin,manager,employee,cashier")]`.
  - **Status**: `MATCH`.

---

### HLUC4: Quáº£n lĂ½ Há»“ sÆ¡ & Loyalty
- **UC 4.1: Xem & Cáº­p nháº­t Há»“ sÆ¡ cĂ¡ nhĂ¢n**
  - **Business Goal**: KhĂ¡ch hĂ ng cĂ¡ nhĂ¢n hĂ³a thĂ´ng tin tĂ i khoáº£n (Há» tĂªn, email, Ä‘á»‹a chá»‰, máº­t kháº©u).
  - **Expected Actors**: Customer (P).
  - **Actual Implementation**: `CustomerController.UpdateProfile` báº¯t buá»™c `[Authorize(Roles = "admin,manager,customer")]` vĂ  kiá»ƒm tra chĂ­nh chá»§ `userId == existing.Id`.
  - **Status**: `MATCH`.

- **UC 4.2: Tra cá»©u Ä‘iá»ƒm & Háº¡ng thĂ nh viĂªn Loyalty**
  - **Business Goal**: Xem tá»•ng sá»‘ Ä‘iá»ƒm tĂ­ch lÅ©y, lá»‹ch sá»­ cá»™ng/trá»« Ä‘iá»ƒm vĂ  háº¡ng thĂ nh viĂªn (`KhĂ¡ch láº»`, `KhĂ¡ch quen`, `KhĂ¡ch VIP`).
  - **Expected Actors**: Customer (P), Admin (S), Manager (S), Cashier (S).
  - **Actual Implementation**: `CustomerController.GetLoyaltyHistory` cho phĂ©p Customer xem Ä‘iá»ƒm cá»§a mĂ¬nh vĂ  Staff tra cá»©u Ä‘iá»ƒm cá»§a khĂ¡ch hĂ ng táº¡i quáº§y.
  - **Status**: `MATCH`.

---

### HLUC5: Quáº£n lĂ½ BĂ¡n hĂ ng POS & Phá»¥c vá»¥
- **UC 5.1: Táº¡o Ä‘Æ¡n hĂ ng POS**
  - **Business Goal**: Láº­p Ä‘Æ¡n hĂ ng má»›i táº¡i quáº§y hoáº·c táº¡i bĂ n phá»¥c vá»¥.
  - **Expected Actors**: Admin (P), Manager (P), Cashier (P), Employee (P).
  - **Actual Implementation**: `OrderController.CreateOrder` xá»­ lĂ½ táº¡o Ä‘Æ¡n POS má»›i, tĂ­nh toĂ¡n láº¡i giĂ¡ tá»± Ä‘á»™ng qua `OrderService.RecalculateOrderFinancials`.
  - **Status**: `MATCH`.

- **UC 5.2: Cáº­p nháº­t Ä‘Æ¡n hĂ ng**
  - **Business Goal**: Thay Ä‘á»•i sá»‘ lÆ°á»£ng mĂ³n, thĂªm mĂ³n má»›i hoáº·c há»§y mĂ³n chÆ°a cháº¿ biáº¿n trĂªn Ä‘Æ¡n hĂ ng Ä‘ang phá»¥c vá»¥.
  - **Expected Actors**: Admin (P), Manager (P).
  - **Actual Implementation**: `OrderController.CreateOrder` tá»± Ä‘á»™ng merge vĂ  cáº­p nháº­t Ä‘Æ¡n hĂ ng náº¿u `order.Id` Ä‘Ă£ tá»“n táº¡i. Backend cho phĂ©p báº¥t ká»³ ngÆ°á»i dĂ¹ng Staff nĂ o cĂ³ quyá»n POS (`admin, manager, cashier, employee`) sá»­a order Ä‘ang má»Ÿ.
  - **Status**: `GAP` (Expected chá»‰ cho Admin & Manager; Actual Backend cho phĂ©p cáº£ Cashier vĂ  Employee cáº­p nháº­t Ä‘Æ¡n POS).

- **UC 5.3: Tiáº¿p nháº­n Ä‘Æ¡n Ä‘áº·t mĂ³n Web Order**
  - **Business Goal**: NhĂ¢n viĂªn xĂ¡c nháº­n Ä‘Æ¡n Ä‘áº·t mĂ³n trá»±c tuyáº¿n tá»« Ä‘iá»‡n thoáº¡i khĂ¡ch vĂ  gáº¯n bĂ n phá»¥c vá»¥.
  - **Expected Actors**: Manager (P), Cashier (P), Employee (P), Admin (S).
  - **Actual Implementation**: `OrderController.AcceptWebOrder` (`AcceptWebOrderAsync`) cĂ³ `[Authorize(Roles = "admin,manager,employee,cashier")]`.
  - **Status**: `MATCH`.

- **UC 5.4: Gá»­i yĂªu cáº§u cháº¿ biáº¿n xuá»‘ng Báº¿p**
  - **Business Goal**: Chuyá»ƒn cĂ¡c mĂ³n Äƒn má»›i gá»i xuá»‘ng mĂ n hĂ¬nh KDS nhĂ  báº¿p Ä‘á»ƒ Ä‘áº§u báº¿p báº¯t Ä‘áº§u cháº¿ biáº¿n.
  - **Expected Actors**: Manager (P), Cashier (P), Employee (P), Admin (S), Kitchen (S).
  - **Actual Implementation**: `OrderController.SendToKitchen` (`SendToKitchenAsync`) tĂ­nh chĂªnh lá»‡ch `SentQuantity` vĂ  phĂ¡t tĂ­n hiá»‡u SignalR tá»›i nhĂ³m Kitchen.
  - **Status**: `MATCH`.

---

### HLUC6: Thanh toĂ¡n & HĂ³a Ä‘Æ¡n
- **UC 6.1: Thanh toĂ¡n Ä‘Æ¡n hĂ ng**
  - **Business Goal**: Thu tiá»n tá»« khĂ¡ch hĂ ng (Tiá»n máº·t / Chuyá»ƒn khoáº£n QR) vĂ  Ä‘Ă³ng Ä‘Æ¡n hĂ ng.
  - **Expected Actors**: Manager (P), Cashier (P), Admin (S), Customer (S).
  - **Actual Implementation**: `OrderController.PayOrder` (`PayOrderAsync`) xĂ¡c thá»±c sá»‘ tiá»n pháº£i Ä‘Ăºng 100% `TotalAmount`, cáº­p nháº­t `PaidAmount`, `PaymentMethod`, `PaymentAt` vĂ  tráº¡ng thĂ¡i `"HoĂ n thĂ nh"`. Attribute backend lĂ  `[Authorize(Roles = "admin,manager,employee,cashier")]`.
  - **Status**: `GAP` (Expected giá»›i háº¡n Primary á»Ÿ Manager & Cashier; Actual Backend má»Ÿ quyá»n cho cáº£ `employee`).

- **UC 6.2: Äá»•i Ä‘iá»ƒm Loyalty**
  - **Business Goal**: Quy Ä‘á»•i Ä‘iá»ƒm tĂ­ch lÅ©y cá»§a khĂ¡ch hĂ ng thĂ nh tiá»n giáº£m giĂ¡ trá»« vĂ o Ä‘Æ¡n hĂ ng.
  - **Expected Actors**: Manager (P), Cashier (P), Admin (S), Customer (S).
  - **Actual Implementation**: `OrderController.RedeemPoints` (`LoyaltyService.RedeemPointsAsync`) trá»« Ä‘iá»ƒm tĂ i khoáº£n khĂ¡ch vĂ  cá»™ng tiá»n giáº£m giĂ¡ vĂ o `order.Discount`. Attribute backend lĂ  `[Authorize(Roles = "admin,manager,employee,cashier")]`.
  - **Status**: `GAP` (Expected giá»›i háº¡n Primary á»Ÿ Manager & Cashier; Actual Backend má»Ÿ quyá»n cho cáº£ `employee`).

- **UC 6.3: Cáº¥u hĂ¬nh thanh toĂ¡n**
  - **Business Goal**: Thiáº¿t láº­p cĂ¡c tham sá»‘ tĂ i chĂ­nh (Tá»· lá»‡ VAT, PhĂ­ dá»‹ch vá»¥, Báº­t/Táº¯t thuáº¿) vĂ  thĂ´ng sá»‘ máº«u in.
  - **Expected Actors**: Admin (P).
  - **Actual Implementation**: `SystemSettingsController.Put` vĂ  `ReceiptSettingsController.Put` cho phĂ©p `admin` (Global) VĂ€ `manager` (chi nhĂ¡nh cá»§a mĂ¬nh) thay Ä‘á»•i cĂ i Ä‘áº·t.
  - **Status**: `GAP` (Expected giá»›i háº¡n á»Ÿ Admin; Actual Backend má»Ÿ quyá»n cho cáº£ `manager` tĂ¹y chá»‰nh thiáº¿t láº­p chi nhĂ¡nh).

- **UC 6.4: In / Xuáº¥t hĂ³a Ä‘Æ¡n**
  - **Business Goal**: Xuáº¥t hoáº·c in hĂ³a Ä‘Æ¡n giáº¥y sau khi giao dá»‹ch hoĂ n táº¥t.
  - **Expected Actors**: Admin (P), Manager (P), Cashier (P), Employee (P).
  - **Actual Implementation**: `InvoiceController.cs` vĂ  `ReceiptSettingsController.cs` cho phĂ©p táº¥t cáº£ Staff truy cáº­p vĂ  in hĂ³a Ä‘Æ¡n.
  - **Status**: `MATCH`.

---

### HLUC7: Äiá»u phá»‘i Cháº¿ biáº¿n (KDS)
- **UC 7.1: Xá»­ lĂ½ Ä‘á»£t cháº¿ biáº¿n KDS thá»i gian thá»±c**
  - **Business Goal**: Tiáº¿p nháº­n danh sĂ¡ch mĂ³n cáº§n náº¥u vĂ  cáº­p nháº­t tiáº¿n Ä‘á»™ (`Chá» xá»­ lĂ½` $\rightarrow$ `Äang náº¥u` $\rightarrow$ `HoĂ n táº¥t`).
  - **Expected Actors**: Kitchen (P).
  - **Actual Implementation**: `KitchenHub.cs` vĂ  `OrderController.UpdateRequestStatus` phá»¥c vá»¥ trá»±c tiáº¿p cho Kitchen.
  - **Status**: `MATCH`.

- **UC 7.2: Cáº­p nháº­t tráº¡ng thĂ¡i táº¡m háº¿t mĂ³n**
  - **Business Goal**: ÄĂ¡nh dáº¥u mĂ³n Äƒn táº¡m thá»i háº¿t hĂ ng (`OutOfStock`) Ä‘á»ƒ dá»«ng nháº­n Ä‘Æ¡n trĂªn POS vĂ  Web.
  - **Expected Actors**: Admin (P), Manager (P), Kitchen (P).
  - **Actual Implementation**: `ProductController.UpdateAvailability` cĂ³ attribute `[Authorize(Roles = "admin,kitchen")]`.
  - **Status**: `GAP` (Actual backend cháº·n Role `manager` trá»±c tiáº¿p gá»i endpoint nĂ y trá»« khi Manager Ä‘Äƒng nháº­p á»Ÿ cháº¿ Ä‘á»™ `kitchen`).

---

### HLUC8: Quáº£n lĂ½ Ca lĂ m viá»‡c & Cháº¥m cĂ´ng
- **UC 8.1: Cháº¥m cĂ´ng QR**
  - **Business Goal**: Thá»±c hiá»‡n quĂ©t QR táº¡i chi nhĂ¡nh Ä‘á»ƒ ghi nháº­n giá» Check-in / Check-out vĂ  tĂ¬nh tráº¡ng Ä‘i muá»™n.
  - **Expected Actors**: Admin (P), Manager (P), Cashier (P), Employee (P), Kitchen (P).
  - **Actual Implementation**: `AttendanceController.CheckIn` vĂ  `CheckOut` má»Ÿ cho toĂ n bá»™ 5 Staff/Admin roles.
  - **Status**: `MATCH`.

- **UC 8.2: Má»Ÿ & Chá»‘t ca lĂ m viá»‡c**
  - **Business Goal**: Khai bĂ¡o sá»‘ tiá»n máº·t Ä‘áº§u ca vĂ  chá»‘t doanh thu tiá»n máº·t/chuyá»ƒn khoáº£n khi káº¿t thĂºc ca.
  - **Expected Actors**: Cashier (P).
  - **Actual Implementation**: `ShiftController.cs` cĂ³ attribute `[Authorize(Roles = "admin,manager,cashier")]`.
  - **Status**: `GAP` (Actual Backend cho phĂ©p cáº£ Manager vĂ  Admin má»Ÿ/chá»‘t ca thay vĂ¬ chá»‰ Cashier).

- **UC 8.3: Xem lá»‹ch lĂ m viá»‡c**
  - **Business Goal**: Xem lá»‹ch phĂ¢n ca cĂ¡ nhĂ¢n trong tuáº§n/thĂ¡ng.
  - **Expected Actors**: Admin (P), Manager (P), Cashier (P), Employee (P), Kitchen (P).
  - **Actual Implementation**: `WorkScheduleController.GetSchedules` há»— trá»£ táº¥t cáº£ cĂ¡c roles.
  - **Status**: `MATCH`.

- **UC 8.4: PhĂ¢n lá»‹ch lĂ m viá»‡c**
  - **Business Goal**: Sáº¯p xáº¿p vĂ  gĂ¡n ca lĂ m viá»‡c cho nhĂ¢n sá»± trong chi nhĂ¡nh.
  - **Expected Actors**: Admin (P), Manager (P).
  - **Actual Implementation**: `WorkScheduleController.CreateSchedule` vĂ  `DeleteSchedule` kiá»ƒm tra `IsAdmin() || IsManager()`.
  - **Status**: `MATCH`.

---

### HLUC9: Quáº£n lĂ½ Váº­n hĂ nh Chi nhĂ¡nh
- **UC 9.1: Quáº£n lĂ½ SÆ¡ Ä‘á»“ bĂ n & Khu vá»±c**
  - **Business Goal**: ThĂªm, sá»­a, xĂ³a bĂ n phá»¥c vá»¥ vĂ  cĂ¡c khu vá»±c trong nhĂ  hĂ ng.
  - **Expected Actors**: Admin (P), Manager (P).
  - **Actual Implementation**: `TableController.cs` vĂ  `AreaController.cs` (Create/Update/Delete) chá»‰ cho phĂ©p `admin` vĂ  `manager`.
  - **Status**: `MATCH`.

- **UC 9.2: Quáº£n lĂ½ Há»“ sÆ¡ & PhĂ¢n lá»‹ch NhĂ¢n sá»±**
  - **Business Goal**: Quáº£n lĂ½ danh sĂ¡ch nhĂ¢n viĂªn chi nhĂ¡nh, táº¡o tĂ i khoáº£n vĂ  phĂ¢n cĂ´ng nhiá»‡m vá»¥.
  - **Expected Actors**: Admin (P), Manager (P).
  - **Actual Implementation**: `EmployeeController.cs` (Create/Update/Delete) kiá»ƒm tra `IsAdmin() || IsManager()`.
  - **Status**: `MATCH`.

- **UC 9.3: Quáº£n lĂ½ Chi phĂ­ váº­n hĂ nh**
  - **Business Goal**: Ghi nháº­n vĂ  theo dĂµi cĂ¡c khoáº£n chi phĂ­ phĂ¡t sinh (Äiá»‡n nÆ°á»›c, bao bĂ¬, gas, vá»‡ sinh).
  - **Expected Actors**: Admin (P), Manager (P).
  - **Actual Implementation**: `ExpenseController.cs` cĂ³ attribute `[Authorize(Roles = "admin,manager,employee,cashier")]`.
  - **Status**: `GAP` (Actual Backend má»Ÿ quyá»n ghi chi phĂ­ cho cáº£ `cashier` vĂ  `employee`).

---

### HLUC10: Quáº£n trá»‹ Há»‡ thá»‘ng & Danh má»¥c
- **UC 10.1: Quáº£n lĂ½ Danh má»¥c Thá»±c Ä‘Æ¡n & Topping**
  - **Business Goal**: ThĂªm, sá»­a, xĂ³a mĂ³n Äƒn, nhĂ³m mĂ³n, giĂ¡ bĂ¡n vĂ  topping dĂ¹ng chung toĂ n há»‡ thá»‘ng.
  - **Expected Actors**: Admin (P).
  - **Actual Implementation**: `ProductController.cs` vĂ  `ToppingController.cs` (Create/Update/Delete) Ä‘á»u cĂ³ `[Authorize(Roles = "admin")]`.
  - **Status**: `MATCH`.

- **UC 10.2: Quáº£n lĂ½ Chi nhĂ¡nh & Æ¯u Ä‘Ă£i toĂ n chuá»—i**
  - **Business Goal**: Quáº£n lĂ½ máº¡ng lÆ°á»›i chi nhĂ¡nh vĂ  cĂ¡c chÆ°Æ¡ng trĂ¬nh khuyáº¿n mĂ£i toĂ n chuá»—i.
  - **Expected Actors**: Admin (P).
  - **Actual Implementation**: `BranchController.cs` vĂ  `PromotionController.cs` Ä‘á»u báº¯t buá»™c `[Authorize(Roles = "admin")]`.
  - **Status**: `MATCH`.

- **UC 10.3: Cáº¥u hĂ¬nh Thiáº¿t láº­p Há»‡ thá»‘ng (VAT/Fee)**
  - **Business Goal**: Cáº¥u hĂ¬nh tá»· lá»‡ VAT, PhĂ­ dá»‹ch vá»¥ vĂ  cĂ¡c tham sá»‘ Loyalty há»‡ thá»‘ng.
  - **Expected Actors**: Admin (P).
  - **Actual Implementation**: `SystemSettingsController.Put` cho phĂ©p `admin` (Global) vĂ  `manager` (chi nhĂ¡nh cá»§a mĂ¬nh).
  - **Status**: `GAP` (Actual Backend há»— trá»£ Manager cáº¥u hĂ¬nh setting chi nhĂ¡nh).

---

### HLUC11: BĂ¡o cĂ¡o Doanh thu & Insights
- **UC 11.1: Xem Dashboard**
  - **Business Goal**: Theo dĂµi biá»ƒu Ä‘á»“ doanh thu, top mĂ³n bĂ¡n cháº¡y, cÆ¡ cáº¥u bĂ¡n hĂ ng vĂ  sá»‘ lÆ°á»£ng nhĂ¢n sá»±.
  - **Expected Actors**: Admin (P), Manager (P).
  - **Actual Implementation**: `DashboardController.GetSummary` báº¯t buá»™c `[Authorize(Roles = "admin,manager")]`.
  - **Status**: `MATCH`.

- **UC 11.2: Xem PhĂ¢n tĂ­ch & Cáº£nh bĂ¡o Insights AI**
  - **Business Goal**: Xem danh sĂ¡ch phĂ¢n tĂ­ch tĂ i chĂ­nh tá»± Ä‘á»™ng vĂ  cĂ¡c cáº£nh bĂ¡o báº¥t thÆ°á»ng tá»« AI.
  - **Expected Actors**: Admin (P), Manager (P).
  - **Actual Implementation**: `BusinessInsightController.cs` báº¯t buá»™c `[Authorize(Roles = "admin,manager")]`.
  - **Status**: `MATCH`.

---

### HLUC12: TÆ°Æ¡ng tĂ¡c Trá»£ lĂ½ áº£o AI Agent
- **UC 12.1: Tra cá»©u thĂ´ng tin qua AI â€“ Read Tools**
  - **Business Goal**: TrĂ² chuyá»‡n báº±ng ngĂ´n ngá»¯ tá»± nhiĂªn Ä‘á»ƒ tra cá»©u doanh thu, thá»±c Ä‘Æ¡n, ca lĂ m, danh sĂ¡ch Ä‘Æ¡n hĂ ng.
  - **Expected Actors**: Admin (P), Manager (P), Cashier (P), Employee (P), Kitchen (P), Customer (P), Guest (P).
  - **Actual Implementation**: `AiController.cs` xá»­ lĂ½ tin nháº¯n Ä‘Ă m thoáº¡i. Guest Ä‘Æ°á»£c sá»­ dá»¥ng `get_menu`. Customer Ä‘Æ°á»£c dĂ¹ng `get_my_order`. Staff Ä‘Æ°á»£c sá»­ dá»¥ng cĂ¡c Read tools thuá»™c pháº¡m vi vai trĂ².
  - **Status**: `MATCH`.

- **UC 12.2: Thá»±c hiá»‡n tĂ¡c vá»¥ nghiá»‡p vá»¥ qua AI â€“ Write Tools**
  - **Business Goal**: Thá»±c hiá»‡n cĂ¡c thao tĂ¡c Ä‘á»•i tráº¡ng thĂ¡i Ä‘Æ¡n, Ä‘áº·t bĂ n hoáº·c cáº­p nháº­t giĂ¡ qua AI Chatbot.
  - **Expected Actors**: Admin (P), Manager (P), Cashier (P), Employee (P), Kitchen (P), Customer (P).
  - **Actual Implementation**: `update_product_price` (Admin), `update_order_status` (Staff), `create_booking` (Customer). Guest bá»‹ cháº·n hoĂ n toĂ n cĂ¡c Write Tools qua `AiPermissionService.cs`.
  - **Status**: `MATCH`.

---

## PART D â€” Äáº¶C BIá»†T: AUDIT "XEM Lá»CH Sá»¬ ÄÆ N HĂ€NG"

Báº£ng phĂ¢n tĂ­ch chuyĂªn sĂ¢u nghiá»‡p vá»¥ Tra cá»©u/Xem lá»‹ch sá»­ Ä‘Æ¡n hĂ ng giá»¯a cĂ¡c Actor:

| Actor | Expected | UI Display Name | Actual Capability in Code | Data Source & Service Method | Business & Domain Alignment | Status |
| :--- | :---: | :--- | :--- | :--- | :--- | :---: |
| **Admin** | **P** | HĂ³a Ä‘Æ¡n / Lá»‹ch sá»­ | Xem toĂ n bá»™ hĂ³a Ä‘Æ¡n toĂ n chuá»—i, lá»c theo chi nhĂ¡nh/ngĂ y | `InvoiceController.Get`, `OrderService.GetOrdersAsync` | Tra cá»©u doanh thu & quáº£n trá»‹ cáº¥p cao. | `MATCH` |
| **Manager** | **P** | HĂ³a Ä‘Æ¡n chi nhĂ¡nh | Xem toĂ n bá»™ hĂ³a Ä‘Æ¡n thuá»™c chi nhĂ¡nh phá»¥ trĂ¡ch | `InvoiceController.Get` (lá»c theo `branchId` JWT) | Quáº£n lĂ½ tĂ i chĂ­nh & váº­n hĂ nh chi nhĂ¡nh. | `MATCH` |
| **Cashier** | **P** | Lá»‹ch sá»­ hĂ³a Ä‘Æ¡n | Xem danh sĂ¡ch hĂ³a Ä‘Æ¡n bĂ¡n hĂ ng táº¡i quáº§y | `InvoiceController.Get` hoáº·c `/pos/invoices` | Äá»‘i soĂ¡t doanh thu quáº§y & in láº¡i hĂ³a Ä‘Æ¡n. | `MATCH` |
| **Employee** | **P** | ÄÆ¡n hĂ ng Ä‘Ă£ táº¡o | Xem lá»‹ch sá»­ cĂ¡c Ä‘Æ¡n hĂ ng do nhĂ¢n viĂªn láº­p/phá»¥c vá»¥ | `OrderController.GetOrders` | Kiá»ƒm tra Ä‘Æ¡n hĂ ng táº¡i bĂ n & há»— trá»£ khĂ¡ch. | `MATCH` |
| **Kitchen** | **P** | Lá»‹ch sá»­ báº¿p (KDS) | Xem danh sĂ¡ch cĂ¡c Ä‘á»£t yĂªu cáº§u cháº¿ biáº¿n (`OrderRequest`) | `KitchenService.GetHistoryAsync` (`GET /api/Order/kitchen/history`) | **KhĂ¡c biá»‡t vá» Domain Entity**: Kitchen xem lá»‹ch sá»­ Ä‘á»£t náº¥u KDS chá»© khĂ´ng xem hĂ³a Ä‘Æ¡n tĂ i chĂ­nh. | `NEEDS REVIEW` |
| **Customer** | **â€“** (trong Expected 2.2) | ÄÆ¡n hĂ ng cá»§a tĂ´i | Xem danh sĂ¡ch & tráº¡ng thĂ¡i 5 Ä‘Æ¡n má»›i nháº¥t cá»§a mĂ¬nh | `OrderService.GetCustomerOrdersAsync` (`GET /api/Order` lá»c theo SÄT) | KhĂ¡ch hĂ ng theo dĂµi tiáº¿n Ä‘á»™ Ä‘Æ¡n hĂ ng cĂ¡ nhĂ¢n. | `NEEDS REVIEW` |

### Káº¿t luáº­n Kiáº¿n trĂºc cho UC "Xem lá»‹ch sá»­ Ä‘Æ¡n hĂ ng":
1. **Äá»‘i vá»›i Admin, Manager, Cashier, Employee**: CĂ¹ng chia sáº» chung má»™t Business Goal lĂ  **"Tra cá»©u lá»‹ch sá»­ hĂ³a Ä‘Æ¡n/Ä‘Æ¡n hĂ ng"** (truy xuáº¥t entity `Order` & `OrderDetail`). Do Ä‘Ă³, gom chung thĂ nh **1 Use Case thá»‘ng nháº¥t** lĂ  `UC2.2: Xem lá»‹ch sá»­ Ä‘Æ¡n hĂ ng`.
2. **Äá»‘i vá»›i Kitchen**: MĂ n hĂ¬nh `/kitchen/history` truy xuáº¥t trá»±c tiáº¿p báº£ng `OrderRequests` vĂ  `OrderRequestItems` (chá»©a cĂ¡c má»‘c thá»i gian `AcceptedAt`, `PreparingAt`, `CompletedAt`). ÄĂ¢y lĂ  **Lá»‹ch sá»­ cháº¿ biáº¿n nhĂ  báº¿p**, khĂ¡c biá»‡t hoĂ n toĂ n vá»›i hĂ³a Ä‘Æ¡n tĂ i chĂ­nh.
   - *Äá» xuáº¥t*: Giá»¯ Kitchen trong UC7.1 / KDS History thay vĂ¬ Ă©p vĂ o UC2.2.
3. **Äá»‘i vá»›i Customer**: Customer thá»±c táº¿ cĂ³ giao diá»‡n "ÄÆ¡n hĂ ng cá»§a tĂ´i" trĂªn `apps/customer-web`. Báº£ng Expected ban Ä‘áº§u Ä‘Ă¡nh `-` cho Customer á»Ÿ UC2.2 lĂ  chÆ°a bao quĂ¡t luá»“ng Customer.

---

## PART E â€” SPECIAL AUDIT: CREATE VS UPDATE ORDER

Báº£ng Ä‘á»‘i chiáº¿u nghiá»‡p vá»¥ Táº¡o Ä‘Æ¡n vs Cáº­p nháº­t Ä‘Æ¡n trĂªn mĂ n hĂ¬nh POS:

| Thao tĂ¡c Nghiá»‡p vá»¥ | Expected Actors | Actual Code Authorization | Logic triá»ƒn khai trong MĂ£ nguá»“n | Status |
| :--- | :--- | :--- | :--- | :---: |
| **5.1: Táº¡o Ä‘Æ¡n hĂ ng POS** | Admin (P), Mgr (P), Cashier (P), Emp (P) | Admin, Manager, Cashier, Employee | `POST /api/Order` khá»Ÿi táº¡o `Order` má»›i, gĂ¡n `Status = "Äang xá»­ lĂ½"`, tá»± Ä‘á»™ng tĂ­nh toĂ¡n tĂ i chĂ­nh. | `MATCH` |
| **5.2: Cáº­p nháº­t Ä‘Æ¡n hĂ ng POS** | Admin (P), Mgr (P) | Admin, Manager, Cashier, Employee | `OrderService.CreateOrUpdateOrderAsync` tá»± Ä‘á»™ng tĂ¬m Ä‘Æ¡n Ä‘ang má»Ÿ cá»§a bĂ n Ä‘á»ƒ merge mĂ³n má»›i hoáº·c thay Ä‘á»•i sá»‘ lÆ°á»£ng. | `GAP` |

### PhĂ¢n tĂ­ch chi tiáº¿t:
- **LĂ½ do GAP**: Trong Expected Business Model, **UC 5.2 (Cáº­p nháº­t Ä‘Æ¡n hĂ ng)** Ä‘Æ°á»£c Ä‘á»‹nh nghÄ©a lĂ  quyá»n háº¡n Ä‘áº·c thĂ¹ cá»§a `Admin` vĂ  `Manager` (vĂ­ dá»¥: Ä‘iá»u chá»‰nh giáº£m mĂ³n, há»§y mĂ³n Ä‘Ă£ Ä‘áº·t). Tuy nhiĂªn, trong mĂ£ nguá»“n `OrderService.cs`, khi nhĂ¢n viĂªn phá»¥c vá»¥ (Employee) hoáº·c Thu ngĂ¢n (Cashier) má»Ÿ má»™t bĂ n Ä‘ang cĂ³ khĂ¡ch vĂ  nháº¥n chá»n thĂªm mĂ³n/tÄƒng sá»‘ lÆ°á»£ng, frontend POS váº«n gá»i chung API `POST /api/Order` Ä‘á»ƒ cáº­p nháº­t Ä‘Æ¡n Ä‘Ă³.
- **Äá» xuáº¥t kháº¯c phá»¥c (Recommended Action)**:
  - *PhÆ°Æ¡ng Ă¡n A (Giá»¯ nguyĂªn Expected Model - KhuyĂªn dĂ¹ng cho BĂ¡o cĂ¡o UML)*: Äá»‹nh nghÄ©a **UC 5.2** lĂ  "Cáº­p nháº­t / Äiá»u chá»‰nh Ä‘Æ¡n hĂ ng nĂ¢ng cao" (dĂ nh riĂªng cho Admin/Manager khi cáº§n can thiá»‡p Ä‘Æ¡n); cĂ²n viá»‡c gá»i thĂªm mĂ³n thĂ´ng thÆ°á»ng cá»§a Employee/Cashier náº±m trong luá»“ng chĂ­nh cá»§a **UC 5.1 (Táº¡o & Cáº­p nháº­t Ä‘Æ¡n POS)**.
  - *PhÆ°Æ¡ng Ă¡n B (Code Fix)*: ThĂªm kiá»ƒm tra Role trong backend náº¿u muá»‘n cháº·n Employee tá»± Ă½ sá»­a Ä‘Æ¡n Ä‘Ă£ gá»­i báº¿p.

---

## PART F â€” ACTOR Ă— USE CASE MATRIX (AFTER AUDIT)

Báº£ng ma tráº­n káº¿t ná»‘i chĂ­nh thá»©c giá»¯a 7 Actor vĂ  29 Sub Use Cases sau khi audit (Pháº£n Ă¡nh Ä‘Ăºng Primary/Supporting Actors):

- **`P`** = Primary Actor (NgÆ°á»i khá»Ÿi táº¡o Use Case)
- **`S`** = Supporting Actor (NgÆ°á»i/Há»‡ thá»‘ng há»— trá»£/tiáº¿p nháº­n)
- **`-`** = KhĂ´ng tham gia

| Sub Use Case ID & Name | Admin | Manager | Cashier | Employee | Kitchen | Customer | Guest |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **UC1.1: QuĂ©t QR BĂ n & Chi nhĂ¡nh** | - | - | - | - | - | **P** | **P** |
| **UC1.2: Xem thá»±c Ä‘Æ¡n & Chi tiáº¿t mĂ³n** | **P** | **P** | **P** | **P** | - | **P** | **P** |
| **UC2.1: Gá»­i Ä‘Æ¡n Ä‘áº·t mĂ³n QR / Web Order** | - | - | S | S | - | **P** | **P** |
| **UC2.2: Xem lá»‹ch sá»­ Ä‘Æ¡n hĂ ng** | **P** | **P** | **P** | **P** | **P** | **P** | - |
| **UC3.1: Táº¡o yĂªu cáº§u Ä‘áº·t bĂ n trá»±c tuyáº¿n** | - | - | - | - | - | **P** | **P** |
| **UC3.2: Tiáº¿p nháº­n & Quáº£n lĂ½ lá»‹ch háº¹n Ä‘áº·t bĂ n** | S | **P** | **P** | **P** | - | - | - |
| **UC4.1: Xem & Cáº­p nháº­t Há»“ sÆ¡ cĂ¡ nhĂ¢n** | - | - | - | - | - | **P** | - |
| **UC4.2: Tra cá»©u Ä‘iá»ƒm & Háº¡ng thĂ nh viĂªn Loyalty** | S | S | S | - | - | **P** | - |
| **UC5.1: Táº¡o Ä‘Æ¡n hĂ ng POS** | **P** | **P** | **P** | **P** | - | - | - |
| **UC5.2: Cáº­p nháº­t Ä‘Æ¡n hĂ ng** | **P** | **P** | **P** | **P** | - | - | - |
| **UC5.3: Tiáº¿p nháº­n Ä‘Æ¡n Ä‘áº·t mĂ³n Web Order** | S | **P** | **P** | **P** | - | - | - |
| **UC5.4: Gá»­i yĂªu cáº§u cháº¿ biáº¿n xuá»‘ng Báº¿p** | S | **P** | **P** | **P** | S | - | - |
| **UC6.1: Thanh toĂ¡n Ä‘Æ¡n hĂ ng** | S | **P** | **P** | **P** | - | S | - |
| **UC6.2: Äá»•i Ä‘iá»ƒm Loyalty** | S | **P** | **P** | **P** | - | S | - |
| **UC6.3: Cáº¥u hĂ¬nh thanh toĂ¡n** | **P** | **P** | - | - | - | - | - |
| **UC6.4: In / Xuáº¥t hĂ³a Ä‘Æ¡n** | **P** | **P** | **P** | **P** | - | - | - |
| **UC7.1: Xá»­ lĂ½ Ä‘á»£t cháº¿ biáº¿n KDS thá»i gian thá»±c** | - | - | - | - | **P** | - | - |
| **UC7.2: Cáº­p nháº­t tráº¡ng thĂ¡i táº¡m háº¿t mĂ³n** | **P** | **P** | - | - | **P** | - | - |
| **UC8.1: Cháº¥m cĂ´ng QR** | **P** | **P** | **P** | **P** | **P** | - | - |
| **UC8.2: Má»Ÿ & Chá»‘t ca lĂ m viá»‡c** | **P** | **P** | **P** | - | - | - | - |
| **UC8.3: Xem lá»‹ch lĂ m viá»‡c** | **P** | **P** | **P** | **P** | **P** | - | - |
| **UC8.4: PhĂ¢n lá»‹ch lĂ m viá»‡c** | **P** | **P** | - | - | - | - | - |
| **UC9.1: Quáº£n lĂ½ SÆ¡ Ä‘á»“ bĂ n & Khu vá»±c** | **P** | **P** | - | - | - | - | - |
| **UC9.2: Quáº£n lĂ½ Há»“ sÆ¡ & PhĂ¢n lá»‹ch NhĂ¢n sá»±** | **P** | **P** | - | - | - | - | - |
| **UC9.3: Quáº£n lĂ½ Chi phĂ­ váº­n hĂ nh** | **P** | **P** | **P** | **P** | - | - | - |
| **UC10.1: Quáº£n lĂ½ Danh má»¥c Thá»±c Ä‘Æ¡n & Topping**| **P** | - | - | - | - | - | - |
| **UC10.2: Quáº£n lĂ½ Chi nhĂ¡nh & Æ¯u Ä‘Ă£i toĂ n chuá»—i**| **P** | - | - | - | - | - | - |
| **UC10.3: Cáº¥u hĂ¬nh Thiáº¿t láº­p Há»‡ thá»‘ng (VAT/Fee)**| **P** | **P** | - | - | - | - | - |
| **UC11.1: Xem Dashboard** | **P** | **P** | - | - | - | - | - |
| **UC11.2: Xem PhĂ¢n tĂ­ch & Cáº£nh bĂ¡o Insights AI** | **P** | **P** | - | - | - | - | - |
| **UC12.1: Tra cá»©u thĂ´ng tin qua AI â€“ Read Tools** | **P** | **P** | **P** | **P** | **P** | **P** | **P** |
| **UC12.2: Thá»±c hiá»‡n tĂ¡c vá»¥ nghiá»‡p vá»¥ qua AI â€“ Write Tools**| **P** | **P** | **P** | **P** | **P** | **P** | - |

---

## PART G â€” AUDIT QUAN Há»† INCLUDE / EXTEND / GENERALIZATION

Báº£ng Ä‘Ă¡nh giĂ¡ chuáº©n má»±c UML 2.5 cho cĂ¡c má»‘i quan há»‡:

| Quan há»‡ UML | Chi tiáº¿t ThĂ nh pháº§n A & B | Audit Result | LĂ½ do Ngá»¯ nghÄ©a UML 2.5 (Semantic Reason) |
| :--- | :--- | :---: | :--- |
| **Actor Generalization** | `Customer --|> Guest` | `VALID` | `Customer` káº¿ thá»«a toĂ n bá»™ kháº£ nÄƒng tÆ°Æ¡ng tĂ¡c cĂ´ng khai cá»§a `Guest` (quĂ©t QR, xem menu, táº¡o Ä‘Æ¡n vĂ£ng lai) vĂ  má»Ÿ rá»™ng thĂªm cĂ¡c Use Case Ä‘á»‹nh danh cĂ¡ nhĂ¢n. |
| **`<<include>>`** | `UC5.4 (Gá»­i báº¿p) --<<include>>--> UC5.1 (Táº¡o/Sá»­a Ä‘Æ¡n POS)` | `VALID` | Má»i Ä‘á»£t gá»­i mĂ³n xuá»‘ng KDS báº¿p báº¯t buá»™c pháº£i dá»±a trĂªn vĂ  cáº­p nháº­t láº¡i `SentQuantity` cá»§a má»™t Ä‘Æ¡n hĂ ng POS Ä‘Ă£ táº¡o. |
| **`<<include>>`** | `UC5.3 (Tiáº¿p nháº­n Web Order) --<<include>>--> UC5.1 (Táº¡o/Sá»­a Ä‘Æ¡n POS)` | `VALID` | Viá»‡c tiáº¿p nháº­n Ä‘Æ¡n Web báº¯t buá»™c pháº£i thá»±c hiá»‡n chuyá»ƒn Ä‘á»•i Ä‘Æ¡n hĂ ng chá» thĂ nh má»™t Ä‘Æ¡n POS chĂ­nh thá»©c gáº¯n bĂ n/ngÆ°á»i nháº­n. |
| **`<<extend>>`** | `UC6.2 (Äá»•i Ä‘iá»ƒm Loyalty) --<<extend>>--> UC5.1 (Táº¡o/Sá»­a Ä‘Æ¡n POS)` | `VALID` | **Extension Point**: Láº­p tĂ i chĂ­nh Ä‘Æ¡n POS.<br>**Condition**: Chá»‰ thá»±c hiá»‡n khi khĂ¡ch hĂ ng yĂªu cáº§u quy Ä‘á»•i Ä‘iá»ƒm tĂ­ch lÅ©y thĂ nh tiá»n giáº£m giĂ¡. |
| **`<<extend>>`** | `UC6.4 (In hĂ³a Ä‘Æ¡n) --<<extend>>--> UC6.1 (Thanh toĂ¡n Ä‘Æ¡n hĂ ng)` | `VALID` | **Extension Point**: HoĂ n táº¥t giao dá»‹ch thanh toĂ¡n.<br>**Condition**: Chá»‰ thá»±c hiá»‡n khi thu ngĂ¢n hoáº·c khĂ¡ch hĂ ng cĂ³ nhu cáº§u xuáº¥t/in hĂ³a Ä‘Æ¡n giáº¥y. |

---

## PART H â€” USE CASE DECOMPOSITION TREE (HĂŒNH 3.1 $\rightarrow$ HĂŒNH 3.7)

Cáº¥u trĂºc phĂ¢n cáº¥p biá»ƒu Ä‘á»“ chĂ­nh thá»©c Ä‘Æ°á»£c xĂ¡c nháº­n:

===================================================================================
                  HĂŒNH 3.1: BIá»‚U Äá»’ USE CASE Tá»”NG QUĂT TOĂ€N Há»† THá»NG
   (Bao gá»“m 7 Actors & 12 High-Level Business Groups HLUC1 -> HLUC12 theo Domain)
===================================================================================
                                       â”‚
     â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”¼â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
     â”‚                                 â”‚                                 â”‚
     â–¼                                 â–¼                                 â–¼
`Domain KhĂ¡ch hĂ ng`           `Domain POS, KDS & Ca`            `Domain Quáº£n trá»‹ & AI`
     â”‚                                 â”‚                                 â”‚
     â”œâ”€â–º HĂŒNH 3.2                      â”œâ”€â–º HĂŒNH 3.3                      â”œâ”€â–º HĂŒNH 3.6
     â”‚   PhĂ¢n rĂ£ Dá»‹ch vá»¥ KhĂ¡ch hĂ ng    â”‚   PhĂ¢n rĂ£ BĂ¡n hĂ ng POS,         â”‚   PhĂ¢n rĂ£ Quáº£n trá»‹ Váº­n hĂ nh,
     â”‚   & Äáº·t mĂ³n Trá»±c tuyáº¿n          â”‚   Thanh toĂ¡n & In HĂ³a Ä‘Æ¡n       â”‚   Há»‡ thá»‘ng & Business Insights
     â”‚   (HLUC1, HLUC2, HLUC3, HLUC4)  â”‚   (HLUC5, HLUC6)                â”‚   (HLUC9, HLUC10, HLUC11)
     â”‚                                 â”‚                                 â”‚
     â””â”€â–º (Chá»©a UC1.1-1.2, UC2.1-2.2,   â”œâ”€â–º HĂŒNH 3.4                      â””â”€â–º HĂŒNH 3.7
          UC3.1-3.2, UC4.1-4.2)        â”‚   PhĂ¢n rĂ£ Äiá»u phá»‘i Cháº¿ biáº¿n    â”‚   PhĂ¢n rĂ£ TÆ°Æ¡ng tĂ¡c Trá»£ lĂ½ áº£o
                                       â”‚   NhĂ  báº¿p KDS (HLUC7)           â”‚   AI Agent (HLUC12)
                                       â”‚                                 â”‚
                                       â””â”€â–º HĂŒNH 3.5                      â””â”€â–º (Chá»©a UC12.1-12.2)
                                           PhĂ¢n rĂ£ Ca lĂ m viá»‡c &
                                           Cháº¥m cĂ´ng NhĂ¢n sá»± (HLUC8)

---

## PART I â€” PHĂ‚N BIá»†T BUSINESS USE CASE VĂ€ TECHNICAL OPERATIONS

Äá»ƒ Ä‘áº£m báº£o biá»ƒu Ä‘á»“ UML sáº¡ch sáº½ vĂ  chuáº©n má»±c, cĂ¡c yáº¿u tá»‘ ká»¹ thuáº­t sau Ä‘Ă¢y **ÄĂƒ ÄÆ¯á»¢C LOáº I Bá» KHá»I SÆ  Äá»’** vĂ  chuyá»ƒn vá» mĂ´ táº£ trong Flow of Events / Technical Architecture:

1. **Calculate VAT / Service Fee / Size Price**: Logic tĂ­nh toĂ¡n tá»± Ä‘á»™ng trong `OrderService.RecalculateOrderFinancials`.
2. **Validate Payment Amount**: Quy táº¯c xĂ¡c thá»±c tham sá»‘ sá»‘ tiá»n trong `PayOrderAsync`.
3. **Earn Loyalty Points Automatically**: TĂ¡c Ä‘á»™ng phá»¥ tá»± Ä‘á»™ng phĂ­a server (`EarnPointsAsync`) sau khi thanh toĂ¡n.
4. **Check Branch Permission / Validate JWT**: CÆ¡ cháº¿ kiá»ƒm soĂ¡t truy cáº­p ká»¹ thuáº­t cá»§a Middleware & Policy.
5. **SignalR Event Broadcasting**: Giao thá»©c truyá»n tin thá»i gian thá»±c giá»¯a Server vĂ  KDS Web Client.
6. **Individual 14 AI Tools**: CÆ¡ cháº¿ thá»±c thi chá»©c nÄƒng ná»™i bá»™ do AI Orchestrator gá»i qua provider hiá»‡n hĂ nh.

---

## PART J â€” CHá»¨C NÄ‚NG Bá» XĂ“A (REMOVED / UNIMPLEMENTED)

- **removed staff-finance module (Quáº£n lĂ½ & TĂ­nh báº£ng lÆ°Æ¡ng)**: ÄĂ£ bá»‹ xĂ³a hoĂ n toĂ n khá»i schema vĂ  mĂ£ nguá»“n qua Migration `20260919075618_Removeremoved staff-finance moduleAndRepairReceiptSettings.cs`. File `removed staff-finance moduleController.cs` vĂ  `removed staff-finance modulePage.tsx` chá»‰ cĂ²n comment obsolete.
- **Káº¿t luáº­n**: **ÄĂƒ LOáº I Bá» 100% KHá»I MĂ” HĂŒNH UML**.

---

## PART K â€” PHĂ‚N LOáº I Káº¾T LÆ¯áº¬N VĂ€ HÆ¯á»NG Xá»¬ LĂ (FINAL CONCLUSION)

### 1. MATCH (CĂ¡c nghiá»‡p vá»¥ Ä‘Ă£ khá»›p 100% giá»¯a Expected vĂ  Code)
- `HLUC1`: QuĂ©t QR bĂ n (`1.1`), Xem thá»±c Ä‘Æ¡n (`1.2`).
- `HLUC2`: Äáº·t mĂ³n QR/Web Order (`2.1`).
- `HLUC3`: Äáº·t bĂ n trá»±c tuyáº¿n (`3.1`), Tiáº¿p nháº­n lá»‹ch háº¹n (`3.2`).
- `HLUC4`: Quáº£n lĂ½ Profile (`4.1`), Tra cá»©u Loyalty (`4.2`).
- `HLUC5`: Táº¡o Ä‘Æ¡n POS (`5.1`), Tiáº¿p nháº­n Ä‘Æ¡n Web (`5.3`), Gá»­i báº¿p (`5.4`).
- `HLUC6`: In hĂ³a Ä‘Æ¡n (`6.4`).
- `HLUC7`: Xá»­ lĂ½ KDS (`7.1`).
- `HLUC8`: Cháº¥m cĂ´ng QR (`8.1`), Xem lá»‹ch lĂ m (`8.3`), PhĂ¢n lá»‹ch lĂ m (`8.4`).
- `HLUC9`: Quáº£n lĂ½ sÆ¡ Ä‘á»“ bĂ n (`9.1`), Quáº£n lĂ½ nhĂ¢n sá»± (`9.2`).
- `HLUC10`: Quáº£n lĂ½ Thá»±c Ä‘Æ¡n/Topping toĂ n chuá»—i (`10.1`), Quáº£n lĂ½ Chi nhĂ¡nh/Æ¯u Ä‘Ă£i (`10.2`).
- `HLUC11`: Xem Dashboard (`11.1`), Xem Business Insights (`11.2`).
- `HLUC12`: AI Read Tools (`12.1`), AI Write Tools (`12.2`).

---

### 2. GAP â€“ NEED CODE FIX (Cáº§n Ä‘iá»u chá»‰nh Backend Authorization náº¿u muá»‘n siáº¿t cháº·t theo Expected Model)
1. **Cáº­p nháº­t Ä‘Æ¡n hĂ ng POS (`UC 5.2`)**:
   - *LĂ½ do*: Backend `OrderController.CreateOrder` cho phĂ©p cáº£ `employee` vĂ  `cashier` cáº­p nháº­t Ä‘Æ¡n hĂ ng Ä‘ang má»Ÿ.
   - *Khuyáº¿n nghá»‹*: Náº¿u muá»‘n siáº¿t cháº·t quyá»n chá»‰ cho Admin/Manager Ä‘iá»u chá»‰nh Ä‘Æ¡n Ä‘Ă£ gá»­i báº¿p, cáº§n bá»• sung role check trong `OrderService.cs`.
2. **Thanh toĂ¡n (`UC 6.1`) & Äá»•i Ä‘iá»ƒm Loyalty (`UC 6.2`)**:
   - *LĂ½ do*: Backend `OrderController.cs` cho phĂ©p Role `employee` gá»i API `PayOrder` vĂ  `RedeemPoints`.
   - *Khuyáº¿n nghá»‹*: ThĂªm attribute `[Authorize(Roles = "admin,manager,cashier")]` trĂªn endpoint `PayOrder` vĂ  `RedeemPoints` Ä‘á»ƒ cháº·n `employee` náº¿u quy trĂ¬nh nhĂ  hĂ ng chá»‰ cho phĂ©p Thu ngĂ¢n thu tiá»n.
3. **Cáº¥u hĂ¬nh thanh toĂ¡n (`UC 6.3`) & Cáº¥u hĂ¬nh VAT/Fee (`UC 10.3`)**:
   - *LĂ½ do*: Backend `SystemSettingsController.cs` vĂ  `ReceiptSettingsController.cs` cho phĂ©p `manager` cáº­p nháº­t setting chi nhĂ¡nh.
   - *Khuyáº¿n nghá»‹*: Giá»¯ nguyĂªn cho Manager tĂ¹y chá»‰nh chi nhĂ¡nh cá»§a mĂ¬nh, hoáº·c giá»›i háº¡n láº¡i cho Admin tĂ¹y theo quyáº¿t Ä‘á»‹nh nghiá»‡p vá»¥.
4. **Cáº­p nháº­t bĂ¡o háº¿t mĂ³n (`UC 7.2`)**:
   - *LĂ½ do*: Backend `ProductController.UpdateAvailability` chá»‰ cĂ³ `[Authorize(Roles = "admin,kitchen")]`. Manager náº¿u Ä‘Äƒng nháº­p á»Ÿ mode `admin` thĂ¬ khĂ´ng bá»‹ cháº·n, nhÆ°ng náº¿u mang token role `manager` thĂ¬ sáº½ bá»‹ 403.
   - *Khuyáº¿n nghá»‹*: Bá»• sung `manager` vĂ o `[Authorize(Roles = "admin,manager,kitchen")]`.
5. **Má»Ÿ & Chá»‘t ca (`UC 8.2`)**:
   - *LĂ½ do*: Backend `ShiftController.cs` cáº¥p quyá»n cho cáº£ `admin, manager, cashier`.
   - *Khuyáº¿n nghá»‹*: PhĂ¹ há»£p thá»±c táº¿ vĂ¬ Quáº£n lĂ½ váº«n cĂ³ thá»ƒ má»Ÿ/chá»‘t ca thay thu ngĂ¢n khi cáº§n.
6. **Quáº£n lĂ½ Chi phĂ­ váº­n hĂ nh (`UC 9.3`)**:
   - *LĂ½ do*: Backend `ExpenseController.cs` cáº¥p quyá»n cho cáº£ `employee` táº¡o phiáº¿u chi.
   - *Khuyáº¿n nghá»‹*: Bá» role `employee` khá»i `ExpenseController` náº¿u chá»‰ cho phĂ©p Manager/Cashier ghi nháº­n chi phĂ­.

---

### 3. NEEDS BUSINESS REVIEW (Äiá»ƒm cáº§n xĂ¡c nháº­n láº¡i thiáº¿t káº¿ nghiá»‡p vá»¥)
1. **UC 2.2 â€” Xem lá»‹ch sá»­ Ä‘Æ¡n hĂ ng**:
   - *Hiá»‡n tráº¡ng*: Staff xem lá»‹ch sá»­ hĂ³a Ä‘Æ¡n táº¡i `/invoices`; Customer xem Ä‘Æ¡n hĂ ng cĂ¡ nhĂ¢n táº¡i `customer-web`; Kitchen xem lá»‹ch sá»­ Ä‘á»£t náº¥u KDS táº¡i `/kitchen/history`.
   - *Quyáº¿t Ä‘á»‹nh*: Giá»¯ chung 1 Use Case `UC2.2: Xem lá»‹ch sá»­ Ä‘Æ¡n hĂ ng` cho Staff/Customer, vĂ  tĂ¡ch rĂµ Kitchen thuá»™c KDS History (`UC7.1`).

---

### 4. RECOMMENDED NEXT STEP (BÆ¯á»C TIáº¾P THEO)

1. **BÆ°á»›c 1**: XĂ¡c nháº­n bĂ¡o cĂ¡o Audit Expected vs Actual nĂ y.
2. **BÆ°á»›c 2**: Quyáº¿t Ä‘á»‹nh xem cĂ³ cáº§n thá»±c hiá»‡n sá»­a code (`Code Fix` cho 6 Ä‘iá»ƒm GAP á»Ÿ má»¥c 2) hay giá»¯ nguyĂªn quyá»n rá»™ng nhÆ° hiá»‡n táº¡i vĂ  cáº­p nháº­t BĂ¡o cĂ¡o Use Case.
3. **BÆ°á»›c 3**: Tiáº¿n hĂ nh xuáº¥t mĂ£ **Draw.io XML** cho tá»«ng hĂ¬nh biá»ƒu Ä‘á»“ (tá»« **HĂ¬nh 3.1** Ä‘áº¿n **HĂ¬nh 3.7**) Ä‘á»ƒ hoĂ n thiá»‡n Há»“ sÆ¡ PhĂ¢n tĂ­ch Thiáº¿t káº¿ UML 2.5 cho Äá»“ Ă¡n!


