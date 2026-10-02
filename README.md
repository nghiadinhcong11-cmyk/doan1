# Restaurant POS System (KiotViet Simplified Clone)

Hệ thống quản lý bán hàng nhà hàng tập trung vào tính đơn giản và chính xác trong doanh thu - chi phí.

## Phạm vi dự án
- **Tập trung**: Quản lý bán hàng đa chi nhánh, quản lý doanh thu, nhân sự, và theo dõi chi tiêu của chủ quán (Expenses). Payroll không thuộc phạm vi hệ thống hiện tại.
- **Tính năng nổi bật**: Tích hợp Kitchen Display System (KDS) qua SignalR và Trợ lý ảo AI (Gemini) hỗ trợ điều hành.

## Cấu trúc thư mục backend phân lớp
- `apps/`: Chứa các ứng dụng frontend (React)
  - `admin-web`: Tích hợp Trang quản trị (Admin) và Giao diện bán hàng (POS). Quản lý chi nhánh, nhân sự, doanh thu và chi tiêu.
  - `customer-web`: Giao diện dành cho khách hàng - Đặt món tại bàn qua mã QR và theo dõi tích điểm.
- `services/`: Chứa mã nguồn backend (ASP.NET Core 8)
  - `api/src/`: Hệ thống API chính
    - `Domain`: Thực thể (Product, Order, Employee...) và các khái niệm nghiệp vụ.
    - `Application`: Logic xử lý nghiệp vụ, DTOs, Services.
    - `Infrastructure`: Persistence (EF Core), dịch vụ hạ tầng và JWT service.
    - `WebAPI`: Controllers, Middleware, Auth config.
    - `AI`: Trợ lý ảo AI, Tool Registry và Permission Service.
- `services/api/src/Infrastructure/Persistence/Migrations/`: EF Core migrations và model snapshot.
- `baocaodoan/`: Bộ tài liệu chi tiết phục vụ báo cáo đồ án.

## Tài liệu chuẩn

`docs/system-audit/` là source of truth được kiểm tra theo source code hiện tại. Bắt đầu tại [`docs/system-audit/00-README.md`](docs/system-audit/00-README.md). Bộ `baocaodoan/` là tài liệu bổ trợ cho báo cáo/khóa luận và có thể chứa nội dung lịch sử; khi có mâu thuẫn, bộ system audit và source code được ưu tiên.

## Phân quyền & Bảo mật (RBAC)

Hệ thống triển khai mô hình phân quyền dựa trên vai trò (Role-Based Access Control) với cơ chế cách ly chi nhánh (Branch Isolation) nghiêm ngặt:

| Role | Phạm vi (Scope) | Mô tả |
| :--- | :--- | :--- |
| **admin** | GLOBAL | Toàn quyền hệ thống, quản lý nhiều chi nhánh. |
| **manager** | OWN_BRANCH | Quản lý nghiệp vụ trong phạm vi chi nhánh được gán (Active role). |
| **cashier** | OWN_BRANCH | Thu ngân, quản lý đơn hàng và thanh toán tại chi nhánh. |
| **kitchen** | OWN_BRANCH | Tiếp nhận và chế biến món ăn tại chi nhánh. |
| **employee** | OWN_BRANCH | Ghi món, phục vụ và quản lý lịch cá nhân. |
| **customer** | OWN_DATA | Khách hàng đặt món và xem thông tin cá nhân qua QR. |

**Nguyên tắc vàng**:
- **Role** là nguồn định danh duy nhất cho việc phân quyền (Authorization).
- **Position** (Chức danh) chỉ dùng để hiển thị và mô tả nghiệp vụ.
- Dữ liệu chi nhánh được cách ly tại Server thông qua JWT `branchId`.
