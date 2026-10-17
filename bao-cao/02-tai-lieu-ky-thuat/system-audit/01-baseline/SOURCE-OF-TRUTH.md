# Source of Truth

## Priority order

When sources disagree, use this order and record the conflict instead of silently choosing a convenient value:

1. Current executable source in the surveyed working tree.
2. EF Core model, `ApplicationDbContext` configuration and canonical migrations.
3. Controllers, application services, DTOs and API/OpenAPI contract.
4. Frontend consumers and role/route configuration.
5. Environment/configuration files and operational scripts, with secrets redacted.
6. Tests, diagnostics and runtime evidence, classified by the environment and date in which they were produced.
7. Current technical documentation.
8. Archived, historical or design-only documentation.

Runtime evidence can prove observed behavior for its named environment and version; it does not override source for what is currently implemented after later changes.

## Authority by information type

| Information | Primary source | Cross-check | Do not infer from |
|---|---|---|---|
| Business rules and money | `OrderService`, related services and persisted `Order` fields | Order controller/DTO, frontend display, invoice/payment code | Frontend totals or old reports |
| Authentication and roles | `Program.cs`, JWT service and controller attributes | Role names, frontend guards, runtime responses | Menu visibility or position labels |
| Branch isolation | Claims/context plus controller/service checks and entity relationships | `ApplicationDbContext`, branch-scoped queries, cross-branch runtime test | Client `branchId`, URL or local storage |
| API contract | Controller routes, DTOs and Swagger/OpenAPI | `packages/shared/swagger.json`, generated TypeScript types, frontend callers | A stale document or screenshot |
| Database schema | Entity configuration, `ApplicationDbContext`, canonical migration chain and model snapshot | Actual isolated test database when runtime access is approved | Archived migration copies or model names alone |
| Migration state | Files under `services/api/src/Infrastructure/Persistence/Migrations/` plus EF discovery/output | Database migration history in an authorized test target | Migration count in an older report |
| Frontend behavior | Current React route/component/API consumer | Manual checklist and runtime evidence | A route mentioned only in documentation |
| Realtime behavior | SignalR hub, notifier/event producer and frontend connection/consumer | Runtime WebSocket/event evidence | A static event name without a connected consumer |
| AI capability and permission | `services/api/src/AI/`, registry, permission service and provider configuration | `AI_RULES.md`, runtime tool audit evidence | Prompt text or design history alone |
| Deployment and environment | `Program.cs`, appsettings templates, scripts and launch configuration | Redacted runtime attestation/runbook | A machine-specific URL or secret-bearing local file |
| Thesis claims | Current source plus verified evidence | Thesis/report diagrams and use cases | Report text that has not been reconciled |

## Conflict handling

For every conflict, create or update one finding with:

- stable ID;
- exact paths and version/snapshot;
- the conflicting statements;
- current classification: `Đã xác minh tĩnh`, `Chưa kiểm tra`, `Bị chặn` or another applicable state;
- impact and severity;
- what runtime or database evidence is required before a conclusion.

For example, the current source directory contains 49 canonical migration `.cs` files (excluding designer files and the model snapshot), with latest file `20261014120000_AddNotificationReadState.cs`. Existing `CODEX-CURRENT-STATE.md` records 43 and an older latest migration. This is a **verified documentation/source-count discrepancy**, not proof that the database is broken or that all 49 migrations are applied.

## Evidence rule

An assertion is not `PASS` unless the evidence method matches the assertion. Static code reading may support `Đã xác minh tĩnh`; it cannot support successful payment, realtime delivery, database consistency or browser behavior by itself.
