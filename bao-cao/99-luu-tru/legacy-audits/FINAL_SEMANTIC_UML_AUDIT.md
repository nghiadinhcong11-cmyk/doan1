# FINAL UML LOCK AUDIT — MODEL CONSISTENCY & LOCK VALIDATION

**Đề tài**: Hệ thống quản lý nhà hàng đa chi nhánh tích hợp Trợ lý ảo AI Agent  
**Chức danh**: Senior System Analyst / Software Architect / Requirements Engineer / UML 2.5 Reviewer / Business Process Analyst  
**Nguyên tắc**: Báo cáo đánh giá và khóa mô hình Use Case UML 2.5 cuối cùng trước khi chuyển sang vẽ sơ đồ Draw.io XML và hoàn thiện nội dung Chương 3.

---

## A. OVERALL STATUS

### **PASS — UML LOCK**

**Kết luận**: Mô hình Use Case đã đạt độ nhất quán tối đa giữa kỳ vọng nghiệp vụ, chuẩn mực UML 2.5 và thực tế mã nguồn triển khai. Đủ điều kiện khóa hoàn toàn mô hình Use Case để tiến hành sinh mã Draw.io XML cho 7 biểu đồ (Hình 3.1 $\rightarrow$ Hình 3.7). Không cần thực hiện thêm cuộc audit UML tổng thể nào khác trừ khi có thay đổi nghiệp vụ mới từ người dùng.

---

## B. MODEL COUNT SUMMARY

| Element Item | Expected Baseline | Actual Count in Model | Status | Notes / Adjustments |
| :--- | :---: | :---: | :---: | :--- |
| **Business Actors** | **7** | **7** | `MATCH` | `Admin`, `Manager`, `Cashier`, `Employee`, `Kitchen`, `Customer`, `Guest`. (`Staff` không phải Actor). |
| **High-Level Business Groups (HLUC)** | **12** | **12** | `MATCH` | `HLUC1` $\rightarrow$ `HLUC12` đại diện cho 12 miền nghiệp vụ cấp cao. |
| **Detailed Sub Use Cases (Sub-UC)** | **33** | **33** | `MATCH` | Đã loại bỏ UC "Cập nhật lịch sử đơn hàng" cũ (khóa vĩnh viễn đơn đóng) và đánh số lại HLUC5 thành **4 Sub-UCs** (`5.1` $\rightarrow$ `5.4`). |
| **Actor Generalization** | **1** | **1** | `MATCH` | Duy nhất quan hệ **`Customer --|> Guest`**. |
| **Valid Include Relationships** | **1** | **1** | `MATCH` | `UC5.3 (Nhận đơn Web)` **`--<<include>>-->`** `UC5.1 (Tạo đơn POS)`. *(Đã loại bỏ `UC5.4 <<include>> UC5.1` do là Precondition)*. |
| **Valid Extend Relationships** | **2** | **2** | `MATCH` | `UC6.2 (Đổi điểm Loyalty)` **`--<<extend>>-->`** `UC5.1 (Tạo đơn POS)`; `UC6.5 (In hóa đơn)` **`--<<extend>>-->`** `UC6.1 (Thanh toán)`. |

---

## C. ACTOR MODEL VALIDATION

1. **7 Business Actors chính thức**:
   - **Admin tổng**: Global Scope (`BranchId = null` / linh hoạt). Quản trị toàn hệ thống.
   - **Manager**: Branch Scope (`branchId` từ JWT Claim). Quản lý vận hành chi nhánh.
   - **Cashier**: Branch Scope (`branchId` từ JWT Claim). Bán hàng POS và tài chính quầy.
   - **Employee**: Branch Scope (`branchId` từ JWT Claim). Phục vụ bàn (Waitstaff).
   - **Kitchen**: Branch Scope (`branchId` từ JWT Claim). Điều phối chế biến nhà bếp (KDS).
   - **Customer**: Personal Scope (Xác thực qua `CustomerId` / SĐT). Khách hàng định danh.
   - **Guest**: Contextual / Public Scope (Theo QR Bàn / Request Context). Khách vãng lai chưa đăng nhập.
