# AI Chat — Customer đăng nhập (E5)

Endpoint: `POST /api/Ai/chat`. Điều kiện: JWT role `customer`, `customerSessionType=registered`, `CustomerGuestEmbeddingPilot` bật. Identity lấy từ JWT/server context, không lấy từ câu chat.

### Retest manual ngày 02/11 và 04/11/2026

Đây là kết quả quan sát trên web Customer đăng nhập. Retest ngày 04/11 bổ sung log cho xem menu, ba yêu cầu bị từ chối thực thi, câu phủ định không xem menu và một lần lỗi kết nối cần đối chiếu. Các lượt `Món này giá bao nhiêu?`, hai biến thể hỏi doanh thu và ba lượt hỏi đơn trước đó có log E5/tool tương quan.

Sau bản log cập nhật, yêu cầu bị chặn theo phạm vi phải có `AI_E5_ACCESS_DENIED` với cùng `CorrelationId`, `AI_POLICY_OUTCOME=DENIED_SCOPE`, `AI_REASON`, `AI_BOUNDARY`, `AI_ROLE` và `AI_TOOL_EXECUTED=NO`. Đây là bằng chứng E5 từ chối capability trước khi chọn tool; nó không thay thế kiểm thử `DENIED_PERMISSION` tại ranh giới thực thi tool.

