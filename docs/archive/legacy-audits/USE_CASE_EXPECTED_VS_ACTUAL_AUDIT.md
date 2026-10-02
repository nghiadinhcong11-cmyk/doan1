# BÁO CÁO AUDIT EXPECTED VS ACTUAL — FINAL USE CASE & ACTOR ALIGNMENT

**Đề tài**: Hệ thống quản lý nhà hàng đa chi nhánh tích hợp Trợ lý ảo AI Agent  
**Đơn vị thực hiện**: Senior System Analyst / Software Architect  
**Phương pháp**: Đối chiếu **Expected Business Model (Mô hình Nghiệp vụ mong muốn)** với **Actual Implementation (Mã nguồn thực tế)** để xác định các điểm MATCH, GAP, MISSING và NEEDS REVIEW.

---

## PART A — EXECUTIVE SUMMARY (TỔNG QUAN AUDIT)

- **Tổng số High-Level Use Cases (HLUC)**: 12 HLUCs
- **Tổng số Sub Use Cases được audit**: 29 Sub-UCs
- **Tổng số Business Actors**: 7 Actors (`Admin`, `Manager`, `Cashier`, `Employee`, `Kitchen`, `Customer`, `Guest`)
- **Số Use Case khớp hoàn toàn (`MATCH`)**: **20 / 29 Sub-UCs**
- **Số Use Case có độ lệch phân quyền backend (`GAP`)**: **7 / 29 Sub-UCs** (Chủ yếu do Backend cấp quyền rộng hơn Expected đối với Role `employee` và `manager`)
- **Số Use Case cần xem xét lại nghiệp vụ (`NEEDS REVIEW`)**: **2 / 29 Sub-UCs** (Gồm: Phân định "Xem lịch sử đơn hàng" giữa các Actor và Phân định "Cập nhật đơn hàng" trên POS)
- **Số tính năng bị thiếu/chưa triển khai (`MISSING`)**: **0 / 29 Sub-UCs** (Tất cả 29 Sub-UCs trong Expected Model đều đã có implementation thực tế trong Backend & Frontend)

---

## PART B — EXPECTED VS ACTUAL ACTOR MATRIX

Bảng đối chiếu tổng quan giữa Expected Business Model và Actual Implementation trong mã nguồn:

| STT | Detailed Sub Use Case | Expected Actors | Actual Code Authorization | Status | Evidence Source (Source Code) |
| :-: | :--- | :--- | :--- | :---: | :--- |
| **1.1** | Quét QR Bàn & Chi nhánh | Cust (P), Guest (P) | Customer, Guest | `MATCH` | `QRScan.tsx`, `TableController.cs` |
| **1.2** | Xem thực đơn & Chi tiết món | Admin (P), Mgr (P), Cashier (P), Emp (P), Cust (P), Guest (P) | Admin, Manager, Cashier, Employee, Kitchen, Customer, Guest | `MATCH` | `ProductController.cs` (`[AllowAnonymous]`) |
| **2.1** | Gửi đơn đặt món QR / Web Order | Cust (P), Guest (P), Cashier (S), Emp (S) | Customer, Guest, Cashier (S), Employee (S) | `MATCH` | `OrderController.cs` (`CreateOrder`) |
| **2.2** | Xem lịch sử đơn hàng | Admin (P), Mgr (P), Cashier (P), Emp (P), Kitchen (P) | Cust (P), Admin (P), Mgr (P), Cashier (P), Emp (P), Kitchen (P - KDS History) | `NEEDS REVIEW` | `OrderController.cs`, `KitchenService.cs` (`GetHistoryAsync`) |
| **3.1** | Tạo yêu cầu đặt bàn trực tuyến | Cust (P), Guest (P) | Customer, Guest, Staff | `MATCH` | `ReservationController.cs` (`CreateReservation`) |
| **3.2** | Tiếp nhận & Quản lý lịch hẹn đặt bàn | Mgr (P), Cashier (P), Emp (P), Admin (S) | Admin, Manager, Cashier, Employee | `MATCH` | `ReservationController.cs` (`[Authorize(Roles="admin,manager,employee,cashier")]`) |
| **4.1** | Xem & Cập nhật Hồ sơ cá nhân | Cust (P) | Customer (chính chủ) | `MATCH` | `CustomerController.cs` (`UpdateProfile`) |
| **4.2** | Tra cứu điểm & Hạng thành viên Loyalty | Cust (P), Admin (S), Mgr (S), Cashier (S) | Customer, Admin, Manager, Cashier, Employee | `MATCH` | `CustomerController.cs` (`GetLoyaltyHistory`), `LoyaltyService.cs` |
| **5.1** | Tạo đơn hàng POS | Admin (P), Mgr (P), Cashier (P), Emp (P) | Admin, Manager, Cashier, Employee | `MATCH` | `OrderController.cs` (`CreateOrder`), `OrderService.cs` |
| **5.2** | Cập nhật đơn hàng | Admin (P), Mgr (P) | Admin, Manager, Cashier, Employee | `GAP` | `OrderController.cs` (`CreateOrder` cho phép sửa order) |
| **5.3** | Tiếp nhận đơn đặt món Web Order | Mgr (P), Cashier (P), Emp (P), Admin (S) | Admin, Manager, Cashier, Employee | `MATCH` | `OrderController.cs` (`AcceptWebOrder`) |
| **5.4** | Gửi yêu cầu chế biến xuống Bếp | Mgr (P), Cashier (P), Emp (P), Admin (S), Kitchen (S) | Admin, Manager, Cashier, Employee, Kitchen | `MATCH` | `OrderController.cs` (`SendToKitchen`) |
| **6.1** | Thanh toán đơn hàng | Mgr (P), Cashier (P), Admin (S), Cust (S) | Admin, Manager, Cashier, Employee | `GAP` | `OrderController.cs` (`PayOrder` cho phép cả `employee`) |
| **6.2** | Đổi điểm Loyalty | Mgr (P), Cashier (P), Admin (S), Cust (S) | Admin, Manager, Cashier, Employee | `GAP` | `OrderController.cs` (`RedeemPoints` cho phép cả `employee`) |
| **6.3** | Cấu hình thanh toán | Admin (P) | Admin, Manager (Branch-scoped) | `GAP` | `ReceiptSettingsController.cs`, `SystemSettingsController.cs` |
| **6.4** | In / Xuất hóa đơn | Admin (P), Mgr (P), Cashier (P), Emp (P) | Admin, Manager, Cashier, Employee | `MATCH` | `InvoiceController.cs`, `ReceiptSettingsController.cs` |
| **7.1** | Xử lý đợt chế biến KDS thời gian thực | Kitchen (P) | Kitchen, Manager, Admin | `MATCH` | `KitchenHub.cs`, `KitchenService.cs` |
| **7.2** | Cập nhật trạng thái tạm hết món | Admin (P), Mgr (P), Kitchen (P) | Admin, Kitchen | `GAP` | `ProductController.cs` (`[Authorize(Roles="admin,kitchen")]`) |
| **8.1** | Chấm công QR | Admin (P), Mgr (P), Cashier (P), Emp (P), Kitchen (P) | Admin, Manager, Cashier, Employee, Kitchen | `MATCH` | `AttendanceController.cs` (`CheckIn`, `CheckOut`) |
| **8.2** | Mở & Chốt ca làm việc | Cashier (P) | Admin, Manager, Cashier | `GAP` | `ShiftController.cs` (`[Authorize(Roles="admin,manager,cashier")]`) |
| **8.3** | Xem lịch làm việc | Admin (P), Mgr (P), Cashier (P), Emp (P), Kitchen (P) | Admin, Manager, Cashier, Employee, Kitchen | `MATCH` | `WorkScheduleController.cs` (`GetSchedules`) |
| **8.4** | Phân lịch làm việc | Admin (P), Mgr (P) | Admin, Manager | `MATCH` | `WorkScheduleController.cs` (`CreateSchedule`, `DeleteSchedule`) |
| **9.1** | Quản lý Sơ đồ bàn & Khu vực | Admin (P), Mgr (P) | Admin, Manager | `MATCH` | `TableController.cs`, `AreaController.cs` |
| **9.2** | Quản lý Hồ sơ & Phân lịch Nhân sự | Admin (P), Mgr (P) | Admin, Manager | `MATCH` | `EmployeeController.cs`, `WorkScheduleController.cs` |
| **9.3** | Quản lý Chi phí vận hành | Admin (P), Mgr (P) | Admin, Manager, Cashier, Employee | `GAP` | `ExpenseController.cs` (`[Authorize(Roles="admin,manager,employee,cashier")]`) |
| **10.1**| Quản lý Danh mục Thực đơn & Topping | Admin (P) | Admin | `MATCH` | `ProductController.cs`, `ToppingController.cs` (Admin-only) |
| **10.2**| Quản lý Chi nhánh & Ưu đãi toàn chuỗi | Admin (P) | Admin | `MATCH` | `BranchController.cs`, `PromotionController.cs` (Admin-only) |
| **10.3**| Cấu hình Thiết lập Hệ thống (VAT/Fee)| Admin (P) | Admin, Manager (Branch-scoped) | `GAP` | `SystemSettingsController.cs` (`Put`) |
| **11.1**| Xem Dashboard | Admin (P), Mgr (P) | Admin, Manager | `MATCH` | `DashboardController.cs` (`GetSummary`) |
| **11.2**| Xem Phân tích & Cảnh báo Insights AI | Admin (P), Mgr (P) | Admin, Manager | `MATCH` | `BusinessInsightController.cs` (`Get`, `GetById`) |
| **12.1**| Tra cứu thông tin qua AI – Read Tools | Admin (P), Mgr (P), Cashier (P), Emp (P), Kitchen (P), Cust (P), Guest (P) | Admin, Manager, Cashier, Employee, Kitchen, Customer, Guest | `MATCH` | `AiController.cs`, `AiPermissionService.cs` |
| **12.2**| Thực hiện tác vụ nghiệp vụ qua AI – Write Tools | Admin (P), Mgr (P), Cashier (P), Emp (P), Kitchen (P), Cust (P) | Admin, Manager, Cashier, Employee, Kitchen, Customer | `MATCH` | `AiController.cs`, `AiPermissionService.cs` |

