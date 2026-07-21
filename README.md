# Restaurant POS System (KiotViet Simplified Clone)

Hệ thống quản lý bán hàng nhà hàng tập trung vào tính đơn giản và chính xác trong doanh thu - chi phí.

## Phạm vi dự án
- **Tập trung**: Quản lý bán hàng, quản lý doanh thu, và theo dõi chi tiêu của chủ quán (Expenses).
- **Loại bỏ**: Không quản lý nguyên liệu (Inventory/Ingredients) và không có phân hệ Bếp (Kitchen) để tránh sự phức tạp và sai lệch số liệu.

## Cấu trúc thư mục (Clean Architecture)

- `apps/`: Chứa các ứng dụng frontend (React)
  - `admin-web`: Trang quản trị cho chủ quán - Quản lý nhân viên, báo cáo doanh thu và **quản lý chi tiêu**.
  - `pos-web`: Giao diện bán hàng (React) - Dành cho quầy thu ngân.
  - `staff-app`: Ứng dụng di động cho nhân viên (React + Capacitor) - Gọi món, xem bàn.
- `services/`: Chứa mã nguồn backend (ASP.NET Core)
  - `api/`: Hệ thống API chính
    - `Domain`: Các thực thể (Product, Order, Expense, User), interface, và logic nghiệp vụ lõi.
    - `Application`: Logic xử lý nghiệp vụ, DTOs, Commands, Queries.
    - `Infrastructure`: DbContext (PostgreSQL), SignalR Hubs (Đồng bộ đơn hàng).
    - `WebAPI`: Controllers, Middlewares.
- `database/`: Chứa các kịch bản khởi tạo database (PostgreSQL).
- `docs/`: Tài liệu thiết kế hệ thống, sơ đồ DB.

## Công nghệ sử dụng
- **Backend**: ASP.NET Core 8, Entity Framework Core, SignalR.
- **Frontend**: React (Vite), Tailwind CSS.
- **Mobile**: Capacitor.
- **Database**: PostgreSQL.
