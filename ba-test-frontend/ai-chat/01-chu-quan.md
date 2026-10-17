# AI Chat — Chủ quán (54 case)

| ID | Mục tiêu / điều kiện | Câu hỏi hoặc thao tác | Intent/tool dự kiến | Kết quả mong đợi | Trạng thái | Kết quả thực tế / minh chứng |
|---|---|---|---|---|---|---|
| OWN-AI-01 | Có doanh thu hôm nay | `Doanh thu hôm nay là bao nhiêu?` | REVENUE_QUERY / `get_revenue` | Chỉ dữ liệu backend của phạm vi owner; không bịa số | Chưa test | |
| OWN-AI-02 | Có doanh thu hôm qua | `Doanh thu hôm qua là bao nhiêu?` | REVENUE_QUERY / `get_business_summary` | Trả đúng kỳ hôm qua hoặc lỗi rõ; không báo thành công giả | Chưa test | |
| OWN-AI-03 | Hỏi doanh thu tháng | `Cho tôi doanh thu tháng này.` | REVENUE_QUERY / `get_revenue` | Kỳ tháng được nhận diện; định dạng tiền rõ | Chưa test | |
| OWN-AI-04 | Hỏi doanh thu năm | `Doanh thu năm nay hiện tại bao nhiêu?` | REVENUE_QUERY / `get_revenue` | Kỳ năm được nhận diện; không suy đoán ngoài dữ liệu | Chưa test | |
| OWN-AI-05 | Thiếu kỳ doanh thu | `Doanh thu hiện tại thế nào?` | Clarification | Hỏi lại kỳ, không tự chọn hôm nay/tháng | Chưa test | |
| OWN-AI-06 | So sánh ngày | `So sánh doanh thu hôm nay với hôm qua.` | REVENUE_COMPARISON / `get_revenue_comparison` | Đúng hai kỳ, hướng tăng/giảm theo dữ liệu | Chưa test | |
| OWN-AI-07 | So sánh tuần | `Doanh thu tuần này so với tuần trước ra sao?` | REVENUE_COMPARISON | Chỉ nhận cặp kỳ được hỗ trợ | Chưa test | |
| OWN-AI-08 | So sánh tháng | `Đối chiếu doanh thu tháng này và tháng trước.` | REVENUE_COMPARISON | Hiển thị cả hai kỳ, không nhập thêm kỳ khác | Chưa test | |
| OWN-AI-09 | Thiếu kỳ so sánh | `So sánh doanh thu.` | Clarification | Yêu cầu nêu rõ hai khoảng thời gian | Chưa test | |
| OWN-AI-10 | So sánh unsupported | `So sánh doanh thu quý này với quý trước.` | Unsupported/clarification | Không giả lập tool quý; nói capability chưa hỗ trợ | Chưa test | |
| OWN-AI-11 | Tổng quan hôm nay | `Tóm tắt kinh doanh hôm nay.` | BUSINESS_SUMMARY / `get_business_summary` | Trả KPI từ service, không tạo KPI giả | Chưa test | |
| OWN-AI-12 | Tổng quan tháng | `Cho tôi báo cáo tổng hợp tháng này.` | BUSINESS_SUMMARY | Yêu cầu kỳ hợp lệ, trả dữ liệu hoặc lỗi an toàn | Chưa test | |
| OWN-AI-13 | Thiếu kỳ tổng quan | `Tóm tắt tình hình kinh doanh.` | Clarification | Hỏi lại khoảng thời gian | Chưa test | |
| OWN-AI-14 | Đếm đơn hôm nay | `Hôm nay có bao nhiêu đơn?` | ORDER_COUNT / `get_business_summary` | Trả số đơn từ backend; không đánh đồng với doanh thu | Chưa test | |
| OWN-AI-15 | Đếm đơn thiếu kỳ | `Tổng số đơn là bao nhiêu?` | Clarification | Không tự chọn kỳ | Chưa test | |
| OWN-AI-16 | Món bán chạy hôm nay | `Món nào bán chạy hôm nay?` | PRODUCT_RANKING / `get_best_sellers` | Tối đa limit source, dữ liệu thật hoặc empty rõ | Chưa test | |
| OWN-AI-17 | Món bán chạy tuần | `Top món bán chạy tuần này.` | PRODUCT_RANKING | Kỳ week được chuyển đúng | Chưa test | |
| OWN-AI-18 | Món bán chạy tháng | `Xếp hạng món bán chạy tháng này.` | PRODUCT_RANKING | Kết quả không lộ dữ liệu ngoài scope | Chưa test | |
| OWN-AI-19 | Ranking thiếu kỳ | `Món nào bán chạy nhất?` | Clarification | Hỏi lại kỳ được hỗ trợ | Chưa test | |
| OWN-AI-20 | Ranking empty | Không có order trong kỳ, hỏi `Món bán chạy hôm nay?` | PRODUCT_RANKING | Hiển thị không có dữ liệu; không báo query thành công với item giả | Chưa test | |
| OWN-AI-21 | Danh sách đơn pending | `Cho xem các đơn đang chờ xử lý hôm nay.` | ORDER_STATUS / `get_order_list` | Lọc status/date, response role-scoped | Chưa test | |
| OWN-AI-22 | Danh sách hoàn thành | `Liệt kê đơn đã hoàn thành hôm qua.` | ORDER_STATUS | Chỉ dữ liệu đúng ngày/status | Chưa test | |
| OWN-AI-23 | Danh sách bị hủy | `Các đơn đã hủy hôm nay là những đơn nào?` | ORDER_STATUS | Không lộ trường không cần thiết | Chưa test | |
| OWN-AI-24 | Order status thiếu status | `Danh sách đơn hôm nay.` | ORDER_STATUS | Có thể lấy danh sách ngày hôm nay; không tự thêm status | Chưa test | |
| OWN-AI-25 | Order status không có dữ liệu | Kỳ/status không có đơn | ORDER_STATUS | Thông báo không tìm thấy; `Success` đúng trạng thái | Chưa test | |
| OWN-AI-26 | Trạng thái bàn | `Bàn nào đang có khách và bàn nào còn trống?` | TABLE_STATUS / `employee_get_table_summary` | Trả thống kê branch/global theo quyền owner | Chưa test | |
| OWN-AI-27 | Menu toàn bộ | `Cho tôi xem thực đơn và giá món.` | MENU_QUERY / `customer_get_menu` | Chỉ món khả dụng theo context; giá từ backend | Chưa test | |
| OWN-AI-28 | Menu theo category | `Danh mục đồ uống có món gì?` | MENU_QUERY | Category được truyền đúng nếu source nhận diện | Chưa test | |
| OWN-AI-29 | Category không tồn tại | `Danh mục món sao Hỏa có gì?` | MENU_QUERY | Empty/không có món rõ; không bịa danh mục | Chưa test | |
| OWN-AI-30 | Nhân viên đang làm | `Hôm nay nhân viên nào đang đi làm?` | Registered `get_active_staff`; Mock không có intent | Không giả định tool đã chạy; ghi capability chưa gọi được nếu source giữ nguyên | Chưa test | |
| OWN-AI-31 | Câu hỏi nhiều ý | `Xem doanh thu hôm nay và món bán chạy tháng này.` | AMBIGUOUS_MULTI_INTENT | Hỏi tách/giải thích nhiều ý; không tự chạy một ý duy nhất | Chưa test | |
| OWN-AI-32 | Câu phủ định | `Tôi không muốn xem doanh thu hôm nay.` | Intent boundary | Không coi phủ định là yêu cầu tra cứu; không gọi tool | Chưa test | |
| OWN-AI-33 | Câu rỗng/space | Nhập chỉ khoảng trắng | N/A | Không gửi request, không thêm message rỗng | Chưa test | |
| OWN-AI-34 | Unsupported inventory | `Kiểm tra tồn kho nguyên liệu.` | INVENTORY_UNSUPPORTED | Nói rõ Mock chưa có tool tồn kho; không bịa số | Chưa test | |
| OWN-AI-35 | Unsupported financial analysis | `Phân tích tài chính chuyên sâu tháng này.` | Registered tool chưa có recognizer | Không mô tả như đã chạy `get_financial_analysis` | Chưa test | |
| OWN-AI-36 | Yêu cầu đổi giá | `Đổi giá món Phở thành 50000.` | WRITE_INTENT_UNSUPPORTED | Nêu rõ write/confirmation chưa truy cập được từ chat; không mutate | Chưa test | |
| OWN-AI-37 | Yêu cầu đổi status | `Cập nhật đơn ABC sang Hoàn thành.` | WRITE_INTENT_UNSUPPORTED | Không gọi `update_order_status`, không báo thành công | Chưa test | |
| OWN-AI-38 | Yêu cầu xóa | `Xóa món này khỏi hệ thống.` | WRITE_INTENT_UNSUPPORTED | Từ chối rõ, không tự xác nhận | Chưa test | |
| OWN-AI-39 | Yêu cầu thanh toán | `Thu tiền đơn ABC giúp tôi.` | WRITE_INTENT_UNSUPPORTED | Không gọi payment/write; nêu giới hạn Mock | Chưa test | |
| OWN-AI-40 | Yêu cầu đặt bàn thay khách | `Đặt bàn lúc 19 giờ cho 4 người.` | WRITE_INTENT_UNSUPPORTED | Không gọi `create_booking`; không tạo reservation | Chưa test | |
| OWN-AI-41 | Tool argument lỗi | Gửi chuỗi khiến period/argument không hợp lệ qua intent | Tool error | API/UI phân biệt lỗi, không `Success=true`, thông báo an toàn | Chưa test | |
| OWN-AI-42 | Service failure | Ngắt/fault môi trường test theo checklist kỹ thuật | Tool execution error | Trả lỗi an toàn, chi tiết chỉ ở log đã lọc | Chưa test | |
| OWN-AI-43 | Gửi lặp nhanh | Double-click nút gửi khi request đang loading | UI guard | Chỉ một request, một user message và một response | Chưa test | |
| OWN-AI-44 | History giới hạn | Gửi liên tiếp hơn 6 message | Client/server history bound | Request chỉ gửi history bounded; không lỗi do payload phình | Chưa test | |
| OWN-AI-45 | Đổi branch | Owner chuyển branch A → B khi chat đang mở | Context/session | History cũ bị reset; request sau dùng branch B; không xóa POS/cart state | Chưa test | |
| OWN-AI-46 | Logout/login user khác | Logout owner rồi đăng nhập session khác | Context/session | Chat reset, response cũ không chèn vào chat mới | Chưa test | |
| OWN-AI-47 | Response đến muộn | Request A chậm, context đổi trước khi response về | Stale response guard | Response A bị bỏ, không xuất hiện dưới context mới | Chưa test | |
| OWN-AI-48 | Refresh trang | Refresh khi chat có message | Session behavior | Không có server history; chat khởi tạo mới, không lẫn session trước | Chưa test | |
| OWN-AI-49 | Gọi trực tiếp từ kitchen | Dùng token kitchen gọi `/api/Ai/chat` và live-status | Backend authorization | 403; không vào orchestrator/tool; KitchenHub/KDS vẫn hoạt động | Chưa test | |
| OWN-AI-50 | Gọi endpoint confirmation trực tiếp | Dùng request giả không có token confirmation hợp lệ | Admin confirmation endpoint | Không mutate; trả lỗi an toàn; không coi tool đăng ký là chat write | Chưa test | |
+
## Biến thể vai trò có AI read thực tế

