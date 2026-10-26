# AUDIT CHƯƠNG 4 – CÀI ĐẶT THỬ NGHIỆM VÀ ĐÁNH GIÁ

## 1. Phạm vi, phiên bản và giới hạn

- HEAD: 9365c18614666ab4c80eee714403ef73b14edf54 — Update repository README for current system.
- Thời điểm audit: 20/10/2026, Asia/Saigon.
- Working tree không sạch: tài liệu cũ trong bao-cao/05-audit-hoc-thuat đang bị xóa và thư mục audit-khoa-luan chưa commit. Không phục hồi hoặc thay đổi các file đó.
- Đã kiểm tra source backend/frontend, DI/controller/service/entity/DbContext, SignalR, manifest/lockfile, checklist, evidence/log/attestation, README/scripts và Git history.
- Không chạy build, test, API, browser, SignalR, AI inference; không truy cập hoặc ghi database; không cập nhật trạng thái test case.
- Tài liệu Word cũ và audit Chương 4 cũ chỉ dùng để đối chiếu lịch sử, không thay thế source hiện tại.

Kết luận chính: source có đường cài đặt cho các module chính, nhưng phần lớn checklist giao diện/liên vai trò vẫn NOT RUN. Một số kết quả AI được ghi trong checklist là reported/manual hoặc có điều kiện, chưa đủ metadata để audit độc lập. Evidence runtime chủ yếu ngày 10–13/10/2026 và có artifact không còn phù hợp HEAD hiện tại; không dùng để khẳng định kết quả hiện hành.

## 2. Cây thư mục rút gọn

    .
    ├── apps/
    │   ├── admin-web/                 # Admin, POS, KDS, catalog, HR, reports
    │   └── customer-web/              # menu, QR, customer/guest ordering
    ├── packages/shared/               # OpenAPI/generated type area
    ├── services/api/
    │   ├── src/Domain/                # entities, enums
    │   ├── src/Application/           # services, DTOs, use cases
    │   ├── src/Infrastructure/        # EF Core, migrations, JWT/persistence
    │   ├── src/WebAPI/                # controllers, middleware, hubs
    │   └── src/AI/                    # pilots, orchestrator, tools, security
    ├── scripts/                       # HTTPS, dev, diagnostics/manual backend
    ├── ba-test-frontend/              # manual UI/AI checklist
    ├── evidence/ và logs/             # runtime/log/attestation artifacts
    ├── .tools/mock-ai-embedding-spike/ # artifact/prototype support
    └── bao-cao/                       # audit/thesis documents

| Thành phần | Phân loại |
|---|---|
| apps, services/api | IMPLEMENTED source |
| ba-test-frontend | Checklist/manual test design và reported results |
| scripts/diagnostics | Diagnostic/support utilities; chưa chứng minh đã chạy trong lượt này |
| evidence, logs | Historical/observational artifacts cần gắn commit/môi trường |
| .tools/mock-ai-embedding-spike | Artifact/prototype support; pilot hiện tại có tham chiếu config |
| bao-cao/05-audit-hoc-thuat | Archived/historical trong working tree hiện tại |
| tests/ | Không thấy thư mục test hiện hành |

## 3. Bản đồ module