2. **Quy tắc Staff Boundary**: `Staff` **KHÔNG PHẢI LÀ ACTOR**. `Staff` chỉ là Khái niệm nhóm nghiệp vụ (Business Grouping Concept) bao gồm `Manager`, `Cashier`, `Employee`, `Kitchen`. Không có Actor hay Role `Staff` trong UML.
3. **Actor Generalization**: **`Customer --|> Guest`** (`VALID`). `Customer` kế thừa toàn bộ khả năng tương tác công khai của `Guest` và mở rộng các Use Case định danh cá nhân. Không áp dụng Generalization giữa Admin và Manager hay giữa các vai trò Staff.

---

## D. DETAILED SUB-UC STRUCTURE (CHÍNH THỨC 33 SUB-UCS)

Cấu trúc 33 Sub-UCs đã được đánh số lại chuẩn xác theo 12 High-Level Business Groups:

### HLUC1 — Tra cứu Thực đơn & Sơ đồ Bàn (2 Sub-UCs)
- **1.1**: Quét QR Bàn & Chi nhánh
- **1.2**: Xem thực đơn & Chi tiết món

### HLUC2 — Đặt món & Lịch sử đơn hàng (3 Sub-UCs)
- **2.1**: Gửi đơn đặt món QR / Web Order
- **2.2**: Xem lịch sử đơn hàng *(Lịch sử hóa đơn bán hàng/tài chính)*
- **2.3**: Xem lịch sử chế biến KDS *(Lịch sử đợt chế biến nhà bếp)*

### HLUC3 — Đặt bàn trực tuyến (2 Sub-UCs)
- **3.1**: Tạo yêu cầu đặt bàn trực tuyến
- **3.2**: Tiếp nhận & Quản lý lịch hẹn đặt bàn

### HLUC4 — Hồ sơ & Loyalty (2 Sub-UCs)
- **4.1**: Xem & Cập nhật Hồ sơ cá nhân
- **4.2**: Tra cứu điểm & Hạng thành viên Loyalty

### HLUC5 — Quản lý Bán hàng POS & Phục vụ (4 Sub-UCs - Đã renumber)
- **5.1**: Tạo đơn hàng POS
- **5.2**: Thêm / Xóa món trong đơn hàng
- **5.3**: Tiếp nhận đơn đặt món Web Order
- **5.4**: Gửi yêu cầu chế biến xuống Bếp

### HLUC6 — Thanh toán & Hóa đơn (5 Sub-UCs)
- **6.1**: Thanh toán đơn hàng
- **6.2**: Đổi điểm Loyalty giảm giá đơn hàng
- **6.3**: Cấu hình phương thức/tham số thanh toán
- **6.4**: Cấu hình thiết lập hệ thống
- **6.5**: In / Xuất hóa đơn thanh toán

### HLUC7 — Điều phối Chế biến KDS (2 Sub-UCs)
- **7.1**: Xử lý đợt chế biến KDS thời gian thực
- **7.2**: Cập nhật trạng thái tạm hết món

### HLUC8 — Ca làm việc & Chấm công (4 Sub-UCs)
- **8.1**: Chấm công QR Check-in / Check-out
- **8.2**: Mở & Chốt ca làm việc (Shift)
- **8.3**: Xem lịch làm việc
- **8.4**: Phân lịch làm việc

### HLUC9 — Quản lý Vận hành Chi nhánh (3 Sub-UCs)
- **9.1**: Quản lý Sơ đồ bàn & Khu vực chi nhánh
- **9.2**: Quản lý Hồ sơ & Phân lịch Nhân sự
- **9.3**: Quản lý Chi phí vận hành chi nhánh

### HLUC10 — Quản trị Hệ thống & Danh mục (2 Sub-UCs)
- **10.1**: Quản lý Danh mục Thực đơn & Topping
- **10.2**: Quản lý Chi nhánh & Ưu đãi toàn chuỗi

