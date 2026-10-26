# AUDIT CHƯƠNG 2 – CƠ SỞ LÝ THUYẾT VÀ CÔNG NGHỆ SỬ DỤNG

## 1. Phạm vi và nguồn bằng chứng

- **Phạm vi:** xác minh công nghệ, kiến trúc, mô hình dữ liệu, bảo mật, nghiệp vụ realtime và AI đang có trong mã nguồn hiện tại; không viết lại Chương 2.
- **HEAD được audit:** `9365c18614666ab4c80eee714403ef73b14edf54` – `Update repository README for current system`.
- **Thời điểm audit:** 20/10/2026, múi giờ Asia/Saigon.
- **Working tree:** không sạch. Có các file cũ bị xóa trong `bao-cao/05-audit-hoc-thuat/` và thư mục audit mới chưa commit. Không thấy thay đổi source được tạo trong lượt audit này.
- **Nguồn ưu tiên:** `services/api`, `apps/admin-web`, `apps/customer-web`, manifest/lockfile, DI trong `Program.cs`, entity/DbContext/migration, controller/service/hub và cấu hình hiện hành.
- **Phương pháp:** kiểm tra tĩnh source, import, dependency, đường gọi và mapping. Không chạy build, test, ứng dụng, migration, truy vấn database hoặc AI inference trong lượt này. Vì vậy các kết luận runtime/DB đã áp dụng được đánh dấu riêng là `UNVERIFIED`.
- **Tài liệu Chương 2 cũ:** bản audit lịch sử trong `bao-cao/05-audit-hoc-thuat/CHUONG-2-AUDIT.md` được dùng để nhận diện đề cương và các tuyên bố cũ, không được dùng thay cho source hiện tại.

## 2. Kết luận tổng quát

1. Hệ thống có backend ASP.NET Core 8, hai frontend React độc lập và PostgreSQL qua EF Core/Npgsql. POS và KDS là các khu vực trong Admin Web; QR ordering và guest chat nằm ở Customer Web.
2. Cấu trúc thư mục thể hiện kiến trúc phân lớp có ảnh hưởng từ Clean Architecture, nhưng chưa đủ căn cứ để gọi là tuân thủ đầy đủ. Nhiều application service đang inject trực tiếp `ApplicationDbContext`, tạo phụ thuộc từ Application vào Infrastructure.
3. API dùng controller/DTO/service; xác thực dùng JWT Bearer; phân quyền kết hợp role và branch context. SignalR được dùng cho kênh bếp, POS và thông báo.
4. Runtime AI hiện tại không dùng mô hình ngôn ngữ lớn, không gọi dịch vụ AI bên ngoài và không dùng `IAiProvider`. Hai pilot `AdminEmbeddingPilot` và `CustomerGuestEmbeddingPilot` được đăng ký trực tiếp; chúng dùng ONNX Runtime, tokenizer và catalog tham chiếu cục bộ để chọn intent/tool, sau đó tool gọi application service.
5. Sentence embedding **đã có đường thực thi trong pilot hiện tại**, nhưng việc artifact có đầy đủ trong môi trường triển khai và kết quả suy luận thực tế chưa được kiểm tra trong lượt này. Không được mô tả nó như LLM, Function Calling hoặc khả năng tự lập kế hoạch.
6. Migrations và EF model được kiểm tra từ source; schema PostgreSQL đang chạy và trạng thái migration đã apply chưa được xác minh bằng kết nối database.

## 3. Bảng công nghệ và phiên bản thực tế