| Module | Frontend | API | Service | Data | Realtime/transaction |
|---|---|---|---|---|---|
| Auth | Admin login, CustomerLogin, session wrapper | AuthController, GuestController | Jwt/auth logic | Employee, Customer, Branch claims | JWT |
| Catalog | Product/Topping/Promotion, DigitalMenu | Product/Topping/PromotionController | ProductService, ToppingService | Product, ProductBranchAvailability, Topping, Promotion | catalog refresh; AI write confirmation |
| QR/order | QRScan, DigitalMenu, POS | GuestController, OrderController | OrderService, ProductService | Table, Area, Branch, Order/Detail | notification tùy flow |
| POS | POSPage, cashier/staff sales | OrderController | OrderService | Order, OrderDetail, Product/Topping | KDS event; accept/payment transaction theo flow |
| KDS | KitchenPage, KitchenHistoryPage | OrderController kitchen endpoints | KitchenService, KitchenNotifier | OrderRequest, OrderRequestItem, OrderDetail | /kitchenHub, notification, reconnect/refetch |
| Payment/Loyalty | PaymentModal, InvoiceHistory, profile | Order, Invoice, Customer controllers | OrderService, LoyaltyService | Order, LoyaltyTransaction, ReceiptSettings | persisted total; transaction theo flow |
| HR | Employee, Attendance, WorkSchedule, Shift pages | HR controllers | EmployeeService/related services | Employee, Attendance, WorkSchedule, Shift | Không có realtime chính |
| Table/reservation | Table/Reservation pages | Table, Area, Reservation controllers | TableService, ReservationService | Area, Table, Reservation, Branch | notification tùy flow |
| Expense/report | Expense, Dashboard, Invoice | Expense/Dashboard/Invoice | DashboardService, expense logic | Expense, Order, Branch, idempotency record | dashboard cache/invalidation |
| Inventory | InventoryPage/subpages | InventoryController | InventoryService | InventoryItem, BranchInventory, stock documents/transactions | confirm dùng execution strategy/transaction |
| Notification | bell/history/center | NotificationController | NotificationService | Notification, NotificationRead | Admin SignalR client |
| AI | Admin/Customer ChatBot | AiController | pilots, AiOrchestrator, registry, tools | tools gọi application services | audit sink, in-memory confirmation |

Customer Web không có client Microsoft SignalR được xác minh; các client thực tế đã thấy ở Admin Web là KitchenPage, POSPage và useNotificationCenter.

## 4. Phân tích module và đường thực thi

### 4.1. Xác thực/phân quyền

Mục tiêu: JWT Bearer, role, branch/customer/guest context.

    Program.cs:122-132
    ValidateIssuerSigningKey = true
    ValidateIssuer = true
    ValidateAudience = true
    ValidateLifetime = true
    ClockSkew = TimeSpan.Zero

Đường gọi: login/customer-token hoặc guest bootstrap → JwtService → JWT claims → controller Authorize → service/object/branch checks.

Bằng chứng: services/api/Program.cs:112-157; Infrastructure/Services/JwtService.cs:21-95; controller attributes. Hạn chế: chưa chạy token expiry/role/branch matrix.

### 4.2. Menu, sản phẩm và tùy chọn

ProductController/ToppingController/PromotionController phục vụ Admin Web và menu Customer Web. OrderService lấy giá Product/Topping từ database; size và topping option được đọc từ cấu hình JSON của Product hoặc Topping entity, rồi ghi UnitPrice snapshot vào OrderDetail.

    detail.UnitPrice = unitPrice;
    subTotal += detail.Quantity * detail.UnitPrice;

Bằng chứng: Application/Services/OrderService.cs:390-450; Domain/Entities/Product.cs; ProductBranchAvailability.cs; ToppingController.cs. Hạn chế: không có entity size/recipe riêng.

### 4.3. QR, POS và order

    QRScan/DigitalMenu
      -> GuestController bootstrap/direct-session
      -> server-issued JWT + branch/table context
      -> OrderController
      -> OrderService.CreateOrUpdateOrderAsync
      -> authoritative price/quantity/branch validation
      -> ApplicationDbContext

Bằng chứng: GuestController.cs:38-116; OrderController.cs:176-194; OrderService.cs:218-496. Hạn chế: create/merge đọc rồi ghi; chưa có bằng chứng runtime chứng minh race-free.

### 4.4. KDS và SignalR

    POS/staff
      -> OrderController.send-to-kitchen
      -> KitchenService
      -> OrderRequest + OrderRequestItem + SentQuantity
      -> DbContext
      -> KitchenNotifier/KitchenHub
      -> Admin KDS/POS/notification clients

Bằng chứng: KitchenService.cs:51-79,164-215; KitchenHub.cs; admin KitchenPage.tsx, POSPage.tsx, useNotificationCenter.ts. Hạn chế: event phát ra không chứng minh browser đã nhận; reconnect chưa chạy.

### 4.5. Thanh toán và Loyalty

PaymentModal → POST /api/Order/{id}/payment → OrderService.PayOrderAsync → kiểm order persisted → payment fields/status → LoyaltyService nếu đủ điều kiện → invoice/response.

Cash/transfer và UI VietQR/branch bank information có đường source. Không thấy webhook ngân hàng hoặc reconciliation tự động. Bằng chứng: OrderController.cs:431-500; OrderService.cs:476-580; LoyaltyService.cs.

