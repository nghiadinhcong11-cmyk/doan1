# REPORT TASK 27 — AI RBAC & VALIDATOR HARDENING

## Executive Summary
Hệ thống AI đã được rà soát và thắt chặt bảo mật tại tầng **Backend Authorization** và **Argument Validation**. Các vai trò vận hành (`cashier`, `kitchen`, `manager`) hiện đã có quyền truy cập đúng đắn vào các công cụ AI cần thiết cho nghiệp vụ thực tế, trong khi các giới hạn bảo mật quan trọng (Admin-only, Branch Isolation) vẫn được duy trì nghiêm ngặt. `AiToolValidator` đã được nâng cấp để xử lý các tham số không hợp lệ mà không gây treo hoặc lỗi server.

## RBAC Changes
Đã cập nhật `AllowedRoles` cho các công cụ sau để phù hợp với luồng nghiệp vụ thực tế:

| Tool | Previous Roles | Updated Roles | Reason |
| :--- | :--- | :--- | :--- |
| `customer_get_menu` | admin, employee, customer | admin, manager, employee, cashier, kitchen, customer | Manager/Cashier/Kitchen cần tra cứu menu để hỗ trợ khách và chuẩn bị món. |
| `get_my_shift` | employee | employee, cashier, kitchen | Mọi nhân viên (bao gồm thu ngân và bếp) đều cần xem lịch làm việc. |
| `employee_get_table_summary`| admin, manager, employee | admin, manager, employee, cashier, kitchen | Thu ngân và bếp cần biết trạng thái bàn để điều phối phục vụ. |
| `update_order_status` | admin, manager, employee | admin, manager, employee, cashier, kitchen | Thu ngân xác nhận hoàn thành, bếp xác nhận bắt đầu chế biến. |
| `get_order_list` | admin, manager, employee | admin, manager, employee, cashier, kitchen | Thu ngân cần danh sách đơn để thanh toán, bếp cần danh sách để xử lý. |
| `get_best_sellers` | admin, manager, employee | admin, manager, employee, cashier, kitchen | Hỗ trợ nhân viên tư vấn món cho khách. |

## Validator Changes
Đã thực hiện các cải tiến tại `AiToolValidator.cs`:
1.  **Object Enforcement**: Thêm phương thức `EnsureObject` để kiểm tra `ValueKind == JsonValueKind.Object`. Các hàm `GetString`, `GetInt32`, `GetDecimal` giờ đây an toàn hơn khi nhận tham số không phải là object.
2.  **Graceful Error Handling**: Thay vì để server ném ngoại lệ không kiểm soát, validator ném `ArgumentException` với thông báo tiếng Việt rõ ràng (ví dụ: "Cấu trúc tham số không hợp lệ", "Thiếu tham số bắt buộc").
3.  **Hỗ trợ Null/Undefined**: `ValidateRequired` giờ đây xử lý an toàn khi toàn bộ arguments là `null` hoặc `undefined`.

## Security Verification
-   **Branch Isolation**: Đã kiểm tra lại logic `ValidateBranchIsolation`. Hệ thống tiếp tục sử dụng `BranchId` từ JWT (UserContext) làm nguồn tin cậy duy nhất. User không thể gửi `branchId` giả mạo qua AI Tool Arguments để xem dữ liệu chi nhánh khác.
-   **RBAC Boundary**: Các công cụ tài chính (`get_revenue`, `get_financial_analysis`, `get_business_summary`) vẫn được giới hạn chặt chẽ cho `admin` và `manager`.
-   **Write Tool Safety**: Các công cụ thay đổi dữ liệu (`update_product_price`, `update_order_status`) vẫn tuân thủ logic kiểm tra quyền tại tầng Service của Backend.

## Test Results
-   **New Tests**: Đã thêm `AiSecurityAuditTests.cs` bao gồm:
    -   Kiểm tra ma trận quyền hạn (RBAC Matrix) cho mọi Role.
    -   Kiểm tra độ bền của Validator với các tham số `null`, `empty`, hoặc sai định dạng.
    -   Kiểm tra cơ chế cách ly chi nhánh (Branch Isolation).
-   **Total Tests**: 173 bài test (tăng từ 135 bài).
-   **Status**: **173/173 PASS**.

## Before/After
-   **Before**: Nhân viên thu ngân (cashier) và nhân viên bếp (kitchen) hầu như không thể sử dụng AI để tra cứu thông tin vận hành cơ bản. Validator có thể ném ngoại lệ không mong muốn nếu model sinh JSON sai định dạng.
-   **After**: Các vai trò vận hành có thể sử dụng AI để hỗ trợ công việc hàng ngày. Hệ thống ổn định hơn trước các lỗi sinh tham số của LLM.

## Remaining Issues
-   Các thông báo lỗi của Validator hữu ích cho AI nhưng đôi khi có thể được Gemini tự ý diễn đạt lại cho người dùng.
-   Chưa có cơ chế giới hạn tần suất gọi Write Tools cho từng cá nhân (ngoài Rate Limiting chung).

## Recommendations for TASK 29
-   Tối ưu hóa **Tool Description** để hướng dẫn Gemini cung cấp tham số chính xác ngay từ lượt đầu tiên (đặc biệt là các giá trị enum cho `period`).
-   Bổ sung thêm các ví dụ sử dụng (few-shot) trong System Prompt để Gemini phân biệt rõ ranh giới giữa các công cụ tương đồng.
