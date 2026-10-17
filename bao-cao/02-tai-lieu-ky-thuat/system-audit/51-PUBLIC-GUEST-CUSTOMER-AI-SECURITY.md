# Task 47H — Public, Guest, Customer, and AI Security Boundary

## Scope and contexts

This is a source and automated-test audit only. No DEV or production database was contacted.

| Context | Source-level contract |
|---|---|
| Anonymous | Active menu/catalog reads, public branch display, customer bootstrap, public reservation creation, and QR/table bootstrap only. |
| Guest QR | A `customer` JWT carrying `customerSessionType=guest`; current QR scanner itself supplies only a table GUID and does not establish a cryptographically verified table session. |
| Registered customer | `customer` JWT carrying `customerSessionType=registered`; own profile, loyalty, orders, reservations, and customer AI. |
| Employee/cashier/kitchen | Employee JWT role, branch claim where present; role-specific operational APIs and AI tools. |
| Manager/admin | Employee JWT role; manager must retain BranchId and admin is global where source defines it. |

## Public surface inventory

The WebAPI has 21 controllers and the controller-action inventory remains the 117-action source inventory documented in `48-ROLE-FUNCTION-MATRIX.md`. Public/anonymous-reachable groups are deliberately limited to the following.

| Endpoint group | Previous exposure | Final policy | Result |
|---|---|---|---|
| `GET /api/Branch` | Entire Branch entity, including bank/configuration fields | Public active branch display DTO only: id, name, address, phone, image | Hardened |
| `GET /api/Product` | Public products, including inactive unless caller filtered | Public/customer callers receive active products only | Hardened |
| `GET /api/Topping`, `GET /api/Promotion` | Public customer menu support | Public read; mutations remain admin-only | Intentional public |
| `GET /api/Table?branchId=...`, `GET /api/Table/{id}` | Customer/QR table bootstrap | Public table context only; current GUID QR trust gap remains | Ambiguous policy gap |
| `POST /api/Reservation` | Public reservation request | Pending-only for non-staff; staff/customer ownership rules remain | Intentional public |
| `GET /api/Customer/exists/{phone}` and `POST /api/Customer` | Login/registration bootstrap | Rate-limited bootstrap; existing customer update requires identity | Intentional public |
| `POST /api/Auth/customer-token` | Customer/guest token bootstrap | Separate from employee login; phone-only ownership proof is unresolved | Ambiguous policy gap |
| `POST /api/Ai/chat` | Anonymous callers were treated as customer | Authenticated JWT required; guest session denied | Hardened |

All employee, HR, financial, inventory, dashboard, settings, notification, insight, and management mutations remain protected by their controller/action authorization contracts.

## Customer and guest boundaries

Registered customer ownership is enforced in profile, loyalty-history, reservation and customer-order paths through JWT name/identifier checks. Guest sessions must not access registered profile, loyalty, HR, management, or AI. AI is denied for `customerSessionType=guest`; registered customer AI receives JWT-derived customer id and phone.

### Unresolved QR/customer policy decisions

1. QR scan accepts a table GUID from client-side QR data. Task 47H.1 verified that the current model has `QrCodeUrl` but no persisted `QrToken`, token generator, uniqueness rule, or server bootstrap endpoint. There is no signed QR/table claim or server-issued table session to distinguish a valid scan from an arbitrary table identifier.
2. Registered customer token creation follows the existing phone-based product contract and therefore has no possession proof. Adding password/OTP is explicitly outside this task.

These are not silently “fixed” because either remedy changes the customer authentication product model and QR deployment format. They require a separate policy/design task.

## AI policy and registered tools

`/api/Ai/chat` now requires authentication. Anonymous and guest QR callers are denied. Tool execution remains independently guarded by `AiPermissionService`; a manager with no BranchId is denied before any tool executes.

| Tool | Risk | Roles |
|---|---|---|
| get_revenue | Read | admin, manager |
| update_product_price | Write | admin |
| get_active_staff | Read | admin, manager |
| get_order_list | Read | admin, manager, employee, cashier, kitchen |
| get_best_sellers | Read | admin, manager, employee, cashier, kitchen |
| get_revenue_comparison | Read | admin, manager |
| get_business_summary | Read | admin, manager |
| get_financial_analysis | Read | admin, manager |
| employee_get_table_summary | Read | admin, manager, employee, cashier, kitchen |
| get_my_shift | Read | employee, cashier, kitchen |
| update_order_status | Write | admin, manager, employee, cashier, kitchen |
| customer_get_menu | Read | authenticated employee roles and registered customer |
| get_my_orders | Read | registered customer context |
| create_booking | Write | registered customer context |

Tool identifiers and model-generated arguments remain untrusted. Existing tools must use the supplied JWT context/branch or the application service boundary; direct arbitrary database access is not introduced.

## Findings and fixes

| Severity | Location | Previous behavior | Fix |
|---|---|---|---|
| High | `AiController.Chat` | Anonymous traffic was assigned `customer`, enabling customer tool discovery/execution paths | Controller now requires authentication; guest sessions are forbidden; customer context comes from claims. |
| High | `BranchController.GetBranches` | Public response serialized bank/account/tax/administrative fields | Active-only public DTO excludes internal fields. |
| Medium | `ProductController.GetProducts` | Unauthenticated/customer caller could retrieve inactive products | Public/customer query filters to active products. |
| High, unresolved policy | QR table and phone customer bootstrap | Client-controlled table GUID and phone identifier do not prove ownership | Deferred: needs signed QR/table-session and customer identity policy. |

## Regression coverage

Tests cover AI controller authorization metadata, guest AI denial, registered customer context propagation, employee JWT role precedence, and public branch DTO filtering. Existing customer ownership, manager AI branch, tool-permission, and QR/order tests remain in the suite.

## Runtime follow-up

Human validation must verify the authenticated Customer Web chat works for a registered customer, guest QR chat returns a safe denial/no UI path, public menu remains active-only, QR ordering bootstrap still loads its table/menu, and no protected API can be reached without a token.

## Task 47H.1A status

The approved QrToken schema foundation is pending a clean migration baseline and an explicitly verified PostgreSQL cryptographic backfill capability. No weak token backfill, schema change, database connection, or database mutation was performed. Guest QR remains an unresolved policy/security boundary until the foundation can be safely migrated and Task 47H.1B consumes it.
