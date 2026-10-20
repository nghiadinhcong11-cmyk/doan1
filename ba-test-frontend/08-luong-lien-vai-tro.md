# Checklist manual — Luồng liên vai trò

Đây là checklist bổ sung khi cần kiểm tra một luồng đi qua nhiều role; không thay thế checklist role.

| # | Luồng | Bước kiểm tra | Kết quả mong đợi | Trạng thái |
|---|---|---|---|---|
| 1 | Customer/guest → bếp → thu ngân | Gửi order, nhận ở bếp, cập nhật trạng thái, thanh toán | Order đi đúng branch và trạng thái nhất quán | NOT RUN |
| 2 | POS → bếp | Tạo/sửa order từ POS rồi mở KDS | KDS nhận đúng món, topping, ghi chú và realtime | NOT RUN |
| 3 | Admin đổi giá → POS/customer | Đổi giá, refresh các màn hình, tạo order mới | Giá mới được đọc từ backend; order cũ không bị đổi sai | NOT RUN |
| 4 | Admin tạo món/topping → catalog/POS | Tạo qua AI, xác nhận, refresh catalog/POS | Item tạo một lần, ban đầu chưa bán tại branch nếu đúng rule | NOT RUN |
| 5 | Thanh toán → invoice/report | Thanh toán order rồi xem invoice/dashboard | Số tiền persisted khớp payment, receipt và report | NOT RUN |
| 6 | Branch A/B | Đổi branch và lặp lại luồng | Không đọc hoặc mutate dữ liệu branch khác | NOT RUN |
