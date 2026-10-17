# AI Embedding UAT Logging Implementation

- Date: 2026-10-17
- Scope: Development/Test UAT diagnostics only
- Database/API chính: không khởi động, không truy cập
- Default: logging OFF
- Verification: isolated TestServer/harness

## 1. Audit hiện trạng

Các log semantic decision trước đây được tạo trong `AiOrchestrator`:

- `services/api/src/AI/Services/AiOrchestrator.cs`: `AI_EMBEDDING_PILOT_DECISION` cho ACCEPT, CLARIFY và UNSUPPORTED.
- Correlation ID trước đây được tạo bên trong orchestrator.
- `AiToolExecutionAuditSink` đã ghi lifecycle tool qua `ILogger`, nhưng chỉ ghi một tập trường an toàn và chưa gom HTTP request/response vào cùng file.
- `Program.cs` đã có `SafeAiDiagnosticFileLoggerProvider`, nhưng provider cũ chỉ nhận failure/tool audit, không có rotation cấu hình và không nhận các sự kiện UAT đầy đủ.

## 2. Thay đổi đã thực hiện

### Backend

- Thêm `AiCorrelationContext` request-scoped:
  - Controller tạo correlation ID một lần.
  - Orchestrator dùng cùng ID.
  - Không dùng correlation ID cho authentication.
- `AiController.Chat` ghi:
  - `AI_HTTP_REQUEST`
  - `AI_HTTP_RESPONSE` qua `Response.OnCompleted`
- `AiOrchestrator` bổ sung `AI_PERIOD` và `AI_REASON` trong semantic decision.
- Pilot tool execution đi qua audit sink với các trạng thái:
  - `SELECTED`
  - `AUTHORIZED`
  - `EXECUTED`
  - `DENIED`
  - `FAILED`
- `AiToolExecutionAuditSink` ghi period đã allow-list và chỉ bật trong Development/Test khi UAT logging bật.
- Không thay đổi model, tokenizer, threshold, capability contract, database hay tool nghiệp vụ.

### File logger

`services/api/src/AI/Diagnostics/SafeAiDiagnosticFileLoggerProvider.cs`:

- Chỉ ghi các event allow-list.
- Không ghi raw message, request body, exception text, JWT, header, cookie, password, connection string, khách hàng hoặc doanh thu.
- Tự tạo thư mục.
- Rotation theo kích thước.
- Mặc định: 5 MiB, giữ 3 file.
- Các trường được ghi là metadata an toàn: timestamp, correlation, route, role, feature flag, environment, decision, intent, tool, period, reason, status, policy outcome và duration.

### Cấu hình

Đã thêm vào `appsettings.json` và `appsettings.Development.json`:

```json
"AI": {
  "UatLogging": {
    "Enabled": false,
    "Path": "logs/ai-uat/ai-chat.log",
    "MaxFileBytes": 5242880,
    "RetainedFiles": 3
  }
}
```

Provider chỉ được đăng ký khi environment là Development/Test/Testing và `AI:UatLogging:Enabled=true`.

## 3. Đường dẫn log

Khi chạy từ repository root:

```text
D:\Dev\doan\logs\ai-uat\ai-chat.log
D:\Dev\doan\logs\ai-uat\ai-chat.log.1
D:\Dev\doan\logs\ai-uat\ai-chat.log.2
```

Nếu dùng path tương đối khác, path được resolve theo ContentRoot của backend.

Ví dụ event ACCEPT:

```text
UTC=... AI_EVENT=AI_EMBEDDING_PILOT_DECISION AI_CORRELATION_ID=... AI_DECISION=ACCEPT AI_INTENT=REVENUE_QUERY AI_TOOL_NAME=get_revenue AI_PERIOD=today AI_REASON=CapabilityValidated AI_DURATION_MS=...
```

Ví dụ tool:

```text
UTC=... AI_EVENT=TOOL_EXECUTION AI_CORRELATION_ID=... AI_TOOL_NAME=get_revenue AI_PERIOD=today AI_ROLE=manager AI_EXECUTION_STATE=EXECUTED AI_POLICY_OUTCOME=EXECUTED AI_BRANCH_SCOPE_MATCH=YES AI_READ_ONLY=READ AI_DURATION_MS=...
```

Không có raw câu hỏi trong các dòng trên.

## 4. Cách đối chiếu một UAT case

