# UI/UX Consistency Audit

**Audit date:** 2026-09-30  
**Scope:** `apps/admin-web`, `apps/customer-web`  
**Mode:** audit only. No frontend, backend, UI, or configuration files were changed.

## 1. Executive summary

The frontend is implemented with React, TypeScript, Vite, Tailwind CSS and `lucide-react`. It has a recognizable visual language, but it is not a formal shared design system. Most UI is built directly inside page components with long Tailwind class strings.

The current state is **PARTIAL to FRAGMENTED**:

- Tailwind is shared within each application, but there are no shared Button/Input/Table/Dialog/Toast primitives.
- Admin pages mix a newer slate/blue dashboard style with older gray/blue CRUD layouts.
- Customer pages use a more cohesive mobile-oriented visual language, but use many page-specific modal and card patterns.
- Navigation is role-specific, but the admin, cashier and kitchen navbars are separate implementations with duplicated behavior.
- Business-critical POS/Kitchen/QR flows have specialized layouts and should not be forced into CRUD patterns.
- Frontend password messaging is not yet aligned with the backend policy of 8-128 characters.

No Payroll page or Payroll navigation item was found in the current frontend inventory.

## 2. Frontend inventory

There are **32 application screens**: 27 admin-side routed components, 5 customer-side screens including the conditional login screen. The router also creates multiple role-context entries for the same admin component; the table below lists the functional screens rather than counting duplicates for each role.

### AUTH

| Application | Route/state | Page | Role | Purpose | Layout/responsive status |
|---|---|---|---|---|---|
| Admin | `/` when logged out | `LoginPage` | admin, manager, cashier, kitchen login modes | Employee login and branch selection for cashier/kitchen | Centered card; responsive width, but no explicit submit loading state and stale credential placeholder |
| Admin | `/profile` | `ProfilePage` | admin/manager | Profile and employee password change | Split/sidebar layout; desktop-oriented but has responsive utilities |
| Admin | role-specific `/pos/profile`, `/kitchen/profile` | `EmployeeProfile` | cashier/kitchen | Employee profile | Centered profile layout; moderate responsive support |
| Customer | conditional before router | `CustomerLogin` | customer/guest QR session | Phone-based customer/guest session creation | Mobile card; responsive, error state present |
| Admin | `/forbidden` | `ForbiddenPage` | any authenticated employee | Permission feedback | Centered card; responsive width |

### DASHBOARD / REPORT / INSIGHT

| Route | Page | Role | Purpose |
|---|---|---|---|
| `/dashboard` | `Dashboard` | admin, manager | KPI, charts, recent orders, branch filter |
| `/business-insights` | `BusinessInsights` | admin, manager | Insight list, detail and actions |
| `/invoices`, `/pos/invoices` | `InvoiceHistory` | admin, manager, cashier | Invoice/order history and print preview |

Dashboard has the strongest reusable local pattern (`Card`, `SectionTitle`, `Skeleton`, `EmptyState`) but these primitives remain private to `Dashboard.tsx`.

### MANAGEMENT / CATALOG / OPERATIONS

| Route | Page | Role/context | Purpose |
|---|---|---|---|
| `/products` | `ProductManagement` | admin/manager | Product/menu CRUD |
| `/toppings` | `ToppingManagement` | admin/manager | Topping/option CRUD |
| `/promotions` | `PromotionManagement` | admin | Loyalty/reward promotion management |
| `/tables` | `TableManagement` | admin/manager | Area/table management |
| `/branches` | `BranchManagement` | admin/manager | Branch CRUD and status |
| `/employees` | `EmployeeManagement` | admin/manager | Employee and role management |
| `/customers` | `CustomerManagement` | admin/manager | Customer and loyalty management |
| `/expenses` | `ExpenseManagement` | admin/manager | Expense CRUD and total |
| `/reservations` | `ReservationManagement` | admin/manager | Reservation management |
| role-specific reservations | `ReservationManagement` | cashier/kitchen | Cashier flow or kitchen read-only flow |
| `/attendance` | `AttendanceManagement` | admin/manager | Attendance overview |
| role-specific attendance | `EmployeeAttendance` | cashier/kitchen | Self/admin attendance and QR face flow |
| `/schedule` | `WorkSchedulePage` | admin/manager | Work schedule planning |
| role-specific schedule | `EmployeeSchedule` | cashier/kitchen | Employee schedule view |
| `/shifts` and role variants | `ShiftManagement` | admin/manager/cashier/kitchen | Shift history/management |

