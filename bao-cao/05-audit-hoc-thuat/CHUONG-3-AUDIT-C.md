# Chương 3 — Audit lượt C: tài chính, bảo mật, AI và giao diện

## 1. Phạm vi và phiên bản

Audit tĩnh ngày 16/10/2026 (Asia/Saigon), nhánh `main`, HEAD `38bbd423b963aaf5779d4ae3356f942ff236643b`; working tree có sửa đổi chưa commit ở frontend ChatBot/route, AI backend, checklist và tài liệu kỹ thuật. Đọc Word gốc SHA-256 `D26DCE6C0437D9E54AB66862E40888C3A2EF21CE565A0016379294A7AE11ABF9`, phần thân B946–1136, Bảng 3.39–3.41. Đối chiếu checkout hiện tại, gồm các sửa đổi chưa commit; không sửa code, Word hoặc bản nháp chương. Không chạy build/test/runtime/DB/AI, không xác minh ảnh hay số trang. `B###` là thứ tự paragraph/table trong OOXML.

## 2. Đề cương Word nguyên gốc đã audit

| Mục Word | Vị trí | Trạng thái |
|---|---:|---|
| 3.4 Thiết kế xử lý đơn hàng và thanh toán | B946–980 | Đã audit toàn bộ công thức/thuyết minh |
| 3.4 Kiểm soát tính toàn vẹn dữ liệu tài chính | B981–1001 | Đã audit toàn bộ, gồm Loyalty |
| 3.5 Thiết kế xác thực, phân quyền, cô lập dữ liệu: phiên; vai trò; object; branch | B1002–1041; Bảng 3.39/B1013 | Đã audit nội dung và từng hàng bảng |
| 3.6 Thiết kế Trợ lý ảo AI Agent: kiến trúc, prompt, orchestration, registry, permission/confirmation, Business Insights | B1042–1099; Bảng 3.40/B1067, 3.41/B1076; Hình 3.17 | Đã audit nội dung, bảng; không audit hình |
| 3.7 Thiết kế giao diện: quản trị, POS, KDS, Customer QR, chat | B1100–1136; Hình 3.18–3.22 | Đã audit thuyết minh/route; không audit hình thức hình |

## 3. Ma trận theo từng mục Word

