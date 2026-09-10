# AGENTS.md — AI Coding Rules & Project Engineering Standards

## 1. PROJECT IDENTITY

This repository is a production-oriented restaurant POS and management system.

The system contains:

* Admin/POS Web application
* Customer QR Ordering Web application
* Kitchen Display System
* Restaurant/branch management
* Employee management
* Payroll
* Receipt configuration
* System settings
* Loyalty configuration
* AI Assistant
* Real-time kitchen communication
* Payment and invoice processing

The project is NOT a prototype where business logic can be freely rewritten.

Prioritize:

1. Correctness
2. Security
3. Data integrity
4. Business-rule consistency
5. Backward compatibility
6. Smallest safe change
7. Maintainability
8. Performance

Do not perform large refactors unless explicitly requested.

---

# 2. TECHNOLOGY STACK

## Backend

* ASP.NET Core 8
* C#
* Entity Framework Core
* PostgreSQL
* Npgsql
* SignalR
* JWT Bearer Authentication

## Frontend

* React
* Vite
* TypeScript
* TailwindCSS

## Database

* PostgreSQL
* Supabase

## AI

* Gemini
* AI tool registry
* AiPermissionService

---

# 3. ARCHITECTURE

The backend follows Clean Architecture principles.

Expected dependency direction:

Domain
↓
Application
↓
Infrastructure
↓
WebAPI

## Domain

Contains:

* Entities
* Enums
* Domain-level business concepts

Domain must NOT depend on any external layers or frameworks (ASP.NET Core, EF Core, etc.).

## Application

Contains:

* Business services interfaces and implementations
* Application logic (Use Cases)
* DTOs

Application services encapsulate the core business rules. Infrastructure implementation details are abstracted away through interfaces.

## Infrastructure

Contains:

* EF Core and DbContext
* Implementation of persistence abstractions (PostgreSQL access)
* External service implementations (Identity, storage, etc.)
* Migrations

## WebAPI

The entry point and composition root. Contains:

* Controllers
* SignalR hubs
* Middleware
* Authentication/Authorization configuration
* Program.cs

## AI

A dedicated module for AI orchestration and tool execution.

* **AiOrchestrator**: Manages the conversation flow and model interactions. It must NOT access the database or DbContext directly.
* **AiToolRegistry**: Centralized list of approved capabilities.
* **AI Tools**: Encapsulated units of work. They must use **Application Services** to interact with the system or perform mutations.
* **AiPermissionService**: Enforces security and role-based access for every tool execution.

---

# 4. GOLDEN RULE

Before modifying code:

1. Read `AGENTS.md`.
2. Identify the target feature.
3. Find all direct consumers of the code.
4. Inspect related services/controllers/entities/DTOs.
5. Inspect frontend consumers if the API contract changes.
6. Inspect migrations if database schema is involved.
7. Understand existing behavior before changing it.
8. Make the smallest safe change.
9. Build the affected backend/frontend.
10. Run relevant tests or manual verification.
11. Report exactly what changed.

Never modify a file simply because it looks related.

---

# 5. NO BLIND CODING

Never assume an implementation is missing.

Before adding:

* Entity
* Service
* Controller
* Endpoint
* DTO
* Migration
* Hook
* API client
* SignalR event
* AI tool

search the repository first.

If an equivalent implementation already exists:

* reuse it
* extend it
* fix it

Do NOT create a duplicate implementation.

---

# 6. CHANGE SCOPE

Every task must follow:

> Smallest change that completely satisfies the requirement.

Do NOT:

* rewrite entire services
* rewrite controllers unnecessarily
* rename unrelated classes
* reorganize folders without need
* replace working libraries
* change architecture
* introduce new patterns without justification
* modify unrelated modules

If a refactor is genuinely required, explain why before doing it.

---

# 7. BUSINESS LOGIC OWNERSHIP

Business logic must have one authoritative source.

For POS financial calculations:

`OrderService` is the source of truth.

The frontend must NOT be trusted for:

* unit price
* product price
* topping price
* option price
* subtotal
* VAT amount
* service fee amount
* total amount
* payment amount
* discounts unless explicitly supported by backend business rules

Clients submit:

* product/item identity
* quantity
* selected option/topping identity
* option labels where applicable

The backend retrieves authoritative prices from the database.

---

# 8. ORDER FINANCIAL RULES

When modifying `OrderService`, preserve these rules unless the user explicitly requests a change.

## Calculation order

1. Item subtotal
2. Service fee
3. VAT
4. Existing legacy discount

Conceptually:

Subtotal
→ Service Fee
→ VAT
→ Discount
→ Final Total

VAT is calculated on:

`Subtotal + ServiceFee`

