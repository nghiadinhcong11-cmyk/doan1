# AI Embedding Development API Verification

**Repository:** `D:\Dev\doan`
**Date:** 2026-10-17
**Scope:** isolated HTTP host only. No existing API process was restarted, no PostgreSQL/Supabase connection was made, and no browser was used.

## 1. Executive result

The isolated HTTP harness completed **25 PASS / 0 FAIL**. It exercised the actual `AiController` route `POST /api/Ai/chat`, the real `AiOrchestrator`, `ManagerEmbeddingPilot`, `AiToolRegistry`, `AiPermissionService`, and a SQLite in-memory `ApplicationDbContext`. The tool implementation was a recording fake; no business database was queried.

The real Development API was **not started** because the preflight could not verify an independent Development/TEST database connection or a Development JWT secret. Therefore this report is evidence for an isolated HTTP host, not evidence from the deployed API process.

## 2. Environment preflight

| Item | Result | Evidence |
|---|---|---|
| `AI:EmbeddingPilot:Enabled` in checked-in Development config | PASS | `false` in `services/api/appsettings.Development.json` |
| Provider mode | PASS | `AI:ProviderMode=Mock` |
| Model/tokenizer paths | PASS | configured under `AI:EmbeddingPilot` |
| Model checksum | PASS | `4654C156F3E4171ABC9C716CDB771BF9116455D15AC1AAB364AEEEDE0E3205B0` |
| Tokenizer checksum | PASS | `0B44A9D7B51C3C62626640CDA0E2C2F70FDACDC25BBBD68038369D14EBDF4C39` |
| Database target | BLOCKED for real API | checked-in Development target is Test/project-ref guarded, but no connection string was present in the process environment |
| JWT secret | BLOCKED for real API | `Jwt__Secret` was absent; `Program.cs` intentionally fails startup without it |
| Existing API port 5000 | NOT RUN | no existing API process was restarted or altered |
| Environment variables | PASS | no secrets printed; relevant secret variables were checked only for presence/length |

The isolated host used loopback HTTP on an ephemeral port and SQLite `Data Source=:memory:`. This was not the configured API connection string and was destroyed at host shutdown.

## 3. Test host and HTTP endpoint

The harness added an isolated `WebApplication` with:

- `AiController` from the production assembly and route `/api/Ai/chat`.
- Real `AiOrchestrator` and real `AiContextService`.
- Real `AiToolRegistry` and `AiPermissionService`.
- SQLite in-memory `ApplicationDbContext` containing a test branch.
- A test authentication handler that creates server-side claims (`Role`, `branchId`, `fullName`). The request body cannot set these claims.
- A recording read-only tool for `get_revenue`.
- A provider that throws if Mock fallback is called while the pilot is enabled.

This proves controller-to-orchestrator HTTP wiring. It is not a JWT cryptographic validation test; production JWT middleware remains configured in `Program.cs` and was not started because its secret/database prerequisites were unavailable.

Command used:

```powershell
dotnet run --project .tools/runtime-pilot-verification/RuntimePilotVerification.csproj -c Release --no-restore
```

Final output:

```text
SUMMARY|passed=25|failed=0|database=NOT_USED|api=NOT_STARTED
```

## 4. Feature flag and role gating

- Flag OFF HTTP case: **PASS**. Manager request did not enter the pilot; the deliberately throwing provider path returned a generic failure response and no tool execution.
- Flag ON + manager HTTP case: **PASS**. The request returned the recording tool result (`"recorded"`).
- Direct role-gate checks for employee and customer: **PASS**.
- Request input cannot set the feature flag, role, or branch; these come from server-side test claims/context.
- Environment gate remains Development/Test/Testing only in `ManagerEmbeddingPilot.IsEnabled`.
- No fallback to Mock was observed after pilot selection; the throwing provider was not called in the enabled manager case.

## 5. HTTP behavior and policy cases

The same policy cases were executed through the real pilot and orchestrator path in the harness:

| Case | Result | Expected behavior |
|---|---|---|
| Revenue today | PASS | ACCEPT → `get_revenue` |
| Revenue year | PASS | ACCEPT → `get_revenue` |
| Summary supported period | PASS | ACCEPT → `get_business_summary` |
| Revenue week / invalid period | PASS | CLARIFY `UnsupportedPeriod` |
| OOD inventory/menu style request | PASS | UNSUPPORTED |
| Negation | PASS | CLARIFY |
| Summary multi-field | PASS | one summary tool |
| Compound OOD | PASS | UNSUPPORTED, no tool |
| Missing BranchId | PASS | fail closed / no tool |
| Missing model | PASS | generic unavailable response, no tool |
| Checksum mismatch | PASS | generic unavailable response, no tool |

The real orchestrator test also confirmed registry lookup and permission evaluation before the recording tool execution. CLARIFY did not execute a tool.

## 6. Branch isolation and permission

Two branch contexts were tested through the real orchestrator and SQLite context service:

- Branch A call carried `11111111-1111-1111-1111-111111111111`.
- Branch B call carried `22222222-2222-2222-2222-222222222222`.
- The recording tool received exactly A then B.
- No branch value was parsed from the user message.
- Missing manager branch was blocked.
- Employee role and missing-branch manager permission checks were denied.

This verifies trusted context propagation and tool authorization in the isolated host. It does not prove production application-service filtering over real order rows; that remains BLOCKED because database access was prohibited.

## 7. Timeout, cancellation, and resource checks

The inherited isolated stress checks also ran in the same command:

- Cold initialization: PASS; latest run approximately 5579.73 ms.
- Warm median: approximately 14.45 ms.
- Warm p95: approximately 17.84 ms.
- 8 concurrent requests: PASS.
- 64-request queue stress: PASS with no deadlock; 64 completed and 0 timed out.
- Cancellation before enqueue: PASS.
- Dispose while active operation: PASS.
- Missing model/checksum mismatch: PASS.

A deterministic queue timeout was not triggered in this environment. Cancellation while waiting for the semaphore and cancellation inside native ONNX inference remain **NOT RUN/BLOCKED**. RSS in the run increased from about 183 MB to 967 MB; this includes the harness, ASP.NET host, SQLite model metadata and ONNX allocations and is not a production capacity estimate.

## 8. Failure handling and logging

- Pilot initialization errors are fail-closed and produce a generic response without model paths or stack traces.
- No tool execution occurs after CLARIFY, UNSUPPORTED, missing branch, model failure, or checksum failure.
- Pilot decisions log only correlation ID, decision, reason, intent/tool for ACCEPT, and duration; input text and secrets are not logged.
- The isolated host used console logging. Production log sink/retention was not tested.

## 9. Build verification

```powershell
dotnet build services/api/RestaurantPOS.api.csproj --no-restore -c Release -o .pilotbuild
```

Result: **PASS**, 0 errors. The isolated harness also built and ran with 25/25 checks passing.

## 10. PASS/FAIL/BLOCKED/NOT RUN

- **PASS:** isolated HTTP controller route, real orchestrator, manager gate, flag OFF/ON behavior, trusted claims, tool registry, permission checks, branch propagation, policy decisions, model failure handling, build, ONNX inference.
- **FAIL:** none.
- **BLOCKED:** real Development API preflight because no verified independent connection string/JWT secret was available; production application-service data filtering and production audit transport were not exercised.
- **NOT RUN:** deployed API process, PostgreSQL/Supabase, real JWT signature validation, browser, cancellation inside native ONNX, deterministic queue timeout, peak memory under a production-like sample size.

## 11. GO/NO-GO

**GO for the user’s next manual web test only after a separately authorized Development/TEST API is configured with an independently verified database and JWT secret, while keeping the feature flag OFF until deliberate enablement.**

**NO-GO for enabling the current user-facing API, production, or marking intents APPROVED.** The isolated HTTP host validates endpoint wiring and policy behavior but does not replace verification of the deployed Development API and its real application-service branch filtering.


## Current status / Superseded by

Historical controlled API verification; current: retained, superseded by live UAT evidence in final acceptance report.
