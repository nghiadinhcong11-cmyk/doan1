# CHAPTER 4 â€” INTEGRATION AND E2E VERIFICATION

## 1. Má»¥c tiĂªu vĂ  nguyĂªn táº¯c

TĂ i liá»‡u nĂ y ghi nháº­n láº§n kiá»ƒm thá»­ tĂ­ch há»£p/E2E ngĂ y 2026-10-07. KhĂ´ng sá»­a mĂ£ nguá»“n nghiá»‡p vá»¥, khĂ´ng reset/stash/ghi Ä‘Ă¨ thay Ä‘á»•i cĂ³ sáºµn vĂ  khĂ´ng truy cáº­p DB váº­n hĂ nh. CĂ¡c nhĂ£n Ä‘Æ°á»£c dĂ¹ng:

- **PASS**: Ä‘Ă£ thá»±c thi vĂ  cĂ³ output/log tÆ°Æ¡ng á»©ng.
- **FAIL**: Ä‘Ă£ thá»±c thi vĂ  quan sĂ¡t lá»—i thuá»™c á»©ng dá»¥ng/test.
- **BLOCKED**: khĂ´ng thá»ƒ thá»±c thi vĂ¬ thiáº¿u háº¡ táº§ng/cáº¥u hĂ¬nh, khĂ´ng pháº£i PASS.
- **NOT RUN**: chÆ°a cháº¡y trong láº§n nĂ y.
- **INSPECTED**: chá»‰ Ä‘á»‘i chiáº¿u code, khĂ´ng pháº£i káº¿t quáº£ runtime.

## 2. PhiĂªn báº£n vĂ  mĂ´i trÆ°á»ng

| Má»¥c | GiĂ¡ trá»‹ |
|---|---|
| Thá»i Ä‘iá»ƒm probe | 2026-10-07 15:13:47 (+07:00) |
| HEAD | `0a480965f5a582c6da5c30fb70d9a5a2d1cd3d4e` |
| Branch | `main` tracking `origin/main` |
| Working tree | Dirty trÆ°á»›c khi báº¯t Ä‘áº§u; cĂ³ nhiá»u thay Ä‘á»•i tracked/untracked. KhĂ´ng reset/stash. |
| API | ChÆ°a khá»Ÿi cháº¡y |
| admin-web/customer-web dev server | ChÆ°a khá»Ÿi cháº¡y |
| Database | Test factory dĂ¹ng SQLite in-memory; PostgreSQL/Supabase runtime chÆ°a dĂ¹ng |
| Automatic migration | `DatabaseSafety:AllowAutomaticMigration=false`; API chá»‰ migrate/seed trong Development khi explicit opt-in (`services/api/Program.cs:392-417`) |
| Cáº¥u hĂ¬nh DB | `appsettings.json` khĂ´ng cĂ³ connection string sá»­ dá»¥ng Ä‘Æ°á»£c; khĂ´ng in giĂ¡ trá»‹ bĂ­ máº­t |
| AI provider live | KhĂ´ng cĂ³ API key cáº¥u hĂ¬nh; khĂ´ng thá»±c hiá»‡n request ra AI provider |

Probe Ä‘Ă£ xĂ¡c nháº­n `docker`, `podman`, `psql` khĂ´ng cĂ³ trong PATH, khĂ´ng phĂ¡t hiá»‡n PostgreSQL service vĂ  khĂ´ng cĂ³ listener táº¡i cĂ¡c cá»•ng 5000/5173/5174/5432/6543 táº¡i thá»i Ä‘iá»ƒm kiá»ƒm tra. Log Ä‘Ă£ loáº¡i bá» secrets: [environment-probe-20261007.txt](../../minh-chung/e2e/environment-probe-20261007.txt).

### 2.1. CĂ¡ch khá»Ÿi cháº¡y Ä‘Æ°á»£c xĂ¡c minh tá»« repository

- API: `dotnet run --project services/api/RestaurantPOS.api.csproj`; Kestrel cáº¥u hĂ¬nh HTTPS port 5000 táº¡i `services/api/Program.cs:371-388` vĂ  yĂªu cáº§u certificate.
- Admin: `npm run dev` táº¡i `apps/admin-web`, port 5173; HTTPS chá»‰ báº­t khi `VITE_HTTPS=true` vĂ  cert/key tá»“n táº¡i (`apps/admin-web/vite.config.ts`).
- Customer: `npm run dev` táº¡i `apps/customer-web`, port 5174; cĂ¹ng Ä‘iá»u kiá»‡n HTTPS (`apps/customer-web/vite.config.ts`).
- API URL frontend Ä‘Æ°á»£c táº¡o táº¡i `apps/admin-web/src/config.ts` vĂ  `apps/customer-web/src/config.ts`, máº·c Ä‘á»‹nh host runtime cĂ¹ng mĂ¡y vĂ  port 5000.
- SignalR URL dĂ¹ng `${API_URL}/kitchenHub`; cĂ¡c connection thá»±c táº¿ Ä‘Æ°á»£c táº¡o trong `POSPage.tsx`, `KitchenPage.tsx`, `Navbar.tsx`, `CashierNavbar.tsx`, `InvoiceHistory.tsx`.

KhĂ´ng khá»Ÿi cháº¡y cĂ¡c process trĂªn vĂ¬ khĂ´ng cĂ³ DB test PostgreSQL/certificate/runtime configuration an toĂ n Ä‘á»ƒ thá»±c hiá»‡n ghi dá»¯ liá»‡u.

## 3. DB safety vĂ  dá»¯ liá»‡u test

### 3.1. Quyáº¿t Ä‘á»‹nh an toĂ n

KhĂ´ng cĂ³ PostgreSQL cĂ´ láº­p, Docker/Podman hoáº·c connection string test xĂ¡c Ä‘á»‹nh. VĂ¬ app chá»‰ Ä‘Äƒng kĂ½ Npgsql vĂ  khĂ´ng cĂ³ fallback local (`Program.cs:310-325`), viá»‡c khá»Ÿi Ä‘á»™ng API cĂ³ thá»ƒ cháº¡m DB Ä‘Æ°á»£c cáº¥u hĂ¬nh bĂªn ngoĂ i. Do Ä‘Ă³:

- KhĂ´ng cháº¡y `dotnet ef database update`.
- KhĂ´ng cháº¡y API vá»›i Supabase.
- KhĂ´ng cháº¡y `DbInitializer.SeedAsync`.
- KhĂ´ng táº¡o/sá»­a/xĂ³a dá»¯ liá»‡u trĂªn DB chÆ°a xĂ¡c Ä‘á»‹nh.
- KhĂ´ng cháº¡y browser flow phá»¥ thuá»™c API.

