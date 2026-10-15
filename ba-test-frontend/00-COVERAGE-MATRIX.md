# Ma tráº­n bao phá»§ vai trĂ² â†’ route/trang â†’ chá»©c nÄƒng â†’ case

Route Ä‘Æ°á»£c Ä‘á»‘i chiáº¿u tá»« cĂ¡c route component hiá»‡n cĂ³, gá»“m cáº£ URL truy cáº­p trá»±c tiáº¿p. Case Ä‘Æ°á»£c Ä‘áº¿m theo ID duy nháº¥t; case liĂªn vai trĂ² khĂ´ng nhĂ¢n báº£n vĂ o tá»«ng role.

| Vai trĂ² | Route/trang | Chá»©c nÄƒng | Case ID | File checklist | Khoáº£ng trá»‘ng |
|---|---|---|---|---|---|
| Chá»§ quĂ¡n | /dashboard, /business-insights | KPI, insight, branch context, refresh | ADM-02, ADM-03, ADM-16 | chu-quan/00-CASES-BY-PAGE.md, chung/refresh-session.md | ChÆ°a cháº¡y runtime/cache |
| Chá»§ quĂ¡n | /employees | Há»“ sÆ¡ nhĂ¢n viĂªn, EmployeeType, BasicSalary | ADM-08, EMP-PROFILE-01..06 | chu-quan/employees.md | ChÆ°a test lÆ°u/reload/API |
| Chá»§ quĂ¡n | /products, /toppings, /promotions | Catalog/topping/khuyáº¿n máº¡i | ADM-04 | chu-quan/00-CASES-BY-PAGE.md | ChÆ°a test |
| Chá»§ quĂ¡n | /tables, /reservations | BĂ n, QR, Ä‘áº·t bĂ n | ADM-05 | chu-quan/00-CASES-BY-PAGE.md | ChÆ°a test |
| Chá»§ quĂ¡n | /pos | POS, giĂ¡, phĂ­, VAT, payment | ADM-06, ADM-14, ADM-15, ADM-20 | chu-quan/00-CASES-BY-PAGE.md, lien-vai-tro/order-to-payment.md | ChÆ°a test transaction/concurrency |
| Chá»§ quĂ¡n | /invoices, /expenses | HĂ³a Ä‘Æ¡n, chi phĂ­, bĂ¡o cĂ¡o | ADM-07 | chu-quan/00-CASES-BY-PAGE.md | ChÆ°a test |
| Chá»§ quĂ¡n | /inventory/* | Kho vĂ  phiáº¿u | ADM-09 | chu-quan/00-CASES-BY-PAGE.md | ChÆ°a test |
| Chá»§ quĂ¡n | /settings, /settings/receipt, /print-templates, /branches | Cáº¥u hĂ¬nh branch/receipt/in | ADM-10, ADM-18, ADM-19 | chu-quan/00-CASES-BY-PAGE.md | ChÆ°a test multi-branch |
| Chá»§ quĂ¡n | /profile, /notifications, URL trá»±c tiáº¿p | session, quyá»n, notification | ADM-01, ADM-11, ADM-13, ADM-17, ADM-21 | chu-quan/00-CASES-BY-PAGE.md, chung/refresh-session.md | ChÆ°a test háº¿t háº¡n phiĂªn |
| Chá»§ quĂ¡n | ChatBot | Mock AI, permission, confirmation | ADM-12, FLOW-09, FLOW-14 | chu-quan/00-CASES-BY-PAGE.md, lien-vai-tro/order-to-payment.md | ChÆ°a test runtime |
| Quáº£n lĂ½ | dashboard, /employees, /attendance, /schedule | Branch scope vĂ  há»“ sÆ¡ nhĂ¢n viĂªn | MGR-01, MGR-06, EMP-PROFILE-01..06 | quan-ly/00-CASES-BY-PAGE.md, quan-ly/employees.md | ChÆ°a test branch/API |
| Quáº£n lĂ½ | catalog, customers, tables, reservations | Quáº£n lĂ½ nghiá»‡p vá»¥ trong branch | MGR-03..05 | quan-ly/00-CASES-BY-PAGE.md | ChÆ°a test |
| Quáº£n lĂ½ | /pos, /invoices, /kitchen/history | order, invoice, kitchen read/update | MGR-07, MGR-08 | quan-ly/00-CASES-BY-PAGE.md | ChÆ°a test |
| Quáº£n lĂ½ | /inventory, /expenses | kho, chi phĂ­ | MGR-09, MGR-10 | quan-ly/00-CASES-BY-PAGE.md | ChÆ°a test |
| Quáº£n lĂ½ | /profile, URL trá»±c tiáº¿p, ChatBot | quyá»n, session, Mock AI | MGR-11..16 | quan-ly/00-CASES-BY-PAGE.md | ChÆ°a test |
| Báº¿p | /kitchen, /kitchen/history | tiáº¿p nháº­n, tráº¡ng thĂ¡i, realtime | KIT-01..04, KIT-08, KIT-16..18 | bep/00-CASES-BY-PAGE.md, lien-vai-tro/order-to-payment.md | ChÆ°a test SignalR |
| Báº¿p | /notifications, /attendance, /schedule, /reservations | notification vĂ  trang há»— trá»£ | KIT-05, KIT-09..10 | bep/00-CASES-BY-PAGE.md | ChÆ°a test |
| NhĂ¢n viĂªn | /staff, /staff/sales, /staff/tables | phá»¥c vá»¥ vĂ  tráº¡ng thĂ¡i order | EMP-01..05, EMP-09 | nhan-vien/00-CASES-BY-PAGE.md | ChÆ°a test |
| NhĂ¢n viĂªn | /attendance, /schedule, /inventory, /notifications | ca, lá»‹ch, tá»“n kho, thĂ´ng bĂ¡o | EMP-06..08 | nhan-vien/00-CASES-BY-PAGE.md | ChÆ°a test |
| Thu ngĂ¢n | /pos, /pos/invoices, /pos/shifts | POS, payment, invoice, ca | CSH-01..07 | thu-ngan/00-CASES-BY-PAGE.md, lien-vai-tro/order-to-payment.md | ChÆ°a test payment hai mĂ¡y |
| Thu ngĂ¢n | /reservations, /notifications, /profile | Ä‘áº·t bĂ n, thĂ´ng bĂ¡o, session | CSH-08..16 | thu-ngan/00-CASES-BY-PAGE.md | ChÆ°a test |
| KhĂ¡ch Ä‘Äƒng nháº­p | /, /profile, /reservation, /scan | menu, order, há»“ sÆ¡, Ä‘áº·t bĂ n | CUS-01..17 | khach-dang-nhap/00-CASES-BY-PAGE.md | ChÆ°a test loyalty/scope |
| KhĂ¡ch QR | /, /scan | QR session, menu, order, chatbot public | GST-01..16 | khach-qr/00-CASES-BY-PAGE.md | ChÆ°a test camera/expiry |
| LiĂªn vai trĂ² | QR â†’ staff â†’ kitchen â†’ cashier â†’ dashboard | order/payment/realtime/branch/concurrency | FLOW-01..17 | lien-vai-tro/order-to-payment.md | ChÆ°a cháº¡y toĂ n chuá»—i |
| DĂ¹ng chung | refresh/polling/cache/session | reload, visibility, retry, invalidation | REF-01..07 | chung/refresh-session.md | ChÆ°a test Network |
| Ká»¹ thuáº­t/runtime | API/database/multi-session | invariant khĂ´ng Ä‘á»§ xĂ¡c minh qua UI | â€” | ky-thuat-runtime/README.md | ChÆ°a cĂ³ káº¿t quáº£ runtime |

## Quy táº¯c Ä‘á»c khoáº£ng trá»‘ng

â€œChÆ°a testâ€ hoáº·c â€œchÆ°a cĂ³ káº¿t quáº£ runtimeâ€ khĂ´ng pháº£i PASS. CĂ¡c route phá»¥/modal/tab Ä‘Æ°á»£c gá»™p dÆ°á»›i trang chá»§; náº¿u source phĂ¡t hiá»‡n hĂ nh Ä‘á»™ng má»›i thĂ¬ bá»• sung vĂ o file trang tÆ°Æ¡ng á»©ng vĂ  cáº­p nháº­t ma tráº­n.

Chi tiáº¿t tá»«ng route/page thá»±c táº¿, component, chá»©c nÄƒng, case ID, file chá»©a bÆ°á»›c test vĂ  khoáº£ng trá»‘ng Ä‘Æ°á»£c duy trĂ¬ táº¡i [00-ROUTE-FUNCTION-MAP.md](00-ROUTE-FUNCTION-MAP.md). Ma tráº­n nĂ y lĂ  má»¥c lá»¥c tá»•ng há»£p; khĂ´ng coi viá»‡c cĂ³ ID legacy lĂ  Ä‘Ă£ bao phá»§ hoáº·c Ä‘Ă£ Ä‘áº¡t.

## Äá»‘i soĂ¡t ID

| NhĂ³m | ID duy nháº¥t |
|---|---:|
| Case theo vai trĂ² | 143 |
| LiĂªn vai trĂ² FLOW | 17 |
| Refresh/session REF | 7 |
| Network NET | 5 |
| Technical TECH | 5 |
| Há»“ sÆ¡ EmployeeType/BasicSalary | 6 |
| Tá»•ng toĂ n bá»™ | 188 |

CĂ¡c case há»“ sÆ¡ dĂ¹ng chung ID giá»¯a chá»§ quĂ¡n vĂ  quáº£n lĂ½ nĂªn chá»‰ tĂ­nh má»™t láº§n. KhĂ´ng cĂ³ ID legacy nĂ o bá»‹ Ä‘á»•i; file legacy chá»‰ lĂ  tra cá»©u.
## Bá»• sung checklist theo khoáº£ng trá»‘ng route

| Pháº¡m vi | Case má»›i | File bÆ°á»›c test | Ghi chĂº |
|---|---|---|---|
| Chá»§ quĂ¡n /customers | ADM-CUS-01..09 | chu-quan/customers.md | Bao phá»§ táº£i, tĂ¬m kiáº¿m debounce, lá»c, modal, loyalty, quyá»n; nĂºt export/delete cáº§n xĂ¡c minh source/runtime |
| Chá»§ quĂ¡n /support | ADM-SUP-01..03 | chu-quan/support.md | Source hiá»‡n thiĂªn vá» ná»™i dung há»— trá»£; khĂ´ng giáº£ Ä‘á»‹nh cĂ³ ticket API |
| Báº¿p /kitchen/tables | KIT-TBL-01..04 | bep/kitchen-tables.md | Bao phá»§ loading, tráº¡ng thĂ¡i bĂ n, reconnect, branch |
| Inventory subpages | INV-SUB-01..08 | chung/inventory-hr-settings.md | DĂ¹ng chung cho admin/manager, staff operational cĂ³ case riĂªng |
| HR subpages | HR-SUB-01..08 | chung/inventory-hr-settings.md | Bao phá»§ employees, attendance, schedule, shifts vĂ  direct URL |
| Settings subpages | SET-SUB-01..08 | chung/inventory-hr-settings.md | Admin thá»±c hiá»‡n; manager kiá»ƒm tra denied route |
| Guest direct routes | KQR-01..06 | khach-qr/guest-direct-routes.md | ChĂ­nh sĂ¡ch guest /reservation vĂ  /profile má»Ÿ; liĂªn káº¿t COV-001 |
# Bá»• sung security/runtime

| Pháº¡m vi | Case má»›i | File bÆ°á»›c test | Ghi chĂº |
|---|---|---|---|
| Authentication, authorization, branch isolation, SignalR and AI scope | AUTH-01..04, BRANCH-01..04, SIG-01..02, AI-01..03 | ky-thuat-runtime/README.md | ChÆ°a test; source review findings SEC-001..SEC-005; khĂ´ng thay tháº¿ runtime/API evidence |

# Bá»• sung security/runtime



## Cập nhật chính sách và kiểm chứng guest/profile — 2026-10-15

| Vai trò | Route/trang | Chức năng | Case ID | File checklist | Khoảng trống |
|---|---|---|---|---|---|
| Khách QR | /profile | Mở từ menu, direct URL, login invitation, không gọi profile API bằng guest, giữ session/giỏ hàng/QR context | KQR-04, KQR-07, KQR-08 | `khach-qr/guest-direct-routes.md` | Chưa test browser/Network; reservation guest policy còn mở |
| Kỹ thuật/runtime | customer profile/session | 401/403/API error/loading termination | AUTH-05 | `ky-thuat-runtime/README.md` | Chưa test API/browser |
| Kỹ thuật/runtime | product availability | Branch A/B mutation, migration precondition, POS/customer/realtime scope | BRANCH-04, SIG-01 | `ky-thuat-runtime/README.md` | Chưa apply migration hoặc test runtime |
| Liên vai trò | order/kitchen/payment | Missing/inactive branch is rejected and no invalid global notification | BRANCH-05, SIG-02 | `ky-thuat-runtime/README.md`, `lien-vai-tro/order-to-payment.md` | Chưa test malformed/orphan data |

| Kỹ thuật/runtime | Customer registration | Internal field mass-assignment boundary | AUTH-06 | `ky-thuat-runtime/README.md` | Chưa test; SEC-006 |
| Kỹ thuật/runtime | Order lifecycle after branch deactivation | Read/accept/status/payment/send/delete recovery policy | BRANCH-08 | `ky-thuat-runtime/README.md` | Chính sách chưa chốt; chưa test |
| Kỹ thuật/runtime | Product availability realtime | Event consumer, branch payload and cross-branch update | SIG-04 | `ky-thuat-runtime/README.md` | Chưa xác minh consumer; SEC-003 |

Case IDs are counted once globally. These additions are not a claim that all routes/functions are covered or passed.

## Owner “Nhân viên” navigation audit addendum

| Phạm vi | Case mới | File bước test | Liên kết |
|---|---|---|---|
| Chủ quán `/employees` | ADM-EMP-01..02, ADM-EMP-04..05 | `chu-quan/employees.md` | `OWNER-EMPLOYEE-NAV-REVIEW.md`, HR-001..HR-003 |
| Chủ quán `/attendance` | ADM-ATT-01..03 | `chu-quan/attendance.md` | `OWNER-EMPLOYEE-NAV-REVIEW.md`, HR-002 |
| Chủ quán `/schedule` | ADM-SCH-01..04 | `chu-quan/schedule.md` | `OWNER-EMPLOYEE-NAV-REVIEW.md`, HR-002, HR-006 |
| Chủ quán `/shifts` | ADM-SHF-01..03 | `chu-quan/shifts.md` | `OWNER-EMPLOYEE-NAV-REVIEW.md`, HR-002 |

Các case mới đều giữ trạng thái `Chưa test`. `ADM-11` và `ADM-17` tiếp tục được tái sử dụng cho direct URL và navigation chính; không tạo case trùng cho hai nội dung đó.

- HR fix coverage update: employee direct PUT and branch matrix are `ADM-EMP-06..08`; attendance, schedule and shifts are tracked in separate page files. All remain `Chưa test`.
