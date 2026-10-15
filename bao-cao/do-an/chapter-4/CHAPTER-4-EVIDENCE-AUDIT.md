# CHAPTER 4 Ă¢â‚¬â€ EVIDENCE AUDIT

Audit phĂ¡Â»Â¥c vĂ¡Â»Â¥ ChĂ†Â°Ă†Â¡ng 4 khÄ‚Â³a luĂ¡ÂºÂ­n **Ă¢â‚¬Å“HĂ¡Â»â€¡ thĂ¡Â»â€˜ng quĂ¡ÂºÂ£n lÄ‚Â½ nhÄ‚Â  hÄ‚Â ng Ă„â€˜a chi nhÄ‚Â¡nh tÄ‚Â­ch hĂ¡Â»Â£p TrĂ¡Â»Â£ lÄ‚Â½ Ă¡ÂºÂ£o AI AgentĂ¢â‚¬Â**.

## A. PhiÄ‚Âªn bĂ¡ÂºÂ£n vÄ‚Â  phĂ¡ÂºÂ¡m vi audit

### A.1. PhiÄ‚Âªn bĂ¡ÂºÂ£n Ă„â€˜Ă†Â°Ă¡Â»Â£c audit

| MĂ¡Â»Â¥c | KĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ |
|---|---|
| ThĂ¡Â»Âi Ă„â€˜iĂ¡Â»Æ’m audit | 2026-10-07, 11:49:02 (+07:00) |
| Commit `HEAD` | `0a480965f5a582c6da5c30fb70d9a5a2d1cd3d4e` |
| Commit date | 2026-10-02 17:23:17 (+07:00) |
| Commit message | `chore: remove obsolete Docker Compose configuration` |
| Branch | `main`, tracking `origin/main` |
| TrĂ¡ÂºÂ¡ng thÄ‚Â¡i working tree | **KhÄ‚Â´ng sĂ¡ÂºÂ¡ch**: cÄ‚Â³ 73 file tracked thay Ă„â€˜Ă¡Â»â€¢i vÄ‚Â  nhiĂ¡Â»Âu file/thĂ†Â° mĂ¡Â»Â¥c untracked. |
| PhiÄ‚Âªn bĂ¡ÂºÂ£n thĂ¡Â»Â±c tĂ¡ÂºÂ¿ Ă„â€˜Ă†Â°Ă¡Â»Â£c Ă„â€˜Ă¡Â»Âc | Working tree tĂ¡ÂºÂ¡i thĂ¡Â»Âi Ă„â€˜iĂ¡Â»Æ’m audit; khÄ‚Â´ng chĂ¡Â»â€° `HEAD`. |

CÄ‚Â¡c thay Ă„â€˜Ă¡Â»â€¢i chĂ†Â°a commit liÄ‚Âªn quan trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p tĂ¡Â»â€ºi AI, xÄ‚Â¡c thĂ¡Â»Â±c, QR guest, order, kitchen, branch isolation, migration vÄ‚Â  test. VÄ‚Â¬ vĂ¡ÂºÂ­y khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c ghi bÄ‚Â¡o cÄ‚Â¡o nhĂ†Â° mĂ¡Â»â„¢t release/commit Ă„â€˜Ä‚Â£ phÄ‚Â¡t hÄ‚Â nh. Audit khÄ‚Â´ng sĂ¡Â»Â­a cÄ‚Â¡c thay Ă„â€˜Ă¡Â»â€¢i cÄ‚Â³ sĂ¡ÂºÂµn.

### A.2. PhĂ¡ÂºÂ¡m vi vÄ‚Â  giĂ¡Â»â€ºi hĂ¡ÂºÂ¡n

- Ă„ÂÄ‚Â£ Ă„â€˜Ă¡Â»Âc mÄ‚Â£ nguĂ¡Â»â€œn backend, hai frontend, entity/DbContext, migration, cĂ¡ÂºÂ¥u hÄ‚Â¬nh vÄ‚Â  test hiĂ¡Â»â€¡n cÄ‚Â³.
- Ă„ÂÄ‚Â£ chĂ¡ÂºÂ¡y test unit/integration dÄ‚Â¹ng SQLite in-memory; khÄ‚Â´ng khĂ¡Â»Å¸i Ă„â€˜Ă¡Â»â„¢ng API.
- Ă„ÂÄ‚Â£ build backend vÄ‚Â  hai frontend.
- ChĂ†Â°a chĂ¡ÂºÂ¡y API, Swagger, browser manual test, QR camera, SignalR runtime, thanh toÄ‚Â¡n VietQR, callback/webhook, E2E, load test hoĂ¡ÂºÂ·c migration trÄ‚Âªn PostgreSQL/Supabase.
- KhÄ‚Â´ng Ă„â€˜Ă¡Â»Âc/ghi giÄ‚Â¡ trĂ¡Â»â€¹ bÄ‚Â­ mĂ¡ÂºÂ­t. Connection string, JWT secret, external AI provider key vÄ‚Â  mĂ¡ÂºÂ­t khĂ¡ÂºÂ©u khÄ‚Â´ng xuĂ¡ÂºÂ¥t hiĂ¡Â»â€¡n trong bÄ‚Â¡o cÄ‚Â¡o.

## B. CĂ¡ÂºÂ¥u trÄ‚Âºc vÄ‚Â  mÄ‚Â´i trĂ†Â°Ă¡Â»Âng thĂ¡Â»Â±c tĂ¡ÂºÂ¿

### B.1. CĂ¡ÂºÂ¥u trÄ‚Âºc thĂ†Â° mĂ¡Â»Â¥c

```text
apps/
  admin-web/                 React + Vite; POS, KDS vÄ‚Â  cÄ‚Â¡c trang quĂ¡ÂºÂ£n trĂ¡Â»â€¹
  customer-web/              React + Vite; QR/menu/Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n/customer
packages/shared/             swagger, generated API types, formatter
services/api/                ASP.NET Core API
  src/Domain/                entity vÄ‚Â  enum
  src/Application/           DTO, interface, application service
  src/Infrastructure/        EF Core DbContext, initializer, migrations
  src/WebAPI/                controller, hub
  src/AI/                    controller, orchestration, tools, permission
tests/RestaurantPOS.Tests/   xUnit + SQLite in-memory
scripts/                     diagnostic/migration-related projects
```

BĂ¡ÂºÂ±ng chĂ¡Â»Â©ng: danh sÄ‚Â¡ch project `.csproj` Ă¡Â»Å¸ `services/api/RestaurantPOS.api.csproj`, `tests/RestaurantPOS.Tests/RestaurantPOS.Tests.csproj` vÄ‚Â  cÄ‚Â¡c manifest frontend. POS nĂ¡ÂºÂ±m trong `apps/admin-web/src/features/pos/pages/POSPage.tsx`; KDS nĂ¡ÂºÂ±m trong `apps/admin-web/src/features/kitchen/pages/KitchenPage.tsx` vÄ‚Â  `KitchenHistoryPage.tsx`. KhÄ‚Â´ng cÄ‚Â³ frontend KDS riÄ‚Âªng Ă„â€˜Ă†Â°Ă¡Â»Â£c xÄ‚Â¡c minh.

`packages/shared` tĂ¡Â»â€œn tĂ¡ÂºÂ¡i vĂ¡Â»â€ºi `swagger.json`, `types/api.ts`, `types/models.ts`, `utils/formatters.ts`, nhĂ†Â°ng khÄ‚Â´ng tÄ‚Â¬m thĂ¡ÂºÂ¥y import trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p tĂ¡Â»Â« `apps/*` tĂ¡Â»â€ºi package nÄ‚Â y. Root `package.json` chĂ¡Â»â€° cung cĂ¡ÂºÂ¥p cÄ‚Â¡c script `codegen`, `codegen:file`, `codegen:check`; Ă„â€˜Ä‚Â¢y lÄ‚Â  bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng vĂ¡Â»Â cĂ†Â¡ chĂ¡ÂºÂ¿ dĂ¡Â»Â± kiĂ¡ÂºÂ¿n/generated types, chĂ†Â°a phĂ¡ÂºÂ£i bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng hai frontend Ă„â€˜ang sĂ¡Â»Â­ dĂ¡Â»Â¥ng chÄ‚Âºng runtime.

### B.2. PhiÄ‚Âªn bĂ¡ÂºÂ£n tĂ¡Â»Â« manifest

| ThÄ‚Â nh phĂ¡ÂºÂ§n | BĂ¡ÂºÂ±ng chĂ¡Â»Â©ng |
|---|---|
| .NET | `services/api/RestaurantPOS.api.csproj:6`, `TargetFramework net8.0` |
| EF Core | `services/api/RestaurantPOS.api.csproj:14-18`, `Microsoft.EntityFrameworkCore 8.0.31` |
| Npgsql | `services/api/RestaurantPOS.api.csproj:19-20`, provider `8.0.11`, package `Npgsql 8.0.9` |
| React | `apps/admin-web/package.json`, `apps/customer-web/package.json`, React `18.2.0` |
| TypeScript | hai manifest, `typescript 5.0.2` |
| Vite | hai manifest, Vite `4.4.5` (lock/build resolved `4.5.14`) |
| Tailwind | hai manifest, `tailwindcss ^3.3.3` vÄ‚Â  `tailwind.config.js` |
| SignalR | `apps/admin-web/package.json`, `@microsoft/signalr ^10.0.11`; backend `Program.cs:33,439` |
| Test | xUnit `2.9.0`, runner `2.8.2`, Microsoft test SDK `17.10.0`, SQLite provider `8.0.31` |

### B.3. API, HTTPS, CORS vÄ‚Â  database safety

