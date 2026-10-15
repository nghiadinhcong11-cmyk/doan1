# Kiá»ƒm chá»©ng ká»¹ thuáº­t/runtime

NhĂ³m nĂ y khĂ´ng pháº£i checklist UI. DĂ¹ng khi cáº§n Network/HTTP client, log Ä‘Ă£ lá»c secret, nhiá»u phiĂªn, schema/migration hoáº·c cáº¥u hĂ¬nh runtime. LÆ°á»£t nĂ y khĂ´ng cháº¡y cĂ¡c kiá»ƒm tra Ä‘Ă³.

| ID | PhÆ°Æ¡ng phĂ¡p | Pháº¡m vi | Äiá»u kiá»‡n | Káº¿t quáº£ cáº§n lÆ°u | Tráº¡ng thĂ¡i |
|---|---|---|---|---|---|
| TECH-01 | API/HTTP client | auth, branch isolation, payment total, idempotency | API test riĂªng vĂ  token test Ä‘Ă£ cáº¥p | request Ä‘Ă£ che secret, status/body, timestamp | ChÆ°a test |
| TECH-02 | hai phiĂªn/Network | SignalR reconnect, order/payment Ä‘á»“ng thá»i | hai browser, dá»¯ liá»‡u riĂªng | event/status trÆ°á»›c-sau, log correlation | ChÆ°a test |
| TECH-03 | database read-only | constraint, transaction, migration/schema | database test riĂªng, quyá»n read-only | schema/query/log; khĂ´ng sá»­a DB trong checklist | ChÆ°a test |
| TECH-04 | log/config | error handling, secret redaction, backup/restore readiness | mĂ´i trÆ°á»ng Ä‘Æ°á»£c phĂ©p | file log Ä‘Ă£ lá»c vĂ  version/config fingerprint | ChÆ°a test |
| TECH-05 | build/typecheck | backend/frontend contract | dependency hiá»‡n cĂ³ | lá»‡nh, commit, output tĂ³m táº¯t | ChÆ°a test |

## Authentication, authorization and branch-isolation cases

These are technical/API or multi-session checks, not UI-only substitutes. Keep request/response evidence redacted and record the tested commit/environment.

| ID | PhÆ°Æ¡ng phĂ¡p | Pháº¡m vi | Äiá»u kiá»‡n vĂ  bÆ°á»›c chĂ­nh | Káº¿t quáº£ mong Ä‘á»£i | Tráº¡ng thĂ¡i | Minh chá»©ng/finding |
|---|---|---|---|---|---|---|
| AUTH-01 | API/HTTP client | Login and claims | Use approved accounts for admin, manager, employee, cashier, kitchen and customer; login through the supported mode; inspect only non-secret claim names/expiry | Accepted roles receive the expected token scope; unsupported mode/role is rejected; no secret is logged | ChÆ°a test | `SECURITY-AND-BRANCH-REVIEW.md` |
| AUTH-02 | API/HTTP client | Expired token | Use a token at/after expiry; call `/api/Auth/me` and one protected endpoint | Protected calls are rejected; no stale identity is accepted | ChÆ°a test | Auth source review |
| AUTH-03 | API/HTTP client | Deactivated employee/customer | Issue a token in an isolated environment, deactivate the identity through approved setup, call a protected endpoint | Token validation rejects the now-invalid identity; record status/body without secrets | ChÆ°a test | Auth source review |
| AUTH-04 | API/HTTP client | Wrong role and client-supplied identity | Send a protected request with a valid token while changing body/query `role`, `userId` or `branchId` to another value | Backend uses authenticated claims/resource checks, not client identity fields; unauthorized action is rejected | ChÆ°a test | Auth source review |
| BRANCH-01 | API/HTTP client | Manager branch read | Token from Branch A requests Branch B area/table/inventory/order/report resources by query or ID | Request is rejected or returns no Branch B data; record endpoint, status and redacted response | ChÆ°a test | SEC-001, SEC-002 |
| BRANCH-02 | API/HTTP client | Resource-ID substitution | With a Branch A staff token, replace an allowed order/table/employee/customer/resource ID with a Branch B ID | Resource is not disclosed or mutated; no branch-crossing side effect | ChÆ°a test | SEC-001, SEC-002 |
| BRANCH-03 | API/HTTP client | Customer profile and loyalty policy | Test manager/cashier/employee against a customer associated with another branch after the product policy is decided | Behavior matches the documented chain-wide or branch-scoped policy; do not mark PASS while policy is open | ChÆ°a test | SEC-001, SEC-002 |
| BRANCH-04 | API/HTTP client | Product availability scope | Two branch sessions: manager/kitchen changes the same product availability; observe API result and affected branch menu | Behavior matches explicit global-vs-branch policy; unrelated branch is not changed if policy is branch-scoped | ChÆ°a test | SEC-003 |
| SIG-01 | Two browser sessions/Network | KitchenHub group scope | Connect admin and non-admin staff sessions with signed branch claims; attempt a mismatched `branchId` query; observe group events | Non-admin cannot select another branch via query; events arrive only in authorized groups | ChÆ°a test | SignalR source review |
| SIG-02 | Two browser sessions/Network | Missing order branch fallback | In an approved isolated fixture with missing/invalid order branch, update request status and observe recipients/logs | Behavior matches fail-closed/global-admin policy; no unintended branch client receives the event | ChÆ°a test | SEC-004 |
| AI-01 | API/HTTP client | AI role/tool allowlist | Call Mock AI with each supported role and disallowed customer/guest tool intents | Tool list and execution follow `AiPermissionService`, role allowlist and risk level; no Gemini/OpenAI call is assumed | ChÆ°a test | SEC-005 |
| AI-02 | API/HTTP client | AI branch scope | Manager/employee tool requests for orders, revenue, tables and status use own vs another branch context | Other-branch data/mutation is rejected or absent; admin global behavior is separately recorded | ChÆ°a test | SEC-005 |
| AI-03 | API/HTTP client | Customer/guest ownership and confirmation | Registered customer requests own orders; guest requests personal orders/write tools; admin price change requires confirmation token | Customer sees only own scope; guest is denied personal/write tools; sensitive admin write needs confirmation | ChÆ°a test | COV-001, SEC-005 |

