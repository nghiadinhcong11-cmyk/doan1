# So sánh các bộ câu hỏi AI Chat

## 1. Phạm vi và kết luận ngắn

Bộ `ba-test-frontend/ai-chat/` không phải bộ câu hỏi AI đầu tiên của repository. Trước đó repository đã có các case AI rải trong checklist theo vai trò, checklist kỹ thuật/runtime và checklist liên vai trò. Ngoài ra còn có ví dụ boundary/tool, kịch bản demo và thiết kế lịch sử; các nhóm này không có cùng mục đích và không được cộng gộp như case test tay.

Kết luận chính:

- Bộ cũ có 23 ID liên quan trực tiếp đến AI chat, quyền AI hoặc confirmation: 9 case theo vai trò, 3 case liên vai trò/UI-session và 11 case API/kỹ thuật.
- Bộ mới có đúng 100 ID duy nhất: 50 chủ quán, 35 customer đăng nhập và 15 khách QR.
- Bộ mới chủ yếu phân rã các case cũ thành tình huống nhỏ hơn, bổ sung intent boundary, empty/error, stale response, đổi context và kiểm tra kitchen bị từ chối.
- Không nên xóa bộ cũ. Một số case cũ kiểm tra manager/employee/cashier, environment gate, confirmation expiry/replay và API permission; các nội dung này chưa được thay thế đầy đủ trong 100 case.
- Kết quả kỳ vọng của các case cũ về write qua chat thường mâu thuẫn với source Mock hiện tại. Source hiện tại đăng ký một số write/confirmation tool nhưng recognizer Mock từ chối write intent trước khi chọn tool. Đây là khác biệt chính sách cần giữ rõ, không tự đổi kết quả test.

Đếm trong báo cáo này dùng quy ước: mỗi dòng case có ID và có thể ghi kết quả độc lập là một tình huống kiểm thử. Các câu ví dụ không có ID được thống kê riêng, không biến thành case mới.

## 2. Nguồn đã tìm thấy

### 2.1. Bộ case cũ được quản lý trong repository

Các file dưới đây là nguồn case cũ hoặc chỉ mục của chúng:

| Nguồn | Nội dung | Commit/history liên quan |
|---|---|---|
| `ba-test-frontend/chu-quan/00-CASES-BY-PAGE.md` | `ADM-12` — Mock AI chat | Được tổ chức tại `3a36c036e1952c03899b6be47e0f9f003628fdd3`; nội dung trước đó nằm trong chuỗi snapshot ngay trước commit này |
| `ba-test-frontend/quan-ly/00-CASES-BY-PAGE.md` | `MGR-10` — scope và quyền Mock AI | `3a36c036e1952c03899b6be47e0f9f003628fdd3` |
| `ba-test-frontend/nhan-vien/00-CASES-BY-PAGE.md` | `EMP-09` — Mock AI cho nhân viên | `3a36c036e1952c03899b6be47e0f9f003628fdd3` |
| `ba-test-frontend/thu-ngan/00-CASES-BY-PAGE.md` | `CSH-11` — Mock AI cashier | `3a36c036e1952c03899b6be47e0f9f003628fdd3` |
| `ba-test-frontend/bep/00-CASES-BY-PAGE.md` | `KIT-12` — Mock AI bếp | `3a36c036e1952c03899b6be47e0f9f003628fdd3`; hiện không còn phù hợp sau khi bỏ AI bếp |
| `ba-test-frontend/khach-dang-nhap/00-CASES-BY-PAGE.md` | `CUS-10` — customer Chatbot, scope và mutation | `3a36c036e1952c03899b6be47e0f9f003628fdd3` |
| `ba-test-frontend/khach-qr/00-CASES-BY-PAGE.md` | `GST-09`, `GST-10`, `GST-15` — guest Chatbot, environment và guest permission | `3a36c036e1952c03899b6be47e0f9f003628fdd3` |
| `ba-test-frontend/lien-vai-tro/order-to-payment.md` | `FLOW-09`, `FLOW-14` — confirmation server, sai user/hết hạn | `3a36c036e1952c03899b6be47e0f9f003628fdd3` |
| `ba-test-frontend/chung/refresh-session.md` | `REF-07` — invalidation UI và Mock AI | `3a36c036e1952c03899b6be47e0f9f003628fdd3` |
| `ba-test-frontend/ky-thuat-runtime/README.md` | `AI-01..AI-11` — role/tool, branch, ownership, failure, write policy, confirmation, concurrency và redaction | Ban đầu ở `3a36c036e1952c03899b6be47e0f9f003628fdd3`, cập nhật thêm tại `1ef623e94cea83f150006c9a22a10126611a171c` |

