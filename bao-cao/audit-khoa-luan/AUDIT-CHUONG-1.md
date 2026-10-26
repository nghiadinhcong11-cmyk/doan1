# Audit Chương 1 — Tổng quan đề tài

## 1. Phạm vi audit

Audit này chỉ cung cấp bằng chứng để ChatGPT viết lại Chương 1 của khóa luận **Hệ thống quản lý nhà hàng đa chi nhánh tích hợp trợ lý ảo AI Agent**. Không viết lại chương, không sửa Word, không sửa source code, migration, database, cấu hình hoặc tài liệu hiện có.

Các nội dung được kiểm tra:

- phạm vi sản phẩm thực tế;
- frontend, backend, vai trò và branch isolation;
- các nghiệp vụ chính và nghiệp vụ hỗ trợ;
- đường thực thi AI hiện tại;
- giới hạn nghiên cứu;
- đối chiếu với Chương 1 cũ ở mức bằng chứng có thể truy cập.

## 2. Phiên bản và nguồn bằng chứng

| Mục | Giá trị |
|---|---|
| Thời điểm audit | 20/10/2026, 22:55:31, Asia/Saigon |
| HEAD | `9365c18614666ab4c80eee714403ef73b14edf54` — `Update repository README for current system` |
| Branch | `main` |
| Working tree | Không sạch: đang có các file audit học thuật và Word cũ bị xóa trong working tree; các thay đổi này không do audit này phục hồi hoặc hoàn tác |
| Nguồn ưu tiên | Source code hiện tại, DI và execution path, entity/DbContext/migration, frontend routes/pages, checklist |
| Nguồn lịch sử | `git show HEAD:bao-cao/05-audit-hoc-thuat/CHUONG-1-AUDIT.md` chỉ dùng để biết các nhận định cũ; không dùng thay source hiện tại |
| Word Chương 1 | Không có bản Word truy cập được trong working tree tại thời điểm audit vì file nằm trong nhóm đang bị xóa; đối chiếu trực tiếp với bản Word hiện tại: `UNVERIFIED` |

Không chạy build, test, ứng dụng, database, migration, seed hoặc UAT trong lượt audit này. Những trạng thái `IMPLEMENTED` dưới đây là trạng thái đường cài đặt trong source, không phải kết quả runtime.

## 3. Kết luận nhanh

1. Sản phẩm hiện có **hai ứng dụng frontend**: `admin-web` và `customer-web`. POS và KDS không phải frontend độc lập; cả hai nằm trong `admin-web` dưới các route theo vai trò.
2. Backend là ASP.NET Core 8, EF Core/Npgsql, PostgreSQL, JWT Bearer và SignalR. Source có các lớp Domain, Application, Infrastructure, WebAPI và AI; không nên tuyên bố hệ thống hoàn toàn tuân thủ Clean Architecture chỉ từ cách chia thư mục.
3. Hệ thống có đường cài đặt cho đơn hàng, POS, thanh toán nội bộ, KDS, khách hàng, loyalty, nhân sự, kho, chi phí, báo cáo, bàn và đặt bàn. Mức độ hoàn thiện runtime của từng đường chưa được audit bằng UAT trong lượt này.
4. Đa chi nhánh thể hiện qua `BranchId`, JWT branch claim, `ProductBranchAvailability`, các query/service theo branch, branch group của SignalR và QR guest session.
5. AI hiện tại là local E5 sentence-embedding runtime qua ONNX Runtime và tokenizer local. Không có provider AI cloud hoặc LLM được đăng ký trong DI hiện tại.
6. Admin AI có các tool đọc và bốn nhóm write đã nối tới application service: đổi giá, đổi tên món, tạo món và tạo topping. Write flow dừng ở preview/confirmation; permission và token confirmation nằm ở backend.
7. Không được viết rằng hệ thống có LLM suy luận tổng quát, Function Calling của provider ngoài, đối soát ngân hàng tự động, payroll, tự trừ nguyên liệu theo công thức hoặc lưu hội thoại lâu dài.

## 4. Bảng tổng hợp phạm vi hệ thống

### 4.1. Ứng dụng và công nghệ