- API Ă„â€˜Ă„Æ’ng kÄ‚Â½ Npgsql tĂ¡ÂºÂ¡i `services/api/Program.cs:324-332`, Ă„â€˜Ă¡Â»Âc `ConnectionStrings:DefaultConnection` hoĂ¡ÂºÂ·c `SUPABASE_CONNECTION_STRING` tĂ¡ÂºÂ¡i `312-318`; khÄ‚Â´ng cÄ‚Â³ fallback localhost.
- Kestrel lĂ¡ÂºÂ¯ng nghe HTTPS port `5000` tĂ¡ÂºÂ¡i `Program.cs:371-388`. Certificate path cÄ‚Â³ thĂ¡Â»Æ’ lĂ¡ÂºÂ¥y tĂ¡Â»Â« `HTTPS_CERT_PATH`; nĂ¡ÂºÂ¿u khÄ‚Â´ng cÄ‚Â³ certificate thÄ‚Â¬ API nÄ‚Â©m lĂ¡Â»â€”i tĂ¡ÂºÂ¡i `383-384`.
- CORS lĂ¡ÂºÂ¥y tĂ¡Â»Â« `Cors:AllowedOrigins` tĂ¡ÂºÂ¡i `334-368`, cho phÄ‚Â©p cÄ‚Â¡c origin localhost/127.0.0.1 cĂ¡Â»â€¢ng 5173/5174 vÄ‚Â  HTTPS tĂ†Â°Ă†Â¡ng Ă¡Â»Â©ng trong Development. KhÄ‚Â´ng thĂ¡Â»Æ’ xÄ‚Â¡c nhĂ¡ÂºÂ­n LAN IP thĂ¡Â»Â±c tĂ¡ÂºÂ¿ tĂ¡Â»Â« code hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i; khÄ‚Â´ng cÄ‚Â³ runtime check.
- REST controllers Ă„â€˜Ă†Â°Ă¡Â»Â£c map tĂ¡ÂºÂ¡i `Program.cs:438`; health endpoint lÄ‚Â  `GET /health` tĂ¡ÂºÂ¡i `431`; SignalR endpoint lÄ‚Â  `/kitchenHub` tĂ¡ÂºÂ¡i `439`.
- Migration/seed **khÄ‚Â´ng tĂ¡Â»Â± Ă„â€˜Ă¡Â»â„¢ng chĂ¡ÂºÂ¡y mĂ¡ÂºÂ·c Ă„â€˜Ă¡Â»â€¹nh**. ChĂ¡Â»â€° khi `ASPNETCORE_ENVIRONMENT=Development` vÄ‚Â  `DatabaseSafety:AllowAutomaticMigration=true`, `Program.cs:392-417` mĂ¡Â»â€ºi gĂ¡Â»Âi `db.Database.Migrate()` rĂ¡Â»â€œi `DbInitializer.SeedAsync`. NĂ¡ÂºÂ¿u khÄ‚Â´ng, API chĂ¡Â»â€° log rĂ¡ÂºÂ±ng automatic mutation bĂ¡Â»â€¹ tĂ¡ÂºÂ¯t.
- `DbInitializer.SeedAsync` nĂ¡ÂºÂ±m Ă¡Â»Å¸ `services/api/src/Infrastructure/Persistence/DbInitializer.cs:16`; mĂ¡ÂºÂ­t khĂ¡ÂºÂ©u seed yÄ‚Âªu cĂ¡ÂºÂ§u secure configuration tĂ¡ÂºÂ¡i khoĂ¡ÂºÂ£ng dÄ‚Â²ng 70-73.
- Audit khÄ‚Â´ng chĂ¡ÂºÂ¡y migration, seed hoĂ¡ÂºÂ·c API vÄ‚Â¬ cĂ¡ÂºÂ¥u hÄ‚Â¬nh runtime yÄ‚Âªu cĂ¡ÂºÂ§u PostgreSQL/Supabase vÄ‚Â  certificate; khÄ‚Â´ng cÄ‚Â³ xÄ‚Â¡c nhĂ¡ÂºÂ­n database isolated Ă„â€˜Ă¡Â»Æ’ cho phÄ‚Â©p cÄ‚Â¡c thao tÄ‚Â¡c Ă„â€˜Ä‚Â³.

## C. BĂ¡ÂºÂ£ng module vÄ‚Â  bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng

| NhÄ‚Â³m | ChĂ¡Â»Â©c nĂ„Æ’ng Ă„â€˜Ä‚Â£ xÄ‚Â¡c minh trong code | Vai trÄ‚Â²/phĂ¡ÂºÂ¡m vi | UI/component | Controller/endpoint chÄ‚Â­nh | Service/entity chÄ‚Â­nh | KiĂ¡Â»Æ’m tra, test vÄ‚Â  hĂ¡ÂºÂ¡n chĂ¡ÂºÂ¿ Ă„â€˜Ä‚Â£ xÄ‚Â¡c minh |
|---|---|---|---|---|---|---|
| A. Auth, quyĂ¡Â»Ân, branch | Staff/customer JWT; token validation kiĂ¡Â»Æ’m tra role, employee/customer tĂ¡Â»â€œn tĂ¡ÂºÂ¡i vÄ‚Â  active; manager bĂ¡Â»â€¹ giĂ¡Â»â€ºi hĂ¡ÂºÂ¡n `branchId`; guest cÄ‚Â³ session type riÄ‚Âªng | `admin` global; `manager` vÄ‚Â  staff theo branch claim; `customer` registered/guest | `apps/admin-web/src/features/auth/pages/LoginPage.tsx`, `ProfilePage.tsx`; `apps/customer-web/src/pages/CustomerLogin.tsx` | `AuthController`: `POST /api/Auth/login`, `POST /api/Auth/customer-token`, `GET /api/Auth/me`, `POST /api/Auth/change-password`; `BranchController` | `JwtService`, `PasswordPolicy`, `LegacyPasswordVerifier`, `RoleNames`, `Employee`, `Customer`, `Branch` | `Program.cs:77-188` kiĂ¡Â»Æ’m tra JWT hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i vÄ‚Â  inactive; `AuthController.cs:129-169,288-306,352-412`; test auth/security/branch. MĂ¡ÂºÂ­t khĂ¡ÂºÂ©u entity vĂ¡ÂºÂ«n lÄ‚Â  property `Password` (`Employee.cs:29`, `Customer.cs:20`); code hĂ¡Â»â€” trĂ¡Â»Â£ legacy verification vÄ‚Â  rehash, cĂ¡ÂºÂ§n runtime/database audit Ă„â€˜Ă¡Â»Æ’ xÄ‚Â¡c Ă„â€˜Ă¡Â»â€¹nh tĂ¡Â»Â· lĂ¡Â»â€¡ tÄ‚Â i khoĂ¡ÂºÂ£n cĂ…Â©. UI guards khÄ‚Â´ng thay thĂ¡ÂºÂ¿ API authorization. |
| B. Menu/product | CRUD product, topping, category/availability; backend lĂ¡ÂºÂ¥y dĂ¡Â»Â¯ liĂ¡Â»â€¡u product/topping | Admin quĂ¡ÂºÂ£n trĂ¡Â»â€¹; manager/kitchen cÄ‚Â³ quyĂ¡Â»Ân availability theo controller; branch scope tÄ‚Â¹y endpoint | `ProductManagement.tsx`, `ToppingManagement.tsx`, `POSPage.tsx`, customer `DigitalMenu.tsx` | `ProductController`, `ToppingController`; GET/POST/PUT/DELETE/PATCH status/availability | `ProductService`, `IProductService`, `Product`, `Topping`, `ApplicationDbContext` | Controller attributes tĂ¡ÂºÂ¡i `ProductController.cs:33-169`, `ToppingController.cs:21-78`; giÄ‚Â¡ order Ă„â€˜Ă†Â°Ă¡Â»Â£c tÄ‚Â­nh lĂ¡ÂºÂ¡i tĂ¡ÂºÂ¡i `OrderService.cs:255-363`. KhÄ‚Â´ng cÄ‚Â³ bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng frontend dÄ‚Â¹ng `packages/shared` generated types. |
| C. QR ordering/POS | Guest bootstrap/session; tĂ¡ÂºÂ¡o order mĂ¡Â»â€ºi hoĂ¡ÂºÂ·c merge order mĂ¡Â»Å¸ theo bÄ‚Â n/branch; pending web order cĂ¡ÂºÂ§n staff accept; POS tĂ¡ÂºÂ¡o/cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t order | Guest chĂ¡Â»â€° theo table token/session vÄ‚Â  branch; authenticated staff theo branch; admin global | Admin `POSPage.tsx`, `TableStatusPage.tsx`; customer `QRScan.tsx`, `DigitalMenu.tsx` | `GuestController: POST /api/guest/bootstrap`; `OrderController: GET/POST /api/Order`, `POST /api/Order/{id}/accept`; table QR endpoint tĂ¡ÂºÂ¡i `TableController` | `OrderController`, `OrderService`, `GuestSessionContext`, `Order`, `OrderItem`, `OrderRequest`, `RestaurantTable` | `OrderController.cs:49-195,206-238`; `OrderService.cs:126-255,391-453`; guest kiĂ¡Â»Æ’m tra table active + branch. Ă„ÂÄ‚Â£ cÄ‚Â³ test guest QR, order merge, branch isolation. Browser QR/camera vÄ‚Â  full E2E chĂ†Â°a chĂ¡ÂºÂ¡y. |
| D. KDS/SignalR | Send cÄ‚Â¡c mÄ‚Â³n mĂ¡Â»â€ºi xuĂ¡Â»â€˜ng bĂ¡ÂºÂ¿p theo chÄ‚Âªnh lĂ¡Â»â€¡ch `Quantity - SentQuantity`; transaction cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t request/order rĂ¡Â»â€œi notifier; group branch/role cho pending change | Hub chĂ¡Â»â€° staff roles `admin,manager,employee,cashier,kitchen`; branch group dÄ‚Â¹ng Ă¡Â»Å¸ pending notifications | Admin `KitchenPage.tsx`, `KitchenHistoryPage.tsx`; frontend connection search chĂ¡Â»â€° xÄ‚Â¡c minh Ă¡Â»Å¸ admin | `POST /api/Order/{id}/send-to-kitchen`, kitchen status/history endpoints; `/kitchenHub` | `KitchenHub`, `KitchenService`, `OrderService`, `KitchenNotifier`, `OrderRequest`, `SentQuantity` | `KitchenHub.cs:10`; map `Program.cs:439`; controller `OrderController.cs:242-276,423-468`; SignalR/events tĂ¡ÂºÂ¡i `OrderController.cs:529-544` vÄ‚Â  notifier. KhÄ‚Â´ng thĂ¡ÂºÂ¥y customer-web tĂ¡ÂºÂ¡o kitchenHub connection. Runtime connection/group delivery chĂ†Â°a chĂ¡ÂºÂ¡y; khÄ‚Â´ng khĂ¡ÂºÂ³ng Ă„â€˜Ă¡Â»â€¹nh E2E KDS. |
| E. Payment/Loyalty | Payment kiĂ¡Â»Æ’m tra amount Ă„â€˜Ä‚Âºng persisted `Order.TotalAmount`; status/paid/payment time; loyalty Earn/Redeem vÄ‚Â  transaction/idempotency checks | Payment staff roles; loyalty customer/staff theo controller vÄ‚Â  branch/order ownership | POS payment UI trong `POSPage.tsx`; loyalty order UI/API references | `POST /api/Order/{id}/payment`, `GET /api/Order/{id}/loyalty`, redeem endpoint trong `OrderController` | `OrderService.PayOrderAsync`, `LoyaltyService`, `Order`, `LoyaltyTransaction` | Payment validation `OrderController.cs:279-315`; financial calc `OrderService.cs:255-363`; loyalty checks `LoyaltyService.cs:23-178`. CÄ‚Â³ transaction/execution strategy cho order/loyalty. KhÄ‚Â´ng cÄ‚Â³ bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng callback/webhook VietQR; khÄ‚Â´ng chĂ¡ÂºÂ¡y thanh toÄ‚Â¡n thĂ¡ÂºÂ­t hoĂ¡ÂºÂ·c race qua HTTP. |
| F. HR/shift/schedule/attendance | Employee CRUD/toggle; shift open/close; work schedule; check-in/out | Admin/manager quĂ¡ÂºÂ£n lÄ‚Â½; staff xem/thao tÄ‚Â¡c theo attribute vÄ‚Â  branch checks | `EmployeeManagement.tsx`, `EmployeeAttendance.tsx`, `AttendanceManagement.tsx`, `ShiftManagement.tsx`, `EmployeeSchedule.tsx`, `WorkSchedulePage.tsx` | `EmployeeController`, `ShiftController`, `WorkScheduleController`, `AttendanceController` | `EmployeeService`, entities `Employee`, `Shift`, `WorkSchedule`, `Attendance` | Endpoint evidence `AttendanceController.cs:18-185`, `ShiftController.cs:17-132`, `WorkScheduleController.cs:17-107`. Tests attendance, employee authorization, authentication. removed staff-finance module khÄ‚Â´ng cÄ‚Â²n lÄ‚Â  module live Ă„â€˜Ă†Â°Ă¡Â»Â£c xÄ‚Â¡c minh; migration `20260919075618_Removeremoved staff-finance moduleAndRepairReceiptSettings.cs` lÄ‚Â  bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng lĂ¡Â»â€¹ch sĂ¡Â»Â­ loĂ¡ÂºÂ¡i bĂ¡Â»Â bĂ¡ÂºÂ£ng, khÄ‚Â´ng dÄ‚Â¹ng Ă„â€˜Ă¡Â»Æ’ khĂ¡ÂºÂ³ng Ă„â€˜Ă¡Â»â€¹nh removed staff-finance module Ă„â€˜Ä‚Â£ triĂ¡Â»Æ’n khai. |
| G. Expense/Dashboard/Business Insights | Expense CRUD; dashboard summary/revenue/best sellers; business insight unread/read/resolve; proactive insight service/background service | Admin/manager; manager branch-scoped qua controller; dashboard service query theo branch | `Dashboard.tsx`, `BusinessInsights.tsx`, `ExpenseManagement.tsx`, `InvoiceHistory.tsx` | `ExpenseController`, `DashboardController`, `BusinessInsightController` | `DashboardService`, `FinancialAnalysisService`, `ProactiveInsightService`, `BusinessInsight`, `Expense` | `DashboardController.cs:15-46`, `BusinessInsightController.cs:16-156`, `ExpenseController.cs:17-104`; dashboard has branch filters in `DashboardService.cs:47-89,334-358`. Build/test passed; no load/performance measurement, no TPS/P95/P99 claim. |
| H. Reservation/table | Reservation create/read/update/status/delete; table/area/table status/QR; branch checks | Customer/anonymous create path exists; staff status management; admin/manager table management | `ReservationManagement.tsx`, `TableManagement.tsx`, `ReservationPage.tsx`, `TableStatusPage.tsx` | `ReservationController`, `TableController`, `AreaController` | `ReservationService`, `TableService`, `Reservation`, `RestaurantTable`, `Area` | `ReservationController.cs:24-217`, `TableController.cs:25-152`; tests `TableReservationHardeningTests`, `QrTokenFoundationTests`. Runtime UI/API flow not manually run. |
| I. AI Agent | Prompt/context assembly, AI provider function calling, role-filtered registry, permission/risk check, up to 5 sequential iterations, confirmation boundary for sensitive price write | Admin/manager read analytics; staff operational read/write order status; customer menu/order/booking; admin product price write | `apps/admin-web/src/components/ChatBot.tsx` | `POST /api/Ai/chat`; admin-only `POST /api/Ai/product-price-confirmation` | `AiController`, `AiOrchestrator`, `PromptBuilder`, `AiContextService`, `AI provider implementation`, `AiToolRegistry`, `AiPermissionService`, `IAiTool` | `AiOrchestrator.cs:37-217`, `AiToolRegistry.cs:21-61`, `AiPermissionService.cs:22-50`; tests AI orchestration/security/validation. No experiment dataset or measured accuracy evidence found. |

