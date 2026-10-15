# BĂ¡ÂºÂ£n Ă„â€˜Ă¡Â»â€œ mÄ‚Â£ nguĂ¡Â»â€œn cÄ‚Â¡c module mĂ¡Â»Â¥c 4.1.2

## 1. PhiÄ‚Âªn bĂ¡ÂºÂ£n vÄ‚Â  phĂ¡ÂºÂ¡m vi audit

| MĂ¡Â»Â¥c | GiÄ‚Â¡ trĂ¡Â»â€¹ |
|---|---|
| ThĂ¡Â»Âi Ă„â€˜iĂ¡Â»Æ’m Ă„â€˜Ă¡Â»Âc mÄ‚Â£ nguĂ¡Â»â€œn | 2026-10-07 22:20:55 (+07:00) |
| Repository | `D:\Dev\doan` |
| HEAD | `0a480965f5a582c6da5c30fb70d9a5a2d1cd3d4e` |
| Branch | `main`, tracking `origin/main` |
| Working tree | Dirty; kĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ gĂ¡ÂºÂ¯n vĂ¡Â»â€ºi working tree hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i, khÄ‚Â´ng chĂ¡Â»â€° HEAD |
| PhĂ¡ÂºÂ¡m vi | Backend ASP.NET Core, hai React frontend, AI module, DbContext/entities, cÄ‚Â¡c test vÄ‚Â  bÄ‚Â¡o cÄ‚Â¡o audit liÄ‚Âªn quan |
| KhÄ‚Â´ng thĂ¡Â»Â±c hiĂ¡Â»â€¡n | KhÄ‚Â´ng sĂ¡Â»Â­a mÄ‚Â£ nguĂ¡Â»â€œn nghiĂ¡Â»â€¡p vĂ¡Â»Â¥; khÄ‚Â´ng khĂ¡Â»Å¸i Ă„â€˜Ă¡Â»â„¢ng API/frontend; khÄ‚Â´ng migration/seed; khÄ‚Â´ng thao tÄ‚Â¡c dĂ¡Â»Â¯ liĂ¡Â»â€¡u; khÄ‚Â´ng Ă„â€˜Ă¡Â»Âc secret |

BÄ‚Â¡o cÄ‚Â¡o nÄ‚Â y lÄ‚Â  audit triĂ¡Â»Æ’n khai tĂ¡Â»Â« mÄ‚Â£ nguĂ¡Â»â€œn. ViĂ¡Â»â€¡c mĂ¡Â»â„¢t Ă„â€˜Ă†Â°Ă¡Â»Âng xĂ¡Â»Â­ lÄ‚Â½ Ă„â€˜Ă†Â°Ă¡Â»Â£c mÄ‚Â´ tĂ¡ÂºÂ£ dĂ†Â°Ă¡Â»â€ºi Ă„â€˜Ä‚Â¢y khÄ‚Â´ng Ă„â€˜Ă¡Â»â€œng nghĂ„Â©a vĂ¡Â»â€ºi viĂ¡Â»â€¡c nÄ‚Â³ Ă„â€˜Ä‚Â£ Ă„â€˜Ă†Â°Ă¡Â»Â£c kiĂ¡Â»Æ’m thĂ¡Â»Â­ HTTP, browser, SignalR hoĂ¡ÂºÂ·c production. CÄ‚Â¡c giĂ¡Â»â€ºi hĂ¡ÂºÂ¡n Ă„â€˜Ă†Â°Ă¡Â»Â£c ghi Ă¡Â»Å¸ cuĂ¡Â»â€˜i tĂ¡Â»Â«ng module.

## 2. BĂ¡ÂºÂ£ng tĂ¡Â»â€¢ng hĂ¡Â»Â£p

| MĂ¡Â»Â¥c khÄ‚Â³a luĂ¡ÂºÂ­n | ChĂ¡Â»Â©c nĂ„Æ’ng | Frontend | Endpoint | Service/tool | File vÄ‚Â  dÄ‚Â²ng tiÄ‚Âªu biĂ¡Â»Æ’u | Ghi chÄ‚Âº |
|---|---|---|---|---|---|---|
| 4.1.2.1 | XÄ‚Â¡c thĂ¡Â»Â±c, JWT, role/branch, guest/customer | `apps/admin-web/src/features/auth/pages/LoginPage.tsx`; `apps/customer-web/src/pages/CustomerLogin.tsx` | `POST /api/Auth/login`; `POST /api/Auth/customer-token`; `POST /api/guest/bootstrap` | `JwtService`, `LegacyPasswordVerifier`, `PasswordPolicy` | `services/api/src/WebAPI/Controllers/AuthController.cs:118-265`; `GuestController.cs:30-63`; `JwtService.cs:13-112` | Role lĂ¡ÂºÂ¥y tĂ¡Â»Â« tÄ‚Â i khoĂ¡ÂºÂ£n/claim; UI guard khÄ‚Â´ng thay thĂ¡ÂºÂ¿ API authorization |
| 4.1.2.2 | Menu, product, topping, availability | `apps/admin-web/src/features/catalog/pages/ProductManagement.tsx`; `apps/customer-web/src/pages/DigitalMenu.tsx` | `GET/POST/PUT/DELETE /api/Product`; `GET/POST/PUT/DELETE /api/Topping` | `ProductService` | `services/api/src/WebAPI/Controllers/ProductController.cs:32-188`; `ProductService.cs:17-88` | Menu public/customer chĂ¡Â»â€° lĂ¡Â»Âc sĂ¡ÂºÂ£n phĂ¡ÂºÂ©m active; ProductController chĂ†Â°a thĂ¡ÂºÂ¥y branch scope |
| 4.1.2.3 | QR bootstrap, guest order, POS accept, merge order | `apps/customer-web/src/pages/QRScan.tsx`, `DigitalMenu.tsx`; `apps/admin-web/src/features/pos/pages/POSPage.tsx` | `POST /api/guest/bootstrap`; `POST/PUT /api/Order`; accept/send/payment routes trong `OrderController` | `OrderService` | `services/api/src/WebAPI/Controllers/GuestController.cs:30-63`; `OrderService.cs:122-253,391-455` | GiÄ‚Â¡ vÄ‚Â  trĂ¡ÂºÂ¡ng thÄ‚Â¡i do backend; merge theo order/table + branch; khÄ‚Â´ng tĂ¡Â»Â± suy ra camera |
| 4.1.2.4 | Delta quantity, request bĂ¡ÂºÂ¿p, hub/group/event | `apps/admin-web/src/features/kitchen/pages/KitchenPage.tsx`; POS page | `POST /api/Order/{id}/send-to-kitchen`; kitchen status routes; `/kitchenHub` | `KitchenService`, `KitchenNotifier` | `services/api/src/Application/Services/KitchenService.cs:26-101`; `KitchenHub.cs:10-39`; `KitchenNotifier.cs:18-39` | `SentQuantity` lÄ‚Â  dĂ¡Â»Â¯ liĂ¡Â»â€¡u; event thĂ¡Â»Â±c tĂ¡ÂºÂ¿ lÄ‚Â  `NewOrderRequest`, `RequestStatusUpdated` |
| 4.1.2.5 | TÄ‚Â i chÄ‚Â­nh, payment, loyalty | `apps/admin-web/src/features/pos`; customer order views | Payment/order routes; customer loyalty routes | `OrderService`, `LoyaltyService` | `services/api/src/Application/Services/OrderService.cs:255-389,457-487`; `LoyaltyService.cs:20-84` | Persisted `Order.TotalAmount` lÄ‚Â  nguĂ¡Â»â€œn kiĂ¡Â»Æ’m tra; chĂ†Â°a thĂ¡ÂºÂ¥y payment-provider callback trong Ă„â€˜Ă†Â°Ă¡Â»Âng nÄ‚Â y |
| 4.1.2.6 | NhÄ‚Â¢n sĂ¡Â»Â±, ca, lĂ¡Â»â€¹ch, chĂ¡ÂºÂ¥m cÄ‚Â´ng | Owner/employee/shift pages trong `apps/admin-web/src/features/hrm` | `EmployeeController`, `ShiftController`, `WorkScheduleController`, `AttendanceController` | `EmployeeService` vÄ‚Â  controller trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p | `services/api/src/WebAPI/Controllers/EmployeeController.cs:34-130`; `ShiftController.cs:27-127`; `AttendanceController.cs:28-160` | Scope manager dÄ‚Â¹ng branch claim; khÄ‚Â´ng Ă„â€˜Ă†Â°a removed staff-finance module vÄ‚Â o module nÄ‚Â y |
| 4.1.2.7 | Chi phÄ‚Â­, dashboard, business insight | `apps/admin-web/src/features/analytics/pages/Dashboard.tsx`, `BusinessInsights.tsx`, `operations/pages/ExpenseManagement.tsx` | `ExpenseController`, `DashboardController`, `BusinessInsightController` | `DashboardService` | `services/api/src/Application/Services/DashboardService.cs:41-145`; controllers tĂ†Â°Ă†Â¡ng Ă¡Â»Â©ng | BÄ‚Â¡o cÄ‚Â¡o lĂ¡Â»Âc status/date/branch; BusinessInsight cÄ‚Â³ status/read/unread vÄ‚Â  branch scope |
| 4.1.2.8 | BÄ‚Â n, khu vĂ¡Â»Â±c, Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n | `apps/customer-web/src/pages/ReservationPage.tsx`; `apps/admin-web/src/features/operations/pages/TableManagement.tsx`, `ReservationManagement.tsx` | `TableController`, `AreaController`, `ReservationController` | `ReservationService`, `TableService` | `services/api/src/WebAPI/Controllers/TableController.cs:24-130`; `ReservationService.cs:20-180` | CÄ‚Â³ kiĂ¡Â»Æ’m tra table/branch vÄ‚Â  owner; chĂ†Â°a thĂ¡ÂºÂ¥y logic chĂ¡Â»â€˜ng trÄ‚Â¹ng hoÄ‚Â n chĂ¡Â»â€°nh trong Ă„â€˜oĂ¡ÂºÂ¡n service Ă„â€˜Ä‚Â£ Ă„â€˜Ă¡Â»Âc |
| 4.1.2.9 | AI context, prompt, AI provider, tools, permission, confirmation | `apps/customer-web/src/components/ChatBot.tsx` | `POST /api/Ai/chat`; `POST /api/Ai/product-price-confirmation` | `AiOrchestrator`, `AiToolRegistry`, `AiPermissionService` | `services/api/src/AI/Controllers/AiController.cs:50-151`; `AiOrchestrator.cs:41-220` | Sequential loop tĂ¡Â»â€˜i Ă„â€˜a 5 vÄ‚Â²ng; history client Ă„â€˜Ă†Â°Ă¡Â»Â£c coi lÄ‚Â  untrusted; write price cÄ‚Â³ confirmation |

## 3. CÄ‚Â¡c module theo mĂ¡Â»Â¥c 4.1.2

## 4.1.2.1. Module xÄ‚Â¡c thĂ¡Â»Â±c vÄ‚Â  phÄ‚Â¢n quyĂ¡Â»Ân

### a. TrÄ‚Â¡ch nhiĂ¡Â»â€¡m vÄ‚Â  phĂ¡ÂºÂ¡m vi

Code hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i gĂ¡Â»â€œm ba Ă„â€˜Ă†Â°Ă¡Â»Âng: staff login, customer login/token vÄ‚Â  guest QR bootstrap. `AuthController` dÄ‚Â¹ng `IPasswordHasher<Employee>`/`IPasswordHasher<Customer>` kĂ¡ÂºÂ¿t hĂ¡Â»Â£p `LegacyPasswordVerifier`; account Ă„â€˜Ă†Â°Ă¡Â»Â£c lĂ¡Â»Âc `IsActive` trĂ†Â°Ă¡Â»â€ºc khi xÄ‚Â¡c minh. `JwtService` phÄ‚Â¡t claim role, branch, session type vÄ‚Â  table. `Program.cs` cĂ¡ÂºÂ¥u hÄ‚Â¬nh JWT bearer vÄ‚Â  kiĂ¡Â»Æ’m tra lĂ¡ÂºÂ¡i principal/Ă„â€˜Ă¡Â»â€˜i tĂ†Â°Ă¡Â»Â£ng Ă¡Â»Å¸ sĂ¡Â»Â± kiĂ¡Â»â€¡n xÄ‚Â¡c thĂ¡Â»Â±c. CÄ‚Â¡c controller sau Ă„â€˜Ä‚Â³ tiĂ¡ÂºÂ¿p tĂ¡Â»Â¥c kiĂ¡Â»Æ’m tra role vÄ‚Â  branch claim.

### b. Ă„ÂĂ†Â°Ă¡Â»Âng xĂ¡Â»Â­ lÄ‚Â½ thĂ¡Â»Â±c tĂ¡ÂºÂ¿

`LoginPage.tsx` Ă¢â€ â€™ API client auth Ă¢â€ â€™ `AuthController.Login` Ă¢â€ â€™ tÄ‚Â¬m `Employee` active Ă¢â€ â€™ `LegacyPasswordVerifier.Verify` Ă¢â€ â€™ rehash nĂ¡ÂºÂ¿u cĂ¡ÂºÂ§n Ă¢â€ â€™ `JwtService.GenerateToken` Ă¢â€ â€™ frontend lĂ†Â°u session. Guest: `QRScan.tsx`/URL QR Ă¢â€ â€™ `GuestController.Bootstrap` Ă¢â€ â€™ regex/token + table/area/branch Ă¢â€ â€™ JWT `customerSessionType=guest`, `tableId`. CÄ‚Â¡c endpoint bĂ¡ÂºÂ£o vĂ¡Â»â€¡ Ă„â€˜Ă¡Â»Âc claim chĂ¡Â»Â© khÄ‚Â´ng tin role do frontend gĂ¡Â»Â­i.

### c. BĂ¡ÂºÂ£ng file/method liÄ‚Âªn quan

| File | Method/Ă„â€˜iĂ¡Â»Æ’m Ă„â€˜Ă¡Â»Âc | Vai trÄ‚Â² |
|---|---|---|
| `services/api/src/WebAPI/Controllers/AuthController.cs` | `Login` 118-279 | Staff mode, password, role, branch, token |
| `services/api/src/WebAPI/Controllers/AuthController.cs` | `GetCustomerToken` 280-320 | Customer active/password/claims |
| `services/api/src/WebAPI/Controllers/GuestController.cs` | `Bootstrap` 30-63 | Guest QR session |
| `services/api/src/Infrastructure/Services/JwtService.cs` | `GenerateToken`, `ValidateToken` | JWT claims/signature/lifetime |
| `services/api/Program.cs` | JWT setup/validation 77-188 | Middleware-level identity validation |
| `services/api/src/Application/Common/Security/RoleNames.cs` | role normalization/classification | Role vs position policy |
| `apps/admin-web/src/features/auth/pages/LoginPage.tsx` | submit handler | UI entry point only |

### d. Ă„ÂoĂ¡ÂºÂ¡n code Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t minh hĂ¡Â»Âa 1 Ă¢â‚¬â€ password, active account, legacy rehash

**NguĂ¡Â»â€œn:** `services/api/src/WebAPI/Controllers/AuthController.cs:127-163`, class `AuthController`, method `Login`.

```csharp
var employee = await _context.Employees
    .FirstOrDefaultAsync(e => e.Username == request.Username && e.IsActive);

if (employee == null)
{
    return Unauthorized(new { message = "TÄ‚Âªn Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p hoĂ¡ÂºÂ·c mĂ¡ÂºÂ­t khĂ¡ÂºÂ©u khÄ‚Â´ng chÄ‚Â­nh xÄ‚Â¡c" });
}

bool passwordValid = false;
bool needsUpgrade = false;

if (string.IsNullOrEmpty(employee.Password))
{
    return Unauthorized(new { message = "TÄ‚Â i khoĂ¡ÂºÂ£n chĂ†Â°a cÄ‚Â³ mĂ¡ÂºÂ­t khĂ¡ÂºÂ©u" });
}

var verification = LegacyPasswordVerifier.Verify(
    _employeeHasher,
    employee,
    employee.Password,
    request.Password);
passwordValid = verification.IsValid;
needsUpgrade = verification.NeedsRehash;

if (!passwordValid)
{
    return Unauthorized(new { message = "TÄ‚Âªn Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p hoĂ¡ÂºÂ·c mĂ¡ÂºÂ­t khĂ¡ÂºÂ©u khÄ‚Â´ng chÄ‚Â­nh xÄ‚Â¡c" });
}

if (needsUpgrade)
{
    employee.Password = _employeeHasher.HashPassword(employee, request.Password);
    await _context.SaveChangesAsync();
}
```

**ChĂ¡Â»Â©c nĂ„Æ’ng:** minh hĂ¡Â»Âa account active Ă„â€˜Ă†Â°Ă¡Â»Â£c chĂ¡Â»Ân trĂ†Â°Ă¡Â»â€ºc, password rĂ¡Â»â€”ng bĂ¡Â»â€¹ tĂ¡Â»Â« chĂ¡Â»â€˜i, verifier hĂ¡Â»â€” trĂ¡Â»Â£ legacy vÄ‚Â  hash Ă„â€˜Ă†Â°Ă¡Â»Â£c nÄ‚Â¢ng cĂ¡ÂºÂ¥p sau lĂ¡ÂºÂ§n Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p hĂ¡Â»Â£p lĂ¡Â»â€¡. Ă„ÂĂ¡ÂºÂ§u vÄ‚Â o lÄ‚Â  username/password tĂ¡Â»Â« request; Ă„â€˜Ă¡ÂºÂ§u ra lÄ‚Â  Unauthorized hoĂ¡ÂºÂ·c tiĂ¡ÂºÂ¿p tĂ¡Â»Â¥c phÄ‚Â¡t token. KhÄ‚Â´ng trÄ‚Â­ch secret/hash cĂ¡Â»Â¥ thĂ¡Â»Æ’.

