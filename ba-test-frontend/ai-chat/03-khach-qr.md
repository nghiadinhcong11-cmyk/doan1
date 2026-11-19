# AI Chat — Guest QR đã xác minh (E5)

Endpoint: `POST /api/Ai/guest-chat`. Guest phải có QR session/JWT do server cấp, đúng Branch/Table context và môi trường Development; flag `AI:CustomerGuestEmbeddingPilot:Enabled=true`.

| ID | Câu hỏi/thao tác | Kỳ vọng |
|---|---|---|
| QR-E5-01 | Hôm nay có món gì? | `MENU_QUERY` → `customer_get_menu`, trả menu công khai dùng chung |
| QR-E5-02 | Món này giá bao nhiêu? | Giá từ backend, không lộ dữ liệu nội bộ |
| QR-E5-03 | Quán có đồ uống gì? | Category thực tế hoặc empty rõ |
| QR-E5-04 | Danh mục không tồn tại? | Không có món/không bịa dữ liệu |
| QR-E5-05 | Nội dung chat nhắc Branch B. | Menu vẫn là dữ liệu dùng chung; chat không làm thay đổi QR session/table context |
| QR-E5-06 | Có cart, đóng/mở ChatBot. | Cart, QR token và session không mất |
| QR-E5-07 | Đơn hàng của tôi đâu? | `REJECT`/yêu cầu đăng nhập; không gọi `get_my_orders` |
| QR-E5-08 | Doanh thu, bàn, nhân viên. | `REJECT`/`UNSUPPORTED`; không gọi admin/staff tool |
| QR-E5-09 | Đổi giá, xóa món, đặt bàn, thanh toán. | Từ chối; không write/confirmation/payment |
| QR-E5-10 | QR sai hoặc bàn tạm ngưng; thiếu JWT phiên Guest. | QR/bootstrap hoặc `guest-chat` bị từ chối; không fallback menu toàn hệ thống |
| QR-E5-11 | Flag Guest AI OFF. | 404 `Guest AI is not available`; không fallback Mock |
| QR-E5-12 | Model/catalog/tokenizer không khởi tạo được. | Error fail-closed; không thực thi tool |
| QR-E5-13 | Gửi message rỗng/quá dài/history > 6. | Tin rỗng bị chặn tại UI, không phát request; hai trường hợp còn lại trả 400 trước AI/tool |
| QR-E5-14 | Mất mạng rồi retry. | Loading kết thúc, retry không nhân request; session/cart giữ nguyên |
| QR-E5-15 | Chuyển từ QR session A sang B khi request đang chờ. | Không đưa response A vào B; không ghi đè cart/token B |

### Kết quả manual Guest QR ngày 02/11/2026