| Thành phần | Bằng chứng hiện tại | Kết quả audit | Mức xác minh |
|---|---|---|---|
| .NET/ASP.NET Core | `services/api/RestaurantPOS.api.csproj:4`, target `net8.0`; `Program.cs` dùng WebApplication, Controllers, Middleware | Backend chạy trên nền ASP.NET Core 8 theo target framework | IMPLEMENTED / tĩnh |
| C# project | `RestaurantPOS.api.csproj` bật nullable và implicit usings | Có kiểm tra nullable ở mức compiler; không đồng nghĩa toàn bộ dữ liệu runtime không null | IMPLEMENTED |
| EF Core | `Microsoft.EntityFrameworkCore 8.0.31`, `ApplicationDbContext` | ORM và DbContext là persistence chính | IMPLEMENTED |
| PostgreSQL/Npgsql | `Npgsql.EntityFrameworkCore.PostgreSQL 8.0.11`, `Npgsql 8.0.9`, `UseNpgsql` tại `Program.cs:373-381` | PostgreSQL là provider cấu hình hiện tại; target thực tế cần DB attestation/runtime để xác nhận | IMPLEMENTED / DB UNVERIFIED |
| JWT | `Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0`; `Program.cs:112-157` | Validate chữ ký, issuer, audience, lifetime; có hỗ trợ token cho SignalR | IMPLEMENTED |
| SignalR server | `AddSignalR`, `MapHub<KitchenHub>("/kitchenHub")` | Realtime cho bếp và một số màn admin/POS | IMPLEMENTED |
| Swagger | `Swashbuckle.AspNetCore 6.4.0`; bật Development | API explorer/Swagger dùng cho Development, không phải bằng chứng test nghiệp vụ | IMPLEMENTED |
| ONNX Runtime | `Microsoft.ML.OnnxRuntime 1.27.1`; `AdminEmbeddingPilot`/`CustomerGuestEmbeddingPilot` | Inference local trong pilot AI hiện tại | IMPLEMENTED trong code; artifact/runtime UNVERIFIED |
| Tokenizers | `Tokenizers.HuggingFace 3.23.1`; pilot gọi `Tokenizer.FromFile` | Tokenizer được đọc từ file cấu hình | IMPLEMENTED trong code |
| Admin Web | React `^18.2.0`, lock resolve 18.3.1; Vite lock 4.5.14; TypeScript lock 5.9.3; Tailwind lock 3.4.19; React Router DOM 7.18.3 | SPA riêng cho admin/manager/cashier/employee/kitchen | IMPLEMENTED / version từ lockfile |
| Customer Web | React `^18.2.0`, cùng lock major; Vite/TypeScript/Tailwind/React Router như trên | SPA cho khách đăng ký và guest QR | IMPLEMENTED |
| Admin SignalR client | `@microsoft/signalr 10.0.11`; `KitchenPage`, `POSPage`, `useNotificationCenter` | Client kết nối `/kitchenHub`, có access token và reconnect ở các đường đã kiểm | IMPLEMENTED |
| Biểu đồ | Admin import `recharts` trong `Dashboard.tsx` | Dùng cho dashboard, không phải thư viện dùng chung hai frontend | IMPLEMENTED |
| QR camera | Admin import `jsqr` trong `EmployeeAttendance.tsx`; Customer `QRScan.tsx` gọi `window.jsQR` nhưng manifest Customer không khai báo `jsqr` | Admin có dependency trực tiếp; Customer có dấu hiệu phụ thuộc runtime/global cần kiểm tra build/runtime | PARTIAL / UNVERIFIED |
| API types | Root `openapi-typescript ^7.12.0`, lock resolve 7.13.0; script codegen | Có cơ chế sinh type từ Swagger, nhưng không chứng minh mọi màn hình đang dùng generated type | PARTIAL |
| Test runner hiện hành | Root README nêu không có thư mục `tests/`; không thấy Playwright/xUnit trong cấu hình hiện tại | Kiểm thử hiện hành chủ yếu là manual UAT/checklist | INSPECTED; test NOT RUN |

## 4. Backend và kiến trúc thực tế

### 4.1. Tổ chức lớp và dependency injection

| Lớp/thành phần | Bằng chứng | Nhận định |
|---|---|---|
| Domain | `services/api/src/Domain` chứa Product, Order, Customer, Branch, Inventory và các khái niệm nghiệp vụ | Có vùng domain riêng; chưa audit toàn bộ file để chứng minh domain hoàn toàn độc lập với framework | PARTIAL |
| Application | `src/Application/Services` chứa `OrderService`, `ProductService`, `InventoryService`, `LoyaltyService`, `DashboardService` và interface/DTO | Application service là nơi giữ phần lớn use case và quy tắc nghiệp vụ | IMPLEMENTED |
| Infrastructure | `ApplicationDbContext`, migrations, `JwtService`, persistence và external implementations | Có lớp hạ tầng riêng | IMPLEMENTED |
| WebAPI | Controllers, middleware, `KitchenHub`, `Program.cs` | Composition root và HTTP/realtime entry point | IMPLEMENTED |
| AI | `src/AI` gồm controller, orchestrator, registry, security, tools và pilot services | AI là module riêng nhưng vẫn đi qua application services khi thao tác dữ liệu | IMPLEMENTED |
| DI | `Program.cs:281-341` đăng ký application services, security, pilot singleton, orchestrator scoped, registry và tools | Có dependency injection rõ ràng; scope/lifetime được khai báo tập trung | IMPLEMENTED |
| Persistence dependency | Nhiều application service inject trực tiếp `ApplicationDbContext` | Đây là điểm phụ thuộc Application → Infrastructure; không nên khẳng định Clean Architecture thuần hoặc Dependency Inversion đầy đủ | PARTIAL |

### 4.2. API, DTO và validation

- API được tổ chức bằng nhiều controller theo bounded area: `Auth`, `Product`, `Topping`, `Order`, `Inventory`, `Dashboard`, `Employee`, `Attendance`, `Shift`, `Reservation`, `Table`, `SystemSettings`, `ReceiptSettings`, `Notification`, `Branch`, `Guest` và các controller liên quan.
- Controller gọi application service; AI tool cũng gọi application service. Không có bằng chứng AI truy cập trực tiếp EF Core.
- DTO/request được dùng ở các đường order, catalog, auth, inventory, settings và AI confirmation. Validation hiện diện ở controller/service/tool tùy use case; không có căn cứ để mô tả một framework validation tập trung áp dụng cho mọi DTO.
- Swagger/OpenAPI được đăng ký trong Development. Đây là tài liệu/khám phá API, không phải bằng chứng mọi endpoint đã được kiểm thử.