**ChÄ‚Âº thÄ‚Â­ch hÄ‚Â¬nh Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t:** Ă¢â‚¬Å“KiĂ¡Â»Æ’m tra trĂ¡ÂºÂ¡ng thÄ‚Â¡i tÄ‚Â i khoĂ¡ÂºÂ£n vÄ‚Â  nÄ‚Â¢ng cĂ¡ÂºÂ¥p password hash khi Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­pĂ¢â‚¬Â.

**ThuyĂ¡ÂºÂ¿t minh hĂ¡Â»Âc thuĂ¡ÂºÂ­t:** Ă„ÂoĂ¡ÂºÂ¡n mÄ‚Â£ thĂ¡Â»Æ’ hiĂ¡Â»â€¡n lĂ¡Â»â€ºp xÄ‚Â¡c thĂ¡Â»Â±c khÄ‚Â´ng chĂ¡Â»â€° kiĂ¡Â»Æ’m tra chuĂ¡Â»â€”i mĂ¡ÂºÂ­t khĂ¡ÂºÂ©u mÄ‚Â  cÄ‚Â²n bĂ¡ÂºÂ£o vĂ¡Â»â€¡ vÄ‚Â²ng Ă„â€˜Ă¡Â»Âi tÄ‚Â i khoĂ¡ÂºÂ£n. Truy vĂ¡ÂºÂ¥n chĂ¡Â»â€° chĂ¡Â»Ân nhÄ‚Â¢n viÄ‚Âªn cÄ‚Â³ `IsActive`, vÄ‚Â¬ vĂ¡ÂºÂ­y tÄ‚Â i khoĂ¡ÂºÂ£n Ă„â€˜Ä‚Â£ vÄ‚Â´ hiĂ¡Â»â€¡u hÄ‚Â³a khÄ‚Â´ng Ă„â€˜i tĂ¡Â»â€ºi bĂ†Â°Ă¡Â»â€ºc xÄ‚Â¡c minh. Khi tÄ‚Â i khoĂ¡ÂºÂ£n cÄ‚Â³ mĂ¡ÂºÂ­t khĂ¡ÂºÂ©u rĂ¡Â»â€”ng, hĂ¡Â»â€¡ thĂ¡Â»â€˜ng trĂ¡ÂºÂ£ lĂ¡Â»â€”i an toÄ‚Â n thay vÄ‚Â¬ cĂ¡Â»â€˜ gĂ¡ÂºÂ¯ng xĂ¡Â»Â­ lÄ‚Â½ mĂ¡Â»â„¢t giÄ‚Â¡ trĂ¡Â»â€¹ khÄ‚Â´ng hĂ¡Â»Â£p lĂ¡Â»â€¡. `LegacyPasswordVerifier` tĂ¡ÂºÂ¡o lĂ¡Â»â€ºp tĂ†Â°Ă†Â¡ng thÄ‚Â­ch cho cÄ‚Â¡c bĂ¡ÂºÂ£n ghi Ă„â€˜Ă†Â°Ă¡Â»Â£c tĂ¡ÂºÂ¡o theo cĂ†Â¡ chĂ¡ÂºÂ¿ cĂ…Â©; kĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ xÄ‚Â¡c minh bao gĂ¡Â»â€œm cĂ¡Â»Â cĂ¡ÂºÂ§n nÄ‚Â¢ng cĂ¡ÂºÂ¥p. NĂ¡ÂºÂ¿u xÄ‚Â¡c thĂ¡Â»Â±c thÄ‚Â nh cÄ‚Â´ng vÄ‚Â  hash cĂ…Â© khÄ‚Â´ng cÄ‚Â²n phÄ‚Â¹ hĂ¡Â»Â£p, controller tĂ¡ÂºÂ¡o hash mĂ¡Â»â€ºi bĂ¡ÂºÂ±ng `IPasswordHasher` rĂ¡Â»â€œi lĂ†Â°u lĂ¡ÂºÂ¡i. CÄ‚Â¡ch nÄ‚Â y giÄ‚Âºp chuyĂ¡Â»Æ’n Ă„â€˜Ă¡Â»â€¢i dĂ¡ÂºÂ§n dĂ¡Â»Â¯ liĂ¡Â»â€¡u mĂ¡ÂºÂ­t khĂ¡ÂºÂ©u mÄ‚Â  khÄ‚Â´ng cĂ¡ÂºÂ§n mĂ¡Â»â„¢t Ă„â€˜Ă¡Â»Â£t reset Ă„â€˜Ă¡Â»â€œng loĂ¡ÂºÂ¡t. Tuy nhiÄ‚Âªn, Ă„â€˜oĂ¡ÂºÂ¡n mÄ‚Â£ chĂ¡Â»â€° lÄ‚Â  bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng cÄ‚Â i Ă„â€˜Ă¡ÂºÂ·t; nÄ‚Â³ chĂ†Â°a chĂ¡Â»Â©ng minh runtime login, rate limiting hay cĂ¡ÂºÂ¥u hÄ‚Â¬nh secret thĂ¡Â»Â±c tĂ¡ÂºÂ¿.

### e. Ă„ÂoĂ¡ÂºÂ¡n code Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t minh hĂ¡Â»Âa 2 Ă¢â‚¬â€ guest bootstrap vÄ‚Â  ranh giĂ¡Â»â€ºi Customer/Guest

**NguĂ¡Â»â€œn:** `services/api/src/WebAPI/Controllers/GuestController.cs:30-63`, class `GuestController`, method `Bootstrap`.

```csharp
[AllowAnonymous]
[HttpPost("bootstrap")]
public async Task<IActionResult> Bootstrap([FromBody] BootstrapRequest? request)
{
    if (request is null || string.IsNullOrWhiteSpace(request.QrToken) || !QrTokenPattern.IsMatch(request.QrToken))
        return BadRequest(new { message = "MÄ‚Â£ QR khÄ‚Â´ng hĂ¡Â»Â£p lĂ¡Â»â€¡." });

    var table = await _context.Tables.AsNoTracking().Include(candidate => candidate.Area)
        .FirstOrDefaultAsync(candidate => candidate.QrToken == request.QrToken && candidate.IsActive);
    if (table is null || table.BranchId == Guid.Empty || table.Area is null)
        return NotFound(new { message = "MÄ‚Â£ QR khÄ‚Â´ng hĂ¡Â»Â£p lĂ¡Â»â€¡." });

    var matchingTableCount = await _context.Tables.AsNoTracking()
        .CountAsync(candidate => candidate.IsActive && candidate.BranchId == table.BranchId && candidate.Name == table.Name);
    if (matchingTableCount != 1)
        return NotFound(new { message = "MÄ‚Â£ QR khÄ‚Â´ng hĂ¡Â»Â£p lĂ¡Â»â€¡." });

    var token = _jwtService.GenerateToken(
        Guid.NewGuid(), "guest", "KhÄ‚Â¡ch tĂ¡ÂºÂ¡i bÄ‚Â n", "customer",
        table.BranchId, customerSessionType: "guest", tableId: table.Id);

    return Ok(new { token, table = new { table.Name, AreaName = table.Area.Name, table.SeatCount },
        branch = new { id = table.BranchId, name = table.BranchName } });
}
```

**ChĂ¡Â»Â©c nĂ„Æ’ng:** token QR phĂ¡ÂºÂ£i Ă„â€˜Ä‚Âºng Ă„â€˜Ă¡Â»â€¹nh dĂ¡ÂºÂ¡ng, tĂ¡Â»â€œn tĂ¡ÂºÂ¡i, active, cÄ‚Â³ area vÄ‚Â  khÄ‚Â´ng mĂ†Â¡ hĂ¡Â»â€œ theo tÄ‚Âªn bÄ‚Â n trong branch. JWT Ă„â€˜Ă†Â°Ă¡Â»Â£c cĂ¡ÂºÂ¥p role `customer` nhĂ†Â°ng cÄ‚Â³ `customerSessionType=guest`, khÄ‚Â¡c customer Ă„â€˜Ă„Æ’ng kÄ‚Â½.

**ChÄ‚Âº thÄ‚Â­ch hÄ‚Â¬nh Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t:** Ă¢â‚¬Å“Guest bootstrap xÄ‚Â¡c lĂ¡ÂºÂ­p ngĂ¡Â»Â¯ cĂ¡ÂºÂ£nh bÄ‚Â n vÄ‚Â  chi nhÄ‚Â¡nh tĂ¡Â»Â« QRĂ¢â‚¬Â.

**ThuyĂ¡ÂºÂ¿t minh hĂ¡Â»Âc thuĂ¡ÂºÂ­t:** QR khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c xem nhĂ†Â° mĂ¡Â»â„¢t mÄ‚Â£ hiĂ¡Â»Æ’n thĂ¡Â»â€¹ thuĂ¡ÂºÂ§n tÄ‚Âºy. Backend kiĂ¡Â»Æ’m tra Ă„â€˜Ă¡Â»â€¹nh dĂ¡ÂºÂ¡ng token trĂ†Â°Ă¡Â»â€ºc khi truy vĂ¡ÂºÂ¥n, chĂ¡Â»â€° chĂ¡ÂºÂ¥p nhĂ¡ÂºÂ­n bÄ‚Â n Ă„â€˜ang hoĂ¡ÂºÂ¡t Ă„â€˜Ă¡Â»â„¢ng, cÄ‚Â³ chi nhÄ‚Â¡nh hĂ¡Â»Â£p lĂ¡Â»â€¡ vÄ‚Â  liÄ‚Âªn kĂ¡ÂºÂ¿t khu vĂ¡Â»Â±c. MĂ¡Â»â„¢t kiĂ¡Â»Æ’m tra Ă„â€˜Ă¡ÂºÂ¿m bĂ¡Â»â€¢ sung loĂ¡ÂºÂ¡i bĂ¡Â»Â trĂ†Â°Ă¡Â»Âng hĂ¡Â»Â£p nhiĂ¡Â»Âu bÄ‚Â n cÄ‚Â¹ng tÄ‚Âªn trong cÄ‚Â¹ng chi nhÄ‚Â¡nh, vÄ‚Â¬ cÄ‚Â¡c Ă„â€˜Ă†Â¡n cĂ…Â© cÄ‚Â²n lĂ†Â°u `TableName` thay vÄ‚Â¬ chĂ¡Â»â€° lĂ†Â°u `TableId`. Sau khi cÄ‚Â¡c Ă„â€˜iĂ¡Â»Âu kiĂ¡Â»â€¡n Ă„â€˜Ă¡ÂºÂ¡t, hĂ¡Â»â€¡ thĂ¡Â»â€˜ng phÄ‚Â¡t JWT cÄ‚Â³ role kĂ¡Â»Â¹ thuĂ¡ÂºÂ­t lÄ‚Â  customer nhĂ†Â°ng gĂ¡ÂºÂ¯n thÄ‚Âªm loĂ¡ÂºÂ¡i phiÄ‚Âªn guest vÄ‚Â  table id. NhĂ¡Â»Â Ă„â€˜Ä‚Â³ cÄ‚Â¡c endpoint cÄ‚Â³ thĂ¡Â»Æ’ phÄ‚Â¢n biĂ¡Â»â€¡t khÄ‚Â¡ch Ă„â€˜Ä‚Â£ Ă„â€˜Ă„Æ’ng kÄ‚Â½ vĂ¡Â»â€ºi khÄ‚Â¡ch chĂ¡Â»â€° Ă„â€˜Ă†Â°Ă¡Â»Â£c phÄ‚Â©p Ă„â€˜Ă¡ÂºÂ·t mÄ‚Â³n tĂ¡ÂºÂ¡i bÄ‚Â n. `AllowAnonymous` chĂ¡Â»â€° Ä‚Â¡p dĂ¡Â»Â¥ng cho bootstrap; quyĂ¡Â»Ân sĂ¡Â»Â­ dĂ¡Â»Â¥ng tÄ‚Â i nguyÄ‚Âªn sau Ă„â€˜Ä‚Â³ phĂ¡ÂºÂ£i dĂ¡Â»Â±a trÄ‚Âªn JWT vÄ‚Â  cÄ‚Â¡c kiĂ¡Â»Æ’m tra ownership tĂ†Â°Ă†Â¡ng Ă¡Â»Â©ng.

### f. GiĂ¡Â»â€ºi hĂ¡ÂºÂ¡n bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng

ChĂ†Â°a xÄ‚Â¡c minh bĂ¡ÂºÂ±ng HTTP viĂ¡Â»â€¡c inactive account, token giĂ¡ÂºÂ£, token hĂ¡ÂºÂ¿t hĂ¡ÂºÂ¡n, customer khÄ‚Â¡c sĂ¡Â»â€˜ Ă„â€˜iĂ¡Â»â€¡n thoĂ¡ÂºÂ¡i hoĂ¡ÂºÂ·c manager truy cĂ¡ÂºÂ­p branch khÄ‚Â¡c. CĂ¡ÂºÂ§n giĂ¡Â»Â¯ cÄ‚Â¡c phÄ‚Â¡t biĂ¡Â»Æ’u runtime auth/branch isolation Ă¡Â»Å¸ trĂ¡ÂºÂ¡ng thÄ‚Â¡i Ă¢â‚¬Å“chĂ†Â°a xÄ‚Â¡c minhĂ¢â‚¬Â.

## 4.1.2.2. Module quĂ¡ÂºÂ£n lÄ‚Â½ thĂ¡Â»Â±c Ă„â€˜Ă†Â¡n vÄ‚Â  sĂ¡ÂºÂ£n phĂ¡ÂºÂ©m

### aĂ¢â‚¬â€œc. TrÄ‚Â¡ch nhiĂ¡Â»â€¡m, Ă„â€˜Ă†Â°Ă¡Â»Âng xĂ¡Â»Â­ lÄ‚Â½ vÄ‚Â  file

`DigitalMenu.tsx` gĂ¡Â»Âi API product/topping Ă„â€˜Ă¡Â»Æ’ hiĂ¡Â»Æ’n thĂ¡Â»â€¹ menu; `ProductController.GetProducts` query `Products`, lĂ¡Â»Âc active cho anonymous/customer, sau Ă„â€˜Ä‚Â³ lĂ¡Â»Âc search/category/group/status. Admin CRUD Ă„â€˜i trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p qua controller; `ProductService` lÄ‚Â  Ă„â€˜Ă†Â°Ă¡Â»Âng Ă„â€˜Ă†Â°Ă¡Â»Â£c cÄ‚Â¡c AI tool dÄ‚Â¹ng Ă„â€˜Ă¡Â»Æ’ tÄ‚Â¬m/cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t giÄ‚Â¡. Availability cÄ‚Â³ allowlist vÄ‚Â  phÄ‚Â¡t event `ProductAvailabilityChanged`.

| File | Method/dÄ‚Â²ng | Ä‚Â nghĂ„Â©a |
|---|---|---|
| `ProductController.cs` | `GetProducts` 32-55 | Menu query vÄ‚Â  active filter |
| `ProductController.cs` | `CreateProduct`, `UpdateProduct` 57-153 | CRUD, image/code/date validation |
| `ProductController.cs` | `ToggleStatus`, `UpdateAvailability` 155-188 | trĂ¡ÂºÂ¡ng thÄ‚Â¡i kinh doanh vÄ‚Â  availability event |
| `ToppingController.cs` | 21-92 | topping CRUD, admin authorization |
| `ProductService.cs` | `GetAllProductsAsync`, `FindProductsByExactNameAsync`, `UpdatePriceByIdAsync` | AI/Ă¡Â»Â©ng dĂ¡Â»Â¥ng dÄ‚Â¹ng lĂ¡ÂºÂ¡i service |
| `DigitalMenu.tsx` | menu rendering/order selection | UI consumer, khÄ‚Â´ng phĂ¡ÂºÂ£i nguĂ¡Â»â€œn giÄ‚Â¡ authoritative |

### d. Ă„ÂoĂ¡ÂºÂ¡n code Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t

**NguĂ¡Â»â€œn:** `services/api/src/WebAPI/Controllers/ProductController.cs:32-55`.

```csharp
[HttpGet]
public async Task<IActionResult> GetProducts([FromQuery] string? search,
    [FromQuery] string? category, [FromQuery] string? group, [FromQuery] bool? isActive)
{
    var query = _context.Products.AsQueryable();

    if (User.Identity?.IsAuthenticated != true || User.IsInRole("customer"))
        query = query.Where(p => p.IsActive);

    if (!string.IsNullOrEmpty(search))
        query = query.Where(p => p.Name.Contains(search) || p.Code.Contains(search));
    if (!string.IsNullOrEmpty(category)) query = query.Where(p => p.Category == category);
    if (!string.IsNullOrEmpty(group)) query = query.Where(p => p.Group == group);
    if (isActive.HasValue) query = query.Where(p => p.IsActive == isActive.Value);

    return Ok(await query.OrderByDescending(p => p.CreatedAt).ToListAsync());
}
```

**ChĂ¡Â»Â©c nĂ„Æ’ng:** public/customer khÄ‚Â´ng nhÄ‚Â¬n thĂ¡ÂºÂ¥y product inactive; staff/admin cÄ‚Â³ thĂ¡Â»Æ’ dÄ‚Â¹ng bĂ¡Â»â„¢ lĂ¡Â»Âc `isActive`. Ă„ÂĂ¡ÂºÂ§u ra lÄ‚Â  danh sÄ‚Â¡ch tĂ¡Â»Â« DB.