---

## PART C — FULL USE CASE AUDIT (CHI TIẾT 12 HIGH-LEVEL USE CASES)

### HLUC1: Tra cứu Thực đơn & Sơ đồ Bàn
- **UC 1.1: Quét QR Bàn & Chi nhánh**
  - **Business Goal**: Nhận diện chi nhánh và số bàn phục vụ để bắt đầu trải nghiệm gọi món/đặt bàn.
  - **Expected Actors**: Customer (P), Guest (P).
  - **Actual Implementation**: Frontend `QRScan.tsx` quét QR chứa payload `{ type, branchId, tableId, tableName }`. Backend `TableController.GetTables` chấp nhận anonymous request với `branchId`.
  - **Status**: `MATCH`.

- **UC 1.2: Xem thực đơn & Chi tiết món**
  - **Business Goal**: Xem danh sách món ăn, thông tin giá, kích thước (Size) và danh sách Topping kèm theo.
  - **Expected Actors**: Admin (P), Manager (P), Cashier (P), Employee (P), Customer (P), Guest (P).
  - **Actual Implementation**: `ProductController.GetProducts` và `ToppingController.GetToppings` mở public `[AllowAnonymous]`. Tất cả các Actor đều xem được thực đơn trên UI Admin-Web hoặc Customer-Web.
  - **Status**: `MATCH`.

---

### HLUC2: Đặt món qua QR / Web Order
- **UC 2.1: Gửi đơn đặt món QR / Web Order**
  - **Business Goal**: Khách hàng chọn món, chọn Size/Topping và tạo đơn đặt món trực tuyến từ thiết bị di động.
  - **Expected Actors**: Customer (P), Guest (P), Cashier (S), Employee (S).
  - **Actual Implementation**: `OrderController.CreateOrder` cho phép tạo đơn vãng lai (`CustomerId = null`) hoặc gắn `CustomerId` nếu đã đăng nhập. Nhân viên POS (Cashier/Employee) tiếp nhận đơn trên màn hình POS.
  - **Status**: `MATCH`.