### 4.6. Nhân sự, bàn, đặt bàn, chi phí, báo cáo, thông báo

| Module | Frontend/API/service/entity | Giới hạn |
|---|---|---|
| HR | Employee/Attendance/WorkSchedule/Shift controllers, services, entities | BasicSalary không chứng minh payroll |
| Table/reservation | Table/Area/Reservation pages/controllers/services | Endpoint scope khác nhau; chưa runtime test |
| Expense | ExpenseController/service, Expense, idempotency record | Chi phí nhập hàng liên kết receipt; không phải COGS recipe tự động |
| Report | DashboardController/DashboardService: summary, revenue, best sellers, order count | Business Insights module cũ không phải runtime hiện hành |
| Notification | NotificationController/Service, NotificationRead, SignalR notification client | Delivery realtime chưa kiểm chứng |

### 4.7. Kho

InventoryPage → InventoryController → InventoryService → BranchInventory/StockReceipt/StockIssue/StockTransaction.

- Receipt/Issue bắt đầu Draft; update chỉ Draft; Confirm/Cancel kiểm trạng thái.
- Receipt Confirm cộng tồn, ghi IN transaction và tạo Expense liên kết receipt.
- Issue Confirm kiểm đủ tồn, trừ tồn và ghi OUT transaction.
- Idempotency key có unique filtered index theo branch cho receipt/issue; manual expense có record riêng.
- Không thấy ProductIngredient/recipe hoặc tự trừ nguyên liệu theo order.

Bằng chứng: InventoryService.cs:219-457; ApplicationDbContext.cs:250-390.

## 5. Các luồng kỹ thuật quan trọng

### 5.1. Accept Web Order

| Bước | Source/method | Logic |
|---|---|---|
| Endpoint | OrderController.cs:355-396 | POST /api/Order/{id}/accept; admin/manager/employee/cashier |
| Load/check | OrderService.AcceptWebOrderAsync | order tồn tại, branch active/allowed, status và CreatedBy chưa claim |
| Bàn | cùng method | table active/cùng branch/chưa có khách hoặc mang về |
| Transaction | OrderService.cs:550+ | CreateExecutionStrategy + BeginTransactionAsync vì Npgsql retry strategy |
| Concurrent claim | conditional update/check | người sau nhận conflict/invalid state theo code path; chưa chạy hai request |
| Output | controller/service | order/table state và response; realtime side effect chưa runtime kiểm |

Đường gọi: staff/POS → controller → AcceptWebOrderAsync → DbContext transaction → order/table update → response/notifier.

### 5.2. Send to Kitchen và Kitchen Status

| Bước | Source | Logic |
|---|---|---|
| Send endpoint | OrderController.cs:397-420 | admin/manager/employee/cashier/kitchen |
| Pending | KitchenService | Quantity - SentQuantity |
| Batch | OrderRequest/OrderRequestItem | RequestNumber và item snapshot ProductName/Options/Note |
| Update | OrderDetail.SentQuantity | gán bằng Quantity sau khi tạo item |
| Notify | KitchenNotifier/KitchenHub | KDS/notification clients |
| Status endpoint | OrderController.cs:603-635 | admin/manager/kitchen |
| Status | KitchenService | Pending → Preparing → Ready → Completed, có Cancelled theo contract |
| Frontend | KitchenPage/History | handler/refetch/reconnect; chưa runtime test |

Không được ghi “chống gửi trùng hoàn toàn” nếu chưa có test double-click/retry/concurrent.

### 5.3. Recalculate Order Financials

    SubTotal = Σ(Quantity × UnitPrice)
    ServiceFeeAmount = RoundVND(SubTotal × ServiceFeePercent / 100)
    TaxableBase = SubTotal + ServiceFeeAmount
    VatAmount = RoundVND(TaxableBase × VatPercent / 100)
    TotalAmount = TaxableBase + VatAmount - Discount

RoundVND là Math.Round(value, 0, MidpointRounding.AwayFromZero), dùng decimal. Product.Price/Topping.Price/size/options là nguồn giá; UnitPrice và financial snapshots là giá lịch sử. Discount legacy được trừ sau fee/VAT; client discount mới mặc định zero theo code comment.

