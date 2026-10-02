# 14. AI Assistant - Trợ lý ảo thông minh

## 1. Kiến trúc AI hiện tại
Hệ thống sử dụng mô hình **AI Orchestration** tích hợp trực tiếp với Google Gemini API (Model: gemini-3.7-flash).
- **Luồng xử lý**: `AiController` -> `AiOrchestrator` -> `GeminiService` -> `AiToolRegistry` -> `Application Service`.
- **Sequential Tool Calling**: Hệ thống hỗ trợ gọi nhiều Tool liên tiếp trong một lượt hỏi (ví dụ: vừa xem doanh thu vừa phân tích món bán chạy), giúp AI giải quyết các yêu cầu phức tạp.
- **Stateless Backend**: Hệ thống không lưu lịch sử chat tại server để đảm bảo riêng tư. Lịch sử được frontend quản lý và gửi kèm mỗi request.

## 2. Phân loại cấp độ AI
Hiện tại hệ thống đạt **Level 3 — Tool-Using Assistant with Financial Intelligence**.
AI không chỉ trả lời văn bản, phân tích số liệu mà còn có khả năng "suy nghĩ" để đưa ra các nhận định (Inference), khuyến nghị (Recommendation) và phát hiện các bất thường về tài chính (Anomaly Detection) dựa trên dữ liệu thực tế.

## 3. Danh sách các công cụ AI (AI Tools) đã triển khai
Hệ thống hiện có **14 công cụ** AI:

| Tool Name | Chức năng | Đối tượng sử dụng |
| :--- | :--- | :--- |
| `get_financial_analysis`| **(Mới)** Phân tích tài chính chuyên sâu (Doanh thu, Chi phí, Lợi nhuận, Tăng trưởng, Bất thường). | Admin, Manager |
| `get_business_summary`| Tóm tắt KPI (Doanh thu, Lợi nhuận, AOV) & Advisor. | Admin, Manager |
| `get_revenue` | Xem doanh thu tổng hợp (ngày/tháng/năm). | Admin, Manager |
| `get_order_list` | Truy vấn danh sách đơn hàng chi tiết. | Admin, Manager, Employee |
| `get_best_sellers` | Thống kê các món bán chạy nhất. | Admin, Manager, Employee |
| `get_revenue_comparison`| So sánh doanh thu giữa các kỳ. | Admin, Manager |
| `get_active_staff` | Xem danh sách nhân viên đang làm. | Admin, Manager |
| `update_product_price`| Cập nhật giá bán sản phẩm. | Admin |
| `customer_get_menu` | Tra cứu thực đơn, giá, topping. | Công khai |
| `get_my_orders` | Xem đơn hàng cá nhân. | Customer |
| `create_booking` | Đặt bàn qua ngôn ngữ tự nhiên. | Customer |
| `get_my_shift` | Xem lịch làm việc cá nhân. | Employee |
| `employee_get_table_summary`| Xem thống kê bàn tại chi nhánh. | Employee, Manager |
| `update_order_status` | Cập nhật trạng thái đơn hàng. | Employee, Manager |

## 4. Bảo mật và Phân quyền AI
- **Bảo mật Role-based**: AI xác thực quyền qua JWT Token. Mỗi Tool có `AllowedRoles` và `RiskLevel` (Read/Write/Destructive).
- **Branch Isolation**: Toàn bộ dữ liệu Analytics và Advisor được lọc nghiêm ngặt theo `BranchId` của người dùng. Admin được quyền xem toàn chuỗi.
- **Hallucination Protection**: Prompt hệ thống yêu cầu AI phân biệt rõ FACT (Sự thật), INFERENCE (Suy luận) và RECOMMENDATION (Khuyến nghị).

## 5. Khả năng Tài chính thông minh (Financial Intelligence)
AI hiện có thể thực hiện các nhiệm vụ chuyên sâu:
- **KPI Snapshot & Profitability**: Cung cấp cái nhìn tổng thể về doanh thu, số đơn, AOV và lợi nhuận ước tính (Estimated Profit), biên lợi nhuận (Profit Margin).
- **Phân tích Cơ cấu Chi phí (Expense Breakdown)**: Bóc tách chi phí theo danh mục (Nguyên liệu, Điện nước, Vận hành...) để tối ưu hóa dòng tiền.
- **Phân tích Tăng trưởng (Growth Analysis)**: So sánh hiệu quả kinh doanh giữa các kỳ (Kỳ này vs Kỳ trước) để đánh giá xu hướng phát triển.
- **Phát hiện Bất thường (Anomaly Detection)**: Tự động cảnh báo các biến động đáng kể (Ví dụ: chi phí một nhóm tăng đột biến > 50% hoặc doanh thu giảm mạnh > 20%).
- **Phân tích Insight**: Tự động nhận diện xu hướng (ví dụ: "Doanh thu tăng nhờ khách chi tiêu nhiều hơn dù lượng đơn giảm").
- **Khuyến nghị hành động (Advisory)**: Gợi ý xây dựng combo, upsell hoặc điều chỉnh chi phí dựa trên bằng chứng dữ liệu (FACT -> INFERENCE -> RECOMMENDATION).
- **Lý giải nguyên nhân**: Trả lời câu hỏi "Tại sao doanh thu tăng nhưng lợi nhuận giảm?" bằng cách kiểm tra đồng thời nhiều chỉ số tài chính.

> **Lưu ý quan trọng về số liệu tài chính**:
> Estimated Profit là chỉ số tham khảo. Do hệ thống chưa có historical cost snapshot và inventory accounting, việc ghi nhận chi phí nguyên liệu trong Expense đồng thời với việc ước tính COGS từ CostPrice có thể dẫn đến double-counting trong một số trường hợp. Vì vậy chỉ số này không được xem là lợi nhuận kế toán chính thức.

## 6. Khả năng Chủ động (Proactive Insights)
Hệ thống tích hợp khả năng AI chủ động phát hiện các vấn đề kinh doanh:
- **Cơ chế**: `BackgroundService` chạy hàng ngày lúc 00:05 (giờ VN) để phân tích dữ liệu.
- **Phát hiện**: Rule Engine deterministic đảm bảo tính chính xác 100% về số liệu.
- **Diễn giải**: Gemini viết lời giải thích (`AiExplanation`) và khuyến nghị (`AiRecommendation`).
- **Giao diện**: Trang "Cảnh báo kinh doanh" cho phép Admin/Manager quản lý và theo dõi các nhận định từ AI.
- **Thông báo**: Tích hợp hệ thống Notification và SignalR để cảnh báo Real-time ngay khi phát hiện bất thường.

## 7. Lộ trình phát triển AI (Roadmap)
- **Phase 1 (Analytics)**: Đã hoàn thành với Sequential Tool Calling.
- **Phase 2 (BI)**: Đã hoàn thành Financial Intelligence & Proactive Insights (API + UI + Real-time).
- **Phase 3 (Agent)**: AI có thể tự động thực hiện các hành động phức tạp theo kế hoạch (Task 24).