| Thành phần | Trạng thái audit | Vai trò thực tế | Bằng chứng |
|---|---|---|---|
| `admin-web` | `IMPLEMENTED` | Admin, manager, cashier, kitchen và employee dùng chung một React/Vite application với shell/route khác nhau | `apps/admin-web/src/App.tsx:95-117`; `apps/admin-web/src/app/routes/ManagementRoutes.tsx:28-195`; `CashierRoutes.tsx`; `KitchenRoutes.tsx`; `StaffRoutes.tsx` |
| `customer-web` | `IMPLEMENTED` | Customer đăng nhập, menu/đặt món, profile, reservation, scan QR và guest session | `apps/customer-web/src/App.tsx:155-233`; `pages/DigitalMenu.tsx`; `pages/CustomerProfile.tsx`; `pages/QRScan.tsx` |
| POS | `IMPLEMENTED` ở đường source | Màn bán hàng và xử lý order nằm trong `admin-web`, dùng `/pos` cho admin/manager/cashier và `/staff/sales` cho employee | `ManagementRoutes.tsx:186-195`; `CashierRoutes.tsx:32-45`; `StaffRoutes.tsx:29-42`; `features/pos/pages/POSPage.tsx` |
| KDS | `IMPLEMENTED` ở đường source | Màn hình bếp và lịch sử bếp nằm trong `admin-web`, dùng `/kitchen` và `/kitchen/history` | `ManagementRoutes.tsx:186-195`; `KitchenRoutes.tsx:25-51`; `features/kitchen/pages/KitchenPage.tsx`; `KitchenHistoryPage.tsx` |
| API | `IMPLEMENTED` | ASP.NET Core 8 Web API, controllers, JWT authentication, rate limit cho AI và SignalR | `services/api/RestaurantPOS.api.csproj`; `services/api/Program.cs:60-61,534-537` |
| Persistence | `IMPLEMENTED` ở model/migration | EF Core `ApplicationDbContext`, Npgsql/PostgreSQL, migration và model snapshot | `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs:14-44`; `services/api/src/Infrastructure/Persistence/Migrations/` |
| AI runtime | `IMPLEMENTED` ở đường DI/runtime; UAT độc lập không chạy trong lượt này | ONNX Runtime + Hugging Face tokenizer + local reference catalog | `RestaurantPOS.api.csproj` package references; `Program.cs:303-335`; `AdminEmbeddingPilot.cs:183-189,475-496`; `CustomerGuestEmbeddingPilot.cs:164-188` |

### 4.2. Vai trò và mục đích

| Vai trò/ngữ cảnh | Phạm vi source hiện tại | Bằng chứng |
|---|---|---|
| `admin` | Quản trị toàn hệ thống, chọn branch cho một số context, quản lý settings/branches và dùng Admin AI | `RoleNames.cs:11`; `ManagementRoutes.tsx:179-184`; `AiController.cs:62-161`; `AiPermissionService.cs:25-33` |
| `manager` | Quản lý nghiệp vụ branch được gán; không được suy thành global admin | `RoleNames.cs:12,35-51`; `ManagementRoutes.tsx:28-195`; các controller có `[Authorize(Roles = "admin,manager")]` |
| `cashier` | POS, thanh toán, invoice và một số thao tác ca/đặt bàn theo quyền | `CashierRoutes.tsx:32-45`; `OrderController.cs:431-432`; `InvoiceController.cs:11-18`; `ShiftController.cs:17` |
| `kitchen` | KDS, lịch sử bếp, cập nhật trạng thái yêu cầu chế biến và một số trang hỗ trợ | `KitchenRoutes.tsx:25-51`; `OrderController.cs:397-419,540-635`; `KitchenHub.cs:10-44` |
| `employee` | Staff overview, sales/order, bàn, inventory overview, notification, attendance, schedule và profile | `StaffRoutes.tsx:25-42`; `OrderController.cs:150-177,355-397`; `AttendanceController.cs:19`; `WorkScheduleController.cs:17` |
| `customer` | Customer đăng nhập, menu, order của mình, profile, loyalty history và reservation | `AuthController.cs:280-318`; `CustomerController.cs:137-282`; `OrderController.cs:162-177,480-517`; `customer-web/src/App.tsx:227-233` |
| Guest QR | Không phải tài khoản customer thông thường; dùng JWT/session do server cấp từ QR của bàn, scope theo branch/table | `GuestController.cs:39-95`; `GuestSessionContext.cs:13-49`; `AiController.cs:183-224` |

## 5. Danh sách chức năng có căn cứ từ source

