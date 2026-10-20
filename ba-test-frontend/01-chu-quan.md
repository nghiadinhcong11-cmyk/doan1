# Checklist manual — Chủ quán (`admin`)

Điều kiện: đăng nhập tài khoản `admin`, API và Admin Web đang chạy. Ghi trạng thái ở cột cuối.

| # | Checklist | Kết quả mong đợi | Trạng thái |
|---|---|---|---|
| 1 | Đăng nhập, refresh, đăng xuất rồi đăng nhập lại | Vào đúng dashboard; session/role không sai | NOT RUN |
| 2 | Mở dashboard và business insights | KPI, doanh thu, chi phí, lợi nhuận và biểu đồ tải đúng | NOT RUN |
| 3 | Đổi branch trên navbar rồi refresh | Dữ liệu đổi đúng branch, không lẫn dữ liệu branch khác | NOT RUN |
| 4 | Mở `/products`, `/toppings`, `/promotions` | Danh sách, tìm kiếm, lọc và trạng thái hiển thị đúng | NOT RUN |
| 5 | Tạo/sửa/tắt món và topping test | Validation đúng; dữ liệu lưu và tải lại đúng | NOT RUN |
| 6 | Mở `/tables`, xem QR và tạo/sửa đặt bàn | Bàn, QR và reservation đúng branch | NOT RUN |
| 7 | Mở `/pos`, thêm món/topping và sửa số lượng | Giá và tổng tiền lấy đúng từ backend | NOT RUN |
| 8 | Kiểm tra VAT, service fee, giảm giá và thanh toán | Tổng tiền POS, payment và receipt khớp nhau | NOT RUN |
| 9 | Mở invoices/expenses và lọc dữ liệu | Danh sách, chi tiết, in và bộ lọc hoạt động đúng | NOT RUN |
| 10 | Mở inventory và thử thao tác test | Tồn kho/phiếu cập nhật đúng; lỗi không làm mất dữ liệu | NOT RUN |
| 11 | Mở employees, attendance, schedule, shifts | Tải, lọc, tạo/sửa/xóa theo quyền admin | NOT RUN |
| 12 | Mở settings, receipt, print templates, branches | Form lưu đúng branch; reload vẫn giữ dữ liệu | NOT RUN |
| 13 | Mở notifications, đánh dấu đã đọc | Badge, danh sách và trạng thái đọc đúng | NOT RUN |
| 14 | Mở URL `/kitchen`, `/settings` và URL không hợp lệ trực tiếp | Trang hợp lệ theo quyền; URL bị cấm/không tồn tại xử lý đúng | NOT RUN |
| 15 | Thử API lỗi, refresh khi đang loading, token hết hạn | Có loading/error an toàn; không lộ stack trace/token | NOT RUN |
| 16 | Mở [AI Admin](ai-chat/01-chu-quan.md) và chạy checklist AI | Read/write tool, confirmation, dấu tiếng Việt và quyền đúng | NOT RUN |

Ghi chú: checklist AI chi tiết nằm riêng trong `ai-chat/01-chu-quan.md`.