### 3.2. Bá»™ dá»¯ liá»‡u test

**ChÆ°a táº¡o** bá»™ dá»¯ liá»‡u Branch A/B, tĂ i khoáº£n, guest, table, product, order, loyalty hoáº·c schedule trong láº§n nĂ y vĂ¬ thiáº¿u DB test cĂ´ láº­p. KhĂ´ng sá»­ dá»¥ng tĂ i khoáº£n/dá»¯ liá»‡u tháº­t. CĂ¡ch chuáº©n bá»‹ Ä‘Æ°á»£c Ä‘á» xuáº¥t cho láº§n cháº¡y sau:

1. Cáº¥p má»™t PostgreSQL database/container riĂªng cho test, cĂ³ connection string chá»‰ tá»›i DB Ä‘Ă³.
2. Cháº¡y migration cĂ³ kiá»ƒm soĂ¡t; khĂ´ng báº­t seed ngoĂ i Development-isolated.
3. Seed prefix riĂªng, vĂ­ dá»¥ `E2E_20261007_`, cho Branch A/B vĂ  toĂ n bá»™ ID liĂªn quan.
4. Ghi manifest ID test vĂ o artifact, khĂ´ng ghi password/token.
5. Dá»n dá»¯ liá»‡u báº±ng transaction/teardown chá»‰ trong DB test; khĂ´ng cháº¡y delete á»Ÿ DB chÆ°a xĂ¡c Ä‘á»‹nh.

## 4. Káº¿t quáº£ kiá»ƒm thá»­

### 4.1. Tá»•ng há»£p theo loáº¡i

| Loáº¡i | PASS | FAIL | BLOCKED | NOT RUN | Ghi chĂº |
|---|---:|---:|---:|---:|---|
| Service/unit suite | 375 test | 0 | 0 | 0 | SQLite in-memory, `Foreign Keys=False`; khĂ´ng pháº£i PostgreSQL/E2E |
| Backend build | 1 | 0 | 0 | 0 | `dotnet build`, 0 warning/0 error |
| HTTP integration | 0 | 0 | 1 | 0 | KhĂ´ng cĂ³ API/DB test |
| Browser E2E | 0 | 0 | 1 | 0 | KhĂ´ng cĂ³ API/DB runtime; khĂ´ng má»Ÿ browser app |
| SignalR live | 0 | 0 | 1 | 0 | KhĂ´ng cĂ³ API/client runtime |
| Payment/provider | 0 | 0 | 1 | 0 | KhĂ´ng cĂ³ live payment/VietQR callback |
| Live AI provider AI | 0 | 0 | 1 | 0 | KhĂ´ng cĂ³ API key/service runtime |
| Frontend production build | 0 | 0 | 1 | 0 | Vite/esbuild `spawn EPERM` trong láº§n cháº¡y nĂ y; TypeScript phase Ä‘Ă£ qua trÆ°á»›c lá»—i |

### 4.2. Lá»‡nh Ä‘Ă£ cháº¡y vĂ  artifact

| Lá»‡nh | Káº¿t quáº£ | Artifact |
|---|---|---|
| `dotnet test tests/RestaurantPOS.Tests/RestaurantPOS.Tests.csproj --no-restore --logger "console;verbosity=minimal"` | **PASS** â€” 375 passed, 0 failed, 0 skipped, total 375 | [dotnet-test-20261007.txt](../../minh-chung/e2e/dotnet-test-20261007.txt) |
| `dotnet build services/api/RestaurantPOS.api.csproj --no-restore --nologo` | **PASS** â€” 0 warning, 0 error | [dotnet-build-20261007.txt](../../minh-chung/e2e/dotnet-build-20261007.txt) |
| `npm run build` táº¡i admin-web | **BLOCKED** â€” Vite config load lá»—i `Error: spawn EPERM` táº¡i esbuild | [admin-build-20261007.txt](../../minh-chung/e2e/admin-build-20261007.txt), [admin-build-20261007-retry.txt](../../minh-chung/e2e/admin-build-20261007-retry.txt) |
| `npm run build` táº¡i customer-web | **BLOCKED** â€” Vite config load lá»—i `Error: spawn EPERM` táº¡i esbuild | [customer-build-20261007.txt](../../minh-chung/e2e/customer-build-20261007.txt) |

`TestDbContextFactory` táº¡o SQLite in-memory táº¡i `tests/RestaurantPOS.Tests/TestDbContextFactory.cs:9-21`, vá»›i `Foreign Keys=False`. VĂ¬ váº­y 375 PASS chá»‰ chá»©ng minh cĂ¡c test code path Ä‘Ă£ cháº¡y trĂªn SQLite fixture; khĂ´ng chá»©ng minh PostgreSQL schema, API HTTP, SignalR, browser, camera/LAN hay production DB.

## 5. Báº£ng test case tĂ­ch há»£p/E2E

