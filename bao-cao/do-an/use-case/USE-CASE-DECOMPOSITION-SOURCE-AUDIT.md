# USE CASE SOURCE AUDIT

## 1. Current source/checkpoint baseline

This is a read-only audit. No source, tests, documentation, database, migration, or Git state was modified.

Authoritative evidence inspected:

- `../../tai-lieu-ky-thuat/system-audit/CODEX-CURRENT-STATE.md`
- `15-THESIS-SOURCE-OF-TRUTH.md`
- `48-ROLE-FUNCTION-MATRIX.md`
- `49-MANAGER-BRANCH-ISOLATION-AUDIT.md`
- `50-UNIFIED-STAFF-AUTHENTICATION.md`
- `51-PUBLIC-GUEST-CUSTOMER-AI-SECURITY.md`
- `52-GUEST-QR-SESSION-SECURITY.md`
- Current WebAPI controllers and application services
- Customer Web and Admin Web routes/components
- AI controllers, tools, and permission checks

Current executable baseline:

- ASP.NET Core 8 API with EF Core/PostgreSQL.
- Customer Web and Admin Web React/Vite applications.
- 23 backend controllers and approximately 117 HTTP actions.
- JWT authentication with role and optional branch claims.
- SignalR kitchen communication.
- AI assistant with AI provider function calling and permissioned tools.
- Guest QR bootstrap and browser validation are complete.
- AI is tool-backed assistance, not a general autonomous agent.

The thesis documentation contains some historical discrepancies. Current executable source takes precedence.

## 2. Verified actors and role hierarchy

| Actor | Authentication | Scope | Verified capabilities |
|---|---|---|---|
| Guest QR customer | Guest customer JWT, `customerSessionType=guest` | One trusted QR table and branch | View menu, select items, manage cart, submit order, access only matching order context |
| Registered Customer | Customer JWT, `customerSessionType=registered` | Own customer identity | Profile, orders, reservations, loyalty history, customer AI, menu, reservation |
| Anonymous/Public visitor | No JWT | Public catalog and selected bootstrap endpoints | View active branches/products/toppings/promotions, submit public reservation, customer bootstrap flows |
| Employee | Employee JWT role `employee` where such an account/token exists | Assigned branch and own staff functions | Attendance, own schedule/profile, operational inventory read model, orders according to endpoint |
| Cashier | Employee JWT role `cashier` | Assigned branch | POS orders, web-order acceptance, payment, shifts, attendance, reservations, invoices |
| Kitchen | Employee JWT role `kitchen` | Assigned branch | KDS requests, kitchen statuses/history, product availability, attendance and schedule |
| Manager | Employee JWT role `manager` | Assigned branch | Branch-scoped management, employees, schedules, expenses, settings, inventory, dashboard, insights |
| Admin | Employee JWT role `admin` | Global or selected branch | Global management, branches, employees, catalog, settings, finance, reporting |

Authentication groups:

- Management login accepts stored roles `admin` and `manager`.
- Staff login accepts stored roles `employee`, `cashier`, and `kitchen`.
- `Position` is descriptive and is not an authorization source.
- Frontend guards are UX only; backend authorization is authoritative.

## 3. UC-D01 Ă¢â‚¬â€ Ă„ÂĂ¡ÂºÂ·t mÄ‚Â³n qua QR/Web

### Scope

This use case is strongly supported and should remain a separate decomposition diagram.

Current customer entry points:

- Normal Customer Web link: `/`
- Table QR link: `/?qr=<QrToken>`
- QR scan page: `/scan`

The QR flow is:

`QR token Ă¢â€ â€™ POST /api/guest/bootstrap Ă¢â€ â€™ trusted table and branch Ă¢â€ â€™ guest JWT Ă¢â€ â€™ menu Ă¢â€ â€™ cart Ă¢â€ â€™ POST /api/Order`

Customer login is not required for Guest QR ordering. Registered customer login is optional for customer-specific functions such as profile, history, loyalty, and AI.

### Evidence table

| ID | Child Use Case | Actor | Guest/Customer/Auth | Frontend Evidence | API Evidence | Service Evidence | UML Type | Include/Extend Target | Confidence |
|---|---|---|---|---|---|---|---|---|---|
| D01-01 | Truy cĂ¡ÂºÂ­p Customer Web | Anonymous, Guest, Customer | No auth for public entry | `customer-web/src/App.tsx` | React routing | N/A | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D01-02 | QuÄ‚Â©t/truy cĂ¡ÂºÂ­p QR bÄ‚Â n | Guest | QR token bootstrap | `QRScan.tsx`, `DigitalMenu.tsx` | `POST /api/guest/bootstrap` | Guest bootstrap service/context | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D01-03 | XÄ‚Â¡c lĂ¡ÂºÂ­p phiÄ‚Âªn QR tin cĂ¡ÂºÂ­y | System behavior initiated by Guest QR entry | Guest JWT created by server | QR bootstrap state in `DigitalMenu.tsx` | `POST /api/guest/bootstrap` | Resolves active table and branch, signs claims | Include Candidate | `Ă„ÂĂ¡ÂºÂ·t mÄ‚Â³n qua QR/Web` | HIGH |
| D01-04 | Xem thĂ¡Â»Â±c Ă„â€˜Ă†Â¡n | Guest, Customer, Anonymous | Public/guest/customer | `DigitalMenu.tsx` | `GET /api/Product`, `GET /api/Topping`, `GET /api/Promotion` | Product/Topping catalog queries | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D01-05 | TÄ‚Â¬m kiĂ¡ÂºÂ¿m/lĂ¡Â»Âc mÄ‚Â³n | Guest, Customer | Public/guest/customer | Product search/category state in `DigitalMenu.tsx` | `GET /api/Product?search=&category=` | Product query filtering | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D01-06 | Xem chi tiĂ¡ÂºÂ¿t mÄ‚Â³n | Guest, Customer | Public/guest/customer | Product detail/customization modal | `GET /api/Product` | Product catalog data | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D01-07 | Xem/chĂ¡Â»Ân topping vÄ‚Â  size | Guest, Customer | Public/guest/customer | Topping and size customization in `DigitalMenu.tsx` | `GET /api/Topping` and product data | Backend validates topping/product IDs and authoritative prices | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D01-08 | ChĂ¡Â»Ân mÄ‚Â³n | Guest, Customer | No customer login required | `DigitalMenu.tsx:addToCart` | Later submitted through `/api/Order` | `OrderService.CreateOrUpdateOrderAsync` | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D01-09 | QuĂ¡ÂºÂ£n lÄ‚Â½ giĂ¡Â»Â hÄ‚Â ng | Guest, Customer | Client-side temporary state | `DigitalMenu.tsx` cart controls | No dedicated cart endpoint | Frontend-only state | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D01-10 | GĂ¡Â»Â­i yÄ‚Âªu cĂ¡ÂºÂ§u gĂ¡Â»Âi mÄ‚Â³n | Guest, Customer | Authenticated Guest JWT or customer JWT | `DigitalMenu.tsx:confirmOrder` | `POST /api/Order` | `OrderService.CreateOrUpdateOrderAsync` | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D01-11 | TĂ¡ÂºÂ¡o/cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t Order Ă„â€˜ang xĂ¡Â»Â­ lÄ‚Â½ | System behavior | Executed as part of submission | Order payload creation in `DigitalMenu.tsx` | `POST /api/Order` | `OrderService` merges or creates pending order and recalculates prices | Include Candidate | `GĂ¡Â»Â­i yÄ‚Âªu cĂ¡ÂºÂ§u gĂ¡Â»Âi mÄ‚Â³n` | HIGH |
| D01-12 | TiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n yÄ‚Âªu cĂ¡ÂºÂ§u Web | Cashier, Employee, Manager, Admin | Authenticated operational staff | POS pending-order UI in `POSPage.tsx` | `POST /api/Order/{id}/accept` | `OrderService.AcceptWebOrderAsync` | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D01-13 | ChĂ¡Â»Ân bÄ‚Â n khi tiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n | Cashier, Employee, Manager, Admin | Staff only | POS accept dialog | `POST /api/Order/{id}/accept` with `TableId` | `AcceptWebOrderAsync` validates branch/table | Include Candidate | `TiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n yÄ‚Âªu cĂ¡ÂºÂ§u Web` | HIGH |
| D01-14 | TĂ¡Â»Â« chĂ¡Â»â€˜i yÄ‚Âªu cĂ¡ÂºÂ§u Web | Staff | No verified rejection endpoint | No confirmed rejection action in current POS flow | No dedicated reject action found | No authoritative service method | Unsupported Ă¢â‚¬â€ Exclude | Ă¢â‚¬â€ | HIGH |
| D01-15 | GĂ¡Â»Â­i Ă„â€˜Ă†Â¡n xuĂ¡Â»â€˜ng bĂ¡ÂºÂ¿p | Cashier, Employee, Manager, Kitchen | Authenticated staff | POS action | `POST /api/Order/{id}/send-to-kitchen` | Kitchen service creates/updates kitchen request | Actor Use Case | Ă¢â‚¬â€ | HIGH |

### Direct actor associations

