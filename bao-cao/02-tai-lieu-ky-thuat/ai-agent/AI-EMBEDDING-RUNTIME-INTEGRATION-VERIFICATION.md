# AI Embedding Runtime Integration Verification

Ngày: 2026-10-17
Phạm vi: harness cô lập, không khởi động API, không kết nối database/Supabase, không bật flag ở runtime đang phục vụ người dùng.

## 1. Runtime wiring

Đã kiểm tra source:

- AI Controller tạo AiUserContext từ JWT/server claims.
- AiOrchestrator kiểm tra role hợp lệ và BranchId trước khi xử lý.
- ManagerEmbeddingPilot chỉ bật khi:
  - environment là Development, Test hoặc Testing;
  - AI:EmbeddingPilot:Enabled=true;
  - role là manager;
  - BranchId tồn tại.
- Role employee, cashier, kitchen, customer và guest không vào pilot.
- Request body không có đường bật feature flag.
- Pilot chỉ chọn get_revenue hoặc get_business_summary.
- ACCEPT gọi registry lookup và AiPermissionService trước ExecuteAsync.
- CLARIFY/UNSUPPORTED trả response trước tool execution.
- Pilot không gọi DashboardService, DbContext hoặc business service trực tiếp.
- MockAiProvider vẫn được dùng khi flag OFF và bởi role ngoài pilot.

Kết quả code wiring: **PASS**.

## 2. Harness

File:

- .tools/runtime-pilot-verification/RuntimePilotVerification.csproj
- .tools/runtime-pilot-verification/Program.cs

Harness dùng:

- model/tokenizer/catalog local của baseline;
- fake AiUserContext;
- fake read-only tool cho permission checks;
- NullLogger;
- không API host;
- không database.

Lệnh:

dotnet restore .tools/runtime-pilot-verification/RuntimePilotVerification.csproj --ignore-failed-sources
dotnet run --project .tools/runtime-pilot-verification/RuntimePilotVerification.csproj -c Release --no-restore

Kết quả:

- **18 PASS**
- **0 FAIL**
- database=NOT_USED
- api=NOT_STARTED

## 3. Permission và branch

| Check | Kết quả |
|---|---|
| Manager có BranchId | PASS |
| Manager thiếu BranchId | PASS, CLARIFY/blocked |
| Employee bị chặn | PASS |
| Customer/guest không vào pilot | PASS |
| BranchId lấy từ trusted context | PASS qua context gate |
| Câu hỏi không thể cung cấp BranchId | PASS, không có parser branch |
| Fake tool manager permission | PASS |
| Fake tool thiếu BranchId | PASS, permission denied |
| Fake tool employee | PASS, permission denied |

Harness không gọi business tool thật, nên branch data isolation ở service/database là **NOT RUN**. Existing AiPermissionService và DashboardService branch filtering đã được kiểm tra source, nhưng chưa có integration test với hai fake branch data sets.

## 4. Semantic policy

ONNX model khóa v3.3 chạy thật.

| Case | Kết quả |
|---|---|
| Revenue today | PASS → get_revenue |
| Revenue year | PASS → get_revenue |
| Summary month | PASS → get_business_summary |
| Revenue unsupported week | PASS → CLARIFY, UnsupportedPeriod |
| Invalid/future period | PASS → CLARIFY, UnsupportedPeriod |
| OOD inventory | PASS → UNSUPPORTED |
| Negation | PASS → CLARIFY |
| Summary multi-field | PASS → get_business_summary |
| Revenue + menu compound | PASS → UNSUPPORTED |
| CLARIFY/UNSUPPORTED không tạo tool call | PASS ở pilot result |

Một lỗi integration được phát hiện trong lần chạy đầu: câu revenue tuần bị semantic top candidate là BUSINESS_SUMMARY và có thể ACCEPT. Nguyên nhân là capability check xảy ra sau semantic resolution. Đã sửa trong ManagerEmbeddingPilot để revenue-only với period yesterday/week bị CLARIFY trước routing; không sửa model, dataset, threshold hay backend business logic.

## 5. Provider/orchestrator contract

Code inspection xác nhận:

- ACCEPT đi qua AiToolRegistry.GetTool.
- ACCEPT kiểm tra read-only guard và AiPermissionService.
- Tool result lỗi trả AiResponse thất bại.
- CLARIFY/UNSUPPORTED không đi vào provider function-call loop.
- Pilot initialization/inference failure trả lỗi fail-closed.
- Không có fallback từ pilot sang MockAiProvider khi flag ON.
- Flag OFF giữ nguyên provider path.

