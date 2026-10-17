# AI Embedding Pilot — Acceptance and Mock Removal Audit

- Audit date: 2026-10-17
- Repository: `D:\Dev\doan`
- Scope: source audit and isolated verification only
- Baseline: `manager-two-intent-v3.3`
- Status: `PENDING_MANUAL_ACCEPTANCE` for both intents; `NO-GO` for removing Mock AI

## 1. Current AI Chat architecture

### Runtime path

For an authenticated chat request:

`POST /api/Ai/chat`
→ `AiController.Chat`
→ trusted `ClaimsPrincipal` role/branch extraction
→ `AiOrchestrator.ProcessAsync` or read-only variant
→ branch/role validation and context loading
→ `AiToolRegistry`
→ manager embedding gate when enabled
→ `ManagerEmbeddingPilot.DecideAsync`
→ for ACCEPT: registry lookup → read-only guard → `AiPermissionService.CanExecuteAsync` → selected tool → application service
→ response.

Evidence:

- `services/api/src/AI/Controllers/AiController.cs:63-67` exposes `POST /api/Ai/chat`; lines 83-130 build `AiUserContext` from authenticated role, user ID and `branchId` claims. Request content does not supply role or branch.
- `services/api/src/AI/Services/AiOrchestrator.cs:104-149` validates known role/branch, builds tool definitions, and selects the embedding path only when the pilot is enabled and role is manager.
- `services/api/src/AI/Services/AiOrchestrator.cs:131-149` returns CLARIFY/UNSUPPORTED directly. The manager pilot exception path returns a safe unavailable response and does not call Mock.
- `services/api/src/AI/Services/AiOrchestrator.cs:392-415` performs ACCEPT tool lookup, read-only check, permission check, cancellation check, and tool execution. The embedding strategy does not call a business service directly.
- `services/api/Program.cs:305-324` registers `MockAiProvider`, `ManagerEmbeddingPilot`, and the orchestrator. The provider remains the IAiProvider implementation for the regular path.
- `services/api/src/AI/Tools/AiToolRegistry.cs:29-75` is the central lookup/schema boundary.
- `services/api/src/AI/Services/AiPermissionService.cs:30-69` enforces known role, customer restrictions, branch presence for branch-scoped staff, allowed role and risk level.
- `services/api/src/AI/Tools/Admin/GetRevenueTool.cs:12-18,44-58` and `GetBusinessSummaryTool.cs:11-18,39-99` are read-only admin/manager tools and pass `userContext.BranchId` to `IDashboardService`.

### Pilot policy

- `ManagerEmbeddingPilot.IsEnabled` requires Development/Test/Testing and `AI:EmbeddingPilot:Enabled=true` (`ManagerEmbeddingPilot.cs:51-52`).
- Missing branch, empty input, negation, unsupported period and out-of-scope capability are handled before ACCEPT (`ManagerEmbeddingPilot.cs:63-76`).
- Model/tokenizer checksum validation, ONNX session initialization and fail-closed initialization are at `ManagerEmbeddingPilot.cs:133-166`.
- E5 query prefix, tokenization, attention mask, mean pooling and L2 normalization are at `ManagerEmbeddingPilot.cs:180-197`.
- ACCEPT produces only `get_revenue` or `get_business_summary` (`ManagerEmbeddingPilot.cs:95-117`).

## 2. Embedding evidence

The isolated runtime harness was rerun with:

```text
dotnet run --project .tools/runtime-pilot-verification/RuntimePilotVerification.csproj -c Release --no-restore
```

Result:

```text
SUMMARY|passed=25|failed=0|database=NOT_USED|api=NOT_STARTED
```

Relevant PASS rows include:

- `manager_revenue_today`, `manager_revenue_year`
- `manager_summary_month`, `summary_multi_field`
- `unsupported_revenue_week`, `invalid_period`
- `ood_inventory`, `negation`, `compound_ood`
- `missing_branch_fail_closed`, `missing_model_fail_closed`, `checksum_mismatch_fail_closed`
- `permission_manager_branch`, `permission_manager_missing_branch`, `permission_employee_denied`
- `real_orchestrator_registry_permission_branch_and_clarify`
- `http_api_flag_on_manager`, `http_api_flag_off_provider_path`
- `concurrent_requests_complete`, `queue_saturation_no_deadlock`