| ID | Má»¥c tiĂªu | Äiá»u kiá»‡n trÆ°á»›c | Dá»¯ liá»‡u | BÆ°á»›c | Mong Ä‘á»£i | Thá»±c táº¿ | Tráº¡ng thĂ¡i | Loáº¡i | Minh chá»©ng |
|---|---|---|---|---|---|---|---|---|---|
| AUTH-01 | Login há»£p lá»‡ | DB + account active | Staff test | POST login | JWT há»£p lá»‡ | ChÆ°a cĂ³ API/DB | BLOCKED | HTTP integration | environment probe |
| AUTH-02 | Sai máº­t kháº©u | Account test | Password sai | POST login | 401, khĂ´ng lá»™ detail | ChÆ°a cháº¡y HTTP; logic cĂ³ test unit | BLOCKED | HTTP integration | `AuthenticationHardeningTests.cs` |
| AUTH-03 | Inactive/token cÅ© | Account inactive hoáº·c branch/role Ä‘á»•i | Token cÅ© | Gá»i endpoint báº£o vá»‡ | Bá»‹ tá»« chá»‘i theo `Program.cs:131-186` | Chá»‰ code/service test, chÆ°a HTTP | INSPECTED | Source/service | `Program.cs:131-186` |
| AUTH-04 | Guest khĂ´ng dĂ¹ng chá»©c nÄƒng customer | Guest session | Guest token | Gá»i endpoint cĂ¡ nhĂ¢n | KhĂ´ng truy cáº­p dá»¯ liá»‡u customer khĂ¡c | ChÆ°a cháº¡y API | BLOCKED | HTTP integration | `GuestController.cs`, `CustomerIdorTests.cs` |
| BR-01 | Manager Ä‘á»c branch A | Hai branch + manager A | ID A | GET list/detail | Chá»‰ tháº¥y A | ChÆ°a seed DB test | BLOCKED | HTTP integration | `ManagerBranchIsolationHardeningTests.cs` |
| BR-02 | Manager truy cáº­p object branch B | Resource B | ID B | GET/PUT/DELETE B | 403/404, khĂ´ng Ä‘á»•i B | ChÆ°a cháº¡y HTTP | BLOCKED | HTTP integration | controller branch guards |
| BR-03 | Admin global scope | Admin test | A vĂ  B | GET/report/manage | Theo code cĂ³ global scope | ChÆ°a cháº¡y API | BLOCKED | HTTP integration | controller attributes/services |
| QR-01 | QR URL â†’ guest bootstrap | API + table token test | Branch A/table A | Má»Ÿ URL, bootstrap | Session guest Ä‘Ăºng table/branch | ChÆ°a cĂ³ API/browser | BLOCKED | Browser E2E | `QRScan.tsx`, `GuestController.cs` |
| QR-02 | Guest chá»n mĂ³n gá»­i yĂªu cáº§u | Guest session | Product/option A | Chá»n mĂ³n, submit | Order pending Ä‘Ăºng bĂ n/branch | ChÆ°a cháº¡y | BLOCKED | Browser E2E | `DigitalMenu.tsx`, `OrderController.cs` |
| QR-03 | POS accept pending | Pending order | Staff A | Accept táº¡i POS | Má»™t staff nháº­n, order cĂ³ owner/table | ChÆ°a cháº¡y | BLOCKED | Browser E2E/HTTP | `OrderService.cs:391-453` |
| KDS-01 | Gá»­i mĂ³n láº§n Ä‘áº§u | Accepted order | Quantity 2 | Send-to-kitchen | Request má»›i Ä‘Ăºng sá»‘ lÆ°á»£ng | ChÆ°a cháº¡y runtime | BLOCKED | HTTP/SignalR | `KitchenService.cs` |
| KDS-02 | Bá»• sung mĂ³n | `SentQuantity < Quantity` | TÄƒng quantity | Send tiáº¿p | Chá»‰ delta táº¡o request | Chá»‰ cĂ³ service tests; live chÆ°a cháº¡y | INSPECTED | Service | `KitchenServiceTests.cs`, `SentQuantity` |
| KDS-03 | Gá»i send khi khĂ´ng cĂ³ delta | Order Ä‘Ă£ gá»­i Ä‘á»§ | KhĂ´ng Ä‘á»•i quantity | Send láº¡i | KhĂ´ng táº¡o request dÆ° | ChÆ°a cháº¡y HTTP | BLOCKED | HTTP integration | `OrderController.cs:242-265` |
| KDS-04 | KhĂ´ng giáº£m dÆ°á»›i SentQuantity | Item Ä‘Ă£ gá»­i | Quantity má»›i nhá» hÆ¡n SentQuantity | Update order | KhĂ´ng lĂ m máº¥t pháº§n Ä‘Ă£ gá»­i | ÄÆ°á»£c kiá»ƒm tra trong code/test; chÆ°a browser | PASS service / NOT RUN E2E | Service + E2E | `OrderService.cs:126-255` |
| SIG-01 | Branch A event isolation | Client POS/KDS A vĂ  B | Mutation á»Ÿ A | Theo dĂµi client hai branch | Chá»‰ client Ä‘Ăºng group nháº­n | ChÆ°a cĂ³ API/client runtime | BLOCKED | Live SignalR | `/kitchenHub`, notifier |
| SIG-02 | Reconnect | Live clients | Máº¥t/káº¿t ná»‘i láº¡i | Ngáº¯t máº¡ng rá»“i reconnect | Theo cÆ¡ cháº¿ app, tráº¡ng thĂ¡i Ä‘á»“ng bá»™ | ChÆ°a cháº¡y | BLOCKED | Live SignalR | `KitchenPage.tsx` reconnect config |
| FIN-01 | Backend authoritative price | Product DB | Payload sá»­a price | Submit order | Total theo DB | Covered by service tests; chÆ°a HTTP | PASS service / NOT RUN E2E | Service | `OrderFinancialCalculationTests.cs` |
| FIN-02 | Amount khĂ¡c total | Payable order | Amount sai | POST payment | 400, khĂ´ng paid | Controller inspected; HTTP chÆ°a cháº¡y | INSPECTED | Source | `OrderController.cs:279-295` |
| FIN-03 | Payment method sai | Payable order | Method khĂ¡c allowlist | POST payment | 400 | ChÆ°a HTTP | BLOCKED | HTTP integration | `OrderController.cs:284-287` |
| FIN-04 | Thanh toĂ¡n láº·p/cáº¡nh tranh | Payable order | Hai request Ä‘á»“ng thá»i | Gá»­i tháº­t song song | Má»™t Paid, request kia conflict/already paid | ChÆ°a cĂ³ API/DB | BLOCKED | HTTP concurrency | `OrderService.PayOrderAsync` |
| LOY-01 | Earn Ä‘á»§ Ä‘iá»u kiá»‡n | Completed order + customer | Order A | Earn | Cá»™ng Ä‘Ăºng má»™t láº§n | Service tests/code pass; retry HTTP chÆ°a cháº¡y | PASS service / NOT RUN E2E | Service | `LoyaltyService.cs:23-84` |
| LOY-02 | Earn láº·p | Earn transaction Ä‘Ă£ cĂ³ | CĂ¹ng order | Gá»i láº¡i | KhĂ´ng cá»™ng láº·p | Idempotency inspected; HTTP chÆ°a cháº¡y | INSPECTED | Source | `LoyaltyService.cs:38-44` |
| LOY-03 | Redeem Ä‘á»§/khĂ´ng Ä‘á»§ | Customer balance | Points valid/too high | Redeem | ÄĂºng Ä‘iá»u kiá»‡n/sá»‘ dÆ° | ChÆ°a HTTP | BLOCKED | HTTP integration | `LoyaltyService.cs:93+` |
| MOD-01 | Product/topping CRUD | Branch/admin test | Product A | Create/update/toggle | Persist Ä‘Ăºng branch | ChÆ°a API/browser | BLOCKED | Browser/HTTP | `ProductController.cs`, `ToppingController.cs` |
| MOD-02 | Table/reservation | Branch/table/customer | Reservation A | Create/update/status | Branch vĂ  tráº¡ng thĂ¡i há»£p lá»‡ | Service tests cháº¡y; UI chÆ°a | PASS service / NOT RUN E2E | Service + browser | `TableReservationHardeningTests.cs` |
| MOD-03 | Employee/shift/schedule/attendance | Staff A | Shift/attendance data | CRUD/check-in/out | Scope vĂ  validation Ä‘Ăºng | Service/controller tests trong 375; live chÆ°a | PASS service / NOT RUN E2E | Service | `AttendanceController.cs`, `ShiftController.cs` |
| MOD-04 | Expense/dashboard/insight | Branch records | Valid/invalid branch | Create/query/mark | ÄĂºng scope, lá»—i phĂ¹ há»£p | ChÆ°a HTTP/browser | BLOCKED | HTTP integration | `DashboardController.cs`, `BusinessInsightController.cs` |
| AI-01 | Tool há»£p lá»‡ | Mock AI provider/context | Tool + args há»£p lá»‡ | Process call | Resolve/execute/result loop | Covered by AI tests | PASS service | Service integration | `AiOrchestratorTests.cs` |
| AI-02 | Tool ngoĂ i quyá»n | Role/branch context | Write tool trĂ¡i role | Process call | Permission denied | Covered by security tests; live AI provider chÆ°a | PASS service / NOT RUN live | Service | `AiPermissionService.cs` |
| AI-03 | Sequential loop max | Mock response function calls | >5 iterations | Process | Dá»«ng táº¡i giá»›i háº¡n | Covered by orchestrator code/tests | PASS service | Service | `AiOrchestrator.cs:86-207` |
| AI-04 | Price confirmation | Admin + test product | update price | Call then confirm | Dá»«ng trÆ°á»›c confirmation, khĂ´ng tá»± confirm | Code/test path; live chÆ°a | INSPECTED | Service/source | `AiOrchestrator.cs:180-189` |
| AI-05 | AI provider live | API key/model/service | Query/ambiguous/multi-step | POST chat | Äá»‘i chiáº¿u tool/data/answer | KhĂ´ng cĂ³ key/API | BLOCKED | Live AI | environment probe |
| BUILD-01 | API build | SDK/deps | API project | dotnet build | No error | PASS | Build | [dotnet-build-20261007.txt](../../minh-chung/e2e/dotnet-build-20261007.txt) |
| BUILD-02 | Admin/customer build | Node/deps | frontend projects | npm run build | Vite bundle | `spawn EPERM` táº¡i esbuild | BLOCKED | Build | frontend build logs |

