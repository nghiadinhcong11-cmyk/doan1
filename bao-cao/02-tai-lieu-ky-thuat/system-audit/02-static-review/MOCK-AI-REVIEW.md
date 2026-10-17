# Mock AI — static review

## Phạm vi và bằng chứng

Review này chỉ đọc source/configuration hiện tại tại HEAD `38bbd423b963aaf5779d4ae3356f942ff236643b`, không gọi API, không mở browser, không kết nối database, không chạy tool nghiệp vụ và không chạy build. Working tree tại thời điểm review sạch (`git status --short` không có output). Các tài liệu bàn giao cũ có commit/status khác nhau; source hiện tại là nguồn ưu tiên.

Phạm vi đã rà soát:

- `apps/admin-web/src/components/ChatBot.tsx`, các route `ManagementRoutes`, `StaffRoutes`, `KitchenRoutes`, `CashierRoutes`.
- `apps/customer-web/src/components/ChatBot.tsx`, `App.tsx` và guest/registered routing.
- `AiController`, request/response/context models, `AiContextService`.
- `IAiProvider`, `MockAiProvider`, `MockAiIntentRecognizer`, `AiOrchestrator`.
- `AiToolRegistry`, `AiPermissionService`, `AiAuthorization`, `AiToolValidator`, confirmation store.
- 14 tool implementations và DI/configuration trong `services/api/Program.cs`, `appsettings*.json`.
- AI rules, audit baseline, security/AI reports, findings register và checklist hiện có.

## Luồng thực tế

```text
Admin/staff ChatBot ──POST /api/Ai/chat──┐
Registered customer ChatBot ────────────┤
Guest/customer ChatBot ─POST /api/Ai/guest-chat
                                       ↓
AiController (JWT/guest gate, context từ claims)
                                       ↓
AiOrchestrator (context → prompt/tools → tối đa 5 lượt)
                                       ↓
IAiProvider = MockAiProvider (Development + AI:ProviderMode=Mock)
                                       ↓
MockAiIntentRecognizer → function call hoặc text
                                       ↓
AiToolRegistry → AiPermissionService → IAiTool
                                       ↓
Application service/EF path → tool result
                                       ↓
Mock format result → AiResponse / server confirmation
                                       ↓
ChatBot render text/confirmation
```

Không thấy provider HTTP, Gemini/OpenAI client, fallback provider hoặc external model registration. `IAiProvider` chỉ là abstraction đang được Mock dùng chung.

## Giao diện và endpoint

| Actor | Giao diện source | Endpoint | Ghi chú tĩnh |
|---|---|---|---|
| Admin, manager, employee, cashier | Admin `ChatBot`, được mount trong các route tương ứng; kitchen route không còn ChatBot | `POST /api/Ai/chat` | Backend allowlist role loại kitchen; role/branch lấy từ JWT, không lấy từ body |
| Registered customer | Customer `ChatBot` | `POST /api/Ai/chat` | Token customer hợp lệ mới dùng route này |
| Guest/public customer | Customer `ChatBot` | `POST /api/Ai/guest-chat` | `[AllowAnonymous]`, Development + Mock; chỉ guest menu read-only |
| Admin | Không có UI riêng ngoài ChatBot | `POST /api/Ai/product-price-confirmation` | Admin-only, token server-side |
| Admin | Không phải chat UI | `GET /api/Ai/live-read-only-status`, `GET /api/Ai/runtime-status` | Status/preflight; runtime-status admin-only |

Không thấy session/history server-side hoặc endpoint lấy lịch sử. History là mảng client gửi lại trong mỗi request; state chat chỉ nằm trong React component.

## Implementation follow-up

Source đã được cập nhật sau review:

- `AiOrchestrator` dừng và trả `Success=false` khi tool result thất bại, không gửi lỗi qua Mock formatter để biến thành text success.
- `AiController.Chat` giới hạn message/history/request size; `chat` và `live-read-only-status` loại role `kitchen` bằng server authorization. `KitchenRoutes` không còn mount ChatBot; KitchenHub/KDS/notification/service bếp không bị xóa.
- Hai ChatBot chặn request in-flight, giới hạn history, reset message state khi token/role/branch/guest-session context đổi và bỏ response cũ; reset không xóa cart/token/QR context.
- Tool argument errors chỉ trả chi tiết khi exception phát sinh từ validator; exception argument từ service chuyển thành thông báo an toàn. Write intent/confirmation vẫn chưa được mở và cần chốt policy.

Các điểm này là `Đã sửa source, chờ kiểm chứng runtime`; báo cáo không nâng thành PASS.