### C.1. AI tools Ă„â€˜Ă„Æ’ng kÄ‚Â½ thĂ¡Â»Â±c tĂ¡ÂºÂ¿

| Tool | Role | Risk | Ghi chÄ‚Âº triĂ¡Â»Æ’n khai |
|---|---|---|---|
| `get_revenue`, `get_revenue_comparison`, `get_business_summary`, `get_financial_analysis`, `get_active_staff` | admin, manager | Read | Admin tools gĂ¡Â»Âi application services/query service, khÄ‚Â´ng cÄ‚Â³ bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng AiOrchestrator truy cĂ¡ÂºÂ­p DbContext trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p. |
| `get_order_list`, `get_best_sellers` | admin, manager, employee, cashier, kitchen | Read | DĂ¡Â»Â¯ liĂ¡Â»â€¡u Ă„â€˜Ă†Â°Ă¡Â»Â£c lĂ¡Â»Âc theo `AiUserContext`/branch trong tool/service. |
| `update_product_price` | admin | Write | `UpdateProductPriceTool.cs:18-52`; cÄ‚Â³ confirmation model/store trong working tree vÄ‚Â  endpoint xÄ‚Â¡c nhĂ¡ÂºÂ­n admin-only. Runtime confirmation chĂ†Â°a chĂ¡ÂºÂ¡y. |
| `employee_get_table_summary`, `get_my_shift` | admin, manager, employee, cashier, kitchen (shift tool khÄ‚Â´ng gĂ¡Â»â€œm admin/manager theo code metadata) | Read | Ă„ÂĂ¡Â»Âc thÄ‚Â´ng tin vĂ¡ÂºÂ­n hÄ‚Â nh/ca qua Application Service. |
| `update_order_status` | admin, manager, employee, cashier, kitchen | Write | Thay Ă„â€˜Ă¡Â»â€¢i trĂ¡ÂºÂ¡ng thÄ‚Â¡i order qua service; cĂ¡ÂºÂ§n manual authorization matrix Ă„â€˜Ă¡Â»Æ’ xÄ‚Â¡c nhĂ¡ÂºÂ­n runtime. |
| `customer_get_menu` | admin, manager, employee, cashier, kitchen, customer | Read | Menu read. |
| `get_my_orders`, `create_booking` | customer | Read / Write | Customer-scoped order/booking. |

## D. LuĂ¡Â»â€œng nghiĂ¡Â»â€¡p vĂ¡Â»Â¥ quan trĂ¡Â»Âng

### D.1. QR/POS

1. Guest gĂ¡Â»Âi `POST /api/guest/bootstrap` (`GuestController.cs:30`) Ă„â€˜Ă¡Â»Æ’ tĂ¡ÂºÂ¡o/kiĂ¡Â»Æ’m tra guest session tĂ¡Â»Â« QR/table token.
2. `OrderController.CreateOrder` phÄ‚Â¢n biĂ¡Â»â€¡t guest vÄ‚Â  authenticated user (`OrderController.cs:119-195`), kiĂ¡Â»Æ’m tra bÄ‚Â n active vÄ‚Â  `BranchId` cĂ¡Â»Â§a session.
3. Order mĂ¡Â»â€ºi Ă„â€˜Ă†Â°Ă¡Â»Â£c tĂ¡ÂºÂ¡o khi payload khÄ‚Â´ng trĂ¡Â»Â order hiĂ¡Â»â€¡n hĂ¡Â»Â¯u; nĂ¡ÂºÂ¿u cÄ‚Â³ order mĂ¡Â»Å¸ cÄ‚Â¹ng bÄ‚Â n/branch, `OrderService.CreateOrUpdateOrderAsync` merge item. VĂ¡Â»â€ºi item Ă„â€˜Ä‚Â£ gĂ¡Â»Â­i bĂ¡ÂºÂ¿p, quantity khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c giĂ¡ÂºÂ£m thĂ¡ÂºÂ¥p hĂ†Â¡n `SentQuantity` (`OrderService.cs:126-255`).
4. Order guest/pending cÄ‚Â³ trĂ¡ÂºÂ¡ng thÄ‚Â¡i `Ă„Âang xĂ¡Â»Â­ lÄ‚Â½` vÄ‚Â  chĂ†Â°a cÄ‚Â³ `CreatedBy`; staff phĂ¡ÂºÂ£i gĂ¡Â»Âi `POST /api/Order/{id}/accept` trĂ†Â°Ă¡Â»â€ºc. `AcceptWebOrderAsync` kiĂ¡Â»Æ’m tra branch, trĂ¡ÂºÂ¡ng thÄ‚Â¡i, table vÄ‚Â  dÄ‚Â¹ng transaction/conditional update (`OrderService.cs:391-453`).
5. ChĂ¡Â»â€° sau bĂ†Â°Ă¡Â»â€ºc send-to-kitchen mĂ¡Â»â€ºi gĂ¡Â»Âi `KitchenService.SendToKitchenAsync`; controller endpoint nĂ¡ÂºÂ±m tĂ¡ÂºÂ¡i `OrderController.cs:242-265`.

KĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n Ă„â€˜Ă†Â°Ă¡Â»Â£c phÄ‚Â©p ghi: code hiĂ¡Â»â€¡n cÄ‚Â³ gate accept cho pending web order. KhÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c ghi rĂ¡ÂºÂ±ng toÄ‚Â n bĂ¡Â»â„¢ order POS Ă„â€˜Ă¡Â»Âu bĂ¡ÂºÂ¯t buĂ¡Â»â„¢c accept nĂ¡ÂºÂ¿u khÄ‚Â´ng phÄ‚Â¢n biĂ¡Â»â€¡t nguĂ¡Â»â€œn order.

### D.2. KDS vÄ‚Â  SignalR