## 6. Lá»—i vĂ  thay Ä‘á»•i

### 6.1. Lá»—i quan sĂ¡t Ä‘Æ°á»£c

- Hai láº§n cháº¡y `npm run build` frontend trong phiĂªn nĂ y khĂ´ng qua Ä‘Æ°á»£c Vite config loader vĂ¬ `Error: spawn EPERM` trong `esbuild`. TypeScript phase Ä‘Ă£ cháº¡y trÆ°á»›c khi Vite bĂ¡o lá»—i; chÆ°a Ä‘á»§ báº±ng chá»©ng Ä‘á»ƒ káº¿t luáº­n lá»—i mĂ£ nguá»“n.
- KhĂ´ng cĂ³ lá»—i test á»©ng dá»¥ng: bá»™ 375 test káº¿t thĂºc PASS.
- KhĂ´ng cĂ³ lá»—i API/HTTP/SignalR vĂ¬ cĂ¡c thĂ nh pháº§n Ä‘Ă³ chÆ°a khá»Ÿi cháº¡y.

### 6.2. Thay Ä‘á»•i trong phiĂªn

KhĂ´ng sá»­a mĂ£ nguá»“n á»©ng dá»¥ng, controller, service, entity, migration hoáº·c test. Chá»‰ táº¡o artifact bĂ¡o cĂ¡o vĂ  log trong `bao-cao/minh-chung/e2e/`.

## 7. AI provider live vĂ  AI accuracy

KhĂ´ng thá»±c hiá»‡n AI provider live vĂ¬ khĂ´ng cĂ³ `AI provider:ApiKey` runtime. KhĂ´ng cĂ³ dataset, Ä‘Ă¡p Ă¡n chuáº©n, tiĂªu chĂ­, tá»­ sá»‘/máº«u sá»‘ hoáº·c raw output Ä‘á»ƒ Ä‘o accuracy. KhĂ´ng Ä‘Æ°á»£c dĂ¹ng cĂ¡c sá»‘ 450 cases, 92%, 100% hoáº·c 98% lĂ m káº¿t quáº£ kiá»ƒm thá»­ cá»§a láº§n nĂ y.

CĂ¡c kiá»ƒm tra AI Ä‘Ă£ cháº¡y lĂ  test/mock/service-level trong bá»™ 375 test; chĂºng khĂ´ng thay tháº¿ live model evaluation.

## 8. Giá»›i háº¡n cĂ²n láº¡i

- ChÆ°a cĂ³ PostgreSQL test database/container vĂ  chÆ°a xĂ¡c minh migration trĂªn provider Npgsql.
- ChÆ°a cháº¡y API HTTP, Swagger, authentication thá»±c, browser, QR URL, camera thiáº¿t bá»‹ hoáº·c LAN mobile.
- ChÆ°a cháº¡y POS â†’ KDS â†’ payment end-to-end.
- ChÆ°a quan sĂ¡t event SignalR thá»±c táº¿ tá»« server tá»›i handler; khĂ´ng Ä‘Ă¡nh dáº¥u realtime Ä‘áº¡t.
- ChÆ°a kiá»ƒm tra thanh toĂ¡n ngĂ¢n hĂ ng/VietQR callback/webhook; payment code path chÆ°a pháº£i báº±ng chá»©ng ngĂ¢n hĂ ng xĂ¡c nháº­n.
- ChÆ°a cháº¡y HTTP concurrency cho accept/payment/loyalty.
- ChÆ°a cháº¡y frontend production build thĂ nh cĂ´ng trong phiĂªn nĂ y do `spawn EPERM`; káº¿t quáº£ build thĂ nh cĂ´ng cá»§a audit trÆ°á»›c khĂ´ng Ä‘Æ°á»£c gĂ¡n láº¡i cho phiĂªn nĂ y.
- ChÆ°a cĂ³ load test, performance metric, coverage UI/E2E hoáº·c AI benchmark.
- Camera vĂ  LAN chÆ°a Ä‘Æ°á»£c kiá»ƒm chá»©ng; khĂ´ng Ä‘Æ°á»£c diá»…n Ä‘áº¡t lĂ  Ä‘Ă£ test QR camera/Ä‘iá»‡n thoáº¡i.

