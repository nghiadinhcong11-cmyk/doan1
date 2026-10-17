# THESIS SOURCE-OF-TRUTH AUDIT

## 0. PhĂ¡ÂºÂ¡m vi, phĂ†Â°Ă†Â¡ng phÄ‚Â¡p vÄ‚Â  trĂ¡ÂºÂ¡ng thÄ‚Â¡i ban Ă„â€˜Ă¡ÂºÂ§u

**Repository:** `D:\Dev\doan`  
**NgÄ‚Â y audit:** 2026-10-03 (Asia/Saigon)  
**LoĂ¡ÂºÂ¡i audit:** READ-ONLY SOURCE CODE AUDIT  
**MĂ¡Â»Â¥c tiÄ‚Âªu:** cung cĂ¡ÂºÂ¥p source of truth cho viĂ¡Â»â€¡c hiĂ¡Â»â€¡u chĂ¡Â»â€°nh ChĂ†Â°Ă†Â¡ng 3 vÄ‚Â  ChĂ†Â°Ă†Â¡ng 4 cĂ¡Â»Â§a khÄ‚Â³a luĂ¡ÂºÂ­n.

Audit nÄ‚Â y chĂ¡Â»â€° quan sÄ‚Â¡t repository hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i. KhÄ‚Â´ng sĂ¡Â»Â­a source code, khÄ‚Â´ng refactor, khÄ‚Â´ng Ä‚Â¡p dĂ¡Â»Â¥ng migration, khÄ‚Â´ng sĂ¡Â»Â­a database vÄ‚Â  khÄ‚Â´ng tĂ¡Â»Â± Ă„â€˜Ă¡Â»â„¢ng Ä‚Â¡p dĂ¡Â»Â¥ng cÄ‚Â¡c Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t trong bÄ‚Â¡o cÄ‚Â¡o.

### TrĂ¡ÂºÂ¡ng thÄ‚Â¡i working tree tĂ¡ÂºÂ¡i thĂ¡Â»Âi Ă„â€˜iĂ¡Â»Æ’m bĂ¡ÂºÂ¯t Ă„â€˜Ă¡ÂºÂ§u

Working tree Ă„â€˜Ä‚Â£ cÄ‚Â³ cÄ‚Â¡c thay Ă„â€˜Ă¡Â»â€¢i/artefact tĂ¡Â»Â« trĂ†Â°Ă¡Â»â€ºc khi audit bĂ¡ÂºÂ¯t Ă„â€˜Ă¡ÂºÂ§u:

```text
 M apps/customer-web/src/App.tsx
 M apps/customer-web/src/pages/DigitalMenu.tsx
 M docs/system-audit/52-GUEST-QR-SESSION-SECURITY.md
 M docs/system-audit/CODEX-CURRENT-STATE.md
?? baocaodoan/UML_Activity_Sequence_Audit.md
?? baocaodoan/use-case/
?? docs/system-audit/54-TOPPING-SCHEMA-DRIFT-REPAIR.md
?? services/Migrations_backup_20260919/
?? services/api/src/Infrastructure/Persistence/Migrations/20261002140936_AddToppingType.Designer.cs
?? services/api/src/Infrastructure/Persistence/Migrations/20261002140936_AddToppingType.cs
```

CÄ‚Â¡c mĂ¡Â»Â¥c trÄ‚Âªn Ă„â€˜Ă†Â°Ă¡Â»Â£c xem lÄ‚Â  thay Ă„â€˜Ă¡Â»â€¢i cÄ‚Â³ sĂ¡ÂºÂµn cĂ¡Â»Â§a ngĂ†Â°Ă¡Â»Âi dÄ‚Â¹ng vÄ‚Â  khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c audit nÄ‚Â y hoÄ‚Â n tÄ‚Â¡c hay chĂ¡Â»â€°nh sĂ¡Â»Â­a. File duy nhĂ¡ÂºÂ¥t Ă„â€˜Ă†Â°Ă¡Â»Â£c tĂ¡ÂºÂ¡o bĂ¡Â»Å¸i audit nÄ‚Â y lÄ‚Â  file hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i.

### Quy tĂ¡ÂºÂ¯c trĂ¡ÂºÂ¡ng thÄ‚Â¡i

- **VERIFIED:** cÄ‚Â³ bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p tĂ¡Â»Â« executable source/runtime configuration/test hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i.
- **PARTIALLY VERIFIED:** source chĂ¡Â»Â©ng minh mĂ¡Â»â„¢t phĂ¡ÂºÂ§n nhĂ†Â°ng khÄ‚Â´ng Ă„â€˜Ă¡Â»Â§ Ă„â€˜Ă¡Â»Æ’ khĂ¡ÂºÂ³ng Ă„â€˜Ă¡Â»â€¹nh toÄ‚Â n bĂ¡Â»â„¢ claim.
- **NOT VERIFIED:** khÄ‚Â´ng tÄ‚Â¬m thĂ¡ÂºÂ¥y execution path/test/artifact Ă„â€˜Ă¡Â»Â§ chĂ¡Â»Â©ng minh.
- **FALSE / OUTDATED:** source hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i chĂ¡Â»Â©ng minh claim ngĂ†Â°Ă¡Â»Â£c lĂ¡ÂºÂ¡i hoĂ¡ÂºÂ·c claim thuĂ¡Â»â„¢c trĂ¡ÂºÂ¡ng thÄ‚Â¡i cĂ…Â©.
- **IMPLEMENTED BUT DIFFERENT:** cÄ‚Â³ capability tĂ†Â°Ă†Â¡ng Ă¡Â»Â©ng nhĂ†Â°ng cÄ‚Â¡ch triĂ¡Â»Æ’n khai khÄ‚Â¡c mÄ‚Â´ tĂ¡ÂºÂ£.
- **MISSING FROM THESIS:** source cÄ‚Â³ nhĂ†Â°ng chĂ†Â°a tÄ‚Â¬m thĂ¡ÂºÂ¥y nĂ¡Â»â„¢i dung tĂ†Â°Ă†Â¡ng Ă¡Â»Â©ng trong tÄ‚Â i liĂ¡Â»â€¡u khÄ‚Â³a luĂ¡ÂºÂ­n Ă„â€˜Ă†Â°Ă¡Â»Â£c kiĂ¡Â»Æ’m tra.

---

## 1. KĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n Ă„â€˜iĂ¡Â»Âu hÄ‚Â nh

Repository hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i lÄ‚Â  mĂ¡Â»â„¢t hĂ¡Â»â€¡ thĂ¡Â»â€˜ng POS/restaurant management cÄ‚Â³ backend ASP.NET Core 8, hai frontend React/Vite vÄ‚Â  PostgreSQL qua EF Core/Npgsql. Backend cÄ‚Â³ cÄ‚Â¡c module order/POS, customer QR, KDS/SignalR, nhÄ‚Â¢n sĂ¡Â»Â±, chi nhÄ‚Â¡nh, loyalty, inventory, analytics/business insight vÄ‚Â  AI tool calling.

CÄ‚Â¡c kĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n chĂ¡ÂºÂ¯c chĂ¡ÂºÂ¯n nhĂ¡ÂºÂ¥t:

1. **VERIFIED:** cĂ¡ÂºÂ¥u hÄ‚Â¬nh runtime vÄ‚Â  source cÄ‚Â³ API ASP.NET Core 8, EF Core 8, Npgsql/PostgreSQL, JWT Bearer, SignalR `/kitchenHub`, React/TypeScript/Vite cho Admin Web vÄ‚Â  Customer Web.
2. **VERIFIED:** current EF model dÄ‚Â¹ng `OrderDetail`, `RestaurantTable`, `OrderRequest`/`OrderRequestItem`; khÄ‚Â´ng cÄ‚Â³ current `Invoice` entity, `Restaurant` entity hoĂ¡ÂºÂ·c `OrderItem` entity.
3. **VERIFIED:** payment kiĂ¡Â»Æ’m tra sĂ¡Â»â€˜ tiĂ¡Â»Ân request bĂ¡ÂºÂ±ng `Order.TotalAmount` persisted vÄ‚Â  dÄ‚Â¹ng conditional update Ă„â€˜Ă¡Â»Æ’ chĂ¡Â»â€˜ng thanh toÄ‚Â¡n trÄ‚Â¹ng/Ă„â€˜ua trĂ¡ÂºÂ¡ng thÄ‚Â¡i.
4. **VERIFIED:** QR guest dÄ‚Â¹ng JWT cÄ‚Â³ `customerSessionType=guest`, `tableId` vÄ‚Â  `branchId`; guest bĂ¡Â»â€¹ chĂ¡ÂºÂ·n khĂ¡Â»Âi AI bĂ¡ÂºÂ±ng `AiController.Chat`.
5. **VERIFIED:** AI cÄ‚Â³ registry DI gĂ¡Â»â€œm 14 tool (8 Admin, 3 Employee, 3 Customer); tool Ă„â€˜Ă†Â°Ă¡Â»Â£c lĂ¡Â»Âc theo role, kiĂ¡Â»Æ’m tra permission/risk rĂ¡Â»â€œi gĂ¡Â»Âi Application Service; vÄ‚Â²ng lĂ¡ÂºÂ·p tool calling tĂ¡Â»â€˜i Ă„â€˜a 5 lĂ†Â°Ă¡Â»Â£t, xĂ¡Â»Â­ lÄ‚Â½ tuĂ¡ÂºÂ§n tĂ¡Â»Â± theo tĂ¡Â»Â«ng function call.
6. **PARTIALLY VERIFIED:** branch isolation Ă„â€˜Ă†Â°Ă¡Â»Â£c kiĂ¡Â»Æ’m tra Ă¡Â»Å¸ nhiĂ¡Â»Âu controller/service vÄ‚Â  cÄ‚Â³ test hardening, nhĂ†Â°ng Admin Ă„â€˜Ă†Â°Ă¡Â»Â£c thÄ‚Âªm vÄ‚Â o nhÄ‚Â³m SignalR global vÄ‚Â  payment phÄ‚Â¡t `PaymentCompleted` qua `Clients.All`; vÄ‚Â¬ vĂ¡ÂºÂ­y khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c mÄ‚Â´ tĂ¡ÂºÂ£ lÄ‚Â  cÄ‚Â´ lĂ¡ÂºÂ­p tuyĂ¡Â»â€¡t Ă„â€˜Ă¡Â»â€˜i Ă¡Â»Å¸ mĂ¡Â»Âi event.
7. **IMPLEMENTED BUT DIFFERENT:** KDS status thuĂ¡Â»â„¢c `OrderRequest`, khÄ‚Â´ng phĂ¡ÂºÂ£i trĂ¡ÂºÂ¡ng thÄ‚Â¡i kitchen Ă„â€˜Ă¡Â»â„¢c lĂ¡ÂºÂ­p trÄ‚Âªn tĂ¡Â»Â«ng `OrderDetail`; `SentQuantity` lÄ‚Â  sĂ¡Â»â€˜ lĂ†Â°Ă¡Â»Â£ng Ă„â€˜Ä‚Â£ gĂ¡Â»Â­i bĂ¡ÂºÂ¿p Ă„â€˜Ă¡Â»Æ’ tÄ‚Â­nh delta.
8. **FALSE / OUTDATED:** khÄ‚Â´ng cÄ‚Â³ current `Invoice` persistence flow khi thanh toÄ‚Â¡n; `InvoiceCode` lÄ‚Â  field cĂ¡Â»Â§a `Order`. KhÄ‚Â´ng tÄ‚Â¬m thĂ¡ÂºÂ¥y payment gateway/webhook server-side.
9. **VERIFIED:** test suite hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i chĂ¡ÂºÂ¡y thÄ‚Â nh cÄ‚Â´ng vĂ¡Â»â€ºi **Total 267 / Passed 267 / Failed 0 / Skipped 0**.
10. **NOT VERIFIED:** khÄ‚Â´ng tÄ‚Â¬m thĂ¡ÂºÂ¥y dataset/evaluation artifact Ă„â€˜Ă¡Â»â€¹nh lĂ†Â°Ă¡Â»Â£ng cho Tool Selection Accuracy, Security Compliance hoĂ¡ÂºÂ·c No-Tool Accuracy.

---

## 2. Inventory repository vÄ‚Â  kiĂ¡ÂºÂ¿n trÄ‚Âºc thĂ¡Â»Â±c tĂ¡ÂºÂ¿

### 2.1 CĂ¡ÂºÂ¥u trÄ‚Âºc hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i

```text
apps/
  admin-web/       React + TypeScript + Vite; Admin, POS, KDS, management
  customer-web/    React + TypeScript + Vite; QR/customer/reservation
packages/
  shared/          shared TypeScript types, swagger, formatters
services/
  api/             mĂ¡Â»â„¢t ASP.NET Core Web SDK project
    src/AI/
    src/Application/
    src/Domain/
    src/Infrastructure/
    src/WebAPI/
scripts/
  diagnostics/     cÄ‚Â¡c utility/E2E/validation project riÄ‚Âªng
tests/
  RestaurantPOS.Tests/
baocaodoan/        tÄ‚Â i liĂ¡Â»â€¡u mÄ‚Â´ tĂ¡ÂºÂ£ hĂ¡Â»â€¡ thĂ¡Â»â€˜ng/khÄ‚Â³a luĂ¡ÂºÂ­n hiĂ¡Â»â€¡n cÄ‚Â³
docs/system-audit/ lĂ¡Â»â€¹ch sĂ¡Â»Â­ audit/hardening
```

### 2.2 Project/application thĂ¡Â»Â±c tĂ¡ÂºÂ¿

| ThÄ‚Â nh phĂ¡ÂºÂ§n | BĂ¡ÂºÂ±ng chĂ¡Â»Â©ng | TrĂ¡ÂºÂ¡ng thÄ‚Â¡i |
|---|---|---|
| Backend/API | `services/api/RestaurantPOS.api.csproj`, `Program.cs` | VERIFIED |
| Admin/Management Web | `apps/admin-web/package.json`, `src/features/settings`, `operations`, `analytics` | VERIFIED |
| POS | `apps/admin-web/src/features/pos/pages/POSPage.tsx` | VERIFIED; nĂ¡ÂºÂ±m trong Admin Web |
| KDS | `apps/admin-web/src/features/kitchen/pages/KitchenPage.tsx`, `KitchenHistoryPage.tsx` | VERIFIED; khÄ‚Â´ng phĂ¡ÂºÂ£i app frontend riÄ‚Âªng |
| Customer QR Web | `apps/customer-web/src/pages/QRScan.tsx`, `DigitalMenu.tsx`, `App.tsx` | VERIFIED |
| Mobile app riÄ‚Âªng | khÄ‚Â´ng tÄ‚Â¬m thĂ¡ÂºÂ¥y Flutter/Capacitor application trong current inventory | NOT VERIFIED |
| Shared package | `packages/shared/index.ts`, `types/api.ts`, `types/models.ts` | VERIFIED |
| Test project | `tests/RestaurantPOS.Tests/RestaurantPOS.Tests.csproj` | VERIFIED |
| Diagnostic projects | `scripts/diagnostics/*/*.csproj` | VERIFIED; khÄ‚Â´ng phĂ¡ÂºÂ£i production application |

KhÄ‚Â´ng tÄ‚Â¬m thĂ¡ÂºÂ¥y `Dockerfile` hoĂ¡ÂºÂ·c `docker-compose.yml/yaml` trong current repository. VÄ‚Â¬ vĂ¡ÂºÂ­y deployment bĂ¡ÂºÂ±ng Docker lÄ‚Â  **NOT VERIFIED** vÄ‚Â  khÄ‚Â´ng nÄ‚Âªn mÄ‚Â´ tĂ¡ÂºÂ£ lÄ‚Â  deployment hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i.

### 2.3 Framework/version thĂ¡Â»Â±c tĂ¡ÂºÂ¿