- `Anonymous -> Truy cĂ¡ÂºÂ­p Customer Web`
- `Guest QR customer -> QuÄ‚Â©t/truy cĂ¡ÂºÂ­p QR bÄ‚Â n`
- `Guest QR customer -> Xem thĂ¡Â»Â±c Ă„â€˜Ă†Â¡n`
- `Guest QR customer -> TÄ‚Â¬m kiĂ¡ÂºÂ¿m/lĂ¡Â»Âc mÄ‚Â³n`
- `Guest QR customer -> Xem chi tiĂ¡ÂºÂ¿t mÄ‚Â³n`
- `Guest QR customer -> Xem/chĂ¡Â»Ân topping vÄ‚Â  size`
- `Guest QR customer -> ChĂ¡Â»Ân mÄ‚Â³n`
- `Guest QR customer -> QuĂ¡ÂºÂ£n lÄ‚Â½ giĂ¡Â»Â hÄ‚Â ng`
- `Guest QR customer -> GĂ¡Â»Â­i yÄ‚Âªu cĂ¡ÂºÂ§u gĂ¡Â»Âi mÄ‚Â³n`
- `Registered Customer -> Xem thĂ¡Â»Â±c Ă„â€˜Ă†Â¡n`
- `Registered Customer -> QuĂ¡ÂºÂ£n lÄ‚Â½ giĂ¡Â»Â hÄ‚Â ng`
- `Registered Customer -> GĂ¡Â»Â­i yÄ‚Âªu cĂ¡ÂºÂ§u gĂ¡Â»Âi mÄ‚Â³n`
- `Cashier -> TiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n yÄ‚Âªu cĂ¡ÂºÂ§u Web`
- `Employee -> TiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n yÄ‚Âªu cĂ¡ÂºÂ§u Web`
- `Manager -> TiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n yÄ‚Âªu cĂ¡ÂºÂ§u Web`
- `Admin -> TiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n yÄ‚Âªu cĂ¡ÂºÂ§u Web`
- `Cashier/Employee/Manager/Kitchen -> GĂ¡Â»Â­i Ă„â€˜Ă†Â¡n xuĂ¡Â»â€˜ng bĂ¡ÂºÂ¿p`

### Valid `<<include>>` relationships

- `Ă„ÂĂ¡ÂºÂ·t mÄ‚Â³n qua QR/Web -> Xem thĂ¡Â»Â±c Ă„â€˜Ă†Â¡n` Ă¢â‚¬â€ the customer must access menu data to select an item.
- `Ă„ÂĂ¡ÂºÂ·t mÄ‚Â³n qua QR/Web -> QuĂ¡ÂºÂ£n lÄ‚Â½ giĂ¡Â»Â hÄ‚Â ng` Ă¢â‚¬â€ the current flow requires a cart before submission.
- `GĂ¡Â»Â­i yÄ‚Âªu cĂ¡ÂºÂ§u gĂ¡Â»Âi mÄ‚Â³n -> TĂ¡ÂºÂ¡o/cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t Order Ă„â€˜ang xĂ¡Â»Â­ lÄ‚Â½` Ă¢â‚¬â€ the endpoint always delegates to the authoritative order service.
- `QuÄ‚Â©t/truy cĂ¡ÂºÂ­p QR bÄ‚Â n -> XÄ‚Â¡c lĂ¡ÂºÂ­p phiÄ‚Âªn QR tin cĂ¡ÂºÂ­y` Ă¢â‚¬â€ a QR entry must bootstrap the server-resolved table and branch before guest ordering.

### Valid `<<extend>>` relationships

- `QuÄ‚Â©t/truy cĂ¡ÂºÂ­p QR bÄ‚Â n -> Ă„ÂĂ¡ÂºÂ·t mÄ‚Â³n qua QR/Web` Ă¢â‚¬â€ conditional extension when the customer enters through a QR token rather than a normal link.
- `ChĂ¡Â»Ân topping vÄ‚Â  size -> ChĂ¡Â»Ân mÄ‚Â³n` Ă¢â‚¬â€ conditional extension when the selected product has configurable size or toppings.
- `TiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n yÄ‚Âªu cĂ¡ÂºÂ§u Web -> GĂ¡Â»Â­i yÄ‚Âªu cĂ¡ÂºÂ§u gĂ¡Â»Âi mÄ‚Â³n` Ă¢â‚¬â€ operational continuation only if staff handling is represented in the same broad system diagram. It is better shown as a separate staff association or activity sequence.

### Relationships NOT to draw

Do not draw these as UML `include` or `extend` relationships:

- `Truy cĂ¡ÂºÂ­p Web -> Xem thĂ¡Â»Â±c Ă„â€˜Ă†Â¡n` merely because it happens next.
- `Xem thĂ¡Â»Â±c Ă„â€˜Ă†Â¡n -> ChĂ¡Â»Ân mÄ‚Â³n` merely because it is chronological.
- `ChĂ¡Â»Ân mÄ‚Â³n -> QuĂ¡ÂºÂ£n lÄ‚Â½ giĂ¡Â»Â hÄ‚Â ng` as a mandatory UML dependency.
- `GĂ¡Â»Â­i yÄ‚Âªu cĂ¡ÂºÂ§u gĂ¡Â»Âi mÄ‚Â³n -> TiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n yÄ‚Âªu cĂ¡ÂºÂ§u Web` as an include; the latter is a later staff action.
- `GĂ¡Â»Â­i yÄ‚Âªu cĂ¡ÂºÂ§u gĂ¡Â»Âi mÄ‚Â³n -> GĂ¡Â»Â­i Ă„â€˜Ă†Â¡n xuĂ¡Â»â€˜ng bĂ¡ÂºÂ¿p`; kitchen dispatch is a later operational step.
- JWT claim parsing, table lookup, branch lookup, DTO mapping, or EF queries as business use cases.

### Business flow

`Customer opens Web or scans table QR Ă¢â€ â€™ optional QR bootstrap Ă¢â€ â€™ view menu Ă¢â€ â€™ search/filter Ă¢â€ â€™ view details Ă¢â€ â€™ select size/toppings Ă¢â€ â€™ add to cart Ă¢â€ â€™ edit cart Ă¢â€ â€™ submit order Ă¢â€ â€™ backend validates items/prices/context Ă¢â€ â€™ pending Order is created or merged Ă¢â€ â€™ staff receives request Ă¢â€ â€™ staff accepts Ă¢â€ â€™ order can be sent to kitchen`

### Diagram recommendation

**KEEP.**

Recommended main child use cases:

- Xem thĂ¡Â»Â±c Ă„â€˜Ă†Â¡n
- TÄ‚Â¬m kiĂ¡ÂºÂ¿m/lĂ¡Â»Âc mÄ‚Â³n
- Xem chi tiĂ¡ÂºÂ¿t mÄ‚Â³n
- ChĂ¡Â»Ân topping/size
- QuĂ¡ÂºÂ£n lÄ‚Â½ giĂ¡Â»Â hÄ‚Â ng
- GĂ¡Â»Â­i yÄ‚Âªu cĂ¡ÂºÂ§u gĂ¡Â»Âi mÄ‚Â³n
- Optional QR entry extension

Do not include `TĂ¡Â»Â« chĂ¡Â»â€˜i yÄ‚Âªu cĂ¡ÂºÂ§u Web` because no current executable rejection flow was found.

## 4. UC-D02 Ă¢â‚¬â€ Theo dÄ‚Âµi trĂ¡ÂºÂ¡ng thÄ‚Â¡i vÄ‚Â  lĂ¡Â»â€¹ch sĂ¡Â»Â­ Ă„â€˜Ă†Â¡n hÄ‚Â ng

### Scope

This is supported for Registered Customers, but not as a complete Guest QR history diagram.

Registered customers can view order history and order details from Customer Web. Guest QR users can access only table/session-matching order data through secured APIs, but the current Guest Web UI does not expose a full order-history screen.

### Evidence table

| ID | Child Use Case | Actor | Guest/Customer/Auth | Frontend Evidence | API Evidence | Service Evidence | UML Type | Include/Extend Target | Confidence |
|---|---|---|---|---|---|---|---|---|---|
| D02-01 | Xem Ă„â€˜Ă†Â¡n hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i | Registered Customer, Guest | Authenticated | Customer profile for registered users; Guest UI has success state but no history page | `GET /api/Order` and `GET /api/Order/{id}` | `OrderService` ownership/session checks | Actor Use Case | Ă¢â‚¬â€ | MEDIUM |
| D02-02 | Xem trĂ¡ÂºÂ¡ng thÄ‚Â¡i Ă„â€˜Ă†Â¡n | Registered Customer, Guest | Authenticated | Registered profile displays `status`; Guest API capability exists | `GET /api/Order/{id}` | Guest table/branch ownership checks | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D02-03 | Xem chi tiĂ¡ÂºÂ¿t Ă„â€˜Ă†Â¡n | Registered Customer, Guest | Authenticated | `CustomerProfile.tsx` order detail modal | `GET /api/Order/{id}` | Ownership and branch/session checks | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D02-04 | Xem lĂ¡Â»â€¹ch sĂ¡Â»Â­ Ă„â€˜Ă†Â¡n hÄ‚Â ng | Registered Customer | Registered JWT | `CustomerProfile.tsx` calls `/api/Order?customerPhone=` | `GET /api/Order?customerPhone=` | `GetOrdersAsync` / customer ownership | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D02-05 | LĂ¡Â»Âc lĂ¡Â»â€¹ch sĂ¡Â»Â­ Ă„â€˜Ă†Â¡n | Registered Customer | Registered JWT | No clear customer-facing filter control found | Query supports filters, but no dedicated Customer Web filter UI | Order query filtering | Unsupported Ă¢â‚¬â€ Exclude | Ă¢â‚¬â€ | MEDIUM |
| D02-06 | Xem thÄ‚Â´ng tin thanh toÄ‚Â¡n/hÄ‚Â³a Ă„â€˜Ă†Â¡n | Registered Customer | Customer order detail | Order detail displays totals and payment-related fields where present | `GET /api/Order/{id}` | Persisted order/payment data | Actor Use Case | Ă¢â‚¬â€ | MEDIUM |
| D02-07 | Theo dÄ‚Âµi trĂ¡ÂºÂ¡ng thÄ‚Â¡i bĂ¡ÂºÂ¿p | Staff, not Customer Web | Staff JWT | POS/KDS uses kitchen-status endpoint | `GET /api/Order/{id}/kitchen-status` | Kitchen service | Actor Use Case | Ă¢â‚¬â€ | HIGH |

