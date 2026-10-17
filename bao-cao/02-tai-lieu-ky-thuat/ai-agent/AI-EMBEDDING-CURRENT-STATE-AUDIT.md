# AI Agent và Sentence Embedding — audit hiện trạng

## 1. Executive Summary

**INSPECTED, chưa chạy runtime trong lượt audit.** `IAiProvider` được DI gắn duy nhất với `MockAiProvider`; Mock dùng luật chuỗi cố định trong `MockAiIntentRecognizer`, phát `functionCall` theo hợp đồng JSON hiện có. `AiOrchestrator` thật sự xử lý chat, chuẩn bị context/schema, gọi provider, kiểm tra tool/permission rồi mới gọi Application Service. Không có model ngôn ngữ ngoài hoặc embedding trên đường chat này (`services/api/Program.cs:303-349`; `services/api/src/AI/Services/MockAiProvider.cs:28-69`; `services/api/src/AI/Services/AiOrchestrator.cs:113-166,217-269`).

Embedding E5 có project console độc lập, adapter ONNX .NET, tham chiếu Python, scorer và decision pipeline; artifact đầu ra cũ còn trên máy. Nó **chưa tích hợp** vào Mock, orchestrator, controller, frontend hoặc API project (`.tools/mock-ai-embedding-spike/README.md:1-5,24-27`; `services/api/Program.cs:303-349`). Các kết quả spike là dữ liệu phát triển, không chứng minh chat hiện tại đạt chất lượng nhận diện ngữ nghĩa.

## 2. Repository Snapshot

- Thời điểm đọc: 2026-10-17 03:40 UTC / 10:40 ICT. HEAD `38bbd423b963aaf5779d4ae3356f942ff236643b` (commit 2026-10-15 13:43:23 +0700, `Close owner employee audit source fixes`), theo `git log -1` và `git rev-parse HEAD`.
- Working tree **dirty trước audit**: AI controller/orchestrator, bốn tool, validator, `Program.cs`, hai ChatBot, nhiều tài liệu đã sửa; `.tools/mock-ai-embedding-spike/`, `ba-test-frontend/ai-chat/` và báo cáo embedding cũ là untracked. `git ls-files .tools/mock-ai-embedding-spike ba-test-frontend/ai-chat` trả rỗng. Vì vậy HEAD không phải phiên bản chính xác của các file hiện đang đọc; các artifact spike không gắn được với commit Git nào từ index hiện tại. Chỉ file báo cáo này được thêm trong audit.
- Stack được source thể hiện: API ASP.NET Core (`services/api/Program.cs`), Mock provider (`services/api/Program.cs:305`), project spike `net8.0`, `Microsoft.ML.OnnxRuntime 1.27.1`, `Tokenizers.HuggingFace 3.23.1` (`.tools/mock-ai-embedding-spike/MockAiEmbeddingSpike.csproj:4-12`). Cấu hình `AI:ProviderMode=Mock` nằm ở `services/api/appsettings.json:27-29` và `services/api/appsettings.Development.json:28-30`; DI hiện không chọn provider theo giá trị này (`services/api/Program.cs:303-305`). `MockAiProvider.IsConfigured` chỉ phản ánh Development, không thấy được orchestrator dùng làm gate (`services/api/src/AI/Services/MockAiProvider.cs:23-26`; `services/api/src/AI/Services/AiOrchestrator.cs:98-166`).

## 3. AI Runtime Architecture

Luồng thực tế của `/api/Ai/chat`:

```text
Admin/Customer ChatBot
  → POST /api/Ai/chat + JWT, message/history
  → AiController.Chat: authorize, validate, lấy role/user/branch từ JWT
  → AiOrchestrator.ProcessAsync (hoặc ProcessReadOnlyAsync nếu policy bật)
  → AiContextService.GetContextAsync [đọc DB] → PromptBuilder
  → AiToolRegistry.GetToolsDefinition [lọc role/read-only/customer]
  → MockAiProvider.GenerateContentAsync
      → MockAiIntentRecognizer.Recognize → text hoặc functionCall
  → AiToolRegistry.GetTool → guest/read-only gate → AiPermissionService.CanExecuteAsync
  → IAiTool.ExecuteAsync → Application Service → EF/DB
  → functionResponse → MockAiProvider.FormatToolResult → AiResponse
```