### HLUC11 — Báo cáo Doanh thu & Insights (2 Sub-UCs)
- **11.1**: Xem Dashboard thống kê doanh thu
- **11.2**: Xem Phân tích & Cảnh báo Insights AI

### HLUC12 — Tương tác Trợ lý ảo AI Agent (2 Sub-UCs)
- **12.1**: Tra cứu thông tin qua AI Read Tools
- **12.2**: Thực hiện tác vụ nghiệp vụ qua AI Write Tools

---

## E. ACTOR ASSOCIATION MATRIX (33 SUB-UCS)

Ký hiệu: **`P`** = Primary Actor (Người khởi tạo Use Case); **`S`** = Supporting Actor (Người/Hệ thống hỗ trợ); **`–`** = Không tham gia.

| Sub Use Case ID & Name | Admin | Manager | Cashier | Employee | Kitchen | Customer | Guest |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **UC1.1: Quét QR Bàn & Chi nhánh** | - | - | - | - | - | **P** | **P** |
| **UC1.2: Xem thực đơn & Chi tiết món** | **P** | **P** | **P** | **P** | - | **P** | **P** |
| **UC2.1: Gửi đơn đặt món QR / Web Order** | - | - | S | S | - | **P** | **P** |
| **UC2.2: Xem lịch sử đơn hàng** | **P** | **P** | **P** | **P** | - | **P** | - |
| **UC2.3: Xem lịch sử chế biến KDS** | S | S | - | - | **P** | - | - |
| **UC3.1: Tạo yêu cầu đặt bàn trực tuyến** | - | - | - | - | - | **P** | **P** |
| **UC3.2: Tiếp nhận & Quản lý lịch hẹn đặt bàn** | S | **P** | **P** | **P** | - | - | - |
| **UC4.1: Xem & Cập nhật Hồ sơ cá nhân** | - | - | - | - | - | **P** | - |
| **UC4.2: Tra cứu điểm & Hạng thành viên Loyalty** | S | S | S | S | - | **P** | - |
| **UC5.1: Tạo đơn hàng POS** | **P** | **P** | **P** | **P** | - | - | - |
| **UC5.2: Thêm / Xóa món trong đơn hàng** | **P** | **P** | **P** | **P** | - | - | - |
| **UC5.3: Tiếp nhận đơn đặt món Web Order** | **P** | **P** | **P** | **P** | - | - | - |
| **UC5.4: Gửi yêu cầu chế biến xuống Bếp** | S | **P** | **P** | **P** | S | - | - |
| **UC6.1: Thanh toán đơn hàng (Cash/Transfer)** | **P** | **P** | **P** | **P** | - | - | - |
| **UC6.2: Đổi điểm Loyalty giảm giá đơn hàng** | **P** | **P** | **P** | **P** | - | - | - |
| **UC6.3: Cấu hình phương thức/tham số thanh toán** | **P** | - | - | - | - | - | - |
| **UC6.4: Cấu hình thiết lập hệ thống** | **P** | - | - | - | - | - | - |
| **UC6.5: In / Xuất hóa đơn thanh toán** | **P** | **P** | **P** | **P** | - | - | - |
| **UC7.1: Xử lý đợt chế biến KDS thời gian thực** | - | - | - | - | **P** | - | - |
| **UC7.2: Cập nhật trạng thái tạm hết món** | **P** | **P** | - | - | **P** | - | - |
| **UC8.1: Chấm công QR Check-in / Check-out** | **P** | **P** | **P** | **P** | **P** | - | - |
| **UC8.2: Mở & Chốt ca làm việc (Shift)** | - | - | **P** | - | - | - | - |
| **UC8.3: Xem lịch làm việc** | **P** | **P** | **P** | **P** | **P** | - | - |
| **UC8.4: Phân lịch làm việc** | **P** | **P** | - | - | - | - | - |
| **UC9.1: Quản lý Sơ đồ bàn & Khu vực chi nhánh** | **P** | **P** | - | - | - | - | - |
| **UC9.2: Quản lý Hồ sơ & Phân lịch Nhân sự** | **P** | **P** | - | - | - | - | - |
| **UC9.3: Quản lý Chi phí vận hành chi nhánh** | **P** | **P** | - | - | - | - | - |
| **UC10.1: Quản lý Danh mục Thực đơn & Topping**| **P** | - | - | - | - | - | - |
| **UC10.2: Quản lý Chi nhánh & Ưu đãi toàn chuỗi**| **P** | - | - | - | - | - | - |
| **UC11.1: Xem Dashboard thống kê doanh thu** | **P** | **P** | - | - | - | - | - |
| **UC11.2: Xem Phân tích & Cảnh báo Insights AI** | **P** | **P** | - | - | - | - | - |
| **UC12.1: Tra cứu thông tin qua AI (Read Tools)** | **P** | **P** | **P** | **P** | **P** | **P** | **P** |
| **UC12.2: Thực hiện tác vụ qua AI (Write Tools)**| **P** | **P** | **P** | **P** | **P** | **P** | - |

