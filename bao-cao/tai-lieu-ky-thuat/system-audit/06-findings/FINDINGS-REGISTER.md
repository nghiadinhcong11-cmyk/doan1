# Findings Register

This is the single register for audit findings. Other checklists should reference an existing ID instead of copying the same issue. A documentation discrepancy or missing evidence can be a finding; it is not automatically an application defect.

## Status and severity

`TrĂ¡ÂºÂ¡ng thÄ‚Â¡i xĂ¡Â»Â­ lÄ‚Â½` and `MĂ¡Â»Â©c Ă„â€˜Ă¡Â»â„¢ nghiÄ‚Âªm trĂ¡Â»Âng` are separate:

- TrĂ¡ÂºÂ¡ng thÄ‚Â¡i: `MĂ¡Â»Å¸`, `Ă„Âang xĂ¡Â»Â­ lÄ‚Â½`, `ChĂ¡Â»Â kiĂ¡Â»Æ’m tra lĂ¡ÂºÂ¡i`, `Ă„ÂÄ‚Â£ Ă„â€˜Ä‚Â³ng`, `Ă„ÂĂ†Â°Ă¡Â»Â£c chĂ¡ÂºÂ¥p nhĂ¡ÂºÂ­n cÄ‚Â³ Ă„â€˜iĂ¡Â»Âu kiĂ¡Â»â€¡n`.
- Severity: `Critical`, `High`, `Medium`, `Low`, `Observation`.
- Priority: `P0` (must resolve before acceptance), `P1` (should resolve), `P2` (can defer). Priority is scheduling, not severity.

## Finding template

| Field | Required content |
|---|---|
| ID | Stable identifier, e.g. `BASE-001` |
| Finding | Concise statement of the observed issue or uncertainty |
| Evidence | Exact path, command/result, version and environment; no secrets |
| Impact | What acceptance, security, data integrity or reproducibility could be affected |
| Severity | Critical/High/Medium/Low/Observation |
| Priority | P0/P1/P2 |
| Processing status | MĂ¡Â»Å¸/Ă„Âang xĂ¡Â»Â­ lÄ‚Â½/ChĂ¡Â»Â kiĂ¡Â»Æ’m tra lĂ¡ÂºÂ¡i/Ă„ÂÄ‚Â£ Ă„â€˜Ä‚Â³ng/ChĂ¡ÂºÂ¥p nhĂ¡ÂºÂ­n cÄ‚Â³ Ă„â€˜iĂ¡Â»Âu kiĂ¡Â»â€¡n |
| Remediation | Proposed owner/action, without changing code in this register |
| Retest condition | Concrete condition and method required to close |
| Retest result | Date, snapshot, method and evidence link |

## Findings

### BASE-001 Ă¢â‚¬â€ Working tree is not a reproducible clean snapshot

| Field | Value |
|---|---|
| Evidence | `git status --porcelain=v1 -uall` at the survey time recorded in [`REPOSITORY-SNAPSHOT.md`](../01-baseline/REPOSITORY-SNAPSHOT.md): 137 modified, 149 deleted, 19,474 untracked entries; untracked output includes dependency/build artefacts |
| Impact | Results cannot be reproduced from HEAD alone; source, generated files and pre-existing edits may be mixed |
| Severity | Medium |
| Priority | P0 |
| Processing status | MĂ¡Â»Å¸ |
| Remediation | Freeze a named review snapshot or clearly separate source changes before acceptance evidence is finalized |
| Retest condition | Re-run status on the agreed snapshot and record the exact inclusion/exclusion rules for generated files |

### DOC-001 Ă¢â‚¬â€ Migration count in current source differs from an existing state document

| Field | Value |
|---|---|
| Evidence | Static count under `services/api/src/Infrastructure/Persistence/Migrations/`: 49 non-designer migration files; latest is `20261014120000_AddNotificationReadState.cs`. [`CODEX-CURRENT-STATE.md`](../CODEX-CURRENT-STATE.md) records 43 and an older latest migration |
| Impact | Reviewers may use an outdated migration baseline or mistake documentation drift for database failure |
| Severity | Medium |
| Priority | P0 |
| Processing status | MĂ¡Â»Å¸ |
| Remediation | Reconcile the documentation against the agreed source snapshot and separately record actual applied-database history |
| Retest condition | On an authorized isolated database, compare EF-discovered migrations, migration history and model snapshot; record results without changing the database in an unapproved environment |

