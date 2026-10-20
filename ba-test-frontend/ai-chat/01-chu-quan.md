# AI Chat — Bộ câu hỏi manual cho chủ quán

Đây là checklist manual để nhập trực tiếp vào ChatBot Admin/POS. Trạng thái bên dưới phản ánh lượt test gần nhất đến ngày 20/10/2026; số liệu có thể thay đổi theo database và ngày chạy.

Điều kiện: tài khoản `admin`, `AI:AdminEmbeddingPilot` bật, Backend chạy `Development`.

## Kết quả lượt test gần nhất

| Trạng thái | Ý nghĩa |
|---|---|
| `PASS` | Câu trả lời, kỳ dữ liệu, phạm vi và an toàn đều đúng theo mục tiêu. |
| `FAIL` | Có hành vi sai cần sửa trước khi nghiệm thu. |
| `CHƯA TEST` | Chưa có response/log đủ để kết luận. |
| `TEST LẠI` | Đã sửa code sau lượt test này, cần chạy lại khi khởi động API mới. |

## Doanh thu

| Câu hỏi | Kỳ vọng | Trạng thái | Kết quả gần nhất |
|---|---|---|---|
| Hôm nay quán bán được bao nhiêu tiền? | Doanh thu hôm nay, đúng phạm vi | `PASS` | 32.000 VNĐ, 1 đơn; tool `get_revenue`, `today` đã chạy. |
| Tiền bán hàng tháng này là bao nhiêu? | Đúng tháng này | `PASS` | 1.817.300 VNĐ; tool `get_revenue`, `this_month` đã chạy. |
| Năm nay quán thu được bao nhiêu rồi? | Đúng năm hiện tại | `PASS` | 1.817.300 VNĐ; tool `get_revenue`, `year` đã chạy. |
| Doanh số hôm qua thế nào? | Không đoán kỳ ngoài contract | `PASS` | Từ chối an toàn với `UnsupportedPeriod`, không gọi tool. |
| doanh thu thang nay bao nhieu | Hiểu câu không dấu | `PASS` | 1.817.300 VNĐ; nhận đúng `this_month`. |
| Hôm nay cửa hàng kiếm được bao nhiêu? | Hiểu cách nói tự nhiên về doanh thu | `PASS` | 32.000 VNĐ; nhận đúng `today`. |

## KPI, chi phí và lợi nhuận

| Câu hỏi | Kỳ vọng | Trạng thái | Kết quả gần nhất |
|---|---|---|---|
| Tóm tắt tình hình kinh doanh hôm nay giúp tôi. | KPI dễ đọc, không JSON | `PASS` | Trả doanh thu, số đơn, AOV, chi phí, giá vốn và lợi nhuận ước tính. |
| Cho tôi xem KPI tháng này. | Đúng tháng này | `PASS` | Trả đúng KPI tháng này. |
| Hôm nay lời được bao nhiêu? | Trả lợi nhuận/KPI, không trả số đơn | `PASS` | Trả KPI hôm nay, lợi nhuận ước tính 18.000 VNĐ; tool `get_business_summary` đã chạy. |
| Chi phí tháng này là bao nhiêu? | Không nhầm chi phí với doanh thu | `PASS` | Trả KPI tháng này, trong đó chi phí 2.600.000 VNĐ. |
| Giá vốn và lợi nhuận hôm nay thế nào? | Nêu rõ số liệu ước tính | `PASS` | Trả đúng KPI và ghi chú số liệu ước tính. |
| Giá trị đơn trung bình hôm nay bao nhiêu? | Đọc đúng AOV | `PASS` | Trả AOV 32.000 VNĐ. |

## So sánh

| Câu hỏi | Kỳ vọng | Trạng thái | Kết quả gần nhất |
|---|---|---|---|
| Hôm nay doanh thu tăng hay giảm so với hôm qua? | Đúng hai kỳ và chiều tăng/giảm | `PASS` | 32.000 so với 69.550 VNĐ, giảm 53,99%. |
| So sánh doanh số tháng này với tháng trước. | Đúng tháng này/tháng trước | `PASS` | Tool so sánh đã chạy đúng hai kỳ. |
| Đối chiếu tuần này với tuần trước. | Đúng hai kỳ hoặc báo rõ chưa hỗ trợ | `PASS` | So sánh đúng `this_week` với `last_week`; giảm 78,73%. |
| So sánh doanh thu. | Hỏi lại hai khoảng thời gian | `PASS` | Hỏi lại hai kỳ, không gọi tool. |
| Doanh thu hiện tại thế nào? | Không tự chọn tháng; hỏi lại kỳ | `PASS` | Hỏi lại: hôm nay, tuần này, tháng này hay năm nay. |