Các file `ba-test-frontend/01-chu-quan.md`, `06-khach-dang-nhap.md` và `07-khach-chua-dang-nhap.md` hiện chỉ là pointer đến các file `00-LEGACY-CASES.md`; chúng không phải bộ AI riêng.

### 2.2. Ví dụ, boundary và thiết kế — không tính là bộ case cũ

| Nguồn | Phân loại | Giá trị khi so sánh |
|---|---|---|
| `tests/benchmark/TOOL_BOUNDARIES.md` | Ví dụ câu hỏi cho tool boundary | Có giá trị để kiểm tra ranh giới menu/popularity, revenue/summary/comparison, own orders/order list, table status và shift/staffing; không có ID, trạng thái hay minh chứng |
| `bao-cao/99-luu-tru/design-history/AI_TASK20_ANALYTICS_DESIGN.md` | Thiết kế analytics/tool calling | Có test plan và boundary lịch sử; mô tả provider/tool flow cũ, không phải checklist thực thi hiện tại |
| `bao-cao/99-luu-tru/design-history/AI_TASK21_BUSINESS_ADVISOR_DESIGN.md` | Kịch bản demo và reasoning | Có 5 demo prompt về tổng quan, tăng/giảm doanh thu, món đóng góp, chi phí/lợi nhuận và staffing; nhiều prompt vượt recognizer Mock hiện tại |
| `bao-cao/99-luu-tru/design-history/AI_TASK22_FINANCIAL_INTELLIGENCE_DESIGN.md` | Thiết kế/ ví dụ FACT-INFERENCE-RECOMMENDATION | `get_financial_analysis`, expense breakdown và anomaly là thiết kế đề xuất; không coi là intent Mock chat hiện tại |
| `bao-cao/99-luu-tru/design-history/AI_TASK22_INVENTORY_DESIGN.md` | Thiết kế inventory AI | Các inventory tool là proposed, trong khi Mock hiện tại trả `INVENTORY_UNSUPPORTED` |
| `bao-cao/99-luu-tru/design-history/AI_TASK23_PROACTIVE_INSIGHTS_DESIGN.md` | Thiết kế proactive insight | Là flow background/notification, không phải câu hỏi chat thủ công |
| `bao-cao/01-khoa-luan/ban-chinh/14_AI_he_thong_hien_tai.md` và `system-audit/10-AI-INTEGRATION.md` | Tài liệu mô tả kiến trúc/tool | Dùng để đối chiếu capability và lịch sử, không có bộ case độc lập |

Không tìm thấy một file cũ chứa một bộ 100 câu hỏi AI độc lập đã bị xóa/di chuyển. Git history cho thấy checklist được tổ chức thành role/runtime files tại commit `3a36c036...`; các file AI source/design được hình thành và thay đổi qua các commit `17e931e9...`, `61fc2bac...`, `8b1012ec...`, `c76d4556...`, `fa7a4c72...`, `5d350d70...` và `accc6e1c...`. Kết luận này chỉ có nghĩa là chưa thấy một bộ 100 câu riêng trong các ref/file đã khảo sát, không phủ nhận khả năng có bản ngoài repository hoặc trong tài liệu chưa được commit.

## 3. Đếm và phân loại