## 9. CĂ¡ch cháº¡y láº¡i vĂ  dá»n dá»¯ liá»‡u

### 9.1. Pháº§n hiá»‡n cĂ³, khĂ´ng cáº§n DB váº­n hĂ nh

```powershell
dotnet test tests/RestaurantPOS.Tests/RestaurantPOS.Tests.csproj --no-restore --logger "console;verbosity=minimal"
dotnet build services/api/RestaurantPOS.api.csproj --no-restore --nologo
```

Hai lá»‡nh trĂªn dĂ¹ng test/build local; test factory dĂ¹ng SQLite in-memory.

### 9.2. Pháº§n integration/E2E cĂ²n chá» háº¡ táº§ng

1. Cáº¥p PostgreSQL test riĂªng hoáº·c container riĂªng, xĂ¡c nháº­n host/database lĂ  test-only.
2. Cung cáº¥p connection string qua environment/user-secrets, khĂ´ng commit vĂ  khĂ´ng in log.
3. XĂ¡c nháº­n certificate HTTPS vĂ  origin frontend.
4. Cháº¡y migration/seed cĂ³ kiá»ƒm soĂ¡t chá»‰ trĂªn DB test.
5. Cháº¡y API port 5000, admin 5173, customer 5174; táº¡o dá»¯ liá»‡u prefix `E2E_20261007_`.
6. Cháº¡y HTTP/browser/SignalR scenarios trong báº£ng trĂªn.
7. LÆ°u screenshot/trace/log Ä‘Ă£ redacted vĂ o `bao-cao/minh-chung/e2e/`.
8. Teardown chá»‰ trĂªn DB test báº±ng manifest ID; xĂ¡c nháº­n DB váº­n hĂ nh khĂ´ng bá»‹ cháº¡m.

## 10. Nhá»¯ng phĂ¡t biá»ƒu trong khĂ³a luáº­n cĂ³ thá»ƒ cáº­p nháº­t sau kiá»ƒm thá»­

| Chá»§ Ä‘á»/module | PhĂ¡t biá»ƒu trÆ°á»›c Ä‘Ă¢y cĂ²n thiáº¿u báº±ng chá»©ng | Báº±ng chá»©ng má»›i | CĂ¢u diá»…n Ä‘áº¡t Ä‘Æ°á»£c phĂ©p Ä‘Æ°a vĂ o khĂ³a luáº­n | Giá»›i háº¡n cáº§n giá»¯ |
|---|---|---|---|---|
| Backend tests | CĂ³ bá»™ kiá»ƒm thá»­ backend | `dotnet-test-20261007.txt`: 375 passed/0 failed/0 skipped | â€œBá»™ test hiá»‡n cĂ³ thá»±c thi thĂ nh cĂ´ng 375 test trĂªn SQLite in-memory.â€ | KhĂ´ng gá»i Ä‘Ă¢y lĂ  E2E/PostgreSQL/UI coverage; FK SQLite bá»‹ táº¯t trong factory. |
| Backend build | Backend hoáº¡t Ä‘á»™ng | `dotnet-build-20261007.txt` | â€œBackend build thĂ nh cĂ´ng vá»›i 0 warning vĂ  0 error trong mĂ´i trÆ°á»ng audit.â€ | Build khĂ´ng chá»©ng minh runtime DB/API. |
| Branch/auth/order/AI service | Logic cĂ³ test service/security | 375 test vĂ  source evidence | â€œCĂ¡c code path service/security tÆ°Æ¡ng á»©ng Ä‘Ă£ Ä‘Æ°á»£c kiá»ƒm tra báº±ng test cĂ´ láº­p.â€ | ChÆ°a cĂ³ HTTP integration vá»›i PostgreSQL vĂ  token runtime. |
| API integration | ChÆ°a cĂ³ evidence | KhĂ´ng cĂ³ DB/API runtime | â€œKiá»ƒm thá»­ HTTP integration bá»‹ BLOCKED do chÆ°a cĂ³ PostgreSQL test cĂ´ láº­p.â€ | KhĂ´ng ghi PASS hoáº·c káº¿t luáº­n production. |
| QR/POS/KDS | Luá»“ng Ä‘Æ°á»£c mĂ´ táº£ trong code | Controller/service/frontend source | â€œLuá»“ng Ä‘Æ°á»£c xĂ¡c minh á»Ÿ má»©c mĂ£ nguá»“n; E2E QRâ†’POSâ†’KDS chÆ°a thá»±c hiá»‡n.â€ | KhĂ´ng kháº³ng Ä‘á»‹nh event/realtime hoáº·c camera. |
| SignalR | CĂ³ cáº¥u hĂ¬nh hub/client | `Program.cs`, hub/client source | â€œHá»‡ thá»‘ng cĂ³ endpoint/client SignalR trong mĂ£ nguá»“n.â€ | ChÆ°a quan sĂ¡t connection, group, event delivery hoáº·c reconnect live. |
| Payment | Backend kiá»ƒm tra persisted total | `OrderController.cs:279-315`, service tests | â€œPayment path kiá»ƒm tra sá»‘ tiá»n theo persisted order total á»Ÿ code.â€ | ChÆ°a test HTTP cáº¡nh tranh, VietQR hoáº·c ngĂ¢n hĂ ng callback. |
| Loyalty | Earn/Redeem code path | `LoyaltyService.cs`, service tests | â€œEarn/Redeem vĂ  idempotency Ä‘Æ°á»£c kiá»ƒm tra á»Ÿ service/source level.â€ | Refund/Reversal vĂ  retry HTTP chÆ°a Ä‘Æ°á»£c xĂ¡c minh Ä‘áº§y Ä‘á»§. |
| AI Agent | Orchestrator/tool permissions cĂ³ test | `AiOrchestratorTests`, security tests, source | â€œTool registry, permission check vĂ  vĂ²ng tuáº§n tá»± Ä‘Æ°á»£c kiá»ƒm tra á»Ÿ service level.â€ | AI provider live vĂ  accuracy benchmark chÆ°a cháº¡y. |
| Frontend build | Build hiá»‡n táº¡i | Log `spawn EPERM` | â€œFrontend build bá»‹ cháº·n bá»Ÿi lá»—i mĂ´i trÆ°á»ng esbuild `spawn EPERM` trong phiĂªn audit.â€ | KhĂ´ng dĂ¹ng build audit trÆ°á»›c Ä‘á»ƒ thay tháº¿; cáº§n cháº¡y láº¡i trĂªn mĂ´i trÆ°á»ng cĂ³ quyá»n spawn phĂ¹ há»£p. |