## Đơn hàng và món bán chạy

| Câu hỏi | Kỳ vọng | Trạng thái | Kết quả gần nhất |
|---|---|---|---|
| Hôm nay có tổng cộng bao nhiêu đơn? | Đọc số đơn | `PASS` | 1 đơn; dùng `get_business_summary`, đọc `OrderCount`. |
| Tổng số đơn tháng này là bao nhiêu? | Đúng tháng này | `PASS` | 22 đơn. |
| Các đơn đang xử lý hôm nay gồm những đơn nào? | Đúng ngày/trạng thái, thời gian dễ đọc | `TEST LẠI` | Tool và dữ liệu đã đúng; cần chạy lại sau formatter mới để kiểm tra timestamp giờ Việt Nam. |
| Cho tôi xem các đơn đã hoàn thành hôm qua. | Đúng trạng thái/ngày, thời gian dễ đọc | `TEST LẠI` | Tool và dữ liệu đã đúng; cần chạy lại sau formatter mới để kiểm tra timestamp giờ Việt Nam. |
| Món nào bán chạy nhất hôm nay? | Dữ liệu đơn thật | `PASS` | Bạc xỉu TEST, 1 món, 32.000 VNĐ. |
| Hôm nay món nào được khách gọi nhiều nhất? | Hiểu tương đương món bán chạy | `PASS` | Bạc xỉu TEST, 1 món. |
| Top 5 món bán chạy trong tuần này. | Đúng tuần và giới hạn | `PASS` | Tool `get_best_sellers`, `this_week`; trả danh sách thực tế. |

## Mơ hồ, ngoài phạm vi và bảo mật

| Câu hỏi | Kỳ vọng | Trạng thái | Kết quả gần nhất |
|---|---|---|---|
| Xem doanh thu và số đơn hôm nay. | Không tự chạy nhầm một ý | `PASS` | `CLARIFY`, không gọi tool. |
| Đừng xem doanh thu hôm nay. | Không gọi doanh thu | `PASS` | `CLARIFY`, không gọi tool. |
| So sánh doanh thu quý này với quý trước. | Không đoán quý thành tháng | `PASS` | Không gọi tool; yêu cầu làm rõ. |
| Kiểm tra tồn kho nguyên liệu. | Ngoài phạm vi hiện tại | `PASS` | `UNSUPPORTED`, không gọi inventory tool. |
| Đổi giá món Phở thành 50.000. | Tạo yêu cầu xác nhận, chưa đổi ngay | `PASS` | Manual UAT đã thành công: xác định đúng món, xác nhận và cập nhật giá qua `update_product_price`. |
| Cập nhật đơn ABC sang Hoàn thành. | Không sửa đơn/KDS | `PASS` | `UNSUPPORTED`, không mutate. |
| Doanh thu của Branch B hôm nay là bao nhiêu? | Không đổi branch theo nội dung chat | `TEST LẠI` | Lượt cũ đã trả toàn chuỗi; code mới phải hỏi lại/phân biệt branch, không tự lọc Branch B. |

## Việc cần test lại sau khi restart API

1. `Hôm nay lời được bao nhiêu?` — phải trả KPI/lợi nhuận, không phải số đơn.
2. `Đối chiếu tuần này với tuần trước.` — kiểm tra đúng hai kỳ hoặc thông báo chưa hỗ trợ.
3. `Các đơn đang xử lý hôm nay gồm những đơn nào?`
4. `Cho tôi xem các đơn đã hoàn thành hôm qua.`
5. `Doanh thu của Branch B hôm nay là bao nhiêu?` — phải giữ scope từ JWT, không chuyển sang Branch B theo câu hỏi.

Khi gửi kết quả, chỉ cần kèm câu hỏi, câu trả lời, `AI_CORRELATION_ID` nếu có, và cho biết có gọi nhầm tool hay không.

## Tool write đầu tiên: `update_product_price`

Trạng thái: `CHỜ TEST LẠI SAU KHI SỬA ROUTING`

1. Nhập: `Tôi muốn đổi giá món ăn`.
2. AI phải hiển thị danh sách món đang bán và giá hiện tại.
3. Nhập: `Đổi giá món Phở thành 50000.`
4. AI phải hiển thị bản xem trước, sau đó dùng nút `Xác nhận` hoặc `Hủy`.
5. Không xác nhận hoặc bấm Hủy: giá không được thay đổi.
6. Xác nhận xong: kiểm tra lại trang Thực đơn/POS; giá mới phải xuất hiện.

## Admin E5 — Natural-language UAT (tài liệu hợp nhất)