## Intent Mock đang hỗ trợ

Recognizer chuẩn hóa chuỗi (bỏ dấu, lowercase, thay ký tự ngoài chữ/số thành khoảng trắng) rồi khớp substring theo allowlist. Các intent có thể chọn tool:

| Intent/code | Tool | Tham số/giới hạn |
|---|---|---|
| `REVENUE_QUERY` | `get_revenue` hoặc `get_business_summary` cho yesterday | today/month/year; thiếu kỳ thì hỏi lại |
| `REVENUE_COMPARISON` | `get_revenue_comparison` | today-vs-yesterday, this/last week, this/last month |
| `PRODUCT_RANKING` | `get_best_sellers` | today/yesterday/week/month; limit cố định 5 |
| `MY_ORDERS` | `get_my_orders` | không có argument; chỉ customer authenticated qua registry/permission |
| `ORDER_STATUS` | `get_order_list` | status và date today/yesterday nếu nhận diện được |
| `ORDER_COUNT` | `get_business_summary` | cần kỳ today/yesterday/week/month/year theo parser |
| `BUSINESS_SUMMARY` | `get_business_summary` | cần kỳ |
| `TABLE_STATUS` | `employee_get_table_summary` | không có argument |
| `MENU_QUERY` | `customer_get_menu` | category tùy marker; guest chỉ được tool này |
| `MY_SHIFT` | `get_my_shift` | không có argument; staff employee/cashier/kitchen |

Text-only outcomes: introduction, inventory unsupported, write unsupported, multi-intent ambiguous, clarification required và unsupported intent. Mock không phải NLU tổng quát; không hiểu mọi câu tự nhiên và không có ý định tự suy đoán ID, branch, customer hay giá.

### Điểm nhận diện cần lưu ý

- Khớp substring, không có parser phủ định. Ví dụ câu chứa `không xem doanh thu` vẫn có thể rơi vào `REVENUE_QUERY`.
- Multi-intent chỉ đếm một số nhóm cố định; các câu nhiều ý ngoài nhóm này có thể chọn tool đầu tiên theo thứ tự rule.
- History client được gắn nhãn `Untrusted conversation history` ở orchestrator, nhưng recognizer vẫn dùng toàn bộ history làm `context` cho follow-up doanh thu.
- Dữ liệu nghiệp vụ trong response tool lấy từ service; không thấy Mock tự bịa order/product/revenue. Nhánh `RequestsStructuredInsight` của provider trả JSON giải thích cố định nếu caller gửi `responseMimeType=application/json`, nhưng chat orchestrator hiện không gửi cấu hình đó.
- Câu write bị chặn ở recognizer trước khi tool selection. Vì vậy Mock chat hiện không tạo được function call cho `update_product_price`, `update_order_status` hoặc `create_booking`.

## Inventory tool và kết quả audit

| Tool | Vai trò | Risk | Scope | Service gọi | Confirmation | Kết quả static |
|---|---|---|---|---|---|---|
| `get_active_staff` | admin, manager | Read | admin toàn hệ thống; manager theo `BranchId` truyền service | `IEmployeeService.GetActiveStaffAsync` | Không | Có đăng ký; Mock không có intent chọn |
| `get_best_sellers` | admin, manager, employee, cashier, kitchen | Read | `BranchId` context | `IDashboardService` | Không | Reachable qua product-ranking; validation period/limit có |
| `get_business_summary` | admin, manager | Read | `BranchId` context; admin global | `IDashboardService` | Không | Reachable; period bắt buộc |
| `get_financial_analysis` | admin, manager | Read | `BranchId` context | `IFinancialAnalysisService` | Không | Có đăng ký; Mock không có intent chọn |
| `get_order_list` | admin, manager, employee, cashier, kitchen | Read | filter gán `BranchId` từ context | `IOrderService` | Không | Reachable qua order-status; role-scoped response |
| `get_revenue_comparison` | admin, manager | Read | `BranchId` context | `IDashboardService` | Không | Reachable; period pair được giới hạn |
| `get_revenue` | admin, manager | Read | `BranchId` context | `IDashboardService` | Không | Reachable; period allowlist |
| `update_product_price` | admin | Write | admin; exact product name | `IProductService` | Có, server token 5 phút | Đăng ký và backend confirmation có; không reachable từ Mock intent hiện tại |
| `employee_get_table_summary` | admin, manager, employee, cashier, kitchen | Read | staff branch; admin global | `ITableService` | Không | Reachable qua table-status |
| `get_my_shift` | employee, cashier, kitchen | Read | `UserId` context, hôm nay | `IEmployeeService` | Không | Reachable qua my-shift |
| `update_order_status` | admin, manager, employee, cashier, kitchen | Write | order service nhận `BranchId`, role | `IOrderService` | Không theo policy source hiện tại | Đăng ký; Mock write guard chặn trước tool |
| `customer_get_menu` | admin, manager, employee, cashier, kitchen, customer | Read | branch context nếu có | `IProductService` | Không | Reachable; guest allowlist chỉ tool này |
| `get_my_orders` | customer | Read | customer identity context | `IOrderService` | Không | Reachable cho registered customer; query service dùng email/phone, không dùng trực tiếp `CustomerId` |
| `create_booking` | customer | Write | customer identity/branch name do service xử lý | `IReservationService` | Không theo policy source hiện tại | Registry/permission customer allowlist loại bỏ; Mock write guard cũng chặn |

