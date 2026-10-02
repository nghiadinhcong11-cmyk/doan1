# AI Task 20 – Analytics & Business Intelligence Design

## 1. Current AI Architecture
Dựa trên mã nguồn thực tế tại `services/api/src/AI`:
- **Pattern**: Orchestration với Gemini Tool Calling. 
- **Flow**: `AiController` nhận request -> `AiOrchestrator` xây dựng prompt với `AiContextService` -> Gemini trả về `functionCall` -> `AiPermissionService` kiểm tra `Role` và `RiskLevel` thông qua `AiAuthorization` -> Thực thi Tool qua `Application Services` -> Trả kết quả về Gemini tổng hợp.
- **Hạn chế hiện tại**: `AiOrchestrator` xử lý vòng lặp tool call nhưng trả về (`return`) ngay sau tool đầu tiên, cản trở việc AI gọi nhiều tool liên tiếp trong một lượt (Sequential Tool Calling).

## 2. Existing Business Data
Dữ liệu hiện tại đã khá đầy đủ cho Analytics cơ bản:
- **Order**: `Id`, `InvoiceCode`, `CreatedAt` (UTC), `PaymentAt` (UTC), `PaidAmount`, `TotalAmount`, `Status`, `BranchId`, `PaymentMethod`.
- **OrderDetail**: `ProductId`, `ProductName`, `Quantity`, `UnitPrice`.
- **Product**: `Name`, `Price`, `CostPrice`.
- **Expense**: `Amount`, `Category`, `ExpenseDate`, `BranchId`.
- **Branch**: `Id`, `Name`.

## 3. Date/Time Handling
- **Database**: Lưu `DateTime.UtcNow`.
- **Logic hiện tại**: `DashboardService` sử dụng `SE Asia Standard Time` để xác định điểm bắt đầu/kết thúc ngày theo giờ Việt Nam, sau đó chuyển đổi sang UTC để truy vấn database.
- **Đề xuất**: Các Tool mới sẽ kế thừa logic này. AI sẽ nhận input là string (VD: "2026-09-10") hoặc các từ khóa (today, yesterday, this_week, this_month). Backend sẽ chịu trách nhiệm quy đổi sang UTC range chính xác.

## 4. Revenue Source of Truth
Dựa trên `DashboardService.cs`, doanh thu (Revenue) được định nghĩa là:
- `Sum(PaidAmount)`
- Điều kiện: `Status == "Hoàn thành"` (hoặc "Completed")
- Chỉ tính các đơn có `PaidAmount > 0`.
- **Refund/Reversal**: Hiện hệ thống chưa có logic Refund rõ ràng trong `OrderService`, chỉ có `Status == "Đã hủy"`. Cần thống nhất: Đơn hủy không tính doanh thu.

## 5. Tool Design: get_order_list
Cho phép AI tra cứu danh sách đơn hàng để phân tích chi tiết.
- **Input Schema**:
    - `fromDate` (string, optional): ISO date.
    - `toDate` (string, optional): ISO date.
    - `status` (string, optional): VD: "Hoàn thành", "Đang xử lý".
    - `limit` (number, default: 10, max: 50).
- **Security**: 
    - `BranchId`: Tự động lấy từ `AiUserContext`. AI không được phép yêu cầu `branchId` khác.
    - `Role`: Admin (Toàn chuỗi), Manager/Employee (Chi nhánh mình). Customer (Không được dùng).
- **Data Safety**: Không trả về `CustomerPhone`, `CustomerEmail` cho AI để tránh lộ PII (trừ khi có yêu cầu nghiệp vụ cụ thể).

## 6. Tool Design: get_best_sellers
Xác định các món bán chạy để tư vấn kinh doanh.
- **Input Schema**:
    - `period` (string): "today", "yesterday", "week", "month".
    - `limit` (number, default: 5).
- **Logic**: 
    - Truy vấn `OrderDetails` thuộc các `Orders` có `Status == "Hoàn thành"` trong khoảng thời gian.
    - Group by `ProductName`.
    - Tính `TotalQuantity` và `TotalRevenue`.
