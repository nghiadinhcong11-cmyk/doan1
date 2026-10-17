# Static Review: Authentication, Authorization and Branch Isolation

## 1. Boundary and evidence

| Item | Value |
|---|---|
| Review type | Static source/configuration/document review only |
| Reference HEAD | `4abeb1bf2e646df837745713a495b164eed60488` |
| Branch / working tree | `main`; clean at review start; branch ahead of `origin/main` by one commit |
| Comparison point | `4476084e` is a handoff reference, not the current HEAD |
| Runtime | Not run: no API, browser, database, migration, SignalR or business transaction |
| Protected area | `services/Migrations_backup_20260919/` was not modified |
| Decisions retained | MockAiProvider remains primary; `EmployeeType` and `BasicSalary` remain profile fields |

Static evidence shows source intent and visible controls. It does not prove deployed token, database, browser-session or SignalR behavior.

## 2. Method

| Method | Status | Conclusion | Evidence |
|---|---|---|---|
| Static source review | Đã kiểm tra tĩnh | Listed controllers, hub and AI tools were inspected from entry point to visible authorization/scope checks | `services/api/src/Program.cs`, controllers, services, AI files |
| Technical verification | Chưa kiểm tra | No build/typecheck was run in this review | Task limit |
| Runtime verification | Chưa kiểm tra | Token expiry, HTTP status, DB scope and SignalR delivery remain unverified | `ba-test-frontend/ky-thuat-runtime/README.md` |
| Manual testing | Chưa kiểm tra | Cases remain `Chưa test` | `ba-test-frontend/` |

Current source is preferred over older audit/thesis documents. A source check is never recorded as runtime PASS.

## 3. Authentication mechanisms observed

- `Program.cs` configures JWT Bearer with issuer, audience, signing-key and lifetime validation, zero clock skew, and a token-validation callback that re-checks current employee/customer state.
- `JwtService.cs` creates subject, role, optional branch, customer session type/auth method, table and token-id claims. Direct guest tokens have a short configured lifetime; actual expiry was not tested.
- `RoleNames.IsValidStaffTokenState` compares token role/branch with current employee role, active state and branch. This is source evidence only.
- `AuthController` contains anonymous management/staff login and registered-customer token issuance; `/me` and password change require authentication. `GuestController` contains anonymous QR bootstrap/direct-session issuance after branch/table validation.
- Password verification uses the legacy-compatible verifier and hashing/policy services; optional rehashing is present. No password or secret was copied here.
- `OnTokenValidated` rejects missing/deactivated staff and inactive/invalid registered customers. Guest/directGuest sessions are signed customer sessions with branch/table claims, not registered customer sessions.
- Frontend route guards and token storage are UX controls; backend authorization remains the security boundary. Guest access to `/reservation` and `/profile` remains open under `COV-001`.

### Anonymous and token entry points

| Entry point | Status | Static result |
|---|---|---|
| `POST /api/Auth/login` | Đã rà soát | Anonymous login path; mode and role restrictions in `AuthController` |
| `POST /api/Auth/customer-token` | Đã rà soát | Anonymous registered-customer token path; active customer/password checks |
| `GET /api/Auth/me` | Đã rà soát | `[Authorize]`; identity comes from claims |
| `POST /api/Auth/change-password` | Đã rà soát | `[Authorize]`; requested identity is compared with token subject |
| `POST /api/Guest/bootstrap` | Đã rà soát | Anonymous; QR/table/branch validation before guest session |
| `POST /api/Guest/direct-session` | Đã rà soát | Anonymous; active branch and optional table validation |
| `POST /api/Ai/guest-chat` | Đã rà soát | Anonymous development-only Mock route; public read-only intent guard |
| Public branch/catalog/table reads | Đã rà soát | Product, topping, promotion, branch and table public reads; branch scope needs runtime confirmation where supplied |

## 4. Controller/action inventory

`Đã rà soát` means the entry point and visible authorization path were inspected; it is not a runtime result.

