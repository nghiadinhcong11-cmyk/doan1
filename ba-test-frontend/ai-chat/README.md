# Bộ test tay AI Chat — runtime E5 hiện tại

Bộ này là checklist manual UAT cho runtime hiện tại. Không dùng kết quả của checklist để suy ra quyền ngoài backend.

## Phạm vi runtime

| Vai trò | Endpoint | Điều kiện | Capability được phép |
|---|---|---|---|
| Admin/chủ quán | `POST /api/Ai/chat` | JWT role `admin`, `AI:AdminEmbeddingPilot:Enabled=true` | Intent đọc gồm doanh thu, KPI, so sánh doanh thu, món bán chạy, số đơn, trạng thái đơn và thực đơn; intent ghi gồm cập nhật giá/tên món, tạo món và tạo topping qua bước xác nhận |
| Customer đăng nhập | `POST /api/Ai/chat` | JWT role `customer`, `customerSessionType=registered`, `AI:CustomerGuestEmbeddingPilot:Enabled=true` | `MENU_QUERY`, `MY_ORDERS` |
| Guest QR | `POST /api/Ai/guest-chat` | Guest QR session do server cấp, Development, `AI:CustomerGuestEmbeddingPilot:Enabled=true` | Chỉ `MENU_QUERY` |
| Manager/Cashier/Employee/Kitchen | Không có đường AI hợp lệ | Backend phải từ chối | Không test như capability AI |

`MockAiProvider`, `IAiProvider`, Mock fallback và Business Insights không phải runtime hiện tại. Các thao tác ghi của Admin phải qua kiểm tra quyền tại tool; luồng xem trước yêu cầu xác nhận trước khi ghi. Trong lượt retest 02/11/2026, người dùng đã xác nhận thành công việc tạo một món và cập nhật giá một món. Chưa có minh chứng kiểm thử từ chối một tài khoản không đủ quyền.

Các checklist theo vai trò và kết quả kiểm chứng được ghi tại: [Admin](01-chu-quan.md), [Customer đăng nhập](02-khach-dang-nhap.md), [Guest QR](03-khach-qr.md) và [PL.6 - Kết quả kiểm chứng E5 Admin](PL6-kiem-chung-E5.md). Kết quả của Customer/Guest được theo dõi riêng, không gộp vào PL.6 Admin.

## Quy tắc test

- Mọi case bắt đầu ở trạng thái `NOT RUN`; chỉ đổi thành `PASS`, `FAIL` hoặc `BLOCKED` sau khi có evidence.
- Ghi endpoint, HTTP status, role/session, branch scope, response và tool thực sự chạy; che JWT, cookie, connection string và dữ liệu nhạy cảm.
- `Flag OFF`, model/tokenizer/catalog lỗi hoặc guest session không hợp lệ phải fail-closed: không fallback provider và không chạy tool.
- Không tự tạo order, sửa order, thanh toán, đặt bàn, cập nhật bếp hoặc gọi Business Insights qua chat. Thao tác đổi giá, đổi tên, tạo món và tạo topping chỉ được kiểm thử qua các tool Admin tương ứng, với xác nhận rõ ràng trước khi ghi.
- Dashboard/báo cáo POS thông thường không phải AI Chat.

## Chuẩn bị

Chuẩn bị dữ liệu test riêng cho Admin, Customer A/B, menu có category và order của Customer A; chuẩn bị QR session hợp lệ và không hợp lệ. Không dùng database thật ngoài môi trường được phép. Người dùng tự khởi động môi trường test; tài liệu này không yêu cầu Codex khởi động server.

## Cách ghi kết quả

Trong từng bảng, điền `Actual response`, `Actual intent/tool`, `Evidence` và `Status`. Với case bị chặn do thiếu dữ liệu hoặc chưa thể kiểm tra JWT/DB/SignalR, ghi `BLOCKED` và lý do cụ thể; không gọi đó là PASS.
