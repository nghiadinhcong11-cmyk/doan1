# BÁO CÁO AUDIT VÀ ĐỐI CHIẾU MÔ HÌNH USE CASE EXPECTED VS ACTUAL SOURCE CODE

**Đề tài**: Hệ thống quản lý nhà hàng đa chi nhánh tích hợp Trợ lý ảo AI Agent  
**Chức danh**: Senior System Analyst / Software Architect / Requirements Engineer / Code Auditor  
**Nguyên tắc cốt lõi**:
- **EXPECTED BASELINE**: Mô hình nghiệp vụ / ma trận Use Case mong muốn cố định (không tự ý thay đổi).
- **ACTUAL SOURCE CODE**: Hành vi thực tế đang tồn tại trong mã nguồn C# (.NET 8 WebAPI, EF Core, SignalR) và TypeScript (React Frontend).
- Độc lập hoàn toàn giữa Expected và Actual. Mọi điểm lệch quyền, thừa quyền hoặc thiếu quyền đều được ghi nhận minh bạch dưới dạng **GAP**, **EXTRA**, **MISSING** hoặc **NEEDS REVIEW**.

---

## PART A — EXECUTIVE SUMMARY (TỔNG QUAN KẾT QUẢ AUDIT)

- **Tổng số Business Actors**: 7 Actors (`Admin`, `Manager`, `Cashier`, `Employee`, `Kitchen`, `Customer`, `Guest`).
- **Tổng số High-Level Use Cases (HLUC)**: 12 HLUCs (`HLUC1` $\rightarrow$ `HLUC12`).
- **Tổng số Detailed Sub Use Cases được audit**: **34 Sub-UCs**.
- **Số Use Case khớp hoàn toàn giữa Expected và Actual (`MATCH`)**: **26 / 34 Sub-UCs** (76.5%).
- **Số Use Case có độ lệch / mở rộng quyền trong Code (`GAP / EXTRA ACTUAL`)**: **6 / 34 Sub-UCs** (17.6%) (Gồm: UC5.3, UC6.3, UC6.4, UC7.2, UC8.2, UC9.3).
- **Số Use Case cần xem xét lại thiết kế nghiệp vụ (`NEEDS REVIEW`)**: **2 / 34 Sub-UCs** (5.9%) (Gồm: UC2.2 Lịch sử đơn hàng vs UC2.3 Lịch sử chế biến; UC5.3 Cập nhật đơn lịch sử).
- **Số Use Case thiếu hoàn toàn implementation (`MISSING`)**: **0 / 34 Sub-UCs** (0%) (Toàn bộ 34 Sub-UCs đều đã có implementation thực tế trong Backend/Frontend).

---

## PART B — FINAL EXPECTED ACTOR MATRIX (BẢNG EXPECTED BASELINE CỐ ĐỊNH)

Ký hiệu: **`P`** = Primary Actor; **`S`** = Supporting Actor; **`–`** = Không tham gia.

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
| **3.2** | Tiếp nhận & Quản lý lịch hẹn | **S** | **P** | **P** | **P** | – | – | – |
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

## PART C — EXPECTED VS ACTUAL MATRIX (BẢNG ĐỐI CHIẾU CHI TIẾT)

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

## PART D — DETAILED GAP (CHI TIẾT CÁC ĐIỂM LỆCH VÀ PHƯƠNG ÁN KHẮC PHỤC)

### GAP 1: UC7.2 — Cập nhật trạng thái tạm hết món (`OutOfStock`)
- **Expected**: Primary: `Admin`, `Manager`, `Kitchen`.
- **Actual**: Backend `ProductController.cs` khai báo `[Authorize(Roles = "admin,kitchen")]`. Role `manager` bị trả về `403 Forbidden` khi trực tiếp gọi endpoint `PATCH /api/product/{id}/availability`.
- **Difference**: Thiếu role `manager` trong backend authorization attribute.
- **Evidence**:
  - File: `services/api/src/WebAPI/Controllers/ProductController.cs`
  - Method: `UpdateAvailability(Guid id, [FromBody] AvailabilityRequest request)`
  - Attribute: `[Authorize(Roles = "admin,kitchen")]`
