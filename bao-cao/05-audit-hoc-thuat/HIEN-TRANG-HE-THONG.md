# Hiện trạng hệ thống để đối chiếu báo cáo

Ngày audit: 16/10/2026, Asia/Saigon. Nhánh `main`, HEAD `38bbd423b963aaf5779d4ae3356f942ff236643b`. Working tree đã có thay đổi chưa commit ở `.gitignore`, frontend chat/route, checklist `ba-test-frontend`, hồ sơ `system-audit`, `chatgpt-context` và một số file AI backend; còn các file/thư mục AI audit chưa track. Những thay đổi đó được giữ nguyên. Chỉ có `AGENTS.md` ở gốc repository được tìm thấy.

Bản nguồn duy nhất trong `bao-cao` là `hệ thống quản lý nhà hàng (ver3).docx`, 4.744.590 byte, SHA-256 `D26DCE6C0437D9E54AB66862E40888C3A2EF21CE565A0016379294A7AE11ABF9`. Đường dẫn ASCII `bao-cao/he-thong-nha-hang-ver3.docx` không tồn tại. Bản gốc là file chưa track và không bị ghi đè.

Văn bản Word đã được đọc theo thứ tự paragraph/table trong `word/document.xml`: bản nguồn có 1.500 paragraph, 43 bảng Word, 28 media, 67 paragraph có `numPr`, 76 trường `SEQ`, 277 trường `REF` và 3 trường `TOC`. Heading1–Heading4 và caption được đọc từ thân; số trang trong danh mục không được dùng để xác định vị trí biên tập.

## Bản đồ nguồn hiện hành

| Nhóm | Mã và vị trí đã kiểm tra | Kết luận tĩnh / giới hạn |
|---|---|---|
| Backend | `services/api/Program.cs:279,296,304,368-369`; `src/Infrastructure/Persistence/ApplicationDbContext.cs` | Các tầng Domain/Application/Infrastructure/WebAPI cùng module AI; EF Core cấu hình Npgsql. Chưa kiểm tra DB đang triển khai. |
| Frontend | `apps/admin-web/src/App.tsx:103-117`; `apps/customer-web/src/App.tsx:187-192` | Hai React web. POS/KDS nằm trong admin-web; customer-web có thực đơn số, đặt bàn, profile, quét QR. Route không chứng minh luồng browser đã chạy. |
| Auth/chi nhánh | `services/api/Program.cs:109-114`; `src/WebAPI/Controllers/OrderController.cs:608-635`; `src/WebAPI/Hubs/KitchenHub.cs:28-44`; `src/AI/Services/AiPermissionService.cs:30-63` | JWT, vai trò và kiểm tra chi nhánh xuất hiện theo từng đường. Phạm vi Manager phải xét theo endpoint/tool. |
| QR/POS/KDS | `src/WebAPI/Controllers/OrderController.cs:142-143,320-338,361-396`; `src/WebAPI/Hubs/KitchenNotifier.cs:22-36`; `apps/admin-web/src/features/pos/pages/POSPage.tsx`; `features/kitchen/pages/KitchenPage.tsx`; `apps/customer-web/src/pages/DigitalMenu.tsx` | Có thành phần cài đặt theo luồng, chưa thử HTTP/SignalR runtime. |
| Tài chính/Loyalty | `src/Application/Services/OrderService.cs:361-402,508`; `src/Application/Services/LoyaltyService.cs` | OrderService tính và lưu snapshot/total bằng decimal; thanh toán so với total backend. Chưa thử thanh toán thực. |
| Nhân sự | `src/Domain/Entities/Employee.cs:10,15`; `src/WebAPI/Controllers/EmployeeController.cs:78-83,235-240`; `src/Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs:328,352` | `EmployeeType` và `BasicSalary` vẫn có trong entity, snapshot và API đọc/ghi. Không suy ra có Payroll. |
| Kho | `src/Infrastructure/Persistence/ApplicationDbContext.cs:276-382`; `src/Application/Services/InventoryService.cs:11,263-310`; `src/WebAPI/Controllers/InventoryController.cs:11-87`; `apps/admin-web/src/app/routes/ManagementRoutes.tsx:40-46` | Có entity, mapping, service, API, UI kho. Migration `20261001044428_AddInventoryManagement.cs` là lịch sử, chưa chứng minh đã áp vào DB. |
| Business Insights | `src/Domain/Entities/BusinessInsight.cs:17-20`; `src/Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs:200-223` | `PeriodStart/PeriodEnd` là DateTime không nullable trong CLR/snapshot; kỳ so sánh nullable. DB thực chưa kiểm. |
| Receipt/system settings | `src/Domain/Entities/ReceiptSettings.cs:5-21`; `src/WebAPI/Controllers/ReceiptSettingsController.cs:11-56`; `SystemSettingsController.cs:12-33` | Có tên thuộc tính cụ thể; quyền đọc/sửa khác nhau. |
| AI | `services/api/Program.cs:304,315-348`; `src/AI/Services/MockAiProvider.cs:8-28`; `src/AI/Services/AiOrchestrator.cs:121,166,257-269`; `src/AI/Tools/AiToolRegistry.cs:21-60` | `IAiProvider` được đăng ký với `MockAiProvider`; mô phỏng xác định/allowlist, chỉ cấu hình trong Development, không gọi LLM ngoài. Có 14 đăng ký `IAiTool`; danh sách thấy được còn lọc theo role. |
| Test/checklist | `ba-test-frontend/README.md`; `bao-cao/01-khoa-luan/chuong-4/CHAPTER-4-EVIDENCE-AUDIT.md:181-186` | Hồ sơ cũ ghi 375 test/SQLite ở 07/10/2026. Project `tests/RestaurantPOS.Tests/RestaurantPOS.Tests.csproj` hiện không tồn tại. Checklist là kịch bản, không là kết quả đã chạy. |
| Hướng dẫn chạy | `services/api/Program.cs:368-369,593`; hai `package.json` frontend | Có cấu hình PostgreSQL và Hub `/kitchenHub`; chưa chạy app, migration, seed hoặc kết nối Supabase. |

`PaymentCompleted` hiện được gửi qua `Clients.Groups(PaymentEventRouting.Groups(branchId))` ở `OrderController.cs:420-430`, không phát `Clients.All`; `PaymentEventRouting.cs:8-19` có group admin và group vai trò theo branch. Tìm kiếm `apps/` và `services/` chưa thấy consumer trực tiếp của chuỗi sự kiện này. Chưa thể kết luận giao nhận SignalR trong runtime.

Đối chiếu bổ sung ngày 16/10/2026 cho phân công audit Chương 1: người dùng đã chọn **thiết kế mục tiêu** Mock AI kết hợp sentence embedding cục bộ để hỗ trợ nhận diện tool. Đây chưa là hiện trạng production. `.tools/mock-ai-embedding-spike/` có `SentenceEmbeddingAdapter.cs:26,38`, `IntentDecisionPipeline.cs:23-30` và package ONNX trong project riêng, nhưng `services/api/Program.cs:304` vẫn đăng ký `MockAiProvider`; `src/AI/Services/MockAiProvider.cs:53` vẫn gọi `MockAiIntentRecognizer.Recognize`. Không thấy adapter/pipeline embedding được đăng ký hay gọi trong production `services/api/src/AI`/`Program.cs`. Không suy model ID, metric, ngưỡng, chất lượng hoặc runtime từ spike.

Prompt 0 là audit tĩnh. Không chạy build/test, không tạo hoặc áp dụng migration, không gọi dịch vụ AI hay kết nối DB dùng chung.
