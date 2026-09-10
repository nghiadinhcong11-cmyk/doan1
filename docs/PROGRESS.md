# Báo cáo tiến độ dự án Restaurant POS

## 1. Backend (ASP.NET Core API)
- **Hệ thống API chính**: Cấu trúc theo Clean Architecture (cơ bản).
- **Dashboard API**:
    - Xử lý múi giờ Việt Nam (`SE Asia Standard Time`).
    - Thống kê doanh thu thời gian thực (Hôm nay, 7 ngày gần nhất, các tháng trong năm).
    - Phân tích doanh thu theo chi nhánh.
    - Thống kê top sản phẩm bán chạy.
    - Theo dõi trạng thái nhân sự (Tổng số, đang hoạt động, đang trong ca làm việc).
    - Tích hợp Memory Cache (2 phút) để tối ưu hiệu suất.
- **AI Assistant (DOAN Assistant)**:
    - Tích hợp Google Gemini 1.5 Flash để giải đáp thắc mắc về dữ liệu nhà hàng.
    - Hỗ trợ truy vấn nhanh về doanh thu, nhân sự và tình trạng bàn.
    - *Lưu ý*: Cần cấu hình API Key hợp lệ trong `appsettings.json`.
- **Quản lý nghiệp vụ**:
    - Quản lý Sản phẩm (Products).
    - Quản lý Nhân viên (Employees) & Điểm danh (Attendance).
    - Quản lý Chi nhánh (Branches).
- **Cấu hình**: Kết nối PostgreSQL, Dockerize hệ thống.

## 2. Admin Web (React + Vite + Tailwind CSS)
- **Hệ thống tích hợp (All-in-One)**: Tích hợp Quản trị, Bán hàng (POS) và Nhà bếp vào cùng một nền tảng.
- **Trang Dashboard**:
    - Giao diện hiện đại, tối ưu cho quản trị viên.
    - Tích hợp biểu đồ trực quan (Recharts): AreaChart (doanh thu), PieChart (chi nhánh, nhân sự).
    - Bộ lọc linh hoạt: Xem doanh thu theo Giờ / Tuần / Tháng.
- **Trang Bán hàng (POS)**:
    - Quản lý phòng bàn, thực đơn.
    - Giao tiếp thời gian thực với Nhà bếp (Gửi món theo đợt).
    - Thanh toán đa phương thức (Tiền mặt, Chuyển khoản VietQR).
- **Màn hình Nhà bếp (Kitchen)**:
    - Tiếp nhận yêu cầu chế biến Realtime qua SignalR.
    - Quản lý trạng thái món ăn (Chế biến, Hoàn tất, Trả món).
- **Trang quản lý**:
    - `ProductManagement`: Quản lý danh mục món ăn/đồ uống, Topping, Size.
    - `EmployeeManagement`: Quản lý thông tin, tài khoản và chi nhánh của nhân viên.
    - `BranchManagement`: Quản lý các cơ sở nhà hàng.
    - `WorkSchedulePage`: Quản lý ca làm việc.
- **Xác thực**: Trang Login với 3 chế độ (Quản trị, Thu ngân, Nhà bếp).

## 3. Các vai trò trong hệ thống
### 3.1. Chủ quán (Admin/Owner)
- **Công cụ**: `Admin Web` (Chế độ Quản trị).
- **Hoạt động chính**: Theo dõi Dashboard, quản lý Thực đơn, Nhân sự, Chi nhánh và Chi tiêu.

### 3.2. Thu ngân / Nhân viên phục vụ (Cashier / Waiter)
- **Công cụ**: `Admin Web` (Chế độ Thu ngân/POS).
- **Hoạt động chính**:
    - Mở bàn, gọi món tại bàn cho khách.
    - **Gửi bếp**: Gửi yêu cầu chế biến theo từng đợt gọi (chỉ gửi món mới).
    - Thực hiện thanh toán, in hóa đơn.
    - Quản lý trạng thái đơn hàng (Đang xử lý, Hoàn thành, Đã hủy).

### 3.3. Bộ phận chế biến (Bếp trưởng/Pha chế)
- **Công cụ**: `Admin Web` (Chế độ Nhà bếp).
- **Hoạt động chính**:
    - Tiếp nhận danh sách món mới theo thời gian thực (Realtime).
    - Cập nhật trạng thái "Đang chế biến" hoặc "Đã xong" để phục vụ nhận biết.

## 4. Hạ tầng (Infrastructure)
- **Docker**: `docker-compose.yml` (PostgreSQL).
- **Realtime**: SignalR tích hợp để đồng bộ Bếp và POS.
- **Database**: PostgreSQL với các bảng Orders, OrderDetails, OrderRequests, OrderRequestItems, Products, Employees, Branches.
- **Dọn dẹp hệ thống**: Loại bỏ các ứng dụng thừa (`pos-web`, `staff-app`), tập trung toàn bộ vào `admin-web` và `customer-web`.

## 5. Các cập nhật mới nhất (Tháng 09/2026)
### 5.1. Hệ thống Gọi món theo đợt (Kitchen Request System)
- **Cơ chế gửi món**: Cho phép gửi món xuống bếp nhiều lần trong một hóa đơn. Hệ thống tự động tính toán phần chênh lệch để chỉ gửi các món mới gọi thêm.
- **Theo dõi số lượng**: Thêm trường `SentQuantity` trong chi tiết đơn hàng để quản lý chính xác số lượng bếp đã nhận và đang chế biến.
- **Database Transaction**: Áp dụng cơ chế `Execution Strategy` để đảm bảo an toàn dữ liệu khi tạo đợt gọi món và cập nhật số lượng đồng thời.

### 5.2. Màn hình Nhà bếp & Điều phối
- **Giao diện Realtime**: Sử dụng SignalR để nhận thông báo đơn mới tức thì kèm âm thanh.
- **Luồng trạng thái**: Chế biến -> Hoàn tất -> Trả món.
- **Công cụ hỗ trợ bếp**: Tích hợp Sơ đồ bàn (tự động cập nhật), Lịch làm việc và Quản lý đặt bàn (chế độ chỉ xem) cho bộ phận bếp.

### 5.3. Tối ưu hóa trải nghiệm & Bảo mật
- **Xác thực đa chế độ**: Trang đăng nhập hỗ trợ 3 cổng riêng biệt: Quản trị, Thu ngân và Nhà bếp với phân quyền chức danh chặt chẽ.
- **Quy trình duyệt đơn**: Đơn hàng từ Web khách sẽ qua bước kiểm duyệt và gán bàn thủ công thay vì tự động nạp, giúp tránh đơn ảo.
- **Đồng bộ hóa dữ liệu**: Thêm nút làm mới (Manual Refresh) trên toàn bộ các màn hình vận hành quan trọng.
- **Fix lỗi Concurrency**: Xử lý triệt để lỗi xung đột khi nhiều người cùng cập nhật một đơn hàng.

## 6. Các tính năng sắp tới
- Hoàn thiện module Quản lý Chi tiêu (Expenses).
- Tích hợp thêm báo cáo chi tiết về hiệu suất bếp.
- Xây dựng module Kho hàng (Inventory).
