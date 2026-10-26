# AUDIT CHƯƠNG 3 – PHÂN TÍCH VÀ THIẾT KẾ HỆ THỐNG

## 1. Phạm vi, phiên bản và giới hạn kiểm chứng

- **Phạm vi:** phân tích tác nhân, quyền, quy trình, dữ liệu, kiến trúc, tài chính, bảo mật, AI và giao diện theo source hiện tại; không viết lại Chương 3 và không tạo sơ đồ mới.
- **HEAD:** `9365c18614666ab4c80eee714403ef73b14edf54` – `Update repository README for current system`.
- **Thời điểm audit:** 20/10/2026, Asia/Saigon.
- **Working tree:** không sạch. Có xóa tài liệu cũ trong `bao-cao/05-audit-hoc-thuat/` và có audit Chương 1/2 mới chưa commit. Không khôi phục, không thay đổi các file đó.
- **Bằng chứng chính:** `services/api/src`, `services/api/Program.cs`, `apps/admin-web`, `apps/customer-web`, manifest/route/import, `ApplicationDbContext`, model snapshot, migrations và các audit lịch sử Chương 3.
- **Không thực hiện:** build, test, khởi động API/frontend, gọi HTTP/SignalR, chạy AI, truy cập PostgreSQL hoặc kiểm tra `__EFMigrationsHistory`. Vì vậy kết quả runtime, schema đã áp dụng, delivery realtime và tính an toàn cạnh tranh được đánh dấu `UNVERIFIED` nếu chưa có bằng chứng tĩnh đủ mạnh.
- **Tài liệu Word:** bản Word cũ và các audit cũ của Chương 3 đã bị xóa khỏi working tree hiện tại nhưng còn trong `HEAD`; audit lịch sử được dùng để nhận diện đề mục và các tuyên bố cũ. Không coi nội dung Word cũ là bằng chứng thay cho source hiện tại.

## 2. Kết luận tổng quát

1. Hệ thống có hai frontend: Admin Web cho quản trị/vận hành và Customer Web cho khách đăng nhập hoặc khách QR. POS và KDS nằm trong Admin Web; Customer Web không kết nối trực tiếp `KitchenHub`.
2. Backend ASP.NET Core 8 dùng controller → application service → `ApplicationDbContext`/Infrastructure; có SignalR `/kitchenHub`. Cấu trúc phân lớp là có thật, nhưng không nên khẳng định Clean Architecture thuần vì application service còn inject trực tiếp DbContext.
3. Có bảy nhóm actor cần phân tích: admin, manager, employee, cashier, kitchen, customer đăng nhập và guest. Guest không phải role JWT riêng; thường mang role `customer` kèm `customerSessionType` và context branch/table.
4. QR bootstrap kiểm tra QR token/bàn/chi nhánh rồi phát guest token. Đơn customer/guest đi qua OrderService, backend lấy giá từ catalog và kiểm tra availability; không tin đơn giá do client gửi.
5. POS tiếp nhận đơn web bằng điều kiện status + `CreatedBy` và có transaction/conditional update ở luồng accept. Luồng tạo/merge order không nên được mô tả là đã có đầy đủ optimistic concurrency chỉ vì luồng accept có bảo vệ cạnh tranh.
6. KDS không dùng một trạng thái duy nhất trên Order để mô tả từng lần chế biến. Mỗi lần gửi thêm tạo `OrderRequest`/`OrderRequestItem`; `SentQuantity` là số đã gửi của từng `OrderDetail`.
7. Thanh toán dùng `Order.TotalAmount` đã lưu; công thức là subtotal → service fee → VAT → discount. Service fee và VAT làm tròn riêng 0 chữ số bằng `MidpointRounding.AwayFromZero`.
8. Kho có chứng từ nhập/xuất, trạng thái Draft/Confirmed/Cancelled, transaction, lock tồn và idempotency key. Chưa thấy recipe/ProductIngredient hoặc tự trừ nguyên liệu theo món.
9. AI hiện tại là local embedding pilot + slot/parser + registry/permission/application service/confirmation. Không mô tả `IAiProvider`, provider cũ, Gemini/LLM hoặc Sequential Tool Calling như kiến trúc đang chạy.

## 3. Ma trận tác nhân và quyền

Ký hiệu: `A` admin, `M` manager, `E` employee, `C` cashier, `K` kitchen, `U` customer đăng ký, `G` guest. Ma trận chỉ ghi quyền có dẫn chứng từ controller/service; menu frontend không thay thế authorization backend.

| Actor | Route/frontend đã thấy | API/policy tiêu biểu | Phạm vi dữ liệu và hành động | Bị từ chối/giới hạn |
|---|---|---|---|---|
| Admin/chủ quán (`A`) | Admin Web `ManagementRoutes`; dashboard, catalog, branches, settings, POS, KDS, inventory, HR | JWT role `admin`; các controller admin; AI `/api/Ai/chat` và confirmation | Global theo thiết kế; quản lý branch, product/topping/promotion, settings, employee, reports, payment/KDS theo endpoint | Vẫn bị chặn nếu thiếu JWT, token hết hạn, resource không tồn tại hoặc trạng thái nghiệp vụ không hợp lệ |
| Manager (`M`) | `ManagementRoutes`; dashboard, inventory, employee, POS/KDS, reservation/table theo route | Controller ghi `admin,manager` hoặc role cụ thể; branch context từ JWT/service | Chủ yếu branch được gán; dashboard/order/inventory/employee/receipt read theo branch tùy service | Không mặc định sửa product price/topping/promotion/system settings/branch; không có quyền Admin AI write |
| Employee (`E`) | `StaffRoutes`: sales, tables redirect, inventory/profile/attendance/schedule | Order accept/send/status có E; customer phục vụ; inventory operational routes tùy endpoint | Branch/user scope theo context; xử lý order và tác vụ vận hành | Không payment ở endpoint chỉ A/M/C; không sửa catalog quản trị, employee, branch/system settings |
| Cashier (`C`) | `CashierRoutes`: POS, invoice, notifications, attendance/schedule/shifts/reservations | Order create/accept/payment; invoice; receipt read; role `[Authorize]` cụ thể | Branch của JWT và order/table tương ứng | Không sửa receipt settings/catalog/branch; KDS request status chỉ role K/A/M theo endpoint |
| Kitchen (`K`) | `KitchenRoutes`: KDS, kitchen history, notifications, tables/reservation read, attendance/schedule/profile | Send-to-kitchen/status order; active/history/detail/update request status; SignalR hub | Branch group và branch checks; xử lý `OrderRequest` | Không payment, không sửa catalog quản trị, không có Admin AI chat; không được coi KDS status là thanh toán/order completion |
| Customer đăng nhập (`U`) | Customer Web `/`, `/reservation`, `/profile`, `/scan`, menu/order/chat | customer-token; `[Authorize(Roles="customer")]` tùy endpoint; customer chat read-only | User/customer phone/CustomerId và branch/session; xem menu, tạo order, profile, history, loyalty theo endpoint | Không dùng admin tools; customer AI chỉ read tools; không tự nhận đơn, bếp, payment staff hoặc catalog write |
| Guest QR/direct (`G`) | Customer Web QR scan/menu/guest chatbot | `GuestController` AllowAnonymous bootstrap/direct-session; guest token role customer + session marker; guest-chat riêng | Branch/table từ server-issued context; QR guest gắn bàn; direct guest có thể takeaway | Không dùng endpoint cần registered customer; không xem lịch sử không có context; chỉ read-only AI/menu flow; QR/token/bàn sai bị từ chối |

### 3.1. Ma trận chức năng tiêu biểu có dẫn chứng

| Chức năng | Admin | Manager | Employee | Cashier | Kitchen | Customer | Guest | Bằng chứng |
|---|---:|---:|---:|---:|---:|---:|---:|---|
| Xem menu/product | Có | Có | Có | Có | Có theo endpoint | Có | Có | `ProductController`, menu tools/frontend |
| Tạo/sửa/xóa Product | Có | Không theo controller | Không | Không | Không; chỉ availability | Không | Không | `ProductController.cs:105-196,221-234` |
| Tạo/sửa/xóa Topping | Có | Không | Không | Không | Không | Không | Không | `ToppingController.cs:42-85` |
| Cập nhật availability Product | Có | Có | Không | Không | Có | Không | Không | `ProductController.cs:221-234` |
| Tạo order | Có | Có | Có | Có; customer/guest theo path | Có theo endpoint Order | Có | Có session hợp lệ | `OrderController.cs:176-194` |
| Accept web order | Có | Có | Có | Có | Không | Không | Không | `OrderController.cs:355-396` |
| Gửi món KDS | Có | Có | Có | Có | Có | Không trực tiếp | Không | `OrderController.cs:397-420` |
| Cập nhật trạng thái request bếp | Có | Có | Không | Không | Có | Không | Không | `OrderController.cs:603-635` |
| Thanh toán | Có | Có | Không | Có | Không | Không trực tiếp | Không | `OrderController.cs:431-440` |
| Quản lý employee | Có | Có trong phạm vi được kiểm tra | Không | Không | Không | Không | Không | `EmployeeController.cs:39-317` |
| Quản lý branch/system settings | Có | Không theo controller | Không | Không | Không | Không | Không | `BranchController.cs`, `SystemSettingsController.cs` |
| Kho chứng từ | Có | Có; operational roles tạo issue theo endpoint | Tạo issue tùy endpoint | Tạo issue tùy endpoint | Tạo issue tùy endpoint | Không | Không | `InventoryController.cs`, `InventoryService.cs` |
| AI admin write | Có | Không | Không | Không | Không | Không | Không | `AiController`, `AiPermissionService`, `AiAuthorization` |

