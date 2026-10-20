# Checklist manual — Bếp (`kitchen`)

Điều kiện: đăng nhập tài khoản bếp, có order test ở đúng branch.

| # | Checklist | Kết quả mong đợi | Trạng thái |
|---|---|---|---|
| 1 | Đăng nhập, refresh và mở `/kitchen` | Vào đúng màn hình bếp, session giữ đúng branch | NOT RUN |
| 2 | Nhận order mới từ POS/customer | Order xuất hiện một lần, đủ món, topping và ghi chú | NOT RUN |
| 3 | Chuyển trạng thái order/món | Trạng thái lưu đúng và hiển thị nhất quán | NOT RUN |
| 4 | Mở kitchen history và lọc | Lịch sử đúng branch, lọc và phân trang đúng | NOT RUN |
| 5 | Kiểm tra realtime khi POS cập nhật order | Màn hình bếp cập nhật, không duplicate | NOT RUN |
| 6 | Mất mạng rồi kết nối lại | Có trạng thái reconnect; dữ liệu không mất hoặc nhân đôi | NOT RUN |
| 7 | Mở notifications và table status | Thông báo/bàn hiển thị đúng và xử lý được | NOT RUN |
| 8 | Mở attendance, schedule, reservations, profile | Chỉ xem/thao tác đúng quyền | NOT RUN |
| 9 | Mở URL admin/POS trực tiếp | Bị chặn đúng quyền | NOT RUN |
| 10 | API lỗi hoặc token hết hạn | Có lỗi an toàn; không lộ dữ liệu nhạy cảm | NOT RUN |