### 4.3. SignalR và realtime

- Hub: `/kitchenHub` (`Program.cs:537`, `KitchenHub.cs`).
- `KitchenHub` yêu cầu role admin/manager/employee/cashier/kitchen, tạo group theo branch/role và user notification. Admin có cách chọn scope riêng; các role khác lấy branch từ JWT.
- `KitchenNotifier`, `KitchenService` và `OrderService` là nhóm producer; `KitchenPage`, `POSPage` và `useNotificationCenter` là consumer đã thấy trong source.
- SignalR group hỗ trợ phân phối, nhưng bản thân group không được xem là thay thế cho authorization. Việc delivery realtime thành công ở môi trường chạy thật chưa được kiểm chứng trong lượt này.

### 4.4. Đánh giá kiến trúc

Kết luận phù hợp để đưa vào cơ sở lý thuyết là **kiến trúc phân lớp lấy cảm hứng từ Clean Architecture, có DI và tách module**, không phải tuyên bố “tuân thủ hoàn toàn Clean Architecture”. Lý do chính là application service còn phụ thuộc trực tiếp vào `ApplicationDbContext`; dependency inversion chưa được thực hiện triệt để qua abstraction persistence. Đây là `PARTIAL`, không phải lỗi làm mất giá trị của phân lớp hiện có.

## 5. Frontend thực tế

### 5.1. Admin Web

- `App.tsx` phân nhánh theo trạng thái đăng nhập/role và đưa vào các route group: `ManagementRoutes`, `CashierRoutes`, `StaffRoutes`, `KitchenRoutes`.
- Management route chứa dashboard, catalog sản phẩm/topping, POS, KDS, inventory, order/invoice, customer, reservation/table, promotion, expense, employee/attendance/shift/schedule, branch/settings/receipt/profile.
- Cashier, employee và kitchen có route/layout/navigation riêng; các route bị cấm chuyển tới `ForbiddenPage` hoặc redirect theo vai trò.
- React Router DOM được dùng cho route; Tailwind utility classes và các component UI nội bộ được dùng cho giao diện; `lucide-react` cho icon; `recharts` cho dashboard.
- API calls chủ yếu là `fetch`/service functions trong từng feature; chưa có bằng chứng một package API client chung duy nhất cho cả hai frontend.
- JWT token được đọc từ storage và gửi trong request; frontend guard chỉ là UX, còn bảo mật phải do backend kiểm tra.
- SignalR được dùng ở KDS, POS và notification center.

### 5.2. Customer Web

- `App.tsx` dùng BrowserRouter với các route `/`, `/reservation`, `/profile`, `/scan`; người dùng đăng nhập và guest QR đi qua các nhánh UI khác nhau.
- `DigitalMenu`, giỏ hàng, product options, order confirmation, reservation, profile, QR scan và chatbot là các nhóm chức năng thấy trong source.
- `App.tsx` bọc/điều chỉnh `fetch` để gắn customer/guest token từ local/session storage và xử lý 401 bằng cách xóa session.
- Customer Web dùng React Router, Tailwind và `lucide-react`; không thấy import SignalR client trong phần đã rà.
- QR scan dùng camera và `window.jsQR` trong `QRScan.tsx`, nhưng `apps/customer-web/package.json` không khai báo `jsqr`; cần kiểm tra bundle/runtime trước khi mô tả đây là dependency được quản lý hoàn chỉnh.

### 5.3. Chia sẻ frontend

Có `apps/shared/ReceiptPreview.tsx` và `packages/shared` cho Swagger/type generation, nhưng chưa có bằng chứng hai frontend đang cùng import một bộ component UI hoặc một API client dùng chung. Không nên mô tả monorepo là “shared component architecture” nếu chỉ dựa vào việc cùng nằm trong repository.

## 6. Cơ sở dữ liệu và giao dịch

### 6.1. EF model và migration

- `ApplicationDbContext` khai báo DbSet cho Product, ProductBranchAvailability, Order, OrderDetail, OrderRequest, OrderRequestItem, Topping, Customer, Branch, Employee, Tables, Reservation, InventoryItem, BranchInventory, stock receipt/issue/transaction, loyalty, notification, settings và các thực thể liên quan.
- Mapping trong `ApplicationDbContext` định nghĩa quan hệ, khóa, index và các ràng buộc; `ApplicationDbContextModelSnapshot.cs` phản ánh model tại thời điểm migration gần nhất.
- Migrations hiện có các nhóm cho topping, kitchen request/history, loyalty, inventory, QR token bàn, product branch availability và các thay đổi loại bỏ business insight/payroll cũ.
- EF model, migration history và schema PostgreSQL đã apply là ba lớp bằng chứng khác nhau. Lượt audit này chưa truy cập `__EFMigrationsHistory` hoặc schema database thật, nên trạng thái áp dụng migration là `UNVERIFIED`.

