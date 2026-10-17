# Chương 3 — Audit lượt B: dữ liệu, kiến trúc và realtime

## 1. Phạm vi và phiên bản

Audit tĩnh ngày 16/10/2026, nhánh `main`, HEAD `38bbd423b963aaf5779d4ae3356f942ff236643b`. Working tree có thay đổi chưa commit ở frontend, AI và tài liệu; không sửa các tệp đó. Word gốc SHA-256 `D26DCE6C0437D9E54AB66862E40888C3A2EF21CE565A0016379294A7AE11ABF9`. Lượt B đọc B738–945 trong `word/document.xml` và đối chiếu entity/EF mapping, service, Hub, frontend. Không kiểm tra DB thật, không chạy migration/build/test/runtime, không audit hình thức hình.

## 2. Đề cương Word nguyên gốc đã audit

- 3.3 Thiết kế cơ sở dữ liệu: mô hình ERD (B739–748, Hình 3.14); chi tiết 29 nhóm bảng (B749–884, Bảng 3.10–3.38); quan hệ/ràng buộc (B885–899); đa chi nhánh (B900–906). Đã audit toàn bộ văn bản và bảng.
- 3.4 Thiết kế kiến trúc và xử lý nghiệp vụ: tổng thể (B908–919, Hình 3.15), backend/tầng (B920–929), frontend/API (B930–936), realtime (B937–945, Hình 3.16). Đã audit; xử lý tiền B946–1001 thuộc lượt C.

## 3. Bảng đối chiếu đầy đủ 29 nhóm bảng Word với CLR

Bảng dưới đây giữ **năm cột gốc** cho ChatGPT. Kiểu và `Null` được sửa theo khai báo CLR tại checkout; `Khóa` là EF mapping được đối chiếu ở mục 4, không phải DB đã triển khai. `UQ*` nghĩa là thành phần của index duy nhất ghép/có điều kiện, không là unique độc lập. `—` không suy ra FK chỉ từ hậu tố `Id`. `OrderDetail.OrderId` là EF shadow FK; `OrderDetail.SubTotal` chỉ tính trong CLR, không là cột. Ý nghĩa ngắn gọn từ Word, không phải văn xuôi chương. Mọi bảng có nguồn `services/api/src/Domain/Entities/<file>`; các cấu hình riêng ở `ApplicationDbContext.cs` được chỉ tại mục 4.

### Bảng 3.10 — Branch (Word B754)

Nguồn CLR: `services/api/src/Domain/Entities/Branch.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính chi nhánh |
| Name | string | — | Không | Tên chi nhánh |
| Address | string | — | Có | Địa chỉ chi nhánh |
| PhoneNumber | string | — | Có | Số điện thoại |
| IsMain | bool | — | Không | Xác định chi nhánh chính |
| IsActive | bool | — | Không | Trạng thái hoạt động |
| BankName | string | — | Có | Tên ngân hàng |
| AccountNumber | string | — | Có | Số tài khoản |
| AccountHolder | string | — | Có | Chủ tài khoản |
| ImageUrl | string | — | Có | Hình ảnh chi nhánh |
| TaxCode | string | — | Có | Mã số thuế |
| RepresentativeName | string | — | Có | Người đại diện |
| RepresentativeEmail | string | — | Có | Email người đại diện |
| Industry | string | — | Có | Ngành nghề |
| BusinessType | string | — | Có | Loại hình kinh doanh |
| CreatedAt | DateTime | — | Không | Thời điểm tạo |

### Bảng 3.11 — Area (Word B758)

Nguồn CLR: `services/api/src/Domain/Entities/Area.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính khu vực |
| BranchId | Guid | FK | Không | Chi nhánh sở hữu khu vực |
| Name | string | — | Không | Tên khu vực |
| DisplayOrder | int | — | Không | Thứ tự hiển thị |
| CreatedAt | DateTime | — | Không | Thời điểm tạo |

### Bảng 3.12 — RestaurantTable (Word B762)

Nguồn CLR: `services/api/src/Domain/Entities/Table.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính bàn |
| Name | string | — | Không | Tên hoặc số bàn |
| AreaId | Guid | FK | Không | Khu vực chứa bàn |
| BranchId | Guid | FK | Không | Chi nhánh quản lý bàn |
| SeatCount | int | — | Không | Số chỗ ngồi |
| Description | string | — | Có | Mô tả bàn |
| IsActive | bool | — | Không | Trạng thái hoạt động |
| Status | string | — | Không | Trạng thái sử dụng bàn |
| QrCodeUrl | string | — | Có | Đường dẫn mã QR |
| QrToken | string | UQ | Không | Token QR duy nhất |
| BranchName | string | — | Có | Tên chi nhánh |
| CreatedAt | DateTime | — | Không | Thời điểm tạo |

### Bảng 3.13 — Employee (Word B766)

Nguồn CLR: `services/api/src/Domain/Entities/Employee.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính nhân viên |
| EmployeeCode | string | — | Có | Mã nhân viên |
| EmployeeType | string | — | Không | Loại nhân viên |
| FullName | string | — | Không | Họ tên |
| PhoneNumber | string | — | Có | Số điện thoại |
| Position | string | — | Có | Chức danh |
| Department | string | — | Có | Bộ phận |
| BasicSalary | decimal | — | Không | Lương cơ bản |
| BranchId | Guid | FK | Có | Chi nhánh được phân công |
| BranchName | string | — | Có | Tên chi nhánh |
| CitizenId | string | — | Có | Số giấy tờ định danh |
| BirthDate | DateTime | — | Có | Ngày sinh |
| Gender | string | — | Có | Giới tính |
| Address | string | — | Có | Địa chỉ |
| StartDate | DateTime | — | Không | Ngày bắt đầu làm việc |
| IsActive | bool | — | Không | Trạng thái hoạt động |
| Role | string | — | Không | Vai trò phân quyền |
| Username | string | — | Có | Tên đăng nhập |
| Password | string | — | Có | Giá trị lưu phục vụ xác minh mật khẩu |
| Note | string | — | Có | Ghi chú |
| CreatedAt | DateTime | — | Không | Thời điểm tạo |

### Bảng 3.14 — Attendance (Word B770)