| Controller | Actions / route shape | Gate observed |
|---|---|---|
| Area | GET; POST; PUT/{id}; DELETE/{id} | Read staff roles; writes admin; non-admin read uses signed branch |
| Attendance | GET; POST check-in; PATCH {id}/check-out | Class staff-role authorization; resource scope needs runtime test |
| Auth | POST login; POST customer-token; GET me; POST change-password | Anonymous issuance; authenticated identity operations |
| Branch | GET; GET management; GET {id}/payment-info; POST; PUT/{id}; PATCH toggle-status; DELETE | Public active list; management writes admin; payment-info role + branch access |
| BusinessInsight | GET; GET {id}; GET unread-count; POST read/resolve | Class admin/manager; service/query branch scope needs runtime test |
| Customer | GET; GET exists/{phone}; GET {phone}; POST; POST walk-in; GET {id}/loyalty-history; PATCH {phone}/profile | Staff list/walk-in; exists anonymous; profile auth; ownership/scope findings SEC-001/SEC-002 |
| Dashboard | GET summary | Admin/manager; branch resolved in service/controller path |
| Employee | GET; GET {id}; POST; PUT {id}; PATCH toggle-status; DELETE {id} | Authenticated class; list/write admin/manager; detail broad staff role |
| Expense | GET; POST; PUT/{id}; DELETE/{id} | Admin/manager; branch and idempotency checks |
| Guest | POST bootstrap; POST direct-session | Explicit anonymous guest issuance |
| Inventory | Items; branch inventory/overview; minimum stock; receipts; issues; transactions | Operational read paths; management writes admin/manager; service branch filters |
| Invoice | GET; GET {id} | Admin/manager/cashier/employee; resource branch needs runtime test |
| Notification | GET; GET count; PATCH {id}/read; POST read-all | Authenticated; user ownership needs multi-user test |
| Order | List/create; accept; send-to-kitchen; kitchen-status; payment; loyalty; redeem; detail; status; delete; kitchen active/history/detail | Action role gates; branch/customer/guest ownership and persisted-total checks need runtime |
| Product | Public list; create/update/delete/toggle; availability | Admin writes except availability includes manager/kitchen; global availability finding SEC-003 |
| Promotion | Public list; create/update/delete/toggle | Admin writes |
| ReceiptSettings | GET; PUT; logo POST/DELETE | Staff reads; admin writes; branch-specific path needs runtime |
| Reservation | List/detail/create/update/status/delete | Authenticated; branch and customer/guest checks in path; guest route policy COV-001 |
| Shift | current/open/close/list | Admin/manager/cashier; employee/resource scope needs runtime |
| SystemSettings | GET; PUT | Admin-only; manager is not enabled at controller level |
| Table | Public list; authenticated detail; QR token; create/update/status/delete | Signed branch used for non-admin detail/write/service queries |
| Topping | Public list; create/update/delete | Admin writes |
| WorkSchedule | list/create/delete | Staff roles; branch/resource scope needs runtime |

No controller action is marked runtime PASS.

## 5. SignalR hub, groups and producers

| Item | Status | Static result |
|---|---|---|
| `/kitchenHub` | Đã rà soát | JWT query token accepted only for this hub path in `Program.cs` |
| `KitchenHub.OnConnectedAsync` | Đã rà soát | Admin/manager/employee/cashier/kitchen only; admin joins global `kitchen-admin`; non-admin branch comes from signed claim; user and branch-role groups are added |
| Invokable hub methods | Không áp dụng | No public client-invokable hub method found; hub sets groups and receives server broadcasts |
| `NewOrderRequest` | Đã rà soát | `KitchenNotifier`; global admin plus branch kitchen group when branch exists |
| `RequestStatusUpdated` | Đã rà soát | Branch-aware overload used when order branch exists; missing branch falls back to global admin group (`SEC-004`) |
| `PaymentCompleted`, `PendingOrderChanged` | Đã rà soát | Branch/role group routing in `PaymentEventRouting` and `OrderController` |
| `ProductAvailabilityChanged` | Đã rà soát | `Clients.All`; global catalog-vs-branch policy is unresolved (`SEC-003`) |

## 6. Resource and branch controls observed

- Staff branch identity is normally taken from signed `branchId`. Non-admin requests with another branch are rejected in inspected Area, Table, Inventory, Expense and Order paths.
- `RoleNames` treats admin as global and manager/employee/cashier/kitchen as branch-scoped. Source does not establish manager as global.
- `OrderController` checks branch access and customer/guest ownership in detail, mutation, payment, kitchen and loyalty paths. `OrderService.GetOrderByIdAsync` is a by-ID lookup, so every caller must apply the surrounding access check; runtime proof is absent.
- `InventoryService` applies branch access/filtering for branch inventory, receipts, issues and transactions. Item master data is separate and management-only.
- `ExpenseController` checks signed branch, requested branch and resource branch, with idempotency handling.
- `ReservationController` resolves branch from signed context and checks customer phone/guest scope in the inspected path. It does not settle the product policy for guest direct routes (`COV-001`).
- The inspected Customer path has no `BranchId` ownership discriminator. Customer lookup and loyalty/profile behavior therefore needs a chain-wide-vs-branch policy (`SEC-001`, `SEC-002`).
- Identity/branch values are derived from claims rather than trusted client role/user/branch fields in the inspected auth, order, table and profile paths. Negative API tests are still required.

## 7. AI controller, permissions and tool inventory