## Quyền, branch và confirmation

- `AiController.Chat` tạo `AiUserContext` từ `ClaimTypes.Role`, `NameIdentifier`, `branchId`, identity name; body chỉ chứa message/history. Staff branch-scoped thiếu branch bị orchestrator từ chối.
- `GuestChat` tạo context server-side với `Role=customer`, `IsGuest=true`, không có user/customer/branch ID; guest tool allowlist và read-only gate chỉ cho `customer_get_menu`. Personal-order phrase bị chặn trước.
- Registry lọc theo `AllowedRoles`, read-only risk và customer allowlist (`customer_get_menu`, `get_my_orders`). Permission service kiểm tra known role, guest, customer allowlist, branch tồn tại cho staff, role allowlist và risk level. Orchestrator lặp lại read-only và guest allowlist ở defense-in-depth.
- Tool hiện tại chủ yếu truyền `AiUserContext.BranchId`/`UserId` xuống service; không tool nào nhận branch ID từ model. `AiToolValidator.ValidateBranchIsolation` có tồn tại nhưng không có call site; finding này đã có `SEC-005`, không tạo finding trùng.
- `update_product_price` validate admin, exact product-name uniqueness, price kiểu decimal/min=0, tạo token random gắn user/product/current price/proposed price/expiry. Endpoint xác nhận chỉ nhận token, lấy product/price từ store, yêu cầu admin JWT và service kiểm tra expected current price.
- Store singleton in-memory; restart mất confirmation. Consume dùng remove một lần, có user binding và expiry. Không có nút hủy/server cancel riêng; client chỉ bỏ confirmation khỏi local message.
- Không có bằng chứng source cho mutation trước confirmation của price tool; `CreateConfirmation` trả ngay và orchestrator dừng lượt, không để Mock tự xác nhận. Tuy nhiên các write tool khác không có confirmation trong policy source; không tự kết luận mọi write bắt buộc confirmation.

## Findings

### AI-001 — Tool failure có thể bị trả về như `AiResponse.Success=true`

- **Mức:** High, P1; source-confirmed, chưa runtime.
- **Đường đi:** `AiOrchestrator.ProcessInternalAsync` gửi `toolResult.Error` trong `functionResponse` rồi gọi lại provider. `MockAiProvider.TryReadToolResult` gọi `FormatToolResult` và trả text; orchestrator không phân biệt text đó là lỗi tool hay câu trả lời thành công và dòng trả text luôn tạo `new AiResponse { Success = true }`.
- **Điều kiện:** tool không tồn tại, permission/read-only bị từ chối, argument invalid hoặc service trả lỗi nhưng không tạo confirmation.
- **Ảnh hưởng:** UI/API consumer có thể coi lỗi nghiệp vụ là thành công; retry/alert/telemetry không phân biệt được, trái quy tắc no-fake-success.
- **Sửa tối thiểu đề xuất:** giữ trạng thái tool failure qua contract nội bộ và trả `Success=false`/error sau lượt tool failure; không suy luận thành công chỉ vì provider tạo được text.

### AI-002 — Write/price confirmation được đăng ký nhưng không reachable qua Mock chat

- **Mức:** Medium, P1; source-confirmed, policy mismatch; chưa runtime.
- **Đường đi:** `MockAiIntentRecognizer.Recognize` gọi `IsWriteMutation` trước các read rules và trả `WRITE_INTENT_UNSUPPORTED`. Vì vậy không tạo function call cho `update_product_price`, `update_order_status`, `create_booking`. Registry/permission cũng chỉ cho customer hai read tool.
- **Ảnh hưởng:** UI admin và checklist `FLOW-09` mô tả đổi giá/confirmation qua Mock chat nhưng source chat không thể bắt đầu flow; confirmation endpoint/tool chỉ còn đường gọi trực tiếp trong code/test harness.
- **Sửa tối thiểu đề xuất:** chốt chính sách trước: hoặc ghi rõ mọi write Mock không hỗ trợ và đổi checklist/UI copy, hoặc thiết kế intent riêng cho write + confirmation server-side. Không tự mở write tool cho customer/staff.