| CÄ‚Â´ng nghĂ¡Â»â€¡ | Evidence | TrĂ¡ÂºÂ¡ng thÄ‚Â¡i |
|---|---|---|
| .NET | `TargetFramework net8.0` trong `RestaurantPOS.api.csproj` | VERIFIED |
| ASP.NET Core | `Microsoft.NET.Sdk.Web`, Controllers, Middleware trong `Program.cs` | VERIFIED |
| EF Core | package `Microsoft.EntityFrameworkCore` 8.0.0; `ApplicationDbContext` | VERIFIED |
| PostgreSQL/Npgsql | package Npgsql EF provider; `UseNpgsql`; Supabase connection configuration | VERIFIED |
| React | React 18.2.0 trong hai frontend | VERIFIED |
| TypeScript | TypeScript 5.x package, `tsconfig.json`, `.tsx/.ts` | VERIFIED |
| Vite | Vite 4.x package vÄ‚Â  build output | VERIFIED |
| SignalR | server `AddSignalR`, `KitchenHub`; client `@microsoft/signalr` 10.0.11 | VERIFIED |
| AI SDK | custom `AI provider implementation` qua `HttpClient`; khÄ‚Â´ng thĂ¡ÂºÂ¥y SDK AI provider bĂ¡ÂºÂ¯t buĂ¡Â»â„¢c trong csproj | VERIFIED; implementation khÄ‚Â¡c claim Ă¢â‚¬Å“SDKĂ¢â‚¬Â nĂ¡ÂºÂ¿u thesis dÄ‚Â¹ng claim Ă„â€˜Ä‚Â³ |
| Test framework | xUnit attributes/assertions vÄ‚Â  `dotnet test` | VERIFIED |

### 2.4 Dependency direction / Clean Architecture

Folder structure cÄ‚Â³ `Domain`, `Application`, `Infrastructure`, `WebAPI` vÄ‚Â  `AI`. Domain entities khÄ‚Â´ng tham chiĂ¡ÂºÂ¿u ASP.NET/EF trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p trong cÄ‚Â¡c file Ă„â€˜Ă†Â°Ă¡Â»Â£c kiĂ¡Â»Æ’m tra. Application services tham chiĂ¡ÂºÂ¿u `ApplicationDbContext` trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p Ă¡Â»Å¸ nhiĂ¡Â»Âu service, vÄ‚Â­ dĂ¡Â»Â¥ `OrderService`, `KitchenService`, `LoyaltyService`, nÄ‚Âªn persistence abstraction chĂ†Â°a Ă„â€˜Ă†Â°Ă¡Â»Â£c tÄ‚Â¡ch hoÄ‚Â n toÄ‚Â n khĂ¡Â»Âi Application.

`Program.cs` lÄ‚Â  composition root vÄ‚Â  tham chiĂ¡ÂºÂ¿u Ă„â€˜Ă¡Â»â€œng thĂ¡Â»Âi `Infrastructure.Persistence`, `Application.Services`, `AI.Services`, `WebAPI.Hubs`. VÄ‚Â¬ vĂ¡ÂºÂ­y:

**KĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n: PARTIALLY VERIFIED / IMPLEMENTED BUT DIFFERENT.** Repository cÄ‚Â³ cÄ‚Â¡c layer vÄ‚Â  hĂ†Â°Ă¡Â»â€ºng tĂ¡Â»â€¢ chĂ¡Â»Â©c gĂ¡ÂºÂ§n Clean Architecture, nhĂ†Â°ng source hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i khÄ‚Â´ng chĂ¡Â»Â©ng minh mĂ¡Â»â„¢t Clean Architecture thuĂ¡ÂºÂ§n vĂ¡Â»â€ºi dependency direction hoÄ‚Â n toÄ‚Â n Ă„â€˜Ă¡Â»â„¢c lĂ¡ÂºÂ­p giĂ¡Â»Â¯a Application vÄ‚Â  Infrastructure. Thesis nÄ‚Âªn dÄ‚Â¹ng Ă¢â‚¬Å“kiĂ¡ÂºÂ¿n trÄ‚Âºc phÄ‚Â¢n lĂ¡Â»â€ºp theo Ă„â€˜Ă¡Â»â€¹nh hĂ†Â°Ă¡Â»â€ºng Clean ArchitectureĂ¢â‚¬Â nĂ¡ÂºÂ¿u muĂ¡Â»â€˜n chÄ‚Â­nh xÄ‚Â¡c.

### 2.5 Monorepo

MĂ¡Â»â„¢t repository chĂ¡Â»Â©a `apps`, `packages`, `services`, `tests`, `scripts`, lockfiles vÄ‚Â  shared types.  
**MONOREPO: VERIFIED** theo nghĂ„Â©a tĂ¡Â»â€¢ chĂ¡Â»Â©c source; khÄ‚Â´ng tÄ‚Â¬m thĂ¡ÂºÂ¥y workspace orchestration phĂ¡Â»Â©c tĂ¡ÂºÂ¡p bĂ¡ÂºÂ¯t buĂ¡Â»â„¢c ngoÄ‚Â i cÄ‚Â¡c package/project hiĂ¡Â»â€¡n cÄ‚Â³.

---

## 3. Actor, role vÄ‚Â  authorization source of truth

### 3.1 Role thĂ¡Â»Â±c sĂ¡Â»Â± xuĂ¡ÂºÂ¥t hiĂ¡Â»â€¡n

CÄ‚Â¡c role executable Ă„â€˜Ă†Â°Ă¡Â»Â£c thĂ¡ÂºÂ¥y trong claims, `[Authorize(Roles=...)]`, AI tools vÄ‚Â  tests:

```text
admin, manager, employee, cashier, kitchen, customer
```

Guest QR khÄ‚Â´ng phĂ¡ÂºÂ£i role JWT riÄ‚Âªng. Guest dÄ‚Â¹ng role `customer` cÄ‚Â¹ng claim `customerSessionType=guest`, `tableId`, `branchId` vÄ‚Â  Ă„â€˜Ă†Â°Ă¡Â»Â£c nhĂ¡ÂºÂ­n diĂ¡Â»â€¡n bĂ¡Â»Å¸i `GuestSessionContext.IsGuest`.

Anonymous lÄ‚Â  trĂ¡ÂºÂ¡ng thÄ‚Â¡i unauthenticated, khÄ‚Â´ng phĂ¡ÂºÂ£i role Ă„â€˜Ă†Â°Ă¡Â»Â£c cĂ¡ÂºÂ¥p quyĂ¡Â»Ân chung. MĂ¡Â»â„¢t sĂ¡Â»â€˜ public endpoint Ă„â€˜Ă†Â°Ă¡Â»Â£c mĂ¡Â»Å¸ rÄ‚Âµ bĂ¡ÂºÂ±ng `[AllowAnonymous]` hoĂ¡ÂºÂ·c khÄ‚Â´ng yÄ‚Âªu cĂ¡ÂºÂ§u JWT.

### 3.2 BĂ¡ÂºÂ£ng capability/authorization

| Role/actor | Authentication | Branch scope | Capability Ă„â€˜Ă†Â°Ă¡Â»Â£c chĂ¡Â»Â©ng minh | HĂ¡ÂºÂ¡n chĂ¡ÂºÂ¿/evidence |
|---|---|---|---|---|
| Admin | JWT Bearer, employee identity kiĂ¡Â»Æ’m tra active trong `Program.cs` | Global/bypass Ă¡Â»Å¸ nhiĂ¡Â»Âu helper; vĂ¡ÂºÂ«n cÄ‚Â³ resource checks tÄ‚Â¹y endpoint | branch, employee, menu, order, analytics, settings, AI admin tools | KhÄ‚Â´ng nÄ‚Âªn diĂ¡Â»â€¦n Ă„â€˜Ă¡ÂºÂ¡t mĂ¡Â»Âi SignalR event lÄ‚Â  branch-isolated; `KitchenHub` thÄ‚Âªm `kitchen-admin`, `KitchenNotifier` gĂ¡Â»Â­i global admin |
| Manager | JWT Bearer + `branchId` claim | Branch-scoped | order, dashboard/analytics, employee/operations tÄ‚Â¹y controller, AI read tools | `AiPermissionService` tĂ¡Â»Â« chĂ¡Â»â€˜i manager khÄ‚Â´ng cÄ‚Â³ `BranchId`; controller/service kiĂ¡Â»Æ’m tra branch Ă¡Â»Å¸ nhiĂ¡Â»Âu flow |
| Employee | JWT Bearer + active employee | Branch-scoped theo claim/resource checks | POS/order status, table, kitchen-related endpoints tÄ‚Â¹y `[Authorize]`, mĂ¡Â»â„¢t sĂ¡Â»â€˜ AI read/write | KhÄ‚Â´ng cÄ‚Â³ bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng employee Ă„â€˜Ă†Â°Ă¡Â»Â£c toÄ‚Â n bĂ¡Â»â„¢ management capability |
| Cashier | JWT Bearer + active employee | Branch-scoped | POS orders, accept web order, payment, redeem, order status | KhÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c cĂ¡ÂºÂ¥p admin-only settings/AI admin write |
| Kitchen | JWT Bearer + active employee | Branch-scoped | active kitchen requests, request status, send-to-kitchen/status endpoint | KhÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c payment; KDS status lÄ‚Â  request-level |
| Customer registered | JWT Bearer, `customerSessionType=registered` hoĂ¡ÂºÂ·c khÄ‚Â´ng guest | Order ownership/customer phone; branch lĂ¡ÂºÂ¥y tĂ¡Â»Â« order/request | xem menu/order cĂ¡Â»Â§a mÄ‚Â¬nh, create order, reservation, customer AI tools | KhÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c staff payment/admin endpoints |
| Guest/Guest QR | JWT Bearer do `/api/guest/bootstrap` cĂ¡ÂºÂ¥p, role `customer`, session claim `guest` | BĂ¡ÂºÂ¯t buĂ¡Â»â„¢c table + branch trong signed token; query/order bĂ¡Â»â€¹ giĂ¡Â»â€ºi hĂ¡ÂºÂ¡n theo table | menu/QR order/order lookup trong session, reservation surface tÄ‚Â¹y public endpoint | `AiController.Chat` trĂ¡ÂºÂ£ `Forbid` cho guest; khÄ‚Â´ng phĂ¡ÂºÂ£i actor hierarchy riÄ‚Âªng |
| Anonymous/public | khÄ‚Â´ng cÄ‚Â³ JWT | khÄ‚Â´ng cÄ‚Â³ user branch context | health, auth/public bootstrap vÄ‚Â  cÄ‚Â¡c endpoint Ă„â€˜Ă†Â°Ă¡Â»Â£c `[AllowAnonymous]` | KhÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c suy diĂ¡Â»â€¦n thÄ‚Â nh Customer capability |

### 3.3 JWT vÄ‚Â  guest bootstrap

- `services/api/src/Infrastructure/Services/JwtService.cs`: tĂ¡ÂºÂ¡o `ClaimTypes.Role`, `NameIdentifier`, `branchId`, `branchName`, `customerSessionType`, `tableId`.
- `services/api/src/WebAPI/Controllers/GuestController.cs`: bootstrap guest tĂ¡Â»Â« QR token vÄ‚Â  phÄ‚Â¡t session cÄ‚Â³ `customerSessionType=guest`.
- `services/api/src/Application/Common/Security/GuestSessionContext.cs`: kiĂ¡Â»Æ’m tra role customer, session type guest, `tableId`, `branchId`.
- `Program.cs` `OnTokenValidated`: employee phĂ¡ÂºÂ£i tĂ¡Â»â€œn tĂ¡ÂºÂ¡i vÄ‚Â  active; customer registered phĂ¡ÂºÂ£i tĂ¡Â»â€œn tĂ¡ÂºÂ¡i/active; guest Ă„â€˜Ă†Â°Ă¡Â»Â£c miĂ¡Â»â€¦n lookup customer theo session type.

**Guest QR cÄ‚Â³ gĂ¡Â»Âi AI khÄ‚Â´ng? FALSE / OUTDATED nĂ¡ÂºÂ¿u thesis mÄ‚Â´ tĂ¡ÂºÂ£ cÄ‚Â³.** `AiController` cÄ‚Â³ `[Authorize]` nhĂ†Â°ng chĂ¡ÂºÂ·n customer token cÄ‚Â³ `customerSessionType=guest` bĂ¡ÂºÂ±ng `Forbid`.

### 3.4 Actor generalization

KhÄ‚Â´ng an toÄ‚Â n Ă„â€˜Ă¡Â»Æ’ vĂ¡ÂºÂ½ `Admin -> Manager -> Employee` chĂ¡Â»â€° vÄ‚Â¬ cĂ¡ÂºÂ¥p bĂ¡ÂºÂ­c tĂ¡Â»â€¢ chĂ¡Â»Â©c. Source khÄ‚Â´ng chĂ¡Â»Â©ng minh kĂ¡ÂºÂ¿ thĂ¡Â»Â«a quyĂ¡Â»Ân theo hierarchy UML.

Generalization/capability abstraction cÄ‚Â³ thĂ¡Â»Æ’ dÄ‚Â¹ng cÄ‚Â³ Ă„â€˜iĂ¡Â»Âu kiĂ¡Â»â€¡n:

| Parent capability actor | Children | Capability chung | Status |
|---|---|---|---|
| Authenticated Staff User | admin, manager, employee, cashier, kitchen | JWT employee identity, active-account validation, staff authenticated API surface | PARTIALLY VERIFIED; cĂ¡ÂºÂ§n giĂ¡Â»Â¯ cÄ‚Â¡c role guards riÄ‚Âªng |
| Order Operations User | admin, manager, employee, cashier | mĂ¡Â»â„¢t phĂ¡ÂºÂ§n order query/mutation/status | PARTIALLY VERIFIED; endpoint quyĂ¡Â»Ân khÄ‚Â´ng Ă„â€˜Ă¡Â»â€œng nhĂ¡ÂºÂ¥t hoÄ‚Â n toÄ‚Â n |
| Kitchen User | admin, manager, kitchen vÄ‚Â  mĂ¡Â»â„¢t sĂ¡Â»â€˜ staff endpoint | nhĂ¡ÂºÂ­n/cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t kitchen request | PARTIALLY VERIFIED; khÄ‚Â´ng dÄ‚Â¹ng Ă„â€˜Ă¡Â»Æ’ suy ra quyĂ¡Â»Ân payment |
| Customer User | registered customer, guest session | customer-facing order/menu context | PARTIALLY VERIFIED; guest bĂ¡Â»â€¹ hĂ¡ÂºÂ¡n chĂ¡ÂºÂ¿ hĂ†Â¡n registered customer |
| Management User | admin, manager | analytics/settings/management capabilities | PARTIALLY VERIFIED; admin global, manager branch-scoped |

KhuyĂ¡ÂºÂ¿n nghĂ¡Â»â€¹ cho Overview Use Case: dÄ‚Â¹ng actor abstraction nhĂ¡ÂºÂ¹ cho layout, nhĂ†Â°ng ghi role/permission Ă¡Â»Å¸ cÄ‚Â¡c use case nhĂ¡ÂºÂ¡y cĂ¡ÂºÂ£m; khÄ‚Â´ng vĂ¡ÂºÂ½ hierarchy quyĂ¡Â»Ân ngĂ¡ÂºÂ§m Ă„â€˜Ă¡Â»â€¹nh.

---

## 4. Use Case Source of Truth

### 4.1 Customer/Guest

| Use case | Actor | Endpoint/controller | Service/entity | Scope | Status |
|---|---|---|---|---|---|
| QR bootstrap/session | Guest QR | `POST /api/guest/bootstrap`, `GuestController` | `QrTokenGenerator`, `JwtService`, `RestaurantTable` | table + branch tĂ¡Â»Â« token | VERIFIED |
| Xem menu | Guest/customer/staff tÄ‚Â¹y endpoint | `ProductController`; AI `customer_get_menu` | `Product`, `Topping`, `ProductService` | public/branch tÄ‚Â¹y action | VERIFIED |
| TĂ¡ÂºÂ¡o order QR | Guest/customer/staff | `POST /api/Order`, `OrderController.CreateOrder` | `OrderService`, `Order`, `OrderDetail` | guest token Ä‚Â©p table/branch | VERIFIED |
| Xem order | Customer/guest | `GET /api/Order/{id}`, `OrderController.GetOrder` | `Order` | ownership/session hoĂ¡ÂºÂ·c branch | VERIFIED |
| Reservation | customer/staff/public surfaces | `ReservationController`, `ReservationService` | `Reservation`, `RestaurantTable` | branch validation | VERIFIED/PARTIALLY VERIFIED tÄ‚Â¹y public path |
| Customer login/profile | registered customer | `AuthController`, `CustomerController` | `Customer` | identity ownership | VERIFIED |
| Loyalty xem/redeem | customer xem own; staff redeem endpoint | `OrderController` loyalty/redeem, `LoyaltyService` | `Customer`, `LoyaltyTransaction`, `Order` | own order hoĂ¡ÂºÂ·c branch staff | VERIFIED |
| AI customer | registered customer | `POST /api/Ai/chat` | Customer AI tools | guest forbidden | VERIFIED |

