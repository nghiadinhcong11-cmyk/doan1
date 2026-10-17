# AI Embedding Manager UAT Failure Analysis

**Audit date:** 2026-10-17
**Repository:** `D:\Dev\doan`
**Baseline:** `manager-two-intent-v3.3`
**Scope:** manager pilot only. No database, production API, browser, migration or MockAiProvider changes.

## 1. Initial manual UAT

The reported observations were:

| ID | Input | Expected | Manual result |
|---|---|---|---|
| M-R03 | Tháng này nhà hàng bán được bao nhiêu tiền? | ACCEPT → `get_revenue(month)` | CLARIFY |
| M-R04 | Báo cáo doanh thu năm nay giúp tôi. | ACCEPT → `get_revenue(year)` | CLARIFY |
| M-S02 | Tóm tắt các chỉ số kinh doanh của hôm qua. | ACCEPT → `get_business_summary(yesterday)` | Reported as this_week; unverified |
| M-S05 | Tình hình kinh doanh tháng này thế nào? | ACCEPT → `get_business_summary(this_month)` | CLARIFY |
| M-X01 | Cho tôi xem danh sách món ăn và giá bán. | UNSUPPORTED | CLARIFY |

The live log file `logs/ai-uat/ai-chat.log` was not present. Therefore no manual request could be linked to a correlation ID.

Status: **LIVE_LOG_NOT_AVAILABLE**.

## 2. Evidence and isolated reproduction

The five inputs were added verbatim to the development harness in `.tools/runtime-pilot-verification/Program.cs:66-70`. The diagnostic rows include decision, intent, tool, parsed period, reason, top score, margin and serialized arguments.

### Before fix

| ID | Decision | Intent/tool | Period | Reason | Top / margin | Classification |
|---|---|---|---|---|---:|---|
| M-R03 | CLARIFY | none | unknown | UnsupportedPeriod | 0.899871 / 0.028871 | Deterministic capability-policy error |
| M-R04 | CLARIFY | none | unknown | UnsupportedPeriod | 0.917347 / 0.004377 | Deterministic output-parser/policy error |
| M-S02 | ACCEPT | BUSINESS_SUMMARY → `get_business_summary` | yesterday | CapabilityValidated | 0.955589 / 0.018101 | Backend result correct; manual evidence unavailable |
| M-S05 | CLARIFY | none | unknown | CapabilityContractMismatch | 0.935475 / 0.010861 | Deterministic intent/capability resolution error |
| M-X01 | CLARIFY | none | unknown | MissingPeriod | 0 / 0 | Deterministic OOD boundary ordering error |

The pre-fix M-S02 result is direct isolated inference evidence. It does not prove what happened in the user’s browser request, because the corresponding live correlation log is missing.

## 3. Root causes

### M-R03 — month

**Root cause:** period parsing returns `this_month`, while the revenue capability check accepted only `today`, `month`, `year` before converting the internal period to the tool period. The existing conversion `ToToolPeriod(REVENUE_QUERY, this_month) → month` was therefore unreachable for validation.

**Fix:** validate the normalized tool period before accepting. The tool argument remains `period=month`; the policy evidence retains `this_month`.

### M-R04 — year

**Root cause:** the generic word “báo cáo” expanded required outputs to the entire business-summary field set, even when the phrase explicitly said “doanh thu”. The request was then incompatible with the revenue-only/year contract and was clarified.

**Fix:** “báo cáo” adds summary fields only when it is not accompanied by a direct revenue phrase. “Báo cáo doanh thu …” remains revenue-only. No sentence-specific rule was added.

### M-S02 — yesterday

**Root cause:** no confirmed backend error. Isolated inference produced exactly `BUSINESS_SUMMARY`, `get_business_summary`, `yesterday`, `CapabilityValidated`, with top score 0.955589 and margin 0.018101.

**Classification:** **evidence gap / possible manual copy mismatch**, not a demonstrated semantic or policy defect. The live case remains unverified until its correlation ID is available.

### M-S05 — this_month

**Root cause:** semantic candidate was revenue while the parser detected summary outputs from “tình hình kinh doanh”. The previous policy rejected this as `CapabilityContractMismatch` instead of resolving the candidate to the capability that supports the requested fields.

**Fix:** when required outputs include fields other than revenue, resolve a revenue candidate to `BUSINESS_SUMMARY`. This is a capability composition rule, not a threshold change.

### M-X01 — menu and prices

**Root cause:** “danh sách món ăn và giá bán” was not in the pilot’s out-of-scope boundary list. Because no period was found, the request reached `MissingPeriod` and returned CLARIFY.

