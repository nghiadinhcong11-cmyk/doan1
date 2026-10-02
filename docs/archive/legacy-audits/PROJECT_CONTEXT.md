# Project Context: Restaurant POS & Management System

**Last Updated**: 2026-09-13
**Source**: Verified against actual source code
**Status**: Production System (Not a prototype)

---

## 1. PROJECT OVERVIEW

**Name**: Restaurant POS System
**Language**: Vietnamese (Hệ thống quản lý bán hàng nhà hàng)
**Scope**: Multi-branch restaurant management with POS, KDS, employee management, payroll, and AI assistant
**Architecture**: Clean Architecture with separate AI module

### Core Features (Implemented)

- Multi-branch POS: Order management with financial integrity
- Kitchen Display System (KDS): Real-time kitchen coordination via SignalR
- Employee & HRM: Staff management, roles, scheduling, attendance
- Payroll: Monthly salary calculation with adjustments and overtime
- Customer/Loyalty: Loyalty points tracking and transaction history
- Reservations & Tables: Table management and reservations
- Expenses: Branch expense tracking and financial analytics
- Receipts: Configurable receipt settings per branch
- AI Assistant: Gemini-powered with tool registry and permission service
- Financial Analytics: Dashboard, business insights, revenue analysis

---

## 2. TECHNOLOGY STACK

### Backend
- Framework: ASP.NET Core 8
- Language: C#
- ORM: Entity Framework Core 8.0.0
- Database: PostgreSQL (via Npgsql 8.0.0, Supabase)
- Authentication: JWT Bearer (custom implementation)
- Real-time: SignalR
- API Documentation: Swagger/Swashbuckle 6.4.0

### Frontend
- Apps: React (2 separate apps)
- Bundler: Vite
- Language: TypeScript
- Styling: TailwindCSS
- Charts: Recharts 3.9.2
- Icons: Lucide React 1.25.0

### AI
- Model: Google Gemini (API-based)
- Integration: Custom orchestrator with tool registry

---

## 3. BACKEND ARCHITECTURE

Backend follows Clean Architecture with strict dependency flow:

- Domain Layer: Entities only (Product, Order, Employee, Payroll, Customer, etc.)
- Application Layer: Services, DTOs, Interfaces (OrderService, PayrollService, etc.)
- Infrastructure Layer: EF Core DbContext, Persistence, Supabase integration
- WebAPI Layer: Controllers, SignalR Hubs, Middleware, Program.cs
- AI Layer: Separate module with Orchestrator, Tools, Permission Service

AI Module Structure:
AI does NOT access DbContext directly. Instead:
- AiOrchestrator: Manages Gemini conversation and tool calling
- AiPermissionService: Authorization gate before tool execution
- AiToolRegistry: Central registry of available tools
- IAiTool implementations: Encapsulated task units that call Application Services
- Tools: Organized by role (Admin, Employee, Customer)

---

## 4. DOMAIN ENTITIES & FINANCIAL RULES

### Order Entity (CRITICAL)

**File**: src/Domain/Entities/Order.cs

Financial fields (calculated by OrderService, never trusted from client):
- SubTotal: Sum of all OrderDetail.Quantity * UnitPrice
- ServiceFeePercent, ServiceFeeAmount: Based on SystemSetting, calculated as SubTotal * rate / 100
- VatPercent, VatAmount: Based on SystemSetting, calculated on (SubTotal + ServiceFee)
- Discount: Legacy field, client discounts ignored until promotion flow implemented
- TotalAmount: SubTotal + ServiceFeeAmount + VatAmount - Discount
- PaidAmount: What customer paid
- PaymentMethod: "Tiền mặt", "Thẻ", "Chuyển khoản"
- Status: "Đang xử lý" (Processing), "Hoàn thành" (Completed), "Đã hủy" (Cancelled)