Service fee is calculated from the configured service-fee percentage.

## Rounding

Use:

```csharp
decimal
```

for money.

Never use:

```csharp
double
float
```

for monetary calculations.

Service fee and VAT are rounded independently in `OrderService.cs`:

1. **Service Fee**: Calculated from Subtotal and rounded to 0 decimals (VND precision) using `MidpointRounding.AwayFromZero`.
2. **VAT**: Calculated from `Subtotal + Service Fee` and rounded to 0 decimals using `MidpointRounding.AwayFromZero`.

Do not introduce multiple intermediate rounding operations beyond these steps.

---

# 9. ORDER FINANCIAL SNAPSHOTS

Orders contain financial snapshots for historical correctness.

Relevant fields:

* `SubTotal`
* `VatPercent`
* `VatAmount`
* `ServiceFeePercent`
* `ServiceFeeAmount`

New/updated orders populate these values.

Historical orders may contain null values.

Do NOT backfill old orders using current settings unless explicitly requested.

Historical orders must remain historically accurate.

---

# 10. PAYMENT RULE

Payment must use the persisted backend order total.

Never trust:

```text
clientTotalAmount
```

for payment validation.

The payment flow must validate against:

```text
Order.TotalAmount
```

stored by the backend.

Never allow a frontend request to overwrite the authoritative payment amount.

---

# 11. SYSTEM SETTINGS

System settings are stored in the database.

Entity:

`SystemSetting`

Important fields:

* `BranchId`
* `Key`
* `Value`
* `Type`
* `UpdatedAt`

Unique constraint:

```text
(BranchId, Key)
```

Settings can be:

* branch-specific
* global when BranchId is null (Planned feature, currently mainly branch-specific)

When resolving a setting for an order:

> Always use the Order.BranchId.

The system currently retrieves settings specifically for the requested `BranchId`. Automatic fallback to global settings is a future improvement.

Branch isolation is mandatory.

---

# 12. SUPPORTED FINANCIAL SETTINGS

Existing keys include:

* `vatPercent`
* `serviceFee`
* `vatEnabled`
* `serviceFeeEnabled`
* `serviceFeePercent`

When adding settings:

1. Preserve backward compatibility.
2. Do not silently rename existing keys.
3. Do not create duplicate keys with slightly different naming.
4. Update the relevant service and frontend together.

---

# 13. BRANCH ISOLATION

This is a multi-branch restaurant system.

Every branch-sensitive operation must verify:

```text
Authenticated User
→ authorized branch
→ requested resource branch
```

Never expose another branch's data.

Current authorized roles are:

* `admin`: Super Admin / Global scope. Có quyền quản lý toàn chuỗi và các thiết lập hệ thống.
* `manager`: Quản lý chi nhánh. Chỉ có quyền trong phạm vi chi nhánh được gán.
* `employee`: Nhân viên.
* `cashier`: Thu ngân.
* `kitchen`: Nhân viên bếp.
* `customer`: Khách hàng.

Role `manager` is planned for future implementation but is not currently used in the codebase.

---

# 14. AUTHENTICATION

Authentication uses:

```text
JWT Bearer
```

Do not:

* remove `[Authorize]`
* weaken authorization
* bypass role checks
* trust frontend role values
* expose sensitive endpoints publicly

Authentication and authorization must be enforced server-side.

---

# 15. AUTHORIZATION

Sensitive operations must have backend authorization.

Examples:

* branch management
* system settings modification
* payroll
* employee administration
* sensitive financial operations
* AI tools capable of modifying data

Frontend guards are UX only.

Frontend authorization must NEVER be considered a security boundary.

---

# 16. AI PERMISSIONS

All AI actions that invoke tools must pass through:

`AiPermissionService`

AI must NOT directly:

* modify database records
* bypass services
* bypass controllers
* bypass authorization
* call unrestricted EF Core operations
* execute arbitrary SQL

AI tool execution must be explicit and permission-controlled.

Each AI tool should define:

* purpose
* input
* output
* required permission
* read/write behavior
* affected module

---

# 17. AI TOOL SAFETY

AI tools must follow least privilege.

Prefer:

```text
AI
→ Tool
→ Application Service
→ Database
```

Do NOT implement:

```text
AI
→ DbContext
→ arbitrary database mutation
```

Read-only tools should remain read-only.

A tool that only needs to inspect orders must not have permission to update orders.

---

# 18. SIGNALR / KITCHEN

Hub:

```text
/kitchenHub
```

The Kitchen Display System depends on real-time events.

Before modifying:

* OrderService
* KitchenService
* OrderController
* KitchenHub
* OrderItem

inspect:

* POS consumers
* Kitchen consumers
* SignalR event names
* group/role behavior