### 3.2. Khác biệt giữa menu và quyền backend

- Admin Web có thể render route hoặc menu theo role, nhưng frontend guard chỉ là UX. `[Authorize]`, `RoleNames`, branch context và service/object checks mới là ranh giới bảo mật.
- Có endpoint dùng `[Authorize]` chung rồi tự kiểm customer/owner/branch trong service; không được suy role từ route hoặc `Position` của Employee.
- Menu có thể cho nhiều role thấy chức năng đọc, trong khi write action ở controller chỉ admin hoặc admin/manager. Ví dụ manager có thể thấy inventory/dashboard nhưng không mặc định sửa Product price.
- `KitchenHub` group có tác dụng phân phối realtime; không được dùng group membership như bằng chứng duy nhất của quyền truy cập dữ liệu.

## 4. Yêu cầu chức năng và phi chức năng có căn cứ

### 4.1. Yêu cầu chức năng

| Mã nhóm | Yêu cầu được source hỗ trợ | Bằng chứng | Trạng thái |
|---|---|---|---|
| F01 | Đăng nhập JWT, thông tin tài khoản, đổi password, customer token, guest session | `AuthController`, `JwtService`, `GuestController` | IMPLEMENTED |
| F02 | Branch, role, branch-scoped data và branch availability | `BranchController`, claims, product availability, service contexts | IMPLEMENTED/PARTIAL theo module |
| F03 | Product, topping, promotion, menu và availability | `ProductController`, `ToppingController`, `PromotionController`, ProductService | IMPLEMENTED |
| F04 | Area, table, table status, QR token, reservation | `AreaController`, `TableController`, `ReservationController`, entities | IMPLEMENTED |
| F05 | Customer/guest menu và tạo/merge order | Customer Web, GuestController, OrderController/Service | IMPLEMENTED; concurrency create/merge chưa chứng minh đầy đủ |
| F06 | POS accept/update/order-to-kitchen/KDS | OrderController, OrderService, KitchenService, KitchenHub | IMPLEMENTED trong code |
| F07 | Payment, persisted total, receipt/invoice, cash/transfer method | OrderService, OrderController, InvoiceController, branch payment info | IMPLEMENTED; bank reconciliation NOT_SUPPORTED |
| F08 | Customer, loyalty balance/earn/redeem/refund/history | CustomerController, LoyaltyService, OrderService | IMPLEMENTED theo endpoint; customer redeem trực tiếp không được suy ra |
| F09 | Employee, attendance, work schedule, shift | controllers/services/entities/routes | IMPLEMENTED; payroll NOT_SUPPORTED |
| F10 | Expense và dashboard revenue/summary/best sellers/order count | ExpenseController, DashboardController, DashboardService | IMPLEMENTED |
| F11 | Inventory items, branch inventory, receipt/issue/transactions | InventoryController/Service/entities | IMPLEMENTED; recipe deduction NOT_SUPPORTED |
| F12 | AI read tools, admin catalog write với confirmation | AiController, pilots, orchestrator, registry, tools | IMPLEMENTED có điều kiện flag/artifact; runtime chưa chạy trong audit |

### 4.2. Yêu cầu phi chức năng

| Nhóm | Căn cứ thực tế | Cách ghi trong luận văn |
|---|---|---|
| Bảo mật | JWT signature/issuer/audience/lifetime; role/controller checks; branch context; password hasher; AI permission | Yêu cầu và cơ chế đã cài; không khẳng định kiểm thử toàn diện |
| Toàn vẹn tài chính | decimal, backend price lookup, persisted total, payment check, transaction ở một số flow | Có thiết kế bảo vệ; cần phân biệt với kết quả test |
| Cô lập chi nhánh | branch claims, branch IDs, service filters, hub groups, ProductBranchAvailability | Có nhiều điểm kiểm; chưa audit exhaustive mọi endpoint |
| Khả dụng/realtime | SignalR reconnect, notification, KDS groups | Có đường triển khai; chưa đo delivery/latency |
| Hiệu năng | AsNoTracking, paging inventory, cache dashboard, execution retry | Không đưa số liệu hiệu năng nếu chưa benchmark |
| Bảo trì | module/layer/service/DTO, DI, migrations, generated API type script | Có cấu trúc hỗ trợ bảo trì; không tự gọi Clean Architecture đầy đủ |
| Triển khai | HTTPS port 5000, CORS allowlist, config environment, Supabase PostgreSQL | Có cấu hình; trạng thái môi trường chạy thật UNVERIFIED |
| Kiểm thử | checklist manual UAT, build history trong README | Không đánh dấu PASS từ source; lượt audit này NOT RUN |

## 5. Đặc tả quy trình nghiệp vụ

Các bước dưới đây là đặc tả source-backed để dựng activity/sequence sau này, không phải sơ đồ mới.

### 5.1. A – Đặt món qua QR

**Điều kiện trước:** branch active; bàn active; `RestaurantTable.QrToken` tồn tại và unique; frontend có QR token hoặc dùng direct guest path.

**Luồng chính:**

1. Customer Web scan camera/QR hoặc nhận QR URL.
2. Gửi `POST /api/Guest/bootstrap` với token; backend kiểm tra định dạng token, tìm table, active status, Area/Branch và quan hệ branch.
3. Backend phát signed guest JWT với role customer, `customerSessionType`, branch/table context; direct session có lifetime riêng và có thể takeaway.
4. Frontend dùng token để lấy menu/product/topping theo branch/availability.
5. Client gửi Order DTO; backend lấy Product/Topping/size/topping price từ database, kiểm branch active, product active/available, quantity và dữ liệu bàn/session.
6. Với guest, `BranchId`/`TableName` phải phù hợp claim; không cho body tự đổi scope. Order có thể tạo mới hoặc merge vào order đang xử lý cùng bàn/branch theo logic OrderService.
7. Backend lưu Order/OrderDetail, tính lại giá và tổng, sau đó response order. Notification/realtime cho staff là đường riêng; không nói QR trực tiếp gọi KitchenHub.

**Nhánh từ chối:** QR sai/hết/không tìm thấy; bàn inactive; bàn khác branch; branch inactive; product unavailable; quantity quá giới hạn (source đặt max 500 cho detail); thiếu product; customer/guest context sai; order body cố gửi Id/branch/table không được phép; thiếu token/role.

**Lớp tham gia:** `QRScan`/`DigitalMenu` → `GuestController`/`ProductController`/`OrderController` → `OrderService`/`ProductService` → `ApplicationDbContext` → PostgreSQL; notification/SignalR chỉ tham gia ở đường producer phù hợp.

### 5.2. B – POS và tiếp nhận đơn

**Điều kiện trước:** JWT staff hợp lệ; branch context; Order DTO có item; branch active; product availability hợp lệ.

**Luồng tạo/cập nhật:** POS gửi product/topping/option/quantity, không được xem giá client là authoritative. `OrderService.CreateOrUpdateOrderAsync` tìm Product/Topping, đọc `SizesJson`/`ToppingsJson` khi cần, tính `UnitPrice`, `SubTotal`, fee/VAT/discount/total; quantity > 500 bị từ chối.

**Options:** `OrderDetail.Options` lưu chuỗi lựa chọn/size; giá size và topping được resolve từ catalog JSON hoặc Topping entity theo đường tương ứng. Product/Topping ID và active/availability phải phù hợp.

**Đơn web chờ tiếp nhận:** không có status riêng “Pending”. Hệ thống nhận diện thường bằng `Status = Đang xử lý` và `CreatedBy` rỗng. `AcceptWebOrderAsync` kiểm order tồn tại, branch active/cùng scope, trạng thái, người nhận chưa được gán, bàn active/cùng branch/chưa có khách; transaction và conditional update được dùng để chống hai nhân viên claim cùng một order ở luồng accept.

**Giới hạn:** Luồng create/merge order đọc rồi ghi không có bằng chứng conditional update tương đương accept. Không khẳng định toàn bộ POS đã chống race. Thay đổi order đã gửi bếp phải tôn trọng `SentQuantity` và trạng thái.