| Vị trí và trích gốc | Nhãn | Dữ kiện sửa/giữ và bằng chứng ngắn | Mức |
|---|---|---|---|
| B947–948: “backend xác định… đơn giá… giá sản phẩm, kích thước, topping” | GIỮ NGUYÊN, BỔ SUNG | `OrderService.cs:286-357` truy Product.Price; size nhận từ chuỗi `ProductName (Size)` và `SizesJson`; topping option tách tên dấu phẩy từ `Options`, đối chiếu `ToppingsJson`; dòng chỉ có ToppingId lấy `Topping.Price`. Nếu Product/Topping không tìm thấy tại bước tính, `unitPrice` có thể còn 0. Không mô tả code là kiểm giá trị mọi option chống giả mạo tuyệt đối. | Tĩnh code, chưa HTTP/test |
| B949–976 công thức S→F→VAT→D, ví dụ 200.000/5%/10% | GIỮ NGUYÊN, BỔ SUNG | `OrderService.cs:361-402`: phí và VAT từng khoản làm tròn 0 VND `AwayFromZero`; VAT trên `S+F`; `T=S+F+V-D`. Bổ sung điều kiện enable/rate và discount. Ví dụ Word số học đúng nhưng giả định, không phải cấu hình runtime. | Tĩnh code/toán |
| B977: “Đơn giá… lưu trong OrderDetail… phân biệt với giá danh mục có thể thay đổi” | BỔ SUNG giới hạn | `OrderService.cs:356` lưu UnitPrice, nhưng mỗi lần `CreateOrUpdateOrderAsync` gọi `RecalculateOrderFinancials` (`:224-225,259-260`), tra **giá danh mục hiện tại** cho mọi dòng (`:286-355`). Vì vậy không khẳng định đơn giá cũ bất biến khi một đơn còn mở được cập nhật. `Order` snapshot phí/VAT nullable cho lịch sử (`Order.cs:19-23`). | Tĩnh code/CLR |
| B978–980: số tiền >0, bằng TotalAmount, tiền mặt/chuyển khoản; chuyển khoản cần xác nhận ứng dụng, không webhook | GIỮ NGUYÊN | `OrderController.cs:395-417` đúng hai phương thức và so persisted total; `OrderService.cs:507-537` conditional update. VietQR URL ảnh từ frontend `POSPage.tsx:1639` dùng thông tin branch và total; không thấy bank webhook handler trong `services/api/src` khi tìm `webhook/VietQR`. | Tĩnh producer/UI/search |
| B981–1001: tài chính, Loyalty `floor(T/K)`, K mặc định 10.000 | GIỮ NGUYÊN, BỔ SUNG | `LoyaltyService.cs:50-95` chỉ Earn khi CustomerId, total dương, PaidAmount≥TotalAmount, status hoàn thành, chưa Earn; `K` từ branch setting `loyaltyPointsPerAmount` mặc định 10.000. `:162-228` Redeem trước thanh toán; `:232-293` Refund/Reversal được gọi từ hủy trạng thái `OrderService.cs:618-622`. Enum/comment các Type không chứng minh mọi đường sử dụng giống nhau. | Tĩnh code |
| B1004–1009: JWT Bearer, hash/legacy, guest token | GIỮ NGUYÊN, BỔ SUNG | `Program.cs:109-129` ký/issuer/audience/lifetime, zero clock skew; `:144-212` đối chiếu role/trạng thái nhân sự/branch và Customer registered với DB. Guest `guest`/`directGuest` được chấp nhận theo marker, không tra Customer. `AuthController.cs:129-145,292-310` kiểm active và `LegacyPasswordVerifier`. Không ghi mật khẩu plain text như chính sách hiện tại; legacy chỉ cơ chế tương thích. | Tĩnh DI/controller |
| B1011–1021/Bảng 3.39: quyền role; “Guest bị từ chối khi sử dụng AI Agent” | SỬA SAI một số ô/ghi chú | Bảng quyền từng hàng dưới đây. `AiController.cs:116-120` cấm QR `customerSessionType=guest` tại `/chat`; nhưng `:158-197` có `/guest-chat` anonymous Development+Mock, `ProcessGuestReadOnlyAsync` allowlist menu. Do đó “Guest hoàn toàn bị từ chối AI” sai. Kitchen có AllowedRoles ở nhiều tool nhưng `/api/Ai/chat` (`:64-66`) không cho Kitchen; đừng đánh ✓ theo tool metadata một mình. `create_booking` được đăng ký nhưng customer allowlist không cho (`AiToolRegistry.cs:12-17,54-60`, `AiPermissionService.cs:15-19,38-43`). | Tĩnh controller/registry |
| B1023–1041: `A=I∧R∧S∧B`, branch/object scope | GIỮ NGUYÊN như nguyên tắc, BỔ SUNG giới hạn | `OrderController.cs:608-685` có `TryResolveBranch`, `HasBranchAccess`, `OwnsOrder`, guest ownership; Inventory service lọc branch (`InventoryService.cs:624-640`); `KitchenHub.cs:10-44` group theo role/branch. Công thức là mô hình diễn giải, không chứng minh **mọi** endpoint kiểm đủ bốn yếu tố. Product/Topping/Customer không có BranchId bắt buộc; ProductBranchAvailability là trạng thái theo branch. | Tĩnh code/thiết kế |
| B1046–1055/Hình 3.17: “GeminiService… Gemini đảm nhiệm xử lý ngôn ngữ” | SỬA SAI | `Program.cs:304` DI `IAiProvider→MockAiProvider`; `MockAiProvider.cs:8-28,53` deterministic Development-only, intent luật; `AiOrchestrator.cs:33-57,166` dùng interface. Giữ Controller/Orchestrator/registry/permission/tool, thay provider mô tả đúng cơ chế. Không có LLM external đã nối runtime. | Tĩnh DI/call path |
| B1057–1062: “prompt… Gemini tổng hợp phản hồi”; nội dung hội thoại trước | SỬA SAI, BỔ SUNG | `AiOrchestrator.cs:113-166` build instruction/context + tối đa 6 history input do client gửi qua `AiController.cs:74-76`; Mock trả mẫu/tool result, không suy luận LLM. History là client React state gửi request, không thấy server lưu chat; không gọi là bộ nhớ bền. | Tĩnh code/search |
| B1063–1071/Bảng 3.40: GeminiService ở bước 4, “Sequential Tool Calling… năm vòng” | SỬA SAI một thành phần, GIỮ NGUYÊN giới hạn | Bước 4 là `IAiProvider/MockAiProvider`, không GeminiService. `AiOrchestrator.cs:151-166` tối đa 5 vòng provider; `:184-335` xử lý functionCall và kết quả tuần tự, `:339-340` lỗi quá giới hạn. Đây là vòng điều phối theo phản hồi provider, **không** chứng minh planning tự chủ. | Tĩnh code |
| B1072–1084/Bảng 3.41: 14 tool; “create_booking Customer”, “Kitchen được tra cứu qua AI”, “Guest không sử dụng AI” | SỬA SAI khả dụng, GIỮ NGUYÊN đăng ký | `Program.cs:331-348` đúng 14 `IAiTool`. Bảng chi tiết bên dưới phân biệt registration, `AllowedRoles/RiskLevel`, registry allowlist, controller entry và kiểm quyền thực thi. `create_booking` Customer đăng ký nhưng bị allowlist chặn. Kitchen không qua `/chat`. Guest public chỉ menu. Caption Word “Bảng 3.41.Danh sách…” thiếu khoảng cách là hình thức; người dùng tự quản, không sửa Word. | Tĩnh DI/controller/registry |
| B1085–1092: “update_product_price Admin, xác nhận riêng” | GIỮ NGUYÊN, BỔ SUNG | Tool chỉ chuẩn bị token; Orchestrator dừng tại confirmation (`AiOrchestrator.cs:292-303`). `ProductPriceConfirmationStore.cs:24-81` token random 32 byte, gắn userId/productId/expected old/new price, 5 phút, remove khi consume; `AiController.cs:255-283` endpoint Admin đối chiếu user và gọi ProductService với expected price. Store in-memory, chưa bền qua restart/multiple instance. Không áp confirmation này cho `update_order_status` hay `create_booking`. | Tĩnh code |
| B1093–1099: insight phát hiện dựa nghiệp vụ, diễn đạt AI, khác Chat | GIỮ NGUYÊN, BỔ SUNG | `ProactiveInsightService.cs:71-81,162-178` phân tích từng branch active, kỳ hôm qua so với hôm kia theo VN timezone qua `FinancialAnalysisService`; `:191-211` ghi EvidenceJson, kỳ hiện tại/so sánh, dedup; `:115-147` sau lưu mới gọi `InsightExplanationService` và notification. `InsightExplanationService.cs:13-92` dùng cùng IAiProvider→Mock, nội dung diễn đạt theo mẫu; lỗi để null. Không gọi đây là suy luận LLM thực hoặc đảm bảo job đã chạy. | Tĩnh code/DI |
| B1102–1129/Hình 3.18–3.21: giao diện quản trị/POS/KDS/QR | GIỮ NGUYÊN, BỔ SUNG | `ManagementRoutes.tsx:35-77` có dashboard, kho, POS, KDS, BusinessInsights; `StaffRoutes.tsx:23-35`, `CashierRoutes.tsx:24-37`, `KitchenRoutes.tsx:22-33` phân vai UI; Customer routes `customer-web/App.tsx:187-195`. POS “đơn chờ” là `Status=Đang xử lý` + CreatedBy trống, không status riêng. KDS quản lý OrderRequest/đợt. | Tĩnh frontend |
| B1130–1136/Hình 3.22: “ChatBot chỉ tích hợp ở Admin Web… Customer Web chưa chứng minh” | SỬA SAI | `apps/customer-web/src/App.tsx:10,182,195` render ChatBot; `apps/customer-web/src/components/ChatBot.tsx:200-212` gọi `/api/Ai/chat` khi có customerToken, nếu không `/guest-chat`. Admin Web render ở Management/Staff/Cashier, nhưng `KitchenRoutes.tsx:20-33` không render. `apps/admin-web/src/components/ChatBot.tsx:177-190,229-230` gọi chat/confirmation. | Tĩnh frontend/call site |

