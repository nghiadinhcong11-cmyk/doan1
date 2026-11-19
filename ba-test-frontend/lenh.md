Lần sau chỉ cần mở PowerShell tại D:\Dev\doan và chạy:

.\scripts\start-dev.ps1 -Mode Localhost

cd D:\Dev\doan
. .\scripts\setup-https.ps1 -Localhost
cd .\apps\customer-web
npm run dev

cd D:\Dev\doan
. .\scripts\setup-https.ps1 -Localhost
cd .\apps\admin-web
npm run dev