**ChÄ‚Âº thÄ‚Â­ch hÄ‚Â¬nh Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t:** Ă¢â‚¬Å“BĂ¡Â»â„¢ lĂ¡Â»Âc menu vÄ‚Â  giĂ¡Â»â€ºi hĂ¡ÂºÂ¡n sĂ¡ÂºÂ£n phĂ¡ÂºÂ©m inactive Ă¡Â»Å¸ APIĂ¢â‚¬Â.

**ThuyĂ¡ÂºÂ¿t minh hĂ¡Â»Âc thuĂ¡ÂºÂ­t:** API menu giĂ¡Â»Â¯ vai trÄ‚Â² cĂ¡Â»â€¢ng Ă„â€˜Ă¡Â»Âc dĂ¡Â»Â¯ liĂ¡Â»â€¡u cho cĂ¡ÂºÂ£ QR ordering vÄ‚Â  giao diĂ¡Â»â€¡n quĂ¡ÂºÂ£n trĂ¡Â»â€¹. Ă„ÂiĂ¡Â»Âu kiĂ¡Â»â€¡n Ă„â€˜Ă¡ÂºÂ§u tiÄ‚Âªn phÄ‚Â¢n biĂ¡Â»â€¡t ngĂ¡Â»Â¯ cĂ¡ÂºÂ£nh cÄ‚Â´ng khai/customer vĂ¡Â»â€ºi ngĂ†Â°Ă¡Â»Âi dÄ‚Â¹ng Ă„â€˜Ä‚Â£ xÄ‚Â¡c thĂ¡Â»Â±c: Ă¡Â»Å¸ ngĂ¡Â»Â¯ cĂ¡ÂºÂ£nh cÄ‚Â´ng khai, truy vĂ¡ÂºÂ¥n chĂ¡Â»â€° lĂ¡ÂºÂ¥y sĂ¡ÂºÂ£n phĂ¡ÂºÂ©m Ă„â€˜ang active. Sau Ă„â€˜Ä‚Â³ cÄ‚Â¡c tham sĂ¡Â»â€˜ tÄ‚Â¬m kiĂ¡ÂºÂ¿m, category, group vÄ‚Â  trĂ¡ÂºÂ¡ng thÄ‚Â¡i Ă„â€˜Ă†Â°Ă¡Â»Â£c biĂ¡ÂºÂ¿n thÄ‚Â nh Ă„â€˜iĂ¡Â»Âu kiĂ¡Â»â€¡n LINQ trÄ‚Âªn cÄ‚Â¹ng mĂ¡Â»â„¢t query. NhĂ†Â° vĂ¡ÂºÂ­y, frontend chĂ¡Â»â€° cung cĂ¡ÂºÂ¥p tiÄ‚Âªu chÄ‚Â­ hiĂ¡Â»Æ’n thĂ¡Â»â€¹; dĂ¡Â»Â¯ liĂ¡Â»â€¡u menu cuĂ¡Â»â€˜i cÄ‚Â¹ng vĂ¡ÂºÂ«n do database quyĂ¡ÂºÂ¿t Ă„â€˜Ă¡Â»â€¹nh. Tuy nhiÄ‚Âªn, Ă„â€˜oĂ¡ÂºÂ¡n code khÄ‚Â´ng tĂ¡Â»Â± thĂ¡Â»Æ’ hiĂ¡Â»â€¡n branch scope cho Product. VÄ‚Â¬ vĂ¡ÂºÂ­y khÄ‚Â´ng nÄ‚Âªn viĂ¡ÂºÂ¿t rĂ¡ÂºÂ±ng menu Ă„â€˜Ä‚Â£ Ă„â€˜Ă†Â°Ă¡Â»Â£c cÄ‚Â´ lĂ¡ÂºÂ­p theo chi nhÄ‚Â¡nh nĂ¡ÂºÂ¿u chĂ†Â°a tÄ‚Â¬m thĂ¡ÂºÂ¥y service/controller khÄ‚Â¡c bĂ¡Â»â€¢ sung Ă„â€˜iĂ¡Â»Âu kiĂ¡Â»â€¡n Ă„â€˜Ä‚Â³. Topping Ă„â€˜Ă†Â°Ă¡Â»Â£c xĂ¡Â»Â­ lÄ‚Â½ Ă¡Â»Å¸ controller riÄ‚Âªng, cÄ‚Â²n AI update price phĂ¡ÂºÂ£i Ă„â€˜i qua `ProductService` vÄ‚Â  confirmation boundary.

### e. GiĂ¡Â»â€ºi hĂ¡ÂºÂ¡n

Ă„ÂÄ‚Â£ xÄ‚Â¡c Ă„â€˜Ă¡Â»â€¹nh Ă„â€˜Ă†Â°Ă¡Â»Âng CRUD vÄ‚Â  availability, nhĂ†Â°ng chĂ†Â°a thĂ¡ÂºÂ¥y trong Ă„â€˜oĂ¡ÂºÂ¡n controller nÄ‚Â y Ă„â€˜iĂ¡Â»Âu kiĂ¡Â»â€¡n branch ownership cho product/topping. KhÄ‚Â´ng suy ra branch isolation tĂ¡Â»Â« role `admin` hoĂ¡ÂºÂ·c tĂ¡Â»Â« UI. KhÄ‚Â´ng tÄ‚Â¬m thĂ¡ÂºÂ¥y Ă„â€˜Ă†Â°Ă¡Â»Âng VietQR/price provider trong module.

## 4.1.2.3. Module Ă„â€˜Ă¡ÂºÂ·t mÄ‚Â³n QR vÄ‚Â  xĂ¡Â»Â­ lÄ‚Â½ Ă„â€˜Ă†Â¡n hÄ‚Â ng tĂ¡ÂºÂ¡i POS

### aĂ¢â‚¬â€œc. TrÄ‚Â¡ch nhiĂ¡Â»â€¡m vÄ‚Â  Ă„â€˜Ă†Â°Ă¡Â»Âng xĂ¡Â»Â­ lÄ‚Â½

`QRScan.tsx` Ă„â€˜Ă¡Â»Âc camera hoĂ¡ÂºÂ·c query `qr`, sau Ă„â€˜Ä‚Â³ guest bootstrap. `DigitalMenu.tsx` gĂ¡Â»Â­i order payload. `OrderController` nhĂ¡ÂºÂ­n order vÄ‚Â  gĂ¡Â»Âi `OrderService.CreateOrUpdateOrderAsync`; POS gĂ¡Â»Âi accept route cho web order. Service chĂ¡Â»â€° tĂ¡ÂºÂ¡o order mĂ¡Â»â€ºi Ă¡Â»Å¸ trĂ¡ÂºÂ¡ng thÄ‚Â¡i pending/unpaid, bĂ¡Â»Â qua discount client vÄ‚Â  lĂ¡ÂºÂ¥y lĂ¡ÂºÂ¡i giÄ‚Â¡ tĂ¡Â»Â« database. NĂ¡ÂºÂ¿u cÄ‚Â³ order mĂ¡Â»Å¸ cÄ‚Â¹ng id/table/branch, service merge cÄ‚Â¡c detail, giĂ¡Â»Â¯ phĂ¡ÂºÂ§n Ă„â€˜Ä‚Â£ gĂ¡Â»Â­i bĂ¡ÂºÂ¿p.

| File | Method/dÄ‚Â²ng | Ä‚Â nghĂ„Â©a |
|---|---|---|
| `customer-web/src/pages/QRScan.tsx` | camera/query QR handler | tĂ¡ÂºÂ¡o route bootstrap; camera chĂ†Â°a phĂ¡ÂºÂ£i bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng runtime |
| `GuestController.cs` | `Bootstrap` 30-63 | table/branch context |
| `OrderController.cs` | create/update/accept routes khoĂ¡ÂºÂ£ng 119-265 | HTTP boundary |
| `OrderService.cs` | `CreateOrUpdateOrderAsync` 122-253 | validation, merge, authoritative price |
| `OrderService.cs` | `AcceptWebOrderAsync` 391-455 | branch/table/concurrency claim |
| `POSPage.tsx` | order list, accept/send/payment UI | POS consumer |

### d. Ă„ÂoĂ¡ÂºÂ¡n code Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t 1 Ă¢â‚¬â€ merge order vÄ‚Â  giĂ¡Â»Â¯ phĂ¡ÂºÂ§n Ă„â€˜Ä‚Â£ gĂ¡Â»Â­i

**NguĂ¡Â»â€œn:** `services/api/src/Application/Services/OrderService.cs:134-218`.

```csharp
if (order.Id != Guid.Empty || (order.TableName != "Mang vĂ¡Â»Â" && !string.IsNullOrEmpty(order.TableName)))
{
    _context.ChangeTracker.Clear();
    var existingOrder = await _context.Orders.AsNoTracking()
        .Include(o => o.Details)
        .FirstOrDefaultAsync(o =>
            (order.Id != Guid.Empty ? o.Id == order.Id :
                o.TableName == order.TableName && o.BranchId == order.BranchId)
            && o.Status == "Ă„Âang xĂ¡Â»Â­ lÄ‚Â½");

    if (existingOrder != null)
    {
        existingOrder.CustomerName = order.CustomerName;
        existingOrder.CustomerPhone = order.CustomerPhone;
        existingOrder.CustomerEmail = order.CustomerEmail;
        existingOrder.CustomerId = order.CustomerId;
        existingOrder.CreatedBy = order.CreatedBy;
        existingOrder.BranchName = order.BranchName;
        existingOrder.Note = order.Note;

        var removedItems = existingOrder.Details.Where(old => !order.Details.Any(newD =>
            newD.ProductId == old.ProductId && newD.ToppingId == old.ToppingId && newD.Options == old.Options)).ToList();
        foreach (var item in removedItems)
        {
            if (item.SentQuantity == 0) _context.OrderDetails.Remove(item);
            else item.Quantity = item.SentQuantity;
        }

        foreach (var newDetail in order.Details)
        {
            var existingDetail = existingOrder.Details.FirstOrDefault(d =>
                d.ProductId == newDetail.ProductId && d.ToppingId == newDetail.ToppingId &&
                (d.Options ?? "").Trim() == (newDetail.Options ?? "").Trim());
            if (existingDetail != null)
                existingDetail.Quantity = Math.Max(newDetail.Quantity, existingDetail.SentQuantity);
        }
        await RecalculateOrderFinancials(existingOrder);
        _context.Orders.Update(existingOrder);
        await _context.SaveChangesAsync();
        return existingOrder;
    }
}
```

**ChĂ¡Â»Â©c nĂ„Æ’ng:** order mĂ¡Â»Å¸ Ă„â€˜Ă†Â°Ă¡Â»Â£c hĂ¡Â»Â£p nhĂ¡ÂºÂ¥t theo branch/table hoĂ¡ÂºÂ·c id; client khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c ghi Status/PaidAmount/PaymentMethod/Discount trong nhÄ‚Â¡nh merge; quantity khÄ‚Â´ng thĂ¡Â»Æ’ giĂ¡ÂºÂ£m thĂ¡ÂºÂ¥p hĂ†Â¡n SentQuantity.

**ChÄ‚Âº thÄ‚Â­ch hÄ‚Â¬nh Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t:** Ă¢â‚¬Å“HĂ¡Â»Â£p nhĂ¡ÂºÂ¥t Ă„â€˜Ă†Â¡n mĂ¡Â»Å¸ vÄ‚Â  bĂ¡ÂºÂ£o vĂ¡Â»â€¡ phĂ¡ÂºÂ§n mÄ‚Â³n Ă„â€˜Ä‚Â£ gĂ¡Â»Â­i bĂ¡ÂºÂ¿pĂ¢â‚¬Â.

**ThuyĂ¡ÂºÂ¿t minh hĂ¡Â»Âc thuĂ¡ÂºÂ­t:** Ă„ÂĂ†Â¡n hÄ‚Â ng QR khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c coi lÄ‚Â  mĂ¡Â»â„¢t bĂ¡ÂºÂ£n sao Ă„â€˜Ă¡Â»â„¢c lĂ¡ÂºÂ­p mĂ¡Â»â€”i lĂ¡ÂºÂ§n khÄ‚Â¡ch nhĂ¡ÂºÂ¥n gĂ¡Â»Â­i. Service trĂ†Â°Ă¡Â»â€ºc hĂ¡ÂºÂ¿t xÄ‚Â³a change tracker Ă„â€˜Ă¡Â»Æ’ payload client khÄ‚Â´ng mang theo graph state cÄ‚Â³ thĂ¡Â»Æ’ gÄ‚Â¢y nhĂ¡ÂºÂ§m khÄ‚Â³a. NÄ‚Â³ tÄ‚Â¬m Ă„â€˜Ă†Â¡n Ă„â€˜ang xĂ¡Â»Â­ lÄ‚Â½ theo id hoĂ¡ÂºÂ·c theo bÄ‚Â n trong cÄ‚Â¹ng chi nhÄ‚Â¡nh. CÄ‚Â¡c trĂ†Â°Ă¡Â»Âng tÄ‚Â i chÄ‚Â­nh vÄ‚Â  trĂ¡ÂºÂ¡ng thÄ‚Â¡i vĂ¡ÂºÂ­n hÄ‚Â nh khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t tĂ¡Â»Â« payload client; thay vÄ‚Â o Ă„â€˜Ä‚Â³, cÄ‚Â¡c detail Ă„â€˜Ă†Â°Ă¡Â»Â£c so khĂ¡Â»â€ºp theo product, topping vÄ‚Â  options. Detail chĂ†Â°a gĂ¡Â»Â­i bĂ¡ÂºÂ¿p cÄ‚Â³ thĂ¡Â»Æ’ bĂ¡Â»â€¹ loĂ¡ÂºÂ¡i bĂ¡Â»Â, cÄ‚Â²n detail Ă„â€˜Ä‚Â£ gĂ¡Â»Â­i chĂ¡Â»â€° Ă„â€˜Ă†Â°Ă¡Â»Â£c giĂ¡Â»Â¯ lĂ¡ÂºÂ¡i vĂ¡Â»â€ºi quantity tĂ¡Â»â€˜i thiĂ¡Â»Æ’u bĂ¡ÂºÂ±ng SentQuantity. Khi mÄ‚Â³n tĂ¡Â»â€œn tĂ¡ÂºÂ¡i, quantity mĂ¡Â»â€ºi dÄ‚Â¹ng `Math.Max`, ngĂ„Æ’n thao tÄ‚Â¡c sĂ¡Â»Â­a Ă„â€˜Ă†Â¡n lÄ‚Â m mĂ¡ÂºÂ¥t phĂ¡ÂºÂ§n Ă„â€˜Ä‚Â£ chuyĂ¡Â»Æ’n sang bĂ¡ÂºÂ¿p. Sau merge, tÄ‚Â i chÄ‚Â­nh Ă„â€˜Ă†Â°Ă¡Â»Â£c tÄ‚Â­nh lĂ¡ÂºÂ¡i tĂ¡Â»Â« giÄ‚Â¡ DB. Ă„ÂÄ‚Â¢y lÄ‚Â  Ă„â€˜iĂ¡Â»Æ’m cĂ¡ÂºÂ§n minh hĂ¡Â»Âa khi giĂ¡ÂºÂ£i thÄ‚Â­ch tÄ‚Â­nh toÄ‚Â n vĂ¡ÂºÂ¹n giĂ¡Â»Â¯a QR, POS vÄ‚Â  KDS.

### e. Ă„ÂoĂ¡ÂºÂ¡n code Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t 2 Ă¢â‚¬â€ accept pending order vÄ‚Â  branch/table claim

**NguĂ¡Â»â€œn:** `services/api/src/Application/Services/OrderService.cs:401-453`.

```csharp
var current = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
if (current == null) return (WebOrderAcceptResult.NotFound, null);
if (allowedBranchId.HasValue && current.BranchId != allowedBranchId.Value)
    return (WebOrderAcceptResult.Forbidden, null);
if (current.Status != "Ă„Âang xĂ¡Â»Â­ lÄ‚Â½" || !string.IsNullOrEmpty(current.CreatedBy))
    return (WebOrderAcceptResult.Conflict, current);

if (tableId.HasValue)
{
    var targetTable = await _context.Tables.AsNoTracking()
        .FirstOrDefaultAsync(t => t.Id == tableId.Value && t.IsActive);
    if (targetTable == null) return (WebOrderAcceptResult.InvalidTable, null);
    if (allowedBranchId.HasValue && targetTable.BranchId != allowedBranchId.Value)
        return (WebOrderAcceptResult.Forbidden, null);
    if (targetTable.BranchId != current.BranchId)
        return (WebOrderAcceptResult.Forbidden, null);

    var tableClaimed = await _context.Tables
        .Where(t => t.Id == targetTable.Id && t.IsActive && t.Status != "CÄ‚Â³ khÄ‚Â¡ch")
        .ExecuteUpdateAsync(s => s.SetProperty(t => t.Status, "CÄ‚Â³ khÄ‚Â¡ch"));
    if (tableClaimed != 1) return (WebOrderAcceptResult.Conflict, current);
}

var claimed = await _context.Orders
    .Where(o => o.Id == id && o.Status == "Ă„Âang xĂ¡Â»Â­ lÄ‚Â½" && string.IsNullOrEmpty(o.CreatedBy)
        && (!allowedBranchId.HasValue || o.BranchId == allowedBranchId.Value))
    .ExecuteUpdateAsync(s => s.SetProperty(o => o.CreatedBy, acceptedBy));
if (claimed != 1) return (WebOrderAcceptResult.Conflict, current);
```

**ChĂ¡Â»Â©c nĂ„Æ’ng:** kiĂ¡Â»Æ’m tra order branch, trĂ¡ÂºÂ¡ng thÄ‚Â¡i chĂ†Â°a nhĂ¡ÂºÂ­n, table active/cÄ‚Â¹ng branch vÄ‚Â  dÄ‚Â¹ng conditional update chĂ¡Â»â€˜ng hai nhÄ‚Â¢n viÄ‚Âªn cÄ‚Â¹ng nhĂ¡ÂºÂ­n.

