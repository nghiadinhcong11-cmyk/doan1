# 08. Khách hàng và Loyalty (Thành viên)

## 1. Mục tiêu module
Quản lý thông tin khách hàng, xây dựng lòng trung thành qua hệ thống tích điểm (Loyalty Points) và các chương trình khuyến mãi.

## 2. Chức năng chính
- Lưu trữ hồ sơ khách hàng: Tên, số điện thoại, email, hạng thành viên.
- **Tích điểm tự động**: Tự động cộng điểm dựa trên giá trị đơn hàng sau khi thanh toán thành công.
- **Lịch sử giao dịch**: Theo dõi lịch sử mua hàng và biến động điểm tích lũy.
- **Hạng thành viên**: Phân cấp khách hàng (Đồng, Bạc, Vàng, Kim cương) dựa trên tổng chi tiêu.

## 3. Business Logic Loyalty
- **Tỷ lệ tích điểm**: Được cấu hình trong hệ thống (Ví dụ: 10.000 VNĐ = 1 điểm).
- **Tính nguyên tử (Atomicity)**: Giao dịch tích điểm được thực hiện đồng thời với bước hoàn tất đơn hàng trong một Transaction để tránh sai lệch.

## 4. API chính
- `GET /api/Customer/{id}`: Xem thông tin khách hàng.
- `GET /api/Customer/loyalty-history`: Xem lịch sử tích điểm.

## 5. Dữ liệu (Entities)
- **Customer**: Thông tin khách hàng, `TotalSpent`, `LoyaltyPoints`.
- **LoyaltyTransaction**: Ghi lại từng lần cộng/trừ điểm.

## 6. Bảo mật & IDOR
- **Customer IDOR Hardening**: Khách hàng chỉ được phép truy cập vào dữ liệu và lịch sử đơn hàng của chính mình (kiểm tra `NameIdentifier` từ JWT).
- **Dữ liệu nhạy cảm**: Mật khẩu khách hàng được băm (hashing) bảo mật.