## 4. Gói dữ kiện tài chính cho người viết

Ký hiệu đúng mã (`OrderService.cs:268-402`): `p_i` là đơn giá backend tính cho dòng (Product.Price + phụ giá size trong SizesJson + các topping được khớp tên trong ToppingsJson, hoặc Topping.Price nếu dòng topping độc lập); `S=Σ Quantity_i×p_i`; `r_s`/`r_v` là rate từ `SystemSettingService.GetSettingsAsync(Order.BranchId)` (`vatPercent`, `serviceFeePercent` hoặc `serviceFee`), áp dụng cờ `vatEnabled/serviceFeeEnabled`, nếu thiếu cờ thì rate >0 được xem enabled. `F=RoundAway(S×r_s/100,0)`; `B=S+F`; `V=RoundAway(B×r_v/100,0)`; `T=B+V-D`. Order mới gán `D=0` bất kể body (`OrderService.cs:244-246`); discount legacy còn trong đơn đang cập nhật (`:162-164,373-376`); Redeem cộng `Discount` và trừ `TotalAmount` theo `LoyaltyService.cs:219-224`.

Ví dụ **giả định để kiểm số học**, không là giá/config đo được: S=200.000 đồng, phí 5%, VAT 10%, D=0 ⇒ F=10.000, B=210.000, V=21.000, T=231.000 đồng. Với S=10.005, phí 5% thì F=RoundAway(500,25)=500; VAT 10% trên 10.505 bằng RoundAway(1.050,5)=1.051; T=11.556 khi D=0. Ví dụ thứ hai chỉ minh họa hai phép làm tròn riêng, không xác nhận tỷ lệ cấu hình thực tế.

