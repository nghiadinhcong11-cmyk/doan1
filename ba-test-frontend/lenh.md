Dưới đây là lệnh đã cập nhật để test luôn AI embedding mới. Chỉ cần thay 172.18.41.58 bằng IP LAN hiện tại trong cả ba terminal.

Terminal 1 — API + chứng chỉ + bật AI embedding
Set-Location D:\Dev\doan

# Lấy IP của card mạng đang dùng route mặc định
$defaultRoute = Get-NetRoute `
  -AddressFamily IPv4 `
  -DestinationPrefix '0.0.0.0/0' |
  Sort-Object RouteMetric, InterfaceIndex |
  Select-Object -First 1

$lanHost = (Get-NetIPAddress `
  -AddressFamily IPv4 `
  -InterfaceIndex $defaultRoute.InterfaceIndex |
  Where-Object {
    $_.IPAddress -notlike '127.*' -and
    $_.IPAddress -notlike '169.254.*'
  } |
  Select-Object -First 1).IPAddress

if ([string]::IsNullOrWhiteSpace($lanHost)) {
  throw 'Không tìm thấy IP mạng hiện tại.'
}

Write-Host "LAN IP hiện tại: $lanHost"

# Xóa cấu hình cũ trong PowerShell session
Remove-Item `
  Env:VITE_API_URL,
  Env:VITE_CUSTOMER_WEB_URL,
  Env:DOAN_DEV_LAN_HOST,
  Env:AI__ProviderMode,
  Env:Database__TargetMode,
  Env:Database__ExpectedProjectRef,
  Env:ConnectionStrings__DefaultConnection,
  Env:SUPABASE_CONNECTION_STRING `
  -ErrorAction SilentlyContinue

# Tạo/kiểm tra certificate cho IP hiện tại
. .\scripts\setup-https.ps1 -LanHost $lanHost

# Backend environment
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:Database__TargetMode = 'Test'
$env:Database__ExpectedProjectRef = 'lizpcvayxcuwjgrxsvfe'
$env:DatabaseSafety__AllowAutomaticMigration = 'false'

# AI embedding pilots
$env:AI__AdminEmbeddingPilot__Enabled = 'true'
$env:AI__CustomerGuestEmbeddingPilot__Enabled = 'true'
$env:AI__UatLogging__Enabled = 'true'

# Khởi động backend
dotnet run `
  --project .\services\api\RestaurantPOS.api.csproj `
  --no-launch-profile

Terminal 2 — Admin Web

Set-Location D:\Dev\doan

. .\scripts\setup-https.ps1 -LanHost 192.168.11.172

npm run dev --prefix .\apps\admin-web

Terminal 3 — Customer Web\
Set-Location D:\Dev\doan

. .\scripts\setup-https.ps1 -LanHost 192.168.11.172

npm run dev --prefix .\apps\customer-web

URL truy cập:
API:      https://172.18.41.58:5000
Admin:    https://172.18.41.58:5173
Customer: https://172.18.41.58:5174

Thứ tự chạy: Terminal 1 trước, sau đó Terminal 2 và 3.
 Worked for 9s • 4:07 PM



