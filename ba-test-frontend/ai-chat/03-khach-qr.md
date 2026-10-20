# AI Chat — Guest QR đã xác minh (E5)

Endpoint: `POST /api/Ai/guest-chat`. Guest phải có QR session/JWT do server cấp, đúng Branch/Table context và môi trường Development; flag `AI:CustomerGuestEmbeddingPilot:Enabled=true`.

| ID | Câu hỏi/thao tác | Kỳ vọng |
|---|---|---|
| QR-E5-01 | Hôm nay có món gì? | `MENU_QUERY` → `customer_get_menu`, menu công khai đúng branch |
| QR-E5-02 | Món này giá bao nhiêu? | Giá từ backend, không lộ dữ liệu nội bộ |
| QR-E5-03 | Quán có đồ uống gì? | Category thực tế hoặc empty rõ |
| QR-E5-04 | Danh mục không tồn tại? | Không có món/không bịa dữ liệu |
| QR-E5-05 | Nội dung chat nhắc Branch B. | Vẫn giữ Branch A từ guest session |
| QR-E5-06 | Có cart, đóng/mở ChatBot. | Cart, QR token và session không mất |
| QR-E5-07 | Đơn hàng của tôi đâu? | `REJECT`/yêu cầu đăng nhập; không gọi `get_my_orders` |
| QR-E5-08 | Doanh thu, bàn, nhân viên. | `REJECT`/`UNSUPPORTED`; không gọi admin/staff tool |
| QR-E5-09 | Đổi giá, xóa món, đặt bàn, thanh toán. | Từ chối; không write/confirmation/payment |
| QR-E5-10 | QR token thiếu, sai, hết hạn hoặc đổi branch. | 401/403; không fallback menu toàn hệ thống |
| QR-E5-11 | Flag OFF hoặc không chạy Development. | 404 `Guest AI is not available`; không fallback Mock |
| QR-E5-12 | Model/catalog/service lỗi. | Error fail-closed; không thực thi tool |
| QR-E5-13 | Gửi message rỗng/quá dài/history > 6. | 400; không gọi tool |
| QR-E5-14 | Mất mạng rồi retry. | Loading kết thúc, retry không nhân request; session/cart giữ nguyên |
| QR-E5-15 | Chuyển từ QR session A sang B khi request đang chờ. | Không đưa response A vào B; không ghi đè cart/token B |

### Chuẩn bị và evidence

Trong Network kiểm tra `POST /api/guest/bootstrap` (nếu flow sử dụng) và `POST /api/Ai/guest-chat`; che Bearer token và `X-Guest-Session` trong evidence. Ghi branch/table context do server cấp, HTTP status và response. Không tự kết luận PASS nếu chưa xác minh session thật.