### 5.3. C – KDS và yêu cầu chế biến

**Điều kiện trước:** Order tồn tại, branch active, status cho phép gửi; có `OrderDetail.Quantity > SentQuantity`; actor có role được endpoint cho phép.

**Luồng:**

1. POS/staff gọi `POST /api/Order/{id}/send-to-kitchen`.
2. `KitchenService` lấy detail chưa gửi; với mỗi detail tính `pending = Quantity - SentQuantity`.
3. Tạo một `OrderRequest` mới và đánh `RequestNumber`; tạo `OrderRequestItem` cho phần pending, giữ ProductId/ProductName/Options/Note.
4. Cập nhật `SentQuantity = Quantity`, lưu cùng transaction theo code path.
5. `KitchenNotifier` phát event qua `KitchenHub`/notification; KDS load active request/history và cập nhật trạng thái.
6. Request có các trạng thái `Pending`, `Preparing`, `Ready`, `Completed`, `Cancelled`; trạng thái request không thay thế `Order.Status` hoặc payment status.

**Bổ sung món:** nếu Quantity tăng sau đợt đầu, `pending` tạo request/đợt mới; không gửi lại phần đã gửi. `SentQuantity` là số đã gửi theo detail, không phải số đã hoàn thành.

**Nhánh lỗi:** order/branch không tồn tại; không còn pending quantity; status order không cho phép; actor không thuộc branch; request status transition không hợp lệ; duplicate/concurrent send chưa được chứng minh hoàn toàn từ source.

**Mất kết nối:** frontend SignalR có reconnect/refetch ở các client đã kiểm. Vì request đã lưu DB trước event, client có thể tải lại active/history; việc event không mất trong mọi failure window và delivery runtime chưa được kiểm chứng.

### 5.4. D – Thanh toán

**Điều kiện trước:** order tồn tại; branch/object scope hợp lệ; order chưa paid; status cho phép thanh toán; phương thức thuộc tập backend hỗ trợ; số tiền request không được dùng thay `Order.TotalAmount`.

**Luồng:** controller nhận payment request → service đọc order persisted → kiểm điều kiện → conditional update/transaction theo code → set `PaidAmount`, `PaymentMethod`, `PaymentAt`, status liên quan → xử lý loyalty đủ điều kiện → response invoice/receipt/notifier tùy đường.

**Phương thức:** source có cash/transfer và UI VietQR/branch bank information. Chưa thấy webhook ngân hàng, callback settlement hoặc auto reconciliation. Vì vậy VietQR chỉ được mô tả là thông tin/hiển thị chuyển khoản nếu không có thêm bằng chứng runtime.

**Công thức:** xem mục 8. Service fee và VAT làm tròn riêng; discount được trừ sau fee và VAT, nhưng discount client mới mặc định zero cho tới khi có server-side promotion flow phù hợp. Không tự mô tả Promotion entity là discount đã tự động áp vào mọi order.

**Loyalty:** `LoyaltyService` có earn/redeem/refund/balance/history. Earn được gọi ở flow thanh toán/hoàn tất theo điều kiện customer; giao dịch loyalty có unique filtered index theo `(OrderId, Type)` khi OrderId có giá trị, giúp hạn chế giao dịch trùng theo loại, nhưng không thay cho test idempotency toàn bộ payment.

**Nhánh lỗi:** order không có, status sai, đã thanh toán, amount không khớp persisted total, method không hợp lệ, customer/loyalty không đủ điều kiện, branch không hợp lệ, transaction/database failure.

### 5.5. E – Kho

**Danh mục:** `InventoryItem` có tên normalized, unit code, active; `BranchInventory` lưu current/minimum quantity theo branch.

**Nhập kho:** tạo `StockReceipt` Draft với item/quantity/unit price; validate item/unit/quantity; tổng tiền tính từ dòng. Có idempotency key theo branch; update chỉ khi Draft. Confirm kiểm branch, lock/tạo row tồn, cộng quantity, ghi `StockTransaction(IN)`, tạo `Expense` liên kết receipt, đổi status Confirmed trong execution strategy + transaction. Cancel chỉ cho Draft.

**Xuất kho:** tạo `StockIssue` Draft với reason/items; update Draft; confirm lock row tồn, kiểm đủ số lượng, trừ quantity, ghi `StockTransaction(OUT)`, đổi Confirmed trong transaction. Cancel chỉ cho Draft.

**Trạng thái:** `Draft`, `Confirmed`, `Cancelled`; item quantity dương; receipt unit price/total không âm; tồn current/minimum không âm; transaction type/reference type có check constraint trong mapping.

**Idempotency:** receipt/issue có `IdempotencyKey` unique filtered theo branch; manual expense có bảng `ManualExpenseIdempotencyRecords` theo branch/user/key. Đây là thiết kế idempotency cho các đường tương ứng, không suy ra mọi API đều idempotent.

**Chưa triển khai:** không thấy ProductIngredient/recipe, tự trừ nguyên liệu do order, đối chiếu tồn với định lượng món, hay payroll/cost accounting đầy đủ. Chi phí nhập hàng được liên kết từ receipt confirmation; chi phí thủ công là đường riêng.

### 5.6. F – Nghiệp vụ hỗ trợ

| Nghiệp vụ | Luồng/lớp đã thấy | Giới hạn |
|---|---|---|
| Nhân sự | EmployeeController/Service, role, branch, active toggle | `BasicSalary` là thuộc tính; không chứng minh payroll calculation |
| Chấm công | AttendanceController/Service, EmployeeId/BranchId, check-in/out, QR camera ở Admin | Không audit runtime camera trong lượt này |
| Lịch làm việc | WorkScheduleController/Service, EmployeeId/Date/BranchId | BranchId có thể nullable ở entity; cần mô tả đúng mapping |
| Ca làm | ShiftController/Service, open/close, starting/ending cash, revenue split | Không coi là payroll hoặc bank reconciliation |
| Đặt bàn | ReservationController/Service, branch/table/status/time/guest count | Quyền và trạng thái thay đổi theo endpoint; không suy public/registered path đồng nhất |
| Chi phí | ExpenseController/Service, branch/date/category/payment/stock receipt link/idempotency thủ công | Không phải toàn bộ COGS tự động từ recipe |
| Báo cáo | DashboardController/Service: summary, revenue period/report, best sellers; cache/invalidation | Business Insights module cũ đã bị loại bỏ; không đưa endpoint cũ vào thiết kế hiện tại |
| Thông báo | NotificationController/Service, NotificationRead, SignalR notification center | Delivery realtime chưa test; read state có cả legacy field và read table |

## 6. Đặc tả UML cần đối chiếu

Không tự tạo hoặc sửa hình. Các đặc tả dưới đây là dữ liệu để ChatGPT dựng lại sơ đồ theo source.

### 6.1. Use Case

| Sơ đồ | Actor/thành phần đúng | Quan hệ/điều kiện cần thể hiện | Cần bỏ/sửa |
|---|---|---|---|
| Use Case tổng quát | Admin, Manager, Employee, Cashier, Kitchen, Customer, Guest; hệ thống backend/frontend | Actor chung có use case riêng theo role; Guest kế thừa context customer ở mức session nhưng không phải role JWT độc lập | Không gộp mọi staff thành một actor nếu sơ đồ cần biểu diễn payment/KDS/HR khác nhau |
| Customer/Guest | Customer registered, QR Guest, Direct Guest; Customer Web; GuestController; Order/Product/Reservation/AI read path | QR bootstrap → guest token → menu → order; registered customer có profile/history; guest giới hạn branch/table/session | Không cho guest dùng admin/customer registered history hoặc AI write |
| Nhân sự vận hành | Manager, Employee, Cashier, Kitchen; Admin có thể bao quát | POS/order accept, kitchen send/status, payment cashier, attendance/shift/reservation/inventory tùy endpoint | Không ghi “mọi nhân viên quản lý mọi dữ liệu”; không gắn kitchen với payment |
| Quản trị | Admin; Branch/Product/Topping/Promotion/Employee/System/Receipt/AI write | Admin global scope; write AI có preview/confirmation; branch/product mutation còn condition service | Không thêm Flutter Web, app thứ ba hoặc external AI provider |

### 6.2. Activity Diagram

- **Đặt món:** QR token validation → guest/customer context → menu/availability → client order → server price/quantity/branch validation → create/merge order → response/notification. Nhánh lỗi phải nằm trước persistence.
- **Tiếp nhận đơn:** staff list pending signal → chọn order/table → transaction đọc order → kiểm status/CreatedBy/table/branch → conditional claim → mark table/order → response conflict nếu đã nhận.
- **KDS:** order details → pending quantity → create request batch → request items → update SentQuantity → save → SignalR/notification → KDS status transitions. Không vẽ hoàn tất request = thanh toán order.
- **Thanh toán:** load order → validate status/branch/method/amount → compute/read persisted total → conditional payment update → loyalty if eligible → invoice/response. VietQR không có callback ngân hàng được xác minh.
- **Trợ lý:** HTTP context/JWT → controller input limit/session check → pilot decision → ACCEPT/CLARIFY/UNSUPPORTED → registry lookup → permission/risk/branch → validate args → read tool hoặc preview confirmation → confirm endpoint → application service → safe response. Không vẽ LLM planning/Sequential Tool Calling nếu không có source.

