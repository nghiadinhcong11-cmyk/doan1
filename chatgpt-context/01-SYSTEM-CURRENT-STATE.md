# System Current State

Status labels in this file mean `ÄĂ£ xĂ¡c minh tá»« source` unless explicitly marked otherwise. No statement here is a runtime PASS.

## Purpose and scope

The repository is a multi-branch restaurant POS and management system. Its current documented focus is sales/order management, branch operations, revenue/expense tracking, staff operations, customer QR ordering, kitchen display/realtime communication, invoices/receipts, loyalty, inventory and a local Mock AI assistant. Staff profile data and attendance/schedule features remain separate source concerns.

Sources: `README.md`, `AGENTS.md`, `services/api/src/`.

## Stack and applications

- Backend: ASP.NET Core 8, C#, EF Core 8, PostgreSQL/Npgsql, JWT Bearer and SignalR.
- Admin/POS web: `apps/admin-web`, React/Vite/TypeScript/Tailwind. Management and operations routes are split by role-specific route modules.
- Customer web: `apps/customer-web`, React/Vite/TypeScript. Supports customer login, public/guest menu, QR scan and reservation/profile screens.
- Shared contract: `packages/shared/swagger.json` and generated API types under `packages/shared/types/`.
- Backend layers: `Domain`, `Application`, `Infrastructure`, `WebAPI`, plus `AI` under `services/api/src/`.

## Roles and branch scope

The role names used by the current source are `admin`, `manager`, `employee`, `cashier`, `kitchen` and `customer`. `admin` is global in the documented model; staff roles are normally branch-scoped; customer access is data/session-scoped. Role is authoritative for authorization; UI position labels are not security boundaries.

JWT/authentication and role/branch handling are configured in `services/api/Program.cs`, `services/api/src/Infrastructure/Services/JwtService.cs`, `services/api/src/Application/Common/Security/RoleNames.cs` and the WebAPI controllers. Frontend guards and local storage are UX/session mechanisms, not the security boundary.

## Main modules

The current source contains controllers and services for branches, products/toppings/promotions, tables/areas/reservations, employees/attendance/shifts/schedules, orders/invoices, expenses, dashboard/insights, notifications, inventory, loyalty, receipt/system settings and guest QR. Presence of a controller or service proves code exists, not that the complete user flow has been runtime verified.

Representative paths: `services/api/src/WebAPI/Controllers/`, `services/api/src/Application/Services/`, `services/api/src/Domain/Entities/` and `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs`.

## Order flow and financial authority

The intended flow is:

`QR/Customer or staff input â†’ OrderController â†’ OrderService â†’ persisted Order â†’ KitchenService/SignalR â†’ POS/KDS/cashier â†’ payment/invoice`.

For guest QR, `GuestController`, `GuestSessionContext`, QR token handling and `OrderController` derive/validate the server-side session context. Client-provided branch/table/order values must not become authority.

`OrderService` is the financial source of truth. The `Order` entity stores `SubTotal`, VAT/service-fee snapshots, `Discount`, `TotalAmount`, payment state and payment timestamp. The service recalculates authoritative values from database-backed product/option/topping data and settings. The payment controller/service compares payment input with persisted order total and handles already-paid/conflict cases in source.

The exact behavior still requires runtime evidence for payment, rounding, duplicate requests, terminal states, loyalty side effects and manipulated client payloads. Do not infer PASS from these source paths.

## Realtime

The backend maps `/kitchenHub` in `Program.cs`. `KitchenHub` authorizes staff roles and assigns role, user and branch-related groups. `KitchenNotifier` publishes kitchen request events; `PaymentEventRouting` publishes payment completion to selected branch/role groups. Admin frontend consumers include `KitchenPage`, POS and notification/cashier components and use SignalR automatic reconnect.

Sources: `services/api/src/WebAPI/Hubs/`, `services/api/src/Application/Services/KitchenService.cs`, `apps/admin-web/src/features/kitchen/` and relevant POS/notification components.