Ví dụ suy diễn, không phải test: SubTotal 100.000, fee 0%, VAT 0%, discount 0 ⇒ Total 100.000. Với fee 5%, VAT 10%, discount 0 ⇒ fee 5.000, VAT 10.500 trên base 105.000, total 115.500.

### 5.4. Pay Order

    POS PaymentModal
      -> OrderController payment
      -> OrderService.PayOrderAsync
      -> persisted Order.TotalAmount/Status check
      -> payment fields + transaction/conditional update
      -> LoyaltyService when eligible
      -> invoice/response

Role endpoint là admin/manager/cashier. Backend phải dùng Order.TotalAmount, không tin client total. Payment lặp, amount sai, status sai, method sai và loyalty không đủ điều kiện là nhánh lỗi cần test. VietQR chưa có callback/reconciliation được xác minh. Chưa chạy double-submit/payment failure.

### 5.5. Stock Operations

Create Draft → validate branch/item/quantity/key → Confirm → lock BranchInventory → update quantity + StockTransaction → create Expense cho receipt → commit.

Execution strategy + ReadCommitted transaction được thấy trong ConfirmReceiptAsync và ConfirmIssueAsync. Đây là bằng chứng theo use case, không phải transaction toàn hệ thống. Chưa kiểm database constraint/double posting thực tế.

### 5.6. AI Execution

    AiController
      -> AiOrchestrator
      -> AdminEmbeddingPilot hoặc CustomerGuestEmbeddingPilot
      -> AiToolRegistry
      -> AiPermissionService + AiAuthorization
      -> IAiTool.ExecuteAsync
      -> Application Service
      -> response/formatter

IAiTool có Name, Description, AllowedRoles, RiskLevel, schema và ExecuteAsync. Registry chỉ tra tool DI. Permission kiểm role/risk/branch; write catalog/price/name cần confirmation bind user/type, expiry khoảng 5 phút và consume một lần. Tool gọi application service, không gọi EF trực tiếp.

Provider hiện hành là các pilot embedding cục bộ được đăng ký trực tiếp trong Program.cs; không có provider ngoài trong DI source hiện tại. Evidence/log cũ có tên provider cũ phải phân loại historical/stale.

## 6. Thống kê checklist

### 6.1. File và số case

Có 16 file Markdown dưới ba-test-frontend:

- 7 role file: admin 16, manager 12, kitchen 10, employee 9, cashier 11, registered customer 11, guest 11 — tổng 80.
- 1 liên vai trò: 6.
- 4 AI file: admin, registered customer, guest QR và README.
- 4 file hỗ trợ: route map, refresh policy, root README, commands.

Tổng role/liên vai trò: **86 dòng case, tất cả NOT RUN**.

### 6.2. AI case theo ID

| Nhóm | ID thiết kế/ghi trong tài liệu | Trạng thái được ghi |
|---|---:|---|
| Admin AI | 51 nhóm/ID theo ANLU, boundary, create, rename, flag | Summary ghi PASS; raw file có status/summary lặp và thiếu metadata per-case |
| Customer AI | 19 CUS-E5 | 01/02/05/06/08–12: PASS có điều kiện/tạm thời; 07 BLOCKED; 03/13 fixed cần retest; 04 PARTIAL; 14–19 NOT RUN |
| Guest AI | 15 QR-E5 | Chỉ expected/evidence instructions; không có actual/status |

Không cộng chữ PASS trong legend/summary thành số test độc lập. Không gộp “PASS có điều kiện” thành PASS audit. Không biến 86 NOT RUN thành kết quả FAIL.

### 6.3. Bảng kết quả có evidence

| Case/nhóm | Actual | Metadata/evidence | Audit status |
|---|---|---|---|
| Admin AI read/write/rename | Markdown ghi response/tool/preview/confirmation | thiếu per-case HTTP status, commit, environment, timestamp và evidence bundle | Reported/manual, chưa xác minh độc lập |
| Customer CUS-E5-01/02/05/06/08–12 | Markdown ghi tool/response/boundary | chính tài liệu yêu cầu bổ sung correlation/account/branch evidence | PASS có điều kiện/tạm thời |
| Customer CUS-E5-07 | setup không phù hợp: case không order nhưng response có order | chưa xác định đúng tài khoản/setup | BLOCKED theo tài liệu |
| Customer CUS-E5-03/13 | code đã sửa, cần retest | chưa có retest result | FIXED — cần retest |
| Customer CUS-E5-04 | CLARIFY thay vì empty rõ | không đủ bundle | PARTIAL |
| Guest AI | không actual | chỉ checklist | NOT RUN |
| Role/cross UI | không actual | 86 dòng NOT RUN | NOT RUN |