### 6.3. Sequence Diagram

| Sơ đồ | Sequence thực tế cần thể hiện | Điều chỉnh bắt buộc |
|---|---|---|
| Thanh toán | POS/Customer? → OrderController → OrderService → DbContext/PostgreSQL → LoyaltyService → response/receipt | Payment là staff endpoint; customer không trực tiếp xác nhận payment theo controller. Dùng persisted TotalAmount, không dùng client total |
| POS–Backend–KDS | POS/Customer Web → OrderController/OrderService → KitchenService → OrderRequest/DB → KitchenNotifier/KitchenHub → KDS clients/notifications | Customer Web không nối trực tiếp KitchenHub; SignalR producer là backend |
| Trợ lý | Admin/Customer client → AiController → AiOrchestrator → Admin/CustomerEmbeddingPilot → AiToolRegistry → AiPermissionService/AiAuthorization → Tool → Application Service → DB → formatter/response | Bỏ IAiProvider/Mock provider cũ, Gemini, prompt-to-LLM và autonomous loop khỏi kiến trúc hiện tại |

### 6.4. Domain Class Diagram

Các nhóm quan hệ nên thể hiện: `Branch–Area–RestaurantTable`; `Branch–Employee/Attendance/WorkSchedule/Shift/Reservation/Expense/Inventory`; `Product–ProductBranchAvailability`; `Order–OrderDetail–Product/Topping`; `Order–OrderRequest–OrderRequestItem`; `Customer–Order/LoyaltyTransaction`; `InventoryItem–BranchInventory/StockReceiptItem/StockIssueItem/StockTransaction`; `Notification–NotificationRead`; `Branch–SystemSetting/ReceiptSettings`.

Không đưa `ProductIngredient`, `Payroll`, `BusinessInsight` hoặc bảng không tồn tại vào class diagram hiện hành. `OrderDetail` là entity hiện tại, không đổi tên thành `OrderItem` trong mô tả source. Quan hệ EF cụ thể phải lấy từ DbContext/migration, không suy từ navigation property.

## 7. Audit cơ sở dữ liệu

### 7.1. Danh mục bảng vật lý hiện có trong model snapshot

Model snapshot hiện liệt kê 31 bảng. Bảng dưới đây tóm tắt entity, khóa, FK, nullable/đặc tính quan trọng, branch scope và nghiệp vụ. Kiểu PostgreSQL chỉ ghi khi mapping xác định trực tiếp; các property còn lại cần đối chiếu snapshot/migration khi đưa vào bảng chính thức. `UNVERIFIED` nghĩa là chưa kiểm schema đã apply.

| Bảng vật lý | Entity | PK | FK/quan hệ | Null/default/constraint đáng chú ý | Branch scope và nghiệp vụ |
|---|---|---|---|---|---|
| `Areas` | `Area` | `Id` | `BranchId → Branch` required | Name, DisplayOrder, CreatedAt; index BranchId | Khu vực bàn theo branch |
| `Attendances` | `Attendance` | `Id` | EmployeeId, BranchId | CheckIn required; CheckOut/Status/Note/Image nullable | Chấm công nhân viên theo branch |
| `Branches` | `Branch` | `Id` | Không | IsActive default true; thông tin ngân hàng/địa chỉ nullable | Nguồn scope và payment info |
| `BranchInventories` | `BranchInventory` | `Id` | BranchId, InventoryItemId required | Decimal precision 18,3; current/min >= 0; unique BranchId+InventoryItemId | Tồn từng item tại branch |
| `Customers` | `Customer` | `Id` | Không trực tiếp | Phone unique; profile/password/active/loyalty fields | User/customer scope, không branch cố định |
| `Employees` | `Employee` | `Id` | BranchId nullable | Role, IsActive default; salary/profile/password fields | Tài khoản nhân viên và branch làm việc |
| `Expenses` | `Expense` | `Id` | BranchId; optional StockReceiptId one-to-one | Amount/description/payment/note required theo mapping; StockReceiptId unique filtered | Chi phí branch; expense nhập hàng liên kết receipt |
| `InventoryItems` | `InventoryItem` | `Id` | — | Name/NormalizedName/UnitCode required; active-filtered unique NormalizedName | Danh mục nguyên liệu/vật tư; chưa có recipe |
| `LoyaltyTransactions` | `LoyaltyTransaction` | `Id` | CustomerId required; OrderId nullable | Type/Points/BalanceAfter; unique filtered OrderId+Type | Lịch sử điểm |
| `ManualExpenseIdempotencyRecords` | `ManualExpenseIdempotencyRecord` | `Id` | BranchId; UserId logical | Key/PayloadHash/ResponseJson required; unique Branch+User+Key | Chống tạo trùng chi phí thủ công |
| `Notifications` | `Notification` | `Id` | BranchId/TargetUserId logical nullable | TargetRole/User/Entity/Route/legacy read fields nullable | Thông báo branch/role/user |
| `NotificationReads` | `NotificationRead` | `(NotificationId, UserId)` | NotificationId required | ReadAt required; composite PK; UserId index | Read state theo user |
| `Orders` | `Order` | `Id` | BranchId required; CustomerId nullable | Financial snapshots nullable; Total/Paid/Discount; index Branch+CreatedAt+Status | Đơn bán hàng, branch/customer |
| `OrderDetails` | `OrderDetail` | `Id` | OrderId shadow required; ProductId/ToppingId nullable | Quantity, SentQuantity, UnitPrice; Options nullable; cascade Order | Dòng món/topping và lịch sử giá dòng |
| `OrderRequests` | `OrderRequest` | `Id` | OrderId required | RequestNumber/Status/CreatedAt; processed/timestamps nullable | Đợt gửi bếp |
| `OrderRequestItems` | `OrderRequestItem` | `Id` | OrderRequestId required; ProductId nullable | ProductName/Quantity; Note/Options nullable; cascade request | Dòng món trong đợt bếp |
| `Products` | `Product` | `Id` | — | Price/CostPrice decimal; SizesJson/ToppingsJson nullable; IsActive/Availability defaults | Catalog global |
| `ProductBranchAvailabilities` | `ProductBranchAvailability` | `(ProductId, BranchId)` | Product/Branch required | Status required max 32; UpdatedAt required; Branch+Status index | Product availability theo branch |
| `Promotions` | `Promotion` | `Id` | — | RequiredPoints/DiscountValue; IsActive; EndDate nullable | Cấu hình khuyến mãi/đổi điểm; không tự khẳng định auto discount |
| `ReceiptSettings` | `ReceiptSettings` | `Id` | BranchId nullable | Unique filtered BranchId non-null; display flags/defaults | Cấu hình hóa đơn/receipt branch |
| `Reservations` | `Reservation` | `Id` | BranchId required; TableId nullable | ReservationTime/guest count/status; table/name nullable | Đặt bàn branch |
| `Tables` | `RestaurantTable` | `Id` | AreaId/BranchId required | QrToken required max 43, unique; active/status defaults | Bàn, QR, trạng thái bàn |
| `Shifts` | `Shift` | `Id` | EmployeeId/BranchId | Start required; End/EndingCash nullable; status/revenue fields | Ca và doanh thu theo nhân viên/branch |
| `StockIssues` | `StockIssue` | `Id` | BranchId required; items 1-N | Status string enum with Draft/Confirmed/Cancelled; key filtered unique; reason required | Phiếu xuất/điều chỉnh giảm tồn |
| `StockIssueItems` | `StockIssueItem` | `Id` | StockIssueId/InventoryItemId required | Quantity > 0; restrict delete | Dòng phiếu xuất |
| `StockReceipts` | `StockReceipt` | `Id` | BranchId required; items 1-N | Total >= 0; status check; key filtered unique | Phiếu nhập, tạo expense khi confirm |
| `StockReceiptItems` | `StockReceiptItem` | `Id` | StockReceiptId/InventoryItemId required | Quantity > 0; UnitPrice >= 0; precision 18,3/18,2 | Dòng phiếu nhập |
| `StockTransactions` | `StockTransaction` | `Id` | BranchId/InventoryItemId required | Quantity > 0; before/after >= 0; Type/ReferenceType check | Audit biến động tồn |
| `SystemSettings` | `SystemSetting` | `Id` | BranchId nullable | Unique filtered `(BranchId, Key)` khi branch non-null; Value/Type/UpdatedAt | Cấu hình hệ thống/branch |
| `Toppings` | `Topping` | `Id` | — | Price/CostPrice; Name/type/category/description/image; active | Catalog topping global |
| `WorkSchedules` | `WorkSchedule` | `Id` | EmployeeId required; BranchId nullable | Date/name/time/branch name nullable | Lịch làm việc |