| Nhóm | Số câu hỏi/case | Số tình huống độc lập theo ID | Số ID duy nhất | Ghi chú |
|---|---:|---:|---:|---|
| Case cũ — role/chat | 9 | 9 | 9 | `ADM-12`, `MGR-10`, `EMP-09`, `CSH-11`, `KIT-12`, `CUS-10`, `GST-09`, `GST-10`, `GST-15` |
| Case cũ — liên vai trò/session | 3 | 3 | 3 | `FLOW-09`, `FLOW-14`, `REF-07` |
| Case cũ — kỹ thuật/runtime | 11 | 11 | 11 | `AI-01..AI-11`; một số cần API/log/nhiều phiên, không thể kiểm chứng chỉ bằng nhập chat |
| **Bộ cũ có ID liên quan AI** | **23** | **23** | **23** | Không cộng các dòng chỉ mục như `MGR-16`, `EMP-14`, `KIT-15` |
| Bộ mới — chủ quán | 50 | 50 | 50 | `OWN-AI-01..50` |
| Bộ mới — customer đăng nhập | 35 | 35 | 35 | `CUS-AI-01..35` |
| Bộ mới — khách QR | 15 | 15 | 15 | `QR-AI-01..15` |
| **Bộ mới** | **100** | **100** | **100** | ID duy nhất đã kiểm tra trong file |

`tests/benchmark/TOOL_BOUNDARIES.md` có các chuỗi ví dụ không ID; `AI_TASK21` có 5 demo prompt chính. Hai nhóm này được bảo toàn như nguồn tham khảo, nhưng không cộng vào 23 case cũ.

## 4. Bảng ánh xạ case cũ–mới

Quan hệ `phân rã` nghĩa là một case cũ rộng được tách thành nhiều case mới. `một phần` nghĩa là chỉ có một phần hành vi cũ được bao phủ. `không còn phù hợp` nghĩa là kỳ vọng cũ mâu thuẫn với source/chính sách hiện tại.