Payment: `OrderController.cs:395-417` A/M/C, amount >0, method chuỗi `Tiền mặt` hoặc `Chuyển khoản`, object branch và request amount bằng persisted `TotalAmount`; `OrderService.cs:484-487` execution strategy + transaction, `:507-537` conditional update `Status="Đang xử lý" && PaidAmount<=0 && TotalAmount==amount`, `:540-570` gắn/khởi tạo member tùy yêu cầu, earn rồi commit, `:573-578` trả AlreadyPaid/NotPayable. Không nói frontend total là nguồn tin cậy. VietQR ở `POSPage.tsx:1639` là URL ảnh, branch payment info từ `BranchController.cs:88-89`; không thấy webhook xác nhận ngân hàng. `PaymentCompleted` gửi qua nhóm sau Paid (`OrderController.cs:418-433`), không có consumer thấy được trong phạm vi tìm ở lượt B.

Loyalty: `Earn` (`LoyaltyService.cs:26-159`) chỉ khi paid đủ, member active, status hoàn thành, chưa Earn, floor `TotalAmount/K` với K branch config/default 10.000; `Redeem` (`:162-228`) qua `POST /api/Order/{id}/redeem` A/M/E/C trước paid, `loyaltyRedemptionValue` default 1.000 và `loyaltyMinRedeemPoints` default 0, conditional decrement points; `Refund`/`Reversal` (`:232-293`) gọi từ đường hủy order `OrderService.cs:618-622`. `LoyaltyTransaction.Type` có comment các nhãn trong `LoyaltyTransaction.cs:10`, nhưng phải mô tả đường gọi thực, không suy từ enum/comment rằng mọi nhánh là chức năng độc lập trên UI.

## 5. Ma trận quyền Bảng 3.39 — đối chiếu từng hàng

Ký hiệu `A/M/E/C/K/U/G` như lượt A. `✓` chỉ vai trò và điều kiện đường đi tĩnh; object/branch/setting vẫn có thể từ chối. `G` = guest QR hoặc guest public tùy endpoint. Word không có cột G nên phải giải thích trong ghi chú, không thêm cột bắt buộc.

