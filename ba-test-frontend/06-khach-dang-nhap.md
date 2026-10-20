# Checklist manual — Khách đăng nhập (`customer`)

Điều kiện: có tài khoản customer và branch/menu test.

| # | Checklist | Kết quả mong đợi | Trạng thái |
|---|---|---|---|
| 1 | Đăng nhập, refresh, đăng xuất | Session chuyển đúng; không lộ dữ liệu account khác | NOT RUN |
| 2 | Mở menu và lọc danh mục | Món, giá, availability và topping đúng API | NOT RUN |
| 3 | Thêm món/topping vào giỏ, sửa số lượng | Giá/tổng hiển thị đúng; quantity không âm/không vượt giới hạn | NOT RUN |
| 4 | Gửi order dine-in/mang về | Order tạo đúng branch/bàn và xuất hiện đúng trạng thái | NOT RUN |
| 5 | Mở lịch sử và chi tiết order | Chỉ thấy order của mình; chi tiết khớp backend | NOT RUN |
| 6 | Mở profile và sửa thông tin hợp lệ | Lưu qua API, reload vẫn đúng | NOT RUN |
| 7 | Đặt bàn và kiểm tra dữ liệu thiếu | Reservation hợp lệ tạo được; form báo lỗi rõ | NOT RUN |
| 8 | Mở scan/QR hợp lệ và QR sai | QR đúng tạo đúng context; QR sai bị từ chối | NOT RUN |
| 9 | Thử URL/order/profile của customer khác | API từ chối, không lộ hoặc sửa được dữ liệu | NOT RUN |
| 10 | Token hết hạn, API lỗi, refresh giữa lúc submit | Chuyển trạng thái an toàn; không gửi order lặp | NOT RUN |
| 11 | Mở chatbot customer nếu có | Chỉ trả capability read-only theo quyền | NOT RUN |
