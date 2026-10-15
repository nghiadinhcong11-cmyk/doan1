# Repository System Audit

Audit preparation index. Repository: `D:\Dev\doan`. The preparation snapshot is recorded in [`01-baseline/REPOSITORY-SNAPSHOT.md`](01-baseline/REPOSITORY-SNAPSHOT.md).

## Audit rule

Current executable source is the primary reference, followed by the EF Core model and canonical migrations, API/service code, frontend consumers, configuration, tests and scripts, and finally historical documents. A document can describe intended behavior, but it cannot prove that behavior is implemented or working at runtime.

Use these labels when classifying evidence:

- `Đã xác minh tĩnh`: supported by source/schema/configuration inspection only.
- `Đã kiểm tra kỹ thuật`: supported by an executed build, typecheck, codegen or inspection command.
- `Đã kiểm tra runtime`: supported by an executed API/database/realtime check in a named environment.
- `Đã kiểm tra thủ công`: supported by a user-operated scenario, normally from [`ba-test-frontend`](../../../ba-test-frontend/README.md).
- `Chưa kiểm tra`, `Bị chặn`, `Không áp dụng`.

Do not record `PASS` merely because code was read or a build succeeded.

## Audit groups and order

| Order | Group | Scope | Primary output |
|---|---|---|---|
| 0 | [`01-baseline`](01-baseline/REPOSITORY-SNAPSHOT.md) | Version, working tree, source of truth and environment limits | Reproducible audit boundary |
| 1 | `02-static-review` | Architecture, API, frontend contracts, security, branches, database, business rules and AI | Static findings and unknowns; existing documents are linked below until further grouping is needed |
| 2 | `03-technical-verification` | Build, typecheck, OpenAPI, dependency/configuration review and migration reconciliation | Command results with environment metadata |
| 3 | `04-runtime-and-manual` | API, database, SignalR, cross-role flows, retry/concurrency and manual UI | Runtime/manual evidence; do not duplicate the checklist |
| 4 | [`05-operations-and-thesis/EVIDENCE-INDEX.md`](05-operations-and-thesis/EVIDENCE-INDEX.md) | Deployment, logging, backup/restore, performance scope, thesis consistency and evidence catalog | Evidence map and limitations |
| 5 | [`06-findings/FINDINGS-REGISTER.md`](06-findings/FINDINGS-REGISTER.md) | One consolidated register for all findings and retests | Prioritized readiness decision |

The remaining groups and files are intentionally not created until there is concrete content to record.

## Existing material to reuse

| Area | Existing source |
|---|---|
| Manual frontend/runtime scenarios | [`ba-test-frontend/README.md`](../../../ba-test-frontend/README.md) and `01`–`08` role/flow checklists |
| System overview, roles, feature and API map | [`01-SYSTEM-OVERVIEW.md`](01-SYSTEM-OVERVIEW.md), [`04-ACTORS-AND-ROLES.md`](04-ACTORS-AND-ROLES.md), [`05-FEATURE-MATRIX.md`](05-FEATURE-MATRIX.md), [`06-API-MAP.md`](06-API-MAP.md) |
| Database and migrations | [`07-DATABASE.md`](07-DATABASE.md), [`31-EF-MIGRATION-TREE-AUDIT.md`](31-EF-MIGRATION-TREE-AUDIT.md), [`32-EF-MIGRATION-RECONCILIATION.md`](32-EF-MIGRATION-RECONCILIATION.md), [`53-EF-MIGRATION-BASELINE-RECONCILIATION.md`](53-EF-MIGRATION-BASELINE-RECONCILIATION.md) |
| Business, POS, QR and realtime | [`08-BUSINESS-FLOWS.md`](08-BUSINESS-FLOWS.md), [`09-REALTIME-SIGNALR.md`](09-REALTIME-SIGNALR.md), [`29-E2E-BUSINESS-FLOW-VERIFICATION.md`](29-E2E-BUSINESS-FLOW-VERIFICATION.md), [`52-GUEST-QR-SESSION-SECURITY.md`](52-GUEST-QR-SESSION-SECURITY.md) |
| Security and branch isolation | [`11-SECURITY-AUDIT.md`](11-SECURITY-AUDIT.md), [`48-ROLE-FUNCTION-MATRIX.md`](48-ROLE-FUNCTION-MATRIX.md), [`49-MANAGER-BRANCH-ISOLATION-AUDIT.md`](49-MANAGER-BRANCH-ISOLATION-AUDIT.md), [`50-UNIFIED-STAFF-AUTHENTICATION.md`](50-UNIFIED-STAFF-AUTHENTICATION.md) |
| AI | [`10-AI-INTEGRATION.md`](10-AI-INTEGRATION.md), [`51-PUBLIC-GUEST-CUSTOMER-AI-SECURITY.md`](51-PUBLIC-GUEST-CUSTOMER-AI-SECURITY.md), repository [`AI_RULES.md`](../../../AI_RULES.md) |
| Deployment and documentation consistency | [`12-DEPLOYMENT.md`](12-DEPLOYMENT.md), [`15-THESIS-SOURCE-OF-TRUTH.md`](15-THESIS-SOURCE-OF-TRUTH.md), [`17-DOCUMENTATION-CONFLICTS.md`](17-DOCUMENTATION-CONFLICTS.md), [`18-THESIS-CONSISTENCY-AUDIT.md`](18-THESIS-CONSISTENCY-AUDIT.md) |

Historical or superseded material remains under [`bao-cao/archive`](../../archive/README.md) and must not override current source without re-verification.

## Working method

Each audit item records separately:

`Phương pháp | Trạng thái thực hiện | Kết luận | Minh chứng`

The methods are static review, technical verification, runtime verification and manual testing. Findings are recorded only once in [`06-findings/FINDINGS-REGISTER.md`](06-findings/FINDINGS-REGISTER.md); other checklists link to the finding ID.