**Fix:** add general OOD capability phrases for menu/item/price-list requests. OOD is evaluated before period validation, so the result is UNSUPPORTED without inference or tool execution.

## 4. Source changes

Changed only:

- `services/api/src/AI/Services/ManagerEmbeddingPilot.cs`
  - Normalize `this_month` to revenue tool period `month` before capability validation.
  - Resolve summary-output requests away from a revenue candidate.
  - Keep “báo cáo doanh thu” revenue-only.
  - Expand the general OOD boundary for menu/item/price requests.
- `.tools/runtime-pilot-verification/Program.cs`
  - Added five regression rows with expected decision, intent, tool and period.
  - Added diagnostic score/margin/argument evidence.

No model, tokenizer, checksum, reference catalog, threshold, role policy, permission service, branch logic, MockAiProvider or customer/guest path was changed.

## 5. Verification

| Check | Result | Evidence |
|---|---|---|
| Five-case regression after fix | **PASS** | M-R03, M-R04, M-S02, M-S05, M-X01 all pass |
| Runtime integration harness | **PASS** | 32 passed, 0 failed; database NOT_USED; API NOT_STARTED |
| Safety regression | **PASS** | negation, ambiguity/missing branch, OOD, unsupported period, compound request, permission and branch checks |
| ONNX lifecycle checks | **PASS** | adapter checks: all lifecycle/concurrency/cancellation fields true |
| Existing embedding decision review | **PASS / informational** | 7 correct accepts, 4 valid deferred, 13 correct non-accepts |
| Backend build | **PASS** | sequential build to `.uat-failure-build`, 0 warnings, 0 errors |
| Isolated UAT event correlation | **PASS** | Harness HTTP checks emitted the expected correlation sequence in `harness-ai-chat.log` |
| Live UAT log correlation | **NOT VERIFIED** | `logs/ai-uat/ai-chat.log` absent |
| Browser retest | **NOT RUN** | user-owned manual UAT |
| Database/API production verification | **NOT RUN** | explicitly excluded |

One initial build attempt ran concurrently with the harness and hit a file-lock on `obj/Release/net8.0/RestaurantPOS.api.dll`. A sequential rebuild passed; this was an environment/process contention issue, not a compiler error.

## 6. Baseline impact

Baseline v3.3 remains intact:

- E5 ONNX model and tokenizer unchanged.
- Frozen reference data unchanged.
- Thresholds and semantic scorer unchanged.
- Intent IDs and tool mappings unchanged.

The changes are limited to deterministic capability/parameter policy around the frozen semantic candidate. They do not claim broader semantic understanding.

M-S02 remains **not manually verified** because no live correlation evidence exists.

## 7. Remaining issues and v3.4 boundary

- Live UAT logging must be captured for any claim about a specific browser request.
- The manager pilot still has no production approval and remains Development/Test gated.
- M-R04 has a narrow semantic margin (0.004377) even though the explicit revenue capability now resolves safely; any margin policy change belongs to a separately evaluated v3.4 candidate.
- Broader lexical coverage should be evaluated as a candidate v3.4 dataset/policy change, not silently added to the frozen baseline.
- No live database or business-data correctness was tested in this task.

## 8. Next manual web tests

After the user restarts the Development backend to load the source changes:

1. “Tháng này nhà hàng bán được bao nhiêu tiền?” → `ACCEPT/get_revenue/month`.
2. “Báo cáo doanh thu năm nay giúp tôi.” → `ACCEPT/get_revenue/year`.
3. “Tóm tắt các chỉ số kinh doanh của hôm qua.” → `ACCEPT/get_business_summary/yesterday`.
4. “Tình hình kinh doanh tháng này thế nào?” → `ACCEPT/get_business_summary/this_month`.
5. “Cho tôi xem danh sách món ăn và giá bán.” → `UNSUPPORTED`, no tool execution.
6. Repeat one negation, one unsupported revenue period and one compound request.
7. Correlate each request through `AI_HTTP_REQUEST` → `AI_EMBEDDING_PILOT_DECISION` → `AI_TOOL_EXECUTION` (only for ACCEPT) → `AI_HTTP_RESPONSE`.

Do not paste raw chat, JWT, customer data or revenue into logs or the report.

## 9. Decision

**GO for continued isolated and Development UAT after the user reloads the backend.**

**NO-GO for UAT approval or production rollout** until the five web cases are retested with correlation logs, especially M-S02.



## Current status / Superseded by

Historical initial UAT failure analysis; current: five regression cases PASS and live retest resolved. Superseded by final acceptance report; original evidence retained.
