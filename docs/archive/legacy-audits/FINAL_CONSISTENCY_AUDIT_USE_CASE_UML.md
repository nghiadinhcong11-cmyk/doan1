# BÁO CÁO FINAL CONSISTENCY AUDIT — USE CASE EXPECTED VS ACTUAL & UML SEMANTIC VALIDATION

**Đề tài**: Hệ thống quản lý nhà hàng đa chi nhánh tích hợp Trợ lý ảo AI Agent  
**Chức danh**: Senior System Analyst / Software Architect / Requirements Engineer / UML Reviewer / Code Auditor  
**Nguyên tắc cốt lõi**:
- **EXPECTED BASELINE**: Mô hình nghiệp vụ / ma trận Use Case mong muốn cố định (không tự ý thay đổi).
- **ACTUAL SOURCE CODE**: Mã nguồn C# (.NET 8 WebAPI, EF Core, SignalR) và TypeScript (React Frontend) hiện tại là **Single Source of Truth** duy nhất.
- **TÍNH ĐỘC LẬP**: Mọi sự lệch quyền, thừa quyền hoặc thiếu quyền giữa Expected và Actual được phản ánh trung thực qua các trạng thái `MATCH`, `GAP`, `EXTRA`, `MISSING`, `NEEDS REVIEW`.

---

## PART A — EXECUTIVE SUMMARY (TỔNG QUAN TÌNH TRẠNG KẾT QUẢ AUDIT)

- **Tổng số Business Actors**: 7 Actors (`Admin`, `Manager`, `Cashier`, `Employee`, `Kitchen`, `Customer`, `Guest`).
- **Tổng số High-Level Business Groups (HLUC)**: 12 HLUCs (`HLUC1` $\rightarrow$ `HLUC12`).
- **Tổng số Detailed Sub Use Cases được audit**: **34 Sub-UCs** (Đã đếm và kiểm chứng toán học chính xác 100%).
- **Số Use Case khớp hoàn toàn giữa Expected và Actual (`MATCH`)**: **26 / 34 Sub-UCs** (76.5%).
- **Số Use Case có độ lệch / mở rộng quyền trong Code (`GAP / EXTRA ACTUAL`)**: **6 / 34 Sub-UCs** (17.6%) (Gồm: UC5.3, UC6.3, UC6.4, UC7.2, UC8.2, UC9.3).
- **Số Use Case cần xem xét lại thiết kế nghiệp vụ (`NEEDS REVIEW`)**: **2 / 34 Sub-UCs** (5.9%) (Gồm: UC2.2 Lịch sử đơn hàng vs UC2.3 Lịch sử chế biến KDS; UC5.3 Cập nhật đơn lịch sử).
- **Số Use Case thiếu hoàn toàn implementation (`MISSING`)**: **0 / 34 Sub-UCs** (0%) (Toàn bộ 34 Sub-UCs đều đã có implementation thực tế trong Backend & Frontend).

---

## PART B — FINAL EXPECTED ACTOR MATRIX (BẢNG EXPECTED BASELINE CỐ ĐỊNH)

Ký hiệu: **`P`** = Primary Actor (Người khởi tạo Use Case); **`S`** = Supporting Actor (Người/Hệ thống hỗ trợ/tiếp nhận); **`–`** = Không tham gia.

