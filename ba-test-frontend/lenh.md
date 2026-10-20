Dưới đây là lệnh đã cập nhật để test luôn AI embedding mới. Chỉ cần thay 172.18.41.58 bằng IP LAN hiện tại trong cả ba terminal.
 192.168.11.172
Terminal 1 — API + chứng chỉ + bật AI embedding
Set-Location D:\Dev\doan

$lanHost = '192.168.11.172'

Remove-Item Env:VITE_API_URL,Env:VITE_CUSTOMER_WEB_URL,Env:DOAN_DEV_LAN_HOST,Env:AI__ProviderMode -ErrorAction SilentlyContinue

. .\scripts\setup-https.ps1 -LanHost $lanHost

$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:DatabaseSafety__AllowAutomaticMigration = 'false'

# Bật AI embedding mới
$env:AI__AdminEmbeddingPilot__Enabled = 'true'
$env:AI__CustomerGuestEmbeddingPilot__Enabled = 'true'
$env:AI__UatLogging__Enabled = 'true'

dotnet run --project .\services\api\RestaurantPOS.api.csproj --no-launch-profile

Terminal 2 — Admin Web
Set-Location D:\Dev\doan

$lanHost = '192.168.11.172'

Remove-Item Env:VITE_API_URL,Env:VITE_CUSTOMER_WEB_URL,Env:DOAN_DEV_LAN_HOST -ErrorAction SilentlyContinue

. .\scripts\setup-https.ps1 -LanHost $lanHost -ValidateOnly

npm run dev --prefix .\apps\admin-web

Terminal 3 — Customer Web\
Set-Location D:\Dev\doan

$lanHost = '192.168.11.172'

Remove-Item Env:VITE_API_URL,Env:VITE_CUSTOMER_WEB_URL,Env:DOAN_DEV_LAN_HOST -ErrorAction SilentlyContinue

. .\scripts\setup-https.ps1 -LanHost $lanHost -ValidateOnly

npm run dev --prefix .\apps\customer-web

URL truy cập:
API:      https://172.18.41.58:5000
Admin:    https://172.18.41.58:5173
Customer: https://172.18.41.58:5174

Thứ tự chạy: Terminal 1 trước, sau đó Terminal 2 và 3.
 Worked for 9s • 4:07 PM