**ChÄ‚Âº thÄ‚Â­ch hÄ‚Â¬nh Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t:** Ă¢â‚¬Å“TiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n Ă„â€˜Ă†Â¡n web bĂ¡ÂºÂ±ng Ă„â€˜iĂ¡Â»Âu kiĂ¡Â»â€¡n branch vÄ‚Â  cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t cĂ¡ÂºÂ¡nh tranhĂ¢â‚¬Â.

### f. GiĂ¡Â»â€ºi hĂ¡ÂºÂ¡n

CÄ‚Â³ code cho QR token, order merge vÄ‚Â  accept; chĂ†Â°a cÄ‚Â³ bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng browser/API runtime. `QRScan.tsx` cÄ‚Â³ camera `getUserMedia`, nhĂ†Â°ng khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c mÄ‚Â´ tĂ¡ÂºÂ£ lÄ‚Â  Ă„â€˜Ä‚Â£ kiĂ¡Â»Æ’m thĂ¡Â»Â­ camera. KhÄ‚Â´ng suy ra customer session Ă„â€˜Ä‚Â£ gĂ¡Â»Â­i order thÄ‚Â nh cÄ‚Â´ng chĂ¡Â»â€° tĂ¡Â»Â« route UI.

## 4.1.2.4. Module quĂ¡ÂºÂ£n lÄ‚Â½ bĂ¡ÂºÂ¿p KDS vÄ‚Â  giao tiĂ¡ÂºÂ¿p thĂ¡Â»Âi gian thĂ¡Â»Â±c

### aĂ¢â‚¬â€œc. TrÄ‚Â¡ch nhiĂ¡Â»â€¡m vÄ‚Â  Ă„â€˜Ă†Â°Ă¡Â»Âng xĂ¡Â»Â­ lÄ‚Â½

`KitchenService.SendToKitchenAsync` lĂ¡ÂºÂ¥y order/detais, lĂ¡Â»Âc `Quantity > SentQuantity`, tĂ¡ÂºÂ¡o `OrderRequest` vÄ‚Â  `OrderRequestItem`, cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t SentQuantity, commit rĂ¡Â»â€œi gĂ¡Â»Âi notifier. `KitchenHub.OnConnectedAsync` Ă„â€˜Ă†Â°a connection vÄ‚Â o nhÄ‚Â³m role/branch. `KitchenNotifier` phÄ‚Â¡t cÄ‚Â¡c event tĂ¡Â»â€ºi nhÄ‚Â³m `kitchen-admin` vÄ‚Â  branch. Frontend `KitchenPage.tsx` tĂ¡ÂºÂ¡o SignalR connection tĂ¡Â»â€ºi `/kitchenHub`, Ă„â€˜Ă„Æ’ng kÄ‚Â½ handler cho `NewOrderRequest` vÄ‚Â  `RequestStatusUpdated`; POS cĂ…Â©ng cÄ‚Â³ connection cho trĂ¡ÂºÂ¡ng thÄ‚Â¡i liÄ‚Âªn quan.

| File | Method/dÄ‚Â²ng | Ä‚Â nghĂ„Â©a |
|---|---|---|
| `KitchenService.cs` | `SendToKitchenAsync` 26-101 | delta request + transaction |
| `KitchenService.cs` | `UpdateRequestStatusAsync` 145-175 | status persistence + notify |
| `KitchenHub.cs` | `OnConnectedAsync` 10-39 | auth vÄ‚Â  group membership |
| `KitchenNotifier.cs` | notify methods 18-39 | producer/event names |
| `KitchenPage.tsx` | SignalR builder/on handlers | frontend consumer |

### d. Ă„ÂoĂ¡ÂºÂ¡n code Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t 1 Ă¢â‚¬â€ tÄ‚Â­nh delta vÄ‚Â  cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t SentQuantity

**NguĂ¡Â»â€œn:** `services/api/src/Application/Services/KitchenService.cs:26-75`.

```csharp
public async Task<OrderRequest> SendToKitchenAsync(Guid orderId)
{
    var strategy = _context.Database.CreateExecutionStrategy();
    return await strategy.ExecuteAsync(async () =>
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        var order = await _context.Orders.Include(o => o.Details)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) throw new InvalidOperationException("KhÄ‚Â´ng tÄ‚Â¬m thĂ¡ÂºÂ¥y Ă„â€˜Ă†Â¡n hÄ‚Â ng.");
        if (order.Status != "Ă„Âang xĂ¡Â»Â­ lÄ‚Â½") throw new InvalidOperationException("Ă„ÂĂ†Â¡n hÄ‚Â ng Ă„â€˜Ä‚Â£ Ă„â€˜Ä‚Â³ng hoĂ¡ÂºÂ·c Ă„â€˜Ä‚Â£ hĂ¡Â»Â§y.");

        var itemsToSend = order.Details.Where(d => d.Quantity > d.SentQuantity).ToList();
        if (!itemsToSend.Any()) throw new InvalidOperationException("KhÄ‚Â´ng cÄ‚Â³ mÄ‚Â³n mĂ¡Â»â€ºi cĂ¡ÂºÂ§n gĂ¡Â»Â­i bĂ¡ÂºÂ¿p.");

        var orderRequest = new OrderRequest { Id = Guid.NewGuid(), OrderId = orderId,
            RequestNumber = (await _context.OrderRequests.Where(r => r.OrderId == orderId)
                .MaxAsync(r => (int?)r.RequestNumber) ?? 0) + 1, Status = "Pending",
            CreatedAt = DateTime.UtcNow };

        foreach (var item in itemsToSend)
        {
            var unsentQuantity = item.Quantity - item.SentQuantity;
            orderRequest.Items.Add(new OrderRequestItem { Id = Guid.NewGuid(),
                OrderRequestId = orderRequest.Id, ProductId = item.ProductId,
                ProductName = item.ProductName, Quantity = unsentQuantity, Options = item.Options });
            item.SentQuantity = item.Quantity;
        }
        _context.OrderRequests.Add(orderRequest);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
```

**ChĂ¡Â»Â©c nĂ„Æ’ng:** chĂ¡Â»â€° delta Ă„â€˜Ă†Â°Ă¡Â»Â£c tĂ¡ÂºÂ¡o thÄ‚Â nh item bĂ¡ÂºÂ¿p; `SentQuantity` tĂ„Æ’ng sau khi request Ă„â€˜Ă†Â°Ă¡Â»Â£c thÄ‚Âªm trong transaction.

**ChÄ‚Âº thÄ‚Â­ch hÄ‚Â¬nh Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t:** Ă¢â‚¬Å“TĂ¡ÂºÂ¡o Ă„â€˜Ă¡Â»Â£t chĂ¡ÂºÂ¿ biĂ¡ÂºÂ¿n tĂ¡Â»Â« phĂ¡ÂºÂ§n quantity chĂ†Â°a gĂ¡Â»Â­iĂ¢â‚¬Â.

**ThuyĂ¡ÂºÂ¿t minh hĂ¡Â»Âc thuĂ¡ÂºÂ­t:** KDS khÄ‚Â´ng nhĂ¡ÂºÂ­n lĂ¡ÂºÂ¡i toÄ‚Â n bĂ¡Â»â„¢ order mĂ¡Â»â€”i lĂ¡ÂºÂ§n POS gĂ¡Â»Â­i mÄ‚Â³n. Service xÄ‚Â¡c Ă„â€˜Ă¡Â»â€¹nh cÄ‚Â¡c detail cÄ‚Â³ quantity lĂ¡Â»â€ºn hĂ†Â¡n SentQuantity, tĂ¡Â»Â« Ă„â€˜Ä‚Â³ tĂ¡ÂºÂ¡o danh sÄ‚Â¡ch delta. NĂ¡ÂºÂ¿u order khÄ‚Â´ng tĂ¡Â»â€œn tĂ¡ÂºÂ¡i, Ă„â€˜Ä‚Â£ Ă„â€˜Ä‚Â³ng hoĂ¡ÂºÂ·c khÄ‚Â´ng cÄ‚Â²n mÄ‚Â³n mĂ¡Â»â€ºi, thao tÄ‚Â¡c bĂ¡Â»â€¹ tĂ¡Â»Â« chĂ¡Â»â€˜i. MĂ¡Â»â€”i request cÄ‚Â³ sĂ¡Â»â€˜ thĂ¡Â»Â© tĂ¡Â»Â± tĂ„Æ’ng theo order vÄ‚Â  chĂ¡Â»Â©a cÄ‚Â¡c item vĂ¡Â»â€ºi quantity chĂ†Â°a gĂ¡Â»Â­i. Sau khi thÄ‚Âªm request, SentQuantity Ă„â€˜Ă†Â°Ă¡Â»Â£c cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t bĂ¡ÂºÂ±ng tĂ¡Â»â€¢ng quantity hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i rĂ¡Â»â€œi transaction Ă„â€˜Ă†Â°Ă¡Â»Â£c commit. CÄ‚Â¡ch lÄ‚Â m nÄ‚Â y liÄ‚Âªn kĂ¡ÂºÂ¿t trĂ¡ÂºÂ¡ng thÄ‚Â¡i mÄ‚Â³n Ă¡Â»Å¸ POS vĂ¡Â»â€ºi trĂ¡ÂºÂ¡ng thÄ‚Â¡i yÄ‚Âªu cĂ¡ÂºÂ§u chĂ¡ÂºÂ¿ biĂ¡ÂºÂ¿n Ă¡Â»Å¸ bĂ¡ÂºÂ¿p, Ă„â€˜Ă¡Â»â€œng thĂ¡Â»Âi trÄ‚Â¡nh tĂ¡ÂºÂ¡o request rĂ¡Â»â€”ng. Notification chĂ¡Â»â€° Ă„â€˜Ă†Â°Ă¡Â»Â£c phÄ‚Â¡t sau commit, nÄ‚Âªn event khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c coi lÄ‚Â  xÄ‚Â¡c nhĂ¡ÂºÂ­n thay thĂ¡ÂºÂ¿ cho dĂ¡Â»Â¯ liĂ¡Â»â€¡u persistence.

### e. Ă„ÂoĂ¡ÂºÂ¡n code Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t 2 Ă¢â‚¬â€ hub group vÄ‚Â  event producer

**NguĂ¡Â»â€œn:** `KitchenHub.cs:10-38` vÄ‚Â  `KitchenNotifier.cs:18-38`.

```csharp
[Authorize(Roles = "admin,manager,employee,cashier,kitchen")]
public class KitchenHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        if (string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase))
            await Groups.AddToGroupAsync(Context.ConnectionId, "kitchen-admin");
        if (!string.IsNullOrWhiteSpace(role))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"role:{role}");
        if (Guid.TryParse(Context.User?.FindFirst("branchId")?.Value, out var branchId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"kitchen-branch:{branchId}");
        await base.OnConnectedAsync();
    }
}

public async Task NotifyNewOrderRequestAsync(object payload)
{
    var branchId = payload.GetType().GetProperty("branchId")?.GetValue(payload)?.ToString();
    if (Guid.TryParse(branchId, out var parsedBranchId))
        await _hubContext.Clients.Groups("kitchen-admin", $"kitchen-branch:{parsedBranchId}")
            .SendAsync("NewOrderRequest", payload);
    else
        await _hubContext.Clients.Group("kitchen-admin").SendAsync("NewOrderRequest", payload);
}
```

**ChĂ¡Â»Â©c nĂ„Æ’ng:** endpoint hub yÄ‚Âªu cĂ¡ÂºÂ§u role; branch claim quyĂ¡ÂºÂ¿t Ă„â€˜Ă¡Â»â€¹nh group; producer phÄ‚Â¡t `NewOrderRequest`. Status producer tĂ†Â°Ă†Â¡ng tĂ¡Â»Â± phÄ‚Â¡t `RequestStatusUpdated`.

**ChÄ‚Âº thÄ‚Â­ch hÄ‚Â¬nh Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t:** Ă¢â‚¬Å“Ă„ÂĂ¡Â»â€¹nh tuyĂ¡ÂºÂ¿n sĂ¡Â»Â± kiĂ¡Â»â€¡n KDS theo role vÄ‚Â  chi nhÄ‚Â¡nhĂ¢â‚¬Â.

### f. GiĂ¡Â»â€ºi hĂ¡ÂºÂ¡n

Ă„ÂÄ‚Â£ xÄ‚Â¡c Ă„â€˜Ă¡Â»â€¹nh cĂ¡ÂºÂ£ producer vÄ‚Â  nĂ†Â¡i nhĂ¡ÂºÂ­n theo code, nhĂ†Â°ng chĂ†Â°a quan sÄ‚Â¡t connection/event thĂ¡ÂºÂ­t. CĂ¡ÂºÂ§n kiĂ¡Â»Æ’m tra thÄ‚Âªm tĂ¡ÂºÂ¥t cĂ¡ÂºÂ£ handler trong `KitchenPage.tsx`, `POSPage.tsx`, `Navbar.tsx`, `CashierNavbar.tsx`; khÄ‚Â´ng khĂ¡ÂºÂ³ng Ă„â€˜Ă¡Â»â€¹nh SignalR isolation runtime. `ProductController.UpdateAvailability` hiĂ¡Â»â€¡n dÄ‚Â¹ng `Clients.All`, nÄ‚Âªn khÄ‚Â´ng nÄ‚Âªn mÄ‚Â´ tĂ¡ÂºÂ£ mĂ¡Â»Âi event sĂ¡ÂºÂ£n phĂ¡ÂºÂ©m lÄ‚Â  branch-scoped.

## 4.1.2.5. Module thanh toÄ‚Â¡n vÄ‚Â  khÄ‚Â¡ch hÄ‚Â ng thÄ‚Â¢n thiĂ¡ÂºÂ¿t

### aĂ¢â‚¬â€œc. TrÄ‚Â¡ch nhiĂ¡Â»â€¡m vÄ‚Â  Ă„â€˜Ă†Â°Ă¡Â»Âng xĂ¡Â»Â­ lÄ‚Â½

`OrderService.RecalculateOrderFinancials` lĂ¡ÂºÂ¥y product/topping price tĂ¡Â»Â« DB, Ă„â€˜Ă¡Â»Âc settings theo `order.BranchId`, tÄ‚Â­nh subtotal Ă¢â€ â€™ service fee Ă¢â€ â€™ VAT Ă¢â€ â€™ discount. `PayOrderAsync` dÄ‚Â¹ng Ă„â€˜iĂ¡Â»Âu kiĂ¡Â»â€¡n database gĂ¡Â»â€œm status, PaidAmount vÄ‚Â  persisted TotalAmount. `LoyaltyService` earn sau order completed/payment vÄ‚Â  cÄ‚Â³ unique/idempotency check theo order/type. Controller kiĂ¡Â»Æ’m tra request amount/method trĂ†Â°Ă¡Â»â€ºc khi gĂ¡Â»Âi service.

| File | Method/dÄ‚Â²ng | Ä‚Â nghĂ„Â©a |
|---|---|---|
| `OrderService.cs` | `RecalculateOrderFinancials` 255-389 | authoritative money rule |
| `OrderService.cs` | `PayOrderAsync` 457-487 | conditional payment transaction |
| `LoyaltyService.cs` | `EarnPointsAsync` 20-84 | earn/idempotency/customer update |
| `LoyaltyService.cs` | `RedeemPointsAsync`, `RefundPointsAsync` 86+ | redeem/refund service paths |
| `OrderController.cs` | payment/loyalty routes khoĂ¡ÂºÂ£ng 279+ | request validation vÄ‚Â  HTTP response |
| `apps/admin-web/src/features/pos` | payment UI | chĂ¡Â»â€° lÄ‚Â  client, khÄ‚Â´ng authoritative |

### d. Ă„ÂoĂ¡ÂºÂ¡n code Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t 1 Ă¢â‚¬â€ authoritative price, fee, VAT vÄ‚Â  rounding

**NguĂ¡Â»â€œn:** `services/api/src/Application/Services/OrderService.cs:255-364,388-389`.

```csharp
var settings = await _settingService.GetSettingsAsync(order.BranchId);
var vatPercent = GetNonNegativeDecimalSetting(settings, "vatPercent");
var vatEnabled = IsEnabled(settings, "vatEnabled", vatPercent);
var serviceFeePercent = GetNonNegativeDecimalSetting(settings, "serviceFeePercent", "serviceFee");
var serviceFeeEnabled = IsEnabled(settings, "serviceFeeEnabled", serviceFeePercent);

decimal subTotal = 0;
foreach (var detail in order.Details)
{
    decimal unitPrice = 0;
    if (detail.ProductId.HasValue)
    {
        var product = await _context.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == detail.ProductId.Value);
        if (product != null) unitPrice = product.Price;
        // Size and comma-separated toppings are matched against product JSON.
    }
    else if (detail.ToppingId.HasValue)
    {
        var topping = await _context.Toppings.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == detail.ToppingId.Value);
        if (topping != null) unitPrice = topping.Price;
    }
    detail.UnitPrice = unitPrice;
    subTotal += detail.Quantity * detail.UnitPrice;
}

order.SubTotal = subTotal;
order.VatPercent = vatEnabled ? vatPercent : 0;
order.ServiceFeePercent = serviceFeeEnabled ? serviceFeePercent : 0;
var serviceFeeAmount = RoundMoney(subTotal * order.ServiceFeePercent.Value / 100);
var taxableBase = subTotal + serviceFeeAmount;
var vatAmount = RoundMoney(taxableBase * order.VatPercent.Value / 100);
order.ServiceFeeAmount = serviceFeeAmount;
order.VatAmount = vatAmount;
order.TotalAmount = taxableBase + vatAmount - order.Discount;

private static decimal RoundMoney(decimal amount) =>
    Math.Round(amount, 0, MidpointRounding.AwayFromZero);
```