| Nhóm | Chức năng | Kết quả audit | Bằng chứng chính | Giới hạn phát biểu |
|---|---|---|---|---|
| Xác thực | Login nhân viên/customer, JWT, change password, `/api/Auth/me` | `IMPLEMENTED` | `AuthController.cs:118-350`; `JwtService.cs:21-76` | Chưa suy ra chất lượng bảo mật production chỉ từ source |
| Chi nhánh | CRUD/toggle branch, branch payment info, branch scope | `IMPLEMENTED` ở source | `BranchController.cs:29-227`; `Branch.cs`; `RoleNames.cs:35-51` | Không khẳng định đã kiểm đủ mọi endpoint cross-branch |
| Catalog | Product, topping, promotion; active/inactive; giá; branch availability | `IMPLEMENTED` | `ProductController.cs:37-234`; `ToppingController.cs:27-85`; `PromotionController.cs:26-89`; `ProductBranchAvailability.cs`; migration `20261015051548_AddProductBranchAvailability.cs` | Product identity/price global, khả năng bán theo branch được tách riêng |
| Bàn/QR | Area, table, QR token, table status | `IMPLEMENTED` | `AreaController.cs`; `TableController.cs:25-152`; `GuestController.cs:39-95`; `QRScan.tsx`; `DigitalMenu.tsx:410-454` | QR guest cần session server hợp lệ; không xem QR là cơ chế thanh toán ngân hàng |
| Reservation | Tạo, xem, cập nhật, status và xóa reservation | `IMPLEMENTED` ở service/API | `ReservationController.cs:24-178`; `ReservationService.cs:20-194` | Chưa có bằng chứng UAT trong lượt này |
| Order | Tạo/cập nhật order, lấy danh sách/chi tiết, accept web order, gọi bếp, status, xóa | `IMPLEMENTED` | `OrderController.cs:50-635`; `OrderService.cs:41-895`; `Order.cs`; `OrderRequest.cs` | Không tự kết luận mọi trạng thái đã E2E đúng |
| Tính tiền | Subtotal, VAT, service fee, discount, total, paid amount và payment snapshot | `IMPLEMENTED` ở service/model | `Order.cs:14-39`; `OrderService.cs:218-356,626-781`; `SystemSettingService.cs` | Không dùng đây làm bằng chứng cho đối soát ngân hàng |
| Thanh toán | Payment endpoint cho cash/transfer và UI VietQR theo thông tin branch | `PARTIAL` | `OrderController.cs:431-478`; `OrderService.cs:626-729`; `BranchController.cs:88-107`; `POSPage.tsx:996-1024` | Không tìm thấy webhook ngân hàng, xác thực giao dịch từ ngân hàng hoặc đối soát tự động |
| KDS/realtime | Gửi request xuống bếp, active/history/detail, cập nhật status, SignalR groups | `IMPLEMENTED` ở source | `KitchenService.cs:33-378`; `OrderController.cs:397-419,588-635`; `KitchenHub.cs:10-44`; `POSPage.tsx:791`; `KitchenPage.tsx:98-166` | Runtime SignalR/E2E chưa chạy trong lượt audit |
| Customer | Profile, order history/detail, reservation, loyalty history | `IMPLEMENTED` ở source | `CustomerProfile.tsx:173-253,399-506`; `CustomerController.cs:137-282` | Phạm vi dữ liệu phải tiếp tục được kiểm bằng runtime IDOR test |
| Loyalty | Earn sau order paid/completed, redeem, refund, balance, history, order summary | `IMPLEMENTED` ở service/API | `LoyaltyService.cs:26-320`; `OrderService.cs:717-720,766-771`; `OrderController.cs:480-515` | Chưa đo hiệu quả kinh doanh hoặc UAT độc lập trong lượt này |
| Nhân sự | Employee CRUD/toggle, role, EmployeeType/BasicSalary field, attendance, schedule, shifts | `PARTIAL` theo phạm vi | `EmployeeController.cs:22-317`; `Employee.cs:10-24`; `AttendanceController.cs`; `WorkScheduleController.cs`; `ShiftController.cs` | Có trường `BasicSalary` nhưng không có payroll engine hiện hành; migration `20260919075618_RemovePayrollAndRepairReceiptSettings.cs` loại các bảng payroll |
| Kho | Inventory item, tồn theo branch, minimum stock, stock receipt/issue, confirm/cancel, transaction history | `IMPLEMENTED` ở source | `InventoryController.cs:12-87`; `InventoryService.cs:16-460`; `InventoryItem.cs`; `BranchInventory.cs`; migration `20261001044428_AddInventoryManagement.cs` | Kho chưa gắn công thức món và chưa tự trừ nguyên liệu theo order |
| Chi phí | Ghi nhận/list/update/delete expense, idempotency cho manual expense | `IMPLEMENTED` ở source | `ExpenseController.cs:20-148`; `Expense.cs`; `ManualExpenseIdempotencyRecord.cs` | Chi phí là ghi nhận vận hành, không phải tự tính toàn bộ COGS từ recipe |
| Báo cáo | Dashboard summary, revenue, order count, best sellers, comparison, invoice | `IMPLEMENTED` ở source | `DashboardController.cs:16-32`; `DashboardService.cs:60-424`; `InvoiceController.cs:11-66` | Dedicated BusinessInsight entity/controller đã bị loại ở migration `20261019104042_DropBusinessInsights.cs`; không mô tả như module hiện hành độc lập |
| Settings/receipt | System settings, receipt settings, print template, branch profile/payment info | `IMPLEMENTED` ở source | `SystemSettingsController.cs:13-31`; `ReceiptSettingsController.cs:12-83`; `ManagementRoutes.tsx:179-184`; `ReceiptPreview.tsx` | Cần phân biệt settings branch với global admin scope |
| Notifications | List/count/read/read-all, routing theo user/branch/role | `IMPLEMENTED` ở source | `NotificationController.cs:13-50`; `NotificationRecipientRouting.cs`; frontend notification hooks | Chưa có bằng chứng tải lớn/realtime đầy đủ |

