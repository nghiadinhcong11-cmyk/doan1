# Chủ quán — ca làm việc

- Route: `/shifts`
- Source: `ShiftManagement.tsx`, `ShiftController`
- Trang này trong owner navigation là đọc lịch sử; open/close ca nằm ở flow POS khác.
- Trạng thái mọi case: `Chưa test`.

| ID | Chức năng | Các bước | Kết quả mong đợi | Trạng thái | Minh chứng/finding |
|---|---|---|---|---|---|
| ADM-SHF-01 | Ngày, branch, empty state | Mở `/shifts`; đổi khoảng ngày và branch; thử dữ liệu rỗng | Filter và empty hiển thị đúng; không suy ra lỗi từ empty | Chưa test | HR-002 |
| ADM-SHF-02 | Lỗi request và refresh | Tạo 401/403/5xx/network failure cho Shift hoặc Branch; bấm refresh | Thông báo an toàn, loading kết thúc, refresh không gửi lặp khi đang pending | Chưa test | HR-002 |
| ADM-SHF-03 | Phân biệt dữ liệu ca và hành động UI | Kiểm tra nút refresh, filter và các nút hiển thị báo cáo | Chỉ hành động có handler được coi là thực thi; không coi trang read-only là có open/close mutation | Chưa test | HR-002; scope audit |