### AI-003 — Chat UI cho phép gửi song song và giữ history theo component, không reset theo actor/branch

- **Mức:** Medium, P1; source-confirmed, chưa runtime.
- **Đường đi:** cả hai `ChatBot.tsx` chỉ kiểm tra `text.trim()`, không chặn khi `isTyping`; nút suggestion/form vẫn gọi `handleSend` trong lúc request trước đang chờ. History được lấy từ state cũ của component. Không thấy effect xóa messages khi token, user, branch hoặc logout đổi.
- **Ảnh hưởng:** request/history có thể đảo thứ tự, duplicate reads/confirmation prompts, context cũ hiển thị cho user/branch khác nếu component không bị unmount; lỗi mạng chỉ thêm một bot message dạng text, không có retry có kiểm soát.
- **Sửa tối thiểu đề xuất:** pending guard/disable send, giới hạn history ở cả client và server, reset hoặc gắn conversation state với authenticated identity/session/branch; phân biệt lỗi retry được với response `Success=false`.

### AI-004 — Một số tool trả exception message trực tiếp thành lỗi nghiệp vụ

- **Mức:** Low/Medium, P2; source-confirmed, cần runtime để xác định nội dung thực tế.
- **Đường đi:** `GetRevenueTool` và các tool bắt `ArgumentException` trả `ex.Message`; `UpdateProductPriceTool`/`CreateBookingTool`/`UpdateOrderStatusTool` cũng có các nhánh trả message service/argument trực tiếp. Internal exception path dùng thông báo an toàn, nhưng service có thể ném message hạ tầng.
- **Ảnh hưởng:** có nguy cơ lộ chi tiết persistence/provider hoặc nội bộ cho UI nếu exception/message không được phân loại.
- **Sửa tối thiểu đề xuất:** allowlist thông báo validation/business đã biết; log chi tiết server-side với correlation ID; mọi exception không tin cậy dùng `CreateInternalError`.

### Existing finding không tạo mới

`SEC-005` đã ghi nhận `AiToolValidator.ValidateBranchIsolation` không được tool gọi tập trung. Review này xác nhận lại call-site gap nhưng không ghi trùng. Không chứng minh được bypass hiện tại vì tool đang truyền branch từ context xuống service; cần runtime negative tests.

### AI-005 — Kitchen AI entry point removed and backend role boundary added

- **Mức:** Scope/security hardening; source-confirmed, runtime pending.
- **Đường đi:** `KitchenRoutes.tsx` no longer imports/renders admin `ChatBot`; `AiController.Chat` and `GetLiveReadOnlyStatus` now authorize only admin/manager/employee/cashier/customer. `runtime-status` and price confirmation remain admin-only.
- **Ảnh hưởng:** Kitchen UI no longer advertises AI and a kitchen JWT is denied at the endpoint boundary. KitchenHub, KDS, notification and kitchen services are outside this change and remain in source.
- **Kiểm chứng cần thiết:** Direct kitchen JWT calls to `/api/Ai/chat` and `/api/Ai/live-read-only-status` must return 403; kitchen routes/KDS/notifications must still render and function in an approved runtime.

## Giới hạn và chính sách cần chốt

- Chưa chứng minh runtime: role/branch denial, customer ownership, actual service query scope, confirmation expiry/replay/concurrency, exception payload, rate limiter, request-size behavior, cancellation, SignalR/DB side effects.
- Cần chốt riêng: write tool nào được hỗ trợ trong Mock; write nào bắt confirmation; guest reservation có được phép hay không; customer orders chain-wide hay scope khác; có cần server-side conversation/session history không.
- Không coi `IAiProvider` thiếu provider thật là finding; Mock-only là boundary được yêu cầu và đang được cấu hình.

## Kết luận static

Kiến trúc route → context → orchestrator → Mock → registry/permission → application service hiện hữu và không thấy đường gọi provider ngoài. Guest boundary/read-only và confirmation price có defense-in-depth đáng kể. AI-001, AI-003 và AI-004 đã có source fix, chờ runtime; AI-002 vẫn là policy mở vì write intent chưa được cho phép. Mọi case vẫn `Chưa test`.