Do not silently rename SignalR events.

Do not remove broadcasts.

Do not replace SignalR with polling unless explicitly requested.

---

# 19. ORDER → KITCHEN FLOW

Expected flow:

```text
POS / Customer
      ↓
OrderService
      ↓
Order created/updated
      ↓
KitchenHub
      ↓
Kitchen clients
```

Changes to:

* Order
* OrderItem
* order status
* quantity
* kitchen status

may affect Kitchen behavior.

Always verify the complete flow after changing order logic.

---

# 20. ORDER MUTATION

When order items are added, removed, or changed:

* recalculate authoritative financial values
* preserve order state rules
* preserve kitchen behavior
* preserve payment state rules
* preserve invoice consistency

Never update only the frontend representation.

---

# 21. DATABASE RULES

Database schema changes MUST use EF Core migrations.

Never manually modify PostgreSQL tables to "make the application work."

Required flow:

```text
Entity/configuration change
→ EF Core migration
→ review migration
→ database update
```

Migration location:

```text
services/api/src/Infrastructure/Persistence/Migrations
```

Never delete existing migrations just to resolve a migration conflict unless explicitly instructed.

Never modify an already-applied migration to change production schema history.

Create a new migration instead.

---

# 22. EF CORE RULES

Before changing an entity:

Inspect:

* DbContext
* entity configuration
* relationships
* indexes
* foreign keys
* existing migrations
* DTOs
* services using the entity

After schema changes:

1. Create migration.
2. Review migration.
3. Build.
4. Apply migration when appropriate.
5. Verify database compatibility.

Never assume the database schema automatically follows C# entities.

---

# 23. BACKWARD COMPATIBILITY

The project already contains existing data.

Do not assume the database is empty.

When adding fields:

* consider nullable vs required
* consider existing rows
* consider migration defaults
* consider old orders
* consider old clients

Avoid destructive migrations.

Do not drop columns/tables unless explicitly required.

---

# 24. DTO/API CONTRACTS

When modifying an API:

Inspect all consumers.

Potential consumers include:

* Admin Web
* Customer Web
* mobile/Capacitor clients
* AI tools
* other backend services

Do not change:

* property names
* enum values
* response shapes
* endpoint paths

without checking consumers.

If a breaking change is necessary, update all consumers in the same task.

---

# 25. FRONTEND API RULES

Frontend must treat backend as the source of truth for:

* authentication
* authorization
* prices
* order totals
* VAT
* service fees
* payment state
* persisted settings

Do not recreate authoritative backend calculations in React merely to "match" the backend.

Frontend calculations may be used for:

* previews
* UI feedback
* temporary display

but backend values remain authoritative.

---

# 26. LOCALSTORAGE RULE

Do not store authoritative business configuration in localStorage.

Examples:

* VAT
* service fee
* payment state
* branch permissions
* employee permissions
* authoritative prices

LocalStorage can be used for:

* temporary UI preferences
* non-critical client state
* cached display preferences

Persistent business settings belong in the backend database.

---

# 27. OWNER SETTINGS

Owner Settings currently covers:

* Profile
* Branches
* Employees
* Payroll
* Print Templates
* Receipt Settings
* System Settings

Before modifying Settings, identify whether the setting belongs to:

* User
* Branch
* Global system
* Order snapshot

Do not store branch settings on the user object.

Do not store persistent business settings only in browser state.

---

# 28. RECEIPT SETTINGS

Receipt settings are branch-specific.

Always scope receipt configuration to the appropriate branch.

Do not allow a manager from Branch A to modify Branch B's receipt configuration.

---

# 29. PAYROLL

Payroll is financially sensitive.

Before changing payroll logic:

Inspect:

* Employee
* PayrollSettings
* attendance data
* salary profiles
* payroll locking
* existing calculation rules

Never change salary formulas without explicitly confirming the business requirement.

Once payroll is locked, do not silently recalculate historical payroll.

---

# 30. PROFILE / BRANCH DATA

Store profile/business information in the Branch database record where appropriate.

Do not reintroduce previously migrated business fields into localStorage.

Branch information should remain synchronized through the API.

---

# 31. SECURITY

Never commit:

* API keys
* JWT secrets
* database passwords
* PFX passwords
* Gemini keys
* Supabase secrets
* access tokens

Never print secrets into logs.

Never expose secrets in:

* frontend bundles
* DTOs
* API responses
* screenshots
* documentation

Environment configuration should contain names/placeholders only in documentation.

---

# 32. HTTPS

HTTPS is required for development/demo.

Backend:

```text
HTTPS
port 5000
```

Do not disable HTTPS just to fix a development error.

