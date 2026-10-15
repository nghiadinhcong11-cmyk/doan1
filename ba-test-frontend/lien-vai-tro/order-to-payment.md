# Checklist liên vai trò — case đã phân loại

Các case nhiều vai trò/nhiều trang được kiểm tra ở đây; trạng thái ban đầu là Chưa test.

| ID | Luồng | Điều kiện/dữ liệu | Bước thực hiện | Kết quả mong đợi | Trạng thái | Minh chứng/finding |
|---|---|---|---|---|---|---|
| FLOW-01 | QR → nhân viên → bếp → POS → thu ngân | Bàn/QR Branch A, các phiên role | Guest gửi món; employee nhận/gửi bếp; kitchen đổi trạng thái; cashier mở thanh toán | Branch/order nhất quán; SignalR cập nhật đúng consumer; tổng khớp persisted | Chưa test | |
| FLOW-02 | Gọi thêm món trong cùng bữa | Order đang mở đã gửi bếp | Guest/customer gọi thêm; staff nhận; bếp xử lý | Đợt gọi phân biệt; không nhân đôi món; tổng/status nhất quán | Chưa test | |
| FLOW-03 | Bếp báo hết món | Món đang bán ở menu/POS/bếp | Bếp báo unavailable; refresh menu/POS; thử đặt | UI phản ánh backend; không đặt món đã hết theo contract | Chưa test | |
| FLOW-04 | Hai phiên cùng thao tác order | POS và customer/employee cùng order | Sửa/gửi/thanh toán gần đồng thời; refresh | Server quyết định trạng thái; không mất dữ liệu hoặc thu hai lần | Chưa test | |
| FLOW-05 | Cách ly hai branch | Branch A/B, tài khoản mỗi branch | Truy cập resource A bằng tài khoản B qua menu/URL | B không xem/sửa A; admin chỉ xem theo branch chọn | Chưa test | |
| FLOW-06 | Order đã thanh toán | Order paid | Thử thêm/sửa/xóa/đổi trạng thái/thanh toán lại từ các role | Backend từ chối trái quy tắc; invoice/order không bị frontend ghi đè | Chưa test | |
| FLOW-07 | Order đã hủy | Order cancelled | Thử gọi thêm/đổi trạng thái/thanh toán | Terminal state được tôn trọng; UI báo rõ | Chưa test | |
| FLOW-08 | QR guest → hóa đơn | Guest session tạo order | Staff nhận, gửi bếp, hoàn tất, mở invoice | Guest không đọc ngoài scope; invoice khớp totals | Chưa test | |
| FLOW-09 | AI confirmation phía server | Admin, Mock AI, sản phẩm test | Yêu cầu đổi giá; dừng ở confirmation; gọi lại/xác nhận | Không mutate trước confirmation; token bind user/item/expiry | Chưa test | |
| FLOW-10 | Hết hạn phiên giữa luồng | Form order/checkout đang mở | Làm token hết hạn trước submit rồi submit/login lại | API 401; không báo giả thành công; authoritative data không mất | Chưa test | |
| FLOW-11 | Reconnect realtime | POS và bếp đang mở | Ngắt mạng lúc có event; nối lại; tải lại danh sách | Kết nối/API state cuối đúng; không duplicate | Chưa test | |
| FLOW-12 | Empty/loading/error theo role | Branch rỗng hoặc API lỗi | Mở các màn hình từng role và refresh | Trạng thái rõ; không stack trace/secret | Chưa test | |
| FLOW-13 | Retry an toàn sau timeout | POST order/expense, mạng chập chờn | Submit một lần, tạo timeout/không rõ kết quả, retry có kiểm soát | Không tự retry mù; retry không duplicate | Chưa test | |
| FLOW-14 | Confirmation sai người/hết hạn | Admin A tạo confirmation; B hoặc token hết hạn | Xác nhận bằng B/item khác/sau expiry | Từ chối và không mutate | Chưa test | |
| FLOW-15 | Cache chỉ invalidate sau commit | Dashboard và mutation từ phiên khác | Quan sát request lỗi/rollback/commit rồi reload | Lỗi không đổi KPI; commit mới hiển thị dữ liệu mới | Chưa test | |
| FLOW-16 | Paid/cancelled immutable xuyên role | Một order paid và một cancelled | Thử mutation từ POS/customer/kitchen/employee | Mọi role tôn trọng terminal state; invoice/loyalty/KDS nhất quán | Chưa test | |
| FLOW-17 | QR/customer → staff → kitchen → cashier → invoice → dashboard | QR/customer, POS/KDS/dashboard cùng branch | Chạy toàn chuỗi; ghi role/branch/order code/thời gian | Order/table/kitchen/payment/invoice/summary đúng branch, không duplicate | Chưa test | |

Công cụ/điều kiện: môi trường local test riêng, DevTools Network, hai phiên/máy khi cần, database kiểm tra read-only khi được phép; chưa chạy trong lượt này.