`UserManagement.tsx` and `SettingsDropdown.tsx` exist but are not routed from `App.tsx`; they should be treated as unused/prototype UI until confirmed otherwise.

### POS / ORDER / PAYMENT

| Route | Page | Role | Purpose | Responsive status |
|---|---|---|---|---|
| `/pos` | `POSPage` | admin, manager, cashier | Product selection, cart/order mutation, kitchen dispatch, payment and receipt flows | Specialized split layout; desktop/tablet aware; many modal states and viewport constraints |
| `/pos/settings/receipt`, `/settings/receipt` | `ReceiptSettingsPage` | admin/manager; cashier read/use context | Receipt configuration | Form layout; moderate responsive support |
| `/print-templates` | `PrintTemplates` | admin/manager | Print template preview/configuration | Preview and modal layouts; fixed receipt preview widths are intentional |
| `/pos` for manager/admin | `POSPage` | admin/manager | POS access outside cashier context | Same specialized POS layout |

### KITCHEN

| Route | Page | Role | Purpose |
|---|---|---|---|
| `/kitchen` | `KitchenPage` | kitchen, admin/manager | Live KDS and SignalR order requests |
| `/kitchen/history` | `KitchenHistoryPage` | kitchen, admin/manager | Kitchen request history and detail |
| `/kitchen/tables` | `TableStatusPage` | kitchen | Table status view |

Kitchen has its own dark navigation and orange/blue status accents. This is an intentional domain-specific visual variant, but it duplicates navigation/dropdown/notification behavior from the other navbars.

### CUSTOMER

| Route | Page | Role | Purpose | Responsive status |
|---|---|---|---|---|
| `/` | `DigitalMenu` | customer/guest | Menu, cart, order submission, loyalty redemption | Mobile-first; bottom navigation and bottom-sheet modals |
| `/reservation` | `ReservationPage` | customer | Customer table reservation | Mobile-first; form and table selection |
| `/scan` | `QRScan` | customer/guest | Camera/QR table scan | Mobile-first camera square; requires mobile/HTTPS validation |
| `/profile` | `CustomerProfile` | customer | Profile, reservations, loyalty, password update | Mobile-first; multiple large bottom sheets/dialogs |
| conditional pre-router | `CustomerLogin` | customer/guest | Phone-based session entry | Mobile-first |

## 3. Current design system

### Actual technology and tokens

| Area | Actual implementation |
|---|---|
| Styling | Tailwind CSS 3.x utility classes |
| Component library | No Shadcn/UI, Radix, MUI, Chakra, or equivalent detected |
| Icons | `lucide-react` in both applications |
| Theme tokens | No custom Tailwind `extend` tokens; both configs are effectively default Tailwind |
| Global CSS | Minimal Tailwind directives; admin print rules; customer font/body/animation rules |
| Font | Admin mostly inherits browser/system sans; customer explicitly uses Inter/system stack |
| Breakpoints | Default Tailwind `sm`, `md`, `lg`, `xl`; no custom breakpoint definitions |
| State/theme | No formal theme provider or dark-mode system |
| Utility helper | `clsx` and `tailwind-merge` are installed in admin, but no shared UI primitive layer uses them |

### Shared component inventory

