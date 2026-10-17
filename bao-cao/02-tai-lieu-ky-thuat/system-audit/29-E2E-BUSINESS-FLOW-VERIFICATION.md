# E2E / Runtime Business Flow Verification

## Scope and safety decision

ÄĂ¢y lĂ  verification phase sau UI/UX Batch 1â€“5. KhĂ´ng sá»­a source, khĂ´ng refactor, khĂ´ng táº¡o migration vĂ  khĂ´ng táº¡o dá»¯ liá»‡u nghiá»‡p vá»¥.

Runtime mutation bá»‹ **BLOCKED** trong mĂ´i trÆ°á»ng hiá»‡n táº¡i vĂ¬:

- API startup gá»i `Database.Migrate()` vĂ  `DbInitializer.SeedAsync()`.
- Configuration cĂ³ Supabase/PostgreSQL remote connection trong .NET User Secrets nhÆ°ng repository khĂ´ng chá»©ng minh database target lĂ  development/test.
- VĂ¬ váº­y khĂ´ng khá»Ÿi Ä‘á»™ng API, khĂ´ng login runtime, khĂ´ng submit order, khĂ´ng chuyá»ƒn status vĂ  khĂ´ng thanh toĂ¡n.

KhĂ´ng cĂ³ secret value nĂ o Ä‘Æ°á»£c ghi vĂ o report.

## 0. Environment discovery

| Item | Status | Evidence/notes |
|---|---|---|
| API project | PRESENT | `services/api/RestaurantPOS.api.csproj` |
| API HTTPS port | PRESENT | `services/api/Program.cs`, `Properties/launchSettings.json`, port 5000 |
| HTTPS certificate | PRESENT | `.https/pos-cert.pfx` exists; password value khĂ´ng ghi nháº­n |
| PostgreSQL/Supabase connection configuration | PRESENT | `ConnectionStrings:DefaultConnection` user-secret key exists; target safety chÆ°a xĂ¡c nháº­n |
| JWT secret configuration | PRESENT | `Jwt:Secret` user-secret key exists; value khĂ´ng ghi nháº­n |
| external AI provider key | MISSING/NOT REQUIRED | KhĂ´ng cáº§n cho QR/POS/Kitchen flow; AI khĂ´ng pháº£i blocker cá»§a flow nĂ y |
| Development seed password | UNKNOWN/MISSING | KhĂ´ng tĂ¬m tháº¥y key seed trong user-secret inventory; seed chá»‰ cháº¡y khi database chÆ°a cĂ³ employee |
| Admin frontend | PRESENT | Vite port 5173, HTTPS config trong `vite.config.ts` |
| Customer frontend | PRESENT | Vite port 5174, runtime API host config trong `src/config.ts` |
| CORS | PRESENT | Development CORS giá»›i háº¡n HTTPS origin port 5173/5174 |
| SignalR endpoint | PRESENT | `/kitchenHub`; JWT query token support trong `Program.cs` |
| Confirmed safe development database | MISSING | KhĂ´ng cĂ³ báº±ng chá»©ng Ä‘á»§ Ä‘á»ƒ cho phĂ©p mutation |

## 1. Baseline

Baseline cháº¡y Ä‘Ăºng project:

| Check | Result |
|---|---|
| `dotnet build services/api/RestaurantPOS.api.csproj --no-restore` | PASS |
| `dotnet test tests/RestaurantPOS.Tests/RestaurantPOS.Tests.csproj --no-restore` | PASS â€” 177/177 |
| `npm run build` trong `apps/admin-web` | PASS â€” existing large-bundle warning |
| `npm run build` trong `apps/customer-web` | PASS |

Working tree cĂ³ nhiá»u pre-existing changes tá»« cĂ¡c phase trÆ°á»›c. KhĂ´ng restore/overwrite vĂ  khĂ´ng cĂ³ source change trong verification phase nĂ y.

## 2. Test capability discovery

Repository cĂ³ backend automated tests dĂ¹ng EF/test fixtures, gá»“m `KitchenServiceTests`, `OrderServiceTests`, `JwtAuthenticationTests`, `HardeningIntegrationTests` vĂ  cĂ¡c test security/business khĂ¡c.

KhĂ´ng tĂ¬m tháº¥y Playwright, Cypress, Selenium, WebApplicationFactory-based browser E2E, Postman/Newman collection hoáº·c browser automation script phĂ¹ há»£p Ä‘á»ƒ reuse. KhĂ´ng cĂ i thĂªm framework trong phase nĂ y.

