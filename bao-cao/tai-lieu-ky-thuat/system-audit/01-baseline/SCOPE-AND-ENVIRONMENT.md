# Scope and Environment

## Audit scope

The planned audit covers:

- architecture and code quality at the boundaries relevant to acceptance;
- ASP.NET Core API, validation, error handling, DTOs and OpenAPI contract;
- React/Vite admin and customer applications, routes, role UX and API consumers;
- JWT authentication, authorization and multi-branch isolation;
- PostgreSQL/EF Core entities, relationships, constraints, migrations, transactions and idempotency;
- order state, pricing, VAT, service fee, payment, invoice, loyalty and inventory;
- POS → kitchen → cashier → invoice flows and SignalR `/kitchenHub`;
- guest QR/customer sessions and QR scope;
- retry, timeout, reconnect and concurrent operations;
- build/configuration/HTTPS/CORS/logging/deployment/backup scope;
- AI provider mode, tool registry, permission checks and confirmation for writes;
- consistency between source, technical documents, thesis diagrams and evidence.

The manual UI scenarios remain in [`ba-test-frontend`](../../../../ba-test-frontend/README.md). They are referenced, not copied.

## Environment facts observed statically

| Area | Observed fact | Verification state |
|---|---|---|
| Backend | ASP.NET Core 8, EF Core 8, Npgsql/PostgreSQL, JWT and SignalR are present in project/source | Đã xác minh tĩnh |
| Frontends | `apps/admin-web` and `apps/customer-web` are React/Vite applications | Đã xác minh tĩnh |
| Realtime | Hub path `/kitchenHub` is mapped in `Program.cs` | Đã xác minh tĩnh; delivery chưa runtime kiểm tra |
| Health | `/health` endpoint is mapped in `Program.cs` | Đã xác minh tĩnh; response chưa gọi |
| Database | Canonical migration directory contains 49 non-designer migration files at survey time | Đã xác minh tĩnh; applied database state chưa kiểm tra |
| AI | Mock provider, AI permission service, registry and write confirmation code are present | Đã xác minh tĩnh; tool runtime chưa kiểm tra |
| Manual test entry | HTTPS/local/LAN preparation and role cases are documented in `ba-test-frontend` and `scripts/` | Đã xác minh tĩnh; môi trường chưa khởi động trong lượt này |

## Not verified in this preparation step

- No `dotnet build`, frontend build, OpenAPI codegen check or dependency command was run.
- No API, browser, WebSocket/SignalR or `/health` request was made.
- No database connection, schema inspection, migration application, seed or query was run.
- No payment, order, inventory, loyalty, QR or AI flow was manually executed.
- No backup/restore or performance test was executed.
- No production/staging environment was accessed.
- No claim is made that existing evidence files still match the current dirty working tree.

## Required conditions for later runtime work

Runtime work needs a separately approved isolated test target, known environment mode, redacted configuration, HTTPS certificate/trust setup where applicable, test accounts for each role, at least two branches for isolation cases, and non-production fixture data. Migration/seed tools must not be run until the target is positively identified and the operation is explicitly approved.

## Limits on conclusions

This preparation establishes audit scope and evidence rules only. It does not certify functionality, security, schema compatibility, deployment readiness or thesis correctness. A missing runtime environment should be recorded as `Bị chặn`, not converted to `PASS` or `FAIL` without the relevant evidence.