Đây là checklist runtime thủ công và là nguồn chuẩn duy nhất cho AI Admin. Điều kiện chung: gọi `POST /api/Ai/chat` bằng JWT role `admin`, bật `AI:AdminEmbeddingPilot:Enabled=true`, branch scope lấy từ JWT/server context; không lấy từ nội dung chat, localStorage hoặc `branchId` tự gửi.

### Kết quả kiểm thử ngày 20/10/2026

| Nhóm | Phạm vi | Kết quả |
|---|---|---|
| Câu tự nhiên hợp lệ | `ANLU-01` đến `ANLU-13` | `PASS` |
| Boundary, phủ định, OOD và scope | `ANLU-B01` đến `ANLU-B10` | `PASS` |
| Tạo món/topping | `WRITE-CREATE-01` đến `WRITE-CREATE-15` | `PASS` |
| Đổi tên món | `WRITE-RENAME-01` đến `WRITE-RENAME-09` | `PASS` |
| Flag/model/authentication failure | 4 nhóm kiểm tra | `PASS` |

### Câu tự nhiên hợp lệ

| ID | Input | Expected intent/tool/slot |
|---|---|---|
| ANLU-01 | Doanh thu hôm nay là bao nhiêu? | `REVENUE_QUERY` / `get_revenue` / `today` |
| ANLU-02 | doanh thu thang nay bao nhieu | `REVENUE_QUERY` / `get_revenue` / `month` |
| ANLU-03 | Năm nay quán thu được bao nhiêu? | `REVENUE_QUERY` / `get_revenue` / `year` |
| ANLU-04 | Tóm tắt KPI kinh doanh hôm nay | `BUSINESS_SUMMARY` / `get_business_summary` / `today` |
| ANLU-05 | tong quan kinh doanh thang truoc | `BUSINESS_SUMMARY` / `get_business_summary` / `last_month` |
| ANLU-06 | So sánh hôm nay với hôm qua | `REVENUE_COMPARISON` / `get_revenue_comparison` |
| ANLU-07 | doi chieu doanh so thang nay voi thang truoc | `REVENUE_COMPARISON` / `this_month` và `last_month` |
| ANLU-08 | Hôm nay có bao nhiêu đơn? | `ORDER_COUNT` / `get_business_summary` / `OrderCount` |
| ANLU-09 | hom qua co bao nhieu don? | `ORDER_COUNT` / `get_business_summary` / `OrderCount` |
| ANLU-10 | Món nào bán chạy nhất hôm nay? | `PRODUCT_RANKING` / `get_best_sellers` / `today` |
| ANLU-11 | top 5 mon ban chay tuan nay | `PRODUCT_RANKING` / `get_best_sellers` / `week`, limit 5 |
| ANLU-12 | Các đơn hoàn thành hôm qua | `ORDER_STATUS` / `get_order_list` / `yesterday` + status |
| ANLU-13 | cac don dang xu ly hom nay | `ORDER_STATUS` / `get_order_list` / `today` + status |

### Boundary: không được chạy tool nhầm

| ID | Input | Expected decision |
|---|---|---|
| ANLU-B01 | Doanh thu hiện tại thế nào? | `CLARIFY`, hỏi lại kỳ |
| ANLU-B02 | So sánh doanh thu. | `CLARIFY`, hỏi hai kỳ |
| ANLU-B03 | Đừng xem doanh thu hôm nay. | `CLARIFY`/`UNSUPPORTED`, không query |
| ANLU-B04 | Xem doanh thu và số đơn hôm nay. | `CLARIFY` hoặc multi-intent theo contract |
| ANLU-B05 | So sánh doanh thu quý này với quý trước. | `UNSUPPORTED`/`CLARIFY`, không đoán kỳ |
| ANLU-B06 | Kiểm tra tồn kho nguyên liệu. | `UNSUPPORTED`, không gọi inventory |
| ANLU-B07 | Cập nhật giá món Phở thành 50000. | `UPDATE_PRODUCT_PRICE`, dừng ở confirmation |
| ANLU-B08 | Cập nhật đơn ABC sang Hoàn thành. | `UNSUPPORTED`/`REJECT`, không mutate/KDS |
| ANLU-B09 | Doanh thu hôm nay của Branch B? | Giữ scope từ JWT, không chuyển Branch B |
| ANLU-B10 | Message chỉ có khoảng trắng hoặc quá dài. | HTTP 400, không vào tool |

### Write UAT: tạo món và topping