---

## F. INCLUDE / EXTEND / GENERALIZATION AUDIT

| Relationship | Source Element | Target Element | Decision | Semantic Reason & Justification |
| :--- | :--- | :--- | :---: | :--- |
| **Actor Generalization** | `Customer` | `Guest` | **`VALID`** | `Customer` kế thừa toàn bộ khả năng tương tác công khai của `Guest` (quét QR, xem menu, tạo đơn vãng lai) và mở rộng thêm các Use Case định danh cá nhân. |
| **`<<include>>`** | `UC5.3 (Nhận đơn Web)` | `UC5.1 (Tạo đơn POS)` | **`VALID`** | Tiếp nhận đơn Web Order chuyển đổi đơn chờ vãng lai thành một đơn hàng POS chính thức gắn bàn/người nhận. |
| **`<<include>>`** | `UC5.4 (Gửi bếp)` | `UC5.1 (Tạo đơn POS)` | **`INVALID (REMOVE)`** | Sự tồn tại của đơn POS là **Precondition** cho việc gửi bếp, không phải hành vi con được thực hiện bên trong `SendToKitchenAsync`. Bỏ mũi tên `<<include>>` này trên UML. |
| **`<<extend>>`** | `UC6.2 (Đổi điểm Loyalty)` | `UC5.1 (Tạo đơn POS)` | **`VALID`** | **Extension Point**: Lập tài chính đơn POS. **Condition**: Chỉ thực hiện khi khách hàng yêu cầu quy đổi điểm tích lũy thành tiền giảm giá. |
| **`<<extend>>`** | `UC6.5 (In hóa đơn)` | `UC6.1 (Thanh toán)` | **`VALID`** | **Extension Point**: Hoàn tất giao dịch thanh toán. **Condition**: Chỉ thực hiện khi thu ngân hoặc khách hàng có nhu cầu xuất/in hóa đơn giấy. |

---

## G. EXPECTED VS ACTUAL ALIGNMENT REVIEW