### 6.2. Quan hệ và đa chi nhánh

- Product là metadata/giá dùng chung; `ProductBranchAvailability` biểu diễn trạng thái bán theo chi nhánh.
- Order có `BranchId`; các dữ liệu tồn kho, settings, receipt và nhiều nghiệp vụ vận hành có scope branch.
- JWT chứa `branchId` khi phù hợp; controller/service/context kiểm tra resource branch trong nhiều luồng. Admin là global scope theo thiết kế hiện hành.
- Cần phân biệt “global catalog” với “khả dụng tại chi nhánh”: sản phẩm có thể tồn tại trong catalog nhưng branch availability ở trạng thái tạm thời chưa bán.

### 6.3. Index, unique và transaction

- Index/unique/FK được cấu hình trong DbContext và migrations; ví dụ branch availability và các quan hệ inventory/order được mapping trong Infrastructure. Khi viết số lượng/ràng buộc cụ thể cần trích đúng mapping/migration, không suy từ tên property.
- Một số luồng dùng `Database.CreateExecutionStrategy()` kết hợp transaction, gồm order, product creation, loyalty và kitchen service. Đây là bằng chứng theo từng use case, không phải cam kết mọi thao tác trong hệ thống đều transactional.
- Npgsql retry strategy ảnh hưởng cách dùng transaction; code hiện tại có dấu hiệu đã bao quanh các luồng liên quan bằng execution strategy. Chưa chạy runtime để xác nhận mọi nhánh, đặc biệt nhánh tạo catalog và topping.

### 6.4. Giới hạn inventory

Inventory hiện có danh mục tồn, tồn theo branch, phiếu nhập/xuất và giao dịch. Chưa thấy quan hệ `ProductIngredient`/recipe hoặc đường tự động trừ nguyên liệu khi order. Vì vậy không được trình bày inventory hiện tại như hệ thống định lượng nguyên liệu theo công thức món.

## 7. Xác thực và phân quyền

| Cơ chế | Bằng chứng | Kết luận |
|---|---|---|
| JWT Bearer | `Program.cs:112-132`; `JwtService.cs:21-95` | Token ký HMAC, có issuer/audience, expiration; validation bật lifetime và clock skew bằng 0 | IMPLEMENTED |
| Claims | `JwtService.cs:44-81` | Có subject/name/role/fullName, branchId, branchName, position, customer session/auth method, tableId tùy loại token | IMPLEMENTED |
| Token expiry | `JwtService.cs:23-24`, guest lifetime giới hạn tối đa 2 giờ | Token thường lấy `Jwt:ExpiryDays`, mặc định code 7 ngày nếu không có cấu hình; guest có lifetime riêng | IMPLEMENTED trong code; cấu hình triển khai UNVERIFIED |
| Role authorization | `[Authorize(Roles=...)]` ở controller/hub; `RoleNames` | Có admin, manager, employee, cashier, kitchen, customer | IMPLEMENTED |
| Branch authorization | branch claim, `GuestSessionContext`, inventory/user context, controller/service checks | Có branch-scoped checks ở nhiều đường; không được suy ra mọi service đều tự authorize độc lập | IMPLEMENTED / phạm vi theo use case |
| Password | `PasswordHasher<Employee>`, `PasswordHasher<Customer>` đăng ký tại `Program.cs:298-300` | Có password hashing/verifier; legacy compatibility cần mô tả thận trọng, không khẳng định mọi bản ghi hiện tại đều đã hash | IMPLEMENTED / DB UNVERIFIED |
| Account inactive | Auth flow kiểm tra trạng thái tài khoản nhân viên/customer khi đăng nhập | Có đường từ chối tài khoản không hoạt động; không phải cơ chế revocation toàn bộ JWT sau khi phát hành nếu chưa có evidence khác | PARTIAL |
| Customer đăng ký và Guest | `customer-token`, `GuestController`, `GuestSessionContext` | Guest dùng role `customer` kèm `customerSessionType`; không có role JWT riêng tên guest | IMPLEMENTED |
| AI authorization | `AiPermissionService`, `AiAuthorization`, confirmation stores | Tool AI có role/risk/branch boundary riêng; write cần admin và confirmation ở các công cụ hiện có | IMPLEMENTED |

## 8. Lý thuyết nghiệp vụ được phép trình bày

