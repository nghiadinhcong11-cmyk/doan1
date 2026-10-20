# Checklist manual — Khách QR (`guest`)

Điều kiện: có QR hợp lệ của bàn/branch và QR sai/hết hạn để kiểm tra lỗi.

| # | Checklist | Kết quả mong đợi | Trạng thái |
|---|---|---|---|
| 1 | Mở customer web không có token | Hiển thị menu guest, không tự nhận là customer | NOT RUN |
| 2 | Quét/mở QR hợp lệ | Tạo đúng guest session, branch và table | NOT RUN |
| 3 | Mở QR sai, hết hạn hoặc bàn không active | Báo lỗi an toàn; không tạo session sai | NOT RUN |
| 4 | Xem menu, giá, topping và promotion | Chỉ hiển thị dữ liệu public | NOT RUN |
| 5 | Thêm/xóa/sửa món trong giỏ | Giỏ đúng session; số lượng hợp lệ | NOT RUN |
| 6 | Gửi guest order | Order đúng branch/bàn, tổng tiền do backend tính, không duplicate | NOT RUN |
| 7 | Thử sửa branch/table/order trong URL hoặc request | Server bỏ giá trị giả và từ chối scope sai | NOT RUN |
| 8 | Mất mạng khi tải menu hoặc submit | Có loading/error/retry; retry không tạo order lặp | NOT RUN |
| 9 | Guest session hết hạn rồi customer đăng nhập | Chuyển session sạch, không trộn token/giỏ hàng sai | NOT RUN |
| 10 | Mở profile/reservation trực tiếp nếu UI cho phép | Hành vi đúng chính sách guest; không gọi API customer trái phép | NOT RUN |
| 11 | Mở chatbot guest nếu có | Chỉ đọc menu/FAQ; không có write tool | NOT RUN |
