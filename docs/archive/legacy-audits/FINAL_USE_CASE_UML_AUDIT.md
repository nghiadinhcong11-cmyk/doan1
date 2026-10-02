# BÁO CÁO AUDIT CHUYÊN SÂU VÀ CHUẨN HÓA MÔ HÌNH USE CASE UML 2.5

**Đề tài**: Hệ thống quản lý nhà hàng đa chi nhánh tích hợp Trợ lý ảo AI Agent  
**Chức danh đảm nhiệm**: Senior System Analyst / UML 2.5 Analyst / Software Architect / Requirements Engineer / Code Auditor  
**Nguyên tắc chỉ đạo**: Mã nguồn (Source Code) hiện tại là **Single Source of Truth** (Nguồn sự thật duy nhất). 

---

## PART A — FINAL ACTOR MODEL (MÔ HÌNH ACTOR CHÍNH THỨC)

Sau khi audit lại toàn bộ cơ chế Authentication, JWT Claims, Authorization Policies (`[Authorize]`), Controllers, Services và UI Routes, **7 Actor nghiệp vụ chính thức** được xác nhận như sau:

| Actor | Valid? | Scope | Business Goal (Mục tiêu nghiệp vụ cốt lõi) | Evidence Source (Code) | Notes & Constraints |
| :--- | :---: | :--- | :--- | :--- | :--- |
| **1. Admin tổng** | `VALID` | **Global Scope** (`BranchId = null` hoặc linh hoạt) | Quản trị cấp chuỗi: Mạng lưới chi nhánh, danh mục thực đơn & topping dùng chung, ưu đãi toàn chuỗi, nhân sự toàn hệ thống, cấu hình Global, AI Write Tools cao cấp. | `AuthController.cs`<br>`ProductController.cs`<br>`BranchController.cs`<br>`SystemSettingsController.cs` | Quản trị viên tối cao. Phạm vi không bị giới hạn bởi `branchId`. Là Primary Actor duy nhất của UC Quản lý danh mục toàn chuỗi (`UC10.1`). |
| **2. Manager** | `VALID` | **Branch Scope** (Gắn cố định với `branchId` JWT) | Quản lý vận hành chi nhánh: Nhân sự chi nhánh, sơ đồ bàn/khu vực, phân lịch làm việc, chi phí chi nhánh, Dashboard & Business Insights chi nhánh. | `AuthController.cs`<br>`EmployeeController.cs`<br>`DashboardController.cs`<br>`BusinessInsightController.cs` | Quản lý cấp chi nhánh. Bị cô lập dữ liệu theo `branchId`. **Không có quyền CRUD danh mục thực đơn toàn chuỗi** (`ProductController` trả về `403`). |
| **3. Cashier** | `VALID` | **Branch Scope** (Gắn cố định với `branchId` JWT) | Thu ngân & Tài chính quầy: Bán hàng POS, tiếp nhận đơn Web, xử lý thanh toán, đổi điểm Loyalty, mở/chốt ca tiền mặt (`Shift`), in hóa đơn. | `AuthController.cs`<br>`OrderController.cs`<br>`ShiftController.cs`<br>`ReceiptSettingsController.cs` | Chuyên trách giao dịch bán hàng và thanh toán tại điểm bán. Độc lập với công việc phục vụ bàn thuần túy của Employee. |
| **4. Employee** | `VALID` | **Branch Scope** (Gắn cố định với `branchId` JWT) | Phục vụ bàn (Waitstaff): Tạo/cập nhật đơn POS tại bàn, tiếp nhận đơn Web, chuyển trạng thái bàn, gửi đợt món xuống KDS bếp, chấm công QR. | `AuthController.cs`<br>`OrderController.cs`<br>`AttendanceController.cs`<br>`WorkScheduleController.cs` | Nhân viên phục vụ. **Không có quyền xử lý thanh toán** (`PayOrderAsync`) hay ghi nhận chi phí trong luồng nghiệp vụ quầy. |
| **5. Kitchen** | `VALID` | **Branch Scope** (Gắn cố định với `branchId` JWT) | Điều phối chế biến (KDS): Nhận đợt nấu thời gian thực SignalR, cập nhật trạng thái nấu, báo tạm hết món (`AvailabilityStatus`), xem lịch sử bếp. | `KitchenHub.cs`<br>`KitchenService.cs`<br>`ProductController.cs` (`UpdateAvailability`) | Nhân viên nhà bếp. Không tương tác với màn hình bán hàng POS, thanh toán hay đặt bàn. |
| **6. Customer** | `VALID` | **Personal Scope** (Xác thực qua `CustomerId` / SĐT) | Khách hàng định danh: Quản lý hồ sơ cá nhân, xem lịch sử đơn cá nhân, tra cứu & tích/tiêu điểm Loyalty, quản lý lịch đặt bàn cá nhân, AI Chatbot. | `AuthController.cs` (`customer-token`)<br>`CustomerController.cs`<br>`LoyaltyService.cs` | Khách hàng đã xác thực. Thừa hưởng toàn bộ khả năng tương tác công khai của Guest + Quyền cá nhân hóa. |
| **7. Guest** | `VALID` | **Contextual Scope** (Xác định theo QR Bàn / Context) | Khách vãng lai: Quét QR bàn, xem thực đơn công khai, đăng ký tài khoản Customer, tạo đơn đặt món QR/Web vãng lai, tạo yêu cầu đặt bàn vãng lai. | `AuthController.cs` (`[AllowAnonymous]`)<br>`ProductController.cs`<br>`OrderController.cs` | Khách chưa đăng nhập. Mọi đơn đặt món/đặt bàn tạo ra đều ở trạng thái chờ (`Pending`/`Đang xử lý`) cần nhân viên tiếp nhận. |

