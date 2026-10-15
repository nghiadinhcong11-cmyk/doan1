# AUTHORIZATION GAP FIX REPORT

**Date**: September 19, 2026  
**Project**: RestaurantPOS (Hệ thống quản lý nhà hàng đa chi nhánh tích hợp Trợ lý ảo AI Agent)  
**Role**: Senior .NET Backend Developer / Software Architect / Security Reviewer  

---

## 1. Scope
This task specifically fixes the 2 backend authorization GAPs identified in the Expected Use Case Baseline:
- **UC7.2 — Cập nhật trạng thái tạm hết món (`OutOfStock`)**: Adding `manager` role to `UpdateAvailability` in `ProductController.cs`.
- **UC9.3 — Quản lý Chi phí vận hành**: Restricting `ExpenseController.cs` class-level attribute to `admin,manager` (removing `employee,cashier`).

No other controllers, services, database models, migrations, or operational exceptions were modified.

---

## 2. UC7.2 Before
- **File**: `services/api/src/WebAPI/Controllers/ProductController.cs`
- **Method**: `UpdateAvailability(Guid id, [FromBody] AvailabilityRequest request)`
- **Authorization Attribute**:
  ```csharp
  [Authorize(Roles = "admin,kitchen")]
  ```
- **Issue**: Role `manager` received `403 Forbidden` when attempting to mark items temporarily out of stock directly.

---

## 3. UC7.2 After
- **File**: `services/api/src/WebAPI/Controllers/ProductController.cs`
- **Method**: `UpdateAvailability(Guid id, [FromBody] AvailabilityRequest request)`
- **Updated Attribute**:
  ```csharp
  [Authorize(Roles = "admin,manager,kitchen")]
  ```

---

## 4. UC7.2 Verification

| Actor | Expected | Actual After Fix | Evidence / Enforcement Mechanism |
| :--- | :---: | :---: | :--- |
| **Admin** | ALLOW | **ALLOW** | `[Authorize(Roles = "admin,manager,kitchen")]` |
| **Manager** | ALLOW | **ALLOW** | `[Authorize(Roles = "admin,manager,kitchen")]` |
| **Cashier** | DENY | **DENY** | Rejected by ASP.NET Core Authorization Middleware (Role `cashier` not in allowed list) |
| **Employee** | DENY | **DENY** | Rejected by ASP.NET Core Authorization Middleware (Role `employee` not in allowed list) |
| **Kitchen** | ALLOW | **ALLOW** | `[Authorize(Roles = "admin,manager,kitchen")]` |
| **Customer** | DENY | **DENY** | Rejected by ASP.NET Core Authorization Middleware |
| **Guest** | DENY | **DENY** | Rejected by ASP.NET Core Authorization Middleware (Unauthenticated) |

---

## 5. UC9.3 Before
- **File**: `services/api/src/WebAPI/Controllers/ExpenseController.cs`
- **Class-level Attribute**:
  ```csharp
  [Authorize(Roles = "admin,manager,employee,cashier")]
  ```
- **Issue**: Allowed Cashiers and Employees to create/read/update operating expense records.

---

## 6. UC9.3 After
- **File**: `services/api/src/WebAPI/Controllers/ExpenseController.cs`
- **Updated Attribute**:
  ```csharp
  [Authorize(Roles = "admin,manager")]
  ```

---

## 7. UC9.3 Verification

| Actor | Expected | Actual After Fix | Evidence / Enforcement Mechanism |
| :--- | :---: | :---: | :--- |
| **Admin** | ALLOW | **ALLOW** | `[Authorize(Roles = "admin,manager")]` |
| **Manager** | ALLOW | **ALLOW** | `[Authorize(Roles = "admin,manager")]` |
| **Cashier** | DENY | **DENY** | Rejected by ASP.NET Core Authorization Middleware (Role `cashier` not in allowed list) |
| **Employee** | DENY | **DENY** | Rejected by ASP.NET Core Authorization Middleware (Role `employee` not in allowed list) |
| **Kitchen** | DENY | **DENY** | Rejected by ASP.NET Core Authorization Middleware (Role `kitchen` not in allowed list) |
| **Customer** | DENY | **DENY** | Rejected by ASP.NET Core Authorization Middleware |
| **Guest** | DENY | **DENY** | Rejected by ASP.NET Core Authorization Middleware (Unauthenticated) |

---

## 8. Branch Isolation
- **Admin**: Operates on Global Scope (`IsAdmin()` bypasses `branchId` filters).
- **Manager**: Operates strictly on Branch Scope (`TryResolveBranch` and `HasBranchAccess` read `branchId` claim from JWT, preventing cross-branch access).

---

## 9. Build Result
- **Status**: **PASS**
- **Command**: `dotnet test tests/RestaurantPOS.Tests/RestaurantPOS.Tests.csproj -o tests/RestaurantPOS.Tests/.gap-fix-output`
- **Build Output**: `RestaurantPOS.api succeeded`, zero errors.

---

## 10. Test Result
- **Status**: **PASS**
- **Summary**: Total tests: 173, Passed: 173, Failed: 0, Skipped: 0.
- **Key Tests Executed**:
  - `AuthorizationHardeningTests.Protected_endpoints_declare_expected_role_requirements` (Verified `admin,manager,kitchen` on `UpdateAvailability` and `admin,manager` on `ExpenseController`).
  - `ExpenseControllerTests.Manager_can_create_expense_only_for_own_branch` (Verified Manager expense creation within own branch).
  - `ExpenseControllerTests.Manager_cannot_create_or_read_expense_from_another_branch` (Verified Manager branch isolation).

---

## 11. Frontend Verification
- `Navbar.tsx` (Admin/Manager top navbar) contains the link to `/expenses`.
- `CashierNavbar.tsx` and `KitchenNavbar.tsx` do NOT expose `/expenses` link.
- Frontend role guards match backend authorization boundaries.

---

## 12. Files Changed
- `services/api/src/WebAPI/Controllers/ProductController.cs`
- `services/api/src/WebAPI/Controllers/ExpenseController.cs`
- `tests/RestaurantPOS.Tests/ExpenseControllerTests.cs`
- `tests/RestaurantPOS.Tests/AuthorizationHardeningTests.cs`

---

## 13. Files Not Changed
- `ShiftController.cs` (UC8.2 operational exception retained)
- `ReceiptSettingsController.cs` (UC6.3 Manager branch-level configuration retained)
- `SystemSettingsController.cs` (UC6.4 Manager branch-level configuration retained)
- `AiController.cs`, `AiOrchestrator.cs`, `AiPermissionService.cs` (AI Agent intact)
- EF Core Entities, AppDbContext, Migrations (Database schema intact)

---

## 14. Remaining Known Issues (Out of Scope / Operational Exceptions)
- **UC8.2 (Mở & Chốt ca)**: `ShiftController.cs` allows `admin,manager,cashier` to allow Manager/Admin backup during Cashier absence.
- **UC6.3 / UC6.4 (Cấu hình Chi nhánh)**: `ReceiptSettingsController.cs` and `SystemSettingsController.cs` allow `manager` to configure settings for their assigned branch.