This finding records a verified documentation/source-count discrepancy only. It does not conclude that the database is broken, that all 49 migrations are applied, or that any migration should be removed.

### PAY-001 Ă¢â‚¬â€ Historical migration chain needs a cleanup decision

| Field | Value |
|---|---|
| Evidence | Static inspection of `ApplicationDbContextModelSnapshot.cs` found `Employee` properties `BasicSalary` and `EmployeeType`, but no `Payroll`, `PayrollAdjustment` or `PayrollSettings` entity block, payroll table mapping, payroll foreign key or payroll relationship. The current `ApplicationDbContext` exposes `DbSet<Employee>` and configures the employee-to-branch relationship. Historical payroll identifiers remain in earlier/later migration source and designer files, including `20260907060600`, `20260907155909` and `20260919075618`. `EmployeeType` and `BasicSalary` are intentionally retained as employee profile fields and are outside this finding. |
| Impact | The current snapshot does not itself preserve payroll entity/table metadata, but the historical migration chain still contains payroll operations and designer metadata. Blind deletion, reordering or snapshot rewriting could break replay/upgrade assumptions or unrelated module changes. |
| Severity | High |
| Priority | P0 |
| Processing status | MĂ¡Â»Å¸ |
| Remediation | Keep the current employee profile fields and current snapshot mappings. Separately review the complete migration chain and choose either preserving the historical chain or adopting a separately approved baseline strategy before any migration-source rewrite. |
| Retest condition | Static chain review followed by an approved isolated migration/model comparison; do not edit `__EFMigrationsHistory` or run against the configured database. |
| Retest result | ChĂ†Â°a kiĂ¡Â»Æ’m tra |

## Method tracking for open findings

| Finding | PhĂ†Â°Ă†Â¡ng phÄ‚Â¡p | TrĂ¡ÂºÂ¡ng thÄ‚Â¡i thĂ¡Â»Â±c hiĂ¡Â»â€¡n | KĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n | Minh chĂ¡Â»Â©ng |
|---|---|---|---|---|
| BASE-001 | Static Git inspection | Ă„ÂÄ‚Â£ xÄ‚Â¡c minh tĂ„Â©nh | ChĂ†Â°a kĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n vĂ¡Â»Â reproducibility | `REPOSITORY-SNAPSHOT.md` |
| BASE-001 | Technical snapshot freeze | ChĂ†Â°a kiĂ¡Â»Æ’m tra | ChĂ†Â°a kĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n | ChĂ†Â°a cÄ‚Â³ agreed clean/frozen snapshot |
| DOC-001 | Static source/document comparison | Ă„ÂÄ‚Â£ xÄ‚Â¡c minh tĂ„Â©nh | ChÄ‚Âªnh lĂ¡Â»â€¡ch tÄ‚Â i liĂ¡Â»â€¡u Ă„â€˜Ä‚Â£ xÄ‚Â¡c minh | `SOURCE-OF-TRUTH.md`, current migration directory, `CODEX-CURRENT-STATE.md` |
| DOC-001 | Database migration-history comparison | ChĂ†Â°a kiĂ¡Â»Æ’m tra | ChĂ†Â°a kĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n vĂ¡Â»Â applied schema | ChĂ†Â°a kĂ¡ÂºÂ¿t nĂ¡Â»â€˜i database theo giĂ¡Â»â€ºi hĂ¡ÂºÂ¡n lĂ†Â°Ă¡Â»Â£t nÄ‚Â y |

| PAY-001 | Static source/migration-chain inspection | Ă„ÂÄ‚Â£ xÄ‚Â¡c minh tĂ„Â©nh | Employee profile fields are retained; historical migration metadata remains under separate review | canonical migration/designer files; `ApplicationDbContextModelSnapshot.cs` |
| PAY-001 | Isolated migration/database comparison | ChĂ†Â°a kiĂ¡Â»Æ’m tra | ChĂ†Â°a kĂ¡ÂºÂ¿t luĂ¡ÂºÂ­n | ChĂ†Â°a cÄ‚Â³ mÄ‚Â´i trĂ†Â°Ă¡Â»Âng Ă„â€˜Ă†Â°Ă¡Â»Â£c phÄ‚Âª duyĂ¡Â»â€¡t |