- Endpoint lÄ‚Â  `/kitchenHub` (`Program.cs:439`), JWT query-string chĂ¡Â»â€° Ă„â€˜Ă†Â°Ă¡Â»Â£c nhĂ¡ÂºÂ­n cho path `/kitchenHub` (`Program.cs:106-117`).
- Hub cÄ‚Â³ `[Authorize(Roles = "admin,manager,employee,cashier,kitchen")]` (`KitchenHub.cs:10`).
- Controller phÄ‚Â¡t `PendingOrderChanged` theo group branch/role tĂ¡ÂºÂ¡i `OrderController.cs:529-544`; payment path cÄ‚Â²n phÄ‚Â¡t `PaymentCompleted` tĂ¡ÂºÂ¡i `OrderController.cs:295-308`.
- `send-to-kitchen` Ă„â€˜Ă†Â°Ă¡Â»Â£c kiĂ¡Â»Æ’m tra branch trĂ†Â°Ă¡Â»â€ºc khi gĂ¡Â»Âi service (`OrderController.cs:245-255`). Logic chÄ‚Âªnh lĂ¡Â»â€¡ch quantity/SentQuantity nĂ¡ÂºÂ±m Ă¡Â»Å¸ `KitchenService.SendToKitchenAsync` vÄ‚Â  item update trong `OrderService`; cĂ¡ÂºÂ§n chĂ¡Â»Â¥p Ă„â€˜Ä‚Âºng Ă„â€˜oĂ¡ÂºÂ¡n code sau khi mĂ¡Â»Å¸ file trong IDE.
- Customer-web khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c tÄ‚Â¬m thĂ¡ÂºÂ¥y trong kĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ tÄ‚Â¬m kiĂ¡ÂºÂ¿m `HubConnectionBuilder`, `kitchenHub`, `PendingOrderChanged`; do Ă„â€˜Ä‚Â³ khÄ‚Â´ng cÄ‚Â³ bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng customer-web kĂ¡ÂºÂ¿t nĂ¡Â»â€˜i KDS.
- ChĂ†Â°a cÄ‚Â³ runtime capture chĂ¡Â»Â©ng minh connection, group membership, event delivery hoĂ¡ÂºÂ·c transaction commit xĂ¡ÂºÂ£y ra Ă„â€˜Ä‚Âºng thĂ¡Â»Â© tĂ¡Â»Â±.

### D.3. Thanh toÄ‚Â¡n vÄ‚Â  tÄ‚Â i chÄ‚Â­nh

- Backend lĂ¡ÂºÂ¥y giÄ‚Â¡ sĂ¡ÂºÂ£n phĂ¡ÂºÂ©m/topping/option tĂ¡Â»Â« DB trong `OrderService.RecalculateOrderFinancials` (`OrderService.cs:255-348`), khÄ‚Â´ng tin unit price client.
- CÄ‚Â´ng thĂ¡Â»Â©c code: `SubTotal`; service fee Ă„â€˜Ă†Â°Ă¡Â»Â£c tÄ‚Â­nh tĂ¡Â»Â« `SubTotal` vÄ‚Â  lÄ‚Â m trÄ‚Â²n; VAT tÄ‚Â­nh trÄ‚Âªn `SubTotal + ServiceFee`; `TotalAmount = taxableBase + vatAmount - Discount` (`OrderService.cs:348-363`).
- `RoundMoney` Ă¡Â»Å¸ `OrderService.cs:388` lÄ‚Â  Ă„â€˜iĂ¡Â»Æ’m cĂ¡ÂºÂ§n trÄ‚Â­ch hÄ‚Â¬nh Ă„â€˜Ă¡Â»Æ’ chĂ¡Â»Â©ng minh precision/rounding; khÄ‚Â´ng suy diĂ¡Â»â€¦n chi tiĂ¡ÂºÂ¿t ngoÄ‚Â i code Ă„â€˜Ä‚Â£ Ă„â€˜Ă¡Â»Âc.
- Payment controller yÄ‚Âªu cĂ¡ÂºÂ§u amount > 0, method thuĂ¡Â»â„¢c tiĂ¡Â»Ân mĂ¡ÂºÂ·t/chuyĂ¡Â»Æ’n khoĂ¡ÂºÂ£n, vÄ‚Â  `request.Amount == existingOrder.TotalAmount` (`OrderController.cs:279-295`). `PayOrderAsync` trĂ¡ÂºÂ£ `AlreadyPaid`/non-payable state; concurrency runtime chĂ†Â°a chĂ¡ÂºÂ¡y.
- CÄ‚Â³ execution strategy/transaction Ă¡Â»Å¸ `OrderService.PayOrderAsync` (xem `OrderService.cs` khoĂ¡ÂºÂ£ng 457 trĂ¡Â»Å¸ Ă„â€˜i), nhĂ†Â°ng chĂ†Â°a thĂ¡Â»Â±c hiĂ¡Â»â€¡n test HTTP Ă„â€˜Ă¡Â»â€œng thĂ¡Â»Âi Ă„â€˜Ă¡Â»Æ’ chĂ¡Â»Â©ng minh toÄ‚Â n bĂ¡Â»â„¢ race behavior.
- VietQR: audit khÄ‚Â´ng tÄ‚Â¬m thĂ¡ÂºÂ¥y bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng Ă„â€˜Ă¡Â»Â§ Ă„â€˜Ă¡Â»Æ’ khĂ¡ÂºÂ³ng Ă„â€˜Ă¡Â»â€¹nh Ă„â€˜Ä‚Â£ cÄ‚Â³ provider/callback/webhook. KhÄ‚Â´ng ghi Ă¢â‚¬Å“Ă„â€˜Ä‚Â£ tÄ‚Â­ch hĂ¡Â»Â£p webhookĂ¢â‚¬Â.

### D.4. Loyalty

- `EarnPointsAsync` yÄ‚Âªu cĂ¡ÂºÂ§u order cÄ‚Â³ customer vÄ‚Â  trĂ¡ÂºÂ¡ng thÄ‚Â¡i hoÄ‚Â n thÄ‚Â nh; kiĂ¡Â»Æ’m tra Ă„â€˜Ä‚Â£ cÄ‚Â³ transaction type `Earn` trĂ†Â°Ă¡Â»â€ºc khi cĂ¡Â»â„¢ng Ă„â€˜iĂ¡Â»Æ’m (`LoyaltyService.cs:23-84`).
- `RedeemPointsAsync` yÄ‚Âªu cĂ¡ÂºÂ§u order Ă„â€˜ang xĂ¡Â»Â­ lÄ‚Â½/chĂ†Â°a thanh toÄ‚Â¡n vÄ‚Â  kiĂ¡Â»Æ’m tra transaction Ă„â€˜Ä‚Â£ redeem, sĂ¡Â»â€˜ Ă„â€˜iĂ¡Â»Æ’m hĂ¡Â»Â£p lĂ¡Â»â€¡ vÄ‚Â  sĂ¡Â»â€˜ dĂ†Â° (`LoyaltyService.cs:93` trĂ¡Â»Å¸ Ă„â€˜i).
- Entity cho phÄ‚Â©p cÄ‚Â¡c type `Earn, Redeem, Adjustment, Refund, Reversal` (`LoyaltyTransaction.cs:10`), nhĂ†Â°ng sĂ¡Â»Â± tĂ¡Â»â€œn tĂ¡ÂºÂ¡i cĂ¡Â»Â§a enum/string type khÄ‚Â´ng tĂ¡Â»Â± chĂ¡Â»Â©ng minh mĂ¡Â»Âi luĂ¡Â»â€œng Refund/Reversal Ă„â€˜Ä‚Â£ hoÄ‚Â n thiĂ¡Â»â€¡n.
- CÄ‚Â³ transaction vÄ‚Â  idempotency checks; chĂ†Â°a cÄ‚Â³ manual test chĂ¡Â»Â©ng minh retry HTTP thĂ¡Â»Â±c tĂ¡ÂºÂ¿, hĂ¡Â»Â§y Ă„â€˜Ă†Â¡n, refund redemption vÄ‚Â  reversal earned points Ă¡Â»Å¸ mĂ¡Â»Âi trĂ¡ÂºÂ¡ng thÄ‚Â¡i.

### D.5. Auth

- DI dÄ‚Â¹ng `PasswordHasher<Employee>` vÄ‚Â  `PasswordHasher<Customer>` (`Program.cs:271-273`), tĂ¡Â»Â©c code path mĂ¡Â»â€ºi dÄ‚Â¹ng ASP.NET Core Identity `PasswordHasher<T>`.
- Login vĂ¡ÂºÂ«n gĂ¡Â»Âi `LegacyPasswordVerifier.Verify` rĂ¡Â»â€œi rehash password khi cĂ¡ÂºÂ§n (`AuthController.cs:129-169,288-306`); account inactive bĂ¡Â»â€¹ loĂ¡ÂºÂ¡i khĂ¡Â»Âi login.
- JWT validation truy vĂ¡ÂºÂ¥n DB Ă„â€˜Ă¡Â»Æ’ kiĂ¡Â»Æ’m tra employee/customer tĂ¡Â»â€œn tĂ¡ÂºÂ¡i vÄ‚Â  active (`Program.cs:131-186`), nÄ‚Âªn token cĂ…Â© cĂ¡Â»Â§a staff inactive/role/branch khÄ‚Â´ng hĂ¡Â»Â£p lĂ¡Â»â€¡ theo trĂ¡ÂºÂ¡ng thÄ‚Â¡i hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i.
- Password policy Ă„â€˜Ă†Â°Ă¡Â»Â£c gĂ¡Â»Âi khi Ă„â€˜Ă¡Â»â€¢i password (`AuthController.cs:388-412`); cĂ¡ÂºÂ§n dÄ‚Â¹ng `PasswordPolicy` source nĂ¡ÂºÂ¿u bÄ‚Â¡o cÄ‚Â¡o muĂ¡Â»â€˜n liĂ¡Â»â€¡t kÄ‚Âª chÄ‚Â­nh xÄ‚Â¡c cÄ‚Â¡c rule.
- Guest vÄ‚Â  registered customer Ă„â€˜Ă†Â°Ă¡Â»Â£c phÄ‚Â¢n biĂ¡Â»â€¡t bĂ¡Â»Å¸i `customerSessionType`/`customerAuthMethod` (`Program.cs:160-186`).
- CÄ‚Â¡c test auth Ă„â€˜Ä‚Â£ chĂ¡ÂºÂ¡y; chĂ†Â°a cÄ‚Â³ browser/manual matrix cho UI/API manager mismatch. KhÄ‚Â´ng ghi Ă¢â‚¬Å“UI vÄ‚Â  API Ă„â€˜Ă¡Â»â€œng bĂ¡Â»â„¢ 100%Ă¢â‚¬Â.