- **Need code fix?**: **CÓ**.
- **Recommended Fix**: Sửa attribute thành `[Authorize(Roles = "admin,manager,kitchen")]`.

---

### GAP 2: UC8.2 — Mở & Chốt ca làm việc (Shift)
- **Expected**: Primary: `Cashier ONLY`.
- **Actual**: Backend `ShiftController.cs` khai báo `[Authorize(Roles = "admin,manager,cashier")]`.
- **Difference**: Backend mở quyền cho cả `admin` và `manager` mở/chốt ca trực tiếp.
- **Evidence**:
  - File: `services/api/src/WebAPI/Controllers/ShiftController.cs`
  - Class level attribute: `[Authorize(Roles = "admin,manager,cashier")]`
- **Need code fix?**: **KHÔNG BẮT BUỘC** (Thực tế vận hành Quản lý/Admin cần có quyền can thiệp ca khi Thu ngân vắng mặt). Đánh dấu là `EXTRA ACTUAL` trong hồ sơ phân tích.

---

### GAP 3: UC9.3 — Quản lý Chi phí vận hành
- **Expected**: Primary: `Admin`, `Manager`.
- **Actual**: Backend `ExpenseController.cs` khai báo `[Authorize(Roles = "admin,manager,employee,cashier")]`.
- **Difference**: Backend mở quyền tạo/sửa phiếu chi phí cho cả `cashier` và `employee`.
- **Evidence**:
  - File: `services/api/src/WebAPI/Controllers/ExpenseController.cs`
  - Class level attribute: `[Authorize(Roles = "admin,manager,employee,cashier")]`
- **Need code fix?**: **CÓ** (Nếu quy trình nhà hàng muốn siết chặt chỉ Manager/Admin mới được ghi nhận chi phí). Sửa attribute thành `[Authorize(Roles = "admin,manager")]`.

---

### GAP 4: UC6.3 & UC6.4 — Cấu hình Mẫu in & Thiết lập Hệ thống Chi nhánh
- **Expected**: Primary: `Admin ONLY`.
- **Actual**: Backend `ReceiptSettingsController.cs` và `SystemSettingsController.cs` cho phép `manager` cập nhật cài đặt cho chi nhánh của mình (`Branch-scoped Put`).
- **Difference**: Backend hỗ trợ Manager phân quyền quản lý thiết lập cấp chi nhánh.
- **Evidence**:
  - File: `services/api/src/WebAPI/Controllers/ReceiptSettingsController.cs` & `SystemSettingsController.cs`
  - Method: `Put` kiểm tra `if (!string.Equals(role, "admin") && !string.Equals(role, "manager")) return Forbid();`
- **Need code fix?**: **KHÔNG BẮT BUỘC** (Phù hợp với kiến trúc đa chi nhánh linh hoạt).

---

## PART E — EXTRA ACTUAL (QUYỀN THỪA NGOÀI EXPECTED)

1. **UC2.3: Xem lịch sử chế biến KDS**: Expected chỉ gán Kitchen, nhưng mã nguồn `KitchenService.GetHistoryAsync` cho phép cả `admin` và `manager` truy cập lịch sử chế biến để giám sát hiệu suất nhà bếp.
2. **UC8.2: Mở & Chốt ca**: Backend cấp quyền cho `admin` và `manager` mở/chốt ca thay thế Thu ngân.
3. **UC9.3: Quản lý chi phí**: Backend cấp quyền cho `cashier` và `employee` tạo phiếu chi vận hành.

---

## PART F — ORDER HISTORY AUDIT (PHÂN TÍCH XEM LỊCH SỬ ĐƠN HÀNG)