No item in this register is marked `PASS`.

## Implementation follow-up — 2026-10-15

| Finding | Updated position | Status | Retest condition |
|---|---|---|---|
| SEC-003 | Branch-aware Product reads now use `ProductBranchAvailabilities`, with explicit `Available` fallback when a branch row is absent; the legacy product-level value is not copied into a branch-scoped response. Kitchen consumes branch-labeled availability events and refetches after reconnect. | Đã sửa source, chờ kiểm chứng runtime/migration | Apply migration in an isolated database; test existing/new product and branch, inactive branch policy, POS/customer/AI reads and SignalR recipients. |
| SEC-004 | Kitchen request status validates status, request/order, active branch and transition before mutation. Invalid transitions return 400 from `OrderController`; no business event is reached on rejected input. | Đã sửa source, chờ kiểm chứng runtime | Test malformed/orphan/inactive-branch requests, repeated completion and concurrent status commands. |
| SEC-006 | Public registration uses `CustomerRegistrationDto` allowlisting `FullName`, `PhoneNumber` and `Password`; server owns group, loyalty, spending, order counts, activity and timestamps. | Đã sửa source, chờ kiểm chứng runtime | Submit extra internal fields and duplicate-phone requests in an isolated API environment. |

These findings are not closed by source inspection or build success. `EmployeeType` and `BasicSalary` remain employee profile fields. The availability migration has not been applied to any database.

### Migration/tooling and frontend availability clarification — 2026-10-15

- `dotnet-ef 8.0.31 migrations list --no-connect` discovered `20261015051548_AddProductBranchAvailability` against the rebuilt isolated assembly. Its explicit `DbContext`/`Migration` attributes are effective.
- `has-pending-model-changes` returned no pending model changes, and an offline script was generated successfully. No database was contacted.
- The missing conventional designer does not block discovery, offline script generation or migration execution because `Up`/`Down` and direct metadata are present. It does mean there is no separate generated `BuildTargetModel` partial for this migration; the current snapshot is the target used by the model comparison. No duplicate migration was created.
- `ProductController`/`ProductService` currently use `Available` when a branch availability row is missing. This remains an implementation default pending user confirmation.
- Only `KitchenPage` consumes `ProductAvailabilityChanged`. POS and DigitalMenu refresh product data through existing fetch/reload/branch-change flows and do not subscribe to this event. Realtime coverage for those clients remains open.

## Policy/source update â€” 2026-10-15

This addendum supersedes earlier policy-uncertainty wording where the user has now made a decision. It does not close any finding without runtime evidence.

| Finding | Updated static position | Processing status | Required recheck |
|---|---|---|---|
| SEC-001 | Customer records are chain-wide. `CustomerController` now returns a minimal lookup shape to operational staff; admin/manager and customer self-service retain the fuller DTO. | Chá» kiá»ƒm tra láº¡i â€” source changed, runtime not verified | Verify staff lookup fields, role restrictions and redacted response in isolated API tests. |
| SEC-002 | Chain-wide customer/loyalty policy is selected. `GetLoyaltyHistory` is limited to admin/manager/customer; profile mutation remains admin/manager/customer with customer self-ownership. | Chá» kiá»ƒm tra láº¡i â€” source changed, runtime not verified | Verify each role and customer ownership with authenticated test identities. |
| SEC-003 | Catalog remains shared, while availability is represented by `ProductBranchAvailability` keyed by product and branch. Controller, POS, kitchen and customer menu now pass/resolve branch context. | Chá» kiá»ƒm tra láº¡i â€” source changed; migration not applied | Apply and verify the new migration in an approved isolated database, then test two branches and realtime recipients. |
| SEC-004 | Order creation, acceptance, payment, status processing and kitchen submission now require an active branch in the application path. Invalid-branch realtime notification is suppressed; kitchen status records an admin notification/log instead of global business broadcast. | Chá» kiá»ƒm tra láº¡i â€” source changed, runtime/data verification not performed | Exercise malformed/orphan order paths in isolated runtime and inspect logs/notifications. |
| SEC-005 | No broad refactor was made. Existing AI tools continue to receive context-derived branch scope; no current bypass was proven statically. | Má»Ÿ | Continue per-tool negative tests and only centralize validation if a concrete bypass is found. |
| COV-001 | Profile policy is fixed: customer login is required; guest QR flows remain available. Reservation guest policy remains open and is not inferred from route rendering. | Chá» kiá»ƒm tra láº¡i | Test guest profile invitation/direct route, preserved QR context, and separately obtain a decision for guest reservation. |