### 4.2 POS/staff

| Use case | Actor | Evidence | Status |
|---|---|---|---|
| ChĂ¡Â»Ân bÄ‚Â n/hiĂ¡Â»Æ’n thĂ¡Â»â€¹ trĂ¡ÂºÂ¡ng thÄ‚Â¡i | staff/POS | `TableController`, `TableService`, `POSPage.tsx`, `TableStatusPage.tsx` | VERIFIED |
| TĂ¡ÂºÂ¡o/cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t order | staff/customer/guest tÄ‚Â¹y claims | `OrderController.CreateOrder` Ă¢â€ â€™ `OrderService.CreateOrUpdateOrderAsync` | VERIFIED |
| TiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n web order | admin/manager/employee/cashier | `POST /api/Order/{id}/accept` Ă¢â€ â€™ `AcceptWebOrderAsync` | VERIFIED |
| GĂ¡Â»Â­i bĂ¡ÂºÂ¿p | authorized staff/kitchen | `POST /api/Order/{id}/send-to-kitchen` Ă¢â€ â€™ `KitchenService.SendToKitchenAsync` | VERIFIED |
| Thanh toÄ‚Â¡n | admin/manager/employee/cashier | `POST /api/Order/{id}/payment` Ă¢â€ â€™ `OrderService.PayOrderAsync` | VERIFIED |
| Redeem loyalty | staff payment roles | `POST /api/Order/{id}/redeem` Ă¢â€ â€™ `LoyaltyService.RedeemPointsAsync` | VERIFIED |
| Invoice/history | staff | `InvoiceController`, `InvoiceHistory.tsx`; order has `InvoiceCode` | PARTIALLY VERIFIED; no Invoice entity/payment-created invoice entity |
| Cancel/complete status | staff/kitchen | `PATCH /api/Order/{id}/status` | VERIFIED |

### 4.3 KDS

| Use case | Evidence | Status |
|---|---|---|
| GĂ¡Â»Â­i request bĂ¡ÂºÂ¿p | `KitchenService.SendToKitchenAsync`, `OrderRequest`, `OrderRequestItem` | VERIFIED |
| NhĂ¡ÂºÂ­n request realtime | `KitchenNotifier.NotifyNewOrderRequestAsync`, `NewOrderRequest`, `/kitchenHub` | VERIFIED |
| CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t request status | `PATCH /api/Order/kitchen/requests/{id}/status`, `UpdateRequestStatusAsync` | VERIFIED |
| Xem active/history/detail | `OrderController` kitchen endpoints, `KitchenPage.tsx`, `KitchenHistoryPage.tsx` | VERIFIED |
| Product availability | `ProductController` availability endpoint, `KitchenPage.tsx` | VERIFIED |
| Item-level kitchen status | khÄ‚Â´ng cÄ‚Â³ field/status riÄ‚Âªng trÄ‚Âªn `OrderDetail` trong current model | FALSE / OUTDATED |
| Polling/recovery | POS/KDS fetch lĂ¡ÂºÂ¡i danh sÄ‚Â¡ch khi cĂ¡ÂºÂ§n; KDS cÄ‚Â³ nÄ‚Âºt refresh | VERIFIED; khÄ‚Â´ng phĂ¡ÂºÂ£i SignalR-only |

### 4.4 Management

Source cÄ‚Â³ controller/service/UI cho branch, employee, attendance, schedule, shift, product, topping, promotion, table/area, reservation, expense, inventory, dashboard, business insight, receipt settings, system settings, notifications.

- **removed staff-finance module:** current `ApplicationDbContext` khÄ‚Â´ng cÄ‚Â³ `DbSet<removed staff-finance module>`, current Domain khÄ‚Â´ng cÄ‚Â³ removed staff-finance module entity; backup/history migration cÄ‚Â³ tÄ‚Âªn removed staff-finance module cĂ…Â©. Current removed staff-finance module capability: **FALSE / OUTDATED** nĂ¡ÂºÂ¿u mÄ‚Â´ tĂ¡ÂºÂ£ lÄ‚Â  persisted active module.
- **Inventory:** current persisted model cÄ‚Â³ `InventoryItem`, `BranchInventory`, `StockReceipt`, `StockReceiptItem`, `StockIssue`, `StockIssueItem`, `StockTransaction`; controller/service vÄ‚Â  tests tĂ¡Â»â€œn tĂ¡ÂºÂ¡i. **VERIFIED**.
- **Business Insights:** `BusinessInsight`, `BusinessInsightController`, `ProactiveInsightService`, background service, UI vÄ‚Â  tests. **VERIFIED**.

### 4.5 AI

Use cases: chat; role-specific read tools; customer booking/menu/order lookup; admin product-price write; employee order-status write. Tool list vÄ‚Â  permission matrix Ă„â€˜Ă†Â°Ă¡Â»Â£c trÄ‚Â¬nh bÄ‚Â y tĂ¡ÂºÂ¡i mĂ¡Â»Â¥c 11.

---

## 5. Activity Diagram Source of Truth

### 5.1 Activity 1 Ă¢â‚¬â€œ Ă„ÂĂ¡ÂºÂ·t mÄ‚Â³n qua QR

**Swimlanes Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t:** Guest/Customer Web Ă¢â€ â€™ Guest Controller/Auth Ă¢â€ â€™ Order Controller Ă¢â€ â€™ OrderService Ă¢â€ â€™ EF/PostgreSQL Ă¢â€ â€™ optional Kitchen/Notifier.

**Main flow Ă„â€˜Ă†Â°Ă¡Â»Â£c source chĂ¡Â»Â©ng minh:**

1. NgĂ†Â°Ă¡Â»Âi dÄ‚Â¹ng mĂ¡Â»Å¸ QR URL.
2. Frontend gĂ¡Â»Â­i QR token tĂ¡Â»â€ºi `POST /api/guest/bootstrap`.
3. Backend giĂ¡ÂºÂ£i mÄ‚Â£/tra `RestaurantTable.QrToken`, xÄ‚Â¡c Ă„â€˜Ă¡Â»â€¹nh `TableId` vÄ‚Â  `BranchId`, phÄ‚Â¡t guest JWT.
4. Frontend lĂ¡ÂºÂ¥y menu/product/topping.
5. Guest submit `POST /api/Order` vĂ¡Â»â€ºi item identity/quantity.
6. Controller lĂ¡ÂºÂ¥y table/branch tĂ¡Â»Â« signed guest claims, khÄ‚Â´ng tin `BranchId`, `TableName`, customer identity do guest gĂ¡Â»Â­i.
7. Validate cÄ‚Â³ item, quantity > 0, product/topping tĂ¡Â»â€œn tĂ¡ÂºÂ¡i.
8. `OrderService.CreateOrUpdateOrderAsync` merge/create `OrderDetail`, giĂ¡Â»Â¯ `SentQuantity`, tÄ‚Â­nh lĂ¡ÂºÂ¡i financial snapshot.
9. Save database vÄ‚Â  trĂ¡ÂºÂ£ order.
10. NĂ¡ÂºÂ¿u trĂ¡ÂºÂ¡ng thÄ‚Â¡i pending web order phÄ‚Â¹ hĂ¡Â»Â£p, controller thÄ‚Â´ng bÄ‚Â¡o pending order change.

**Decision owner:** authentication/session vÄ‚Â  branch/table: `GuestController`/`OrderController`; item validation: `OrderController`; merge/calculation: `OrderService`.

**Guards:** token hĂ¡Â»Â£p lĂ¡Â»â€¡; table active; table branch khĂ¡Â»â€ºp claim; order mĂ¡Â»â€ºi Ă„â€˜Ă¡Â»â€˜i vĂ¡Â»â€ºi guest; item list khÄ‚Â´ng rĂ¡Â»â€”ng; quantity > 0; IDs tĂ¡Â»â€œn tĂ¡ÂºÂ¡i; branch access.

**Loop:** lĂ¡ÂºÂ·p tĂ¡Â»Â«ng `OrderDetail` Ă„â€˜Ă¡Â»Æ’ merge/tÄ‚Â­nh subtotal; lĂ¡ÂºÂ·p product/topping identity kiĂ¡Â»Æ’m tra.

**Unsupported/khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c vĂ¡ÂºÂ½:** guest AI; client-authoritative total/price; payment gateway callback; item-level kitchen status; tĂ¡ÂºÂ¡o Invoice entity khi submit.

**Status: VERIFIED**, vĂ¡Â»â€ºi lĂ†Â°u Ä‚Â½ UI details vĂ¡Â»Â cart khÄ‚Â´ng thay thĂ¡ÂºÂ¿ backend flow.

### 5.2 Activity 2 Ă¢â‚¬â€œ TiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n Web Order

**Flow:** POS query pending orders Ă¢â€ â€™ staff chĂ¡Â»Ân order/table Ă¢â€ â€™ `POST /api/Order/{id}/accept` Ă¢â€ â€™ controller lĂ¡ÂºÂ¥y branch cĂ¡Â»Â§a staff nĂ¡ÂºÂ¿u role branch-scoped Ă¢â€ â€™ `OrderService.AcceptWebOrderAsync` mĂ¡Â»Å¸ transaction Ă¢â€ â€™ load pending `Order`/web order Ă¢â€ â€™ validate branch/state/table Ă¢â€ â€™ claim table vÄ‚Â  order Ă¢â€ â€™ commit Ă¢â€ â€™ return accepted order Ă¢â€ â€™ notify pending list.

**KĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n chÄ‚Â­nh xÄ‚Â¡c:**

- Accept **khÄ‚Â´ng tĂ¡ÂºÂ¡o Order mĂ¡Â»â€ºi**; source cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t/claim order hiĂ¡Â»â€¡n hĂ¡Â»Â¯u.
- Accept **khÄ‚Â´ng merge OrderDetails trong controller**; source sĂ¡Â»Â­ dĂ¡Â»Â¥ng order Ă„â€˜Ä‚Â£ persisted vÄ‚Â  trĂ¡ÂºÂ£ order hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i.
- Accept **khÄ‚Â´ng phĂ¡ÂºÂ£i flow tÄ‚Â­nh tiĂ¡Â»Ân mĂ¡Â»â€ºi**; khÄ‚Â´ng cÄ‚Â³ call rÄ‚Âµ tĂ¡Â»â€ºi `CreateOrUpdateOrderAsync` trong accept path.
- Accept **khÄ‚Â´ng tĂ¡Â»Â± send-to-kitchen**; endpoint send-to-kitchen lÄ‚Â  action riÄ‚Âªng.
- CÄ‚Â³ conflict handling bĂ¡ÂºÂ±ng kĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ `Conflict` khi order Ă„â€˜Ä‚Â£ Ă„â€˜Ă†Â°Ă¡Â»Â£c nhÄ‚Â¢n viÄ‚Âªn khÄ‚Â¡c nhĂ¡ÂºÂ­n hoĂ¡ÂºÂ·c khÄ‚Â´ng cÄ‚Â²n pending.
- CÄ‚Â³ validate branch, table vÄ‚Â  transaction/commit trong `AcceptWebOrderAsync`.
- KhÄ‚Â´ng tÄ‚Â¬m thĂ¡ÂºÂ¥y `RejectWebOrder` endpoint tĂ†Â°Ă†Â¡ng Ă¡Â»Â©ng. **NOT VERIFIED / FALSE** nĂ¡ÂºÂ¿u thesis vĂ¡ÂºÂ½ reject action.

### 5.3 Activity 3 Ă¢â‚¬â€œ Payment & Loyalty

**Main flow:** POS gĂ¡Â»Â­i `POST /api/Order/{id}/payment` Ă¢â€ â€™ validate amount > 0/payment method Ă¢â€ â€™ load persisted order Ă¢â€ â€™ branch access Ă¢â€ â€™ request amount phĂ¡ÂºÂ£i bĂ¡ÂºÂ±ng `Order.TotalAmount` Ă¢â€ â€™ `OrderService.PayOrderAsync` mĂ¡Â»Å¸ transaction Ă¢â€ â€™ conditional `ExecuteUpdateAsync` chĂ¡Â»â€° cho order `Status == "Ă„Âang xĂ¡Â»Â­ lÄ‚Â½"`, `PaidAmount <= 0`, `TotalAmount == amount` Ă¢â€ â€™ nĂ¡ÂºÂ¿u update thÄ‚Â nh cÄ‚Â´ng reload/order state Ă¢â€ â€™ `LoyaltyService.EarnPointsAsync` Ă¢â€ â€™ commit Ă¢â€ â€™ controller phÄ‚Â¡t `PaymentCompleted`.

**ThĂ¡Â»Â© tĂ¡Â»Â± source:** conditional payment update Ă¢â€ â€™ `EarnPointsAsync` Ă¢â€ â€™ transaction commit. NĂ¡ÂºÂ¿u conditional update Ă¡ÂºÂ£nh hĂ†Â°Ă¡Â»Å¸ng 0 row, trĂ¡ÂºÂ£ `AlreadyPaid` hoĂ¡ÂºÂ·c khÄ‚Â´ng payable tÄ‚Â¹y trĂ¡ÂºÂ¡ng thÄ‚Â¡i; khÄ‚Â´ng chĂ¡ÂºÂ¡y loyalty.

**Loyalty redeem:** action riÄ‚Âªng `POST /api/Order/{id}/redeem`; `RedeemPointsAsync` cÄ‚Â³ transaction, idempotency check trÄ‚Âªn order/type, trĂ¡Â»Â« Ă„â€˜iĂ¡Â»Æ’m vÄ‚Â  giĂ¡ÂºÂ£m `Order.TotalAmount`, tĂ¡ÂºÂ¡o `LoyaltyTransaction`. Ă„ÂÄ‚Â¢y khÄ‚Â´ng phĂ¡ÂºÂ£i cÄ‚Â¹ng call chain vĂ¡Â»â€ºi payment earn trong mĂ¡Â»â„¢t request redeem.

**KhÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c vĂ¡ÂºÂ½ nhĂ†Â° Ă„â€˜Ä‚Â£ triĂ¡Â»Æ’n khai:** Invoice entity creation; table tĂ¡Â»Â± Ă„â€˜Ă¡Â»â„¢ng free/update do payment; VietQR server-side gateway; payment gateway/webhook; refund/reversal hoÄ‚Â n chĂ¡Â»â€°nh. POS cÄ‚Â³ hiĂ¡Â»Æ’n thĂ¡Â»â€¹ VietQR image tĂ¡Â»Â« `img.vietqr.io`, nhĂ†Â°ng Ă„â€˜Ä‚Â³ lÄ‚Â  UI/payment instruction, khÄ‚Â´ng phĂ¡ÂºÂ£i gateway settlement integration.

### 5.4 Activity 4 Ă¢â‚¬â€œ KDS

**Flow:** POS/staff gĂ¡Â»Âi `send-to-kitchen` Ă¢â€ â€™ `KitchenService.SendToKitchenAsync` load order/details Ă¢â€ â€™ chĂ¡Â»Ân cÄ‚Â¡c detail cÄ‚Â³ `Quantity > SentQuantity` Ă¢â€ â€™ tĂ¡ÂºÂ¡o `OrderRequest` mĂ¡Â»â€ºi vÄ‚Â  `OrderRequestItem` vĂ¡Â»â€ºi delta quantity Ă¢â€ â€™ cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t `SentQuantity = Quantity` Ă¢â€ â€™ save trong transaction Ă¢â€ â€™ notifier `NewOrderRequest` Ă¢â€ â€™ KDS nhĂ¡ÂºÂ­n event hoĂ¡ÂºÂ·c refresh/polling Ă¢â€ â€™ PATCH request status Ă¢â€ â€™ service cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t `OrderRequest.Status`, `ProcessedAt/ProcessedBy` nĂ¡ÂºÂ¿u cÄ‚Â³ Ă¢â€ â€™ notifier `RequestStatusUpdated`.

