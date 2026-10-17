# AI Embedding Runtime Pilot Report

Ngày: 2026-10-17
Phạm vi: Development/TEST, role manager, hai intent nghiên cứu.
Trạng thái: **IMPLEMENTED BEHIND FEATURE FLAG; NOT PRODUCTION_APPROVED**

## 1. Thay đổi source

- services/api/RestaurantPOS.api.csproj
  - thêm Microsoft.ML.OnnxRuntime 1.27.1.
  - thêm Tokenizers.HuggingFace 3.23.1.
- services/api/src/AI/Services/ManagerEmbeddingPilot.cs
  - semantic strategy riêng cho baseline v3.3.
  - lazy-load tokenizer/session/catalog một lần.
  - checksum model/tokenizer trước khi load.
  - bounded admission, cancellation và timeout.
  - manager-only decision: ACCEPT, CLARIFY, UNSUPPORTED.
  - capability/period/OOD/negation validation.
  - fail closed khi artifact lỗi hoặc inference unavailable.
- services/api/src/AI/Services/AiOrchestrator.cs
  - gọi pilot sau trusted AiUserContext và role/branch validation.
  - ACCEPT đi qua AiToolRegistry và AiPermissionService trước khi ExecuteAsync.
  - CLARIFY/UNSUPPORTED không gọi tool.
  - pilot không chạy cho role khác hoặc khi flag OFF.
- services/api/Program.cs
  - đăng ký ManagerEmbeddingPilot singleton.
- appsettings.json và appsettings.Development.json
  - thêm AI:EmbeddingPilot, mặc định Enabled=false.
  - đường dẫn model/tokenizer/catalog chỉ trỏ local artifact.

MockAiProvider chưa bị xóa và vẫn là đường mặc định khi flag OFF.

## 2. Runtime flow

Khi flag OFF:

User → AiController → AiOrchestrator → MockAiProvider → existing registry/permission/tool flow.

Khi flag ON trong Development/TEST và Role=manager:

User → trusted AiUserContext/BranchId validation → ManagerEmbeddingPilot → capability decision → AiToolRegistry → AiPermissionService → get_revenue hoặc get_business_summary.

- ACCEPT chỉ tạo một tool decision trong pilot.
- CLARIFY trả yêu cầu làm rõ và dừng trước tool execution.
- UNSUPPORTED trả thông báo ngoài phạm vi và dừng.
- Inference error trả lỗi fail-closed; không fallback sang keyword recognizer.
- Role khác giữ provider hiện tại.
- Role/BranchId không lấy từ nội dung chat.

## 3. Feature flag và baseline

Configuration:

- AI:EmbeddingPilot:Enabled = false.
- Environment gate: Development, Test hoặc Testing.
- Model: model_O4.onnx, checksum được kiểm tra tại startup lazy-load.
- Tokenizer checksum được kiểm tra.
- Catalog là reference v3 đã khóa.
- Timeout mặc định: 1500 ms.
- Admission capacity: 3 (one inference slot plus bounded waiters).

Baseline manifest không bị sửa:

AI-EMBEDDING-V33-BASELINE-MANIFEST.json vẫn ghi RESEARCH_BASELINE_LOCKED và productionApproved=false.

## 4. Capability and authorization

- get_revenue: today/month/year, role admin/manager, Read, BranchId từ trusted context.
- get_business_summary: today/yesterday/this_week/last_week/this_month/last_month, role admin/manager, Read, BranchId từ trusted context.
- Revenue month mapping converts the semantic this_month period to tool argument month.
- Invalid revenue periods are CLARIFY; no summary fallback.
- Summary period is validated before ExecuteAsync, preventing its existing backend default-to-today branch from being used accidentally.
- Pilot does not access DashboardService or database directly.
- Existing AiPermissionService remains the authorization gate.
- Existing branch validation in AiOrchestrator and tool/service context remains authoritative.
- Spike metadata is not used as a replacement for authorization.

## 5. ONNX lifecycle and resources

- Model and tokenizer are loaded lazily once per singleton ManagerEmbeddingPilot.
- Session and tokenizer are reused across requests.
- Inference is serialized with one slot; admission is bounded.
- Cancellation is honored while waiting and before/after synchronous ONNX inference.
- Timeout returns a fail-closed error; native inference is not interrupted mid-call.
- Dispose releases session, tokenizer and semaphores.

Historical spike measurements only:

- Warm median about 5.422 ms.
- Working set about 616.8 MB; another local lifecycle observation recorded approximately 627,376,128 bytes.
- These values are not production capacity guarantees. No target-backend concurrency or cold-start benchmark was run in this task.

## 6. Verification

Command:

dotnet build services/api/RestaurantPOS.api.csproj --no-restore -c Release -o .pilotbuild

Result: **PASS**, 0 errors. Existing repository nullable warnings remain.

Not run:

- API runtime startup.
- Database connection.
- Supabase TEST/MAIN.
- Browser/manual web test.
- Permission/branch integration tests.
- Model-enabled end-to-end request.

Reason: the task constraints prohibit database/browser activity, and no isolated API test harness for these new paths exists in the repository.

Code-level checks completed by inspection:

- Flag default is OFF.
- Manager role is required.
- BranchId is required for branch-scoped manager.
- Only two pilot tools can be selected.
- Tool Registry lookup and AiPermissionService gate run before execution.
- CLARIFY, UNSUPPORTED and initialization failure do not execute tools.
- Model/tokenizer checksum is enforced.
- Mock provider remains available when disabled.

## 7. Other AI functionality

When the pilot is OFF, all existing roles and intents use MockAiProvider unchanged.

When enabled for manager, the pilot intercepts the manager request before MockAiProvider; it is deliberately limited to the two semantic intents. Other manager requests are returned as pilot UNSUPPORTED/CLARIFY rather than silently falling back to Mock keyword routing.

All non-manager roles continue through the existing Mock path.

## 8. Rollback

1. Set AI:EmbeddingPilot:Enabled=false.
2. Restart the API process so the existing scoped Mock provider path is used.
3. If required, remove the pilot service registration and package references in a separate rollback change.
4. Do not change Tool Registry, permission service, backend tools or database.
5. Preserve logs and pilot artifacts for diagnosis.

No production configuration was changed.

## 9. Readiness

Status: **BLOCKED for limited runtime rollout until controlled runtime tests pass.**

The code path is isolated and fail-closed, and the backend builds. Before enabling in Development/TEST, add/run an isolated test harness covering:

- manager ACCEPT for both tools;
- invalid period, negation and OOD;
- no function call for CLARIFY/UNSUPPORTED;
- manager branch isolation;
- non-manager unchanged behavior;
- cancellation, timeout, queue saturation and unavailable model;
- audit events and rollback.

A user can then perform manual web checks after those automated/runtime checks. This report does not mark either intent APPROVED or PRODUCTION_APPROVED.



## Current status / Superseded by

Historical runtime pilot report; current: manager-only pilot remains Development/Test gated.
