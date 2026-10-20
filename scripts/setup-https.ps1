[CmdletBinding()]
param(
  [string[]]$LanHost,
  [switch]$Localhost,
  [switch]$ValidateOnly
)

$ErrorActionPreference = 'Stop'

function Invoke-External([string]$File, [string[]]$Arguments) {
  $token = [Guid]::NewGuid().ToString('N')
  $outputPath = Join-Path ([IO.Path]::GetTempPath()) "restaurant-pos-https-$token.out"
  $errorPath = Join-Path ([IO.Path]::GetTempPath()) "restaurant-pos-https-$token.err"
  try {
    $process = Start-Process -FilePath $File -ArgumentList $Arguments -RedirectStandardOutput $outputPath -RedirectStandardError $errorPath -WindowStyle Hidden -Wait -PassThru
    if ($process.ExitCode -ne 0) { throw "Command failed: $File" }
    return [IO.File]::ReadAllText($outputPath).Trim()
  } finally {
    Remove-Item -LiteralPath $outputPath, $errorPath -Force -ErrorAction SilentlyContinue
  }
}

function Test-HostValue([string]$Value) {
  return -not [string]::IsNullOrWhiteSpace($Value) -and $Value -match '^[A-Za-z0-9.-]+$'
}

function Get-UsableLanAddresses {
  Get-NetIPAddress -AddressFamily IPv4 -Type Unicast -ErrorAction SilentlyContinue |
    Where-Object { $_.IPAddress -notlike '127.*' -and $_.IPAddress -notlike '169.254.*' } |
    ForEach-Object {
      $adapter = Get-NetAdapter -InterfaceIndex $_.InterfaceIndex -ErrorAction SilentlyContinue
      [pscustomobject]@{
        IPAddress = $_.IPAddress
        Interface = if ($adapter) { $adapter.Name } else { "Index $($_.InterfaceIndex)" }
        Status = if ($adapter) { $adapter.Status } else { 'Unknown' }
      }
    } | Sort-Object IPAddress -Unique
}

function Read-SelectedHost {
  $addresses = @(Get-UsableLanAddresses)
  if ($addresses.Count -eq 0) { throw 'NO_USABLE_LAN_ADDRESS: no non-loopback IPv4 adapter was found.' }
  Write-Host 'Usable LAN addresses (choose explicitly; VPN/virtual adapters may be listed):'
  for ($index = 0; $index -lt $addresses.Count; $index++) {
    $item = $addresses[$index]
    Write-Host ("[{0}] {1} ({2}, {3})" -f ($index + 1), $item.IPAddress, $item.Interface, $item.Status)
  }
  $selection = Read-Host 'Enter the number, or type a hostname/IP explicitly'
  if ($selection -match '^\d+$' -and [int]$selection -ge 1 -and [int]$selection -le $addresses.Count) {
    return $addresses[[int]$selection - 1].IPAddress
  }
  if (Test-HostValue $selection) { return $selection }
  throw 'INVALID_LAN_HOST: enter one listed address or a hostname/IP containing only letters, digits, dot and hyphen.'
}

function Get-HostValues([string[]]$Values) {
  $hosts = [System.Collections.Generic.List[string]]::new()
  foreach ($value in @($Values)) {
    foreach ($part in ($value -split ',')) {
      $trimmed = $part.Trim()
      if (-not [string]::IsNullOrWhiteSpace($trimmed)) { $hosts.Add($trimmed) }
    }
  }
  return @($hosts | Sort-Object -Unique)
}

function Get-CertificateText([string]$OpenSsl, [string]$CertificatePath) {
  return Invoke-External $OpenSsl @('x509', '-in', $CertificatePath, '-noout', '-text')
}

function Test-CertificateCoversHost([string]$Text, [string]$HostValue) {
  if ($HostValue -match '^\d+(\.\d+){3}$') { return $Text -match [regex]::Escape("IP Address:$HostValue") }
  return $Text -match [regex]::Escape("DNS:$HostValue")
}

