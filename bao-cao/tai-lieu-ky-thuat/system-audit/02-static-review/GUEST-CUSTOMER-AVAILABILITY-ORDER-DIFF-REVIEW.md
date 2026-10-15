# Static diff review: guest profile, customer permissions, branch availability and order branch validation

## 1. Review boundary and snapshot

| Item | Result |
|---|---|
| HEAD | `4abeb1bf2e646df837745713a495b164eed60488` on `main` |
| Working tree | Dirty; the review separates files in the requested feature diff from existing audit/context/checklist changes |
| Runtime | Not run: no API, browser, database, migration apply, seed, SignalR or business transaction |
| Code changes | Not made in this review |
| Protected decisions | MockAiProvider, EmployeeType and BasicSalary retained |
| Reference material | `SECURITY-AND-BRANCH-REVIEW.md`, findings register, `chatgpt-context/`, current source and current diff |

The current uncommitted code diff includes the guest profile/session changes, customer controller changes, product availability model/controller/frontend changes and order/kitchen branch validation. Other modified audit/checklist/context files are not treated as application evidence for this review.

## 2. Customer permissions and ownership

| Endpoint/action | Role | Ownership/scope | Fields read/written | Evidence | Open point |
|---|---|---|---|---|---|
| `GET /api/Customer` | admin/manager/employee/cashier | Chain-wide list; customer self is not relevant | Admin/manager receive `MapToDto`; employee/cashier receive `Id`, name, phone, loyalty points, group | `CustomerController.GetCustomers`, `MapForCaller` | Runtime response minimization and whether a full chain-wide list is operationally necessary |
| `GET /api/Customer/{phone}` | customer | Phone must equal `User.Identity.Name` | Customer gets full DTO | `CustomerController.GetByPhone` | Static assumption that JWT `Name` is the normalized phone; runtime identity test pending |
| `GET /api/Customer/{phone}` | admin/manager | Chain-wide | Full DTO | `CustomerController.GetByPhone`, `IsManagementStaff` | Runtime role/response test pending |
| `GET /api/Customer/{phone}` | employee/cashier/kitchen/other authenticated staff | Chain-wide lookup | Minimal lookup DTO; no email/address/password/spending/orders | `MapForCaller` | Verify exact business minimum and role matrix at runtime |
| `POST /api/Customer` existing record | customer | `NameIdentifier` must equal customer ID | Only selected profile fields are copied; loyalty/spending/group are not copied | `CustomerController.CreateOrUpdate` | Request still binds the full `Customer` entity |
| `POST /api/Customer` new record | anonymous or authenticated | Public registration path; guest token rejected | Entity binding; server resets ID, totals, loyalty, activity and timestamps, but does not explicitly reset `CustomerGroup` | `CustomerController.CreateOrUpdate(Customer customer)` | **SEC-006:** client-supplied `CustomerGroup` can become a rank/group value; replace entity binding with a registration DTO before acceptance |
| `POST /api/Customer/walk-in` | admin/manager/employee/cashier | New chain-wide customer record | `WalkInMemberCreateDto` accepts name/phone/email; server initializes internal counters | `CustomerController.CreateWalkInMember` | Runtime response minimization and duplicate race test pending |
| `PATCH /api/Customer/{phone}/profile` | customer | `NameIdentifier` must equal target customer ID | `CustomerProfileUpdateDto`: name, email, address, gender, birthday, new password | `CustomerController.UpdateProfile`, DTO definition | Static protection is present; runtime ID/phone substitution test pending |
| `PATCH /api/Customer/{phone}/profile` | admin/manager | Chain-wide management scope | Same profile DTO; does not expose loyalty/spending/group/role fields | `[Authorize]`, `MapToDto`, DTO definition | Business audit should confirm manager may edit all chain-wide profiles |
| `GET /api/Customer/{id}/loyalty-history` | customer | `NameIdentifier` must equal target customer ID | History read | `CustomerController.GetLoyaltyHistory` | Runtime ownership test pending |
| `GET /api/Customer/{id}/loyalty-history` | admin/manager | Chain-wide | History read | Role attribute and controller | Runtime role test pending |
| AI `get_my_orders` | customer | AI context derives customer ID/phone from JWT; tool has no client parameters | Own recent orders | `AiController`, `GetMyOrderTool`, `OrderService.GetOrdersByCustomerAsync` | Runtime token/context test pending; email-or-phone query behavior should be confirmed if email is ever populated |