## Diff-review follow-up cases

| ID | Phương pháp | Phạm vi | Điều kiện và bước chính | Kết quả mong đợi | Trạng thái | Minh chứng/finding |
|---|---|---|---|---|---|---|
| AUTH-06 | API/HTTP client | Customer registration mass-assignment boundary | In an isolated environment, submit public registration payloads containing CustomerGroup, LoyaltyPoints, TotalSpending, TotalOrders, IsActive, Id and timestamps; repeat with an existing phone and a different authenticated customer context; use redacted response capture. | Only full name, phone and password affect registration; internal fields are ignored/server-controlled; duplicate handling does not overwrite or transfer the existing account. | Chưa test | SEC-006 |
| BRANCH-08 | API/HTTP client and database fixture | Open order after branch deactivation | Create an open order in Branch A, deactivate the branch through approved test setup, then test read, accept, status, payment, send-to-kitchen and delete as admin and branch staff. | Behavior matches an explicit recovery policy; no operation is marked PASS until the inactive-branch policy is decided and observed. | Chưa test | SEC-004, SEC-003 |
| SIG-04 | Two browser sessions/Network | Product availability realtime consumer | Change one product in Branch A and inspect connected kitchen, POS and customer-menu clients for Branch A/B, event payload branchId, the selected-branch UI, reconnect, and page reload. Repeat when the branch row is initially missing. | Kitchen may update from the event; POS/customer behavior must be recorded as live update or reload-only, with no cross-branch value. Reconnect/reload obtains server state; a missing row displays the current `Available` default, pending policy confirmation. | Chưa test | SEC-003 |

## Evidence requirements

For each case record: test ID, source commit, environment identifier, token role/branch without token value, request path and redacted payload, status/body summary, timestamp, and screenshot/log correlation where relevant. `ChÆ°a test` is the required initial state; adding a case or reading source does not close a finding.




## Policy implementation follow-up cases

| ID | Phương pháp | Phạm vi | Điều kiện và bước chính | Kết quả mong đợi | Trạng thái | Minh chứng/finding |
|---|---|---|---|---|---|---|
| AUTH-05 | Browser Network/API client | Guest profile and customer profile error handling | Guest QR/direct session opens `/profile`; inspect requests. Then use an approved customer session and simulate/observe API 401/403/error in isolated environment. | Guest sees login invitation without customer profile request or infinite loading; customer error ends loading and shows a recoverable state; no guest session/cart/QR context is cleared by opening profile. | Chưa test | COV-001 |
| BRANCH-05 | API/HTTP client | Order branch invariant | In an isolated fixture, attempt create/process/send-to-kitchen/payment/status for an order with missing or inactive branch; capture redacted status, log and admin notification. | Operation is rejected before business processing; no invalid branch payload is broadcast; valid branch behavior remains branch-scoped. | Chưa test | SEC-004 |
| BRANCH-06 | Two-branch API/HTTP client | Customer chain-wide policy and least-privilege response | Use admin/manager/employee/cashier/customer identities to lookup, update profile and request loyalty history for a chain-wide customer record. | Chain-wide lookup follows policy; operational staff receive only required fields and cannot use full profile mutation; customer self-ownership is enforced. | Chưa test | SEC-001, SEC-002 |
| BRANCH-07 | Two-branch API/HTTP client | Product availability after schema transition | Apply the approved migration only in a disposable test DB, seed two branches, mutate availability from manager/kitchen in Branch A, inspect Branch B/POS/customer menu. | Product catalog is shared but availability differs per branch; unauthorized branch mutation is rejected; do not mark PASS before migration is applied and observed. | Chưa test | SEC-003 |
| SIG-03 | Two browser sessions/Network | Branch availability realtime | Connect authorized kitchen/manager clients from two branches; mutate one branch product availability and observe event payload/groups. | Event contains the branch scope and reaches only authorized recipients for that branch plus intended global admin audience. | Chưa test | SEC-003 |