function Test-CertificatePair([string]$OpenSsl, [string]$CertificatePath, [string]$KeyPath, [string]$CaPath, [string[]]$RequiredHosts) {
  if (-not (Test-Path -LiteralPath $CertificatePath) -or -not (Test-Path -LiteralPath $KeyPath)) { return $false }
  try {
    $text = Get-CertificateText $OpenSsl $CertificatePath
    & $OpenSsl x509 -in $CertificatePath -checkend 0 -noout 2>$null | Out-Null
    if ($LASTEXITCODE -ne 0) { return $false }
    $verification = & $OpenSsl verify -CAfile $CaPath $CertificatePath 2>$null | Out-String
    if ($LASTEXITCODE -ne 0 -or $verification -notmatch ': OK') { return $false }
    foreach ($requiredHost in $RequiredHosts) {
      if (-not (Test-CertificateCoversHost $text $requiredHost)) { return $false }
    }
    $certPublicKey = ((& $OpenSsl x509 -in $CertificatePath -noout -pubkey 2>$null) -join "`n").Trim()
    $keyPublicKey = ((& $OpenSsl pkey -in $KeyPath -pubout 2>$null) -join "`n").Trim()
    if ($LASTEXITCODE -ne 0) { return $false }
    return -not [string]::IsNullOrWhiteSpace($certPublicKey) -and $certPublicKey -eq $keyPublicKey
  } catch { return $false }
}