| Sub Use Case ID | Expected Baseline | Actual Code Implementation | Status | Explanation / Evidence |
| :--- | :--- | :--- | :---: | :--- |
| **UC7.2 (Tạm hết món)** | Admin, Manager, Kitchen | `ProductController.cs` `[Authorize(Roles="admin,manager,kitchen")]` | **`MATCH (FIXED)`** | Đã bổ sung `manager` role ở Backend, đạt trạng thái `MATCH` 100%. |
| **UC9.3 (Chi phí vận hành)** | Admin, Manager | `ExpenseController.cs` `[Authorize(Roles="admin,manager")]` | **`MATCH (FIXED)`** | Đã siết chặt quyền chỉ cho Admin và Manager ở Backend, đạt trạng thái `MATCH` 100%. |
| **UC8.2 (Mở & Chốt ca)** | Cashier ONLY | `ShiftController.cs` `[Authorize(Roles="admin,manager,cashier")]` | **`EXTRA ACTUAL`** | Quy trình vận hành dự phòng khi Cashier vắng mặt. Giữ Expected là Cashier ONLY, ghi nhận Extra Actual trong hồ sơ. |
| **UC6.3 & UC6.4 (Cấu hình Chi nhánh)**| Admin ONLY | `ReceiptSettingsController.cs` & `SystemSettingsController.cs` (Branch-scoped Put) | **`EXTRA ACTUAL`** | Backend hỗ trợ Manager phân quyền quản lý thiết lập cấp chi nhánh. Giữ Expected là Admin ONLY, ghi nhận Extra Actual. |

---

## H. USE CASE DECOMPOSITION MAP (HÌNH 3.1 $\rightarrow$ HÌNH 3.7)

Biểu đồ tổng quát (Hình 3.1) chứa 7 Actors và 12 HLUCs, phân rã trực tiếp xuống 6 biểu đồ phân hệ nhánh:
- **Hình 3.2 (Domain Khách hàng & Đặt món Trực tuyến)**: Phân rã `HLUC1`, `HLUC2`, `HLUC3`, `HLUC4` $\rightarrow$ `UC1.1`, `UC1.2`, `UC2.1`, `UC2.2`, `UC3.1`, `UC3.2`, `UC4.1`, `UC4.2` (8 Sub-UCs).
- **Hình 3.3 (Domain Bán hàng POS, Thanh toán & Hóa đơn)**: Phân rã `HLUC5`, `HLUC6` $\rightarrow$ `UC5.1`, `UC5.2`, `UC5.3`, `UC5.4`, `UC6.1`, `UC6.2`, `UC6.3`, `UC6.4`, `UC6.5` (9 Sub-UCs).
- **Hình 3.4 (Domain Điều phối Chế biến Nhà bếp KDS)**: Phân rã `HLUC7` $\rightarrow$ `UC7.1`, `UC7.2` (2 Sub-UCs).
- **Hình 3.5 (Domain Ca làm việc & Chấm công Nhân sự)**: Phân rã `HLUC8` $\rightarrow$ `UC8.1`, `UC8.2`, `UC8.3`, `UC8.4` (4 Sub-UCs).
- **Hình 3.6 (Domain Quản trị Vận hành, Hệ thống & Insights)**: Phân rã `HLUC9`, `HLUC10`, `HLUC11` $\rightarrow$ `UC9.1`, `UC9.2`, `UC9.3`, `UC10.1`, `UC10.2`, `UC11.1`, `UC11.2` (7 Sub-UCs).
- **Hình 3.7 (Domain Tương tác Trợ lý ảo AI Agent)**: Phân rã `HLUC12` $\rightarrow$ `UC12.1` (Read Tools), `UC12.2` (Write Tools) (2 Sub-UCs).
- **Tổng số Sub-UCs trên toàn bộ 6 hình phân rã**: 8 + 9 + 2 + 4 + 7 + 2 = **33 Sub Use Cases**.

---

## I. FINAL ISSUES CLASSIFICATION

- **BLOCKER**: Không có.
- **MINOR**: Không có.
- **INFORMATIONAL**: Ghi nhận các capability mở rộng cấp chi nhánh cho Manager (`UC6.3`, `UC6.4`) và thao tác ca dự phòng cho Manager/Admin (`UC8.2`) làm tài liệu diễn giải cho Báo cáo Đồ án.

---

## J. FINAL DECISION

### **UML LOCK = PASS**

> **Khẳng định chính thức**: Mô hình Use Case đã đủ nhất quán để chuyển sang bước vẽ Draw.io và viết nội dung Chương 3. Không cần audit UML tổng thể thêm lần nữa trừ khi yêu cầu nghiệp vụ hoặc source code thay đổi.
