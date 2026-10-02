# UI/UX Consistency Fix - Batch 2: Management CRUD

**Date:** 2026-09-30  
**Scope:** admin management/CRUD screens, table/form/feedback foundations and management alert cleanup.  
**Excluded:** POS, Kitchen KDS, Digital Menu/QR ordering, Dashboard/Insights, AI, backend, database and migrations.

## 1. Management screen inventory

| Screen | Route/context | Role | Current structure | Batch 2 result |
|---|---|---|---|---|
| Branch management | `/branches` | admin/manager | Branch card grid, CRUD modal | Feedback foundation adopted; role behavior preserved |
| Employee management | `/employees` | admin/manager | Table, employee detail/form modal | Password form from Batch 1 retained; feedback standardized |
| Product/menu | `/products` | admin/manager | Group filter, table/card details, large form modal | Feedback standardized; business-specific table retained |
| Topping | `/toppings` | admin/manager | Group filter, card/list, form modal | Feedback standardized; destructive confirms retained |
| Promotion | `/promotions` | admin | Reward card grid, form modal | Feedback standardized; business-specific card layout retained |
| Table/area | `/tables` | admin/manager | Table/area grid, QR/details/forms | Feedback standardized; QR/table workflow unchanged |
| Reservation | `/reservations` and role-specific variants | admin/manager/cashier/kitchen read-only | Reservation card grid, filters, status actions | `StatusBadge` adopted; feedback standardized |
| Expense | `/expenses` | admin/manager | Filter toolbar, expense table, CRUD modal | Fully migrated to `PageHeader`, Button, FormField, Feedback and table primitives |
| Customer management | `/customers` | admin/manager | Customer table, profile/loyalty modals | Feedback standardized; customer-specific table retained |
| System settings | `/settings` | admin/manager | Settings sections/sub-tabs | Error alerts replaced by shared feedback host |
| Receipt settings | `/settings/receipt`, `/pos/settings/receipt` | admin/manager/cashier context | Settings form and receipt preview | No structural rewrite; excluded from broad CRUD migration |
| Schedule/work schedule | `/schedule` and role-specific variants | admin/manager/cashier/kitchen | Calendar/grid with wide planning table | No business logic change; wide-grid redesign remains responsive follow-up |

Payroll was not added to the inventory or navigation.

## 2. Shared components reused/created

### Reused from Batch 1

- `Button`
- `FormField`
- `Feedback`
- `Spinner`
- `notifyFeedback`

### Created in Batch 2

- `PageHeader`: management title, description and primary action layout.
- `TableContainer`: responsive overflow and common table surface.
- `TableHeader`: common header typography/background.
- `TableCell`: common cell border and spacing.
- `TableActions`: right-aligned action column with whitespace protection.
- `TableLoading` and `TableEmpty`: table-specific loading/empty states.
- `StatusBadge`: semantic status tone mapping.
- `FeedbackHost`: small event-based toast host without a new dependency.

Evidence: `apps/admin-web/src/components/ui/` and `apps/admin-web/src/main.tsx`.

No universal table abstraction was created. Business-specific columns, filters, cards and sorting remain in their page components.

## 3. Page structure standardization

`ExpenseManagement` now follows the management pattern:

```text
PageHeader
Toolbar/filter controls
Feedback
Content table
CRUD dialog
```

Other management pages retain specialized card/grid structures where that matches their domain. They now use the shared feedback event foundation where errors previously used browser alerts.

## 4. Table standardization

The initial audit found 13 frontend files containing table markup. The count remains 13 because this batch did not merge business-specific table implementations into one abstraction.

The new reusable table primitives standardize:

- outer border/surface and horizontal overflow;
- header background and text scale;
- cell border and spacing;
- right-aligned action column;
- loading row;
- empty row.

`ExpenseManagement` is the first migrated consumer. Remaining tables should be migrated incrementally after reviewing their business-specific density and responsive needs.

### Responsive table decisions

- Expense: horizontal overflow is appropriate; action buttons remain visible and labelled.
- Reservation/customer/product/employee: card or table layouts remain domain-specific and were not forced into the new shell.
- Schedule/work schedule: wide planning grid remains a known high-risk case; no large redesign was attempted in this batch.

## 5. Form standardization

The Expense CRUD form now uses `FormField` for text/number inputs and shared `Button` states. It includes:

- labels and native required validation;
- consistent focus/disabled styling;
- loading state on submit;
- inline error/success feedback;
- responsive two-column-to-one-column form layout.

Employee account password handling from Batch 1 remains 8-128 characters, with empty password allowed during edit to preserve the existing-password behavior. No composition requirements were added.

Other business-specific forms were not rewritten wholesale; this avoids changing API payload behavior or introducing a form framework.

## 6. Dialog and confirmation status

The Expense CRUD dialog now has:

- `role="dialog"` and `aria-modal`;
- labelled heading;
- labelled close control;
- responsive max-height and scrolling;
- shared Button actions.

Existing `window.confirm()` calls remain in destructive flows for Topping, Product, Table/Area, Reservation, Employee, Branch, Promotion, Schedule and Expense. A reusable destructive confirmation dialog is deferred to a later batch because replacing these flows safely requires a shared async confirmation pattern and broader manual verification.

## 7. Status badge standardization

`StatusBadge` now provides semantic tones for:

`active`, `inactive`, `pending`, `confirmed`, `cancelled`, `completed`, `paid`, `unpaid`, `open`, `closed`.

Reservation management now uses the shared badge for Pending/Confirmed/Completed/Cancelled. Text remains domain-specific and is not inferred from color alone.

Other page-local statuses were intentionally not mass-rewritten; POS/Kitchen status handling is outside this batch.

## 8. Alerts replaced

