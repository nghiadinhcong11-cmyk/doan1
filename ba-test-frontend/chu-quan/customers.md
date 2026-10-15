# Chủ quán — /customers

- Route/source: /customers; CustomerManagement.tsx; Navbar.tsx.
- Đây là checklist chứa bước test, không phải mục lục. Tất cả case Chưa test.

| ID | Chức năng | Điều kiện/dữ liệu | Các bước | Kết quả mong đợi | Trạng thái | Kết quả thực tế | Minh chứng/finding |
|---|---|---|---|---|---|---|---|
| ADM-CUS-01 | Tải danh sách và trạng thái rỗng/loading/error | API Customer có dữ liệu, rỗng và môi trường có thể mô phỏng lỗi | Mở /customers; quan sát loading; thử dữ liệu rỗng và lỗi API | Loading kết thúc; danh sách/empty/error hiển thị rõ; không lộ stack trace/secret | Chưa test | | |
| ADM-CUS-02 | Tìm kiếm theo tên/số điện thoại | Có khách hàng khớp và không khớp | Nhập từng phần tên/số; đổi nhanh nhiều lần; xóa ô tìm kiếm | Request tìm kiếm được debounce; kết quả cuối không bị request cũ ghi đè; empty đúng | Chưa test | | |
| ADM-CUS-03 | Lọc nhóm khách hàng | Có Khách VIP, Khách quen, Khách lẻ | Chọn từng radio và Tất cả | Chỉ hiển thị đúng nhóm; dữ liệu chi nhánh theo quyền hiện tại | Chưa test | | |
| ADM-CUS-04 | Thêm khách hàng bằng modal | Tên/số điện thoại hợp lệ; chuẩn bị email/date tùy chọn | Mở Thêm hội viên; nhập form; lưu; đóng/mở lại | POST thành công thì modal đóng và danh sách tải lại; lỗi giữ dữ liệu và báo lỗi | Chưa test | | |
| ADM-CUS-05 | Validation và thao tác lặp form | Bỏ trống tên/số; email sai; bấm lưu nhiều lần | Submit từng dữ liệu; bấm Save liên tiếp; đóng bằng X | Required/type validation hoạt động theo source; không tạo bản ghi ngoài ý muốn; X không submit | Chưa test | | |
| ADM-CUS-06 | Sửa khách hàng và giữ dữ liệu | Bản ghi đã có email/address/gender/birthday | Mở Sửa; đổi một trường; lưu; reload | Giá trị mới lưu; trường không sửa không mất; branch/scope không đổi | Chưa test | | |
| ADM-CUS-07 | Lịch sử loyalty | Khách có id và lịch sử; khách không có lịch sử; API lỗi | Bấm Lịch sử; đóng modal; thử lỗi API | Modal loading/data/empty/error rõ; không xem ID khách ngoài scope | Chưa test | | |
| ADM-CUS-08 | Xuất file và nút Xóa | UI hiện nút Xuất file/Xóa | Bấm từng nút; ghi nhận request hoặc không có handler | Nếu source chưa có hành vi thì ghi finding, không coi nút là chức năng đã triển khai; không ghi PASS | Chưa test | | |
| ADM-CUS-09 | Quyền/branch/direct URL | Admin và manager ở Branch A; dữ liệu Branch B | Mở URL trực tiếp và thử tìm/lịch sử khách Branch B | Backend quyết định scope; không lộ hoặc sửa dữ liệu ngoài quyền | Chưa test | | |