## 7. Evidence runtime và build history

| Artifact | Có thể xác nhận | Giới hạn |
|---|---|---|
| evidence/backend TLS logs | Có log kiểm tra TLS/backend ngày 13/10 | Không chứng minh HEAD hiện tại |
| evidence/ai-diagnostics/backend logs | Có startup/diagnostic logs | Không phải build/test log hiện hành |
| runtime-attestation JSON | 8 file schema runtime-attestation-v1, status READY; sample tạo 13/10, target Test, auto migration disabled | process/validity cũ; sample có safety flag của phiên cũ, không đại diện embedding/write runtime hiện tại |
| logs/ai-uat/harness-ai-chat.log | Có harness log | chưa map đầy đủ case ID/commit/environment |
| README/scripts | Có lệnh build/dev/HTTPS/manual backend | là hướng dẫn, không phải execution result |

Không tìm thấy log build gốc phù hợp HEAD hiện tại. Không chạy build trong audit này. Các số build/test lịch sử trong tài liệu cũ không được dùng làm kết quả current. Unit test project cũ/công cụ test cũ không được coi là suite hiện hành.

## 8. Môi trường kiểm thử

| Thành phần | Source/config hiện tại | Trạng thái |
|---|---|---|
| Backend | ASP.NET Core 8, Kestrel HTTPS 5000 | IMPLEMENTED/config; chưa chạy |
| Admin Web | Vite dev 5173 | documented; chưa chạy |
| Customer Web | Vite dev 5174 | documented; chưa chạy |
| Database | PostgreSQL/Supabase via Npgsql; connection secure config | source/config; DB/schema NOT VERIFIED |
| HTTPS/LAN | setup-https.ps1, PEM loader, DOAN_DEV_LAN_HOST, explicit origins | source/config; runtime current NOT RUN |
| CORS | explicit origins + credentials, không wildcard credentials | IMPLEMENTED source |
| Migration safety | automatic migration false mặc định; opt-in Development isolated DB | IMPLEMENTED rule; audit không migrate |
| AI flags | appsettings Development false; UAT commands bật bằng environment variables | config path; current runtime flag NOT VERIFIED |

## 9. Đánh giá kết quả và hạn chế

### 9.1. Có đường thực thi trong source

Auth/JWT/branch; catalog; QR guest; customer/guest order; POS/accept; KDS/SentQuantity/OrderRequest; payment/loyalty; inventory; HR/table/reservation/expense/dashboard/notification; AI pilot/orchestrator/registry/permission/confirmation.

### 9.2. Chưa có đủ bằng chứng runtime

- 86 role/liên vai trò UI case.
- Guest AI và phần lớn Customer AI.
- Build HEAD hiện tại.
- PostgreSQL schema/migration applied.
- SignalR delivery/reconnect.
- Concurrent accept/send/payment và double posting inventory.
- Accuracy/latency/coverage AI.

### 9.3. Giới hạn nghiệp vụ

- Không thấy bank webhook/reconciliation tự động.
- Không thấy ProductIngredient/recipe hoặc tự trừ nguyên liệu theo order.
- Payroll không phải module runtime hiện hành.
- Business Insights cũ không phải module hiện hành; Dashboard/report vẫn có.
- Customer Web không trực tiếp nối KitchenHub.
- AI không gọi external LLM và không lưu hội thoại lâu dài.

### 9.4. Rủi ro cần phân loại

| Phát hiện | Phân loại | Không được kết luận |
|---|---|---|
| 86 case NOT RUN | NOT RUN | UI đã nghiệm thu toàn diện |
| Customer CUS-E5-07 setup sai | BLOCKED | lỗi chức năng đã tái hiện |
| PASS có điều kiện | REPORTED/PARTIAL EVIDENCE | PASS tuyệt đối |
| log/attestation cũ lệch source | HISTORICAL/STALE | bằng chứng AI HEAD |
| thiếu current build log | UNVERIFIED | build HEAD đã pass |
| concurrency chưa chạy | UNVERIFIED RISK | race-free/idempotent toàn hệ thống |
| Customer QR dùng window.jsQR nhưng manifest không khai báo jsqr | POTENTIAL RISK | lỗi build/runtime nếu chưa chạy |