Nguồn CLR: `services/api/src/Domain/Entities/Attendance.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính |
| EmployeeId | Guid | FK | Không | Nhân viên chấm công |
| EmployeeName | string | — | Có | Tên nhân viên |
| CheckInTime | DateTime | — | Không | Thời điểm vào ca |
| CheckOutTime | DateTime | — | Có | Thời điểm ra ca |
| BranchId | Guid | FK | Không | Chi nhánh |
| Status | string | — | Có | Trạng thái chấm công |
| Note | string | — | Có | Ghi chú |
| ImagePath | string | — | Có | Đường dẫn ảnh liên quan |

### Bảng 3.15 — WorkSchedule (Word B774)

Nguồn CLR: `services/api/src/Domain/Entities/WorkSchedule.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính lịch làm việc |
| EmployeeId | Guid | FK | Không | Nhân viên được phân công |
| EmployeeName | string | — | Có | Tên nhân viên |
| Date | DateTime | — | Không | Ngày làm việc |
| ShiftName | string | — | Có | Tên ca làm việc |
| StartTime | string | — | Có | Thời gian bắt đầu |
| EndTime | string | — | Có | Thời gian kết thúc |
| BranchId | Guid | FK | Có | Chi nhánh làm việc |
| BranchName | string | — | Có | Tên chi nhánh |

### Bảng 3.16 — Shift (Word B778)

Nguồn CLR: `services/api/src/Domain/Entities/Shift.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính ca làm việc |
| EmployeeId | Guid | FK | Không | Nhân viên thực hiện ca |
| EmployeeName | string | — | Không | Tên nhân viên |
| BranchId | Guid | FK | Không | Chi nhánh làm việc |
| BranchName | string | — | Có | Tên chi nhánh |
| StartTime | DateTime | — | Không | Thời điểm bắt đầu ca |
| EndTime | DateTime | — | Có | Thời điểm kết thúc ca |
| StartingCash | decimal | — | Không | Số tiền mặt đầu ca |
| EndingCash | decimal | — | Có | Số tiền mặt cuối ca |
| TotalRevenue | decimal | — | Không | Tổng doanh thu trong ca |
| CashRevenue | decimal | — | Không | Doanh thu thanh toán tiền mặt |
| TransferRevenue | decimal | — | Không | Doanh thu thanh toán chuyển khoản |
| Status | string | — | Không | Trạng thái ca làm việc |
| Note | string | — | Không | Ghi chú |

### Bảng 3.17 — Customer (Word B782)

Nguồn CLR: `services/api/src/Domain/Entities/Customer.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính khách hàng |
| FullName | string | — | Có | Họ và tên khách hàng |
| PhoneNumber | string | UQ | Có | Số điện thoại khách hàng |
| Email | string | — | Có | Địa chỉ email |
| Address | string | — | Có | Địa chỉ liên hệ |
| Birthday | DateTime | — | Có | Ngày sinh |
| Gender | string | — | Có | Giới tính |
| CustomerGroup | string | — | Có | Nhóm khách hàng |
| TotalSpending | decimal | — | Không | Tổng số tiền khách đã chi tiêu |
| TotalOrders | int | — | Không | Tổng số đơn hàng |
| LoyaltyPoints | int | — | Không | Số điểm tích lũy hiện tại |
| IsActive | bool | — | Không | Trạng thái hoạt động của tài khoản |
| Password | string | — | Có | Giá trị lưu phục vụ xác minh mật khẩu |
| CreatedAt | DateTime | — | Không | Thời điểm tạo hồ sơ |
| UpdatedAt | DateTime | — | Không | Thời điểm cập nhật hồ sơ |
| LastOrderDate | DateTime | — | Có | Thời điểm đặt hàng gần nhất |

### Bảng 3.18 — Order (Word B786)

Nguồn CLR: `services/api/src/Domain/Entities/Order.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính đơn hàng |
| InvoiceCode | string | — | Có | Mã hóa đơn |
| CreatedAt | DateTime | — | Không | Thời điểm tạo đơn hàng |
| PaymentAt | DateTime | — | Có | Thời điểm thanh toán |
| CustomerId | Guid | FK | Có | Khách hàng liên kết với đơn |
| CustomerName | string | — | Có | Tên khách hàng |
| CustomerPhone | string | — | Có | Số điện thoại khách hàng |
| CustomerEmail | string | — | Có | Email khách hàng |
| SubTotal | decimal | — | Có | Tổng tiền sản phẩm trước thuế và phí |
| VatAmount | decimal | — | Có | Số tiền thuế VAT |
| VatPercent | decimal | — | Có | Tỷ lệ thuế VAT |
| ServiceFeeAmount | decimal | — | Có | Số tiền phí dịch vụ |
| ServiceFeePercent | decimal | — | Có | Tỷ lệ phí dịch vụ |
| TotalAmount | decimal | — | Không | Tổng tiền cần thanh toán |
| Discount | decimal | — | Không | Giá trị giảm giá |
| PaidAmount | decimal | — | Không | Số tiền đã thanh toán |
| PaymentMethod | string | — | Có | Phương thức thanh toán |
| Status | string | — | Không | Trạng thái đơn hàng |
| Note | string | — | Có | Ghi chú đơn hàng |
| TableName | string | — | Có | Tên bàn phục vụ |
| CreatedBy | string | — | Có | Người tạo đơn hàng |
| BranchId | Guid | FK | Không | Chi nhánh sở hữu đơn hàng |
| BranchName | string | — | Có | Tên chi nhánh |

### Bảng 3.19 — OrderDetail (Word B791)

Nguồn CLR: `services/api/src/Domain/Entities/Order.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính chi tiết đơn hàng |
| OrderId | Guid? (shadow EF) | FK | Có | Đơn hàng chứa chi tiết theo quan hệ EF; shadow property nullable trong snapshot |
| ProductId | Guid | FK | Có | Sản phẩm được chọn |
| ToppingId | Guid | FK | Có | Topping được chọn |
| ProductName | string | — | Không | Tên sản phẩm tại thời điểm đặt |
| Quantity | int | — | Không | Tổng số lượng đặt |
| SentQuantity | int | — | Không | Số lượng đã gửi xuống bếp |
| UnitPrice | decimal | — | Không | Đơn giá được ghi nhận |
| Options | string | — | Có | Tùy chọn của sản phẩm |
| SubTotal | Giá trị tính toán | không map | — | Thành tiền của chi tiết đơn |

### Bảng 3.20 — OrderRequest (Word B796)