- **UC 2.2: Xem lịch sử đơn hàng**
  - **Business Goal**: Tra cứu các đơn hàng/hóa đơn đã và đang xử lý trong hệ thống.
  - **Expected Actors**: Admin (P), Manager (P), Cashier (P), Employee (P), Kitchen (P).
  - **Actual Implementation**: 
    - Staff/Admin (Admin, Manager, Cashier, Employee) xem lịch sử hóa đơn tại `GET /api/Order` hoặc `/invoices`.
    - Customer xem lịch sử đơn của chính mình qua `GET /api/Order` (lọc theo SĐT từ JWT Name claim) hoặc `DigitalMenu.tsx`.
    - Kitchen tra cứu lịch sử đợt chế biến KDS qua `GET /api/Order/kitchen/history` (`KitchenService.GetHistoryAsync`).
  - **Status**: `NEEDS REVIEW` (Xem chi tiết tại Part D).

---

### HLUC3: Đặt bàn trực tuyến
- **UC 3.1: Tạo yêu cầu đặt bàn trực tuyến**
  - **Business Goal**: Đặt trước lịch hẹn giữ bàn tại nhà hàng.
  - **Expected Actors**: Customer (P), Guest (P).
  - **Actual Implementation**: `ReservationController.CreateReservation` cho phép Guest tạo đơn với `Status = "Pending"` hoặc Customer tự động điền SĐT chính chủ.
  - **Status**: `MATCH`.

- **UC 3.2: Tiếp nhận & Quản lý lịch hẹn đặt bàn**
  - **Business Goal**: Nhân viên/Quản lý duyệt, xếp bàn hoặc hủy lịch hẹn đặt bàn.
  - **Expected Actors**: Manager (P), Cashier (P), Employee (P), Admin (S).
  - **Actual Implementation**: `ReservationController.UpdateStatus` có attribute `[Authorize(Roles = "admin,manager,employee,cashier")]`.
  - **Status**: `MATCH`.

---

### HLUC4: Quản lý Hồ sơ & Loyalty
- **UC 4.1: Xem & Cập nhật Hồ sơ cá nhân**
  - **Business Goal**: Khách hàng cá nhân hóa thông tin tài khoản (Họ tên, email, địa chỉ, mật khẩu).
  - **Expected Actors**: Customer (P).
  - **Actual Implementation**: `CustomerController.UpdateProfile` bắt buộc `[Authorize(Roles = "admin,manager,customer")]` và kiểm tra chính chủ `userId == existing.Id`.
  - **Status**: `MATCH`.

- **UC 4.2: Tra cứu điểm & Hạng thành viên Loyalty**
  - **Business Goal**: Xem tổng số điểm tích lũy, lịch sử cộng/trừ điểm và hạng thành viên (`Khách lẻ`, `Khách quen`, `Khách VIP`).
  - **Expected Actors**: Customer (P), Admin (S), Manager (S), Cashier (S).
  - **Actual Implementation**: `CustomerController.GetLoyaltyHistory` cho phép Customer xem điểm của mình và Staff tra cứu điểm của khách hàng tại quầy.
  - **Status**: `MATCH`.

---

### HLUC5: Quản lý Bán hàng POS & Phục vụ
- **UC 5.1: Tạo đơn hàng POS**
  - **Business Goal**: Lập đơn hàng mới tại quầy hoặc tại bàn phục vụ.
  - **Expected Actors**: Admin (P), Manager (P), Cashier (P), Employee (P).
  - **Actual Implementation**: `OrderController.CreateOrder` xử lý tạo đơn POS mới, tính toán lại giá tự động qua `OrderService.RecalculateOrderFinancials`.
  - **Status**: `MATCH`.

- **UC 5.2: Cập nhật đơn hàng**
  - **Business Goal**: Thay đổi số lượng món, thêm món mới hoặc hủy món chưa chế biến trên đơn hàng đang phục vụ.
  - **Expected Actors**: Admin (P), Manager (P).
  - **Actual Implementation**: `OrderController.CreateOrder` tự động merge và cập nhật đơn hàng nếu `order.Id` đã tồn tại. Backend cho phép bất kỳ người dùng Staff nào có quyền POS (`admin, manager, cashier, employee`) sửa order đang mở.
  - **Status**: `GAP` (Expected chỉ cho Admin & Manager; Actual Backend cho phép cả Cashier và Employee cập nhật đơn POS).

- **UC 5.3: Tiếp nhận đơn đặt món Web Order**
  - **Business Goal**: Nhân viên xác nhận đơn đặt món trực tuyến từ điện thoại khách và gắn bàn phục vụ.
  - **Expected Actors**: Manager (P), Cashier (P), Employee (P), Admin (S).
  - **Actual Implementation**: `OrderController.AcceptWebOrder` (`AcceptWebOrderAsync`) có `[Authorize(Roles = "admin,manager,employee,cashier")]`.
  - **Status**: `MATCH`.

- **UC 5.4: Gửi yêu cầu chế biến xuống Bếp**
  - **Business Goal**: Chuyển các món ăn mới gọi xuống màn hình KDS nhà bếp để đầu bếp bắt đầu chế biến.
  - **Expected Actors**: Manager (P), Cashier (P), Employee (P), Admin (S), Kitchen (S).
  - **Actual Implementation**: `OrderController.SendToKitchen` (`SendToKitchenAsync`) tính chênh lệch `SentQuantity` và phát tín hiệu SignalR tới nhóm Kitchen.
  - **Status**: `MATCH`.

---

### HLUC6: Thanh toán & Hóa đơn
- **UC 6.1: Thanh toán đơn hàng**
  - **Business Goal**: Thu tiền từ khách hàng (Tiền mặt / Chuyển khoản QR) và đóng đơn hàng.
  - **Expected Actors**: Manager (P), Cashier (P), Admin (S), Customer (S).
  - **Actual Implementation**: `OrderController.PayOrder` (`PayOrderAsync`) xác thực số tiền phải đúng 100% `TotalAmount`, cập nhật `PaidAmount`, `PaymentMethod`, `PaymentAt` và trạng thái `"Hoàn thành"`. Attribute backend là `[Authorize(Roles = "admin,manager,employee,cashier")]`.
  - **Status**: `GAP` (Expected giới hạn Primary ở Manager & Cashier; Actual Backend mở quyền cho cả `employee`).