**ChĂ¡Â»Â©c nĂ„Æ’ng:** giÄ‚Â¡ client khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c dÄ‚Â¹ng lÄ‚Â m giÄ‚Â¡ cuĂ¡Â»â€˜i; VAT tÄ‚Â­nh trÄ‚Âªn subtotal + service fee; phÄ‚Â­ vÄ‚Â  VAT lÄ‚Â m trÄ‚Â²n Ă„â€˜Ă¡Â»â„¢c lĂ¡ÂºÂ­p 0 chĂ¡Â»Â¯ sĂ¡Â»â€˜.

**ChÄ‚Âº thÄ‚Â­ch hÄ‚Â¬nh Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t:** Ă¢â‚¬Å“ChuĂ¡Â»â€”i tÄ‚Â­nh toÄ‚Â¡n tÄ‚Â i chÄ‚Â­nh authoritative tĂ¡ÂºÂ¡i OrderServiceĂ¢â‚¬Â.

**ThuyĂ¡ÂºÂ¿t minh hĂ¡Â»Âc thuĂ¡ÂºÂ­t:** OrderService lÄ‚Â  nĂ†Â¡i tĂ¡ÂºÂ­p trung quy tĂ¡ÂºÂ¯c tÄ‚Â i chÄ‚Â­nh cĂ¡Â»Â§a hĂ¡Â»â€¡ thĂ¡Â»â€˜ng. Settings Ă„â€˜Ă†Â°Ă¡Â»Â£c truy vĂ¡ÂºÂ¥n theo BranchId cĂ¡Â»Â§a order, sau Ă„â€˜Ä‚Â³ tĂ¡Â»Â«ng detail Ă„â€˜Ă†Â°Ă¡Â»Â£c Ă„â€˜Ă¡Â»â€˜i chiĂ¡ÂºÂ¿u product hoĂ¡ÂºÂ·c topping trong database. TÄ‚Â¹y chĂ¡Â»Ân size vÄ‚Â  topping Ă„â€˜Ă†Â°Ă¡Â»Â£c cĂ¡Â»â„¢ng vÄ‚Â o unit price sau khi tÄ‚Â¬m thĂ¡ÂºÂ¥y trong cĂ¡ÂºÂ¥u hÄ‚Â¬nh sĂ¡ÂºÂ£n phĂ¡ÂºÂ©m. Subtotal Ă„â€˜Ă†Â°Ă¡Â»Â£c tÄ‚Â­nh bĂ¡ÂºÂ±ng quantity nhÄ‚Â¢n unit price authoritative. PhÄ‚Â­ dĂ¡Â»â€¹ch vĂ¡Â»Â¥ vÄ‚Â  VAT cÄ‚Â³ cĂ¡Â»Â enabled/percentage riÄ‚Âªng; phÄ‚Â­ Ă„â€˜Ă†Â°Ă¡Â»Â£c lÄ‚Â m trÄ‚Â²n trĂ†Â°Ă¡Â»â€ºc, rĂ¡Â»â€œi tĂ¡ÂºÂ¡o taxable base cho VAT. CĂ¡ÂºÂ£ hai khoĂ¡ÂºÂ£n dÄ‚Â¹ng decimal vÄ‚Â  `MidpointRounding.AwayFromZero`, phÄ‚Â¹ hĂ¡Â»Â£p Ă„â€˜Ă¡Â»â„¢ chÄ‚Â­nh xÄ‚Â¡c VND. Discount hiĂ¡Â»â€¡n Ă„â€˜Ă†Â°Ă¡Â»Â£c giĂ¡Â»Â¯ nhĂ†Â° snapshot legacy, cÄ‚Â²n discount client trong order mĂ¡Â»â€ºi bĂ¡Â»â€¹ Ă„â€˜Ă¡ÂºÂ·t vĂ¡Â»Â 0 Ă¡Â»Å¸ nhÄ‚Â¡nh tĂ¡ÂºÂ¡o Ă„â€˜Ă†Â¡n. VÄ‚Â¬ vĂ¡ÂºÂ­y frontend chĂ¡Â»â€° nÄ‚Âªn hiĂ¡Â»Æ’n thĂ¡Â»â€¹ preview, khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c xem tĂ¡Â»â€¢ng tiĂ¡Â»Ân client lÄ‚Â  nguĂ¡Â»â€œn xÄ‚Â¡c nhĂ¡ÂºÂ­n thanh toÄ‚Â¡n.

### e. Ă„ÂoĂ¡ÂºÂ¡n code Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t 2 Ă¢â‚¬â€ payment condition vÄ‚Â  loyalty idempotency

**NguĂ¡Â»â€œn:** `OrderService.cs:457-487` vÄ‚Â  `LoyaltyService.cs:20-44`.

```csharp
var affected = await _context.Orders
    .Where(o => o.Id == id && o.Status == "Ă„Âang xĂ¡Â»Â­ lÄ‚Â½" && o.PaidAmount <= 0
        && o.TotalAmount == amount)
    .ExecuteUpdateAsync(setters => setters
        .SetProperty(o => o.PaidAmount, amount)
        .SetProperty(o => o.PaymentMethod, paymentMethod)
        .SetProperty(o => o.PaymentAt, DateTime.UtcNow)
        .SetProperty(o => o.Status, "HoÄ‚Â n thÄ‚Â nh"));

if (affected == 1)
{
    await _loyaltyService.EarnPointsAsync(id);
    await transaction.CommitAsync();
    return (PaymentAttemptResult.Paid, await GetOrderByIdAsync(id));
}

var current = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
if (current == null) return (PaymentAttemptResult.NotFound, null);
if (current.PaidAmount > 0 || current.Status == "HoÄ‚Â n thÄ‚Â nh")
    return (PaymentAttemptResult.AlreadyPaid, current);
return (PaymentAttemptResult.NotPayable, current);
```

Trong `LoyaltyService.EarnPointsAsync`, transaction Ă„â€˜Ă†Â°Ă¡Â»Â£c mĂ¡Â»Å¸ qua execution strategy; order phĂ¡ÂºÂ£i completed vÄ‚Â  cÄ‚Â³ CustomerId; `AnyAsync(t => t.OrderId == orderId && t.Type == "Earn")` chĂ¡ÂºÂ·n earn lĂ¡ÂºÂ·p trĂ†Â°Ă¡Â»â€ºc khi cĂ¡Â»â„¢ng Ă„â€˜iĂ¡Â»Æ’m.

**ChÄ‚Âº thÄ‚Â­ch hÄ‚Â¬nh Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t:** Ă¢â‚¬Å“Thanh toÄ‚Â¡n cÄ‚Â³ Ă„â€˜iĂ¡Â»Âu kiĂ¡Â»â€¡n vÄ‚Â  chĂ¡Â»â€˜ng earn Ă„â€˜iĂ¡Â»Æ’m lĂ¡ÂºÂ·pĂ¢â‚¬Â.

### f. VietQR/refund/reversal vÄ‚Â  giĂ¡Â»â€ºi hĂ¡ÂºÂ¡n

Ă„ÂÄ‚Â£ tÄ‚Â¬m thĂ¡ÂºÂ¥y payment method/order payment vÄ‚Â  cÄ‚Â¡c method loyalty Earn/Redeem/Refund. ChĂ†Â°a xÄ‚Â¡c Ă„â€˜Ă¡Â»â€¹nh Ă„â€˜Ă†Â°Ă¡Â»Â£c trong cÄ‚Â¡c Ă„â€˜Ă†Â°Ă¡Â»Âng controller/service Ă„â€˜Ä‚Â£ Ă„â€˜Ă¡Â»Âc mĂ¡Â»â„¢t callback ngÄ‚Â¢n hÄ‚Â ng/VietQR hoĂ¡ÂºÂ·c webhook xÄ‚Â¡c nhĂ¡ÂºÂ­n thanh toÄ‚Â¡n. KhÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c viĂ¡ÂºÂ¿t rĂ¡ÂºÂ±ng ngÄ‚Â¢n hÄ‚Â ng xÄ‚Â¡c nhĂ¡ÂºÂ­n tĂ¡Â»Â± Ă„â€˜Ă¡Â»â„¢ng. RefundPoints lÄ‚Â  service path; cĂ¡ÂºÂ§n audit riÄ‚Âªng controller/luĂ¡Â»â€œng gĂ¡Â»Âi Ă„â€˜Ă¡Â»Æ’ kĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n Ă„â€˜Ä‚Â£ exposed qua API. Ă„ÂÄ‚Â¢y lÄ‚Â  code evidence, khÄ‚Â´ng phĂ¡ÂºÂ£i payment runtime evidence.

## 4.1.2.6. Module quĂ¡ÂºÂ£n lÄ‚Â½ nhÄ‚Â¢n sĂ¡Â»Â±, ca lÄ‚Â m viĂ¡Â»â€¡c vÄ‚Â  chĂ¡ÂºÂ¥m cÄ‚Â´ng

### aĂ¢â‚¬â€œc. PhĂ¡ÂºÂ¡m vi vÄ‚Â  file

Module hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i gĂ¡Â»â€œm employee, shift, work schedule vÄ‚Â  attendance. removed staff-finance module khÄ‚Â´ng Ă„â€˜Ă†Â°a vÄ‚Â o bĂ¡ÂºÂ£n Ă„â€˜Ă¡Â»â€œ nÄ‚Â y theo yÄ‚Âªu cĂ¡ÂºÂ§u. `EmployeeController` cho admin/manager list/create vÄ‚Â  manager bĂ¡Â»â€¹ Ä‚Â©p branch claim. `ShiftController` kiĂ¡Â»Æ’m tra employee/branch vÄ‚Â  khÄ‚Â´ng cho mĂ¡Â»Å¸ ca thĂ¡Â»Â© hai. `WorkScheduleController` giĂ¡Â»â€ºi hĂ¡ÂºÂ¡n manager theo branch vÄ‚Â  staff theo employee. `AttendanceController` kiĂ¡Â»Æ’m tra employee active, QR payload/branch vÄ‚Â  thĂ¡Â»Âi gian.

| File | Method/dÄ‚Â²ng | Ä‚Â nghĂ„Â©a |
|---|---|---|
| `EmployeeController.cs` | `GetEmployees` 34-68; `CreateEmployee` 91-130 | role/branch and password hash |
| `ShiftController.cs` | `GetCurrentShift`, `OpenShift`, `CloseShift` 27-127 | ca mĂ¡Â»Å¸/Ă„â€˜Ä‚Â³ng, branch, doanh thu ca |
| `WorkScheduleController.cs` | `GetSchedules`, `CreateSchedule` 27-103 | lĂ¡Â»â€¹ch vÄ‚Â  branch |
| `AttendanceController.cs` | `GetAttendances`, `CheckIn` 28-160 | query/check-in validation |
| `EmployeeService.cs` | employee business operations | service boundary nĂ†Â¡i Ă„â€˜Ă†Â°Ă¡Â»Â£c dÄ‚Â¹ng |

### d. Ă„ÂoĂ¡ÂºÂ¡n code Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t Ă¢â‚¬â€ manager branch vÄ‚Â  khÄ‚Â´ng tĂ¡ÂºÂ¡o management principal

**NguĂ¡Â»â€œn:** `services/api/src/WebAPI/Controllers/EmployeeController.cs:34-68,91-110`.

```csharp
[Authorize(Roles = "admin,manager")]
public async Task<IActionResult> GetEmployees(..., Guid? branchId)
{
    if (!IsAdmin() && !IsManager()) return Forbid();
    var query = _context.Employees.AsQueryable();

    if (IsManager())
    {
        var ownBranch = CurrentUserBranchId();
        if (ownBranch == null) return Forbid();
        query = query.Where(e => e.BranchId == ownBranch.Value);
    }
    else if (branchId.HasValue)
    {
        query = query.Where(e => e.BranchId == branchId.Value);
    }

    var employees = await query.OrderByDescending(e => e.CreatedAt).ToListAsync();
    foreach (var e in employees) e.Password = null;
    return Ok(employees);
}

if (IsManager())
{
    var ownBranch = CurrentUserBranchId();
    if (!ownBranch.HasValue) return Forbid();
    employee.BranchId = ownBranch.Value;
    if (IsManagementRole(employee.Role)) employee.Role = "employee";
}
```

**ChĂ¡Â»Â©c nĂ„Æ’ng:** manager khÄ‚Â´ng thĂ¡Â»Æ’ Ă„â€˜Ă¡Â»â€¢i branch trong create vÄ‚Â  khÄ‚Â´ng thĂ¡Â»Æ’ tĂ¡ÂºÂ¡o management role; password Ă„â€˜Ă†Â°Ă¡Â»Â£c loĂ¡ÂºÂ¡i khĂ¡Â»Âi response.

**ChÄ‚Âº thÄ‚Â­ch hÄ‚Â¬nh Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t:** Ă¢â‚¬Å“GiĂ¡Â»â€ºi hĂ¡ÂºÂ¡n nhÄ‚Â¢n sĂ¡Â»Â± theo branch claim vÄ‚Â  roleĂ¢â‚¬Â.

**ThuyĂ¡ÂºÂ¿t minh hĂ¡Â»Âc thuĂ¡ÂºÂ­t:** NhÄ‚Â¢n sĂ¡Â»Â± lÄ‚Â  dĂ¡Â»Â¯ liĂ¡Â»â€¡u nhĂ¡ÂºÂ¡y cĂ¡ÂºÂ£m nÄ‚Âªn controller khÄ‚Â´ng chĂ¡Â»â€° dĂ¡Â»Â±a vÄ‚Â o branchId do client truyĂ¡Â»Ân. Khi ngĂ†Â°Ă¡Â»Âi gĂ¡Â»Âi lÄ‚Â  manager, branch Ă„â€˜Ă†Â°Ă¡Â»Â£c lĂ¡ÂºÂ¥y tĂ¡Â»Â« claim hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i vÄ‚Â  dÄ‚Â¹ng lÄ‚Â m Ă„â€˜iĂ¡Â»Âu kiĂ¡Â»â€¡n query; tham sĂ¡Â»â€˜ branchId chĂ¡Â»â€° cÄ‚Â³ Ä‚Â½ nghĂ„Â©a trong nhÄ‚Â¡nh admin. TrĂ†Â°Ă¡Â»â€ºc khi tĂ¡ÂºÂ¡o nhÄ‚Â¢n viÄ‚Âªn, manager tiĂ¡ÂºÂ¿p tĂ¡Â»Â¥c bĂ¡Â»â€¹ Ä‚Â©p employee.BranchId vĂ¡Â»Â branch cĂ¡Â»Â§a mÄ‚Â¬nh. NĂ¡ÂºÂ¿u payload yÄ‚Âªu cĂ¡ÂºÂ§u role quĂ¡ÂºÂ£n lÄ‚Â½, role bĂ¡Â»â€¹ hĂ¡ÂºÂ¡ xuĂ¡Â»â€˜ng employee Ă„â€˜Ă¡Â»Æ’ ngĂ„Æ’n manager tĂ¡ÂºÂ¡o thÄ‚Âªm principal cÄ‚Â³ quyĂ¡Â»Ân quĂ¡ÂºÂ£n lÄ‚Â½. Response danh sÄ‚Â¡ch cĂ…Â©ng xÄ‚Â³a Password trĂ†Â°Ă¡Â»â€ºc khi trĂ¡ÂºÂ£ vĂ¡Â»Â. Ca lÄ‚Â m, lĂ¡Â»â€¹ch vÄ‚Â  chĂ¡ÂºÂ¥m cÄ‚Â´ng lĂ¡ÂºÂ·p lĂ¡ÂºÂ¡i mÄ‚Â´ hÄ‚Â¬nh branch claim Ă¡Â»Å¸ cÄ‚Â¡c controller tĂ†Â°Ă†Â¡ng Ă¡Â»Â©ng. Tuy nhiÄ‚Âªn, cÄ‚Â¡c Ă„â€˜oĂ¡ÂºÂ¡n nÄ‚Â y cĂ¡ÂºÂ§n Ă„â€˜Ă†Â°Ă¡Â»Â£c Ă„â€˜Ă¡Â»â€˜i chiĂ¡ÂºÂ¿u thÄ‚Âªm vĂ¡Â»â€ºi authorization base controller vÄ‚Â  runtime claims trĂ†Â°Ă¡Â»â€ºc khi kĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n toÄ‚Â n bĂ¡Â»â„¢ CRUD Ă„â€˜Ä‚Â£ Ă„â€˜Ă†Â°Ă¡Â»Â£c bĂ¡ÂºÂ£o vĂ¡Â»â€¡ Ă„â€˜Ă¡Â»â€œng nhĂ¡ÂºÂ¥t.

### e. GiĂ¡Â»â€ºi hĂ¡ÂºÂ¡n

Ă„ÂÄ‚Â£ xÄ‚Â¡c Ă„â€˜Ă¡Â»â€¹nh Ă„â€˜Ă†Â°Ă¡Â»Âng code cho employee/shift/schedule/attendance. ChĂ†Â°a kiĂ¡Â»Æ’m tra runtime check-in QR, xung Ă„â€˜Ă¡Â»â„¢t ca trÄ‚Âªn database thĂ¡ÂºÂ­t hoĂ¡ÂºÂ·c toÄ‚Â n bĂ¡Â»â„¢ role matrix. removed staff-finance module vÄ‚Â  salary formula cĂ¡Â»â€˜ Ä‚Â½ khÄ‚Â´ng Ă„â€˜Ă†Â°a vÄ‚Â o module nÄ‚Â y.

