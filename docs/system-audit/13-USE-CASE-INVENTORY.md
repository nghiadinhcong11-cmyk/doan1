# Use-Case Inventory

| ID | Name | Actor | Related API | Status |
|---|---|---|---|---|
| UC-01 | Employee login | employee roles | `POST /api/Auth/login` | IMPLEMENTED |
| UC-02 | Customer/guest token access | customer/guest | `POST /api/Auth/customer-token` | IMPLEMENTED |
| UC-03 | Manage branches | admin | Branch endpoints | IMPLEMENTED |
| UC-04 | Manage staff and roles | admin/manager | Employee endpoints | IMPLEMENTED |
| UC-05 | Manage menu/catalog | admin | Product/Topping/Promotion endpoints | IMPLEMENTED/PARTIAL for discounts |
| UC-06 | Manage tables and areas | admin/staff | Area/Table endpoints | IMPLEMENTED |
| UC-07 | Create and update POS order | employee/cashier/manager/admin | Order create/update | IMPLEMENTED |
| UC-08 | Send order batch to kitchen | POS staff | `POST /api/Order/{id}/send-to-kitchen` | IMPLEMENTED |
| UC-09 | Process kitchen request | kitchen/manager/admin | kitchen request endpoints | IMPLEMENTED |
| UC-10 | Record payment | cashier/staff | `POST /api/Order/{id}/payment` | IMPLEMENTED; manual confirmation |
| UC-11 | Customer order by QR/table | customer/guest | Order + Table/Product endpoints | IMPLEMENTED/PARTIAL security verification |
| UC-12 | Reservation | customer/staff | Reservation endpoints | IMPLEMENTED |
| UC-13 | Attendance and schedules | employee/manager/admin | Attendance/WorkSchedule/Shift | IMPLEMENTED |
| UC-14 | Record branch expense | admin/manager | Expense endpoints | IMPLEMENTED |
| UC-15 | View dashboard/insights | admin/manager | Dashboard/BusinessInsight | IMPLEMENTED |
| UC-16 | Manage receipt/system settings | admin/manager | settings endpoints | IMPLEMENTED |
| UC-17 | Chat with AI assistant | authenticated roles/customer | `POST /api/Ai/chat` | IMPLEMENTED |
| UC-18 | Execute AI tool action | role-authorized AI user | tool registry | IMPLEMENTED/PARTIAL branch policy |
| UC-19 | Inventory/ingredient management | unknown | no verified endpoint/entity | PLANNED/UNKNOWN |

Use-case diagrams should use only UC-01–UC-19, with UC-20 shown as out of scope/planned if needed.
