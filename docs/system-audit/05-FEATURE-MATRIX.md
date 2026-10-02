# Feature Matrix

| Module | Feature | Actor | Frontend | Backend | Database | Status |
|---|---|---|---|---|---|---|
| Auth | Employee login/JWT | admin/manager/cashier/kitchen | `features/auth/pages/LoginPage.tsx` | `AuthController`, `JwtService` | Employee | IMPLEMENTED |
| Auth | Customer/guest token | customer/guest | `CustomerLogin.tsx` | `AuthController.customer-token` | Customer | IMPLEMENTED |
| Branch | CRUD/status/profile | admin, manager (read/UI varies) | `BranchManagement`, settings | `BranchController` | Branch | IMPLEMENTED |
| Catalog | Product/topping/options/availability | admin/staff/kitchen | catalog + kitchen pages | Product/Topping controllers/services | Product/Topping | IMPLEMENTED |
| Catalog | Promotions | admin | `PromotionManagement` | `PromotionController` | Promotion | PARTIALLY IMPLEMENTED: CRUD exists; order discount application is not evidenced |
| POS | Create/merge/update/cancel order | staff/customer paths | `POSPage`, `DigitalMenu` | `OrderController`, `OrderService` | Order/OrderDetail | IMPLEMENTED |
| Kitchen | Batch requests, status, history | kitchen/admin/manager | KDS pages | `KitchenService`, Order endpoints | OrderRequest/Items | IMPLEMENTED |
| Payment | Cash/bank transfer recording | admin/manager/employee/cashier | POS | Order payment endpoint/service | Order | IMPLEMENTED; no external settlement webhook evidenced |
| Customer | Profile, loyalty, reservations | customer/staff | customer + operations | Customer/Reservation/Loyalty services | Customer/Loyalty/Reservation | IMPLEMENTED/PARTIAL by subfeature |
| HRM | Employee, attendance, shifts/schedules | admin/manager/staff | hrm pages | controllers/services | Employee/Attendance/Shift/WorkSchedule | IMPLEMENTED; Payroll is out of current scope |
| Finance | Expenses, dashboard, invoices, insights | admin/manager | operations/analytics | Expense/Dashboard/BusinessInsight controllers | Expense/BusinessInsight/Order | IMPLEMENTED |
| Settings | Receipt/system settings | admin/manager | settings pages | controllers/services | ReceiptSettings/SystemSetting | IMPLEMENTED |
| AI | Gemini chat/tool calling | role-dependent | admin/customer ChatBot | AI controller/orchestrator/tools | Mostly reads existing data; conversation history request-scoped | IMPLEMENTED/PARTIAL: some write tools |
| Realtime | Kitchen events | staff/kitchen | admin KDS/POS consumers | KitchenHub/Notifier | none directly | IMPLEMENTED with polling fallback |
| Inventory | Stock/ingredient deduction | unknown | no verified end-to-end page/entity | no verified current implementation | no Ingredient/Inventory entity in active model | PLANNED/UNKNOWN |