## 6. Quy trình nghiệp vụ trọng tâm

| Quy trình | Đường cài đặt được xác minh | Kết luận | Giới hạn |
|---|---|---|---|
| Khách quét QR → xem menu → gửi yêu cầu | `QRScan.tsx`/`DigitalMenu.tsx` → `GuestController.bootstrap` → public/product APIs → `OrderController` | `IMPLEMENTED` ở source | Cần QR hợp lệ và guest session; chưa chứng minh E2E hiện hành bằng lượt audit này |
| Nhân viên tiếp nhận order trực tuyến | `OrderController` pending/accept → `OrderService.AcceptWebOrderAsync` → order/table state | `IMPLEMENTED` | Chưa kết luận idempotency toàn bộ request từ source đơn thuần |
| POS xử lý order/gọi thêm món | `POSPage.tsx` → `OrderController` create/update → `OrderService.CreateOrUpdateOrderAsync`; `send-to-kitchen` cho phần mới | `IMPLEMENTED` | Không tự khẳng định mọi trường hợp cạnh tranh hoặc offline |
| Chuyển yêu cầu xuống KDS | `OrderController.send-to-kitchen` → `KitchenService.SendToKitchenAsync` → `OrderRequest` → SignalR notifier/Hub | `IMPLEMENTED` ở source | Chưa chạy producer/consumer realtime trong audit này |
| Bếp nhận/cập nhật trạng thái | `KitchenPage.tsx` → `OrderController` kitchen endpoints → `KitchenService.UpdateRequestStatusAsync` | `IMPLEMENTED` | Cần UAT để kết luận hiển thị/trạng thái thực tế |
| Thu ngân thanh toán | POS payment UI → `OrderController/{id}/payment` → `OrderService.PayOrderAsync` → paid status/payment snapshot | `IMPLEMENTED` ở source; thanh toán ngân hàng `PARTIAL` | Chưa có bank webhook/đối soát tự động |
| Loyalty/lịch sử giao dịch | Payment/status completion → `LoyaltyService.EarnPointsAsync`; redeem/refund/history endpoints/UI | `IMPLEMENTED` ở source | Không khẳng định số điểm/transaction đã kiểm thực tế |
| Nhân sự | Employee, attendance, schedule, shift controllers/services/pages | `IMPLEMENTED` một phần theo từng nghiệp vụ | Không có payroll engine hiện hành |
| Kho | Inventory CRUD, receipt/issue workflow, minimum stock, transaction history | `IMPLEMENTED` độc lập | Không có recipe/product-ingredient và auto consumption từ order |
| Chi phí/báo cáo | Expense controller/service; dashboard aggregates orders/expenses | `IMPLEMENTED` ở source | COGS tự động theo nguyên liệu và đối soát ngân hàng không có bằng chứng |
| Đặt bàn | Reservation controller/service; table/branch context; customer/admin pages | `IMPLEMENTED` ở source | Chưa kiểm runtime toàn bộ trạng thái và conflict |

## 7. Đa chi nhánh

| Thành phần | Bằng chứng | Nhận định |
|---|---|---|
| JWT branch scope | `JwtService.cs:21-76` tạo claim `branchId`; `RoleNames.cs:35-51` kiểm branch của staff | `IMPLEMENTED` ở authentication/source |
| Branch-sensitive APIs | Controller/service nhận `branchId`, `authorizedBranchId` hoặc `InventoryUserContext` | `IMPLEMENTED` ở nhiều module; cần audit endpoint-by-endpoint nếu viết tuyên bố tuyệt đối |
| Catalog availability | `ProductBranchAvailability` và `ProductService.GetSellableProductsAsync` chỉ chọn sản phẩm active + availability `Available` tại branch | `IMPLEMENTED` ở source; product metadata/price vẫn dùng catalog chung |
| Admin branch selection | `AiController.cs:97-112` chỉ nhận `X-Branch-Id` cho admin; manager giữ JWT branch | `IMPLEMENTED` cho AI route |
| QR guest scope | `GuestController.cs:39-95`, `GuestSessionContext.cs:13-49`, frontend session storage | `IMPLEMENTED` ở source |
| SignalR branch groups | `KitchenHub.cs:28-44` tạo group theo branch và role; admin có group kitchen-admin | `IMPLEMENTED` ở source |

