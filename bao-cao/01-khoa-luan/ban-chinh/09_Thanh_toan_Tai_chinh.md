# 09. Thanh toán và Tài chính

## 1. Mục tiêu module
Đảm bảo tính chính xác tuyệt đối trong việc tính toán hóa đơn, xử lý thanh toán và ghi nhận doanh thu hệ thống.

## 2. Các thành phần tài chính trong đơn hàng
Mỗi đơn hàng lưu trữ một "snapshot" tài chính để phục vụ báo cáo lịch sử:
- `SubTotal`: Tổng tiền món ăn gốc.
- `ServiceFeeAmount`: Tiền phí dịch vụ (dựa trên % cấu hình).
- `VatAmount`: Tiền thuế giá trị gia tăng (dựa trên % cấu hình).
- `DiscountAmount`: Số tiền được giảm giá.
- `TotalAmount`: Số tiền khách phải trả cuối cùng.

## 3. Quy trình thanh toán
1. Thu ngân chốt đơn hàng.
2. Hệ thống tính toán lại toàn bộ số tiền (Server-side calculation).
3. Xử lý phương thức thanh toán (Tiền mặt, Chuyển khoản, Ví điện tử).
4. Ghi nhận doanh thu và in hóa đơn (Invoice).

## 4. Business Logic quan trọng
- **Financial Integrity**: Hệ thống không tin tưởng vào tổng số tiền gửi lên từ phía Client (Frontend). Mọi phép tính đều dựa trên giá sản phẩm trong database tại thời điểm thanh toán.
- **VAT Calculation**: Thuế VAT được tính dựa trên `(Subtotal + ServiceFee)`.

## 5. Báo cáo doanh thu và Lợi nhuận (Dashboard)
- Hỗ trợ xem doanh thu theo ngày, tháng, năm.
- Phân tích doanh thu theo chi nhánh.
- **Tính toán lợi nhuận**:
  `Net Profit = Doanh thu - Tổng chi phí (Expenses) - Giá vốn hàng bán ước tính (Estimated COGS)`.
- **COGS**: Hiện tại dựa trên trường `CostPrice` được cấu hình trong sản phẩm thay vì định lượng chi tiết.
- AI Assistant hỗ trợ báo cáo nhanh qua Tool `get_revenue`.

## 6. Dữ liệu (Entities)
- **Order**: Chứa các trường tài chính snapshot.
- **Expense**: Ghi nhận các chi phí vận hành khác của nhà hàng.
- **Invoice**: Dữ liệu hóa đơn đã xuất.

## 7. Bảo mật tài chính
- Quyền hủy đơn hàng hoặc sửa đổi giá món đã chốt chỉ dành cho role `admin`.
- Mọi giao dịch tài chính đều được log lại để đối soát.