| Nguồn/ID cũ | Vai trò | Intent/chức năng cũ | ID mới tương ứng | Quan hệ | Khác biệt quan trọng |
|---|---|---|---|---|---|
| `ADM-12` | Chủ quán/admin | Hỏi menu/doanh thu/đơn; thử đổi giá; Mock trả kết quả và write dừng ở confirmation | `OWN-AI-01..29`, `OWN-AI-34..42`, `OWN-AI-50` | Phân rã | Bộ mới tách từng intent, thiếu kỳ, empty/error và write unsupported; đổi giá không còn kỳ vọng chat tạo confirmation |
| `MGR-10` | Manager | Dữ liệu Branch A/B, doanh thu/đơn/nhân sự, thử tool admin | `OWN-AI-01..29`, `OWN-AI-45`, `AI-02` | Một phần | Bộ mới tập trung chủ quán; manager branch runtime vẫn nằm ở `AI-02`, không được coi là covered bởi owner UI case |
| `EMP-09` | Employee | Bàn, menu, ca; thử sửa giá/xem báo cáo | `OWN-AI-26..29`, `OWN-AI-36..40`, `AI-01` | Một phần | Không có bộ role employee trong 100 case; `get_my_shift` positive và employee permission cần bộ cũ/runtime riêng |
| `CSH-11` | Cashier | Menu, bàn, đơn; thử đổi giá/xem tài chính | `OWN-AI-26..29`, `OWN-AI-36..39`, `AI-01` | Một phần | Không có cashier role case trong 100; cần giữ `CSH-11` cho allowlist/financial denial |
| `KIT-12` | Kitchen | Xem bàn/đơn/món; từ chối doanh thu | `OWN-AI-49`, `AI-01`, `AI-02` | Cập nhật chính sách | UI kitchen đã bỏ ChatBot và backend deny kitchen; kỳ vọng cũ “Chatbot hiển thị cho kitchen” không còn phù hợp |
| `CUS-10` | Customer đăng nhập | Menu, đơn của mình, đặt bàn; từ chối customer khác/đổi giá | `CUS-AI-01..21`, `CUS-AI-22..35` | Phân rã | Bộ mới tách menu/order/ownership/session/error; booking/price/payment được đánh dấu write chưa hỗ trợ, không phải confirmation chat |
| `GST-09` | Khách QR/guest | Menu/giá/địa điểm; hỏi đơn cá nhân hoặc dữ liệu cá nhân | `QR-AI-01..05`, `QR-AI-07..13` | Phân rã | Bộ mới tách public menu, branch/session, login required và từng loại dữ liệu cá nhân/write |
| `GST-10` | Khách QR/guest | Guest Mock chỉ bật ở Development, không fallback provider | Chưa có tương ứng trực tiếp; tham chiếu `AI-01`, `AI-04`, `QR-AI-14` | Bị bỏ sót một phần | Bộ mới có error/loading nhưng chưa có case riêng kiểm tra environment gate và không fallback provider |
| `GST-15` | Khách QR/guest | Guest không được gọi tool cá nhân/staff/write | `QR-AI-07..13`, `CUS-AI-12..21`, `AI-03` | Phân rã | Bộ mới cụ thể hóa order, loyalty, profile, admin/staff data và mutation |
| `FLOW-09` | Admin + confirmation | Đổi giá, dừng ở confirmation, xác nhận server | `OWN-AI-36`, `OWN-AI-50`, `AI-07..AI-09` | Một phần/cập nhật kỳ vọng | Direct confirmation vẫn là API capability; Mock chat hiện không reach write intent, nên bộ mới không khẳng định confirmation đã dùng được từ chat |
| `FLOW-14` | Admin A/B + confirmation | Xác nhận bằng user khác, item khác hoặc sau expiry | `OWN-AI-50`, `AI-08` | Một phần | `AI-08` giữ đầy đủ wrong-user/item/expiry/replay; `OWN-AI-50` chỉ là direct invalid-token smoke case |
| `REF-07` | UI + nhiều context | Invalidation sau success/failure/rollback khi thao tác UI và Mock AI | `OWN-AI-41..48`, `CUS-AI-28..35`, `QR-AI-06`, `QR-AI-14..15` | Phân rã/mở rộng | Bộ mới thêm duplicate send, bounded history, stale response và bảo toàn cart/QR token |
| `AI-01` | Tất cả role | Tool allowlist, customer/guest deny, không provider thật | `OWN-AI-49`, `CUS-AI-12`, `CUS-AI-15..17`, `QR-AI-11..13`, `AI-01` | Phân rã | Bộ mới có denial theo vai trò; `AI-01` vẫn là kiểm chứng API tổng hợp, không thể thay bằng chat UI |
| `AI-02` | Manager/employee/admin | Branch scope với order/revenue/table/status | `OWN-AI-45`, `CUS-AI-06`, `CUS-AI-11`, `QR-AI-05`, `AI-02` | Phân rã | Bộ mới kiểm tra branch/session riêng; direct claim/resource substitution vẫn cần API case |
| `AI-03` | Customer/guest/admin | Own order, guest personal deny, admin price confirmation | `CUS-AI-08..21`, `QR-AI-07..13`, `OWN-AI-50`, `AI-03` | Phân rã | Bộ mới phân biệt customer đã đăng nhập với guest và không coi write tool đăng ký là chat reachable |
| `AI-04` | API/runtime | Tool không tồn tại, argument lỗi, denied path | `OWN-AI-41`, `CUS-AI-34`, `QR-AI-14`, `AI-04` | Phân rã | Bộ mới thêm UI biểu diễn lỗi; `AI-04` vẫn cần gọi API và ghi response contract |
| `AI-05` | API/runtime | Service failure, không fake success, không lộ exception | `OWN-AI-42`, `CUS-AI-34`, `QR-AI-14`, `AI-05` | Phân rã | Bộ mới thêm ngữ cảnh role; failure injection/log vẫn không thể xác minh chỉ bằng chat |
| `AI-06` | Mock recognizer | Supported, unsupported, thiếu period, multi-intent, phủ định | `OWN-AI-05..10`, `OWN-AI-13`, `OWN-AI-15`, `OWN-AI-19`, `OWN-AI-31..35`, `CUS-AI-22..27`, `QR-AI-10..13`, `AI-06` | Phân rã/mở rộng | Bộ mới có câu cụ thể theo role; vẫn cần technical case để kiểm tra tool code/arguments thực tế |
| `AI-07` | API/policy | Quyết định write policy cho đổi giá/status/booking | `OWN-AI-36..40`, `CUS-AI-18..21`, `QR-AI-10`, `QR-AI-13`, `AI-07` | Phân rã | Kết quả mới đúng source hiện tại: từ chối/không mutate; policy read-only/write vẫn mở |
| `AI-08` | API confirmation | Binding user/item/price, expiry, one-use/replay | `OWN-AI-50`, `AI-08` | Một phần | Bộ mới không tạo case mới thay thế mọi nhánh confirmation; giữ technical case cũ làm nguồn chính |
| `AI-09` | API confirmation | Không mutation trước confirmation, provider không tự xác nhận | `OWN-AI-36`, `OWN-AI-50`, `AI-09` | Một phần | Bộ mới kiểm tra refusal qua chat; `AI-09` vẫn là read-back/API mutation guard |
| `AI-10` | Browser/Network | Double-send, request chậm, đổi user/branch/refresh | `OWN-AI-43..48`, `CUS-AI-28..32`, `QR-AI-06`, `QR-AI-14..15`, `AI-10` | Phân rã | Bộ mới đưa phần UI/session vào đúng role; technical multi-session vẫn cần giữ |
| `AI-11` | API/runtime | Validation/not-found/conflict/infrastructure redaction | `OWN-AI-41..42`, `CUS-AI-33..35`, `QR-AI-14`, `AI-11` | Phân rã | Bộ mới thêm safe rendering; log redaction và phân biệt lỗi vẫn cần API/log evidence |