Do not replace HTTPS with HTTP as a permanent solution.

Do not weaken certificate validation as a workaround unless explicitly requested and clearly marked development-only.

---

# 33. LAN CONFIGURATION

The current development LAN IP may change.

Never introduce new machine-specific hardcoded IP addresses.

Existing configuration may contain the current development IP, but new code should prefer:

* environment variables
* configuration
* runtime detection
* configurable API base URL

If changing LAN configuration, update all affected consumers consistently.

---

# 34. CORS

CORS must remain restrictive enough for the intended development environment.

Do not use:

```csharp
AllowAnyOrigin()
```

together with credentials as a shortcut.

When changing CORS:

Inspect:

* Admin Web
* Customer Web
* LAN access
* HTTPS
* SignalR

---

# 35. SIGNALR SECURITY

SignalR connections must respect authentication and branch/role visibility.

Do not expose kitchen events to unauthorized users.

Do not broadcast sensitive order information globally when branch-specific groups are available.

---

# 36. ERROR HANDLING

Do not hide exceptions with empty catch blocks.

Bad:

```csharp
catch
{
}
```

Prefer:

* meaningful error handling
* structured logging
* appropriate HTTP status codes
* safe user-facing messages

Do not expose stack traces or sensitive database details to clients.

---

# 37. LOGGING

Logs should help diagnose:

* authentication failures
* database failures
* SignalR failures
* payment failures
* AI tool failures
* unexpected business-rule violations

Never log:

* passwords
* JWT secrets
* API keys
* full access tokens
* sensitive credentials

---

# 38. C# STYLE

Prefer:

* clear naming
* small methods
* guard clauses
* async/await
* dependency injection
* explicit business logic
* nullable reference types where appropriate

Avoid:

* giant methods
* deeply nested conditionals
* magic numbers
* duplicated calculations
* unnecessary abstractions

Do not introduce abstractions merely to make code "look clean."

---

# 39. MONEY

All monetary values must use:

```csharp
decimal
```

Never use:

```csharp
double
float
```

for:

* price
* salary
* subtotal
* tax
* fee
* payment
* discount

Avoid converting money through floating-point values.

---

# 40. NULLABILITY

Handle nullable database fields explicitly.

Do not replace nullable historical fields with arbitrary defaults merely to simplify frontend code.

Example:

A historical order with:

```text
VatAmount = null
```

does NOT automatically mean:

```text
VatAmount = 0
```

unless the business rule explicitly defines that behavior.

---

# 41. ENUMS AND STATUS

Before adding or changing status values:

Search all usages.

Check:

* backend
* frontend
* database
* SignalR
* filters
* reports
* AI tools

Never change enum numeric values casually.

Prefer additive changes when possible.

---

# 42. FRONTEND TYPESCRIPT

Avoid:

```typescript
any
```

unless there is a documented reason.

Prefer:

* interfaces
* typed API responses
* typed request DTOs
* discriminated unions where useful

If the backend DTO changes, update frontend types.

---

# 43. REACT

Avoid unnecessary global state.

Prefer local state for:

* forms
* temporary UI state
* dialogs

Use existing project patterns before introducing another state-management library.

Do not add dependencies without checking whether the repository already solves the problem.

---

# 44. API CLIENTS

Before creating an API helper:

Search for existing:

* api.ts
* axios configuration
* fetch wrapper
* service functions
* authentication interceptors

Reuse the existing API infrastructure.

Do not create multiple competing API clients.

---

# 45. UI CHANGES

When changing UI:

Preserve:

* existing navigation
* role guards
* responsive behavior
* accessibility
* existing design system
* loading states
* error states

Do not redesign unrelated screens.

---

# 46. CAMERA / QR

Customer QR ordering may depend on browser camera permissions.

Do not weaken HTTPS requirements to solve camera access.

When testing camera functionality, verify:

* HTTPS
* browser permissions
* LAN accessibility
* correct frontend API URL
* mobile browser behavior

---

# 47. AI ASSISTANT ARCHITECTURE

AI architecture:

```text
User
 ↓
AI Orchestrator
 ↓
Tool Registry
 ↓
AiPermissionService
 ↓
Application Service
 ↓
Database / external service
```

AI should not contain duplicated business rules that already belong in application services.

For example:

Do NOT implement a second order-total calculation inside the AI assistant.

Instead:

```text
AI
→ Order tool
→ OrderService
```

---

# 48. AI TOOL DESIGN

Every tool should be:

* narrowly scoped
* deterministic where possible
* permission-aware
* validated
* auditable where appropriate
* resistant to malformed AI input

Validate all AI-generated arguments.

Never assume the LLM produced valid IDs, quantities, prices, roles, or branch IDs.