| Tool / route | Allowed roles / risk | Status |
|---|---|---|
| `POST /api/Ai/chat` | Authenticated; registry + `AiPermissionService` + risk policy | Đã rà soát; MockAiProvider/orchestrator path |
| `GET /api/Ai/live-read-only-status` | Authenticated | Đã rà soát |
| `GET /api/Ai/runtime-status` | Admin | Đã rà soát; development-only non-secret status |
| `POST /api/Ai/product-price-confirmation` | Admin; confirmation token bound to user | Đã rà soát |
| `POST /api/Ai/guest-chat` | Anonymous, development + Mock gate | Đã rà soát; public read-only intent guard |
| `get_menu` | admin, manager, employee, cashier, kitchen, customer; Read | Đã rà soát |
| `get_my_orders` | customer; Read | Đã rà soát; token-derived identity/phone |
| `create_booking` | customer; Write | Đã rà soát; guest allowlist blocks it |
| `get_active_staff` | admin, manager; Read | Đã rà soát; context branch passed |
| `get_best_sellers` | admin, manager, employee, cashier, kitchen; Read | Đã rà soát; context branch passed |
| `get_business_summary` | admin, manager; Read | Đã rà soát; context branch passed |
| `get_financial_analysis` | admin, manager; Read | Đã rà soát; context branch passed |
| `get_order_list` | admin, manager, employee, cashier, kitchen; Read | Đã rà soát; `OrderQueryFilter.BranchId` from context |
| `get_revenue_comparison` | admin, manager; Read | Đã rà soát; context branch passed |
| `get_revenue` | admin, manager; Read | Đã rà soát; context branch passed |
| `update_product_price` | admin; Write | Đã rà soát; confirmation path admin-only |
| `get_my_shift` | employee, cashier, kitchen; Read | Đã rà soát; token-derived user id |
| `get_table_summary` | admin, manager, employee, cashier, kitchen; Read | Đã rà soát; admin-global or context branch |
| `update_order_status` | admin, manager, employee, cashier, kitchen; Write | Đã rà soát; context branch and role passed to order service |

`AiPermissionService` checks known role, guest/customer allowlists and risk level. `AiToolValidator.ValidateBranchIsolation` exists, but inspected tools mostly enforce scope by passing `AiUserContext.BranchId` to application services; no direct tool call site was found. This is a defense-in-depth observation, not proof of a current bypass. `AiContextService` gives customer context a list of active products/branches and admin a chain-wide context; data minimization should be confirmed against policy.

## 8. Findings from this review

### SEC-001 — Authenticated staff customer lookup has no explicit branch/privacy boundary

`CustomerController.GetByPhone` uses `[Authorize]`; customer-role callers are restricted to their own phone, but authenticated staff can request any phone and receive `MapToDto(customer)`. The inspected customer path has no branch predicate. This may be intended for a chain-wide POS directory, but scope is not explicit.

### SEC-002 — Customer loyalty/profile administrative scope is not represented by a branch key

`GetLoyaltyHistory` permits admin/manager/cashier/employee/customer and checks self-ownership only for customer role. `UpdateProfile` permits admin/manager/customer and checks self-ownership only for customer role. The inspected path has no customer branch discriminator, so manager chain-wide access cannot be classified without a policy decision.

### SEC-003 — Product availability mutation/broadcast is global while manager/kitchen are branch-scoped

`ProductController.UpdateAvailability` allows manager/kitchen, updates a Product with no `BranchId`, and broadcasts `ProductAvailabilityChanged` via `Clients.All`. If availability is global catalog state this is expected; if branch-specific it is an isolation risk. Policy is unresolved.

### SEC-004 — Missing order branch uses a global kitchen-admin notification fallback

`KitchenService.UpdateRequestStatusAsync` calls the unscoped notifier when joined order branch is missing; `KitchenNotifier` sends it to global `kitchen-admin`. Only admins join that group in the inspected hub, so this is not proven cross-role exposure, but it is fail-open for malformed/orphaned data.

### SEC-005 — AI branch isolation is not centralized in the shared validator

`AiToolValidator.ValidateBranchIsolation` has no call site in the inspected tool source. Current tools usually pass context branch to services. A future tool accepting a branch argument could omit equivalent validation. This is a maintainability/defense-in-depth risk, not a demonstrated current exploit.

## 9. Limits and decisions required

- No runtime token, expiry, deactivation, resource-ID swap, branch swap, SignalR group or AI tool test was executed.
- No database was connected; source/model observations do not prove applied schema or data ownership.
- Decide whether customer profile/loyalty lookup and Product availability are chain-wide or branch-scoped.
- Decide whether missing order branch must fail closed instead of notifying global admins.
- `COV-001` remains open for guest `/reservation` and `/profile`; route rendering is not authorization evidence.

## 10. Manual/API cases to execute later

## 11. Post-fix static review — 2026-10-15