Không được viết “mọi chức năng đã chứng minh cách ly tuyệt đối” nếu chưa có bộ test runtime cho từng API, branch A/B và role tương ứng.

## 8. Phạm vi AI hiện tại

### 8.1. Entry point và DI

| Thành phần | Bằng chứng | Kết luận |
|---|---|---|
| Admin chat | `AiController.Chat`, `POST /api/Ai/chat`, `[Authorize(Roles = "admin,customer")]`, giới hạn message 1000 ký tự và history tối đa 6 mục | `IMPLEMENTED` |
| Guest chat | `AiController.GuestChat`, `POST /api/Ai/guest-chat`, chỉ cho verified QR session, Development và customer/guest pilot enabled | `IMPLEMENTED` nhưng có giới hạn môi trường |
| Confirmation endpoints | `product-price-confirmation`, `product-name-confirmation`, `catalog-creation-confirmation`, admin-only | `IMPLEMENTED` |
| Orchestrator | `AiOrchestrator` gọi pilot → decision → registry → permission → tool → formatter | `IMPLEMENTED` ở source |
| DI pilot | `Program.cs:303-335` đăng ký `AdminEmbeddingPilot`, `CustomerGuestEmbeddingPilot`, `AiPermissionService`, stores, registry và tools | `IMPLEMENTED` |
| Provider cloud | Không tìm thấy registration/http client/provider cloud trong current AI runtime; các lớp cũ `MockAiProvider`/`IAiProvider` không còn là đường DI hiện tại | `NOT_SUPPORTED` trong runtime hiện tại |

Lưu ý: một số XML comment hoặc tên lịch sử trong source còn ghi “Mock AI”, nhưng kết luận runtime phải dựa vào DI và `AiOrchestrator`: hai embedding pilots là đường quyết định hiện tại.

### 8.2. Cơ chế nhận diện và xử lý

1. `AdminEmbeddingPilot` và `CustomerGuestEmbeddingPilot` đọc model ONNX, tokenizer và reference catalog từ cấu hình; thiếu artifact thì throw/fail-closed (`AdminEmbeddingPilot.cs:183-189`; `CustomerGuestEmbeddingPilot.cs:164-172`).
2. Text được encode với prefix `query: `, chạy inference, pooling/normalize vector và so sánh với catalog/reference embeddings (`AdminEmbeddingPilot.cs:475-496`; `CustomerGuestEmbeddingPilot.cs:187-208`).
3. Pilot trả decision `ACCEPT`, `CLARIFY`, `UNSUPPORTED` hoặc `REJECT` cùng intent, tool, score/margin và slots. Đây là bộ quyết định có phạm vi cố định, không phải suy luận ngôn ngữ tổng quát.
4. `AiOrchestrator` không tự truy cập DbContext. Khi decision được chấp nhận, `AiToolRegistry` tìm tool, `AiPermissionService` kiểm role/risk/branch/scope, sau đó tool gọi application service.
5. Customer đã đăng nhập chỉ được `customer_get_menu` và `get_my_orders`; guest chỉ được menu read-only (`AiPermissionService.cs:5-69`).
6. Admin được dùng các tool đã allowlist: `get_revenue`, `get_business_summary`, `get_revenue_comparison`, `get_best_sellers`, `get_order_list`, `customer_get_menu`, `update_product_price`, `update_product_name`, `create_product`, `create_topping` (`AiPermissionService.cs:19-33`).
7. Tool registry hiện đăng ký 11 `IAiTool` implementation trong `Program.cs:327-341`: 9 nhóm admin/customer read/write nêu trên cộng `get_my_orders` và `customer_get_menu` theo registration thực tế.
8. Argument parsing, slot resolution, catalog lookup và response formatting nằm trong pilot/tool/formatter; không có evidence cho planner nhiều bước, memory dài hạn, function calling của LLM hoặc sinh câu trả lời tự do.

### 8.3. Write flow và confirmation

| Write flow | Đường thực thi | Trạng thái |
|---|---|---|
| Đổi giá | `UpdateProductPriceTool` → `ProductService.UpdatePriceByIdAsync` với expected current price | `IMPLEMENTED` ở source |
| Đổi tên | `UpdateProductNameTool` → `ProductService.UpdateNameByIdAsync` với expected current name | `IMPLEMENTED` ở source |
| Tạo món | `CreateProductTool` → `ProductService.CreateProductAsync` | `IMPLEMENTED` ở source |
| Tạo topping | `CreateToppingTool` → `ToppingService.CreateToppingAsync` | `IMPLEMENTED` ở source |