| Component/pattern | Status | Evidence |
|---|---|---|
| Admin navigation | Reusable but duplicated into `Navbar`, `CashierNavbar`, `KitchenNavbar` | `apps/admin-web/src/components/*.tsx` |
| Customer bottom navigation | Reusable within customer app | `apps/customer-web/src/components/BottomNav.tsx` |
| ChatBot | Reused within each app, but two separate implementations | `apps/admin-web/src/components/ChatBot.tsx`, `apps/customer-web/src/components/ChatBot.tsx` |
| Button | Page-specific | No shared Button component found |
| Input/select/textarea | Page-specific | Repeated inline Tailwind classes |
| Table | Page-specific | 13 files contain `<table>` |
| Card | Local to Dashboard only | `Dashboard.tsx` defines `Card` |
| Badge/status | Local/page-specific | POS, Dashboard, Kitchen use separate status class logic |
| Dialog/modal/drawer | Page-specific | Repeated `fixed inset-0` implementations |
| Toast/alert | Fragmented | Inline banners, `alert()`, custom toast blocks, page-local state |
| Loading/skeleton | Page-specific | Dashboard skeleton, spinners, plain `Đang tải...` text |
| Empty/error state | Page-specific | Dashboard local `EmptyState`; other pages use plain text or no explicit state |
| Search/filter/pagination | Page-specific | Repeated markup and behavior in management pages |
| Breadcrumb/PageHeader | Not standardized | Each page creates its own header/layout |

## 4. Visual consistency findings

### Typography

- Page titles vary between `text-xl`, `text-2xl`, `text-3xl`, uppercase/italic headings, and `font-black` dashboard headings.
- Admin CRUD pages frequently use very small uppercase labels (`text-[9px]`/`text-[10px]`), while Dashboard/Kitchen use a more conventional slate typography scale.
- Customer UI intentionally uses compact uppercase labels, but the same semantic actions are not typographically aligned with admin UI.
- Helper text and error text have no shared scale or component.

### Spacing, radius, shadow and color

- Page padding varies from `p-4`, `p-6`, `p-8`, `p-10`, to `p-12`.
- Modal radii range from `rounded-xl`/`rounded-2xl` to `rounded-[3rem]`; customer bottom sheets use very large radii such as `rounded-t-[3rem]`.
- Shadows range from `shadow-sm` to custom `shadow-2xl` and arbitrary blue/red shadows.
- Backgrounds mix `#f0f2f5`, `#f8f9fa`, `gray-50`, `slate-50`, and customer `gray-50`.
- Primary blue appears as `#0070f4`, `blue-600`, and `blue-700`; Kitchen introduces orange and slate; status colors are not centralized.

### Actions and controls

- Add/create actions are represented by different icon/text/button combinations across pages.
- Delete/edit actions vary between icon-only buttons, text links, and full buttons.
- Save/cancel controls have different placement, width, capitalization, and disabled styles.
- Close controls are often icon-only without a consistent accessible label.
- Search controls range from full inputs to icon-only buttons.

## 5. Standard management page structure

The most reusable target structure is:

```text
PageHeader
  title + description
  primary action

Toolbar
  search + filters + secondary actions

Content
  table/list/grid
  loading / empty / error state

Pagination

Dialog or sheet
  form
  validation
  cancel + submit
```

Pages closest to this pattern are `EmployeeManagement`, `ProductManagement`, `BranchManagement`, and `InvoiceHistory`, but they implement it independently. `ExpenseManagement` is notably compressed into a one-file inline layout with a simpler table/modal treatment. `SystemSettings`, `POSPage`, `KitchenPage`, and `DigitalMenu` are specialized and should retain different structures.

## 6. Table consistency

### Actual state

Thirteen admin files contain `<table>` markup. There is no shared table component. Observed variations include:

- header casing and typography;
- row padding from compact `p-3` to spacious `px-8 py-5`;
- action columns as icon-only controls or text links;
- inconsistent status badge colors and shape;
- different loading behavior: spinner, plain text, skeleton, or no visible state;
- different empty-state language and layout;
- selective `overflow-x-auto`; schedule pages use explicit `min-w-[1000px]`/`min-w-[1200px]`.

