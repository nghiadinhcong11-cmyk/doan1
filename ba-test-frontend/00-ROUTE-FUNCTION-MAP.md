# Route/function map — đối soát source

Nguồn route: ManagementRoutes.tsx, CashierRoutes.tsx, StaffRoutes.tsx, KitchenRoutes.tsx và apps/customer-web/src/App.tsx. Đây là file mục lục/đối soát; không thay thế bước test.

- File mục lục: role README và file này.
- File chứa bước test: page checklist và 00-CASES-BY-PAGE.md.
- File dùng chung: chung/web-runtime.md và chung/refresh-session.md.
- File liên vai trò: lien-vai-tro/order-to-payment.md.
- File kỹ thuật/runtime: ky-thuat-runtime/README.md.
- Tất cả case hiện vẫn Chưa test nếu chưa có runtime/manual evidence.

| Vai trò | Route/page thực tế | Component/source | Chức năng | Case ID | File chứa bước test | Khoảng trống |
|---|---|---|---|---|---|---|
| Chủ quán | /dashboard | Dashboard.tsx | KPI, insight, branch, refresh/cache | ADM-02, ADM-03, ADM-16, REF-01, REF-04 | chu-quan/00-CASES-BY-PAGE.md, chung/refresh-session.md | Chưa runtime |
| Chủ quán | /business-insights | BusinessInsights.tsx | insight, chi tiết, resolve | ADM-02 | chu-quan/00-CASES-BY-PAGE.md | Chưa test từng action |
| Chủ quán | /notifications | NotificationHistoryPage.tsx | unread, mark-read, filter, pagination | ADM-21 | chu-quan/00-CASES-BY-PAGE.md | Chưa test |
| Chủ quán | /products, /toppings, /promotions | catalog pages | CRUD, validation, trạng thái, giá | ADM-04 | chu-quan/00-CASES-BY-PAGE.md | Chưa test từng trang |
| Chủ quán | /expenses | ExpenseManagement.tsx | danh sách, filter, ghi nhận, retry | ADM-07, FLOW-13, FLOW-15 | chu-quan/00-CASES-BY-PAGE.md, lien-vai-tro/order-to-payment.md | Chưa test |
| Chủ quán | /inventory/* | InventoryPage.tsx | overview, item, receipt, issue, history | ADM-09, FLOW-03 | chu-quan/00-CASES-BY-PAGE.md | Chưa có case riêng từng subpage/action |
| Chủ quán | /employees | EmployeeManagement.tsx | hồ sơ, EmployeeType, BasicSalary, quyền branch | ADM-08, EMP-PROFILE-01..06 | chu-quan/employees.md, chu-quan/00-CASES-BY-PAGE.md | Chưa test |
| Chủ quán | /attendance, /schedule, /shifts | HR pages | chấm công, lịch, ca | ADM-08 | chu-quan/00-CASES-BY-PAGE.md | Chưa tách theo trang |
| Chủ quán | /customers | CustomerManagement.tsx | danh sách, tìm/lọc, loyalty/scope | — | — | Chưa có case chuyên biệt |
| Chủ quán | /profile | ProfilePage.tsx | hồ sơ, session | ADM-01, ADM-10, ADM-13 | chu-quan/00-CASES-BY-PAGE.md | Chưa test |
| Chủ quán | /tables, /reservations | operations pages | bàn/QR, đặt bàn | ADM-05, FLOW-01 | chu-quan/00-CASES-BY-PAGE.md, lien-vai-tro/order-to-payment.md | Chưa test |
| Chủ quán | /pos | POSPage.tsx | order, giá, VAT/fee, payment, loyalty | ADM-06, ADM-14, ADM-15, ADM-20, FLOW-04, FLOW-06, FLOW-07, REF-05 | chu-quan/00-CASES-BY-PAGE.md, lien-vai-tro/order-to-payment.md | Chưa runtime/concurrency |
| Chủ quán | /invoices | InvoiceHistory.tsx | filter, pagination, detail, print | ADM-07, REF-01 | chu-quan/00-CASES-BY-PAGE.md, chung/refresh-session.md | Chưa test |
| Chủ quán | /kitchen, /kitchen/history | kitchen pages | KDS, history, realtime | ADM-17, FLOW-01, FLOW-11, FLOW-16 | chu-quan/00-CASES-BY-PAGE.md, lien-vai-tro/order-to-payment.md | Chưa có case admin-specific history |
| Chủ quán | /settings, /settings/receipt, /print-templates, /branches | settings/pos pages | system, receipt, print, branch settings | ADM-10, ADM-18, ADM-19 | chu-quan/00-CASES-BY-PAGE.md | Chưa test từng form |
| Chủ quán | /support | SupportPage.tsx | hỗ trợ | — | — | Chưa có case |
| Chủ quán | /forbidden, unknown, / redirect | route guards | direct URL, redirect, forbidden | ADM-11, ADM-13 | chu-quan/00-CASES-BY-PAGE.md, chung/web-runtime.md | Chưa test |
| Chủ quán | ChatBot | ChatBot.tsx | Mock AI, permission, confirmation | ADM-12, FLOW-09, FLOW-14, REF-07 | chu-quan/00-CASES-BY-PAGE.md, lien-vai-tro/order-to-payment.md | Chưa runtime |
| Quản lý | /dashboard, /business-insights | ManagementRoutes + analytics | KPI/insight branch | MGR-01, MGR-02, MGR-12 | quan-ly/00-CASES-BY-PAGE.md | Chưa test |
| Quản lý | /products, /toppings, /promotions, /customers | management pages | catalog/customer operations | MGR-03, MGR-04 | quan-ly/00-CASES-BY-PAGE.md | Customer actions chưa tách đầy đủ |
| Quản lý | /employees, /attendance, /schedule, /shifts | HR pages | hồ sơ, EmployeeType/BasicSalary, attendance, schedule, shift | MGR-06, EMP-PROFILE-01..06 | quan-ly/employees.md, quan-ly/00-CASES-BY-PAGE.md | Chưa test |
| Quản lý | /tables, /reservations | operations pages | bàn và đặt bàn | MGR-05 | quan-ly/00-CASES-BY-PAGE.md | Chưa test |
| Quản lý | /pos, /invoices | POS/analytics | order/payment/invoice branch | MGR-07, FLOW-04, FLOW-06, FLOW-07 | quan-ly/00-CASES-BY-PAGE.md, lien-vai-tro/order-to-payment.md | Chưa runtime |
| Quản lý | /kitchen, /kitchen/history | kitchen pages | kitchen/realtime | MGR-08, FLOW-01, FLOW-11 | quan-ly/00-CASES-BY-PAGE.md, lien-vai-tro/order-to-payment.md | Chưa test |
| Quản lý | /settings*, /branches, /print-templates | route guard | forbidden theo source | MGR-11 | quan-ly/00-CASES-BY-PAGE.md | Chưa test direct URL |
| Bếp | /kitchen, /kitchen/history | KitchenPage, KitchenHistoryPage | nhận/cập nhật, history, realtime | KIT-01..KIT-04, KIT-08, KIT-16..KIT-18, FLOW-01, FLOW-11 | bep/00-CASES-BY-PAGE.md, bep/kitchen.md, lien-vai-tro/order-to-payment.md | Chưa SignalR runtime |
| Bếp | /kitchen/notifications, /kitchen/tables | notification/TableStatusPage | notification, trạng thái bàn | KIT-05, — | bep/00-CASES-BY-PAGE.md | Table Status chưa có case riêng |
| Bếp | /kitchen/attendance, /kitchen/schedule, /kitchen/reservations | HR/reservation pages | attendance, schedule, read-only reservation | KIT-09, KIT-10 | bep/00-CASES-BY-PAGE.md | Chưa tách theo trang |
| Bếp | /kitchen/profile, /forbidden, unknown | profile/guards | profile, redirect, quyền | KIT-06, KIT-07, KIT-11 | bep/00-CASES-BY-PAGE.md | Chưa test |
| Nhân viên | /staff, /staff/sales, /staff/tables | StaffOverviewPage, POSPage | overview, tables redirect, order | EMP-01..EMP-05, EMP-09, FLOW-01, FLOW-02 | nhan-vien/00-CASES-BY-PAGE.md, nhan-vien/staff-operations.md | Chưa runtime |
| Nhân viên | /staff/inventory, /staff/notifications | InventoryPage, NotificationHistoryPage | inventory, notification | EMP-07, EMP-08 | nhan-vien/00-CASES-BY-PAGE.md | Chưa test |
| Nhân viên | /staff/attendance, /staff/schedule, /staff/profile | HR/profile pages | attendance, schedule, session | EMP-06 | nhan-vien/00-CASES-BY-PAGE.md | Chưa tách theo trang |
| Thu ngân | /pos, /pos/invoices | POSPage, InvoiceHistory | POS/payment/invoice | CSH-01..CSH-05, CSH-16, FLOW-04, FLOW-06, REF-05 | thu-ngan/00-CASES-BY-PAGE.md, thu-ngan/pos-payment.md, lien-vai-tro/order-to-payment.md | Chưa runtime |
| Thu ngân | /pos/notifications, /pos/attendance, /pos/schedule, /pos/shifts | notification/HR pages | notification, attendance, schedule, shift | CSH-06, CSH-07, CSH-09 | thu-ngan/00-CASES-BY-PAGE.md | Chưa tách theo trang |
| Thu ngân | /pos/reservations, /pos/profile | reservation/profile pages | reservation, session | CSH-06, CSH-08 | thu-ngan/00-CASES-BY-PAGE.md | Chưa test |
| Thu ngân | /pos/settings/receipt, unknown, /forbidden | route guards | denied settings, redirect | CSH-10 | thu-ngan/00-CASES-BY-PAGE.md | Chưa test |
| Khách đăng nhập | / | DigitalMenu | menu, cart, order, options | CUS-01..CUS-04, CUS-16, FLOW-02, FLOW-06, FLOW-07 | khach-dang-nhap/00-CASES-BY-PAGE.md, khach-dang-nhap/menu-profile-reservation.md | Chưa runtime |
| Khách đăng nhập | /reservation, /profile, /scan | ReservationPage, CustomerProfile, QRScan | reservation, profile/loyalty, QR scan | CUS-05..CUS-07, CUS-15 | khach-dang-nhap/00-CASES-BY-PAGE.md | Chưa camera/runtime |
| Khách đăng nhập | login conditional shell, unknown | CustomerLogin/App | login, expiry, redirect | CUS-07, CUS-09, CUS-11 | khach-dang-nhap/00-CASES-BY-PAGE.md | Chưa test |
| Khách QR | / | DigitalMenu guest | QR context, menu/cart/order | GST-01..GST-04, GST-06, GST-16, FLOW-01, FLOW-02, FLOW-08 | khach-qr/00-CASES-BY-PAGE.md, khach-qr/qr-menu-order.md, lien-vai-tro/order-to-payment.md | Chưa runtime |
| Khách QR | /scan | QRScan | camera/QR bootstrap | GST-02..GST-04, NET-02, NET-03 | khach-qr/00-CASES-BY-PAGE.md, chung/web-runtime.md | Chưa camera/runtime |
| Khách QR | /reservation, /profile | App routes dưới shell | hành vi truy cập trực tiếp | — | — | Chưa xác định guest policy; cần test/decision |
| Khách QR | ChatBot guest | ChatBot.tsx | public read-only, cấm tool write | GST-09, GST-10, GST-15 | khach-qr/00-CASES-BY-PAGE.md, khach-qr/qr-menu-order.md | Chưa runtime |

## Kết luận coverage

Phân loại hết case cũ không chứng minh bao phủ hết route. Các dấu — là khoảng trống chưa có case riêng. TECH-01..TECH-05 và NET-01..NET-05 là kiểm chứng độc lập, không tính là case UI.

## Danh sách route khai báo nguyên dạng

### Chủ quán và quản lý — ManagementRoutes.tsx

/dashboard; /business-insights; /notifications; /products; /expenses; /inventory; /inventory/items; /inventory/receipts; /inventory/receipts/:id; /inventory/issues; /inventory/issues/:id; /inventory/history; /employees; /attendance; /schedule; /customers; /promotions; /profile; /toppings; /support; /forbidden; /; *.

Admin-only settings: /settings; /settings/receipt; /print-templates; /branches. Operations: /pos; /tables; /invoices; /shifts; /reservations; /kitchen; /kitchen/history. Manager vào các route settings/branches/print qua wrapper chuyển /forbidden theo source.

### Thu ngân — CashierRoutes.tsx

/pos; /pos/notifications; /pos/invoices; /pos/settings/receipt; /pos/attendance; /pos/schedule; /pos/shifts; /pos/reservations; /pos/profile; /forbidden; *.

### Nhân viên — StaffRoutes.tsx

/staff; /staff/notifications; /staff/sales; /staff/tables; /staff/profile; /staff/attendance; /staff/schedule; /staff/inventory; /forbidden; *.

### Bếp — KitchenRoutes.tsx

/kitchen; /kitchen/notifications; /kitchen/history; /kitchen/tables; /kitchen/attendance; /kitchen/schedule; /kitchen/reservations; /kitchen/profile; /forbidden; *.

### Customer Web — App.tsx

/; /reservation; /profile; /scan; *. CustomerLogin là conditional shell, không phải một route riêng. Guest dùng cùng shell nhưng quyền thực tế của /reservation và /profile chưa được runtime xác minh; đây là khoảng trống, không phải khẳng định guest được phép dùng.
## Checklist bổ sung sau đối soát

- /customers: ADM-CUS-01..09 in chu-quan/customers.md.
- /support: ADM-SUP-01..03 in chu-quan/support.md.
- /kitchen/tables: KIT-TBL-01..04 in bep/kitchen-tables.md.
- Inventory subpages: INV-SUB-01..08 in chung/inventory-hr-settings.md.
- HR subpages: HR-SUB-01..08 in chung/inventory-hr-settings.md.
- Settings subpages: SET-SUB-01..08 in chung/inventory-hr-settings.md.
- Guest direct /reservation and /profile: KQR-01..06 in khach-qr/guest-direct-routes.md; policy remains open under COV-001.

The route map remains a source-derived index. The linked files contain the actual preparation, steps, expected result, status, actual result and evidence fields.