PhĂ¢n loáº¡i:

- **AUTOMATED VERIFIED**: build/test suite backend hiá»‡n cĂ³.
- **STATIC VERIFIED**: source trace, API contract vĂ  frontend flow.
- **RUNTIME VERIFIED**: chÆ°a Ä‘áº¡t vĂ¬ database safety gate cháº·n API startup/mutation.
- **MANUAL VERIFICATION REQUIRED**: browser, camera QR, realtime multi-client, payment vĂ  responsive viewport.

## 3. Business-flow verification matrix

| ID | Flow | Static | Automated | Runtime | Result | Evidence | Notes |
|---|---|---|---|---|---|---|---|
| AUTH-01 | Staff login | VERIFIED | PARTIAL | NOT RUN | MANUAL REQUIRED | `AuthController`, `CustomerLogin`, JWT tests | KhĂ´ng login runtime trĂªn remote DB |
| AUTH-02 | Guest QR token | VERIFIED | PARTIAL | NOT RUN | MANUAL REQUIRED | `CustomerLogin.tsx`, `/api/Auth/customer-token` | Guest khĂ´ng yĂªu cáº§u password |
| QR-01 | QR resolution | VERIFIED | NO browser E2E | NOT RUN | MANUAL REQUIRED | `QRScan.tsx`, `DigitalMenu.tsx` | Cáº§n camera/valid table tháº­t |
| MENU-01 | Menu loading | VERIFIED | NO E2E | NOT RUN | MANUAL REQUIRED | Product/Topping fetch trong `DigitalMenu.tsx` | Cáº§n API/database an toĂ n |
| CART-01 | Cart operations | VERIFIED | NO frontend E2E | NOT RUN | MANUAL REQUIRED | `DigitalMenu.tsx` | Static payload/options trace pass |
| ORDER-01 | Submit customer order | VERIFIED | Backend order tests only | NOT RUN | MANUAL REQUIRED | `POST /api/Order`, `confirmOrder` | KhĂ´ng táº¡o mutation do DB gate |
| ORDER-02 | POS/order visibility | VERIFIED | Backend tests partial | NOT RUN | MANUAL REQUIRED | POS order fetch/active order flow | ChÆ°a cĂ³ multi-client runtime |
| KITCHEN-01 | Kitchen receives order | VERIFIED | `KitchenServiceTests` partial | NOT RUN | MANUAL REQUIRED | `KitchenService`, `KitchenPage`, `NewOrderRequest` | Cáº§n customer/POS order tháº­t |
| KITCHEN-02 | Kitchen status transition | VERIFIED | Kitchen/service tests partial | NOT RUN | MANUAL REQUIRED | Pending â†’ Preparing â†’ Ready â†’ Completed | KhĂ´ng mutate shared DB |
| POS-01 | Existing order update | VERIFIED | Order tests partial | NOT RUN | MANUAL REQUIRED | `POSPage` `sentQuantity`/existing order flow | Cáº§n xĂ¡c nháº­n runtime delta |
| POS-02 | Add dish | VERIFIED | Backend tests partial | NOT RUN | MANUAL REQUIRED | POS send-to-kitchen flow | ChÆ°a runtime verify item delta |
| REALTIME-01 | SignalR update | VERIFIED | SignalR/backend tests partial | NOT RUN | MANUAL REQUIRED | `/kitchenHub`, event names in source | Cáº§n hai browser clients |
| PAY-01 | Payment | VERIFIED | Payment/backend tests partial | NOT RUN | MANUAL REQUIRED | `Cash`/`Chuyá»ƒn khoáº£n` source flow | KhĂ´ng táº¡o payment mutation |
| TABLE-01 | Table status | VERIFIED | Backend tests partial | NOT RUN | MANUAL REQUIRED | DigitalMenu table status PATCH, POS status flow | Cáº§n development DB an toĂ n |
| NEG-01 | Invalid QR | VERIFIED | NO frontend E2E | NOT RUN | MANUAL REQUIRED | DigitalMenu controlled feedback | Cáº§n route/table runtime |
| NEG-02 | Duplicate/invalid submit | VERIFIED | Static guard only | NOT RUN | MANUAL REQUIRED | `isSubmittingOrder` guard | ChÆ°a test browser double-tap |
| MOBILE-01 | Customer mobile | STATIC | NO viewport automation | NOT RUN | MANUAL REQUIRED | Batch 5 responsive source | Cáº§n 390/430/768 runtime |
| MOBILE-02 | POS responsive | STATIC | NO viewport automation | NOT RUN | MANUAL REQUIRED | Batch 3 report/source | Cáº§n browser viewport test |
| MOBILE-03 | Kitchen responsive | STATIC | NO viewport automation | NOT RUN | MANUAL REQUIRED | Batch 4 report/source | Cáº§n browser viewport test |