Chứng cứ: frontend `apps/admin-web/src/components/ChatBot.tsx:164-190`, `apps/customer-web/src/components/ChatBot.tsx:181-212`; controller `services/api/src/AI/Controllers/AiController.cs:63-140`; orchestrator `services/api/src/AI/Services/AiOrchestrator.cs:98-166,217-335`; provider `services/api/src/AI/Services/MockAiProvider.cs:45-69,111-145`; registry `services/api/src/AI/Tools/AiToolRegistry.cs:21-75`; permission `services/api/src/AI/Services/AiPermissionService.cs:30-69`.

Guest đi nhánh riêng `POST /api/Ai/guest-chat`: chỉ Development + `ProviderMode=Mock`, chặn truy vấn đơn cá nhân, rồi `ProcessGuestReadOnlyAsync` với allowlist duy nhất `customer_get_menu` (`services/api/src/AI/Controllers/AiController.cs:162-196`; `services/api/src/AI/Services/AiOrchestrator.cs:27-31,72-85`). Customer Web chọn chat JWT hoặc guest theo token (`apps/customer-web/src/components/ChatBot.tsx:203-212`). Chat chính có `[Authorize]` và role allowlist admin/manager/employee/cashier/customer; kitchen không có quyền vào route chat dù nhiều tool khai báo kitchen (`services/api/src/AI/Controllers/AiController.cs:22-24,63-67`).

Orchestrator có vòng `while` tối đa 5 lượt provider và xử lý nhiều `functionCall` trong một lượt (`services/api/src/AI/Services/AiOrchestrator.cs:151-155,217-335`). **Đây là năng lực hợp đồng**; Mock hiện phát một function call hoặc text, rồi khi có `functionResponse` nó trả text, nên chat Mock thông thường chỉ gọi tối đa một tool (`services/api/src/AI/Services/MockAiProvider.cs:45-69,158-174`). Tool lỗi trả `AiResponse.Success=false`; tool yêu cầu xác nhận dừng ngay tại boundary (`services/api/src/AI/Services/AiOrchestrator.cs:292-314`). Endpoint xác nhận giá tách biệt, admin-only, token gắn user/hết hạn/dùng một lần, và service kiểm tra lại giá cũ trước cập nhật (`services/api/src/AI/Controllers/AiController.cs:255-283`; `services/api/src/AI/Security/ProductPriceConfirmationStore.cs:29-86`; `services/api/src/Application/Services/ProductService.cs:76-103`). Mock hiện từ chối write trước khi chọn tool nên đường xác nhận này không được kích hoạt bởi câu chat Mock (`services/api/src/AI/Services/MockAiIntentRecognizer.cs:36-42`).

Request/response: `AiRequest.Message`, `History` (`services/api/src/AI/Models/AiRequest.cs:5-9`); `AiResponse.Message`, `Success`, `Error`, `Confirmation` (`services/api/src/AI/Models/AiResponse.cs:3-9`). Controller giới hạn message 1000 ký tự, history 6 mục/1000 ký tự; orchestrator đưa history client vào như user untrusted, không chấp nhận role model do client khai (`services/api/src/AI/Controllers/AiController.cs:69-76`; `services/api/src/AI/Services/AiOrchestrator.cs:123-141`). `AiContextService` **trực tiếp đọc DbContext** để tạo ngữ cảnh trước khi provider chạy; AI tool execution vẫn qua Application Service (`services/api/src/AI/Services/AiContextService.cs:14-21,48-113`). Đây là điểm lệch so với quy tắc kiến trúc “AI không truy cập DbContext trực tiếp”, cần ghi nhận, chưa sửa.

`PromptBuilder.BuildSystemInstruction` chọn `AdminPrompt`, `EmployeePrompt` hoặc `CustomerPrompt` theo role (`services/api/src/AI/Services/PromptBuilder.cs:12-44`); Mock không phân tích prompt đó để chọn intent mà đọc `contents` (`services/api/src/AI/Services/MockAiProvider.cs:32-53`). `AiToolValidator` kiểm tra required/type/range ở các tool có tham số (`services/api/src/AI/Utils/AiToolValidator.cs:22-107`). `AiToolExecutionAuditSink` nhận các sự kiện selected/denied/executed/failed từ orchestrator (`services/api/Program.cs:310-322`; `services/api/src/AI/Services/AiOrchestrator.cs:226-289`); file diagnostic chỉ đăng ký khi cấu hình đường dẫn (`services/api/Program.cs:48-53`). Không lấy việc có sink/logger làm bằng chứng đã có log chạy trong audit này.