**Ghi chú kiểu dữ liệu:** các giá trị tiền dùng CLR `decimal`; snapshot mapping cần kiểm tra precision từng field trước khi ghi chính xác `numeric(p,s)` vào Word. Inventory quantity được mapping precision `(18,3)`; money trong stock receipt/unit price `(18,2)` theo `ApplicationDbContext.cs:252-365`. Kiểu UUID/boolean/timestamp/string của PostgreSQL cần xác nhận từ migration/schema nếu cần nêu chi tiết ở mức DB vật lý.

### 7.1.1. Bảng thuộc tính trọng yếu theo định dạng đề xuất

Bảng này ghi các thuộc tính trọng yếu đủ để dựng thiết kế; danh sách CLR đầy đủ vẫn phải lấy trực tiếp từ entity tương ứng. Kiểu PostgreSQL được ghi chắc chắn chỉ ở các property có Fluent mapping/precision rõ; các kiểu còn lại ghi theo CLR và cần kiểm schema thật nếu muốn chuyển thành kiểu vật lý.

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| `Branch.Id`, `Name`, `IsActive`, `BankName`, `AccountNumber` | `Guid`, `string`, `bool`, `string?` | PK | Name/IsActive bắt buộc theo entity; thông tin khác có thể null | Chi nhánh và thông tin thanh toán |
| `Area.Id`, `BranchId`, `Name`, `DisplayOrder` | `Guid`, `Guid`, `string`, `int` | PK/FK Branch | Không null ở khóa/name | Khu vực trong branch |
| `RestaurantTable.Id`, `AreaId`, `BranchId`, `Name`, `SeatCount`, `QrToken`, `Status`, `IsActive` | `Guid`, `Guid`, `Guid`, `string`, `int`, `string`, `string`, `bool` | PK/FK/unique QrToken | QrToken required; Description/QrCodeUrl/BranchName nullable | Bàn, QR và trạng thái bàn |
| `Employee.Id`, `Role`, `BranchId`, `Username`, `Password`, `IsActive`, `BasicSalary` | `Guid`, `string`, `Guid?`, `string?`, `string?`, `bool`, `decimal` | PK/FK nullable | Profile/branch/password tùy dữ liệu | Tài khoản và nhân sự |
| `Customer.Id`, `PhoneNumber`, `Password`, `IsActive`, `LoyaltyPoints`, `TotalSpending`, `TotalOrders` | `Guid`, `string?`, `string?`, `bool`, `int`, `decimal`, `int` | PK/unique PhoneNumber | Phone/password/profile nullable | Khách đăng ký và điểm |
| `Product.Id`, `Name`, `Category`, `Group`, `Price`, `CostPrice`, `SizesJson`, `ToppingsJson`, `IsActive` | `Guid`, `string?`, `string?`, `string?`, `decimal`, `decimal`, `string?`, `string?`, `bool` | PK | Name/category/group/json nullable theo entity | Catalog món và giá |
| `ProductBranchAvailability.ProductId`, `BranchId`, `Status`, `UpdatedAtUtc` | `Guid`, `Guid`, `string`, `DateTime` | Composite PK/FK | Status/time required | Khả dụng món theo branch |
| `Topping.Id`, `Name`, `Price`, `CostPrice`, `ToppingType`, `Category`, `IsActive` | `Guid`, `string`, `decimal`, `decimal`, `string?`, `string?`, `bool` | PK | Type/category/description/image nullable | Catalog topping |
| `Promotion.Id`, `Name`, `RequiredPoints`, `DiscountValue`, `PromotionType`, `IsActive`, `EndDate` | `Guid`, `string`, `int`, `decimal`, `string`, `bool`, `DateTime?` | PK | EndDate nullable | Cấu hình promotion/đổi điểm |
| `Order.Id`, `BranchId`, `CustomerId`, `Status`, `SubTotal`, `ServiceFeeAmount`, `VatAmount`, `Discount`, `TotalAmount`, `PaidAmount`, `PaymentMethod` | `Guid`, `Guid`, `Guid?`, `string`, `decimal?`, `decimal?`, `decimal?`, `decimal`, `decimal`, `decimal`, `string?` | PK/FK Branch/Customer | Customer, snapshots/payment method nullable | Đơn và snapshot tài chính |
| `OrderDetail.Id`, `OrderId`, `ProductId`, `ToppingId`, `ProductName`, `Quantity`, `SentQuantity`, `UnitPrice`, `Options` | `Guid`, `Guid`, `Guid?`, `Guid?`, `string`, `int`, `int`, `decimal`, `string?` | PK/FK | Product/Topping/Options nullable; Order required | Dòng món, giá snapshot, số gửi bếp |
| `OrderRequest.Id`, `OrderId`, `RequestNumber`, `Status`, `CreatedAt`, `ProcessedBy` | `Guid`, `Guid`, `int`, `string`, `DateTime`, `string?` | PK/FK Order | Process/timestamps/note nullable | Đợt chế biến |
| `OrderRequestItem.Id`, `OrderRequestId`, `ProductId`, `ProductName`, `Quantity`, `Note`, `Options` | `Guid`, `Guid`, `Guid?`, `string`, `int`, `string?`, `string?` | PK/FK | ProductId/note/options nullable | Dòng món của đợt bếp |
| `Reservation.Id`, `BranchId`, `TableId`, `ReservationTime`, `NumberOfGuests`, `Status`, `CustomerPhone` | `Guid`, `Guid`, `Guid?`, `DateTime`, `int`, `string`, `string?` | PK/FK | Table/customer fields nullable | Đặt bàn |
| `Attendance.Id`, `EmployeeId`, `BranchId`, `CheckInTime`, `CheckOutTime`, `Status`, `Note` | `Guid`, `Guid`, `Guid`, `DateTime`, `DateTime?`, `string?`, `string?` | PK/FK | CheckOut/status/note/image nullable | Chấm công |
| `WorkSchedule.Id`, `EmployeeId`, `BranchId`, `Date`, `ShiftName`, `StartTime`, `EndTime` | `Guid`, `Guid`, `Guid?`, `DateTime`, `string?`, `string?`, `string?` | PK/FK | Branch/time labels nullable | Lịch làm việc |
| `Shift.Id`, `EmployeeId`, `BranchId`, `StartTime`, `EndTime`, `StartingCash`, `EndingCash`, `Status` | `Guid`, `Guid`, `Guid`, `DateTime`, `DateTime?`, `decimal`, `decimal?`, `string` | PK/FK | EndTime/EndingCash nullable | Ca và chốt tiền |
| `InventoryItem.Id`, `Name`, `NormalizedName`, `UnitCode`, `IsActive` | `Guid`, `string`, `string`, `string`, `bool` | PK/unique active NormalizedName | Required mapping | Danh mục tồn |
| `BranchInventory.Id`, `BranchId`, `InventoryItemId`, `CurrentQuantity`, `MinimumStock` | `Guid`, `Guid`, `Guid`, `decimal`, `decimal` | PK/FK/unique pair | Required; numeric 18,3; non-negative | Tồn theo branch |
| `StockReceipt.Id`, `BranchId`, `Status`, `TotalAmount`, `CreatedBy`, `IdempotencyKey` | `Guid`, `Guid`, enum string, `decimal`, `Guid`, `string?` | PK/FK/filtered unique key | Supplier/purchase/note/key nullable; total non-negative | Phiếu nhập |
| `StockReceiptItem.Id`, `StockReceiptId`, `InventoryItemId`, `Quantity`, `UnitPrice` | `Guid`, `Guid`, `Guid`, `decimal`, `decimal` | PK/FK | Required; quantity > 0, unit price >= 0 | Dòng phiếu nhập |
| `StockIssue.Id`, `BranchId`, `Status`, `Reason`, `CreatedBy`, `IdempotencyKey` | `Guid`, `Guid`, enum string, `string`, `Guid`, `string?` | PK/FK/filtered unique key | Reason required; note/key nullable | Phiếu xuất |
| `StockIssueItem.Id`, `StockIssueId`, `InventoryItemId`, `Quantity` | `Guid`, `Guid`, `Guid`, `decimal` | PK/FK | Required; quantity > 0 | Dòng phiếu xuất |
| `StockTransaction.Id`, `BranchId`, `InventoryItemId`, `Type`, `Quantity`, `BeforeQuantity`, `AfterQuantity`, `ReferenceType`, `ReferenceId` | `Guid`, `Guid`, `Guid`, enum string, `decimal`, `decimal`, `decimal`, `string`, `Guid` | PK/FK | Required; quantity/before/after non-negative rules | Lịch sử biến động tồn |
| `Expense.Id`, `BranchId`, `StockReceiptId`, `Amount`, `ExpenseDate`, `Category`, `PaymentMethod`, `CreatedBy` | `Guid`, `Guid`, `Guid?`, `decimal`, `DateTime`, `string`, `string`, `Guid?` | PK/FK/unique receipt link | StockReceipt/CreatedBy/UpdatedAt nullable; core text required | Chi phí branch |
| `LoyaltyTransaction.Id`, `CustomerId`, `OrderId`, `Type`, `Points`, `BalanceAfter` | `Guid`, `Guid`, `Guid?`, `string`, `int`, `int` | PK/FK/filtered unique Order+Type | Order/description nullable | Giao dịch điểm |
| `Notification.Id`, `BranchId`, `TargetRole`, `TargetUserId`, `EntityId`, `IsRead` | `Guid`, `Guid?`, `string?`, `Guid?`, `Guid?`, `bool` | PK | Target/entity/route nullable | Thông báo |
| `NotificationRead.NotificationId`, `UserId`, `ReadAt` | `Guid`, `Guid`, `DateTime` | Composite PK/FK Notification | Required | Read state theo user |
| `ReceiptSettings.Id`, `BranchId`, `PaperWidth`, `ShowLogo`, `ShowAddress`, `FontSize` | `Guid`, `Guid?`, `int`, `bool`, `bool`, `int` | PK/filtered unique BranchId | Branch nullable; display defaults | Cấu hình hóa đơn |
| `SystemSetting.Id`, `BranchId`, `Key`, `Value`, `Type`, `UpdatedAt` | `Guid`, `Guid?`, `string`, `string`, `string`, `DateTime` | PK/filtered unique BranchId+Key | Branch nullable; key/value/type required | Cấu hình hệ thống |
| `ManualExpenseIdempotencyRecord.Id`, `BranchId`, `UserId`, `IdempotencyKey`, `PayloadHash`, `ExpenseId`, `ResponseJson` | `Guid`, `Guid`, `Guid`, `string`, `string`, `Guid`, `string` | PK/unique Branch+User+Key | Required mapping | Record chống lặp expense |