## E. AI Agent vÄ‚Â  bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng Ă„â€˜Ä‚Â¡nh giÄ‚Â¡

### E.1. LuĂ¡Â»â€œng runtime Ă„â€˜Ă†Â°Ă¡Â»Â£c xÄ‚Â¡c minh bĂ¡ÂºÂ±ng code

`AiController` nhĂ¡ÂºÂ­n chat cÄ‚Â³ `[Authorize]` vÄ‚Â  rate limit; `AiOrchestrator.ProcessAsync`:

1. KiĂ¡Â»Æ’m tra role/branch scope (`AiOrchestrator.cs:37-50`).
2. TĂ¡ÂºÂ¡o context vÄ‚Â  system instruction (`51-56`).
3. ChuyĂ¡Â»Æ’n history client thÄ‚Â nh user text khÄ‚Â´ng tin cĂ¡ÂºÂ­y (`58-76`).
4. GĂ¡Â»Â­i request tĂ¡Â»â€ºi AI provider vĂ¡Â»â€ºi tool definitions (`78-99`).
5. Ă„ÂĂ¡Â»Âc `functionCall`, resolve tÄ‚Âªn qua registry, kiĂ¡Â»Æ’m tra `AiPermissionService`, rĂ¡Â»â€œi gĂ¡Â»Âi `tool.ExecuteAsync` (`101-170`).
6. TrĂ¡ÂºÂ£ function response vĂ¡Â»Â model vÄ‚Â  lĂ¡ÂºÂ·p tĂ¡Â»â€˜i Ă„â€˜a `MAX_ITERATIONS = 5` (`180-207`).
7. DĂ¡Â»Â«ng tĂ¡ÂºÂ¡i confirmation boundary; exception Ă„â€˜Ă†Â°Ă¡Â»Â£c log server-side vÄ‚Â  trĂ¡ÂºÂ£ lĂ¡Â»â€”i tĂ¡Â»â€¢ng quÄ‚Â¡t (`209-216`).

`AI provider implementation.cs:21-52` Ă„â€˜Ă¡Â»Âc model tĂ¡Â»Â« `AI provider:Model`, chĂ¡Â»â€° dÄ‚Â¹ng default khi thiĂ¡ÂºÂ¿u cĂ¡ÂºÂ¥u hÄ‚Â¬nh lÄ‚Â  `MockAiProvider`; report phĂ¡ÂºÂ£i ghi Ă¢â‚¬Å“cĂ¡ÂºÂ¥u hÄ‚Â¬nh thĂ¡Â»Â±c tĂ¡ÂºÂ¿/giÄ‚Â¡ trĂ¡Â»â€¹ fallback nÄ‚Â yĂ¢â‚¬Â, khÄ‚Â´ng khĂ¡ÂºÂ³ng Ă„â€˜Ă¡Â»â€¹nh model production nĂ¡ÂºÂ¿u chĂ†Â°a Ă„â€˜Ă¡Â»Âc environment runtime. API key chĂ¡Â»â€° Ă„â€˜Ă†Â°Ă¡Â»Â£c lĂ¡ÂºÂ¥y tĂ¡Â»Â« configuration.

### E.2. Tool, quyĂ¡Â»Ân vÄ‚Â  truy cĂ¡ÂºÂ­p DB

- Registry lĂ¡Â»Âc tool theo `AllowedRoles` trĂ†Â°Ă¡Â»â€ºc khi gĂ¡Â»Â­i schema (`AiToolRegistry.cs:35-60`).
- Permission service kiĂ¡Â»Æ’m tra known role, branch bĂ¡ÂºÂ¯t buĂ¡Â»â„¢c vĂ¡Â»â€ºi staff scoped, role vÄ‚Â  risk level (`AiPermissionService.cs:22-50`).
- CÄ‚Â¡c tool Ă„â€˜Ă†Â°Ă¡Â»Â£c DI tĂ¡ÂºÂ¡i `Program.cs:289-308`; tool gĂ¡Â»Âi application service theo tĂ¡Â»Â«ng file tool. KhÄ‚Â´ng thĂ¡ÂºÂ¥y `AiOrchestrator` inject trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p `ApplicationDbContext`.
- CÄ‚Â³ tool write (`update_product_price`, `update_order_status`, `create_booking`); do Ă„â€˜Ä‚Â³ Ă„â€˜Ä‚Â¡nh giÄ‚Â¡ Ă¢â‚¬Å“AI chĂ¡Â»â€° read-onlyĂ¢â‚¬Â lÄ‚Â  sai.
- ChĂ†Â°a tÄ‚Â¬m thĂ¡ÂºÂ¥y dataset/script/phĂ†Â°Ă†Â¡ng phÄ‚Â¡p thĂ¡Â»Â±c nghiĂ¡Â»â€¡m chĂ¡Â»Â©ng minh cÄ‚Â¡c con sĂ¡Â»â€˜ **450 cases, Tool Selection Accuracy 92%, Security Compliance 100%, No-Tool Accuracy 98%**. CÄ‚Â¡c con sĂ¡Â»â€˜ nÄ‚Â y phĂ¡ÂºÂ£i ghi: **Ă¢â‚¬Å“ChĂ†Â°a cÄ‚Â³ bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng thĂ¡Â»Â±c nghiĂ¡Â»â€¡m; khÄ‚Â´ng dÄ‚Â¹ng nhĂ†Â° kĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ Ă„â€˜o.Ă¢â‚¬Â** Test xUnit AI khÄ‚Â´ng phĂ¡ÂºÂ£i dataset benchmark 450 mĂ¡ÂºÂ«u vÄ‚Â  khÄ‚Â´ng Ă„â€˜Ă¡Â»Â§ Ă„â€˜Ă¡Â»Æ’ suy ra cÄ‚Â¡c accuracy percentages.
- ChĂ†Â°a xÄ‚Â¡c minh cÄ‚Â³ lĂ†Â°u hĂ¡Â»â„¢i thoĂ¡ÂºÂ¡i bĂ¡Â»Ân vĂ¡Â»Â¯ng/audit log riÄ‚Âªng; `request.History` lÄ‚Â  dĂ¡Â»Â¯ liĂ¡Â»â€¡u client gĂ¡Â»Â­i vÄ‚Â o, khÄ‚Â´ng phĂ¡ÂºÂ£i bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng server lĂ†Â°u lĂ¡Â»â€¹ch sĂ¡Â»Â­.

## F. KĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ kiĂ¡Â»Æ’m thĂ¡Â»Â­ vÄ‚Â  build

### F.1. LĂ¡Â»â€¡nh Ă„â€˜Ä‚Â£ chĂ¡ÂºÂ¡y trong audit

ThĂ¡Â»Âi Ă„â€˜iĂ¡Â»Æ’m: 2026-10-07; working tree nhĂ†Â° mĂ¡Â»Â¥c A; khÄ‚Â´ng migrate/seed/API.

| LĂ¡Â»â€¡nh | KĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ |
|---|---|
| `dotnet test tests/RestaurantPOS.Tests/RestaurantPOS.Tests.csproj --no-restore --logger "console;verbosity=minimal"` | **Passed 375, Failed 0, Skipped 0, Total 375**, duration khoĂ¡ÂºÂ£ng 9 s. |
| `dotnet build services/api/RestaurantPOS.api.csproj --no-restore --nologo` | **Build succeeded, 0 warning, 0 error**. |
| `npm run build` tĂ¡ÂºÂ¡i `apps/admin-web` | **Build succeeded**; Vite 4.5.14; cÄ‚Â³ warning chunk JS minified khoĂ¡ÂºÂ£ng 1.3 MB > 500 kB. |
| `npm run build` tĂ¡ÂºÂ¡i `apps/customer-web` | **Build succeeded**; Vite 4.5.14. |

Test factory tĂ¡ÂºÂ¡o `SqliteConnection("Data Source=:memory:;Foreign Keys=False")`, mĂ¡Â»Å¸ connection vÄ‚Â  `EnsureCreated()` tĂ¡ÂºÂ¡i `tests/RestaurantPOS.Tests/TestDbContextFactory.cs:9-21`. VÄ‚Â¬ vĂ¡ÂºÂ­y test run trÄ‚Âªn DB cÄ‚Â´ lĂ¡ÂºÂ­p in-memory, khÄ‚Â´ng phĂ¡ÂºÂ£i PostgreSQL/Supabase. MĂ¡Â»â„¢t sĂ¡Â»â€˜ test riÄ‚Âªng cĂ…Â©ng dÄ‚Â¹ng SQLite in-memory.

### F.2. PhÄ‚Â¢n nhÄ‚Â³m test theo tÄ‚Âªn file

Ă„ÂĂ¡ÂºÂ¿m attribute `[Fact]`/`[Theory]` tĂ¡Â»Â« source cho thĂ¡ÂºÂ¥y cÄ‚Â¡c nhÄ‚Â³m chÄ‚Â­nh: order/financial, kitchen, guest QR, auth/JWT/password, branch/authorization, AI/security, loyalty/analytics/dashboard, reservation/table, attendance/employee, inventory. CÄ‚Â¡c nhÄ‚Â³m cÄ‚Â³ thĂ¡Â»Æ’ giao nhau; tĂ¡Â»â€¢ng thĂ¡Â»Â±c thi lÄ‚Â  375, khÄ‚Â´ng cĂ¡Â»â„¢ng cÄ‚Â¡c con sĂ¡Â»â€˜ nhÄ‚Â³m thÄ‚Â nh coverage.

KhÄ‚Â´ng cÄ‚Â³ frontend test runner/E2E framework Ă„â€˜Ă†Â°Ă¡Â»Â£c phÄ‚Â¡t hiĂ¡Â»â€¡n trong `apps`; khÄ‚Â´ng cÄ‚Â³ bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng test browser, camera QR, API live, SignalR live, load test hoĂ¡ÂºÂ·c UI coverage.

## G. BĂ¡ÂºÂ£ng test case phĂ¡Â»Â¥c vĂ¡Â»Â¥ khÄ‚Â³a luĂ¡ÂºÂ­n

KÄ‚Â½ hiĂ¡Â»â€¡u trĂ¡ÂºÂ¡ng thÄ‚Â¡i: **PASS (executed)** chĂ¡Â»â€° dÄ‚Â¹ng cho test Ă„â€˜Ä‚Â£ chĂ¡ÂºÂ¡y; **INSPECTED** chĂ¡Â»â€° Ă„â€˜Ă¡Â»â€˜i chiĂ¡ÂºÂ¿u mÄ‚Â£ nguĂ¡Â»â€œn; **NOT RUN** chĂ†Â°a chĂ¡ÂºÂ¡y; **PROPOSED** lÄ‚Â  kĂ¡Â»â€¹ch bĂ¡ÂºÂ£n Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t.