## 4.1.2.7. Module quĂ¡ÂºÂ£n lÄ‚Â½ chi phÄ‚Â­, bÄ‚Â¡o cÄ‚Â¡o vÄ‚Â  Business Insights

### aĂ¢â‚¬â€œc. TrÄ‚Â¡ch nhiĂ¡Â»â€¡m vÄ‚Â  file

`ExpenseController` nhĂ¡ÂºÂ­n/Ă„â€˜Ă¡Â»Âc chi phÄ‚Â­ cÄ‚Â³ branch authorization. `DashboardService` lĂ¡Â»Âc Ă„â€˜Ă†Â¡n hoÄ‚Â n thÄ‚Â nh, paid amount, khoĂ¡ÂºÂ£ng thĂ¡Â»Âi gian ViĂ¡Â»â€¡t Nam, chi phÄ‚Â­ branch vÄ‚Â  cost product/topping Ă„â€˜Ă¡Â»Æ’ tÄ‚Â­nh revenue, expense, COGS, profit. `BusinessInsightController` lĂ¡Â»Âc unread theo branch, Ă„â€˜Ä‚Â¡nh dĂ¡ÂºÂ¥u read vÄ‚Â  trĂ¡ÂºÂ£ metadata kĂ¡Â»Â³ phÄ‚Â¢n tÄ‚Â­ch. `InsightBackgroundService`/AI services tĂ¡ÂºÂ¡o insight vÄ‚Â  cÄ‚Â³ deduplication key Ă¡Â»Å¸ entity/configuration.

| File | Method/dÄ‚Â²ng | Ä‚Â nghĂ„Â©a |
|---|---|---|
| `ExpenseController.cs` | routes 20-108 | ghi/query expense, branch guard |
| `DashboardController.cs` | summary/revenue routes | HTTP reporting boundary |
| `DashboardService.cs` | `GetSummaryAsync` 41-145 | time/status/branch/revenue/cost |
| `BusinessInsightController.cs` | list/unread/read routes | evidence state and branch scope |
| `BusinessInsight.cs` / `ApplicationDbContext.cs` | status/dedup/index config | persistence constraints, khÄ‚Â´ng suy ra runtime migration |
| `InsightBackgroundService.cs` | scheduled analysis | background producer |

### d. Ă„ÂoĂ¡ÂºÂ¡n code Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t Ă¢â‚¬â€ chĂ¡Â»â€° tiÄ‚Âªu bÄ‚Â¡o cÄ‚Â¡o vÄ‚Â  phĂ¡ÂºÂ¡m vi branch

**NguĂ¡Â»â€œn:** `services/api/src/Application/Services/DashboardService.cs:41-105`.

```csharp
var nowUtc = DateTime.UtcNow;
var nowVn = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, VietnamZone);
var todayStartVn = new DateTime(nowVn.Year, nowVn.Month, nowVn.Day, 0, 0, 0);
var todayEndVn = todayStartVn.AddDays(1);
var startUtc = TimeZoneInfo.ConvertTimeToUtc(todayStartVn, VietnamZone);
var endUtc = TimeZoneInfo.ConvertTimeToUtc(todayEndVn, VietnamZone);

Guid? scopedBranchId = null;
IQueryable<Order> ordersQuery = _context.Orders.AsNoTracking();
if (!string.IsNullOrEmpty(branchId) && Guid.TryParse(branchId, out var bId))
{
    scopedBranchId = bId;
    ordersQuery = ordersQuery.Where(o => o.BranchId == bId);
}

var ordersToday = await ordersQuery
    .Where(o => o.CreatedAt >= startUtc && o.CreatedAt < endUtc
        && o.Status == "HoÄ‚Â n thÄ‚Â nh" && o.PaidAmount > 0)
    .Select(o => new { o.Id, o.CreatedAt, o.PaidAmount, o.BranchId })
    .ToListAsync();
var todayRevenue = ordersToday.Sum(o => (double)o.PaidAmount);

var expensesQuery = _context.Expenses.AsNoTracking()
    .Where(e => e.ExpenseDate >= startUtc && e.ExpenseDate < endUtc)
    .Where(e => !scopedBranchId.HasValue || e.BranchId == scopedBranchId.Value);
var totalExpenses = await expensesQuery.Select(e => (decimal?)e.Amount).SumAsync() ?? 0m;
var netProfit = (decimal)todayRevenue - totalExpenses - costOfGoodsSold;
```

**ChĂ¡Â»Â©c nĂ„Æ’ng:** report day boundary theo giĂ¡Â»Â ViĂ¡Â»â€¡t Nam, chĂ¡Â»â€° lĂ¡ÂºÂ¥y order completed/paid, scope expense theo branch vÄ‚Â  tÄ‚Â­nh net profit.

**ChÄ‚Âº thÄ‚Â­ch hÄ‚Â¬nh Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t:** Ă¢â‚¬Å“XÄ‚Â¡c Ă„â€˜Ă¡Â»â€¹nh kĂ¡Â»Â³ bÄ‚Â¡o cÄ‚Â¡o, phĂ¡ÂºÂ¡m vi branch vÄ‚Â  lĂ¡Â»Â£i nhuĂ¡ÂºÂ­nĂ¢â‚¬Â.

**ThuyĂ¡ÂºÂ¿t minh hĂ¡Â»Âc thuĂ¡ÂºÂ­t:** Dashboard khÄ‚Â´ng sĂ¡Â»Â­ dĂ¡Â»Â¥ng mĂ¡Â»Âi order Ă„â€˜Ă¡Â»Æ’ suy ra doanh thu. Ă„ÂĂ¡ÂºÂ§u tiÄ‚Âªn service chuyĂ¡Â»Æ’n thĂ¡Â»Âi Ă„â€˜iĂ¡Â»Æ’m UTC hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i sang mÄ‚Âºi giĂ¡Â»Â ViĂ¡Â»â€¡t Nam, tĂ¡ÂºÂ¡o biÄ‚Âªn ngÄ‚Â y rĂ¡Â»â€œi chuyĂ¡Â»Æ’n ngĂ†Â°Ă¡Â»Â£c vĂ¡Â»Â UTC Ă„â€˜Ă¡Â»Æ’ truy vĂ¡ÂºÂ¥n. Khi cÄ‚Â³ branchId hĂ¡Â»Â£p lĂ¡Â»â€¡, query order vÄ‚Â  expense Ă„â€˜Ă¡Â»Âu Ă„â€˜Ă†Â°Ă¡Â»Â£c giĂ¡Â»â€ºi hĂ¡ÂºÂ¡n theo branch. Doanh thu trong ngÄ‚Â y chĂ¡Â»â€° gĂ¡Â»â€œm order cÄ‚Â³ trĂ¡ÂºÂ¡ng thÄ‚Â¡i hoÄ‚Â n thÄ‚Â nh vÄ‚Â  PaidAmount dĂ†Â°Ă†Â¡ng. Chi phÄ‚Â­ Ă„â€˜Ă†Â°Ă¡Â»Â£c lĂ¡Â»Âc cÄ‚Â¹ng kĂ¡Â»Â³; giÄ‚Â¡ vĂ¡Â»â€˜n Ă„â€˜Ă†Â°Ă¡Â»Â£c lĂ¡ÂºÂ¥y tĂ¡Â»Â« cost cĂ¡Â»Â§a product/topping trong order details, sau Ă„â€˜Ä‚Â³ trĂ¡Â»Â« cÄ‚Â¹ng expense Ă„â€˜Ă¡Â»Æ’ ra net profit. Quy tĂ¡ÂºÂ¯c nÄ‚Â y phÄ‚Â¹ hĂ¡Â»Â£p vĂ¡Â»â€ºi bÄ‚Â¡o cÄ‚Â¡o vĂ¡ÂºÂ­n hÄ‚Â nh nhĂ†Â°ng khÄ‚Â´ng nÄ‚Âªn mĂ¡Â»Å¸ rĂ¡Â»â„¢ng thÄ‚Â nh kĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n vĂ¡Â»Â mĂ¡Â»Âi loĂ¡ÂºÂ¡i bÄ‚Â¡o cÄ‚Â¡o nĂ¡ÂºÂ¿u chĂ†Â°a Ă„â€˜Ă¡Â»Âc cÄ‚Â¡c method cÄ‚Â²n lĂ¡ÂºÂ¡i. Cache dashboard, timezone conversion vÄ‚Â  sĂ¡Â»Â± khÄ‚Â¡c nhau giĂ¡Â»Â¯a provider SQLite/PostgreSQL cĂ…Â©ng cĂ¡ÂºÂ§n Ă„â€˜Ă†Â°Ă¡Â»Â£c ghi chÄ‚Âº khi mÄ‚Â´ tĂ¡ÂºÂ£ kĂ¡ÂºÂ¿t quĂ¡ÂºÂ£.

### e. Business Insights

Ă„ÂĂ†Â°Ă¡Â»Âng `BusinessInsightController` cÄ‚Â³ `GetUnreadCount` lĂ¡Â»Âc status `Unread`; admin cÄ‚Â³ thĂ¡Â»Æ’ xem toÄ‚Â n cĂ¡Â»Â¥c, role khÄ‚Â¡c bĂ¡ÂºÂ¯t buĂ¡Â»â„¢c branch claim vÄ‚Â  query `BranchId`. CÄ‚Â¡c endpoint detail/read tĂ†Â°Ă†Â¡ng tĂ¡Â»Â± kiĂ¡Â»Æ’m tra branch. Entity/context cÄ‚Â³ `DeduplicationKey`, `Status`, `CreatedAt`, `PeriodStart/End` vÄ‚Â  index/unique configuration. Ă„ÂÄ‚Â¢y lÄ‚Â  bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng cho evidence lifecycle vÄ‚Â  chĂ¡Â»â€˜ng trÄ‚Â¹ng Ă¡Â»Å¸ persistence model; khÄ‚Â´ng tĂ¡Â»Â± khĂ¡ÂºÂ³ng Ă„â€˜Ă¡Â»â€¹nh background AI Ă„â€˜Ä‚Â£ tĂ¡ÂºÂ¡o insight thÄ‚Â nh cÄ‚Â´ng.

### f. GiĂ¡Â»â€ºi hĂ¡ÂºÂ¡n

ChĂ†Â°a xÄ‚Â¡c Ă„â€˜Ă¡Â»â€¹nh Ă„â€˜Ă¡ÂºÂ§y Ă„â€˜Ă¡Â»Â§ mĂ¡Â»Âi route expense/dashboard/insight tĂ¡Â»Â« frontend tĂ¡Â»â€ºi response; chĂ†Â°a cÄ‚Â³ runtime report values. KhÄ‚Â´ng Ă„â€˜Ă†Â°a cÄ‚Â¡c con sĂ¡Â»â€˜ trong tÄ‚Â i liĂ¡Â»â€¡u cĂ…Â© thÄ‚Â nh kĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ audit hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i.

## 4.1.2.8. Module Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n vÄ‚Â  quĂ¡ÂºÂ£n lÄ‚Â½ bÄ‚Â n

### aĂ¢â‚¬â€œc. TrÄ‚Â¡ch nhiĂ¡Â»â€¡m vÄ‚Â  file

`TableController` dÄ‚Â¹ng `TableService` Ă„â€˜Ă¡Â»Æ’ list/detail/create/update vÄ‚Â  lĂ¡ÂºÂ¥y QR token; manager bĂ¡Â»â€¹ giĂ¡Â»â€ºi hĂ¡ÂºÂ¡n branch. `ReservationController` chuĂ¡ÂºÂ©n hÄ‚Â³a customer phone, buĂ¡Â»â„¢c guest status Pending, scope staff vĂ¡Â»Â branch. `ReservationService` kiĂ¡Â»Æ’m tra table tĂ¡Â»â€œn tĂ¡ÂºÂ¡i vÄ‚Â  table.BranchId khĂ¡Â»â€ºp reservation.BranchId, sau Ă„â€˜Ä‚Â³ lĂ†Â°u; get/update/delete nhĂ¡ÂºÂ­n authorized branch/customer phone.

| File | Method/dÄ‚Â²ng | Ä‚Â nghĂ„Â©a |
|---|---|---|
| `TableController.cs` | `GetTables`, `GetTable`, `GetQrToken` 24-73 | list/detail/opaque QR credential |
| `TableController.cs` | `CreateTable`, `UpdateTable` 75-130 | admin/manager branch guard |
| `ReservationController.cs` | create/get/update 23-150 | customer/guest/staff ownership |
| `ReservationService.cs` | `GetByIdAsync`, `CreateReservationAsync`, `UpdateReservationAsync` 48-163 | table/branch/owner checks |
| `CreateBookingTool.cs` | `ExecuteAsync` 41-70 | AI tool gĂ¡Â»Âi reservation service |
| `ReservationPage.tsx` | customer booking form | frontend input |

### d. Ă„ÂoĂ¡ÂºÂ¡n code Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t Ă¢â‚¬â€ table/branch validation trong reservation service

**NguĂ¡Â»â€œn:** `services/api/src/Application/Services/ReservationService.cs:62-107`.

```csharp
public async Task<Reservation> CreateReservationAsync(Reservation reservation)
{
    if (reservation.Id == Guid.Empty) reservation.Id = Guid.NewGuid();
    if (reservation.CreatedAt == default) reservation.CreatedAt = DateTime.UtcNow;

    if (reservation.TableId.HasValue)
    {
        var table = await _context.Tables.FindAsync(reservation.TableId.Value);
        if (table == null) throw new InvalidOperationException("BÄ‚Â n Ă„â€˜Ă†Â°Ă¡Â»Â£c chĂ¡Â»Ân khÄ‚Â´ng tĂ¡Â»â€œn tĂ¡ÂºÂ¡i.");

        if (reservation.BranchId == Guid.Empty)
        {
            reservation.BranchId = table.BranchId;
            reservation.BranchName = table.BranchName;
        }
        else if (table.BranchId != reservation.BranchId)
        {
            throw new InvalidOperationException("BÄ‚Â n Ă„â€˜Ă†Â°Ă¡Â»Â£c chĂ¡Â»Ân khÄ‚Â´ng thuĂ¡Â»â„¢c chi nhÄ‚Â¡nh nÄ‚Â y.");
        }
        reservation.TableName = table.Name;
    }

    if (reservation.BranchId == Guid.Empty)
        throw new InvalidOperationException("Ă„ÂĂ¡ÂºÂ·t bÄ‚Â n phĂ¡ÂºÂ£i thuĂ¡Â»â„¢c mĂ¡Â»â„¢t chi nhÄ‚Â¡nh.");
    reservation.ReservationTime = DateTime.SpecifyKind(reservation.ReservationTime, DateTimeKind.Utc);
    _context.Reservations.Add(reservation);
    await _context.SaveChangesAsync();
    return reservation;
}
```

**ChĂ¡Â»Â©c nĂ„Æ’ng:** xÄ‚Â¡c minh table, suy ra branch khi cĂ¡ÂºÂ§n, tĂ¡Â»Â« chĂ¡Â»â€˜i cross-branch table vÄ‚Â  bĂ¡ÂºÂ¯t buĂ¡Â»â„¢c reservation cÄ‚Â³ branch.

**ChÄ‚Âº thÄ‚Â­ch hÄ‚Â¬nh Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t:** Ă¢â‚¬Å“KiĂ¡Â»Æ’m tra bÄ‚Â n thuĂ¡Â»â„¢c Ă„â€˜Ä‚Âºng chi nhÄ‚Â¡nh khi Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â nĂ¢â‚¬Â.

**ThuyĂ¡ÂºÂ¿t minh hĂ¡Â»Âc thuĂ¡ÂºÂ­t:** Ă„ÂĂ¡ÂºÂ·t bÄ‚Â n Ă„â€˜Ă†Â°Ă¡Â»Â£c kiĂ¡Â»Æ’m tra Ă¡Â»Å¸ service thay vÄ‚Â¬ chĂ¡Â»â€° tin vÄ‚Â o lĂ¡Â»Â±a chĂ¡Â»Ân trÄ‚Âªn giao diĂ¡Â»â€¡n. NĂ¡ÂºÂ¿u request cÄ‚Â³ TableId, service phĂ¡ÂºÂ£i tÄ‚Â¬m Ă„â€˜Ă†Â°Ă¡Â»Â£c table; khi BranchId chĂ†Â°a cÄ‚Â³, branch Ă„â€˜Ă†Â°Ă¡Â»Â£c suy ra tĂ¡Â»Â« table. NĂ¡ÂºÂ¿u request Ă„â€˜Ä‚Â£ cÄ‚Â³ branch nhĂ†Â°ng table thuĂ¡Â»â„¢c branch khÄ‚Â¡c, thao tÄ‚Â¡c bĂ¡Â»â€¹ tĂ¡Â»Â« chĂ¡Â»â€˜i. Sau Ă„â€˜Ä‚Â³ service yÄ‚Âªu cĂ¡ÂºÂ§u branch khÄ‚Â´ng rĂ¡Â»â€”ng, chuĂ¡ÂºÂ©n hÄ‚Â³a DateTime vĂ¡Â»Â UTC vÄ‚Â  lĂ†Â°u reservation. Controller cÄ‚Â²n giĂ¡Â»â€ºi hĂ¡ÂºÂ¡n trĂ¡ÂºÂ¡ng thÄ‚Â¡i Pending cho khÄ‚Â¡ch vÄ‚Â£ng lai, Ă„â€˜Ă¡Â»â€œng thĂ¡Â»Âi dÄ‚Â¹ng phone trong JWT cho customer Ă„â€˜Ä‚Â£ Ă„â€˜Ă„Æ’ng nhĂ¡ÂºÂ­p. CÄ‚Â¡c kiĂ¡Â»Æ’m tra ownership Ă¡Â»Å¸ Get/Update/Delete truyĂ¡Â»Ân authorizedBranchId hoĂ¡ÂºÂ·c authorizedCustomerPhone vÄ‚Â o service. Ă„ÂoĂ¡ÂºÂ¡n code chĂ¡Â»Â©ng minh bĂ¡ÂºÂ£o vĂ¡Â»â€¡ branch/table cĂ†Â¡ bĂ¡ÂºÂ£n, nhĂ†Â°ng khÄ‚Â´ng tĂ¡Â»Â± chĂ¡Â»Â©ng minh chĂ¡Â»â€˜ng trÄ‚Â¹ng lĂ¡Â»â€¹ch nĂ¡ÂºÂ¿u chĂ†Â°a thĂ¡ÂºÂ¥y truy vĂ¡ÂºÂ¥n overlap hoĂ¡ÂºÂ·c unique constraint tĂ†Â°Ă†Â¡ng Ă¡Â»Â©ng.