Confirmation stores (`ProductPriceConfirmationStore`, `ProductNameConfirmationStore`, `CatalogCreationConfirmationStore`) dùng token ngẫu nhiên, ràng buộc user ID, thời hạn mặc định 5 phút và consume một lần. Đây là state in-memory của process, không phải conversation store bền vững. `ProductService` có kiểm tra expected price/name để tránh ghi đè giá trị đã thay đổi; không được mở rộng kết luận này thành đảm bảo đồng thời cho toàn bộ hệ thống.

### 8.4. Giới hạn nhiều bước

`AiRequest.History` chỉ nhận tối đa 6 message và mỗi content tối đa 1000 ký tự (`AiController.cs:79-83,194-196`). Frontend gửi một phần lịch sử hội thoại; không tìm thấy `DbSet`/entity lưu conversation. Write flow nhiều lượt hiện là state confirmation ngắn hạn, không phải agent planner có memory dài hạn.

## 9. Chức năng ngoài phạm vi hoặc chưa đủ bằng chứng

| Nội dung | Kết quả audit | Bằng chứng/kết luận dùng cho Chương 1 |
|---|---|---|
| Đối soát ngân hàng tự động | `NOT_SUPPORTED` | Có payment method cash/transfer và UI VietQR; không tìm thấy webhook, bank transaction reader hoặc reconciliation service |
| Quản lý/tính lương | `NOT_SUPPORTED` ở runtime hiện hành | `Employee.BasicSalary` và UI field còn tồn tại, nhưng payroll tables/service đã bị loại bởi migration `20260919075618_RemovePayrollAndRepairReceiptSettings.cs`; không có payroll controller/service hiện tại |
| Tự trừ nguyên liệu theo công thức món | `NOT_SUPPORTED` | Có inventory độc lập; không có `ProductIngredient`, recipe mapping hoặc đường Order → stock consumption. `DashboardService.cs:107` còn phân biệt cost recording với recipe/stock consumption |
| Lưu hội thoại AI lâu dài | `NOT_SUPPORTED` | Có request `History` giới hạn; không có conversation entity/DbSet/service lưu hội thoại |
| LLM suy luận | `NOT_SUPPORTED` trong runtime hiện tại | Không có provider/SDK/cloud HTTP call; runtime dùng E5 embedding + rule/slot/allowlist logic |
| Sentence embedding trong ứng dụng | `IMPLEMENTED` ở source; UAT runtime `UNVERIFIED` trong lượt này | `AdminEmbeddingPilot`/`CustomerGuestEmbeddingPilot` được DI và có ONNX/tokenizer load path; cần artifact/config và runtime để xác minh deployment cụ thể |
| BusinessInsight module độc lập | `REMOVED` | `20261019104042_DropBusinessInsights.cs`; dashboard summary/revenue vẫn còn qua `DashboardService` |
| Test tự động hiện hành | `UNVERIFIED`/`NOT RUN` | Working tree không có thư mục `tests/`; checklist manual có trong `ba-test-frontend`; không coi checklist là kết quả test |
| Hiệu năng, độ chính xác AI, hiệu quả kinh doanh | `UNVERIFIED` | Không có kết quả benchmark/UAT được chạy trong lượt audit; không đưa số liệu vào Chương 1 |

## 10. Đối chiếu với Chương 1 hiện tại

### 10.1. Khả năng đối chiếu trực tiếp

Bản Word Chương 1 và các audit cũ trong `bao-cao/05-audit-hoc-thuat/` đang là file bị xóa trong working tree tại thời điểm audit. Vì vậy:

- không được coi nội dung Word cũ là bản Chương 1 hiện tại;
- không thể đánh dấu một câu cụ thể trong Word là đúng/sai bằng source trực tiếp;
- lịch sử `CHUONG-1-AUDIT.md` từ HEAD cho thấy Chương 1 cũ từng có các ý về LLM/Function Calling, Gemini, payroll và kiểm thử tự động, nhưng đây chỉ là nguồn lịch sử để cảnh báo, không phải bằng chứng hiện hành;
- ChatGPT chỉ được xem bảng dưới đây như hướng đối chiếu cần thực hiện khi bản Word hiện hành được cung cấp lại.

### 10.2. Ma trận đối chiếu theo mục 1.1–1.6