### Direct actor associations

- `Registered Customer -> Xem trĂ¡ÂºÂ¡ng thÄ‚Â¡i Ă„â€˜Ă†Â¡n`
- `Registered Customer -> Xem chi tiĂ¡ÂºÂ¿t Ă„â€˜Ă†Â¡n`
- `Registered Customer -> Xem lĂ¡Â»â€¹ch sĂ¡Â»Â­ Ă„â€˜Ă†Â¡n hÄ‚Â ng`
- `Registered Customer -> Xem thÄ‚Â´ng tin thanh toÄ‚Â¡n/hÄ‚Â³a Ă„â€˜Ă†Â¡n`
- `Guest QR customer -> Xem trĂ¡ÂºÂ¡ng thÄ‚Â¡i/chi tiĂ¡ÂºÂ¿t matching order` Ă¢â‚¬â€ API-supported, UI evidence limited.
- `Cashier/Employee/Manager/Kitchen -> Theo dÄ‚Âµi trĂ¡ÂºÂ¡ng thÄ‚Â¡i bĂ¡ÂºÂ¿p`

### Valid relationships

- `Xem lĂ¡Â»â€¹ch sĂ¡Â»Â­ Ă„â€˜Ă†Â¡n hÄ‚Â ng -> Xem chi tiĂ¡ÂºÂ¿t Ă„â€˜Ă†Â¡n` may be an include only if the diagram models the detail view as mandatory for every history item. Otherwise keep them as separate actor use cases.
- `Xem Ă„â€˜Ă†Â¡n hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i -> Xem trĂ¡ÂºÂ¡ng thÄ‚Â¡i Ă„â€˜Ă†Â¡n` is a plausible include if Ă¢â‚¬Å“current orderĂ¢â‚¬Â is explicitly defined as always displaying status.
- Do not use `extend` for status transitions; status changes are staff/system actions.

### Relationships NOT to draw

- `Xem lĂ¡Â»â€¹ch sĂ¡Â»Â­ -> Xem trĂ¡ÂºÂ¡ng thÄ‚Â¡i` as a chronological arrow.
- `Xem Ă„â€˜Ă†Â¡n -> Parse JWT`, `Check ownership`, or `Load order`.
- `Guest QR -> Xem lĂ¡Â»â€¹ch sĂ¡Â»Â­ Ă„â€˜Ă†Â¡n hÄ‚Â ng` as a full UI capability; current Customer Web does not provide it.

### Business flow

`Registered customer opens profile Ă¢â€ â€™ backend resolves customer identity Ă¢â€ â€™ fetches orders/reservations/loyalty Ă¢â€ â€™ customer selects an order Ă¢â€ â€™ detail modal displays status, items, totals, and available persisted payment data`

### Diagram recommendation

**KEEP, but title it Ă¢â‚¬Å“Theo dÄ‚Âµi Ă„â€˜Ă†Â¡n hÄ‚Â ng cĂ¡Â»Â§a khÄ‚Â¡ch hÄ‚Â ngĂ¢â‚¬Â rather than implying complete Guest history.**

Guest QR order status/detail can be shown as a constrained variant or note in UC-D01. Registered customer history deserves separate thesis treatment because it includes profile, order history, loyalty, and reservation history.

## 5. UC-D03 Ă¢â‚¬â€ Ă„ÂĂ¡ÂºÂ·t bÄ‚Â n trĂ¡Â»Â±c tuyĂ¡ÂºÂ¿n

### Scope

Fully supported.

Customer Web provides public reservation creation. Registered customers can view, update, and delete their own reservations. Staff can update reservation status and manage reservations within branch scope.

### Evidence table

| ID | Child Use Case | Actor | Guest/Customer/Auth | Frontend Evidence | API Evidence | Service Evidence | UML Type | Include/Extend Target | Confidence |
|---|---|---|---|---|---|---|---|---|---|
| D03-01 | Xem chi nhÄ‚Â¡nh | Anonymous, Customer | Public | `ReservationPage.tsx` | `GET /api/Branch` | Branch query | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D03-02 | ChĂ¡Â»Ân bÄ‚Â n, thĂ¡Â»Âi gian vÄ‚Â  sĂ¡Â»â€˜ khÄ‚Â¡ch | Anonymous, Customer | Public | `ReservationPage.tsx` loads tables and collects fields | `GET /api/Table`, `POST /api/Reservation` | Reservation validation | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D03-03 | TĂ¡ÂºÂ¡o yÄ‚Âªu cĂ¡ÂºÂ§u Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n | Anonymous, Customer | Public or authenticated | Reservation form | `POST /api/Reservation` | `ReservationService.CreateReservationAsync` | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D03-04 | Xem Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n cĂ¡Â»Â§a mÄ‚Â¬nh | Registered Customer | Customer JWT/ownership | `CustomerProfile.tsx` | `GET /api/Reservation?customerPhone=` | Ownership filtering | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D03-05 | CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n | Registered Customer, Staff | Authenticated/branch or ownership | Customer UI does not clearly expose editing; backend exists | `PUT /api/Reservation/{id}` | `UpdateReservationAsync` | Actor Use Case | Ă¢â‚¬â€ | MEDIUM |
| D03-06 | HĂ¡Â»Â§y Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n | Registered Customer | Authenticated/ownership | Customer UI does not clearly expose cancellation; backend delete exists | `DELETE /api/Reservation/{id}` | `DeleteReservationAsync` | Actor Use Case | Ă¢â‚¬â€ | MEDIUM |
| D03-07 | XÄ‚Â¡c nhĂ¡ÂºÂ­n/cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t trĂ¡ÂºÂ¡ng thÄ‚Â¡i Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n | Cashier, Employee, Manager, Admin | Staff JWT | `ReservationManagement.tsx` | `PATCH /api/Reservation/{id}/status` | `UpdateStatusAsync` | Actor Use Case | Ă¢â‚¬â€ | HIGH |

### Direct actor associations

- `Anonymous -> TĂ¡ÂºÂ¡o yÄ‚Âªu cĂ¡ÂºÂ§u Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n`
- `Registered Customer -> TĂ¡ÂºÂ¡o yÄ‚Âªu cĂ¡ÂºÂ§u Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n`
- `Registered Customer -> Xem Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n cĂ¡Â»Â§a mÄ‚Â¬nh`
- `Registered Customer -> CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t/HĂ¡Â»Â§y Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n`
- `Cashier/Employee/Manager/Admin -> XÄ‚Â¡c nhĂ¡ÂºÂ­n/cĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t trĂ¡ÂºÂ¡ng thÄ‚Â¡i Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n`

### Valid relationships

- `TĂ¡ÂºÂ¡o yÄ‚Âªu cĂ¡ÂºÂ§u Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n -> ChĂ¡Â»Ân bÄ‚Â n, thĂ¡Â»Âi gian vÄ‚Â  sĂ¡Â»â€˜ khÄ‚Â¡ch` Ă¢â‚¬â€ include, because those values are part of the reservation form.
- `XÄ‚Â¡c nhĂ¡ÂºÂ­n Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n -> TĂ¡ÂºÂ¡o yÄ‚Âªu cĂ¡ÂºÂ§u Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n` should not be treated as an extension; it is a UI action/submit step.
- `ChĂ¡Â»Ân bÄ‚Â n -> TĂ¡ÂºÂ¡o yÄ‚Âªu cĂ¡ÂºÂ§u Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n` is conditional because table selection is optional in the Customer Web form.

### Business flow

`Customer opens reservation page Ă¢â€ â€™ selects branch Ă¢â€ â€™ optionally selects table Ă¢â€ â€™ enters guest count/time/contact data Ă¢â€ â€™ submits pending reservation Ă¢â€ â€™ staff reviews Ă¢â€ â€™ staff updates status Ă¢â€ â€™ customer can view resulting reservation`

### Diagram recommendation

**KEEP.**

The diagram should distinguish:

- public reservation request creation;
- customer reservation self-service;
- staff reservation processing.

## 6. UC-D04 Ă¢â‚¬â€ Loyalty vÄ‚Â  Voucher

### Scope

Loyalty is supported. Promotion CRUD exists, but complete voucher/discount application is not sufficiently evidenced.

### Evidence table

