# 10. Bảo mật, RBAC và Branch Isolation

## 1. Cơ chế Xác thực (Authentication)
- **Chuẩn hóa JWT (JSON Web Token)**: Hệ thống sử dụng `Microsoft.AspNetCore.Authentication.JwtBearer` để xác thực. Token được ký bằng khóa bảo mật (Secret Key) cấu hình tại server.
- **Claims-based Identity**: Mọi thông tin định danh (`userId`, `role`, `branchId`) đều được trích xuất trực tiếp từ Claims của Token, đảm bảo tính chống giả mạo.
- **Password Security**: Sử dụng `BCrypt` hoặc `Identity PasswordHasher` để băm mật khẩu. Chức năng đổi mật khẩu (`change-password`) được bảo vệ chống IDOR, chỉ cho phép người dùng tự đổi mật khẩu của chính mình.
- **HTTPS & Secure Config**: Yêu cầu HTTPS Metadata trong môi trường Production. Các thông tin nhạy cảm (JWT Secret, Database Key) được quản lý qua biến môi trường, không hardcode trong mã nguồn.

## 2. Phân quyền theo Vai trò (RBAC - Role-Based Access Control)
Hệ thống áp dụng bảng ma trận quyền hạn (Authorization Matrix) chặt chẽ:
- **admin**: Toàn quyền hệ thống, quản lý chi nhánh, thực đơn và nhân sự toàn chuỗi.
- **manager**: Quản lý nghiệp vụ (Đơn hàng, Bếp, Nhân sự, Báo cáo) trong phạm vi chi nhánh.
- **cashier**: Quyền bán hàng, thanh toán và quản lý đơn hàng.
- **kitchen**: Tiếp nhận yêu cầu từ KDS và cập nhật trạng thái chế biến món.
- **employee**: Tạo đơn, phục vụ món và xem lịch làm việc cá nhân.
- **customer**: Quyền xem menu, gọi món và đặt bàn tại chính bàn mình đang ngồi.

## 3. Cách ly Chi nhánh (Branch Isolation Hardening)
Đảm bảo an toàn dữ liệu cho mô hình đa chi nhánh:
- **JWT Scope**: `branchId` trong JWT là nguồn định danh duy nhất được tin cậy. Nhân viên chi nhánh A không thể truy cập dữ liệu chi nhánh B bằng cách thay đổi tham số trong URL (Lỗi 403 Forbidden).
- **Backend Enforcement**: Mọi truy vấn (Orders, Kitchen, Shifts, Reservations) đều tự động áp dụng filter theo `BranchId` lấy từ người dùng hiện tại.

## 4. Chống lỗi hổng IDOR (Insecure Direct Object Reference)
- **Customer IDOR**: Khách hàng chỉ có thể truy cập lịch sử đơn hàng và thông tin loyalty của chính mình qua xác thực định danh.
- **Table Isolation**: Ràng buộc chặt chẽ giữa Khách hàng -> Bàn -> Đơn hàng. Chặn mọi hành vi can thiệp vào đơn hàng của bàn khác.
- **Self-service Security**: Các thao tác cập nhật hồ sơ cá nhân chỉ được thực hiện bởi chính chủ tài khoản.

## 5. Bảo mật AI (AI Assistant Security)
- **AI Authorization & Risk Level**: Mỗi công cụ AI (Tool) được gắn một mức độ rủi ro:
    - `Read`: Xem dữ liệu công khai hoặc dữ liệu được phép.
    - `Write`: Thay đổi trạng thái dữ liệu (cần quyền nhân viên/quản lý).
    - `Destructive`: Xóa dữ liệu (chỉ dành cho Admin).
- **Role Source**: AI không tin vào tham số `?role=admin` truyền qua query string mà xác thực quyền hạn dựa trên JWT Token của người dùng đang chat.
- **Validation**: Mọi tham số AI sinh ra đều được `AiToolValidator` kiểm tra nghiêm ngặt (ví dụ: giá mới phải là số dương).

## 6. Bảo mật hạ tầng
- **Rate Limiting**: Chính sách `ai-limiter` giới hạn 15 yêu cầu chat mỗi phút trên một địa chỉ IP để chống tấn công từ chối dịch vụ và lạm dụng API AI.
- **CORS Hardening**: Chỉ cho phép các nguồn tin cậy kết nối đến API.
- **Error Protection**: Không hiển thị Stack Trace hoặc chi tiết database lỗi cho phía Client.