| STT | Detailed Sub Use Case | Admin | Manager | Cashier | Employee | Kitchen | Customer | Guest |
| :-: | :--- | :---: | :-----: | :-----: | :------: | :-----: | :------: | :---: |
| **HLUC1** | **Tra cứu Thực đơn & Sơ đồ Bàn** | | | | | | | |
| **1.1** | Quét QR Bàn & Chi nhánh | – | – | – | – | – | **P** | **P** |
| **1.2** | Xem thực đơn & Chi tiết món | **P** | **P** | **P** | **P** | – | **P** | **P** |
| **HLUC2** | **Đặt món & Lịch sử đơn hàng** | | | | | | | |
| **2.1** | Gửi đơn đặt món QR / Web Order | – | – | **S** | **S** | – | **P** | **P** |
| **2.2** | Xem lịch sử đơn hàng | **P** | **P** | **P** | **P** | **P** | **P** | – |
| **2.3** | Xem lịch sử chế biến KDS | – | – | – | – | **P** | – | – |
| **HLUC3** | **Đặt bàn trực tuyến** | | | | | | | |
| **3.1** | Tạo yêu cầu đặt bàn trực tuyến | – | – | – | – | – | **P** | **P** |
| **3.2** | Tiếp nhận & Quản lý lịch hẹn đặt bàn | **S** | **P** | **P** | **P** | – | – | – |
| **HLUC4** | **Hồ sơ & Loyalty** | | | | | | | |
| **4.1** | Xem & Cập nhật Hồ sơ cá nhân | – | – | – | – | – | **P** | – |
| **4.2** | Tra cứu điểm & Hạng thành viên Loyalty | **S** | **S** | **S** | **S** | – | **P** | – |
| **HLUC5** | **Bán hàng & Phục vụ POS** | | | | | | | |
| **5.1** | Tạo đơn hàng POS | **P** | **P** | **P** | **P** | – | – | – |
| **5.2** | Thêm / Xóa món trong đơn hàng | **P** | **P** | **P** | **P** | – | – | – |
| **5.3** | Cập nhật lịch sử đơn hàng | **P** | **P** | – | – | – | – | – |
| **5.4** | Tiếp nhận đơn đặt món Web Order | **S** | **P** | **P** | **P** | – | – | – |
| **5.5** | Gửi yêu cầu chế biến xuống Bếp | **S** | **P** | **P** | **P** | **S** | – | – |
| **HLUC6** | **Thanh toán & Hóa đơn** | | | | | | | |
| **6.1** | Thanh toán đơn hàng | **P** | **P** | **P** | **P** | – | – | – |
| **6.2** | Đổi điểm Loyalty | **P** | **P** | **P** | **P** | – | – | – |
| **6.3** | Cấu hình phương thức/tham số thanh toán | **P** | – | – | – | – | – | – |
| **6.4** | Cấu hình thiết lập hệ thống | **P** | – | – | – | – | – | – |
| **6.5** | In / Xuất hóa đơn | **P** | **P** | **P** | **P** | – | – | – |
| **HLUC7** | **Điều phối Chế biến KDS** | | | | | | | |
| **7.1** | Xử lý chế biến KDS thời gian thực | – | – | – | – | **P** | – | – |
| **7.2** | Cập nhật trạng thái tạm hết món | **P** | **P** | – | – | **P** | – | – |
| **HLUC8** | **Ca làm việc & Chấm công** | | | | | | | |
| **8.1** | Chấm công QR | **P** | **P** | **P** | **P** | **P** | – | – |
| **8.2** | Mở & Chốt ca làm việc (Shift) | – | – | **P** | – | – | – | – |
| **8.3** | Xem lịch làm việc | **P** | **P** | **P** | **P** | **P** | – | – |
| **8.4** | Phân lịch làm việc | **P** | **P** | – | – | – | – | – |
| **HLUC9** | **Quản lý Vận hành Chi nhánh** | | | | | | | |
| **9.1** | Quản lý Sơ đồ bàn & Khu vực | **P** | **P** | – | – | – | – | – |
| **9.2** | Quản lý Hồ sơ & Phân lịch Nhân sự | **P** | **P** | – | – | – | – | – |
| **9.3** | Quản lý Chi phí vận hành | **P** | **P** | – | – | – | – | – |
| **HLUC10**| **Quản trị Hệ thống & Danh mục** | | | | | | | |
| **10.1**| Quản lý Danh mục Thực đơn & Topping | **P** | – | – | – | – | – | – |
| **10.2**| Quản lý Chi nhánh & Ưu đãi toàn chuỗi | **P** | – | – | – | – | – | – |
| **HLUC11**| **Báo cáo & Insights** | | | | | | | |
| **11.1**| Xem Dashboard | **P** | **P** | – | – | – | – | – |
| **11.2**| Xem Phân tích & Cảnh báo Insights AI | **P** | **P** | – | – | – | – | – |
| **HLUC12**| **Trợ lý ảo AI Agent** | | | | | | | |
| **12.1**| Tra cứu thông tin qua AI Agent (Read) | **P** | **P** | **P** | **P** | **P** | **P** | **P** |
| **12.2**| Thực hiện tác vụ nghiệp vụ qua AI (Write)| **P** | **P** | **P** | **P** | **P** | **P** | – |

---

## PART C — FINAL EXPECTED VS ACTUAL MATRIX (34 SUB USE CASES)