## 5. Boundary/tool examples đối chiếu với bộ mới

| Boundary/example cũ | ID mới tương ứng | Đánh giá |
|---|---|---|
| `customer_get_menu`: menu, giá, topping | `OWN-AI-27..29`, `CUS-AI-01..07`, `CUS-AI-22`, `QR-AI-01..04` | Được bao phủ tốt; bộ mới thêm empty/category/branch availability |
| `get_best_sellers`: món phổ biến/bán chạy theo kỳ | `OWN-AI-16..20` | Được bao phủ và thêm thiếu kỳ/empty |
| `get_revenue`: tổng bán hàng theo kỳ | `OWN-AI-01..05` | Được bao phủ; bộ mới kiểm tra thiếu kỳ và năm/tháng |
| `get_revenue_comparison`: hai kỳ | `OWN-AI-06..10` | Được bao phủ; bộ mới kiểm tra cặp kỳ không hỗ trợ |
| `get_business_summary`: KPI snapshot | `OWN-AI-11..15` | Được bao phủ; không mở rộng thành financial-analysis giả |
| `get_financial_analysis`: deep dive/anomaly | `OWN-AI-35` | Cố ý chỉ kiểm tra refusal; archive là thiết kế, không phải capability Mock hiện tại |
| `get_my_orders` so với `get_order_list` | `CUS-AI-08..11`, `OWN-AI-21..25`, `QR-AI-07` | Boundary own order/branch order được tách rõ |
| `employee_get_table_summary` so với order list | `OWN-AI-21..26`, `CUS-AI-16`, `QR-AI-12` | Owner positive; customer/guest denial được bổ sung |
| `get_my_shift` so với `get_active_staff` | `OWN-AI-30`, `AI-01` | `get_active_staff` được kiểm tra là registered nhưng không có recognizer; positive `get_my_shift` chưa có trong bộ 100 |

## 6. Chỉ có ở bộ cũ, chỉ có ở bộ mới và chưa bao phủ

### 6.1. Bộ cũ còn giá trị nhưng bộ mới chưa thay thế đầy đủ

- `MGR-10`, `EMP-09`, `CSH-11`: positive/negative role matrix cho manager, employee và cashier.
- `GST-10`: guest endpoint chỉ chạy Development + Mock và không fallback provider.
- `FLOW-14`/`AI-08`: đầy đủ wrong-user, wrong-item, expiry và replay của confirmation; bộ mới chỉ trỏ lại technical case, không nhân bản.
- `AI-01..AI-05`, `AI-11`: gọi API, kiểm tra logger, service failure, response contract và permission; đây không phải việc nhập một câu vào ChatBot.
- `TOOL_BOUNDARIES.md` với positive `get_my_shift` và `get_active_staff`; bộ mới chưa có role employee để kiểm tra ca cá nhân.
- `REF-07`: đối chiếu invalidation business data sau mutation/rollback; bộ mới tập trung chat state, không thay thế toàn bộ invalidation của module.

### 6.2. Bộ mới bổ sung so với bộ cũ

