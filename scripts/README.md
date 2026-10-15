# Repository scripts

Thư mục này chỉ chứa công cụ phát triển, kiểm tra cấu hình và bảo trì có phạm vi rõ ràng. Không script nào được phép dùng credential/token làm tham số dòng lệnh; giá trị nhạy cảm, nếu thật sự cần cho một thao tác được hỗ trợ, phải lấy từ User Secrets hoặc biến môi trường của phiên chạy.

## Công cụ còn được hỗ trợ

| Công cụ | Mục đích | Tác động |
| --- | --- | --- |
| [`setup-https.ps1`](setup-https.ps1) | Tạo hoặc cập nhật chứng chỉ HTTPS development trong `.https`; hỗ trợ SAN LAN khi `DOAN_DEV_LAN_HOST` được đặt. | Ghi `.https` và có thể ghi chứng chỉ LAN theo cấu hình; không ghi database. Chỉ dùng development. |
| `npm run codegen:file` | Sinh `packages/shared/types/api.ts` từ OpenAPI snapshot. | Ghi source được sinh; cần Node/npm. |
| `npm run codegen:check` | So sánh type sinh ra với snapshot mà không giữ file tạm. | Ghi file generated tạm rồi xóa khi pass; không gọi API. |
| [`diagnostics/StartManualTestBackend.ps1`](diagnostics/StartManualTestBackend.ps1) | Khởi động API development đã build cho checklist thủ công với `AI__ProviderMode=Mock`, target `Test`, migration tự động tắt và runtime attestation. | Khởi động process, đọc database qua API startup nếu ứng dụng yêu cầu; có thể ghi attestation cục bộ. Không chạy production. Không dùng nếu chưa xác minh target database. |
| `diagnostics/VerifyRuntimeAttestation.ps1` | Đối chiếu attestation với PID, process start time, correlation, mode và target database. | Chỉ đọc file attestation và process metadata. |
| [`phase2_readonly_audit.sql`](phase2_readonly_audit.sql) | Bộ truy vấn audit Phase 2 chỉ đọc để kiểm tra metadata/dữ liệu đã chọn. | Có thể đọc database nếu được chạy thủ công; không chạy trong phiên audit này. Không chứa lệnh ghi theo kiểm tra tĩnh. |

Lệnh khởi động backend cho checklist (chỉ sau khi đã xác nhận database test cô lập và API đã build):

```powershell
powershell -File .\scripts\diagnostics\StartManualTestBackend.ps1 -Configuration Release
```

Lệnh kiểm tra/chuẩn bị vận hành được giữ lại:

```powershell
dotnet run --project .\scripts\diagnostics\TestConnectionSelect1\TestConnectionSelect1.csproj --no-restore
dotnet run --project .\scripts\diagnostics\RunTestSchemaDeployment\RunTestSchemaDeployment.csproj -- --dry-run
dotnet run --project .\scripts\diagnostics\RunTestSeed\RunTestSeed.csproj -- --dry-run
dotnet run --project .\scripts\diagnostics\RunManualExpenseMigrationUpgrade\RunManualExpenseMigrationUpgrade.csproj -- --dry-run
dotnet run --project .\scripts\diagnostics\QrTokenBackfill\QrTokenBackfill.csproj -- --dry-run
```

Các lệnh trên chỉ là tài liệu vận hành; phiên audit này không thực thi chúng. `--apply`/`--execute` có thể ghi database và cần phê duyệt riêng.

## Bảo trì dữ liệu còn được hỗ trợ

- `diagnostics/QrTokenBackfill`: tool bảo trì riêng, dry-run mặc định; `--execute` có thể ghi database và chỉ hợp lệ với target development được guard.

Không còn tool provision/reconcile tài khoản, fixture branch, ground-truth hoặc audit role riêng cho E2E. Checklist thủ công dùng tài khoản và dữ liệu test đã được người vận hành chuẩn bị, không tạo marker hoặc credential mapping tự động.

## Database/migration tools — human-only

- `diagnostics/TestConnectionSelect1`: kiểm tra target development, SSL và `SELECT 1`, không migration/seed.
- `diagnostics/RunTestSchemaDeployment`: dry-run/check/inspect mặc định; `--apply` có thể chạy migration trên target được guard.
- `diagnostics/RunTestSeed`: dry-run/check mặc định; `--apply` có thể seed dữ liệu test.
- `diagnostics/RunManualExpenseMigrationUpgrade`: dry-run/check mặc định; `--apply` có thể áp dụng nâng cấp migration được giới hạn.

Không chạy các lệnh `--apply`, migration hoặc seed trong phiên audit này. Các tool này không phải cách khởi động ứng dụng và không được dùng với production.

## Thực hành và phạm vi

- HTTPS development chuẩn: chạy `powershell -File .\scripts\setup-https.ps1`, sau đó dùng các URL trong root README.
- Kiểm thử giao diện hiện hành thực hiện thủ công theo [`ba-test-frontend/README.md`](../ba-test-frontend/README.md); repository không còn bộ test xUnit/Node/Playwright để chạy.
- CORS, branch isolation, role readiness và đối soát doanh thu được kiểm tra trong checklist; không còn static test runner riêng cho các nội dung này.
- Tài liệu trong `bao-cao/` có thể chứa lệnh hoặc đường dẫn lịch sử. Nếu một báo cáo cũ nhắc tới launcher/preflight đã retired, không phục hồi hoặc chạy lại lệnh đó; dùng tool hiện hành ở README này.
- Không có script hiện hành nào gửi request tới provider AI bên ngoài. Mock AI được cấu hình ở runtime của API và launcher test hiện hành chỉ đặt `AI__ProviderMode=Mock`.
## Launcher HTTPS/LAN

`start-dev.ps1` la launcher van hanh duy nhat cho development local/LAN. No goi `setup-https.ps1` trong cung process, truyen certificate URL va `DOAN_DEV_LAN_HOST` cho ca backend, Admin Web va Customer Web, ep `DatabaseSafety__AllowAutomaticMigration=false`, kiem tra port va chi dung cac PID do no tao.

```powershell
powershell -File .\scripts\start-dev.ps1 -Mode Localhost
powershell -File .\scripts\start-dev.ps1 -Mode Lan -LanHost <ip-hoac-hostname>
powershell -File .\scripts\start-dev.ps1 -Stop
```

Neu khong biet IP, co the chay setup rieng de hien danh sach adapter va chon ro:

```powershell
powershell -File .\scripts\setup-https.ps1
```

Script setup chi su dung CA mkcert hien tai va bao loi neu mkcert/CA thieu; nguoi van hanh phai cai mkcert va trust CA bang quy trinh duoc phe duyet. Khong dua private key vao frontend/public hoac thiet bi dien thoai.

Khi chay API/Admin/Customer trong ba terminal rieng, chay setup khong co `-ValidateOnly` mot lan o terminal chuan bi. Hai terminal con lai dung cung `-LanHost` voi `-ValidateOnly`; che do nay chi validate va export environment, khong tao/thay certificate. Dung dot-sourcing de environment con hieu luc trong terminal hien tai.