### Customer conclusion

The profile update DTO does not expose loyalty points, spending, group, role or other internal fields, and the customer self-service checks use token claims. The concrete static gap is the public `POST /api/Customer` new-record path accepting the domain `Customer` entity and leaving `CustomerGroup` client-controlled. This is recorded as `SEC-006`; no code was changed in this review.

## 3. Availability-by-branch review

### Model and authorization

- `ProductBranchAvailability` uses composite key `(ProductId, BranchId)`, required `Status` max length 32 and required `UpdatedAtUtc`.
- Product and branch foreign keys use cascade delete. This removes state when either parent is physically deleted; branch deactivation is a soft state and retains rows.
- `ProductController.UpdateAvailability` resolves branch from the signed staff claim. Manager/kitchen cannot replace it with another query branch. Admin must supply an explicit branch ID.
- The mutation checks the branch exists and is active before writing.
- `ProductController.GetProducts` resolves staff/guest branch context but does not independently verify that an arbitrary public/customer `branchId` is active.
- `ProductService.GetAllProductsAsync` overlays a branch row when found. If no row exists, it leaves the global `Product.AvailabilityStatus` unchanged.

### Consumers and realtime

| Consumer | Branch behavior | Static result |
|---|---|---|
| POS | Sends `selectedBranchId` to product GET | Branch-aware read; runtime pending |
| Kitchen | Sends selected branch to GET/PATCH | Branch-aware mutation/read; runtime pending |
| Customer DigitalMenu | Sends selected/QR branch where available | Branch-aware when context exists; no-branch path can retain global fallback |
| AI `get_menu` | Passes `AiUserContext.BranchId` to `ProductService` | Uses context when present; registered customer context may have no branch |
| SignalR producer | Sends `branchId` and groups to `kitchen-admin` plus branch kitchen/manager groups | Payload/group is branch-labeled, but admin group is global by design |
| SignalR consumer | No `ProductAvailabilityChanged` handler found in the inspected frontend | Availability realtime update is not proven; kitchen currently refreshes product data separately |

### Residual risks and lifecycle behavior

1. Missing availability rows fall back to global `Products.AvailabilityStatus`. This is not a neutral “Available” default: a legacy global `OutOfStock` value can affect a branch with no row, and a newly created product or branch has no explicit branch state until seeded/initialized.
2. The migration seeds all products crossed with all branches, including inactive branches. This is structurally complete for existing rows but its inactive-branch policy is not explicitly defined.
3. Invalid legacy status strings are copied unchanged; consumers treat every value other than `Available` as unavailable. Data normalization is not present in the migration.
4. Concurrent updates to the same product/branch row have a composite-key uniqueness boundary but no concurrency token or compare-and-swap rule. Last-write behavior and duplicate-insert handling require runtime verification.
5. No frontend consumer of `ProductAvailabilityChanged` was found, so branch-labeled events do not currently demonstrate end-to-end realtime synchronization.

These points extend `SEC-003`; they are not separate duplicate findings.

## 4. Migration transition review

| Area | Static conclusion | Not established without schema/data test |
|---|---|---|
| Up | Creates `ProductBranchAvailabilities`, composite PK, two cascade FKs and `(BranchId, Status)` index; then cross-joins existing Products and Branches | Actual target schema, applied migration order and row counts |
| Down | Drops the new table | Safety against an already-used production migration and rollback data loss |
| Seed | `COALESCE(NULLIF(AvailabilityStatus, ''), 'Available')`, one row per existing product/branch pair, `ON CONFLICT DO NOTHING` | Whether every source product/branch row exists and whether legacy values are valid |
| Model/snapshot | Current `ApplicationDbContext` and snapshot contain the new entity, key, index and relationships | The migration source now carries explicit `DbContext`/`Migration` metadata; no generated `BuildTargetModel` designer was added. Controlled EF tooling/schema review is still required before acceptance |
| Types | Model and migration use `uuid`, `character varying(32)`, required string and `timestamp with time zone` | Actual PostgreSQL schema/provider compatibility |
| Inactive branches | Seed includes them; GET does not reject every public/customer inactive branch ID | Product/branch lifecycle policy |