| Hàng Word B1013 | Kết luận đối chiếu và đường dẫn | Nhãn |
|---|---|---|
| Đăng nhập hệ thống A/M/E/C/K/U ✓ | Staff login theo nhóm + customer-token (`AuthController.cs:118-310`); G bootstrap/direct-session không đăng nhập tài khoản (`GuestController.cs:39-116`). | GIỮ NGUYÊN, bổ sung G khác đăng nhập |
| Xem thực đơn A/M/E/C/K/U ✓ | `ProductController.cs:37` GET công khai; Guest cũng xem được. | GIỮ NGUYÊN, bổ sung public/G |
| Quản lý giá sản phẩm qua AI chỉ A ✓ | `UpdateProductPriceTool.cs:19-22,54-77`; Admin confirmation endpoint `AiController.cs:255-283`; đúng. | GIỮ NGUYÊN |
| Tra cứu danh sách khách A/M/E/C ✓ | `CustomerController.cs:35-36` role đúng; data/branch scope phải xét controller. | GIỮ NGUYÊN, không suy quyền mọi hồ sơ |
| Tạo hồ sơ khách tại quầy A/M/E/C ✓ | `CustomerController.cs:155-156` walk-in role đúng. | GIỮ NGUYÊN |
| Xem đơn hàng cá nhân U ✓ | `OrderController.cs:69-107,481-497` customer identity/ownership; `GetMyOrderTool.cs:26-33` có tool U. G QR có thể xem theo bàn qua Order GET, nhưng không phải lịch sử thành viên; directGuest không bàn bị chặn list. | GIỮ NGUYÊN, bổ sung G scope |
| Gửi yêu cầu đặt món QR U ✓ | `GuestController.cs:39-74`, `OrderController.cs:142-194`; G QR cũng làm được; U đăng ký có thể đặt món theo branch active. | BỔ SUNG ghi chú G |
| Thanh toán A/M/C ✓ | `OrderController.cs:395-396`, branch+status+total check; E/K/U/G không có. | GIỮ NGUYÊN |
| Gửi món A/M/E/C/K ✓ | `OrderController.cs:361-362`, `HasBranchAccess` `:367-369`; đúng dù Kitchen UI chính là request. | GIỮ NGUYÊN |
| KDS request status A/M/K ✓ | `OrderController.cs:567-585`; object branch check. | GIỮ NGUYÊN |
| Tạo/xử lý phiếu nhập A/M ✓ | `InventoryController.cs:51-64`; read có role rộng hơn, confirm A/M. | GIỮ NGUYÊN |
| Tạo/chỉnh phiếu xuất A/M/E ✓, C/K — | `InventoryController.cs:74-78` chỉ base authorize A/M/E/C/K, **không** endpoint-specific E-only; service phải xét `InventoryUserContext` và branch. Word cột C/K có thể sai nếu service không cấm; cần tránh ✓/— từ UI một mình. | CẦN XÁC MINH chi tiết service policy; không giữ ô E-only tuyệt đối |
| Xác nhận xuất A/M ✓ | `InventoryController.cs:80-81` endpoint explicit A/M. | GIỮ NGUYÊN |
| Truy vấn doanh thu qua AI A/M ✓ | `GetRevenueTool.cs:15-18,44-56`; chat A/M, Manager branch claim. | GIỮ NGUYÊN |
| Món bán chạy AI A/M/E/C/K ✓ | Tool AllowedRoles có K (`GetBestSellersTool.cs:15-18`), nhưng `/api/Ai/chat` loại K (`AiController.cs:64-66`), Kitchen UI không chat (`KitchenRoutes.tsx`). K ✓ **không hiệu lực theo đường chat hiện tại**. | SỬA SAI ô K |
| Ca làm việc AI E/C/K ✓ | `GetMyShiftTool.cs:14-17`; K bị chặn ở chat, E/C có. | SỬA SAI ô K |
| Đặt bàn qua AI U ✓ | `CreateBookingTool.cs:15-18` đăng ký nhưng `AiToolRegistry.cs:12-17,54-60` và `AiPermissionService.cs:15-19,38-43` chỉ allow `customer_get_menu`, `get_my_orders` cho U, nên **không thực thi qua chat hiện tại**. Đặt bàn trực tiếp REST vẫn có (`ReservationController.cs:86`). | SỬA SAI ô U cho “qua AI” |
| Cập nhật status đơn qua AI A/M/E/C/K ✓ | Tool `UpdateOrderStatusTool.cs:15-18,41-49` có K, nhưng chat endpoint loại K; A/M/E/C chạy theo branch/status ở `OrderService.cs:691-724`. | SỬA SAI ô K |

Đối với phiếu xuất: `InventoryController` base role A/M/E/C/K nhưng service có kiểm quyền riêng; cần đọc kỹ service `Require...` trước khi quyết định ô C/K. Hồ sơ này chưa cho phép ChatGPT sửa ô đó thành ✓. Đây là **một hàng còn CẦN XÁC MINH**, không phủ định ma trận còn lại.

## 6. AI: tool đăng ký, quyền hiệu lực và điều kiện

`Program.cs:331-348` đăng ký đúng 14 `IAiTool`. `IAiTool.cs:8-23` yêu cầu Name/Description/AllowedRoles/RiskLevel/GetSchema/ExecuteAsync. `AiToolRegistry.cs:43-75` lọc schema theo role, read-only, allowlist và Customer whitelist; `AiOrchestrator.cs:227-269` lookup + guard lại trước thực thi; `AiPermissionService.cs:30-74` role/risk/branch guard. `ToolRiskLevel` phân Read/Write/Destructive, nhưng 14 tool hiện chỉ dùng Read/Write. Bảng sau ghi role metadata **khác** khả dụng qua `/chat` khi Kitchen hoặc Customer whitelist cản.