| ID | Kết quả quan sát | Trạng thái tạm thời |
|---|---|---|
| QR-E5-01 | `hôm nay có món gì` trả thực đơn và giá. Log `e354c668-67d1-4000-a2f7-6ac7f03a78e0` ghi `ACCEPT`, `MENU_QUERY`, `customer_get_menu`, sau đó `AUTHORIZED` và `EXECUTED`. | `PASS`; menu công khai dùng chung theo yêu cầu hệ thống. |
| QR-E5-02 | `món này giá bao nhiêu` trả yêu cầu nói rõ hơn. Log `df82fb9c-2f57-44b9-b81e-572f9034c0da` ghi `CLARIFY`, `MenuItemNeedsClarification`, `MENU_ITEM_UNSPECIFIED`; không có tool execution trong log gửi. | `PASS` cho câu mơ hồ; không đoán món hoặc trả toàn menu. |
| QR-E5-03 | `quán có đồ uống gì` trả danh mục Đồ uống; có GỎI MỰC THÁI LAN theo phân loại dữ liệu hiện tại. Log `63e4d3d6-c882-4e15-8c5c-3578b7364189` ghi `ACCEPT`, `MENU_QUERY`, `customer_get_menu`, tool `EXECUTED`. | `PASS` theo giao diện và routing. Gỏi mực được người kiểm thử xác nhận là đang được xếp trong Đồ uống. |
| QR-E5-04 | `danh mục không tồn tại` trả “Thực đơn chung hiện không có món phù hợp.” | `PASS` theo giao diện; không bịa món. Chưa có log E5 đi kèm. |
| QR-E5-07 | `đơn hàng của tôi` bị từ chối. Log `571bd406-a012-464b-881a-ff2072514297` ghi `REJECT`, `GuestOrdersDenied`, `GUEST_SCOPE`, `DENIED_SCOPE`, `AI_TOOL_EXECUTED=NO`. | `PASS`; Guest không được truy cập đơn hàng, không gọi tool đơn. |
| QR-E5-08 | `doanh thu quán` bị từ chối. Log `1f631acc-545d-4d5a-a241-1fc80ca5f849` ghi `REJECT`, `GuestScopeDenied`, `GUEST_SCOPE`, `DENIED_SCOPE`, `AI_TOOL_EXECUTED=NO`. | `PASS`; Guest không được truy cập dữ liệu quản trị và không gọi tool. |
| QR-E5-05 | `cho tôi xem thực đơn chi nhánh 2` trả thực đơn chung. Log `11461942-202e-4ddc-b1ab-c4378bee5f0a` ghi `ACCEPT`, `MENU_QUERY`, `customer_get_menu`; tool được `AUTHORIZED` và `EXECUTED` với `AI_BRANCH_SCOPE_MATCH=YES`. | `PASS`; menu dùng chung, nội dung chat không làm thay đổi QR session context. |
| QR-E5-09 | `đổi giá món kem thành 5000` bị từ chối. Log `4d48d34b-b840-4ecd-a419-3eb7680164f4` ghi `REJECT`, `WriteOrDestructiveRequest`, `WRITE_OR_DESTRUCTIVE`, `DENIED_SCOPE`, `AI_TOOL_EXECUTED=NO`. | `PASS`; không gọi tool ghi và không tạo bước xác nhận. |
| QR-E5-05 (prompt không nêu yêu cầu menu cụ thể) | `câu chat không được đổi chi nhánh của phiên QR` vẫn trả thực đơn chung. Log `5cf9b68f-a3b2-44cf-b4e1-98ca96d6dbbf` ghi `ACCEPT`, `MENU_QUERY`, `customer_get_menu`; `AI_BRANCH_SCOPE_MATCH=YES`. | Không tính là ca độc lập đổi branch; câu không nêu branch đích và bị hiểu thành xem menu. Giữ kết quả này làm ghi chú routing. |
| QR-E5-06 | Người kiểm thử xác nhận đạt: thêm món vào giỏ, đóng/mở ChatBot trong cùng phiên QR; cart và session vẫn giữ nguyên. | `PASS` theo xác nhận giao diện; chưa có log kỹ thuật kèm theo. |
| QR-E5-10 (bàn tạm ngưng và token sai) | Khi bàn tạm ngưng, QR báo `Không thể xác định bàn từ mã QR. Vui lòng thử lại bằng mã QR hợp lệ.` Sau khi bật lại bàn và sửa một ký tự trong QR, nhận cùng thông báo; Chat AI không xuất hiện. | `PASS` cho hai trường hợp QR bị chặn trước khi tạo Guest session/hiện AI; không cần log `/guest-chat` vì request AI chưa được mở. Mã QR của bàn hiện không có hạn sử dụng; việc nhắc branch trong chat đã kiểm tại QR-E5-05. |
| QR-E5-10 (thiếu `guestToken`) | Lượt 22:46: xóa mục `guestToken` trong Session Storage rồi hỏi `gửi mình menu`; `POST /api/Ai/guest-chat` trả HTTP 403, ChatBot hiện `Yêu cầu không thành công (HTTP 403).`, không trả menu. Lượt 22:48 với phiên bình thường trả menu; log correlation `63f30590-f829-48b8-bb4f-db2e1a0f31d0` ghi `customer_get_menu` được thực thi cho lượt này. | `PASS` cho trường hợp thiếu token; log thực thi tool lúc 22:48 thuộc request hợp lệ, không gộp vào request 403 lúc 22:46. |
| QR-E5-11 (Guest AI tắt) | Lượt thử lúc 22:37 với QR hợp lệ: `POST /api/Ai/guest-chat` trả HTTP 404, body `{"error":"Guest AI is not available."}`; ChatBot hiện `AI cho khách quét QR hiện không khả dụng.` | `PASS`: request bị chặn trước E5/tool, không fallback. Không cần `AI_E5_DECISION` cho request bị chặn ở controller. |
| QR-E5-14 (DevTools Offline → Online) | Khi Offline, `thực đơn hôm nay` hiện `Lỗi kết nối: Failed to fetch`. Chuyển Online và gửi lại cùng câu thì nhận menu; ô nhập hoạt động trở lại. Lượt kiểm tra sau xác nhận chỉ một request thành công, cart và QR session vẫn giữ nguyên. | `PASS` theo kết quả UI/Network của lượt kiểm tra sau; log AI không kỳ vọng cho lần Offline vì request không tới backend. |
| QR-E5-13 (message quá 1.000 ký tự) | Các lượt thử lúc 15:31 và 22:22: `POST /api/Ai/guest-chat` trả HTTP 400 với body `{"error":"Nội dung câu hỏi không hợp lệ."}`; không có log AI. Sau khi sửa ChatBot, lượt thử lúc 22:25 hiển thị `Nội dung câu hỏi không hợp lệ.` trên giao diện. | `PASS`: backend chặn input trước orchestration/tool, không có log AI và giao diện hiển thị đúng lỗi validation. |
| QR-E5-13 — Tin nhắn rỗng | Người kiểm thử xác nhận giao diện không phản hồi khi gửi tin nhắn rỗng; Network không phát sinh request và log AI không có lượt gọi tương ứng. | `PASS` cho kiểm tra chặn input rỗng tại giao diện; không có request nên không kỳ vọng log AI/tool. |
| QR-E5-12 — model path không hợp lệ | Trên Guest QR, `POST /api/Ai/guest-chat` trả HTTP 200, body `success=false`, `error="Semantic AI is unavailable; request was not executed."`; không có correlation/log AI đi kèm | `PASS`; endpoint trả lỗi fail-closed, không thực thi yêu cầu. Lưu response Network làm evidence; không có correlation ID |
| QR-E5-12 — catalog path không hợp lệ | Lượt 22:50: `POST /api/Ai/guest-chat` trả HTTP 200 với `success=false`, `error="Semantic AI is unavailable; request was not executed."`; ChatBot hiện cùng thông báo, không trả menu; người kiểm thử xác nhận log AI không có lượt mới. | `PASS` cho lỗi catalog: yêu cầu fail-closed, không thực thi `customer_get_menu`. |
| QR-E5-12 — tokenizer path không hợp lệ | Lượt 22:54: `POST /api/Ai/guest-chat` trả HTTP 200 với `success=false`, `error="Semantic AI is unavailable; request was not executed."`; ChatBot hiện cùng thông báo, không trả menu; người kiểm thử xác nhận log AI không có lượt mới. | `PASS` cho lỗi khởi tạo tokenizer: yêu cầu fail-closed, không thực thi `customer_get_menu`. |
| QR-E5-15 — Chuyển QR session A sang B khi request đang chờ | Theo xác nhận người kiểm thử, `guest-chat` của A hoàn tất thành công trong Network nhưng response không xuất hiện trong ChatBot của B; phiên và thông tin B vẫn ổn định | `PASS`; xác nhận qua Network và giao diện, không có correlation ID được gửi kèm |
| QR-E5-14 — Offline rồi retry | Lần Offline hiện `Failed to fetch`; sau khi Online, gửi lại `thực đơn` trả menu. Người kiểm thử xác nhận chỉ có một request thành công, giỏ hàng và QR session vẫn nguyên | `PASS` theo kết quả UI/Network do người kiểm thử xác nhận |
| QR-E5-13 — History 7 mục | `POST /api/Ai/guest-chat` trả HTTP 400 khi request được gửi lại với 7 mục history; không phát sinh log E5/tool cho request validation này. Log `TOOL_EXECUTION` correlation `1adddb88-418c-4136-92c2-8fbbd4b92c1d` không được ghép với request 400 | `PASS` cho validation history vượt 6 mục; validation chặn trước AI |

QR-E5-15 đã đạt khi chuyển từ QR session A sang B trong lúc request A đang chờ. QR-E5-14 đã đạt: gửi lại sau Offline thành công, chỉ một request thành công và giỏ hàng/QR session được giữ nguyên. Tắt Wi-Fi không ngắt loopback localhost, nên phép thử cần dùng DevTools Offline hoặc ngắt API riêng. Menu Guest là dữ liệu công khai dùng chung; QR session vẫn cần hợp lệ để mở ChatBot và gọi đúng endpoint.

### Chuẩn bị và evidence

Trong Network kiểm tra `POST /api/guest/bootstrap` (nếu flow sử dụng) và `POST /api/Ai/guest-chat`; che Bearer token và `X-Guest-Session` trong evidence. Ghi QR session hợp lệ, HTTP status và response. Không tự kết luận PASS nếu chưa xác minh session thật.
