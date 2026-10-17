# Chương 3 — Audit lượt A: yêu cầu, tác nhân, Use Case và quy trình

## 1. Phạm vi và phiên bản

- Audit tĩnh ngày 16/10/2026 (Asia/Saigon), nhánh `main`, HEAD `38bbd423b963aaf5779d4ae3356f942ff236643b`. Working tree có sửa đổi chưa commit ở frontend chat/route, AI backend, checklist và tài liệu kỹ thuật; các sửa đổi đó là đầu vào hiện tại, không bị thay đổi trong lượt này.
- Word gốc `bao-cao/hệ thống quản lý nhà hàng (ver3).docx`, SHA-256 `D26DCE6C0437D9E54AB66862E40888C3A2EF21CE565A0016379294A7AE11ABF9`. Vị trí `B###` là thứ tự phần tử paragraph/table trong `word/document.xml`, không phải trang. Đọc thân Chương 3 B559–737 cho lượt A; số chương/mục theo heading và numbering Word, caption theo thân.
- Không chạy build/test, HTTP, SignalR, DB hay AI. Không kiểm tra nội dung đồ họa; chỉ đối chiếu thuyết minh/caption với nghiệp vụ. Phần chi tiết dữ liệu, realtime, tài chính, auth và tool thuộc lượt B/C.

## 2. Đề cương Word nguyên gốc đã audit

| Mục trong thân Word | Vị trí | Trạng thái |
|---|---:|---|
| 3.1 Phân tích yêu cầu và quy trình nghiệp vụ; 3.1.1 Các tác nhân và vai trò | B560–571, B565/Bảng 3.1 | Đã audit |
| 3.1.2 Yêu cầu chức năng | B572–584, B575/Bảng 3.2 F01–F12, B583/Bảng 3.3 | Đã audit |
| 3.1.3 Yêu cầu phi chức năng | B585–604 | Đã audit ở mức yêu cầu thiết kế; chưa đo runtime |
| 3.1.4 Các quy trình nghiệp vụ chính | B605–625 | Đã audit |
| 3.2 Phân tích và mô hình hóa hệ thống bằng UML; Use Case tổng quát/Customer–Guest/nhân sự/quản trị | B626–655, Hình 3.1–3.4 | Đã audit thuyết minh, không audit hình |
| Đặc tả UC01–UC06 | B656–675, B658/661/664/667/670/673, Bảng 3.4–3.9 | Đã audit toàn bộ hàng UC |
| Biểu đồ hoạt động QR/tiếp nhận/thanh toán–Loyalty/KDS/AI | B676–707, Hình 3.5–3.9 | Đã audit thuyết minh, không audit hình |
| Biểu đồ trình tự thanh toán/POS–Backend–KDS/AI | B708–729, Hình 3.10–3.12 | Đã audit thuyết minh, không audit hình |
| Biểu đồ lớp miền nghiệp vụ | B730–737, Hình 3.13 | Đã audit lời mô tả; chi tiết quan hệ ở lượt B |

Heading trong Word dùng numbering tự động; `3.1/3.2` ở đây theo cấp heading của Chương 3, không lấy số mục từ danh mục đầu.

## 3. Ma trận theo từng mục

