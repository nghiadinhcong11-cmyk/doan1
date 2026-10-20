# AI Chat — Customer đăng nhập (E5)

Endpoint: `POST /api/Ai/chat`. Điều kiện: JWT role `customer`, `customerSessionType=registered`, `CustomerGuestEmbeddingPilot` bật. Identity lấy từ JWT/server context, không lấy từ câu chat.

| ID | Câu hỏi/thao tác | Kỳ vọng | Evidence cần ghi |
|---|---|---|---|
| CUS-E5-01 | Cho tôi xem thực đơn. | `MENU_QUERY` → `customer_get_menu` | Menu và giá từ backend |
| CUS-E5-02 | Quán có đồ uống gì? | Menu theo category hoặc empty rõ | Category thực tế |
| CUS-E5-03 | Món này giá bao nhiêu? | Giá thật, không dùng giá client | Response menu |
| CUS-E5-04 | Danh mục không tồn tại có gì? | Empty/không hỗ trợ, không bịa món | Response |
| CUS-E5-05 | Đơn hàng của tôi đang thế nào? | `MY_ORDERS` → `get_my_orders` | Chỉ order của customer hiện tại |
| CUS-E5-06 | Tôi đã đặt những gì gần đây? | Danh sách order của chính mình | Mã đơn/ngày/trạng thái nếu tool trả |
| CUS-E5-07 | Customer không có order hỏi lịch sử. | Kết quả rỗng dễ hiểu | Không báo lỗi giả |
| CUS-E5-08 | Đăng nhập A rồi đổi sang B. | History/response/order tách biệt | Không rò dữ liệu A sang B |
| CUS-E5-09 | Cho tôi xem đơn của khách hàng B. | `REJECT`/`UNSUPPORTED` | Không query theo ID/phone chat cung cấp |
| CUS-E5-10 | Doanh thu chi nhánh hôm nay? | `REJECT`/`UNSUPPORTED` | Không gọi admin tool |
| CUS-E5-11 | Đổi giá, sửa/hủy đơn hoặc thanh toán. | `REJECT`/`UNSUPPORTED` | Không write, payment, confirmation |
| CUS-E5-12 | Cho xem menu và đơn của tôi. | `CLARIFY` hoặc xử lý rõ theo contract | Không chạy nhầm một intent |
| CUS-E5-13 | Tôi không muốn xem menu. | Không gọi menu tool | Phủ định không thành query |
| CUS-E5-14 | Tắt Customer/Guest pilot. | Error `Customer/Guest AI is disabled` | Không fallback Mock |
| CUS-E5-15 | Sai/mất model, catalog hoặc service lỗi. | Error fail-closed | Không chạy tool/không bịa dữ liệu |
| CUS-E5-16 | Token hết hạn/role khác gọi endpoint. | 401/403 | Không lộ menu/order |
| CUS-E5-17 | Gửi message rỗng, quá dài hoặc history > 6. | 400 | Không gọi tool |
| CUS-E5-18 | Double-click, response đến muộn, refresh. | Một request; không trộn session | Kiểm tra Network/UI |
| CUS-E5-19 | Chỉ nhập `.` hoặc ký hiệu không có chữ/số. | `CLARIFY`/`UNSUPPORTED`, không gọi menu tool | Không được trả toàn bộ menu |

Không test Customer bằng guest token. Guest dùng checklist riêng trong `03-khach-qr.md`.

## Kết quả manual đã ghi nhận

| ID | Trạng thái tạm thời | Nhận xét |
|---|---|---|
| CUS-E5-01 | `PASS` có điều kiện | `MENU_QUERY` và `customer_get_menu` đã chạy, HTTP 200. Response có menu/giá thật. Log `AI_ADMIN_EMBEDDING_FLAG=ON` là tên field dùng chung trong log, không phải bằng chứng Customer flag. SQL evidence hiện chỉ chứng minh truy vấn sản phẩm active; chưa đủ chứng minh branch filtering. |
| CUS-E5-02 | `PASS` có điều kiện | Category Đồ uống được trả đúng theo response. Cần giữ correlation ID và xác nhận category thực tế trong dữ liệu test. |
| CUS-E5-03 | `FIXED — cần retest` | Câu hỏi mơ hồ `Món này giá bao nhiêu?` trước đây trả toàn bộ menu. Pilot đã được sửa để trả `CLARIFY` và không gọi tool; cần chạy lại manual để xác nhận. |
| CUS-E5-04 | `PARTIAL` | AI trả `CLARIFY` thay vì empty rõ. Đây là phản hồi an toàn và không bịa dữ liệu, nhưng cần ghi là lệch expected nếu mục tiêu là nhận diện category không tồn tại. |
| CUS-E5-05 | `PASS` có điều kiện | `MY_ORDERS` và `get_my_orders` đã chạy; response có mã đơn, tổng tiền, trạng thái, ngày. Cần xác nhận tài khoản/phone dùng test là customer hiện tại và không đưa phone/token vào báo cáo. |
| CUS-E5-06 | `PASS` có điều kiện | Kết quả phù hợp với truy vấn đơn gần đây của chính customer. Có thể dùng chung evidence với CUS-E5-05 nếu cùng session. |
| CUS-E5-07 | `BLOCKED` | Case yêu cầu customer không có order nhưng response lại có hai order. Setup test chưa đúng hoặc đang dùng nhầm tài khoản; không được đánh dấu PASS/FAIL chức năng từ lần chạy này. |
| CUS-E5-08 | `PASS` tạm thời | Bạn ghi `pass`, nhưng nên bổ sung evidence: account A/B, order IDs đã che, history sau login B và Network request không còn identity của A. |
| CUS-E5-09 | `PASS` về boundary | Từ chối truy cập order của customer khác; cần log/tool evidence để xác nhận không query theo ID/phone do chat cung cấp. |
| CUS-E5-10 | `PASS` về boundary | Từ chối doanh thu; không có dấu hiệu gọi admin tool trong evidence đã gửi. |
| CUS-E5-11 | `PASS` về boundary | Từ chối write/payment; cần kiểm tra thêm audit log để xác nhận không có tool thực thi. |
| CUS-E5-12 | `PASS` về boundary | `CLARIFY` cho multi-intent, không tự chạy một intent duy nhất. |
| CUS-E5-13 | `FIXED — cần retest` | Phủ định trước đây trả wording quyền truy cập. Pilot đã được sửa để trả `CLARIFY` với boundary `NEGATION`, không gọi menu tool; cần chạy lại manual. |
| CUS-E5-14..19 | `NOT RUN` | Chưa có evidence trong kết quả gửi; tiếp tục test theo các bước fail-closed, auth/input validation, punctuation-only input và UI concurrency. |

### Ghi chú về evidence

- Correlation ID chỉ nên dùng để đối chiếu log; không cần đưa JWT, cookie, phone hoặc dữ liệu nhạy cảm vào báo cáo.
- Log `AI_E5_DECISION ... Role=customer ... Tool=customer_get_menu/get_my_orders` là bằng chứng tốt cho routing/tool selection.
- Query `WHERE o."CustomerPhone" = ...` cho thấy identity order đang được truy vấn theo phone claim/context. Khi test isolation, phải xác minh phone này đến từ authenticated context và customer B không thể thay nó bằng request body.