`EmployeeType` and `BasicSalary` remain employee profile fields and are not a Payroll finding. The availability migration source is intentionally not applied to any database in this audit step.

### SEC-001 â€” Authenticated staff customer lookup has no explicit branch/privacy boundary

| Field | Value |
|---|---|
| Evidence | `services/api/src/WebAPI/Controllers/CustomerController.cs`, `GetByPhone(string phoneNumber)`: `[Authorize]`; customer role is restricted to its own phone, while authenticated staff may return `MapToDto(customer)` for any phone. The inspected customer path has no branch predicate. |
| Impact | A staff token may view another customer's profile. This may be intended for a chain-wide POS directory, but branch/privacy scope is not explicit. |
| Severity | Medium |
| Priority | P1 |
| Confidence | Static behavior confirmed; policy impact unresolved |
| Processing status | Má»Ÿ |
| Remediation | Decide chain-wide, branch-scoped or role-limited customer lookup, then enforce and test the chosen policy. |
| Retest condition | Use two branch staff tokens and customer records in an isolated test environment; request another branch/customer phone and record status/body with sensitive values redacted. |
| Retest result | ChÆ°a kiá»ƒm tra |

### SEC-002 â€” Customer loyalty/profile administrative scope is not represented by a branch key

| Field | Value |
|---|---|
| Evidence | `CustomerController.GetLoyaltyHistory(Guid id)` allows admin/manager/cashier/employee/customer and checks self-ownership only for customer role. `UpdateProfile(string phoneNumber, CustomerProfileUpdateDto)` allows admin/manager/customer and checks self-ownership only for customer role. The inspected customer path does not show a `BranchId` discriminator. |
| Impact | A manager or operational staff member may access or update a customer outside an intended branch boundary if policy is branch-scoped. The source cannot distinguish this from an intentional chain-wide CRM policy. |
| Severity | Medium |
| Priority | P1 |
| Confidence | Static authorization shape confirmed; policy and runtime data scope unresolved |
| Processing status | Má»Ÿ |
| Remediation | Decide and document customer ownership; if branch-scoped, add an authoritative ownership relation/check before acceptance. |
| Retest condition | Test manager/staff access to another branch customer ID/profile in an isolated environment after policy is decided. |
| Retest result | ChÆ°a kiá»ƒm tra |

### SEC-003 â€” Product availability mutation/broadcast is global while manager/kitchen are branch-scoped

| Field | Value |
|---|---|
| Evidence | `services/api/src/WebAPI/Controllers/ProductController.cs`, `UpdateAvailability`: `[Authorize(Roles = "admin,manager,kitchen")]`; loads Product by ID without branch; updates `AvailabilityStatus`; broadcasts `ProductAvailabilityChanged` via `Clients.All`. `Product` has no `BranchId` in the inspected entity. |
| Impact | If availability is branch-specific, a manager/kitchen user can change and notify every branch's menu state. If catalog state is intentionally global, behavior is expected and needs documentation/test evidence. |
| Severity | High |
| Priority | P0 |
| Confidence | Global source behavior confirmed; defect classification depends on policy |
| Processing status | Má»Ÿ |
| Remediation | Decide global-vs-branch availability. For branch scope, add authoritative branch-specific enforcement; for global scope, document permission and event scope. |
| Retest condition | Two-branch runtime test with manager/kitchen tokens, product ID, API response and SignalR recipients in an isolated environment. |
| Retest result | ChÆ°a kiá»ƒm tra |

### SEC-004 â€” Missing order branch uses a global kitchen-admin notification fallback