| Mục Word | Nhận định hiện tại | Kết quả audit | Mức xác minh | Bằng chứng | Hướng xử lý |
|---|---|---|---|---|---|
| 1.1 Lý do chọn đề tài | Bài toán đa chi nhánh, QR, POS, KDS, thanh toán, vận hành và nhu cầu hỗ trợ tự nhiên là phù hợp với phạm vi source | `PARTIAL` | `INSPECTED` source; nội dung Word `UNVERIFIED` | `DigitalMenu.tsx`; `ManagementRoutes.tsx`; `OrderController.cs`; `KitchenService.cs`; `DashboardService.cs` | Viết lý do như bài toán kỹ thuật/nghiệp vụ của đề tài; không thêm số liệu thị trường hoặc lợi ích định lượng nếu chưa có nguồn |
| 1.2 Mục tiêu nghiên cứu | Có thể đặt mục tiêu xây dựng hệ thống đa chi nhánh và tích hợp AI tool theo quyền/xác nhận; không được viết mục tiêu thành LLM đã tích hợp | `PARTIAL` | `INSPECTED` source | `Program.cs`; `AiOrchestrator.cs`; `AdminEmbeddingPilot.cs`; frontend routes | Phân tách mục tiêu hệ thống, mục tiêu AI, mục tiêu kiểm chứng; embedding là cơ chế hiện tại, không gọi là LLM |
| 1.3 Nhiệm vụ nghiên cứu | Có đủ nhóm nhiệm vụ phân tích nghiệp vụ, thiết kế, cài đặt, tích hợp và kiểm thử thủ công | `PARTIAL` | `INSPECTED` source; test result `UNVERIFIED` | Controllers/services/entities; `ba-test-frontend/README.md` | Diễn đạt là nhiệm vụ đã thực hiện/nghiên cứu; không khẳng định mọi ca đã chạy |
| 1.4 Đối tượng và phạm vi | Đối tượng gồm hệ thống quản lý đa chi nhánh, các role, QR/POS/KDS và AI tool runtime | `IMPLEMENTED` cho phạm vi source | `INSPECTED` | `RoleNames.cs`; `App.tsx`; controllers; `AiPermissionService.cs` | Ghi rõ ngoài phạm vi: bank reconciliation, payroll, recipe-based stock deduction, persistent AI memory, LLM |
| 1.5 Phương pháp nghiên cứu | Phù hợp với khảo cứu tài liệu, phân tích nghiệp vụ, thiết kế phần mềm, cài đặt theo tầng và manual UAT | `PARTIAL` | `INSPECTED` source; Word `UNVERIFIED` | source tree, migration, frontend checklist | Chỉ nêu phương pháp đã có căn cứ; nếu nói khảo sát thực địa/định lượng/benchmark phải cung cấp nguồn riêng |
| 1.6 Ý nghĩa khoa học và thực tiễn | Có ý nghĩa ứng dụng ở mô hình hóa nghiệp vụ, branch isolation, tool permission/confirmation và tích hợp embedding local | `PARTIAL` | `INSPECTED` source; hiệu quả thực tế `UNVERIFIED` | `AiPermissionService.cs`; `AiOrchestrator.cs`; services; branch/QR/KDS code | Dùng “đóng góp ứng dụng”, “hỗ trợ”, “hướng tới”; không khẳng định giảm chi phí/thời gian hay độ chính xác nếu chưa đo |

## 11. Những nhận định được phép và không được phép dùng

### 11.1. Có thể dùng khi viết Chương 1

- Hệ thống là nền tảng quản lý nhà hàng đa chi nhánh với hai frontend: một ứng dụng quản trị/POS/KDS và một ứng dụng customer/QR.
- Backend tập trung xử lý nghiệp vụ, lưu trữ, xác thực JWT, phân quyền branch và giao tiếp realtime với KDS.
- POS và KDS cùng thuộc `admin-web`; Customer Web phục vụ khách đăng nhập và QR guest.
- Hệ thống có các nhóm nghiệp vụ order, payment nội bộ, KDS, catalog, bàn/đặt bàn, customer/loyalty, nhân sự, inventory, expense và dashboard/reporting.
- AI là local sentence-embedding pilot chạy trên backend; decision được đưa qua orchestrator, registry, permission service và application service.
- Admin AI có read tools và write tools có confirmation cho giá, tên món, tạo món và tạo topping.
- Branch scope và QR guest context là các cơ chế cốt lõi để giới hạn dữ liệu.

### 11.2. Chưa được phép khẳng định