### Recommended shared pattern

Use a shared table shell with configurable density, a consistent `TableHeader`, `TableRow`, action column, `StatusBadge`, `TableEmptyState`, `TableLoadingState`, and an explicit mobile overflow policy. Do not force POS order lines or Kitchen cards into the same table component.

## 7. Form consistency

Observed forms include login, employee, branch, product, topping, promotion, table, reservation, expense, receipt/settings, customer profile, and password change.

Common inconsistencies:

- Labels and required markers are not standardized; most required fields rely only on the HTML `required` attribute.
- Placeholder language and casing vary widely.
- Error messages are sometimes inline, sometimes `alert()`, and sometimes only logged to the console.
- Disabled/loading states exist in some forms but not consistently in all submit actions.
- Form controls vary between bordered rounded inputs, borderless gray inputs, and bottom-border-only inputs.
- Dialog form widths and spacing vary substantially.

### Password-specific finding

Backend policy is now 8-128 characters, but frontend references are inconsistent:

- `ProfilePage` checks minimum 8 characters but displays older composition guidance requiring uppercase, lowercase, digit and special character.
- `LoginPage` still uses a placeholder suggesting `123456`.
- Employee creation and customer profile password inputs do not visibly communicate the 8-128 policy.
- Customer profile password change does not show the same client-side policy guidance.

This is a **P1 consistency and usability issue** because users can receive different expectations from the UI and backend.

## 8. Feedback states

### Loading

Loading support exists on many pages, but the visual treatment is fragmented:

- Dashboard has reusable local skeletons.
- Kitchen uses a full-screen spinner initially.
- Some CRUD pages use plain `Đang tải...` text.
- Some action buttons show spinners or disabled text; others remain visually unchanged.

### Empty

Dashboard, Kitchen history and some management pages have explicit empty messages. There is no shared empty-state component, icon or action convention.

### Error

Error banners exist in Dashboard, Kitchen and customer login. Other pages frequently use `alert()` or only `console.error`. A repository search found approximately 99 `alert()` call sites across the two frontend source trees; this is a strong consistency finding, not an assertion that every call is user-visible in every state.

### Success

Success is represented by inline green banners, modal success screens, custom POS toast blocks, and browser alerts. There is no shared toast/notification contract.

### Disabled

Disabled controls generally use `disabled:opacity-*` or `disabled:bg-*`, but the visual treatment is not standardized. Several icon-only actions do not visibly communicate disabled or loading state.

## 9. Responsive audit

### Good existing patterns

- Customer app is intentionally mobile-first with bottom navigation and bottom-sheet dialogs.
- POS uses a mobile/desktop split transition (`w-full h-[52%]` to `md:w-[60%] md:h-full`).
- Dashboard uses responsive grids and an overflow wrapper for its recent-orders table.
- Admin navbars include mobile menu behavior.
- QR scanner uses an aspect-ratio camera region and small-screen-friendly sizing.

### Risks

- `EmployeeSchedule` and `WorkSchedulePage` intentionally use `min-w-[1000px]` and `min-w-[1200px]`; they require horizontal scrolling and are not practical on mobile.
- Several admin pages use full viewport-height layouts and independent scroll containers; this can create nested-scroll and keyboard/viewport issues on small screens.
- Large modal radii and padding (`rounded-[3rem]`, `p-10`/`p-12`) reduce usable form width on narrow screens.
- Invoice/receipt and print-preview flows contain fixed-width content by design; the surrounding preview needs explicit mobile overflow verification.
- Customer `DigitalMenu` and `CustomerProfile` have multiple bottom sheets with large padding and `max-h` rules; the implementation is responsive-aware but should be manually verified against small-height devices and virtual keyboards.
- Notification/popover widths such as `w-80` and chatbot dimensions need viewport testing at 320px.
- `main` height calculations differ between `100vh` and `100dvh` across admin role shells.

