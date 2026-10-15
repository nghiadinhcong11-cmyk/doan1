# Ma trận bao phủ vai trò → route/trang → chức năng → case

Route được đối chiếu từ các route component hiện có, gồm cả URL truy cập trực tiếp. Case được đếm theo ID duy nhất; case liên vai trò không nhân bản vào từng role.

| Vai trò | Route/trang | Chức năng | Case ID | File checklist | Khoảng trống |
|---|---|---|---|---|---|
| Chủ quán | /dashboard, /business-insights | KPI, insight, branch context, refresh | ADM-02, ADM-03, ADM-16 | chu-quan/00-CASES-BY-PAGE.md, chung/refresh-session.md | Chưa chạy runtime/cache |
| Chủ quán | /employees | Hồ sơ nhân viên, EmployeeType, BasicSalary | ADM-08, EMP-PROFILE-01..06 | chu-quan/employees.md | Chưa test lưu/reload/API |
| Chủ quán | /products, /toppings, /promotions | Catalog/topping/khuyến mại | ADM-04 | chu-quan/00-CASES-BY-PAGE.md | Chưa test |
| Chủ quán | /tables, /reservations | Bàn, QR, đặt bàn | ADM-05 | chu-quan/00-CASES-BY-PAGE.md | Chưa test |
| Chủ quán | /pos | POS, giá, phí, VAT, payment | ADM-06, ADM-14, ADM-15, ADM-20 | chu-quan/00-CASES-BY-PAGE.md, lien-vai-tro/order-to-payment.md | Chưa test transaction/concurrency |
| Chủ quán | /invoices, /expenses | Hóa đơn, chi phí, báo cáo | ADM-07 | chu-quan/00-CASES-BY-PAGE.md | Chưa test |
| Chủ quán | /inventory/* | Kho và phiếu | ADM-09 | chu-quan/00-CASES-BY-PAGE.md | Chưa test |
| Chủ quán | /settings, /settings/receipt, /print-templates, /branches | Cấu hình branch/receipt/in | ADM-10, ADM-18, ADM-19 | chu-quan/00-CASES-BY-PAGE.md | Chưa test multi-branch |
| Chủ quán | /profile, /notifications, URL trực tiếp | session, quyền, notification | ADM-01, ADM-11, ADM-13, ADM-17, ADM-21 | chu-quan/00-CASES-BY-PAGE.md, chung/refresh-session.md | Chưa test hết hạn phiên |
| Chủ quán | ChatBot | Mock AI, permission, confirmation | ADM-12, FLOW-09, FLOW-14 | chu-quan/00-CASES-BY-PAGE.md, lien-vai-tro/order-to-payment.md | Chưa test runtime |
| Quản lý | dashboard, /employees, /attendance, /schedule | Branch scope và hồ sơ nhân viên | MGR-01, MGR-06, EMP-PROFILE-01..06 | quan-ly/00-CASES-BY-PAGE.md, quan-ly/employees.md | Chưa test branch/API |
| Quản lý | catalog, customers, tables, reservations | Quản lý nghiệp vụ trong branch | MGR-03..05 | quan-ly/00-CASES-BY-PAGE.md | Chưa test |
| Quản lý | /pos, /invoices, /kitchen/history | order, invoice, kitchen read/update | MGR-07, MGR-08 | quan-ly/00-CASES-BY-PAGE.md | Chưa test |
| Quản lý | /inventory, /expenses | kho, chi phí | MGR-09, MGR-10 | quan-ly/00-CASES-BY-PAGE.md | Chưa test |
| Quản lý | /profile, URL trực tiếp, ChatBot | quyền, session, Mock AI | MGR-11..16 | quan-ly/00-CASES-BY-PAGE.md | Chưa test |
| Bếp | /kitchen, /kitchen/history | tiếp nhận, trạng thái, realtime | KIT-01..04, KIT-08, KIT-16..18 | bep/00-CASES-BY-PAGE.md, lien-vai-tro/order-to-payment.md | Chưa test SignalR |
| Bếp | /notifications, /attendance, /schedule, /reservations | notification và trang hỗ trợ | KIT-05, KIT-09..10 | bep/00-CASES-BY-PAGE.md | Chưa test |
| Nhân viên | /staff, /staff/sales, /staff/tables | phục vụ và trạng thái order | EMP-01..05, EMP-09 | nhan-vien/00-CASES-BY-PAGE.md | Chưa test |
| Nhân viên | /attendance, /schedule, /inventory, /notifications | ca, lịch, tồn kho, thông báo | EMP-06..08 | nhan-vien/00-CASES-BY-PAGE.md | Chưa test |
| Thu ngân | /pos, /pos/invoices, /pos/shifts | POS, payment, invoice, ca | CSH-01..07 | thu-ngan/00-CASES-BY-PAGE.md, lien-vai-tro/order-to-payment.md | Chưa test payment hai máy |
| Thu ngân | /reservations, /notifications, /profile | đặt bàn, thông báo, session | CSH-08..16 | thu-ngan/00-CASES-BY-PAGE.md | Chưa test |
| Khách đăng nhập | /, /profile, /reservation, /scan | menu, order, hồ sơ, đặt bàn | CUS-01..17 | khach-dang-nhap/00-CASES-BY-PAGE.md | Chưa test loyalty/scope |
| Khách QR | /, /scan | QR session, menu, order, chatbot public | GST-01..16 | khach-qr/00-CASES-BY-PAGE.md | Chưa test camera/expiry |
| Liên vai trò | QR → staff → kitchen → cashier → dashboard | order/payment/realtime/branch/concurrency | FLOW-01..17 | lien-vai-tro/order-to-payment.md | Chưa chạy toàn chuỗi |
| Dùng chung | refresh/polling/cache/session | reload, visibility, retry, invalidation | REF-01..07 | chung/refresh-session.md | Chưa test Network |
| Kỹ thuật/runtime | API/database/multi-session | invariant không đủ xác minh qua UI | — | ky-thuat-runtime/README.md | Chưa có kết quả runtime |

## Quy tắc đọc khoảng trống

“Chưa test” hoặc “chưa có kết quả runtime” không phải PASS. Các route phụ/modal/tab được gộp dưới trang chủ; nếu source phát hiện hành động mới thì bổ sung vào file trang tương ứng và cập nhật ma trận.

Chi tiết từng route/page thực tế, component, chức năng, case ID, file chứa bước test và khoảng trống được duy trì tại [00-ROUTE-FUNCTION-MAP.md](00-ROUTE-FUNCTION-MAP.md). Ma trận này là mục lục tổng hợp; không coi việc có ID legacy là đã bao phủ hoặc đã đạt.

## Đối soát ID

| Nhóm | ID duy nhất |
|---|---:|
| Case theo vai trò | 143 |
| Liên vai trò FLOW | 17 |
| Refresh/session REF | 7 |
| Network NET | 5 |
| Technical TECH | 5 |
| Hồ sơ EmployeeType/BasicSalary | 6 |
| Tổng toàn bộ | 183 |

Các case hồ sơ dùng chung ID giữa chủ quán và quản lý nên chỉ tính một lần. Không có ID legacy nào bị đổi; file legacy chỉ là tra cứu.
## Bổ sung checklist theo khoảng trống route

| Phạm vi | Case mới | File bước test | Ghi chú |
|---|---|---|---|
| Chủ quán /customers | ADM-CUS-01..09 | chu-quan/customers.md | Bao phủ tải, tìm kiếm debounce, lọc, modal, loyalty, quyền; nút export/delete cần xác minh source/runtime |
| Chủ quán /support | ADM-SUP-01..03 | chu-quan/support.md | Source hiện thiên về nội dung hỗ trợ; không giả định có ticket API |
| Bếp /kitchen/tables | KIT-TBL-01..04 | bep/kitchen-tables.md | Bao phủ loading, trạng thái bàn, reconnect, branch |
| Inventory subpages | INV-SUB-01..08 | chung/inventory-hr-settings.md | Dùng chung cho admin/manager, staff operational có case riêng |
| HR subpages | HR-SUB-01..08 | chung/inventory-hr-settings.md | Bao phủ employees, attendance, schedule, shifts và direct URL |
| Settings subpages | SET-SUB-01..08 | chung/inventory-hr-settings.md | Admin thực hiện; manager kiểm tra denied route |
| Guest direct routes | KQR-01..06 | khach-qr/guest-direct-routes.md | Chính sách guest /reservation và /profile mở; liên kết COV-001 |