| ID | ChĂ¡Â»Â©c nĂ„Æ’ng | Ă„ÂiĂ¡Â»Âu kiĂ¡Â»â€¡n trĂ†Â°Ă¡Â»â€ºc | DĂ¡Â»Â¯ liĂ¡Â»â€¡u Ă„â€˜Ă¡ÂºÂ§u vÄ‚Â o | BĂ†Â°Ă¡Â»â€ºc thĂ¡Â»Â±c hiĂ¡Â»â€¡n | KĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ mong Ă„â€˜Ă¡Â»Â£i | KĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ thĂ¡Â»Â±c tĂ¡ÂºÂ¿ | PhĂ†Â°Ă†Â¡ng phÄ‚Â¡p | TrĂ¡ÂºÂ¡ng thÄ‚Â¡i | Minh chĂ¡Â»Â©ng |
|---|---|---|---|---|---|---|---|---|---|
| T01 | Backend build | SDK/dependency local | API csproj | ChĂ¡ÂºÂ¡y dotnet build | KhÄ‚Â´ng lĂ¡Â»â€”i | 0 warning/0 error | Automated build | PASS | command output; `RestaurantPOS.api.csproj` |
| T02 | Unit/integration suite | SQLite in-memory | Fixtures test | ChĂ¡ÂºÂ¡y dotnet test | Test chĂ¡ÂºÂ¡y khÄ‚Â´ng chĂ¡ÂºÂ¡m DB thĂ¡ÂºÂ­t | 375 passed, 0 failed, 0 skipped | xUnit/SQLite | PASS | `TestDbContextFactory.cs:9-21`; test output |
| T03 | Order financial snapshots | SQLite fixture | VAT/service fee/product prices | ChĂ¡ÂºÂ¡y test order financial | Backend tÄ‚Â­nh lĂ¡ÂºÂ¡i tĂ¡Â»Â« DB, snapshot Ă„â€˜Ă†Â°Ă¡Â»Â£c lĂ†Â°u | Covered by executed order financial tests; khÄ‚Â´ng tÄ‚Â¡ch thÄ‚Â nh runtime API | Automated | PASS Ă¡Â»Å¸ test suite; khÄ‚Â´ng phĂ¡ÂºÂ£i E2E | `OrderFinancialCalculationTests.cs`, `OrderService.cs:255-363` |
| T04 | Client manipulated total | Order fixture | amount khÄ‚Â¡c `Order.TotalAmount` | ChĂ¡ÂºÂ¡y security/payment test nĂ¡ÂºÂ¿u fixture | BĂ¡Â»â€¹ tĂ¡Â»Â« chĂ¡Â»â€˜i | Code validation Ă„â€˜Ä‚Â£ inspected; khÄ‚Â´ng cÄ‚Â³ HTTP manual run riÄ‚Âªng trong audit | Source + suite | INSPECTED | `OrderController.cs:279-295` |
| T05 | Branch isolation manager | Hai branch, manager claim | resource khÄ‚Â¡c branch | ChĂ¡ÂºÂ¡y hardening tests | Forbid/khÄ‚Â´ng trĂ¡ÂºÂ£ dĂ¡Â»Â¯ liĂ¡Â»â€¡u khÄ‚Â¡c branch | Covered by branch/auth tests in suite | xUnit/SQLite | PASS Ă¡Â»Å¸ test suite | `ManagerBranchIsolationHardeningTests.cs`, `FullBranchIsolationTests.cs` |
| T06 | Guest QR session | Table active/token | bootstrap + order | GĂ¡Â»Âi browser/API live | Guest bĂ¡Â»â€¹ giĂ¡Â»â€ºi hĂ¡ÂºÂ¡n table/branch | ChĂ†Â°a chĂ¡ÂºÂ¡y API/browser | Proposed manual | NOT RUN | `GuestController.cs`, `QrTokenFoundationTests.cs` |
| T07 | Pending order acceptance | Pending guest order | `POST /api/Order/{id}/accept` | Staff accept cÄ‚Â¹ng lÄ‚Âºc | MĂ¡Â»â„¢t request accept thÄ‚Â nh cÄ‚Â´ng, request kia conflict | CÄ‚Â³ conditional update/transaction trong code; chĂ†Â°a race HTTP | Source + proposed concurrency | INSPECTED | `OrderService.cs:391-453` |
| T08 | KDS send quantity delta | Order cÄ‚Â³ Quantity/SentQuantity | send-to-kitchen | GĂ¡Â»Âi endpoint + mĂ¡Â»Å¸ KDS | ChĂ¡Â»â€° gĂ¡Â»Â­i phĂ¡ÂºÂ§n mĂ¡Â»â€ºi; event Ă„â€˜Ă¡ÂºÂ¿n Ă„â€˜Ä‚Âºng branch | Test service Ă„â€˜Ä‚Â£ nĂ¡ÂºÂ±m trong 375; SignalR runtime chĂ†Â°a chĂ¡ÂºÂ¡y | xUnit + source | PASS service; NOT RUN SignalR | `KitchenServiceTests.cs`, `KitchenHub.cs`, `Program.cs:439` |
| T09 | Payment duplicate/concurrent | Payable order | Hai payment request | GĂ¡Â»Â­i Ă„â€˜Ă¡Â»â€œng thĂ¡Â»Âi | MĂ¡Â»â„¢t Paid, request kia AlreadyPaid/conflict | ChĂ†Â°a chĂ¡ÂºÂ¡y HTTP race | Proposed manual/concurrency | NOT RUN | `OrderService.PayOrderAsync`, `OrderController.cs:279-315` |
| T10 | Loyalty earn idempotency | Completed customer order | GĂ¡Â»Âi earn nhiĂ¡Â»Âu lĂ¡ÂºÂ§n | Retry same request | KhÄ‚Â´ng cĂ¡Â»â„¢ng lĂ¡ÂºÂ·p | Logic check vÄ‚Â  transaction Ă„â€˜Ä‚Â£ inspected; runtime retry chĂ†Â°a chĂ¡ÂºÂ¡y | Source + tests | INSPECTED | `LoyaltyService.cs:23-84` |
| T11 | AI tool permission deny | JWT role khÄ‚Â´ng nĂ¡ÂºÂ±m AllowedRoles | Function call write/read trÄ‚Â¡i quyĂ¡Â»Ân | GĂ¡Â»Â­i chat | Permission service tĂ¡Â»Â« chĂ¡Â»â€˜i | Unit/security tests suite passed; AI provider live chĂ†Â°a chĂ¡ÂºÂ¡y | xUnit/source | PASS test; NOT RUN live | `AiPermissionService.cs:22-50`, `AiOrchestrator.cs:156-170` |
| T12 | AI confirmation | Admin + price write | update price | GĂ¡Â»Â­i tool call rĂ¡Â»â€œi xÄ‚Â¡c nhĂ¡ÂºÂ­n | KhÄ‚Â´ng self-confirm trong cÄ‚Â¹ng turn | Confirmation boundary Ă„â€˜Ä‚Â£ inspected; chĂ†Â°a gĂ¡Â»Âi AI provider | Source/manual proposed | INSPECTED | `AiOrchestrator.cs:180-189`, `AiController.cs` |
| T13 | Admin frontend build | Node/npm deps | admin source | npm run build | TSC/Vite thÄ‚Â nh cÄ‚Â´ng | ThÄ‚Â nh cÄ‚Â´ng; chunk warning >500 kB | Build | PASS | build output |
| T14 | Customer frontend build | Node/npm deps | customer source | npm run build | TSC/Vite thÄ‚Â nh cÄ‚Â´ng | ThÄ‚Â nh cÄ‚Â´ng | Build | PASS | build output |
| T15 | HTTPS/LAN/QR camera | Certificate, isolated DB, LAN | browser/mobile | Start API + Vite + camera | Runtime HTTPS/LAN/QR flow | ChĂ†Â°a chĂ¡ÂºÂ¡y vÄ‚Â¬ audit khÄ‚Â´ng khĂ¡Â»Å¸i Ă„â€˜Ă¡Â»â„¢ng API | Proposed E2E | NOT RUN | `Program.cs:371-388`, `QRScan.tsx` |

## H. Danh sÄ‚Â¡ch hÄ‚Â¬nh/minh chĂ¡Â»Â©ng cĂ¡ÂºÂ§n chĂ¡Â»Â¥p

