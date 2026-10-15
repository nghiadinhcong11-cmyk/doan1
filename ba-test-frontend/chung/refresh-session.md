# Checklist refresh/session — case đã phân loại

| ID | Phạm vi | Cách kiểm tra | Kết quả mong đợi | Trạng thái | Minh chứng/finding |
|---|---|---|---|---|---|
| REF-01 | Dashboard và Invoice History | Bấm refresh, đổi filter/page/branch, theo dõi Network | Chỉ tải khi mở/đổi truy vấn; không polling ngầm; loading/error đúng | Chưa test | |
| REF-02 | Invoice History trong POS | Mở/đóng modal, đổi filter/page/branch, nhận PaymentCompleted/reconnect | Tải đúng lúc; event không tự tải lịch sử trái thiết kế; payment state đúng | Chưa test | |
| REF-03 | POS/KDS/Table Status | Visible/hidden tab, API chậm, đổi branch; đo polling | Chu kỳ theo source; chỉ polling khi visible; cleanup/event đúng | Chưa test | |
| REF-04 | Dashboard cache và PaymentCompleted | Mutation, bấm refresh dashboard, thanh toán phiên khác | TTL/invalidation đúng; payment/POS/notification vẫn hoạt động | Chưa test | |
| REF-05 | Thanh toán cùng order từ hai máy | A mở payment, B thanh toán, A polling rồi thử lại | A nhận trạng thái mới; backend từ chối thanh toán lặp | Chưa test | |
| REF-06 | Consent/customer scope khi thanh toán | Customer tồn tại/chưa có member; consent bật/tắt; order có owner | createMember/ownership/branch scope đúng contract | Chưa test | |
| REF-07 | Invalidation từ UI và Mock AI | Đổi status qua UI và Mock AI; thử success/failure/rollback | Chỉ invalidate sau commit; lỗi không làm KPI thay đổi | Chưa test | |

Dùng DevTools Network để tách Fetch/XHR, SignalR/WebSocket và asset; chưa chạy runtime trong lượt này.