function Get-AtomicTempPath([string]$TargetPath) {
  return "$TargetPath.tmp.$([Guid]::NewGuid().ToString('N'))"
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$certDirectory = Join-Path $repoRoot 'certs'
$certificatePath = Join-Path $certDirectory 'doan-pos-lan.pem'
$keyPath = Join-Path $certDirectory 'doan-pos-lan-key.pem'
$openssl = (Get-Command openssl -ErrorAction Stop).Source
$mkcert = (Get-Command mkcert -ErrorAction SilentlyContinue).Source
if (-not $mkcert) { throw 'MKCERT_NOT_FOUND: install mkcert separately and run mkcert -install once if approved; this script does not install tools or modify trust stores.' }

$caRoot = (Invoke-External $mkcert @('-CAROOT')).Trim()
$caPath = Join-Path $caRoot 'rootCA.pem'
if (-not (Test-Path -LiteralPath $caPath)) { throw "MKCERT_CA_NOT_FOUND: current mkcert CA was not found at $caPath. Trust setup must be performed manually." }

if ($Localhost -and @($LanHost).Count -gt 0) { throw 'INVALID_MODE: do not combine -Localhost with -LanHost.' }

$runtimeLanHost = $null
if (-not $Localhost) {
  if (@($LanHost).Count -eq 0 -and -not [string]::IsNullOrWhiteSpace($env:DOAN_DEV_LAN_HOST)) {
    $LanHost = @($env:DOAN_DEV_LAN_HOST)
  }
  if (@($LanHost).Count -eq 0) { $LanHost = @(Read-SelectedHost) }
  $runtimeLanHost = ((@($LanHost)[0] -split ',')[0]).Trim()
  $LanHost = @(Get-HostValues $LanHost)
  if (-not (Test-HostValue $runtimeLanHost)) { throw 'INVALID_LAN_HOST: host must contain only letters, digits, dot and hyphen, without scheme, port or path.' }
  foreach ($hostValue in @($LanHost)) {
    $parsedIp = $null
    if (-not [System.Net.IPAddress]::TryParse($hostValue, [ref]$parsedIp) -or $parsedIp.AddressFamily -ne [System.Net.Sockets.AddressFamily]::InterNetwork) {
      throw "INVALID_LAN_IP: '$hostValue' must be an IPv4 address."
    }
  }
}

$hostsFile = Join-Path $certDirectory 'lan-hosts.txt'
$savedHosts = @(
  if (Test-Path -LiteralPath $hostsFile) {
    Get-Content -LiteralPath $hostsFile |
      ForEach-Object { $_.Trim() } |
      Where-Object { $_ -and $_ -notmatch '^#' }
  }
)
$savedHosts = @(Get-HostValues $savedHosts)
foreach ($savedHost in $savedHosts) {
  $parsedIp = $null
  if (-not [System.Net.IPAddress]::TryParse($savedHost, [ref]$parsedIp) -or $parsedIp.AddressFamily -ne [System.Net.Sockets.AddressFamily]::InterNetwork) {
    throw "INVALID_SAVED_LAN_IP: $savedHost"
  }
}

$knownLanHosts = @()
$requestedHosts = if ($Localhost) {
  @('localhost', '127.0.0.1')
}
else {
  $knownLanHosts = @($savedHosts + $LanHost) | Sort-Object -Unique
  @('localhost', '127.0.0.1') + @($knownLanHosts)
}
$requestedHosts = @($requestedHosts | Sort-Object -Unique)
Write-Host 'Required HTTPS hosts:'
$requestedHosts | ForEach-Object { Write-Host "  - $_" }

$existingValid = Test-CertificatePair $openssl $certificatePath $keyPath $caPath $requestedHosts
if ($existingValid) {
  Write-Host "Reusing verified mkcert leaf: $certificatePath"
} else {
  if ($ValidateOnly) {
    throw 'CERTIFICATE_VALIDATION_FAILED: -ValidateOnly refused to create or replace the leaf certificate. Run setup once without -ValidateOnly in the preparation terminal.'
  }
  New-Item -ItemType Directory -Force -Path $certDirectory | Out-Null
  $tempCert = Get-AtomicTempPath $certificatePath
  $tempKey = Get-AtomicTempPath $keyPath
  try {
    Invoke-External $mkcert (@('-cert-file', $tempCert, '-key-file', $tempKey) + $requestedHosts) | Out-Null
    if (-not (Test-CertificatePair $openssl $tempCert $tempKey $caPath $requestedHosts)) { throw 'GENERATED_CERTIFICATE_INVALID: generated leaf failed SAN, expiry, issuer or key-match validation.' }
    Move-Item -LiteralPath $tempCert -Destination $certificatePath -Force
    Move-Item -LiteralPath $tempKey -Destination $keyPath -Force
    Write-Host "Created verified multi-LAN mkcert leaf covering: $($requestedHosts -join ', ')"
  } finally {
    Remove-Item -LiteralPath $tempCert, $tempKey -Force -ErrorAction SilentlyContinue
  }
}

if (-not (Test-CertificatePair $openssl $certificatePath $keyPath $caPath $requestedHosts)) { throw 'CERTIFICATE_FINAL_VALIDATION_FAILED: refusing to export runtime configuration.' }

if (-not $Localhost -and -not $ValidateOnly) {
  $knownLanHosts | Set-Content -LiteralPath $hostsFile -Encoding utf8
}

$expectedApiUrl = if ($Localhost) { 'https://localhost:5000' } else { "https://${runtimeLanHost}:5000" }
$expectedCustomerUrl = if ($Localhost) { 'https://localhost:5174' } else { "https://${runtimeLanHost}:5174" }
if (-not [string]::IsNullOrWhiteSpace($env:VITE_API_URL) -and $env:VITE_API_URL -ne $expectedApiUrl) {
  throw "ENV_OVERRIDE_CONFLICT: VITE_API_URL is already '$($env:VITE_API_URL)' but this mode requires '$expectedApiUrl'. Clear the stale variable and rerun."
}
if (-not [string]::IsNullOrWhiteSpace($env:VITE_CUSTOMER_WEB_URL) -and $env:VITE_CUSTOMER_WEB_URL -ne $expectedCustomerUrl) {
  throw "ENV_OVERRIDE_CONFLICT: VITE_CUSTOMER_WEB_URL is already '$($env:VITE_CUSTOMER_WEB_URL)' but this mode requires '$expectedCustomerUrl'. Clear the stale variable and rerun."
}

$env:DOAN_DEV_LAN_HOST = if ($Localhost) { '' } else { $runtimeLanHost }
$env:HTTPS_PEM_CERT_PATH = (Resolve-Path -LiteralPath $certificatePath).Path
$env:HTTPS_PEM_KEY_PATH = (Resolve-Path -LiteralPath $keyPath).Path
$env:VITE_HTTPS_CERT_PATH = $env:HTTPS_PEM_CERT_PATH
$env:VITE_HTTPS_KEY_PATH = $env:HTTPS_PEM_KEY_PATH
$env:VITE_HTTPS = 'true'
$env:VITE_API_URL = $expectedApiUrl
$env:VITE_CUSTOMER_WEB_URL = $expectedCustomerUrl

Write-Host 'HTTPS configuration verified in the current PowerShell process.'
Write-Host ("Mode: {0}" -f $(if ($Localhost) { 'localhost' } else { 'LAN' }))
Write-Host "Certificate: $certificatePath"
if (-not $Localhost) { Write-Host ("LAN SAN hosts: {0}" -f ((@($requestedHosts | Where-Object { $_ -notin @('localhost', '127.0.0.1') })) -join ', ')) }
Write-Host ("API:      {0}" -f $env:VITE_API_URL)
Write-Host ("Admin:    {0}" -f $(if ($Localhost) { 'https://localhost:5173' } else { "https://${runtimeLanHost}:5173" }))
Write-Host ("Customer: {0}" -f $env:VITE_CUSTOMER_WEB_URL)
Write-Host 'Trust requirement: install only the public mkcert root CA on each device; never copy a private key.'