This addendum reviews the implementation changes without runtime, database, migration-apply, seed or SignalR execution.

| Area | Static result | Evidence | Remaining limit |
|---|---|---|---|
| Registration | Public registration is bound to `CustomerRegistrationDto`; only name, phone and password enter the create path. Existing duplicate records are not overwritten with internal fields. | `services/api/src/WebAPI/Controllers/CustomerController.cs`, `apps/customer-web/src/pages/CustomerLogin.tsx` | Extra-field and duplicate-account requests not executed. |
| Kitchen request status | Request/order join, active branch and status transition are checked before mutation. Invalid transitions are mapped to HTTP 400. | `KitchenService.UpdateRequestStatusAsync`, `OrderController.UpdateRequestStatus` | Transaction/concurrency behavior and notification recipients require runtime. |
| Branch availability | Branch-aware reads use the composite-key row; missing rows use `Available`, not legacy global `Products.AvailabilityStatus`. | `ProductController.GetProducts`, `ProductService.GetAllProductsAsync` | Migration/table state and all consumer behavior are not runtime verified. |
| Availability realtime | Kitchen filters `ProductAvailabilityChanged` by selected branch and refetches after reconnect. | `apps/admin-web/src/features/kitchen/pages/KitchenPage.tsx` | POS/customer/AI live consumers and actual hub delivery remain unverified. |
| Migration metadata | The migration has explicit `DbContext` and `Migration` attributes; Up/Down and snapshot remain source-only. | `20261015051548_AddProductBranchAvailability.cs`, `ApplicationDbContextModelSnapshot.cs` | No database schema or applied migration history was inspected. |

### Remaining policy question

Create/accept/payment/status/send-to-kitchen paths reject inactive branches. Open orders can therefore remain stored while processing or payment is blocked. The source does not define the recovery rule after reactivation or administrative cancellation/completion; this is a business decision, not silently changed here.

### 10.1 Post-decision source update — 2026-10-15

The earlier findings remain open until the required runtime or isolated-database evidence exists. Current source position after the approved policy changes:

- Customer and loyalty are chain-wide. Operational staff lookup is response-minimized; full profile mutation is not granted to every staff role. This is source/DI evidence, not runtime verification.
- Product catalog data remains shared. Availability is modeled per `(ProductId, BranchId)` in `ProductBranchAvailability`; controller and frontend callers resolve branch context where available. The migration source seeds existing product/branch pairs from legacy availability, but it has not been applied.
- Order creation, acceptance, payment, status updates and kitchen submission reject missing/inactive branches in the service path. Pending-order notification is suppressed when branch is missing; invalid kitchen request status uses an admin notification/log path instead of a global business-status payload.
- Guest `/profile` requires customer login. The customer frontend terminates guest loading, shows a login invitation, and does not call customer profile APIs with a guest token. Guest reservation policy remains open.
- MockAiProvider remains the provider path; no Gemini/OpenAI integration was restored.

### Methods after source changes

| Method | Status | Conclusion | Evidence |
|---|---|---|---|
| Static source/configuration review | Đã kiểm tra tĩnh | Affected policy paths reviewed | `CustomerController`, `ProductController`, `ProductBranchAvailability`, `OrderService`, `KitchenService`, customer `App.tsx`/`CustomerProfile.tsx` |
| Backend isolated build | Đã kiểm tra kỹ thuật | Compile/build succeeded in isolated output; not runtime evidence | `dotnet build --no-restore -p:UseAppHost=false -p:OutDir=.artifacts/api/` |
| Frontend build/typecheck | Đã kiểm tra kỹ thuật | Customer and admin production builds completed; this is not runtime evidence | `npm run build` in `apps/customer-web` and `apps/admin-web` |
| Migration/database verification | Chưa kiểm tra | Migration source exists; no database was connected or changed | `20261015051548_AddProductBranchAvailability.cs` |
| API/browser/SignalR/manual verification | Chưa kiểm tra | No PASS conclusion is allowed | `ba-test-frontend/` |

| Scope | Checklist | Status |
|---|---|---|
| Login, expired token, deactivated employee/customer | `ba-test-frontend/ky-thuat-runtime/README.md`, `AUTH-*` | Chưa test |
| Wrong role, wrong branch, changed resource ID | same file, `AUTH-*` / `BRANCH-*` | Chưa test |
| Guest/customer direct routes and ownership | `ba-test-frontend/khach-qr/guest-direct-routes.md`, `KQR-*` | Chưa test; COV-001 |
| Customer lookup/loyalty scope | runtime file, `BRANCH-*` | Chưa test; SEC-001/002 |
| SignalR branch groups and availability | runtime file, `SIG-*` | Chưa test; SEC-003/004 |
| AI role/branch/customer scope | runtime file, `AI-*` | Chưa test; SEC-005 |