Static connection/event paths are verified; delivery, branch visibility, reconnect recovery and duplicate-event behavior are not verified in this context update.

## AI

Current application registration in `Program.cs` uses `MockAiProvider` through `IAiProvider`; `appsettings*.json` also selects Mock mode. `MockAiProvider` is deterministic, recognizes an allowlist of intents and does not call an external model. The registry, `AiPermissionService`, role-aware tools and product-price confirmation code are real application paths, but their end-to-end behavior is not runtime-verified here.

No provider-specific external model integration is active. Do not describe Mock AI as a real model integration.

## Local run configuration

The current development scripts/configuration identify:

- API HTTPS: `https://localhost:5000`.
- Admin Vite: `https://localhost:5173`.
- Customer Vite: `https://localhost:5174`.
- SignalR: API base plus `/kitchenHub`.
- LAN is opt-in through configured `DOAN_DEV_LAN_HOST` and HTTPS setup.
- Automatic database migration is configured off for the development launcher.

Sources: `scripts/start-dev.ps1`, `scripts/setup-https.ps1`, `services/api/Program.cs`, `services/api/appsettings.json`, `services/api/appsettings.Development.json` and the Vite configs. These are configured endpoints, not proof that processes are currently running.

## Runtime limits

This context update did not build, connect to PostgreSQL, run migrations, start the applications, call `/health`, call an API, open a browser or test SignalR. Existing evidence must be checked against the current working tree before being reused.
## Employee profile decision

EmployeeType and BasicSalary are intentionally retained as employee profile fields. They are read, bound and persisted by the current employee API and surfaced by the admin employee page. They do not establish salary calculation, salary periods, deductions or payment. Representative sources are `services/api/src/Domain/Entities/Employee.cs`, `services/api/src/WebAPI/Controllers/EmployeeController.cs`, `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` and `apps/admin-web/src/features/hrm/pages/EmployeeManagement.tsx`. This is static source confirmation, not runtime verification.
## Migration snapshot reconciliation

Static inspection of services/api/src/Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs found Employee.BasicSalary and Employee.EmployeeType. It found no Payroll, PayrollAdjustment or PayrollSettings entity blocks, payroll table mappings, payroll foreign keys or payroll relationships. Current ApplicationDbContext exposes DbSet<Employee> and configures Employee â†’ Branch. Payroll names still occur in historical migration source/designer files; this is historical migration metadata/operations, not proof that the current EF model still contains those entities or that a database is clean.



## Current policy implementation update — 2026-10-15

- Customer and loyalty are chain-wide. CustomerController now minimizes operational staff lookup responses; customer self-service and admin/manager paths retain the fuller profile contract. Runtime authorization and response tests remain pending.
- Product catalog remains shared. Availability is represented by ProductBranchAvailability per product/branch, with controller and frontend branch context. The source migration seeds existing pairs from legacy availability but has not been applied to a database.
- Order processing paths now require an active branch; invalid-branch realtime notification is suppressed or converted to an admin notification/log path. Runtime and malformed-data verification remain pending.
- Guest /profile now ends loading and shows login invitation without calling the customer profile API or clearing QR/session/cart context. Guest reservation policy remains open.
- MockAiProvider is still the only provider path; EmployeeType and BasicSalary remain employee profile fields.

- Latest source fixes: public registration is allowlisted through `CustomerRegistrationDto`; kitchen request status validates request/order, active branch and legal transition before mutation; branch-aware availability reads use the branch row or explicit `Available` when missing; KitchenPage filters availability events by selected branch and refetches after reconnect. These are source/build observations, not runtime or database evidence.

- Availability frontend boundary: POS and DigitalMenu currently refetch through existing load, branch-change or manual-refresh paths; they do not subscribe to `ProductAvailabilityChanged`. The kitchen consumer therefore does not establish live synchronization for all frontend surfaces. The missing-row `Available` value remains a source default pending confirmation.