| TÄ‚Âªn hÄ‚Â¬nh Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t | File/hÄ‚Â m/mÄ‚Â n hÄ‚Â¬nh | Thao tÄ‚Â¡c lĂ¡ÂºÂ¥y hÄ‚Â¬nh | NĂ¡Â»â„¢i dung chĂ¡Â»Â©ng minh |
|---|---|---|---|
| H4.1 CĂ¡ÂºÂ¥u trÄ‚Âºc solution thĂ¡Â»Â±c tĂ¡ÂºÂ¿ | Explorer/terminal; `apps`, `packages`, `services`, `tests` | MĂ¡Â»Å¸ repository tree, Ă¡ÂºÂ©n `node_modules/bin/obj` | PhÄ‚Â¢n biĂ¡Â»â€¡t thĂ†Â° mĂ¡Â»Â¥c vĂ¡Â»â€ºi `.csproj`, vĂ¡Â»â€¹ trÄ‚Â­ cÄ‚Â¡c layer vÄ‚Â  hai frontend |
| H4.2 Pipeline khĂ¡Â»Å¸i Ă„â€˜Ă¡Â»â„¢ng an toÄ‚Â n | `Program.cs:392-417` | MĂ¡Â»Å¸ code tĂ¡ÂºÂ¡i vÄ‚Â¹ng `AllowAutomaticMigration` | Migration/seed chĂ¡Â»â€° chĂ¡ÂºÂ¡y khi Development opt-in |
| H4.3 Order financial authority | `OrderService.cs:255-363` | ChĂ¡Â»Â¥p hÄ‚Â m `RecalculateOrderFinancials` | GiÄ‚Â¡ lĂ¡ÂºÂ¥y tĂ¡Â»Â« DB, snapshot, service fee/VAT/total |
| H4.4 Payment server validation | `OrderController.cs:279-315` | ChĂ¡Â»Â¥p endpoint payment | amount phĂ¡ÂºÂ£i bĂ¡ÂºÂ±ng persisted `TotalAmount`, method/state checks |
| H4.5 QR accept gate | `OrderController.cs:206-238`, `OrderService.cs:391-453` | ChĂ¡Â»Â¥p controller + service accept | Guest pending order cĂ¡ÂºÂ§n nhÄ‚Â¢n viÄ‚Âªn accept vÄ‚Â  branch/concurrency checks |
| H4.6 KDS/SignalR endpoint | `Program.cs:106-117,439`; `KitchenHub.cs:10` | ChĂ¡Â»Â¥p map hub + authorization | endpoint, token path vÄ‚Â  role boundary |
| H4.7 AI orchestration | `AiOrchestrator.cs:86-207` | ChĂ¡Â»Â¥p function call/permission/loop | flow AI provider Ă¢â€ â€™ tool Ă¢â€ â€™ permission Ă¢â€ â€™ result, max 5 iterations |
| H4.8 AI permission refusal | `AiPermissionService.cs:22-50`, ChatBot UI | DÄ‚Â¹ng test fixture hoĂ¡ÂºÂ·c mÄ‚Â´i trĂ†Â°Ă¡Â»Âng isolated Ă„â€˜Ă¡Â»Æ’ yÄ‚Âªu cĂ¡ÂºÂ§u trÄ‚Â¡i quyĂ¡Â»Ân; chĂ¡Â»Â¥p response thĂ¡ÂºÂ­t | TĂ¡Â»Â« chĂ¡Â»â€˜i do role/risk/branch; **khÄ‚Â´ng tĂ¡ÂºÂ¡o Ă¡ÂºÂ£nh giĂ¡ÂºÂ£** |
| H4.9 Test output | terminal output cĂ¡Â»Â§a `dotnet test` | ChĂ¡ÂºÂ¡y Ă„â€˜Ä‚Âºng command audit hoĂ¡ÂºÂ·c mĂ¡Â»Å¸ log Ă„â€˜Ä‚Â£ lĂ†Â°u | 375 passed/0 failed/0 skipped |
| H4.10 Frontend build | terminal admin/customer | ChĂ¡ÂºÂ¡y `npm run build` trong tĂ¡Â»Â«ng app | TSC/Vite build; ghi warning chunk admin nĂ¡ÂºÂ¿u dÄ‚Â¹ng |
| H4.11 UI QR/POS/KDS | `POSPage.tsx`, `DigitalMenu.tsx`, `KitchenPage.tsx` | ChĂ¡Â»â€° chĂ¡Â»Â¥p sau manual runtime test | Ă¡ÂºÂ¢nh giao diĂ¡Â»â€¡n chĂ¡Â»â€° Ă„â€˜Ă†Â°Ă¡Â»Â£c dÄ‚Â¹ng nĂ¡ÂºÂ¿u flow Ă„â€˜Ä‚Â£ chĂ¡ÂºÂ¡y thĂ¡ÂºÂ­t |

KhÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c chĂ¡Â»Â¥p hoĂ¡ÂºÂ·c Ă„â€˜Ă†Â°a vÄ‚Â o luĂ¡ÂºÂ­n vĂ„Æ’n Ă¡ÂºÂ£nh/log chĂ†Â°a thĂ¡Â»Â±c sĂ¡Â»Â± tĂ¡ÂºÂ¡o tĂ¡Â»Â« mÄ‚Â´i trĂ†Â°Ă¡Â»Âng audit.

## I. HĂ¡ÂºÂ¡n chĂ¡ÂºÂ¿ Ă„â€˜Ä‚Â£ xÄ‚Â¡c minh

1. Working tree khÄ‚Â´ng sĂ¡ÂºÂ¡ch; kĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ khÄ‚Â´ng Ă„â€˜Ă¡ÂºÂ¡i diĂ¡Â»â€¡n duy nhĂ¡ÂºÂ¥t cho commit `HEAD`.
2. ChĂ†Â°a cÄ‚Â³ bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng E2E/browser/frontend automated tests.
3. ChĂ†Â°a cÄ‚Â³ runtime verification cĂ¡Â»Â§a HTTPS/LAN, camera QR, SignalR connection/group/event, payment thĂ¡ÂºÂ­t hoĂ¡ÂºÂ·c AI provider live.
4. ChĂ†Â°a cÄ‚Â³ load test; khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c Ă„â€˜Ă†Â°a TPS, P95/P99, concurrent users hoĂ¡ÂºÂ·c Ă¢â‚¬Å“chĂ¡Â»â€¹u tĂ¡ÂºÂ£i tĂ¡Â»â€˜tĂ¢â‚¬Â.
5. Database production/integration chĂ†Â°a Ă„â€˜Ă†Â°Ă¡Â»Â£c kiĂ¡Â»Æ’m tra. Migration vÄ‚Â  seed chĂ†Â°a Ă„â€˜Ă†Â°Ă¡Â»Â£c apply trong audit; tÄ‚Â¬nh trĂ¡ÂºÂ¡ng schema Supabase thĂ¡Â»Â±c tĂ¡ÂºÂ¿ chĂ†Â°a xÄ‚Â¡c minh.
6. CORS cÄ‚Â³ danh sÄ‚Â¡ch origin cĂ¡ÂºÂ¥u hÄ‚Â¬nh nhĂ†Â°ng LAN hostname/IP runtime chĂ†Â°a Ă„â€˜Ă†Â°Ă¡Â»Â£c xÄ‚Â¡c minh; build khÄ‚Â´ng chĂ¡Â»Â©ng minh browser CORS/SignalR.
7. `packages/shared` cÄ‚Â³ codegen nhĂ†Â°ng khÄ‚Â´ng tÄ‚Â¬m thĂ¡ÂºÂ¥y frontend import; cĂ¡ÂºÂ§n trÄ‚Â¡nh tuyÄ‚Âªn bĂ¡Â»â€˜ shared types Ă„â€˜ang lÄ‚Â  nguĂ¡Â»â€œn type runtime.
8. Password path hĂ¡Â»â€” trĂ¡Â»Â£ ASP.NET PasswordHasher vÄ‚Â  legacy verifier, nhĂ†Â°ng chĂ†Â°a cÄ‚Â³ migration/runtime inventory Ă„â€˜Ă¡Â»Æ’ chĂ¡Â»Â©ng minh mĂ¡Â»Âi mĂ¡ÂºÂ­t khĂ¡ÂºÂ©u Ă„â€˜Ä‚Â£ Ă„â€˜Ă†Â°Ă¡Â»Â£c chuyĂ¡Â»Æ’n Ă„â€˜Ă¡Â»â€¢i.
9. Loyalty cÄ‚Â³ type `Refund`/`Reversal` trong model nhĂ†Â°ng audit chĂ†Â°a chĂ¡Â»Â©ng minh mĂ¡Â»â€”i nghiĂ¡Â»â€¡p vĂ¡Â»Â¥ Ă„â€˜Ä‚Â£ cÄ‚Â³ endpoint/flow hoÄ‚Â n chĂ¡Â»â€°nh.
10. VietQR/callback/webhook chĂ†Â°a cÄ‚Â³ bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng Ă„â€˜Ă¡Â»Â§ Ă„â€˜Ă¡Â»Æ’ khĂ¡ÂºÂ³ng Ă„â€˜Ă¡Â»â€¹nh Ă„â€˜Ä‚Â£ triĂ¡Â»Æ’n khai.
11. removed staff-finance module bĂ¡Â»â€¹ loĂ¡ÂºÂ¡i khĂ¡Â»Âi live domain theo migration `Removeremoved staff-finance moduleAndRepairReceiptSettings`; khÄ‚Â´ng ghi removed staff-finance module lÄ‚Â  module Ă„â€˜Ä‚Â£ triĂ¡Â»Æ’n khai hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i. Inventory cÄ‚Â³ entity/service/UI, nhĂ†Â°ng audit nÄ‚Â y khÄ‚Â´ng chĂ¡ÂºÂ¡y inventory E2E end-to-end.
12. Admin bundle cÄ‚Â³ cĂ¡ÂºÂ£nh bÄ‚Â¡o chunk lĂ¡Â»â€ºn sau minification; Ă„â€˜Ä‚Â¢y lÄ‚Â  warning build, chĂ†Â°a phĂ¡ÂºÂ£i sĂ¡Â»â€˜ Ă„â€˜o hiĂ¡Â»â€¡u nĂ„Æ’ng runtime.
13. KhÄ‚Â´ng cÄ‚Â³ bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng thĂ¡Â»Â±c nghiĂ¡Â»â€¡m cho 450 cases vÄ‚Â  ba tĂ¡Â»Â· lĂ¡Â»â€¡ AI nÄ‚Âªu trong yÄ‚Âªu cĂ¡ÂºÂ§u; khÄ‚Â´ng dÄ‚Â¹ng cÄ‚Â¡c sĂ¡Â»â€˜ Ă„â€˜Ä‚Â³ lÄ‚Â m kĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ Ă„â€˜o.

## J. ThÄ‚Â´ng tin cÄ‚Â²n thiĂ¡ÂºÂ¿u

- Database schema/version thĂ¡Â»Â±c tĂ¡ÂºÂ¿ cĂ¡Â»Â§a mÄ‚Â´i trĂ†Â°Ă¡Â»Âng demo/production vÄ‚Â  lĂ¡Â»â€¹ch sĂ¡Â»Â­ migrations Ă„â€˜Ä‚Â£ apply.
- Environment name, certificate availability, LAN hostname/IP vÄ‚Â  origin thĂ¡Â»Â±c tĂ¡ÂºÂ¿ dÄ‚Â¹ng khi demo.
- Browser/device matrix cho QR camera, customer ordering, POS, KDS vÄ‚Â  responsive UI.
- Evidence log cĂ¡Â»Â§a SignalR: connection, group membership, event name, sender/receiver vÄ‚Â  branch filtering.
- Evidence runtime thanh toÄ‚Â¡n: VietQR provider, amount source, callback/webhook hoĂ¡ÂºÂ·c quy trÄ‚Â¬nh manual confirmation.
- Dataset, script, seed, rubric, numerator/denominator vÄ‚Â  raw output cĂ¡Â»Â§a AI benchmark.
- Test coverage report vÄ‚Â  phÄ‚Â¢n loĂ¡ÂºÂ¡i chÄ‚Â­nh xÄ‚Â¡c tĂ¡Â»Â«ng test theo nghiĂ¡Â»â€¡p vĂ¡Â»Â¥; 375 test khÄ‚Â´ng Ă„â€˜Ă¡Â»â€œng nghĂ„Â©a 375 E2E cases hay UI coverage.
- TÄ‚Â i liĂ¡Â»â€¡u/version cĂ¡Â»Â§a cÄ‚Â¡c thay Ă„â€˜Ă¡Â»â€¢i chĂ†Â°a commit Ă„â€˜Ă¡Â»Æ’ quyĂ¡ÂºÂ¿t Ă„â€˜Ă¡Â»â€¹nh phĂ¡ÂºÂ§n nÄ‚Â o thuĂ¡Â»â„¢c phiÄ‚Âªn bĂ¡ÂºÂ£n chÄ‚Â­nh thĂ¡Â»Â©c cĂ¡Â»Â§a luĂ¡ÂºÂ­n vĂ„Æ’n.