## 4. Tool Registry & Authorization

`Program.cs:332-349` đăng ký **14 IAiTool** (8 Admin, 3 Employee, 3 Customer). Registry lọc `AllowedRoles`, read-only, customer allowlist và guest allowlist trước khi gửi schema; gate thứ hai nằm ở orchestrator/`AiPermissionService` (`services/api/src/AI/Tools/AiToolRegistry.cs:45-75`; `services/api/src/AI/Services/AiOrchestrator.cs:240-269`; `services/api/src/AI/Services/AiPermissionService.cs:30-69`). Bảng ghi role **khai báo trong từng tool**, sau đó đánh dấu khả năng chọn qua Mock. Ký hiệu A=admin, M=manager, E=employee, C=cashier, K=kitchen, U=customer. K bị route chat chặn; customer chỉ thực thi menu và my-orders; guest chỉ menu (`AiController.cs:63-67`; `AiToolRegistry.cs:12-17,55-60`; `AiPermissionService.cs:13-18,37-46`).

| Tool (source `Name/AllowedRoles/RiskLevel`, `ExecuteAsync`) | Nghiệp vụ | Role khai báo | Loại | Input schema | Service gọi / branch scope | Trạng thái chat Mock |
|---|---|---|---|---|---|---|
| `get_revenue` (`services/api/src/AI/Tools/Admin/GetRevenueTool.cs:15-18,25-56`) | Doanh thu kỳ | A,M | Read | `period` | `DashboardService.GetRevenueByPeriodAsync(period, BranchId)`; null admin = toàn hệ thống | Có rule |
| `update_product_price` (`.../Admin/UpdateProductPriceTool.cs:19-22,38-79`) | Đề nghị đổi giá | A | Write; confirmation | `productName,newPrice` | `ProductService.FindProductsByExactNameAsync`; confirm endpoint mới gọi `UpdatePriceByIdAsync`; catalog giá toàn hệ thống | Đăng ký nhưng Mock từ chối write |
| `get_active_staff` (`.../Admin/GetActiveStaffTool.cs:13-16,23-39`) | Nhân sự đi làm | A,M | Read | không | `EmployeeService.GetActiveStaffAsync(BranchId)` | Không có rule chọn |
| `get_order_list` (`.../Admin/GetOrderListTool.cs:17-20,27-77`) | Đơn theo ngày/status | A,M,E,C,K | Read | `fromDate,toDate,status,limit` | `OrderService.GetOrdersAsync`, filter BranchId từ context | Có rule; K bị chat chặn |
| `get_best_sellers` (`.../Admin/GetBestSellersTool.cs:15-18,25-86`) | Món bán chạy | A,M,E,C,K | Read | `period,limit` | `DashboardService.GetBestSellersAsync(..., BranchId,limit)` | Có rule; K bị chat chặn |
| `get_revenue_comparison` (`.../Admin/GetRevenueComparisonTool.cs:14-17,24-61`) | So sánh doanh thu | A,M | Read | `currentPeriod,comparisonPeriod` | `DashboardService.GetRevenueReportAsync` hai kỳ với BranchId | Có rule |
| `get_business_summary` (`.../Admin/GetBusinessSummaryTool.cs:14-17,24-81`) | KPI, số đơn | A,M | Read | `period` | `DashboardService.GetBusinessSummaryAsync(..., BranchId)` | Có rule; cũng dùng cho doanh thu hôm qua |
| `get_financial_analysis` (`.../Admin/GetFinancialAnalysisTool.cs:17-20,27-59`) | Phân tích tài chính | A,M | Read | `period` | `FinancialAnalysisService.GetFinancialAnalysisAsync(..., BranchId,period)` | Không có rule chọn |
| `employee_get_table_summary` (`.../Employee/GetTableSummaryTool.cs:14-17,24-34`) | Tình trạng bàn | A,M,E,C,K | Read | không | `TableService.GetTableStatusCountsAsync`; staff dùng BranchId, admin null | Có rule; K bị chat chặn |
| `get_my_shift` (`.../Employee/GetMyShiftTool.cs:14-17,24-34`) | Ca của chính mình | E,C,K | Read | không | `EmployeeService.GetShiftsByEmployeeIdAsync(UserId,today)` | Có rule; K bị chat chặn |
| `update_order_status` (`.../Employee/UpdateOrderStatusTool.cs:15-18,25-55`) | Đổi trạng thái đơn | A,M,E,C,K | Write | `orderCode,newStatus` | `OrderService.UpdateOrderStatusByCodeAsync(...,BranchId,Role)` | Đăng ký nhưng Mock từ chối write |
| `customer_get_menu` (`.../Customer/GetMenuTool.cs:18-21,28-49`) | Thực đơn/giá | A,M,E,C,K,U | Read | `category?` | `ProductService.GetAllProductsAsync(category,BranchId)`; guest/customer null = catalog chung | Có rule; guest được phép |
| `get_my_orders` (`.../Customer/GetMyOrderTool.cs:14-17,24-33`) | Đơn gần đây của mình | U | Read | không | `OrderService.GetOrdersByCustomerAsync(Email,PhoneNumber)`; không dùng BranchId | Có rule; guest bị chặn |
| `create_booking` (`.../Customer/CreateBookingTool.cs:15-18,22-75`) | Đặt bàn | U | Write | `bookingTime,guestCount,branchName,tableName?,note?` | `ReservationService.CreateBookingFromAiAsync`, tìm branch theo tên | Đăng ký nhưng customer allowlist chặn, Mock cũng từ chối write |