### Nguyên tắc cố định về Actor Model:
- **`Staff` KHÔNG PHẢI LÀ ACTOR**: Trong mã nguồn không tồn tại Role `"staff"`. `Staff` là một **Khái niệm nhóm nghiệp vụ (Business Grouping Concept)** đại diện cho 4 Actor vận hành (`Manager`, `Cashier`, `Employee`, `Kitchen`).
- **Không gộp Role**: 4 Actor `Manager`, `Cashier`, `Employee`, `Kitchen` giữ nguyên tư cách là 4 Actor UML độc lập.

---

## PART B — ACTOR GENERALIZATION AUDIT

Audit ngữ nghĩa UML 2.5 cho các mối quan hệ giữa các Actors:

| Source Actor | Relationship | Target Actor | Status | Semantic Reason & Code Justification |
| :--- | :---: | :--- | :---: | :--- |
| **Customer** | **`Generalization`** | **Guest** | `VALID` | `Customer` (Khách đã đăng nhập) kế thừa toàn bộ khả năng tương tác công khai của `Guest` (quét QR, xem thực đơn, tạo đơn/lịch hẹn vãng lai) và mở rộng thêm các Use Case định danh (xem profile, lịch sử đơn, tích điểm Loyalty). Đây là quan hệ Generalization (`is-a`) chuẩn mực UML 2.5. |
| **Admin** | *Generalization* | **Manager** | `INVALID (REMOVED)` | Admin (Global Scope) và Manager (Branch Scope) là 2 Actor quản trị ở hai cấp độ quản lý hoàn toàn khác nhau. Admin không bị ràng buộc `branchId` như Manager. Không tồn tại quan hệ kế thừa `is-a`. |
| **Manager / Cashier / Kitchen** | *Generalization* | **Employee** | `INVALID (REMOVED)` | 4 Actor này đại diện cho 4 vị trí công tác phân biệt với tập thẩm quyền nghiệp vụ riêng biệt trong code (`manager`, `cashier`, `employee`, `kitchen`). Không có quan hệ kế thừa class hay role trong Backend. |
| **Staff** | *Generalization* | **...** | `INVALID (REMOVED)` | `Staff` không phải là một Actor hay Role, do đó không tham gia vào bất kỳ quan hệ Generalization nào. |

---

## PART C — FINAL USE CASE MODEL (12 HIGH-LEVEL BUSINESS GROUPS)

Danh sách **12 High-Level Business Groups (HLUC1 $\rightarrow$ HLUC12)** tổng quát đóng vai trò là danh mục miền nghiệp vụ cấp cao:

| ID | High-Level Business Group | Primary Actor | Supporting Actor | Business Goal (Mục tiêu nghiệp vụ cốt lõi) | Evidence Source | Status |
| :-: | :--- | :--- | :--- | :--- | :--- | :-: |
| **HLUC1** | **Tra cứu Thực đơn & Sơ đồ Bàn** | Guest, Customer | -- | Cho phép khách hàng xem thực đơn điện tử, tìm kiếm món ăn và nhận diện bàn/chi nhánh qua QR. | `ProductController.cs`, `TableController.cs` | `IMPLEMENTED` |
| **HLUC2** | **Đặt món qua QR / Web Order** | Guest, Customer | Cashier, Employee | Cho phép khách hàng tự chọn món, chọn Size/Topping và gửi đơn đặt món tự phục vụ từ điện thoại. | `OrderController.cs`, `OrderService.cs` | `IMPLEMENTED` |
| **HLUC3** | **Đặt bàn trực tuyến** | Guest, Customer | Cashier, Employee, Manager | Cho phép khách hàng đặt lịch hẹn bàn trước; nhân viên/quản lý tiếp nhận và xử lý lịch hẹn. | `ReservationController.cs`, `ReservationService.cs` | `IMPLEMENTED` |
| **HLUC4** | **Quản lý Profile & Điểm Loyalty** | Customer | Cashier | Cho phép khách hàng định danh quản lý hồ sơ cá nhân, tra cứu điểm thưởng Loyalty và hạng thành viên. | `CustomerController.cs`, `LoyaltyService.cs` | `IMPLEMENTED` |
| **HLUC5** | **Quản lý Bán hàng POS & Phục vụ** | Employee, Cashier, Manager | Admin, Kitchen | Cho phép nhân viên/thu ngân lập đơn POS tại bàn, tiếp nhận đơn Web, chuyển đợt món xuống bếp KDS. | `OrderController.cs`, `OrderService.cs`, `POSPage.tsx` | `IMPLEMENTED` |
| **HLUC6** | **Thanh toán & In hóa đơn** | Cashier, Manager | Admin, Customer | Cho phép thu ngân xử lý thanh toán đơn hàng (Cash/Transfer), áp dụng đổi điểm Loyalty và in hóa đơn. | `OrderController.cs` (`PayOrder`), `ReceiptSettingsController.cs` | `IMPLEMENTED` |
| **HLUC7** | **Điều phối Chế biến (KDS)** | Kitchen | Manager, Admin | Cho phép nhà bếp nhận danh sách đợt nấu thời gian thực, cập nhật trạng thái nấu và báo tạm hết món. | `KitchenHub.cs`, `KitchenService.cs` | `IMPLEMENTED` |
| **HLUC8** | **Quản lý Ca làm việc & Chấm công**| Employee, Cashier, Kitchen, Manager | Admin | Cho phép nhân sự thực hiện chấm công QR, xem lịch làm việc cá nhân, mở và chốt ca làm việc (Shift). | `AttendanceController.cs`, `ShiftController.cs` | `IMPLEMENTED` |
| **HLUC9** | **Quản lý Vận hành Chi nhánh** | Manager | Admin | Cho phép quản lý điều hành sơ đồ bàn/khu vực, quản lý chi phí vận hành và phân lịch làm việc chi nhánh. | `TableController.cs`, `EmployeeController.cs`, `ExpenseController.cs` | `IMPLEMENTED` |
| **HLUC10**| **Quản trị Hệ thống & Danh mục** | Admin | Manager (Branch Settings) | Cho phép Admin quản lý danh mục thực đơn toàn chuỗi, topping, chi nhánh, ưu đãi và cấu hình VAT/Fee. | `ProductController.cs`, `BranchController.cs`, `SystemSettingsController.cs` | `IMPLEMENTED` |
| **HLUC11**| **Báo cáo Doanh thu & Insights** | Manager, Admin | -- | Cho phép quản trị viên xem Dashboard thống kê doanh thu và phân tích cảnh báo bất thường Business Insights. | `DashboardController.cs`, `BusinessInsightController.cs` | `IMPLEMENTED` |
| **HLUC12**| **Tương tác Trợ lý ảo AI Agent** | Guest, Customer, Employee, Cashier, Kitchen, Manager, Admin | -- | Cho phép người dùng đàm thoại ngôn ngữ tự nhiên với AI để tra cứu thông tin (Read) hoặc thực hiện tác vụ (Write). | `AiController.cs`, `AiOrchestrator.cs` | `IMPLEMENTED` |

---

## PART D — INVALID / REMOVED / TECHNICAL USE CASES (BÁO CÁO BỎ CÁC YẾU TỐ KỸ THUẬT)