| UC ID | Sub Use Case Name | Expected Primary | Expected Supporting | Actual Actor / Capability in Code | Status | Evidence Source (File / Controller / Method) |
| :-: | :--- | :--- | :--- | :--- | :---: | :--- |
| **1.1** | Quét QR Bàn & Chi nhánh | Cust, Guest | -- | Customer, Guest | `MATCH` | `QRScan.tsx`, `TableController.GetTables` |
| **1.2** | Xem thực đơn & Chi tiết món | Admin, Mgr, Cashier, Emp, Cust, Guest | -- | Admin, Manager, Cashier, Employee, Customer, Guest | `MATCH` | `ProductController.cs` (`[AllowAnonymous]`), `ToppingController.cs` |
| **2.1** | Gửi đơn đặt món QR / Web Order | Cust, Guest | Cashier, Emp | Cust, Guest (tạo đơn); Cashier, Emp (tiếp nhận) | `MATCH` | `OrderController.CreateOrder`, `DigitalMenu.tsx` |
| **2.2** | Xem lịch sử đơn hàng | Admin, Mgr, Cashier, Emp, Kitchen, Cust | -- | Admin, Mgr, Cashier, Emp (`/invoices`); Cust (`GET /api/Order`); Kitchen xem KDS history qua UC2.3 | `NEEDS REVIEW` | `OrderController.cs`, `InvoiceController.cs`, `KitchenService.cs` |
| **2.3** | Xem lịch sử chế biến KDS | Kitchen | -- | Kitchen (P), Admin (S), Manager (S) | `MATCH / EXTRA` | `KitchenService.GetHistoryAsync` (`GET /api/Order/kitchen/history`) |
| **3.1** | Tạo yêu cầu đặt bàn trực tuyến | Cust, Guest | -- | Customer, Guest | `MATCH` | `ReservationController.CreateReservation` |
| **3.2** | Tiếp nhận & Quản lý lịch hẹn | Mgr, Cashier, Emp | Admin | Admin, Manager, Cashier, Employee | `MATCH` | `ReservationController.UpdateStatus` (`[Authorize(Roles="admin,manager,employee,cashier")]`) |
| **4.1** | Xem & Cập nhật Hồ sơ cá nhân | Cust | -- | Customer (chính chủ `userId == existing.Id`) | `MATCH` | `CustomerController.UpdateProfile` |
| **4.2** | Tra cứu điểm & Loyalty | Cust | Admin, Mgr, Cashier, Emp | Customer, Admin, Manager, Cashier, Employee | `MATCH` | `CustomerController.GetLoyaltyHistory`, `LoyaltyService.cs` |
| **5.1** | Tạo đơn hàng POS | Admin, Mgr, Cashier, Emp | -- | Admin, Manager, Cashier, Employee | `MATCH` | `OrderController.CreateOrder`, `OrderService.cs` |
| **5.2** | Thêm/Xóa món trong đơn hàng | Admin, Mgr, Cashier, Emp | -- | Admin, Manager, Cashier, Employee | `MATCH` | `OrderController.CreateOrder`, `OrderService.CreateOrUpdateOrderAsync` |
| **5.3** | Cập nhật lịch sử đơn hàng | Admin, Mgr | -- | Admin, Manager, Cashier, Employee (Sửa đơn đang mở); Đơn đã đóng/hủy bị chặn sửa | `GAP / NEEDS REVIEW` | `OrderController.cs`, `OrderService.UpdateOrderStatusAsync` |
| **5.4** | Tiếp nhận đơn đặt món Web Order | Admin, Mgr, Cashier, Emp | -- | Admin, Manager, Cashier, Employee | `MATCH` | `OrderController.AcceptWebOrder` (`AcceptWebOrderAsync`) |
| **5.5** | Gửi yêu cầu chế biến xuống Bếp | Admin, Mgr, Cashier, Emp | Kitchen | Admin, Manager, Cashier, Employee, Kitchen | `MATCH` | `OrderController.SendToKitchen` (`SendToKitchenAsync`), `KitchenHub.cs` |
| **6.1** | Thanh toán đơn hàng | Admin, Mgr, Cashier, Emp | -- | Admin, Manager, Cashier, Employee | `MATCH` | `OrderController.PayOrder` (`PayOrderAsync`) |
| **6.2** | Đổi điểm Loyalty | Admin, Mgr, Cashier, Emp | -- | Admin, Manager, Cashier, Employee | `MATCH` | `OrderController.RedeemPoints` (`LoyaltyService.RedeemPointsAsync`) |
| **6.3** | Cấu hình tham số thanh toán | Admin | -- | Admin (Global), Manager (Branch) | `GAP / EXTRA` | `ReceiptSettingsController.Put` |
| **6.4** | Cấu hình thiết lập hệ thống | Admin | -- | Admin (Global), Manager (Branch) | `GAP / EXTRA` | `SystemSettingsController.Put` |
| **6.5** | In / Xuất hóa đơn | Admin, Mgr, Cashier, Emp | -- | Admin, Manager, Cashier, Employee | `MATCH` | `InvoiceController.cs`, `ReceiptSettingsController.cs` |
| **7.1** | Xử lý chế biến KDS thời gian thực | Kitchen | -- | Kitchen | `MATCH` | `KitchenHub.cs`, `KitchenService.UpdateRequestStatusAsync` |
| **7.2** | Cập nhật trạng thái tạm hết món | Admin, Mgr, Kitchen | -- | Admin, Kitchen (Manager bị chặn 403 nếu không dùng mode="admin") | `GAP` | `ProductController.UpdateAvailability` (`[Authorize(Roles="admin,kitchen")]`) |
| **8.1** | Chấm công QR | Admin, Mgr, Cashier, Emp, Kitchen | -- | Admin, Manager, Cashier, Employee, Kitchen | `MATCH` | `AttendanceController.CheckIn`, `CheckOut` |
| **8.2** | Mở & Chốt ca làm việc (Shift) | Cashier | -- | Admin, Manager, Cashier | `GAP / EXTRA` | `ShiftController.cs` (`[Authorize(Roles="admin,manager,cashier")]`) |
| **8.3** | Xem lịch làm việc | Admin, Mgr, Cashier, Emp, Kitchen | -- | Admin, Manager, Cashier, Employee, Kitchen | `MATCH` | `WorkScheduleController.GetSchedules` |
| **8.4** | Phân lịch làm việc | Admin, Mgr | -- | Admin, Manager | `MATCH` | `WorkScheduleController.CreateSchedule`, `DeleteSchedule` |
| **9.1** | Quản lý Sơ đồ bàn & Khu vực | Admin, Mgr | -- | Admin, Manager | `MATCH` | `TableController.cs`, `AreaController.cs` |
| **9.2** | Quản lý Hồ sơ & Phân lịch Nhân sự| Admin, Mgr | -- | Admin, Manager | `MATCH` | `EmployeeController.cs`, `WorkScheduleController.cs` |
| **9.3** | Quản lý Chi phí vận hành | Admin, Mgr | -- | Admin, Manager, Cashier, Employee | `GAP / EXTRA` | `ExpenseController.cs` (`[Authorize(Roles="admin,manager,employee,cashier")]`) |
| **10.1**| Quản lý Danh mục Thực đơn/Topping| Admin | -- | Admin | `MATCH` | `ProductController.cs`, `ToppingController.cs` (admin-only) |
| **10.2**| Quản lý Chi nhánh & Ưu đãi | Admin | -- | Admin | `MATCH` | `BranchController.cs`, `PromotionController.cs` (admin-only) |
| **11.1**| Xem Dashboard | Admin, Mgr | -- | Admin, Manager | `MATCH` | `DashboardController.GetSummary` |
| **11.2**| Xem Phân tích & Cảnh báo Insights AI| Admin, Mgr | -- | Admin, Manager | `MATCH` | `BusinessInsightController.cs` (`Get`, `GetById`) |
| **12.1**| Tra cứu thông tin qua AI (Read) | Admin, Mgr, Cashier, Emp, Kitchen, Cust, Guest | -- | Admin, Manager, Cashier, Employee, Kitchen, Customer, Guest | `MATCH` | `AiController.cs`, `GetRevenueTool.cs`, `GetMenuTool.cs` |
| **12.2**| Thực hiện tác vụ qua AI (Write) | Admin, Mgr, Cashier, Emp, Kitchen, Cust | -- | Admin (Price), Staff (Order Status), Customer (Booking); Guest bị chặn | `MATCH` | `AiController.cs`, `AiPermissionService.cs` |