| ID | Child Use Case | Actor | Guest/Customer/Auth | Frontend Evidence | API Evidence | Service Evidence | UML Type | Include/Extend Target | Confidence |
|---|---|---|---|---|---|---|---|---|---|
| D04-01 | Xem Ă„â€˜iĂ¡Â»Æ’m loyalty | Registered Customer | Registered JWT | Customer profile displays points | `GET /api/Customer/{id}/loyalty-history`, customer profile data | `LoyaltyService` | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D04-02 | Xem lĂ¡Â»â€¹ch sĂ¡Â»Â­ tÄ‚Â­ch Ă„â€˜iĂ¡Â»Æ’m | Registered Customer | Registered JWT | `CustomerProfile.tsx` | `GET /api/Customer/{id}/loyalty-history` | Loyalty history query | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D04-03 | TÄ‚Â­ch Ă„â€˜iĂ¡Â»Æ’m tĂ¡Â»Â« Ă„â€˜Ă†Â¡n hÄ‚Â ng | Registered Customer/system | Customer identity and completed order | Order detail displays earned points | Order completion and loyalty paths | `LoyaltyService` / `OrderService` | Include Candidate | `HoÄ‚Â n tĂ¡ÂºÂ¥t Ă„â€˜Ă†Â¡n hÄ‚Â ng` | MEDIUM |
| D04-04 | Ă„ÂĂ¡Â»â€¢i Ă„â€˜iĂ¡Â»Æ’m giĂ¡ÂºÂ£m giÄ‚Â¡ | Cashier, Employee, Manager, Admin | Staff JWT | POS redemption control | `POST /api/Order/{id}/redeem` | `LoyaltyService.RedeemPointsAsync` | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D04-05 | QuĂ¡ÂºÂ£n lÄ‚Â½ khuyĂ¡ÂºÂ¿n mÄ‚Â£i | Admin | Admin JWT | `PromotionManagement.tsx` | `GET/POST/PUT/DELETE/PATCH /api/Promotion` | Direct controller/EF operations | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D04-06 | Xem khuyĂ¡ÂºÂ¿n mÄ‚Â£i | Anonymous, Customer | Public | Customer menu/catalog | `GET /api/Promotion` | Promotion query | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D04-07 | Ä‚Âp dĂ¡Â»Â¥ng voucher/khuyĂ¡ÂºÂ¿n mÄ‚Â£i vÄ‚Â o Ă„â€˜Ă†Â¡n | Customer, Staff | Not fully evidenced | No reliable customer voucher flow found | Promotion endpoint exists, but no authoritative order-discount integration proven | No complete service evidence | Unsupported Ă¢â‚¬â€ Exclude | Ă¢â‚¬â€ | HIGH |
| D04-08 | Xem hĂ¡ÂºÂ¡ng khÄ‚Â¡ch hÄ‚Â ng | Registered Customer | Customer JWT | No clearly verified dedicated UI behavior | Customer data may contain rank-related fields | Not enough current evidence | Unsupported Ă¢â‚¬â€ Exclude | Ă¢â‚¬â€ | LOW |

### Direct actor associations

- `Registered Customer -> Xem Ă„â€˜iĂ¡Â»Æ’m loyalty`
- `Registered Customer -> Xem lĂ¡Â»â€¹ch sĂ¡Â»Â­ tÄ‚Â­ch Ă„â€˜iĂ¡Â»Æ’m`
- `Cashier/Employee/Manager/Admin -> Ă„ÂĂ¡Â»â€¢i Ă„â€˜iĂ¡Â»Æ’m giĂ¡ÂºÂ£m giÄ‚Â¡`
- `Anonymous/Registered Customer -> Xem khuyĂ¡ÂºÂ¿n mÄ‚Â£i`
- `Admin -> QuĂ¡ÂºÂ£n lÄ‚Â½ khuyĂ¡ÂºÂ¿n mÄ‚Â£i`

### Valid relationships

- `Ă„ÂĂ¡Â»â€¢i Ă„â€˜iĂ¡Â»Æ’m giĂ¡ÂºÂ£m giÄ‚Â¡ -> Xem Ă„â€˜iĂ¡Â»Æ’m loyalty` may be modeled as an include only if the POS explicitly loads and displays the customer's available points before redemption.
- `QuĂ¡ÂºÂ£n lÄ‚Â½ khuyĂ¡ÂºÂ¿n mÄ‚Â£i -> Xem khuyĂ¡ÂºÂ¿n mÄ‚Â£i` is not an include; management and public display are separate actor goals.
- Do not model voucher application because complete executable support is not established.

### Business flow

`Customer earns points through eligible completed orders Ă¢â€ â€™ customer views points/history Ă¢â€ â€™ staff may redeem points on an order Ă¢â€ â€™ loyalty transaction and order discount are persisted`

### Diagram recommendation

**KEEP as Ă¢â‚¬Å“LoyaltyĂ¢â‚¬Â; treat Voucher/Promotion as a separate smaller management capability or merge it into Catalog/Marketing.**

Do not draw Ă¢â‚¬Å“Use voucherĂ¢â‚¬Â until end-to-end discount application is confirmed.

## 7. UC-D05 Ă¢â‚¬â€ AI Agent

### Scope

The current system exposes an authenticated AI assistant with role- and risk-gated tools. It is not a general autonomous agent.

### Evidence table

| ID | Child Use Case | Actor | Guest/Customer/Auth | Frontend Evidence | API Evidence | Service Evidence | UML Type | Include/Extend Target | Confidence |
|---|---|---|---|---|---|---|---|---|---|
| D05-01 | TrÄ‚Â² chuyĂ¡Â»â€¡n vĂ¡Â»â€ºi AI Assistant | Registered Customer, Staff, Manager, Admin | Authenticated | `ChatBot.tsx` | `POST /api/Ai/chat` | `AiOrchestrator` | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D05-02 | Xem thĂ¡Â»Â±c Ă„â€˜Ă†Â¡n qua AI | Registered Customer and staff roles | Authenticated | Chat suggestions/UI | Registered `customer_get_menu` tool | `ProductService` | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D05-03 | Ă„ÂĂ¡ÂºÂ·t bÄ‚Â n qua AI | Registered Customer | Customer JWT | Chat suggestion Ă¢â‚¬Å“TÄ‚Â´i muĂ¡Â»â€˜n Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â nĂ¢â‚¬Â | AI tool `create_booking` | `ReservationService.CreateBookingFromAiAsync` | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D05-04 | Xem Ă„â€˜Ă†Â¡n hÄ‚Â ng qua AI | Registered Customer | Registered customer JWT | Chat interface | AI tool `get_my_orders` | `OrderService` | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D05-05 | Xem ca lÄ‚Â m qua AI | Employee, Cashier, Kitchen | Staff JWT | Chat interface if available | AI tool `get_my_shift` | `EmployeeService` | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D05-06 | Tra cĂ¡Â»Â©u vĂ¡ÂºÂ­n hÄ‚Â nh qua AI | Employee, Cashier, Kitchen, Manager, Admin | Authenticated role/branch | Chat interface | `get_order_list`, `get_table_summary`, `get_best_sellers` | Order/Dashboard/Table services | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D05-07 | PhÄ‚Â¢n tÄ‚Â­ch kinh doanh qua AI | Manager, Admin | Authenticated with branch/global scope | Chat interface | Revenue, summary, financial-analysis tools | Dashboard/financial services | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D05-08 | CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t trĂ¡ÂºÂ¡ng thÄ‚Â¡i Ă„â€˜Ă†Â¡n qua AI | Employee, Cashier, Kitchen, Manager, Admin | Authenticated branch-scoped role | Chat interface | `update_order_status` | `OrderService.UpdateOrderStatusByCodeAsync` | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D05-09 | CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t giÄ‚Â¡ sĂ¡ÂºÂ£n phĂ¡ÂºÂ©m qua AI | Admin | Admin JWT | Chat interface | `update_product_price` | `ProductService.UpdatePriceByNameAsync` | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D05-10 | AI tĂ¡Â»Â± chĂ¡Â»Â§ quyĂ¡ÂºÂ¿t Ă„â€˜Ă¡Â»â€¹nh nghiĂ¡Â»â€¡p vĂ¡Â»Â¥ | None | Not supported | No evidence | Orchestrator calls explicitly registered tools | No autonomous workflow engine | Unsupported Ă¢â‚¬â€ Exclude | Ă¢â‚¬â€ | HIGH |

### Direct actor associations

- `Registered Customer -> TrÄ‚Â² chuyĂ¡Â»â€¡n vĂ¡Â»â€ºi AI Assistant`
- `Manager/Admin -> PhÄ‚Â¢n tÄ‚Â­ch kinh doanh qua AI`
- `Employee/Cashier/Kitchen/Manager/Admin -> Tra cĂ¡Â»Â©u vĂ¡ÂºÂ­n hÄ‚Â nh qua AI`
- `Admin -> CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t giÄ‚Â¡ sĂ¡ÂºÂ£n phĂ¡ÂºÂ©m qua AI`
- `Employee/Cashier/Kitchen/Manager/Admin -> CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t trĂ¡ÂºÂ¡ng thÄ‚Â¡i Ă„â€˜Ă†Â¡n qua AI`
- `Registered Customer -> Ă„ÂĂ¡ÂºÂ·t bÄ‚Â n/Xem Ă„â€˜Ă†Â¡n/Xem thĂ¡Â»Â±c Ă„â€˜Ă†Â¡n qua AI`

### Valid relationships

- `TrÄ‚Â² chuyĂ¡Â»â€¡n vĂ¡Â»â€ºi AI Assistant -> ThĂ¡Â»Â±c thi cÄ‚Â´ng cĂ¡Â»Â¥ Ă„â€˜Ă†Â°Ă¡Â»Â£c cĂ¡ÂºÂ¥p quyĂ¡Â»Ân` is a valid include candidate at system level.
- Specific tools should not all be drawn as mandatory includes because the model invokes them conditionally based on the user request.
- `Ă„ÂĂ¡ÂºÂ·t bÄ‚Â n qua AI -> TĂ¡ÂºÂ¡o Ă„â€˜Ă¡ÂºÂ·t bÄ‚Â n` can be represented as an include when the tool actually performs the booking.

### Relationships NOT to draw