Nguồn CLR: `services/api/src/Domain/Entities/OrderRequest.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính yêu cầu chế biến |
| OrderId | Guid | FK | Không | Đơn hàng phát sinh yêu cầu |
| RequestNumber | int | — | Không | Số thứ tự đợt gửi món |
| Status | string | — | Không | Trạng thái yêu cầu chế biến |
| CreatedAt | DateTime | — | Không | Thời điểm tạo yêu cầu |
| AcceptedAt | DateTime | — | Có | Thời điểm tiếp nhận yêu cầu |
| PreparingAt | DateTime | — | Có | Thời điểm bắt đầu chế biến |
| CompletedAt | DateTime | — | Có | Thời điểm hoàn thành chế biến |
| ProcessedBy | string | — | Có | Nhân sự xử lý yêu cầu |
| Note | string | — | Có | Ghi chú của yêu cầu |

### Bảng 3.21 — OrderRequestItem (Word B800)

Nguồn CLR: `services/api/src/Domain/Entities/OrderRequest.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính chi tiết yêu cầu |
| OrderRequestId | Guid | FK | Không | Yêu cầu chế biến chứa món |
| ProductId | Guid | — | Có | Mã sản phẩm liên quan |
| ProductName | string | — | Không | Tên món cần chế biến |
| Quantity | int | — | Không | Số lượng món cần chế biến |
| Note | string | — | Có | Ghi chú chế biến |
| Options | string | — | Có | Thông tin tùy chọn của món |

### Bảng 3.22 — LoyaltyTransaction (Word B805)

Nguồn CLR: `services/api/src/Domain/Entities/LoyaltyTransaction.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính giao dịch điểm |
| CustomerId | Guid | FK | Không | Khách hàng phát sinh giao dịch |
| OrderId | Guid | FK, UQ* | Có | Đơn hàng liên quan đến giao dịch |
| Type | string | UQ* | Không | Loại giao dịch điểm |
| Points | int | — | Không | Số điểm phát sinh |
| BalanceAfter | int | — | Không | Số dư điểm sau giao dịch |
| Description | string | — | Có | Nội dung mô tả giao dịch |
| CreatedAt | DateTime | — | Không | Thời điểm phát sinh giao dịch |

### Bảng 3.23 — Reservation (Word B810)

Nguồn CLR: `services/api/src/Domain/Entities/Reservation.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính yêu cầu đặt bàn |
| CustomerName | string | — | Có | Họ tên khách đặt bàn |
| CustomerPhone | string | — | Có | Số điện thoại liên hệ |
| ReservationTime | DateTime | — | Không | Thời gian khách dự kiến đến |
| NumberOfGuests | int | — | Không | Số lượng khách đặt bàn |
| BranchId | Guid | FK | Không | Chi nhánh tiếp nhận đặt bàn |
| BranchName | string | — | Có | Tên chi nhánh |
| Note | string | — | Có | Ghi chú của khách hoặc nhân viên |
| Status | string | — | Không | Trạng thái yêu cầu đặt bàn |
| CreatedAt | DateTime | — | Không | Thời điểm tạo yêu cầu |
| TableId | Guid | FK | Có | Bàn được phân công |
| TableName | string | — | Có | Tên bàn được phân công |

### Bảng 3.24 — Product (Word B814)

Nguồn CLR: `services/api/src/Domain/Entities/Product.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính sản phẩm |
| Code | string | — | Có | Mã sản phẩm |
| Name | string | — | Có | Tên sản phẩm |
| Category | string | — | Có | Danh mục sản phẩm |
| Group | string | — | Có | Nhóm sản phẩm |
| Price | decimal | — | Không | Giá bán sản phẩm |
| CostPrice | decimal | — | Không | Giá vốn sản phẩm |
| ImageUrl | string | — | Có | Đường dẫn hình ảnh sản phẩm |
| SizesJson | string | — | Có | Dữ liệu cấu hình kích thước |
| ToppingsJson | string | — | Có | Dữ liệu cấu hình topping |
| IsActive | bool | — | Không | Trạng thái hoạt động |
| AvailabilityStatus | string | — | Không | Tình trạng cung cấp sản phẩm |
| CreatedAt | DateTime | — | Không | Thời điểm tạo sản phẩm |

### Bảng 3.25 — Topping (Word B818)

Nguồn CLR: `services/api/src/Domain/Entities/Topping.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính topping |
| Name | string | — | Không | Tên topping |
| Price | decimal | — | Không | Giá bán topping |
| CostPrice | decimal | — | Không | Giá vốn topping |
| ToppingType | string | — | Có | Loại topping |
| Category | string | — | Có | Danh mục topping |
| Description | string | — | Có | Mô tả topping |
| ImageUrl | string | — | Có | Đường dẫn hình ảnh |
| IsActive | bool | — | Không | Trạng thái hoạt động |
| CreatedAt | DateTime | — | Không | Thời điểm tạo topping |

### Bảng 3.26 — Promotion (Word B822)

Nguồn CLR: `services/api/src/Domain/Entities/Promotion.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính chương trình khuyến mại |
| Name | string | — | Không | Tên chương trình khuyến mại |
| Description | string | — | Có | Nội dung mô tả chương trình |
| RequiredPoints | int | — | Không | Số điểm yêu cầu để sử dụng ưu đãi |
| DiscountValue | decimal | — | Không | Giá trị giảm giá |
| PromotionType | string | — | Không | Loại chương trình khuyến mại |
| IsActive | bool | — | Không | Trạng thái hoạt động |
| EndDate | DateTime | — | Có | Thời điểm kết thúc chương trình |
| CreatedAt | DateTime | — | Không | Thời điểm tạo chương trình |

### Bảng 3.27 — InventoryItem (Word B827)

Nguồn CLR: `services/api/src/Domain/Entities/InventoryItem.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính mặt hàng kho |
| Name | string | — | Không | Tên nguyên liệu hoặc vật tư |
| NormalizedName | string | UQ* | Không | Tên mặt hàng đã được chuẩn hóa |
| UnitCode | string | — | Không | Mã đơn vị tính |
| IsActive | bool | — | Không | Trạng thái hoạt động của mặt hàng |
| CreatedAtUtc | DateTime | — | Không | Thời điểm tạo mặt hàng |
| UpdatedAtUtc | DateTime | — | Có | Thời điểm cập nhật gần nhất |