## 10. Danh sách ảnh/minh chứng cần bổ sung

### Runtime UI

1. Admin login/dashboard/catalog/branch/settings.
2. POS cart có size/topping/note, accept order và payment.
3. KDS active/history với RequestNumber, quantity, status.
4. Customer registered menu/cart/order/profile/loyalty.
5. Guest QR scan hợp lệ/invalid và menu/order.
6. Inventory receipt/issue Draft → Confirmed, overview và history.
7. Employee/attendance/schedule/shift/reservation/notification theo role.
8. Admin AI read, preview, confirmation, cancel, refetch.
9. Customer/Guest AI supported, clarify, unsupported, fail-closed.

### Source code

| Nội dung | File/method/line |
|---|---|
| JWT | Program.cs:112-157; JwtService.cs:21-95 |
| Order price/financial | OrderService.cs:218-496 |
| Accept | OrderController.cs:355-396; OrderService.cs:550+ |
| KDS | KitchenService.cs:51-79,164-215; KitchenHub.cs |
| Payment | OrderController.cs:431-500; OrderService.PayOrderAsync |
| Inventory | InventoryService.cs:219-457; ApplicationDbContext.cs:250-390 |
| AI | AiController.cs; AiOrchestrator.cs; AdminEmbeddingPilot.cs; IAiTool.cs; AiPermissionService.cs |

### Build/evidence

- Terminal backend/admin/customer build, ghi HEAD, timestamp, environment, exit code.
- Network request/response QR bootstrap, order, accept, send kitchen, payment, inventory confirm và AI confirmation; che token/PII.
- SignalR connection/event/reconnect với branch/role.
- Read-only API/DB before-after trong isolated test target.
- AI bundle per case gồm input, role/session, HTTP status, actual intent/tool, response, correlation ID, environment, commit và timestamp.

## 11. Đối chiếu từng mục 4.1–4.3

| Mục | Kết quả | Hướng viết |
|---|---|---|
| 4.1.1 Tổ chức mã nguồn | IMPLEMENTED/INSPECTED | cây source active; phân biệt archive/prototype/checklist |
| 4.1.2 Cài đặt module | IMPLEMENTED trong code, runtime mixed | bản đồ frontend→API→service→data và snippets; không biến source thành PASS |
| 4.2 Kiểm thử | checklist + reported AI; role/cross NOT RUN | nêu số file/ID/trạng thái theo evidence; không dùng số lịch sử 375 |
| 4.2 Build | command/config có, current log thiếu | chỉ ghi lệnh và evidence cần lưu; không nói build current pass |
| 4.2 Environment | HTTPS/CORS/LAN/Test target/migration safety có source | không khẳng định DB/runtime hiện tại chưa chạy |
| 4.2 AI | có Admin/Customer/Guest case, kết quả mixed | phân biệt selection/clarify/unsupported/permission/confirmation; không công bố accuracy/latency |
| 4.3 Results/limitations | đường cài đặt có, minh chứng không đồng nhất | tách IMPLEMENTED, REPORTED, INSPECTED, UNVERIFIED, NOT RUN; định hướng không phải chức năng hoàn tất |

## 12. Kiến thức bắt buộc ChatGPT sử dụng

- Source implementation, reported manual result và audit evidence là ba mức khác nhau.
- Checklist case được thiết kế không phải case đã chạy; case ghi PASS không phải lúc nào có đủ evidence độc lập.
- Log/attestation phải gắn commit, process, environment, database target và validity.
- Không dùng số test/build lịch sử của checkout cũ cho HEAD hiện tại.
- Không công bố accuracy/latency/coverage từ threshold hoặc manual claims.
- Current AI là local embedding pilot với orchestrator/tool/permission/confirmation; không phải external LLM.
- Không đưa bank reconciliation, recipe inventory deduction, payroll hoặc Business Insights cũ vào kết quả đã hoàn thiện.
- Không tự tạo ảnh, log, status hoặc kết quả kiểm thử.

## 13. Phạm vi cố ý không thay đổi

Audit này không sửa code, migration, database, checklist, status case, Word, ảnh, log hoặc evidence. Không thực hiện build/test/runtime verification.