**Important distinction:** `SentQuantity` lÄ‚Â  snapshot/delta tracking trÄ‚Âªn `OrderDetail`; status lÄ‚Â  trÄ‚Âªn `OrderRequest`. KhÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c Ă„â€˜Ă¡Â»â€œng nhĂ¡ÂºÂ¥t `SentQuantity` vĂ¡Â»â€ºi item-level kitchen status.

**Groups/events:**

- `kitchen-admin`
- `kitchen-branch:{branchId}`
- `role:{role}`
- `branch:{branchId}`
- `branch:{branchId}:role:{role}`
- `NewOrderRequest`
- `RequestStatusUpdated`
- `PaymentCompleted`

`KitchenNotifier` gĂ¡Â»Â­i NewOrderRequest tĂ¡Â»â€ºi admin-global + branch kitchen group; request status cÄ‚Â³ overload branch-aware nhĂ†Â°ng cĂ…Â©ng cÄ‚Â³ overload global admin. **Branch isolation: PARTIALLY VERIFIED.**

### 5.5 Activity 5 Ă¢â‚¬â€œ AI Tool Execution

**Flow:** authenticated UI Ă¢â€ â€™ `POST /api/Ai/chat` Ă¢â€ â€™ `AiController.Chat` Ă„â€˜Ă¡Â»Âc role/userId/branch claims Ă¢â€ â€™ guest customer bĂ¡Â»â€¹ Forbid Ă¢â€ â€™ `AiOrchestrator.ProcessAsync` lĂ¡ÂºÂ¥y context/prompt Ă¢â€ â€™ registry lĂ¡ÂºÂ¥y schema theo role Ă¢â€ â€™ `AI provider implementation.GenerateContentAsync` Ă¢â€ â€™ nĂ¡ÂºÂ¿u text thÄ‚Â¬ final response Ă¢â€ â€™ nĂ¡ÂºÂ¿u function call thÄ‚Â¬ lookup registry Ă¢â€ â€™ `AiPermissionService.CanExecuteAsync` kiĂ¡Â»Æ’m tra role/risk/manager branch claim Ă¢â€ â€™ `tool.ExecuteAsync(arguments, userContext)` Ă¢â€ â€™ tool gĂ¡Â»Âi Application Service/DB Ă¢â€ â€™ function response Ă„â€˜Ă†Â°a lĂ¡ÂºÂ¡i AI provider Ă¢â€ â€™ lĂ¡ÂºÂ·p.

**Loop:** `const int MAX_ITERATIONS = 5`; sequential theo cÄ‚Â¡c lĂ†Â°Ă¡Â»Â£t HTTP AI provider. NĂ¡ÂºÂ¿u mĂ¡Â»â„¢t response chĂ¡Â»Â©a nhiĂ¡Â»Âu function call, source duyĂ¡Â»â€¡t tuĂ¡ÂºÂ§n tĂ¡Â»Â± tĂ¡Â»Â«ng part vÄ‚Â  gom function responses trong cÄ‚Â¹ng lĂ†Â°Ă¡Â»Â£t; khÄ‚Â´ng cÄ‚Â³ parallel task execution.

**Database boundary:** `AiOrchestrator` khÄ‚Â´ng nhĂ¡ÂºÂ­n `DbContext`; tools nhĂ¡ÂºÂ­n Application Service. KhÄ‚Â´ng thĂ¡ÂºÂ¥y AI gĂ¡Â»Âi EF/SQL trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p trong orchestrator. **VERIFIED.**

**Error:** tool khÄ‚Â´ng tĂ¡Â»â€œn tĂ¡ÂºÂ¡i, permission denied, tool validation/exception vÄ‚Â  vĂ†Â°Ă¡Â»Â£t quÄ‚Â¡ 5 vÄ‚Â²ng Ă„â€˜Ă†Â°Ă¡Â»Â£c chuyĂ¡Â»Æ’n thÄ‚Â nh `AiToolResult`/`AiResponse` error; controller trĂ¡ÂºÂ£ HTTP 500 generic message khi exception Ă¡Â»Å¸ controller.

---

## 6. Sequence Diagram Source of Truth

### 6.1 Payment with concurrency control

```text
POSPage.tsx
  -> POST /api/Order/{id}/payment
OrderController.PayOrder
  -> GetOrderByIdAsync
  -> HasBranchAccess
  -> OrderService.PayOrderAsync
       -> BeginTransactionAsync
       -> Orders.Where(Status, PaidAmount, TotalAmount).ExecuteUpdateAsync
       -> LoyaltyService.EarnPointsAsync
       -> CommitAsync
  <- PaymentAttemptResult
OrderController
  -> HubContext.Clients.All.SendAsync("PaymentCompleted", payload)
  <- 200 / 409 / 400 / 404
```

`ExecuteUpdateAsync` lÄ‚Â  concurrency gate. Hai payment requests cÄ‚Â¹ng order khÄ‚Â´ng cÄ‚Â¹ng thĂ¡Â»Âa Ă„â€˜iĂ¡Â»Âu kiĂ¡Â»â€¡n `PaidAmount <= 0`/status. Tuy nhiÄ‚Âªn event lÄ‚Â  `Clients.All`, nÄ‚Âªn diagram phĂ¡ÂºÂ£i thĂ¡Â»Æ’ hiĂ¡Â»â€¡n broadcast global vÄ‚Â  `branchId` payload, khÄ‚Â´ng mÄ‚Â´ tĂ¡ÂºÂ£ lÄ‚Â  chĂ¡Â»â€° branch group.

### 6.2 KDS + SignalR

```text
POS -> OrderController.SendToKitchen
     -> KitchenService.SendToKitchenAsync
     -> DB transaction: OrderDetails.SentQuantity,
        OrderRequest, OrderRequestItems
     -> KitchenNotifier
     -> HubContext.Groups(kitchen-admin, kitchen-branch:{branchId})
     -> NewOrderRequest
     -> KDS KitchenPage listener / refresh fallback

KDS -> PATCH /api/Order/kitchen/requests/{id}/status
    -> OrderController.UpdateRequestStatus
    -> KitchenService.UpdateRequestStatusAsync
    -> DB OrderRequest.Status
    -> KitchenNotifier
    -> RequestStatusUpdated
    -> KDS/admin listeners
```

Hub `OnConnectedAsync` thÄ‚Âªm cÄ‚Â¡c group dĂ¡Â»Â±a trÄ‚Âªn claims. KhÄ‚Â´ng cÄ‚Â³ bĂ¡ÂºÂ±ng chĂ¡Â»Â©ng frontend sĂ¡Â»Â­ dĂ¡Â»Â¥ng mĂ¡Â»Âi group; client chĂ¡Â»Â§ yĂ¡ÂºÂ¿u connect `/kitchenHub` vÄ‚Â  lĂ¡ÂºÂ¯ng nghe event. KDS vĂ¡ÂºÂ«n cÄ‚Â³ fetch active requests/history, do Ă„â€˜Ä‚Â³ realtime cÄ‚Â³ recovery/polling path.

### 6.3 AI Agent

```text
Customer/staff UI
 -> AiController.Chat
 -> AiOrchestrator.ProcessAsync
 -> AiContextService + PromptBuilder + AiToolRegistry
 -> AI provider implementation / external AI provider
 <- functionCall?
 -> AiToolRegistry.GetTool
 -> AiPermissionService.CanExecuteAsync
 -> IAiTool.ExecuteAsync
 -> Application Service
 -> EF Core / PostgreSQL
 <- AiToolResult
 -> AI provider implementation (next iteration, max 5)
 <- final text
 -> AiResponse
```

Ă„ÂÄ‚Â¢y lÄ‚Â  sequential tool calling; khÄ‚Â´ng vĂ¡ÂºÂ½ `par`/parallel fragment. DÄ‚Â¹ng `loop` vĂ¡Â»â€ºi guard `iterations < 5`, `alt` cho no function call / function call / permission denied / unknown tool.

---

## 7. Domain Class Model Audit

### 7.1 Current entities

Current `ApplicationDbContext` cÄ‚Â³ cÄ‚Â¡c DbSet:

```text
Product, Order, OrderDetail, Expense, Employee, RestaurantTable, Area,
Branch, Attendance, WorkSchedule, Customer, Shift, Reservation, Topping,
Promotion, OrderRequest, OrderRequestItem, Notification, ReceiptSettings,
SystemSetting, LoyaltyTransaction, BusinessInsight, InventoryItem,
BranchInventory, StockReceipt, StockReceiptItem, StockIssue, StockIssueItem,
StockTransaction
```

### 7.2 Important entity/relationship facts

| Entity | PK/fields Ă„â€˜Ä‚Â¡ng chÄ‚Âº Ä‚Â½ | Relationship/branch | Status/evidence |
|---|---|---|---|
| Branch | `Id`, name/profile/bank/image fields | parent cho Employee/Table/Attendance/Schedule/Expense/inventory vÄ‚Â  branch-scoped records | VERIFIED |
| Employee | `Id`, role, branch, active, password/profile | `BranchId` FK | VERIFIED |
| Customer | `Id`, phone, active, loyalty fields/password | order/customer, loyalty transaction/customer | VERIFIED |
| Area | `Id`, branch/name | area/table relationship theo model/source | VERIFIED |
| RestaurantTable | class name lÄ‚Â  `RestaurantTable`, `Id`, `QrToken`, `BranchId`, status | branch FK; unique non-null QR token | VERIFIED |
| Product | `Id`, price/availability/catalog fields | order detail optional ProductId | VERIFIED |
| Topping | `Id`, `Price`, `CostPrice`, `ToppingType`, category/active | order detail optional ToppingId | VERIFIED |
| Order | `Id`, branch/table/customer, `TotalAmount`, snapshots, `InvoiceCode`, payment/status | has many `Details`; optional Customer | VERIFIED |
| OrderDetail | product/topping ids, quantity, `SentQuantity`, prices/options | Order FK; Product/Topping optional | VERIFIED |
| OrderRequest | order, request number/status/branch/time fields | has many OrderRequestItems | VERIFIED |
| OrderRequestItem | request, product/name/options/quantity | OrderRequest FK cascade | VERIFIED |
| LoyaltyTransaction | customer/order/type/points | unique filtered `(OrderId, Type)` for non-null order | VERIFIED |
| Attendance | employee/branch/time/status | Employee + Branch FK | VERIFIED |
| WorkSchedule | employee/branch/date/shift fields | Employee + Branch FK | VERIFIED |
| Shift | employee/branch/open-close/cash fields | current DbSet | VERIFIED |
| Expense | branch, payment/note/amount, optional StockReceipt | Branch restrict; StockReceipt one-to-one | VERIFIED |
| InventoryItem | normalized name/unit | global catalog; branch quantities separate | VERIFIED |
| BranchInventory | branch + inventory item + quantity/minimum | unique branch/item; nonnegative checks | VERIFIED |
| StockReceipt/Issue | branch, status, idempotency, items | branch-scoped, transaction records | VERIFIED |
| SystemSetting | branch/key/value/type/updated | unique `(BranchId, Key)` | VERIFIED |
| ReceiptSettings | branch-specific receipt config | unique BranchId | VERIFIED |
| BusinessInsight | branch, deduplication/status/severity | analytics/background insights | VERIFIED |
| Notification | target role/user/time | indexed target fields | VERIFIED |
| Reservation | table/customer/branch/status/time | branch/table validation | VERIFIED |

### 7.3 Explicit absence/differences

- `Restaurant` entity: **NOT FOUND / FALSE** in current Domain/DbContext.
- `RestaurantId`: **NOT FOUND** as current canonical ownership field; source uses `BranchId`.
- `OrderItems`: **FALSE / OUTDATED**; canonical class is `OrderDetail` and navigation `Order.Details`.
- `removed staff-finance module`, `removed staff-finance moduleSettings`, `EmployeeSalaryProfile`: not in current DbContext/current entity inventory; old backup migrations contain them. **FALSE / OUTDATED** as current persisted capability.
- `Invoice` entity/table: not in current DbContext/current Domain inventory. `InvoiceController`/`InvoiceCode` exist, so invoice/history is only **PARTIALLY VERIFIED**.

Recommended domain class diagram should center `Branch`, `Employee`, `Customer`, `RestaurantTable`, `Order`, `OrderDetail`, `Product`, `Topping`, `OrderRequest`, `OrderRequestItem`, loyalty and current inventory entities. Do not add Restaurant/Invoice/removed staff-finance module classes unless current source is changed later.

---

## 8. ERD/database audit

### 8.1 Persisted model

The current DbContext and current migration chain end with `20261002140936_AddToppingType` plus `ApplicationDbContextModelSnapshot.cs`. Current model contains the DbSets listed in section 7.1.

Important current constraints/configuration in `ApplicationDbContext.cs`:

- `Order -> Details` one-to-many, cascade delete.
- `OrderRequest -> Items` one-to-many, cascade delete.
- optional `OrderDetail.ProductId` and `OrderDetail.ToppingId`.
- `RestaurantTable.QrToken` required, max length 43, unique index.
- `SystemSetting` unique index `(BranchId, Key)`.
- `ReceiptSettings.BranchId` unique index.
- `LoyaltyTransaction` filtered unique index `(OrderId, Type)` for non-null order.
- branch inventory unique `(BranchId, InventoryItemId)` and nonnegative quantity checks.
- stock receipt/issue idempotency unique by branch when key is non-null.
- financial/stock decimal precision configured in model.

### 8.2 Migration history warning

`services/Migrations_backup_20260919/` contains historical model snapshots/migrations with removed staff-finance module and older schema objects. It must not be treated as the current runtime model. Current model/snapshot wins under this audit rule.

### 8.3 ERD consistency classification

No complete thesis ERD/source document was found under a current `baocaokhoaluan` directory to compare field-by-field. Therefore all comparison rows that require the thesis's exact drawing are **NOT VERIFIED** until the actual thesis document/ERD is supplied or added to the repository.

Source-grounded correction list for any existing ERD:

| Concept in ERD | Source of truth | Classification to apply if ERD differs |
|---|---|---|
| Restaurant/RestaurantId | absent; Branch is owner | WRONG ENTITY / WRONG FIELD |
| OrderItem | current class `OrderDetail` | WRONG ENTITY NAME |
| Invoice table | no current Invoice DbSet/entity | OUTDATED / UNSUPPORTED |
| removed staff-finance module tables | only historical backup migration evidence | OUTDATED |
| OrderRequest/OrderRequestItem | current DbSets and service | MISSING FROM REPORT if absent |
| LoyaltyTransaction | current DbSet and service | MISSING FROM REPORT if absent |
| Inventory current model | six current inventory DbSets | MISSING/OUTDATED if only old ingredient model shown |
| QR table token | `RestaurantTable.QrToken`, unique required | MISSING/WRONG if client-controlled QR field shown |

---

## 9. Multi-branch isolation audit

### 9.1 Mechanism

`JwtService` puts `branchId` in employee/customer tokens when available. Branch-scoped controller helpers parse claims and compare resource branch. Admin is commonly treated as global. Guest branch/table is signed into the guest token and `GuestSessionContext` is checked against the table.

### 9.2 Endpoint/group matrix

| Endpoint/group | Roles | Branch check | Admin bypass/global | Result/evidence |
|---|---|---|---|---|
| `GET /api/Order` | authenticated | customer/guest forced own phone/session; staff resolves branch | admin may resolve requested branch | VERIFIED with role-specific scope |
| `POST /api/Order` | authenticated guest/customer/staff | guest branch/table from token; existing order/resource check | admin broader | VERIFIED |
| `POST /api/Order/{id}/accept` | admin/manager/employee/cashier | branch-scoped roles pass token branch to service | admin no allowed branch id | VERIFIED with conflict transaction |
| `POST /api/Order/{id}/payment` | admin/manager/employee/cashier | `HasBranchAccess(existingOrder.BranchId)` | admin global | VERIFIED; `PaymentCompleted` event global |
| kitchen active/history/status | admin/manager/kitchen plus selected staff | controller resolves branch or resource branch | admin global | VERIFIED/PARTIAL due global admin notifier |
| `GET/PATCH /api/Employee` | staff management roles | controller/service tests enforce manager branch | admin broad | VERIFIED by `EmployeeControllerAuthorizationTests` |
| `GET/PUT /api/ReceiptSettings` | management roles | branch setting lookup/write | admin may choose branch | VERIFIED/PARTIAL; inspect caller supplied branch and controller guard |
| inventory endpoints | management/inventory roles | branch inventory and service branch validation | admin broader | VERIFIED; tested by inventory tests |
| `/kitchenHub` | admin/manager/employee/cashier/kitchen | groups include branch claims | `kitchen-admin` global | PARTIALLY VERIFIED |
| `/api/Ai/chat` | `[Authorize]` | context includes branch; manager requires branch for tools | admin global for allowed tools | VERIFIED/PARTIAL per tool implementation |