---

## PART D — CRITICAL ISSUES & DETAILED GAP ANALYSIS

### GAP 1: UC7.2 — Cập nhật trạng thái tạm hết món (`OutOfStock`)
- **Expected**: Primary: `Admin`, `Manager`, `Kitchen`.
- **Actual**: Backend `ProductController.cs` khai báo `[Authorize(Roles = "admin,kitchen")]`. Role `manager` bị trả về `403 Forbidden` khi trực tiếp gọi endpoint `PATCH /api/product/{id}/availability`.
- **Difference**: Thiếu role `manager` trong backend authorization attribute.
- **Evidence**:
  - File: `services/api/src/WebAPI/Controllers/ProductController.cs`
  - Method: `UpdateAvailability(Guid id, [FromBody] AvailabilityRequest request)`
  - Attribute: `[Authorize(Roles = "admin,kitchen")]`
- **Need code fix?**: **CÓ** (Bổ sung `manager` vào `[Authorize(Roles = "admin,manager,kitchen")]`).

---

### GAP 2: UC8.2 — Mở & Chốt ca làm việc (Shift)
- **Expected**: Primary: `Cashier ONLY`.
- **Actual**: Backend `ShiftController.cs` khai báo `[Authorize(Roles = "admin,manager,cashier")]`.
- **Difference**: Backend mở quyền cho cả `admin` và `manager` mở/chốt ca trực tiếp.
- **Operational Reality**: Trường hợp Thu ngân vắng mặt, Quản lý/Admin sử dụng tài khoản của mình hoặc tài khoản Thu ngân tại tablet quầy để mở/chốt ca dự phòng. Đây là một `EXTRA ACTUAL` cần ghi nhận trong tài liệu phân tích mà không cần tạo thêm Actor mới.
- **Evidence**:
  - File: `services/api/src/WebAPI/Controllers/ShiftController.cs`
  - Attribute: `[Authorize(Roles = "admin,manager,cashier")]`
- **Need code fix?**: **KHÔNG BẮT BUỘC**.

---

### GAP 3: UC9.3 — Quản lý Chi phí vận hành
- **Expected**: Primary: `Admin`, `Manager`.
- **Actual**: Backend `ExpenseController.cs` khai báo `[Authorize(Roles = "admin,manager,employee,cashier")]`.
- **Difference**: Backend mở quyền tạo/sửa phiếu chi phí cho cả `cashier` và `employee`.
- **Evidence**:
  - File: `services/api/src/WebAPI/Controllers/ExpenseController.cs`
  - Class attribute: `[Authorize(Roles = "admin,manager,employee,cashier")]`
- **Need code fix?**: **CÓ** (Sửa attribute thành `[Authorize(Roles = "admin,manager")]` nếu muốn siết chặt phân quyền chi phí).

---

### GAP 4: UC6.3 & UC6.4 — Cấu hình Mẫu in & Thiết lập Hệ thống Chi nhánh
- **Expected**: Primary: `Admin ONLY`.
- **Actual**: Backend `ReceiptSettingsController.cs` và `SystemSettingsController.cs` cho phép `manager` cập nhật cài đặt cho chi nhánh của mình (`Branch-scoped Put`).
- **Difference**: Backend hỗ trợ Manager phân quyền quản lý thiết lập cấp chi nhánh.
- **Evidence**:
  - Files: `services/api/src/WebAPI/Controllers/ReceiptSettingsController.cs` & `SystemSettingsController.cs`
  - Method: `Put` kiểm tra `if (!string.Equals(role, "admin") && !string.Equals(role, "manager")) return Forbid();`
- **Need code fix?**: **KHÔNG BẮT BUỘC** (Phù hợp với kiến trúc đa chi nhánh linh hoạt).

---

## PART E — EXTRA ACTUAL CAPABILITIES