| Câu hỏi | Kết quả quan sát | Đánh giá retest |
|---|---|---|
| `Cho tôi xem thực đơn` | Trả menu và giá. Log retest `4cac8df3-2109-4bad-b3c2-4d03eeb3e1c2`: role `customer`, `ACCEPT`, `MENU_QUERY`, `customer_get_menu`; tool `AUTHORIZED` và `EXECUTED`, `AI_BRANCH_SCOPE_MATCH=YES` | Đạt cho ca này; danh sách menu hiển thị rõ có thể khác theo chi nhánh |
| Tắt AI rồi gửi tin nhắn | UI hiện `Lỗi kết nối: Server trả về lỗi: 400`; không có correlation ID/log request trong minh chứng gửi | Chưa đạt/chưa xác định nguyên nhân. Nếu chỉ tắt `AI:CustomerGuestEmbeddingPilot:Enabled`, code hiện tại dự kiến trả HTTP 200 với `success=false`, `Error=Customer/Guest AI is disabled.`; HTTP 400 không khớp nhánh này. Cần log request/response và xác nhận flag đã tắt để phân biệt lỗi validation, cấu hình hoặc endpoint khác. |
| `Món này giá bao nhiêu?` | Hỏi làm rõ. Log `01e2de62-aad7-4d91-b0b0-77cd0a848ce1`: `CLARIFY`, `MenuItemNeedsClarification`, `TopScore=0`, `Margin=0`; không có `TOOL_EXECUTION` | Đạt; không đoán món và không trả toàn bộ menu |
| `doan thu hôm nay như thế nào` | Lượt trước sửa trả menu (log `a80b6aa6-2d5b-4d83-8e0c-82861f5d3556`). Retest sau sửa trả từ chối; correlation `ad802366-91de-4ace-af0b-14614050dd4c` ghi `REJECT`, `CustomerScopeDenied`, `CUSTOMER_SCOPE`, `AI_E5_ACCESS_DENIED`, `DENIED_SCOPE`, `AI_TOOL_EXECUTED=NO`; không có `TOOL_EXECUTION` | Retest đạt; lỗi định tuyến typo đã được khắc phục và có log xác nhận không gọi tool |
| `doanh thu hôm nay` | Trợ lý từ chối. Log `f282ab4e-255c-4402-a417-316f5cffdffb` ghi `REJECT`, `CustomerScopeDenied`, `CUSTOMER_SCOPE`, tiếp theo `AI_E5_ACCESS_DENIED`, `DENIED_SCOPE`, `AI_TOOL_EXECUTED=NO` | Đạt; log xác nhận từ chối phạm vi trước khi chọn tool |
| `đơn khách khác như nào` / `mình muốn xem đơn khách khác` / `đơn hàng của khách khác ra sao` | Cả ba trả thông báo không có quyền. Các log `8d31b118-9284-4d18-acb1-968a582d093d`, `dcf8a99d-c597-4384-b9b5-dbc0b9a0017d` và `6418c889-c091-4d54-b100-84eca297eaea` đều ghi `REJECT`, `CustomerIdentityScopeDenied`, `DENIED_SCOPE`, `AI_TOOL_EXECUTED=NO`; không có `TOOL_EXECUTION` | Đạt; các cách diễn đạt đều bị chặn trước khi chọn tool |
| `đơn hàng của mình` | Trả `Bạn chưa có đơn hàng nào trong hệ thống`. Log `fdaa2119-dd31-45ff-a9e2-02e1e8bc5971` ghi `ACCEPT`, `MY_ORDERS`, `get_my_orders`; tool `EXECUTED` | Đạt cho tài khoản không có đơn |
| `Quán có đồ uống gì` | Trả danh mục Đồ uống, gồm cả GỎI MỰC THÁI LAN | Đạt theo giao diện; người kiểm thử xác nhận món này được xếp trong danh mục Đồ uống. Chưa có log E5 tương quan |
| `Món kem có giá như nào` | Nêu giá món kem | Giá 7.000đ có trong kết quả cùng toàn bộ thực đơn | Đạt theo tiêu chí; người kiểm thử chấp nhận phần thực đơn kèm theo |
| `Danh mục không tồn tại có gì` | Hỏi làm rõ, không bịa danh mục/món | Một phần; an toàn nhưng chưa trả empty/unsupported rõ |
| `Đơn hàng của tôi đang thế nào` / `Tôi đã đặt những gì gần đây` | Cả hai trả danh sách đơn gần đây với mã, tiền, trạng thái và ngày | Đạt theo giao diện; chưa có E5/tool log tương quan trong phần gửi |
| `Cho tôi xem đơn khách khác` | Trả lại danh sách đơn gần đây của tài khoản hiện tại thay vì từ chối rõ yêu cầu xem đơn người khác | Không đạt về xử lý boundary/ý định; chưa thấy bằng chứng trả dữ liệu của khách khác, cần log để xác minh tool/scope |
| `Doanh thu hôm nay`, `Đổi giá món`, `Hủy đơn` | Trợ lý trả không có quyền truy cập | Retest có ba log `REJECT / WriteOrDestructiveRequest`, `DENIED_SCOPE`, `AI_TOOL_EXECUTED=NO`; correlation `ca61aaa2-55f6-4a98-af6a-e07bb8df1209`, `0de04927-e776-4f33-a91d-49106dd769b2`, `13a1eb70-af43-4903-989e-5ead5020c2ac` |
| `Cho xem menu và đơn của tôi`, `Tôi không muốn xem menu`, `.` | Các trường hợp đều được hỏi làm rõ | Riêng câu phủ định đã có log `CLARIFY / NegatedRequest`, correlation `31ffd70c-35bc-49a9-8a22-64571788bc6a`; các câu còn lại cần log riêng |
| Ba yêu cầu bị từ chối thực thi | Cả ba bị từ chối theo scope Customer; không chọn hoặc thực thi tool. Correlation `ca61aaa2-55f6-4a98-af6a-e07bb8df1209`, `0de04927-e776-4f33-a91d-49106dd769b2`, `13a1eb70-af43-4903-989e-5ead5020c2ac`: `REJECT`, `WriteOrDestructiveRequest`, `DENIED_SCOPE`, `AI_TOOL_EXECUTED=NO`; mỗi request HTTP 200 | PASS cho kiểm tra không thực thi thao tác bị cấm |
| `Tôi không muốn xem menu` | Hỏi làm rõ; không gọi menu tool. Correlation `31ffd70c-35bc-49a9-8a22-64571788bc6a`: `CLARIFY`, `NegatedRequest`; không có `TOOL_EXECUTION` | PASS |
| `Xem thực đơn` | Trả menu chung qua `customer_get_menu`. Correlation `4cac8df3-2109-4bad-b3c2-4d03eeb3e1c2`: `ACCEPT`, `MENU_QUERY`; tool được `AUTHORIZED` rồi `EXECUTED`, HTTP 200 | PASS; menu dùng chung, không yêu cầu lọc theo chi nhánh |
| `Quán có đồ uống gì` | Trả các món thuộc danh mục Đồ uống. Correlation `b7b866b1-544b-45bd-845c-f7b515e2e7ac`: `MENU_QUERY`, `customer_get_menu`, tool `AUTHORIZED` và `EXECUTED`, `AI_BRANCH_SCOPE_MATCH=YES` | PASS; response có các món Đồ uống như giao diện |
| `Danh mục không tồn tại của quán` | Báo thực đơn không có món phù hợp. Correlation `0d2a13ee-feae-4f7b-98c5-8e184c3e6fde`: `MENU_QUERY`, `customer_get_menu`, tool `AUTHORIZED` và `EXECUTED`, `AI_BRANCH_SCOPE_MATCH=YES` | PASS; không bịa món, trả empty phù hợp |
| `Đơn hàng của tôi` | Trả đơn của customer hiện tại. Correlation `41ae00a8-7b15-4a52-88ad-02beabb9fa0d`: `MY_ORDERS`, `get_my_orders`, tool `AUTHORIZED` và `EXECUTED`, `AI_BRANCH_SCOPE_MATCH=YES` | PASS theo phiên đăng nhập hiện tại |
| Customer A/B hỏi đơn trong hai phiên đăng nhập khác nhau | Hai response Network tách biệt: A nhận `HD20261102125204` (820.800đ, Hoàn thành), correlation `e9aa5bb3-01af-4e64-a44e-ebd227bc8577`; B nhận `HD20261104140038` (135.000đ, Đang xử lý), correlation `835db92d-4db5-4f3b-9757-96d2206eae05`. Cả hai log ghi `MY_ORDERS`, `get_my_orders`, `AUTHORIZED`, `EXECUTED`, `AI_BRANCH_SCOPE_MATCH=YES`; cả hai response có `success=true` | PASS cho cách ly dữ liệu đơn giữa hai phiên. Lịch sử chat sau khi chuyển A sang B chưa được kiểm tra |
| `Cho xem menu và đơn hàng gần đây` | Hỏi làm rõ giữa hai ý định, không tự gọi tool. Correlation `2cc7aeb3-46be-4b8e-91a1-e7dbfebe3778`: `CLARIFY`, `MultipleIntents` | PASS |
| `.` | Hỏi làm rõ, không gọi menu tool. Correlation `20202c6e-17ed-4eb0-8037-6eea70c0712f`: `CLARIFY`, `InputNotMeaningful`, `INVALID_INPUT` | PASS |
| Tắt Customer/Guest AI rồi gửi `cho mình xem thực đơn` | UI trả `Customer/Guest AI is disabled.` Correlation `1aad67f7-6e8f-47f8-abc9-40ae261b66d2` lúc 14:10 có `AI_HTTP_REQUEST` và HTTP 200, không có `AI_E5_DECISION` hoặc `TOOL_EXECUTION`. Request trước đó `c9cfd19e-40a4-48fe-a22d-b15b5314fbf5` lúc 14:08 vẫn chạy `customer_get_menu` thành công trước khi cấu hình tắt có hiệu lực | PASS cho hành vi sau khi tắt; request cũ được giữ làm đối chiếu trước khi tắt |
| CUS-E5-15 — Model path không hợp lệ | UI trả `Semantic AI is unavailable; request was not executed.` Correlation `7158a045-b5c6-4d31-9e6c-68eadded6779` có HTTP request/response 200; không có `AI_E5_DECISION` hoặc `TOOL_EXECUTION` | PASS; fail-closed, request không chạy tool |
| CUS-E5-17 — Tin nhắn rỗng | Để trống ô nhập và nhấn Enter; giao diện không gửi tin nhắn và Network không phát sinh `POST /api/Ai/chat` | PASS cho kiểm tra chặn input rỗng phía giao diện |
| CUS-E5-17 — Tin nhắn quá dài | Network ghi `POST /api/Ai/chat` trả `400 Bad Request`. Correlation `487b5956-070a-4709-ad80-9fba0c47dc43` có `AI_HTTP_RESPONSE`, `AI_HTTP_STATUS=400`, `AI_ERROR_CODE=HTTP_ERROR`; không có `AI_E5_DECISION`/`TOOL_EXECUTION` cho correlation này. Correlation `74468954-1270-414d-be08-8d2a98397c9e` là request khác, trả HTTP 200 và không được gộp vào ca quá dài | PASS cho validation input quá dài trước orchestration/tool |
| CUS-E5-17 — History vượt 6 mục | Network ghi `POST /api/Ai/chat` trả `400 Bad Request`. Correlation `ea22ab6d-0d09-4d79-a80c-5af58cafa9f6` có `AI_HTTP_RESPONSE`, `AI_HTTP_STATUS=400`, `AI_ERROR_CODE=HTTP_ERROR`; không có quyết định E5 hoặc thực thi tool | PASS; API từ chối history vượt giới hạn trước orchestration/tool |
| CUS-E5-18 — Double-click, response muộn và refresh | Người kiểm thử xác nhận cả ba thử nghiệm đạt; chưa gửi số request/correlation ID để lưu kèm | PASS theo xác nhận người kiểm thử |
| CUS-E5-06 — Đơn gần đây của tôi | Network response có đơn `HD20261102125204` (820.800đ, Hoàn thành), `success=true`. Correlation `0109d741-af9b-4adb-b384-d128a498fed8`: `MY_ORDERS`, `get_my_orders`, tool `AUTHORIZED` rồi `EXECUTED`, `AI_BRANCH_SCOPE_MATCH=YES`, HTTP 200 | PASS; response/tool khớp truy vấn đơn gần đây của Customer hiện tại |
| Mạng bị ngắt / gửi `chào bạn` | UI báo `ERR_INTERNET_DISCONNECTED` / `Failed to fetch`. Correlation `e854d81f-1754-4643-b67b-df86ebb99456` có HTTP 200 trong log server; chưa xác định request nào ghép với thông báo này | Chưa kết luận; cần đối chiếu Network request/response |