### 9.3 Important limitations

- Admin global behavior is intentional in multiple paths; do not describe Admin as branch-scoped.
- SignalR uses global `kitchen-admin` in addition to branch groups. `PaymentCompleted` uses `Clients.All`.
- A client can submit a branch id, but sensitive paths must derive/validate against JWT/resource. Frontend `selectedBranchId` is UX state and is not a security boundary.
- The existence of a passing branch test suite does not prove every current endpoint has identical isolation semantics; matrix evidence must remain endpoint-specific.

**Overall branch isolation: PARTIALLY VERIFIED**, not Ă¢â‚¬Å“absolute isolationĂ¢â‚¬Â.

---

## 10. Order and financial integrity audit

### 10.1 Calculation source and formula

`OrderService` is the authoritative calculation owner. Client submits item identity, quantity, option/topping identity/labels; controller validates identity existence; service loads authoritative prices.

Current formula represented in `OrderService`:

```text
SubTotal = sum(item price * quantity + applicable topping/option values)
ServiceFeeAmount = round(SubTotal * ServiceFeePercent / 100, 0, AwayFromZero)
TaxableBase = SubTotal + ServiceFeeAmount
VatAmount = round(TaxableBase * VatPercent / 100, 0, AwayFromZero)
TotalAmount = TaxableBase + VatAmount - Discount
```

`VatPercent`, `ServiceFeePercent`, `VatAmount`, `ServiceFeeAmount`, `SubTotal` are persisted snapshots on `Order` where populated. Settings are resolved by branch/order context. Historical nullable snapshots must not be backfilled merely from current settings.

### 10.2 Flow findings

| Flow | Source finding | Status |
|---|---|---|
| Create/update order | `CreateOrUpdateOrderAsync`; merge details; preserves sent quantity; recalculates | VERIFIED |
| Invalid quantity/items | controller rejects empty/quantity <= 0 | VERIFIED |
| Product/topping authority | controller checks IDs; service calculates | VERIFIED |
| Payment amount | `PayOrder` compares request amount to loaded `Order.TotalAmount`; service conditional update also compares | VERIFIED |
| Duplicate payment | conditional `ExecuteUpdateAsync`, `PaidAmount <= 0`, status gate; result `AlreadyPaid` | VERIFIED |
| Payment transaction | service begins transaction; loyalty earn; commit | VERIFIED |
| Loyalty earn | `EarnPointsAsync`, idempotency query/index, customer update + transaction | VERIFIED |
| Loyalty redeem | separate transactional service; updates order total and transaction | VERIFIED |
| Refund/reversal | no complete equivalent flow demonstrated in current controller/service inventory | NOT VERIFIED |
| Invoice creation on payment | no current Invoice entity; payment updates order and emits event | FALSE / OUTDATED |
| Client total overwrite | no trusted client total path for payment; persisted total is authoritative | VERIFIED |
| Table release after payment | no payment call proving automatic table free/update | NOT VERIFIED |

### 10.3 Financial claims safe for thesis

Safe: Ă¢â‚¬Å“backend recalculates order financial values and payment validates against persisted order total with conditional update/concurrency guard.Ă¢â‚¬Â

Unsafe/overclaimed: Ă¢â‚¬Å“gateway confirms bank payment automaticallyĂ¢â‚¬Â, Ă¢â‚¬Å“invoice is atomically created on paymentĂ¢â‚¬Â, Ă¢â‚¬Å“all refunds are supportedĂ¢â‚¬Â, Ă¢â‚¬Å“financial consistency is formally guaranteed in every failure modeĂ¢â‚¬Â. Those claims are **NOT VERIFIED** unless additional source/test evidence is added.

---

## 11. SignalR/KDS audit

### Hub and authorization

- Route: `/kitchenHub` in `Program.cs`.
- Hub: `services/api/src/WebAPI/Hubs/KitchenHub.cs`.
- Authorization: `[Authorize(Roles = "admin,manager,employee,cashier,kitchen")]`.
- JWT query-string token is accepted for `/kitchenHub` in `JwtBearerEvents.OnMessageReceived`.

### Exact groups

```text
kitchen-admin
role:{role}
kitchen-branch:{branchId}
branch:{branchId}
branch:{branchId}:role:{role}
```

### Exact events observed

```text
NewOrderRequest
RequestStatusUpdated
PaymentCompleted
```

### Client/recovery behavior

`KitchenPage.tsx` connects with SignalR token and listens for KDS events, but also fetches active requests and exposes refresh. `POSPage.tsx` connects to the hub and fetches order state again after event/interaction. Thus realtime is implemented with a recovery/polling/read-after-event path.

### Global behavior warnings

- `KitchenNotifier.NotifyNewOrderRequestAsync`: branch-specific kitchen group plus global `kitchen-admin`.
- `KitchenNotifier.NotifyRequestStatusUpdatedAsync`: overloads include global `kitchen-admin`; branch-aware overload also sends admin global.
- `OrderController.PayOrder`: `_hub.Clients.All.SendAsync("PaymentCompleted", ...)`.

**Conclusion: VERIFIED realtime SignalR, PARTIALLY VERIFIED branch isolation.**

---

## 12. AI Agent audit

### 12.1 Architecture

| Layer | Evidence | Status |
|---|---|---|
| Controller | `AI/Controllers/AiController.cs`, `[Authorize]`, `POST /api/Ai/chat` | VERIFIED |
| Context | `AiContextService`, `AiUserContext`, claims | VERIFIED |
| Prompt | `PromptBuilder`, `AdminPrompt`, `EmployeePrompt`, `CustomerPrompt` | VERIFIED |
| Registry | `AiToolRegistry`, 14 DI registrations in `Program.cs` | VERIFIED |
| Model integration | `AI provider implementation` via injected `HttpClient`, config model/key mechanism | VERIFIED |
| Permission | `AiPermissionService`, `AiAuthorization`, allowed roles/risk | VERIFIED |
| Validation | `AiToolValidator` and per-tool argument parsing/validation | VERIFIED |
| Tool/Application Service | tool implementations call services such as `IOrderService`, `IDashboardService`, `IProductService` | VERIFIED |
| Direct DbContext in orchestrator | not present | VERIFIED absence |
| Max loop | `MAX_ITERATIONS = 5` | VERIFIED |
| Parallel function execution | no `Task.WhenAll`/parallel execution in orchestrator | VERIFIED absence; sequential |

### 12.2 Registered tool matrix

| Tool | Read/write | Risk | Allowed roles | Underlying service/data | Branch/ownership |
|---|---|---|---|---|---|
| `get_revenue` | Read | Read | admin, manager | `IDashboardService` | date + branch context |
| `update_product_price` | Write | Write | admin | `IProductService` | product/catalog validation |
| `get_active_staff` | Read | Read | admin, manager | `IEmployeeService` | manager branch context |
| `get_order_list` | Read | Read | admin, manager, employee, cashier, kitchen | `IOrderService` | `OrderQueryFilter.BranchId` context |
| `get_best_sellers` | Read | Read | admin, manager, employee, cashier, kitchen | `IDashboardService` | branch context |
| `get_revenue_comparison` | Read | Read | admin, manager | `IDashboardService` | branch context |
| `get_business_summary` | Read | Read | admin, manager | `IDashboardService` | branch context |
| `get_financial_analysis` | Read | Read | admin, manager | `IFinancialAnalysisService` | branch context |
| `employee_get_table_summary` | Read | Read | admin, manager, employee, cashier, kitchen | table/order service path | branch context |
| `get_my_shift` | Read | Read | employee, cashier, kitchen | shift service/context | own employee identity |
| `update_order_status` | Write | Write | admin, manager, employee, cashier, kitchen | `IOrderService.UpdateOrderStatus...` | resource/branch checks |
| `customer_get_menu` | Read | Read | admin, manager, employee, cashier, kitchen, customer | product service | branch parameter/available menu |
| `get_my_orders` | Read | Read | customer | order service | customer phone/ownership |
| `create_booking` | Write | Write | customer | reservation service | customer/branch/table validation |

No registered `get_financial_analysis` assumption is needed: it is present. No registered tool named `get_menu` exactly was found; canonical source name is `customer_get_menu`. No evidence of `get_my_order` singular; canonical source name is `get_my_orders`.

### 12.3 AI role conclusions

- Guest: `[Authorize]` token may reach controller, but explicit `Forbid`; **VERIFIED no AI**.
- Registered customer: customer tools, including write `create_booking`, are allowed by explicit role list; **VERIFIED**.
- Employee/cashier/kitchen: read tools and `update_order_status` according to registry; **VERIFIED**.
- Manager: management/read tools; manager without branch claim rejected by `AiPermissionService`; **VERIFIED**.
- Admin: all admin tools including product price write; **VERIFIED**.

### 12.4 AI evaluation

Repository has unit/integration-like tests for orchestrator, AI security, tool validation, analytics and business advisor. No dataset/result artifact/metric file proving production Tool Selection Accuracy, Security Compliance percentage or No-Tool Accuracy was found.  
**AI Evaluation: PARTIALLY VERIFIED for behavioral tests; NOT VERIFIED for quantitative benchmark claims.**

---

## 13. Frontend audit

| App | Framework | Authentication | Major routes/screens | API/SignalR | Status |
|---|---|---|---|---|---|
| Admin Web | React 18, TS, Vite, Tailwind | local token storage + backend JWT | login/profile; POS; table; kitchen/history; analytics; catalog; HRM; operations; settings; inventory | fetch wrapper/direct fetch, SignalR in POS/KDS | VERIFIED |
| Customer Web | React 18, TS, Vite, Tailwind | customer/guest session token | QRScan; DigitalMenu; customer login/profile; reservation; chatbot | direct fetch; no evidence of kitchen SignalR | VERIFIED |
| Shared | TypeScript package | N/A | shared API/model types and formatters | N/A | VERIFIED |

Frontend role guards/localStorage are UX controls only. Backend `[Authorize]`, role checks and resource checks are the security boundary.

### Frontend/backend mismatch risks

- Frontends send `selectedBranchId`, but backend must decide authorized branch.
- Admin Web displays VietQR image from an external URL; this is not evidence of server-side payment gateway/webhook.
- KDS frontend displays connection status and refresh, matching realtime plus recovery behavior.
- Current customer QR changes are pre-existing working-tree changes; this audit did not edit them.

---

## 14. Test and verification audit

### 14.1 Commands executed

```text
dotnet build services/api/RestaurantPOS.api.csproj --no-restore
dotnet test tests/RestaurantPOS.Tests/RestaurantPOS.Tests.csproj --no-restore
npm run build                         (apps/admin-web)
npm run build                         (apps/customer-web)
```

### 14.2 Results

| Target | Result |
|---|---|
| Backend build | PASS; 0 warning, 0 error |
| Test project | PASS; Total 267, Passed 267, Failed 0, Skipped 0; duration about 22 s |
| Admin Web build | PASS; Vite completed; one bundle-size warning (main JS about 1.3 MB minified) |
| Customer Web build | PASS; Vite completed; 1272 modules transformed |

### 14.3 Test coverage represented by file names

Tests exist for authentication/JWT, authorization, branch isolation, AI controller/orchestrator/security/validation, orders, financial calculation/integrity, loyalty, KDS, inventory, attendance, employee authorization, reservations/tables, business insights and QR guest sessions.

Tests do not by themselves prove all production external integrations. In particular, no live payment gateway/webhook benchmark, production SignalR network test, or quantitative AI evaluation artifact was found.

---

## 15. Deployment/runtime audit

### Development/demo evidence

- Kestrel listens on HTTPS port 5000 in `Program.cs`.
- Certificate path is `.https/pos-cert.pfx` or `HTTPS_CERT_PATH`; missing certificate stops startup.
- API uses Supabase/PostgreSQL connection configuration; no localhost fallback.
- Development CORS allows configured origins and specific localhost/LAN HTTPS conditions; credentials are enabled for SignalR.
- Admin/customer Vite apps use runtime host and port 5000; development uses HTTPS API.
- Customer frontend defaults to port 5174; Admin Vite commonly uses 5173 through configuration/launch.
- Automatic database migration/seeding is an explicit Development-only configuration opt-in, not an unconditional startup behavior.

**Deployment classification: DEVELOPMENT/DEMO evidence is VERIFIED. PRODUCTION deployment is NOT VERIFIED.**

Do not claim the whole system is cloud-deployed merely because the database connection can target Supabase. Do not claim Docker deployment; no Docker files were found.

---

## 16. Chapter 3 consistency matrix

The repository did not contain a complete editable thesis Chapter 3 text to compare line-by-line. The matrix below records source status for the planned subsection claims and marks direct thesis comparison as `NOT VERIFIED`/`MISSING FROM THESIS` where the claim text was not present in the repository.

| MĂ¡Â»Â¥c | Source-grounded conclusion | Status | UML/document action |
|---|---|---|---|
| 3.1.1 Actors/roles | six JWT roles + guest session actor; no actor hierarchy by rank | VERIFIED | REVISE |
| 3.1.2 Functional requirements | order, QR, KDS, staff, management, AI, inventory present | VERIFIED | KEEP/REVISE |
| 3.1.3 Non-functional requirements | JWT, rate limits, HTTPS, branch checks, transactions present; quantitative scalability not evidenced | PARTIALLY VERIFIED | REVISE |
| 3.1.4 Main business processes | QR, acceptance, payment/loyalty, KDS, AI paths traceable | VERIFIED | KEEP/REVISE |
| 3.2.1 Overview Use Case | capabilities broad but role hierarchy must not be invented | PARTIALLY VERIFIED | REBUILD/REVISE |
| 3.2.2 Use Case specifications | endpoint/service/entity evidence available | VERIFIED as source basis | ADD/REVISE |
| 3.2.3 Activity | QR/payment/KDS/AI supported; web reject and item kitchen status unsupported | PARTIALLY VERIFIED | REBUILD affected diagrams |
| 3.2.4 Sequence | payment/KDS/AI exact chains available | VERIFIED | REVISE with global SignalR warnings |
| 3.2.5 Class | current model uses Branch/RestaurantTable/OrderDetail; no Restaurant/Invoice/removed staff-finance module | VERIFIED source; thesis match NOT VERIFIED | REBUILD |
| 3.3.1 ERD | current DbContext/migrations available | VERIFIED source; thesis match NOT VERIFIED | REBUILD if old ERD |
| 3.3.2 entities/tables | current DbSets listed in section 8 | VERIFIED | REVISE |
| 3.3.3 relationships/constraints | current EF configurations prove key relations/indexes | VERIFIED | REVISE |
| 3.3.4 multi-branch data | BranchId-based plus Admin global behavior | PARTIALLY VERIFIED | REVISE |
| 3.4.1 architecture | layered monorepo with partial Clean Architecture | PARTIALLY VERIFIED | REVISE |
| 3.4.2 backend layers | controllers/services/domain/infrastructure/AI are present | VERIFIED | KEEP/REVISE |
| 3.4.3 frontend/API | two React/Vite clients and direct/fetch API consumers | VERIFIED | KEEP |
| 3.4.4 realtime KDS | SignalR + recovery fetch | VERIFIED | KEEP/REVISE |
| 3.4.5 order/payment | server calculation + conditional payment | VERIFIED | KEEP/REVISE |
| 3.4.6 financial integrity | strong guards exist; no absolute guarantee/gateway proof | PARTIALLY VERIFIED | REVISE language |
| 3.5.1 authentication | JWT bearer, active identity validation, guest session | VERIFIED | KEEP |
| 3.5.2 role authorization | role attributes and AI permissions | VERIFIED | KEEP/REVISE |
| 3.5.3 data access | endpoint/resource branch and ownership checks | PARTIALLY VERIFIED | REVISE |
| 3.5.4 branch isolation | broad implementation, global Admin events | PARTIALLY VERIFIED | REVISE limitation |
| 3.6.1 AI architecture | Controller Ă¢â€ â€™ Orchestrator Ă¢â€ â€™ Registry/Permission Ă¢â€ â€™ Tool Ă¢â€ â€™ Service | VERIFIED | KEEP |
| 3.6.2 orchestration/tool calling | sequential, max 5 | VERIFIED | KEEP |
| 3.6.3 registry/execution | 15 DI tools, role schemas | VERIFIED | KEEP |
| 3.6.4 AI permissions | role/risk/manager branch check | VERIFIED/PARTIAL per tool | REVISE |
| 3.6.5 business insights | dashboard/financial/insight services and background insight | VERIFIED | KEEP/REVISE |
| 3.7.1 admin UI | settings/management/analytics screens | VERIFIED | KEEP |
| 3.7.2 POS/cashier UI | POSPage, payment, redeem, web acceptance | VERIFIED | KEEP |
| 3.7.3 KDS UI | KitchenPage/history, SignalR + refresh | VERIFIED | KEEP |
| 3.7.4 customer QR UI | QRScan/DigitalMenu/profile/reservation | VERIFIED | KEEP/REVISE |