### 7.2. Quan hệ và ràng buộc quan trọng

- `Order → OrderDetail` 1-N, cascade delete; `OrderDetail → Product/Topping` optional vì một dòng có thể đại diện product hoặc topping.
- `Order → OrderRequest → OrderRequestItem` 1-N và cascade từ request xuống item; OrderRequestItem có ProductId nullable và lưu ProductName snapshot.
- `Branch → Area/Table/Order/Reservation/Expense/Attendance/WorkSchedule/Shift/Inventory/Stock/Settings` là các quan hệ branch trực tiếp hoặc qua Area.
- `Product ↔ Branch` là quan hệ nhiều-nhiều logic qua `ProductBranchAvailabilities`, composite key `(ProductId, BranchId)`.
- `Customer → Order` 1-N optional trên Order; `Customer → LoyaltyTransaction` 1-N.
- `InventoryItem → BranchInventory/receipt item/issue item/transaction` 1-N; BranchInventory unique theo branch/item.
- `StockReceipt → Expense` one-to-one logic qua unique `Expense.StockReceiptId`; xác nhận receipt tạo expense.
- `Notification → NotificationRead` 1-N; NotificationRead composite PK theo notification/user.
- `SystemSetting` và `ReceiptSettings` có unique filtered index cho branch non-null; không được suy ra fallback global tự động nếu service hiện tại không làm.

### 7.3. Đối chiếu 29 bảng trong Word với model hiện tại

Bản Word cũ không còn trong working tree để đọc trực tiếp; số “29 bảng” được nhận diện từ yêu cầu và hồ sơ lịch sử. Bảng dưới đây là đối chiếu audit cần dùng khi sửa Chương 3, trong đó `UNVERIFIED` chỉ việc chưa có bản Word hiện tại/DB thật để đối chiếu, không phải kết luận source thiếu.

| Nhóm/bảng Word cần đối chiếu | Model hiện tại | Kết luận |
|---|---|---|
| Branch | `Branches` / `Branch` | Giữ |
| Area | `Areas` / `Area` | Giữ |
| Table | `Tables` / `RestaurantTable` | Giữ; bổ sung QrToken unique |
| Employee | `Employees` / `Employee` | Giữ; nêu role/branch/active, không gọi salary là payroll |
| Customer | `Customers` / `Customer` | Giữ; thêm registered vs guest distinction |
| Product | `Products` / `Product` | Giữ; SizesJson/ToppingsJson là JSON snapshot/config |
| Topping | `Toppings` / `Topping` | Giữ |
| Product branch status | `ProductBranchAvailabilities` | Có thể là bảng mới so với danh sách 29; phải bổ sung |
| Promotion | `Promotions` / `Promotion` | Giữ nhưng không gán auto discount nếu chưa có flow |
| Order | `Orders` / `Order` | Giữ; financial snapshots nullable |
| Order detail | `OrderDetails` / `OrderDetail` | Giữ; tên hiện tại là OrderDetail |
| Order request | `OrderRequests` / `OrderRequest` | Bổ sung nếu Word chỉ ghi Order |
| Order request item | `OrderRequestItems` / `OrderRequestItem` | Bổ sung |
| Reservation | `Reservations` / `Reservation` | Giữ |
| Shift | `Shifts` / `Shift` | Giữ |
| Attendance | `Attendances` / `Attendance` | Giữ |
| Work schedule | `WorkSchedules` / `WorkSchedule` | Giữ |
| Inventory item | `InventoryItems` / `InventoryItem` | Giữ |
| Branch inventory | `BranchInventories` / `BranchInventory` | Bổ sung/giữ tùy Word |
| Stock receipt | `StockReceipts` / `StockReceipt` | Bổ sung |
| Stock receipt item | `StockReceiptItems` / `StockReceiptItem` | Bổ sung |
| Stock issue | `StockIssues` / `StockIssue` | Bổ sung |
| Stock issue item | `StockIssueItems` / `StockIssueItem` | Bổ sung |
| Stock transaction | `StockTransactions` / `StockTransaction` | Bổ sung |
| Expense | `Expenses` / `Expense` | Giữ; có link receipt/idempotency riêng |
| Loyalty transaction | `LoyaltyTransactions` / `LoyaltyTransaction` | Giữ/bổ sung |
| Notification | `Notifications` / `Notification` | Giữ/bổ sung |
| Notification read | `NotificationReads` / `NotificationRead` | Bổ sung |
| Receipt settings | `ReceiptSettings` / `ReceiptSettings` | Giữ/bổ sung |
| System settings | `SystemSettings` / `SystemSetting` | Giữ/bổ sung |
| Manual expense idempotency | `ManualExpenseIdempotencyRecords` | Bảng hiện hành cần bổ sung nếu Word thiếu |

Danh sách trên có 31 bảng hiện tại; không được ép model hiện tại về đúng 29 bảng của Word. Khi có bản Word mới, cần map từng tên bảng/figure cụ thể và ghi `REMOVED`, `ADDED` hoặc `RENAMED` thay vì gộp imprecise.

### 7.4. Bảng nên trình bày trong mục 3.3.2 và phụ lục

- **Nên trình bày trong thân Chương 3:** Branch, Area, Table, Employee, Customer, Product, ProductBranchAvailability, Topping, Order, OrderDetail, OrderRequest, OrderRequestItem, Reservation, InventoryItem, BranchInventory, StockReceipt/Item, StockIssue/Item, StockTransaction, LoyaltyTransaction, Expense, Notification và SystemSetting/ReceiptSettings ở mức bảng tóm tắt.
- **Nên gộp:** cặp header-detail của stock receipt/issue; Notification và NotificationRead; Employee/Attendance/WorkSchedule/Shift khi giải thích nhóm HR.
- **Nên chuyển phụ lục:** toàn bộ property của 31 bảng, check constraints chi tiết, indexes, migration chronology, audit/idempotency records và bảng model snapshot.
- **Không trình bày như bảng hiện hành:** payroll, BusinessInsight cũ, ProductIngredient/Recipe hoặc PaymentGateway/BankReconciliation vì chưa có entity/table/runtime path tương ứng.

## 8. Thiết kế kiến trúc thực tế

```text
Admin Web / Customer Web
        │ HTTPS REST/fetch
        ▼
ASP.NET Core Controllers + AiController
        │
        ├── Application Services (Order/Product/Kitchen/Inventory/...)
        │       │
        │       └── ApplicationDbContext / Infrastructure
        │                    │
        │                    └── PostgreSQL/Supabase
        │
        └── KitchenHub (/kitchenHub) ⇄ Admin KDS/POS/Notification clients

Admin/Customer AI client
        → AiController → AiOrchestrator → local embedding pilot
        → ToolRegistry → Permission/Authorization → Tool
        → Application Service → DbContext
```

