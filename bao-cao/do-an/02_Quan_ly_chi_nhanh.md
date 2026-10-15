# 02. Quản lý chi nhánh và Thiết lập hệ thống

## 1. Mục tiêu module
Cho phép quản trị viên vận hành hệ thống đa điểm (Multi-branch), quản lý thông tin từng cơ sở và thiết lập các thông số vận hành riêng biệt cho mỗi chi nhánh.

## 2. Chức năng chính
- Thêm mới, chỉnh sửa thông tin chi nhánh (Tên, địa chỉ, số điện thoại).
- Cấu hình thiết lập hệ thống (System Settings) theo từng chi nhánh.
- Cấu hình mẫu in hóa đơn (Receipt Settings) riêng cho mỗi cơ sở.
- Cách ly dữ liệu: Đảm bảo dữ liệu chi nhánh này không bị truy cập trái phép từ chi nhánh khác.

## 3. Business Logic quan trọng
- **Branch Isolation**: Mọi truy vấn liên quan đến đơn hàng, sản phẩm, nhân sự đều phải lọc theo `BranchId`.
- **Global vs Local Settings**: Các thiết lập như `vatPercent`, `serviceFee` được lưu trữ trong bảng `SystemSettings` theo cặp khóa-giá trị gắn với `BranchId`.

## 4. API chính
- `GET /api/Branch`: Lấy danh sách chi nhánh.
- `GET /api/SystemSettings/{branchId}`: Lấy cấu hình của chi nhánh.
- `POST /api/SystemSettings`: Cập nhật cấu hình.

## 5. Cấu trúc dữ liệu (Entity)
- **Branch**: `Id`, `Name`, `Address`, `Phone`, `IsActive`.
- **SystemSetting**: `BranchId`, `Key`, `Value`, `Type`.

## 6. Bảo mật (Security)
- Chỉ Role `admin` mới có quyền quản lý danh sách chi nhánh.
- Role `manager` chỉ được xem và sửa thiết lập của chính chi nhánh mình quản lý.
- Mọi API thiết lập đều được bảo vệ bởi `[Authorize]` và kiểm tra quyền sở hữu `BranchId`.
