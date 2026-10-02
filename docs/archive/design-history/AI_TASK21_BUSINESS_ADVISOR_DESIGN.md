# AI Task 21 – Business Advisor Design

## 1. Current AI Baseline
Dựa trên kết quả audit Task 20:
- **Kiến trúc**: Orchestration với Gemini 1.5 Flash/Pro.
- **Khả năng**: Đã hỗ trợ Sequential Tool Calling (tối đa 5 lượt lặp), cho phép gọi chuỗi các công cụ để tổng hợp thông tin.
- **Công cụ Analytics**: Đã có `get_revenue`, `get_order_list`, `get_best_sellers`, `get_revenue_comparison`.
- **Bảo mật**: Đảm bảo Branch Isolation và RBAC thông qua `AiPermissionService`.

## 2. Available Business Data
Hệ thống hiện có dữ liệu phong phú tại `DashboardService` và `OrderService`:
- **Doanh thu (Revenue)**: `PaidAmount` từ các đơn hàng có trạng thái "Hoàn thành".
- **Số lượng đơn (Order Count)**: Đếm số lượng đơn hàng hợp lệ.
- **Giá trị đơn hàng trung bình (AOV)**: Có thể tính bằng `Revenue / Order Count`.
- **Món bán chạy (Best Sellers)**: Truy vấn từ `OrderDetails`.
- **Chi phí (Expense)**: Dữ liệu từ bảng `Expenses`.
- **Lợi nhuận (Profit)**: Được tính sơ bộ bằng `Revenue - Expenses - COGS (dựa trên CostPrice)`.
- **Nhân sự**: Thống kê số lượng nhân viên đang làm việc (On-duty).

## 3. KPI Design
AI Business Advisor sẽ tập trung vào các chỉ số hiệu năng chính (KPI):
- **Revenue**: Doanh thu thực tế đã thu tiền.
- **Order Count**: Tổng số lượt phục vụ.
- **Average Order Value (AOV)**: Mức chi tiêu trung bình của mỗi khách hàng.
- **Growth Rate**: Tỷ lệ tăng trưởng doanh thu/đơn hàng so với kỳ trước.
- **Top Products Contribution**: Tỷ trọng đóng góp doanh thu của các món bán chạy nhất.
- **Profit Margin (Sơ bộ)**: Tỷ suất lợi nhuận dựa trên dữ liệu giá vốn hiện có.

## 4. Business Insight Design
AI sẽ thực hiện suy luận (Inference) từ dữ liệu để tìm ra Insight:
- **Volume vs Value**: "Doanh thu tăng nhờ AOV tăng mặc dù số lượng đơn giảm" -> Khách hàng đang mua các món đắt tiền hơn hoặc combo lớn hơn.
- **Product Concentration**: "Món X đóng góp 40% doanh thu nhưng chỉ chiếm 10% số lượng đơn" -> Món X là sản phẩm cao cấp chủ lực.
- **Trend Detection**: Phát hiện xu hướng tăng/giảm liên tục trong 7 ngày gần nhất.
- **Operational Insight**: "Số đơn tăng cao nhưng nhân sự on-duty thấp" -> Cảnh báo quá tải vận hành.

## 5. Recommendation Design
Khuyến nghị (Recommendation) phải mang tính tham khảo và dựa trên bằng chứng dữ liệu:
- **Upselling**: Nếu AOV thấp -> Gợi ý khuyến khích combo hoặc topping.
- **Menu Optimization**: Nếu món có doanh thu cao nhưng số lượng ít -> Gợi ý đẩy mạnh quảng bá.
- **Cost Control**: Nếu lợi nhuận thấp dù doanh thu cao -> Gợi ý rà soát chi phí vận hành (Expenses).
- **Staffing**: Gợi ý điều chỉnh nhân sự dựa trên dự báo đơn hàng từ xu hướng quá khứ.

## 6. Tool Architecture Options
### Option A: Phối hợp các Tool hiện có
AI tự gọi `get_revenue`, `get_best_sellers`, `get_revenue_comparison` rồi tự tính toán AOV và Insight.
- **Ưu điểm**: Linh hoạt, không cần code thêm tool mới.
- **Nhược điểm**: Tiêu tốn nhiều token, độ trễ cao do gọi nhiều lần, AI có thể tính toán sai AOV (halucination logic).

