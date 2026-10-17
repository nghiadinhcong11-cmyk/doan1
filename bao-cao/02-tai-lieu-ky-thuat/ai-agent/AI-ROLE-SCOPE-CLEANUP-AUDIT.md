# AI Role Scope Cleanup Audit

**Audit date:** 2026-10-17
**Repository:** `D:\Dev\doan`
**Scope:** source, configuration, DI registrations, frontend references, isolated harnesses and builds. No database/API/browser access was used.

## 1. Executive summary

The active AI surface is now intentionally concentrated on:

- **Admin/manager:** administrative read tools and the locked manager Sentence Embedding pilot.
- **Customer/guest:** customer self-service tools and the guest menu allowlist.
- **Kitchen:** no AI UI remains in the Kitchen route; KDS and SignalR remain independent.
- **Employee/cashier:** the generic Admin Web ChatBot was removed from their route shells. Their backend AI registrations remain because several tools are shared, still registered, or have unresolved dependency/runtime impact.

No backend tool registration was removed in this audit. This is deliberate minimal safe removal: source inspection did not prove any registered tool was dead and kitchen-only. MockAiProvider remains required for flag-OFF behavior, non-pilot roles, guest/customer paths, and existing provider dependencies.

## 2. Runtime architecture and security boundaries

The inspected path is:

`POST /api/Ai/chat` / `POST /api/Ai/guest-chat`
→ `AiController`
→ trusted `AiUserContext`
→ `AiOrchestrator`
→ manager feature gate and `ManagerEmbeddingPilot` when enabled
→ `AiToolRegistry` lookup
→ `AiPermissionService.CanExecuteAsync`
→ tool `ExecuteAsync`
→ application service.

Relevant source:

- `services/api/src/AI/Controllers/AiController.cs`: `Chat` and `GuestChat`.
- `services/api/src/AI/Services/AiOrchestrator.cs`: orchestration, manager pilot gate, decision handling and tool execution.
- `services/api/src/AI/Services/ManagerEmbeddingPilot.cs`: v3.3 semantic policy.
- `services/api/src/AI/Tools/AiToolRegistry.cs`: registry lookup/schema filtering.
- `services/api/src/AI/Services/AiPermissionService.cs`: role, risk, customer allowlist and branch-context checks.
- `services/api/Program.cs`: DI registrations.
- `services/api/src/AI/Diagnostics/AiCorrelationContext.cs` and `SafeAiDiagnosticFileLoggerProvider.cs`: correlation/UAT diagnostics.

The pilot continues to use trusted server/JWT role and branch context. It does not take role or BranchId from chat text, does not call business services directly, and remains fail-closed for unavailable inference.

## 3. Scope before and after cleanup

| Area | Before audit | After audit |
|---|---|---|
| Admin | ChatBot and administrative tools | Kept |
| Manager | ChatBot plus manager embedding pilot | Kept; v3.3 protected |
| Customer | Customer ChatBot and customer allowlist | Kept |
| Guest QR | Guest chat endpoint with menu-only permission | Kept for review/limited demo |
| Employee | Generic Admin Web ChatBot was rendered | ChatBot removed from StaffRoutes |
| Cashier | Generic Admin Web ChatBot was rendered | ChatBot removed from CashierRoutes |
| Kitchen | Kitchen ChatBot had already been removed from KitchenRoutes | No AI UI; KDS unchanged |
| Backend tools | Shared and role-specific registrations | Retained pending dependency-safe migration |

## 4. Complete AI tool inventory

Risk levels are read from each `IAiTool.RiskLevel` implementation. AllowedRoles are read from the tool source, not inferred from filenames.

