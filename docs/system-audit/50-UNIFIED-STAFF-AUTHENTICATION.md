# Task 47D — Unified Management and Staff Authentication

## Summary

Employee-system authentication now accepts two semantic login groups. The request selects only a group; the authenticated `Employee.Role` stored by the database determines the JWT role and post-login shell.

| Login group | Accepted stored roles | Shell |
|---|---|---|
| `management` | `admin`, `manager` | Management shell |
| `staff` | `employee`, `cashier`, `kitchen` | Role-specific operational shell |

Customer authentication remains separate: registered and guest customer tokens continue to use the existing customer endpoints and `customerToken` storage.

## Security contract

- `AuthController.Login` rejects missing, unknown, and role-mismatched login groups.
- A token role is always the normalized `Employee.Role` from the authenticated record. The request cannot nominate an effective role.
- JWT branch fields come from that employee record. The obsolete request `BranchId` is ignored for authorization and cannot expand scope.
- Existing active-account checks, password verification, and legacy password rehashing remain intact.
- The admin-web employee-system session continues to use the established `adminToken` key; customer sessions remain isolated in `customerToken`. A successful employee-system login clears all old employee-system session state before the server token and role are stored.

## Routing and staff shell

| Stored role | Landing route | UI shell |
|---|---|---|
| admin | `/dashboard` | management |
| manager | `/dashboard` | branch-scoped management |
| employee | `/staff/profile` | StaffNavbar |
| cashier | `/pos` | CashierNavbar |
| kitchen | `/kitchen` | KitchenNavbar |

The employee shell provides only profile, own attendance, own work schedule, and the existing inventory overview read model. It does not expose employee management, expenses, business insights, branch settings, inventory administration, or receipt/issue confirmation. Existing backend authorization remains the enforcement boundary.

The issue-draft backend permission remains an operational API capability, but its current web form depends on the manager-only inventory-item catalogue endpoint. It is intentionally not exposed in the employee shell until that read-model/API contract is separately designed.

## Tests and validation

`UnifiedStaffAuthenticationTests` cover management/staff acceptance, role-mismatch denial, unknown/missing login group, stored-role JWT issuance, and rejection of client branch override. Existing employee controller, attendance, work-schedule, and inventory service tests continue to cover employee authorization paths.

No migration, database connection, or database mutation is part of this task. Browser/API runtime login confirmation remains human-host validation.

## Human runtime checklist

1. Sign in an admin and manager with **Đăng nhập quản lý**; verify their stored roles and manager branch scope.
2. Sign in employee, cashier, and kitchen with **Đăng nhập nhân viên**; verify `/staff/profile`, `/pos`, and `/kitchen` respectively.
3. Attempt each account in the opposite group and verify a safe denial without a JWT.
4. Verify an employee cannot directly reach management routes and cannot confirm inventory documents.