### Bảng 3.28 — BranchInventory (Word B832)

Nguồn CLR: `services/api/src/Domain/Entities/BranchInventory.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính bản ghi tồn kho |
| BranchId | Guid | FK, UQ* | Không | Chi nhánh quản lý tồn kho |
| InventoryItemId | Guid | FK, UQ* | Không | Mặt hàng được theo dõi |
| CurrentQuantity | decimal | — | Không | Số lượng tồn kho hiện tại |
| MinimumStock | decimal | — | Không | Ngưỡng tồn kho tối thiểu |
| UpdatedAtUtc | DateTime | — | Không | Thời điểm cập nhật tồn kho |

### Bảng 3.29 — StockReceipt (Word B837)

Nguồn CLR: `services/api/src/Domain/Entities/StockReceipt.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính phiếu nhập kho |
| BranchId | Guid | FK, UQ* | Không | Chi nhánh thực hiện nhập kho |
| SupplierName | string | — | Có | Tên nhà cung cấp |
| PurchaseDate | DateTime | — | Có | Ngày mua hàng |
| TotalAmount | decimal | — | Không | Tổng giá trị hàng nhập |
| Status | StockDocumentStatus | — | Không | Trạng thái phiếu nhập kho |
| Note | string | — | Có | Ghi chú của phiếu nhập |
| CreatedBy | Guid | — | Không | Định danh người tạo phiếu |
| CreatedAtUtc | DateTime | — | Không | Thời điểm tạo phiếu |
| ConfirmedBy | Guid | — | Có | Định danh người xác nhận |
| ConfirmedAtUtc | DateTime | — | Có | Thời điểm xác nhận |
| IdempotencyKey | string | UQ* | Có | Khóa hỗ trợ ngăn xử lý chứng từ trùng lặp |

### Bảng 3.30 — StockReceiptItem (Word B842)

Nguồn CLR: `services/api/src/Domain/Entities/StockReceiptItem.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính chi tiết phiếu nhập |
| StockReceiptId | Guid | FK | Không | Phiếu nhập kho chứa mặt hàng |
| InventoryItemId | Guid | FK | Không | Mặt hàng được nhập kho |
| Quantity | decimal | — | Không | Số lượng mặt hàng nhập kho |
| UnitPrice | decimal | — | Không | Đơn giá nhập của mặt hàng |

### Bảng 3.31 — StockIssue (Word B847)

Nguồn CLR: `services/api/src/Domain/Entities/StockIssue.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính phiếu xuất kho |
| BranchId | Guid | FK, UQ* | Không | Chi nhánh thực hiện xuất kho |
| Reason | string | — | Không | Lý do xuất kho |
| Note | string | — | Có | Ghi chú của phiếu xuất |
| Status | StockDocumentStatus | — | Không | Trạng thái phiếu xuất kho |
| CreatedBy | Guid | — | Không | Định danh người tạo phiếu |
| CreatedAtUtc | DateTime | — | Không | Thời điểm tạo phiếu |
| ConfirmedBy | Guid | — | Có | Định danh người xác nhận |
| ConfirmedAtUtc | DateTime | — | Có | Thời điểm xác nhận |
| IdempotencyKey | string | UQ* | Có | Khóa hỗ trợ ngăn xử lý chứng từ trùng lặp |

### Bảng 3.32 — StockIssueItem (Word B852)

Nguồn CLR: `services/api/src/Domain/Entities/StockIssueItem.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính chi tiết phiếu xuất |
| StockIssueId | Guid | FK | Không | Phiếu xuất kho chứa mặt hàng |
| InventoryItemId | Guid | FK | Không | Mặt hàng được xuất kho |
| Quantity | decimal | — | Không | Số lượng mặt hàng xuất kho |

### Bảng 3.33 — StockTransaction (Word B857)

Nguồn CLR: `services/api/src/Domain/Entities/StockTransaction.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính giao dịch kho |
| BranchId | Guid | FK | Không | Chi nhánh phát sinh giao dịch |
| InventoryItemId | Guid | FK | Không | Mặt hàng phát sinh biến động |
| Type | StockTransactionType | — | Không | Loại giao dịch kho |
| Quantity | decimal | — | Không | Số lượng biến động |
| BeforeQuantity | decimal | — | Không | Số lượng tồn trước giao dịch |
| AfterQuantity | decimal | — | Không | Số lượng tồn sau giao dịch |
| ReferenceType | string | — | Không | Loại chứng từ phát sinh giao dịch |
| ReferenceId | Guid | — | Không | Định danh chứng từ nguồn |
| CreatedBy | Guid | — | Không | Định danh người thực hiện |
| CreatedAtUtc | DateTime | — | Không | Thời điểm phát sinh giao dịch |
| Note | string | — | Có | Ghi chú của giao dịch |

### Bảng 3.34 — Expense (Word B862)

Nguồn CLR: `services/api/src/Domain/Entities/Expense.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính khoản chi phí |
| BranchId | Guid | FK | Không | Chi nhánh phát sinh chi phí |
| Description | string | — | Không | Nội dung khoản chi |
| Amount | decimal | — | Không | Số tiền chi phí |
| ExpenseDate | DateTime | — | Không | Ngày phát sinh chi phí |
| Category | string | — | Không | Nhóm chi phí |
| PaymentMethod | string | — | Không | Phương thức thanh toán |
| Note | string | — | Không | Ghi chú khoản chi |
| CreatedBy | Guid | — | Có | Định danh người tạo khoản chi |
| StockReceiptId | Guid | FK, UQ* | Có | Phiếu nhập kho liên quan |
| CreatedAt | DateTime | — | Không | Thời điểm tạo bản ghi |
| UpdatedAt | DateTime | — | Có | Thời điểm cập nhật gần nhất |

### Bảng 3.35 — BusinessInsight (Word B867)

Nguồn CLR: `services/api/src/Domain/Entities/BusinessInsight.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính bản ghi phân tích |
| BranchId | Guid | FK | Có | Chi nhánh liên quan đến kết quả phân tích |
| Type | string | — | Không | Loại nhận định kinh doanh |
| Severity | string | — | Không | Mức độ quan trọng của nhận định |
| Title | string | — | Không | Tiêu đề nhận định |
| Summary | string | — | Không | Nội dung tóm tắt kết quả phân tích |
| AiExplanation | string | — | Có | Nội dung giải thích được AI hỗ trợ tạo |
| AiRecommendation | string | — | Có | Khuyến nghị được AI hỗ trợ tạo |
| EvidenceJson | string | — | Không | Dữ liệu làm căn cứ cho nhận định |
| PeriodStart | DateTime | — | Không | Thời điểm bắt đầu kỳ phân tích |
| PeriodEnd | DateTime | — | Không | Thời điểm kết thúc kỳ phân tích |
| DetectedAt | DateTime | — | Không | Thời điểm phát hiện nhận định |
| Status | string | — | Không | Trạng thái xử lý |
| DeduplicationKey | string | UQ | Không | Khóa hỗ trợ ngăn tạo nhận định trùng lặp |
| CreatedAt | DateTime | — | Không | Thời điểm tạo bản ghi |
| ComparisonPeriodStart | DateTime? | — | Có | Bắt đầu kỳ so sánh |
| ComparisonPeriodEnd | DateTime? | — | Có | Kết thúc kỳ so sánh |