### 16.1 UML action summary

| Diagram | Action | Reason |
|---|---|---|
| Overview Use Case | REBUILD/REVISE | remove rank hierarchy; use capability groups |
| QR Activity | KEEP with REVISE | preserve signed guest session and server calculation |
| Web Order Activity | REVISE | no reject, no new Order creation, no automatic kitchen send |
| Payment Activity | REVISE | show conditional update Ă¢â€ â€™ loyalty Ă¢â€ â€™ commit and global event |
| KDS Activity | REVISE | request-level status; SentQuantity delta |
| AI Activity | KEEP with REVISE | show permission gate and max-5 sequential loop |
| Payment Sequence | KEEP with REVISE | exact concurrency/transaction/broadcast |
| KDS Sequence | KEEP with REVISE | exact groups/events and polling fallback |
| AI Sequence | KEEP with REVISE | no parallel fragment, registry/permission chain |
| Domain Class | REBUILD | current entity names/absence of Invoice/removed staff-finance module |
| ERD | REBUILD if old | use current DbContext/snapshot, not backup migration |

---

## 17. Chapter 4 consistency matrix

| MĂ¡Â»Â¥c | Source-grounded result | Status | Recommendation |
|---|---|---|---|
| 4.1.1 environment | .NET/Node/Vite/HTTPS 5000/Supabase configuration | VERIFIED | KEEP/REVISE |
| 4.1.2 source structure | monorepo with two frontends, one API, tests | VERIFIED | KEEP |
| 4.1.3 backend/database deployment | API + PostgreSQL/Npgsql; migration opt-in | VERIFIED | REVISE deployment wording |
| 4.1.4 frontends | Admin Web + Customer Web; POS/KDS inside Admin | VERIFIED | REVISE if thesis lists separate apps |
| 4.1.5 KDS/realtime | SignalR route/groups/events + refresh | VERIFIED/PARTIAL isolation | REVISE |
| 4.1.6 AI/tools | AI provider custom integration, registry, 14 tools | VERIFIED | KEEP/REVISE |
| 4.2.1 test method | xUnit/dotnet test current run | VERIFIED | KEEP |
| 4.2.2 functional tests | 267 passing tests across modules | VERIFIED | list actual command/date |
| 4.2.3 auth/branch tests | dedicated hardening tests pass | VERIFIED; endpoint-wide absolute claim PARTIAL | REVISE |
| 4.2.4 financial tests | calculation/integrity/payment/loyalty tests pass | VERIFIED | KEEP with limitations |
| 4.2.5 AI tests | controller/security/orchestrator/tool tests pass | VERIFIED behavioral; benchmark NOT VERIFIED | REVISE |
| 4.3.1 implementation result | build and test result above | VERIFIED | update numbers |
| 4.3.2 requirement response | many capabilities source-supported | PARTIALLY VERIFIED until thesis claims compared |
| 4.3.3 performance/response | no complete benchmark/latency dataset found | NOT VERIFIED | remove numeric claims unless rerun |
| 4.3.4 limitations | global SignalR admin, no gateway/webhook, no invoice entity, request-level KDS status, no removed staff-finance module current | VERIFIED | add explicitly |
| 4.4.1 management expansion | inventory/business insights already implemented; future-work wording may be outdated | IMPLEMENTED BUT DIFFERENT | revise future work |
| 4.4.2 AI expansion | AI already has 14 tools and sequential loop | PARTIALLY VERIFIED | distinguish existing from future |
| 4.4.3 deployment/integration | no Docker/production evidence | NOT VERIFIED | describe development/demo only |

---

## 18. Cross-chapter consistency issues

These issues should be checked in Chapter 1, Chapter 2, abstract and conclusion, but were not auto-edited:

1. If the thesis calls the system Ă¢â‚¬Å“production-readyĂ¢â‚¬Â, source only verifies local/demo build/test, not production deployment or benchmark.
2. If the thesis calls it Ă¢â‚¬Å“cloud deployedĂ¢â‚¬Â, only database connectivity to Supabase is evidenced; full system cloud deployment is not.
3. If the thesis says all events are branch-isolated, `kitchen-admin` and `Clients.All` contradict that absolute wording.
4. If the thesis says invoice is created at payment, current model has no `Invoice` entity.
5. If the thesis says removed staff-finance module is current functionality, current model no longer contains removed staff-finance module DbSet/entity; historical migration backup is not current implementation.
6. If the thesis says item-level KDS status, source shows request-level status and `SentQuantity` tracking.
7. If the thesis says guest can use AI, `AiController` explicitly forbids guest session tokens.
8. If future work lists inventory/business insights/basic AI tools, those capabilities already exist in current source and should be moved to implementation/evaluation with limitations.

---

## 19. Canonical terminology

| Concept | ThuĂ¡ÂºÂ­t ngĂ¡Â»Â¯ nÄ‚Âªn dÄ‚Â¹ng | TÄ‚Âªn current source | ThuĂ¡ÂºÂ­t ngĂ¡Â»Â¯ cĂ…Â©/sai cĂ¡ÂºÂ§n bĂ¡Â»Â |
|---|---|---|---|
| Order detail | Chi tiĂ¡ÂºÂ¿t Ă„â€˜Ă†Â¡n hÄ‚Â ng | `OrderDetail`, `Order.Details` | `OrderItem` nĂ¡ÂºÂ¿u khÄ‚Â´ng chĂ¡Â»â€° lÄ‚Â  mÄ‚Â´ tĂ¡ÂºÂ£ nghiĂ¡Â»â€¡p vĂ¡Â»Â¥ |
| Guest | KhÄ‚Â¡ch vÄ‚Â£ng lai theo QR | role `customer` + `customerSessionType=guest` | role `guest` nhĂ†Â° JWT role riÄ‚Âªng |
| Customer | KhÄ‚Â¡ch hÄ‚Â ng Ă„â€˜Ă„Æ’ng kÄ‚Â½ | `Customer`/role `customer` | Ă„â€˜Ă¡Â»â€œng nhĂ¡ÂºÂ¥t mĂ¡Â»Âi customer vĂ¡Â»â€ºi guest |
| Branch | Chi nhÄ‚Â¡nh | `Branch`, `BranchId` | `Restaurant`, `RestaurantId` |
| Table | BÄ‚Â n nhÄ‚Â  hÄ‚Â ng | `RestaurantTable`/`Tables` DbSet | `Table` entity nĂ¡ÂºÂ¿u sĂ†Â¡ Ă„â€˜Ă¡Â»â€œ cĂ¡ÂºÂ§n tÄ‚Âªn source |
| Web Order | Ă„ÂĂ†Â¡n web chĂ¡Â»Â tiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n | `Order.Status`, `AcceptWebOrderAsync` | flow tĂ¡ÂºÂ¡o Order mĂ¡Â»â€ºi khi accept |
| POS Order | Ă„ÂĂ†Â¡n tĂ¡ÂºÂ¡o/cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t qua `OrderController` | `Order` | tÄ‚Â¡ch entity POSOrder khÄ‚Â´ng cÄ‚Â³ source |
| Kitchen Request | Ă„ÂĂ¡Â»Â£t yÄ‚Âªu cĂ¡ÂºÂ§u chĂ¡ÂºÂ¿ biĂ¡ÂºÂ¿n | `OrderRequest`, `OrderRequestItem` | item kitchen status |
| KDS | MÄ‚Â n hÄ‚Â¬nh bĂ¡ÂºÂ¿p | `KitchenPage`, `KitchenHub` | app KDS riÄ‚Âªng nĂ¡ÂºÂ¿u khÄ‚Â´ng ghi lÄ‚Â  logical module |
| SentQuantity | SĂ¡Â»â€˜ lĂ†Â°Ă¡Â»Â£ng Ă„â€˜Ä‚Â£ gĂ¡Â»Â­i bĂ¡ÂºÂ¿p | `OrderDetail.SentQuantity` | kitchen status |
| Loyalty | TÄ‚Â­ch Ă„â€˜iĂ¡Â»Æ’m/Ă„â€˜Ă¡Â»â€¢i Ă„â€˜iĂ¡Â»Æ’m | `LoyaltyService`, `LoyaltyTransaction` | discount voucher nĂ¡ÂºÂ¿u khÄ‚Â´ng cÄ‚Â³ source |
| Invoice | MÄ‚Â£/lĂ¡Â»â€¹ch sĂ¡Â»Â­ hÄ‚Â³a Ă„â€˜Ă†Â¡n, cĂ¡ÂºÂ§n thĂ¡ÂºÂ­n trĂ¡Â»Âng | `Order.InvoiceCode`, `InvoiceController` | `Invoice` entity nĂ¡ÂºÂ¿u chĂ†Â°a cÄ‚Â³ |
| Payment | Ghi nhĂ¡ÂºÂ­n thanh toÄ‚Â¡n theo order total | `PayOrderAsync`, payment method cash/transfer | gateway/webhook nĂ¡ÂºÂ¿u khÄ‚Â´ng cÄ‚Â³ integration |
| AI Assistant/Agent | TrĂ¡Â»Â£ lÄ‚Â½ AI dÄ‚Â¹ng AI provider + tools | `AiController`, `AiOrchestrator` | AI truy cĂ¡ÂºÂ­p DbContext trĂ¡Â»Â±c tiĂ¡ÂºÂ¿p |
| Tool Calling | GĂ¡Â»Âi function/tool theo lĂ†Â°Ă¡Â»Â£t | AI provider functionCall + loop | parallel tool execution |
| Business Insights | Insight/KPI phÄ‚Â¢n tÄ‚Â­ch | `BusinessInsight`, `ProactiveInsightService` | chĂ¡Â»â€° mÄ‚Â´ tĂ¡ÂºÂ£ lÄ‚Â  future nĂ¡ÂºÂ¿u source Ă„â€˜Ä‚Â£ cÄ‚Â³ |
| Inventory | TĂ¡Â»â€œn kho theo chi nhÄ‚Â¡nh | `BranchInventory`, stock receipts/issues | ingredient model cĂ…Â© nĂ¡ÂºÂ¿u khÄ‚Â´ng cÄ‚Â²n |
| removed staff-finance module | ChĂ¡Â»Â©c nĂ„Æ’ng lĂ¡Â»â€¹ch sĂ¡Â»Â­/khÄ‚Â´ng current | old migrations only | active current module |

---

## 20. Unsupported / overclaimed statements

### Claim: Ă¢â‚¬Å“TĂ¡ÂºÂ¥t cĂ¡ÂºÂ£ dĂ¡Â»Â¯ liĂ¡Â»â€¡u Ă„â€˜Ă†Â°Ă¡Â»Â£c cÄ‚Â´ lĂ¡ÂºÂ­p tuyĂ¡Â»â€¡t Ă„â€˜Ă¡Â»â€˜i giĂ¡Â»Â¯a cÄ‚Â¡c chi nhÄ‚Â¡nh.Ă¢â‚¬Â

**Status:** PARTIALLY VERIFIED.  
**Evidence:** branch checks trong controllers/services vÄ‚Â  tests; nhĂ†Â°ng `KitchenHub`/`KitchenNotifier` cÄ‚Â³ `kitchen-admin` global, payment dÄ‚Â¹ng `Clients.All`.  
**CÄ‚Â¡ch viĂ¡ÂºÂ¿t an toÄ‚Â n:** Ă¢â‚¬Å“CÄ‚Â¡c endpoint nghiĂ¡Â»â€¡p vĂ¡Â»Â¥ chÄ‚Â­nh kiĂ¡Â»Æ’m tra branch/ownership theo claims; mĂ¡Â»â„¢t sĂ¡Â»â€˜ event dÄ‚Â nh cho Admin Ă„â€˜Ă†Â°Ă¡Â»Â£c phÄ‚Â¡t Ă¡Â»Å¸ nhÄ‚Â³m global vÄ‚Â  cĂ¡ÂºÂ§n Ă„â€˜Ă†Â°Ă¡Â»Â£c mÄ‚Â´ tĂ¡ÂºÂ£ nhĂ†Â° ngoĂ¡ÂºÂ¡i lĂ¡Â»â€¡ thiĂ¡ÂºÂ¿t kĂ¡ÂºÂ¿.Ă¢â‚¬Â

### Claim: Ă¢â‚¬Å“Thanh toÄ‚Â¡n gĂ¡Â»Âi cĂ¡Â»â€¢ng thanh toÄ‚Â¡n/VietQR webhook tĂ¡Â»Â± Ă„â€˜Ă¡Â»â„¢ng.Ă¢â‚¬Â

**Status:** FALSE / NOT VERIFIED.  
**Evidence:** payment endpoint chĂ¡Â»â€° nhĂ¡ÂºÂ­n `Amount`/`PaymentMethod` (`TiĂ¡Â»Ân mĂ¡ÂºÂ·t`, `ChuyĂ¡Â»Æ’n khoĂ¡ÂºÂ£n`), conditional update order, loyalty vÄ‚Â  SignalR; frontend hiĂ¡Â»Æ’n thĂ¡Â»â€¹ Ă¡ÂºÂ£nh VietQR external.  
**CÄ‚Â¡ch viĂ¡ÂºÂ¿t an toÄ‚Â n:** Ă¢â‚¬Å“HĂ¡Â»â€¡ thĂ¡Â»â€˜ng ghi nhĂ¡ÂºÂ­n thanh toÄ‚Â¡n tiĂ¡Â»Ân mĂ¡ÂºÂ·t/chuyĂ¡Â»Æ’n khoĂ¡ÂºÂ£n sau khi Ă„â€˜Ă¡Â»â€˜i chiĂ¡ÂºÂ¿u sĂ¡Â»â€˜ tiĂ¡Â»Ân vĂ¡Â»â€ºi `Order.TotalAmount`; chĂ†Â°a cÄ‚Â³ server-side gateway/webhook Ă„â€˜Ă†Â°Ă¡Â»Â£c xÄ‚Â¡c minh.Ă¢â‚¬Â

### Claim: Ă¢â‚¬Å“Payment tĂ¡ÂºÂ¡o Invoice entity.Ă¢â‚¬Â

**Status:** FALSE / OUTDATED.  
**Evidence:** current DbContext khÄ‚Â´ng cÄ‚Â³ `DbSet<Invoice>`; `Order` cÄ‚Â³ `InvoiceCode`; payment khÄ‚Â´ng add Invoice.  
**CÄ‚Â¡ch viĂ¡ÂºÂ¿t an toÄ‚Â n:** Ă¢â‚¬Å“Order lĂ†Â°u mÄ‚Â£ hÄ‚Â³a Ă„â€˜Ă†Â¡n/lĂ¡Â»â€¹ch sĂ¡Â»Â­ Ă„â€˜Ă†Â°Ă¡Â»Â£c truy vĂ¡ÂºÂ¥n qua cÄ‚Â¡c API hiĂ¡Â»â€¡n cÄ‚Â³; persistence Invoice Ă„â€˜Ă¡Â»â„¢c lĂ¡ÂºÂ­p chĂ†Â°a Ă„â€˜Ă†Â°Ă¡Â»Â£c source chĂ¡Â»Â©ng minh.Ă¢â‚¬Â