- **Financial Rule**: Tuân thủ logic doanh thu của hệ thống (chỉ tính đơn hoàn thành).

## 7. Tool Design: get_revenue_comparison
So sánh hiệu quả kinh doanh giữa các mốc thời gian.
- **Input Schema**:
    - `currentPeriod` (string): "today", "this_week", "this_month".
    - `comparisonPeriod` (string): "yesterday", "last_week", "last_month".
- **Output**:
    - Doanh thu, số lượng đơn của từng kỳ.
    - Chênh lệch tuyệt đối và tỷ lệ phần trăm (%).
    - Xu hướng (Tăng/Giảm/Không đổi).

## 8. Permission Matrix
| Tool | Admin | Manager | Employee | Customer | Risk Level |
| :--- | :---: | :---: | :---: | :---: | :---: |
| get_order_list | Global | Branch | Branch (Read) | X | Read |
| get_best_sellers | Global | Branch | Branch | X | Read |
| get_revenue_comparison | Global | Branch | X | X | Read |

## 9. Branch Isolation
- Mọi Tool Analytics **bắt buộc** sử dụng `branchId` từ `AiUserContext`.
- Admin có thể để `branchId = null` để xem toàn hệ thống.
- Logic lọc branch phải thực hiện ở tầng `Application Service`, AI Tool chỉ là lớp vỏ bọc.

## 10. Database / Performance
- **Vấn đề phát hiện**: Bảng `Orders` hiện thiếu index trên `CreatedAt` và `BranchId`.
- **Đề xuất**: Cần tạo composite index `(BranchId, CreatedAt, Status)` để tối ưu hóa các câu lệnh Group By và Filter theo thời gian.
- **Limit**: Áp dụng giới hạn `limit` chặt chẽ cho AI để tránh trả về JSON quá lớn gây overload memory hoặc quá giới hạn context của LLM.

## 11. Multi-tool Reasoning
- **Yêu cầu**: Nâng cấp `AiOrchestrator` để cho phép AI gọi nhiều tool trong một lượt (VD: xem doanh thu X và so sánh với Y).
- **Giải pháp**: Thay đổi vòng lặp `foreach` trong `AiOrchestrator` để thu thập tất cả `toolResult` trước khi gửi phản hồi cuối cùng cho Gemini.

## 12. Hallucination Protection
- System Prompt phải yêu cầu AI: "Nếu Tool trả về kết quả rỗng hoặc lỗi, hãy thông báo rõ, không được tự bịa số liệu."
- AI chỉ được phân tích dựa trên dữ liệu từ Tool, không được dùng kiến thức chung để dự đoán doanh thu.

## 13. Test Plan
- **TC01 (Auth)**: Khách hàng (Customer) gọi `get_order_list` -> Mong đợi: 403 Forbidden.
- **TC02 (Isolation)**: Manager chi nhánh A gọi `get_order_list` kèm `branchId` của B -> Mong đợi: Chỉ trả về dữ liệu A hoặc lỗi.
- **TC03 (Validation)**: `fromDate` > `toDate` -> Mong đợi: Lỗi validation rõ ràng.
- **TC04 (Performance)**: Truy vấn khoảng thời gian 1 năm -> Mong đợi: Backend tự động giới hạn hoặc yêu cầu rút ngắn range.
- **TC05 (Correctness)**: So sánh kết quả `get_revenue` với báo cáo Dashboard thực tế.

## 14. Recommended Implementation Plan
1.  Bổ sung composite index cho bảng `Orders`.
2.  Mở rộng `IDashboardService` để hỗ trợ so sánh doanh thu và lấy top sellers linh hoạt hơn.
3.  Triển khai 3 AI Tools theo thiết kế.
4.  Cập nhật `AiOrchestrator` để hỗ trợ Sequential Tool Calling.

## 15. Risks / Open Questions
- **Data Volume**: Với nhà hàng có hàng vạn đơn hàng, query analytics có thể gây chậm DB nếu không có caching tốt.
- **AI Token Limit**: Danh sách đơn hàng quá dài sẽ làm vượt giới hạn token của Gemini. Cần pagination hoặc tóm tắt (summarization) ở tầng service.
