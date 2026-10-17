# AI Reports Index

**Đối chiếu:** 17/10/2026

## Nguồn tham chiếu chính

- [AI-EMBEDDING-MANAGER-ACCEPTANCE-FINAL.md](AI-EMBEDDING-MANAGER-ACCEPTANCE-FINAL.md) — nghiệm thu manager pilot.
- [AI-REPORTS-INDEX.md](AI-REPORTS-INDEX.md) — chỉ mục này.
- [AI-ROLE-SCOPE-CLEANUP-AUDIT.md](AI-ROLE-SCOPE-CLEANUP-AUDIT.md) — phạm vi role và MockAiProvider.
- [AI-EMBEDDING-UAT-LOGGING-IMPLEMENTATION.md](AI-EMBEDDING-UAT-LOGGING-IMPLEMENTATION.md) — logging UAT.
- [AI-INTENT-RESEARCH-AND-APPROVAL-PROCESS.md](AI-INTENT-RESEARCH-AND-APPROVAL-PROCESS.md) — quy trình nghiên cứu.

## Baseline và evidence

- [AI-EMBEDDING-V33-BASELINE-MANIFEST.json](AI-EMBEDDING-V33-BASELINE-MANIFEST.json) — `manager-two-intent-v3.3`.
- [AI-EMBEDDING-2INTENT-MANAGER-V33-VERIFICATION.md](AI-EMBEDDING-2INTENT-MANAGER-V33-VERIFICATION.md).
- [AI-EMBEDDING-MANAGER-UAT-FAILURE-ANALYSIS.md](AI-EMBEDDING-MANAGER-UAT-FAILURE-ANALYSIS.md).
- [AI-EMBEDDING-RUNTIME-INTEGRATION-STRESS-VERIFICATION.md](AI-EMBEDDING-RUNTIME-INTEGRATION-STRESS-VERIFICATION.md).
- Raw live log: `services/api/logs/ai-uat/ai-chat.log`.

## Historical research

v1/v2/v3/v3.1/v3.2 và boundary review giữ nguyên kết quả lịch sử. Các file đã được bổ sung `Current status / Superseded by`; không dùng metric lịch sử như metric hiện tại.

- [AI-EMBEDDING-CURRENT-STATE-AUDIT.md](AI-EMBEDDING-CURRENT-STATE-AUDIT.md)
- [AI-EMBEDDING-SPIKE-VERIFICATION.md](AI-EMBEDDING-SPIKE-VERIFICATION.md)
- Các báo cáo `AI-EMBEDDING-2INTENT-MANAGER-*`.

## Runtime/startup

- [AI-EMBEDDING-RUNTIME-PILOT-REPORT.md](AI-EMBEDDING-RUNTIME-PILOT-REPORT.md)
- [AI-EMBEDDING-DEVELOPMENT-API-VERIFICATION.md](AI-EMBEDDING-DEVELOPMENT-API-VERIFICATION.md)
- [AI-EMBEDDING-HTTPS-STARTUP-FIX.md](AI-EMBEDDING-HTTPS-STARTUP-FIX.md)

## Research artifacts cần giữ

`.tools/mock-ai-embedding-spike/`, `.tools/runtime-pilot-verification/`, ONNX model/tokenizer, datasets, baseline manifest, raw failure evidence, UAT checklist và live log.

## Current AI scope

- Admin/manager: quản trị; semantic pilot đã xác nhận **chỉ manager và hai intent**.
- Customer/guest: giữ các capability hiện có theo permission.
- Employee/cashier/kitchen: không mở rộng semantic pilot; chỉ là future scope.
- POS/KDS nghiệp vụ độc lập với cleanup AI.

Các tài liệu đồ án/khóa luận cũ cần đọc kèm báo cáo final để phân biệt mô tả lịch sử với trạng thái hiện tại.