- Do not draw `AI -> Database`.
- Do not draw JWT parsing, tool registry lookup, or permission checks as business use cases.
- Do not claim the AI independently manages the restaurant.
- Do not associate Guest QR customer with AI; Guest AI is explicitly denied.

### Business flow

`Authenticated actor opens chatbot Ă¢â€ â€™ submits request Ă¢â€ â€™ AI orchestrator interprets request Ă¢â€ â€™ permission service filters allowed tools Ă¢â€ â€™ optional tool executes through application service Ă¢â€ â€™ AI returns result`

### Diagram recommendation

**KEEP**, but title it Ă¢â‚¬Å“TĂ†Â°Ă†Â¡ng tÄ‚Â¡c vĂ¡Â»â€ºi AI Assistant cÄ‚Â³ kiĂ¡Â»Æ’m soÄ‚Â¡t quyĂ¡Â»ÂnĂ¢â‚¬Â.

## 8. UC-D06 Ă¢â‚¬â€ XĂ¡Â»Â­ lÄ‚Â½ Ă„â€˜Ă†Â¡n hÄ‚Â ng tĂ¡ÂºÂ¡i POS

### Scope

Fully supported and central to the thesis.

### Evidence table

| ID | Child Use Case | Actor | Guest/Customer/Auth | Frontend Evidence | API Evidence | Service Evidence | UML Type | Include/Extend Target | Confidence |
|---|---|---|---|---|---|---|---|---|---|
| D06-01 | ChĂ¡Â»Ân bÄ‚Â n | Cashier, Employee, Manager, Admin | Staff JWT | `POSPage.tsx` | `GET /api/Table` | `TableService` | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D06-02 | TĂ¡ÂºÂ¡o Ă„â€˜Ă†Â¡n POS | Cashier, Employee, Manager, Admin | Staff JWT | `POSPage.tsx` | `POST /api/Order` | `OrderService.CreateOrUpdateOrderAsync` | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D06-03 | ThÄ‚Âªm/sĂ¡Â»Â­a/xÄ‚Â³a mÄ‚Â³n | Cashier, Employee, Manager, Admin | Staff JWT | POS cart/order editor | `POST /api/Order` | Order service recalculates authoritative values | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D06-04 | TiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n Ă„â€˜Ă†Â¡n Web | Cashier, Employee, Manager, Admin | Staff JWT | Pending-order POS area | `POST /api/Order/{id}/accept` | `AcceptWebOrderAsync` | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D06-05 | GĂ¡Â»Â­i Ă„â€˜Ă†Â¡n xuĂ¡Â»â€˜ng bĂ¡ÂºÂ¿p | Cashier, Employee, Manager, Admin, Kitchen | Staff JWT | POS send-to-kitchen action | `POST /api/Order/{id}/send-to-kitchen` | Kitchen service | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D06-06 | Theo dÄ‚Âµi trĂ¡ÂºÂ¡ng thÄ‚Â¡i chĂ¡ÂºÂ¿ biĂ¡ÂºÂ¿n | Cashier, Employee, Manager, Admin | Staff JWT | POS kitchen-status UI | `GET /api/Order/{id}/kitchen-status` | Kitchen service | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D06-07 | Thanh toÄ‚Â¡n Ă„â€˜Ă†Â¡n | Cashier, Employee, Manager, Admin | Staff JWT | POS payment controls | `POST /api/Order/{id}/payment` | `OrderService.PayOrderAsync` | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D06-08 | Ä‚Âp dĂ¡Â»Â¥ng Ă„â€˜Ă¡Â»â€¢i Ă„â€˜iĂ¡Â»Æ’m | Cashier, Employee, Manager, Admin | Staff JWT | POS loyalty controls | `POST /api/Order/{id}/redeem` | `LoyaltyService` | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D06-09 | Xem hÄ‚Â³a Ă„â€˜Ă†Â¡n | Cashier, Employee, Manager, Admin | Staff JWT | Invoice history and POS | `GET /api/Invoice`, `GET /api/Invoice/{id}` | Direct scoped order queries | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D06-10 | HoÄ‚Â n tĂ¡ÂºÂ¥t/hĂ¡Â»Â§y Ă„â€˜Ă†Â¡n | Staff roles according to endpoint | Staff JWT | POS status actions | `PATCH /api/Order/{id}/status`, `DELETE /api/Order/{id}` | Order service status rules | Actor Use Case | Ă¢â‚¬â€ | HIGH |

### Direct actor associations

- `Cashier -> TĂ¡ÂºÂ¥t cĂ¡ÂºÂ£ POS actions`
- `Employee -> POS order actions where endpoint permits`
- `Manager/Admin -> POS order actions`
- `Kitchen -> GĂ¡Â»Â­i Ă„â€˜Ă†Â¡n xuĂ¡Â»â€˜ng bĂ¡ÂºÂ¿p / kitchen status operations`

### Valid relationships

- `TĂ¡ÂºÂ¡o Ă„â€˜Ă†Â¡n POS -> ChĂ¡Â»Ân bÄ‚Â n`
- `TĂ¡ÂºÂ¡o Ă„â€˜Ă†Â¡n POS -> ThÄ‚Âªm/sĂ¡Â»Â­a/xÄ‚Â³a mÄ‚Â³n`
- `GĂ¡Â»Â­i Ă„â€˜Ă†Â¡n xuĂ¡Â»â€˜ng bĂ¡ÂºÂ¿p -> TĂ¡ÂºÂ¡o yÄ‚Âªu cĂ¡ÂºÂ§u bĂ¡ÂºÂ¿p`
- `Thanh toÄ‚Â¡n Ă„â€˜Ă†Â¡n -> XÄ‚Â¡c thĂ¡Â»Â±c tĂ¡Â»â€¢ng tiĂ¡Â»Ân tĂ¡Â»Â« Order.TotalAmount`
- `Thanh toÄ‚Â¡n Ă„â€˜Ă†Â¡n -> CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t trĂ¡ÂºÂ¡ng thÄ‚Â¡i hoÄ‚Â n tĂ¡ÂºÂ¥t`

These are reasonable include candidates only when the diagram models required system behavior. The actual execution order belongs in the activity diagram.

### Business flow

`Staff selects branch/table Ă¢â€ â€™ adds products/toppings Ă¢â€ â€™ backend recalculates totals Ă¢â€ â€™ saves pending order Ă¢â€ â€™ optionally sends to kitchen Ă¢â€ â€™ tracks preparation Ă¢â€ â€™ records payment Ă¢â€ â€™ completes order Ă¢â€ â€™ invoice/reporting data becomes available`

### Diagram recommendation

**KEEP.**

## 9. UC-D07 Ă¢â‚¬â€ KDS

### Scope

Fully supported for Kitchen and relevant operational staff.

### Evidence table

| ID | Child Use Case | Actor | Guest/Customer/Auth | Frontend Evidence | API Evidence | Service Evidence | UML Type | Include/Extend Target | Confidence |
|---|---|---|---|---|---|---|---|---|---|
| D07-01 | Xem yÄ‚Âªu cĂ¡ÂºÂ§u bĂ¡ÂºÂ¿p Ă„â€˜ang hoĂ¡ÂºÂ¡t Ă„â€˜Ă¡Â»â„¢ng | Kitchen, Manager, Admin | Staff JWT | `KitchenPage.tsx` | `GET /api/Order/kitchen/active-requests` | `KitchenService` | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D07-02 | CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t trĂ¡ÂºÂ¡ng thÄ‚Â¡i mÄ‚Â³n/yÄ‚Âªu cĂ¡ÂºÂ§u | Kitchen, Manager, Admin | Staff JWT | Kitchen status buttons | `PATCH /api/Order/kitchen/requests/{id}/status` | Kitchen service | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D07-03 | Xem lĂ¡Â»â€¹ch sĂ¡Â»Â­ bĂ¡ÂºÂ¿p | Kitchen, Manager, Admin | Staff JWT | `KitchenHistoryPage.tsx` | `GET /api/Order/kitchen/history` | Kitchen service | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D07-04 | Xem chi tiĂ¡ÂºÂ¿t yÄ‚Âªu cĂ¡ÂºÂ§u bĂ¡ÂºÂ¿p | Kitchen, Manager, Admin | Staff JWT | Kitchen history detail | `GET /api/Order/kitchen/requests/{id}` | Kitchen service | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D07-05 | NhĂ¡ÂºÂ­n yÄ‚Âªu cĂ¡ÂºÂ§u mĂ¡Â»â€ºi realtime | Kitchen | Authenticated SignalR | Kitchen SignalR client | `/kitchenHub`, `NewOrderRequest` | Hub notifier | Include Candidate | `Xem yÄ‚Âªu cĂ¡ÂºÂ§u bĂ¡ÂºÂ¿p` | HIGH |
| D07-06 | CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t tÄ‚Â¬nh trĂ¡ÂºÂ¡ng sĂ¡ÂºÂµn cÄ‚Â³ mÄ‚Â³n | Kitchen, Manager, Admin | Staff JWT | Kitchen product availability UI | `PATCH /api/Product/{id}/availability` | Product/controller logic | Actor Use Case | Ă¢â‚¬â€ | HIGH |

### Valid relationships

- `Xem yÄ‚Âªu cĂ¡ÂºÂ§u bĂ¡ÂºÂ¿p -> NhĂ¡ÂºÂ­n yÄ‚Âªu cĂ¡ÂºÂ§u mĂ¡Â»â€ºi realtime`
- `Xem yÄ‚Âªu cĂ¡ÂºÂ§u bĂ¡ÂºÂ¿p -> CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t trĂ¡ÂºÂ¡ng thÄ‚Â¡i yÄ‚Âªu cĂ¡ÂºÂ§u`
- `CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t trĂ¡ÂºÂ¡ng thÄ‚Â¡i yÄ‚Âªu cĂ¡ÂºÂ§u -> CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t trĂ¡ÂºÂ¡ng thÄ‚Â¡i mÄ‚Â³n`