- **UC 6.2: Đổi điểm Loyalty**
  - **Business Goal**: Quy đổi điểm tích lũy của khách hàng thành tiền giảm giá trừ vào đơn hàng.
  - **Expected Actors**: Manager (P), Cashier (P), Admin (S), Customer (S).
  - **Actual Implementation**: `OrderController.RedeemPoints` (`LoyaltyService.RedeemPointsAsync`) trừ điểm tài khoản khách và cộng tiền giảm giá vào `order.Discount`. Attribute backend là `[Authorize(Roles = "admin,manager,employee,cashier")]`.
  - **Status**: `GAP` (Expected giới hạn Primary ở Manager & Cashier; Actual Backend mở quyền cho cả `employee`).

- **UC 6.3: Cấu hình thanh toán**
  - **Business Goal**: Thiết lập các tham số tài chính (Tỷ lệ VAT, Phí dịch vụ, Bật/Tắt thuế) và thông số mẫu in.
  - **Expected Actors**: Admin (P).
  - **Actual Implementation**: `SystemSettingsController.Put` và `ReceiptSettingsController.Put` cho phép `admin` (Global) VÀ `manager` (chi nhánh của mình) thay đổi cài đặt.
  - **Status**: `GAP` (Expected giới hạn ở Admin; Actual Backend mở quyền cho cả `manager` tùy chỉnh thiết lập chi nhánh).

- **UC 6.4: In / Xuất hóa đơn**
  - **Business Goal**: Xuất hoặc in hóa đơn giấy sau khi giao dịch hoàn tất.
  - **Expected Actors**: Admin (P), Manager (P), Cashier (P), Employee (P).
  - **Actual Implementation**: `InvoiceController.cs` và `ReceiptSettingsController.cs` cho phép tất cả Staff truy cập và in hóa đơn.
  - **Status**: `MATCH`.

---

### HLUC7: Điều phối Chế biến (KDS)
- **UC 7.1: Xử lý đợt chế biến KDS thời gian thực**
  - **Business Goal**: Tiếp nhận danh sách món cần nấu và cập nhật tiến độ (`Chờ xử lý` $\rightarrow$ `Đang nấu` $\rightarrow$ `Hoàn tất`).
  - **Expected Actors**: Kitchen (P).
  - **Actual Implementation**: `KitchenHub.cs` và `OrderController.UpdateRequestStatus` phục vụ trực tiếp cho Kitchen.
  - **Status**: `MATCH`.

- **UC 7.2: Cập nhật trạng thái tạm hết món**
  - **Business Goal**: Đánh dấu món ăn tạm thời hết hàng (`OutOfStock`) để dừng nhận đơn trên POS và Web.
  - **Expected Actors**: Admin (P), Manager (P), Kitchen (P).
  - **Actual Implementation**: `ProductController.UpdateAvailability` có attribute `[Authorize(Roles = "admin,kitchen")]`.
  - **Status**: `GAP` (Actual backend chặn Role `manager` trực tiếp gọi endpoint này trừ khi Manager đăng nhập ở chế độ `kitchen`).

---

### HLUC8: Quản lý Ca làm việc & Chấm công
- **UC 8.1: Chấm công QR**
  - **Business Goal**: Thực hiện quét QR tại chi nhánh để ghi nhận giờ Check-in / Check-out và tình trạng đi muộn.
  - **Expected Actors**: Admin (P), Manager (P), Cashier (P), Employee (P), Kitchen (P).
  - **Actual Implementation**: `AttendanceController.CheckIn` và `CheckOut` mở cho toàn bộ 5 Staff/Admin roles.
  - **Status**: `MATCH`.

- **UC 8.2: Mở & Chốt ca làm việc**
  - **Business Goal**: Khai báo số tiền mặt đầu ca và chốt doanh thu tiền mặt/chuyển khoản khi kết thúc ca.
  - **Expected Actors**: Cashier (P).
  - **Actual Implementation**: `ShiftController.cs` có attribute `[Authorize(Roles = "admin,manager,cashier")]`.
  - **Status**: `GAP` (Actual Backend cho phép cả Manager và Admin mở/chốt ca thay vì chỉ Cashier).

- **UC 8.3: Xem lịch làm việc**
  - **Business Goal**: Xem lịch phân ca cá nhân trong tuần/tháng.
  - **Expected Actors**: Admin (P), Manager (P), Cashier (P), Employee (P), Kitchen (P).
  - **Actual Implementation**: `WorkScheduleController.GetSchedules` hỗ trợ tất cả các roles.
  - **Status**: `MATCH`.

- **UC 8.4: Phân lịch làm việc**
  - **Business Goal**: Sắp xếp và gán ca làm việc cho nhân sự trong chi nhánh.
  - **Expected Actors**: Admin (P), Manager (P).
  - **Actual Implementation**: `WorkScheduleController.CreateSchedule` và `DeleteSchedule` kiểm tra `IsAdmin() || IsManager()`.
  - **Status**: `MATCH`.

---

### HLUC9: Quản lý Vận hành Chi nhánh
- **UC 9.1: Quản lý Sơ đồ bàn & Khu vực**
  - **Business Goal**: Thêm, sửa, xóa bàn phục vụ và các khu vực trong nhà hàng.
  - **Expected Actors**: Admin (P), Manager (P).
  - **Actual Implementation**: `TableController.cs` và `AreaController.cs` (Create/Update/Delete) chỉ cho phép `admin` và `manager`.
  - **Status**: `MATCH`.

- **UC 9.2: Quản lý Hồ sơ & Phân lịch Nhân sự**
  - **Business Goal**: Quản lý danh sách nhân viên chi nhánh, tạo tài khoản và phân công nhiệm vụ.
  - **Expected Actors**: Admin (P), Manager (P).
  - **Actual Implementation**: `EmployeeController.cs` (Create/Update/Delete) kiểm tra `IsAdmin() || IsManager()`.
  - **Status**: `MATCH`.

- **UC 9.3: Quản lý Chi phí vận hành**
  - **Business Goal**: Ghi nhận và theo dõi các khoản chi phí phát sinh (Điện nước, bao bì, gas, vệ sinh).
  - **Expected Actors**: Admin (P), Manager (P).
  - **Actual Implementation**: `ExpenseController.cs` có attribute `[Authorize(Roles = "admin,manager,employee,cashier")]`.
  - **Status**: `GAP` (Actual Backend mở quyền ghi chi phí cho cả `cashier` và `employee`).

---