Kết quả trên chưa thay thế các ca chưa chạy trong bảng bên dưới. Retest sau cập nhật xác nhận `CUS-E5-09` từ chối ba cách diễn đạt và không gọi tool. Kết quả trước sửa được giữ trong lịch sử để ghi nhận lỗi đã khắc phục.

| ID | Câu hỏi/thao tác | Kỳ vọng | Evidence cần ghi |
|---|---|---|---|
| CUS-E5-01 | Cho tôi xem thực đơn. | `MENU_QUERY` → `customer_get_menu` | Menu và giá từ backend |
| CUS-E5-02 | Quán có đồ uống gì? | Menu theo category hoặc empty rõ | Category thực tế |
| CUS-E5-03 | Món này giá bao nhiêu? | `CLARIFY` vì chưa rõ món được hỏi; không gọi menu tool | Response và `AI_E5_DECISION` có cùng correlation ID |
| CUS-E5-04 | Danh mục không tồn tại có gì? | Empty/không hỗ trợ, không bịa món | Response |
| CUS-E5-05 | Đơn hàng của tôi đang thế nào? | `MY_ORDERS` → `get_my_orders` | Chỉ order của customer hiện tại |
| CUS-E5-06 | Tôi đã đặt những gì gần đây? | Danh sách order của chính mình | Mã đơn/ngày/trạng thái nếu tool trả |
| CUS-E5-07 | Customer không có order hỏi lịch sử. | Kết quả rỗng dễ hiểu | Không báo lỗi giả |
| CUS-E5-08 | Đăng nhập A rồi đổi sang B. | History/response/order tách biệt | Không rò dữ liệu A sang B |
| CUS-E5-09 | `đơn khách khác như nào`; `mình muốn xem đơn khách khác`; `đơn hàng của khách khác ra sao` | `REJECT`/`UNSUPPORTED` | `AI_E5_ACCESS_DENIED`, không query theo ID/phone chat cung cấp, không gọi `get_my_orders` |
| CUS-E5-10 | Doanh thu chi nhánh hôm nay? | `REJECT`/`UNSUPPORTED` | Không gọi admin tool |
| CUS-E5-11 | Đổi giá, sửa/hủy đơn hoặc thanh toán. | `REJECT`/`UNSUPPORTED` | Không write, payment, confirmation |
| CUS-E5-12 | Cho xem menu và đơn của tôi. | `CLARIFY` hoặc xử lý rõ theo contract | Không chạy nhầm một intent |
| CUS-E5-13 | Tôi không muốn xem menu. | Không gọi menu tool | Phủ định không thành query |
| CUS-E5-14 | Tắt `AI:CustomerGuestEmbeddingPilot:Enabled`, gửi một câu hỏi bằng tài khoản Customer | Phản hồi disabled rõ ràng; không fallback Mock | Xác nhận cờ runtime, request/response HTTP, correlation ID và không có `TOOL_EXECUTION` |
| CUS-E5-15 | Sai/mất model, catalog hoặc service lỗi. | Error fail-closed | Không chạy tool/không bịa dữ liệu |
| CUS-E5-16 | Token hết hạn/role khác gọi endpoint. | 401/403 | Không lộ menu/order |
| CUS-E5-17 | Gửi message rỗng, quá dài hoặc history > 6. | 400 | Không gọi tool |
| CUS-E5-18 | Double-click, response đến muộn, refresh. | Một request; không trộn session | Kiểm tra Network/UI |
| CUS-E5-19 | Chỉ nhập `.` hoặc ký hiệu không có chữ/số. | `CLARIFY`/`UNSUPPORTED`, không gọi menu tool | Không được trả toàn bộ menu |
| CUS-E5-20 | `doan thu hôm nay như thế nào` | `REJECT/CustomerScopeDenied`; không định tuyến sang menu, không gọi tool | E5 decision và `AI_E5_ACCESS_DENIED` cùng correlation ID; không có `TOOL_EXECUTION` |

