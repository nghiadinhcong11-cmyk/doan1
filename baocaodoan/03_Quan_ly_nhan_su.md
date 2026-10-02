# 03. Quản lý nhân sự (HRM)

Module HRM hiện quản lý hồ sơ nhân viên, chi nhánh, vai trò, lịch làm việc, ca và chấm công. Payroll, Salary Profile và các chức năng tính lương không thuộc phạm vi hệ thống hiện tại.

Chức năng chính: quản lý hồ sơ/trạng thái nhân viên; Shift và WorkSchedule; Attendance; AI Assistant đọc lịch ca khi được cấp quyền.

Các endpoint Employee, Attendance, Shift và WorkSchedule được bảo vệ theo role/branch ở backend. Entity hiện tại gồm Employee, Attendance, Shift và WorkSchedule. Payroll entity/stub và trường lương đã được loại khỏi current source/model; migration cũ có thể vẫn chứa lịch sử schema.