### HLUC10: Quản trị Hệ thống & Danh mục
- **UC 10.1: Quản lý Danh mục Thực đơn & Topping**
  - **Business Goal**: Thêm, sửa, xóa món ăn, nhóm món, giá bán và topping dùng chung toàn hệ thống.
  - **Expected Actors**: Admin (P).
  - **Actual Implementation**: `ProductController.cs` và `ToppingController.cs` (Create/Update/Delete) đều có `[Authorize(Roles = "admin")]`.
  - **Status**: `MATCH`.

- **UC 10.2: Quản lý Chi nhánh & Ưu đãi toàn chuỗi**
  - **Business Goal**: Quản lý mạng lưới chi nhánh và các chương trình khuyến mãi toàn chuỗi.
  - **Expected Actors**: Admin (P).
  - **Actual Implementation**: `BranchController.cs` và `PromotionController.cs` đều bắt buộc `[Authorize(Roles = "admin")]`.
  - **Status**: `MATCH`.

- **UC 10.3: Cấu hình Thiết lập Hệ thống (VAT/Fee)**
  - **Business Goal**: Cấu hình tỷ lệ VAT, Phí dịch vụ và các tham số Loyalty hệ thống.
  - **Expected Actors**: Admin (P).
  - **Actual Implementation**: `SystemSettingsController.Put` cho phép `admin` (Global) và `manager` (chi nhánh của mình).
  - **Status**: `GAP` (Actual Backend hỗ trợ Manager cấu hình setting chi nhánh).

---

### HLUC11: Báo cáo Doanh thu & Insights
- **UC 11.1: Xem Dashboard**
  - **Business Goal**: Theo dõi biểu đồ doanh thu, top món bán chạy, cơ cấu bán hàng và số lượng nhân sự.
  - **Expected Actors**: Admin (P), Manager (P).
  - **Actual Implementation**: `DashboardController.GetSummary` bắt buộc `[Authorize(Roles = "admin,manager")]`.
  - **Status**: `MATCH`.

- **UC 11.2: Xem Phân tích & Cảnh báo Insights AI**
  - **Business Goal**: Xem danh sách phân tích tài chính tự động và các cảnh báo bất thường từ AI.
  - **Expected Actors**: Admin (P), Manager (P).
  - **Actual Implementation**: `BusinessInsightController.cs` bắt buộc `[Authorize(Roles = "admin,manager")]`.
  - **Status**: `MATCH`.

---

### HLUC12: Tương tác Trợ lý ảo AI Agent
- **UC 12.1: Tra cứu thông tin qua AI – Read Tools**
  - **Business Goal**: Trò chuyện bằng ngôn ngữ tự nhiên để tra cứu doanh thu, thực đơn, ca làm, danh sách đơn hàng.
  - **Expected Actors**: Admin (P), Manager (P), Cashier (P), Employee (P), Kitchen (P), Customer (P), Guest (P).
  - **Actual Implementation**: `AiController.cs` xử lý tin nhắn đàm thoại. Guest được sử dụng `get_menu`. Customer được dùng `get_my_order`. Staff được sử dụng các Read tools thuộc phạm vi vai trò.
  - **Status**: `MATCH`.

- **UC 12.2: Thực hiện tác vụ nghiệp vụ qua AI – Write Tools**
  - **Business Goal**: Thực hiện các thao tác đổi trạng thái đơn, đặt bàn hoặc cập nhật giá qua AI Chatbot.
  - **Expected Actors**: Admin (P), Manager (P), Cashier (P), Employee (P), Kitchen (P), Customer (P).
  - **Actual Implementation**: `update_product_price` (Admin), `update_order_status` (Staff), `create_booking` (Customer). Guest bị chặn hoàn toàn các Write Tools qua `AiPermissionService.cs`.
  - **Status**: `MATCH`.

---

## PART D — ĐẶC BIỆT: AUDIT "XEM LỊCH SỬ ĐƠN HÀNG"

Bảng phân tích chuyên sâu nghiệp vụ Tra cứu/Xem lịch sử đơn hàng giữa các Actor:

| Actor | Expected | UI Display Name | Actual Capability in Code | Data Source & Service Method | Business & Domain Alignment | Status |
| :--- | :---: | :--- | :--- | :--- | :--- | :---: |
| **Admin** | **P** | Hóa đơn / Lịch sử | Xem toàn bộ hóa đơn toàn chuỗi, lọc theo chi nhánh/ngày | `InvoiceController.Get`, `OrderService.GetOrdersAsync` | Tra cứu doanh thu & quản trị cấp cao. | `MATCH` |
| **Manager** | **P** | Hóa đơn chi nhánh | Xem toàn bộ hóa đơn thuộc chi nhánh phụ trách | `InvoiceController.Get` (lọc theo `branchId` JWT) | Quản lý tài chính & vận hành chi nhánh. | `MATCH` |
| **Cashier** | **P** | Lịch sử hóa đơn | Xem danh sách hóa đơn bán hàng tại quầy | `InvoiceController.Get` hoặc `/pos/invoices` | Đối soát doanh thu quầy & in lại hóa đơn. | `MATCH` |
| **Employee** | **P** | Đơn hàng đã tạo | Xem lịch sử các đơn hàng do nhân viên lập/phục vụ | `OrderController.GetOrders` | Kiểm tra đơn hàng tại bàn & hỗ trợ khách. | `MATCH` |
| **Kitchen** | **P** | Lịch sử bếp (KDS) | Xem danh sách các đợt yêu cầu chế biến (`OrderRequest`) | `KitchenService.GetHistoryAsync` (`GET /api/Order/kitchen/history`) | **Khác biệt về Domain Entity**: Kitchen xem lịch sử đợt nấu KDS chứ không xem hóa đơn tài chính. | `NEEDS REVIEW` |
| **Customer** | **–** (trong Expected 2.2) | Đơn hàng của tôi | Xem danh sách & trạng thái 5 đơn mới nhất của mình | `OrderService.GetCustomerOrdersAsync` (`GET /api/Order` lọc theo SĐT) | Khách hàng theo dõi tiến độ đơn hàng cá nhân. | `NEEDS REVIEW` |