| Chủ đề | Thành phần thực tế | Kết quả audit |
|---|---|---|
| POS | Admin Web có `POSPage`, order/cart/payment/table flow; backend `OrderController` và `OrderService` | IMPLEMENTED; lý thuyết POS nên giới hạn ở bán hàng, order, thanh toán và vận hành hiện có |
| KDS | `KitchenPage`, `KitchenHistoryPage`, `KitchenHub`, `KitchenService`, `OrderRequest` | IMPLEMENTED; request chế biến và trạng thái bếp là phần riêng trong đường order |
| QR ordering | Customer Web `QRScan`, `DigitalMenu`, guest bootstrap/session, bàn/QR token | IMPLEMENTED; không đồng nghĩa mọi QR payment hay QR ngoài phạm vi đều có |
| Order và yêu cầu bếp | `Order`, `OrderDetail`, `OrderRequest`, `OrderRequestItem`, sent quantity/status | IMPLEMENTED |
| Thanh toán | Order payment endpoint, persisted order total, payment method, receipt/invoice; VietQR UI/branch bank information | IMPLEMENTED ở payment flow/display; chưa thấy webhook ngân hàng, đối soát tự động hoặc gateway settlement |
| Loyalty | `LoyaltyService` balance/earn/redeem/refund/history; order flow gọi earn theo điều kiện | IMPLEMENTED |
| Inventory | inventory item, branch inventory, receipt/issue/transaction/min stock | IMPLEMENTED một phần theo kho; không có recipe-based consumption |
| Nhân sự/ca/chấm công | Employee, Attendance, Shift, WorkSchedule, role route/controller | IMPLEMENTED; payroll không còn là module hiện hành |
| Realtime | SignalR hub/group/notification clients | IMPLEMENTED trong code; delivery/latency chưa kiểm |
| Đa chi nhánh | Branch entity, branch claims, branch availability, scoped settings/inventory/orders, hub groups | IMPLEMENTED ở nhiều module; không khẳng định mọi dữ liệu đều đã được audit branch isolation |

## 9. Audit AI hiện tại

### 9.1. Provider và entry point

- `Program.cs:305-306` đăng ký trực tiếp `AdminEmbeddingPilot` và `CustomerGuestEmbeddingPilot` singleton.
- `Program.cs:308-316` tạo `AiOrchestrator` scoped và truyền hai pilot vào orchestrator.
- Không tìm thấy `IAiProvider`, `MockAiProvider` hoặc provider HTTP/external trong đường source hiện hành. Các tên cũ chỉ có thể xuất hiện trong tài liệu/lịch sử và phải đánh dấu `REMOVED` khi đối chiếu.
- Entry point chính là `AiController`: admin/customer chat qua `/api/Ai/chat`; guest QR có entry point riêng với điều kiện session/flag. Admin chat yêu cầu admin; customer registered và guest bị giới hạn theo context.

### 9.2. Embedding pipeline đã thấy trong code

`AdminEmbeddingPilot` và customer pilot có các bước được xác minh tĩnh:

1. Đọc model ONNX, tokenizer JSON và reference catalog từ configuration.
2. Tokenize với prefix `query: `; tạo `input_ids`, `token_type_ids`, `attention_mask`, giới hạn tối đa 512 token.
3. Chạy `InferenceSession` của ONNX Runtime.
4. Lấy output token embedding, mean-pooling theo attention mask với chiều 384.
5. L2-normalize vector.
6. So điểm bằng dot product/cosine trên vector đã normalize; chọn score cao nhất và tính margin so với ứng viên thứ hai.
7. So với score/margin threshold; kết hợp nhận diện lexical/slot boundary để quyết định `ACCEPT`, `CLARIFY` hoặc `UNSUPPORTED`.
8. Parse các slot như period, status, product name, new price/name, category/group/topping type; tool chỉ được gọi sau khi slot hợp lệ.

Bằng chứng: `AdminEmbeddingPilot.cs:68-116`, `366-435`, `473-501`; DI tại `Program.cs:302-341`; cấu hình tại `appsettings.Development.json:25-50`.

Điểm cần giới hạn:

- Threshold mặc định admin là score `0.70`, margin `0.00`; customer margin `0.015`; timeout mặc định 1500 ms. Đây là cấu hình hiện tại, không phải độ chính xác AI.
- Cấu hình Development hiện để hai pilot `Enabled=false`; việc môi trường UAT đang bật flag và có đủ artifact phải được kiểm riêng.
- Catalog/model/tokenizer nằm ngoài source chính trong đường `.tools/...`; hash/provenance, license, version model và khả năng load trong môi trường audit chưa được kiểm chứng ở lượt này.
- Embedding chỉ hỗ trợ định tuyến/đánh giá tương đồng; không tự sinh văn bản, không chứng minh LLM reasoning, không tự trích tham số ngoài parser đang viết, không phải Function Calling của một LLM.

### 9.3. Tool contract, quyền và confirmation