| Tool | Source | AllowedRoles | Risk | Main dependency | Target / recommendation |
|---|---|---|---|---|---|
| `get_revenue` | `AI/Tools/Admin/GetRevenueTool.cs` | admin, manager | Read | DashboardService | Admin/Manager — **KEEP**; v3.3 pilot |
| `get_business_summary` | `AI/Tools/Admin/GetBusinessSummaryTool.cs` | admin, manager | Read | DashboardService | Admin/Manager — **KEEP**; v3.3 pilot |
| `get_financial_analysis` | `AI/Tools/Admin/GetFinancialAnalysisTool.cs` | admin, manager | Read | financial/dashboard services | Admin/Manager — **KEEP** |
| `get_revenue_comparison` | `AI/Tools/Admin/GetRevenueComparisonTool.cs` | admin, manager | Read | dashboard/reporting services | Admin/Manager — **KEEP** |
| `get_active_staff` | `AI/Tools/Admin/GetActiveStaffTool.cs` | admin, manager | Read | staff/application service | Admin/Manager — **KEEP** |
| `update_product_price` | `AI/Tools/Admin/UpdateProductPriceTool.cs` | admin | Write | product service | Admin — **KEEP**, confirmation/permission boundary required |
| `get_order_list` | `AI/Tools/Admin/GetOrderListTool.cs` | admin, manager, employee, cashier, kitchen | Read | OrderService/branch context | Shared — **REVIEW**, not kitchen-only |
| `get_best_sellers` | `AI/Tools/Admin/GetBestSellersTool.cs` | admin, manager, employee, cashier, kitchen | Read | reporting/order data | Shared — **REVIEW**, not kitchen-only |
| `employee_get_table_summary` | `AI/Tools/Employee/GetTableSummaryTool.cs` | admin, manager, employee, cashier, kitchen | Read | table/order services | Shared — **REVIEW/FUTURE** |
| `update_order_status` | `AI/Tools/Employee/UpdateOrderStatusTool.cs` | admin, manager, employee, cashier, kitchen | Write | OrderService | Shared mutation — **REVIEW/FUTURE**; do not remove business logic |
| `get_my_shift` | `AI/Tools/Employee/GetMyShiftTool.cs` | employee, cashier, kitchen | Read | attendance/schedule services | Employee/cashier/kitchen — **FUTURE** |
| `customer_get_menu` | `AI/Tools/Customer/GetMenuTool.cs` | admin, manager, employee, cashier, kitchen, customer | Read | menu/product service | Customer/guest shared menu — **KEEP** |
| `get_my_orders` | `AI/Tools/Customer/GetMyOrderTool.cs` | customer | Read | customer order service | Customer — **KEEP** |
| `create_booking` | `AI/Tools/Customer/CreateBookingTool.cs` | customer | Write | booking/table service | Customer — **REVIEW/KEEP** with explicit confirmation and validation |

All 14 registrations are present in `services/api/Program.cs` and are discoverable through `AiToolRegistry`. The registry filters customer tools to `customer_get_menu` and `get_my_orders`; guest execution is limited by `AiPermissionService` to read-only `customer_get_menu`.

## 5. Kitchen-specific residue audit

### Findings

- `apps/admin-web/src/app/routes/KitchenRoutes.tsx` contains no ChatBot import or render after the earlier UI cleanup.
- A repository search found no kitchen-only AI route, hook, prompt component, or `/api/Ai` call under the KDS feature.
- `PromptBuilder` maps `kitchen`, `employee` and `cashier` to the shared EmployeePrompt; there is no separate KitchenPrompt to delete.
- Tools that include the kitchen role (`get_order_list`, `get_best_sellers`, `employee_get_table_summary`, `update_order_status`, `get_my_shift`, and the shared menu tool) are not kitchen-only and therefore remain.
- `KitchenPage`, `KitchenService`, `KitchenController`, `KitchenHub`, OrderRequest flow and SignalR events were not changed.

### Classification

- **AI-specific code no longer used:** Kitchen ChatBot route rendering (already removed).
- **Still used/shared:** registry, permission, orchestrator, shared prompts/tools, logging and business services.
- **Feature-gated:** ManagerEmbeddingPilot and its flag.
- **Not proven dead:** all backend tools with employee/cashier/kitchen role mappings.

## 6. Files changed in this cleanup

- `apps/admin-web/src/app/routes/StaffRoutes.tsx`: removed ChatBot import and render.
- `apps/admin-web/src/app/routes/CashierRoutes.tsx`: removed ChatBot import and render.
- `apps/admin-web/src/app/routes/KitchenRoutes.tsx`: already has the same AI UI removal in the working tree; no backend/KDS logic was altered by this audit.
- `bao-cao/02-tai-lieu-ky-thuat/ai-agent/AI-ROLE-SCOPE-CLEANUP-AUDIT.md`
- `bao-cao/02-tai-lieu-ky-thuat/ai-agent/AI-FUTURE-EXTENSIONS.md`