## 10. Role UX audit

| Role | Current UX | Finding |
|---|---|---|
| admin | Common admin `Navbar`, dashboard, management and settings | Broad navigation; branch/settings items are conditionally shown, but many page-level checks are duplicated |
| manager | Common admin `Navbar`, branch-limited management | Similar UX to admin with hidden/disabled actions; distinction is not always explained in UI |
| employee | Backend role exists, but `LoginPage` login mode and `App` state type do not expose a dedicated employee mode | **P1 role UX gap**; needs confirmed intended login/navigation behavior |
| cashier | Dedicated `CashierNavbar`, POS-first route and limited menu | Appropriate specialized flow, but navbar duplicates notification/menu behavior |
| kitchen | Dedicated `KitchenNavbar`, KDS-first route | Appropriate specialized flow; orange/slate visual variant is intentional |
| customer | Separate mobile web app with phone/guest session and bottom nav | Cohesive mobile flow; guest and registered session terminology is not clearly surfaced to users |

Frontend guards are UX guards only; backend remains the authorization boundary. The audit found no Payroll navigation item or page.

## 11. Business-critical screens

### POS

`POSPage` is a specialized, high-density workflow with product grid, cart, table/branch controls, kitchen status, payment and multiple dialogs. It should not be normalized into the management CRUD page pattern. Main UX risks are modal count, dense controls, and small-screen readability. It already has responsive split behavior and inline kitchen/payment feedback.

### Kitchen

`KitchenPage` uses live SignalR state, recovery messaging and status cards. The distinct orange/salon visual treatment is justified by the operational context. Standardize status semantics and feedback components without making the KDS look like a generic data table.

### QR customer ordering

`DigitalMenu`, `QRScan`, `ReservationPage` and `CustomerLogin` form a mobile-first flow. Bottom sheets and horizontal category scrolling are appropriate. Main risks are browser alert usage, session terminology, and small viewport/modal testing.

### Payment and reservation

Payment feedback is embedded in POS-specific states; reservation feedback differs between admin and customer pages. A shared feedback vocabulary would reduce ambiguity while preserving different layouts.

### Dashboard/Insights and AI

Dashboard has the most coherent local design language and should be the reference for management typography/card treatment. Business Insights uses a two-pane, fixed-height layout suitable for investigation but potentially fragile on short screens. ChatBot is duplicated between applications and uses separate visual implementations.

## 12. Terminology audit

Current strategy is **mixed Vietnamese/English**.

Examples that should be standardized:

- `Employee` / `Staff` / `Nhân viên`;
- `Branch` / `Cơ sở` / `Chi nhánh`;
- `Order` / `Đơn hàng` / `Order ID`;
- `Table` / `Bàn`;
- `Kitchen` / `Nhà bếp` / `Bếp`;
- `Created`, `Accepted`, `Completed`, `Order ID` appearing beside Vietnamese UI;
- `Dashboard`, `Business Insights`, `Support`, `Profile`, and `ChatBot` alongside Vietnamese labels.

Recommended product-facing vocabulary is Vietnamese, with technical identifiers retained only where useful (for example, invoice code or order code). Status labels should have one canonical display map shared by POS, Kitchen, Invoice History and notifications.

## 13. Duplication analysis

### Worth extracting/shared standardization

1. `PageHeader` with title, description, primary action and optional branch context.
2. `Button` variants: primary, secondary, danger, icon, loading/disabled.
3. Form field primitives with label, required marker, helper/error text and consistent focus state.
4. `DataTable` shell with responsive overflow, loading, empty and error slots.
5. `StatusBadge` with canonical business status-to-color mapping.
6. `Modal`/`Sheet` shell with responsive max-height, close behavior and focus/accessibility rules.
7. `Toast`/inline feedback primitives replacing ad hoc success/error blocks and most browser alerts.
8. `Pagination` and search/filter toolbar.
9. Admin navigation data model shared by `Navbar`, `CashierNavbar` and `KitchenNavbar`, while preserving role-specific visual/layout shells.
10. Dashboard `Card`, `SectionTitle`, `Skeleton` and `EmptyState` promoted only if at least three screens adopt them.

