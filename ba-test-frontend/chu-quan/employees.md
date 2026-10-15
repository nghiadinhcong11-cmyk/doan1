# Nhân viên — hồ sơ và thông tin ca làm

- Vai trò: Chủ quán/Quản lý chi nhánh
- Route: /employees
- Source: apps/admin-web/src/features/hrm/pages/EmployeeManagement.tsx, services/api/src/WebAPI/Controllers/EmployeeController.cs, services/api/src/Domain/Entities/Employee.cs
- Phạm vi: nhập, lưu, sửa, tải lại và hiển thị EmployeeType/BasicSalary như thông tin hồ sơ. Không suy ra đây là tính lương.

## Điều kiện và validation đã thấy từ source

Chuẩn bị tài khoản có quyền quản lý nhân viên và branch test. UI hiện kiểm tra các trường bắt buộc của form hiện có như họ tên, vai trò, branch; password có nhánh validation riêng khi tạo/sửa. Chưa thấy rule riêng về miền giá trị cho EmployeeType hoặc BasicSalary ngoài kiểu dữ liệu input; không tự thêm rule.

| ID | Chức năng | Điều kiện/dữ liệu | Các bước | Kết quả mong đợi | Trạng thái | Kết quả thực tế | Minh chứng/finding |
|---|---|---|---|---|---|---|---|
| EMP-PROFILE-01 | Mở và hiển thị hồ sơ | Có nhân viên có EmployeeType và BasicSalary | Mở /employees, xem danh sách và chi tiết | Hai trường hiển thị đúng dữ liệu API, không bị bỏ qua | Chưa test | | |
| EMP-PROFILE-02 | Nhập hồ sơ mới | Có branch và dữ liệu nhân viên hợp lệ | Mở form thêm; nhập EmployeeType và BasicSalary cùng các trường bắt buộc; lưu | Form gửi hai trường; bản ghi được tạo nếu các validation hiện có đạt | Chưa test | | |
| EMP-PROFILE-03 | Sửa hồ sơ | Có nhân viên đã lưu | Mở sửa; thay EmployeeType/BasicSalary; lưu | Giá trị mới được gửi và hiển thị lại; không mất trường khác | Chưa test | | |
| EMP-PROFILE-04 | Tải lại sau lưu | EMP-PROFILE-02 hoặc 03 đã hoàn tất | Reload trang, mở lại danh sách/chi tiết | Hai giá trị vẫn lấy từ backend, không chỉ từ state trình duyệt | Chưa test | | |
| EMP-PROFILE-05 | Validation form liên quan | Thiếu một trường bắt buộc hiện có hoặc password không hợp lệ theo form | Submit; sửa lỗi; submit lại | Thông báo validation đúng; không ghi nhận PASS cho rule chưa thấy trong source | Chưa test | | |
| EMP-PROFILE-06 | Quyền và branch | Tài khoản manager ở Branch A; có dữ liệu Branch B | Mở URL trực tiếp và thử sửa bản ghi ngoài branch | Backend quyết định quyền/scope; không lộ hoặc sửa dữ liệu ngoài phạm vi | Chưa test | | |

Ghi chú: kiểm chứng API contract, dữ liệu database và branch isolation đầy đủ được theo dõi ở checklist kỹ thuật/runtime và flow liên vai trò, không giả lập bằng UI.