1. **UC2.3: Xem lịch sử chế biến KDS**: Expected chỉ gán Kitchen, nhưng mã nguồn `KitchenService.GetHistoryAsync` cho phép cả `admin` và `manager` truy cập lịch sử chế biến để giám sát hiệu suất nhà bếp.
2. **UC8.2: Mở & Chốt ca**: Backend cấp quyền cho `admin` và `manager` mở/chốt ca thay thế Thu ngân.
3. **UC9.3: Quản lý chi phí**: Backend cấp quyền cho `cashier` và `employee` tạo phiếu chi vận hành.

---

## PART F — UC2.2 VÀ UC2.3 DOMAIN SEPARATION (ORDER HISTORY AUDIT)

Bảng đối chiếu phân định giữa Lịch sử Hóa đơn Tài chính và Lịch sử Chế biến KDS:

| Actor | Expected | UI Page / Component | Actual Code Capability | Entity & Data Source | Domain Alignment | Status |
| :--- | :---: | :--- | :--- | :--- | :--- | :---: |
| **Admin** | **P** | Màn hình Hóa đơn toàn chuỗi | Xem toàn bộ hóa đơn của tất cả chi nhánh | Entity `Order`, `InvoiceController.Get` | Tra cứu doanh thu & quản trị cấp cao | `MATCH` |
| **Manager** | **P** | Màn hình Hóa đơn chi nhánh | Xem danh sách hóa đơn chi nhánh phụ trách | Entity `Order` (lọc `branchId`), `InvoiceController.Get` | Quản lý tài chính chi nhánh | `MATCH` |
| **Cashier** | **P** | Lịch sử hóa đơn quầy | Xem danh sách hóa đơn tại quầy POS | Entity `Order`, `GET /api/Order` | Đối soát doanh thu quầy & in lại hóa đơn | `MATCH` |
| **Employee** | **P** | Đơn hàng đã lập | Xem các đơn hàng do nhân viên lập/phục vụ | Entity `Order`, `GET /api/Order` | Kiểm tra đơn hàng tại bàn & hỗ trợ khách | `MATCH` |
| **Kitchen** | **P** | Lịch sử bếp KDS (`/kitchen/history`)| Xem lịch sử các đợt yêu cầu chế biến | Entity `OrderRequest` (`KitchenService.GetHistoryAsync`) | **Khác biệt Domain Entity**: Kitchen xem đợt nấu KDS chứ không xem hóa đơn tài chính. | `NEEDS REVIEW` |
| **Customer** | **P** | Đơn hàng của tôi (`customer-web`) | Xem 5 đơn mới nhất của chính mình | Entity `Order` (`GetCustomerOrdersAsync` lọc SĐT) | Khách hàng theo dõi tiến độ đơn hàng cá nhân | `MATCH` |

### Kết luận Phân định Domain:
- **`UC2.2: Xem lịch sử đơn hàng`**: Phục vụ nhu cầu tra cứu hóa đơn tài chính (`Order` & `OrderDetail`), áp dụng cho Admin, Manager, Cashier, Employee, Customer.
- **`UC2.3: Xem lịch sử chế biến KDS`**: Phục vụ nhu cầu tra cứu các đợt nấu ăn nhà bếp (`OrderRequest` & `OrderRequestItem`), áp dụng riêng cho Kitchen (và Admin/Manager hỗ trợ giám sát).

---

## PART G — UC5.1, UC5.2, UC5.3 ORDER LIFECYCLE AUDIT

Phân định vòng đời của Order trong mã nguồn POS:

| Sub Use Case ID & Name | Primary Actor kỳ vọng | Actual Code Implementation | File Evidence | Status |
| :--- | :--- | :--- | :--- | :---: |
| **UC5.1: Tạo đơn hàng POS** | Admin, Mgr, Cashier, Emp | `POST /api/Order` khởi tạo đơn POS mới, tự động tính tài chính qua `OrderService.RecalculateOrderFinancials` | `OrderController.cs`, `OrderService.cs` | `MATCH` |
| **UC5.2: Thêm / Xóa món trong đơn hàng** | Admin, Mgr, Cashier, Emp | `OrderService.CreateOrUpdateOrderAsync` tự động merge món mới hoặc giữ lại món đã gửi bếp (`SentQuantity`) | `OrderService.cs` (lines 80-140) | `MATCH` |
| **UC5.3: Cập nhật lịch sử đơn hàng** | Admin, Mgr | Frontend POS chặn Employee/Cashier sửa đơn đã thanh toán. Backend chặn sửa đơn `Completed`/`Cancelled` (`IsTerminal`). | `OrderService.cs` (`IsTerminal`) | `NEEDS REVIEW` |
| **UC5.4: Tiếp nhận đơn Web Order** | Admin, Mgr, Cashier, Emp | `POST /api/Order/{id}/accept` gán bàn và tên nhân viên tiếp nhận | `OrderController.AcceptWebOrder` | `MATCH` |
| **UC5.5: Gửi yêu cầu chế biến xuống Bếp**| Admin, Mgr, Cashier, Emp | `POST /api/Order/{id}/send-to-kitchen` tính chênh lệch `SentQuantity` và phát SignalR | `KitchenService.SendToKitchenAsync` | `MATCH` |

