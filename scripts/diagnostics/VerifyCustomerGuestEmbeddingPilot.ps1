[CmdletBinding()]
param([string]$RepositoryRoot)

$ErrorActionPreference = "Stop"
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) { $RepositoryRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).Path }
$pilot = Join-Path $RepositoryRoot "services\api\src\AI\Services\CustomerGuestEmbeddingPilot.cs"
$orchestrator = Join-Path $RepositoryRoot "services\api\src\AI\Services\AiOrchestrator.cs"
$permission = Join-Path $RepositoryRoot "services\api\src\AI\Services\AiPermissionService.cs"
$settings = Join-Path $RepositoryRoot "services\api\appsettings.json"
$developmentSettings = Join-Path $RepositoryRoot "services\api\appsettings.Development.json"

foreach ($path in @($pilot, $orchestrator, $permission, $settings, $developmentSettings)) {
    if (-not (Test-Path -LiteralPath $path)) { throw "Missing required file: $path" }
}

$pilotText = Get-Content -LiteralPath $pilot -Raw
$orchestratorText = Get-Content -LiteralPath $orchestrator -Raw
$permissionText = Get-Content -LiteralPath $permission -Raw
$settingsText = Get-Content -LiteralPath $settings -Raw
$developmentText = Get-Content -LiteralPath $developmentSettings -Raw

$checks = [ordered]@{
    "feature_flag_default_off" = (($settingsText -match 'CustomerGuestEmbeddingPilot') -and ($settingsText -match '"Enabled"\s*:\s*false'))
    "development_feature_flag_default_off" = (($developmentText -match 'CustomerGuestEmbeddingPilot') -and ($developmentText -match '"Enabled"\s*:\s*false'))
    "canonical_e5_paths" = (($pilotText -match 'ModelPath') -and ($pilotText -match 'TokenizerPath') -and ($pilotText -match 'query: '))
    "guest_requires_verified_session_and_branch" = ($pilotText -match 'HasVerifiedGuestSession.*BranchId')
    "guest_only_menu_tool_permission" = (($permissionText -match 'customer_get_menu') -and ($permissionText -match 'tool\.RiskLevel == ToolRiskLevel\.Read'))
    "no_mock_fallback_in_embedding_branch" = (($orchestratorText -match 'CustomerGuestEmbeddingPilot') -and ($orchestratorText -match 'Customer/Guest AI is disabled') -and ($orchestratorText -notmatch 'MockAiIntentRecognizer|_aiProvider|GenerateContentAsync'))
    "guest_scope_excludes_orders" = ($pilotText -match 'GuestOrdersDenied')
    "guest_order_boundary_is_token_aware" = (($pilotText -match 'IsGuestOrderRequest') -and ($pilotText -match 'thuc '))
    "guest_order_paraphrases_blocked" = (($pilotText -match 'da dat') -and ($pilotText -match 'da goi'))
    "write_boundary_rejected" = ($pilotText -match 'WriteOrDestructiveRequest')
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value })
$checks | Format-Table -AutoSize
if ($failed.Count -gt 0) { throw "Customer/Guest pilot static checks failed: $($failed.Name -join ', ')" }
Write-Output "Customer/Guest embedding pilot static checks passed: $($checks.Count)/$($checks.Count)"
