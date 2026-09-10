## CẬP NHẬT MỚI NHẤT: CHUẨN HÓA JWT AUTHENTICATION & CLAIMSPRINCIPAL

1. **Cấu hình JWT Bearer chuẩn mực**:
   - Thêm package `Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.0.
   - Thêm cài đặt khóa bảo mật `Jwt` trong `appsettings.json`.
   - Tạo Service quản lý Token: `IJwtService` & `JwtService` (`src/Infrastructure/Services/JwtService.cs`).
   - Kích hoạt `AddAuthentication().AddJwtBearer()` và middleware `UseAuthentication()` trước `UseAuthorization()` trong `Program.cs`.
   - Cấu hình Swagger UI hỗ trợ nút **Authorize (Bearer Token)** để dễ dàng test các API có bảo vệ.
   - Hỗ trợ token cho cả SignalR `/kitchenHub` qua query param `access_token`.

2. **Cập nhật AuthController & Đăng Nhập**:
   - Khi Admin hoặc Nhân viên (Thu ngân, Nhà bếp) đăng nhập thành công tại `POST /api/Auth/login`, backend sẽ ký và trả về JWT Bearer Token chứa đầy đủ claims (`NameIdentifier`, `Name`, `Role`, `fullName`, `branchId`, `branchName`, `position`).
   - Bổ sung `POST /api/Auth/customer-token` để cấp token cho khách hàng.
   - Bổ sung `GET /api/Auth/me` có `[Authorize]` để lấy thông tin người dùng hiện tại từ Claims.

3. **Bảo mật Endpoint AI Assistant (AiController)**:
   - Thay vì tin tưởng tham số query string không an toàn `?role=...`, `AiController` giờ đây đọc thông tin định danh và vai trò trực tiếp từ `User` (`ClaimsPrincipal`).
   - Nếu không có token hợp lệ, hệ thống tự động khóa vai trò về `customer` (Khách vãng lai), ngăn chặn triệt để hành vi giả mạo vai trò `admin` hoặc `employee` để gọi các AI tools quản trị.

4. **Đồng Bộ Frontend**:
   - `LoginPage.tsx` & `App.tsx`: Lưu JWT token vào `localStorage` khi đăng nhập và xóa token khi đăng xuất.
   - Cả 2 ChatBot (`admin-web` và `customer-web`): Gửi JWT token qua HTTP Header `Authorization: Bearer <token>` khi trò chuyện với AI.

## HARDENING JWT AUTHORIZATION & CHANGE PASSWORD (09/2026)

### 1. ChangePassword chống IDOR

- `POST /api/Auth/change-password` yêu cầu `[Authorize]`.
- Account đích được xác định bằng `ClaimTypes.NameIdentifier` trong JWT.
- `request.Id` phải trùng user ID trong JWT; ID khác bị từ chối với `403 Forbidden`.
- Role `customer` chỉ đổi Customer; các role nhân viên/admin chỉ đổi Employee.
- Current password được kiểm tra trước khi cập nhật.
- Chưa có flow admin reset password cho user khác. Nếu cần, phải tạo endpoint riêng với policy admin.

### 2. Authorization matrix

| Controller / nhóm endpoint | Anonymous | Customer | Employee | Cashier | Kitchen | Admin |
|---|---:|---:|---:|---:|---:|---:|
| Auth/login, customer-token | Có | Có | Có | Có | Có | Có |
| Auth/me | Không | Có | Có | Có | Có | Có |
| Auth/change-password | Không | Self | Self | Self | Self | Self |
| AI chat | Có, role customer | Có | Có | Có | Có | Có |
| Dashboard, Expense | Không | Không | Không | Không | Không | Có |
| Employee management | Không | Không | Không | Không | Không | Có |
| Catalog mutations | Không | Không | Không | Không | Không | Có |
| Order status/delete/send kitchen | Không | Không | Có | Có | Có | Có |
| Kitchen requests | Không | Không | Không | Không | Có | Có |
| Shift | Không | Không | Không | Có | Không | Có |
| Catalog lookup và guest order creation | Có | Có | Có | Có | Có | Có |

Catalog/menu và guest order vẫn public để giữ flow khách vãng lai. Các endpoint chưa có ownership rule riêng không được xem là đã hoàn tất authorization theo resource.

### 3. Branch authorization

- Với Order, Kitchen, Shift và Reservation management, `branchId` claim trong JWT là nguồn identity đáng tin cậy.
- `branchId` từ query/body chỉ là filter/request input sau khi kiểm tra quyền.
- Employee/Cashier branch A không thể dùng `?branchId=B` để mở rộng scope sang branch B.
- Resource mutation ở branch khác bị từ chối với `403 Forbidden`.
- Admin được phép cross-branch theo rule hiện tại.

### 4. AI role source

- Đã xóa parameter `role` khỏi `AiController.Chat`.
- Role đi theo luồng:

```text
JWT -> ClaimsPrincipal -> ClaimTypes.Role -> AiUserContext -> AiAuthorization
```

- `/api/Ai/chat?role=admin` không còn là API contract và không thể nâng quyền.
- Anonymous request luôn được xử lý với role `customer`.

### 5. Secret configuration và HTTPS

- Đã xóa database credential, Gemini API key và JWT secret khỏi `appsettings.json`.
- `Jwt:Secret` không còn fallback hardcoded trong `Program.cs` hoặc `JwtService.cs`.
- Production phải cung cấp secret qua secure configuration:

```text
Jwt__Secret=<secret-from-secret-manager>
Gemini__ApiKey=<api-key-from-secret-manager>
```

- Docker Compose yêu cầu `JWT_SECRET` và `GEMINI_API_KEY` từ environment.
- `RequireHttpsMetadata` chỉ tắt trong Development; production tự động bật.
- Credential đã từng commit cần được rotate dù đã xóa khỏi working tree.

### 6. Regression tests

Test suite hiện có **54 tests**:

```text
Passed: 54
Failed: 0
Skipped: 0
```

Đã test JWT invalid/expired, role claim, AI query-role injection, ChangePassword self/IDOR, role authorization metadata, cross-branch filtering và net profit calculation `Revenue - Expenses - COGS`.

Lệnh test trực tiếp có thể bị file lock nếu API local đang chạy. Khi đó cần dùng output directory riêng để tránh chạy stale assembly.

### 7. Remaining gaps

- Chưa có `WebApplicationFactory` test đầy đủ cho HTTP pipeline; startup hiện gắn PostgreSQL migration/seeding và port cố định.
- `Order GET`, customer order history và một số resource query cần ownership policy riêng.
- Nếu yêu cầu admin reset password user khác, cần endpoint/policy riêng, audit log và quy trình reset an toàn.