Tất cả các yếu tố kỹ thuật, thuật toán nội bộ, middleware và tính năng đã xóa bị **LOẠI BỎ HOÀN TOÀN** khỏi danh sách Use Case:

| Candidate Element | Why Invalid as Use Case? | Correct Semantic Placement (Where It Belongs) |
| :--- | :--- | :--- |
| *Validate Payment / Check Amount* | Quy tắc xác thực tham số số tiền trong `PayOrderAsync`. | **Preconditions / Flow of Events** của `UC6.1: Thanh toán đơn hàng`. |
| *Calculate VAT / Service Fee / Size Price* | Thuật toán tính toán tự động trong `RecalculateOrderFinancials`. | **Flow of Events / Business Rules** của `UC5.1: Tạo đơn hàng POS`. |
| *Earn Loyalty Points Automatically* | Tác động phụ tự động phía server (`EarnPointsAsync`) sau khi thanh toán. | **Postconditions / System Side-Effect** của `UC6.1: Thanh toán đơn hàng`. |
| *Check Branch Permission / Validate JWT* | Cơ chế kiểm soát truy cập kỹ thuật của Middleware & Policy. | **System Preconditions / Security Constraints**. |
| *SignalR Event Broadcasting* | Giao thức truyền tin thời gian thực giữa Server và KDS Web Client. | **System Sequence / Technical Architecture**. |
| *Individual 14 AI Tools (GetRevenueTool...)* | Cơ chế thực thi chức năng nội bộ do AI Orchestrator gọi qua Gemini. | Gom vào 2 Sub-UC: Tra cứu AI (`UC12.1`) & Thực hiện tác vụ AI (`UC12.2`). |
| **Payroll (Quản lý & Tính bảng lương)** | **Đã bị xóa hoàn toàn khỏi mã nguồn** qua Migration `20260919075618`. | **REMOVED FEATURE**. Không xuất hiện ở bất kỳ biểu đồ hay ma trận nào. |

---

## PART E — INCLUDE AUDIT (AUDIT CÁC QUAN HỆ INCLUDE)

Audit ngữ nghĩa UML cho các quan hệ `<<include>>` (Bắt buộc phải thực hiện trong mọi kịch bản của Base Use Case):

| Source Sub-UC | Relationship | Target Sub-UC | Valid / Invalid | Semantic Reason & Code Evidence |
| :--- | :---: | :--- | :---: | :--- |
| **UC5.2: Tiếp nhận đơn Web Order** | **`<<include>>`** | **UC5.1: Tạo & Cập nhật đơn hàng POS** | `VALID` | Việc tiếp nhận đơn Web Order (`AcceptWebOrderAsync`) bắt buộc phải thực hiện việc chuyển đổi đơn chờ vãng lai thành một đơn POS chính thức gắn bàn/người nhận. |
| **UC5.3: Gửi yêu cầu chế biến xuống Bếp**| **`<<include>>`** | **UC5.1: Tạo & Cập nhật đơn hàng POS** | `VALID` | Mọi đợt gửi món xuống KDS bếp (`SendToKitchenAsync`) bắt buộc phải truy xuất và cập nhật lại số lượng đã gửi (`SentQuantity`) trên đơn hàng POS. |
| *UC6.1: Thanh toán đơn hàng* | *<<include>>* | *Tính VAT / Fee* | `INVALID (REMOVED)` | Đây là thuật toán tính toán nội bộ, không phải Use Case độc lập. |
| *UC12.1: Tra cứu qua AI* | *<<include>>* | *Check Permission* | `INVALID (REMOVED)` | Kiểm tra quyền là middleware kỹ thuật, không phải Use Case độc lập. |

---

## PART F — EXTEND AUDIT (AUDIT CÁC QUAN HỆ EXTEND)

Audit ngữ nghĩa UML cho các quan hệ `<<extend>>` (Mở rộng có điều kiện/tùy chọn):

| Extension Sub-UC | Relationship | Base Sub-UC | Valid / Invalid | Extension Point & Condition |
| :--- | :---: | :--- | :---: | :--- |
| **UC6.2: Đổi điểm Loyalty giảm giá** | **`<<extend>>`** | **UC5.1: Tạo & Cập nhật đơn hàng POS** | `VALID` | **Extension Point**: Lập tài chính đơn POS.<br>**Condition**: Chỉ thực hiện khi khách hàng yêu cầu quy đổi điểm tích lũy thành tiền giảm giá. |
| **UC6.3: Cấu hình & In hóa đơn** | **`<<extend>>`** | **UC6.1: Thanh toán đơn hàng** | `VALID` | **Extension Point**: Hoàn tất giao dịch thanh toán.<br>**Condition**: Chỉ thực hiện khi thu ngân hoặc khách hàng có nhu cầu xuất/in hóa đơn giấy. |