- `IAiTool` có `Name`, `Description`, `AllowedRoles`, `RiskLevel`, schema JSON và `ExecuteAsync`.
- Risk enum có `Read`, `Write`, `Destructive`; tool registry hiện đăng ký read tools cho báo cáo/menu/order và write tools cho đổi giá, đổi tên, tạo product, tạo topping.
- `AiToolRegistry` nhận danh sách tool qua DI và tra cứu theo tên; schema không nên được mô tả là schema động do một LLM sinh ra.
- `AiPermissionService` và `AiAuthorization` kiểm role/risk/branch. Customer chỉ được nhóm read phù hợp; admin được các catalog/report tools; tool write bị giới hạn admin.
- Write catalog/price/name dùng confirmation store in-memory, ràng buộc user, loại thao tác, thời hạn khoảng năm phút và consume một lần. Các service vẫn kiểm dữ liệu chính thức, duplicate/name normalization, expected current price/name hoặc transaction tùy use case.
- `AiOrchestrator` có giới hạn vòng xử lý và xử lý `CLARIFY`/`UNSUPPORTED`; response được formatter theo role/tool. Không có bằng chứng về agent tự lập kế hoạch mở rộng hoặc gọi tool tùy ý.
- Lỗi model/catalog/AI được hướng tới fail-closed; không fallback provider cũ. Chưa chạy kiểm thử lỗi trong lượt audit.

### 9.4. Phân loại AI

| Thành phần/ý tưởng | Trạng thái | Cách viết được phép |
|---|---|---|
| Local E5 sentence-embedding pilot | IMPLEMENTED trong code path khi flag bật và artifact hợp lệ | Mô tả là bộ định tuyến ý định cục bộ có embedding, không phải LLM |
| Admin/customer pilot | IMPLEMENTED / điều kiện môi trường | Hai boundary khác nhau về role, scope, catalog và intent |
| `IAiProvider`/Mock provider cũ | REMOVED khỏi runtime hiện hành | Chỉ nêu trong audit lịch sử nếu cần đối chiếu, không đưa vào công nghệ đang dùng |
| External AI API/Gemini | NOT_SUPPORTED trong code path hiện hành | Không viết là provider tích hợp |
| LLM Function Calling | DESIGN_ONLY/không phải cơ chế hiện hành | Chỉ nêu như khái niệm so sánh nếu thực sự cần, không gán cho implementation |
| Multi-step autonomous planning | NOT_SUPPORTED/UNVERIFIED | Không suy ra từ loop limit hoặc orchestrator |
| Long-term conversation memory | NOT_SUPPORTED trong source đã rà | History request ngắn hạn khác với lưu hội thoại lâu dài |

## 10. Môi trường phát triển và kiểm thử

- API target `.NET 8`; frontend dùng Node/npm/Vite theo manifest và lockfile. Runtime Node/.NET thực tế của máy chưa được ghi nhận trong lượt này.
- Kestrel cấu hình HTTPS port 5000 trong `Program.cs`; script `scripts/setup-https.ps1` phục vụ certificate. Root README mô tả Admin Web 5173, Customer Web 5174 và LAN host cấu hình qua environment/config.
- CORS đọc `Cors:AllowedOrigins`, chỉ cho các origin được liệt kê và `AllowCredentials`; không dùng `AllowAnyOrigin` cùng credentials. Tên policy là `AllowAll` nhưng hành vi hiện tại không phải wildcard; khi viết báo cáo nên mô tả hành vi, không dựa vào tên.
- Connection string PostgreSQL/Supabase phải đến từ secure configuration; không có fallback database local trong đường startup đã rà.
- Automatic migration/seeding bị tắt mặc định và chỉ cho phép opt-in Development qua `DatabaseSafety:AllowAutomaticMigration`. Đây là cơ chế an toàn startup, không phải bằng chứng schema đã được apply.
- Root README hiện ghi manual UAT là phương thức xác minh chính; không có thư mục test hiện hành hoặc Playwright/xUnit suite được xác minh trong lượt này. Các checklist thủ công chỉ là bằng chứng quy trình/ghi nhận, không tự biến thành kết quả PASS.

## 11. Đối chiếu Chương 2 mục 2.1–2.6

Đề cương lịch sử được xác minh gồm: khái niệm/nghiệp vụ cơ bản; phương pháp và công cụ phân tích thiết kế; kiến trúc/kỹ thuật phát triển; database/bảo mật/môi trường và AI được phân bổ trong các tiểu mục cũ. Bản Word nguồn hiện không còn trong working tree audit và không được dùng làm source hiện hành, nên phát hiện dưới đây là đối chiếu theo chủ đề; mọi câu cụ thể ngoài hồ sơ lịch sử phải được ChatGPT đối chiếu lại bản Word gốc.