Phần đường dẫn rút gọn `.../` trong bảng đều là `services/api/src/AI/Tools/`. `RiskLevel.Destructive` không có tool nào trong 14 đăng ký (`Program.cs:332-349`, từng `RiskLevel` ở bảng). 11 read, 3 write, 0 destructive; trong chat Mock, 9 read có rule (kể cả menu), 2 read chưa có rule (`get_active_staff`, `get_financial_analysis`), 3 write không thể chọn bằng nhận diện hiện tại (`MockAiIntentRecognizer.cs:21-180`).

**Scope thực tế:** Staff thiếu BranchId bị orchestrator và permission chặn (`AiOrchestrator.cs:100-110`; `AiPermissionService.cs:49-59`). Dashboard/Employee/Table/Order services chỉ áp filter khi BranchId có giá trị (`services/api/src/Application/Services/DashboardService.cs:385-446`; `EmployeeService.cs:20-41`; `TableService.cs:182-187`; `OrderService.cs:41-47`), phù hợp admin toàn hệ thống nhưng phụ thuộc gate đầu vào cho manager/staff. `OrderService.UpdateOrderStatusByCodeAsync` kiểm tra branch của order đối với non-admin (`OrderService.cs:691-705`). `GetMyOrderTool` có `CustomerId` trong context nhưng service chỉ lọc email/phone; `AiController.Chat` đặt `CustomerId` và `PhoneNumber=User.Identity.Name`, không đặt `Email` (`AiController.cs:122-130`; `GetMyOrderTool.cs:26-33`; `OrderService.cs:745-769`). Khi claim Name không phải phone, có rủi ro kết quả rỗng; cần kiểm chứng với JWT thật. `AiAuthorization.IsInAuthorizedBranch` tồn tại nhưng không được `AiPermissionService.CanExecuteAsync` gọi; dữ liệu phụ thuộc service riêng (`AiAuthorization.cs:53-59`; `AiPermissionService.cs:30-69`). `ProductService.GetAllProductsAsync` lấy catalog chung rồi phủ availability theo BranchId, nên branch scoping ở đây là trạng thái khả dụng, không phải lọc product theo branch (`ProductService.cs:20-45`).

## 5. Current Natural Language Handling

