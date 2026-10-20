# Restaurant POS System (KiotViet Simplified Clone)

Hệ thống quản lý bán hàng nhà hàng tập trung vào tính đơn giản và chính xác trong doanh thu - chi phí.

## Phạm vi dự án
- **Tập trung**: Quản lý bán hàng đa chi nhánh, quản lý doanh thu, nhân sự, và theo dõi chi tiêu của chủ quán (Expenses). Nhân sự hiện tập trung vào hồ sơ, ca, lịch và chấm công.
- **Tính năng nổi bật**: Tích hợp Kitchen Display System (KDS) qua SignalR và Trợ lý ảo Mock AI hỗ trợ điều hành trong môi trường local.

## Cấu trúc thư mục backend phân lớp
- `apps/`: Chứa các ứng dụng frontend (React)
  - `admin-web`: Tích hợp Trang quản trị (Admin) và Giao diện bán hàng (POS). Quản lý chi nhánh, nhân sự, doanh thu và chi tiêu.
  - `customer-web`: Giao diện dành cho khách hàng - Đặt món tại bàn qua mã QR và theo dõi tích điểm.
- `services/`: Chứa mã nguồn backend (ASP.NET Core 8)
  - `api/src/`: Hệ thống API chính
    - `Domain`: Thực thể (Product, Order, Employee...) và các khái niệm nghiệp vụ.
    - `Application`: Logic xử lý nghiệp vụ, DTOs, Services.
    - `Infrastructure`: Persistence (EF Core), dịch vụ hạ tầng và JWT service.
    - `WebAPI`: Controllers, Middleware, Auth config.
    - `AI`: Trợ lý ảo AI, Tool Registry và Permission Service.
- `services/api/src/Infrastructure/Persistence/Migrations/`: EF Core migrations và model snapshot.
- `bao-cao/`: Kho tài liệu báo cáo, kỹ thuật, hình ảnh và minh chứng; xem [`bao-cao/README.md`](bao-cao/README.md).
- `scripts/`: Công cụ HTTPS, OpenAPI và diagnostics có phạm vi rõ ràng; xem [`scripts/README.md`](scripts/README.md).

## Tài liệu chuẩn

`bao-cao/02-tai-lieu-ky-thuat/system-audit/` là source of truth được kiểm tra theo source code hiện tại. Bắt đầu tại [`bao-cao/02-tai-lieu-ky-thuat/system-audit/00-README.md`](bao-cao/02-tai-lieu-ky-thuat/system-audit/00-README.md). Bộ `bao-cao/01-khoa-luan/` là tài liệu bổ trợ cho báo cáo/khóa luận và có thể chứa nội dung lịch sử; khi có mâu thuẫn, bộ system audit và source code được ưu tiên.

## Phân quyền & Bảo mật (RBAC)

Hệ thống triển khai mô hình phân quyền dựa trên vai trò (Role-Based Access Control) với cơ chế cách ly chi nhánh (Branch Isolation) nghiêm ngặt:

| Role | Phạm vi (Scope) | Mô tả |
| :--- | :--- | :--- |
| **admin** | GLOBAL | Toàn quyền hệ thống, quản lý nhiều chi nhánh. |
| **manager** | OWN_BRANCH | Quản lý nghiệp vụ trong phạm vi chi nhánh được gán (Active role). |
| **cashier** | OWN_BRANCH | Thu ngân, quản lý đơn hàng và thanh toán tại chi nhánh. |
| **kitchen** | OWN_BRANCH | Tiếp nhận và chế biến món ăn tại chi nhánh. |
| **employee** | OWN_BRANCH | Ghi món, phục vụ và quản lý lịch cá nhân. |
| **customer** | OWN_DATA | Khách hàng đặt món và xem thông tin cá nhân qua QR. |

**Nguyên tắc vàng**:
- **Role** là nguồn định danh duy nhất cho việc phân quyền (Authorization).
- **Position** (Chức danh) chỉ dùng để hiển thị và mô tả nghiệp vụ.
- Dữ liệu chi nhánh được cách ly tại Server thông qua JWT `branchId`.

## Chạy local HTTP/HTTPS

- Chuẩn bị chứng chỉ bằng `.\scripts\setup-https.ps1` trong PowerShell; script dùng một cặp PEM multi-LAN trong `certs/`.
- Chạy API tại `https://localhost:5000`, Admin Web tại `https://localhost:5173` và Customer Web tại `https://localhost:5174`. API và SignalR đều dùng cùng base URL API; SignalR chạy tại `/kitchenHub`.
- LAN là chế độ opt-in: đặt `DOAN_DEV_LAN_HOST` đúng hostname/IP có trong SAN của `certs/doan-pos-lan.pem`, chạy lại script setup rồi khởi động các app trong cùng session. Không dùng IP cũ hardcode trong cấu hình.
- Production không dùng chứng chỉ tự ký này; TLS phải do hosting hoặc reverse proxy production cung cấp. `VITE_HTTPS=false` chỉ là override local có chủ đích và không thay đổi API HTTPS.

