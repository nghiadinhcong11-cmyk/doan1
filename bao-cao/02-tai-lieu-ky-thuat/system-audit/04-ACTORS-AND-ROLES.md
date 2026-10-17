# Actors and Roles

| Actor/role | Evidence-based access | Scope/status |
|---|---|---|
| `admin` | Branch management, broad staff/catalog/settings/reporting/POS/kitchen/AI access | System-wide role; JWT branch may be present but controller helpers generally treat admin as unrestricted |
| `manager` | Accepted by many controllers/UI for branch management, reports, HRM, POS/KDS and business insights | Implemented in authorization checks despite older docs claiming it was absent; branch-scoped in controller helpers |
| `employee` | Staff-facing order/table/attendance/kitchen-related operations and selected AI tools | Branch-scoped |
| `cashier` | POS/payment/order/invoice flows and selected staff operations | Branch-scoped |
| `kitchen` | KDS requests/status/product availability and selected order/AI operations | Branch-scoped |
| `customer` | Customer token, customer profile/order/reservation/loyalty and customer AI tools | Identity/phone scoped where checks exist; QR menu endpoints include anonymous-access paths |
| guest | Can request `/api/Auth/customer-token` by phone and receive a customer JWT; can use customer web flow | Not a separate JWT role |

Authorization is a mixture of `[Authorize(Roles=...)]`, controller helper checks, JWT `branchId`, and frontend role guards. `Position` is display data, not the authoritative role. The manager role is present in `Employee.Role` default/comments, login mode responses, controller attributes, and frontend logic; it is not merely planned.

Evidence: `AuthController.cs`, `JwtService.cs`, `Employee.cs`, controller attributes/helpers, `apps/admin-web/src/App.tsx`.