**A. Intent recognition.** Mock đọc text cuối, chuẩn hóa bỏ dấu tiếng Việt, punctuation, gộp whitespace, lowercase rồi dùng `Contains` trên danh sách cụm cố định (`MockAiProvider.cs:51-69`; `MockAiIntentRecognizer.cs:21-24,428-459`). Không thấy regex hoặc semantic matching ở runtime. Thứ tự rule hardcode: giới thiệu/tồn kho/write → multi-intent → comparison → ranking → my orders → order status/count → revenue/summary → table/menu/shift → unsupported (`MockAiIntentRecognizer.cs:26-180`). Câu đồng nghĩa chỉ được nhận nếu cụm trong danh sách; đổi trật tự từ có thể còn match nếu cụm con giữ nguyên, nhưng không có suy luận ngữ nghĩa tổng quát. `IsMultiIntent` đếm nhóm cụm; comparison được ngoại lệ (`MockAiIntentRecognizer.cs:44-46,330-345`). Intent gần nhau phân biệt theo ưu tiên rule và marker, không theo khoảng cách semantic. Rủi ro chọn sai/không nhận khi paraphrase ngoài danh sách là suy luận từ code, chưa đo trên chat hiện tại.

**B. Entity/parameter extraction.** `ResolvePeriod`, `ResolveRevenuePeriod`, `ResolveComparison`, `ResolveOrderStatus`, `ExtractCategory`, `AddDateRange` lấy kỳ/status/category từ marker và dựng ngày today/yesterday theo giờ Việt Nam (`MockAiIntentRecognizer.cs:347-419`). Branch không trích từ câu; lấy từ JWT (`AiController.cs:102-105,122-130`). Danh tính khách hàng cũng từ JWT/context; không xác minh entity bằng embedding. Khi kỳ thiếu hoặc comparison không đủ cặp, trả câu hỏi làm rõ (`MockAiIntentRecognizer.cs:51-68,96-120`). History được ghép để hỗ trợ một số rule doanh thu, nhưng không có quản lý ngữ cảnh hội thoại tổng quát (`MockAiIntentRecognizer.cs:21-24,183-191`).

**C. Tool execution.** `MockAiProvider.HasTool` kiểm tra schema role trước khi phát function call; orchestrator còn tra registry, gate guest/read-only, permission role/risk rồi gọi service (`MockAiProvider.cs:61-79`; `AiOrchestrator.cs:226-269`). Input ngoài phạm vi trả `UNSUPPORTED_INTENT`; inventory và write trả unsupported riêng; nhiều intent trả yêu cầu tách câu (`MockAiIntentRecognizer.cs:31-46,180`). Cơ chế này giúp abstain với mẫu nhận biết được, nhưng không bảo đảm tránh mọi false positive trên câu chưa thử. Không đánh đồng gọi đúng tool với lấy đúng tham số hoặc dữ liệu (`MockAiIntentRecognizer.cs:49-120`; các `ExecuteAsync` ở bảng 4).

## 6. Previous Embedding Spike