Đây là biểu diễn luồng source, không phải sơ đồ UML mới. Customer Web không có client `@microsoft/signalr` được xác minh; SignalR client đã thấy ở Admin Web `KitchenPage`, `POSPage` và `useNotificationCenter`. Admin và Customer gọi API qua fetch/service; backend là nguồn giá, trạng thái, quyền và tổng tiền.

### 8.1. Thành phần chính

| Thành phần | Vai trò | Source |
|---|---|---|
| Admin Web | Management/POS/KDS/cashier/staff/HR/catalog/settings | `apps/admin-web/src/App.tsx`, route groups |
| Customer Web | menu, order, reservation, profile, QR guest, customer/guest chatbot | `apps/customer-web/src/App.tsx`, pages/features |
| WebAPI controllers | HTTP boundary, DTO, auth attributes, error/status response | `services/api/src/WebAPI/Controllers` |
| Application services | use case, giá/tổng, branch/object rules, transaction | `services/api/src/Application/Services` |
| Domain | entity/enums/inventory rules | `services/api/src/Domain` |
| Infrastructure | EF/Npgsql, DbContext, migrations, JWT service | `services/api/src/Infrastructure` |
| SignalR | bếp, notification, POS refresh/event | `KitchenHub`, `KitchenNotifier`, frontend clients |
| AI module | pilot, orchestrator, registry, tool, permission, confirmation | `services/api/src/AI` |

Không thêm Flutter Web, mobile app hoặc ứng dụng thứ ba vào kiến trúc hiện tại nếu không có source mới.

## 9. Thiết kế tài chính đã truy vết

### 9.1. Nguồn giá

- Product base price lấy từ `Product.Price`.
- Size được đọc từ `Product.SizesJson` và cộng giá size vào unit price.
- Product toppings trong `Product.ToppingsJson` được match theo tên trong Options và cộng giá.
- Topping độc lập dùng `Topping.Price` khi `OrderDetail.ToppingId` có giá trị.
- `OrderDetail.UnitPrice` là snapshot sau server resolve; `OrderDetail.SubTotal = Quantity × UnitPrice`.
- Product/Topping availability, active state và branch availability được kiểm trước khi order.

### 9.2. Công thức

Với các đại lượng `q_i`, `p_i`, `s` là service fee %, `v` là VAT %, `D` là discount:

```text
SubTotal = Σ(q_i × p_i)
ServiceFeeAmount = RoundVND(SubTotal × s / 100)
TaxableBase = SubTotal + ServiceFeeAmount
VatAmount = RoundVND(TaxableBase × v / 100)
TotalAmount = TaxableBase + VatAmount − Discount
```

`RoundVND(x) = Math.Round(x, 0, MidpointRounding.AwayFromZero)`. Service fee và VAT được làm tròn độc lập; tiền dùng `decimal`. Giá trị VAT/service snapshot có thể nullable ở order lịch sử.

Discount hiện được giữ như legacy snapshot; code comment cho biết client discount mới mặc định zero cho tới khi có promotion flow server-side phù hợp. Không viết rằng Promotion entity tự động giảm mọi order.

### 9.3. Hai ví dụ suy diễn từ công thức, không phải kết quả kiểm thử

**Ví dụ 1 – không phí/thuế:** SubTotal 100.000, service 0%, VAT 0%, discount 0 → TotalAmount 100.000.

**Ví dụ 2 – có phí/thuế:** SubTotal 100.000, service 5% → ServiceFeeAmount 5.000; VAT 10% trên 105.000 → VatAmount 10.500; discount 0 → TotalAmount 115.500.

Hai ví dụ chỉ minh họa công thức đọc từ `OrderService`; chưa được chạy như test trong lượt audit.

### 9.4. Sửa order và lịch sử

- Backend tính lại unit price/subtotal/fees/VAT/total khi tạo hoặc cập nhật order theo điều kiện service.
- Dòng đã gửi bếp phải giữ `Quantity >= SentQuantity`; phần gửi thêm được tính từ chênh lệch.
- Order lưu financial snapshots để lịch sử không phụ thuộc settings hiện tại; các field lịch sử nullable không được tự thay bằng zero nếu chưa có business rule.
- Payment cần đối chiếu `Order.TotalAmount` persisted; không tin `clientTotalAmount`.
- Transaction hiện diện ở các flow quan trọng nhưng không được suy thành transaction toàn hệ thống; payment runtime và retry behavior chưa được chạy trong audit.

## 10. Phân quyền và cô lập dữ liệu

### 10.1. Authentication

- JWT Bearer validate signing key, issuer, audience, lifetime, `ClockSkew=0` trong `Program.cs:112-132`.
- Claims gồm subject/name/role/fullName và tùy token có `branchId`, `branchName`, `position`, `customerSessionType`, `customerAuthMethod`, `tableId` (`JwtService.cs:44-81`).
- JWT lifetime thường lấy `Jwt:ExpiryDays`, fallback code 7 ngày; direct guest lifetime giới hạn tối đa 2 giờ.
- `OnTokenValidated` kiểm known role/account state theo startup auth pipeline; không có bằng chứng token revocation store đầy đủ sau khi token phát hành.

### 10.2. Authorization và scope

- Controller `[Authorize(Roles=...)]` là lớp đầu; application service/context kiểm branch/object trong các flow như order, inventory, guest, kitchen.
- `Role` là security authority; `Position` chỉ mô tả nhân sự/UI.
- Admin global; Manager/employee/cashier/kitchen có branch context theo JWT và service helper. Customer/guest scope dựa CustomerId/phone/session/table/branch tùy endpoint.
- AI permission là boundary riêng: role + risk + tool allowlist + branch/context + confirmation cho write.

### 10.3. Endpoint/flow cần giữ cảnh báo, chưa kết luận là lỗi

1. Một số controller dùng `[Authorize]` chung, nên phải đọc tiếp service/object check trước khi mô tả phạm vi; attribute không tự chứng minh ownership.
2. `OrderService.CreateOrUpdateOrderAsync` có read-validate-save; source không đủ để khẳng định race-free khi hai client cùng sửa order.
3. Payment/loyalty có transaction/conditional logic theo use case, nhưng chưa tái hiện retry/double-submit bằng runtime test.
4. Hub group và query branch của admin cần được mô tả cùng `OnConnectedAsync`/JWT logic; không coi client-supplied branchId là quyền.
5. `ReceiptSettings`/`SystemSettings` có filtered unique index nhưng global fallback không được mặc định suy ra; service hiện phải được trích khi viết behavior.

Các điểm trên là `POTENTIAL RISK/UNVERIFIED`, không phải `FAIL` đã tái hiện.

## 11. Thiết kế trợ lý AI hiện tại

### 11.1. Đường gọi

`Admin/Customer client → POST /api/Ai/chat → AiController → AiOrchestrator → AdminEmbeddingPilot hoặc CustomerGuestEmbeddingPilot → AiToolRegistry → AiPermissionService/AiAuthorization → IAiTool → Application Service → DbContext → formatter/response`.

Guest QR dùng `/api/Ai/guest-chat` với session verification và read-only scope. Confirmation write đi qua các endpoint riêng: product price, product name, catalog creation.

### 11.2. Thành phần và trách nhiệm

| Thành phần | Thực tế |
|---|---|
| `AiController` | JWT/context extraction, input/history limits, admin/customer/guest route, flag/environment checks, confirmation endpoint |
| `AdminEmbeddingPilot` | ONNX/tokenizer/catalog; intent score, lexical boundary, slot parse, `ACCEPT/CLARIFY/UNSUPPORTED`; admin intents |
| `CustomerGuestEmbeddingPilot` | customer/guest scope hẹp hơn, menu/order read-only |
| `AiOrchestrator` | điều phối decision → registry → permission → execute/confirmation → formatter; safe failure; không truy cập DbContext trực tiếp |
| `AiToolRegistry` | nhận `IEnumerable<IAiTool>` từ DI và tìm tool theo tên |
| `IAiTool` | Name, Description, AllowedRoles, RiskLevel, JSON schema, ExecuteAsync |
| `AiPermissionService`/`AiAuthorization` | allowlist role/tool, read/write risk, branch/context và guest/customer restriction |
| Admin tools | revenue, order list, best sellers, comparison, summary, update price/name, create product/topping |
| Customer tools | menu, my orders |
| Confirmation stores | in-memory token, user binding, type binding, expiry khoảng 5 phút, consume một lần |
| Application services | thực hiện catalog/report/order business rule; AI không EF trực tiếp |

### 11.3. Đối chiếu tên mục 3.6