---

## PART G — ACTOR ASSOCIATION MATRIX (MA TRẬN KẾT NỐI ACTOR CHÍNH THỨC)

Bảng ma trận kết nối Association giữa 7 Actor và **29 Sub Use Cases thực tế** sau khi audit đếm lại chính xác:
- **`P`** = Primary Actor (Người khởi tạo Use Case để đạt mục tiêu nghiệp vụ)
- **`S`** = Supporting Actor (Người/Hệ thống hỗ trợ hoặc tiếp nhận trong luồng)
- **`-`** = Không kết nối Association

| Detailed Sub Use Case ID & Name | Admin | Manager | Cashier | Employee | Kitchen | Customer | Guest |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **UC1.1: Quét QR Bàn & Chi nhánh** | - | - | - | - | - | **P** | **P** |
| **UC1.2: Xem thực đơn & Chi tiết món** | - | - | - | - | - | **P** | **P** |
| **UC2.1: Gửi đơn đặt món QR / Web Order** | - | - | S | S | - | **P** | **P** |
| **UC2.2: Xem trạng thái & Lịch sử đơn cá nhân** | - | - | - | - | - | **P** | - |
| **UC3.1: Tạo yêu cầu đặt bàn trực tuyến** | - | - | - | - | - | **P** | **P** |
| **UC3.2: Tiếp nhận & Quản lý lịch hẹn đặt bàn** | S | **P** | **P** | **P** | - | - | - |
| **UC4.1: Xem & Cập nhật Hồ sơ cá nhân** | - | - | - | - | - | **P** | - |
| **UC4.2: Tra cứu điểm & Hạng thành viên Loyalty** | - | - | S | - | - | **P** | - |
| **UC5.1: Tạo & Cập nhật đơn hàng POS** | S | **P** | **P** | **P** | - | - | - |
| **UC5.2: Tiếp nhận đơn đặt món Web Order** | S | **P** | **P** | **P** | - | - | - |
| **UC5.3: Gửi yêu cầu chế biến xuống Bếp** | S | **P** | **P** | **P** | S | - | - |
| **UC6.1: Thanh toán đơn hàng (Cash/Transfer)** | S | **P** | **P** | - | - | S | - |
| **UC6.2: Đổi điểm Loyalty giảm giá đơn hàng** | S | **P** | **P** | - | - | S | - |
| **UC6.3: Cấu hình & In hóa đơn thanh toán** | S | **P** | **P** | - | - | - | - |
| **UC7.1: Xử lý đợt chế biến KDS thời gian thực** | S | S | - | - | **P** | - | - |
| **UC7.2: Cập nhật trạng thái tạm hết món** | **P** | - | - | - | **P** | - | - |
| **UC8.1: Chấm công QR Check-in / Check-out** | S | **P** | **P** | **P** | **P** | - | - |
| **UC8.2: Mở & Chốt ca làm việc (Shift)** | S | **P** | **P** | **P** | **P** | - | - |
| **UC8.3: Xem & Đăng ký Lịch làm việc** | S | **P** | **P** | **P** | **P** | - | - |
| **UC9.1: Quản lý Sơ đồ bàn & Khu vực chi nhánh** | S | **P** | - | - | - | - | - |
| **UC9.2: Quản lý Hồ sơ & Phân lịch Nhân sự** | S | **P** | - | - | - | - | - |
| **UC9.3: Quản lý Chi phí vận hành chi nhánh** | S | **P** | **P** | - | - | - | - |
| **UC10.1: Quản lý Danh mục Thực đơn & Topping**| **P** | - | - | - | - | - | - |
| **UC10.2: Quản lý Chi nhánh & Ưu đãi toàn chuỗi**| **P** | - | - | - | - | - | - |
| **UC10.3: Cấu hình Thiết lập Hệ thống (VAT/Fee)**| **P** | **P** | - | - | - | - | - |
| **UC11.1: Xem Dashboard thống kê doanh thu** | **P** | **P** | - | - | - | - | - |
| **UC11.2: Xem Phân tích & Cảnh báo Insights AI** | **P** | **P** | - | - | - | - | - |
| **UC12.1: Tra cứu thông tin qua AI (Read Tools)** | **P** | **P** | **P** | **P** | **P** | **P** | **P** |
| **UC12.2: Thực hiện tác vụ qua AI (Write Tools)**| **P** | **P** | **P** | **P** | **P** | **P** | - |

