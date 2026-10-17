# AI Embedding Runtime Integration Stress Verification

**Repository:** `D:\Dev\doan`
**Audit date:** 2026-10-17
**Scope:** isolated Development harness only; no API process, PostgreSQL, Supabase, migration, browser, or production configuration.

## 1. Executive result

The isolated harness completed **23 PASS / 0 FAIL**. The test used the real `AiOrchestrator`, `ManagerEmbeddingPilot`, `AiToolRegistry`, `AiPermissionService`, and a SQLite in-memory `ApplicationDbContext` for context queries. Business tools were fakes; no business database was used.

This is evidence that the pilot wiring can execute safely in the isolated harness. It is **not approval to enable the feature in a user-facing API** and is not a production-readiness claim. GO is limited to proceeding to controlled API Development/TEST verification after the remaining blocked items are addressed.

## 2. Source changes in this verification

- `.tools/runtime-pilot-verification/Program.cs`: added real-orchestrator, SQLite in-memory, branch A/B, checksum, cancellation, lifecycle, and queue stress checks.
- `.tools/runtime-pilot-verification/RuntimePilotVerification.csproj`: added `Microsoft.EntityFrameworkCore.Sqlite` only for the isolated harness.
- `services/api/src/AI/Services/ManagerEmbeddingPilot.cs`: added operation tracking and deferred resource disposal so disposing during an active operation does not dispose the ONNX session prematurely.
- `services/api/src/AI/Services/AiOrchestrator.cs`: added sanitized `AI_EMBEDDING_PILOT_DECISION` logs for ACCEPT, CLARIFY, and UNSUPPORTED decisions.

The v3.3 model, tokenizer, reference catalog, threshold, and baseline manifest were not changed.

## 3. Runtime wiring evidence

### Real orchestrator path

`AiOrchestrator.ProcessReadOnlyAsync` validates the trusted role/branch, obtains context through the real `AiContextService`, gets definitions from the real `AiToolRegistry`, invokes `ManagerEmbeddingPilot` only for manager when the feature flag is enabled, and on ACCEPT performs registry lookup, read-only guard, `AiPermissionService.CanExecuteAsync`, then `IAiTool.ExecuteAsync` (`services/api/src/AI/Services/AiOrchestrator.cs:89-150, 392-430`).

The harness used a provider that throws if called. The ACCEPT path still succeeded, proving no fallback to Mock provider. The fake recording tool received exactly the selected tool call.

### Feature gate

The pilot remains disabled unless the environment is Development/Test/Testing and `AI:EmbeddingPilot:Enabled` is true (`ManagerEmbeddingPilot.IsEnabled`, `services/api/src/AI/Services/ManagerEmbeddingPilot.cs:52`). Employee and guest checks returned `PilotDisabledOrRoleNotAllowed`. The request text was never used to supply role or branch.

## 4. Executed verification matrix

| Check | Result | Evidence |
|---|---|---|
| Flag OFF | PASS | no pilot decision; disabled reason |
| Employee/customer role gate | PASS | no semantic pilot |
| Manager revenue today/year | PASS | ACCEPT → `get_revenue` |
| Manager summary supported period | PASS | ACCEPT → `get_business_summary` |
| Revenue unsupported week / invalid period | PASS | CLARIFY `UnsupportedPeriod` |
| OOD inventory and compound OOD | PASS | UNSUPPORTED `OutOfScopeCapability` |
| Negation | PASS | CLARIFY `NegatedRequest` |
| Summary multi-field | PASS | ACCEPT single `get_business_summary` |
| Missing manager branch | PASS | CLARIFY `MissingBranchContext` |
| Missing model | PASS | fail-closed; no tool |
| Checksum mismatch | PASS | fail-closed; no inference/tool |
| Cancellation before enqueue | PASS | `OperationCanceledException`; no tool |
| Dispose with active operation | PASS | operation completed safely after dispose request |
| Permission manager/branch | PASS | authorized manager branch; missing branch and employee denied |
| Real AiOrchestrator ACCEPT | PASS | real context, registry, permission, fake tool |
| Real AiOrchestrator Branch A/B | PASS | calls carried `1111...` then `2222...`; no cross-branch value was supplied |
| Real AiOrchestrator CLARIFY | PASS | week revenue produced no tool call |
| Concurrent requests | PASS | 8/8 completed |
| Queue stress | PASS | 64/64 completed; no deadlock or semaphore leak |

Harness command:

```powershell
dotnet restore .tools/runtime-pilot-verification/RuntimePilotVerification.csproj --ignore-failed-sources
dotnet run --project .tools/runtime-pilot-verification/RuntimePilotVerification.csproj -c Release --no-restore
```