## Káº¿t luáº­n

Láº§n nĂ y Ä‘Ă£ bá»• sung báº±ng chá»©ng thá»±c thi cho bá»™ test cĂ´ láº­p vĂ  backend build, nhÆ°ng chÆ°a thá»ƒ bá»• sung báº±ng chá»©ng integration/E2E thá»±c sá»± vĂ¬ thiáº¿u PostgreSQL test cĂ´ láº­p, API runtime, certificate/runtime secrets vĂ  mĂ´i trÆ°á»ng browser-to-backend. Káº¿t quáº£ Ä‘Ăºng lĂ  **service suite PASS**, **backend build PASS**, cĂ²n HTTP integration, browser E2E, SignalR live, payment provider, AI provider live vĂ  frontend Vite build lĂ  **BLOCKED**, khĂ´ng pháº£i PASS.

---

# ADDENDUM â€” Láº¦N CHáº Y POSTGRESQL/API/FRONTEND THá»°C Táº¾ 2026-10-07

Pháº§n nĂ y giá»¯ nguyĂªn lá»‹ch sá»­ probe ban Ä‘áº§u á»Ÿ trĂªn vĂ  ghi nháº­n láº§n cháº¡y tiáº¿p theo theo yĂªu cáº§u E2E. Náº¿u tráº¡ng thĂ¡i cÅ© mĂ¢u thuáº«n vá»›i addendum nĂ y, dĂ¹ng tráº¡ng thĂ¡i má»›i hÆ¡n trong addendum.

## A. Blocker ban Ä‘áº§u vĂ  cĂ¡ch giáº£i quyáº¿t

### A.1. PostgreSQL

Blocker ban Ä‘áº§u lĂ  khĂ´ng cĂ³ `postgres`, `initdb`, `pg_ctl`, `psql` trong PATH; khĂ´ng cĂ³ PostgreSQL service, Docker hoáº·c Podman. ÄĂ£ kiá»ƒm tra thĂªm cĂ¡c thÆ° má»¥c cĂ i Ä‘áº·t Windows thĂ´ng dá»¥ng vĂ  khĂ´ng tĂ¬m tháº¥y binary.

ÄĂ£ dĂ¹ng gĂ³i binary Windows chĂ­nh thá»©c do PostgreSQL dáº«n tá»›i EDB (trang táº£i Windows vĂ  trang binary archive):

- https://www.postgresql.org/download/windows/
- https://www.enterprisedb.com/download-postgresql-binaries

ÄĂ£ táº£i PostgreSQL 17.11 Windows x64 binary archive, kiá»ƒm tra SHA-256 trÆ°á»›c khi giáº£i nĂ©n, rá»“i táº¡o cluster riĂªng báº±ng tĂ i khoáº£n ngÆ°á»i dĂ¹ng:

- binary: `.tools/postgresql-17.11/pgsql`
- data directory: `.tools/pgdata-e2e`
- endpoint: `127.0.0.1:55432`
- database: `restaurant_e2e`
- role: `e2e_admin`

`pg_ctl` gáº·p cáº£nh bĂ¡o restricted-token cá»§a mĂ´i trÆ°á»ng Windows sandbox nĂªn khĂ´ng khá»Ÿi Ä‘á»™ng Ä‘Æ°á»£c; cháº¡y trá»±c tiáº¿p `postgres.exe` trĂªn cluster riĂªng Ä‘Ă£ thĂ nh cĂ´ng vĂ  PostgreSQL 17.11 bĂ¡o `ready to accept connections`. KhĂ´ng thay Ä‘á»•i service há»‡ thá»‘ng, firewall, Supabase hay database váº­n hĂ nh.

Migration báº±ng EF Core cháº¡y **PASS** trĂªn Ä‘Ăºng database nĂ y; xem [postgres-migration-summary-20261007.txt](../../minh-chung/e2e/postgres-migration-summary-20261007.txt). Seed test Ä‘Æ°á»£c xĂ¡c nháº­n báº±ng psql; xem [postgres-seed-summary-20261007.txt](../../minh-chung/e2e/postgres-seed-summary-20261007.txt).

### A.2. API HTTPS

API Ä‘á»c Ä‘Ăºng cáº¥u hĂ¬nh `Program.cs`, cháº¡y vá»›i `DatabaseSafety:AllowAutomaticMigration=false`, connection Ä‘Æ°á»£c cáº¥p qua environment vĂ  khĂ´ng ghi secret vĂ o artifact. Kestrel khá»Ÿi Ä‘á»™ng thĂ nh cĂ´ng trĂªn HTTPS port 5000.

Blocker cĂ²n láº¡i lĂ  TLS handshake cá»§a mĂ´i trÆ°á»ng:

- curl Schannel: `SEC_E_NO_CREDENTIALS`;
- OpenSSL: `unexpected eof while reading`;
- in-app browser: `ERR_CONNECTION_CLOSED` khi má»Ÿ `/health`;
- PFX do `dotnet dev-certs` táº¡o cĂ²n bá»‹ `CryptographicException: Access denied` khi Kestrel náº¡p.

ÄĂ£ thá»­ PFX repository, PFX self-signed OpenSSL cĂ³ password, PFX self-signed OpenSSL khĂ´ng password vĂ  PFX .NET dev-cert. KhĂ´ng táº¯t HTTPS, khĂ´ng bá» kiá»ƒm tra certificate trong mĂ£ nghiá»‡p vá»¥, khĂ´ng dĂ¹ng HTTP fallback. Chi tiáº¿t: [api-https-probe-20261007.txt](../../minh-chung/e2e/api-https-probe-20261007.txt).

### A.3. Frontend/browser

ÄĂ£ khá»Ÿi cháº¡y Ä‘Æ°á»£c cáº£ hai dev server sau khi cháº¡y trong execution context cĂ³ quyá»n spawn:

- admin-web: `http://localhost:5173/` â€” browser load trang Ä‘Äƒng nháº­p;
- customer-web: `http://localhost:5174/` â€” browser load trang guest QR scan.