| Tool `Name` (file class) | AllowedRoles, RiskLevel | Tác vụ/service + kiểm tra thêm; khả dụng qua chat hiện tại |
|---|---|---|
| `get_revenue` (`GetRevenueTool.cs:15-18,44-56`) | A/M, Read | `IDashboardService.GetRevenueByPeriodAsync(period, BranchId)`; A/M qua chat, M branch claim. |
| `update_product_price` (`UpdateProductPriceTool.cs:19-22,54-77`) | A, Write | ProductService tra tên/giá, không sửa ngay; tạo confirmation token, Admin endpoint consume với expected price. |
| `get_active_staff` (`GetActiveStaffTool.cs:13-16,35-39`) | A/M, Read | EmployeeService.GetActiveStaffAsync(BranchId); A/M qua chat. |
| `get_order_list` (`GetOrderListTool.cs:17-20,44-78`) | A/M/E/C/K, Read | OrderService query branch/status/limit, role-scoped response; K metadata có nhưng không qua chat. |
| `get_best_sellers` (`GetBestSellersTool.cs:15-18,41-91`) | A/M/E/C/K, Read | DashboardService theo thời gian/branch, financial detail tùy role; K không qua chat. |
| `get_revenue_comparison` (`GetRevenueComparisonTool.cs:14-17,40-61`) | A/M, Read | Hai kỳ qua DashboardService/BranchId; A/M qua chat. |
| `get_business_summary` (`GetBusinessSummaryTool.cs:14-17,39-81`) | A/M, Read | KPI qua DashboardService/BranchId; A/M qua chat. |
| `get_financial_analysis` (`GetFinancialAnalysisTool.cs:17-20,42-59`) | A/M, Read | FinancialAnalysisService/current–comparison/BranchId; A/M qua chat. |
| `employee_get_table_summary` (`GetTableSummaryTool.cs:14-17,26-34`) | A/M/E/C/K, Read | TableService count theo branch cho staff; K không qua chat. |
| `get_my_shift` (`GetMyShiftTool.cs:14-17,26-34`) | E/C/K, Read | EmployeeService theo UserId/ngày; K không qua chat. |
| `update_order_status` (`UpdateOrderStatusTool.cs:15-18,41-49`) | A/M/E/C/K, Write | Validate `orderCode/newStatus`, OrderService kiểm branch/transition (`OrderService.cs:691-724`); **không confirmation token riêng**; K không qua chat. |
| `customer_get_menu` (`GetMenuTool.cs:18-21,42-49`) | A/M/E/C/K/U, Read | ProductService theo category/BranchId. U qua chat; G public Development `/guest-chat` chỉ tool này; K không qua `/chat`. |
| `get_my_orders` (`GetMyOrderTool.cs:14-17,26-33`) | U, Read | OrderService theo contact từ signed context; U qua chat, G public không được. |
| `create_booking` (`CreateBookingTool.cs:15-18,41-75`) | U, Write | ReservationService có implementation, **nhưng Customer whitelist trong registry/permission chặn**, nên không được gọi qua chat hiện tại. Không viết “AI đặt bàn đã khả dụng”. |

### Đường chạy, lỗi và xác nhận

`AiController.cs:63-76` giới hạn input message ≤1000 ký tự, history ≤6 và mỗi content ≤1000; `/chat` A/M/E/C/U, không K. Orchestrator build context/prompt (`AiOrchestrator.cs:113-121`), gửi `IAiProvider.GenerateContentAsync` (`:155-166`), parse candidate/functionCall (`:169-224`), tra tool (`:227`), chặn guest allowlist/read-only (`:245-255`), `AiPermissionService` (`:257-261`) rồi `ExecuteAsync` (`:269`). Tool error trả `AiResponse.Success=false` (`:305-313`); confirmation trả sớm (`:292-303`); tối đa 5 vòng (`:151-155,339-340`). Đây là function-call **contract/envelope mô phỏng**, không LLM function calling từ Gemini. `MockAiProvider.cs:29-68` nhận intent bằng `MockAiIntentRecognizer`, kiểm tool có trong schema, format kết quả theo mẫu.

Confirmation giá: `UpdateProductPriceTool.cs:54-77` xác minh Admin/UserId/sản phẩm/giá mới, tạo token; `ProductPriceConfirmationStore.cs:24-81` lưu ConcurrentDictionary in-memory, 5 phút, gắn UserId, ProductId, giá cũ kỳ vọng và giá mới, token bị consume một lần; `AiController.cs:255-283` Admin endpoint consume và gọi `ProductService.UpdatePriceByIdAsync` với expected price. Không có cam kết bền vững qua restart hay đa instance. `create_booking` và `update_order_status` không dùng confirmation này.