### Claim: Ă¢â‚¬Å“KDS cÄ‚Â³ trĂ¡ÂºÂ¡ng thÄ‚Â¡i riÄ‚Âªng cho tĂ¡Â»Â«ng mÄ‚Â³n.Ă¢â‚¬Â

**Status:** FALSE / OUTDATED.  
**Evidence:** `OrderRequest.Status` lÄ‚Â  request-level; `OrderDetail.SentQuantity` dÄ‚Â¹ng tÄ‚Â­nh delta.  
**CÄ‚Â¡ch viĂ¡ÂºÂ¿t an toÄ‚Â n:** Ă¢â‚¬Å“MĂ¡Â»â€”i Ă„â€˜Ă¡Â»Â£t gĂ¡Â»Â­i bĂ¡ÂºÂ¿p cÄ‚Â³ trĂ¡ÂºÂ¡ng thÄ‚Â¡i xĂ¡Â»Â­ lÄ‚Â½; sĂ¡Â»â€˜ lĂ†Â°Ă¡Â»Â£ng Ă„â€˜Ä‚Â£ gĂ¡Â»Â­i Ă„â€˜Ă†Â°Ă¡Â»Â£c theo dÄ‚Âµi trÄ‚Âªn chi tiĂ¡ÂºÂ¿t order.Ă¢â‚¬Â

### Claim: Ă¢â‚¬Å“Guest QR dÄ‚Â¹ng AI.Ă¢â‚¬Â

**Status:** FALSE.  
**Evidence:** `AiController.Chat` trĂ¡ÂºÂ£ `Forbid` khi `customerSessionType=guest`.  
**CÄ‚Â¡ch viĂ¡ÂºÂ¿t an toÄ‚Â n:** Ă¢â‚¬Å“AI dÄ‚Â nh cho customer Ă„â€˜Ä‚Â£ xÄ‚Â¡c thĂ¡Â»Â±c vÄ‚Â  cÄ‚Â¡c role nhÄ‚Â¢n viÄ‚Âªn Ă„â€˜Ă†Â°Ă¡Â»Â£c cĂ¡ÂºÂ¥p; guest QR bĂ¡Â»â€¹ tĂ¡Â»Â« chĂ¡Â»â€˜i.Ă¢â‚¬Â

### Claim: Ă¢â‚¬Å“removed staff-finance module lÄ‚Â  module hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i.Ă¢â‚¬Â

**Status:** FALSE / OUTDATED.  
**Evidence:** current Domain/DbContext khÄ‚Â´ng cÄ‚Â³ removed staff-finance module; chĂ¡Â»â€° thĂ¡ÂºÂ¥y historical migration/backup.  
**CÄ‚Â¡ch viĂ¡ÂºÂ¿t an toÄ‚Â n:** Ă¢â‚¬Å“removed staff-finance module xuĂ¡ÂºÂ¥t hiĂ¡Â»â€¡n trong lĂ¡Â»â€¹ch sĂ¡Â»Â­ schema nhĂ†Â°ng khÄ‚Â´ng thuĂ¡Â»â„¢c current runtime model Ă„â€˜Ă†Â°Ă¡Â»Â£c audit.Ă¢â‚¬Â

### Claim: Ă¢â‚¬Å“AI Ă„â€˜Ă¡ÂºÂ¡t Tool Selection Accuracy/Security Compliance X%.Ă¢â‚¬Â

**Status:** NOT VERIFIED.  
**Evidence:** cÄ‚Â³ behavioral tests, khÄ‚Â´ng cÄ‚Â³ dataset/result artifact/metric definition.  
**CÄ‚Â¡ch viĂ¡ÂºÂ¿t an toÄ‚Â n:** Ă¢â‚¬Å“Ă„ÂÄ‚Â£ kiĂ¡Â»Æ’m thĂ¡Â»Â­ hÄ‚Â nh vi tool/permission bĂ¡ÂºÂ±ng automated tests hiĂ¡Â»â€¡n cÄ‚Â³; chĂ†Â°a xÄ‚Â¡c minh benchmark Ă„â€˜Ă¡Â»â€¹nh lĂ†Â°Ă¡Â»Â£ng.Ă¢â‚¬Â

### Claim: Ă¢â‚¬Å“HĂ¡Â»â€¡ thĂ¡Â»â€˜ng production-ready/highly scalable.Ă¢â‚¬Â

**Status:** NOT VERIFIED.  
**Evidence:** build/test local pass; khÄ‚Â´ng cÄ‚Â³ production deployment, load test hoĂ¡ÂºÂ·c SLO/latency artifact.  
**CÄ‚Â¡ch viĂ¡ÂºÂ¿t an toÄ‚Â n:** Ă¢â‚¬Å“BĂ¡ÂºÂ£n triĂ¡Â»Æ’n khai development/demo Ă„â€˜Ä‚Â£ build vÄ‚Â  test thÄ‚Â nh cÄ‚Â´ng trong mÄ‚Â´i trĂ†Â°Ă¡Â»Âng audit.Ă¢â‚¬Â

---

## 21. Missing content nÄ‚Âªn bĂ¡Â»â€¢ sung vÄ‚Â o khÄ‚Â³a luĂ¡ÂºÂ­n

1. QR guest security: signed table/branch session, token claims, token khÄ‚Â´ng do client tĂ¡Â»Â± chĂ¡Â»Ân.
2. Exact payment concurrency gate: persisted total, conditional update, loyalty earn, commit.
3. Request-level KDS model vÄ‚Â  Ä‚Â½ nghĂ„Â©a `SentQuantity`.
4. Exact SignalR groups/events vÄ‚Â  ngoĂ¡ÂºÂ¡i lĂ¡Â»â€¡ global Admin/`Clients.All`.
5. AI registry gĂ¡Â»â€œm 14 tool, allowed roles, risk level, max 5 sequential iterations.
6. Current inventory model theo `BranchInventory`, stock receipt/issue/transaction.
7. `BusinessInsight`/proactive insight/background service.
8. Current absence of removed staff-finance module/Invoice/Restaurant entity Ă„â€˜Ă¡Â»Æ’ trÄ‚Â¡nh ERD/class diagram lĂ¡Â»â€”i thĂ¡Â»Âi.
9. Current test result 267/267, thay cho sĂ¡Â»â€˜ liĂ¡Â»â€¡u cĂ…Â© chĂ†Â°a chĂ¡ÂºÂ¡y lĂ¡ÂºÂ¡i.
10. Deployment limitation: HTTPS dev/demo port 5000, Supabase connection option, no Docker proof, no production benchmark.

---

## 22. Recommended revised Chapter 3 outline

| MĂ¡Â»Â¥c Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t | Action | LÄ‚Â½ do source-grounded |
|---|---|---|
| 3.1 BĂ¡Â»â€˜i cĂ¡ÂºÂ£nh, actors/capabilities | REVISE | tÄ‚Â¡ch role vÄ‚Â  capability; guest khÄ‚Â´ng phĂ¡ÂºÂ£i role JWT riÄ‚Âªng |
| 3.2 YÄ‚Âªu cĂ¡ÂºÂ§u chĂ¡Â»Â©c nĂ„Æ’ng theo module | REVISE | nhÄ‚Â³m Customer/Guest, POS, KDS, Management, AI |
| 3.3 YÄ‚Âªu cĂ¡ÂºÂ§u phi chĂ¡Â»Â©c nĂ„Æ’ng cÄ‚Â³ evidence | REVISE | chĂ¡Â»â€° giĂ¡Â»Â¯ JWT, branch checks, rate limit, HTTPS, transaction; bĂ¡Â»Â claim Ă„â€˜Ă¡Â»â€¹nh lĂ†Â°Ă¡Â»Â£ng khÄ‚Â´ng cÄ‚Â³ test |
| 3.4 Use Case tĂ¡Â»â€¢ng quÄ‚Â¡t vÄ‚Â  Ă„â€˜Ă¡ÂºÂ·c tĂ¡ÂºÂ£ | REBUILD | bĂ¡Â»Â hierarchy rank; dÄ‚Â¹ng capability abstraction |
| 3.5 Activity/Sequence | REBUILD affected diagrams | bÄ‚Â¡m 5 activity/3 sequence source of truth |
| 3.6 Domain Class vÄ‚Â  ERD | REBUILD | dÄ‚Â¹ng current model; bĂ¡Â»Â Restaurant/Invoice/removed staff-finance module nĂ¡ÂºÂ¿u khÄ‚Â´ng cÄ‚Â³ source |
| 3.7 KiĂ¡ÂºÂ¿n trÄ‚Âºc backend/frontend | REVISE | mÄ‚Â´ tĂ¡ÂºÂ£ partial Clean Architecture vÄ‚Â  two frontend apps |
| 3.8 Order/payment/loyalty/integrity | KEEP + REVISE | Ă„â€˜Ä‚Â¢y lÄ‚Â  flow cÄ‚Â³ evidence mĂ¡ÂºÂ¡nh |
| 3.9 KDS/SignalR | KEEP + REVISE | thÄ‚Âªm groups/events/global limitation |
| 3.10 Authentication/authorization/branch | KEEP + REVISE | trÄ‚Â¬nh bÄ‚Â y endpoint-specific isolation |
| 3.11 AI Agent | KEEP + REVISE | registry/permission/loop/tool matrix |
| 3.12 UI design | KEEP | screens hiĂ¡Â»â€¡n cÄ‚Â³; trÄ‚Â¡nh biĂ¡ÂºÂ¿n UI guard thÄ‚Â nh security claim |

CÄ‚Â¡c mĂ¡Â»Â¥c cĂ…Â© nÄ‚Âªn **MERGE** nĂ¡ÂºÂ¿u lĂ¡ÂºÂ·p lĂ¡ÂºÂ¡i cÄ‚Â¡c phĂ¡ÂºÂ§n 3.4/3.7/3.10; khÄ‚Â´ng cĂ¡ÂºÂ§n thÄ‚Âªm class/entity khÄ‚Â´ng tĂ¡Â»â€œn tĂ¡ÂºÂ¡i.

---

## 23. Recommended revised Chapter 4 outline

| MĂ¡Â»Â¥c Ă„â€˜Ă¡Â»Â xuĂ¡ÂºÂ¥t | Action | NĂ¡Â»â„¢i dung cĂ¡ÂºÂ§n cÄ‚Â³ |
|---|---|---|
| 4.1 MÄ‚Â´i trĂ†Â°Ă¡Â»Âng | KEEP/REVISE | .NET 8, Node/Vite, PostgreSQL/Supabase, HTTPS 5000 |
| 4.2 CĂ¡ÂºÂ¥u trÄ‚Âºc triĂ¡Â»Æ’n khai | REVISE | monorepo, one API, Admin Web, Customer Web, logical POS/KDS |
| 4.3 TriĂ¡Â»Æ’n khai nghiĂ¡Â»â€¡p vĂ¡Â»Â¥ | KEEP | order/payment/loyalty, KDS, inventory, AI |
| 4.4 KiĂ¡Â»Æ’m thĂ¡Â»Â­ method | KEEP | xUnit command vÄ‚Â  mÄ‚Â´i trĂ†Â°Ă¡Â»Âng chĂ¡ÂºÂ¡y thĂ¡Â»Â±c tĂ¡ÂºÂ¿ |
| 4.5 KĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ test | REBUILD data | 267 total, 267 passed, 0 failed, 0 skipped |
| 4.6 Auth/branch/security tests | KEEP/REVISE | claim theo test, khÄ‚Â´ng overclaim endpoint-wide perfection |
| 4.7 Financial tests | KEEP | calculation/concurrency/loyalty limitations |
| 4.8 AI tests/evaluation | REVISE | behavioral tests verified; quantitative benchmark not verified |
| 4.9 KĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ vÄ‚Â  Ă„â€˜Ä‚Â¡nh giÄ‚Â¡ | REVISE | separate build/test evidence from qualitative claims |
| 4.10 Limitations | ADD | no gateway/webhook, no Invoice entity, request-level KDS, global events, no Docker/production proof |
| 4.11 Future work | REVISE | khÄ‚Â´ng liĂ¡Â»â€¡t kÄ‚Âª inventory/basic AI/business insight lÄ‚Â  future nĂ¡ÂºÂ¿u Ă„â€˜Ä‚Â£ implemented |

---

## 24. Diagram revision plan

| Diagram | HiĂ¡Â»â€¡n trĂ¡ÂºÂ¡ng source | Action | NĂ¡Â»â„¢i dung bĂ¡ÂºÂ¯t buĂ¡Â»â„¢c | LoĂ¡ÂºÂ¡i bĂ¡Â»Â |
|---|---|---|---|---|
| Overview Use Case | nhiĂ¡Â»Âu role/capability | REBUILD | capability groups, guest/customer distinction, AI | AdminĂ¢â€ â€™ManagerĂ¢â€ â€™Employee hierarchy |
| Customer/Guest Use Cases | QR/session/order/reservation | KEEP/REVISE | bootstrap, menu, order, own order, reservation | guest AI |
| POS Use Cases | order, web acceptance, payment, loyalty, kitchen send | KEEP/REVISE | accept existing order, separate send kitchen | reject nĂ¡ÂºÂ¿u unsupported, gateway |
| KDS Use Cases | request/status/availability/history | KEEP/REVISE | request-level state | item-level status |
| AI Use Cases | chat/tools/permissions | KEEP | role/risk/loop | direct DB actor edge |
| QR Activity | supported | KEEP/REVISE | signed session, server calculation | client total authority |
| Web Order Activity | supported with caveats | REVISE | transaction/claim/conflict | new order/reject/auto kitchen |
| Payment Activity | supported | REVISE | conditional updateĂ¢â€ â€™loyaltyĂ¢â€ â€™commit | invoice/gateway automatic |
| KDS Activity | supported | REVISE | SentQuantity/request status | item-level kitchen status |
| AI Activity | supported | KEEP/REVISE | max 5 sequential, permission | parallel fragment |
| Payment Sequence | strong evidence | KEEP/REVISE | `Clients.All` event | branch-only event claim |
| KDS Sequence | strong evidence | KEEP/REVISE | exact groups/events/polling | guaranteed delivery claim |
| AI Sequence | strong evidence | KEEP/REVISE | registry/permission/application service | AIĂ¢â€ â€™DbContext |
| Domain Class | likely historical mismatch | REBUILD | current entities | Restaurant/Invoice/removed staff-finance module/OrderItem |
| ERD | current model traceable | REBUILD if old | current DbSets/indexes/FKs | backup-only schema |
| Architecture | layered source | REVISE | partial Clean Architecture, AI path | claim of pure isolation if not proven |
| Auth/Authorization | useful | ADD/REVISE | JWT claims, role gates, branch checks | frontend-only security |
| AI Architecture | useful | KEEP | AI provider, registry, permission, tools | direct DB |
| Deployment | no Docker | REVISE/ADD | HTTPS dev/demo, Supabase option | Docker/production claim |

---

## 25. Tables/Figures revision plan

### NÄ‚Âªn giĂ¡Â»Â¯ hoĂ¡ÂºÂ·c cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t

- actor/role capability table;
- endpoint Ă¢â€ â€™ controller Ă¢â€ â€™ service Ă¢â€ â€™ entity table;
- branch isolation matrix;
- financial formula and payment concurrency table;
- exact AI tool registry matrix;
- current DbSet/entity/relationship table;
- KDS group/event table;
- test result table 267/267;
- deployment environment table.

### NÄ‚Âªn vĂ¡ÂºÂ½ lĂ¡ÂºÂ¡i

- Overview Use Case nĂ¡ÂºÂ¿u cÄ‚Â³ role hierarchy;
- Domain Class/ERD nĂ¡ÂºÂ¿u dÄ‚Â¹ng Restaurant/OrderItem/Invoice/removed staff-finance module cĂ…Â©;
- Payment sequence nĂ¡ÂºÂ¿u bĂ¡Â»Â qua conditional update/loyalty/commit;
- KDS diagrams nĂ¡ÂºÂ¿u vĂ¡ÂºÂ½ item-level status hoĂ¡ÂºÂ·c khÄ‚Â´ng thĂ¡Â»Æ’ hiĂ¡Â»â€¡n global Admin groups;
- AI diagrams nĂ¡ÂºÂ¿u vĂ¡ÂºÂ½ parallel tools/direct database.

