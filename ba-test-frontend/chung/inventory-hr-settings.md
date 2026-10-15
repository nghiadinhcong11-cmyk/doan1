# Checklist dùng chung — inventory, HR và settings

Các case dưới đây là file chứa bước test dùng chung. Role page chỉ tham chiếu, không sao chép. Khi chạy, ghi rõ role/branch trong Kết quả thực tế và Minh chứng.

## Inventory subpages

Source: InventoryPage.tsx, inventoryApi.ts, ManagementRoutes.tsx, StaffRoutes.tsx.

| ID | Trang/chức năng | Role thực hiện | Điều kiện/dữ liệu | Các bước | Kết quả mong đợi | Trạng thái | Kết quả thực tế | Minh chứng/finding |
|---|---|---|---|---|---|---|---|---|
| INV-SUB-01 | /inventory overview | admin, manager | Branch có item/tồn kho và branch rỗng | Mở route; đổi branch; reload | Summary đúng branch; empty/loading/error rõ | Chưa test | | |
| INV-SUB-02 | /inventory/items | admin, manager | Item có đơn vị/số lượng; dữ liệu rỗng | Mở items; tìm/lọc nếu có; reload | Danh sách và định dạng đúng; không lộ branch khác | Chưa test | | |
| INV-SUB-03 | /inventory/receipts | admin, manager | Phiếu nhập ở nhiều status | Mở receipts; đổi status/page; mở :id | List/filter/detail cùng branch; lỗi detail rõ | Chưa test | | |
| INV-SUB-04 | /inventory/issues | admin, manager | Phiếu xuất/issue nhiều status | Mở issues; lọc; mở :id | Status/filter/detail đúng; không duplicate khi reload | Chưa test | | |
| INV-SUB-05 | /inventory/history | admin, manager | Transaction nhiều ngày/item/type | Mở history; đổi filter/page/date | Kết quả đúng query; empty và lỗi rõ | Chưa test | | |
| INV-SUB-06 | Tạo/xác nhận phiếu và modal | admin/manager theo quyền source | Dữ liệu item hợp lệ và invalid | Mở modal; submit hợp lệ/thiếu field/bấm lặp; đóng modal | Validation theo source; commit không tạo bản ghi lặp; cancel không submit | Chưa test | | |
| INV-SUB-07 | Staff inventory operational | employee | Tài khoản employee, Branch A/B | Mở /staff/inventory; thử direct URL/branch khác | Chỉ xem đúng phạm vi và thao tác được source cho phép | Chưa test | | |
| INV-SUB-08 | Lỗi API/retry inventory | API chậm/lỗi hoặc mất kết nối test | Mở từng subpage; retry/reload | Error không mất context; retry không tạo POST lặp | Chưa test | | |

## HR subpages

Source: EmployeeManagement.tsx, AttendanceManagement.tsx, WorkSchedulePage.tsx, ShiftManagement.tsx, EmployeeAttendance.tsx, EmployeeSchedule.tsx.

| ID | Trang/chức năng | Role thực hiện | Điều kiện/dữ liệu | Các bước | Kết quả mong đợi | Trạng thái | Kết quả thực tế | Minh chứng/finding |
|---|---|---|---|---|---|---|---|---|
| HR-SUB-01 | /employees danh sách/modal | admin, manager | Nhân viên Branch A/B | Mở, tìm nếu có, mở thêm/sửa/đóng modal | Role/branch và validation đúng; không ghi lương ngoài phạm vi hồ sơ | Chưa test | | |
| HR-SUB-02 | EmployeeType/BasicSalary profile | admin, manager | Hồ sơ có hai trường | Nhập/sửa/lưu/reload/hiển thị | Hai trường giữ đúng giá trị backend; không suy ra tính lương | Chưa test | | |
| HR-SUB-03 | /attendance | admin, manager | Dữ liệu chấm công hoặc rỗng | Mở; đổi filter/date/employee; thử dữ liệu rỗng | Kết quả/filter/empty/error đúng branch | Chưa test | | |
| HR-SUB-04 | /schedule | admin, manager | Nhân viên và lịch làm | Mở; đổi tuần/branch; reload | Lịch đúng scope; trạng thái loading/error rõ | Chưa test | | |
| HR-SUB-05 | /shifts | admin, manager, cashier | Ca mở/đóng và dữ liệu không có | Mở; thao tác từng action; submit lặp | Chỉ action source có mới được test; không tạo ca trùng | Chưa test | | |
| HR-SUB-06 | /pos/attendance và /kitchen/attendance | cashier, kitchen | Tài khoản tương ứng | Mở trực tiếp; chấm công theo UI; refresh | Chỉ dữ liệu/nghiệp vụ được source cho phép; scope đúng | Chưa test | | |
| HR-SUB-07 | /pos/schedule và /kitchen/schedule | cashier, kitchen | Có lịch/không lịch | Mở; đổi ngày/tuần; reload | Hiển thị đúng và không xem branch khác | Chưa test | | |
| HR-SUB-08 | URL trực tiếp và hết hạn phiên HR | Mỗi role HR | Token hết hạn hoặc role không phù hợp | Mở các URL trực tiếp; thao tác sau expiry | Redirect/401/error đúng; không ghi PASS từ đọc code | Chưa test | | |

## Settings subpages

Source: SystemSettings.tsx, ReceiptSettingsPage.tsx, BranchManagement.tsx, PrintTemplates.tsx.

| ID | Trang/chức năng | Role thực hiện | Điều kiện/dữ liệu | Các bước | Kết quả mong đợi | Trạng thái | Kết quả thực tế | Minh chứng/finding |
|---|---|---|---|---|---|---|---|---|
| SET-SUB-01 | /settings tải và chọn branch | admin | Branch A/B, system settings | Mở; đổi branch; reload | Query dùng branch đang chọn; không hiển thị dữ liệu branch khác | Chưa test | | |
| SET-SUB-02 | /settings sửa VAT/fee/system setting | admin | Setting hiện có và form hợp lệ/invalid | Mở form; đổi; lưu; reload | PUT thành công giữ branch; lỗi không báo thành công | Chưa test | | |
| SET-SUB-03 | /settings/receipt form/preview | admin | Receipt config Branch A/B | Đổi từng toggle/text/size; xem preview; reload | Preview và persisted settings khớp branch | Chưa test | | |
| SET-SUB-04 | Upload/xóa logo receipt | admin | File ảnh hợp lệ/không hợp lệ | Upload; reload; delete; thử lỗi | Logo đúng branch; lỗi rõ; không lộ file/secret | Chưa test | | |
| SET-SUB-05 | /print-templates | admin | Template hiện có/không có | Mở; sửa preview; lưu hoặc thao tác source có | Chỉ ghi case cho action thực sự có; không PASS nếu source chưa có lưu | Chưa test | | |
| SET-SUB-06 | /branches danh sách/tạo/sửa | admin | Branch A/B và dữ liệu hợp lệ/thiếu | Mở; tạo/sửa; đóng modal; reload | Required/branch data đúng; không tạo trùng do bấm lặp | Chưa test | | |
| SET-SUB-07 | Manager bị chặn settings | manager | Tài khoản manager | Mở trực tiếp /settings, /branches, /print-templates | Redirect /forbidden theo route wrapper; backend vẫn là boundary | Chưa test | | |
| SET-SUB-08 | Deep link/session settings | admin/manager | URL trực tiếp, token hết hạn | Mở từng URL; hết hạn rồi thao tác | Guard/401/redirect rõ, không giữ mutation giả | Chưa test | | |