**Tools unregistered:** none. Removing a registration without proving all consumers and prompt/provider paths would be unsafe.

## 7. Kept dependencies and protections

The following remain intentionally protected:

- `MockAiProvider` and `MockAiIntentRecognizer` for flag-OFF and non-pilot paths.
- `IAiProvider`, `AiOrchestrator`, `AiToolRegistry`, `AiPermissionService`.
- Manager-only gate, trusted JWT context and branch isolation.
- Read-only guard, confirmation boundary and fail-closed behavior.
- `ManagerEmbeddingPilot`, multilingual E5 ONNX, tokenizer and locked `manager-two-intent-v3.3` baseline.
- `AI_EMBEDDING_PILOT_DECISION`, correlation context, `AI_HTTP_REQUEST`, `AI_TOOL_EXECUTION`, `AI_HTTP_RESPONSE` and `SafeAiDiagnosticFileLoggerProvider`.
- Business services, controllers, entities, migrations, KDS and SignalR.

## 8. Role impact

| Role | Result |
|---|---|
| admin | Admin AI path and administrative read/write tools remain. |
| manager | Embedding pilot and `get_revenue`/`get_business_summary` remain protected. |
| customer | Customer ChatBot and authenticated self-service allowlist remain. |
| guest | Guest endpoint remains menu-only and read-only through permission checks. |
| employee | ChatBot UI removed; POS/HR/business routes remain. Backend shared tools are retained for future/review. |
| cashier | ChatBot UI removed; POS/payment routes remain. Backend shared tools are retained for future/review. |
| kitchen | No AI UI; KDS, order status, notifications and SignalR remain. |

## 9. Verification

| Check | Result | Evidence |
|---|---|---|
| Backend build | PASS | `dotnet build services/api/RestaurantPOS.api.csproj --no-restore -c Release -o .role-scope-build`; 0 warnings, 0 errors |
| Admin Web build | PASS | `npm run build` in `apps/admin-web`; Vite completed |
| Customer Web build | PASS | `npm run build` in `apps/customer-web`; Vite completed |
| Runtime/AI harness | PASS | `RuntimePilotVerification`: 27 passed, 0 failed; database NOT_USED, API NOT_STARTED |
| Manager pilot | PASS in harness | ACCEPT, CLARIFY, UNSUPPORTED, permission/branch, fail-closed and concurrency checks |
| Kitchen AI UI static check | PASS | no ChatBot/AI route reference in KitchenRoutes/KDS feature |
| Tool registration inventory | PASS | 14 `IAiTool` registrations matched to source classes |
| Database/migration/browser | NOT RUN | explicitly excluded by task constraints |

The harness was isolated and did not connect to PostgreSQL/Supabase. The builds produced only build output; no runtime API was started.

## 10. Manual acceptance checklist

The user should verify in the normal development UI:

1. Admin: ChatBot still opens and administrative read flows work.
2. Manager: v3.3 revenue/summary pilot, unsupported period, negation and out-of-scope behavior; correlate with UAT logs.
3. Customer: own orders/menu; verify no administrative data is exposed.
4. Guest QR: menu-only behavior and safe refusal for other requests.
5. Employee/cashier: no AI button appears; POS, payment, shift and order flows still work.
6. Kitchen: KDS active requests, status updates, product availability, history, notifications and SignalR remain functional.
7. For any AI request, correlate `AI_HTTP_REQUEST`, `AI_EMBEDDING_PILOT_DECISION`, `AI_TOOL_EXECUTION` and `AI_HTTP_RESPONSE` without logging raw chat or secrets.

## 11. Decision

- **GO:** narrow UI cleanup for employee/cashier and existing kitchen AI UI removal, supported by source checks and builds.
- **NO-GO:** removing MockAiProvider, shared EmployeePrompt, shared tools, or backend registrations. Dependencies and role mappings are still active or unresolved.
- **Overall:** **PARTIAL / GO for minimal safe scope cleanup; NO-GO for complete backend AI-role removal** until a separate migration proves replacement coverage and regression for each affected role.

No intent is marked APPROVED and no production readiness is claimed.




## Current status / Superseded by

Current scope cleanup audit; manager pilot is retained, MockAiProvider removal remains NO-GO.