| Actor | Expected | UI Page / Component | Actual Code Capability | Entity & Data Source | Status |
| :--- | :---: | :--- | :--- | :--- | :---: |
| **Admin** | **P** | Màn hình Hóa đơn toàn chuỗi | Xem toàn bộ hóa đơn của tất cả chi nhánh | Entity `Order`, `InvoiceController.Get` | `MATCH` |
| **Manager** | **P** | Màn hình Hóa đơn chi nhánh | Xem danh sách hóa đơn chi nhánh phụ trách | Entity `Order` (lọc `branchId`), `InvoiceController.Get` | `MATCH` |
| **Cashier** | **P** | Lịch sử hóa đơn quầy | Xem danh sách hóa đơn tại quầy POS | Entity `Order`, `GET /api/Order` | `MATCH` |
| **Employee** | **P** | Đơn hàng đã lập | Xem các đơn hàng do nhân viên lập/phục vụ | Entity `Order`, `GET /api/Order` | `MATCH` |
| **Kitchen** | **P** | Lịch sử bếp KDS (`/kitchen/history`)| Xem lịch sử các đợt yêu cầu chế biến | Entity `OrderRequest` (`KitchenService.GetHistoryAsync`) | `NEEDS REVIEW` |
| **Customer** | **P** | Đơn hàng của tôi (`customer-web`) | Xem 5 đơn mới nhất của chính mình | Entity `Order` (`GetCustomerOrdersAsync` lọc SĐT) | `MATCH` |

### Kết luận Phân định Domain:
- **`UC2.2: Xem lịch sử đơn hàng`**: Phục vụ nhu cầu tra cứu hóa đơn tài chính (`Order` & `OrderDetail`), áp dụng cho Admin, Manager, Cashier, Employee, Customer.
- **`UC2.3: Xem lịch sử chế biến KDS`**: Phục vụ nhu cầu tra cứu các đợt nấu ăn nhà bếp (`OrderRequest` & `OrderRequestItem`), áp dụng riêng cho Kitchen (và Admin/Manager hỗ trợ giám sát).

---

## PART G — CREATE / ADD / DELETE / UPDATE ORDER AUDIT

| Sub Use Case ID & Name | Primary Actor kỳ vọng | Actual Code Implementation | File Evidence | Status |
| :--- | :--- | :--- | :--- | :---: |
| **UC5.1: Tạo đơn hàng POS** | Admin, Mgr, Cashier, Emp | `POST /api/Order` khởi tạo đơn POS mới, tự động tính tài chính | `OrderController.cs`, `OrderService.cs` | `MATCH` |
| **UC5.2: Thêm / Xóa món trong đơn hàng** | Admin, Mgr, Cashier, Emp | `OrderService.CreateOrUpdateOrderAsync` tự động merge món mới hoặc giữ lại món đã gửi bếp | `OrderService.cs` (lines 80-140) | `MATCH` |
| **UC5.3: Cập nhật lịch sử đơn hàng** | Admin, Mgr | Frontend POS chặn Employee/Cashier sửa đơn đã thanh toán. Backend chặn sửa đơn `Completed`/`Cancelled`. | `OrderService.cs` (`IsTerminal`) | `NEEDS REVIEW` |
| **UC5.4: Tiếp nhận đơn đặt món Web Order** | Admin, Mgr, Cashier, Emp | `POST /api/Order/{id}/accept` gán bàn và tên nhân viên tiếp nhận | `OrderController.AcceptWebOrder` | `MATCH` |
| **UC5.5: Gửi yêu cầu chế biến xuống Bếp**| Admin, Mgr, Cashier, Emp | `POST /api/Order/{id}/send-to-kitchen` tính chênh lệch `SentQuantity` và phát SignalR | `KitchenService.SendToKitchenAsync` | `MATCH` |

---

## PART H — PAYMENT / INVOICE AUDIT

