# AI Embedding Live API Pilot Verification

**Repository:** `D:\Dev\doan`
**Audit date:** 2026-10-17
**Scope:** live API preflight and controlled startup attempt only. No schema change, migration, seed, write operation, browser test, or frontend change was performed.

## 1. Executive result

The live API pilot was **not enabled successfully** because the API could not complete startup. The process failed during HTTPS certificate loading before binding port 5000. No API process was left running and no HTTP request reached the real API/database.

The checked-in Development configuration keeps `AI:EmbeddingPilot:Enabled=false`. The feature flag was supplied only as a temporary process environment value during the startup attempt; it was not written to configuration files.

Status: **BLOCKED for live API verification**.

## 2. Environment preflight

| Item | Result | Evidence |
|---|---|---|
| API process/port | BLOCKED | No listener was present on port 5000 before the attempt; no process was left running afterward |
| Development environment | PASS for intended launch | Temporary process environment set `ASPNETCORE_ENVIRONMENT=Development` |
| Database target | INSPECTED | User Secrets entry exists; project configuration and secret target mode are `Test`; automatic migration was set false |
| Database connection | NOT RUN | Startup did not reach a database query or application endpoint |
| JWT configuration | INSPECTED | User Secrets contains a JWT secret; value was never printed or logged |
| Embedding feature flag | OFF in repository | `services/api/appsettings.Development.json` contains `AI:EmbeddingPilot:Enabled=false` |
| Temporary pilot flag | NOT EFFECTIVE | `AI__EmbeddingPilot__Enabled=true` was passed to a short-lived startup attempt that failed before listening |
| Model checksum | PASS | E5 model SHA-256 matches frozen v3.3 baseline |
| Tokenizer checksum | PASS | tokenizer SHA-256 matches frozen v3.3 baseline |
| Admin Web API target | PASS by source inspection | `apps/admin-web/src/config.ts` derives `https://<current-host>:5000` unless `VITE_API_URL` overrides it |
| Database writes/migrations | NOT RUN | `DatabaseSafety:AllowAutomaticMigration=false`; no migration or seed command was executed |

The user-secret database target was inspected only for non-secret metadata. Connection string, password, JWT secret, access token and credentials were not printed.

## 3. Startup attempt and blocking evidence

Backend was rebuilt successfully before the attempt:

```powershell
dotnet build services/api/RestaurantPOS.api.csproj --no-restore -c Release
```

Result: **PASS**, 0 warnings, 0 errors.

A temporary Development launch used:

- `Database:TargetMode=Test`
- automatic migration disabled
- background insights disabled
- `AI:ProviderMode=Mock`
- temporary `AI:EmbeddingPilot:Enabled=true`
- existing User Secrets configuration
- existing repository PEM certificate paths

The process reported the database target attestation as blocked and then failed in HTTPS configuration:

```text
BACKEND_STARTUP_PHASE=HTTPS_CONFIGURATION
BACKEND_HTTPS_CERTIFICATE_VALID=NO
System.Security.Cryptography.CryptographicException: Access denied.
```

The failure originated while `DevelopmentHttpsCertificateLoader` attempted to export the PEM certificate/key pair into a persisted PKCS#12 object. The process exited before port 5000 was bound.

The repository HTTPS validation script confirmed the PEM pair is structurally valid and reusable, but that did not resolve the runtime private-key access error. HTTPS was not disabled and no HTTP workaround was used.

## 4. API and database verification status

No live `POST /api/Ai/chat` request was sent. Therefore the following have no live result:

- manager JWT authentication against the API
- manager BranchId from a real JWT
- real `get_revenue` execution
- real `get_business_summary` execution
- DashboardService/database branch filtering
- live tool audit events
- live HTTP response bodies
- live latency/RAM

These remain **NOT RUN**, rather than PASS or FAIL.

The previously completed isolated HTTP harness remains valid evidence for controller/orchestrator wiring, but it used SQLite in-memory data and a test authentication handler; it does not substitute for this live API check.

## 5. Admin Web inspection

`apps/admin-web/src/config.ts` resolves the API as:

```text
https://<browser-host>:5000
```

unless `VITE_API_URL` is explicitly configured. `ChatBot.tsx` posts to `/api/Ai/chat` and sends the stored `adminToken`. No frontend source was changed and no browser was started.

## 6. Required user action before retry

A live retry requires resolving the HTTPS startup prerequisite in the same Development user context:

1. Confirm the Development HTTPS certificate/private-key pair can be loaded by the API process, or run the repository’s approved HTTPS setup/repair procedure in the preparation terminal.
2. Keep `Database:TargetMode=Test` and automatic migration disabled.
3. Start the API without killing or replacing another process.
4. Confirm `https://localhost:5000/health` returns 200.
5. Only then set the temporary process flag `AI__EmbeddingPilot__Enabled=true` and run the read-only manager checks.

No new secret, certificate password, connection string, or JWT was created by this audit.

## 7. Planned live read-only cases

Once startup is healthy, the controlled read-only matrix is:

- `Doanh thu hôm nay/tháng này/năm nay` → `get_revenue`
- `Tổng quan kinh doanh hôm nay/tuần này` → `get_business_summary`
- `Doanh thu tuần này/hôm qua` → CLARIFY, no tool
- Negation → CLARIFY, no tool
- OOD request → UNSUPPORTED, no tool
- Multi-field summary → `get_business_summary`
- Missing BranchId/invalid role → fail closed

The live run must record HTTP status, sanitized response, selected tool, trusted BranchId, and absence of writes. It must not alter model, dataset, threshold, schema, or MockAiProvider.

## 8. Rollback / disabling pilot

Because the flag was never persisted and no API process remained running, the repository is already in the previous OFF state. For a future process launch, disable with:

```text
AI__EmbeddingPilot__Enabled=false
```

or omit the environment override and use the checked-in Development setting. A running process should be stopped/restarted only by the operator responsible for that process; this audit did not do so.

## 9. Final status

- **PASS:** configuration inspection, model/tokenizer checksum, backend build, Admin Web API target inspection, HTTPS validation script.
- **BLOCKED:** live API startup due HTTPS private-key access error.
- **NOT RUN:** live HTTP AI tests, real JWT validation, real manager BranchId, real Supabase/PostgreSQL reads, tool execution evidence, live latency/RAM, browser test.
- **FAIL:** none in application logic; startup prerequisite failed.

**GO/NO-GO:** **NO-GO for live pilot verification and user web testing until the API starts successfully with its existing Development HTTPS certificate and independently verified Test database target.** No production deployment or intent approval is implied.


## Current status / Superseded by

Historical live pilot verification; current: five-case UAT evidence is consolidated in final acceptance report.
