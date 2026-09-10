$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$httpsDir = Join-Path $repoRoot '.https'
$openssl = (Get-Command openssl -ErrorAction Stop).Source
$lanIp = (ipconfig | Select-String 'IPv4' | ForEach-Object {
  if ($_ -match '(\d{1,3}(?:\.\d{1,3}){3})') { $Matches[1] }
} | Where-Object { $_ -notlike '169.254.*' -and $_ -ne '127.0.0.1' } | Select-Object -First 1)
if (-not $lanIp) { throw 'No LAN IPv4 address found. Connect to Wi-Fi and retry.' }

New-Item -ItemType Directory -Force -Path $httpsDir | Out-Null
$key = Join-Path $httpsDir 'pos-key.pem'
$cert = Join-Path $httpsDir 'pos-cert.pem'
$pfx = Join-Path $httpsDir 'pos-cert.pfx'
& $openssl req -x509 -newkey rsa:2048 -nodes -days 825 `
  -keyout $key -out $cert -subj '/CN=RestaurantPOS Local' `
  -addext "subjectAltName=DNS:localhost,IP:127.0.0.1,IP:$lanIp"
$certPassword = [Guid]::NewGuid().ToString('N')
$certPassword | Out-File -FilePath (Join-Path $repoRoot '.https/cert-password.txt')
& $openssl pkcs12 -export -out $pfx -inkey $key -in $cert -passout "pass:$certPassword"

$env:HTTPS_CERT_PATH = (Resolve-Path -LiteralPath $pfx).Path
$env:HTTPS_CERT_PASSWORD = $certPassword
$env:VITE_HTTPS = 'true'
if (-not (Test-Path -LiteralPath $env:HTTPS_CERT_PATH)) { throw "HTTPS certificate was not created: $env:HTTPS_CERT_PATH" }
Write-Host "HTTPS certificate created for LAN IP: $lanIp"
Write-Host "Certificate: $env:HTTPS_CERT_PATH"
Write-Host "Frontend: https://${lanIp}:5173"
Write-Host "API:      https://${lanIp}:5000/swagger"
Write-Host 'Run the backend in this same PowerShell window to use HTTPS.'