### Bảng 3.36 — ReceiptSettings (Word B872)

Nguồn CLR: `services/api/src/Domain/Entities/ReceiptSettings.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính cấu hình hóa đơn |
| BranchId | Guid | FK, UQ* | Có | Chi nhánh áp dụng cấu hình |
| PaperWidth | int | — | Không | Khổ giấy in hóa đơn |
| ShowLogo | bool | — | Không | Cho phép hiển thị logo trên hóa đơn |
| ThankYouText | string | — | Không | Nội dung lời cảm ơn |
| FontFamily | string | — | Không | Phông chữ được sử dụng |
| FontSize | int | — | Không | Kích thước chữ |
| UpdatedAt | DateTime | — | Không | Thời điểm cập nhật cấu hình |
| ShowAddress | bool | — | Không | Hiện địa chỉ |
| ShowPhone | bool | — | Không | Hiện số điện thoại |
| ShowStaff | bool | — | Không | Hiện nhân viên |
| ShowPaymentMethod | bool | — | Không | Hiện phương thức thanh toán |
| ShowOrderNote | bool | — | Không | Hiện ghi chú đơn |
| ShowThankYou | bool | — | Không | Hiện lời cảm ơn |

### Bảng 3.37 — SystemSetting (Word B877)

Nguồn CLR: `services/api/src/Domain/Entities/SystemSetting.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính cấu hình |
| BranchId | Guid | FK, UQ* | Có | Chi nhánh áp dụng cấu hình |
| Key | string | UQ* | Không | Tên hoặc mã tham số cấu hình |
| Value | string | — | Không | Giá trị cấu hình |
| Type | string | — | Không | Kiểu dữ liệu của tham số |
| UpdatedAt | DateTime | — | Không | Thời điểm cập nhật gần nhất |

### Bảng 3.38 — Notification (Word B882)

Nguồn CLR: `services/api/src/Domain/Entities/Notification.cs`; EF: `services/api/src/Infrastructure/Persistence/ApplicationDbContext.cs` (xem mục 4).

| Thuộc tính | Kiểu dữ liệu | Khóa | Null | Ý nghĩa |
|---|---|---|---|---|
| Id | Guid | PK | Không | Khóa chính thông báo |
| Type | string | — | Không | Loại thông báo |
| Title | string | — | Không | Tiêu đề thông báo |
| Message | string | — | Không | Nội dung thông báo |
| EntityType | string | — | Có | Loại đối tượng phát sinh thông báo |
| EntityId | Guid | — | Có | Định danh đối tượng liên quan |
| Route | string | — | Có | Đường dẫn điều hướng khi mở thông báo |
| TargetRole | string | — | Có | Vai trò người nhận |
| TargetUserId | Guid | — | Có | Định danh người dùng nhận thông báo |
| BranchId | Guid | — | Có | Phạm vi chi nhánh của thông báo |
| IsRead | bool | — | Không | Trạng thái đã đọc thông báo |
| CreatedAt | DateTime | — | Không | Thời điểm tạo thông báo |
| UseLegacyReadState | bool | — | Không | Cờ tương thích trạng thái đọc thông báo cũ |

## 4. Đối chiếu mapping, quan hệ và sửa sai theo mục