---

# 49. AI WRITE OPERATIONS

For destructive or financially sensitive operations:

Prefer a confirmation workflow.

Examples:

* deleting employee
* changing payroll
* changing branch settings
* modifying orders
* changing prices
* issuing refunds
* deleting data

The AI should not perform high-impact mutations merely because the user wording is ambiguous.

---

# 50. AI HALLUCINATION DEFENSE

Never allow the AI to invent:

* product IDs
* order IDs
* employee IDs
* prices
* branch IDs
* payment status
* database records

The AI must retrieve authoritative data before acting.

If data cannot be verified:

Return an explicit inability to verify.

Do not guess.

---

# 51. TESTING REQUIREMENT

After backend changes:

Run at minimum:

```text
dotnet build
```

For frontend changes:

Run at minimum:

```text
npm run build
```

When available, run relevant tests.

For business-critical changes, manually verify the affected flow.

---

# 52. REQUIRED TEST MATRIX FOR ORDER CHANGES

When modifying order financial logic, test at least:

1. No VAT + no service fee
2. VAT only
3. Service fee only
4. VAT + service fee
5. Zero quantity / invalid quantity
6. Multiple items
7. Toppings/options
8. Order update
9. Payment
10. Existing order with null snapshots
11. Different branch settings
12. Client sends manipulated totalAmount

Expected result:

The backend remains authoritative.

---

# 53. REQUIRED TEST MATRIX FOR BRANCH CHANGES

When modifying branch-sensitive logic, test:

1. Admin accessing own branch
2. Admin accessing another branch
3. Manager accessing own branch
4. Manager attempting another branch
5. Global setting
6. Branch-specific override
7. Missing setting
8. New branch
9. Existing branch

---

# 54. MIGRATION SAFETY

Before creating a migration:

Check:

```text
git status
```

Inspect existing migrations.

Check whether the model change is already represented by an existing migration.

Do not generate duplicate migrations blindly.

After generating a migration:

Review:

* columns
* nullability
* defaults
* indexes
* foreign keys
* destructive operations

If the migration unexpectedly drops or recreates important data structures, STOP and investigate.

---

# 55. GIT SAFETY

Never run destructive Git commands without explicit instruction.

Avoid:

```text
git reset --hard
git clean -fd
git checkout -- .
```

Do not discard unrelated user changes.

Before modifying a heavily changed file:

Inspect:

```text
git status
```

Preserve existing uncommitted work.

---

# 56. USER CHANGES ARE SACRED

The working tree may contain unfinished but intentional work.

Never assume uncommitted changes are mistakes.

If unrelated changes exist:

* preserve them
* do not revert them
* do not overwrite them
* do not "clean up" them

Only modify files necessary for the requested task.

---

# 57. WHEN A BUG IS FOUND

Do not immediately patch the visible error.

Trace:

```text
UI
→ API
→ Controller
→ Service
→ Entity
→ EF Core
→ PostgreSQL
```

Find the actual source of the bug.

Fix the root cause where possible.

Avoid compensating hacks.

---

# 58. API ERROR DEBUGGING

When an API fails:

Check in this order:

1. Browser Network tab
2. HTTP status
3. Request payload
4. Response body
5. Controller
6. Service
7. EF Core query
8. Database schema
9. Migration state
10. Authentication/authorization
11. CORS/HTTPS
12. SignalR if real-time functionality is involved

Do not assume a frontend error means the frontend is the root cause.

---

# 59. DATABASE ERROR DEBUGGING

For PostgreSQL/EF Core errors:

Check:

* entity definition
* DbContext
* migration history
* actual database schema
* column names
* nullable constraints
* foreign keys
* indexes
* connection string
* PostgreSQL version/provider compatibility

Never solve a schema mismatch by manually adding columns.

Use EF Core migrations.

---

# 60. "NO ROUTE TO HOST" / NETWORK ERRORS

If the application reports:

```text
No route to host
```

or similar network errors:

Do not immediately assume:

* Gemini token expired
* API quota exhausted
* database broken

First verify:

* target host
* target port
* backend process
* LAN IP
* firewall
* HTTPS certificate
* frontend API URL
* device network
* SignalR URL

Network failures and API quota failures are different categories.

---

# 61. GEMINI / AI API

Never commit Gemini API keys.

AI model configuration must come from environment/configuration (e.g., `Gemini:Model` in `appsettings.json`).

When changing Gemini integration:

Check:

* current configuration (e.g., `gemini-3.7-flash` is the current default)
* API endpoint
* authentication
* quota
* retry behavior
* error handling

Do not assume a specific model version is permanently available without checking settings.

---

# 62. EXTERNAL SERVICES

