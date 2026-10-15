# Khách QR — truy cập trực tiếp /reservation và /profile

- Source: apps/customer-web/src/App.tsx, ReservationPage.tsx, CustomerProfile.tsx, CustomerLogin.tsx.
- Source hiện không có route guard riêng cho guest đối với hai path này. Khi chưa đăng nhập và không ở guest/QR mode, App render CustomerLogin; khi guest mode hoặc có query qr, App render Routes và hai path được khai báo.
- Quyền dữ liệu cuối cùng vẫn phải do API xác minh. Chính sách sản phẩm “guest có được xem reservation/profile hay không” chưa được chốt từ source.

| ID | Chức năng | Điều kiện/dữ liệu | Các bước | Kết quả mong đợi từ source | Trạng thái | Kết quả thực tế | Minh chứng/finding |
|---|---|---|---|---|---|---|---|
| KQR-01 | Direct /reservation khi chưa đăng nhập | Xóa customerToken, guest mode và mở Customer Web | Nhập trực tiếp /reservation; refresh; dùng Back/Forward | App hiển thị CustomerLogin thay vì khẳng định guest được xem reservation; chưa có redirect route riêng | Chưa test | | COV-001 |
| KQR-02 | Direct /profile khi chưa đăng nhập | Xóa customerToken, guest mode và mở Customer Web | Nhập trực tiếp /profile; refresh | App hiển thị CustomerLogin; không được coi là đã xác minh quyền profile | Chưa test | | COV-001 |
| KQR-03 | Direct /reservation trong guest session | Tạo guest QR session hợp lệ ở Branch A | Mở /reservation trực tiếp; quan sát page/API; thử dữ liệu không thuộc session | Route được render theo shell hiện tại; API phải quyết định dữ liệu guest được phép xem; nếu policy không cho phép phải redirect/error rõ | Chưa test | | COV-001 |
| KQR-04 | Direct /profile trong guest session | Guest QR session hợp lệ, không có customerToken | Mở /profile; quan sát page/API; thử reload/logout | Ghi nhận hành vi thực tế; không giả định guest có hồ sơ customer hoặc được xem dữ liệu cá nhân | Chưa test | | COV-001 |
| KQR-05 | /reservation và /profile với query qr | Mở /reservation?qr=... hoặc /profile?qr=... với QR hợp lệ/sai/hết hạn | Mở URL; refresh; theo dõi bootstrap và authorization | isQrBootstrap chỉ điều khiển shell theo source; bootstrap/session/API phải quyết định scope; QR sai/hết hạn không mở dữ liệu ngoài scope | Chưa test | | COV-001 |
| KQR-06 | Logout/chuyển mode sau direct route | Có guest session và customer login khả dụng | Mở direct route; logout/login; reload | Token/session cũ bị dọn theo App; dữ liệu hiển thị thuộc session hiện tại; không trộn guest/customer state | Chưa test | | COV-001 |

Finding COV-001: source chưa xác định chính sách sản phẩm cho guest tại /reservation và /profile. Không tự coi route declaration là quyền truy cập; cần quyết định nghiệp vụ hoặc kiểm chứng API trên môi trường test riêng.