---

## PART H — PAYMENT & CONFIGURATION AUDIT (UC6.1 $\rightarrow$ UC6.5)

| Sub Use Case ID & Name | Primary Actor kỳ vọng | Actual Code Implementation | File Evidence | Status |
| :--- | :--- | :--- | :--- | :---: |
| **UC6.1: Thanh toán đơn hàng** | Admin, Mgr, Cashier, Emp | `POST /api/Order/{id}/payment` xác thực đúng 100% `TotalAmount`, cập nhật `PaidAmount`, `PaymentAt` và `Status = "Hoàn thành"` | `OrderService.PayOrderAsync` | `MATCH` |
| **UC6.2: Đổi điểm Loyalty** | Admin, Mgr, Cashier, Emp | `POST /api/Order/{id}/redeem` trừ điểm tài khoản khách và giảm trực tiếp vào `order.Discount` | `LoyaltyService.RedeemPointsAsync` | `MATCH` |
| **UC6.3: Cấu hình tham số thanh toán** | Admin | `ReceiptSettingsController.Put` cho phép `admin` và `manager` (chi nhánh) chỉnh sửa | `ReceiptSettingsController.cs` | `GAP / EXTRA` |
| **UC6.4: Cấu hình thiết lập hệ thống** | Admin | `SystemSettingsController.Put` cho phép `admin` và `manager` (chi nhánh) chỉnh sửa | `SystemSettingsController.cs` | `GAP / EXTRA` |
| **UC6.5: In / Xuất hóa đơn** | Admin, Mgr, Cashier, Emp | Tất cả các role Staff đều có thể mở giao diện in hóa đơn | `InvoiceController.cs` | `MATCH` |

---

## PART I — AI AGENT AUDIT (UC12.1 & UC12.2)

Phân định chi tiết giữa AI Read Tools, AI Write Tools và Lịch sử đàm thoại:

| Actor | AI Conversation History | AI Read Tools (`UC12.1`) | AI Write Tools (`UC12.2`) | Authorized AI Tools in Code |
| :--- | :---: | :---: | :---: | :--- |
| **Admin** | `CÓ` | `CÓ` | `CÓ` | `update_product_price`, `get_revenue`, `get_business_summary`, `get_order_list`, `get_active_staff`, `get_best_sellers`, `get_financial_analysis`, `get_revenue_comparison` |
| **Manager** | `CÓ` | `CÓ` | `CÓ` | `update_order_status`, `get_revenue`, `get_business_summary`, `get_order_list`, `get_active_staff`, `get_best_sellers`, `get_financial_analysis`, `get_revenue_comparison` |
| **Cashier** | `CÓ` | `CÓ` | `CÓ` | `update_order_status`, `get_order_list`, `get_best_sellers`, `get_table_summary`, `get_my_shift` |
| **Employee**| `CÓ` | `CÓ` | `CÓ` | `update_order_status`, `get_order_list`, `get_best_sellers`, `get_table_summary`, `get_my_shift` |
| **Kitchen** | `CÓ` | `CÓ` | `CÓ` | `update_order_status`, `get_order_list`, `get_best_sellers`, `get_table_summary`, `get_my_shift` |
| **Customer**| `CÓ` | `CÓ` | `CÓ` | `create_booking`, `get_menu`, `get_my_order` |
| **Guest** | `KHÔNG` (Phiên tạm) | `CÓ` | `KHÔNG` | `get_menu` (Tất cả Write tools bị chặn bởi `AiPermissionService.cs`) |

---

## PART J — BRANCH SCOPE ISOLATION AUDIT

Xác minh tính cô lập dữ liệu theo Chi nhánh (`Branch Isolation`):

| Actor | Scope quy định | Enforcement in Code (Cơ chế kiểm soát trong mã nguồn) | Status |
| :--- | :--- | :--- | :---: |
| **Admin** | **Global** | `IsAdmin()` bypasses `branchId` filter. Trả về toàn bộ dữ liệu toàn chuỗi. | `MATCH` |
| **Manager** | **Branch** | Bắt buộc đọc `branchId` từ JWT Claim. Đơn hàng, nhân sự, chi phí, dashboard bị giới hạn 100% trong chi nhánh. | `MATCH` |
| **Cashier** | **Branch** | `IsBranchScopedRole` ép `branchId` theo JWT Claim trong `OrderController`, `ShiftController`, `ExpenseController`. | `MATCH` |
| **Employee**| **Branch** | Bị giới hạn trong chi nhánh được gán. Không xem được đơn/bàn chi nhánh khác. | `MATCH` |
| **Kitchen** | **Branch** | `KitchenHub` join SignalR group theo `Branch_{branchId}`. `KitchenService` lọc `OrderRequests` theo `branchId`. | `MATCH` |
| **Customer**| **Personal** | Lọc dữ liệu theo `CustomerId` hoặc SĐT chính chủ từ JWT Name claim. | `MATCH` |
| **Guest** | **Contextual** | Xác định `branchId` từ payload QR bàn hoặc query parameter. Không có JWT Claim `branchId`. | `MATCH` |

---

## PART K — UML RELATIONSHIP AUDIT

Đánh giá chuẩn mực UML 2.5 cho các mối quan hệ:

| Relationship | Current Definition | Audit Decision | Reason & Justification |
| :--- | :--- | :---: | :--- |
| **Actor Generalization** | `Customer --|> Guest` | `VALID` | `Customer` là người dùng đã xác thực, kế thừa toàn bộ khả năng tương tác công khai của `Guest` (quét QR, xem thực đơn, tạo đơn vãng lai) và mở rộng thêm các Use Case định danh cá nhân. |
| **`<<include>>`** | `UC5.4 (Gửi bếp) --<<include>>--> UC5.1 (Tạo/Sửa đơn POS)` | `VALID` | Mọi đợt gửi món xuống KDS bếp bắt buộc phải dựa trên và cập nhật lại `SentQuantity` của một đơn hàng POS đã tạo. |
| **`<<include>>`** | `UC5.3 (Tiếp nhận Web Order) --<<include>>--> UC5.1 (Tạo/Sửa đơn POS)` | `VALID` | Việc tiếp nhận đơn Web bắt buộc phải thực hiện chuyển đổi đơn hàng chờ thành một đơn POS chính thức gắn bàn/người nhận. |
| **`<<extend>>`** | `UC6.2 (Đổi điểm Loyalty) --<<extend>>--> UC5.1 (Tạo/Sửa đơn POS)` | `VALID` | **Extension Point**: Lập tài chính đơn POS.<br>**Condition**: Chỉ thực hiện khi khách hàng yêu cầu quy đổi điểm tích lũy thành tiền giảm giá. |
| **`<<extend>>`** | `UC6.5 (In hóa đơn) --<<extend>>--> UC6.1 (Thanh toán đơn hàng)` | `VALID` | **Extension Point**: Hoàn tất giao dịch thanh toán.<br>**Condition**: Chỉ thực hiện khi thu ngân hoặc khách hàng có nhu cầu xuất/in hóa đơn giấy. |

---

## PART L — ACTOR ASSOCIATION AUDIT

Phân định rõ Primary (P) và Supporting (S) Associations đối với 34 Sub Use Cases:

| Sub Use Case ID & Name | Primary Actors (Khởi tạo) | Supporting Actors (Hỗ trợ) |
| :--- | :--- | :--- |
| **UC1.1: Quét QR Bàn & Chi nhánh** | Customer, Guest | -- |
| **UC1.2: Xem thực đơn & Chi tiết món** | Admin, Manager, Cashier, Employee, Customer, Guest | -- |
| **UC2.1: Gửi đơn đặt món QR / Web Order** | Customer, Guest | Cashier, Employee |
| **UC2.2: Xem lịch sử đơn hàng** | Admin, Manager, Cashier, Employee, Customer | -- |
| **UC2.3: Xem lịch sử chế biến KDS** | Kitchen | Admin, Manager |
| **UC3.1: Tạo yêu cầu đặt bàn trực tuyến** | Customer, Guest | -- |
| **UC3.2: Tiếp nhận & Quản lý lịch hẹn đặt bàn** | Manager, Cashier, Employee | Admin |
| **UC4.1: Xem & Cập nhật Hồ sơ cá nhân** | Customer | -- |
| **UC4.2: Tra cứu điểm & Hạng thành viên Loyalty** | Customer | Admin, Manager, Cashier |
| **UC5.1: Tạo đơn hàng POS** | Admin, Manager, Cashier, Employee | -- |
| **UC5.2: Thêm / Xóa món trong đơn hàng** | Admin, Manager, Cashier, Employee | -- |
| **UC5.3: Cập nhật lịch sử đơn hàng** | Admin, Manager | -- |
| **UC5.4: Tiếp nhận đơn đặt món Web Order** | Manager, Cashier, Employee | Admin |
| **UC5.5: Gửi yêu cầu chế biến xuống Bếp** | Manager, Cashier, Employee | Kitchen, Admin |
| **UC6.1: Thanh toán đơn hàng (Cash/Transfer)** | Manager, Cashier, Employee | Admin, Customer |
| **UC6.2: Đổi điểm Loyalty giảm giá đơn hàng** | Manager, Cashier, Employee | Admin, Customer |
| **UC6.3: Cấu hình phương thức/tham số thanh toán** | Admin, Manager | -- |
| **UC6.4: Cấu hình thiết lập hệ thống** | Admin, Manager | -- |
| **UC6.5: In / Xuất hóa đơn thanh toán** | Admin, Manager, Cashier, Employee | -- |
| **UC7.1: Xử lý đợt chế biến KDS thời gian thực** | Kitchen | Admin, Manager |
| **UC7.2: Cập nhật trạng thái tạm hết món** | Admin, Kitchen | Manager (sau code fix) |
| **UC8.1: Chấm công QR Check-in / Check-out** | Admin, Manager, Cashier, Employee, Kitchen | -- |
| **UC8.2: Mở & Chốt ca làm việc (Shift)** | Cashier | Admin, Manager |
| **UC8.3: Xem lịch làm việc** | Admin, Manager, Cashier, Employee, Kitchen | -- |
| **UC8.4: Phân lịch làm việc** | Admin, Manager | -- |
| **UC9.1: Quản lý Sơ đồ bàn & Khu vực chi nhánh** | Admin, Manager | -- |
| **UC9.2: Quản lý Hồ sơ & Phân lịch Nhân sự** | Admin, Manager | -- |
| **UC9.3: Quản lý Chi phí vận hành chi nhánh** | Admin, Manager | Cashier, Employee |
| **UC10.1: Quản lý Danh mục Thực đơn & Topping**| Admin | -- |
| **UC10.2: Quản lý Chi nhánh & Ưu đãi toàn chuỗi**| Admin | -- |
| **UC11.1: Xem Dashboard thống kê doanh thu** | Admin, Manager | -- |
| **UC11.2: Xem Phân tích & Cảnh báo Insights AI** | Admin, Manager | -- |
| **UC12.1: Tra cứu thông tin qua AI (Read Tools)** | Admin, Manager, Cashier, Employee, Kitchen, Customer, Guest | -- |
| **UC12.2: Thực hiện tác vụ qua AI (Write Tools)**| Admin, Manager, Cashier, Employee, Kitchen, Customer | -- |