### e. GiĂ¡Â»â€ºi hĂ¡ÂºÂ¡n

ChĂ†Â°a tÄ‚Â¬m thĂ¡ÂºÂ¥y mĂ¡Â»â„¢t Ă„â€˜oĂ¡ÂºÂ¡n rÄ‚Âµ rÄ‚Â ng chĂ¡Â»Â©ng minh chĂ¡Â»â€˜ng Ă„â€˜Ă¡ÂºÂ·t trÄ‚Â¹ng theo khoĂ¡ÂºÂ£ng thĂ¡Â»Âi gian; khÄ‚Â´ng mÄ‚Â´ tĂ¡ÂºÂ£ tÄ‚Â­nh nĂ„Æ’ng Ă„â€˜Ä‚Â³ nhĂ†Â° Ă„â€˜Ä‚Â£ cÄ‚Â³. `CreateBookingTool` lÄ‚Â  Ă„â€˜Ă†Â°Ă¡Â»Âng AI gĂ¡Â»Âi service, khÄ‚Â´ng phĂ¡ÂºÂ£i bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng tool Ă„â€˜Ä‚Â£ Ă„â€˜Ă†Â°Ă¡Â»Â£c AI provider gĂ¡Â»Âi live.

## 4.1.2.9. Module TrĂ¡Â»Â£ lÄ‚Â½ Ă¡ÂºÂ£o AI Agent

### aĂ¢â‚¬â€œc. TrÄ‚Â¡ch nhiĂ¡Â»â€¡m vÄ‚Â  Ă„â€˜Ă†Â°Ă¡Â»Âng xĂ¡Â»Â­ lÄ‚Â½

`AiController` lĂ¡ÂºÂ¥y claims, tĂ¡Â»Â« chĂ¡Â»â€˜i guest chat, tĂ¡ÂºÂ¡o `AiUserContext` vÄ‚Â  gĂ¡Â»Âi orchestrator. `AiContextService` tĂ¡ÂºÂ¡o dĂ¡Â»Â¯ liĂ¡Â»â€¡u ngĂ¡Â»Â¯ cĂ¡ÂºÂ£nh theo role/branch; `PromptBuilder` chĂ¡Â»Ân prompt. `AI provider implementation` Ă„â€˜Ă¡Â»Âc `AI provider:ApiKey` vÄ‚Â  `AI provider:Model`, mĂ¡ÂºÂ·c Ă„â€˜Ă¡Â»â€¹nh model khi thiĂ¡ÂºÂ¿u cĂ¡ÂºÂ¥u hÄ‚Â¬nh. `AiToolRegistry` cÄ‚Â´ng bĂ¡Â»â€˜ schema cÄ‚Â¡c tool cÄ‚Â³ AllowedRoles. `AiPermissionService` kiĂ¡Â»Æ’m tra known role, branch requirement, AllowedRoles vÄ‚Â  risk. `AiOrchestrator` xĂ¡Â»Â­ lÄ‚Â½ history untrusted, gĂ¡Â»Âi AI provider, nhĂ¡ÂºÂ­n function call, resolve tool, permission-check, execute service vÄ‚Â  dĂ¡Â»Â«ng tĂ¡ÂºÂ¡i confirmation. Product-price flow dÄ‚Â¹ng confirmation store vÄ‚Â  endpoint admin consume token.

| File | Method/dÄ‚Â²ng | Ä‚Â nghĂ„Â©a |
|---|---|---|
| `AiController.cs` | `Chat` 50-120 | claims Ă¢â€ â€™ context Ă¢â€ â€™ orchestrator |
| `AiController.cs` | `ConfirmProductPrice` 123-151 | server-side confirmation boundary |
| `AiContextService.cs` | `GetContextAsync` 21-105 | role/branch context |
| `PromptBuilder.cs` | `BuildSystemInstruction` 12-29 | role prompt |
| `AI provider implementation.cs` | `GenerateContentAsync` 21-52 | config/model/HTTP provider |
| `AiToolRegistry.cs` | `GetAI providerToolsDefinition` 34-61 | published schema filtered by role |
| `AiPermissionService.cs` | `CanExecuteAsync` 22-50 | role/branch/risk enforcement |
| `AiOrchestrator.cs` | `ProcessAsync` 41-220 | sequential tool loop and confirmation |
| `UpdateProductPriceTool.cs` | `ExecuteAsync` 52-87 | admin-only validation and pending confirmation |
| `ProductPriceConfirmationStore.cs` | `Create`, `TryConsume` 29-87 | expiry, user binding, one-time consume |

### d. Ă„ÂoĂ¡ÂºÂ¡n code Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t 1 Ă¢â‚¬â€ orchestrator, untrusted history vÄ‚Â  sequential calls

**NguĂ¡Â»â€œn:** `services/api/src/AI/Services/AiOrchestrator.cs:41-93,147-207`.

```csharp
if (!RoleNames.IsKnownRole(userContext.Role))
    return new AiResponse { Success = false, Error = "BĂ¡ÂºÂ¡n khÄ‚Â´ng cÄ‚Â³ quyĂ¡Â»Ân sĂ¡Â»Â­ dĂ¡Â»Â¥ng AI Assistant." };
if (RoleNames.IsBranchScopedStaffRole(userContext.Role) && !userContext.BranchId.HasValue)
    return new AiResponse { Success = false, Error = "KhÄ‚Â´ng xÄ‚Â¡c Ă„â€˜Ă¡Â»â€¹nh Ă„â€˜Ă†Â°Ă¡Â»Â£c chi nhÄ‚Â¡nh cĂ¡Â»Â§a tÄ‚Â i khoĂ¡ÂºÂ£n." };

var contextData = await _contextService.GetContextAsync(userContext);
var systemInstruction = _promptBuilder.BuildSystemInstruction(userContext, contextData);
var toolsDef = _toolRegistry.GetAI providerToolsDefinition(userContext.Role);
var contentsList = new List<object>();
if (request.History != null)
    foreach (var h in request.History.Where(h => !string.IsNullOrWhiteSpace(h.Content)))
        contentsList.Add(new { role = "user", parts = new object[] {
            new { text = $"Untrusted conversation history:\n{h.Content}" } } });
contentsList.Add(new { role = "user", parts = new object[] { new { text = request.Message } } });

int iterations = 0;
const int MAX_ITERATIONS = 5;
while (iterations < MAX_ITERATIONS)
{
    var response = await _aiProvider.GenerateAsync(new { contents = contentsList.ToArray(),
        system_instruction = new { parts = new object[] { new { text = systemInstruction } } },
        tools = toolsDef.Length > 0 ? toolsDef : null });
    // parse candidates; collect model text/functionCall parts
    // resolve tool, permission-check, execute, append function response
    iterations++;
}
return new AiResponse { Success = false,
    Error = "Ă„ÂÄ‚Â£ vĂ†Â°Ă¡Â»Â£t quÄ‚Â¡ giĂ¡Â»â€ºi hĂ¡ÂºÂ¡n lĂ†Â°Ă¡Â»Â£t gĂ¡Â»Âi Tool (Sequential Tool Calling)." };
```

**LĂ†Â°u Ä‚Â½:** Ă„â€˜oĂ¡ÂºÂ¡n trÄ‚Âªn giĂ¡Â»Â¯ lĂ¡ÂºÂ¡i cÄ‚Â¡c dÄ‚Â²ng then chĂ¡Â»â€˜t vÄ‚Â  Ă„â€˜Ä‚Â¡nh dĂ¡ÂºÂ¥u phĂ¡ÂºÂ§n parse/dispatch Ă„â€˜Ä‚Â£ lĂ†Â°Ă¡Â»Â£c bĂ¡ÂºÂ±ng comment; Ă„â€˜Ä‚Â¢y khÄ‚Â´ng phĂ¡ÂºÂ£i Ă„â€˜oĂ¡ÂºÂ¡n mÄ‚Â£ liÄ‚Âªn tĂ¡Â»Â¥c nguyÄ‚Âªn vĂ„Æ’n Ă„â€˜Ă¡ÂºÂ§y Ă„â€˜Ă¡Â»Â§. KhoĂ¡ÂºÂ£ng dÄ‚Â²ng gĂ¡Â»â€˜c cĂ¡ÂºÂ§n chĂ¡Â»Â¥p khi dÄ‚Â n trang: 41-93 vÄ‚Â  147-207.

**ChĂ¡Â»Â©c nĂ„Æ’ng:** phÄ‚Â¢n biĂ¡Â»â€¡t sĂ¡Â»â€˜ vÄ‚Â²ng orchestrator (`MAX_ITERATIONS=5`) vĂ¡Â»â€ºi sĂ¡Â»â€˜ function call; history khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c giĂ¡Â»Â¯ role do client cung cĂ¡ÂºÂ¥p; tool phĂ¡ÂºÂ£i qua registry vÄ‚Â  permission.

**ChÄ‚Âº thÄ‚Â­ch hÄ‚Â¬nh Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t:** Ă¢â‚¬Å“VÄ‚Â²ng Ă„â€˜iĂ¡Â»Âu phĂ¡Â»â€˜i tuĂ¡ÂºÂ§n tĂ¡Â»Â± vÄ‚Â  biÄ‚Âªn an toÄ‚Â n cĂ¡Â»Â§a AI AgentĂ¢â‚¬Â.

**ThuyĂ¡ÂºÂ¿t minh hĂ¡Â»Âc thuĂ¡ÂºÂ­t:** Orchestrator lÄ‚Â  lĂ¡Â»â€ºp Ă„â€˜iĂ¡Â»Âu phĂ¡Â»â€˜i, khÄ‚Â´ng phĂ¡ÂºÂ£i nĂ†Â¡i chĂ¡Â»Â©a truy vĂ¡ÂºÂ¥n nghiĂ¡Â»â€¡p vĂ¡Â»Â¥. NÄ‚Â³ xÄ‚Â¡c thĂ¡Â»Â±c role vÄ‚Â  branch context, lĂ¡ÂºÂ¥y dĂ¡Â»Â¯ liĂ¡Â»â€¡u ngĂ¡Â»Â¯ cĂ¡ÂºÂ£nh, tĂ¡ÂºÂ¡o system instruction vÄ‚Â  chĂ¡Â»â€° cÄ‚Â´ng bĂ¡Â»â€˜ tool phÄ‚Â¹ hĂ¡Â»Â£p role. History gĂ¡Â»Â­i tĂ¡Â»Â« client Ă„â€˜Ă†Â°Ă¡Â»Â£c bĂ¡Â»Âc thÄ‚Â nh nĂ¡Â»â„¢i dung untrusted vĂ¡Â»â€ºi role user, trÄ‚Â¡nh Ă„â€˜Ă¡Â»Æ’ client giĂ¡ÂºÂ£ mĂ¡ÂºÂ¡o mĂ¡Â»â„¢t lĂ†Â°Ă¡Â»Â£t model Ă„â€˜Ä‚Â£ tin cĂ¡ÂºÂ­y. Khi AI provider trĂ¡ÂºÂ£ function call, orchestrator tÄ‚Â¬m tool theo tÄ‚Âªn, kiĂ¡Â»Æ’m tra permission rĂ¡Â»â€œi mĂ¡Â»â€ºi gĂ¡Â»Âi `ExecuteAsync`; kĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ Ă„â€˜Ă†Â°Ă¡Â»Â£c Ă„â€˜Ă†Â°a lĂ¡ÂºÂ¡i vÄ‚Â o conversation cho lĂ†Â°Ă¡Â»Â£t tiĂ¡ÂºÂ¿p theo. `MAX_ITERATIONS=5` lÄ‚Â  giĂ¡Â»â€ºi hĂ¡ÂºÂ¡n sĂ¡Â»â€˜ vÄ‚Â²ng trao Ă„â€˜Ă¡Â»â€¢i vĂ¡Â»â€ºi model, khÄ‚Â´ng phĂ¡ÂºÂ£i giĂ¡Â»â€ºi hĂ¡ÂºÂ¡n sĂ¡Â»â€˜ tool call tuyĂ¡Â»â€¡t Ă„â€˜Ă¡Â»â€˜i trong toÄ‚Â n bĂ¡Â»â„¢ request, vÄ‚Â¬ mĂ¡Â»â„¢t vÄ‚Â²ng cÄ‚Â³ thĂ¡Â»Æ’ chĂ¡Â»Â©a nhiĂ¡Â»Âu function call. NĂ¡ÂºÂ¿u tool trĂ¡ÂºÂ£ confirmation, vÄ‚Â²ng xĂ¡Â»Â­ lÄ‚Â½ dĂ¡Â»Â«ng ngay trĂ†Â°Ă¡Â»â€ºc khi AI provider cÄ‚Â³ thĂ¡Â»Æ’ tĂ¡Â»Â± xÄ‚Â¡c nhĂ¡ÂºÂ­n thao tÄ‚Â¡c nhĂ¡ÂºÂ¡y cĂ¡ÂºÂ£m.

### e. Ă„ÂoĂ¡ÂºÂ¡n code Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t 2 Ă¢â‚¬â€ tool registry/permission/confirmation

**NguĂ¡Â»â€œn:** `AiToolRegistry.cs:34-61`, `AiPermissionService.cs:22-50`, `UpdateProductPriceTool.cs:52-77`, `AiController.cs:123-151`.

```csharp
var allowedTools = _tools.Where(t =>
    t.AllowedRoles.Contains(role.ToLower(), StringComparer.OrdinalIgnoreCase)).ToList();
return new object[] { new {
    function_declarations = allowedTools.Select(t => t.GetSchema()).ToArray()
} };

if (!RoleNames.IsKnownRole(context.Role)) return false;
if (RoleNames.IsBranchScopedStaffRole(context.Role) && !context.BranchId.HasValue) return false;
if (!_authorization.IsRoleAllowed(tool, context)) return false;
if (!_authorization.IsRiskLevelAllowed(tool, context)) return false;
return true;

if (!string.Equals(userContext.Role, RoleNames.Admin, StringComparison.OrdinalIgnoreCase))
    return AiToolResult.CreateError("BĂ¡ÂºÂ¡n khÄ‚Â´ng cÄ‚Â³ quyĂ¡Â»Ân cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t giÄ‚Â¡ sĂ¡ÂºÂ£n phĂ¡ÂºÂ©m.");
var matches = await _productService.FindProductsByExactNameAsync(productName);
if (matches.Count != 1) return AiToolResult.CreateError("SĂ¡ÂºÂ£n phĂ¡ÂºÂ©m khÄ‚Â´ng duy nhĂ¡ÂºÂ¥t hoĂ¡ÂºÂ·c khÄ‚Â´ng tĂ¡Â»â€œn tĂ¡ÂºÂ¡i.");
var confirmation = _confirmationStore.Create(userContext.UserId, matches.Single(), newPrice);
return AiToolResult.CreateConfirmation("Vui lÄ‚Â²ng xÄ‚Â¡c nhĂ¡ÂºÂ­n Ă„â€˜Ă¡Â»â€¢i giÄ‚Â¡.", confirmation);

if (!_confirmationStore.TryConsume(request.Token, userId, out var pending, out var error))
    return BadRequest(new { message = error });
var result = await _productService.UpdatePriceByIdAsync(
    pending.ProductId, pending.ExpectedCurrentPrice, pending.ProposedNewPrice);
```

**LĂ†Â°u Ä‚Â½:** Ă„â€˜oĂ¡ÂºÂ¡n nÄ‚Â y lÄ‚Â  cÄ‚Â¡c trÄ‚Â­ch Ă„â€˜oĂ¡ÂºÂ¡n rÄ‚Âºt gĂ¡Â»Ân tĂ¡Â»Â« bĂ¡Â»â€˜n file, khÄ‚Â´ng trÄ‚Â¬nh bÄ‚Â y nhĂ†Â° mĂ¡Â»â„¢t Ă„â€˜oĂ¡ÂºÂ¡n mÄ‚Â£ liÄ‚Âªn tĂ¡Â»Â¥c. ChĂ¡Â»Â¥p riÄ‚Âªng theo tĂ¡Â»Â«ng file/dÄ‚Â²ng Ă„â€˜Ă¡Â»Æ’ trÄ‚Â¡nh lÄ‚Â m mĂ¡ÂºÂ¥t ranh giĂ¡Â»â€ºi module.

**ChĂ¡Â»Â©c nĂ„Æ’ng:** tool Ă„â€˜Ă†Â°Ă¡Â»Â£c cÄ‚Â´ng bĂ¡Â»â€˜ (`GetAI providerToolsDefinition`) khÄ‚Â¡c vĂ¡Â»â€ºi tool Ă„â€˜Ă†Â°Ă¡Â»Â£c phÄ‚Â©p execute (`CanExecuteAsync`); update price yÄ‚Âªu cĂ¡ÂºÂ§u admin vÄ‚Â  confirmation token gĂ¡ÂºÂ¯n user, expiry, one-time consume.

