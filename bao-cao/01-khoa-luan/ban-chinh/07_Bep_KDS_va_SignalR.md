# 07. Bếp (KDS) và SignalR

## 1. Mục tiêu module
Tối ưu hóa giao tiếp giữa khu vực phục vụ và khu vực chế biến (Bếp/Bar), giảm thiểu sai sót và tăng tốc độ phục vụ qua màn hình KDS (Kitchen Display System).

## 2. Chức năng chính
- **Màn hình KDS**: Hiển thị danh sách các món ăn cần chế biến theo thứ tự thời gian.
- **Phân loại món**: Phân biệt món mới, món đang làm và món đã hoàn thành.
- **Thông báo thời gian thực**: Cập nhật ngay khi có yêu cầu gọi món mới từ POS hoặc QR Ordering.

## 3. Công nghệ SignalR
Hệ thống sử dụng **SignalR Hub** (`KitchenHub`) để đẩy dữ liệu:
- **Event: `ReceiveOrderUpdate`**: Phát đi khi có đơn hàng mới hoặc thay đổi trạng thái món.
- **Group Isolation**: Các thông báo được gửi theo nhóm chi nhánh (`BranchGroup`), đảm bảo bếp chi nhánh A không nhận nhầm đơn của chi nhánh B.

## 4. Quy trình xử lý tại bếp
1. Nhân viên bếp nhấn "Bắt đầu" khi bắt đầu chế biến (Trạng thái: `Processing`).
2. Nhấn "Hoàn thành" khi món đã sẵn sàng phục vụ (Trạng thái: `Served`).
3. Hệ thống tự động gửi thông báo ngược lại cho nhân viên phục vụ/thu ngân.

## 5. API & Hub
- **Hub Endpoint**: `/kitchenHub`.
- **Dịch vụ hỗ trợ**: `KitchenService.cs`.

## 6. Bảo mật
- Kết nối SignalR yêu cầu xác thực JWT.
- Chỉ người dùng có Role `kitchen`, `admin`, `manager`, hoặc `employee` (tùy cấu hình) mới được phép truy cập vào màn hình KDS của chi nhánh.