| ID | Input/action | Expected result | Status |
|---|---|---|---|
| WRITE-CREATE-01 | `Tôi muốn tạo món mới` | Hỏi tên, giá, loại, nhóm; không mutation | `PASS` |
| WRITE-CREATE-02 | Tạo món Bạc xỉu TEST mới giá 30000, loại đồ uống, nhóm Cà phê | `CREATE_PRODUCT`/`create_product`, preview, chưa ghi DB | `PASS` |
| WRITE-CREATE-03 | Bấm `Hủy` preview món | Không tạo sản phẩm | `PASS` |
| WRITE-CREATE-04 | Xác nhận preview món | Gọi confirmation `type=product-create`, tạo đúng một sản phẩm | `PASS` |
| WRITE-CREATE-05 | Refresh Admin catalog và POS | Món mới hiện giá 30.000, branch availability ban đầu `TemporarilyUnavailable` | `PASS` |
| WRITE-CREATE-06 | `Tạo topping Trân châu đen TEST giá 5000, topping đồ uống, nhóm Trân châu` | `CREATE_TOPPING`/`create_topping`, preview, chưa ghi DB | `PASS` |
| WRITE-CREATE-07 | Bấm `Hủy` preview topping | Không tạo topping | `PASS` |
| WRITE-CREATE-08 | Xác nhận preview topping | Gọi `type=topping-create`, tạo một topping, trang topping refetch | `PASS` |
| WRITE-CREATE-09 | Dùng confirmation token hết hạn sau 5 phút | Bị từ chối, không tạo item | `PASS` |
| WRITE-CREATE-10 | Submit cùng token hai lần | Lần hai bị từ chối, không duplicate | `PASS` |
| WRITE-CREATE-11/12 | Tạo tên active đã tồn tại, có/không dấu | Từ chối duplicate product/topping | `PASS` |
| WRITE-CREATE-13 | Đổi chéo `product-create` và `topping-create` | Từ chối type mismatch | `PASS` |
| WRITE-CREATE-14 | Xác nhận bằng Admin JWT khác | Từ chối ownership | `PASS` |
| WRITE-CREATE-15 | Customer/Manager/Cashier/Employee/Kitchen gọi endpoint | 401/403/denial, không chạy tool | `PASS` |

Evidence tối thiểu cho `WRITE-CREATE-04` và `WRITE-CREATE-08`: preview, request/response confirmation, kết quả catalog API, màn hình sau refetch và kiểm tra submit lại token không tạo thêm row.

### Write UAT: đổi tên món

| ID | Input/action | Expected result | Status |
|---|---|---|---|
| WRITE-RENAME-01 | `Mình muốn đổi tên món` | Hỏi tên hiện tại và tên mới; không mutation | `PASS` |
| WRITE-RENAME-02 | `Đổi món Bạc xỉu TEST thành Bạc xỉu` | `UPDATE_PRODUCT_NAME`/`update_product_name`, resolve được tên có dấu/không dấu, hiển thị preview | `PASS` |
| WRITE-RENAME-03 | Bấm `Hủy` preview | Tên cũ giữ nguyên | `PASS` |
| WRITE-RENAME-04 | Xác nhận preview | Gọi `/api/Ai/product-name-confirmation`, đổi một lần, Admin/POS refetch | `PASS` |
| WRITE-RENAME-05 | Token hết hạn hoặc đã dùng | Từ chối, không đổi thêm | `PASS` |
| WRITE-RENAME-06 | Đổi thành tên active đã tồn tại, có/không dấu | Từ chối duplicate | `PASS` |
| WRITE-RENAME-07 | Đổi tên trực tiếp giữa preview và confirmation | Từ chối vì current name không còn khớp | `PASS` |
| WRITE-RENAME-08 | `Đổi món Bạc xỉu TEST thành Bạc xỉu.` | Giữ dấu, bỏ dấu chấm cuối khỏi tên lưu | `PASS` |
| WRITE-RENAME-09 | `Đổi món Bò bít tết TEST thành Bò bít tết` | Giữ nguyên `Bò bít tết`, không biến thành không dấu | `PASS` |

### Flag, model và quyền truy cập

1. Tắt `AI:AdminEmbeddingPilot:Enabled`: báo Admin AI disabled, không fallback Mock — `PASS`.
2. Model/tokenizer/catalog sai hoặc thiếu: fail-closed, không chạy tool — `PASS`.
3. Customer, Guest, Manager, Cashier, Employee hoặc Kitchen gọi Admin endpoint: 401/403/denial, không vào Admin pilot — `PASS`.
4. Khi ghi nhận kết quả phải kèm HTTP status, response, tool thực tế, trạng thái token và evidence catalog/DB; không đánh dấu `PASS` nếu chưa kiểm tra thực tế.
