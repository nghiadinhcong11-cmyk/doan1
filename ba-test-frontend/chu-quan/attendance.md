# Chủ quán — chấm công

- Route: `/attendance`
- Source: `AttendanceManagement.tsx`, `AttendanceController`, `BranchController`
- Trạng thái mọi case: `Chưa test`.

| ID | Chức năng | Các bước | Kết quả mong đợi | Trạng thái | Minh chứng/finding |
|---|---|---|---|---|---|
| ADM-ATT-01 | Danh sách, filter và empty state | Mở `/attendance`; lọc branch/status/tên; thử dữ liệu có và không có | Loading, empty và dữ liệu hiển thị phân biệt; scope backend đúng | Chưa test | HR-002 |
| ADM-ATT-02 | Lỗi tải attendance/branch và retry | Dùng môi trường test để tạo 401/403/5xx/network failure cho từng request; bấm làm mới | Có thông báo an toàn, loading kết thúc, không coi lỗi là empty/success; retry không gửi lặp | Chưa test | HR-002 |
| ADM-ATT-03 | QR branch action và nút UI | Chọn branch, kích hoạt/đổi QR; kiểm tra các nút xuất/tải/in | QR dùng đúng branch; các nút có UI phải có hành vi rõ hoặc ghi nhận là chưa thực thi, không đánh dấu PASS chỉ vì hiện nút | Chưa test | HR-002; scope audit |