### Do not extract prematurely

- POS cart/order panels;
- Kitchen live request cards;
- customer bottom sheets;
- receipt print layouts;
- one-off support/forbidden/QR camera visuals.

## 14. Priority matrix

| ID | Page/route | Problem | Severity | Current component | Recommended shared pattern | Files affected |
|---|---|---|---|---|---|---|
| UI-001 | Admin login `/` | Login placeholder suggests old `123456` credential and does not reflect current password policy | P1 | `LoginPage` | Shared password-policy helper text; remove default-credential wording | `features/auth/pages/LoginPage.tsx` |
| UI-002 | Admin profile/password; customer profile | Password guidance is inconsistent: frontend text claims composition rules while backend requires 8-128; customer UI lacks equivalent guidance | P1 | `ProfilePage`, `CustomerProfile`, employee form | Shared password field/policy message | `ProfilePage.tsx`, `CustomerProfile.tsx`, `EmployeeManagement.tsx` |
| UI-003 | Admin employee role flow | Backend role `employee` exists but login mode/App role typing/navigation exposes only admin/manager/cashier/kitchen | P1 | `LoginPage`, `App`, navbars | Confirm and implement one explicit employee UX profile | `LoginPage.tsx`, `App.tsx`, navigation components |
| UI-004 | Admin management tables | Schedule screens require `min-w-[1000px]`/`min-w-[1200px]`, making mobile use impractical | P1 | `EmployeeSchedule`, `WorkSchedulePage` | Responsive table strategy or mobile list/card mode | Both schedule pages |
| UI-005 | Admin CRUD forms | Many workflows use browser `alert()` for errors/success; feedback is inconsistent and interruptive | P1 | Page-local handlers | Shared toast/inline feedback component | Approximately 99 call sites across both apps; prioritize operations/settings/customer flows |
| UI-006 | Admin role shells | `Navbar`, `CashierNavbar`, `KitchenNavbar` duplicate notifications, dropdowns and menu behavior; role UX can drift | P1 | Three navbar components | Shared navigation model and primitives with role-specific shells | `components/Navbar.tsx`, `CashierNavbar.tsx`, `KitchenNavbar.tsx` |
| UI-007 | Admin route/page access | Common routes are rendered under role shells with varied frontend checks; some pages can be reached before showing an API/forbidden response | P1 | `App.tsx` and page-local role checks | Central UX route capability map; retain backend authorization | `App.tsx`, navigation and management pages |
| UI-008 | Customer session entry | Guest vs registered customer session purpose is not clearly communicated in UI | P1 | `CustomerLogin`, `DigitalMenu` | Explicit guest/registered session copy without changing backend semantics | `customer-web/src/pages/CustomerLogin.tsx`, `DigitalMenu.tsx` |
| UI-009 | All management pages | No shared PageHeader/Button/Input/Table/Dialog primitives; same actions look different | P2 | Page-local Tailwind markup | Establish shared primitives from current Dashboard patterns | Most admin feature pages |
| UI-010 | All admin layouts | Background, radius, shadow, padding and typography tokens vary materially | P2 | Inline utility classes | Document and adopt a small token layer | Admin feature pages and navbars |
| UI-011 | Tables | 13 independent table implementations with inconsistent density, empty/loading/action states | P2 | Inline `<table>` markup | Shared DataTable shell and state slots | 13 current table files |
| UI-012 | Modal/dialog flows | Modal radius/width/padding/z-index conventions vary from `rounded-xl` to `rounded-[3rem]` and multiple z-index levels | P2 | Inline fixed overlays | Shared Modal/Sheet primitives | Admin CRUD, POS, customer pages |
| UI-013 | Feedback states | Loading/empty/error/success states are page-specific; some pages only log errors | P2 | Local state blocks | Shared feedback primitives | All data-fetching pages |
| UI-014 | Status labels | POS, Kitchen, Dashboard, Invoice History and notifications use separate status color/label logic | P2 | Page-local helper/classes | Canonical `StatusBadge` mapping | `POSPage`, `KitchenPage`, `Dashboard`, `InvoiceHistory`, nav notifications |
| UI-015 | Customer app | Browser alerts and confirmation dialogs are used for order/reward errors and confirmations | P2 | `DigitalMenu`, `ReservationPage` | Mobile toast/confirmation sheet | Customer pages |
| UI-016 | Admin/Customer copy | Vietnamese UI is mixed with English technical/status terms without a documented glossary | P2 | Many pages | Product terminology glossary and display maps | Both apps |
| UI-017 | All dialogs | Icon-only close/actions do not consistently expose accessible labels | P2 | Inline buttons | Shared accessible icon button | Modal-heavy pages |
| UI-018 | Customer modals | Large bottom sheets and fixed max-heights need short-viewport/virtual-keyboard verification | P2 | `DigitalMenu`, `CustomerProfile`, `ReservationPage` | Shared mobile Sheet with safe-area/keyboard policy | Customer modal pages |
| UI-019 | Admin cosmetic language | Uppercase/italic/font-black styling is overused and differs by feature | P3 | Page-local typography | Restrained typography scale | Admin feature pages |
| UI-020 | Brand/nav | Brand names and accent colors differ among admin, cashier, kitchen and customer shells | P3 | Four app shells | Shared brand/identity tokens with intentional Kitchen accent | Navbars and login/customer shells |
| UI-021 | Animation | `animate-in`, custom animations and hover transitions are not governed by one motion policy | P3 | Page-local classes/global customer CSS | Motion tokens and reduced-motion review | Both apps |

