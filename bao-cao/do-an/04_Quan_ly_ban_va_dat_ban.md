# 04. Quản lý bàn và Đặt bàn (Reservation)

## 1. Mục tiêu module
Quản lý không gian nhà hàng, theo dõi trạng thái bàn trong thời gian thực và xử lý các yêu cầu đặt chỗ của khách hàng.

## 2. Chức năng chính
- **Sơ đồ bàn (Table Map)**: Hiển thị danh sách bàn theo khu vực (Area).
- **Trạng thái bàn**: Cập nhật tức thời (Trống, Có khách, Đã đặt, Đang dọn dẹp).
- **Đặt bàn (Reservation)**: Khách hàng đặt chỗ trước qua web hoặc AI Assistant.
- **Tự động hóa**: AI hỗ trợ khách đặt bàn qua Tool `create_booking`.

## 3. Business Logic
- **Đồng bộ trạng thái**: Khi có đơn hàng mới gắn với bàn, trạng thái bàn tự động chuyển sang "Có khách".
- **Isolation**: Đảm bảo khách hàng ở bàn A không thể xem hoặc can thiệp vào đơn hàng của bàn B (Table Isolation Hardening).

## 4. API chính
- `GET /api/Table/status`: Lấy trạng thái toàn bộ bàn.
- `POST /api/Reservation`: Tạo yêu cầu đặt bàn mới.
- `PUT /api/Table/{id}/status`: Cập nhật trạng thái bàn thủ công.

## 5. Dữ liệu (Entities)
- **Area**: Khu vực (Tầng 1, Tầng 2, Sân vườn...).
- **Table**: Thông tin bàn, vị trí, sức chứa và trạng thái hiện tại.
- **Reservation**: Thông tin đặt chỗ (Tên khách, số điện thoại, thời gian, số lượng người).

## 6. Bảo mật & UX
- **Hardening**: Chặn truy cập trái phép vào thông tin đặt bàn của người khác qua Customer IDOR Protection.
- **Giao diện**: Hiển thị trực quan màu sắc theo trạng thái bàn trên POS Admin.
