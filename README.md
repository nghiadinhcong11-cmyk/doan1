# Restaurant POS System

Hệ thống quản lý nhà hàng đa chi nhánh gồm Admin/POS Web, Customer Web đặt món qua QR, Kitchen Display System và trợ lý AI local.

## Phạm vi hiện tại

- Quản lý chi nhánh, thực đơn, topping, khuyến mãi, bàn và đặt bàn.
- POS, đơn hàng, thanh toán, hóa đơn, doanh thu, chi phí và báo cáo.
- Quản lý nhân viên, chấm công, ca làm và lịch làm việc.
- Kho và các nghiệp vụ hỗ trợ vận hành nhà hàng.
- KDS realtime qua SignalR tại `/kitchenHub`.
- Customer Web cho khách đăng nhập và khách QR/guest.
- AI chạy bằng local E5 sentence-embedding pilots, không gọi provider bên ngoài.

## Cấu trúc repository

```text
apps/
  admin-web/       # Admin, POS, quản lý vận hành
  customer-web/    # Menu, đặt món, profile và QR guest

services/api/      # ASP.NET Core 8 API
  src/Domain/       # Entity và khái niệm nghiệp vụ
  src/Application/  # DTO, application service, use case
  src/Infrastructure/# EF Core, PostgreSQL, migration
  src/WebAPI/       # Controller, authentication, middleware
  src/AI/           # Orchestrator, pilot, tool registry, permission

ba-test-frontend/  # Checklist manual theo role
bao-cao/            # Báo cáo, hình ảnh và audit học thuật
scripts/            # HTTPS, codegen và diagnostics có phạm vi rõ ràng
certs/              # Certificate development multi-LAN
```

## Kiến trúc và bảo mật

Luồng xử lý chính:

```text
Web / AI
  → Controller hoặc AI Tool
  → Application Service
  → Infrastructure / PostgreSQL
```

- API dùng ASP.NET Core 8, EF Core, PostgreSQL, JWT Bearer và SignalR.
- `admin` có phạm vi toàn hệ thống; `manager`, `cashier`, `kitchen`, `employee` được giới hạn theo branch; `customer` chỉ truy cập dữ liệu của mình; guest bị giới hạn theo QR session.
- Role là nguồn phân quyền. Không dùng `Position`, localStorage hoặc dữ liệu client để cấp quyền.
- Giá, tổng tiền, payment state và branch scope được backend xác minh.
- AI không truy cập trực tiếp EF Core. Mọi tool phải đi qua `AiPermissionService` và application service.
- Write tool nhạy cảm dùng confirmation token, kiểm tra ownership, thời hạn, expected value và chỉ dùng một lần.

## AI runtime hiện tại

Runtime dùng hai pilot local:

- `AdminEmbeddingPilot` cho `POST /api/Ai/chat` của Admin; yêu cầu JWT role `admin`.
- `CustomerGuestEmbeddingPilot` cho Customer Web và guest; chỉ hỗ trợ capability theo scope tương ứng.

Admin AI hiện hỗ trợ đọc doanh thu/KPI/đơn hàng/món bán chạy và các write flow:

- Đổi giá sản phẩm: `update_product_price`.
- Tạo sản phẩm: `create_product`.
- Tạo topping: `create_topping`.
- Đổi tên sản phẩm: `update_product_name`.

Các write flow đều phải preview và xác nhận. AI fail-closed khi thiếu hoặc sai model, tokenizer hay reference catalog; không có fallback `MockAiProvider`.

Các flag và đường dẫn model nằm trong `services/api/appsettings.Development.json` dưới:

```text
AI:AdminEmbeddingPilot
AI:CustomerGuestEmbeddingPilot
AI:UatLogging
```

Các flag pilot mặc định tắt. Khi UAT, bật bằng biến môi trường của phiên chạy API, không commit secret hoặc token.

## Chạy local bằng HTTPS

Yêu cầu: .NET SDK 8, Node/npm và PostgreSQL/Supabase development phù hợp với cấu hình ứng dụng.

### API

```powershell
Set-Location D:\Dev\doan
. .\scripts\setup-https.ps1
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project .\services\api\RestaurantPOS.api.csproj --no-launch-profile
```

API mặc định: `https://localhost:5000`.

### Admin Web

```powershell
Set-Location D:\Dev\doan
npm run dev --prefix .\apps\admin-web
```

Admin mặc định: `https://localhost:5173`.

### Customer Web

```powershell
Set-Location D:\Dev\doan
npm run dev --prefix .\apps\customer-web
```

Customer mặc định: `https://localhost:5174`.

### Truy cập từ LAN

LAN là chế độ opt-in. Thay `<LAN_IP>` bằng IP hiện tại có thể truy cập từ thiết bị test:

```powershell
Set-Location D:\Dev\doan
$lanHost = '<LAN_IP>'
. .\scripts\setup-https.ps1 -LanHost $lanHost
```

Terminal Admin và Customer dùng cùng `$lanHost` với `-ValidateOnly`. Không hardcode IP LAN vào source/config. Khi đổi Wi-Fi hoặc IP, chạy lại setup và dùng certificate/QR phù hợp; không chuyển private key sang thiết bị client.

## Build và kiểm tra

```powershell
dotnet build .\services\api\RestaurantPOS.api.csproj
npm run build --prefix .\apps\admin-web
npm run build --prefix .\apps\customer-web
npm run codegen:check
git diff --check
```

Kiểm thử nghiệp vụ frontend hiện được thực hiện thủ công. Mở [ba-test-frontend/README.md](ba-test-frontend/README.md) để chọn checklist theo role. AI checklist nằm tại [ba-test-frontend/ai-chat/README.md](ba-test-frontend/ai-chat/README.md).

Repository hiện không có thư mục `tests/` hoặc bộ Playwright/Node test runner hoạt động; không coi việc build thành công là thay thế cho manual UAT.

## Tài liệu

- [Checklist frontend theo role](ba-test-frontend/README.md)
- [Checklist AI](ba-test-frontend/ai-chat/README.md)
- [Tài liệu báo cáo](bao-cao/README.md)
- [Audit học thuật](bao-cao/05-audit-hoc-thuat/README.md)
- [Scripts và diagnostics](scripts/README.md)
- [Ví dụ biến môi trường](.env.example)

Không commit API key, JWT secret, database password, private key hoặc access token. Các tài liệu lịch sử trong `bao-cao/` có thể không phản ánh runtime hiện tại; khi có mâu thuẫn, ưu tiên source code, cấu hình hiện tại và checklist UAT.