| Mục | Nhận định cần giữ/sửa | Kết quả audit | Hướng xử lý |
|---|---|---|---|
| 2.1. Khái niệm và nghiệp vụ cơ bản | POS, KDS, QR ordering, order, loyalty và đa chi nhánh liên quan trực tiếp | IMPLEMENTED; inventory và HR cũng có trong source | Giữ lý thuyết nền; giới hạn theo nghiệp vụ thực tế; thêm inventory vừa mức; không mở rộng sang payroll/đối soát ngân hàng |
| 2.2. Phân tích/thiết kế và công cụ | UML/Draw.io/VS Code là lý thuyết/công cụ tài liệu, không phải bằng chứng runtime | UNVERIFIED cho việc tác giả thực sự dùng từng công cụ trong checkout hiện tại | Chuyển phần mô tả công cụ sang mức phương pháp; cần bằng chứng file sơ đồ hoặc lịch sử nếu muốn khẳng định đã sử dụng |
| 2.3. Kiến trúc và kỹ thuật backend/frontend | REST/controller/service/EF/React/Vite/TypeScript/Tailwind/SignalR phù hợp | IMPLEMENTED; Clean Architecture chỉ PARTIAL | Dùng thuật ngữ “kiến trúc phân lớp lấy cảm hứng Clean Architecture”; không khẳng định dependency inversion toàn diện |
| 2.4. Cơ sở dữ liệu và bảo mật | PostgreSQL, EF migration, JWT, role/branch auth có căn cứ | IMPLEMENTED ở code; schema applied và mọi branch path chưa kiểm runtime | Phân biệt model/migration/schema; bổ sung JWT/password/authorization; không khẳng định DB thật hoặc mọi endpoint đã test |
| 2.5. AI agent và orchestration | Có orchestrator, registry, tool schema, permission, confirmation và local embedding pilot | IMPLEMENTED có điều kiện; external LLM/provider cũ REMOVED | Viết embedding pipeline đúng source; bỏ Gemini/Mock provider cũ khỏi mô tả hiện hành; không gán Function Calling/autonomy |
| 2.6. Môi trường và kiểm thử | HTTPS, CORS, PostgreSQL/Supabase config và manual UAT có căn cứ | IMPLEMENTED/PARTIAL; không có bằng chứng test runner hiện hành hoặc runtime result trong lượt này | Chỉ nêu versions theo manifest/lockfile; ghi test là manual/NOT RUN trong audit; không dùng kết quả lịch sử nếu không gắn môi trường/HEAD phù hợp |

### 11.1. Thuật ngữ cần chuẩn hóa

| Không nên dùng hoặc dễ gây hiểu sai | Nên dùng |
|---|---|
| “Clean Architecture hoàn toàn” | “Kiến trúc phân lớp lấy cảm hứng từ Clean Architecture; còn phụ thuộc trực tiếp DbContext ở application service” |
| “AI dùng Gemini/LLM” | “AI pilot embedding cục bộ dùng ONNX Runtime và tokenizer khi được bật” |
| “AI Function Calling tự động” | “Orchestrator chọn tool từ catalog, kiểm slot/quyền và gọi tool đã đăng ký” |
| “Guest là một role JWT riêng” | “Guest session dùng role customer kèm claim/session type và branch/table context” |
| “Database đã migrate” | “EF model/migration hiện có; trạng thái schema đã apply cần kiểm tra database” |
| “Inventory tự trừ nguyên liệu theo món” | “Inventory quản lý tồn/phiếu/giao dịch; chưa thấy recipe-based consumption” |
| “Realtime bảo đảm delivery” | “Có đường SignalR producer/consumer và group; delivery runtime chưa được audit” |
| “Độ chính xác 70%” từ threshold `0.70` | “Score threshold cấu hình 0.70; chưa phải accuracy” |
| “Lưu lịch sử hội thoại” | “Request có history ngắn hạn; chưa thấy persistence hội thoại lâu dài” |

## 12. Nội dung ngoài phạm vi hoặc chưa được phép khẳng định

| Nội dung | Phân loại | Căn cứ/giới hạn |
|---|---|---|
| Đối soát ngân hàng tự động | NOT_SUPPORTED | Chưa thấy webhook/reconciliation service trong source rà soát |
| Quản lý lương/payroll hiện hành | REMOVED / NOT_SUPPORTED | Migration hiện hành có dấu hiệu loại bỏ payroll; không thấy module runtime tương ứng |
| Tự trừ nguyên liệu theo công thức | NOT_SUPPORTED | Không thấy Product–Ingredient/recipe và order deduction path |
| Lưu hội thoại AI lâu dài | NOT_SUPPORTED | Có history request ngắn và audit sink, không thấy conversation repository |
| Suy luận bằng LLM sinh văn bản | NOT_SUPPORTED | Không có external model/provider trong runtime hiện tại |
| Sentence embedding trong pilot | IMPLEMENTED có điều kiện | Code có ONNX/tokenizer/pooling/normalization; artifact, flag và inference runtime cần kiểm riêng |
| Schema PostgreSQL đã áp dụng đầy đủ | UNVERIFIED | Chưa truy cập database/migration history |
| Hiệu năng, accuracy, latency, uptime | UNVERIFIED | Không chạy benchmark/measurement trong lượt audit |
| Tất cả branch-sensitive endpoint đều đúng | UNVERIFIED | Có nhiều guard evidence nhưng chưa audit exhaustive matrix/runtime |

## 13. Danh mục tài liệu cần tìm để trích dẫn