### Kết luận Kiến trúc cho UC "Xem lịch sử đơn hàng":
1. **Đối với Admin, Manager, Cashier, Employee**: Cùng chia sẻ chung một Business Goal là **"Tra cứu lịch sử hóa đơn/đơn hàng"** (truy xuất entity `Order` & `OrderDetail`). Do đó, gom chung thành **1 Use Case thống nhất** là `UC2.2: Xem lịch sử đơn hàng`.
2. **Đối với Kitchen**: Màn hình `/kitchen/history` truy xuất trực tiếp bảng `OrderRequests` và `OrderRequestItems` (chứa các mốc thời gian `AcceptedAt`, `PreparingAt`, `CompletedAt`). Đây là **Lịch sử chế biến nhà bếp**, khác biệt hoàn toàn với hóa đơn tài chính.
   - *Đề xuất*: Giữ Kitchen trong UC7.1 / KDS History thay vì ép vào UC2.2.
3. **Đối với Customer**: Customer thực tế có giao diện "Đơn hàng của tôi" trên `apps/customer-web`. Bảng Expected ban đầu đánh `-` cho Customer ở UC2.2 là chưa bao quát luồng Customer.

---

## PART E — SPECIAL AUDIT: CREATE VS UPDATE ORDER

Bảng đối chiếu nghiệp vụ Tạo đơn vs Cập nhật đơn trên màn hình POS:

| Thao tác Nghiệp vụ | Expected Actors | Actual Code Authorization | Logic triển khai trong Mã nguồn | Status |
| :--- | :--- | :--- | :--- | :---: |
| **5.1: Tạo đơn hàng POS** | Admin (P), Mgr (P), Cashier (P), Emp (P) | Admin, Manager, Cashier, Employee | `POST /api/Order` khởi tạo `Order` mới, gán `Status = "Đang xử lý"`, tự động tính toán tài chính. | `MATCH` |
| **5.2: Cập nhật đơn hàng POS** | Admin (P), Mgr (P) | Admin, Manager, Cashier, Employee | `OrderService.CreateOrUpdateOrderAsync` tự động tìm đơn đang mở của bàn để merge món mới hoặc thay đổi số lượng. | `GAP` |

### Phân tích chi tiết:
- **Lý do GAP**: Trong Expected Business Model, **UC 5.2 (Cập nhật đơn hàng)** được định nghĩa là quyền hạn đặc thù của `Admin` và `Manager` (ví dụ: điều chỉnh giảm món, hủy món đã đặt). Tuy nhiên, trong mã nguồn `OrderService.cs`, khi nhân viên phục vụ (Employee) hoặc Thu ngân (Cashier) mở một bàn đang có khách và nhấn chọn thêm món/tăng số lượng, frontend POS vẫn gọi chung API `POST /api/Order` để cập nhật đơn đó.
- **Đề xuất khắc phục (Recommended Action)**:
  - *Phương án A (Giữ nguyên Expected Model - Khuyên dùng cho Báo cáo UML)*: Định nghĩa **UC 5.2** là "Cập nhật / Điều chỉnh đơn hàng nâng cao" (dành riêng cho Admin/Manager khi cần can thiệp đơn); còn việc gọi thêm món thông thường của Employee/Cashier nằm trong luồng chính của **UC 5.1 (Tạo & Cập nhật đơn POS)**.
  - *Phương án B (Code Fix)*: Thêm kiểm tra Role trong backend nếu muốn chặn Employee tự ý sửa đơn đã gửi bếp.

---

## PART F — ACTOR × USE CASE MATRIX (AFTER AUDIT)

Bảng ma trận kết nối chính thức giữa 7 Actor và 29 Sub Use Cases sau khi audit (Phản ánh đúng Primary/Supporting Actors):

- **`P`** = Primary Actor (Người khởi tạo Use Case)
- **`S`** = Supporting Actor (Người/Hệ thống hỗ trợ/tiếp nhận)
- **`-`** = Không tham gia

| Sub Use Case ID & Name | Admin | Manager | Cashier | Employee | Kitchen | Customer | Guest |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **UC1.1: Quét QR Bàn & Chi nhánh** | - | - | - | - | - | **P** | **P** |
| **UC1.2: Xem thực đơn & Chi tiết món** | **P** | **P** | **P** | **P** | - | **P** | **P** |
| **UC2.1: Gửi đơn đặt món QR / Web Order** | - | - | S | S | - | **P** | **P** |
| **UC2.2: Xem lịch sử đơn hàng** | **P** | **P** | **P** | **P** | **P** | **P** | - |
| **UC3.1: Tạo yêu cầu đặt bàn trực tuyến** | - | - | - | - | - | **P** | **P** |
| **UC3.2: Tiếp nhận & Quản lý lịch hẹn đặt bàn** | S | **P** | **P** | **P** | - | - | - |
| **UC4.1: Xem & Cập nhật Hồ sơ cá nhân** | - | - | - | - | - | **P** | - |
| **UC4.2: Tra cứu điểm & Hạng thành viên Loyalty** | S | S | S | - | - | **P** | - |
| **UC5.1: Tạo đơn hàng POS** | **P** | **P** | **P** | **P** | - | - | - |
| **UC5.2: Cập nhật đơn hàng** | **P** | **P** | **P** | **P** | - | - | - |
| **UC5.3: Tiếp nhận đơn đặt món Web Order** | S | **P** | **P** | **P** | - | - | - |
| **UC5.4: Gửi yêu cầu chế biến xuống Bếp** | S | **P** | **P** | **P** | S | - | - |
| **UC6.1: Thanh toán đơn hàng** | S | **P** | **P** | **P** | - | S | - |
| **UC6.2: Đổi điểm Loyalty** | S | **P** | **P** | **P** | - | S | - |
| **UC6.3: Cấu hình thanh toán** | **P** | **P** | - | - | - | - | - |
| **UC6.4: In / Xuất hóa đơn** | **P** | **P** | **P** | **P** | - | - | - |
| **UC7.1: Xử lý đợt chế biến KDS thời gian thực** | - | - | - | - | **P** | - | - |
| **UC7.2: Cập nhật trạng thái tạm hết món** | **P** | **P** | - | - | **P** | - | - |
| **UC8.1: Chấm công QR** | **P** | **P** | **P** | **P** | **P** | - | - |
| **UC8.2: Mở & Chốt ca làm việc** | **P** | **P** | **P** | - | - | - | - |
| **UC8.3: Xem lịch làm việc** | **P** | **P** | **P** | **P** | **P** | - | - |
| **UC8.4: Phân lịch làm việc** | **P** | **P** | - | - | - | - | - |
| **UC9.1: Quản lý Sơ đồ bàn & Khu vực** | **P** | **P** | - | - | - | - | - |
| **UC9.2: Quản lý Hồ sơ & Phân lịch Nhân sự** | **P** | **P** | - | - | - | - | - |
| **UC9.3: Quản lý Chi phí vận hành** | **P** | **P** | **P** | **P** | - | - | - |
| **UC10.1: Quản lý Danh mục Thực đơn & Topping**| **P** | - | - | - | - | - | - |
| **UC10.2: Quản lý Chi nhánh & Ưu đãi toàn chuỗi**| **P** | - | - | - | - | - | - |
| **UC10.3: Cấu hình Thiết lập Hệ thống (VAT/Fee)**| **P** | **P** | - | - | - | - | - |
| **UC11.1: Xem Dashboard** | **P** | **P** | - | - | - | - | - |
| **UC11.2: Xem Phân tích & Cảnh báo Insights AI** | **P** | **P** | - | - | - | - | - |
| **UC12.1: Tra cứu thông tin qua AI – Read Tools** | **P** | **P** | **P** | **P** | **P** | **P** | **P** |
| **UC12.2: Thực hiện tác vụ nghiệp vụ qua AI – Write Tools**| **P** | **P** | **P** | **P** | **P** | **P** | - |

