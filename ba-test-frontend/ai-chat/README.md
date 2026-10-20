# Bộ test tay AI Chat — runtime E5 hiện tại

Bộ này là checklist manual UAT cho runtime hiện tại. Không dùng kết quả của checklist để suy ra quyền ngoài backend.

## Phạm vi runtime

| Vai trò | Endpoint | Điều kiện | Capability được phép |
|---|---|---|---|
| Admin/chủ quán | `POST /api/Ai/chat` | JWT role `admin`, `AI:AdminEmbeddingPilot:Enabled=true` | 6 intent đọc và `update_product_price` với xác nhận hai bước |
| Customer đăng nhập | `POST /api/Ai/chat` | JWT role `customer`, `customerSessionType=registered`, `AI:CustomerGuestEmbeddingPilot:Enabled=true` | `MENU_QUERY`, `MY_ORDERS` |
| Guest QR | `POST /api/Ai/guest-chat` | Guest QR session do server cấp, Development, `AI:CustomerGuestEmbeddingPilot:Enabled=true` | Chỉ `MENU_QUERY` |
| Manager/Cashier/Employee/Kitchen | Không có đường AI hợp lệ | Backend phải từ chối | Không test như capability AI |

`MockAiProvider`, `IAiProvider`, Mock fallback và Business Insights không phải runtime hiện tại. Admin đã có đúng một write tool là `update_product_price`; tool này bắt buộc xác nhận trước khi ghi. Nếu gặp tên provider cũ trong UI/log, ghi finding thay vì coi đó là runtime hiện tại.

## Quy tắc test

- Mọi case bắt đầu ở trạng thái `NOT RUN`; chỉ đổi thành `PASS`, `FAIL` hoặc `BLOCKED` sau khi có evidence.
- Ghi endpoint, HTTP status, role/session, branch scope, response và tool thực sự chạy; che JWT, cookie, connection string và dữ liệu nhạy cảm.
- `Flag OFF`, model/tokenizer/catalog lỗi hoặc guest session không hợp lệ phải fail-closed: không fallback provider và không chạy tool.
- Không tự tạo order, sửa order, thanh toán, đặt bàn, cập nhật bếp hoặc gọi Business Insights qua chat. Đổi giá chỉ được test qua luồng `update_product_price` của Admin với xác nhận rõ ràng.
- Dashboard/báo cáo POS thông thường không phải AI Chat.

## Chuẩn bị

Chuẩn bị dữ liệu test riêng cho Admin, Customer A/B, menu có category và order của Customer A; chuẩn bị QR session hợp lệ và không hợp lệ. Không dùng database thật ngoài môi trường được phép. Người dùng tự khởi động môi trường test; tài liệu này không yêu cầu Codex khởi động server.

## Cách ghi kết quả

Trong từng bảng, điền `Actual response`, `Actual intent/tool`, `Evidence` và `Status`. Với case bị chặn do thiếu dữ liệu hoặc chưa thể kiểm tra JWT/DB/SignalR, ghi `BLOCKED` và lý do cụ thể; không gọi đó là PASS.