Guest: `/chat` từ chối marker đúng `guest` (`AiController.cs:116-120`); `/guest-chat` anonymous chỉ trong Development khi `AI:ProviderMode=Mock`, giới hạn menu Read (`AiController.cs:158-197`; `AiOrchestrator.cs:27-31,72-85`). `directGuest` không nằm trong điều kiện chặn cụ thể của `/chat`; vì vậy không thể khái quát “mọi guest bị cấm chat” từ mã. Customer Web ChatBot khi không có `customerToken` chọn `/guest-chat`, khi có token chọn `/chat` (`customer-web/ChatBot.tsx:200-212`). Cần giữ phần này là phát hiện kỹ thuật/giới hạn, không tuyên bố đã khai thác hoặc kiểm runtime.

## 7. Embedding mục tiêu: từng mắt xích và trạng thái

Không có reference/registration `SentenceEmbeddingAdapter` hoặc `IntentDecisionPipeline` trong `services/api/Program.cs`/`services/api/src`; API vẫn `MockAiIntentRecognizer.Recognize` (`MockAiProvider.cs:53`). `.tools/mock-ai-embedding-spike/README.md:1-5` gọi đây là console local-only, không API/chat runner. Profile `profiles/max-targeted-revision-v2.json:3-8,64-65` tự ghi “not integrated”, not auto-loaded. Artifact/manifest hoặc tập evaluation trong spike **không** là kết quả của hệ thống sản xuất.

| Mắt xích | HIỆN TRẠNG nối production | THIẾT KẾ MỤC TIÊU hoặc spike có mã | CHƯA CHỐT/không viết như kết quả |
|---|---|---|---|
| Model/tokenizer | Không trong API DI. | Spike `SentenceEmbeddingAdapter.cs:38-45,209-235` nạp ONNX/tokenizer local; csproj có `Microsoft.ML.OnnxRuntime 1.27.1`, `Tokenizers.HuggingFace 3.23.1`. | Model ID/revision production, artifact provenance/license/phân phối chưa chốt. Manifest spike nêu `intfloat/multilingual-e5-small` revision `614241f...`, chỉ là lựa chọn offline. |
| Preprocessing/embedding | Mock production dùng nhận diện theo luật (`MockAiProvider.cs:53`). | Spike `IntentDecisionPipeline.cs:43,159` normalize; adapter xử lý token/tensor, output `last_hidden_state` 384, mean pooling + L2 (`SentenceEmbeddingAdapter.cs:173-200,224-235`). | Không sao chép thông số spike thành mô tả production. |
| Corpus/tool mẫu | Production dùng luật intent và tool schema, không vector corpus. | Spike `IntentCatalog.cs` giữ câu mẫu để thử nghiệm offline. | Câu mẫu cuối, coverage theo 14 tool và cách cập nhật chưa chốt. |
| Similarity/aggregation | Không có trong API. | Spike `IntentScorer.cs:36-59` cosine và gộp theo profile; profile offline có max/threshold/margin. | Không gọi cosine là xác suất. Chọn scorer/threshold/margin production sau hiệu chỉnh/đánh giá độc lập. |
| Từ chối/làm rõ | Mock rule có response/fallback; không phải score-threshold embedding. | Spike `IntentDecisionPipeline` có các nhánh score/margin/capability/parameter state, chỉ offline. | Mức ngoài phạm vi/câu hỏi làm rõ chưa chốt production. |
| Tham số | Production `MockAiIntentRecognizer` tạo arguments, từng tool validate; orchestrator chuyển JsonElement. | Thiết kế mục tiêu xử lý tham số **riêng** sau nhận diện tool. | Parser/schema/giá trị thiếu/sai/ambiguous chưa chốt; embedding không tự trích tham số. |
| Permission/execute | Registry lọc **trước** provider theo role/readOnly/allowlist (`AiToolRegistry.cs:43-75`); orchestrator + permission lọc **sau** lựa chọn (`AiOrchestrator.cs:227-269`); tool/service kiểm nghiệp vụ. | Giữ boundary này khi đổi recognizer. | Không được bỏ guard vì embedding có điểm cao. |
| Response/history | Mock format kết quả theo mẫu (`MockAiProvider.cs:36-68`); client gửi history tối đa 6, server không thấy persistence chat. | Có thể thiết kế làm rõ/phản hồi theo kết quả service. | Lưu history bền, số bước, chính sách context chưa chốt; không nói embedding sinh văn bản/planning. |

