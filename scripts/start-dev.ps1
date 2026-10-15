[CmdletBinding()]
param(
  [ValidateSet('Localhost', 'Lan')]
  [string]$Mode = 'Localhost',
  [string]$LanHost,
  [switch]$Stop
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$statePath = Join-Path ([IO.Path]::GetTempPath()) 'restaurant-pos-dev-processes.json'

function Get-DescendantProcessIds([int[]]$RootIds) {
  $all = @(Get-CimInstance Win32_Process -ErrorAction SilentlyContinue)
  $ids = [System.Collections.Generic.HashSet[int]]::new()
  $queue = [System.Collections.Generic.Queue[int]]::new()
  foreach ($rootId in $RootIds) { [void]$ids.Add($rootId); $queue.Enqueue($rootId) }
  while ($queue.Count -gt 0) {
    $parentId = $queue.Dequeue()
    foreach ($child in $all | Where-Object { $_.ParentProcessId -eq $parentId }) {
      if ($ids.Add([int]$child.ProcessId)) { $queue.Enqueue([int]$child.ProcessId) }
    }
  }
  return @($ids)
}

function Stop-TrackedProcesses([int[]]$RootIds) {
  $ids = @(Get-DescendantProcessIds $RootIds) | Sort-Object -Descending
  foreach ($id in $ids) {
    Stop-Process -Id $id -Force -ErrorAction SilentlyContinue
  }
}

if ($Stop) {
  if (-not (Test-Path -LiteralPath $statePath)) {
    Write-Host 'No launcher state was found.'
    exit 0
  }
  $state = @(Get-Content -Raw -LiteralPath $statePath | ConvertFrom-Json)
  $rootIds = @($state | ForEach-Object { [int]$_.Pid })
  Stop-TrackedProcesses $rootIds
  Remove-Item -LiteralPath $statePath -Force -ErrorAction SilentlyContinue
  Write-Host 'Stopped only processes recorded by this launcher.'
  exit 0
}

$ports = @(5000, 5173, 5174)
foreach ($port in $ports) {
  $listeners = @(Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction SilentlyContinue)
  if ($listeners.Count -gt 0) {
    $details = $listeners | ForEach-Object {
      $process = Get-Process -Id $_.OwningProcess -ErrorAction SilentlyContinue
      "PID $($_.OwningProcess) $($process.ProcessName)"
    }
    throw "PORT_IN_USE: port $port is already used by $($details -join ', '). Stop the owning application explicitly; this launcher will not kill it."
  }
}

$setup = Join-Path $repoRoot 'scripts\setup-https.ps1'
if (-not (Test-Path -LiteralPath $setup)) { throw 'HTTPS_SETUP_NOT_FOUND' }

# Dot-sourcing is intentional: setup-https exports the verified certificate and
# URL variables into this process so all three children receive one configuration.
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:DatabaseSafety__AllowAutomaticMigration = 'false'
if ($Mode -eq 'Lan') {
  . $setup -LanHost $LanHost
} else {
  . $setup -Localhost
}

$processes = [System.Collections.Generic.List[object]]::new()
function Start-Tracked([string]$FilePath, [string[]]$Arguments, [string]$WorkingDirectory, [string]$Name) {
  $process = Start-Process -FilePath $FilePath -ArgumentList $Arguments -WorkingDirectory $WorkingDirectory -NoNewWindow -PassThru
  $processes.Add([pscustomobject]@{ Name = $Name; Pid = $process.Id; Process = $process })
  Write-Host ("STARTED {0}: PID {1}" -f $Name, $process.Id)
}

try {
  Start-Tracked 'dotnet' @('run', '--project', (Join-Path $repoRoot 'services\api\RestaurantPOS.api.csproj'), '--no-launch-profile') $repoRoot 'Backend'
  Start-Tracked 'npm.cmd' @('run', 'dev') (Join-Path $repoRoot 'apps\admin-web') 'Admin Web'
  Start-Tracked 'npm.cmd' @('run', 'dev') (Join-Path $repoRoot 'apps\customer-web') 'Customer Web'

  $state = @($processes | ForEach-Object { [pscustomobject]@{ Name = $_.Name; Pid = $_.Pid } })
  $state | ConvertTo-Json | Set-Content -LiteralPath $statePath -Encoding UTF8
  Write-Host "State file: $statePath"
  Write-Host 'Press Ctrl+C to stop processes created by this launcher, or run scripts\start-dev.ps1 -Stop.'
  Write-Host ("API: {0}" -f $env:VITE_API_URL)
  if ($Mode -eq 'Lan') {
    Write-Host ("Admin: https://${LanHost}:5173")
    Write-Host ("Customer: https://${LanHost}:5174")
  } else {
    Write-Host 'Admin: https://localhost:5173'
    Write-Host 'Customer: https://localhost:5174'
  }

  while ($true) {
    Start-Sleep -Seconds 1
    $exited = @($processes | Where-Object { $_.Process.HasExited })
    if ($exited.Count -gt 0) {
      foreach ($item in $exited) { Write-Warning "$($item.Name) exited with code $($item.Process.ExitCode)." }
      break
    }
  }
} finally {
  if ($processes.Count -gt 0) {
    Stop-TrackedProcesses @($processes | ForEach-Object { $_.Pid })
  }
  Remove-Item -LiteralPath $statePath -Force -ErrorAction SilentlyContinue
}
