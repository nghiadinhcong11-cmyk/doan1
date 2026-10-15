# Chủ quán — lịch làm việc

- Route: `/schedule`
- Source: `WorkSchedulePage.tsx`, `WorkScheduleController`
- Trạng thái mọi case: `Chưa test`.

| ID | Chức năng | Các bước | Kết quả mong đợi | Trạng thái | Minh chứng/finding |
|---|---|---|---|---|---|
| ADM-SCH-01 | Tuần, branch, tìm kiếm và empty/error | Mở `/schedule`; đổi tuần/branch/tên; thử dữ liệu rỗng và request lỗi | Loading, empty và lỗi phân biệt; dữ liệu đúng scope | Chưa test | HR-002 |
| ADM-SCH-02 | Tạo lịch thất bại | Submit lịch thiếu/sai dữ liệu hoặc tạo 4xx/5xx/network failure | Modal còn mở, dữ liệu nhập còn nguyên, lỗi an toàn hiển thị, không refetch như thành công | Chưa test | HR-002 |
| ADM-SCH-03 | Tạo/xóa thành công và refetch lỗi | Tạo hoặc xóa với response success rồi làm refetch thất bại | Không gửi lại mutation đã thành công; UI nêu rõ mutation đã nhận nhưng dữ liệu tải lại chưa thành công | Chưa test | HR-002 residual |
| ADM-SCH-04 | Submit lặp | Bấm submit liên tục/nhấn Enter nhiều lần trong lúc request pending | Chỉ có một mutation được gửi | Chưa test | HR-002 residual |