Hai production build cÅ©ng Ä‘Ă£ **PASS** khi cháº¡y láº¡i trong cĂ¹ng Ä‘iá»u kiá»‡n escalated; lá»—i `spawn EPERM` cá»§a láº§n probe khĂ´ng cĂ²n tĂ¡i hiá»‡n á»Ÿ láº§n cháº¡y nĂ y. Xem [frontend-runtime-20261007.txt](../../minh-chung/e2e/frontend-runtime-20261007.txt).

Browser chá»‰ kiá»ƒm tra UI vĂ  QR scan page. ChÆ°a cáº¥p quyá»n camera, chÆ°a quĂ©t QR báº±ng camera tháº­t vĂ  khĂ´ng coi viá»‡c má»Ÿ URL QR lĂ  camera E2E.

## B. MĂ´i trÆ°á»ng thá»±c sá»± Ä‘Ă£ cháº¡y vĂ  dá»¯ liá»‡u tĂ¡i láº­p

- API build: `dotnet build services/api/RestaurantPOS.api.csproj --no-restore --nologo` â€” PASS.
- Service suite: 375 passed, 0 failed, 0 skipped trĂªn SQLite fixture â€” PASS, nhÆ°ng khĂ´ng gĂ¡n lĂ  PostgreSQL E2E.
- PostgreSQL migration/seed: PASS trĂªn cluster riĂªng.
- Runner `.tools/e2e-runner/E2eRunner.csproj`: gá»i trá»±c tiáº¿p `OrderService`, `KitchenService`, `LoyaltyService` vá»›i `ApplicationDbContext`/Npgsql PostgreSQL tháº­t â€” PASS.
- Dá»¯ liá»‡u dĂ¹ng prefix `E2E`/`e2e_`; cĂ³ branch-specific settings, account active/inactive, customer, product cĂ³ size/topping, QR table. Password vĂ  QR token khĂ´ng lÆ°u trong bĂ¡o cĂ¡o.
- Báº±ng chá»©ng runner: [postgres-service-e2e-20261007.txt](../../minh-chung/e2e/postgres-service-e2e-20261007.txt).

Runner Ä‘Ă£ thá»±c thi Ä‘Æ°á»£c:

1. táº¡o order vá»›i giĂ¡ láº¥y tá»« DB;
2. kiá»ƒm tra subtotal 150000, service fee 7500, VAT 15750, total 173250;
3. cháº¥p nháº­n web order vá»›i branch context;
4. gá»­i báº¿p láº§n Ä‘áº§u quantity 2;
5. tÄƒng quantity, request láº§n hai chá»‰ chá»©a delta quantity 1 vĂ  giá»¯ `SentQuantity=2`;
6. amount thanh toĂ¡n sai bá»‹ tá»« chá»‘i;
7. hai payment request Ä‘á»“ng thá»i trĂªn hai DbContext: Ä‘Ăºng má»™t `Paid`, má»™t `AlreadyPaid`;
8. payment tá»± earn loyalty má»™t láº§n vĂ  gá»i earn láº·p khĂ´ng cá»™ng láº¡i.

ÄĂ¢y lĂ  **application-service integration trĂªn PostgreSQL**, khĂ´ng pháº£i HTTP/browser E2E. VĂ¬ HTTPS handshake chÆ°a qua nĂªn chÆ°a Ä‘Æ°á»£c tĂ­nh lĂ  Ä‘Äƒng nháº­p HTTP, QRâ†’POSâ†’KDS browser, SignalR live hoáº·c payment HTTP.

## C. Tráº¡ng thĂ¡i cáº­p nháº­t theo test matrix

| NhĂ³m | Tráº¡ng thĂ¡i má»›i | Káº¿t quáº£ thá»±c táº¿ |
|---|---|---|
| PostgreSQL migration/seed | PASS | Cluster riĂªng, 47 migrations, seed prefix E2E |
| Order financial calculation trĂªn PostgreSQL | PASS | GiĂ¡ authoritative tá»« DB; subtotal/service fee/VAT/total Ä‘Ăºng |
| Order update + SentQuantity | PASS | Delta báº¿p 2 rá»“i 1; khĂ´ng gá»­i láº¡i pháº§n cÅ© |
| Service-level payment wrong amount | PASS | Amount sai bá»‹ `NotPayable` |
| Service-level concurrent payment | PASS | Má»™t `Paid`, má»™t `AlreadyPaid`, hai DbContext |
| Service-level loyalty idempotency | PASS | Earn tá»« payment má»™t láº§n; earn láº·p khĂ´ng cá»™ng |
| API startup | PASS | Kestrel start HTTPS 5000, automatic migration/seed táº¯t |
| API `/health` vĂ  HTTP auth | BLOCKED | TLS handshake Ä‘Ă³ng trÆ°á»›c HTTP |
| Admin/customer dev runtime | PASS | Cáº£ hai URL load Ä‘Æ°á»£c trong browser |
| Admin/customer production build | PASS | Build thĂ nh cĂ´ng khi execution context Ä‘Æ°á»£c phĂ©p spawn |
| QR guestâ†’POSâ†’KDS báº±ng browser | BLOCKED | API HTTPS chÆ°a truy cáº­p Ä‘Æ°á»£c; camera chÆ°a test |
| SignalR live / hai branch | BLOCKED | ChÆ°a cĂ³ HTTP/Hub connection do TLS |
| Payment HTTP/concurrency | BLOCKED | Chá»‰ service-level Ä‘Ă£ PASS |
| Loyalty HTTP/retry | BLOCKED | Chá»‰ service-level Ä‘Ă£ PASS |
| Quáº£n trá»‹ qua HTTP/browser | BLOCKED | ChÆ°a qua API transport |
| AI tools/permission mock-service | PASS | Káº¿ thá»«a 375 test/service evidence |
| AI provider live | BLOCKED | KhĂ´ng cĂ³ API key; khĂ´ng gá»i live |

CĂ¡c dĂ²ng `AUTH-*`, `BR-*`, `QR-*`, `SIG-*`, `FIN-02/03/04`, `LOY-02/03`, `MOD-*` trong báº£ng cÅ© váº«n giá»¯ BLOCKED/NOT RUN á»Ÿ cáº¥p HTTP/browser náº¿u chÆ°a cĂ³ dĂ²ng PASS service má»›i tÆ°Æ¡ng á»©ng. KhĂ´ng chuyá»ƒn service PASS thĂ nh E2E PASS.