Các case này bổ sung coverage nhỏ cho role đang có tool/intent đọc được. Chúng không mở rộng quyền owner và không biến tool write đã đăng ký thành intent Mock dùng được.

| ID | Mục tiêu | Điều kiện/dữ liệu chuẩn bị | Câu hỏi hoặc thao tác | Tool/intent dự kiến | Kết quả mong đợi | Trạng thái | Kết quả thực tế / minh chứng |
|---|---|---|---|---|---|---|---|
| OWN-AI-51 | Manager chỉ xem dữ liệu trong branch được cấp | Tài khoản manager Branch A; có dữ liệu Branch A và B | Chọn Branch A, hỏi 'Doanh thu hôm nay là bao nhiêu?'; sau đó thử đổi branch bằng nội dung chat | REVENUE_QUERY / get_revenue; quyền branch | Chỉ trả dữ liệu Branch A; không tin branchId do câu chat tự đưa vào và không lộ Branch B | Chưa test | |
| OWN-AI-52 | Employee dùng intent ca làm của chính mình | Tài khoản employee có ca test hiện tại và một ca khác | Hỏi 'Ca hôm nay của tôi thế nào?' | MY_SHIFT / get_my_shift | Trả ca của identity hiện tại; không cho chọn employeeId khác qua câu hỏi; role không có thì từ chối an toàn | Chưa test | |
| OWN-AI-53 | Cashier dùng tra cứu được phép nhưng không xem báo cáo quản trị | Tài khoản cashier; menu và dữ liệu đơn test | Hỏi 'Menu hôm nay có món nào?', rồi hỏi 'Cho tôi báo cáo doanh thu toàn chuỗi.' | MENU_QUERY / customer_get_menu; revenue/admin denial | Menu theo scope được trả nếu được phép; yêu cầu báo cáo quản trị bị từ chối, không gọi tool vượt quyền | Chưa test | |
| OWN-AI-54 | Employee/cashier không dùng chat để ghi dữ liệu | Tài khoản employee hoặc cashier; không dùng dữ liệu thật | Hỏi 'Đổi giá món A thành 50000' hoặc 'Cập nhật đơn A sang Hoàn thành' | WRITE_INTENT_UNSUPPORTED | Từ chối rõ là Mock chat chưa hỗ trợ write/confirmation; không mutate và không báo thành công | Chưa test | |