Model card spike tại revision được manifest nêu đã được hồ sơ Chương 2 ghi là trang revision truy cập được, hiển thị MIT, nhưng README đầy đủ/điều kiện phân phối artifact chưa xác minh. Lượt này không truy cập Internet lại và không xác minh hash/runtime mới; dùng mô tả spike có ghi trạng thái, không tuyên bố model đã tích hợp hoặc đạt benchmark AI.

## 8. Giao diện và Business Insights: dữ kiện/giới hạn

| Chủ đề | Dữ kiện cho người viết |
|---|---|
| UI quản trị | `ManagementRoutes.tsx:35-77` Dashboard/BusinessInsights/Product/Expense/Inventory/Employee/POS/KDS; settings routes admin-only `:60-64`. Manager nhìn thấy một số route chung không có nghĩa write Product/SystemSettings. |
| POS/KDS | POS ở Management/Staff/Cashier (`ManagementRoutes.tsx:68`, `StaffRoutes.tsx:25`, `CashierRoutes.tsx:24`); KDS ở Management/Kitchen (`ManagementRoutes.tsx:73-74`, `KitchenRoutes.tsx:22-24`). Order web “pending” không là status riêng; KDS request theo đợt. |
| Customer/Chat | Customer routes `App.tsx:187-195`, ChatBot render `:182,195`; registered dùng `/chat`, no customer token dùng `/guest-chat` (`ChatBot.tsx:200-212`). Admin Web ChatBot render ở Management/Staff/Cashier, không thấy KitchenRoutes render. Không đưa tên tự đặt trong UI code vào văn bản khóa luận; gọi “hệ thống”/“Trợ lý”. |
| History chat | Hai ChatBot dùng React `messages` state, gửi `messages.slice(-6)` (`admin ChatBot.tsx:185-190`; `customer ChatBot.tsx:195-212`); token/guest context dùng local/sessionStorage. Tìm trong `services/api/src` không thấy entity/repository chat history. Không viết đã lưu lịch sử chat server. |
| Insight dữ liệu | `ProactiveInsightService.cs:71-81,162-178` dùng kỳ hôm qua/hôm kia VN timezone trên branch active, FinancialAnalysisService trả Anomalies; `:191-211` EvidenceJson/kỳ/dedup; `:115-147` gọi provider giải thích sau khi lưu. `MockAiProvider.cs:36-42` trả JSON mẫu cho structured insight; `InsightExplanationService.cs:62-92` parse/gán, lỗi giữ null. Phân biệt **quy tắc tính/phát hiện** với **diễn đạt provider mô phỏng**. |

## 9. Phát biểu không được giữ, câu hỏi và handoff

- Không giữ B1049/B1054/B1061/B1065/B1067 “GeminiService/Gemini đang xử lý”; không đưa embedding spike thành cài đặt. Không giữ B1019/B1083 “Guest bị từ chối AI” tuyệt đối; chỉ cấm QR guest ở `/chat`, có public menu chat Development. Không giữ B1136 “Customer Web chưa có chat”.
- Bảng 3.39 và 3.41 phải sửa phân biệt metadata tool vs endpoint/registry reachability: K được khai trong 5 tool nhưng không qua `/chat`; U `create_booking` bị allowlist chặn. Dòng phiếu xuất C/K chưa chốt cho tới khi đọc trọn policy service. Không tuyên bố mọi tool Write có confirmation.
- Phát hiện sản phẩm cần ghi riêng, không sửa code: `directGuest` chưa nằm trong điều kiện cấm `/chat` giống QR guest; Customer `create_booking` đăng ký nhưng bị allowlist chặn; `OrderService` tính lại UnitPrice dòng cũ theo catalog khi cập nhật đơn mở. Đây là audit tĩnh, chưa tái hiện runtime/DB.
- Quyết định chưa chốt: model/version/pooling/normalization/corpus/scorer/ngưỡng/margin/parameter parser/clarification/history/bước cho embedding production; người viết chỉ được trình bày như **thiết kế mục tiêu**. Cần chọn có sửa bảng quyền Word ngay theo checkout dù sản phẩm có thể đổi sau; không được tự viết chức năng AI đặt bàn đã chạy.
- ChatGPT đủ dữ kiện để chỉnh B946–1136, Bảng 3.39–3.41 và thuyết minh Hình 3.17–3.22 theo cấu trúc Word. Không viết lại toàn văn chương, không tạo hình/Word. Còn cần kiểm chứng runtime độc lập về giao nhận Hub, concurrent/payment, schema DB và chất lượng embedding trước khi phát biểu kết quả; lượt này không chạy.