| Field | Value |
|---|---|
| Evidence | `services/api/src/Application/Services/KitchenService.cs`, `UpdateRequestStatusAsync`: if joined order branch is missing, calls `NotifyRequestStatusUpdatedAsync(requestId, status)`; `KitchenNotifier` sends that overload to global `kitchen-admin`. `KitchenHub` allows only admin to join that group. |
| Impact | Orphaned/malformed order data can deliver a status event outside a branch-specific group. Current source limits the group to admins, but fail-open behavior weakens isolation and may expose identifiers/status to all admins. |
| Severity | Medium |
| Priority | P1 |
| Confidence | Static fallback confirmed; occurrence and sensitivity require runtime/data-integrity evidence |
| Processing status | Má»Ÿ |
| Remediation | Decide whether missing branch must fail closed, be logged/suppressed, or intentionally notify global admins. |
| Retest condition | Isolated test with missing/invalid branch relation; observe event recipients and logs without changing the configured database. |
| Retest result | ChÆ°a kiá»ƒm tra |

### SEC-005 â€” AI branch isolation is not centralized in the shared validator

| Field | Value |
|---|---|
| Evidence | `services/api/src/AI/Utils/AiToolValidator.cs` defines `ValidateBranchIsolation`, but inspected tools have no call site. Current tools usually pass `AiUserContext.BranchId` to services (`GetOrderListTool`, dashboard tools, `GetTableSummaryTool`, `UpdateOrderStatusTool`). |
| Impact | A future tool accepting a branch argument could omit the shared check and rely on inconsistent service behavior. Current tools are not proven bypassable by this observation alone. |
| Severity | Low |
| Priority | P2 |
| Confidence | Static call-site gap confirmed; current exploit not demonstrated |
| Processing status | Má»Ÿ |
| Remediation | Keep branch enforcement at service boundaries and/or require consistent tool argument validation for every new tool. |
| Retest condition | Enumerate tool arguments and execute role/branch negative tests in an isolated runtime environment. |
| Retest result | ChÆ°a kiá»ƒm tra |

## Method tracking for security findings

| Finding | PhÆ°Æ¡ng phĂ¡p | Tráº¡ng thĂ¡i thá»±c hiá»‡n | Káº¿t luáº­n | Minh chá»©ng |
|---|---|---|---|---|
| SEC-001 | Static controller/entity review | ÄĂ£ kiá»ƒm tra tÄ©nh | Staff arbitrary-phone lookup is source-visible; intended scope unresolved | `CustomerController.GetByPhone` |
| SEC-001 | API cross-branch/resource test | ChÆ°a kiá»ƒm tra | ChÆ°a káº¿t luáº­n runtime | `ba-test-frontend/ky-thuat-runtime/README.md` |
| SEC-002 | Static controller/model review | ÄĂ£ kiá»ƒm tra tÄ©nh | Customer branch ownership not represented in inspected path | `CustomerController`, Customer entity usage |
| SEC-002 | Two-branch role test | ChÆ°a kiá»ƒm tra | ChÆ°a káº¿t luáº­n runtime | `BRANCH-*` |
| SEC-003 | Static controller/entity/hub review | ÄĂ£ kiá»ƒm tra tÄ©nh | Availability mutation and broadcast are global in source | `ProductController`, `Product.cs` |
| SEC-003 | Two-branch API/SignalR test | ChÆ°a kiá»ƒm tra | Depends on product policy | `SIG-*` |
| SEC-004 | Static service/hub review | ÄĂ£ kiá»ƒm tra tÄ©nh | Missing-branch fallback reaches global admin group | `KitchenService`, `KitchenNotifier`, `KitchenHub` |
| SEC-004 | Orphan-data SignalR test | ChÆ°a kiá»ƒm tra | ChÆ°a káº¿t luáº­n runtime | `SIG-*` |
| SEC-005 | Static AI tool call-site review | ÄĂ£ kiá»ƒm tra tÄ©nh | Shared validator is not centralized/called by tools | `AiToolValidator`, `AI/Tools` |
| SEC-005 | AI role/branch negative tests | ChÆ°a kiá»ƒm tra | ChÆ°a káº¿t luáº­n runtime | `AI-*` |


### PAY-001 verification addendum â€” snapshot comparison