## K. NhĂ¡Â»Â¯ng phÄ‚Â¡t biĂ¡Â»Æ’u trong ChĂ†Â°Ă†Â¡ng 4 cĂ¡ÂºÂ§n sĂ¡Â»Â­a hoĂ¡ÂºÂ·c bĂ¡Â»Â

| PhÄ‚Â¡t biĂ¡Â»Æ’u cĂ¡ÂºÂ§n xĂ¡Â»Â­ lÄ‚Â½ | Ă„ÂĂ¡Â»Â nghĂ¡Â»â€¹ viĂ¡ÂºÂ¿t lĂ¡ÂºÂ¡i |
|---|---|
| Ă¢â‚¬Å“HĂ¡Â»â€¡ thĂ¡Â»â€˜ng Ă„â€˜Ä‚Â£ kiĂ¡Â»Æ’m thĂ¡Â»Â­ 450 trĂ†Â°Ă¡Â»Âng hĂ¡Â»Â£p, Tool Selection Accuracy 92%, Security Compliance 100%, No-Tool Accuracy 98%.Ă¢â‚¬Â | Ă¢â‚¬Å“ChĂ†Â°a cÄ‚Â³ bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng thĂ¡Â»Â±c nghiĂ¡Â»â€¡m/dataset/script/raw output trong repository; khÄ‚Â´ng sĂ¡Â»Â­ dĂ¡Â»Â¥ng cÄ‚Â¡c con sĂ¡Â»â€˜ nÄ‚Â y nhĂ†Â° kĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ Ă„â€˜o.Ă¢â‚¬Â |
| Ă¢â‚¬Å“Ă„ÂÄ‚Â£ kiĂ¡Â»Æ’m thĂ¡Â»Â­ E2E toÄ‚Â n hĂ¡Â»â€¡ thĂ¡Â»â€˜ng.Ă¢â‚¬Â | BĂ¡Â»Â hoĂ¡ÂºÂ·c Ă„â€˜Ă¡Â»â€¢i thÄ‚Â nh: Ă¢â‚¬Å“Ă„ÂÄ‚Â£ chĂ¡ÂºÂ¡y 375 test backend trÄ‚Âªn SQLite in-memory; E2E/browser/SignalR runtime chĂ†Â°a Ă„â€˜Ă†Â°Ă¡Â»Â£c thĂ¡Â»Â±c hiĂ¡Â»â€¡n trong audit.Ă¢â‚¬Â |
| Ă¢â‚¬Å“API tĂ¡Â»Â± Ă„â€˜Ă¡Â»â„¢ng migration vÄ‚Â  seed khi khĂ¡Â»Å¸i Ă„â€˜Ă¡Â»â„¢ng.Ă¢â‚¬Â | Ă„ÂĂ¡Â»â€¢i thÄ‚Â nh: Ă¢â‚¬Å“Migration/seed chĂ¡Â»â€° chĂ¡ÂºÂ¡y khi Development vÄ‚Â  `DatabaseSafety:AllowAutomaticMigration=true`; mĂ¡ÂºÂ·c Ă„â€˜Ă¡Â»â€¹nh bĂ¡Â»â€¹ tĂ¡ÂºÂ¯t.Ă¢â‚¬Â |
| Ă¢â‚¬Å“Customer-web kĂ¡ÂºÂ¿t nĂ¡Â»â€˜i KDS.Ă¢â‚¬Â | BĂ¡Â»Â: khÄ‚Â´ng tÄ‚Â¬m thĂ¡ÂºÂ¥y `HubConnectionBuilder`/`kitchenHub` trong customer-web. KDS Ă„â€˜Ă†Â°Ă¡Â»Â£c xÄ‚Â¡c minh trong admin-web. |
| Ă¢â‚¬Å“Thanh toÄ‚Â¡n VietQR cÄ‚Â³ callback/webhook tĂ¡Â»Â± Ă„â€˜Ă¡Â»â„¢ng.Ă¢â‚¬Â | BĂ¡Â»Â nĂ¡ÂºÂ¿u chĂ†Â°a cung cĂ¡ÂºÂ¥p file/provider/runtime log chĂ¡Â»Â©ng minh; code audit chĂ†Â°a Ă„â€˜Ă¡Â»Â§ bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng. |
| Ă¢â‚¬Å“Loyalty Ă„â€˜Ä‚Â£ hoÄ‚Â n thiĂ¡Â»â€¡n Earn, Redeem, Refund, Reversal.Ă¢â‚¬Â | TÄ‚Â¡ch theo bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng: Earn/Redeem/idempotency cÄ‚Â³ code; sĂ¡Â»Â± tĂ¡Â»â€œn tĂ¡ÂºÂ¡i Ă„â€˜Ă¡ÂºÂ§y Ă„â€˜Ă¡Â»Â§ cĂ¡Â»Â§a Refund/Reversal chĂ†Â°a Ă„â€˜Ă†Â°Ă¡Â»Â£c xÄ‚Â¡c minh. |
| Ă¢â‚¬Å“removed staff-finance module Ă„â€˜Ä‚Â£ triĂ¡Â»Æ’n khai.Ă¢â‚¬Â | BĂ¡Â»Â khĂ¡Â»Âi mÄ‚Â´ tĂ¡ÂºÂ£ chĂ¡Â»Â©c nĂ„Æ’ng hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i; migration `20260919075618_Removeremoved staff-finance moduleAndRepairReceiptSettings` cho thĂ¡ÂºÂ¥y bĂ¡ÂºÂ£ng removed staff-finance module bĂ¡Â»â€¹ loĂ¡ÂºÂ¡i bĂ¡Â»Â khĂ¡Â»Âi model migration sau Ă„â€˜Ä‚Â³. |
| Ă¢â‚¬Å“Inventory end-to-end Ă„â€˜Ä‚Â£ hoÄ‚Â n thiĂ¡Â»â€¡n.Ă¢â‚¬Â | ChĂ¡Â»â€° ghi entity/service/controller/UI tĂ¡Â»â€œn tĂ¡ÂºÂ¡i; E2E inventory chĂ†Â°a chĂ¡ÂºÂ¡y trong audit. |
| Ă¢â‚¬Å“HĂ¡Â»â€¡ thĂ¡Â»â€˜ng phĂ¡ÂºÂ£n hĂ¡Â»â€œi nhanh/chĂ¡Â»â€¹u tĂ¡ÂºÂ£i tĂ¡Â»â€˜t.Ă¢â‚¬Â | BĂ¡Â»Â nĂ¡ÂºÂ¿u khÄ‚Â´ng cÄ‚Â³ load-test metrics; chĂ¡Â»â€° ghi build/test kĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ Ă„â€˜Ä‚Â£ Ă„â€˜o Ă„â€˜Ă†Â°Ă¡Â»Â£c. |
| Ă¢â‚¬Å“Shared package Ă„â€˜Ă†Â°Ă¡Â»Â£c hai frontend dÄ‚Â¹ng chung.Ă¢â‚¬Â | Ă„ÂĂ¡Â»â€¢i thÄ‚Â nh: Ă¢â‚¬Å“`packages/shared` tĂ¡Â»â€œn tĂ¡ÂºÂ¡i vÄ‚Â  cÄ‚Â³ codegen/types; audit chĂ†Â°a tÄ‚Â¬m thĂ¡ÂºÂ¥y import trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p tĂ¡Â»Â« hai frontend.Ă¢â‚¬Â |
| Ă¢â‚¬Å“MĂ¡ÂºÂ­t khĂ¡ÂºÂ©u dÄ‚Â¹ng mĂ¡Â»â„¢t thuĂ¡ÂºÂ­t toÄ‚Â¡n duy nhĂ¡ÂºÂ¥t.Ă¢â‚¬Â | Ă„ÂĂ¡Â»â€¢i thÄ‚Â nh: Ă¢â‚¬Å“DI dÄ‚Â¹ng ASP.NET Core `PasswordHasher<T>`, Ă„â€˜Ă¡Â»â€œng thĂ¡Â»Âi login/change-password cÄ‚Â²n xĂ¡Â»Â­ lÄ‚Â½ legacy verifier vÄ‚Â  rehash.Ă¢â‚¬Â |

## KĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n audit

PhiÄ‚Âªn bĂ¡ÂºÂ£n working tree **build thÄ‚Â nh cÄ‚Â´ng** vÄ‚Â  bĂ¡Â»â„¢ test cÄ‚Â´ lĂ¡ÂºÂ­p Ă„â€˜Ä‚Â£ chĂ¡ÂºÂ¡y **375/375 PASS**, vĂ¡Â»â€ºi bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng rÄ‚Âµ nhĂ¡ÂºÂ¥t Ă¡Â»Å¸ order/financial, auth/security, branch isolation, AI orchestration/tool permission, kitchen service, reservation/table, attendance vÄ‚Â  inventory service tests. Tuy nhiÄ‚Âªn, cÄ‚Â¡c kĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n vĂ¡Â»Â runtime E2E, browser/UI coverage, SignalR delivery, PostgreSQL/Supabase schema, VietQR callback, tĂ¡ÂºÂ£i hĂ¡Â»â€¡ thĂ¡Â»â€˜ng vÄ‚Â  AI accuracy chĂ†Â°a Ă„â€˜Ă†Â°Ă¡Â»Â£c chĂ¡Â»Â©ng minh trong lĂ¡ÂºÂ§n audit nÄ‚Â y. ChĂ†Â°Ă†Â¡ng 4 nÄ‚Âªn dÄ‚Â¹ng cÄ‚Â¡c kĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n cÄ‚Â³ `file:line` Ă¡Â»Å¸ bÄ‚Â¡o cÄ‚Â¡o nÄ‚Â y vÄ‚Â  Ă„â€˜Ä‚Â¡nh dĂ¡ÂºÂ¥u phĂ¡ÂºÂ§n cÄ‚Â²n lĂ¡ÂºÂ¡i lÄ‚Â  chĂ†Â°a kiĂ¡Â»Æ’m chĂ¡Â»Â©ng/Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t kiĂ¡Â»Æ’m thĂ¡Â»Â­.





