# E2E / Runtime Business Flow Verification

## Scope and safety decision

Đây là verification phase sau UI/UX Batch 1–5. Không sửa source, không refactor, không tạo migration và không tạo dữ liệu nghiệp vụ.

Runtime mutation bị **BLOCKED** trong môi trường hiện tại vì:

- API startup gọi `Database.Migrate()` và `DbInitializer.SeedAsync()`.
- Configuration có Supabase/PostgreSQL remote connection trong .NET User Secrets nhưng repository không chứng minh database target là development/test.
- Vì vậy không khởi động API, không login runtime, không submit order, không chuyển status và không thanh toán.

Không có secret value nào được ghi vào report.

## 0. Environment discovery

| Item | Status | Evidence/notes |
|---|---|---|
| API project | PRESENT | `services/api/RestaurantPOS.api.csproj` |
| API HTTPS port | PRESENT | `services/api/Program.cs`, `Properties/launchSettings.json`, port 5000 |
| HTTPS certificate | PRESENT | `.https/pos-cert.pfx` exists; password value không ghi nhận |
| PostgreSQL/Supabase connection configuration | PRESENT | `ConnectionStrings:DefaultConnection` user-secret key exists; target safety chưa xác nhận |
| JWT secret configuration | PRESENT | `Jwt:Secret` user-secret key exists; value không ghi nhận |
| Gemini API key | MISSING/NOT REQUIRED | Không cần cho QR/POS/Kitchen flow; AI không phải blocker của flow này |
| Development seed password | UNKNOWN/MISSING | Không tìm thấy key seed trong user-secret inventory; seed chỉ chạy khi database chưa có employee |
| Admin frontend | PRESENT | Vite port 5173, HTTPS config trong `vite.config.ts` |
| Customer frontend | PRESENT | Vite port 5174, runtime API host config trong `src/config.ts` |
| CORS | PRESENT | Development CORS giới hạn HTTPS origin port 5173/5174 |
| SignalR endpoint | PRESENT | `/kitchenHub`; JWT query token support trong `Program.cs` |
| Confirmed safe development database | MISSING | Không có bằng chứng đủ để cho phép mutation |

## 1. Baseline

Baseline chạy đúng project:

| Check | Result |
|---|---|
| `dotnet build services/api/RestaurantPOS.api.csproj --no-restore` | PASS |
| `dotnet test tests/RestaurantPOS.Tests/RestaurantPOS.Tests.csproj --no-restore` | PASS — 177/177 |
| `npm run build` trong `apps/admin-web` | PASS — existing large-bundle warning |
| `npm run build` trong `apps/customer-web` | PASS |

Working tree có nhiều pre-existing changes từ các phase trước. Không restore/overwrite và không có source change trong verification phase này.

## 2. Test capability discovery

Repository có backend automated tests dùng EF/test fixtures, gồm `KitchenServiceTests`, `OrderServiceTests`, `JwtAuthenticationTests`, `HardeningIntegrationTests` và các test security/business khác.

Không tìm thấy Playwright, Cypress, Selenium, WebApplicationFactory-based browser E2E, Postman/Newman collection hoặc browser automation script phù hợp để reuse. Không cài thêm framework trong phase này.

Phân loại:

- **AUTOMATED VERIFIED**: build/test suite backend hiện có.
- **STATIC VERIFIED**: source trace, API contract và frontend flow.
- **RUNTIME VERIFIED**: chưa đạt vì database safety gate chặn API startup/mutation.
- **MANUAL VERIFICATION REQUIRED**: browser, camera QR, realtime multi-client, payment và responsive viewport.

## 3. Business-flow verification matrix

