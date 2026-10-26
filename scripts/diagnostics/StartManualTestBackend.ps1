[CmdletBinding()]
param(
    [string]$RepositoryRoot,
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$testProjectRef = 'lizpcvayxcuwjgrxsvfe'
$RepositoryRoot = if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
} else {
    (Resolve-Path -LiteralPath $RepositoryRoot).Path
}

$apiDirectory = Join-Path $RepositoryRoot 'services\api'
$apiDll = Join-Path $apiDirectory "bin\$Configuration\net8.0\RestaurantPOS.api.dll"
$attestationDirectory = Join-Path $RepositoryRoot 'evidence\ai-diagnostics\runtime-attestations'
$correlationId = [Guid]::NewGuid().ToString('D')
$attestationPath = Join-Path $attestationDirectory "runtime-attestation-$correlationId.json"
$verifier = Join-Path $RepositoryRoot 'scripts\diagnostics\VerifyRuntimeAttestation.ps1'

function Fail-Safely([string]$Code) {
    Write-Output "TEST_BACKEND_START: BLOCKED_CODE_$Code"
    exit 1
}

function Get-CanonicalIdentity([System.Security.Principal.IdentityReference]$IdentityReference) {
    try { return $IdentityReference.Translate([System.Security.Principal.NTAccount]).Value }
    catch { return $IdentityReference.Value }
}

function Ensure-RestrictedAttestationDirectory {
    if (-not (Test-Path -LiteralPath $attestationDirectory)) {
        New-Item -ItemType Directory -Path $attestationDirectory -Force | Out-Null
        $current = [System.Security.Principal.WindowsIdentity]::GetCurrent()
        if ($null -eq $current -or [string]::IsNullOrWhiteSpace($current.Name)) { Fail-Safely 'WINDOWS_IDENTITY_UNAVAILABLE' }
        $acl = New-Object System.Security.AccessControl.DirectorySecurity
        $acl.SetAccessRuleProtection($true, $false)
        $identity = New-Object System.Security.Principal.NTAccount($current.Name)
        $rule = [System.Security.AccessControl.FileSystemAccessRule]::new(
            $identity,
            ([System.Security.AccessControl.FileSystemRights]::Modify),
            ([System.Security.AccessControl.InheritanceFlags]::ContainerInherit -bor [System.Security.AccessControl.InheritanceFlags]::ObjectInherit),
            [System.Security.AccessControl.PropagationFlags]::None,
            [System.Security.AccessControl.AccessControlType]::Allow)
        $acl.AddAccessRule($rule)
        Set-Acl -LiteralPath $attestationDirectory -AclObject $acl
    }

    $current = [System.Security.Principal.WindowsIdentity]::GetCurrent()
    if ($null -eq $current -or [string]::IsNullOrWhiteSpace($current.Name)) { Fail-Safely 'WINDOWS_IDENTITY_UNAVAILABLE' }
    $acl = Get-Acl -LiteralPath $attestationDirectory
    $currentName = Get-CanonicalIdentity (New-Object System.Security.Principal.NTAccount($current.Name))
    $ownerName = Get-CanonicalIdentity (New-Object System.Security.Principal.NTAccount($acl.Owner))
    $allowedRights = [System.Security.AccessControl.FileSystemRights]::Modify -bor [System.Security.AccessControl.FileSystemRights]::Synchronize
    $rules = @($acl.Access)
    if ($ownerName -ne $currentName -or -not $acl.AreAccessRulesProtected -or $rules.Count -ne 1) { Fail-Safely 'ATTESTATION_DIRECTORY_SECURITY_FAILED' }
    $rule = $rules[0]
    $ruleName = Get-CanonicalIdentity $rule.IdentityReference
    if ($ruleName -ne $currentName -or
        $rule.AccessControlType -ne [System.Security.AccessControl.AccessControlType]::Allow -or
        $rule.FileSystemRights -ne $allowedRights -or
        $rule.InheritanceFlags -ne ([System.Security.AccessControl.InheritanceFlags]::ContainerInherit -bor [System.Security.AccessControl.InheritanceFlags]::ObjectInherit) -or
        $rule.PropagationFlags -ne [System.Security.AccessControl.PropagationFlags]::None -or
        $rule.IsInherited) { Fail-Safely 'ATTESTATION_DIRECTORY_SECURITY_FAILED' }
}