- Hệ thống sử dụng LLM, Gemini, OpenAI hoặc Function Calling của provider cloud.
- AI hiểu ngôn ngữ tự nhiên tổng quát hoặc có độ chính xác/hiệu năng cụ thể.
- Hệ thống đã đối soát giao dịch ngân hàng tự động.
- Hệ thống có payroll/tính lương hoàn chỉnh.
- Hệ thống tự động trừ nguyên liệu theo công thức món.
- AI lưu lịch sử hội thoại lâu dài hoặc có memory giữa các phiên.
- Tất cả API đã được chứng minh cách ly branch bằng runtime test.
- Tất cả nghiệp vụ đã PASS E2E hoặc đã được triển khai production.
- Hệ thống đã giảm thời gian, chi phí hoặc tăng doanh thu nếu chưa có số liệu đo.

## 12. Kiến thức bắt buộc cho từng mục 1.1–1.6

| Mục | Kiến thức ChatGPT phải sử dụng |
|---|---|
| 1.1 | Nêu vấn đề vận hành đa chi nhánh và liên thông QR → order → POS → KDS → payment; không biến nhận định thị trường thành số liệu khảo sát |
| 1.2 | Tách mục tiêu xây dựng hệ thống khỏi mục tiêu AI: embedding local, nhận diện intent/tool có kiểm soát, permission, confirmation và fail-closed |
| 1.3 | Trình bày chuỗi nhiệm vụ từ khảo sát/ phân tích nghiệp vụ → mô hình hóa → cài đặt backend/frontend → tích hợp AI → manual UAT; không nhầm nhiệm vụ với kết quả test |
| 1.4 | Nêu hai frontend, backend và các role; xác định rõ scope branch, QR guest và các chức năng ngoài phạm vi |
| 1.5 | Mô tả phương pháp phân tích source/nghiệp vụ, thiết kế theo tầng, phát triển service/API/frontend, migration và manual verification; chỉ thêm phương pháp có bằng chứng |
| 1.6 | Nêu ý nghĩa khoa học ở mức tích hợp/mô hình hóa ứng dụng và ý nghĩa thực tiễn ở mức hỗ trợ vận hành; không tuyên bố hiệu quả định lượng chưa đo |

## 13. Hình, bảng và công thức nên giữ hoặc sửa

Đây là khuyến nghị cho Chương 1, không phải chỉnh sửa Word.

| Loại | Khuyến nghị |
|---|---|
| Hình kiến trúc tổng quan | Giữ hoặc bổ sung sơ đồ `Admin Web / Customer Web → API → Application Service → EF Core/PostgreSQL`, kèm KDS/SignalR và AI tool path |
| Hình tác nhân | Giữ sơ đồ role `admin`, `manager`, `cashier`, `kitchen`, `employee`, `customer`, guest QR; thể hiện guest là session context, không phải role nhân viên |
| Hình luồng nghiệp vụ | Giữ một luồng QR/customer → order → POS/accept → KDS → payment → loyalty; ghi rõ nhánh gọi thêm món |
| Hình AI | Sửa để thể hiện `E5 embedding pilot → decision/slots → AiOrchestrator → registry → permission → tool → application service`; không vẽ LLM/provider cloud nếu source hiện tại không có |
| Bảng phạm vi chức năng | Nên có cột `Đã cài đặt`, `Một phần/giới hạn`, `Ngoài phạm vi` theo các bảng audit trong file này |
| Bảng vai trò | Giữ bảng role/scope; không gán quyền từ `Position` |
| Công thức tài chính | Không cần đưa vào Chương 1 nếu chỉ là tổng quan; nếu có đề cập thì chỉ nói backend tính subtotal/service fee/VAT/discount/total, để chi tiết sang chương thiết kế/cài đặt |
| Công thức AI/độ chính xác | Không đưa accuracy, threshold như kết quả khoa học nếu chưa có benchmark được xác minh; có thể mô tả embedding/score/margin ở chương kỹ thuật |

## 14. Handoff cho ChatGPT

- Có đủ bằng chứng source để viết lại phạm vi, mục tiêu, nhiệm vụ, đối tượng, phương pháp và ý nghĩa ở mức tổng quan.
- Phải dùng trạng thái “đã cài đặt trong source” thay cho “đã vận hành hoàn chỉnh” nếu chưa có runtime evidence tương ứng.
- Phải loại các mô tả cũ về provider LLM/cloud, payroll, bank reconciliation, recipe-based inventory deduction và persistent AI conversation nếu chúng xuất hiện trong Word.
- Phải giữ giới hạn rằng AI hiện là local E5 embedding pilot với intent/tool catalogue có kiểm soát; không gọi là LLM agent tổng quát.
- Phải ghi rõ bản Word Chương 1 hiện chưa thể đối chiếu trực tiếp trong working tree ở audit này. Muốn có bảng “đúng/sai theo từng câu Word”, cần cung cấp lại Word hoặc bản text hiện hành.
- File này chỉ là báo cáo bằng chứng audit, không phải bản Chương 1 và không được dùng như văn xuôi thay thế chương.