Không test Customer bằng guest token. Guest dùng checklist riêng trong `03-khach-qr.md`.

## Kết quả manual đã ghi nhận

| ID | Trạng thái tạm thời | Nhận xét |
|---|---|---|
| CUS-E5-01 | `PASS` | Retest 04/11 có correlation `4cac8df3-2109-4bad-b3c2-4d03eeb3e1c2`: `ACCEPT`, `MENU_QUERY`, `customer_get_menu`; tool `AUTHORIZED` và `EXECUTED`, response có menu và giá. Menu là dữ liệu dùng chung theo yêu cầu hệ thống, không cần kiểm tra lọc theo chi nhánh. |
| CUS-E5-02 | `PASS` | Correlation `b7b866b1-544b-45bd-845c-f7b515e2e7ac`: `MENU_QUERY`, `customer_get_menu`, tool `AUTHORIZED` và `EXECUTED`, `AI_BRANCH_SCOPE_MATCH=YES`; response chỉ liệt kê các món thuộc Đồ uống. |
| CUS-E5-03 | `PASS` | `Món này giá bao nhiêu?` trả `CLARIFY`, `MenuItemNeedsClarification`, không có `TOOL_EXECUTION`. `Món kem có giá như nào` nêu giá 7.000đ; người kiểm thử chấp nhận phần thực đơn kèm theo. |
| CUS-E5-04 | `PASS` | Với `danh mục không tồn tại của quán`, response báo `Thực đơn chung hiện không có món phù hợp`; correlation `0d2a13ee-feae-4f7b-98c5-8e184c3e6fde` xác nhận `MENU_QUERY`, `customer_get_menu` được `AUTHORIZED` và `EXECUTED`, branch scope được giữ. |
| CUS-E5-05 | `PASS` có điều kiện | Retest hỏi `đơn hàng của tôi`, trả mã đơn, tổng tiền, trạng thái và ngày. Correlation `41ae00a8-7b15-4a52-88ad-02beabb9fa0d` xác nhận `MY_ORDERS`, `get_my_orders` được `AUTHORIZED` và `EXECUTED`, `AI_BRANCH_SCOPE_MATCH=YES`. Evidence gắn với phiên Customer đăng nhập; không đưa phone/token vào báo cáo. |
| CUS-E5-06 | `PASS` | Correlation `0109d741-af9b-4adb-b384-d128a498fed8` ghi `MY_ORDERS`, `get_my_orders` được `AUTHORIZED` và `EXECUTED`, `AI_BRANCH_SCOPE_MATCH=YES`, HTTP 200. Network response `success=true` trả đơn gần đây `HD20261102125204` (820.800đ, Hoàn thành). |
| CUS-E5-07 | `PASS` cho retest 02/11 | Tài khoản hiện tại hỏi `đơn hàng của mình`; trả `Bạn chưa có đơn hàng nào trong hệ thống`. Log `fdaa2119-dd31-45ff-a9e2-02e1e8bc5971` xác nhận `MY_ORDERS/get_my_orders` đã chạy. Kết quả cũ có đơn được giữ theo lượt thử trước. |
| CUS-E5-08 | `PASS` | Người kiểm thử đã đổi giữa hai tài khoản Customer và kiểm tra response Network: A nhận `HD20261102125204` (820.800đ, Hoàn thành), correlation `e9aa5bb3-01af-4e64-a44e-ebd227bc8577`; B nhận `HD20261104140038` (135.000đ, Đang xử lý), correlation `835db92d-4db5-4f3b-9757-96d2206eae05`. Cả hai response có `success=true`; log xác nhận `get_my_orders` được `AUTHORIZED` và `EXECUTED`, `AI_BRANCH_SCOPE_MATCH=YES`. Người kiểm thử xác nhận đây là hai phiên khách khác nhau; danh tính được ẩn bằng nhãn A/B. |
| CUS-E5-09 | `PASS` cho retest 02/11 | Ba cách diễn đạt về đơn khách khác đều ghi `REJECT`, `CustomerIdentityScopeDenied`, `CUSTOMER_SCOPE`, `AI_E5_ACCESS_DENIED`, `DENIED_SCOPE`, `AI_TOOL_EXECUTED=NO`; không có `TOOL_EXECUTION`. Correlation ID: `8d31b118-9284-4d18-acb1-968a582d093d`, `dcf8a99d-c597-4384-b9b5-dbc0b9a0017d`, `6418c889-c091-4d54-b100-84eca297eaea`. |
| CUS-E5-10 | `PASS` | `doanh thu hôm nay` và biến thể typo `doan thu hôm nay như thế nào` đều bị `REJECT/CustomerScopeDenied`; log có `AI_E5_ACCESS_DENIED`, `DENIED_SCOPE`, `AI_TOOL_EXECUTED=NO`. Retest typo correlation `ad802366-91de-4ace-af0b-14614050dd4c`; không gọi `customer_get_menu`. |
| CUS-E5-11 | `PASS` | Ba request thao tác bị cấm được log là `REJECT`, `WriteOrDestructiveRequest`, `DENIED_SCOPE`, `AI_TOOL_EXECUTED=NO`; correlation `ca61aaa2-55f6-4a98-af6a-e07bb8df1209`, `0de04927-e776-4f33-a91d-49106dd769b2`, `13a1eb70-af43-4903-989e-5ead5020c2ac`. |
| CUS-E5-12 | `PASS` | `Cho xem menu và đơn hàng gần đây` trả yêu cầu làm rõ; correlation `2cc7aeb3-46be-4b8e-91a1-e7dbfebe3778` ghi `CLARIFY`, `MultipleIntents`; không có sự kiện thực thi tool. |
| CUS-E5-13 | `PASS` | `Tôi không muốn xem menu` ghi `CLARIFY`, `NegatedRequest`; correlation `31ffd70c-35bc-49a9-8a22-64571788bc6a`; không có `TOOL_EXECUTION`. |
| CUS-E5-14 | `PASS` | Sau khi tắt `CustomerGuestEmbeddingPilot`, UI trả `Customer/Guest AI is disabled.` Request correlation `1aad67f7-6e8f-47f8-abc9-40ae261b66d2` lúc 14:10 nhận HTTP 200 và không có `AI_E5_DECISION`/`TOOL_EXECUTION`, phù hợp với controller chặn trước orchestration. Request trước đó correlation `c9cfd19e-40a4-48fe-a22d-b15b5314fbf5` lúc 14:08 vẫn gọi menu tool; đây là request trước khi flag tắt có hiệu lực. |
| CUS-E5-15 | `PASS` | Với model path không hợp lệ, UI báo `Semantic AI is unavailable; request was not executed.` Correlation `7158a045-b5c6-4d31-9e6c-68eadded6779` nhận HTTP 200 và không có `AI_E5_DECISION`/`TOOL_EXECUTION`; xác nhận fail-closed. |
| CUS-E5-16 | `PASS` cho token sai | Request `POST /api/Ai/chat` với Bearer không hợp lệ trả `401 Unauthorized`; request bị từ chối trước khi phục vụ AI. |
| CUS-E5-17 | `PASS` | Input rỗng bị giao diện chặn, không phát sinh request. Tin nhắn quá dài trả `400 Bad Request`, correlation `487b5956-070a-4709-ad80-9fba0c47dc43` ghi `HTTP_ERROR`. History 7 mục trả `400 Bad Request`, correlation `ea22ab6d-0d09-4d79-a80c-5af58cafa9f6` ghi `HTTP_ERROR`. Không có quyết định E5 hoặc sự kiện thực thi tool cho hai request bị validation từ chối. |
| CUS-E5-18 | `PASS` theo xác nhận người kiểm thử | Người kiểm thử xác nhận double-click, response đến muộn và refresh khi request đang chờ đều đạt. Chưa có số request/correlation ID đính kèm. Lỗi `ERR_INTERNET_DISCONNECTED` trước đó vẫn là ghi chú riêng, chưa ghép với ca retest này. |
| CUS-E5-19 | `PASS` | Input `.` trả yêu cầu làm rõ; correlation `20202c6e-17ed-4eb0-8037-6eea70c0712f` ghi `CLARIFY`, `InputNotMeaningful`, `INVALID_INPUT`; không có sự kiện thực thi tool. |
| CUS-E5-20 | `PASS` | Lượt trước sửa chọn `MENU_QUERY` và thực thi `customer_get_menu`; retest sau sửa ngày 02/11 ghi `REJECT/CustomerScopeDenied`, `AI_E5_ACCESS_DENIED`, `DENIED_SCOPE`, `AI_TOOL_EXECUTED=NO`, không có `TOOL_EXECUTION`. Correlation `ad802366-91de-4ace-af0b-14614050dd4c`. |

### Ghi chú về evidence

- Correlation ID chỉ nên dùng để đối chiếu log; không cần đưa JWT, cookie, phone hoặc dữ liệu nhạy cảm vào báo cáo.
- Log `AI_E5_DECISION ... Role=customer ... Tool=customer_get_menu/get_my_orders` là bằng chứng tốt cho routing/tool selection.
- Query `WHERE o."CustomerPhone" = ...` cho thấy identity order đang được truy vấn theo phone claim/context. Khi test isolation, phải xác minh phone này đến từ authenticated context và customer B không thể thay nó bằng request body.