## 4. Static source conclusions

### Customer QR

- QR parser preserves `tableId` routing.
- Customer login preserves guest and registered customer token semantics.
- Digital Menu resolves table/branch before submit when table context exists.
- Options, toppings, quantity, notes and branch/table fields are present in the order payload.
- `isSubmittingOrder` prevents duplicate submit from the same frontend instance.

### POS and Kitchen

- POS sends orders and new items through the existing Order API/send-to-kitchen flow.
- `sentQuantity` is used to distinguish already-sent items from new item quantity.
- Kitchen receives `NewOrderRequest`, tracks active statuses and removes completed/cancelled requests.
- SignalR event names and `/kitchenHub` were not modified in the UI batches.
- Valid Kitchen transitions remain `Pending`, `Preparing`, `Ready`, `Completed`, with `Cancelled` as the cancellation path.

### Payment

- Source currently exposes cash and bank transfer/QR presentation in POS payment UI.
- No payment runtime request was issued in this phase.
- No claim is made that external bank/QR confirmation was verified.

## 5. Startup verification status

Frontend production builds pass, but full startup verification is not complete:

- Customer/Admin Vite servers were not used for browser E2E because the API database safety gate is unresolved.
- API was not started because startup performs migration/seed against the configured remote Supabase target.
- Therefore â€œAPI startsâ€, â€œno runtime exceptionâ€, and â€œfull flow worksâ€ remain **MANUAL VERIFICATION REQUIRED**.

## 6. Mobile manual checklist

| Viewport | Customer QR | POS | Kitchen | Status |
|---|---|---|---|---|
| 390Ă—844 | QR context, categories, product, modal, cart, submit | Product/cart/actions | Cards/options/status action | MANUAL REQUIRED |
| 430Ă—932 | Same flow, larger mobile layout | Same flow | Same flow | MANUAL REQUIRED |
| 768Ă—1024 | Tablet menu/cart/modal | Product/cart split | KDS grid/action reachability | MANUAL REQUIRED |

## 7. Test data and cleanup

```text
TEST DATA CREATED: NONE
```

No order, payment, table status or kitchen request was created/updated by this verification phase. No cleanup was necessary.

## 8. Blockers and human actions required

1. Confirm that the configured Supabase/PostgreSQL database is a disposable development/test database, not production/shared operational data.
2. Provide/activate a safe development runtime configuration and seed accounts/table/product fixtures without exposing values in documentation.
3. Run the browser flow with two clients: customer QR and admin/Kitchen, then verify SignalR event propagation.
4. Run payment only against a safe development order and document whether bank-transfer/QR confirmation is manual or external.

## Final summary

```text
DEVELOPMENT ENVIRONMENT:
BLOCKED

BACKEND TESTS:
177/177

ADMIN BUILD:
PASS

CUSTOMER BUILD:
PASS

QR FLOW:
MANUAL

CUSTOMER ORDER:
MANUAL

POS:
MANUAL

KITCHEN:
MANUAL

ADD-DISH FLOW:
MANUAL

SIGNALR:
MANUAL

PAYMENT:
MANUAL

TABLE STATUS:
MANUAL

MOBILE:
MANUAL

AUTH SEMANTICS:
PRESERVED

BUSINESS LOGIC:
PRESERVED

SOURCE CHANGES DURING VERIFICATION:
NONE

TEST DATA CREATED:
NONE

BLOCKERS:
1. Database target safety chÆ°a Ä‘Æ°á»£c xĂ¡c nháº­n lĂ  development/test.
2. API startup cĂ³ migration/seed side effects.
3. KhĂ´ng cĂ³ browser E2E automation trong repository.

HUMAN MANUAL TESTS REQUIRED:
1. Guest QR â†’ menu â†’ cart â†’ submit â†’ POS/Kitchen.
2. Kitchen status transitions vĂ  SignalR multi-client update.
3. Payment vĂ  mobile viewport checks trĂªn development environment an toĂ n.

RECOMMENDATION:
FIX RUNTIME ENVIRONMENT BLOCKERS FIRST
```