- Mô hình đã **chạy trong spike theo artifact/log lịch sử**: `intfloat/multilingual-e5-small`, Hugging Face revision `614241f622f53c4eeff9890bdc4f31cfecc418b3`, ONNX `model_O4.onnx` (235,052,531 bytes); `tokenizer.json`, `tokenizer_config.json`, `config.json`, pooling config còn trong `.tools/mock-ai-embedding-spike/artifacts/onnx/` và `1_Pooling/`. Nguồn/revision/checksum ghi tại `bao-cao/02-tai-lieu-ky-thuat/system-audit/02-static-review/MOCK-AI-EMBEDDING-SPIKE.md:67-86`; file hiện hữu được kiểm kê read-only trong audit. `model.onnx` cũng còn nhưng báo cáo cũ nói tải dở và **không dùng**; không suy ra tính hợp lệ từ kích thước file (`.../MOCK-AI-EMBEDDING-SPIKE.md:5,86`).
- Ứng viên thứ hai `paraphrase-multilingual-MiniLM-L12-v2` revision `e8f8c211226b894fcb81acc59f3b34ba3efd5f42` có ONNX O4/tokenizer/metadata dưới `artifacts/minilm/`, nhưng tài liệu ghi chưa load, inference, benchmark hay so sánh; trạng thái **artifact-only** (`.../MOCK-AI-EMBEDDING-SPIKE.md:3-7`).
- .NET adapter **có mã hoàn chỉnh ở mức console spike**: tải tokenizer và `InferenceSession`, prefix `query: `, giới hạn 512 token, tạo `input_ids`, `token_type_ids`, `attention_mask`, chạy ONNX, masked mean pooling trên `last_hidden_state`, L2 normalize 384 chiều; kiểm tra metadata và quản lý concurrency/disposal (`.tools/mock-ai-embedding-spike/SentenceEmbeddingOptions.cs:1-9`; `SentenceEmbeddingAdapter.cs:26-55,71-92,98-117,159-235,256-263`). Python dùng cùng prefix/tokenizer/ONNX, masked pooling/L2 (`reference.py:26-58,77-79`). Truncation giữ token special cuối là logic spike tự viết, chưa chứng minh tương đương cấu hình truncation chính thức cho mọi input (`SentenceEmbeddingAdapter.cs:86-92`; `.../MOCK-AI-EMBEDDING-SPIKE.md:145-156`).
- Có so sánh Python/.NET trong script `compare.py:14-26` và artifact `artifacts/dotnet-results.json`, `python-results.json`; báo cáo cũ ghi token IDs/mask khớp, cosine delta tối đa `1.192092896e-07` (`.../MOCK-AI-EMBEDDING-SPIKE.md:160-185`). Đây là **PASS lịch sử cho artifact E5 đã chọn**, chưa chạy lại lượt này. Báo cáo cũ ghi median warm .NET 5.422 ms (n=20), khởi tạo 2,248.311 ms, working set 26.9→616.8 MB; số đo một máy, không phải SLA production (`.../MOCK-AI-EMBEDDING-SPIKE.md:197-226`).
- Có `IntentCatalog`, `IntentScorer` cosine (max/top-two mean), threshold score/margin, và `IntentDecisionPipeline` tách candidate/capability/parameter/authorization pending (`.tools/mock-ai-embedding-spike/IntentCatalog.cs:3-5`; `IntentScorer.cs:3-59`; `IntentDecisionPipeline.cs:5-23,43-141`). CLI mặc định `full-baseline`, max, .85/.02; profile v2 chỉ chọn tường minh, max/.85/.01 (`DecisionReviewRunner.cs:29-59`; `profiles/max-targeted-revision-v2.json:3-7,64-65`). Có abstain/clarification **trong spike**, không phải policy runtime.
- Dữ liệu tiếng Việt còn: 24 CAL, 18 SUP, 30 EMB và 15 check v2, cùng catalog 34 mẫu baseline/37 mẫu v2 theo tài liệu và JSON (`.tools/mock-ai-embedding-spike/README.md:18-22,39-46,88-95`; `check-targeted-v2.json`; `calibration*.json`; `evaluation-EMB-E01-E30.json`). Tài liệu v2 ghi 27/36 ready accepted đúng, 0 accepted sai trên các tập đã dùng phát triển, nhưng 3/6 new ready và lỗi menu/order/multi-intent còn chặn tích hợp (`.../MOCK-AI-EMBEDDING-CALIBRATION.md:134-153`; `.tools/mock-ai-embedding-spike/README.md:88-96`). Không coi đây là đánh giá độc lập hoặc chat E2E.
- Không có `ProjectReference` từ API hay lời gọi `SentenceEmbeddingAdapter`/`IntentDecisionPipeline` trong `services/api`, admin/customer source. `MockAiProvider.GenerateContentAsync` gọi `MockAiIntentRecognizer.Recognize`, không gọi scorer (`MockAiProvider.cs:51-53`). Spike README cũng ghi console độc lập (`.tools/mock-ai-embedding-spike/README.md:1-5,24-27`).

## 7. Existing Tests & Evidence

