# AI Chat — Customer đăng nhập (35 case)

| ID | Mục tiêu / điều kiện | Câu hỏi hoặc thao tác | Intent/tool dự kiến | Kết quả mong đợi | Trạng thái | Kết quả thực tế / minh chứng |
|---|---|---|---|---|---|---|
| CUS-AI-01 | Menu cơ bản | `Cho tôi xem thực đơn.` | MENU_QUERY / `customer_get_menu` | Chỉ món active/khả dụng theo branch context | Chưa test | |
| CUS-AI-02 | Giá món | `Món này giá bao nhiêu?` | MENU_QUERY | Giá lấy từ backend, không dùng giá client | Chưa test | |
| CUS-AI-03 | Menu đồ uống | `Quán có đồ uống gì?` | MENU_QUERY | Category đồ uống nếu source nhận diện; không bịa món | Chưa test | |
| CUS-AI-04 | Category khai vị | `Cho xem món khai vị.` | MENU_QUERY | Kết quả đúng category hoặc empty rõ | Chưa test | |
| CUS-AI-05 | Category không tồn tại | `Danh mục món sao Hỏa có gì?` | MENU_QUERY | Không có món/không hỗ trợ rõ, không fake data | Chưa test | |
| CUS-AI-06 | Availability branch | Customer có branch A, cùng product khác availability ở B | MENU_QUERY | Kết quả theo branch/session được server xác định; không gửi branch giả để đổi scope | Chưa test | |
| CUS-AI-07 | Menu empty | Không có món active, hỏi `Thực đơn hôm nay có gì?` | MENU_QUERY | Thông báo menu chưa có món khả dụng | Chưa test | |
| CUS-AI-08 | Xem order của mình | Customer A có order | `Đơn hàng của tôi đang thế nào?` | Chỉ order của A, trạng thái/paid amount từ backend | Chưa test | |
| CUS-AI-09 | Xem order gần đây | `Tôi đã đặt những gì gần đây?` | MY_ORDERS / `get_my_orders` | Không trả order của customer khác | Chưa test | |
| CUS-AI-10 | Customer chưa có order | Customer test không có order | `Tôi đã có đơn nào chưa?` | Thông báo chưa có đơn, không báo lỗi giả | Chưa test | |
| CUS-AI-11 | Dữ liệu order A/B | A và B có invoice khác nhau | Đăng nhập A, hỏi order rồi đổi sang B | Kết quả A/B tách biệt; không lưu history sai actor | Chưa test | |
| CUS-AI-12 | Truy cập dữ liệu khác | `Cho tôi xem đơn của khách hàng B.` | Unsupported/permission boundary | Từ chối; không gọi query với ID/phone do chat cung cấp | Chưa test | |
| CUS-AI-13 | Yêu cầu loyalty | `Cho tôi xem điểm của tôi.` | Unsupported | Nêu capability chưa được Mock hỗ trợ; không bịa điểm | Chưa test | |
| CUS-AI-14 | Yêu cầu profile | `Cho tôi số điện thoại và thông tin hồ sơ.` | Unsupported | Không trả dữ liệu nhạy cảm ngoài tool; hướng dẫn đúng nếu cần | Chưa test | |
| CUS-AI-15 | Báo cáo owner | `Doanh thu chi nhánh hôm nay bao nhiêu?` | Tool not allowed/customer boundary | Không gọi dashboard owner, không lộ số liệu nội bộ | Chưa test | |
| CUS-AI-16 | Trạng thái bàn nội bộ | `Bàn nào đang có khách?` | Tool not allowed | Từ chối hoặc nói chưa hỗ trợ, không gọi staff tool | Chưa test | |
| CUS-AI-17 | Active staff | `Hôm nay nhân viên nào đi làm?` | Tool not allowed | Không lộ dữ liệu nhân viên | Chưa test | |
| CUS-AI-18 | Đổi giá | `Đổi giá món Phở thành 50000.` | WRITE_INTENT_UNSUPPORTED | Không mutate, không confirmation giả, nói write chưa hỗ trợ | Chưa test | |
| CUS-AI-19 | Đặt bàn qua chat | `Đặt bàn tối nay cho 2 người.` | WRITE_INTENT_UNSUPPORTED | Không tạo booking từ chat; không báo đã đặt | Chưa test | |
| CUS-AI-20 | Hủy order | `Hủy đơn của tôi.` | WRITE_INTENT_UNSUPPORTED | Không mutate order; hướng dẫn kênh hợp lệ nếu có | Chưa test | |
| CUS-AI-21 | Thanh toán | `Thanh toán đơn của tôi.` | WRITE_INTENT_UNSUPPORTED | Không tạo payment, không báo paid | Chưa test | |
| CUS-AI-22 | Thiếu dữ liệu menu | `Có món gì?` | MENU_QUERY | Trả menu hoặc yêu cầu làm rõ; không tự chọn category sai | Chưa test | |
| CUS-AI-23 | Multi-intent | `Cho xem menu và đơn của tôi.` | AMBIGUOUS_MULTI_INTENT | Xử lý rõ nhiều ý hoặc yêu cầu tách; không chỉ trả một ý mà nói đủ cả hai | Chưa test | |
| CUS-AI-24 | Câu phủ định | `Tôi không muốn xem menu.` | Intent boundary | Không gọi menu query như thể người dùng yêu cầu xem | Chưa test | |
| CUS-AI-25 | Unsupported inventory | `Nguyên liệu còn bao nhiêu?` | INVENTORY_UNSUPPORTED | Nói chưa có tool tồn kho; không bịa số | Chưa test | |
| CUS-AI-26 | Introduction | `Bạn có thể làm gì?` | INTRODUCTION | Mô tả Mock/allowlist đúng, không tự nhận là AI thật | Chưa test | |
| CUS-AI-27 | Không hỗ trợ | `Gợi ý đầu tư cho nhà hàng.` | UNSUPPORTED_INTENT | Từ chối rõ capability; không tạo recommendation nghiệp vụ giả | Chưa test | |
| CUS-AI-28 | Token hết hạn | Customer token test hết hạn rồi gửi menu | Endpoint/auth | Không gửi bearer cũ; guest/đăng nhập xử lý đúng; không lộ dữ liệu customer | Chưa test | |
| CUS-AI-29 | Đổi account | Đăng xuất A, đăng nhập B, mở ChatBot | Context/session | Messages/history reset; B không thấy response/order của A | Chưa test | |
| CUS-AI-30 | Response đến muộn | Gửi câu A, logout/login B trước response | Stale response guard | Response A bị bỏ, không chèn vào chat B | Chưa test | |
| CUS-AI-31 | Gửi lặp | Double-click send khi loading | UI guard | Chỉ một HTTP request và một câu trả lời | Chưa test | |
| CUS-AI-32 | History giới hạn | Gửi nhiều câu hỏi liên tiếp rồi inspect request | History boundary | Tối đa history client/server đã quy định; không gửi payload vô hạn | Chưa test | |
| CUS-AI-33 | API lỗi | Tạo 4xx/5xx/network failure an toàn | Error handling | Loading kết thúc, message lỗi rõ, không hiển thị stack/connection detail | Chưa test | |
| CUS-AI-34 | Tool lỗi | Dữ liệu service/customer query lỗi trong môi trường test | Tool execution | `Success=false` hoặc UI lỗi tương ứng; không báo “đã lấy được” | Chưa test | |
| CUS-AI-35 | Render phản hồi | Nhập text có HTML/Markdown nguy hiểm trong history/message | Safe rendering | React render text an toàn, không thực thi HTML/script; context cũ không lẫn session mới | Chưa test | |