Static comparison completed: `ApplicationDbContextModelSnapshot.cs` contains `EmployeeType` and `BasicSalary` under `Employee`, and contains no `Payroll`, `PayrollAdjustment` or `PayrollSettings` entity blocks, payroll table mappings, foreign keys or relationships. `ApplicationDbContext.cs` contains `DbSet<Employee>` and the employee-to-branch configuration; current Domain/Application/WebAPI source has no active payroll model. Historical migration source/designer references remain separately. This addendum is static evidence only; no migration or database was changed.
### COV-001 â€” Guest policy for direct customer routes is not established by source

| Field | Value |
|---|---|
| Evidence | `apps/customer-web/src/App.tsx` declares `/reservation` and `/profile` in the shared Customer Web shell. The shell renders `CustomerLogin` when neither customer nor guest/QR mode is active; in guest/QR mode it renders both routes. No separate guest route guard is visible in the inspected source. |
| Impact | A route declaration alone does not establish whether a QR guest may view or mutate reservation/profile data. Manual testing could otherwise mistake rendered UI for an authorization decision. |
| Severity | Medium |
| Priority | P1 |
| Processing status | Má»Ÿ |
| Remediation | Decide the intended guest policy, then verify API authorization and direct-route behavior in an isolated runtime environment. |
| Retest condition | Test unauthenticated, guest QR and authenticated customer sessions against both routes; record redirect/API status and data scope without using production data. |
| Retest result | ChÆ°a kiá»ƒm tra |




## Diff-review addendum — 2026-10-15

### SEC-003 residual review — availability fallback and realtime consumer

| Field | Value |
|---|---|
| Evidence | Product GET/ProductService overlays a branch row only when one exists; otherwise the global Product.AvailabilityStatus remains visible. The migration seeds existing product/branch pairs, but Product creation and Branch creation do not create rows. No frontend ProductAvailabilityChanged handler was found; the producer sends a branch-labeled payload to a global admin group plus branch groups. |
| Conclusion | The branch model and mutation scope are present in source, but the fallback is not a neutral default and can expose global availability in a branch with a missing row. End-to-end realtime synchronization is not demonstrated. |
| Severity | Medium |
| Priority | P1 |
| Processing status | Chờ kiểm tra lại |
| Retest condition | In an isolated database, test pre-migration, post-seed, new product, new branch, inactive branch and concurrent update cases; observe POS/customer/kitchen/AI and SignalR behavior. |

### SEC-004 residual review — kitchen status mutation ordering

| Field | Value |
|---|---|
| Evidence | KitchenService.UpdateRequestStatusAsync updates OrderRequest.Status (or executes the completed transition) before joining the request to Order and validating an active BranchId. Invalid branch then suppresses realtime and creates an admin notification/log. |
| Conclusion | The invalid payload broadcast is suppressed, but the invalid-branch request status can still be mutated before validation. This is a concrete ordering gap, not merely a missing shared validator. |
| Severity | Medium |
| Priority | P1 |
| Processing status | Mở — source review found a residual issue; no code changed in this review |
| Retest condition | After moving validation before mutation, test malformed/orphan request status and confirm no DB status change or business event occurs. |

### SEC-005 classification clarification

No current AI branch-scope bypass was demonstrated. AiToolValidator lacking call sites is retained as a defense-in-depth/maintainability recommendation, not a confirmed authorization vulnerability.

### SEC-006 — Public customer registration binds the domain entity and leaves CustomerGroup client-controlled

| Field | Value |
|---|---|
| Evidence | services/api/src/WebAPI/Controllers/CustomerController.cs, CreateOrUpdate(Customer customer): the anonymous new-record path resets ID, activity, loyalty, spending and timestamps, but does not reset CustomerGroup; the action binds the full domain entity rather than an allowlisted registration DTO. |
| Impact | An unauthenticated registration request can set a customer rank/group value outside the intended registration fields. Other internal fields are partly overwritten, so the exact exploitable field set is narrower than unrestricted entity binding, but the boundary is unsafe. |
| Severity | Medium |
| Priority | P1 |
| Confidence | Static behavior confirmed; runtime request test not run |
| Processing status | Mở |
| Remediation | Introduce/use a registration DTO containing only approved public fields and set CustomerGroup/internal fields server-side. Preserve the separate walk-in DTO. |
| Retest condition | Submit redacted registration requests containing internal fields in an isolated environment; verify ignored fields and normal customer token/profile behavior. |
| Retest result | Chưa kiểm tra |