- Phân rã theo intent thực tế của Mock: revenue, comparison, summary, order count/status, best sellers, menu, table status.
- Thiếu period, cặp period không hỗ trợ, category rỗng/không tồn tại, order/menu empty.
- Multi-intent, phủ định, khoảng trắng và unsupported inventory/financial analysis.
- Write intent đổi giá/status/xóa/thanh toán/đặt bàn được kiểm tra như refusal, không coi registered write tool là chat feature.
- Tool failure, service failure, safe error/UI state, duplicate send, history bound, đổi branch/user, refresh và stale response.
- Customer/guest ownership, profile/loyalty/admin/staff denial, cart/QR token preservation.
- Kitchen direct endpoint denial sau khi UI kitchen đã bỏ ChatBot.

### 6.3. Cả hai bộ chưa bao phủ hoặc cần policy/runtime

- Positive `get_my_shift` cho employee và positive `get_active_staff` cho manager/admin qua Mock; source có tool nhưng recognizer hiện không có intent `get_active_staff`.
- Environment gate guest-chat ở ngoài Development, không fallback provider (`GST-10` chỉ có ở bộ cũ).
- Confirmation expiry/replay/concurrency bằng API thật và dữ liệu thay đổi sau khi confirmation tạo.
- Cancellation/timeout, rate limit và `RequestAborted` trong orchestrator.
- Tool registry đầy đủ 14 tool so với intent thực sự gọi được, đặc biệt các registered tools không có recognizer.
- Render XSS/Markdown ở admin ChatBot và kiểm tra nhiều tab/response ordering thực tế; bộ mới có case kỳ vọng nhưng vẫn cần runtime.
- Chính sách chính thức cho write intent, guest booking và customer reservation qua chat.
- Ownership/branch checks của từng service bằng resource ID thực tế; chat text không thể chứng minh thay cho API test.

## 7. Khác biệt kết quả mong đợi và trường hợp không còn phù hợp

### 7.1. Write/confirmation

Các case cũ `ADM-12`, `CUS-10`, `EMP-09`, `CSH-11`, `FLOW-09` và `FLOW-14` có cách diễn đạt cho thấy người dùng có thể yêu cầu write qua chat và dừng ở confirmation. Source Mock hiện tại xử lý `IsWriteMutation` trước các read intent và trả `WRITE_INTENT_UNSUPPORTED`; vì vậy kỳ vọng “chat tạo confirmation” không còn là hành vi đã chứng minh. Tool `UpdateProductPriceTool`, `UpdateOrderStatusTool` và `CreateBookingTool` vẫn tồn tại/đăng ký, còn policy có cho phép expose qua chat hay không vẫn mở.

Bộ mới là bản ghi đúng hơn cho source hiện tại: không mutate, không confirmation giả, không báo thành công. `FLOW-09`, `FLOW-14`, `AI-08`, `AI-09` phải được giữ để kiểm tra direct API/confirmation sau khi policy và môi trường test được chốt.

### 7.2. Provider và financial intelligence

Archive mô tả provider thật, sequential reasoning, profit/anomaly/recommendation và inventory tools. Current source dùng `MockAiProvider` deterministic; Mock recognizer không phải LLM và không có intent cho toàn bộ capability thiết kế. Do đó các demo như “phân tích tình hình kinh doanh và cho 3 lời khuyên” hoặc “tại sao doanh thu giảm” chỉ là design/demo history. `OWN-AI-35` kiểm tra refusal, không chuyển thiết kế archive thành chức năng hiện tại.

### 7.3. Kitchen

`KIT-12` từng kỳ vọng ChatBot hiển thị cho kitchen. Sau thay đổi source, kỳ vọng đúng là không có entry point và endpoint AI từ kitchen bị từ chối. Đây là case cũ cần đánh dấu obsolete theo current source, nhưng vẫn giữ ID/minh chứng lịch sử.

## 8. Đề xuất nguồn chính và cách hợp nhất

