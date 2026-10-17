# AI Embedding v3.3 — Freeze Baseline và Runtime Integration Readiness

Ngày audit: 2026-10-17
Trạng thái: RESEARCH_BASELINE_LOCKED
Production: NOT APPROVED
Phạm vi: audit runtime và freeze artifact; không tích hợp embedding.

## 1. Baseline đã freeze

Manifest: AI-EMBEDDING-V33-BASELINE-MANIFEST.json

Model:

- intfloat/multilingual-e5-small.
- Source revision: 614241f622f53c4eeff9890bdc4f31cfecc418b3.
- ONNX SHA-256: 4654C156F3E4171ABC9C716CDB771BF9116455D15AC1AAB364AEEEDE0E3205B0.
- Tokenizer SHA-256: 0B44A9D7B51C3C62626640CDA0E2C2F70FDACDC25BBBD68038369D14EBDF4C39.
- Config SHA-256: BBB7C1333FC4B3E27FBC9CD5D2070AABCC1D4DFB99917C3633E772F97545A6B6.
- Prefix query: , masked mean pooling, L2 normalization, dimension 384, max 512 tokens.

Dataset/artifact hashes are recorded in the manifest. The frozen result is the v3.3 result JSON.

Scoring freeze: policy v3.3, Max aggregation, minimum score 0.75, minimum margin 0.0, boundary gap 0.0. Explicit negation produces CLARIFY. OOD is checked before missing period. Unsupported revenue periods produce CLARIFY without fallback or runner-up substitution.

Verification: adjudicated historical 20/20, fresh holdout 16/16, policy checks 11/11, ONNX inference PASS.

The two intents are RESEARCH_BASELINE_LOCKED. They are not PRODUCTION_APPROVED.

## 2. Capability compatibility

GetRevenueTool.cs lines 12-58 confirms get_revenue, roles admin/manager, Read risk, required period, allowlist today/month/year and BranchId from AiUserContext.

GetBusinessSummaryTool.cs lines 11-99 confirms get_business_summary, roles admin/manager, Read risk, periods yesterday/this_week/last_week/this_month/last_month/today, and summary output fields.

DashboardService.cs lines 357-383 and 407-421 confirms revenue calculation by paid completed orders and branch. Lines 424-497 confirm summary revenue, order count, AOV, expenses, estimated COGS, estimated net profit and top products with branch filtering.

Important mismatch: GetBusinessSummaryTool lines 71-75 use a default branch that maps unknown periods to today. The frozen spike policy rejects invalid periods. An integration validator must block invalid periods before this tool is called; spike metadata cannot override backend behavior.

## 3. Runtime architecture audit

IAiProvider is services/api/src/AI/Services/IAiProvider.cs. MockAiProvider is MockAiProvider.cs. Program.cs lines 303-305 registers the scoped deterministic Mock provider. It uses MockAiIntentRecognizer and no E5/ONNX matcher.

AiOrchestrator builds trusted context and role-filtered tools, calls the provider in a loop up to five iterations, parses function calls, checks permission, executes tools and feeds results back. Function-call execution is gated by registry lookup, read-only mode and AiPermissionService at AiOrchestrator.cs lines 226-269.

AiToolRegistry filters tool definitions by role, risk and customer restrictions. AiPermissionService checks known role, branch presence for branch-scoped staff, allowed role and risk. AiUserContext carries Role, BranchId and identity fields.

AiController.Chat is authenticated for all existing roles and reads role/branch from server claims. Program.cs lines 328-349 register all tools. There is no manager-only semantic pilot gate.

## 4. Safe future insertion boundary

A future semantic strategy should sit after trusted AiUserContext creation and before a provider function call can reach tool execution:

AiController trusted context → manager-only feature flag → embedding candidate and capability policy → existing provider/orchestrator response → registry → AiPermissionService → tool.

This must be a separate strategy/service. It must not infer role/branch from text, replace permission checks, call tools directly, broaden the registry, change non-manager behavior or execute on CLARIFY/UNSUPPORTED.

Changing only MockAiProvider is insufficient because its request body is not a typed trusted authorization context. The orchestrator/controller boundary is required.

## 5. Resource and lifecycle readiness

SentenceEmbeddingAdapter loads tokenizer and InferenceSession in its constructor and reuses them for its lifetime. It enforces MaxConcurrency 1 because tokenizer/session concurrency is not assumed. Admission is bounded; overflow raises EmbeddingBusyException. Cancellation is checked while waiting and before/after synchronous ONNX inference; native inference is not interrupted mid-call. Dispose waits for active operations.

Historical local measurements record warm median about 5.422 ms and working set about 616.8 MB; another lifecycle audit recorded RSS after adapter about 627,376,128 bytes. These are single-machine observations, not production limits. Cold start, concurrent API load, GC behavior, worker count and container limits are unverified. The current API has no embedding registration or timeout/fallback contract.

A future integration should investigate a shared singleton session, explicit queue/timeout behavior and a disabled-feature fallback to the existing Mock recognizer. None of this is implemented.

## 6. Post-integration test plan

Automated tests should cover adapter preflight and hashes; negation, OOD, period and composition; both tool contracts; manager permissions; non-manager isolation; branch A/B isolation; CLARIFY/UNSUPPORTED without function calls; raw/adjudicated/frozen regression; unavailable model, queue saturation and cancellation; feature flag states; audit logging; and rollback to current Mock behavior.

Manual web verification by the user should cover manager revenue supported periods, unsupported revenue periods, summary periods, OOD, negation, compound requests, other roles and branch isolation. No browser or database test was run.

## 7. Risks and rollback

Risks:

- Summary default period fallback conflicts with the frozen no-fallback contract.
- Existing provider has a broad role/tool surface; incomplete gating could change other roles.
- Memory overhead is much larger than model file size.
- Inference is serialized and native cancellation is cooperative.
- Spike capability metadata is not runtime authorization evidence.
- Raw historical labels differ from adjudicated labels and both must remain traceable.

Rollback:

1. Keep the feature flag disabled by default.
2. Remove only semantic strategy registration/configuration.
3. Restore the existing Mock provider path.
4. Leave registry, permission service, backend tools and database unchanged.
5. Keep manifest and result artifacts.

## 8. Readiness matrix

| Area | Status | Evidence |
|---|---|---|
| Frozen model/tokenizer and hashes | READY for research | Manifest and local artifacts |
| v3.3 dataset/scoring freeze | READY for research | Hashes and 16/16 holdout |
| ONNX local inference | READY for spike | Result JSON |
| Capability metadata | PARTIAL | Matches source, but summary has default fallback |
| Manager-only runtime gate | MISSING | No current pilot gate |
| Existing permission/branch enforcement | READY in runtime; not connected to spike | AiPermissionService and AiUserContext |
| Safe CLARIFY/UNSUPPORTED integration | PARTIAL | Offline evidence only |
| API session lifecycle | UNKNOWN | No embedding service registration |
| Production memory/latency budget | BLOCKED | Historical single-process measurements only |
| Rollback plan | READY as plan | Existing Mock path can remain default |

## 9. Decision

**NO-GO for runtime implementation now.**

The research baseline is frozen and ready for review. A limited runtime pilot requires an approved manager-only feature gate, an explicit invalid-period validator before summary execution, integration tests for permission and branch isolation, and resource measurements in the target backend environment. This report does not approve deployment.



## Current status / Superseded by

Historical readiness audit; current: baseline remains locked, runtime acceptance summarized in final report.