## D. Thay Ä‘á»•i do láº§n nĂ y táº¡o vĂ  pháº§n khĂ´ng sá»­a

ÄĂ£ táº¡o riĂªng trong working tree cho láº§n cháº¡y:

- `.tools/` chá»©a binary PostgreSQL, cluster data, certificate test-local vĂ  runner táº¡m;
- `bao-cao/minh-chung/e2e/postgres-migration-summary-20261007.txt`;
- `bao-cao/minh-chung/e2e/postgres-seed-summary-20261007.txt`;
- `bao-cao/minh-chung/e2e/postgres-service-e2e-20261007.txt`;
- `bao-cao/minh-chung/e2e/api-https-probe-20261007.txt`;
- `bao-cao/minh-chung/e2e/frontend-runtime-20261007.txt`;
- pháº§n addendum nĂ y cá»§a bĂ¡o cĂ¡o.

KhĂ´ng sá»­a controller/service/entity/migration/test nghiá»‡p vá»¥ Ä‘á»ƒ vÆ°á»£t blocker. KhĂ´ng reset, stash hoáº·c ghi Ä‘Ă¨ thay Ä‘á»•i cĂ³ sáºµn. Runner chá»‰ lĂ  harness táº¡m ngoĂ i source á»©ng dá»¥ng; khĂ´ng Ä‘Æ°a credential vĂ o source/report.

## E. Dá»n dá»¯ liá»‡u vĂ  dá»«ng process

Khi káº¿t thĂºc phiĂªn, cáº§n dá»«ng bá»‘n process test báº±ng Ctrl+C: `postgres.exe` cluster port 55432, API port 5000, admin-web port 5173 vĂ  customer-web port 5174. Database `restaurant_e2e`, data directory `.tools/pgdata-e2e`, certificate vĂ  runner Ä‘á»u lĂ  test-only; cĂ³ thá»ƒ xĂ³a toĂ n bá»™ `.tools` sau khi lÆ°u artifact bĂ¡o cĂ¡o náº¿u nhĂ³m muá»‘n teardown. KhĂ´ng cháº¡y teardown trĂªn database ngoĂ i `.tools`.

## F. PhĂ¡t biá»ƒu khĂ³a luáº­n Ä‘Æ°á»£c phĂ©p cáº­p nháº­t / pháº£i giá»¯ chÆ°a xĂ¡c minh

### Äá»§ báº±ng chá»©ng Ä‘á»ƒ cáº­p nháº­t

- CĂ³ thá»ƒ ghi: â€œEF Core migration Ä‘Ă£ Ä‘Æ°á»£c cháº¡y thĂ nh cĂ´ng trĂªn má»™t PostgreSQL 17.11 test instance cĂ´ láº­p.â€
- CĂ³ thá»ƒ ghi: â€œTrĂªn PostgreSQL test, OrderService dĂ¹ng giĂ¡ authoritative trong DB vĂ  tĂ­nh snapshot subtotal/service fee/VAT/total theo cáº¥u hĂ¬nh branch.â€
- CĂ³ thá»ƒ ghi: â€œTrĂªn PostgreSQL test, bá»• sung mĂ³n chá»‰ táº¡o delta theo `SentQuantity`; payment amount sai bá»‹ tá»« chá»‘i; hai payment cáº¡nh tranh cho káº¿t quáº£ má»™t thĂ nh cĂ´ng vĂ  má»™t AlreadyPaid; loyalty earn idempotent.â€
- CĂ³ thá»ƒ ghi: â€œAdmin-web vĂ  customer-web dev/production build Ä‘Ă£ cháº¡y thĂ nh cĂ´ng trong execution context cho phĂ©p Vite spawn; browser load Ä‘Æ°á»£c login vĂ  QR scan page.â€
- CĂ³ thá»ƒ ghi: â€œService/unit suite hiá»‡n cĂ³ 375 test PASS trĂªn SQLite in-memory.â€

### Váº«n pháº£i giá»¯ â€œchÆ°a xĂ¡c minhâ€

- ÄÄƒng nháº­p HTTP, inactive account vĂ  má»i claim authorization/branch isolation qua JWT runtime.
- QR guestâ†’submitâ†’POS acceptâ†’KDSâ†’payment báº±ng browser/HTTP.
- SignalR connection, group isolation, event delivery, reconnect vĂ  hai branch live.
- HTTP payment retry/concurrency, payment provider/VietQR/webhook vĂ  invoice/receipt browser flow.
- Loyalty Redeem/Refund qua HTTP vĂ  retry thá»±c táº¿.
- Camera tháº­t, quyá»n camera, thiáº¿t bá»‹ mobile/LAN vĂ  Ä‘á»™ tin cáº­y certificate trĂªn Ä‘iá»‡n thoáº¡i.
- CĂ¡c luá»“ng quáº£n trá»‹ qua API/browser production.
- AI provider live, accuracy benchmark, hoáº·c má»i káº¿t luáº­n dá»±a trĂªn API key chÆ°a cung cáº¥p.

### Blocker cĂ²n láº¡i vĂ  Ä‘iá»u kiá»‡n giáº£i quyáº¿t

1. **HTTPS handshake trong mĂ´i trÆ°á»ng hiá»‡n táº¡i**: cáº§n cháº¡y API trong Windows user session/host cĂ³ Kestrel TLS credential hoáº¡t Ä‘á»™ng (hoáº·c cung cáº¥p certificate store/permission tÆ°Æ¡ng thĂ­ch), sau Ä‘Ă³ xĂ¡c nháº­n `curl -k https://127.0.0.1:5000/health` tráº£ HTTP 200. KhĂ´ng cáº§n AI provider key Ä‘á»ƒ giáº£i quyáº¿t blocker QR/POS/KDS.
2. **Browser-to-API E2E**: sau khi health pass, cháº¡y láº¡i login, branch denial, QR URL, order acceptance, KDS/SignalR vĂ  payment báº±ng cĂ¹ng DB test; lÆ°u response/DB evidence Ä‘Ă£ redacted.
3. **Camera/mobile/LAN**: cáº§n thiáº¿t bá»‹/browser HTTPS tháº­t vĂ  quyá»n camera; khĂ´ng thay tháº¿ báº±ng URL QR.
4. **AI provider live**: chá»‰ má»Ÿ khi cĂ³ API key há»£p lá»‡ do chá»§ há»‡ thá»‘ng cung cáº¥p; náº¿u khĂ´ng, giá»¯ BLOCKED.