OrderDetail (child):
- ProductId, ToppingId, Quantity, UnitPrice
- SentQuantity: Tracks what was already sent to kitchen (cannot be reduced)
- Options: Comma-separated topping/size selections
- UnitPrice: Calculated from Product.Price + size premium + topping prices

### Financial Calculation (OrderService.RecalculateOrderFinancials)

**Source File**: src/Application/Services/OrderService.cs

Sequence:
1. For each OrderDetail, calculate UnitPrice:
   - Start with Product.Price
   - Add size premium (parsed from Product.SizesJson)
   - Add topping prices (parsed from Product.ToppingsJson)
   - SubTotal += Quantity * UnitPrice
2. ServiceFeeAmount = SubTotal * ServiceFeePercent / 100
3. VAT base = SubTotal + ServiceFeeAmount
4. VatAmount = VAT base * VatPercent / 100
5. TotalAmount = SubTotal + ServiceFeeAmount + VatAmount - Discount

Rounding: MidpointRounding.AwayFromZero (rounds .5 away from zero)

Financial Safety Rules:
- All prices sourced from DB, never from client
- New orders: Status="Đang xử lý", PaidAmount=0, PaymentMethod=null, Discount=0
- Client cannot override Status, PaidAmount, PaymentMethod, Discount
- Cannot reduce SentQuantity (already in kitchen)
- Order merge: Updates existing "Đang xử lý" order instead of creating new

---

## 5. EMPLOYEE & AUTHORIZATION

### Employee Entity

**File**: src/Domain/Entities/Employee.cs

Role field (authorization source):
- "admin": System-wide access, all branches
- "cashier": POS payment & order (own branch only)
- "kitchen": KDS display (own branch only)
- "employee": Service & scheduling (own branch only)

Position field: Display only, NOT used for authorization

NOT IMPLEMENTED: "manager" role (defended in KitchenHub but no Employee.Role support)

### JWT Token Claims

**File**: src/WebAPI/Controllers/AuthController.cs

Claims included:
- NameIdentifier: userId (Guid)
- Name: username (employees) or phone (customers)
- Role: authorization role
- fullName, branchId, branchName, position: Custom claims

Token validation: Signature, expiration, issuer/audience

---

## 6. AUTHORIZATION & BRANCH ISOLATION

### Implementation

Roles with JWT branchId claim + TryResolveBranch() controller validation:

- Admin: No branch restriction
- Cashier/Kitchen/Employee: Own branch only (JWT branchId must match request)
- Customer: Own phone identity only

Branch isolation enforced at:
- Controller: TryResolveBranch() verifies user branch matches request
- Service: OrderService filters queries by BranchId
- SignalR: KitchenHub groups by branch

### Issue: Manager Role Declared But Not Implemented

KitchenHub.cs includes "manager" in Authorize attribute:
[Authorize(Roles = "admin,manager,employee,cashier,kitchen")]

But Employee.Role does NOT support "manager" value. Appears defensive for future implementation.

---

## 7. AI SYSTEM & TOOLS

### AiOrchestrator (Conversation Management)

**File**: src/AI/Services/AiOrchestrator.cs

Process:
1. Build system instruction (role-specific via PromptBuilder)
2. Get allowed tools from AiToolRegistry (filtered by user role)
3. Call Gemini with history + system instruction + tools
4. If Gemini requests function call:
   - Get tool from registry
   - Check permission via AiPermissionService
   - Execute tool (must call Application Service, NOT DbContext)
   - Collect result
   - Loop (max 5 iterations)
5. Return final text response

Constraints:
- Temperature 0.2 (low randomness)
- Max 1024 output tokens
- Sequential tool calling only
- NO direct DbContext access

---

### AiPermissionService (Authorization Gate)

**File**: src/AI/Services/AiPermissionService.cs

Checks before tool execution:
1. Is user's role in tool.AllowedRoles?
2. Can user execute this risk level?
   - Read: All roles allowed
   - Write: admin, employee, cashier, kitchen (not customer unless explicit)
   - Destructive: admin only