For external APIs:

* validate responses
* handle timeouts
* handle unavailable service
* do not trust external data blindly
* do not block core POS operations unnecessarily

External AI/API failures should degrade gracefully where possible.

---

# 63. PERFORMANCE

Do not optimize prematurely.

But avoid obvious problems:

* N+1 EF Core queries
* loading entire tables unnecessarily
* repeated database calls inside loops
* unnecessary React re-renders
* duplicated API requests

Before adding caching, verify correctness first.

Caching must never cause branch/security/data-integrity violations.

---

# 64. ASYNC

Use asynchronous APIs for database and network operations.

Prefer:

```csharp
await ...
```

Do not introduce:

```csharp
.Result
.Wait()
```

inside ASP.NET request paths.

Avoid blocking threads unnecessarily.

---

# 65. TRANSACTIONS

Use database transactions when multiple related financial/state changes must succeed or fail together.

Examples may include:

* payment
* order mutation
* inventory mutation
* payroll locking

Do not introduce transactions everywhere without need.

---

# 66. INVENTORY

If inventory logic is modified:

Inspect all relationships between:

* Product
* Ingredient
* ProductIngredient
* OrderItem
* Order

Never deduct inventory merely because an order exists unless that is the established business rule.

Verify cancellation/refund behavior.

---

# 67. RECEIPT / INVOICE

Receipt/invoice data must represent persisted order state.

Do not generate receipt totals independently from `OrderService`.

Avoid discrepancies between:

* POS display
* payment
* invoice
* receipt

All must ultimately agree with backend persisted financial values.

---

# 68. REPORTING

Reports should use authoritative persisted data.

Do not calculate historical VAT/service fee using today's settings.

Historical reports must respect order snapshots where available.

---

# 69. DOCUMENTATION

When implementing a non-obvious business rule:

Add a concise comment explaining WHY.

Do not add comments that merely restate the code.

Bad:

```csharp
// Calculate total
total = subtotal + fee;
```

Good:

```csharp
// VAT is calculated after service fee according to the restaurant's configured pricing rule.
```

---

# 70. COMMENT POLICY

Avoid excessive comments.

Prefer readable code.

Comments should explain:

* business rules
* compatibility decisions
* security decisions
* non-obvious workarounds

---

# 71. DEPENDENCY POLICY

Before adding a package:

1. Check whether an existing dependency solves the problem.
2. Check whether the package is already installed.
3. Consider maintenance/security implications.
4. Avoid dependencies for trivial functionality.

Do not add packages just because they are convenient.

---

# 72. FILE ORGANIZATION

Follow the existing project structure.

Do not move files unless required.

Expected structure:

```text
apps/
  admin-web/
  customer-web/

services/
  api/

scripts/
```

Backend layers:

```text
Domain/
Application/
Infrastructure/
WebAPI/
```

---

# 73. NO DUPLICATE BUSINESS LOGIC

If the same rule appears in multiple places, identify the authoritative owner.

Examples:

Price calculation:
→ OrderService

Authorization:
→ Backend authorization / AiPermissionService

Database schema:
→ EF Core model + migrations

Branch isolation:
→ Backend

Receipt configuration:
→ ReceiptSettings API/service

Do not duplicate these rules in frontend and AI code.

---

# 74. WHEN REQUIREMENTS ARE AMBIGUOUS

Do not invent business rules.

If the implementation can safely proceed:

* use the smallest reasonable interpretation
* preserve existing behavior

If ambiguity affects:

* money
* security
* permissions
* data deletion
* database schema
* order state

STOP and ask for clarification.

---

# 75. BEFORE MODIFYING ORDER SERVICE

Always inspect at least:

```text
Order.cs
OrderItem.cs
OrderService.cs
OrderController.cs
KitchenHub.cs
KitchenService.cs
SystemSettingService.cs
SystemSettingsController.cs
relevant DTOs
relevant frontend API consumers
recent migrations
```

---

# 76. BEFORE MODIFYING SYSTEM SETTINGS

Inspect:

```text
SystemSetting entity
ApplicationDbContext
SystemSettingService
SystemSettingsController
SystemSettings.tsx
migration
OrderService
```

Determine whether the setting affects:

* display only
* POS behavior
* financial calculation
* branch behavior
* AI permissions

---

# 77. BEFORE MODIFYING AUTHORIZATION

Inspect:

```text
Program.cs
JWT configuration
Controller attributes
AiPermissionService
frontend route guards
role/position claims
```

Never fix an authorization problem by simply removing `[Authorize]`.

---

# 78. BEFORE MODIFYING SIGNALR

Inspect:

```text
KitchenHub
OrderService
KitchenService
SignalR event names
frontend SignalR client
authentication
branch/group membership
```