| Vị trí/trích gốc | Nhãn | Kết luận chính xác và bằng chứng | Mức |
|---|---|---|---|
| B740–748/Hình 3.14: “ERD thể hiện khóa chính, khóa ngoại”; `Notification.EntityId` tham chiếu logic | GIỮ NGUYÊN, BỔ SUNG | `ApplicationDbContext.cs:20-45` có DbSet các nhóm; `Notification.cs:11-16` có `EntityId`/`TargetUserId`/`BranchId` không có navigation hay `HasForeignKey` tương ứng trong mapping `:215-225`. Không đổi nhãn logic thành FK. `StockTransaction.ReferenceId` và `CreatedBy` cũng là định danh logic theo entity/mapping `:360-382`. | Tĩnh EF; chưa DB |
| B750–751: “29 nhóm bảng chính… EF Core và migration hiện hành” | SỬA SAI | Đây là **29 nhóm được Word chọn mô tả**, không phải đếm DB triển khai. DbContext còn `ProductBranchAvailability`, `NotificationRead`, `ManualExpenseIdempotencyRecord` (`ApplicationDbContext.cs:17-45,117-131,217-225,398-407`); Word không liệt kê chúng. Câu nói migration hiện hành chỉ là mã lịch sử/snapshot, chưa kiểm schema DB thật. | Tĩnh CLR/EF/snapshot |
| B766/Bảng 3.13: `EmployeeType`, `BasicSalary` | GIỮ NGUYÊN, BỔ SUNG | Cả hai đang ở CLR (`Employee.cs:10,15`), được Employee API đọc/ghi (`EmployeeController.cs:78-83,235-240`) và snapshot (`ApplicationDbContextModelSnapshot.cs:328,352` theo kiểm tra cũ; cần đối chiếu exact line khi trích). Không suy ra module tính lương; nếu Chương 4 nói không dùng các trường, cần sửa mâu thuẫn theo hiện trạng. | Tĩnh CLR/API/EF; chưa DB |
| B791–793/Bảng 3.19: `OrderId` “FK*”, ghi `Guid`, Null “Không”; `SubTotal` “giá trị tính toán” | SỬA SAI một ô, GIỮ NGUYÊN ghi chú | `Order.cs:40-51` không khai báo `OrderId`; `ApplicationDbContext.cs:79-83` cấu hình shadow FK, nhưng snapshot `ApplicationDbContextModelSnapshot.cs:721-760` ghi `Property<Guid?>("OrderId")`. Vì vậy bảng audit trên sửa `Guid?`, Null **Có** ở mức EF snapshot. `SubTotal` chỉ getter `Quantity*UnitPrice`, snapshot không liệt kê cột. | Tĩnh CLR/EF/snapshot; chưa DB |
| B800–802/Bảng 3.21: `OrderRequestItem.ProductId` không đánh FK | GIỮ NGUYÊN | `OrderRequest.cs:29` có Guid? ProductId; mapping chỉ nối `OrderRequestId` (`ApplicationDbContext.cs:98-102`), không thấy Product FK. Không suy ProductId là FK vật lý. | Tĩnh EF |
| B805–807/Bảng 3.22: `(OrderId, Type)` unique có điều kiện | GIỮ NGUYÊN | `ApplicationDbContext.cs:253-255` đúng unique composite filter `"OrderId" IS NOT NULL`; không phải hai unique riêng. | Tĩnh EF |
| B827–859/Bảng 3.27–3.33: nhóm kho và các ràng buộc | GIỮ NGUYÊN, BỔ SUNG | Entity/DbSet và mapping đầy đủ; precision kho, check constraints, unique/filter, FK/delete ở `ApplicationDbContext.cs:269-382`. `StockTransaction.ReferenceId` chỉ tham chiếu logic, không FK tới nhiều loại chứng từ. Chức năng kho đang có API/service/UI, không tự loại khỏi báo cáo. | Tĩnh CLR/EF/service |
| B862–864/Bảng 3.34: `Expense.StockReceiptId` FK và unique có điều kiện | GIỮ NGUYÊN | `Expense.cs:19,23`; `ApplicationDbContext.cs:385-395` cấu hình quan hệ đến StockReceipt, `Restrict`, unique filter `StockReceiptId IS NOT NULL`. Không nhầm với `StockTransaction.ReferenceId`. | Tĩnh EF |
| B867–869/Bảng 3.35: `PeriodStart`, `PeriodEnd` “Chưa xác minh”; thiếu kỳ so sánh | SỬA SAI, BỔ SUNG | `BusinessInsight.cs:17-20`: hai kỳ phân tích `DateTime` **không nullable**; `ComparisonPeriodStart/End` là `DateTime?` **nullable**. Snapshot `ApplicationDbContextModelSnapshot.cs:200-223` phản ánh các thuộc tính. Đã sửa năm-cột audit phía trên, bổ sung hai trường so sánh. | Tĩnh CLR/snapshot; chưa DB |
| B872–874/Bảng 3.36: “chưa cung cấp đầy đủ tên… cờ cấu hình” | SỬA SAI | `ReceiptSettings.cs:10-21` cho tên đầy đủ: `ShowLogo`, `ShowAddress`, `ShowPhone`, `ShowStaff`, `ShowPaymentMethod`, `ShowOrderNote`, `ShowThankYou`, `ThankYouText`, `FontFamily`, `FontSize`, `PaperWidth`, `UpdatedAt`. Bảng audit đã bổ sung 6 cờ bị thiếu. `ApplicationDbContext.cs:226-229` unique BranchId khi khác null. Không viết lại “chưa xác minh”. | Tĩnh CLR/EF |
| B877–879/Bảng 3.37: `(BranchId,Key)` unique có điều kiện | GIỮ NGUYÊN, BỔ SUNG | `ApplicationDbContext.cs:230-233` có composite unique filter `BranchId IS NOT NULL`; `SystemSetting.cs:8-13` cho BranchId nullable. Điều này **không chứng minh** tự fallback global; `SystemSettingService` hiện lấy theo branch. | Tĩnh EF/service |
| B882–884/Bảng 3.38: Notification theo role/user/branch, `EntityId` logic | GIỮ NGUYÊN, BỔ SUNG | `Notification.cs:7-23` còn `UseLegacyReadState` mà Word thiếu; `ApplicationDbContext.cs:217-225` có `NotificationRead` khóa kép `(NotificationId,UserId)` hỗ trợ đọc riêng. `NotificationRecipientRouting.cs:14-60` định tuyến user **hoặc** role/branch, từ chối scope rỗng/nhập nhằng. | Tĩnh CLR/EF/service |
| B885–899: ràng buộc QR, kho, Loyalty, insight | GIỮ NGUYÊN, BỔ SUNG | `QrToken` unique `ApplicationDbContext.cs:155-161`; tồn branch+item unique/quantity check `:281-293`; receipt/issue key `(BranchId,IdempotencyKey)` unique filter `:296-345`; Loyalty `(OrderId,Type)` filter `:253-255`; insight dedup unique `:52-65`. Đây là model EF, không xác nhận đã áp DB. | Tĩnh EF |
| B900–906: đa chi nhánh trực tiếp/gián tiếp; Product/Topping/Customer không BranchId | GIỮ NGUYÊN, BỔ SUNG | Đúng ở mức entity. `ProductBranchAvailability` thêm trạng thái khả dụng theo branch (không làm Product có `BranchId`), cấu hình `ApplicationDbContext.cs:117-131`; scope controller/service vẫn là nguồn quyền. | Tĩnh CLR/EF |
| B909–919/Hình 3.15: “Backend kết nối với dịch vụ Gemini”, “Gemini… bên ngoài” | SỬA SAI | `Program.cs:304` DI `IAiProvider→MockAiProvider`; không có bằng chứng call Gemini trong đường hiện tại. Sơ đồ/thuyết minh nghiệp vụ cần đặt provider mô phỏng ở boundary, embedding chỉ mục tiêu. | Tĩnh DI |
| B920–929: Domain/Application/Infrastructure/WebAPI/AI “phân lớp logic trong một dự án backend” | GIỮ NGUYÊN, BỔ SUNG | Đúng trọng tâm: `services/api/RestaurantPOS.Api.csproj` là project backend; `Application` có chỗ tham chiếu trực tiếp `Infrastructure` DbContext (ví dụ `OrderService.cs:using RestaurantPOS.Infrastructure...`), nên nói “vận dụng nguyên tắc Clean Architecture”, không khẳng định các assembly độc lập/đảo phụ thuộc chặt. | Tĩnh project/import |
| B930–936: hai React/TypeScript/Vite, POS/KDS ở Admin Web | GIỮ NGUYÊN | Routes `apps/admin-web/src/app/routes/ManagementRoutes.tsx:35-77`, `StaffRoutes.tsx:21-35`, `CashierRoutes.tsx:22-37`, `KitchenRoutes.tsx:20-33`; Customer routes `apps/customer-web/src/App.tsx:187-195`. Frontend role guard là trải nghiệm, backend vẫn kiểm quyền. | Tĩnh route |
| B937–945/Hình 3.16: `NewOrderRequest`, `RequestStatusUpdated`, REST+Hub+polling; “PaymentCompleted broadcast tới toàn bộ kết nối” | SỬA SAI một câu, GIỮ NGUYÊN phần còn lại | `OrderController.cs:420-430` gửi `PaymentCompleted` qua `Clients.Groups(PaymentEventRouting.Groups(branchId))`, **không** `Clients.All`. `PaymentEventRouting.cs:8-19` trả group `kitchen-admin` và branch-role manager/employee/cashier. Chưa thấy consumer chuỗi `PaymentCompleted` khi tìm `services/api/src` và `apps/*/src`; không khẳng định UI nhận/giao runtime. Group admin global là có chủ ý; không gọi mọi sự kiện chỉ trong branch. | Tĩnh producer/group/search consumer |