Observed final summary:

```text
SUMMARY|passed=23|failed=0|database=NOT_USED|api=NOT_STARTED
```

## 5. Branch A/B isolation

The real context service queried the SQLite in-memory context for two seeded branches. Two manager contexts produced two tool executions, with the recording tool receiving exactly Branch A then Branch B. The user message did not contain a branch override and no request field was used to replace the trusted `BranchId`.

This verifies propagation and isolation of trusted context in the pilot path. It does not prove every production application service query is branch-safe; the fake tool did not read production order data.

## 6. Stress, timeout, cancellation, and lifecycle

Benchmark from the final run:

```text
cold_ms=2620.21
warm_median_ms=7.63
warm_p95_ms=12.68
rss_before=146,534,400
rss_after=924,643,328
rss_delta=778,108,928
concurrent=8
```

The RSS delta includes .NET, ONNX Runtime, SQLite model construction, and harness allocations; it must not be treated as production steady-state memory. Cold initialization includes checksum/file reads, tokenizer load, ONNX session creation, and reference encoding.

- Model/session reuse was observed through warm inference after one initialization.
- 8 concurrent requests completed without deadlock.
- 64-request queue stress completed without deadlock; this run did not trigger the 100 ms timeout branch (`timeouts=0`). Therefore timeout-under-saturation remains **NOT RUN as a positive timeout assertion**.
- Cancellation before enqueue was PASS.
- Cancellation while waiting for a semaphore and cancellation inside native ONNX inference were not independently demonstrated; classify **BLOCKED/NOT RUN**.
- Dispose with an active operation was PASS after adding deferred resource disposal.
- Missing model and checksum mismatch were PASS and fail closed.
- No automatic retry or Mock fallback was observed.

## 7. Runtime policy parity

The runtime checks preserved the frozen v3.3 policy for the tested rows:

- Revenue today/year: ACCEPT `get_revenue`.
- Revenue week: CLARIFY `UnsupportedPeriod`.
- Summary supported period and multi-field output: ACCEPT `get_business_summary`.
- Invalid/future period: CLARIFY before tool execution.
- OOD inventory/menu style requests: UNSUPPORTED.
- Negated request: CLARIFY.
- Compound request containing an out-of-scope capability: UNSUPPORTED.

One runtime correction made before this verification is the explicit revenue-only unsupported-period guard before semantic routing, preventing a week revenue request from being accepted by the summary tool. This is a runtime policy correction, not a model, dataset, or threshold change.

## 8. Audit and failure response

The orchestrator emits sanitized decision logs with correlation ID, decision, intent/tool (for ACCEPT), reason, and duration. It does not log the message text, JWT, password, model path, or stack trace in the user response. Existing tool execution audit infrastructure remains in the normal orchestrator path.

Failure behavior observed:

- Initialization errors are logged server-side and returned as a generic fail-closed response.
- CLARIFY and UNSUPPORTED return without registry execution.
- Missing branch, missing model, and checksum mismatch do not call tools.
- The harness provider throws if fallback is attempted; no fallback occurred.

A separate production log sink review is still required after API deployment; this harness uses `NullLogger` and therefore does not validate emitted log transport or retention.

## 9. Build verification

Command:

```powershell
dotnet build services/api/RestaurantPOS.api.csproj --no-restore -c Release -o .pilotbuild
```

Result: **Build succeeded, 0 warnings, 0 errors** for the isolated output directory `.pilotbuild`.

The harness itself also built and ran successfully. No API process was started.

## 10. PASS/FAIL/BLOCKED/NOT RUN summary

- **PASS:** 23 isolated checks, backend build, ONNX load, real orchestrator ACCEPT/CLARIFY path, permission gate, trusted branch propagation, fail-closed initialization errors, active-dispose safety.
- **FAIL:** none in the final harness run.
- **BLOCKED:** production application-service data isolation and production audit sink behavior were not exercised because database/API access was prohibited.
- **NOT RUN:** cancellation while queued, cancellation during native ONNX inference, a deterministic timeout exception under queue saturation, API-level request/response verification, browser verification, PostgreSQL/Supabase behavior.

## 11. GO/NO-GO

**GO for the next controlled Development/TEST API verification step only**, subject to keeping the feature flag OFF until that step is explicitly enabled and observing the blocked items above.

**NO-GO for production, intent approval, or unrestricted rollout.** The isolated harness proves wiring and fail-closed behavior, but not production database/service filtering, deployed logging, native inference cancellation semantics, or API/browser behavior.


## Current status / Superseded by

Historical stress verification; current: 23 PASS evidence retained, superseded for final UAT state by final acceptance report.