| ID | Flow | Static | Automated | Runtime | Result | Evidence | Notes |
|---|---|---|---|---|---|---|---|
| AUTH-01 | Staff login | VERIFIED | PARTIAL | NOT RUN | MANUAL REQUIRED | `AuthController`, `CustomerLogin`, JWT tests | Không login runtime trên remote DB |
| AUTH-02 | Guest QR token | VERIFIED | PARTIAL | NOT RUN | MANUAL REQUIRED | `CustomerLogin.tsx`, `/api/Auth/customer-token` | Guest không yêu cầu password |
| QR-01 | QR resolution | VERIFIED | NO browser E2E | NOT RUN | MANUAL REQUIRED | `QRScan.tsx`, `DigitalMenu.tsx` | Cần camera/valid table thật |
| MENU-01 | Menu loading | VERIFIED | NO E2E | NOT RUN | MANUAL REQUIRED | Product/Topping fetch trong `DigitalMenu.tsx` | Cần API/database an toàn |
| CART-01 | Cart operations | VERIFIED | NO frontend E2E | NOT RUN | MANUAL REQUIRED | `DigitalMenu.tsx` | Static payload/options trace pass |
| ORDER-01 | Submit customer order | VERIFIED | Backend order tests only | NOT RUN | MANUAL REQUIRED | `POST /api/Order`, `confirmOrder` | Không tạo mutation do DB gate |
| ORDER-02 | POS/order visibility | VERIFIED | Backend tests partial | NOT RUN | MANUAL REQUIRED | POS order fetch/active order flow | Chưa có multi-client runtime |
| KITCHEN-01 | Kitchen receives order | VERIFIED | `KitchenServiceTests` partial | NOT RUN | MANUAL REQUIRED | `KitchenService`, `KitchenPage`, `NewOrderRequest` | Cần customer/POS order thật |
| KITCHEN-02 | Kitchen status transition | VERIFIED | Kitchen/service tests partial | NOT RUN | MANUAL REQUIRED | Pending → Preparing → Ready → Completed | Không mutate shared DB |
| POS-01 | Existing order update | VERIFIED | Order tests partial | NOT RUN | MANUAL REQUIRED | `POSPage` `sentQuantity`/existing order flow | Cần xác nhận runtime delta |
| POS-02 | Add dish | VERIFIED | Backend tests partial | NOT RUN | MANUAL REQUIRED | POS send-to-kitchen flow | Chưa runtime verify item delta |
| REALTIME-01 | SignalR update | VERIFIED | SignalR/backend tests partial | NOT RUN | MANUAL REQUIRED | `/kitchenHub`, event names in source | Cần hai browser clients |
| PAY-01 | Payment | VERIFIED | Payment/backend tests partial | NOT RUN | MANUAL REQUIRED | `Cash`/`Chuyển khoản` source flow | Không tạo payment mutation |
| TABLE-01 | Table status | VERIFIED | Backend tests partial | NOT RUN | MANUAL REQUIRED | DigitalMenu table status PATCH, POS status flow | Cần development DB an toàn |
| NEG-01 | Invalid QR | VERIFIED | NO frontend E2E | NOT RUN | MANUAL REQUIRED | DigitalMenu controlled feedback | Cần route/table runtime |
| NEG-02 | Duplicate/invalid submit | VERIFIED | Static guard only | NOT RUN | MANUAL REQUIRED | `isSubmittingOrder` guard | Chưa test browser double-tap |
| MOBILE-01 | Customer mobile | STATIC | NO viewport automation | NOT RUN | MANUAL REQUIRED | Batch 5 responsive source | Cần 390/430/768 runtime |
| MOBILE-02 | POS responsive | STATIC | NO viewport automation | NOT RUN | MANUAL REQUIRED | Batch 3 report/source | Cần browser viewport test |
| MOBILE-03 | Kitchen responsive | STATIC | NO viewport automation | NOT RUN | MANUAL REQUIRED | Batch 4 report/source | Cần browser viewport test |

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
- Therefore “API starts”, “no runtime exception”, and “full flow works” remain **MANUAL VERIFICATION REQUIRED**.

## 6. Mobile manual checklist

| Viewport | Customer QR | POS | Kitchen | Status |
|---|---|---|---|---|
| 390×844 | QR context, categories, product, modal, cart, submit | Product/cart/actions | Cards/options/status action | MANUAL REQUIRED |
| 430×932 | Same flow, larger mobile layout | Same flow | Same flow | MANUAL REQUIRED |
| 768×1024 | Tablet menu/cart/modal | Product/cart split | KDS grid/action reachability | MANUAL REQUIRED |

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
1. Database target safety chưa được xác nhận là development/test.
2. API startup có migration/seed side effects.
3. Không có browser E2E automation trong repository.

HUMAN MANUAL TESTS REQUIRED:
1. Guest QR → menu → cart → submit → POS/Kitchen.
2. Kitchen status transitions và SignalR multi-client update.
3. Payment và mobile viewport checks trên development environment an toàn.

RECOMMENDATION:
FIX RUNTIME ENVIRONMENT BLOCKERS FIRST
```