### Business flow

`Order sent to kitchen Ă¢â€ â€™ SignalR notifies KDS Ă¢â€ â€™ kitchen views active request Ă¢â€ â€™ kitchen accepts/prepares/completes Ă¢â€ â€™ status is persisted Ă¢â€ â€™ POS can read kitchen status Ă¢â€ â€™ history is available`

### Diagram recommendation

**KEEP.**

## 10. UC-D08 Ă¢â‚¬â€ ChĂ¡ÂºÂ¥m cÄ‚Â´ng vÄ‚Â  lĂ¡Â»â€¹ch lÄ‚Â m viĂ¡Â»â€¡c

### Scope

Supported, but should distinguish staff self-service from management scheduling.

### Evidence table

| ID | Child Use Case | Actor | Guest/Customer/Auth | Frontend Evidence | API Evidence | Service Evidence | UML Type | Include/Extend Target | Confidence |
|---|---|---|---|---|---|---|---|---|---|
| D08-01 | Xem lĂ¡Â»â€¹ch lÄ‚Â m viĂ¡Â»â€¡c cĂ¡Â»Â§a mÄ‚Â¬nh | Employee, Cashier, Kitchen | Staff JWT | Staff/Kitchen schedule pages | `GET /api/WorkSchedule` | WorkSchedule controller/service logic | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D08-02 | TĂ¡ÂºÂ¡o lĂ¡Â»â€¹ch lÄ‚Â m viĂ¡Â»â€¡c | Manager, Admin | Staff JWT | `WorkSchedulePage.tsx` | `POST /api/WorkSchedule` | Branch and employee validation | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D08-03 | XÄ‚Â³a lĂ¡Â»â€¹ch lÄ‚Â m viĂ¡Â»â€¡c | Manager, Admin | Staff JWT | Work schedule management | `DELETE /api/WorkSchedule/{id}` | Branch validation | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D08-04 | Xem lĂ¡Â»â€¹ch sĂ¡Â»Â­ chĂ¡ÂºÂ¥m cÄ‚Â´ng | Employee, Cashier, Kitchen, Manager, Admin | Staff JWT | Attendance pages | `GET /api/Attendance` | Attendance controller | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D08-05 | Check-in | Employee, Cashier, Kitchen | Staff JWT | `EmployeeAttendance.tsx` | `POST /api/Attendance/check-in` | Attendance persistence and QR validation | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D08-06 | Check-out | Employee, Cashier, Kitchen | Staff JWT | Attendance page | `PATCH /api/Attendance/{id}/check-out` | Attendance validation | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D08-07 | QuĂ¡ÂºÂ£n lÄ‚Â½ chĂ¡ÂºÂ¥m cÄ‚Â´ng nhÄ‚Â¢n viÄ‚Âªn | Manager, Admin | Staff JWT | `AttendanceManagement.tsx` | `GET /api/Attendance` | Branch/identity logic | Actor Use Case | Ă¢â‚¬â€ | MEDIUM |

### Diagram recommendation

**KEEP**, but separate:

- Staff self-service attendance/schedule.
- Manager/Admin scheduling and attendance review.

Do not combine this diagram with cash-shift management.

## 11. UC-D09 Ă¢â‚¬â€ QuĂ¡ÂºÂ£n lÄ‚Â½ ca thu ngÄ‚Â¢n

### Scope

Supported and specifically concerns cashier shifts, not generic work schedules.

### Evidence table

| ID | Child Use Case | Actor | Guest/Customer/Auth | Frontend Evidence | API Evidence | Service Evidence | UML Type | Include/Extend Target | Confidence |
|---|---|---|---|---|---|---|---|---|---|
| D09-01 | MĂ¡Â»Å¸ ca | Cashier, Manager, Admin | Staff JWT | `ShiftManagement.tsx`, POS shift panel | `POST /api/Shift/open` | Shift controller | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D09-02 | Xem ca hiĂ¡Â»â€¡n tĂ¡ÂºÂ¡i | Cashier, Manager, Admin | Staff JWT | POS shift panel | `GET /api/Shift/current/{employeeId}` | Shift query | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D09-03 | Ă„ÂÄ‚Â³ng ca | Cashier, Manager, Admin | Staff JWT | POS shift panel | `POST /api/Shift/close/{id}` | Branch-scoped revenue reconciliation | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D09-04 | Xem lĂ¡Â»â€¹ch sĂ¡Â»Â­ ca | Cashier, Manager, Admin | Staff JWT | Shift management | `GET /api/Shift` | Shift query | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D09-05 | Ă„ÂĂ¡Â»â€˜i soÄ‚Â¡t doanh thu ca | Cashier, Manager, Admin | Staff JWT | Close-shift result | `POST /api/Shift/close/{id}` | Completed same-branch order aggregation | Include Candidate | `Ă„ÂÄ‚Â³ng ca` | HIGH |

### Diagram recommendation

**KEEP.**

Use Ă¢â‚¬Å“CashierĂ¢â‚¬Â as the principal operational actor. Manager/Admin are supervisory actors.

## 12. UC-D10 Ă¢â‚¬â€ QuĂ¡ÂºÂ£n lÄ‚Â½ nhÄ‚Â¢n sĂ¡Â»Â± vÄ‚Â  vĂ¡ÂºÂ­n hÄ‚Â nh chi nhÄ‚Â¡nh

### Scope

This candidate is too broad for one detailed decomposition. It should be split into:

1. QuĂ¡ÂºÂ£n lÄ‚Â½ nhÄ‚Â¢n sĂ¡Â»Â±.
2. QuĂ¡ÂºÂ£n lÄ‚Â½ chi nhÄ‚Â¡nh vÄ‚Â  bÄ‚Â n/khu vĂ¡Â»Â±c.
3. CĂ¡ÂºÂ¥u hÄ‚Â¬nh vĂ¡ÂºÂ­n hÄ‚Â nh.

### Evidence table

| ID | Child Use Case | Actor | Guest/Customer/Auth | Frontend Evidence | API Evidence | Service Evidence | UML Type | Include/Extend Target | Confidence |
|---|---|---|---|---|---|---|---|---|---|
| D10-01 | Xem danh sÄ‚Â¡ch nhÄ‚Â¢n viÄ‚Âªn | Manager, Admin | Staff JWT | `EmployeeManagement.tsx` | `GET /api/Employee` | Employee controller/service | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D10-02 | TĂ¡ÂºÂ¡o nhÄ‚Â¢n viÄ‚Âªn | Manager, Admin | Staff JWT | Employee management form | `POST /api/Employee` | Employee service | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D10-03 | CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t hĂ¡Â»â€œ sĂ†Â¡ nhÄ‚Â¢n viÄ‚Âªn | Manager, Admin | Staff JWT | Employee edit form | `PUT /api/Employee/{id}` | Employee service | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D10-04 | PhÄ‚Â¢n quyĂ¡Â»Ân/vai trÄ‚Â² nhÄ‚Â¢n viÄ‚Âªn | Manager, Admin | Staff JWT | Employee role field | `PUT /api/Employee/{id}` | Stored `Employee.Role` drives JWT | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D10-05 | GÄ‚Â¡n chi nhÄ‚Â¡nh nhÄ‚Â¢n viÄ‚Âªn | Manager, Admin | Staff JWT | Employee branch field | `POST/PUT /api/Employee` | Branch validation and isolation | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D10-06 | KÄ‚Â­ch hoĂ¡ÂºÂ¡t/vÄ‚Â´ hiĂ¡Â»â€¡u hÄ‚Â³a nhÄ‚Â¢n viÄ‚Âªn | Manager, Admin | Staff JWT | Employee toggle | `PATCH /api/Employee/{id}/toggle-status` | Employee service | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D10-07 | XÄ‚Â³a nhÄ‚Â¢n viÄ‚Âªn | Manager, Admin | Staff JWT | Employee management | `DELETE /api/Employee/{id}` | Employee service | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D10-08 | QuĂ¡ÂºÂ£n lÄ‚Â½ chi nhÄ‚Â¡nh | Admin | Admin JWT | `BranchManagement.tsx` | `POST/PUT/PATCH/DELETE /api/Branch` | Branch controller | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D10-09 | QuĂ¡ÂºÂ£n lÄ‚Â½ bÄ‚Â n/khu vĂ¡Â»Â±c | Manager, Admin | Staff JWT | `TableManagement.tsx` | `/api/Table`, `/api/Area` | Table/area services | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D10-10 | CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t trĂ¡ÂºÂ¡ng thÄ‚Â¡i bÄ‚Â n | Cashier, Employee, Manager, Admin | Staff JWT | POS/table management | `PATCH /api/Table/{id}/status` | Table service | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D10-11 | TĂ¡ÂºÂ¡o/xem QR bÄ‚Â n | Manager, Admin | Staff JWT | Table QR display | `GET /api/Table/{id}/qr-token` | Server-generated QR token | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D10-12 | QuĂ¡ÂºÂ£n lÄ‚Â½ cĂ¡ÂºÂ¥u hÄ‚Â¬nh chi nhÄ‚Â¡nh | Manager, Admin, Cashier, Employee depending on setting | Authenticated | System/receipt settings pages | `GET/PUT /api/SystemSettings`, `/api/ReceiptSettings` | Settings services | Actor Use Case | Ă¢â‚¬â€ | HIGH |