### Quan hệ, khóa và precision cần ghi đúng

| Nhóm | Mapping hiện hành | Giới hạn |
|---|---|---|
| Tổ chức/đơn | Area→Branch `NoAction` (`ApplicationDbContext.cs:69-76`); Table→Branch/Area `NoAction` (`:141-154`); Order→Branch `NoAction`, Order→OrderDetail shadow FK `Cascade` (`:79-89`); OrderRequest→Order và Request→Items (`:92-102`); OrderDetail→Product/Topping optional (`:105-114`); Order→Customer optional (`:260-264`). | Không suy delete behavior không cấu hình là cùng một kiểu; snapshot là bằng chứng EF được sinh, không phải DB runtime. |
| Nhân sự/đặt bàn | Employee→Branch (`:135-138`), Attendance→Employee/Branch (`:164-171`), Schedule→Employee/Branch (`:174-181`); Shift→Employee/Branch `ClientSetNull` (`:183-192`); Reservation→Branch `NoAction`, →Table `ClientSetNull` (`:194-205`). | `Shift.BranchId`/`EmployeeId` CLR Guid không nullable dù delete behavior ClientSetNull; không tự diễn giải xóa parent sẽ luôn hợp lệ. |
| Kho | BranchInventory `(BranchId,InventoryItemId)` unique; `CurrentQuantity/MinimumStock` precision `(18,3)`; receipt total & item unit price `(18,2)`, quantities `(18,3)`; FK kho dùng `Restrict` cho quan hệ nêu tại `:269-382`. Status receipt/issue enum lưu string, transaction type lưu string. | Check constraints EF; chưa chứng minh migration áp dụng. `ReferenceId`, `CreatedBy`, `ConfirmedBy` không được cấu hình FK chỉ vì tên. |
| Cấu hình/điểm/notification | ReceiptSettings BranchId unique khi not null; SystemSetting `(BranchId,Key)` unique khi BranchId not null; Customer PhoneNumber unique; Loyalty `(OrderId,Type)` unique khi OrderId not null; NotificationRead composite PK và FK cascade. | Với `BranchId=null`, unique filter này không ràng buộc singleton global. Cần service quyết định semantics. |

## 5. Kho: dữ kiện theo đường xử lý

| Bước/nhóm | Code và kết luận tĩnh | Không được suy rộng |
|---|---|---|
| Danh mục/tồn | `InventoryService.cs:16-85,167-198` quản lý item, branch inventory và minimum stock; `InventoryController.cs:17-40` item write A/M, đọc tồn cho base role A/M/E/C/K và service scope branch. | Không đồng nhất item kho với Product/món; chưa thấy định mức ProductIngredient hiện hành. |
| Phiếu nhập | `InventoryService.cs:219-261` tạo/sửa Draft và idempotency key theo branch; `:263-323` confirm trong transaction `ReadCommitted`, khóa dòng tồn, cộng quantity, tạo `StockTransaction(IN)` và `Expense` gắn `StockReceiptId`, đổi status Confirmed. | Không gọi tạo phiếu là nhập tồn; chỉ confirm thay đổi. `Expense` phát sinh từ confirm, không phải webhook/thanh toán tự động. |
| Phiếu xuất | `InventoryService.cs:356-396` tạo/sửa Draft; `:397-448` confirm transaction, kiểm đủ tồn, khóa dòng tồn, trừ quantity, tạo `StockTransaction(OUT)`, đổi status; `:449-459` cancel theo điều kiện. | Không khẳng định âm kho không thể xảy ra ở mọi môi trường chỉ từ mã; trên PostgreSQL còn check constraint/row lock. Không suy tự trừ khi Order tạo/paid. |
| Chống lặp/cạnh tranh | `ApplicationDbContext.cs:309-311,340-343` unique `(BranchId,IdempotencyKey)` khi not null; `InventoryService.cs:271,404` transaction `ReadCommitted`, `:587-621` với PostgreSQL `INSERT...ON CONFLICT DO NOTHING` và `SELECT...FOR UPDATE` trên từng tồn theo thứ tự ID. | Là cơ chế tĩnh có chủ đích; chưa có test đồng thời/runtime được chạy trong lượt này. |
| Tham chiếu chi phí | `Expense.StockReceiptId` có FK/unique-filter (`ApplicationDbContext.cs:385-395`); service kiểm Expense đã có trước tạo (`InventoryService.cs:298-309`). `StockTransaction.ReferenceType/ReferenceId` chỉ logic (`:360-382`). | Không gọi mọi hậu tố Id là FK. |
| Trừ nguyên liệu theo món | Tìm `ProductIngredient`/recipe/deduction liên kết `OrderService` và `InventoryService` ở `services/api/src`: không có đường gọi tự động từ tạo/gửi bếp/paid Order sang xuất kho; tồn biến động qua xác nhận chứng từ. | Ghi “chưa cài đặt tự trừ theo định mức món”, không loại module kho. |

