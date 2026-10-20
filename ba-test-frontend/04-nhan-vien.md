# Checklist manual — Nhân viên (`employee`)

Điều kiện: đăng nhập nhân viên ở đúng branch.

| # | Checklist | Kết quả mong đợi | Trạng thái |
|---|---|---|---|
| 1 | Đăng nhập, refresh, mở `/staff` | Session và trang tổng quan đúng role | NOT RUN |
| 2 | Mở `/staff/sales` và tạo order test | Chọn món, topping, bàn và ghi chú đúng | NOT RUN |
| 3 | Sửa/hủy order theo quyền | Backend giữ đúng trạng thái; UI báo lỗi khi không được phép | NOT RUN |
| 4 | Mở `/staff/tables` | Bàn và trạng thái bàn đúng branch | NOT RUN |
| 5 | Mở inventory và notifications | Dữ liệu tải đúng; thông báo đánh dấu được | NOT RUN |
| 6 | Mở attendance, schedule và profile | Hiển thị đúng thông tin cá nhân/ca | NOT RUN |
| 7 | Refresh khi API lỗi hoặc token hết hạn | Có loading/error/login lại an toàn | NOT RUN |
| 8 | Thử URL ngoài quyền như settings/admin | Bị chặn, không gọi thành công endpoint nhạy cảm | NOT RUN |
| 9 | Kiểm tra order mới từ customer/POS | Realtime hiển thị đúng, không duplicate | NOT RUN |