1. Dùng `ba-test-frontend/ai-chat/` làm nguồn chính cho trải nghiệm ChatBot của chủ quán, customer đăng nhập và khách QR. Giữ nguyên `OWN-AI-*`, `CUS-AI-*`, `QR-AI-*` và trạng thái `Chưa test` cho đến khi có evidence.
2. Giữ nguyên các ID cũ trong file cũ; không đổi ID và không copy nguyên văn thành case mới. Khi cần thực thi, cập nhật kết quả/minh chứng tại case cũ hoặc ghi liên kết hai chiều.
3. Dùng `ba-test-frontend/ky-thuat-runtime/README.md` làm nguồn chính cho API/log/nhiều phiên/confirmation. Bộ 100 chỉ tham chiếu các case này, không thay thế chúng.
4. Ghi `ADM-12`, `CUS-10`, `GST-09` là case legacy đã được phân rã; không xóa vì chúng giữ baseline lịch sử.
5. Ghi `KIT-12` là legacy không còn phù hợp với current source và trỏ sang `OWN-AI-49`/AI role-denial evidence; không đổi nội dung file cũ trong lượt này.
6. Nếu policy cho phép write intent trong tương lai, bổ sung case mới với ID mới và expected result mới; không sửa ngược bộ hiện tại thành “write đã hoạt động”.

### Số case cần sau hợp nhất

Không nên cộng `23 + 100` thành coverage mới. Với quy ước giữ mọi nguồn có giá trị:

- 100 case trải nghiệm hiện tại giữ nguyên.
- 11 case technical `AI-01..AI-11` giữ nguyên vì cần API/log/nhiều phiên/confirmation.
- `FLOW-09`, `FLOW-14`, `REF-07` giữ nguyên vì kiểm tra confirmation/invalidation liên module.
- Các role case cũ `MGR-10`, `EMP-09`, `CSH-11` chỉ cần giữ như baseline hoặc bổ sung vào một bộ role mở rộng sau khi quyết định phạm vi; chưa gộp vào 100.
- `GST-10` cần được giữ/bổ sung riêng cho environment gate.
- `KIT-12` giữ lịch sử nhưng không đưa vào expected current behavior như một positive ChatBot case.

Theo cách này, số lượng tối thiểu để không mất coverage là **100 + 11 + 3 + 4 role/environment baseline = 118 case record**, trong đó 100 là trải nghiệm role hiện tại, 14 là technical/cross-flow bắt buộc giữ, và 4 là các khoảng role/environment cũ chưa được bộ mới thay thế (`MGR-10`, `EMP-09`, `CSH-11`, `GST-10`). Đây là số record sau hợp nhất có loại trùng theo hành vi, không phải một bộ 118 câu hỏi phải chạy đồng thời. `ADM-12`, `CUS-10`, `GST-09` được giữ làm legacy mapping nhưng không cộng thêm vì đã được phân rã vào 100.

## 9. Giới hạn khảo sát

- Chỉ đọc source/tài liệu/Git history; không chạy API, browser, database, build hoặc câu hỏi.
- Không sửa code và không sửa/xóa bộ câu hỏi cũ hoặc `ba-test-frontend/ai-chat/`.
- Các kết luận về runtime, confirmation expiry/replay, response stale thực tế, log và service ownership vẫn là kiểm chứng chưa chạy.
- Vì một số tài liệu repository bị mojibake encoding, mapping dựa trên ID, cấu trúc bảng, đường dẫn và source intent/tool; không dùng lỗi hiển thị encoding để kết luận khác nội dung.

## 10. Cập nhật sau hợp nhất

Bộ chính đã được hợp nhất theo báo cáo này tại `ba-test-frontend/ai-chat/` và hiện có **104 ID giao diện**: `OWN-AI-01..54`, `CUS-AI-01..35`, `QR-AI-01..15`. Bốn case mới `OWN-AI-51..54` chuyển phần coverage còn thiếu của manager, employee và cashier vào đúng bộ chính; `get_my_shift` được kiểm tra theo source hiện tại.

`GST-10` được chuyển thành `AI-12` trong `ky-thuat-runtime/README.md` vì environment/provider gate cần API/config evidence. `AI-01..AI-11`, `FLOW-09`, `FLOW-14` và `REF-07` vẫn là kiểm tra kỹ thuật/liên vai trò, không sao chép vào UI chat. `KIT-12` chỉ còn là lịch sử; hành vi hiện tại là kitchen bị từ chối endpoint và không có ChatBot.

Khuyến nghị “118 record” ở phần trước là phân tích baseline trước khi hợp nhất, không phải mục tiêu số lượng. Sau hợp nhất không ép 118 hoặc 150: dùng 104 case UI cộng với các case kỹ thuật/cross-flow khi cần, loại trùng theo hành vi và giữ nguyên minh chứng/finding lịch sử.