Đây là danh mục mục tiêu, không phải danh sách tài liệu đã được sử dụng hoặc số tài liệu tham khảo đã chốt:

1. Microsoft Learn: ASP.NET Core fundamentals, middleware, dependency injection, authentication/authorization và Web API.
2. Microsoft Learn: EF Core overview, relationships, migrations, transactions và concurrency.
3. Npgsql documentation: EF Core provider và PostgreSQL connection/provider behavior.
4. PostgreSQL documentation: constraints, indexes, transactions và isolation.
5. ASP.NET Core SignalR documentation: hub, groups, authentication và reconnect behavior.
6. RFC 7519 và RFC 8725 cho JWT và các thực hành an toàn liên quan.
7. OWASP Password Storage và Authorization Cheat Sheets cho password hashing và server-side authorization.
8. React, TypeScript, Vite, Tailwind CSS và React Router documentation để mô tả đúng framework, build và routing.
9. ONNX Runtime C# documentation và Hugging Face Tokenizers documentation cho pipeline inference/tokenization.
10. Wang et al., *Text Embeddings by Weakly-Supervised Contrastive Pre-training* để giải thích E5/embedding nếu model/version được chốt; cần kiểm tra bibliographic details và model license từ nguồn chính thức trước khi đưa vào Word.
11. Nguồn học thuật hoặc tài liệu ngành được kiểm chứng riêng cho POS, KDS, QR ordering, loyalty, inventory và multi-branch management nếu muốn trình bày các khái niệm này như cơ sở lý thuyết, thay vì chỉ định nghĩa theo hệ thống.

Không tự tạo DOI, năm xuất bản, số phiên bản, số liệu hiệu năng hoặc kết quả nghiên cứu. Các citation trên phải được ChatGPT kiểm tra lại tại thời điểm biên soạn báo cáo.

## 14. Bảng truy vết source code trọng yếu

| Chủ đề | File/đường dẫn | Thành phần/điểm cần đọc |
|---|---|---|
| DI và startup | `services/api/Program.cs` | `AddControllers`, `AddSignalR`, auth, CORS, AI registration, `AddDbContext`, hub mapping |
| Backend service | `services/api/src/Application/Services/` | Order/Product/Topping/Inventory/Loyalty/Dashboard/Kitchen |
| Persistence | `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` | DbSet, Fluent mapping, FK/index/unique |
| Migration | `services/api/src/Infrastructure/Persistence/Migrations/` | schema history/model snapshot; chưa chứng minh đã apply |
| API | `services/api/src/WebAPI/Controllers/` | controller route/authorization/DTO boundary |
| Realtime | `services/api/src/WebAPI/Hubs/KitchenHub.cs` | authentication, group, branch/role scope |
| JWT | `services/api/src/Infrastructure/Services/JwtService.cs` | claims, expiration, signature validation |
| AI orchestration | `services/api/src/AI/Orchestration/AiOrchestrator.cs` | pilot decision, tool execute, confirmation, safe failure |
| AI contract/security | `services/api/src/AI/Tools/IAiTool.cs`, `src/AI/Security/` | schema, risk, role, branch, confirmation stores |
| AI inference | `services/api/src/AI/Services/AdminEmbeddingPilot.cs`, `CustomerGuestEmbeddingPilot.cs` | tokenizer, ONNX, pooling, normalization, score/margin, slot parser |
| Admin routes | `apps/admin-web/src/App.tsx`, `src/app/routes/*` | role route/layout/POS/KDS/management |
| Customer routes | `apps/customer-web/src/App.tsx`, `src/pages/*` | customer/guest/QR/menu/profile/chat |
| Frontend manifests | `apps/admin-web/package.json`, `apps/customer-web/package.json`, lockfiles | declared/resolved dependency versions |

## 15. Kiến thức bắt buộc ChatGPT sử dụng khi viết Chương 2

- Phân biệt framework/dependency version trong manifest và lockfile với runtime version thực tế.
- Phân biệt kiến trúc được tổ chức theo thư mục với mức độ tuân thủ dependency rule đã xác minh.
- Phân biệt EF model, migration files và schema database đã apply.
- Trình bày branch isolation là tập hợp claim/controller/service/context checks theo use case, không mặc định là tính chất tự động của mọi entity.
- Trình bày POS/KDS/QR/payment/loyalty/inventory/HR theo đúng đường cài đặt và giới hạn; không mở rộng sang payroll, bank reconciliation hoặc recipe inventory.
- Trình bày AI là local embedding pilot + deterministic parsing/slot validation + registry/permission/application services/confirmation; không gọi đây là LLM agent hoặc external provider.
- Không biến score threshold thành accuracy; không biến code tồn tại thành kết quả kiểm thử.
- Những kết quả chưa chạy trong audit này phải giữ nhãn `UNVERIFIED`, `NOT RUN` hoặc được kiểm tra lại trước khi đưa vào báo cáo chính thức.