## 6. Kiến trúc và giao tiếp realtime

Đường thực tế: Admin Web (`apps/admin-web`) và Customer Web (`apps/customer-web`) dùng REST của một backend ASP.NET Core; `Program.cs:368-369` cấu hình EF/Npgsql, `:593` map `/kitchenHub`. Backend chia Domain/Application/Infrastructure/WebAPI/AI theo namespace/thư mục trong một project, không là microservice/assembly độc lập. `Application` gọi DbContext Infrastructure ở một số service, nên chỉ nhận định kiến trúc phân lớp logic. Customer Web có ChatBot UI nhưng tìm `@microsoft/signalr`, `HubConnectionBuilder`, `/kitchenHub` trong `apps/customer-web/src` và manifest không thấy consumer Hub; không suy Customer Web nhận sự kiện realtime. Frontend Admin Web có kết nối Hub ở POS, KDS và notification center, reconnect thì tải lại ở `POSPage.tsx:783-836`, `KitchenPage.tsx:96-147`, `useNotificationCenter.ts:118-151`.

| Event | Producer, điều kiện và nhóm nhận | Consumer thấy trong checkout, tác động UI | Chưa xác minh |
|---|---|---|---|
| `NewOrderRequest` | `KitchenService.cs:88-100` sau lưu request gọi `KitchenNotifier.cs:18-24`; groups `kitchen-admin` + `kitchen-branch:{branchId}` nếu branch hợp lệ; overload thiếu branch chỉ admin. | `KitchenPage.tsx:115-118,145` nhận để refresh active requests; reconnect refetch. | Giao nhận runtime, thứ tự khi lỗi Hub. |
| `RequestStatusUpdated` | `KitchenService.cs:223-255` sau cập nhật, gọi notifier; `KitchenNotifier.cs:27-38` gửi admin + branch, payload có id/status/branchId trong overload branch. | `KitchenPage.tsx:115-118`; `POSPage.tsx:790-836` cập nhật/tải lại trạng thái bếp. | Không bảo đảm mọi client nhận khi mất kết nối; có refresh/reconnect. |
| `PendingOrderChanged` | `OrderController.cs:308-310,335-338,695-718` sau create web/accept; groups admin + branch-role manager/employee/cashier, payload branchId là tín hiệu invalidation, không chứa count. | `CashierNavbar.tsx:76-100`, `useNotificationCenter.ts:136-151` refresh danh sách/đếm từ API. | Chưa test duplicate/missed event. |
| `PaymentCompleted` | `OrderController.cs:418-433` sau PayOrder result Paid; `PaymentEventRouting.cs:8-19`: admin global `kitchen-admin`, branch-role manager/employee/cashier. **Không `Clients.All`**. | Không tìm thấy `.on('PaymentCompleted')` trong `apps/admin-web/src`, `apps/customer-web/src`, `services/api/src` tại lượt này. | Có phát tĩnh; không có consumer tìm thấy, nên chưa được viết là UI tự cập nhật nhờ event này. |
| `ProductAvailabilityChanged` | `ProductController.cs:195-228` sau đổi availability theo branch, groups admin + branch-role kitchen/manager. | `KitchenPage.tsx:115-118,147` refresh sản phẩm. | Chưa xác nhận UI customer nhận trực tiếp (không thấy Hub consumer). |
| `NotificationCreated` | `NotificationService.cs:70-84` chỉ sau khi `NotificationRecipientRouting.Resolve` có scope; user group hoặc role/branch group; không broadcast rỗng. | `CashierNavbar.tsx:75-116`, `useNotificationCenter.ts:118-151` refresh thông báo. | Không thấy Customer Web Hub consumer; thông báo lưu DB nhưng giao nhận realtime không kiểm runtime. |

`KitchenHub.cs:10-44` chỉ A/M/E/C/K kết nối, admin vào `kitchen-admin`; mọi role vào `role:{role}` và có thể user group; staff branch từ JWT claim, admin có thể chọn branch query. Admin global nhận nhiều event; branch group không có nghĩa mọi sự kiện chỉ một chi nhánh. `Program.cs:593` route Hub. Với completion đợt KDS, `KitchenService.cs:190-205` conditional update `Status != Completed`, chỉ khi affected=1 mới tạo notification `ORDER_REQUEST_COMPLETED` tại `:257-287`. Thông báo có `EntityType=OrderRequest`, `EntityId=requestId`, `Route=/staff`, `TargetRole=employee`, `BranchId=order.BranchId`; `ProcessedBy` là chuỗi hiển thị, không `TargetUserId`. Một đợt hoàn tất không suy cả Order hoàn tất.

## 7. AI: hiện trạng / mục tiêu / chưa chốt ở phần kiến trúc

| HIỆN TRẠNG | THIẾT KẾ MỤC TIÊU | CHƯA CHỐT |
|---|---|---|
| Backend `Program.cs:304` DI MockAiProvider; `:313-348` orchestrator và 14 tool registrations. Không có Gemini trong đường DI. | Mock kết hợp sentence embedding local để hỗ trợ nhận diện tool; chỉ đưa vào sơ đồ như mục tiêu khi ghi rõ trạng thái. | Model/tokenizer/metric/ngưỡng/tích hợp production chưa được chứng minh. Chi tiết ở lượt C. |

## 8. Phát biểu không được giữ, câu hỏi và handoff

- Không giữ B914/B919 “Gemini service bên ngoài” như hiện trạng; không giữ B945 “PaymentCompleted broadcast tới toàn bộ kết nối”. Không giữ B869 “chưa xác minh” kỳ phân tích hay B874 “chưa biết tên” receipt flags. B793 phải sửa nullable shadow `OrderId` theo snapshot.
- Cần người viết xác nhận có muốn thêm một câu về ba entity phụ `ProductBranchAvailability`, `NotificationRead`, `ManualExpenseIdempotencyRecord` ngoài 29 nhóm Word không; không cần phá dàn ý/bảng để giải thích DB không giới hạn 29 bảng.
- ChatGPT có đủ bảng 3.10–3.38 năm cột và sửa cụ thể, kiến trúc, kho, realtime để chỉnh các đoạn B738–945. Phần tài chính B946–1001, phân quyền và AI B1002–1099 thuộc lượt C. Không dùng file này để suy DB đã triển khai hoặc kết quả runtime.