### Diagram recommendation

**SPLIT.**

Ă¢â‚¬Å“QuĂ¡ÂºÂ£n lÄ‚Â½ nhÄ‚Â¢n sĂ¡Â»Â± vÄ‚Â  vĂ¡ÂºÂ­n hÄ‚Â nh chi nhÄ‚Â¡nhĂ¢â‚¬Â is too broad. Keep Ă¢â‚¬Å“QuĂ¡ÂºÂ£n lÄ‚Â½ nhÄ‚Â¢n sĂ¡Â»Â±Ă¢â‚¬Â as a decomposition diagram. Put Ă¢â‚¬Å“QuĂ¡ÂºÂ£n lÄ‚Â½ chi nhÄ‚Â¡nhĂ¢â‚¬Â, Ă¢â‚¬Å“QuĂ¡ÂºÂ£n lÄ‚Â½ bÄ‚Â n/khu vĂ¡Â»Â±cĂ¢â‚¬Â, and Ă¢â‚¬Å“CĂ¡ÂºÂ¥u hÄ‚Â¬nh hĂ¡Â»â€¡ thĂ¡Â»â€˜ngĂ¢â‚¬Â in separate diagrams or the general system diagram.

## 13. UC-D11 Ă¢â‚¬â€ ThĂ¡Â»Â±c Ă„â€˜Ă†Â¡n, Topping vÄ‚Â  Inventory

### Scope

This candidate should be split. Catalog management and inventory are not cohesive enough for one decomposition diagram.

### Evidence table

| ID | Child Use Case | Actor | Guest/Customer/Auth | Frontend Evidence | API Evidence | Service Evidence | UML Type | Include/Extend Target | Confidence |
|---|---|---|---|---|---|---|---|---|---|
| D11-01 | Xem sĂ¡ÂºÂ£n phĂ¡ÂºÂ©m | Anonymous, Customer, Staff | Public/authenticated | Customer menu, POS, Product Management | `GET /api/Product` | Product query | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D11-02 | QuĂ¡ÂºÂ£n lÄ‚Â½ sĂ¡ÂºÂ£n phĂ¡ÂºÂ©m | Admin | Admin JWT | `ProductManagement.tsx` | `POST/PUT/DELETE /api/Product` | Product controller | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D11-03 | CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t trĂ¡ÂºÂ¡ng thÄ‚Â¡i bÄ‚Â¡n mÄ‚Â³n | Admin, Manager, Kitchen | Staff JWT | Product/Kitchen availability UI | `PATCH /api/Product/{id}/availability` | Product controller | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D11-04 | QuĂ¡ÂºÂ£n lÄ‚Â½ topping | Admin | Admin JWT | `ToppingManagement.tsx` | `GET/POST/PUT/DELETE /api/Topping` | Topping controller | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D11-05 | QuĂ¡ÂºÂ£n lÄ‚Â½ khuyĂ¡ÂºÂ¿n mÄ‚Â£i | Admin | Admin JWT | `PromotionManagement.tsx` | `/api/Promotion` | Promotion controller | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D11-06 | QuĂ¡ÂºÂ£n lÄ‚Â½ nguyÄ‚Âªn liĂ¡Â»â€¡u master | Manager, Admin | Staff JWT | Inventory item page | `/api/inventory/items` | `InventoryService` | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D11-07 | Xem tĂ¡Â»â€œn kho theo chi nhÄ‚Â¡nh | Operational staff, Manager, Admin | Staff JWT | Inventory overview | `/api/inventory/branches/{branchId}`, `/overview/{branchId}` | BranchInventory service | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D11-08 | ThiĂ¡ÂºÂ¿t lĂ¡ÂºÂ­p tĂ¡Â»â€œn tĂ¡Â»â€˜i thiĂ¡Â»Æ’u | Manager, Admin | Staff JWT | Inventory overview | `PUT /api/inventory/branches/{branchId}/items/{id}/minimum-stock` | Inventory service | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D11-09 | TĂ¡ÂºÂ¡o phiĂ¡ÂºÂ¿u nhĂ¡ÂºÂ­p kho | Manager, Admin | Staff JWT | Receipt form | `POST /api/inventory/receipts` | `CreateReceiptAsync` | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D11-10 | XÄ‚Â¡c nhĂ¡ÂºÂ­n/hĂ¡Â»Â§y phiĂ¡ÂºÂ¿u nhĂ¡ÂºÂ­p | Manager, Admin | Staff JWT | Receipt detail page | `/confirm`, `/cancel` | Stock update and linked Expense creation | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D11-11 | TĂ¡ÂºÂ¡o phiĂ¡ÂºÂ¿u xuĂ¡ÂºÂ¥t kho | Manager, Admin and operational API users | Staff JWT | Issue form | `POST /api/inventory/issues` | `CreateIssueAsync` | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D11-12 | XÄ‚Â¡c nhĂ¡ÂºÂ­n/hĂ¡Â»Â§y phiĂ¡ÂºÂ¿u xuĂ¡ÂºÂ¥t | Manager, Admin | Staff JWT | Issue detail page | `/confirm`, `/cancel` | Stock decrement and ledger | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D11-13 | Xem lĂ¡Â»â€¹ch sĂ¡Â»Â­ biĂ¡ÂºÂ¿n Ă„â€˜Ă¡Â»â„¢ng kho | Operational staff, Manager, Admin | Staff JWT | Inventory history | `GET /api/inventory/transactions` | Stock ledger query | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D11-14 | CĂ¡ÂºÂ£nh bÄ‚Â¡o tĂ¡Â»â€œn thĂ¡ÂºÂ¥p/hĂ¡ÂºÂ¿t hÄ‚Â ng | Manager, Admin, operational staff | Staff JWT | Inventory overview | Inventory overview response | Inventory service computes low/out-of-stock | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D11-15 | Ghi nhĂ¡ÂºÂ­n chi phÄ‚Â­ nhĂ¡ÂºÂ­p kho | System/Manager/Admin | As part of receipt confirmation | Receipt detail shows expense result | No separate user action for this linkage | `ConfirmReceiptAsync` creates exactly one Expense | Include Candidate | `XÄ‚Â¡c nhĂ¡ÂºÂ­n phiĂ¡ÂºÂ¿u nhĂ¡ÂºÂ­p` | HIGH |

### Diagram recommendation

**SPLIT into:**

- Catalog/Product/Topping/Promotion Management.
- Inventory and Stock Control.

Inventory is complex enough for its own thesis decomposition diagram because it includes master data, branch balances, receipts, issues, confirmations, stock ledger, low-stock state, and expense linkage.

## 14. UC-D12 Ă¢â‚¬â€ Dashboard, Business Insights vÄ‚Â  chi phÄ‚Â­

### Scope

Dashboard, insights, and expenses are related to management reporting but represent distinct business goals. They may share a management overview diagram but should not be forced into one crowded decomposition.

### Evidence table

| ID | Child Use Case | Actor | Guest/Customer/Auth | Frontend Evidence | API Evidence | Service Evidence | UML Type | Include/Extend Target | Confidence |
|---|---|---|---|---|---|---|---|---|---|
| D12-01 | Xem dashboard KPI | Manager, Admin | Staff JWT | `Dashboard.tsx` | `GET /api/Dashboard/summary` | `DashboardService.GetSummaryAsync` | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D12-02 | LĂ¡Â»Âc dashboard theo chi nhÄ‚Â¡nh | Admin; manager fixed to own branch | Staff JWT | Branch selector/dashboard | Dashboard query | Dashboard branch scope | Include Candidate | `Xem dashboard KPI` | HIGH |
| D12-03 | Xem business insights | Manager, Admin | Staff JWT | `BusinessInsights.tsx` | `GET /api/BusinessInsight` | BusinessInsight persistence/query | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D12-04 | Ă„ÂÄ‚Â¡nh dĂ¡ÂºÂ¥u insight Ă„â€˜Ä‚Â£ Ă„â€˜Ă¡Â»Âc | Manager, Admin | Staff JWT | Business insights UI | `POST /api/BusinessInsight/{id}/read` | Controller/EF update | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D12-05 | XĂ¡Â»Â­ lÄ‚Â½/resolve insight | Manager, Admin | Staff JWT | Business insights UI | `POST /api/BusinessInsight/{id}/resolve` | Controller/EF update | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D12-06 | Xem danh sÄ‚Â¡ch chi phÄ‚Â­ | Manager, Admin | Staff JWT | `ExpenseManagement.tsx` | `GET /api/Expense` | Expense branch filtering | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D12-07 | TĂ¡ÂºÂ¡o chi phÄ‚Â­ | Manager, Admin | Staff JWT | Expense form | `POST /api/Expense` | Expense controller | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D12-08 | SĂ¡Â»Â­a chi phÄ‚Â­ | Manager, Admin | Staff JWT | Expense form | `PUT /api/Expense/{id}` | Expense service/controller | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D12-09 | XÄ‚Â³a chi phÄ‚Â­ | Manager, Admin | Staff JWT | Expense management | `DELETE /api/Expense/{id}` | Expense controller | Actor Use Case | Ă¢â‚¬â€ | HIGH |
| D12-10 | PhÄ‚Â¢n tÄ‚Â­ch tÄ‚Â i chÄ‚Â­nh | Manager, Admin | Staff JWT | AI/business insight interface | AI `get_financial_analysis` tool | Financial analysis service | Actor Use Case | Ă¢â‚¬â€ | MEDIUM |

### Diagram recommendation