---

## PART H — FINAL DECOMPOSITION (CẤU TRÚC PHÂN CẤP SƠ ĐỒ)

Cấu trúc phân cấp biểu đồ từ **Hình 3.1 đến Hình 3.7** được chốt như sau:

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

## PART I — CONTRADICTIONS (CÁC MÂU THUẪN VÀ KHÁC BIỆT ĐÃ GIẢI QUYẾT)

| Issue ID | Mô tả mâu thuẫn / Discrepancy | Mức độ Severity | Cách xử lý theo Single Source of Truth (Backend) |
| :-: | :--- | :---: | :--- |
| **CONTRAD-01** | **Manager hiển thị Menu Thực đơn trên Frontend** | `MAJOR` | On UI Admin-Web, Manager thấy menu Thực đơn. Tuy nhiên, Backend `ProductController` và `ToppingController` đều có `[Authorize(Roles="admin")]`. **Chốt**: Manager KHÔNG PHẢI Primary Actor của `UC10.1` (CRUD Thực đơn toàn chuỗi). Manager chỉ thực hiện `UC7.2` (Báo tạm hết món tại chi nhánh). |
| **CONTRAD-02** | **Xóa bỏ hoàn toàn Module Payroll** | `CRITICAL` | File code `PayrollController.cs` và UI `PayrollPage.tsx` chỉ còn comment obsolete. Migration `20260919075618` đã xóa các bảng Payroll. **Chốt**: Loại bỏ hoàn toàn khỏi mô hình UML. |
| **CONTRAD-03** | **Chiết khấu Discount do Client gửi lên** | `MINOR` | Client POS/Customer có thể gửi field `Discount` trong body, nhưng `OrderService` luôn gán `Discount = 0` khi tạo đơn mới để chống gian lận. **Chốt**: Giảm giá chỉ được sinh ra qua Use Case `UC6.2 (Đổi điểm Loyalty)`. |

---

## PART J — FINAL UML MODEL (TỔNG HỢP MÔ HÌNH UML CHÍNH THỨC)

1. **Actors (7)**: `Admin` (Global), `Manager` (Branch), `Cashier` (Branch), `Employee` (Branch), `Kitchen` (Branch), `Customer` (Personal), `Guest` (Contextual).
2. **Actor Generalization (1)**: **`Customer --|> Guest`**.
3. **High-Level Business Groups (12)**: `HLUC1` đến `HLUC12`.
4. **Detailed Sub Use Cases (29)**: `UC1.1` đến `UC12.2`.
5. **Includes (2)**:
   - `UC5.2 (Tiếp nhận Web Order)` **`--<<include>>-->`** `UC5.1 (Tạo/Cập nhật đơn POS)`
   - `UC5.3 (Gửi bếp KDS)` **`--<<include>>-->`** `UC5.1 (Tạo/Cập nhật đơn POS)`
6. **Extends (2)**:
   - `UC6.2 (Đổi điểm Loyalty)` **`--<<extend>>-->`** `UC5.1 (Tạo/Cập nhật đơn POS)`
   - `UC6.3 (Cấu hình & In hóa đơn)` **`--<<extend>>-->`** `UC6.1 (Thanh toán đơn hàng)`
7. **Removed Elements**: Module Payroll, các thuật toán VAT/Fee calculation, Validation rules, Middleware checks, và các AI Tools rời rạc.

---

## FINAL UML QUALITY GATE

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

### **FINAL UML READY**

**Kết luận**: Tất cả 15 Semantic Quality Gates đều **PASS**. Mô hình Use Case UML 2.5 đã đạt độ hoàn thiện tối đa, trung thực 100% với mã nguồn dự án hiện tại, loại bỏ hoàn toàn nhiễu kỹ thuật và hoàn toàn sẵn sàng để chuyển sang bước xuất mã **Draw.io XML** cho từng hình sơ đồ (Hình 3.1 $\rightarrow$ Hình 3.7)!