Verify both producer and consumer.

---

# 79. BEFORE MODIFYING DATABASE

Inspect:

```text
Entity
DbContext
Entity configuration
Migrations
Services
DTOs
API
Frontend
```

Then create an EF Core migration.

---

# 80. COMPLETION CRITERIA

A task is NOT complete merely because the code compiles.

A task is complete when:

* requested behavior works
* architecture is preserved
* authorization is preserved
* branch isolation is preserved
* financial rules are preserved
* database schema is synchronized
* frontend/backend contracts are synchronized
* affected real-time flows still work
* build passes
* relevant tests/manual verification pass
* no secrets were introduced
* no unrelated changes were made

---

# 81. FINAL RESPONSE FORMAT FOR AI AGENTS

After implementation, report:

## Changed

* files changed
* important logic changes

## Database

* migration created/applied
* schema changes

## Verification

* backend build
* frontend build
* tests
* manual checks

## Risks

* remaining known issues
* limitations

## Not Changed

Mention important areas deliberately left untouched.

Never claim a test passed if it was not actually run.

Never claim a migration was applied if it was not actually applied.

Never claim a feature works without verification.

---

# 82. MOST IMPORTANT RULES

When in doubt, follow these priorities:

```text
1. Security
2. Data integrity
3. Financial correctness
4. Authorization / branch isolation
5. Existing business behavior
6. Architecture
7. Backward compatibility
8. Smallest safe change
9. Performance
10. Cosmetic improvements
```

---

# 83. KNOWN RISKS / TECHNICAL DEBT

The following issues are identified in the current baseline. They must not be ignored when planning features, but should only be fixed through explicit tasks.

* **Hardcoded CORS**: `Program.cs` contains a hardcoded LAN IP (`192.168.11.172`). Future changes should move this to configuration.
* **Security**: Employee and Customer passwords are currently stored in clear-text.
* **AI Validation**: There is no centralized schema validator for AI Tool inputs yet. Every tool must perform its own rigorous input validation.
* **Branch Isolation in HRM**: The `EmployeeController` is currently `admin`-only.
* **SystemSettings**: Global fallback (`BranchId IS NULL`) is not yet implemented in `SystemSettingService`.
* **Testing**: Automated frontend tests are currently missing.
# 91. WORKSPACE SAFETY

The repository may contain intentional uncommitted changes.

Before modifying files:

1. Inspect `git status`.
2. Identify files directly related to the task.
3. Do not modify unrelated files.
4. Do not overwrite existing user changes.
5. Do not revert changes merely because they are incomplete.

The agent must prefer:

```text
inspect
→ diagnose
→ minimal change
→ verify
```

over:

```text
rewrite
→ clean
→ regenerate
→ hope it works
```

---

# 92. NO UNAUTHORIZED ENVIRONMENT CHANGES

Do not automatically change the development environment.

Do NOT change without explicit justification:

* .NET SDK version
* Node.js version
* npm/pnpm version
* package versions
* NuGet packages
* Docker configuration
* PostgreSQL configuration
* Supabase configuration
* Gemini model configuration
* environment variables
* HTTPS certificates
* firewall configuration
* Windows network configuration

If an environment change appears necessary:

1. Explain why.
2. Identify the exact change.
3. Prefer a project-local/configurable solution.
4. Do not make unrelated environment changes.

---

# 93. NO DESTRUCTIVE DEBUGGING

Never use destructive commands as a first debugging step.

Do NOT automatically run:

```text
git reset --hard
git clean -fd
dotnet clean
rm -rf
Delete database
Drop table
Drop schema
Remove migrations
Delete node_modules
Delete package-lock.json
```

unless the user explicitly requests it or the command is clearly required and its impact has been explained.

When debugging:

```text
observe
→ inspect
→ reproduce
→ identify root cause
→ apply minimal fix
```

---

# 94. NO BLIND REGENERATION

Do not regenerate:

* migrations
* API clients
* DTOs
* OpenAPI clients
* lock files
* configuration files
* generated code

merely because something is inconsistent.

First determine why the inconsistency exists.

Never solve:

```text
schema mismatch
```

by blindly generating another migration.

Never solve:

```text
build error
```

by blindly reinstalling all dependencies.

---

# 95. NO AUTOMATIC DEPENDENCY UPDATES

Do not update dependencies unless the task explicitly requires it.

Before adding or updating a package:

1. Search the repository for an existing solution.
2. Check whether the package already exists.
3. Explain why the dependency is necessary.
4. Prefer the smallest dependency change.

Do not perform broad dependency upgrades to solve an unrelated bug.

---

# 96. CONFIGURATION SAFETY

