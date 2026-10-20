using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.Application.Common.Security;

namespace RestaurantPOS.AI.Diagnostics;

public sealed record AiToolExecutionAuditEvent(
    Guid CorrelationId,
    string ToolName,
    string ExecutionState,
    string PolicyOutcome,
    string Role,
    bool BranchScopeMatch,
    string ReadOnlyClassification,
    long DurationMilliseconds,
    string ErrorCategory,
    string Period);

public interface IAiToolExecutionAuditSink
{
    void Record(
        Guid correlationId,
        string toolName,
        AiUserContext userContext,
        ToolRiskLevel? riskLevel,
        string executionState,
        string policyOutcome,
        bool readOnlyMode,
        long durationMilliseconds,
        string errorCategory,
        string? period = null);

    IReadOnlyList<AiToolExecutionAuditEvent> Snapshot();
}

/// <summary>
/// Development-only, bounded and sanitized audit sink for actual AI tool
/// lifecycle events. It is not exposed through an API endpoint.
/// </summary>
public sealed class AiToolExecutionAuditSink : IAiToolExecutionAuditSink
{
    private const int MaximumEvents = 256;
    private static readonly HashSet<string> ApprovedToolNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "get_revenue",
        "update_product_price",
        "update_product_name",
        "create_product",
        "create_topping",
        "get_active_staff",
        "get_order_list",
        "get_best_sellers",
        "get_revenue_comparison",
        "get_business_summary",
        "get_financial_analysis",
        "employee_get_table_summary",
        "get_my_shift",
        "update_order_status",
        "customer_get_menu",
        "get_my_orders",
        "create_booking"
    };

    private static readonly HashSet<string> ApprovedStates = new(StringComparer.Ordinal)
    {
        "SELECTED",
        "AUTHORIZED",
        "EXECUTED",
        "DENIED",
        "FAILED"
    };

    private static readonly HashSet<string> ApprovedPolicyOutcomes = new(StringComparer.Ordinal)
    {
        "SELECTED",
        "AUTHORIZED",
        "DENIED_READ_ONLY",
        "DENIED_PERMISSION",
        "DENIED_TOOL_NOT_FOUND",
        "EXECUTED",
        "FAILED"
    };

    private static readonly HashSet<string> ApprovedErrorCategories = new(StringComparer.Ordinal)
    {
        "NONE",
        "TOOL_RESULT_ERROR",
        "TOOL_EXCEPTION",
        "TOOL_NOT_FOUND",
        "READ_ONLY_BLOCKED",
        "PERMISSION_DENIED"
    };

    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<AiToolExecutionAuditSink> _logger;
    private readonly object _sync = new();
    private readonly Queue<AiToolExecutionAuditEvent> _events = new();

    public AiToolExecutionAuditSink(
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger<AiToolExecutionAuditSink> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    public void Record(
        Guid correlationId,
        string toolName,
        AiUserContext userContext,
        ToolRiskLevel? riskLevel,
        string executionState,
        string policyOutcome,
        bool readOnlyMode,
        long durationMilliseconds,
        string errorCategory,
        string? period = null)
    {
        if (!IsEnabled(readOnlyMode))
            return;

        var safeEvent = new AiToolExecutionAuditEvent(
            correlationId,
            ApprovedToolNames.Contains(toolName) ? toolName : "UNKNOWN",
            ApprovedStates.Contains(executionState) ? executionState : "FAILED",
            ApprovedPolicyOutcomes.Contains(policyOutcome) ? policyOutcome : "FAILED",
            SafeRole(userContext.Role),
            IsBranchScopePresent(userContext),
            SafeRiskLevel(riskLevel),
            Math.Max(0, durationMilliseconds),
            ApprovedErrorCategories.Contains(errorCategory) ? errorCategory : "TOOL_EXCEPTION",
            SafePeriod(period));

        lock (_sync)
        {
            _events.Enqueue(safeEvent);
            while (_events.Count > MaximumEvents)
                _events.Dequeue();
        }

        // The message contains only allow-listed scalar values. The custom
        // Development logger provider applies a second field-level filter.
        _logger.LogWarning(
            "AI_AUDIT_EVENT=TOOL_EXECUTION AI_FAILURE_CODE=NONE AI_PHASE=TOOL_EXECUTION AI_CORRELATION_ID={CorrelationId} AI_TOOL_NAME={ToolName} AI_PERIOD={Period} AI_EXECUTION_STATE={ExecutionState} AI_POLICY_OUTCOME={PolicyOutcome} AI_ROLE={Role} AI_BRANCH_SCOPE_MATCH={BranchScopeMatch} AI_READ_ONLY={ReadOnly} AI_DURATION_MS={DurationMilliseconds} AI_ERROR_CATEGORY={ErrorCategory}",
            safeEvent.CorrelationId,
            safeEvent.ToolName,
            safeEvent.Period,
            safeEvent.ExecutionState,
            safeEvent.PolicyOutcome,
            safeEvent.Role,
            safeEvent.BranchScopeMatch ? "YES" : "NO",
            safeEvent.ReadOnlyClassification,
            safeEvent.DurationMilliseconds,
            safeEvent.ErrorCategory);
    }

    public IReadOnlyList<AiToolExecutionAuditEvent> Snapshot()
    {
        lock (_sync)
        {
            return _events.ToArray();
        }
    }

    private bool IsEnabled(bool readOnlyMode) =>
        (_environment.IsDevelopment() || string.Equals(_environment.EnvironmentName, "Test", StringComparison.OrdinalIgnoreCase) || string.Equals(_environment.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase)) &&
        _configuration.GetValue<bool>("AI:UatLogging:Enabled");

    private bool IsBoolean(string key, bool expected) =>
        bool.TryParse(_configuration[key], out var value) && value == expected;

    private static string SafeRole(string? role) =>
        RoleNames.IsKnownRole(role) ? role!.Trim().ToLowerInvariant() : "UNKNOWN";

    private static bool IsBranchScopePresent(AiUserContext context) =>
        !RoleNames.IsBranchScopedStaffRole(context.Role) || context.BranchId.HasValue;

    private static string SafeRiskLevel(ToolRiskLevel? riskLevel) => riskLevel switch
    {
        ToolRiskLevel.Read => "READ",
        ToolRiskLevel.Write => "WRITE",
        ToolRiskLevel.Destructive => "DESTRUCTIVE",
        _ => "UNKNOWN"
    };

    private static string SafePeriod(string? period) => period is "today" or "yesterday" or "this_week" or "last_week" or "this_month" or "last_month" or "month" or "year" ? period : "unknown";
}