| Sub Use Case ID & Name | Primary Actor kỳ vọng | Actual Code Implementation | File Evidence | Status |
| :--- | :--- | :--- | :--- | :---: |
| **UC6.1: Thanh toán đơn hàng** | Admin, Mgr, Cashier, Emp | `POST /api/Order/{id}/payment` xác thực đúng 100% `TotalAmount`, cập nhật `PaidAmount`, `PaymentAt` và `Status = "Hoàn thành"` | `OrderService.PayOrderAsync` | `MATCH` |
| **UC6.2: Đổi điểm Loyalty** | Admin, Mgr, Cashier, Emp | `POST /api/Order/{id}/redeem` trừ điểm tài khoản khách và giảm trực tiếp vào `order.Discount` | `LoyaltyService.RedeemPointsAsync` | `MATCH` |
| **UC6.3: Cấu hình tham số thanh toán** | Admin | `ReceiptSettingsController.Put` cho phép `admin` và `manager` (chi nhánh) chỉnh sửa | `ReceiptSettingsController.cs` | `GAP / EXTRA` |
| **UC6.4: Cấu hình thiết lập hệ thống** | Admin | `SystemSettingsController.Put` cho phép `admin` và `manager` (chi nhánh) chỉnh sửa | `SystemSettingsController.cs` | `GAP / EXTRA` |
| **UC6.5: In / Xuất hóa đơn** | Admin, Mgr, Cashier, Emp | Tất cả các role Staff đều có thể mở giao diện in hóa đơn | `InvoiceController.cs` | `MATCH` |

---

## PART I — SHIFT AUDIT (UC8.2 MỞ & CHỐT CA)

- **Expected**: Primary: `Cashier ONLY`.
- **Actual Code**:
  - Controller: `ShiftController.cs` có `[Authorize(Roles = "admin,manager,cashier")]`.
  - Frontend: Màn hình Quản lý ca (`ShiftManagement.tsx`) cho phép Thu ngân, Quản lý chi nhánh và Admin thực hiện mở/chốt ca.
- **Đánh giá**: Code cho phép cả Admin và Manager thực hiện mở/chốt ca dự phòng khi Thu ngân vắng mặt. Đây là một `EXTRA ACTUAL` cần ghi nhận trong tài liệu phân tích.

---

## PART J — EXPENSE AUDIT (UC9.3 QUẢN LÝ CHI PHÍ VẬN HÀNH)

- **Expected**: Primary: `Admin`, `Manager`.
- **Actual Code**:
  - Controller: `ExpenseController.cs` có `[Authorize(Roles = "admin,manager,employee,cashier")]`.
- **Đánh giá**: Backend cho phépả Employee và Cashier tạo phiếu chi phí. Nếu quy trình nghiệp vụ yêu cầu siết chặt chỉ Manager/Admin ghi nhận chi phí, cần tiến hành **Code Fix** trên `ExpenseController.cs`.

---

## PART K — OUT-OF-STOCK AUDIT (UC7.2 BÁO TẠM HẾT MÓN)

- **Expected**: Primary: `Admin`, `Manager`, `Kitchen`.
- **Actual Code**:
  - Controller: `ProductController.cs` method `UpdateAvailability` có attribute `[Authorize(Roles = "admin,kitchen")]`.
- **Đánh giá**: Role `manager` bị trả về `403 Forbidden` do thiếu trong attribute backend. Đây là **GAP cần sửa code** (`Code Fix`).

---

## PART L — AI AGENT AUDIT

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

## PART M — BRANCH SCOPE AUDIT

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

## PART N — UML SEMANTIC REVIEW