The harness also reported `cold_ms=4089.96`, warm median `7.69 ms`, warm p95 `10.29 ms), and RSS delta `493,674,496 bytes`. These are isolated process measurements and are not production capacity claims.

Backend build was rerun with:

```text
dotnet build services/api/RestaurantPOS.api.csproj --no-restore -c Release -o .pilotbuild
```

Result: PASS, 0 warnings and 0 errors.

The existing `.tools/runtime-pilot-verification/http-run.log` contains an isolated TestServer decision for `REVENUE_QUERY/get_revenue` and the 25-pass summary. It is not evidence that every user request on the live API or Admin Web used Embedding.

## 3. REVENUE_QUERY acceptance

| Check | Evidence/status |
|---|---|
| today/month/year capability | PASS in isolated policy/runtime checks; tool is `get_revenue` |
| Correct tool mapping | PASS: `ManagerEmbeddingPilot.cs:99-117`; tool source `GetRevenueTool.cs:15-18` |
| Period validation | PASS for supported periods; `GetRevenueTool.cs:48-56` rejects invalid values |
| yesterday/week | PASS as CLARIFY in isolated checks; no automatic summary fallback in pilot |
| Branch scope | PASS in fake branch/orchestrator checks; tool receives trusted context branch |
| Manual web acceptance | NOT VERIFIED per case; user reported the chat works normally, but no per-case runtime evidence was supplied |
| Research state | `PENDING_MANUAL_ACCEPTANCE`; not APPROVED |

## 4. BUSINESS_SUMMARY acceptance

| Check | Evidence/status |
|---|---|
| today/yesterday/this_week/last_week/this_month/last_month | Capability is implemented in `GetBusinessSummaryTool.cs:49-75`; isolated checks cover supported summary routing |
| Multi-field summary | PASS: `summary_multi_field` |
| Output | Tool returns revenue, order count, AOV, expenses, estimated COGS/net profit and top products (`GetBusinessSummaryTool.cs:83-98`) |
| Invalid period | PASS as CLARIFY in isolated checks; no implicit today fallback in pilot policy |
| Branch scope | PASS in fake branch/orchestrator checks |
| Manual web acceptance | NOT VERIFIED per case |
| Research state | `PENDING_MANUAL_ACCEPTANCE`; not APPROVED |

## 5. Safety and regression status

| Area | Status | Evidence |
|---|---|---|
| Manager-only gate | PASS | `ManagerEmbeddingPilot.cs:51-52,63-64`; harness manager/employee/guest rows |
| Trusted role/branch | PASS by source and harness | `AiController.cs:83-130`; branch rows |
| CLARIFY does not execute a tool | PASS | orchestrator early returns and harness |
| UNSUPPORTED does not execute a tool | PASS | orchestrator early returns and harness |
| Permission denial | PASS | `AiOrchestrator.cs:407-408`, permission harness rows |
| Inference/model failure | PASS fail-closed | `AiOrchestrator.cs:132-137`; harness rows |
| Negation | PASS in isolated checks | `ManagerEmbeddingPilot.cs:71-72` |
| Role outside pilot unaffected with flag OFF | PASS in isolated provider-path check |
| Live API log evidence | NOT VERIFIED | no live runtime log was provided |
| Browser verification | NOT RUN | explicitly assigned to user |

The user’s statement that the Admin Web chat works normally is recorded as manual smoke-test context only. It does not establish that all baseline cases, all periods, safety cases, or both tool mappings passed through the live API.

## 6. Mock AI inventory and removal classification

### A. Candidate for removal only after a later migration

- `services/api/src/AI/Services/MockAiProvider.cs`: the regular provider implementation for non-pilot paths. It can be removed only after every consumer has a replacement and a role-by-role regression proves no behavior loss.
- Mock-only keyword catalog inside `MockAiIntentRecognizer.cs`: removable only together with the provider paths that still depend on it.
- The phrase “Mock AI” in explanatory comments/messages can be cleaned up after the implementation migration; it is not a functional removal.

### B. Needs replacement before removal

- `IAiProvider` usage in `services/api/src/Application/Services/InsightExplanationService.cs:11-20,60-93`. This service still requests generated insight explanations through the provider. Removing Mock without another provider/service would break this dependency.
- `MockAiIntentRecognizer.IsPersonalOrderRequest` is used by the anonymous guest route at `AiController.cs:178-185`. This safety check needs an explicit replacement before Mock-specific recognizer code can be deleted.

### C. Must remain for current roles/capabilities

- Mock provider path remains the fallback/regular path when the pilot is OFF and for roles other than manager (`AiOrchestrator.cs:151+`, `Program.cs:303-305`).
- The current registry still exposes many non-pilot capabilities: registrations are in `Program.cs:334-351`, including staff, customer and admin tools.
- No evidence exists that admin, employee, cashier, kitchen, customer or guest QR flows have been migrated to Embedding. They must retain the current provider behavior until separately researched and approved.

### D. Shared components that must not be removed

- `AiOrchestrator`
- `IAiProvider` while `InsightExplanationService` or any other consumer remains
- `AiToolRegistry`
- `AiPermissionService` / `AiAuthorization`
- trusted JWT context and branch isolation
- confirmation boundary and read-only guard
- business tools and application services
- execution audit sink

### Keyword routing and suggested prompts

- Mock routing is deterministic normalization/rule matching in `MockAiIntentRecognizer.Recognize` (`MockAiIntentRecognizer.cs:21-180`), with normalized keyword predicates such as `IsRevenue`, `IsBusinessSummary`, and multi-intent checks (`MockAiIntentRecognizer.cs:183-220,330-345`).
- Admin and customer ChatBot components still provide free-text input and send `message/history` to `/api/Ai/chat` (`apps/admin-web/src/components/ChatBot.tsx:173-201,338`; `apps/customer-web/src/components/ChatBot.tsx:189-224,315`). Repository search found no old quick-question/suggested-prompt array in these components. The remaining “Gợi ý” occurrence is ordinary menu UI text, not an AI Chat prompt.
- `AiController.GuestChat` remains explicitly Mock-gated by Development and `AI:ProviderMode=Mock` (`AiController.cs:158-196`).

## 7. Impact by role

| Role | Current impact if Mock is removed now | Recommendation |
|---|---|---|
| manager | Two pilot intents can use Embedding only when gated; flag OFF still needs provider path | Keep Mock until explicit default-path migration and acceptance |
| admin | Existing non-pilot tools/provider behavior remains | Keep |
| employee | Existing rule-based provider path and tools remain | Keep |
| cashier | Existing provider path remains | Keep |
| kitchen | Existing authorization and guest route restrictions remain | Keep |
| customer | Customer tool restrictions and `get_my_orders` path remain | Keep |
| guest QR | `GuestChat` and personal-order safety check depend on Mock recognizer/provider mode | Keep |

## 8. Required evidence before intent approval

Both intents remain `PENDING_MANUAL_ACCEPTANCE` because the available web statement is a smoke-test statement, not per-case evidence. To mark an intent APPROVED later, retain:

1. Per-case live Admin Web/API evidence for supported and rejected periods.
2. Correlated `AI_EMBEDDING_PILOT_DECISION` logs showing decision, intent/tool, reason and duration.
3. Tool execution evidence showing trusted branch context and permission result.
4. Safety cases: negation, ambiguity, OOD, missing branch and inference failure.
5. Regression evidence matching frozen v3.3 artifacts.

## 9. Plan for eventual full Mock removal

1. Keep the current provider and recognizer while roles/capabilities remain unmigrated.
2. Define and approve a replacement contract for insight explanations and guest personal-order safety.
3. Migrate one role/capability group at a time behind a feature flag.
4. Run role, permission, branch, read-only and regression tests.
5. Remove only unused Mock-specific branches after dependency analysis and acceptance.
6. Keep rollback by restoring the previous provider registration and disabling the semantic feature flag.

## 10. Rollback

For the pilot, set `AI:EmbeddingPilot:Enabled=false` in the Development/Test process configuration and use the existing provider path. No baseline/model/dataset change is required. Do not change production configuration as part of this audit. If a future Mock removal is attempted, rollback requires restoring the `IAiProvider` registration and the affected recognizer/safety dependency.

## 11. Final conclusion

- Sentence Embedding is demonstrably wired into the manager pilot in source and passed the isolated 25/25 harness.
- The pilot uses the real orchestrator, registry, permission service and branch context in the isolated test host.
- The current evidence does not prove every live Admin Web case traversed Embedding; live logs are absent, so runtime log verification is NOT VERIFIED.
- Both `REVENUE_QUERY` and `BUSINESS_SUMMARY` remain `PENDING_MANUAL_ACCEPTANCE`, not APPROVED.
- Mock AI cannot be safely removed now. It still serves flag-OFF behavior, non-manager roles, guest behavior, and the insight explanation dependency.
- Decision for deleting old Mock AI: **NO-GO**.



## Current status / Superseded by

Historical audit; current: manager pilot retest PASS, but MockAiProvider removal remains NO-GO.
