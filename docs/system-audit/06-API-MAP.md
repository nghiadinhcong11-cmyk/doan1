# API Map

All controller routes use `api/[controller]`. The following is the verified controller surface; method-level details remain in source and Swagger.

| Controller | Representative endpoints | Roles/notes |
|---|---|---|
| Auth | `POST /api/Auth/login`, `POST /customer-token`, `GET /me`, `POST /change-password` | Login has strict rate limit; `/me` and password change require JWT |
| Branch | `GET/POST /api/Branch`, `PUT/PATCH/DELETE /api/Branch/{id}` | Writes admin; reads are used by customer/admin flows |
| Product | CRUD, toggle-status, availability | Role checks in controller; used by menu/KDS |
| Topping/Promotion | CRUD and toggles | Admin UI primarily |
| Area/Table | Area/table CRUD, table status/details | Branch/table flows |
| Order | list/get/create, accept web order, send-to-kitchen, kitchen status/requests/history, payment, loyalty/redeem, status/delete | Role and branch checks vary per action |
| Invoice | list/detail | admin/manager/cashier/employee |
| Customer | list, existence, phone/profile, create/update, loyalty history | customer identity checks plus staff access |
| Reservation | CRUD/list | customer and staff flows |
| Employee/Attendance/Shift/WorkSchedule | HRM CRUD and attendance endpoints | authorized staff; branch checks in controllers; no Payroll endpoint |
| Expense | list/create/update/delete | admin/manager; branch validation |
| Dashboard/BusinessInsight | summary, insights, read/resolve | admin/manager, branch-aware |
| ReceiptSettings/SystemSettings | branch settings CRUD/resolution | role and branch-specific behavior |
| Notification | list/read/read-all | authenticated |
| AI | `POST /api/Ai/chat` | JWT-derived role/context; AI rate limit |

Evidence: all files under `services/api/src/WebAPI/Controllers/` and `services/api/src/AI/Controllers/AiController.cs`. For exact request/response shapes, use source DTOs and `packages/shared/swagger.json`; do not infer undocumented endpoints from filenames.