ISSUE: Branch isolation NOT checked here. Relies on tool implementations to filter by AiUserContext.BranchId.

---

### AI Tool Interface & Registry

**Interface**: src/AI/Tools/IAiTool.cs

Each tool implements:
- Name, Description
- AllowedRoles (array of role strings)
- RiskLevel (Read, Write, Destructive)
- GetSchema(): Gemini tool declaration
- ExecuteAsync(): Tool execution

**Registry**: src/AI/Tools/AiToolRegistry.cs

Responsibilities:
- Store all registered tools
- Lookup tool by name
- Filter tools by role
- Generate Gemini tool schemas

Tool Categories:
- Admin/ (7 tools): GetRevenue, GetBusinessSummary, GetFinancialAnalysis, UpdateProductPrice, etc.
- Employee/ (3 tools): GetMyShift, GetTableSummary, UpdateOrderStatus
- Customer/: (to be populated)

Tool Pattern (all tools follow):
1. Validate input parameters
2. Call Application Service (NEVER DbContext)
3. Return AiToolResult (success/error)

---

## 8. PERSISTENCE & DATABASE

### ApplicationDbContext

**File**: src/Infrastructure/Persistence/ApplicationDbContext.cs

27 DbSets covering all entities

Key relationships:
- Order → OrderDetails (1-many, cascade delete)
- OrderRequest → OrderRequestItems (1-many, cascade delete)
- Payroll → PayrollAdjustment (1-many, cascade delete)
- Employee → Attendance, WorkSchedule
- Branch → Tables, Areas, Employees
- Customer → LoyaltyTransaction

Performance indexes:
- Order: (BranchId, CreatedAt, Status)
- Payroll: (EmployeeId, BranchId, Month, Year) UNIQUE
- SystemSetting: (BranchId, Key) UNIQUE
- Customer: PhoneNumber UNIQUE
- BusinessInsight: (DeduplicationKey, BranchId, Status, CreatedAt)

Auto-migration on startup via Program.cs

---

## 9. CONTROLLERS & API ENDPOINTS

### AuthController

- POST /api/auth/login: Employee login
- POST /api/auth/customer-token: Customer token (phone-based)
- GET /api/auth/me: Current user info
- POST /api/auth/change-password: Password change

### Order Management

- GET /api/order: List with filters
- GET /api/order/{id}: Single order
- POST /api/order: Create/update
- PUT /api/order/{id}: Accept web order
- DELETE /api/order/{id}: Cancel order

### Other Controllers (22 total)

ProductController, ToppingController, EmployeeController, PayrollController, DashboardController, FinancialAnalysisController, BranchController, AreaController, TableController, ReservationController, SystemSettingsController, ReceiptSettingsController, etc.

### Real-time (KitchenHub - SignalR)

Groups:
- kitchen-admin (admin users)
- role:{role} (by role)
- kitchen-branch:{branchId} (by branch)
- branch:{branchId}:role:{role} (by branch + role)

---

## 10. FRONTEND APPLICATIONS

### Admin Web (apps/admin-web/)

Features by module:
- analytics: Dashboards, business insights
- auth: Login, role-based UI
- catalog: Product & topping management
- hrm: Employee, payroll, scheduling
- kitchen: KDS display (real-time SignalR)
- operations: Branch, area, table, reservation
- pos: Order creation, payment, loyalty
- settings: System configuration

### Customer Web (apps/customer-web/)

- QR-based table ordering
- Order tracking
- Loyalty points view

### Shared Package

packages/shared/types/: OpenAPI-generated TypeScript types
Codegen: npm run codegen (live API) or npm run codegen:file (local swagger.json)

---

## 11. KNOWN ISSUES & INCONSISTENCIES

### 1. AI Branch Isolation Not Enforced (MEDIUM RISK)