| Bằng chứng | Trạng thái | Giới hạn |
|---|---|---|
| Source DI/controller/orchestrator/tool/adapter hiện tại | INSPECTED | Không khởi động API, không query DB, không đo kết quả chat. |
| `artifacts/dotnet-results.json`, `python-results.json`, `compare.py` và báo cáo parity E5 | PASS **lịch sử được báo cáo**, INSPECTED artifact hiện còn | Chưa chạy lại; artifact untracked, không quy được commit. `MOCK-AI-EMBEDDING-SPIKE.md:160-185`. |
| `artifacts/adapter-lifecycle-checks.json` | PASS **lịch sử được ghi trong JSON** | Kiểm tra semaphore/disposal spike, không phải production; `.tools/mock-ai-embedding-spike/AdapterLifecycleChecks.cs`, `artifacts/adapter-lifecycle-checks.json`. |
| CAL/SUP/EMB và `targeted-v2-*.json` | PASS/FAIL **theo từng case lịch sử**, không gộp thành PASS toàn hệ thống | Dữ liệu đã dùng phát triển; lỗi còn ở `MOCK-AI-EMBEDDING-CALIBRATION.md:134-153`; artifact untracked. |
| 104 case manual chat (`ba-test-frontend/ai-chat/README.md:3,49-51`) | NOT RUN theo checklist còn hiện hữu | Các case hiện ghi `Chưa test`; checklist untracked, không phải runner. |
| Chat runtime, DB/branch ownership, confirmation, frontend E2E trên source hiện tại | NOT RUN | Audit cấm runtime/database; không lấy kết quả spike thay thế. |
| MiniLM inference/benchmark/parity | NOT RUN | Chỉ thấy file và tài liệu tải xuống (`MOCK-AI-EMBEDDING-SPIKE.md:3-7`). |

Không tìm thấy test tự động AI/embedding trong `tests/` ở working tree hiện tại qua `rg --files`/`rg` tên và tham chiếu AI. Git history `17e931e9` ghi đã xóa `AiToolRequest.cs`, `GeminiService.cs`, `IGeminiService.cs`; đó là source cũ, không phải test. `git ls-files` xác nhận spike và checklist AI chưa được commit. Không thấy log chạy chat hiện tại có thể quy cho HEAD + working tree này. **Không có build/test mới trong audit** theo ràng buộc chỉ đọc; mọi kết luận runtime là phân tích source.

## 8. Readiness Matrix

| Thành phần | Mức | Căn cứ |
|---|---|---|
| AI Agent runtime hiện tại | PARTIAL | Có luồng code; chưa chạy trên working tree dirty, context trực tiếp DB (`AiOrchestrator.cs:113-269`; `AiContextService.cs:14-21`). |
| Mock AI Provider | PARTIAL | Rule deterministic có code; chỉ Development theo `IsConfigured`, chưa kiểm E2E (`MockAiProvider.cs:23-69`). |
| Intent routing | PARTIAL | Hardcode phrase/rule, không semantic (`MockAiIntentRecognizer.cs:21-180,428-459`). |
| Tool Registry | READY **về cấu trúc source** | 14 DI, lọc schema và lookup; mức hoạt động thực tế chưa xác minh (`Program.cs:328-349`; `AiToolRegistry.cs:21-75`). |
| Role/permission enforcement | PARTIAL | Có gate đa tầng; kitchen tool khai báo nhưng route chặn; chưa E2E (`AiController.cs:63-67`; `AiPermissionService.cs:30-69`). |
| Branch isolation | PARTIAL | JWT BranchId và filter service; phụ thuộc gate, chưa kiểm DB thật (`AiController.cs:102-105`; `DashboardService.cs:385-446`). |
| Sentence Embedding model E5 | SPIKE ONLY | Artifact và kết quả console cũ; không runtime API (`.../MOCK-AI-EMBEDDING-SPIKE.md:67-86`). |
| Tokenizer/preprocessing | SPIKE ONLY | Có source/đối chiếu cũ; truncation cần xác minh khi tích hợp (`SentenceEmbeddingAdapter.cs:71-92`). |
| ONNX inference adapter | SPIKE ONLY | Có source và artifact console; chưa được API tham chiếu (`SentenceEmbeddingAdapter.cs:26-55,159-235`). |
| Similarity calculation | SPIKE ONLY | Cosine, max/mean thử nghiệm (`IntentScorer.cs:26-59`). |
| Threshold/abstention | SPIKE ONLY | .85/.02 default; v2 .85/.01 explicit, chưa chọn policy runtime (`DecisionReviewRunner.cs:39-59`; `profiles/max-targeted-revision-v2.json:64-65`). |
| Dataset tiếng Việt | PARTIAL | CAL/SUP/EMB/new checks có file; đã dùng phát triển, thiếu đánh giá độc lập mới (`README.md:18-22,88-95`). |
| Test harness | PARTIAL | Console/decision-review/compare có source+artifact; checklist chat chưa chạy (`README.md:7-16`; `ba-test-frontend/ai-chat/README.md:3,49-51`). |
| Tích hợp embedding không phá workflow | UNKNOWN | Chưa có adapter trong API, chưa kiểm permission/latency/fallback/chat E2E; không suy ra từ parity console (`Program.cs:303-349`; spike `README.md:24-27`). |