if (-not (Test-Path -LiteralPath $apiDll -PathType Leaf)) { Fail-Safely 'API_BUILD_NOT_FOUND' }
if (-not (Test-Path -LiteralPath $verifier -PathType Leaf)) { Fail-Safely 'VERIFIER_NOT_FOUND' }
$existingListener = Get-NetTCPConnection -State Listen -LocalPort 5000 -ErrorAction SilentlyContinue
if ($null -ne $existingListener) { Fail-Safely 'BACKEND_ALREADY_RUNNING' }
Ensure-RestrictedAttestationDirectory

$startInfo = [System.Diagnostics.ProcessStartInfo]::new()
$startInfo.FileName = 'dotnet'
$startInfo.Arguments = '"' + $apiDll + '"'
$startInfo.WorkingDirectory = $apiDirectory
$startInfo.UseShellExecute = $false
$startInfo.CreateNoWindow = $true
$startInfo.RedirectStandardOutput = $true
$startInfo.RedirectStandardError = $true
$startInfo.Environment['ASPNETCORE_ENVIRONMENT'] = 'Development'
$startInfo.Environment['Database__TargetMode'] = 'Test'
$startInfo.Environment['Database__ExpectedProjectRef'] = $testProjectRef
$startInfo.Environment['DatabaseSafety__AllowAutomaticMigration'] = 'false'
$startInfo.Environment['AdminE2E__RuntimeAttestation__Enabled'] = 'true'
$startInfo.Environment['AdminE2E__RuntimeAttestation__LaunchCorrelationId'] = $correlationId
$startInfo.Environment['AdminE2E__RuntimeAttestation__AttestationPath'] = $attestationPath
$startInfo.Environment['DEV_SUPABASE_PROJECT_REF'] = $testProjectRef

$backend = [System.Diagnostics.Process]::new()
$backend.StartInfo = $startInfo
[void]$backend.Start()
$backendStartTimeUtc = [DateTimeOffset]$backend.StartTime.ToUniversalTime()
$stdoutTask = $backend.StandardOutput.ReadToEndAsync()
$stderrTask = $backend.StandardError.ReadToEndAsync()

try {
    $ready = $false
    $deadline = [DateTime]::UtcNow.AddSeconds(45)
    while ([DateTime]::UtcNow -lt $deadline) {
        if ($backend.HasExited) { Fail-Safely 'BACKEND_EXITED_BEFORE_READY' }
        if (Test-Path -LiteralPath $attestationPath -PathType Leaf) {
            & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $verifier `
                -AttestationPath $attestationPath `
                -ExpectedPid $backend.Id `
                -ExpectedProcessStartTimeUtc $backendStartTimeUtc `
                -ExpectedCorrelationId $correlationId `
                -ExpectedMode AdminE2E `
                -ExpectedTargetMode Test `
                -ExpectedProjectRef $testProjectRef | Out-Null
            if ($LASTEXITCODE -ne 0) { Fail-Safely 'RUNTIME_ATTESTATION_REJECTED' }

            Add-Type -AssemblyName System.Net.Http -ErrorAction Stop
            $client = New-Object -TypeName System.Net.Http.HttpClient
            $client.Timeout = [TimeSpan]::FromSeconds(2)
            try {
                $response = $client.GetAsync('https://localhost:5000/health').GetAwaiter().GetResult()
                if ([int]$response.StatusCode -eq 200) {
                    Write-Output "TEST_BACKEND_START: STARTED_TEST_ATTESTATION_READY PID_$($backend.Id)"
                    Write-Output "TEST_BACKEND_START: ATTESTATION_FILE_$attestationPath"
                    Write-Output 'TEST_BACKEND_START: DATABASE_AUTOMIGRATION_DISABLED'
                    Write-Output 'TEST_BACKEND_START: AI_PROVIDER_MOCK'
                    $ready = $true
                    return
                }
            }
            finally { $client.Dispose() }
        }
        Start-Sleep -Milliseconds 500
    }
    Fail-Safely 'READINESS_TIMEOUT'
}
finally {
    # Only terminate a child created by this invocation when startup was blocked.
    if ($backend.HasExited -eq $false -and -not $ready) { $backend.Kill() }
    $backend.Dispose()
}