1. **Actor Generalization (`Customer --|> Guest`)**: `VALID`. `Customer` là người dùng đã xác thực, kế thừa toàn bộ khả năng tương tác công khai của `Guest` và mở rộng thêm các Use Case định danh cá nhân.
2. **`<<include>>` Relationships**:
   - `UC5.4 (Gửi bếp) --<<include>>--> UC5.1 (Tạo/Sửa đơn POS)`: `VALID`.
   - `UC5.3 (Tiếp nhận Web Order) --<<include>>--> UC5.1 (Tạo/Sửa đơn POS)`: `VALID`.
3. **`<<extend>>` Relationships**:
   - `UC6.2 (Đổi điểm Loyalty) --<<extend>>--> UC5.1 (Tạo/Sửa đơn POS)`: `VALID`.
   - `UC6.5 (In hóa đơn) --<<extend>>--> UC6.1 (Thanh toán đơn hàng)`: `VALID`.

---

## PART O — BÁO CÁO TỔNG HỢP VÀ KẾT LUẬN CUỐI CÙNG

### A. MATCH — CÓ THỂ GIỮ NGUYÊN (26 SUB-UCS)
- `UC1.1`, `UC1.2`, `UC2.1`, `UC2.3`, `UC3.1`, `UC3.2`, `UC4.1`, `UC4.2`, `UC5.1`, `UC5.2`, `UC5.4`, `UC5.5`, `UC6.1`, `UC6.2`, `UC6.5`, `UC7.1`, `UC8.1`, `UC8.3`, `UC8.4`, `UC9.1`, `UC9.2`, `UC10.1`, `UC10.2`, `UC11.1`, `UC11.2`, `UC12.1`, `UC12.2`.

---

### B. GAP / EXTRA — CẦN XEM XÉT SỬA CODE HOẶC ĐIỀU CHỈNH HỒ SƠ (6 SUB-UCS)
1. **UC7.2 (Cập nhật tạm hết món)**: Sửa code backend `ProductController.cs` bổ sung Role `manager` vào `[Authorize(Roles = "admin,manager,kitchen")]`.
2. **UC9.3 (Quản lý chi phí)**: Sửa code backend `ExpenseController.cs` đổi thành `[Authorize(Roles = "admin,manager")]` nếu không muốn cho Cashier/Employee tự ý tạo phiếu chi.
3. **UC5.3 (Cập nhật đơn lịch sử)**: Cần làm rõ trên tài liệu UML: UC5.3 là "Điều chỉnh đơn nâng cao" dành cho Admin/Manager, phân biệt với gọi thêm món tại bàn của POS (`UC5.1`/`UC5.2`).
4. **UC8.2 (Mở & Chốt ca)**: Giữ nguyên Backend cho phép Admin/Manager hỗ trợ chốt ca khi vắng Thu ngân, ghi nhận Extra Actual trong hồ sơ.
5. **UC6.3 & UC6.4 (Cấu hình thanh toán & Thiết lập)**: Giữ nguyên Backend cho phép Manager tùy chỉnh cài đặt cho chi nhánh của mình.

---

### C. NEEDS REVIEW — PHÂN ĐỊNH DOMAIN & LƯU HỒ SƠ (2 SUB-UCS)
1. **UC2.2 (Xem lịch sử đơn hàng)**: Tách biệt rõ `UC2.2` (Lịch sử hóa đơn tài chính `Order` cho Admin, Manager, Cashier, Employee, Customer) và `UC2.3` (Lịch sử đợt chế biến nhà bếp `OrderRequest` cho Kitchen).

---

### RECOMMENDED NEXT STEP (CÁC BƯỚC TIẾP THEO)
1. Tiến hành sửa code backend cho **GAP 1 (`UC7.2`)** và **GAP 2 (`UC9.3`)** nếu muốn siết chặt phân quyền 100% theo Expected.
2. Khóa hồ sơ Phân tích Thiết kế UML 2.5 với 34 Sub Use Cases đã audit.
3. Tiến hành sinh mã **Draw.io XML** cho từng biểu đồ từ **Hình 3.1 đến Hình 3.7** phục vụ hoàn thiện Báo cáo Đồ án!