Runtime API orchestration end-to-end: **NOT RUN**, vì API không được khởi động theo ràng buộc.

Confirmation boundary:

- Pilot chỉ cho phép hai tool Read; không mở write/destructive tool.
- Existing confirmation handling trong orchestrator không bị thay đổi.
- Confirmation execution end-to-end: **NOT RUN**.

## 6. Failure handling

| Case | Kết quả |
|---|---|
| Model path missing | PASS, initialization fail-closed |
| Tokenizer/checksum validation | PASS bằng code path |
| Initialization exception | PASS, không gọi tool |
| Inference exception | PASS bằng code path fail-closed |
| Cancellation trước/đang chờ | PARTIAL; token được truyền và kiểm tra trong strategy |
| Timeout | NOT RUN trong harness; code path có bounded timeout |
| Queue saturation | PARTIAL; bounded semaphore có trong code, chưa tạo tải timeout nhân tạo |
| Dispose lifecycle | NOT RUN trực tiếp |
| Process survives pilot failure | PASS theo exception handling ở orchestrator; chưa chạy API process |

Không có response nào chứa đường dẫn model, checksum hoặc stack trace nội bộ.

## 7. Benchmark có kiểm soát

Môi trường:

- Windows, .NET SDK 10.0.200.
- Target net8.0.
- ONNX Runtime 1.27.1.
- Tokenizers.HuggingFace 3.23.1.
- Một process harness, CPU local, không database.

Một phiên benchmark trong harness:

- Cold initialization + first decision: **5203.73 ms**.
- Warm inference, n=10: median **12.61 ms**.
- Warm p95: **18.49 ms**.
- RSS trước benchmark: **71,331,840 bytes**.
- RSS sau model/session load: **557,936,640 bytes**.
- RSS delta quan sát: **486,604,800 bytes**.
- Concurrent requests: 8/8 hoàn tất qua serialized inference.

Giới hạn:

- Đây là một process, một máy, một lần cold và 10 warm samples.
- RSS bao gồm runtime/JIT/native allocations, không tách riêng model.
- Chưa đo p99, nhiều worker, container limit, GC pressure hoặc API request overhead.
- Không dùng số đo này để tuyên bố production-ready.

## 8. PASS/FAIL/BLOCKED/NOT RUN

| Hạng mục | Trạng thái |
|---|---|
| Backend build | PASS |
| Harness build/run | PASS |
| Feature flag OFF default | PASS |
| Manager-only gate | PASS |
| Non-manager isolation | PASS |
| Permission fake checks | PASS |
| Branch context gate | PASS |
| ONNX inference | PASS |
| Intent/policy cases | PASS |
| No tool on CLARIFY/UNSUPPORTED | PASS at pilot boundary |
| Missing model fail-closed | PASS |
| Checksum validation | PASS by code path |
| Cancellation | PARTIAL |
| Timeout under load | NOT RUN |
| Queue saturation under load | PARTIAL |
| Dispose stress | NOT RUN |
| API end-to-end | NOT RUN |
| Database/Supabase | NOT RUN |
| Browser | NOT RUN |
| Production capacity | BLOCKED |

## 9. Lỗi còn tồn tại và phạm vi sửa

- Runtime API chưa có integration harness cho actual AiOrchestrator + fake registry/tool; hiện mới kiểm tra pilot trực tiếp và permission service fake.
- Branch A/B data isolation chưa chạy với fake business service.
- Timeout, queue saturation và dispose stress chưa có phép đo thực thi đầy đủ.
- Summary backend vẫn có default period fallback; pilot chặn trước khi gọi nhưng tool source chưa đổi.
- Pilot hiện trả response trực tiếp cho ACCEPT thay vì tạo provider function-call envelope; tool registry/permission vẫn được gọi, nhưng full provider response contract cần integration test riêng.
- Không có audit event riêng cho pilot execution trong harness.

## 10. GO/NO-GO

**NO-GO cho bật pilot trên Development/TEST ở thời điểm này.**

Cơ chế wiring, policy cơ bản, permission gate, checksum và ONNX inference đã PASS. Tuy nhiên cần hoàn tất:

1. Orchestrator integration test với fake registry/tool.
2. Branch A/B isolation test.
3. Timeout/queue/cancellation/dispose stress tests.
4. Kiểm tra audit events và failure response qua API pipeline.
5. Xác nhận người dùng thử thủ công trên web sau khi automated gates hoàn tất.

Không đánh dấu APPROVED hoặc PRODUCTION READY.



## Current status / Superseded by

Historical isolated integration verification; current: retained, superseded by later stress/live evidence.