### Screenshot nÄ‚Âªn chĂ¡Â»Â¥p

- POS order/payment flow;
- Customer QR menu/order;
- KDS active requests vÄ‚Â  connection/recovery state;
- Admin dashboard/business insight;
- AI chat vĂ¡Â»â€ºi tool result Ă„â€˜Ä‚Â£ Ă¡ÂºÂ©n secret;
- test/build output tĂ¡Â»â€¢ng hĂ¡Â»Â£p.

KhÄ‚Â´ng cĂ¡ÂºÂ§n screenshot mĂ¡Â»Âi CRUD page. KhÄ‚Â´ng Ă„â€˜Ă†Â°a token, connection string, API key, JWT secret hoĂ¡ÂºÂ·c thÄ‚Â´ng tin ngÄ‚Â¢n hÄ‚Â ng nhĂ¡ÂºÂ¡y cĂ¡ÂºÂ£m vÄ‚Â o thesis.

---

## 26. Priority Fix Checklist

### P0 Ă¢â‚¬â€œ Sai source/phĂ¡ÂºÂ£i sĂ¡Â»Â­a

- [ ] XÄ‚Â³a hoĂ¡ÂºÂ·c sĂ¡Â»Â­a claim Invoice entity Ă„â€˜Ă†Â°Ă¡Â»Â£c tĂ¡ÂºÂ¡o khi payment.
- [ ] XÄ‚Â³a hoĂ¡ÂºÂ·c sĂ¡Â»Â­a claim payment gateway/VietQR webhook tĂ¡Â»Â± Ă„â€˜Ă¡Â»â„¢ng.
- [ ] XÄ‚Â³a guest khĂ¡Â»Âi AI use case; guest session bĂ¡Â»â€¹ `Forbid`.
- [ ] SĂ¡Â»Â­a KDS tĂ¡Â»Â« item-level status thÄ‚Â nh request-level status + `SentQuantity`.
- [ ] SĂ¡Â»Â­a OrderItem thÄ‚Â nh `OrderDetail` trong class/ERD/UML.
- [ ] SĂ¡Â»Â­a Restaurant/RestaurantId thÄ‚Â nh Branch/BranchId nĂ¡ÂºÂ¿u Ă„â€˜ang dÄ‚Â¹ng trong diagram.
- [ ] Ă„ÂĂ†Â°a removed staff-finance module ra khĂ¡Â»Âi current runtime model hoĂ¡ÂºÂ·c ghi rÄ‚Âµ lÄ‚Â  historical/outdated.
- [ ] KhÄ‚Â´ng tuyÄ‚Âªn bĂ¡Â»â€˜ branch isolation tuyĂ¡Â»â€¡t Ă„â€˜Ă¡Â»â€˜i vÄ‚Â¬ global Admin SignalR/`Clients.All`.

### P1 Ă¢â‚¬â€œ ThiĂ¡ÂºÂ¿u nĂ¡Â»â„¢i dung quan trĂ¡Â»Âng

- [ ] BĂ¡Â»â€¢ sung QR guest signed session/table/branch evidence.
- [ ] BĂ¡Â»â€¢ sung payment conditional update vÄ‚Â  thĂ¡Â»Â© tĂ¡Â»Â± loyalty/commit.
- [ ] BĂ¡Â»â€¢ sung 14 AI tools, role/risk/branch matrix.
- [ ] BĂ¡Â»â€¢ sung exact KDS event/group names.
- [ ] BĂ¡Â»â€¢ sung current inventory entities vÄ‚Â  branch inventory constraints.
- [ ] BĂ¡Â»â€¢ sung BusinessInsight/proactive insight.
- [ ] BĂ¡Â»â€¢ sung test output thĂ¡ÂºÂ­t: 267/267/0/0.
- [ ] BĂ¡Â»â€¢ sung development/demo deployment limitations.

### P2 Ă¢â‚¬â€œ UML/ERD/thuĂ¡ÂºÂ­t ngĂ¡Â»Â¯

- [ ] Rebuild Overview Use Case theo capability, khÄ‚Â´ng theo cĂ¡ÂºÂ¥p bĂ¡ÂºÂ­c.
- [ ] Rebuild current Domain Class Diagram.
- [ ] Rebuild ERD tĂ¡Â»Â« current DbContext/model snapshot.
- [ ] Ă„ÂÄ‚Â¡nh dĂ¡ÂºÂ¥u Web Order accept khÄ‚Â´ng create Order mĂ¡Â»â€ºi, khÄ‚Â´ng auto send kitchen.
- [ ] Ă„ÂÄ‚Â¡nh dĂ¡ÂºÂ¥u khÄ‚Â´ng cÄ‚Â³ Reject Web Order endpoint Ă„â€˜Ă†Â°Ă¡Â»Â£c chĂ¡Â»Â©ng minh.
- [ ] DÄ‚Â¹ng `customer_get_menu` vÄ‚Â  `get_my_orders` Ă„â€˜Ä‚Âºng tÄ‚Âªn tool runtime.
- [ ] PhÄ‚Â¢n biĂ¡Â»â€¡t UI VietQR image vĂ¡Â»â€ºi server-side payment integration.

### P3 Ă¢â‚¬â€œ CĂ¡ÂºÂ£i thiĂ¡Â»â€¡n hĂ¡Â»Âc thuĂ¡ÂºÂ­t/trÄ‚Â¬nh bÄ‚Â y

- [ ] TÄ‚Â¡ch rÄ‚Âµ design, implementation, test evidence vÄ‚Â  limitation.
- [ ] KhÄ‚Â´ng dÄ‚Â¹ng Ă¢â‚¬Å“enterprise-gradeĂ¢â‚¬Â, Ă¢â‚¬Å“highly scalableĂ¢â‚¬Â, Ă¢â‚¬Å“production-readyĂ¢â‚¬Â, Ă¢â‚¬Å“guaranteed realtimeĂ¢â‚¬Â nĂ¡ÂºÂ¿u khÄ‚Â´ng cÄ‚Â³ benchmark.
- [ ] CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t mĂ¡Â»Â¥c lĂ¡Â»Â¥c ChĂ†Â°Ă†Â¡ng 3/4 theo module vÄ‚Â  evidence.
- [ ] Ghi date/command/environment cho kĂ¡ÂºÂ¿t quĂ¡ÂºÂ£ test.
- [ ] Ă„ÂĂ†Â°a future work Ă„â€˜Ä‚Â£ implemented sang phĂ¡ÂºÂ§n triĂ¡Â»Æ’n khai/kĂ¡ÂºÂ¿t quĂ¡ÂºÂ£.

---

## 27. Final verdict

1. **ChĂ†Â°Ă†Â¡ng 3 khĂ¡Â»â€ºp source Ă¡Â»Å¸ mĂ¡Â»Â©c nÄ‚Â o?** ChĂ†Â°a thĂ¡Â»Æ’ chĂ¡ÂºÂ¥m toÄ‚Â n vĂ„Æ’n vÄ‚Â¬ current repository khÄ‚Â´ng chĂ¡Â»Â©a bĂ¡ÂºÂ£n editable Ă„â€˜Ă¡ÂºÂ§y Ă„â€˜Ă¡Â»Â§ cĂ¡Â»Â§a ChĂ†Â°Ă†Â¡ng 3; theo source basis, cÄ‚Â¡c module chÄ‚Â­nh khĂ¡Â»â€ºp Ă¡Â»Å¸ mĂ¡Â»Â©c **PARTIALLY VERIFIED**, do cÄ‚Â³ cÄ‚Â¡c Ă„â€˜iĂ¡Â»Æ’m dĂ¡Â»â€¦ sai vĂ¡Â»Â entity names, removed staff-finance module/invoice, KDS status, guest AI vÄ‚Â  SignalR isolation.
2. **ChĂ†Â°Ă†Â¡ng 4 khĂ¡Â»â€ºp source Ă¡Â»Å¸ mĂ¡Â»Â©c nÄ‚Â o?** **PARTIALLY VERIFIED** cho Ă„â€˜Ă¡ÂºÂ¿n khi cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t test/deployment/limitation; build/test hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i cung cĂ¡ÂºÂ¥p evidence tĂ¡Â»â€˜t cho implementation.
3. **Sai nghiÄ‚Âªm trĂ¡Â»Âng nhĂ¡ÂºÂ¥t:** Invoice/payment gateway, removed staff-finance module current status, item-level KDS, guest AI, absolute branch isolation, production/benchmark claims.
4. **CÄ‚Â³ thĂ¡Â»Æ’ giĂ¡Â»Â¯:** backend/frontend architecture overview, QR ordering core, server financial calculation, payment conditional gate, KDS request flow, AI registry/orchestrator, current test method.
5. **ChĂ¡Â»â€° cĂ¡ÂºÂ§n chĂ¡Â»â€°nh nhĂ¡ÂºÂ¹:** terminology, build/test numbers, environment/HTTPS wording, screenshot descriptions.
6. **Overview Use Case actors/use cases:** capability groups cho Customer/Guest, Staff/POS, KDS, Management, Admin/Manager, AI Assistant; use cases menu/order/payment/KDS/management/AI/inventory/insights.
7. **Actor generalization phÄ‚Â¹ hĂ¡Â»Â£p:** Authenticated Staff User, Customer User, Management User, Order Operations User Ă¡Â»Å¸ mĂ¡Â»Â©c layout cÄ‚Â³ ghi giĂ¡Â»â€ºi hĂ¡ÂºÂ¡n; khÄ‚Â´ng generalize theo rank.
8. **Activity phĂ¡ÂºÂ£i sĂ¡Â»Â­a:** Web Order Acceptance, Payment & Loyalty, KDS; QR vÄ‚Â  AI cĂ¡ÂºÂ§n cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t guard/loop/limitation.
9. **Sequence phĂ¡ÂºÂ£i sĂ¡Â»Â­a:** cĂ¡ÂºÂ£ ba nÄ‚Âªn ghi exact source chain; Ă„â€˜Ă¡ÂºÂ·c biĂ¡Â»â€¡t Payment `Clients.All`, KDS global admin groups, AI max 5 sequential.
10. **Domain Class Diagram:** chĂ¡Â»â€° khĂ¡Â»â€ºp nĂ¡ÂºÂ¿u dÄ‚Â¹ng current entities; class diagram cÄ‚Â³ Restaurant/Invoice/removed staff-finance module/OrderItem lÄ‚Â  khÄ‚Â´ng khĂ¡Â»â€ºp.
11. **ERD:** current model traceable tĂ¡Â»Â« DbContext/migrations; thesis ERD match chĂ†Â°a xÄ‚Â¡c minh vÄ‚Â¬ khÄ‚Â´ng cÄ‚Â³ bĂ¡ÂºÂ£n ERD/Chapter 3 Ă„â€˜Ă¡ÂºÂ§y Ă„â€˜Ă¡Â»Â§ trong repository.
12. **Architecture Diagram:** thĂ¡Â»Æ’ hiĂ¡Â»â€¡n monorepo, one API, two frontend apps, logical POS/KDS, partial Clean Architecture, AI tool pipeline.
13. **BĂ¡ÂºÂ£ng phĂ¡ÂºÂ£i cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t:** actor/role, endpoint-use case, branch matrix, financial formula, AI tool matrix, DbSet/relationship, SignalR groups/events, test result, deployment.
14. **HÄ‚Â¬nh phĂ¡ÂºÂ£i vĂ¡ÂºÂ½ lĂ¡ÂºÂ¡i:** Overview Use Case, Domain Class, ERD vÄ‚Â  cÄ‚Â¡c Activity/Sequence nÄ‚Âªu Ă¡Â»Å¸ mĂ¡Â»Â¥c 24.
15. **Test thĂ¡Â»Â±c tĂ¡ÂºÂ¿:** Total 267 / Passed 267 / Failed 0 / Skipped 0.
16. **AI Evaluation VERIFIED:** behavioral controller/orchestrator/security/tool validation tests. Quantitative benchmark: NOT VERIFIED.
17. **Claim phĂ¡ÂºÂ£i xÄ‚Â³a/sĂ¡Â»Â­a:** gateway/webhook, Invoice entity, removed staff-finance module current, guest AI, item-level KDS, absolute branch isolation, production-ready/scalable without artifact.
18. **Claim cĂ¡ÂºÂ§n viĂ¡ÂºÂ¿t thĂ¡ÂºÂ­n trĂ¡Â»Âng:** Clean Architecture, financial consistency, realtime, cloud deployment, AI quality/security percentages.
19. **ChĂ¡Â»Â©c nĂ„Æ’ng source cÄ‚Â³ nhĂ†Â°ng thesis dĂ¡Â»â€¦ thiĂ¡ÂºÂ¿u:** inventory current model, BusinessInsight/proactive insight, QR token hardening, request-level KDS, 14 AI tools, Admin/global SignalR behavior.
20. **CÄ‚Â³ cĂ¡ÂºÂ§n Ă„â€˜Ă¡Â»â€¢i mĂ¡Â»Â¥c lĂ¡Â»Â¥c ChĂ†Â°Ă†Â¡ng 3?** CÄ‚Â³, nÄ‚Âªn merge/reorder theo source-grounded modules vÄ‚Â  tÄ‚Â¡ch UML/ERD/current model rÄ‚Âµ hĂ†Â¡n.
21. **CÄ‚Â³ cĂ¡ÂºÂ§n Ă„â€˜Ă¡Â»â€¢i mĂ¡Â»Â¥c lĂ¡Â»Â¥c ChĂ†Â°Ă†Â¡ng 4?** CÄ‚Â³, phĂ¡ÂºÂ£i cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t test result thĂ¡ÂºÂ­t, tÄ‚Â¡ch development/demo khĂ¡Â»Âi production, thÄ‚Âªm limitation vÄ‚Â  phÄ‚Â¢n biĂ¡Â»â€¡t behavioral AI tests vĂ¡Â»â€ºi benchmark.
22. **ThĂ¡Â»Â© tĂ¡Â»Â± sĂ¡Â»Â­a hĂ¡Â»Â£p lÄ‚Â½:** (1) canonical terminology/entity model; (2) actors/use cases; (3) ERD/domain/architecture; (4) five activity + three sequence diagrams; (5) auth/branch/AI tables; (6) implementation chapter; (7) test/deployment results; (8) limitations/future work; (9) cross-chapter final consistency pass.

---

## 28. Audit completion record

### Files read as primary evidence

```text
AGENTS.md
Program.cs
services/api/RestaurantPOS.api.csproj
services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs
services/api/src/Domain/Entities/*.cs
services/api/src/WebAPI/Controllers/*.cs
services/api/src/AI/**/*.cs
services/api/src/Application/Services/{OrderService,KitchenService,LoyaltyService,...}.cs
services/api/src/WebAPI/Hubs/{KitchenHub,KitchenNotifier}.cs
services/api/src/Infrastructure/Persistence/Migrations/*.cs
apps/admin-web/src/**/*.tsx
apps/customer-web/src/**/*.tsx
apps/*/package.json
tests/RestaurantPOS.Tests/**/*.cs
```

### Build/test record

```text
Backend build: PASS (0 warning, 0 error)
Tests: PASS (267 total, 267 passed, 0 failed, 0 skipped)
Admin Web build: PASS (bundle-size warning only)
Customer Web build: PASS
```

### Scope confirmation

- Audit type: READ-ONLY.
- Source code intentionally modified by this audit: NO.
- Database modified: NO.
- Migration applied or changed by this audit: NO.
- Thesis source files modified by this audit: NO.
- Audit output intentionally created/updated: `baocaokhoaluan/THESIS-SOURCE-OF-TRUTH-AUDIT.md` only.
- No commit or push performed.

## THESIS SOURCE AUDIT COMPLETE Ă¢â‚¬â€ READY FOR CHAPTER 3/4 REVISION




## Current AI evidence note — 17/10/2026

This audit preserves historical source-of-truth findings. For current Sentence Embedding status, use `../../02-tai-lieu-ky-thuat/ai-agent/AI-EMBEDDING-MANAGER-ACCEPTANCE-FINAL.md`: only manager and two read intents have live UAT evidence; the pilot is pending final sign-off, production is NO-GO, and MockAiProvider remains.