No P0 issue was established from source inspection alone. P0 would require a confirmed blocker in a critical live workflow; this audit is static and does not replace manual device/browser testing.

## 15. Proposed design system based on existing UI

### A. Layout

Use admin management shell: `bg-slate-50`, responsive page padding, max content width, and one primary scroll owner. Keep POS/Kitchen/customer shells specialized.

### B. PageHeader

Adopt Dashboard's title/description/action pattern with optional branch selector and status context.

### C. Typography

Use a small scale: page title, section title, body, label, helper/error. Reduce arbitrary `text-[9px]`/`text-[10px]` usage to compact metadata only.

### D. Buttons

Define primary blue, secondary neutral, danger red, outline and icon-only variants. All variants need hover, focus-visible, disabled and loading states.

### E. Forms

Use one field structure: label, required marker, control, helper/error. Centralize password policy copy as 8-128 characters.

### F. Tables

Use a shared shell for headers, density, row hover, actions, responsive overflow and states. Specialized schedules may use a separate responsive planning grid.

### G. Cards

Promote Dashboard's `Card` only after confirming reuse in management/insight pages; preserve POS/Kitchen card density where necessary.

### H. Dialogs

Use one modal and one mobile sheet pattern with standard widths, radius, padding, max-height, escape/overlay behavior and accessible labels.

### I. Badges/status

Centralize status display names and semantic colors. Do not derive colors separately in POS, Kitchen and reports.

### J. Toast/feedback

Provide success/error/info/warning variants and retain inline errors for form fields. Replace browser `alert()` progressively.

### K. Loading

Use spinner for actions, skeleton for known page structure, and full-page loader only for initial shell blocking.

### L. Empty/Error

Use consistent icon, message, optional explanation and retry/primary action. Keep domain-specific copy where useful.

### M. Responsive rules

- Desktop management screens may scroll wide tables deliberately, but must provide a usable mobile alternative for critical actions.
- POS/Kitchen keep specialized density and orientation behavior.
- Customer screens remain mobile-first.
- Dialogs must fit 320px width and short viewport heights.
- Prefer `100dvh` for app shells and document nested-scroll ownership.

