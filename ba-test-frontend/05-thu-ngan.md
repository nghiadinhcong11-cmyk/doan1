# Checklist manual — Thu ngân (`cashier`)

Điều kiện: đăng nhập thu ngân, có món/topping và order test.

| # | Checklist | Kết quả mong đợi | Trạng thái |
|---|---|---|---|
| 1 | Đăng nhập, refresh và mở `/pos` | Session/branch đúng; POS tải đủ dữ liệu | NOT RUN |
| 2 | Tạo order nhiều món, topping và số lượng | Subtotal, VAT, phí và total đúng backend | NOT RUN |
| 3 | Sửa món, xóa món, đổi bàn hoặc ghi chú | UI cập nhật đúng; không gửi duplicate request | NOT RUN |
| 4 | Thanh toán tiền mặt | Order chuyển đúng trạng thái, số tiền khớp | NOT RUN |
| 5 | Thanh toán QR/VietQR | QR đúng số tiền backend; callback/trạng thái đúng | NOT RUN |
| 6 | Thanh toán lỗi, hủy hoặc retry | Không ghi nhận thanh toán sai hoặc hai lần | NOT RUN |
| 7 | Mở `/pos/invoices`, xem/in hóa đơn | Hóa đơn khớp order đã lưu | NOT RUN |
| 8 | Mở notifications, reservations, profile | Dữ liệu và quyền đúng branch | NOT RUN |
| 9 | Mở attendance, schedule, shifts | Chức năng cá nhân/ca hoạt động đúng | NOT RUN |
| 10 | Mở settings/URL ngoài quyền | Bị chặn đúng quyền | NOT RUN |
| 11 | Refresh khi API lỗi hoặc token hết hạn | Thông báo an toàn; không mất order đã lưu | NOT RUN |
