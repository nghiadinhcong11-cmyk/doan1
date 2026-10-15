# Bếp — /kitchen/tables

- Route/source: /kitchen/tables; TableStatusPage.tsx; KitchenRoutes.tsx; KitchenNavbar.tsx.
- Đây là checklist chứa bước test cho sơ đồ/trạng thái bàn.

| ID | Chức năng | Điều kiện/dữ liệu | Các bước | Kết quả mong đợi | Trạng thái | Kết quả thực tế | Minh chứng/finding |
|---|---|---|---|---|---|---|---|
| KIT-TBL-01 | Tải sơ đồ bàn và trạng thái rỗng/loading/error | Branch có bàn và branch không có bàn | Mở URL; quan sát loading; thử API chậm/lỗi | Bàn/status đúng branch; loading/empty/error rõ; không crash | Chưa test | | |
| KIT-TBL-02 | Lọc/chọn bàn và liên kết trạng thái | Có bàn trống/đang dùng/đang chờ nếu source hiển thị | Chọn từng bàn; refresh; đối chiếu trạng thái với order/KDS khi có | Hiển thị đúng dữ liệu read-only của bếp; không cho mutation ngoài UI contract | Chưa test | | |
| KIT-TBL-03 | Realtime/reload và branch isolation | Hai branch hoặc event thay đổi từ phiên khác | Mở hai phiên; đổi trạng thái ở nguồn được phép; reload/ngắt kết nối | Chỉ nhận dữ liệu branch đúng; reconnect lấy trạng thái cuối; không duplicate | Chưa test | | |
| KIT-TBL-04 | URL trực tiếp/quyền | Tài khoản kitchen và role khác | Mở /kitchen/tables trực tiếp; thử route từ role khác | Guard/backend xử lý đúng; không xem dữ liệu ngoài quyền | Chưa test | | |

