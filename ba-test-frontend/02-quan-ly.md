# Checklist manual — Quản lý (`manager`)

Điều kiện: đăng nhập manager được gán branch. Mọi dữ liệu phải nằm trong branch được cấp quyền.

| # | Checklist | Kết quả mong đợi | Trạng thái |
|---|---|---|---|
| 1 | Đăng nhập, refresh và mở dashboard | Session giữ đúng role/branch | NOT RUN |
| 2 | Xem dashboard và business insights | Chỉ thấy KPI/dữ liệu branch của mình | NOT RUN |
| 3 | Xem/sửa món, topping, promotion và customer | Thao tác được phép lưu đúng; không vượt branch | NOT RUN |
| 4 | Xem employees, attendance, schedule, shifts | Danh sách và bộ lọc đúng branch; quyền đúng | NOT RUN |
| 5 | Xem tables và reservations | Bàn/đặt bàn đúng branch, không tạo duplicate | NOT RUN |
| 6 | Mở POS, tạo order và thanh toán test | Tổng tiền backend đúng; payment thành công đúng trạng thái | NOT RUN |
| 7 | Xem invoices, expenses và inventory | Dữ liệu và bộ lọc đúng branch | NOT RUN |
| 8 | Mở kitchen/history và kiểm tra cập nhật trạng thái | Realtime/lịch sử hiển thị đúng quyền | NOT RUN |
| 9 | Mở settings, branches, print templates trực tiếp | Bị từ chối hoặc chỉ thấy chức năng được phép | NOT RUN |
| 10 | Thử URL/resource của branch khác | API/UI từ chối, không lộ dữ liệu | NOT RUN |
| 11 | Token hết hạn, API lỗi, refresh giữa lúc tải | Có thông báo an toàn, không lộ token/stack trace | NOT RUN |
| 12 | Mở chatbot nếu role được phép | Chỉ chạy capability được cấp; write phải cần xác nhận | NOT RUN |
