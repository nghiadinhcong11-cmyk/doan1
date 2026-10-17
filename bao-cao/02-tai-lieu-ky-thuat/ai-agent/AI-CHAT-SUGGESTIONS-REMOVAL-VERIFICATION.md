# Bàn giao dọn câu gợi ý AI Chat và quy trình từng intent

**Thời điểm:** 2026-10-17. **HEAD khi bắt đầu:** `38bbd423b963aaf5779d4ae3356f942ff236643b`. Working tree đã dirty, gồm thay đổi sẵn trong cả hai `ChatBot.tsx`. Các đoạn quản lý context theo token, request đang chạy, response lỗi và confirmation có sẵn trong working tree đã được giữ nguyên.

## Phạm vi đã kiểm tra trước thay đổi

| Nguồn câu | Vị trí | Vai trò và kết luận |
|---|---|---|
| Nút gợi ý Admin UI | `apps/admin-web/src/components/ChatBot.tsx`, `getSuggestions`, JSX `getSuggestions().map` | Đổi bộ câu theo pathname `/pos`, `/dashboard`, `/settings`, fallback. Click gọi `handleSend(s)`. **Đã gỡ** function, nút và `useLocation` chỉ phục vụ function đó. |
| Nút gợi ý Customer UI | `apps/customer-web/src/components/ChatBot.tsx`, `getSuggestions`, JSX `getSuggestions().map` | Bốn câu cố định. Click gọi `handleSend(s)`. **Đã gỡ** function và nút. |
| Lời chào Customer | `apps/customer-web/src/components/ChatBot.tsx`, `CUSTOMER_WELCOME` và initial message | Lời chào cũ nêu đích danh menu/giá. Đổi thành lời mời nhập câu hỏi trung tính, dùng cùng constant cho initial/reset. |
| Mock intent rules | `services/api/src/AI/Services/MockAiIntentRecognizer.cs`, `MockAiProvider.cs` | Phục vụ nhận diện runtime, **không sửa**. |
| Embedding spike | `.tools/mock-ai-embedding-spike/IntentCatalog.cs`, `calibration*.json`, `evaluation-EMB-E01-E30.json` | Dữ liệu nghiên cứu/lịch sử, **không sửa/xóa**. |
| Case kiểm thử | `ba-test-frontend/ai-chat/` | Checklist, không phải nút gợi ý UI; **không sửa/xóa**. |

Tìm trong `apps/admin-web/src` và `apps/customer-web/src` chỉ thấy hai `getSuggestions` của ChatBot cho cơ chế gợi ý AI. Không thấy API hoặc file cấu hình frontend cấp câu gợi ý cho hai component. Direct consumers: Admin `ManagementRoutes.tsx`, `StaffRoutes.tsx`, `CashierRoutes.tsx`; Customer `App.tsx`. Thay đổi chỉ nằm trong ChatBot, nên các route tiếp tục render component như trước. `handleSend`, input/form, request endpoint, history, response/error/typing và confirmation của Admin không đổi.

## File thay đổi trong lượt này

1. `apps/admin-web/src/components/ChatBot.tsx`: bỏ danh sách gợi ý theo route, dải nút chọn nhanh và imports chỉ còn phục vụ phần đó. Giữ ô nhập tự do và `onSubmit → handleSend(input)`.
2. `apps/customer-web/src/components/ChatBot.tsx`: bỏ danh sách gợi ý cố định, dải nút chọn nhanh và imports thừa; lời chào trung tính. Giữ ô nhập, gửi chat JWT/guest và phản hồi/lỗi.
3. `bao-cao/02-tai-lieu-ky-thuat/ai-agent/AI-INTENT-RESEARCH-AND-APPROVAL-PROCESS.md`: quy trình một intent một lần, hồ sơ role/tool/scope/parameters, development/test độc lập, gate phê duyệt, baseline và regression.
4. Báo cáo này.

Không thay đổi backend, provider, orchestrator, tool registry, role, database, migration, model, spike hoặc dữ liệu kiểm thử. Không chọn role/hai intent và không tạo câu tham chiếu mới.

## Verification

| Kiểm tra | Trạng thái | Bằng chứng và giới hạn |
|---|---|---|
| Admin Web `npm run build` (`tsc && vite build`) | **PASS** | Exit 0; 2,182 modules; Vite built in 12.91s. Có cảnh báo bundle lớn hơn 500 kB đã tồn tại ở đường build, không phải lỗi build. |
| Customer Web `npm run build` (`tsc && vite build`) | **PASS** | Exit 0; 1,279 modules; Vite built in 6.25s. |
| Tìm lại câu/nút cũ trong hai ChatBot source và `dist` | **PASS** | `rg` cho `getSuggestions` và toàn bộ câu gợi ý cũ không trả match. |
| Đường nhập/gửi/nhận/lỗi trong source | **INSPECTED** | Cả hai vẫn có `messages.map`, form `onSubmit → handleSend(input)`, `fetch`, `data.success === false`, catch hiển thị lỗi; Customer còn chọn `/api/Ai/chat` hoặc `/api/Ai/guest-chat`. Đây là kiểm tra source, không phải thực thi backend. |
| UI nhìn trực tiếp trong browser | **BLOCKED** | Vite Customer đã khởi động cục bộ tại `https://127.0.0.1:5177/`, nhưng browser inventory trả `apps:[]`, `browsers:[]` và in-app browser báo không khả dụng. Dev server đã dừng; không có ảnh/chứng cứ UI thực tế. |
| Gửi/nhận chat với backend thật | **NOT RUN** | Không chạy API/database; không gửi request chat. Không tuyên bố chức năng E2E PASS. |
| Lint | **NOT RUN** | Cả hai `package.json` không có lint script; build đã chạy TypeScript compiler. |
| Diff whitespace | **PASS** | `git diff --check` trên hai ChatBot không phát hiện lỗi; Git chỉ in cảnh báo chuyển LF/CRLF cho Admin. |

Không cài dependency. `npm run dev` Customer lần đầu bị sandbox `spawn EPERM`, lần chạy với quyền được cấp đã khởi động Vite thành công; chặn cuối cùng là không có browser surface. Build frontend có tạo output `dist` theo script hiện có, nhưng không sửa dependency hay source khác.

## Kết luận và bước tiếp theo

Bộ câu gợi ý cũ đã được gỡ khỏi hai AI Chat ở source và bundle build. Đường nhập chat tự do được giữ nguyên và TypeScript/Vite build thành công; UI trực quan và E2E chat vẫn chưa được xác minh trong phiên này. Quy trình nghiên cứu đã được chuẩn bị, **chưa có intent nào APPROVED**. Role, phạm vi intent đầu tiên và tiêu chí chấp nhận cần được người dùng quyết định trước khi tạo dữ liệu thí nghiệm; không tự triển khai intent mới.


## Current status / Superseded by

Historical UI cleanup; current: suggestion removal remains valid. Superseded for AI runtime status by AI-EMBEDDING-MANAGER-ACCEPTANCE-FINAL.md.