**SPLIT or use one high-level Management Reporting diagram with three clearly separated groups:**

- Dashboard/KPI.
- Business Insights.
- Expense Management.

Expense management should not be represented as an `include` of dashboard viewing. It is an independent management goal.

## 15. Branch management / System settings assessment

### Branch management

Supported actions:

- List active branches publicly.
- Create branch.
- Update branch.
- Toggle active status.
- Delete branch.

Only Admin can mutate branches.

Recommendation: **GENERAL-DIAGRAM ONLY or separate small diagram.**

It is important to the thesis, but the action set is small. It can be shown in the general Admin/Management diagram unless branch administration is a thesis focus.

### Table and area management

Supported actions:

- Create/update/delete tables.
- Create/update/delete areas.
- Update table status.
- Generate/display server-issued QR token.

Recommendation: Include under a Ă¢â‚¬Å“Branch Operations and Table ManagementĂ¢â‚¬Â diagram, not under employee management.

### System settings

Supported actions:

- Read/update branch-scoped system settings.
- Read/update receipt settings.
- Configure VAT/service fee and related order behavior.
- Configure receipt display fields.
- Configure bank/payment-related receipt information where exposed.

Recommendation: **KEEP as a separate small diagram if configuration is important to the thesis; otherwise GENERAL-DIAGRAM ONLY.**

### Receipt settings

Receipt settings are branch-sensitive and actor-visible. They should not be treated as an internal implementation detail.

### QR ordering settings

The QR token lifecycle is a system-supported behavior but should not be drawn as Ă¢â‚¬Å“JWT claim parsing.Ă¢â‚¬Â The business-level use case is:

- `QuĂ¡ÂºÂ£n lÄ‚Â½ QR bÄ‚Â n`
- `TĂ¡ÂºÂ¡o/xem mÄ‚Â£ QR bÄ‚Â n`
- `Ă„ÂĂ¡ÂºÂ·t mÄ‚Â³n qua QR`

## 16. Final recommended decomposition diagram set

| Final Diagram | Actors | Main Child Use Cases | Decision |
|---|---|---|---|
| Customer / Guest Ordering | Guest, Customer, Anonymous, Staff continuation | View menu, search, item detail, toppings, cart, submit order, QR entry | KEEP |
| Customer Order Status and History | Registered Customer, limited Guest, Staff | View current order, status, details, history | KEEP |
| Online Reservation | Anonymous, Customer, Staff | Select branch/table/time, create reservation, view/update, staff status | KEEP |
| Loyalty | Registered Customer, Cashier, Manager, Admin | View points/history, redeem points | KEEP |
| Promotion Management | Admin, Anonymous/Customer viewer | Manage and view promotions | MERGE with Catalog or separate small diagram |
| AI Assistant | Customer, Staff, Manager, Admin | Chat, menu lookup, reservation, order lookup, analytics, permissioned actions | KEEP |
| POS Order Processing | Cashier, Employee, Manager, Admin | Select table, create/update order, accept web request, payment, invoice | KEEP |
| Kitchen Display System | Kitchen, Manager, Admin | Active requests, status updates, history, availability | KEEP |
| Attendance and Work Schedule | Employee, Cashier, Kitchen, Manager, Admin | Check-in/out, own schedule, create/delete schedules, review attendance | KEEP |
| Cashier Shift Management | Cashier, Manager, Admin | Open, close, reconcile, history | KEEP |
| Employee Management | Manager, Admin | Employee CRUD, role/branch assignment, activation | SPLIT from branch operations |
| Branch/Table Operations | Admin, Manager, Cashier, Employee | Branches, tables, areas, table status, QR management | KEEP or general diagram |
| Catalog Management | Admin, Manager, Kitchen, Customer | Products, toppings, availability, promotions | KEEP |
| Inventory and Stock Control | Manager, Admin, operational staff | Inventory master, balances, receipts, issues, ledger, low stock, expense link | KEEP as separate diagram |
| Dashboard and Business Insights | Manager, Admin | KPI dashboard, branch filtering, insights, read/resolve | KEEP |
| Expense Management | Manager, Admin | View/create/update/delete expenses | KEEP or combine with financial management |
| System/Receipt Settings | Admin, Manager, authorized staff | VAT/service fee, receipt settings, branch settings | GENERAL-DIAGRAM ONLY or separate |

Recommended final thesis set: approximately **15Ă¢â‚¬â€œ17 diagrams**, depending on whether Promotion, Branch Operations, Expense, and Settings are kept separate.

The original 12 candidates should not be reproduced literally:

- UC-D10 should be split.
- UC-D11 should be split.
- UC-D12 should be split or clearly grouped.
- UC-D04 should focus on Loyalty and avoid unsupported voucher redemption claims.
- UC-D02 should distinguish Registered Customer history from Guest QR capabilities.

## 17. Unsupported / uncertain functionality

Do not draw these as confirmed current use cases:

- Guest QR full order history UI.
- Customer voucher redemption.
- Complete promotion-to-order discount application.
- Customer rank/tier management unless separately evidenced.
- Staff rejection of Web order requests.
- Autonomous AI restaurant management.
- Guest AI interaction.
- Generic Ă¢â‚¬Å“Employee loginĂ¢â‚¬Â as a confirmed production login path; the stored `employee` role exists, but current login mode behavior requires careful distinction from the staff shell.
- Inventory consumption automatically tied to product sales; current inventory flow is stock-receipt/issue based.
- removed staff-finance module management; the removed staff-finance module subsystem is outside current scope.
- Ă¢â‚¬Å“Manage all branchesĂ¢â‚¬Â for Manager; Manager is branch-scoped.
- Ă¢â‚¬Å“Manage catalog by branchĂ¢â‚¬Â; Products, Toppings, and Promotions are currently global/shared master data.
- Ă¢â‚¬Å“Check customer paymentĂ¢â‚¬Â as a customer-initiated action; payment is staff/POS functionality.

## 18. UML consistency warnings

1. Every actor-visible child use case must have an explicit actor association.

   Examples:

   - `Customer -> ChĂ¡Â»Ân mÄ‚Â³n`
   - `Customer -> QuĂ¡ÂºÂ£n lÄ‚Â½ giĂ¡Â»Â hÄ‚Â ng`
   - `Customer -> GĂ¡Â»Â­i yÄ‚Âªu cĂ¡ÂºÂ§u gĂ¡Â»Âi mÄ‚Â³n`
   - `Cashier -> TiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n yÄ‚Âªu cĂ¡ÂºÂ§u Web`
   - `Kitchen -> CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t trĂ¡ÂºÂ¡ng thÄ‚Â¡i chĂ¡ÂºÂ¿ biĂ¡ÂºÂ¿n`

2. Do not use `<<include>>` to mean Ă¢â‚¬Å“happens next.Ă¢â‚¬Â

   For example, `GĂ¡Â»Â­i yÄ‚Âªu cĂ¡ÂºÂ§u gĂ¡Â»Âi mÄ‚Â³n -> TiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n yÄ‚Âªu cĂ¡ÂºÂ§u Web` is a business flow, not automatically an include.

3. Do not use `<<extend>>` to mean Ă¢â‚¬Å“may happen later.Ă¢â‚¬Â

4. QR bootstrap should be represented at the business level as Ă¢â‚¬Å“XÄ‚Â¡c lĂ¡ÂºÂ­p phiÄ‚Âªn QR bÄ‚Â nĂ¢â‚¬Â or Ă¢â‚¬Å“QuÄ‚Â©t QR bÄ‚Â n,Ă¢â‚¬Â not as JWT parsing or claim extraction.

5. EF queries, repositories, DTO mapping, branch-claim parsing, and ownership checks are internal behaviors and should normally be excluded.

6. Payment, kitchen processing, and staff acceptance occur after customer order submission. They belong in separate staff/KDS diagrams or activity diagrams.

7. Guest QR and Registered Customer are different actors even though both may carry JWT role `customer`.

8. Manager is not a global administrator. Manager use cases must be branch-scoped.

9. Employee position must not be used as an actor authorization source. Use the concrete role.

10. AI tool invocation should be shown as conditional behavior under the AI Assistant, not as a direct actor-to-database relationship.

11. `QuĂ¡ÂºÂ£n lÄ‚Â½ nhÄ‚Â¢n sĂ¡Â»Â± vÄ‚Â  vĂ¡ÂºÂ­n hÄ‚Â nh chi nhÄ‚Â¡nh` is too broad for one detailed use-case diagram.

12. Inventory should not be hidden inside generic Menu Management. It has enough independent behavior to justify its own diagram.

13. Promotion visibility is supported, but voucher discount application is not sufficiently evidenced.

14. Customer WebĂ¢â‚¬â„¢s cart is temporary frontend state. Do not represent it as a persisted Ă¢â‚¬Å“cart managementĂ¢â‚¬Â subsystem.

15. Order acceptance does not necessarily create a completely new Order. Current service logic may accept, merge, or update an existing pending order depending on context. Use Ă¢â‚¬Å“TiĂ¡ÂºÂ¿p nhĂ¡ÂºÂ­n yÄ‚Âªu cĂ¡ÂºÂ§u WebĂ¢â‚¬Â rather than Ă¢â‚¬Å“TĂ¡ÂºÂ¡o Ă„â€˜Ă†Â¡n mĂ¡Â»â€ºi khi chĂ¡ÂºÂ¥p nhĂ¡ÂºÂ­n.Ă¢â‚¬Â

## Final verdict

`USE CASE SOURCE AUDIT: PASS Ă¢â‚¬â€ READY FOR PLANTUML DECOMPOSITION`