---

## PART G — AUDIT QUAN HỆ INCLUDE / EXTEND / GENERALIZATION

Bảng đánh giá chuẩn mực UML 2.5 cho các mối quan hệ:

| Quan hệ UML | Chi tiết Thành phần A & B | Audit Result | Lý do Ngữ nghĩa UML 2.5 (Semantic Reason) |
| :--- | :--- | :---: | :--- |
| **Actor Generalization** | `Customer --|> Guest` | `VALID` | `Customer` kế thừa toàn bộ khả năng tương tác công khai của `Guest` (quét QR, xem menu, tạo đơn vãng lai) và mở rộng thêm các Use Case định danh cá nhân. |
| **`<<include>>`** | `UC5.4 (Gửi bếp) --<<include>>--> UC5.1 (Tạo/Sửa đơn POS)` | `VALID` | Mọi đợt gửi món xuống KDS bếp bắt buộc phải dựa trên và cập nhật lại `SentQuantity` của một đơn hàng POS đã tạo. |
| **`<<include>>`** | `UC5.3 (Tiếp nhận Web Order) --<<include>>--> UC5.1 (Tạo/Sửa đơn POS)` | `VALID` | Việc tiếp nhận đơn Web bắt buộc phải thực hiện chuyển đổi đơn hàng chờ thành một đơn POS chính thức gắn bàn/người nhận. |
| **`<<extend>>`** | `UC6.2 (Đổi điểm Loyalty) --<<extend>>--> UC5.1 (Tạo/Sửa đơn POS)` | `VALID` | **Extension Point**: Lập tài chính đơn POS.<br>**Condition**: Chỉ thực hiện khi khách hàng yêu cầu quy đổi điểm tích lũy thành tiền giảm giá. |
| **`<<extend>>`** | `UC6.4 (In hóa đơn) --<<extend>>--> UC6.1 (Thanh toán đơn hàng)` | `VALID` | **Extension Point**: Hoàn tất giao dịch thanh toán.<br>**Condition**: Chỉ thực hiện khi thu ngân hoặc khách hàng có nhu cầu xuất/in hóa đơn giấy. |

---

## PART H — USE CASE DECOMPOSITION TREE (HÌNH 3.1 $\rightarrow$ HÌNH 3.7)

Cấu trúc phân cấp biểu đồ chính thức được xác nhận:

===================================================================================
                  HÌNH 3.1: BIỂU ĐỒ USE CASE TỔNG QUÁT TOÀN HỆ THỐNG
   (Bao gồm 7 Actors & 12 High-Level Business Groups HLUC1 -> HLUC12 theo Domain)
===================================================================================
                                       │
     ┌─────────────────────────────────┼─────────────────────────────────┐
     │                                 │                                 │
     ▼                                 ▼                                 ▼
`Domain Khách hàng`           `Domain POS, KDS & Ca`            `Domain Quản trị & AI`
     │                                 │                                 │
     ├─► HÌNH 3.2                      ├─► HÌNH 3.3                      ├─► HÌNH 3.6
     │   Phân rã Dịch vụ Khách hàng    │   Phân rã Bán hàng POS,         │   Phân rã Quản trị Vận hành,
     │   & Đặt món Trực tuyến          │   Thanh toán & In Hóa đơn       │   Hệ thống & Business Insights
     │   (HLUC1, HLUC2, HLUC3, HLUC4)  │   (HLUC5, HLUC6)                │   (HLUC9, HLUC10, HLUC11)
     │                                 │                                 │
     └─► (Chứa UC1.1-1.2, UC2.1-2.2,   ├─► HÌNH 3.4                      └─► HÌNH 3.7
          UC3.1-3.2, UC4.1-4.2)        │   Phân rã Điều phối Chế biến    │   Phân rã Tương tác Trợ lý ảo
                                       │   Nhà bếp KDS (HLUC7)           │   AI Agent (HLUC12)
                                       │                                 │
                                       └─► HÌNH 3.5                      └─► (Chứa UC12.1-12.2)
                                           Phân rã Ca làm việc &
                                           Chấm công Nhân sự (HLUC8)

---

## PART I — PHÂN BIỆT BUSINESS USE CASE VÀ TECHNICAL OPERATIONS

Để đảm bảo biểu đồ UML sạch sẽ và chuẩn mực, các yếu tố kỹ thuật sau đây **ĐÃ ĐƯỢC LOẠI BỎ KHỎI SƠ ĐỒ** và chuyển về mô tả trong Flow of Events / Technical Architecture:

1. **Calculate VAT / Service Fee / Size Price**: Logic tính toán tự động trong `OrderService.RecalculateOrderFinancials`.
2. **Validate Payment Amount**: Quy tắc xác thực tham số số tiền trong `PayOrderAsync`.
3. **Earn Loyalty Points Automatically**: Tác động phụ tự động phía server (`EarnPointsAsync`) sau khi thanh toán.
4. **Check Branch Permission / Validate JWT**: Cơ chế kiểm soát truy cập kỹ thuật của Middleware & Policy.
5. **SignalR Event Broadcasting**: Giao thức truyền tin thời gian thực giữa Server và KDS Web Client.
6. **Individual 14 AI Tools**: Cơ chế thực thi chức năng nội bộ do AI Orchestrator gọi qua Gemini.