The current code can run before the migration only where it does not query the new set; branch-aware Product GET/PATCH and AI branch menu paths require the table/model schema. Running the new code against a database without the table is expected to fail. No database was connected or changed.

## 5. Guest profile and login review

- Initial `App` state recognizes `customerGuestMode`, QR/direct guest flags and guest token.
- `CustomerProfile` returns from its loading effect for guest mode and does not call customer profile, loyalty, order or reservation APIs.
- Registered customer profile fetch uses the stored customer token and all loading/error paths observed end in `finally` or explicit state termination.
- The global fetch wrapper still clears guest session keys on a guest 401. This is appropriate for an expired guest token but must be verified against the requirement to preserve context when merely opening profile; the profile path itself makes no request.
- Login request changes only the UI state; it does not clear guest session/cart/QR context until successful login. Successful login/logout explicitly changes session type.
- Direct `/profile` and menu navigation are represented in `KQR-04`, `KQR-07`, `KQR-08` and `AUTH-05`; all remain `Chưa test`.
- Guest reservation policy remains open and was not changed.

No static redirect/retry loop was found in the reviewed profile path. Browser evidence is still required.

## 6. Order branch validation review

| Path | Validation observed | Mutation/event ordering | Residual question |
|---|---|---|---|
| Create | Controller resolves guest branch from signed context, checks active branch/table, checks staff branch access; service checks active branch again | Branch checks precede order persistence and pending notification | Runtime manipulated branch/resource test |
| Accept | Controller derives staff branch claim; service checks order branch active before table claim/conditional update | Validation precedes transaction mutation | What admin should do with open orders in an inactive branch |
| Update/status | Controller checks resource branch; service checks current and target active branches before status/loyalty/save | Validation precedes status mutation in this path | Branch move and terminal/loyalty semantics need runtime/business confirmation |
| Payment | Controller checks resource branch and persisted total; service checks active branch before payment updates | Validation precedes payment mutation | Existing open orders in deactivated branch become non-payable until policy/recovery is defined |
| Send to kitchen | Controller checks resource branch; `KitchenService` checks active branch | Validation precedes request creation | Same inactive-branch recovery question |
| Kitchen request status | Joins request to order and checks active branch before realtime broadcast | **Status is mutated before this branch check** in `KitchenService.UpdateRequestStatusAsync`; invalid branch is logged/notified after the status write | This is a concrete ordering gap for `SEC-004`; fix should validate before status mutation |

The source therefore fails closed for several order mutations, but not uniformly for kitchen request status. “Branch exists/active” and “actor owns branch” are separate: controller/resource checks enforce actor scope, while service checks enforce active branch state.

## 7. Required follow-up before acceptance

1. Replace public `Customer` entity binding with a registration DTO and explicitly whitelist allowed fields; this addresses `SEC-006`.
2. Decide whether missing availability rows may use global fallback. For the approved branch policy, prefer an explicit initialization/default rule and reject or handle missing branch context deliberately.
3. Decide whether inactive branches are seeded/readable and how open orders are recovered after deactivation.
4. Move kitchen request branch validation before status mutation, then verify no invalid realtime payload is emitted.
5. Regenerate/review migration metadata in a controlled EF tooling run; do not apply it to the configured database in this audit.
6. Add/verify a `ProductAvailabilityChanged` consumer or explicitly document polling-only behavior.

No item above is marked PASS. Build success from the previous implementation turn is not runtime or schema evidence.

## Implementation follow-up — 2026-10-15

The following source changes address the concrete gaps above. They remain open for runtime or isolated-database verification.