### Option B: Tool tổng hợp `get_business_summary`
Tạo một Tool mới trả về toàn bộ KPI summary cho một khoảng thời gian.
- **Ưu điểm**: Dữ liệu nhất quán (như Dashboard), tiết kiệm lượt gọi tool, giảm thiểu sai số tính toán.
- **Nhược điểm**: Thêm một chút code tại Application Layer.

## 7. Recommended Architecture
**Đề xuất chọn Phương án B**: Triển khai thêm tool `get_business_summary`.
- Tool này sẽ gọi `IDashboardService.GetSummaryAsync` (đã có sẵn logic tính toán KPI chính xác).
- AI sẽ nhận được một "snapshot" toàn diện về tình hình kinh doanh để từ đó thực hiện Reasoning chuyên sâu.

## 8. Multi-tool Reasoning
Nhờ cơ chế Sequential Tool Calling hiện tại, AI có thể:
1. Gọi `get_business_summary` để lấy bức tranh tổng quan.
2. Nếu phát hiện doanh thu giảm, AI có thể gọi tiếp `get_order_list` để xem các đơn hàng gần nhất có vấn đề gì không (ví dụ đơn bị hủy nhiều).
3. Tổng hợp Insight và Recommendation cuối cùng.

## 9. Fact vs Inference vs Recommendation
Hệ thống Prompt sẽ được tinh chỉnh để AI phân biệt rõ:
- **Fact**: Số liệu thô từ Tool.
- **Inference**: Nhận định logic từ Fact (ví dụ: doanh thu tăng 10%).
- **Recommendation**: Hành động gợi ý dựa trên nhận định.

## 10. Security & Branch Isolation
- **Identity**: Luôn lấy `BranchId` từ `AiUserContext` (JWT).
- **Isolation**: Admin xem được toàn chuỗi, Manager chỉ xem được chi nhánh mình.
- **PII Protection**: Business Summary không trả về thông tin định danh khách hàng (tên, số điện thoại).

## 11. Performance
- **Reuse**: Tái sử dụng `IDashboardService` giúp tận dụng cơ chế Memory Cache (2 phút) hiện có.
- **Latency**: Giảm số lượt lặp giữa AI và Server bằng cách cung cấp dữ liệu KPI tập trung.

## 12. Test Plan
- **KPI Accuracy**: Đảm bảo AOV, Profit tính toán khớp với logic Dashboard.
- **Reasoning Test**: User hỏi "Tại sao doanh thu giảm?", AI phải biết gọi Tool so sánh và tìm nguyên nhân (ví dụ số đơn giảm).
- **Constraint Test**: AI không được đưa ra khuyến nghị viển vông (ví dụ "tăng giá" mà không có căn cứ).
- **Security Test**: Employee không được xem summary chứa dữ liệu lợi nhuận nếu role này bị cấm.

## 13. Demo Scenarios
1. "Phân tích tình hình kinh doanh hôm nay và cho tôi 3 lời khuyên."
2. "Tại sao doanh thu tuần này lại thấp hơn tuần trước?"
3. "Món nào đang là 'gà đẻ trứng vàng' của nhà hàng?"
4. "Tình hình chi phí và lợi nhuận tháng này thế nào?"
5. "Tôi có nên thêm nhân viên vào ca chiều nay không?" (Dựa trên số đơn hiện tại).

## 14. Implementation Plan
- **Phase 1**: Tạo tool `get_business_summary` kết nối với `IDashboardService`.
- **Phase 2**: Cập nhật `AdminPrompt` với các quy tắc suy luận Business Advisor.
- **Phase 3**: Triển khai unit test cho tool mới và các kịch bản reasoning.
- **Phase 4**: Cập nhật tài liệu `baocaodoan`.

## 15. Risks / Open Questions
- **Dữ liệu chi phí**: Nếu nhà hàng không nhập `Expenses` đầy đủ, KPI Lợi nhuận sẽ bị sai lệch. AI cần có câu lưu ý về việc này.
- **Context Window**: Business Summary quá lớn có thể chiếm nhiều context. Cần format JSON trả về tối giản nhất.