**ChÄ‚Âº thÄ‚Â­ch hÄ‚Â¬nh Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t:** Ă¢â‚¬Å“TĂ¡Â»Â« AllowedRoles Ă„â€˜Ă¡ÂºÂ¿n xÄ‚Â¡c nhĂ¡ÂºÂ­n thao tÄ‚Â¡c cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t giÄ‚Â¡Ă¢â‚¬Â.

### f. AI provider, context vÄ‚Â  giĂ¡Â»â€ºi hĂ¡ÂºÂ¡n

`AI provider implementation` Ă„â€˜Ă¡Â»Âc `AI provider:ApiKey`, model tĂ¡Â»Â« `AI provider:Model` hoĂ¡ÂºÂ·c default code; nĂ¡ÂºÂ¿u thiĂ¡ÂºÂ¿u key thÄ‚Â¬ throw. `AiContextService` lĂ¡Â»Âc tables theo `userContext.BranchId` cho role nhÄ‚Â¢n viÄ‚Âªn vÄ‚Â  lĂ¡ÂºÂ¥y context toÄ‚Â n hĂ¡Â»â€¡ thĂ¡Â»â€˜ng cho admin. `AiController` chĂ¡ÂºÂ·n guest bĂ¡ÂºÂ±ng `customerSessionType=guest`. ChĂ†Â°a cÄ‚Â³ bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng AI provider live, Ă„â€˜Ă¡Â»â„¢ chÄ‚Â­nh xÄ‚Â¡c hoĂ¡ÂºÂ·c persistence server-side cĂ¡Â»Â§a conversation history; history hiĂ¡Â»â€¡n Ă„â€˜i vÄ‚Â o request do client gĂ¡Â»Â­i.

## 4. Danh sÄ‚Â¡ch Ă†Â°u tiÄ‚Âªn chĂ¡Â»Â¥p code cho ChĂ†Â°Ă†Â¡ng 4

### Ă†Â¯u tiÄ‚Âªn cao

1. `OrderService.cs:255-364,388-389` Ă¢â‚¬â€ authoritative price, fee/VAT, rounding vÄ‚Â  snapshots.
2. `OrderService.cs:134-218` Ă¢â‚¬â€ merge order vÄ‚Â  bĂ¡ÂºÂ£o vĂ¡Â»â€¡ SentQuantity.
3. `KitchenService.cs:42-75` Ă¢â‚¬â€ delta quantity vÄ‚Â  OrderRequest.
4. `OrderService.cs:401-453` Ă¢â‚¬â€ accept branch/table/concurrency.
5. `AuthController.cs:127-163` + `GuestController.cs:30-63` Ă¢â‚¬â€ active/password/legacy vÄ‚Â  guest bootstrap.
6. `KitchenHub.cs:10-38` + `KitchenNotifier.cs:18-38` Ă¢â‚¬â€ phĂ¡ÂºÂ£i chĂ¡Â»Â¥p cĂ¡ÂºÂ£ nĂ†Â¡i phÄ‚Â¡t vÄ‚Â  nĂ†Â¡i nhĂ¡ÂºÂ­n event.
7. `AiOrchestrator.cs:41-93,147-207` Ă¢â‚¬â€ history untrusted, permission, sequential loop, confirmation boundary.
8. `AiToolRegistry.cs:34-61` + `AiPermissionService.cs:22-50` Ă¢â‚¬â€ published schema khÄ‚Â¡c execution permission.
9. `AiController.cs:123-151` + `ProductPriceConfirmationStore.cs:54-86` Ă¢â‚¬â€ endpoint consume confirmation.

### CÄ‚Â³ thĂ¡Â»Æ’ bĂ¡Â»Â Ă„â€˜Ă¡Â»Æ’ trÄ‚Â¡nh trÄ‚Â¹ng

- KhÄ‚Â´ng cĂ¡ÂºÂ§n chĂ¡Â»Â¥p toÄ‚Â n bĂ¡Â»â„¢ entity `Order`, `Product`, `Employee` nĂ¡ÂºÂ¿u Ă„â€˜Ä‚Â£ chĂ¡Â»Â¥p service nĂ†Â¡i quy tĂ¡ÂºÂ¯c Ă„â€˜Ă†Â°Ă¡Â»Â£c thĂ¡Â»Â±c thi.
- KhÄ‚Â´ng cĂ¡ÂºÂ§n chĂ¡Â»Â¥p cĂ¡ÂºÂ£ CRUD Product vÄ‚Â  Topping; chĂ¡Â»Ân `GetProducts` cÄ‚Â¹ng mĂ¡Â»â„¢t Ă„â€˜oĂ¡ÂºÂ¡n validation/update lÄ‚Â  Ă„â€˜Ă¡Â»Â§.
- KhÄ‚Â´ng cĂ¡ÂºÂ§n chĂ¡Â»Â¥p tĂ¡ÂºÂ¥t cĂ¡ÂºÂ£ controller ca/lĂ¡Â»â€¹ch/chĂ¡ÂºÂ¥m cÄ‚Â´ng; chĂ¡Â»Ân `EmployeeController` cho branch/role vÄ‚Â  mĂ¡Â»â„¢t Ă„â€˜oĂ¡ÂºÂ¡n `ShiftController` hoĂ¡ÂºÂ·c `AttendanceController` cho Ă„â€˜iĂ¡Â»Âu kiĂ¡Â»â€¡n nghiĂ¡Â»â€¡p vĂ¡Â»Â¥.
- KhÄ‚Â´ng cĂ¡ÂºÂ§n chĂ¡Â»Â¥p mĂ¡Â»Âi AI tool; chĂ¡Â»Ân mĂ¡Â»â„¢t read tool Ă„â€˜Ă¡Â»Æ’ minh hĂ¡Â»Âa service call vÄ‚Â  `UpdateProductPriceTool` Ă„â€˜Ă¡Â»Æ’ minh hĂ¡Â»Âa confirmation.
- KhÄ‚Â´ng Ă„â€˜Ă†Â°a cÄ‚Â¡c snippet tĂ¡Â»Â« test vÄ‚Â o hÄ‚Â¬nh triĂ¡Â»Æ’n khai; test chĂ¡Â»â€° nÄ‚Âªn lÄ‚Â  bĂ¡ÂºÂ£ng bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng kiĂ¡Â»Æ’m thĂ¡Â»Â­ riÄ‚Âªng.

## 5. NhĂ¡Â»Â¯ng phÄ‚Â¡t biĂ¡Â»Æ’u trong ChĂ†Â°Ă†Â¡ng 4 cĂ¡ÂºÂ§n sĂ¡Â»Â­a Ă„â€˜Ă¡Â»Æ’ khĂ¡Â»â€ºp code

1. ViĂ¡ÂºÂ¿t Ă¢â‚¬Å“backend kiĂ¡Â»Æ’m tra giÄ‚Â¡ authoritative tĂ¡Â»Â« database tĂ¡ÂºÂ¡i OrderServiceĂ¢â‚¬Â, khÄ‚Â´ng viĂ¡ÂºÂ¿t frontend tĂ¡Â»Â± tÄ‚Â­nh tĂ¡Â»â€¢ng lÄ‚Â  nguĂ¡Â»â€œn chÄ‚Â­nh.
2. ViĂ¡ÂºÂ¿t Ă¢â‚¬Å“SentQuantity lÄ‚Â  trĂ†Â°Ă¡Â»Âng dĂ¡Â»Â¯ liĂ¡Â»â€¡u dÄ‚Â¹ng Ă„â€˜Ă¡Â»Æ’ tĂ¡ÂºÂ¡o delta requestĂ¢â‚¬Â, khÄ‚Â´ng gĂ¡Â»Âi `SentQuantity` lÄ‚Â  mĂ¡Â»â„¢t SignalR event.
3. ViĂ¡ÂºÂ¿t Ă¢â‚¬Å“SignalR dÄ‚Â¹ng cÄ‚Â¡c event `NewOrderRequest` vÄ‚Â  `RequestStatusUpdated` vĂ¡Â»â€ºi group branch/roleĂ¢â‚¬Â, Ă„â€˜Ă¡Â»â€œng thĂ¡Â»Âi nÄ‚Âªu rĂ¡ÂºÂ±ng runtime delivery chĂ†Â°a Ă„â€˜Ă†Â°Ă¡Â»Â£c chĂ¡Â»Â©ng minh nĂ¡ÂºÂ¿u chĂ¡Â»â€° cÄ‚Â³ source evidence.
4. PhÄ‚Â¢n biĂ¡Â»â€¡t `admin` global vĂ¡Â»â€ºi manager/staff branch-scoped; khÄ‚Â´ng suy quyĂ¡Â»Ân tĂ¡Â»Â« `Position`.
5. PhÄ‚Â¢n biĂ¡Â»â€¡t customer Ă„â€˜Ă„Æ’ng kÄ‚Â½ vĂ¡Â»â€ºi guest: guest lÄ‚Â  JWT customer cÄ‚Â³ `customerSessionType=guest` vÄ‚Â  `tableId`.
6. ViĂ¡ÂºÂ¿t Ă¢â‚¬Å“AI tool Ă„â€˜Ă†Â°Ă¡Â»Â£c lĂ¡Â»Âc Ă„â€˜Ă¡Â»Æ’ cÄ‚Â´ng bĂ¡Â»â€˜ cho AI provider vÄ‚Â  tiĂ¡ÂºÂ¿p tĂ¡Â»Â¥c bĂ¡Â»â€¹ kiĂ¡Â»Æ’m tra PermissionService khi executeĂ¢â‚¬Â; khÄ‚Â´ng Ă„â€˜Ă¡Â»â€œng nhĂ¡ÂºÂ¥t AllowedRoles vĂ¡Â»â€ºi authorization hoÄ‚Â n chĂ¡Â»â€°nh.
7. ViĂ¡ÂºÂ¿t Ă¢â‚¬Å“Sequential Tool Calling tĂ¡Â»â€˜i Ă„â€˜a 5 vÄ‚Â²ng orchestratorĂ¢â‚¬Â; khÄ‚Â´ng gĂ¡Â»Âi Ă„â€˜Ä‚Â¢y lÄ‚Â  tĂ¡Â»â€˜i Ă„â€˜a 5 tool call.
8. ViĂ¡ÂºÂ¿t Ă¢â‚¬Å“history client Ă„â€˜Ă†Â°Ă¡Â»Â£c xĂ¡Â»Â­ lÄ‚Â½ nhĂ†Â° untrusted contextĂ¢â‚¬Â; khÄ‚Â´ng khĂ¡ÂºÂ³ng Ă„â€˜Ă¡Â»â€¹nh hĂ¡Â»â€¡ thĂ¡Â»â€˜ng lĂ†Â°u hĂ¡Â»â„¢i thoĂ¡ÂºÂ¡i bĂ¡Â»Ân vĂ¡Â»Â¯ng nĂ¡ÂºÂ¿u chĂ†Â°a tÄ‚Â¬m thĂ¡ÂºÂ¥y persistence path.
9. ViĂ¡ÂºÂ¿t Ă¢â‚¬Å“payment Ă„â€˜Ă¡Â»â€˜i chiĂ¡ÂºÂ¿u `Order.TotalAmount` persisted vÄ‚Â  cÄ‚Â³ Ă„â€˜iĂ¡Â»Âu kiĂ¡Â»â€¡n cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t chĂ¡Â»â€˜ng lĂ¡ÂºÂ·pĂ¢â‚¬Â; khÄ‚Â´ng khĂ¡ÂºÂ³ng Ă„â€˜Ă¡Â»â€¹nh ngÄ‚Â¢n hÄ‚Â ng/VietQR callback Ă„â€˜Ä‚Â£ xÄ‚Â¡c nhĂ¡ÂºÂ­n nĂ¡ÂºÂ¿u chĂ†Â°a thĂ¡ÂºÂ¥y endpoint/provider path.
10. ViĂ¡ÂºÂ¿t Ă¢â‚¬Å“Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n kiĂ¡Â»Æ’m tra table/branch/ownerĂ¢â‚¬Â; khÄ‚Â´ng viĂ¡ÂºÂ¿t cÄ‚Â³ chĂ¡Â»â€˜ng Ă„â€˜Ă¡ÂºÂ·t trÄ‚Â¹ng/xung Ă„â€˜Ă¡Â»â„¢t nĂ¡ÂºÂ¿u chĂ†Â°a tÄ‚Â¬m thĂ¡ÂºÂ¥y overlap query hoĂ¡ÂºÂ·c constraint cĂ¡Â»Â¥ thĂ¡Â»Æ’.
11. KhÄ‚Â´ng Ă„â€˜Ă†Â°a removed staff-finance module hoĂ¡ÂºÂ·c cÄ‚Â¡c trĂ†Â°Ă¡Â»Âng Ă„â€˜Ä‚Â£ loĂ¡ÂºÂ¡i bĂ¡Â»Â vÄ‚Â o phĂ¡ÂºÂ¡m vi module 4.1.2.6.
12. TĂ¡ÂºÂ¥t cĂ¡ÂºÂ£ kĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n trÄ‚Âªn lÄ‚Â  code evidence tĂ¡ÂºÂ¡i working tree dirty; khÄ‚Â´ng gĂ¡Â»Âi lÄ‚Â  kĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ E2E/runtime.

## 6. NĂ¡Â»â„¢i dung chĂ†Â°a Ă„â€˜Ă¡Â»Â§ cĂ„Æ’n cĂ¡Â»Â© vÄ‚Â  nĂ†Â¡i cĂ¡ÂºÂ§n kiĂ¡Â»Æ’m tra tiĂ¡ÂºÂ¿p

- ChĂ†Â°a cÄ‚Â³ cĂ„Æ’n cĂ¡Â»Â© source Ă„â€˜Ă¡ÂºÂ§y Ă„â€˜Ă¡Â»Â§ Ă„â€˜Ă¡Â»Æ’ kĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n Product/Topping CRUD Ă„â€˜Ă†Â°Ă¡Â»Â£c branch-isolated trong mĂ¡Â»Âi route.
- ChĂ†Â°a cÄ‚Â³ cĂ„Æ’n cĂ¡Â»Â© source Ă„â€˜Ă¡ÂºÂ§y Ă„â€˜Ă¡Â»Â§ Ă„â€˜Ă¡Â»Æ’ kĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n Reservation cÄ‚Â³ chĂ¡Â»â€˜ng trÄ‚Â¹ng theo thĂ¡Â»Âi gian.
- ChĂ†Â°a xÄ‚Â¡c Ă„â€˜Ă¡Â»â€¹nh callback VietQR/ngÄ‚Â¢n hÄ‚Â ng hoĂ¡ÂºÂ·c refund provider cĂ¡Â»Â¥ thĂ¡Â»Æ’.
- ChĂ†Â°a xÄ‚Â¡c Ă„â€˜Ă¡Â»â€¹nh persistence server-side cho conversation history.
- ChĂ†Â°a xÄ‚Â¡c nhĂ¡ÂºÂ­n toÄ‚Â n bĂ¡Â»â„¢ frontend handler SignalR ngoÄ‚Â i cÄ‚Â¡c component Ă„â€˜Ä‚Â£ nÄ‚Âªu.
- ChĂ†Â°a xÄ‚Â¡c nhĂ¡ÂºÂ­n runtime JWT issuer/audience/secret, password migration thĂ¡Â»Â±c tĂ¡ÂºÂ¿ hoĂ¡ÂºÂ·c claim refresh.
- ChĂ†Â°a xÄ‚Â¡c nhĂ¡ÂºÂ­n mĂ¡Â»Âi AI tool Ă„â€˜Ă¡Â»Âu gĂ¡Â»Âi Application Service; cĂ¡ÂºÂ§n Ă„â€˜Ă¡Â»Âc tĂ¡Â»Â«ng tool nĂ¡ÂºÂ¿u luĂ¡ÂºÂ­n vĂ„Æ’n mÄ‚Â´ tĂ¡ÂºÂ£ toÄ‚Â n bĂ¡Â»â„¢ registry.
- ChĂ†Â°a xÄ‚Â¡c nhĂ¡ÂºÂ­n database index/foreign-key/cascade tĂ¡Â»Â« migration runtime chĂ¡Â»â€° bĂ¡ÂºÂ±ng tÄ‚Âªn entity; nĂ¡ÂºÂ¿u cĂ¡ÂºÂ§n phÄ‚Â¡t biĂ¡Â»Æ’u schema, phĂ¡ÂºÂ£i trÄ‚Â­ch migration/DbContext configuration cĂ¡Â»Â¥ thĂ¡Â»Æ’.

**KĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n:** file nÄ‚Â y cung cĂ¡ÂºÂ¥p bĂ¡ÂºÂ£n Ă„â€˜Ă¡Â»â€œ Ă„â€˜Ă†Â°Ă¡Â»Âng xĂ¡Â»Â­ lÄ‚Â½ vÄ‚Â  cÄ‚Â¡c Ă„â€˜oĂ¡ÂºÂ¡n mÄ‚Â£ nguĂ¡Â»â€œn tiÄ‚Âªu biĂ¡Â»Æ’u Ă„â€˜Ă¡Â»Æ’ chĂ¡Â»Â¥p cho mĂ¡Â»Â¥c 4.1.2. NÄ‚Â³ khÄ‚Â´ng tuyÄ‚Âªn bĂ¡Â»â€˜ PASS runtime/E2E vÄ‚Â  khÄ‚Â´ng thay thĂ¡ÂºÂ¿ bÄ‚Â¡o cÄ‚Â¡o [CHAPTER-4-E2E-VERIFICATION.md](CHAPTER-4-E2E-VERIFICATION.md) hoĂ¡ÂºÂ·c [CHAPTER-4-EVIDENCE-AUDIT.md](CHAPTER-4-EVIDENCE-AUDIT.md).




