[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][ValidateScript({ Test-Path -LiteralPath $_ })][string]$AttestationPath,
    [Parameter(Mandatory = $true)][int]$ExpectedPid,
    [Parameter(Mandatory = $true)][DateTimeOffset]$ExpectedProcessStartTimeUtc,
    [Parameter(Mandatory = $true)][ValidatePattern('^[0-9a-fA-F-]{36}$')][string]$ExpectedCorrelationId,
    [ValidateSet('ManagerAB', 'Customer', 'AdminE2E')][string]$ExpectedMode = 'ManagerAB',
    [Parameter(Mandatory = $true)][ValidateSet('Test', 'Main')][string]$ExpectedTargetMode,
    [Parameter(Mandatory = $true)][ValidatePattern('^[a-z0-9]{20,32}$')][string]$ExpectedProjectRef
)

$ErrorActionPreference = 'Stop'
$result = [ordered]@{
    schema = $false
    process = $false
    startTime = $false
    correlation = $false
    fresh = $false
    target = $false
    safety = $false
}

try {
    $attestation = Get-Content -LiteralPath $AttestationPath -Raw | ConvertFrom-Json
    $result.schema = $attestation.schemaVersion -eq 'runtime-attestation-v1' -and $attestation.status -eq 'READY'
    $process = Get-Process -Id $ExpectedPid -ErrorAction Stop
    $result.process = $process.ProcessName -eq 'dotnet'
    $actualStart = [DateTimeOffset]$process.StartTime.ToUniversalTime()
    $attestationStart = [DateTimeOffset]$attestation.processStartTimeUtc
    $result.startTime = [Math]::Abs(($actualStart - $ExpectedProcessStartTimeUtc.ToUniversalTime()).TotalSeconds) -lt 1 -and
        [Math]::Abs(($actualStart - $attestationStart.ToUniversalTime()).TotalSeconds) -lt 1
    $result.correlation = $attestation.correlationId -eq $ExpectedCorrelationId
    $result.target = $attestation.databaseTargetMode -eq $ExpectedTargetMode -and $attestation.databaseProjectRef -eq $ExpectedProjectRef
    $now = [DateTimeOffset]::UtcNow
    $generated = [DateTimeOffset]$attestation.generatedAtUtc
    $validUntil = [DateTimeOffset]$attestation.validUntilUtc
    $result.fresh = $generated -le $now -and $validUntil -gt $now -and ($validUntil - $generated).TotalMinutes -le 15
    $safety = $attestation.safety
    $allowlist = if ($ExpectedMode -eq 'Customer') { $safety.customerAllowlist -eq 'VERIFIED' } elseif ($ExpectedMode -eq 'AdminE2E') { $safety.managerAllowlist -eq 'NOT_APPLICABLE' -and $safety.customerAllowlist -eq 'NOT_APPLICABLE' } else { $safety.managerAllowlist -eq 'VERIFIED' }
    $readOnly = if ($ExpectedMode -eq 'AdminE2E') { $safety.adminE2EReadOnly -eq $true } else { $safety.aiReadOnlyEnforced -eq $true }
    $result.safety = $safety.environmentDevelopment -and $safety.runtimeMode -eq $ExpectedMode -and $safety.approvedDevelopmentTarget -and
        $safety.mockProvider -and $readOnly -and
        $safety.automaticMigrationsDisabled -and $safety.backgroundInsightsDisabled -and
        $safety.externalAiDisabled -and $allowlist
}
catch {
    # Never expose process arguments, secrets or raw exception text.
}

$ready = @($result.Values) -notcontains $false
foreach($key in $result.Keys) { Write-Output ("ATTESTATION_{0}={1}" -f $key.ToUpperInvariant(), $(if($result[$key]){'PASS'}else{'FAIL'})) }
Write-Output ("SAFE_RUNTIME_READY={0}" -f $(if($ready){'YES'}else{'NO'}))
exit $(if($ready){0}else{1})