---

## PART J — CHỨC NĂNG BỊ XÓA (REMOVED / UNIMPLEMENTED)

- **Payroll (Quản lý & Tính bảng lương)**: Đã bị xóa hoàn toàn khỏi schema và mã nguồn qua Migration `20260919075618_RemovePayrollAndRepairReceiptSettings.cs`. File `PayrollController.cs` và `PayrollPage.tsx` chỉ còn comment obsolete.
- **Kết luận**: **ĐÃ LOẠI BỎ 100% KHỎI MÔ HÌNH UML**.

---

## PART K — PHÂN LOẠI KẾT LƯẬN VÀ HƯỚNG XỬ LÝ (FINAL CONCLUSION)

### 1. MATCH (Các nghiệp vụ đã khớp 100% giữa Expected và Code)
- `HLUC1`: Quét QR bàn (`1.1`), Xem thực đơn (`1.2`).
- `HLUC2`: Đặt món QR/Web Order (`2.1`).
- `HLUC3`: Đặt bàn trực tuyến (`3.1`), Tiếp nhận lịch hẹn (`3.2`).
- `HLUC4`: Quản lý Profile (`4.1`), Tra cứu Loyalty (`4.2`).
- `HLUC5`: Tạo đơn POS (`5.1`), Tiếp nhận đơn Web (`5.3`), Gửi bếp (`5.4`).
- `HLUC6`: In hóa đơn (`6.4`).
- `HLUC7`: Xử lý KDS (`7.1`).
- `HLUC8`: Chấm công QR (`8.1`), Xem lịch làm (`8.3`), Phân lịch làm (`8.4`).
- `HLUC9`: Quản lý sơ đồ bàn (`9.1`), Quản lý nhân sự (`9.2`).
- `HLUC10`: Quản lý Thực đơn/Topping toàn chuỗi (`10.1`), Quản lý Chi nhánh/Ưu đãi (`10.2`).
- `HLUC11`: Xem Dashboard (`11.1`), Xem Business Insights (`11.2`).
- `HLUC12`: AI Read Tools (`12.1`), AI Write Tools (`12.2`).

---

### 2. GAP – NEED CODE FIX (Cần điều chỉnh Backend Authorization nếu muốn siết chặt theo Expected Model)
1. **Cập nhật đơn hàng POS (`UC 5.2`)**:
   - *Lý do*: Backend `OrderController.CreateOrder` cho phép cả `employee` và `cashier` cập nhật đơn hàng đang mở.
   - *Khuyến nghị*: Nếu muốn siết chặt quyền chỉ cho Admin/Manager điều chỉnh đơn đã gửi bếp, cần bổ sung role check trong `OrderService.cs`.
2. **Thanh toán (`UC 6.1`) & Đổi điểm Loyalty (`UC 6.2`)**:
   - *Lý do*: Backend `OrderController.cs` cho phép Role `employee` gọi API `PayOrder` và `RedeemPoints`.
   - *Khuyến nghị*: Thêm attribute `[Authorize(Roles = "admin,manager,cashier")]` trên endpoint `PayOrder` và `RedeemPoints` để chặn `employee` nếu quy trình nhà hàng chỉ cho phép Thu ngân thu tiền.
3. **Cấu hình thanh toán (`UC 6.3`) & Cấu hình VAT/Fee (`UC 10.3`)**:
   - *Lý do*: Backend `SystemSettingsController.cs` và `ReceiptSettingsController.cs` cho phép `manager` cập nhật setting chi nhánh.
   - *Khuyến nghị*: Giữ nguyên cho Manager tùy chỉnh chi nhánh của mình, hoặc giới hạn lại cho Admin tùy theo quyết định nghiệp vụ.
4. **Cập nhật báo hết món (`UC 7.2`)**:
   - *Lý do*: Backend `ProductController.UpdateAvailability` chỉ có `[Authorize(Roles = "admin,kitchen")]`. Manager nếu đăng nhập ở mode `admin` thì không bị chặn, nhưng nếu mang token role `manager` thì sẽ bị 403.
   - *Khuyến nghị*: Bổ sung `manager` vào `[Authorize(Roles = "admin,manager,kitchen")]`.
5. **Mở & Chốt ca (`UC 8.2`)**:
   - *Lý do*: Backend `ShiftController.cs` cấp quyền cho cả `admin, manager, cashier`.
   - *Khuyến nghị*: Phù hợp thực tế vì Quản lý vẫn có thể mở/chốt ca thay thu ngân khi cần.
6. **Quản lý Chi phí vận hành (`UC 9.3`)**:
   - *Lý do*: Backend `ExpenseController.cs` cấp quyền cho cả `employee` tạo phiếu chi.
   - *Khuyến nghị*: Bỏ role `employee` khỏi `ExpenseController` nếu chỉ cho phép Manager/Cashier ghi nhận chi phí.

---

### 3. NEEDS BUSINESS REVIEW (Điểm cần xác nhận lại thiết kế nghiệp vụ)
1. **UC 2.2 — Xem lịch sử đơn hàng**:
   - *Hiện trạng*: Staff xem lịch sử hóa đơn tại `/invoices`; Customer xem đơn hàng cá nhân tại `customer-web`; Kitchen xem lịch sử đợt nấu KDS tại `/kitchen/history`.
   - *Quyết định*: Giữ chung 1 Use Case `UC2.2: Xem lịch sử đơn hàng` cho Staff/Customer, và tách rõ Kitchen thuộc KDS History (`UC7.1`).

---

### 4. RECOMMENDED NEXT STEP (BƯỚC TIẾP THEO)

1. **Bước 1**: Xác nhận báo cáo Audit Expected vs Actual này.
2. **Bước 2**: Quyết định xem có cần thực hiện sửa code (`Code Fix` cho 6 điểm GAP ở mục 2) hay giữ nguyên quyền rộng như hiện tại và cập nhật Báo cáo Use Case.
3. **Bước 3**: Tiến hành xuất mã **Draw.io XML** cho từng hình biểu đồ (từ **Hình 3.1** đến **Hình 3.7**) để hoàn thiện Hồ sơ Phân tích Thiết kế UML 2.5 cho Đồ án!