| Mục cũ | Kết luận | Tên/ranh giới đề xuất |
|---|---|---|
| 3.6.1 Kiến trúc trợ lý | Có thể giữ nếu mô tả local pilot/orchestrator/tool boundary | “Kiến trúc trợ lý AI cục bộ và kiểm soát tool” |
| 3.6.2 Xây dựng prompt và ngữ cảnh | Không phù hợp nếu nói prompt LLM sinh tool call; hiện có input/history/context/parser/catalog | “Ngữ cảnh yêu cầu, catalog tham chiếu và trích slot” |
| 3.6.3 Orchestration và Sequential Tool Calling | Tên cũ quá mức; không thấy chain tool tự chủ | “Điều phối quyết định, kiểm quyền và thực thi tool” |
| 3.6.4 Tool Registry | Giữ, có source rõ | “Tool registry và contract schema” |
| 3.6.5 Kiểm soát quyền và xác nhận | Giữ, là boundary quan trọng | “Risk, permission và confirmation boundary” |
| 3.6.6 Business Insights | Business Insights module cũ đã bị drop; dashboard summary/revenue vẫn còn | “AI read tools cho báo cáo và catalog” |

Không đưa `IAiProvider`/provider cũ, Gemini, LLM prompt engineering hoặc Function Calling vào kiến trúc hiện hành. Nếu cần nói về chúng, chỉ đặt ở phần lịch sử/loại bỏ với nhãn `REMOVED`.

## 12. Thiết kế giao diện và ảnh minh chứng

### 12.1. Route/chức năng hiện tại

| Khu vực | Frontend/route | Ảnh nên chụp |
|---|---|---|
| Quản trị tổng quan | Admin Management `/dashboard`, reports, customers, expenses | dashboard KPI, revenue/best sellers, expense |
| Chi nhánh/cấu hình | Admin Management settings/branches/system/receipt | branch list/form, system settings, receipt settings |
| Catalog | Admin `/products`, topping, promotion | product list/form/availability, topping, promotion |
| POS | Admin Management/Cashier `/pos`, Staff `/staff/sales` | menu/cart, options/size/topping, accept order, payment |
| Thu ngân/hóa đơn | Cashier routes, invoice | payment modal, persisted total/receipt, invoice history |
| KDS | Admin Management/Kitchen `/kitchen`, history | active requests, status transitions, history, reconnect indicator nếu có |
| Nhân viên | Staff/Kitchen/Cashier profile, attendance, schedule, shifts | role-specific nav, attendance, schedule, shift open/close |
| Kho | Admin Management `/inventory` | item list, branch overview, receipt draft/confirm, issue confirm, transactions |
| Khách đăng nhập | Customer `/`, `/profile`, `/reservation` | login, menu, options/cart, order history, profile/loyalty, reservation |
| Guest QR | Customer `/scan`, direct menu/session | scan, invalid token, guest menu, order confirmation |
| AI admin | Admin chatbot | read query, write preview, confirmation/cancel, result/refetch |
| AI customer/guest | Customer chatbot/guest-chat | supported menu/order read, unsupported/clarify boundary |

### 12.2. Không được đưa vào ảnh/thiết kế nếu không có source

- Flutter Web hoặc mobile app thứ ba.
- Customer Web trực tiếp kết nối KitchenHub.
- Gemini/external LLM chat window.
- Business Insights page/API đã bị loại bỏ.
- Payroll calculation, bank reconciliation hoặc recipe inventory.

## 13. Danh sách bảng và hình cần có trong Chương 3

### Bảng nên giữ/sửa

1. Ma trận actor–use case–role–branch scope.
2. Danh sách F01–F12 đã chỉnh trạng thái và giới hạn.
3. Bảng yêu cầu phi chức năng kèm cột “bằng chứng/đo lường”.
4. Bảng đặc tả UC QR, POS, accept, KDS, payment, AI.
5. Bảng trạng thái Order và OrderRequest tách biệt.
6. Bảng entity/database tóm tắt theo nhóm, không ép 29 bảng nếu model có 31.
7. Bảng API/controller/service/SignalR trace.
8. Bảng công thức tài chính và nguồn giá.
9. Bảng quyền AI/risk/confirmation.
10. Bảng route frontend theo role.

### Hình nên giữ/sửa hoặc chụp lại

1. Use Case tổng quát bảy actor.
2. Use Case Customer/Guest tách registered/QR/direct session.
3. Use Case vận hành: manager/employee/cashier/kitchen.
4. Use Case quản trị/admin.
5. Activity QR ordering.
6. Activity accept web order.
7. Activity KDS theo RequestNumber/SentQuantity.
8. Activity payment + loyalty.
9. Activity AI hiện tại.
10. Sequence payment.
11. Sequence POS–Backend–KDS–SignalR.
12. Sequence AI Controller–Pilot–Registry–Permission–Tool–Service.
13. Domain class diagram gồm các entity hiện tại và các nhóm stock/notification/settings.
14. Component/deployment view gồm hai web, API, PostgreSQL và SignalR; không thêm app không tồn tại.
15. Screenshot route/role, POS, KDS, customer QR, inventory, AI confirmation làm minh chứng triển khai.

## 14. Đối chiếu từng mục 3.1–3.7

| Mục | Kết quả audit | Nội dung ChatGPT được phép dùng | Nội dung phải sửa/không khẳng định |
|---|---|---|---|
| 3.1 Phân tích yêu cầu/quy trình | IMPLEMENTED/PARTIAL theo F01–F12; actor và flow có source | Bảy actor, branch scope, QR/POS/KDS/payment/inventory/HR | Không đánh đồng menu với quyền, không gọi Business Insights/payroll hiện hành |
| 3.2 UML/quy trình mô hình hóa | PARTIAL vì chỉ audit thuyết minh lịch sử, không kiểm hình hiện tại | Đặc tả actor/message/condition ở mục 6 | Không khẳng định hình cũ khớp source; không tự tạo hình trong audit |
| 3.3 Database | IMPLEMENTED model/mapping/migrations; schema applied UNVERIFIED | 31 bảng snapshot, quan hệ/index/check quan trọng | Không nói schema thật giống snapshot; sửa danh sách 29 bảng |
| 3.4 Kiến trúc | IMPLEMENTED layered/DI/REST/SignalR; Clean compliance PARTIAL | Hai frontend, API, services, DbContext, hub, AI module | Không thêm Flutter/third-party; không gọi Clean Architecture tuyệt đối |
| 3.5 Security/branch | IMPLEMENTED nhiều guard; exhaustive/runtime UNVERIFIED | JWT, role, branch, guest/customer, AI permission | Không suy controller attribute đủ cho ownership mọi endpoint |
| 3.6 AI | IMPLEMENTED có điều kiện flag/artifact; old provider REMOVED | Local embedding pilot, orchestrator, tool registry, permission, confirmation | Bỏ LLM/Gemini/IAiProvider/Sequential Tool Calling/Business Insights cũ |
| 3.7 UI | IMPLEMENTED routes/components; runtime/manual screenshots chưa audit lượt này | Admin/Customer role routes, POS/KDS/QR/AI | Không dùng menu frontend làm proof authorization; không thêm app không tồn tại |

## 15. Những điểm cần kiểm tra bổ sung trước khi viết chính thức

- Đọc bản Word Chương 3 hiện hành nếu có bản mới để map chính xác từng caption, bảng và số hình; file cũ hiện chỉ còn trong Git history.
- Kiểm tra DB thực tế: `__EFMigrationsHistory`, bảng/column/index/check/FK đã apply và khác biệt so với snapshot.
- Chạy manual/API test cho QR invalid, cross-branch, duplicate accept, concurrent send-to-kitchen, payment double-submit, loyalty duplicate, inventory idempotency và expired AI confirmation.
- Kiểm tra Customer Web QR build/runtime vì `QRScan.tsx` gọi `window.jsQR` nhưng manifest Customer không khai báo package tương ứng.
- Kiểm tra mọi endpoint có `[Authorize]` chung bằng ownership/branch condition thực tế trước khi viết câu “cô lập dữ liệu tuyệt đối”.
- Kiểm tra artifact model/tokenizer/catalog, flag môi trường và đường inference trước khi khẳng định AI embedding đã chạy trong môi trường trình diễn.

## 16. Trạng thái kiểm thử audit

| Hạng mục | Trạng thái trong lượt này | Lý do |
|---|---|---|
| Build backend/frontend | NOT RUN | Audit chỉ đọc source |
| HTTP endpoint | NOT RUN | Không khởi động API |
| QR/mobile | NOT RUN | Không điều khiển browser/device |
| SignalR delivery/reconnect | INSPECTED tĩnh, runtime NOT RUN | Có producer/consumer/config nhưng chưa chạy |
| Database schema/migration applied | UNVERIFIED | Không kết nối database |
| AI inference/tool execution | INSPECTED tĩnh, runtime NOT RUN | Chưa load artifact/gọi endpoint |
| UML hình | NOT INSPECTED trực tiếp | Không có bản Word trong working tree hiện tại |

Audit này là hồ sơ bằng chứng cho ChatGPT viết lại Chương 3, không phải nội dung Chương 3 và không thay thế kiểm thử thực tế.