### N. Icons

Continue `lucide-react`; standardize icon sizes by context (16/18/20/24) and add accessible labels to icon-only controls.

## 16. Recommended fix batches

### Batch 1 - Foundations and feedback

Create only shared primitives that have at least three consumers: button, field, status badge, modal/sheet, toast, loading/empty/error states. Align password copy with backend policy. Build and manually test affected forms.

### Batch 2 - Layout and navigation

Introduce a shared navigation data model and PageHeader. Keep admin/cashier/kitchen visual shells distinct. Verify role-specific menu visibility and route fallbacks.

### Batch 3 - Management CRUD

Migrate Product, Topping, Promotion, Branch, Employee, Customer, Expense and Reservation screens incrementally to shared form/table/modal patterns. One feature at a time, with frontend build after each group.

### Batch 4 - POS and Kitchen

Standardize only cross-cutting feedback/status elements. Preserve POS split layout and Kitchen operational density. Manually test order, kitchen dispatch, status update and payment displays.

### Batch 5 - Customer QR/order

Unify mobile sheet/feedback patterns, preserve QR guest flow, and clarify guest versus registered session copy. Test 320px width, camera permissions and short-height devices.

### Batch 6 - Dashboard/Insights and AI

Promote Dashboard local card/skeleton patterns where justified, standardize insight status/detail states, and assess whether chatbot visuals can share only primitives rather than forcing identical shells.

### Batch 7 - Responsive and final polish

Test desktop/tablet/mobile breakpoints, schedule grids, dialogs, POS, Kitchen, QR ordering and keyboard navigation. Address P3 typography/animation/brand details last.

## 17. Final metrics

```text
TOTAL PAGES: 32 application screens
P0: 0
P1: 8
P2: 10
P3: 3
TOP 5 PRIORITY SCREENS:
1. POS (/pos)
2. Kitchen KDS (/kitchen)
3. Customer Digital Menu and QR ordering (/ and /scan)
4. Employee management/password form (/employees)
5. Admin login/profile password flows (/ and /profile)
SHARED COMPONENTS TO STANDARDIZE:
PageHeader, Button, FormField, DataTable, StatusBadge, Modal/Sheet,
Toast/Feedback, Loading/Skeleton, Empty/ErrorState, Pagination,
navigation data model, password-policy helper text
RESPONSIVE RISK: HIGH
DESIGN SYSTEM STATUS: FRAGMENTED
RECOMMENDED FIRST FIX BATCH: Batch 1 - foundations, feedback and password-policy copy
```

## 18. Evidence index

- Routing and role shells: `apps/admin-web/src/App.tsx`, `apps/customer-web/src/App.tsx`.
- Admin navigation: `apps/admin-web/src/components/Navbar.tsx`, `CashierNavbar.tsx`, `KitchenNavbar.tsx`.
- Customer navigation: `apps/customer-web/src/components/BottomNav.tsx`.
- Global styling: `apps/admin-web/src/index.css`, `apps/customer-web/src/index.css`, both `tailwind.config.js` files.
- Design dependencies: both `package.json` files.
- Password UI mismatch: `apps/admin-web/src/features/auth/pages/LoginPage.tsx`, `ProfilePage.tsx`, `apps/admin-web/src/features/hrm/pages/EmployeeManagement.tsx`, `apps/customer-web/src/pages/CustomerProfile.tsx`.
- Business-critical flows: `apps/admin-web/src/features/pos/pages/POSPage.tsx`, `apps/admin-web/src/features/kitchen/pages/KitchenPage.tsx`, `apps/customer-web/src/pages/DigitalMenu.tsx`, `QRScan.tsx`, `ReservationPage.tsx`.
- Table/modal/feedback duplication: the page files listed in the table and `rg` inventory used during this audit.
