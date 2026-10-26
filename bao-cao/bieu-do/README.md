# Bộ mã UML phục vụ báo cáo

Các file trong thư mục này là mã PlantUML dùng để dựng lại biểu đồ trên draw.io/diagrams.net.

## Danh sách biểu đồ

| File | Hình | Nội dung |
|---|---:|---|
| 01-usecase-overview.puml | 3.1 | Use Case tổng quát, nhóm actor và kế thừa |
| 02-usecase-customer-guest.puml | 3.2 | Use Case khách hàng đăng nhập và khách QR chưa đăng nhập |
| 03-usecase-operations.puml | 3.3 | Use Case nhóm nhân sự vận hành |
| 04-usecase-admin-ai.puml | 3.4 | Use Case nhóm quản trị và trợ lý ảo |
| 05-activity-qr-order.puml | 3.5 | Activity đặt món bằng QR, dạng swimlane |
| 06-activity-pos-kds.puml | 3.6 | Activity tiếp nhận đơn trực tuyến, dạng swimlane |
| 11-activity-payment-loyalty.puml | 3.7 | Activity thanh toán và Loyalty, dạng swimlane |
| 12-activity-kds-request.puml | 3.8 | Activity gửi món và xử lý yêu cầu KDS, dạng swimlane |
| 13-activity-ai-request.puml | 3.9 | Activity trợ lý ảo, gồm phân tích intent |
| 07-sequence-payment.puml | 3.10 | Sequence thanh toán đơn hàng |
| 14-sequence-pos-backend-kds.puml | 3.11 | Sequence POS, Backend và KDS |
| 08-sequence-ai.puml | 3.12 | Sequence trợ lý ảo, gồm intent và tham số |
| 09-class-domain.puml | 3.13 | Class diagram miền nghiệp vụ |
| 15-erd.puml | 3.14 | ERD các bảng nghiệp vụ chính |
| 17-system-architecture.puml | 3.15 | Kiến trúc tổng thể của hệ thống |
| 18-sequence-realtime-kds.puml | 3.16 | Biểu đồ khối giao tiếp thời gian thực Backend–KDS |
| 16-ai-architecture.puml | 3.17 | Kiến trúc trợ lý ảo thông minh |

Hình 3.15 và 3.16 chưa có tên trong danh sách yêu cầu hiện tại nên chưa tự gán nội dung.

## Quy ước kỹ thuật

- Actor generalization chỉ biểu diễn hành vi dùng chung, không thay thế role authorization.
- Activity diagram dùng swimlane theo trách nhiệm: actor, frontend, controller, application service, KDS hoặc AI.
- OrderService là nguồn tính giá, VAT, phí dịch vụ và tổng tiền; frontend không phải nguồn authoritative.
- Thanh toán dùng `Order.TotalAmount` đã lưu, không dùng tổng tiền do client gửi để xác thực.
- AI đi theo luồng `AiOrchestrator → phân tích intent → AiToolRegistry → AiPermissionService → AI Tool → Application Service`.
- Embedding pilot hiện tại là local và chịu feature flag; không mô tả như một provider AI bên ngoài.
- Khách QR chỉ dùng verified session và branch/table context; direct-session là cơ chế backend, không phải actor riêng.

## Cách sử dụng

1. Mở draw.io/diagrams.net.
2. Chọn Arrange → Insert → Advanced → PlantUML.
3. Sao chép nội dung file `.puml` vào hộp thoại.
4. Chỉnh bố cục khi đưa vào báo cáo nhưng không thay đổi tên actor, quan hệ và điều kiện nghiệp vụ.