Configuration values may differ between:

* local development
* LAN development
* demo
* production

Do not hardcode machine-specific values.

Prefer:

```text
environment variables
→ appsettings configuration
→ runtime configuration
```

over:

```text
hardcoded IP
hardcoded credentials
hardcoded port
hardcoded API key
```

Never expose secrets while diagnosing configuration issues.

---

# 97. DEBUGGING EVIDENCE

Never conclude the root cause of an error without evidence.

For example:

Do NOT immediately conclude:

```text
Gemini quota exhausted
```

when the error is:

```text
No route to host
```

Do NOT immediately conclude:

```text
Frontend bug
```

when an API returns:

```text
500 Internal Server Error
```

Do NOT immediately conclude:

```text
Database is broken
```

when EF Core reports a schema error.

Instead:

```text
Error
→ classify error
→ inspect evidence
→ trace dependency
→ identify root cause
→ fix root cause
```

When reporting a diagnosis, distinguish:

```text
Confirmed
Likely
Unknown
```

Never present speculation as fact.

---

# 98. COMMAND SAFETY

Before running a command that can modify project state, understand its effect.

Commands that require extra caution include:

```text
dotnet ef database update
dotnet ef migrations remove
dotnet ef migrations add
npm install
npm update
git commands
docker commands
database commands
```

Prefer read-only inspection commands first.

For example:

```text
git status
git diff
dotnet ef migrations list
dotnet build
npm run build
```

before destructive or state-changing commands.

---

# 99. PROCESS SAFETY

Do not automatically kill running processes.

Do not use:

```text
taskkill /F
kill -9
```

merely because a port is occupied.

First determine:

1. Which process owns the port.
2. Whether it is the expected backend/frontend process.
3. Whether another application depends on it.
4. Whether restarting is actually necessary.

If restart is necessary, prefer the least disruptive approach.

---

# 100. DATABASE SAFETY

Never delete, reset, truncate, or recreate the development database as a generic troubleshooting step.

Before any database-destructive operation:

* identify affected data
* verify whether data is disposable
* explain consequences
* obtain explicit user approval

Existing restaurant data must be treated as valuable.

Prefer:

```text
inspect schema
→ inspect migrations
→ identify mismatch
→ create safe migration if required
```

instead of:

```text
drop database
→ recreate
```

---

# 101. STOP CONDITIONS

The agent must stop and ask for clarification when a change could materially affect:

* financial calculations
* payment
* refunds
* payroll
* authorization
* branch isolation
* database destruction
* historical financial data
* destructive bulk operations
* security configuration

The agent may continue automatically for low-risk implementation details that do not change business rules.

---

# 102. FILE SCOPE DECLARATION

Before making a substantial change, identify:

```text
Task:
<requested feature>

Files expected to change:
<list>

Files inspected but intentionally unchanged:
<list>

Reason:
<short explanation>
```

If implementation later requires additional files:

Explain why before expanding scope.

Do not silently expand the task.

---

# 103. VERIFICATION EVIDENCE

Verification must distinguish between:

```text
Built
Tested
Manually verified
Inspected
Not verified
```

For example:

```text
Backend:
✓ dotnet build
✓ API endpoint manually tested
✗ automated tests unavailable
```

Do not convert:

```text
code looks correct
```

into:

```text
feature verified
```

---

# 104. NO FALSE COMPLETION

Never say:

```text
Done
Feature works
Migration applied
API fixed
Payment works
AI tool works
```

unless there is evidence supporting the statement.

Use precise language:

```text
Implemented but not runtime-tested.
```

or:

```text
Build passes; runtime flow still needs verification.
```

or:

```text
The root cause appears to be X, but this has not yet been confirmed.
```

---

# 105. PRESERVE USER WORKFLOW

Do not force a new workflow when the existing project already has one.

Before introducing:

* new API client
* new service
* new state manager
* new authentication flow
* new migration strategy
* new AI orchestration pattern
* new UI pattern

inspect the existing implementation first.

Prefer integration over replacement.

---

# 106. AGENT BEHAVIOR PRINCIPLE

The coding agent must behave like a careful senior engineer working in an existing codebase.

It should:

```text
Inspect before editing.
Understand before refactoring.
Verify before concluding.
Reuse before duplicating.
Validate before mutating.
Preserve before replacing.
```

The agent must optimize for:

```text
correctness > convenience
evidence > assumption
minimal change > rewrite
existing architecture > new architecture
data safety > debugging speed
```

The AI must prefer a smaller correct change over a larger "cleaner" rewrite.

Never sacrifice correctness for speed.

Never sacrifice security for convenience.

Never sacrifice historical financial accuracy for simplified code.