Issue: AiPermissionService.CanExecuteAsync() checks role and risk level but NOT branch isolation.

Impact: Non-admin read tools could return cross-branch data if tool doesn't filter by AiUserContext.BranchId.

Recommendation: Add _authorization.IsInAuthorizedBranch() call to CanExecuteAsync().

### 2. Manager Role Not Implemented (LOW RISK)

Finding:
- KitchenHub: [Authorize(Roles = "admin,manager,employee,cashier,kitchen")]
- Employee.Role does NOT include "manager"
- No controllers check for "manager"

Status: Defensive code for future implementation.

### 3. Discount Handling Incomplete (MEDIUM RISK)

Issue: Client-supplied discounts ignored (set to 0 on creation). Comments: "until server-side promotion flow".

Status: Promotions/discounts not yet implemented. Current system has no discount application path.

### 4. Missing Financial Test Coverage (HIGH RISK)

Finding: No unit tests found for OrderService financial calculations (VAT, service fee, rounding, merge).

Risk: Refactoring could break POS accuracy without catching errors.

Recommendation: Add comprehensive tests for all financial scenarios.

### 5. AI Tool Validators Not Centralized

Issue: Each tool calls AiToolValidator.ValidateRequired() individually. No central validation in AiOrchestrator.

Recommendation: Consider central validation before tool execution.

---

## 12. TECHNICAL RECOMMENDATIONS

### High Priority

1. AI Branch Isolation: Add branch check to AiPermissionService.CanExecuteAsync()
2. Financial Tests: Add unit tests for OrderService calculations
3. Clarify Manager Role: Either implement fully or remove defensive references

### Medium Priority

4. Promotion Flow: Implement server-side discount/promotion application
5. Centralize AI Validation: Move input validation to AiOrchestrator
6. Update Documentation: Clarify customer is NOT Employee role

### Low Priority

7. Rate Limiting Docs: Document which endpoints use which limiter
8. Structured Logging: Add logs for financial operations
9. AI Tool Matrix: Document role-to-tool authorization mapping

---

## 13. BUILD & VERIFICATION

### Backend Build

cd services/api
dotnet build
dotnet run

Requires: .https/pos-cert.pfx with password

Database: Auto-migrates on startup (db.Database.Migrate())

### Frontend Build

cd apps/admin-web
npm install
npm run dev

### Type Generation

npm run codegen: Generate from live API
npm run codegen:file: Generate from local swagger.json
npm run codegen:check: Verify types are current

---

## 14. EXTERNAL DEPENDENCIES

- Supabase PostgreSQL: Database (connection pooling port 6543)
- Google Gemini API: AI model (15 req/min rate limit per IP)
- HTTPS Certificate: .https/pos-cert.pfx (local dev)

---

## 15. PROJECT STATUS SUMMARY

Production Ready: YES (not a prototype)

Implemented & Verified:
- Multi-branch POS with financial integrity
- Order lifecycle (create → merge → payment → completion)
- JWT authentication (employees & customers)
- Branch isolation enforcement
- Role-based access control (5 roles)
- KDS real-time via SignalR
- Employee payroll system
- Loyalty point tracking
- AI assistant with tool registry
- Financial analytics & insights

Partially Implemented:
- AI branch isolation (needs enforcement in permission service)
- Discount/promotion flow (placeholders only, client discounts ignored)
- Manager role (defensive code, not in use)

Not Yet Implemented:
- Comprehensive AI tool suite (only admin/employee tools present)
- Advanced business rules (inventory, expiration dates, etc.)
- Audit trail / transaction logging

---

**Document Verified**: 2026-09-13 via direct source code inspection
**Accuracy Level**: High (verified against actual implementations)
**Last Source Code Check**: Program.cs, OrderService, AuthController, AiOrchestrator, AiPermissionService, OrderEntity, EmployeeEntity, ApplicationDbContext