| Vị trí, trích gốc đủ ngữ cảnh | Nhãn | Kết luận/dữ kiện cho người viết | Bằng chứng và mức |
|---|---|---|---|
| B563–565/Bảng 3.1: “sáu vai trò xác thực… Guest… không phải vai trò JWT độc lập”; Admin toàn hệ thống, Manager chi nhánh | GIỮ NGUYÊN, BỔ SUNG | Mô hình Guest đúng theo token role `customer` + `customerSessionType`. Bổ sung `guest` (QR có `tableId`) và `directGuest` (có thể không có bàn); quyền từng nghiệp vụ phải theo endpoint và object check. Admin không mặc nhiên qua mọi điều kiện nghiệp vụ. | `GuestController.cs:59-66` gọi `GenerateToken(...,"customer",...,customerSessionType:"guest")`; `JwtService.cs:27-33` tạo `directGuest`; `GuestSessionContext.cs:11-28`; tĩnh. |
| B567–571: “quyền thực tế còn phụ thuộc từng API… AI dùng ngữ cảnh và quyền người dùng” | GIỮ NGUYÊN, BỔ SUNG | Đúng nguyên tắc. AI không có quyền độc lập; cần phân biệt `/api/Ai/chat`, `/guest-chat`, tool allowlist/permission và frontend chat theo vai trò (lượt C). | `AiController.cs:64-66,165`; `OrderController.cs:608-685`; tĩnh. |
| B575/Bảng 3.2 F01–F12 | BỔ SUNG | Cả 12 nhóm có dấu vết implementation, nhưng nội dung từng nhóm rộng hơn một endpoint. Bảng đối chiếu F phía dưới nêu ranh giới: F03 ghi “giá bán” nhưng write Product/Topping/Promotion là admin; F08 “sử dụng điểm” là endpoint staff, không khẳng định Customer tự đổi; F11 kho theo chứng từ thủ công; F12 Mock provider, không LLM thật. | Controller/service/frontend dẫn trong bảng F; tĩnh, chưa runtime. |
| B583/Bảng 3.3: `Order, OrderDetail`, `OrderRequest`, kho | GIỮ NGUYÊN, BỔ SUNG | Tên entity nghiệp vụ nhìn chung khớp. `OrderDetail` là dòng món; không gọi `OrderItem` nếu nói entity hiện hành. Các quan hệ vật lý cần bảng lượt B. | `OrderService.cs:208-219`; `KitchenService.cs:60-79`; tĩnh. |
| B588–604 yêu cầu phi chức năng: bảo mật, toàn vẹn, phản hồi, bảo trì, sử dụng, kiểm thử; “chỉ tiêu định lượng… khi có phương pháp đo” | GIỮ NGUYÊN | Phù hợp dạng yêu cầu/tiêu chí thiết kế. Không biến thành kết quả đạt được hoặc bảo đảm runtime. Lượt C đối chiếu các kiểm soát thực tế và rủi ro. | Word B585–604; đây là yêu cầu, không phải bằng chứng kiểm thử. |
| B608–609 và B678–683: “quét QR… xác định bàn, chi nhánh… thiết lập phiên”; “QR, bàn hoặc sản phẩm không hợp lệ” | GIỮ NGUYÊN, BỔ SUNG | QR bootstrap kiểm tra token định dạng 43 ký tự, bàn active, quan hệ khu vực/chi nhánh, tên bàn duy nhất trong chi nhánh; tạo guest JWT. Guest tạo đơn với `BranchId`/`TableName` lấy từ claim, body `Id` phải rỗng. Không viết rằng quét QR tự tạo đơn. Có thêm direct-session không cần QR, có thể takeaway. | `GuestController.cs:17,39-74,82-116`; `OrderController.cs:160-194,298-310`; tĩnh. |
| B610–612, B684–689: “đơn web đang chờ… tiếp nhận”; “đơn đã được người khác tiếp nhận… bị từ chối” | SỬA SAI | Đơn web **không có status `Pending` riêng**: `Status="Đang xử lý"`, `CreatedBy` trống là dấu hiệu chưa nhân sự nhận. `AcceptWebOrderAsync` kiểm tra chi nhánh active/claim, status, CreatedBy, bàn active/cùng branch; conditional update bàn chưa `Có khách` và order chưa được claim, trong transaction. Trả conflict/not found/forbidden/invalid table. Không khẳng định toàn bộ cập nhật đơn tránh race; create/merge là đường riêng. | `OrderService.cs:236-246,406-473`; `OrderController.cs:320-354`; tĩnh, chưa đo cạnh tranh runtime. |
| B613–615, B696–701: “phần sản phẩm chưa… yêu cầu chế biến”; “cập nhật số lượng đã gửi” | GIỮ NGUYÊN, BỔ SUNG | `Quantity > SentQuantity` trên từng `OrderDetail`; một `OrderRequest`/đợt mới, `OrderRequestItem.Quantity = Quantity-SentQuantity`, rồi `SentQuantity=Quantity`. Đơn phải `Đang xử lý`, branch active. Trạng thái request `Pending→Preparing→Ready→Completed`, có nhánh `Cancelled`, **không phải status toàn Order**. Chưa chứng minh gửi đồng thời hoàn toàn an toàn chỉ từ code. | `KitchenService.cs:33-79,164-215,294-307`; `OrderController.cs:361-392,567-585`; tĩnh. |
| B616–618, B690–695, B709–714: “thu ngân… tiền mặt hoặc chuyển khoản… backend kiểm tra… Loyalty” | GIỮ NGUYÊN, BỔ SUNG | Payment endpoint còn cho admin/manager; phải bằng `Order.TotalAmount`, status `Đang xử lý` và unpaid, phương thức 2 giá trị. Loyalty earn chỉ có member gắn đơn và đủ điều kiện; chi tiết công thức/transaction ở lượt C. | `OrderController.cs:395-440`; `OrderService.cs:476-580`; `LoyaltyService.cs:26-60`; tĩnh. |
| B619–621: “lập chứng từ… xác nhận… điều chỉnh tồn” | GIỮ NGUYÊN | Khớp kiến trúc kho thủ công; không tự trừ nguyên liệu khi bán món. Chi tiết toàn bộ module ở lượt B. | `InventoryController.cs:17-87`; `InventoryService.cs` các nhánh confirm; tĩnh. |
| B622–624: “gửi yêu cầu đến mô hình Gemini”; “mô hình đề xuất… công cụ” | SỬA SAI | Hiện DI `IAiProvider→MockAiProvider`, dùng nhận diện intent xác định; không có Gemini/LLM runtime trong đường này. Giữ ý kiểm tra tool/quyền/đối số/xác nhận nhưng mô tả theo call path thực. Embedding cục bộ chỉ là thiết kế mục tiêu, spike không phải production. | `Program.cs:304,331-348`; `MockAiProvider.cs:53`; `AiOrchestrator.cs`; tĩnh. |
| B630–655 thuyết minh Hình 3.1–3.4: nhóm Staff, Customer/Guest, quản trị | GIỮ NGUYÊN, BỔ SUNG | Nhóm tác nhân phù hợp ở mức UC. Thuyết minh phải chỉ rõ `Cashier` có payment, `Kitchen` có send-to-kitchen/status Order nhưng KDS request chỉ admin/manager/kitchen; Manager không quản trị tất cả settings; Product/Topping/Promotion write admin. Hình chỉ cần phản ánh nghiệp vụ, không audit nét vẽ. | `OrderController.cs:361-396,567-599`; `SystemSettingsController.cs:12-33`; `ProductController.cs:83-196`; tĩnh. |
| B658/Bảng 3.4 UC01 | BỔ SUNG | Điều kiện “QR hợp lệ” khớp QR path, nhưng Customer đăng ký cũng có path menu/direct branch; tách phiên QR `guest` và `directGuest`. Sau gửi, đơn web có thể merge vào đơn `Đang xử lý` cùng bàn/branch; không mặc định tạo đơn mới. | `GuestController.cs:39-116`; `OrderController.cs:160-194,281-310`; `OrderService.cs:145-160`; tĩnh. |
| B661/Bảng 3.5 UC02 | BỔ SUNG | Backend lấy giá Product/Topping/size, không tin giá body; khi sửa dòng đã gửi, `Quantity` tối thiểu bằng `SentQuantity`; xóa dòng đã gửi giữ phần đã gửi. Create/merge không có conditional update tương đương accept, nên không tự tuyên bố chống race. | `OrderService.cs:176-225,270-402`; tĩnh. |
| B664/Bảng 3.6 UC03 | SỬA SAI | “đơn đang chờ tiếp nhận” là cặp điều kiện `Đang xử lý` + `CreatedBy` trống, không status “chờ” riêng. Bàn nhận có thể do nhân sự chọn; bàn phải active/cùng branch và chưa `Có khách`. Nếu không có `TableId`, service gán `Mang về`. | `OrderService.cs:426-460`; tĩnh. |
| B667/Bảng 3.7 UC04 | BỔ SUNG | “nhân sự gửi món” gồm admin/manager/employee/cashier/**kitchen** ở endpoint; KDS update request chỉ admin/manager/kitchen. Một order có nhiều request theo đợt; không coi hoàn tất một request là hoàn thành order. | `OrderController.cs:361-392,567-585`; `KitchenService.cs:51-79,294-307`; tĩnh. |
| B670/Bảng 3.8 UC05 | GIỮ NGUYÊN, BỔ SUNG | Vai trò ghi đúng. Nêu tiền mặt/chuyển khoản là chuỗi hợp lệ; VietQR nếu có chỉ tạo thông tin chuyển khoản, không phải webhook tự xác nhận. Nêu cặp check controller và update điều kiện tại service; chi tiết ở C. | `OrderController.cs:395-440`; `OrderService.cs:508-537`; tĩnh. |
| B673/Bảng 3.9 UC06: “Orchestrator gửi yêu cầu tới Gemini… Mô hình trả lời hoặc đề xuất công cụ” | SỬA SAI | Thay bằng provider mô phỏng đang DI, tool selection dựa trên luật/intent hiện hành. Không mô tả năng lực LLM, function calling thật, hay embedding đã tích hợp. Giữ nhánh permission, validation, confirmation theo điều kiện thật ở lượt C. | `Program.cs:304`; `MockAiProvider.cs:53`; `AiController.cs:64-66,165`; tĩnh. |
| B702–707/Hình 3.9 và B722–729/Hình 3.12: “gửi yêu cầu đến Gemini”, “Gemini không trực tiếp truy cập DB” | SỬA SAI | Loại tên Gemini khỏi thuyết minh hiện trạng. Trình tự hiện tại: client→AiController→AiOrchestrator→IAiProvider/MockAiProvider→registry/permission/validation/confirmation→tool→application service→response. Có thể trình bày embedding ở tầng thiết kế mục tiêu, ghi rõ chưa nối DI. Không suy vòng tool là lập kế hoạch tự chủ. | `Program.cs:304,331-348`; `AiOrchestrator.cs`; `AiToolRegistry.cs`; tĩnh. |
| B715–721/Hình 3.11: REST ghi nghiệp vụ, SignalR thông báo, giao diện tải lại | GIỮ NGUYÊN, BỔ SUNG | Đúng phân tách kênh. Ghi event/group/consumer chính xác từ lượt B; không nói mọi frontend đều nối Hub. | `KitchenNotifier.cs:18-38`; `KitchenPage.tsx:96-117`; tĩnh. |
| B730–737/Hình 3.13: `OrderDetail`, `OrderRequest` là đợt chế biến; kho gồm `InventoryItem`, `BranchInventory` | GIỮ NGUYÊN, BỔ SUNG | Đúng khái niệm. Quan hệ EF, khóa, delete và bảng Word chi tiết cần đối chiếu ở lượt B; không suy từ biểu đồ ra schema DB thực. | `ApplicationDbContext.cs`; các entity Domain; tĩnh. |

### Bảng đối chiếu F01–F12 (không suy triển khai runtime)

| Mã | Nội dung Word | Hiện trạng tĩnh/giới hạn | Nguồn chính |
|---|---|---|---|
| F01 | Xác thực/tài khoản | Có login, customer-token, me, change-password; trạng thái Employee được kiểm tra trong JWT validation. Không khẳng định mọi đường đổi mật khẩu/thông báo đã test. | `AuthController.cs:118,280,325,350`; `Program.cs:109-190` |
| F02 | Chi nhánh/phân quyền | Admin write Branch; Manager branch-scoped trong nhiều endpoint; không suy tất cả API giống nhau. | `BranchController.cs:56-211`; `OrderController.cs:608-635` |
| F03 | Thực đơn | Product/Topping/Promotion read và write; write Product/Topping/Promotion admin, availability Product cho admin/manager/kitchen. | `ProductController.cs:37-196`; `ToppingController.cs:27-85`; `PromotionController.cs:26-89` |
| F04 | Khu vực/bàn/đặt bàn/QR | Có Area/Table/Reservation, QR token trên Table; write Area admin, Table admin/manager; Reservation public create, các nhánh khác kiểm tra theo endpoint. | `AreaController.cs:27-80`; `TableController.cs:25-152`; `ReservationController.cs:24-169` |
| F05 | QR/POS/đơn web | Có bootstrap/direct session, Order create/merge, accept; web pending được nhận diện `CreatedBy` trống. | `GuestController.cs:39-116`; `OrderController.cs:142-354`; `OrderService.cs:130-265,406-473` |
| F06 | KDS | Có send-to-kitchen, request status/history/detail và Hub; một request là một đợt. | `OrderController.cs:361-392,552-605`; `KitchenService.cs:33-79,164-307` |
| F07 | Thanh toán | Có payment endpoint staff giới hạn, paid conditional update; chưa kiểm chứng ngân hàng/runtime. | `OrderController.cs:395-440`; `OrderService.cs:476-580` |
| F08 | Customer/Loyalty | Có Customer profile/history, Order loyalty và staff redeem; earn khi thanh toán đủ điều kiện. Không suy Customer tự redeem. | `CustomerController.cs:35-214`; `OrderController.cs:444-474`; `LoyaltyService.cs` |
| F09 | Nhân sự | Có Employee, Attendance, WorkSchedule, Shift; quyền mỗi nhánh khác nhau. `BasicSalary` không chứng minh chức năng tính lương. | `EmployeeController.cs:39-317`; `AttendanceController.cs:18-188`; `WorkScheduleController.cs:16-106`; `ShiftController.cs:16-131` |
| F10 | Chi phí/báo cáo/insight | Expense, Dashboard, BusinessInsight endpoints cho admin/manager; logic phân tích cần lượt C. | `ExpenseController.cs:19-148`; `DashboardController.cs:15-32`; `BusinessInsightController.cs:16-156` |
| F11 | Kho | Có danh mục/tồn/phiếu nhập-xuất/lịch sử và UI; nhập/xuất qua chứng từ, chưa thấy auto deduct theo món. | `InventoryController.cs:11-87`; `InventoryService.cs`; `ManagementRoutes.tsx:40-46` |
| F12 | AI | Có Controller/Orchestrator/registry/tool/permission; DI Mock, không Gemini/embedding production. | `AiController.cs:64-66,165`; `Program.cs:304,331-348` |

## 4. Ma trận vai trò theo chức năng

Ma trận chỉ phản ánh các đường tiêu biểu của UC và F01–F12, không thay thế việc đọc từng endpoint. Ký hiệu `A/M/E/C/K/U/G` = admin/manager/employee/cashier/kitchen/customer/guest. `G` mang JWT role customer nhưng có session marker; frontend guard chỉ là UX.

| Route/menu | Endpoint và role tại controller | Service/object/branch scope và ngoại lệ đáng viết |
|---|---|---|
| Admin/Manager `/dashboard`, `/business-insights` (`ManagementRoutes.tsx:35-36`) | `GET /api/Dashboard/summary`, `/api/BusinessInsight` A/M (`DashboardController.cs:16,32`; `BusinessInsightController.cs:17,27`) | Manager cần scope branch trong query/controller; không suy quyền global từ route. |
| Admin/Manager `/products`, `/inventory`, `/employees` (`ManagementRoutes.tsx:38,40-47`) | Product write A; inventory item/receipt write A/M; Employee write A/M (`ProductController.cs:83-196`; `InventoryController.cs:25-64`; `EmployeeController.cs:122-317`) | Menu chung không đồng nghĩa Manager sửa giá Product. Inventory issue create/update còn có base role E/C/K, confirm chỉ A/M; các service xác minh branch/đối tượng. |
| Admin-only settings routes (`ManagementRoutes.tsx:60-64`) | Branch write A; SystemSettings toàn controller A; ReceiptSettings read A/M/E/C, write/logo A (`BranchController.cs:109-211`; `SystemSettingsController.cs:12-33`; `ReceiptSettingsController.cs:11-83`) | Manager có thể đọc ReceiptSettings qua API nhưng frontend settings route admin-only; không khẳng định Manager sửa receipt/system settings. |
| POS A/M `/pos`, E `/staff/sales`, C `/pos` (`ManagementRoutes.tsx:68`; `StaffRoutes.tsx:25`; `CashierRoutes.tsx:24`) | `POST /api/Order` `[Authorize]`; accept A/M/E/C; pay A/M/C (`OrderController.cs:142-145,320-321,395-396`) | Create phải kiểm tra role/session, branch/order; payment kiểm tra persisted total và object branch. `[Authorize]` đơn thuần không tự đồng nghĩa mọi role được sửa mọi đơn. |
| KDS K `/kitchen`, A/M `/kitchen` (`KitchenRoutes.tsx:22`; `ManagementRoutes.tsx:73`) | send-to-kitchen A/M/E/C/K; request active/status/history A/M/K (`OrderController.cs:361-362,552-599`) | `HasBranchAccess`/`TryResolveBranch` giới hạn staff; request status kiểm tra order branch; K không có payment. |
| Customer Web `/`, `/reservation`, `/profile`, `/scan` (`customer-web/App.tsx:187-192`) | Guest bootstrap public; Order create `[Authorize]`; CustomerController các endpoint có quyền khác nhau (`GuestController.cs:39,82`; `OrderController.cs:142-143`; `CustomerController.cs:35-214`) | U có account/ownership, G có `guest`/`directGuest` claims. G QR giới hạn bàn+branch; direct guest không bàn chỉ takeaway. Order GET của guest không bàn bị cấm (`OrderController.cs:69-94`). |
| Chat admin/staff/cashier/customer (`ManagementRoutes.tsx:77`; `StaffRoutes.tsx:35`; `CashierRoutes.tsx:37`; `customer-web/App.tsx:182,195`) | `/api/Ai/chat` A/M/E/C/U; `/api/Ai/guest-chat` đường G; K không nằm trong allowlist chat (`AiController.cs:64-66,165`) | Tool tiếp tục lọc role, allowlist/read-only và kiểm tra thực thi. K route hiện không render ChatBot (`KitchenRoutes.tsx:20-33`). |

Điểm rủi ro cần ghi **phát hiện sản phẩm riêng**, không lồng thành lời cam đoan trong báo cáo: `OrderController.cs:504-545` cho K đổi trạng thái/xóa Order với branch check; điều kiện nghiệp vụ hạn chế terminal, nhưng không đồng nghĩa chỉ người có quyền tài chính được phép mọi cập nhật. `OrderService.cs:145-228` create/merge bằng đọc rồi SaveChanges, không thấy conditional update chống ghi đồng thời tương tự accept; không kết luận an toàn cạnh tranh cho luồng này.

## 5. Trình tự nghiệp vụ chuẩn để kiểm tra UC/Activity/Sequence

1. **QR bootstrap**: frontend gửi `QrToken`; backend kiểm tra mẫu/tra Table active cùng Area/Branch, tránh tên bàn trùng trong branch; phát JWT `customerSessionType=guest`, `branchId`, `tableId`. Direct menu path có `directGuest` 30 phút, có thể không `tableId` (`GuestController.cs:39-116`; `JwtService.cs:27-33`).
2. **Tạo/hợp nhất đơn**: `POST /api/Order` xác minh session; guest lấy branch/bàn từ claims và xóa danh tính member client gửi; Customer đăng ký chọn branch active; staff phải qua branch access. Dòng Quantity >0, tồn tại Product/Topping; service lấy giá DB, tìm order `Đang xử lý` theo `Id` hoặc bàn+branch, merge dòng theo ProductId/ToppingId/Options, giữ `SentQuantity`, tính lại tiền. Order mới là `Đang xử lý`, unpaid, discount body bị bỏ qua (`OrderController.cs:149-310`; `OrderService.cs:130-265`).
3. **Tiếp nhận web order**: A/M/E/C gọi `accept`; điều kiện `Đang xử lý` và `CreatedBy` rỗng, branch active/đúng claim, bàn chọn active/cùng branch/chưa có khách; transaction + conditional updates để claim bàn và order. Không có bàn thì đặt `Mang về`; conflict khi claim không còn hợp lệ (`OrderController.cs:320-354`; `OrderService.cs:406-473`).
4. **Gửi bếp**: role A/M/E/C/K, order branch hợp lệ và `Đang xử lý`; các detail `Quantity>SentQuantity` hình thành một `OrderRequest` mới với số lượng chênh và `SentQuantity` cập nhật; thông báo KDS sau lưu (`OrderController.cs:361-380`; `KitchenService.cs:33-117`).
5. **Cập nhật đợt bếp**: K/A/M gọi PATCH request, object check branch; trạng thái hợp lệ `Pending→Preparing→Ready→Completed`, có thể Cancelled từ các trạng thái chưa cuối. `Completed` dùng conditional update để tránh tạo notification nhân viên trùng từ lệnh lặp; đây là **đợt gọi**, không tự hoàn tất order (`OrderController.cs:567-585`; `KitchenService.cs:164-307`).
6. **Thanh toán/Loyalty**: A/M/C chọn tiền mặt/chuyển khoản; controller so amount với total persisted, service conditional update unpaid+`Đang xử lý`+total bằng amount trong transaction/retry; member hợp lệ có thể earn. Redeem là endpoint khác A/M/E/C, không phải mặc nhiên Customer tự đổi (`OrderController.cs:395-474`; `OrderService.cs:476-580`; `LoyaltyService.cs`).

## 6. AI: hiện trạng, thiết kế mục tiêu, chưa chốt

| Mắt xích | HIỆN TRẠNG được viết như cài đặt | THIẾT KẾ MỤC TIÊU được viết như định hướng | CHƯA CHỐT / không được viết như kết quả |
|---|---|---|---|
| Provider | `IAiProvider` DI tới `MockAiProvider` (`Program.cs:304`); nhận diện intent có luật trong Mock (`MockAiProvider.cs:53`) | Mock kết hợp sentence embedding cục bộ hỗ trợ chọn tool | Model/version/tokenizer/pooling/metric/ngưỡng và tích hợp production; không gọi Gemini/LLM đang chạy |
| Điều phối/tool | Controller→Orchestrator→provider→tool registry/permission→tool; 14 IAiTool đăng ký (`Program.cs:331-348`) | Cho phép từ chối/làm rõ, tách tham số khỏi nhận diện intent | Không suy embedding tự trích tham số, sinh trả lời hoặc lập kế hoạch nhiều bước |
| Quyền/xác nhận | Có kiểm tra role/tool, read-only guard và luồng confirmation cần mô tả chính xác ở C | Quyền và điều kiện nghiệp vụ do backend giữ, phản hồi theo mẫu/kết quả | Không viết mọi thao tác nhạy cảm đã được bảo vệ như nhau khi chưa kiểm tra từng tool |

## 7. Phát biểu không được giữ và căn cứ thiếu

- B623–624, B673/UC06, B704, B724–726: bỏ khẳng định Gemini được gọi trong hiện trạng. Không thay máy móc bằng “embedding đã tích hợp”; spike riêng chưa nối DI.
- B685–687 và B664/UC03: không gọi “chờ tiếp nhận” là một giá trị status của Order; phải diễn giải `Đang xử lý` + `CreatedBy` trống.
- B652–655/Bảng 3.1: không suy Manager có quyền sửa mọi cấu hình, giá và phạm vi global. Product write và SystemSettings/ReceiptSettings write khác với route quản lý chung.
- B699 hoặc UC04: không đồng nhất `OrderRequest.Completed` với `Order.Status=Hoàn thành`; không khẳng định SignalR giao được runtime.
- B604: không gán chỉ số phi chức năng định lượng khi không có đo lường. Lượt này không chạy test nào.

## 8. Câu hỏi chưa giải quyết

1. Có cần trong báo cáo phân biệt `directGuest` với Guest QR ở UC01 không? Code có hai đường; nếu UC chỉ nói QR, giữ UC01 hẹp và giải thích direct menu ở phần giao diện/quy trình khác.
2. Người viết muốn ghi rủi ro create/merge đồng thời và quyền K sửa/xóa Order ở phần hạn chế Chương 4 hay chỉ trong hồ sơ audit? Đây là phát hiện tĩnh, không được viết là lỗi runtime đã tái hiện.
3. Sơ đồ Use Case hiện do người dùng tự quản; nội dung nghiệp vụ phải phản ánh ma trận trên, nhưng lượt này không đánh giá hình hoặc số hình.

## 9. Handoff

ChatGPT đủ dữ kiện chỉnh B563–625, đặc tả UC01–UC06, thuyết minh Hình 3.1–3.12 và câu mô tả lớp miền ở B730–737. Giữ cấu trúc/độ chi tiết của Word. Còn cần lượt B để khóa quan hệ/bảng/realtime, lượt C cho tiền/quyền/AI chi tiết. Không coi bảng audit này là văn xuôi thay thế Chương 3. Không sửa Word hoặc sản phẩm.
