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

## Owner navigation follow-up cases

Các case dưới đây bổ sung cho audit tĩnh mục “Nhân viên”. Chúng chỉ là checklist runtime; mọi trạng thái giữ `Chưa test` cho đến khi người dùng chạy trong môi trường được duyệt.

| ID | Chức năng | Điều kiện/dữ liệu | Các bước | Kết quả mong đợi | Trạng thái | Minh chứng/finding |
|---|---|---|---|---|---|---|
| ADM-EMP-01 | Danh sách, tìm kiếm và bộ lọc nhân viên | Có nhân viên khác trạng thái/branch/phòng ban; danh sách dài | Mở `/employees`; tìm theo mã/tên/SĐT; đổi active/inactive/all, branch, department; kiểm tra empty | Kết quả đúng filter; phân biệt empty với lỗi; ghi nhận hiện trạng không có pagination | Chưa test | OWNER-EMPLOYEE-NAV-REVIEW; observation |
| ADM-EMP-02 | CRUD, toggle, delete và đồng bộ sau thao tác | Có employee test không phải tài khoản đang dùng; API trả success/failure được kiểm soát | Thêm; sửa; reload; mở rộng chi tiết; toggle; thử thao tác lặp; xóa bản ghi test; kiểm tra attendance/schedule/shift liên quan | Response/UI nhất quán; lỗi không báo thành công; dữ liệu sau reload đúng; lịch sử không mất ngoài chính sách được duyệt | Chưa test | OWNER-EMPLOYEE-NAV-REVIEW; HR-001, HR-002 |
| ADM-EMP-03 | Chấm công, lịch và ca từ submenu | Có dữ liệu attendance/schedule/shift ở Branch A/B và dữ liệu rỗng | Mở `/attendance`, `/schedule`, `/shifts`; đổi filter/date/week/branch; tạo/xóa schedule theo UI; refresh/retry khi API lỗi | Scope admin đúng; loading/empty/error rõ; không tạo/xóa lặp ngoài hành động xác nhận | Chưa test | Reuse HR-SUB-03..05; OWNER-EMPLOYEE-NAV-REVIEW; HR-002 |
| ADM-EMP-04 | Không tự vô hiệu hóa/xóa admin hiện tại | Tài khoản admin test chính là employee target | Mở `/employees`; thử “Cho nghỉ” và “Xóa vĩnh viễn” trên chính tài khoản; kiểm tra JWT hiện tại và đăng nhập lại | Hành vi phải khớp policy self-protection được duyệt; không đánh dấu PASS nếu policy/session behavior chưa được quyết định | Chưa test | HR-001 |
| ADM-EMP-05 | Gán/chuyển branch nhân viên | Có branch active, inactive và ID không tồn tại trong fixture; admin token | Tạo/sửa employee bằng UI; lặp request đã chỉnh `branchId` trực tiếp cho active/inactive/unknown; ghi status/body đã che dữ liệu nhạy cảm | Backend là authority; kết quả valid/invalid/inactive deterministic và đúng policy; không tin danh sách branch từ UI | Chưa test | HR-003 |

Ghi chú: kiểm chứng API contract, dữ liệu database và branch isolation đầy đủ được theo dõi ở checklist kỹ thuật/runtime và flow liên vai trò, không giả lập bằng UI.

## Source-fix retest note — 2026-10-15

`ADM-EMP-02`, `ADM-EMP-03`, `ADM-EMP-04` và `ADM-EMP-05` đã được giữ ở trạng thái `Chưa test` sau khi sửa HR-001/002/003. Khi chạy tay, cần ghi riêng HTTP status/body đã che dữ liệu nhạy cảm, không coi build là runtime PASS.

| ADM-EMP-06 | Tự vô hiệu hóa qua đường PUT | Admin hiện tại và employee ID tương ứng | Gửi/simulate PUT hồ sơ chính mình với `isActive=false`, không dùng nút toggle | Backend từ chối thay đổi trạng thái; các trường hồ sơ hợp lệ không bị chặn ngoài policy | Chưa test | HR-004 |
| ADM-EMP-07 | Phân biệt branch bỏ qua, null và giữ nguyên | Employee thuộc branch inactive; admin token | Sửa hồ sơ với payload giữ nguyên branch; payload bỏ `branchId`; payload đặt `branchId:null`; ghi status/body và bản ghi sau đó | Kết quả đúng policy đã duyệt; không vô tình xóa/chuyển branch khi chỉ sửa hồ sơ | Chưa test | HR-005 |
| ADM-EMP-08 | Branch same/different và role kết hợp | Admin token; branch hiện tại, branch active khác, branch inactive và role hiện hành | Gửi lần lượt omitted, cùng ID, ID active khác, ID inactive/unknown; thử đổi role cùng từng branch; kiểm tra status/body đã che dữ liệu | Omitted/cùng ID giữ branch; chuyển khác chỉ nhận active hợp lệ; role–branch giữ đúng rule hiện hành; không đổi lịch sử | Chưa test | HR-003 |

