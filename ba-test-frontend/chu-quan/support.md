# Chủ quán — /support

- Route/source: /support; SupportPage.tsx; Navbar.tsx.
- Source hiện thể hiện đây là trang hỗ trợ nội dung tĩnh. Chưa thấy API hoặc thao tác gửi yêu cầu trong component.

| ID | Chức năng | Điều kiện/dữ liệu | Các bước | Kết quả mong đợi | Trạng thái | Kết quả thực tế | Minh chứng/finding |
|---|---|---|---|---|---|---|---|
| ADM-SUP-01 | Tải trang hỗ trợ | Đăng nhập admin; mở trực tiếp và từ menu Help | Mở /support; refresh; chuyển đi rồi quay lại | Trang tải đúng layout/nội dung; không mất session | Chưa test | | |
| ADM-SUP-02 | Responsive/điều hướng hỗ trợ | Desktop và viewport mobile | Mở menu hỗ trợ; bấm liên kết/nút nếu có; dùng Back/Forward | Liên kết đúng route; không mở route trái quyền; nội dung không bị che | Chưa test | | |
| ADM-SUP-03 | Xác minh phạm vi hành động | Source hiện tại không xác nhận form/ticket API | Kiểm tra toàn bộ nút/link/tab trên trang; theo dõi UI và Network khi test runtime | Chỉ ghi chức năng thực sự có; nếu không có gửi ticket thì ghi finding tài liệu/UI, không tạo case giả | Chưa test | | |