## Kiểm thử hiện hành

Repository dùng build/typecheck và OpenAPI codegen để kiểm tra tính hợp lệ của source/config. Bộ test tự động xUnit, Node source-contract và Playwright đã được gỡ theo quyết định chuyển sang kiểm thử thủ công; checklist hiện hành nằm tại [`ba-test-frontend/README.md`](ba-test-frontend/README.md). Không chạy `dotnet test`, Playwright hoặc Node test runner sau khi đợt dọn này hoàn tất.
## Multi-LAN HTTPS development

Dung mot PEM certificate duy nhat trong `certs/`. Danh sach IPv4 LAN da dang ky duoc luu trong `certs/lan-hosts.txt`.

```powershell
$lanHost = '172.18.41.58'
. .\scripts\setup-https.ps1 -LanHost $lanHost
```

`setup-https.ps1` duoc dot-source trong terminal API. IP moi se duoc them vao danh sach va cap lai leaf certificate bao gom toan bo IP da dang ky; IP da dang ky se duoc tai su dung neu certificate con hop le.

Sau khi doi Wi-Fi/IP: chay lai launcher voi host moi, trust public root CA tren dien thoai neu thiet bi chua trust, roi tao/in lai QR vi QR cu van chua host cu. Khong bao gio chuyen private key server hoac CA private key sang dien thoai. Dung cung CA thi cap leaf moi khong tu dong doi yeu cau trust CA; neu CA doi/het han hoac chinh sach mobile doi thi phai trust lai theo chinh sach thiet bi.

Firewall, client isolation, VPN va routing la dieu kien mang rieng; cung Wi-Fi khong dam bao dien thoai truy cap duoc may tinh. API/Admin/Customer van giu ba cong 5000/5173/5174 va khong dung CORS wildcard.

`StartManualTestBackend.ps1` van la launcher test doc lap co runtime attestation va target database test; khong dung no de khoi dong POS development hang ngay.

Neu `VITE_API_URL`, `VITE_CUSTOMER_WEB_URL` hoac `DOAN_DEV_LAN_HOST` trong shell dang tro toi host cu, xoa cac bien nay truoc khi dot-source setup.

### Chay ba terminal PowerShell rieng cung LAN

`setup-https.ps1` la setup-only: no chi kiem tra/chuan bi certificate va export environment, khong khoi dong ung dung. Trong PowerShell, dung dot-sourcing (`. .\scripts\setup-https.ps1`) de bien moi truong duoc giu trong chinh terminal; goi bang `&` se khong truyen bien nguoc ve parent shell.

Thay gia tri `$lanHost` bang IP LAN hien tai. Chay block 1 truoc mot lan de dang ky/cap lai leaf neu can; chi sau khi block 1 thanh cong moi chay block 2 va 3. Hai block sau dung `-ValidateOnly`, khong ghi lai certificate.

**Terminal 1 — setup va API**

```powershell
Set-Location D:\Dev\doan
$lanHost = '172.18.41.58' # thay bang IP LAN hien tai
try {
    . .\scripts\setup-https.ps1 -LanHost $lanHost
} catch {
    Write-Error $_
    exit 1
}
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:DatabaseSafety__AllowAutomaticMigration = 'false'
dotnet run --project .\services\api\RestaurantPOS.api.csproj --no-launch-profile
```

**Terminal 2 — validate va Admin Web**

```powershell
Set-Location D:\Dev\doan
$lanHost = '172.18.41.58' # phai trung IP o Terminal 1
try {
    . .\scripts\setup-https.ps1 -LanHost $lanHost -ValidateOnly
} catch {
    Write-Error $_
    exit 1
}
npm run dev --prefix .\apps\admin-web
```

**Terminal 3 — validate va Customer Web**

```powershell
Set-Location D:\Dev\doan
$lanHost = '172.18.41.58' # phai trung IP o Terminal 1
try {
    . .\scripts\setup-https.ps1 -LanHost $lanHost -ValidateOnly
} catch {
    Write-Error $_
    exit 1
}
npm run dev --prefix .\apps\customer-web
```

URL LAN: API `https://172.18.41.58:5000`, Admin `https://172.18.41.58:5173`, Customer `https://172.18.41.58:5174`. Khi doi IP, thay `$lanHost` trong ca ba terminal. Backend van can `ConnectionStrings__DefaultConnection`/User Secrets va `Jwt__Secret` hop le; setup HTTPS khong tao hoac in cac secret nay.

Khi doi Wi-Fi/IP, dung Ctrl+C ca ba process, chay lai block 1 voi IP moi, sau do chay lai block 2 va 3 voi cung IP. QR da in voi IP cu khong tu cap nhat. Neu IP moi chua co trong SAN, block 1 se cap leaf bang CA mkcert hien tai; hai block `-ValidateOnly` phai chay sau do. Neu CA mkcert doi hoac thiet bi chua trust CA, xu ly trust public root CA theo chinh sach thiet bi; khong chuyen private key sang dien thoai.