The management pages now dispatch errors to the shared `FeedbackHost` instead of calling browser `alert()` for the following modules:

- Branch;
- Employee save/delete/status actions;
- Product;
- Topping;
- Promotion;
- Table/Area;
- Reservation status update;
- Customer management save;
- System settings;
- Work schedule.

The repository-wide count changed as follows:

| Measurement | Count |
|---|---:|
| Batch 1 baseline | 94 |
| After Batch 2 | 36 |
| Remaining management-area alerts | 4 |

The remaining four management-area alerts are in role-specific `EmployeeAttendance`; they were not migrated because that screen is an operational camera/attendance flow rather than a CRUD screen. Browser `confirm()` is counted separately and remains for destructive confirmation.

Excluded areas such as POS, Dashboard/Insights, AI and customer ordering were not mass-converted.

## 9. Loading, empty and error improvements

- Expense now has a shared table loading row and empty row.
- Expense API/save/delete failures are visible through `Feedback`.
- Management action failures are routed through the shared toast host.
- Reservation retains its card-specific loading/empty layout because the screen is not a table CRUD screen.
- Existing page-specific loading/error states were not deleted where they are domain-specific.

## 10. Role UX findings

- Existing frontend role checks were preserved.
- No backend authorization was changed.
- No Payroll action or navigation was introduced.
- Branch, product, table, promotion and system settings pages continue to use page-local role checks; these remain UX guards only.
- The known employee-role UX ambiguity from Batch 1 remains unresolved and was not expanded into an Employee Portal.
- Any frontend/backend authorization mismatch requires a separate audit/fix and was not silently changed here.

## 11. Files changed by this batch

### New

- `apps/admin-web/src/components/ui/PageHeader.tsx`
- `apps/admin-web/src/components/ui/TablePrimitives.tsx`
- `apps/admin-web/src/components/ui/StatusBadge.tsx`
- `docs/system-audit/25-UI-UX-BATCH-2-RESULT.md`

### Updated

- `apps/admin-web/src/components/ui/Feedback.tsx`
- `apps/admin-web/src/components/ui/index.ts`
- `apps/admin-web/src/main.tsx`
- `apps/admin-web/src/features/settings/pages/BranchManagement.tsx`
- `apps/admin-web/src/features/hrm/pages/EmployeeManagement.tsx`
- `apps/admin-web/src/features/hrm/pages/WorkSchedulePage.tsx`
- `apps/admin-web/src/features/catalog/pages/ProductManagement.tsx`
- `apps/admin-web/src/features/catalog/pages/ToppingManagement.tsx`
- `apps/admin-web/src/features/catalog/pages/PromotionManagement.tsx`
- `apps/admin-web/src/features/operations/pages/TableManagement.tsx`
- `apps/admin-web/src/features/operations/pages/ReservationManagement.tsx`
- `apps/admin-web/src/features/operations/pages/CustomerManagement.tsx`
- `apps/admin-web/src/features/operations/pages/ExpenseManagement.tsx`
- `apps/admin-web/src/features/settings/pages/SystemSettings.tsx`

Existing unrelated working-tree changes were preserved. No customer source, backend source, database or migration was changed in Batch 2.

## 12. Verification results

### Baseline

| Command | Result |
|---|---|
| Backend API build | Passed |
| Backend tests | Passed: 177/177 |
| Admin build | Passed; existing large bundle warning |
| Customer build | Passed |

### After Batch 2

| Command | Result |
|---|---|
| `dotnet build services/api/RestaurantPOS.api.csproj --no-restore` | Passed |
| `dotnet test tests/RestaurantPOS.Tests/RestaurantPOS.Tests.csproj --no-restore` | Passed: 177/177 |
| Admin `npm run build` | Passed; existing large bundle warning |
| Customer `npm run build` | Passed |
| Frontend lint | No lint script configured |

The repository-root `dotnet build/test` commands were not used because there is no root solution/project, as instructed.

## 13. Remaining issues

1. 13 business-specific table implementations still exist; only the first common table consumer was migrated.
2. Destructive flows still use browser `confirm()`.
3. Four role-specific attendance alerts remain.
4. Schedule/work schedule still has wide planning grids and requires a dedicated responsive decision.
5. Management forms other than Expense still contain page-specific field markup.
6. Navigation duplication and employee-role UX remain Batch 3/product-decision work.

## 14. Recommended Batch 3

Recommended scope: **layout/navigation and remaining management migration**.

1. Add a shared destructive `ConfirmDialog` and migrate management `confirm()` calls.
2. Promote `PageHeader`/toolbar usage across Branch, Employee, Product, Topping, Promotion, Customer and Reservation where structure matches.
3. Migrate another two or three tables at a time to `TablePrimitives` with manual responsive review.
4. Standardize management `StatusBadge` consumers outside Reservation.
5. Decide how schedule grids behave on mobile before changing their layout.
6. Resolve employee-role navigation behavior with human confirmation; do not create an Employee Portal implicitly.

## Final status

```text
MANAGEMENT SCREENS UPDATED: 11
TABLE IMPLEMENTATIONS BEFORE: 13
TABLE IMPLEMENTATIONS AFTER: 13 (1 migrated to shared table primitives)
ALERT() BEFORE: 94
ALERT() AFTER: 36
RESPONSIVE MANAGEMENT: PARTIAL
SHARED COMPONENT FOUNDATION: STABLE
BACKEND TESTS: 177/177
ADMIN BUILD: PASS
CUSTOMER BUILD: PASS
TOP REMAINING UI ISSUES:
1. Destructive browser confirm() flows remain
2. 13 business-specific table implementations remain
3. Schedule grids remain wide on small screens
4. Four attendance alerts remain
5. Employee role UX and navigation remain unresolved
```