## 9. Gaps & Risks

1. Phạm vi intent hiện dựa vào cụm từ và thứ tự rule; câu đồng nghĩa, đảo cấu trúc hoặc hai intent gần nhau chưa có bằng chứng độ chính xác ngoài checklist chưa chạy (`MockAiIntentRecognizer.cs:21-180`; `ba-test-frontend/ai-chat/README.md:49-51`).
2. 14 tool đăng ký không đồng nghĩa 14 capability chat: hai read không rule; ba write không chọn; kitchen bị route chặn dù tool khai báo; customer booking bị allowlist chặn (`Program.cs:332-349`; `MockAiIntentRecognizer.cs:36-42,180`; `AiController.cs:63-67`; `AiPermissionService.cs:43-46`).
3. Scope dữ liệu không có một check branch chung khi execute; một số service dùng `BranchId` nullable để biểu thị admin global. Gate staff thiếu branch là điểm bảo vệ bắt buộc. `IsInAuthorizedBranch` hiện không dùng trong permission path (`AiAuthorization.cs:53-59`; `AiPermissionService.cs:30-69`).
4. `get_my_orders` lọc theo email/phone, trong khi context chat chỉ gán `CustomerId` và `PhoneNumber` từ claim Name; cần đối chiếu JWT thật trước khi kết luận chính xác/thiếu đơn (`AiController.cs:122-130`; `GetMyOrderTool.cs:26-33`; `OrderService.cs:745-769`).
5. Spike E5 tốn working set đáng kể theo lần đo cũ và dùng truncation riêng; concurrency/tokenizer safety chỉ được kiểm ở console. MiniLM chưa inference. Không có bằng chứng latency, isolation, permission, fallback khi chạy trong API (`SentenceEmbeddingAdapter.cs:54-55,86-92,212-235`; `MOCK-AI-EMBEDDING-SPIKE.md:197-226`).
6. CAL/SUP/EMB đã dùng để hiệu chỉnh; không được xem số liệu trên chúng là chất lượng ngoài mẫu. Artifact spike untracked và source API dirty khiến không thể gắn một commit chung cho toàn bộ kết quả (`.tools/mock-ai-embedding-spike/README.md:84-96`; `CALIBRATION-FREEZE.md:30-31,48-67`).

## 10. Audit Conclusion

Hệ thống chat hiện tại là **Mock rule-based** trong kiến trúc có orchestrator, tool registry và permission gate; chưa có semantic matching trong request path. E5 đã đạt một spike local ONNX/.NET có đối chiếu Python, scorer, threshold/abstain và dữ liệu thử tiếng Việt, nhưng chỉ ở project độc lập. Có thể tái sử dụng adapter, script đối chiếu, catalog/scorer/pipeline và mẫu phân tách intent–parameter–authorization **sau khi kiểm chứng lại**, cùng các Application Service/gate hiện hành. Chưa có chính sách chọn intent/threshold, đánh giá độc lập, tích hợp API, kiểm thử E2E/branch/role và quyết định tài nguyên vận hành đủ để kết luận embedding sẵn sàng triển khai.

Trước khi thiết kế thử nghiệm nhỏ cần chốt **phạm vi capability chỉ đọc**, tiêu chí abstain/clarification và lỗi chấp nhận được, bộ dữ liệu đánh giá mới tách khỏi CAL/SUP/EMB, cách kiểm tra tham số/entity độc lập với intent, ràng buộc RAM/latency, và cách giữ nguyên permission/confirmation/branch gate. Audit này không chọn intent hay câu thử nghiệm, không đổi role hoặc nghiệp vụ.


## Current status / Superseded by

Historical current-state audit; current: architecture evidence retained, runtime acceptance superseded by final acceptance report.