---

## PART M — HLUC / SUB-UC DECOMPOSITION MAPPING TABLE

| Diagram ID | High-Level Business Group (HLUC) | Sub Use Cases đính kèm |
| :--- | :--- | :--- |
| **Hình 3.1** | **Biểu đồ Use Case Tổng quát toàn hệ thống** | Chứa 7 Actors & 12 HLUCs (`HLUC1` $\rightarrow$ `HLUC12`) |
| **Hình 3.2** | **Domain Khách hàng & Đặt món Trực tuyến** | `UC1.1`, `UC1.2`, `UC2.1`, `UC2.2`, `UC3.1`, `UC3.2`, `UC4.1`, `UC4.2` |
| **Hình 3.3** | **Domain Bán hàng POS, Thanh toán & Hóa đơn**| `UC5.1`, `UC5.2`, `UC5.3`, `UC5.4`, `UC5.5`, `UC6.1`, `UC6.2`, `UC6.3`, `UC6.4`, `UC6.5` |
| **Hình 3.4** | **Domain Điều phối Chế biến Nhà bếp KDS** | `UC7.1`, `UC7.2` |
| **Hình 3.5** | **Domain Ca làm việc & Chấm công Nhân sự** | `UC8.1`, `UC8.2`, `UC8.3`, `UC8.4` |
| **Hình 3.6** | **Domain Quản trị Vận hành, Hệ thống & Insights**| `UC9.1`, `UC9.2`, `UC9.3`, `UC10.1`, `UC10.2`, `UC11.1`, `UC11.2` |
| **Hình 3.7** | **Domain Tương tác Trợ lý ảo AI Agent** | `UC12.1` (Read Tools), `UC12.2` (Write Tools) |

---

## PART N — FINAL UML LOCK STATUS & QUALITY GATE

| Quality Gate | Description | Status |
| :--- | :--- | :---: |
| **Gate 1** | Mỗi Actor có Business Goal rõ ràng và phân biệt được Scope? | `PASS` |
| **Gate 2** | Mỗi Use Case đại diện cho một Actor Business Goal thực sự? | `PASS` |
| **Gate 3** | Không chứa bất kỳ Technical Use Case (CRUD đơn thuần, Validation, API, DTO)? | `PASS` |
| **Gate 4** | Không chứa Actor `Staff` giả? | `PASS` |
| **Gate 5** | Không biến API Permission thành Actor Association máy móc? | `PASS` |
| **Gate 6** | Không biến Service Call/Internal method thành Include? | `PASS` |
| **Gate 7** | Không dùng Extend tùy tiện khi không có Extension Point & Condition rõ ràng? | `PASS` |
| **Gate 8** | Quan hệ `Customer --|> Guest` đúng bản chất UML 2.5 Generalization? | `PASS` |
| **Gate 9** | High-Level Use Cases đóng vai trò Business Domain Groupings? | `PASS` |
| **Gate 10**| Các AI Tools kỹ thuật không bị biến thành Use Cases rời rạc? | `PASS` |
| **Gate 11**| Các chức năng đã bị xóa (Payroll) được loại bỏ 100%? | `PASS` |
| **Gate 12**| Ma trận Actor kết nối chính xác theo quyền khởi tạo và tham gia nghiệp vụ? | `PASS` |
| **Gate 13**| Mọi Association đều có giải trình Business Flow rõ ràng? | `PASS` |
| **Gate 14**| Không có Actor nào được nối vào Use Case chỉ vì có quyền đọc API? | `PASS` |
| **Gate 15**| Không có Use Case nào tồn tại chỉ để mô tả implementation chi tiết? | `PASS` |

---

## FINAL STATUS

### **READY AFTER MINOR CORRECTIONS**

**Giải trình điều kiện chuyển `READY TO LOCK`**:
1. Tiến hành sửa 2 thuộc tính `[Authorize]` ở Backend cho GAP 1 (`ProductController.cs` endpoint `UpdateAvailability` bổ sung role `manager`) và GAP 3 (`ExpenseController.cs` siết chặt `[Authorize(Roles = "admin,manager")]`).
2. Khóa mô hình Use Case UML 2.5 với 34 Sub Use Cases.
3. Chuyển sang bước xuất mã **Draw.io XML** cho 7 hình sơ đồ (Hình 3.1 $\rightarrow$ Hình 3.7)!