| Finding | Source change | Current conclusion | Remaining verification |
|---|---|---|---|
| SEC-006 | `CustomerController.CreateOrUpdate(CustomerRegistrationDto)` now accepts only `fullName`, `phoneNumber` and `password`; existing duplicate records are not overwritten with client-controlled internal fields. `CustomerLogin.tsx` sends the allowlisted contract. | Source mass-assignment path removed. | Registration with extra fields, duplicate phone and account ownership in an isolated API test. |
| SEC-004 | `KitchenService.UpdateRequestStatusAsync` validates status, request/order existence, active branch and transition before any status write. `OrderController` maps invalid transitions to 400. | Source ordering gap corrected; notification/event code is reached only after validation and mutation. | Orphan/inactive-branch request, repeated completion and concurrent commands in isolated runtime. |
| SEC-003 | Branch-aware reads in `ProductController` and `ProductService` use the branch row, or explicit `Available` when the row is missing. They no longer copy legacy `Products.AvailabilityStatus` into a branch-scoped response. `KitchenPage` consumes `ProductAvailabilityChanged` only for its selected branch and refetches after reconnect. | Branch read/write rule is consistent for the inspected branch-aware paths. | Apply migration in an isolated database, test new product/branch and POS/customer/AI consumers, and verify SignalR recipients. |

### Availability and migration clarification

`Products.AvailabilityStatus` remains transition data for non-branch/global legacy paths and migration seeding. It is not the operational value for a branch-aware read when `ProductBranchAvailabilities` has no row. The current source does not auto-create rows for every new product or branch; the explicit branch-read default is `Available`, while an update creates the missing `(ProductId, BranchId)` row.

`20261015051548_AddProductBranchAvailability.cs` retains the existing cross-product/branch seed and does not normalize non-empty legacy strings. It now has explicit `DbContext` and `Migration` metadata. No database was connected or changed; schema application and actual legacy-value validity remain pending. The migration seeds inactive branches too, and the lifecycle policy for inactive branches is still a decision item.

### EF tooling reconciliation

Using an isolated rebuilt assembly (`.artifacts/ef/`) and `dotnet-ef 8.0.31` without database access:

- `migrations list --no-connect` discovered `20261015051548_AddProductBranchAvailability`.
- `migrations has-pending-model-changes` returned `No changes have been made to the model since the last migration.` This compares the current model to the source snapshot; it is not a database check.
- Offline `migrations script` generated the expected `CREATE TABLE`, composite primary key, cascade foreign keys, branch/status index, cross-join seed, migration-history insert and transaction boundaries.

The migration is therefore discoverable and executable from its explicit `DbContext`/`Migration` metadata plus `Up`/`Down`; the missing conventional `.Designer.cs` did not block these operations. It does mean this migration does not carry a separate generated `BuildTargetModel` partial like most older migrations in this repository. The current snapshot is the model target used by the pending-model check, so no duplicate migration or snapshot rewrite was made. A future migration should be scaffolded/reviewed with the project’s normal EF tooling before this migration is applied.

### Availability consumer boundary

The `Available` value for a missing branch row is current source behavior, chosen to preserve the existing product-creation default; it is not yet a user-confirmed business policy. `KitchenPage` consumes `ProductAvailabilityChanged` for its selected branch and refetches on reconnect. The inspected POS page listens to `RequestStatusUpdated`, not `ProductAvailabilityChanged`; its product list refreshes through existing fetch/reload flows. `DigitalMenu` fetches products during bootstrap/branch changes/manual refresh and has no availability SignalR consumer. Therefore kitchen realtime does not represent POS/customer live coverage; SIG-04/BRANCH-07 remain open for reload/live behavior.

### Inactive branch with open orders

Static source shows create, accept, payment, status processing and send-to-kitchen paths require an active branch. A kitchen request status update is now also rejected before mutation. Existing orders can therefore remain open but become non-payable/non-processable while their branch is inactive. The repository does not define a recovery policy for reactivation, administrative completion, cancellation or migration of those orders; this must be decided before runtime acceptance. No broad bypass was added.
