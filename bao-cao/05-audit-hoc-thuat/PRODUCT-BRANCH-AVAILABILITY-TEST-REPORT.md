# ProductBranchAvailability — áp dụng trên Supabase TEST

Ngày thực hiện: 16/10/2026. **Chỉ migration `20261015051548_AddProductBranchAvailability` đã được người dùng phê duyệt và áp dụng bằng EF Core CLI.** Không chạy file SQL thủ công, không chỉnh history thủ công, không thay database MAIN/production.

## Tóm tắt trạng thái

| Hoạt động | Kết quả thực tế | Trạng thái |
|---|---|---|
| Xác minh TEST ngay trước migration | API PID 14116 đang nghe HTTPS cổng 5000; runtime attestation và EF preflight cùng xác nhận target TEST `eorboopkwgmvainlxzkz`, host `aws-0-ap-northeast-1.pooler.supabase.com`, database `postgres`, automatic migration tắt. | PASS |
| History trước migration | 49 ID, khớp 49 migration đầu của source; mới nhất `20261014120000_AddNotificationReadState`; **chỉ** Product migration Pending. Bảng mới chưa có, 7 Products × 2 Branches = 14 cặp dự kiến. | PASS |
| Áp dụng bằng EF Core | Lệnh `dotnet ef database update 20261015051548_AddProductBranchAvailability --project services/api/RestaurantPOS.api.csproj --startup-project services/api/RestaurantPOS.api.csproj --context ApplicationDbContext --no-build` kết thúc mã 0, log ghi `Applying migration '20261015051548_AddProductBranchAvailability'` và `Done.` | PASS |
| Bảng, ràng buộc, dữ liệu | Truy vấn transaction READ ONLY xác nhận bảng, PK kép, hai FK, index, 14 dòng và history mới. Chi tiết bên dưới. | PASS |
| Product API GET không đăng nhập | `/api/Product` và `/api/Product?branchId=...` cho cả hai branch hợp lệ đều HTTP 200, JSON array 7 sản phẩm, tất cả `Available`. | PASS — đúng các request đã thử |
| Menu Admin Web | Người dùng trực tiếp mở Admin Web sau migration và xác nhận menu hiển thị trở lại. Bằng chứng là quan sát thủ công do người dùng báo trong hội thoại; không có ảnh, log trình duyệt hoặc bản ghi Network kèm theo. | PASS — chỉ cho kết quả hiển thị được người dùng xác nhận |
| Product API với phiên Admin/manager; chuyển chi nhánh | Chưa có kết quả HTTP của phiên có quyền hoặc quan sát chuyển chi nhánh được ghi nhận. | NOT RUN |
| Menu Customer Web sau migration; xử lý `Available`/`Unavailable` theo branch | Chưa có kết quả quan sát Customer Web hoặc phép thử chuyển trạng thái; dữ liệu hiện chỉ có `Available`. | NOT RUN |

## Bằng chứng target và binary

- Process API sau restart do người dùng thực hiện: `RestaurantPOS.api.exe` PID **14116**, khởi động 23:12:13 giờ địa phương; parent `dotnet.exe` PID 4340. `netstat` xác nhận PID 14116 lắng nghe `0.0.0.0:5000`.
- [Runtime attestation của API](../../services/api/bin/Debug/net8.0/runtime-attestations/runtime-attestation-database-14116-20261016161214.json) ghi `Development`, `mode=Test`, host/port/database nêu trên, TEST project ref đã được `SupabaseProjectTargetGuard` kiểm tra, `automaticMigrationDisabled=true`, `connectionStatus=READ_ONLY_VERIFIED`. Chính API dùng connection string sẽ cấp cho DbContext, mở PostgreSQL trong transaction `READ ONLY`, đọc `current_database()=postgres` và rollback. Không ghi username, password hoặc connection string.
- SHA-256 của DLL API mới trong attestation và đọc lại trên đĩa đều là `1C62F4CD08C66365E402EADFF10E8E08C29A57B7108DFB8F5EBF14E348D5BAC1`. EXE đều là `4EA11445E31B1BFCB362984FF98E591A2F10D6F064314A90905E36EB9AC16D8B`. Đây là fingerprint file process đã dùng lúc startup, không phải dump image trong bộ nhớ.
- Migration source không có diff với HEAD `38bbd423b963aaf5779d4ae3356f942ff236643b`. SQL do EF `--no-build` sinh từ binary Debug mới trùng byte file [SQL audit](PRODUCT-BRANCH-AVAILABILITY-EF.sql), SHA-256 `9BD27D5F370DF325C23BBA16D421049BC079A831359B993827AA642CE9AEE1FD`.
- Ngay trước lệnh ghi, `dotnet ef migrations list --no-build` trên cùng cấu hình TEST và attestation EF CLI `READ_ONLY_VERIFIED` xác nhận 49 migration áp dụng, Product là mục Pending duy nhất. Attestation EF CLI preflight: `services/api/bin/Debug/net8.0/runtime-attestations/runtime-attestation-database-11440-20261016161913.json`.