1. Lấy `AI_CORRELATION_ID` từ dòng `AI_HTTP_REQUEST`.
2. Lọc toàn bộ file theo ID đó.
3. Kiểm tra thứ tự:
   - request
   - decision
   - tool lifecycle nếu ACCEPT
   - response
4. Với CLARIFY/UNSUPPORTED, phải không có `EXECUTED` và không có business tool call.
5. Kiểm tra `AI_PERIOD`, `AI_REASON`, `AI_TOOL_NAME`, `AI_BRANCH_SCOPE_MATCH` và status.

## 5. Verification

### Lệnh đã chạy

```text
dotnet build services/api/RestaurantPOS.api.csproj --no-restore -c Release -o .uatloggingbuild2
dotnet run --project .tools/runtime-pilot-verification/RuntimePilotVerification.csproj -c Release --no-restore
```

### Kết quả

```text
SUMMARY|passed=27|failed=0|database=NOT_USED|api=NOT_STARTED
```

Bao gồm:

- ACCEPT manager và đúng tool.
- CLARIFY period không hỗ trợ.
- UNSUPPORTED ngoài phạm vi.
- Permission và branch isolation.
- Inference/model fail-closed.
- Concurrent requests.
- Correlation ID request → decision → tool → response.
- HTTP ACCEPT.
- HTTP CLARIFY/UNSUPPORTED không gọi tool.
- Feature flag OFF giữ provider path.
- Logging mặc định OFF.
- File creation và rotation.

Log harness thực tế được ghi tại `logs/ai-uat/harness-ai-chat.log`. Kiểm tra nội dung không khớp các mẫu Authorization, Bearer, password, secret, connection string, access token, JWT, customer hoặc raw message.

Build có 0 error và 24 warning nullable đã tồn tại trong các module khác; không phát sinh lỗi compile mới.

## 6. Giới hạn

- Chưa khởi động API chính.
- Chưa kiểm thử browser/Admin Web trong lượt này.
- Chưa kiểm tra log của process đang phục vụ người dùng.
- Rotation đã kiểm thử trong isolated logger với file 64 KiB; cấu hình runtime mặc định là 5 MiB.
- Các log tool của provider path thông thường chỉ có period khi caller cung cấp được period; pilot manager đã cung cấp period.
- Correlation ID frontend không được tin cậy; backend tự tạo và chuẩn hóa ID. Không thêm browser console logging.

## 7. Lệnh PowerShell khởi động Development HTTPS LAN

Lệnh dưới đây chỉ là hướng dẫn để người dùng tự chạy sau khi xác nhận API không đang phục vụ process khác. Nó dùng launcher HTTPS LAN hiện có, bật pilot và UAT logging cho process Development:

```powershell
$ErrorActionPreference = ''Stop''
if ([string]::IsNullOrWhiteSpace($env:DOAN_DEV_LAN_HOST)) {
  throw ''Set DOAN_DEV_LAN_HOST to the approved LAN host before starting Development.''
}
$env:AI__EmbeddingPilot__Enabled = ''true''
$env:AI__UatLogging__Enabled = ''true''
$env:AI__UatLogging__Path = ''logs/ai-uat/ai-chat.log''
$env:AI__UatLogging__MaxFileBytes = ''5242880''
$env:AI__UatLogging__RetainedFiles = ''3''
.\scripts\start-dev.ps1 -Mode Lan -LanHost $env:DOAN_DEV_LAN_HOST
```

Launcher sẽ gọi `scripts/setup-https.ps1`, kiểm tra port 5000/5173/5174, khởi động backend HTTPS cùng Admin Web và Customer Web, rồi ghi log vào thư mục trên. Không đặt các biến này trong Production.

Để tắt UAT logging ở lần chạy sau:

```powershell
$env:AI__UatLogging__Enabled = ''false''
```

Để rollback pilot mà vẫn giữ backend:

```powershell
$env:AI__EmbeddingPilot__Enabled = ''false''
```

## 8. Kết luận

- Cơ chế logging tập trung đã dùng lại `ILogger`, không thêm package.
- Metadata request, semantic decision, tool execution và response dùng chung correlation ID.
- CLARIFY/UNSUPPORTED không tạo tool execution.
- Logging mặc định OFF và chỉ mở trong Development/Test.
- Có giới hạn file và rotation.
- Chưa có thao tác với API chính, database hoặc browser.



## Current status / Superseded by

Logging implementation evidence; current: UAT logging is active and live correlation evidence is recorded in final acceptance report.
