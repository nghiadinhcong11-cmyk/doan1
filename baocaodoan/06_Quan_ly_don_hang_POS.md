# 06. Quản lý đơn hàng (POS Order)

## 1. Mục tiêu module
Xử lý toàn bộ quy trình từ lúc khách gọi món, nhân viên tạo đơn, chuyển yêu cầu xuống bếp đến khi hoàn tất đơn hàng. Hệ thống hỗ trợ gọi món nhiều đợt trên cùng một đơn hàng.

## 2. Quy trình nghiệp vụ (Workflow)
1. **Tạo đơn**: Thu ngân/Nhân viên hoặc Khách hàng (qua QR) tạo đơn hàng mới gắn với bàn.
2. **Gửi bếp theo đợt (Kitchen Request System)**:
    - Cho phép gọi thêm món nhiều lần.
    - Hệ thống tự động so sánh số lượng đã gọi và số lượng đã gửi bếp (`SentQuantity`).
    - Chỉ những món mới hoặc phần số lượng tăng thêm mới được gửi xuống bếp qua SignalR.
3. **Chế biến**: Bếp tiếp nhận danh sách món mới theo thời gian thực (Real-time).
4. **Phục vụ**: Nhân viên phục vụ món tại bàn khi bếp báo hoàn thành.
5. **Thanh toán**: Thu ngân chốt đơn, tính toán tài chính và in hóa đơn.

## 3. Business Logic tài chính (Nguồn: OrderService)
- **Authoritative Calculation**: Mọi tính toán tiền bạc đều thực hiện tại Backend.
- **Thứ tự tính toán**:
    1. Tổng tiền món (Subtotal).
    2. Phí dịch vụ (Service Fee) = Subtotal * % ServiceFee.
    3. Thuế VAT = (Subtotal + Service Fee) * % VAT.
    4. Giảm giá (Discount).
    5. Tổng thanh toán (Total Amount).
- **Rounding**: Làm tròn tiền đến đơn vị đồng (0 decimals) cho tiền VNĐ (`MidpointRounding.AwayFromZero`).

## 4. API chính
- `POST /api/Order`: Tạo đơn hàng mới.
- `POST /api/Order/{id}/send-to-kitchen`: Gửi yêu cầu chế biến các món mới cho đơn hàng hiện tại.
- `GET /api/Order/{id}`: Xem chi tiết đơn hàng (có snapshots tài chính).
- `PUT /api/Order/{id}/status`: Cập nhật trạng thái đơn hàng.

## 5. Dữ liệu (Entities)
- **Order**: `Id`, `TableId`, `BranchId`, `Status`, `SubTotal`, `VatAmount`, `TotalAmount`, `PaidAmount`.
- **OrderDetail**: Chứa thông tin từng món, giá tại thời điểm gọi, `Quantity` và `SentQuantity`.
- **OrderRequest**: Lưu vết các đợt gửi món xuống bếp.

## 6. Bảo mật & Tính toàn vẹn
- **Concurrency Control**: Sử dụng cơ chế `Execution Strategy` và kiểm tra phiên bản dữ liệu để tránh xung đột khi nhiều người cùng cập nhật một đơn hàng.
- **Table Isolation**: Khách hàng chỉ có thể xem và gọi món cho đúng bàn của mình.
- **Authoritative Prices**: Giá sản phẩm được lấy từ database, không tin tưởng giá từ frontend gửi lên.

