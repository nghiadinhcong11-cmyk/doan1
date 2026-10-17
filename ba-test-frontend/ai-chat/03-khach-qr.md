# AI Chat — Khách QR/guest (15 case)

| ID | Mục tiêu / điều kiện | Câu hỏi hoặc thao tác | Intent/tool dự kiến | Kết quả mong đợi | Trạng thái | Kết quả thực tế / minh chứng |
|---|---|---|---|---|---|---|
| QR-AI-01 | Guest menu | Guest QR hợp lệ, hỏi `Hôm nay có món gì?` | MENU_QUERY / `customer_get_menu` | Guest nhận menu công khai theo session/branch; không cần customer login | Chưa test | |
| QR-AI-02 | Guest giá món | `Món này giá bao nhiêu?` | MENU_QUERY | Giá active từ backend; không lộ dữ liệu nội bộ | Chưa test | |
| QR-AI-03 | Guest category | `Quán có đồ uống gì?` | MENU_QUERY | Chỉ trả menu/category public được phép | Chưa test | |
| QR-AI-04 | Guest category empty | `Có món thuộc danh mục không tồn tại không?` | MENU_QUERY | Empty/không có món rõ, không fake item | Chưa test | |
| QR-AI-05 | Branch scope | Session QR Branch A, thử nội dung/branch query của B | Guest scope | Server giữ branch/session context; không dùng branch do chat tự nêu để đổi scope | Chưa test | |
| QR-AI-06 | Cart preservation | Có cart QR chưa gửi, reset/đóng/mở ChatBot | UI/session | Cart, QR token và guest session vẫn nguyên; chỉ history chat reset nếu context đổi | Chưa test | |
| QR-AI-07 | Guest personal order | `Đơn hàng của tôi đâu?` | `IsPersonalOrderRequest` / login required | Trả yêu cầu đăng nhập; không gọi `get_my_orders` | Chưa test | |
| QR-AI-08 | Guest loyalty | `Tôi có bao nhiêu điểm?` | Guest boundary | Yêu cầu login/không hỗ trợ; không gọi customer tool ngoài menu | Chưa test | |
| QR-AI-09 | Guest profile | `Cho tôi xem hồ sơ của tôi.` | Guest boundary | Không gọi profile/customer API; thông báo cần đăng nhập nếu phù hợp | Chưa test | |
| QR-AI-10 | Guest booking | `Đặt bàn cho tôi tối nay.` | WRITE_INTENT_UNSUPPORTED/guest boundary | Không gọi write tool hoặc tạo reservation; nêu giới hạn rõ | Chưa test | |
| QR-AI-11 | Guest admin data | `Doanh thu chi nhánh hôm nay bao nhiêu?` | Guest allowlist | Không gọi dashboard/revenue; không lộ dữ liệu owner | Chưa test | |
| QR-AI-12 | Guest staff data | `Bàn nào đang có khách và nhân viên nào đi làm?` | Guest allowlist | Không gọi staff/table/staff tools; trả từ chối/unsupported an toàn | Chưa test | |
| QR-AI-13 | Guest mutation | `Đổi giá món này` hoặc `xóa món` | Guest read-only | Không mutation, không confirmation giả, không báo thành công | Chưa test | |
| QR-AI-14 | Guest loading/error | Ngắt mạng khi gửi menu rồi retry thủ công | UI error | Loading kết thúc, lỗi an toàn, retry không tạo request trùng; guest token/cart không bị xóa | Chưa test | |
| QR-AI-15 | Session transition/stale response | Gửi câu ở session A, chuyển sang QR/session B trước response | Context/stale response | History/response A không xuất hiện trong B; cart/token của B không bị xóa hoặc ghi đè | Chưa test | |