Lần gọi EF update đầu trong sandbox thất bại ở TLS handshake **trước khi mở database**, mã thoát 1. Sau đó preflight đọc history ngoài sandbox vẫn cho thấy Product Pending và bảng chưa có. Lần chạy lại ngoài sandbox dùng đúng lệnh đã phê duyệt, mã thoát 0. Không có bằng chứng thao tác ghi nào từ lần thất bại đầu.

## Truy vấn chỉ đọc sau migration

Tiện ích kiểm tra tạm đối chiếu host/port/database/project ref với attestation của API trước khi kết nối; `SupabaseProjectTargetGuard` xác nhận TEST. Nó mở transaction `REPEATABLE READ`, đặt `READ ONLY`, đọc catalog/dữ liệu bằng `SELECT`, rồi rollback. Kết quả:

| Điều kiện | Kết quả thực tế | Trạng thái |
|---|---|---|
| Database / transaction | `postgres` / `transaction_read_only=on` | PASS |
| Bảng `ProductBranchAvailabilities` | Tồn tại; `ProductId uuid NOT NULL`, `BranchId uuid NOT NULL`, `Status varchar NOT NULL`, `UpdatedAtUtc timestamptz NOT NULL`; không có column default | PASS |
| PK | `PRIMARY KEY ("ProductId", "BranchId")` | PASS |
| FK | `BranchId → Branches(Id)` và `ProductId → Products(Id)`, cả hai `ON DELETE CASCADE` | PASS |
| Index | `IX_ProductBranchAvailabilities_BranchId_Status` trên `(BranchId, Status)`; PK tạo unique index riêng | PASS |
| Backfill | 7 Products × 2 Branches = **14 dòng thực tế**; `missingPairs=0`, `duplicatePairs=0`, `statusMismatch=0`; cả 14 dòng `Available` | PASS |
| EF history | 50 dòng; migration Product là ID mới nhất và xuất hiện **đúng một lần** | PASS |

Số 14 là kết quả đã đọc sau migration, không còn chỉ là dự kiến. Không tự sửa dữ liệu.

## Product API và giới hạn kết luận

`GET /api/Branch` trả HTTP 200 và hai branch hợp lệ. Gọi HTTPS GET trực tiếp tới process PID 14116:

| Request | HTTP | Response đã xác minh |
|---|---:|---|
| `/api/Product` | 200 | JSON array 7 sản phẩm, `Available: 7` |
| `/api/Product?branchId=40000000-0000-4000-8000-000000000001` | 200 | JSON array 7 sản phẩm, `Available: 7` |
| `/api/Product?branchId=bcecbb3f-860f-416a-81f3-197d2d1c34fe` | 200 | JSON array 7 sản phẩm, `Available: 7` |

Lỗi HTTP 500 trước đó **không tái hiện trên các GET đã kiểm tra**. Bằng chứng HTTP 200 ở bảng trên là phép gọi API trực tiếp không đăng nhập; bằng chứng menu Admin Web hiển thị trở lại là quan sát thủ công **riêng** do người dùng xác nhận sau migration; bằng chứng bảng và backfill là truy vấn database READ ONLY. Các nguồn này hỗ trợ kết luận chức năng đã phục hồi trong phạm vi quan sát, nhưng **chưa đủ để khẳng định migration là nguyên nhân duy nhất** khắc phục lỗi 500.

`ProductController.GetProducts` tự đọc `Products` rồi `ProductBranchAvailabilities` cho branch; `ProductService.GetAllProductsAsync` là đường gọi khác, không được gọi bởi endpoint GET này. Admin POS gọi endpoint GET đó với `branchId`; Customer Web cũng dùng `/api/Product?branchId=...`. Chưa ghi nhận Network/HTTP của phiên Admin hoặc manager, chuyển chi nhánh, quan sát Customer Web sau migration, hay trường hợp `Unavailable`; không gán PASS cho các trường hợp này.

## Thay đổi và rủi ro còn lại

Source chỉ được sửa để tạo attestation ở [Program.cs](../../services/api/Program.cs) và [DatabaseTargetAttestationWriter.cs](../../services/api/src/WebAPI/Diagnostics/DatabaseTargetAttestationWriter.cs). Migration, entity, DbContext, Product API và frontend không sửa trong lượt này. Build attestation trước restart: 0 lỗi, 24 cảnh báo hiện hữu; không chạy test suite. Tiện ích kiểm tra tạm đã xóa.

Attestation Development/TEST mở một kết nối đọc lúc startup; nếu probe lỗi, nó ghi `BLOCKED` nhưng không chặn API. EF design-time có thể tạo attestation riêng của process CLI; chỉ attestation PID API mới chứng minh runtime API. Để kết luận đầy đủ luồng menu, vẫn cần kiểm tra phiên Admin/manager có ghi nhận Network, chuyển chi nhánh, Customer Web sau migration và `Unavailable`. Không tự tạo dữ liệu kiểm thử hay thay đổi logic nghiệp vụ ở lượt này.
