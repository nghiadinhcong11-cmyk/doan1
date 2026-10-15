using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace RestaurantPOS.AI.Diagnostics;

/// <summary>
/// Writes only the allow-listed AI diagnostic fields to a local Development file.
/// It deliberately ignores all other log messages and never serializes exception text.
/// </summary>
public sealed class SafeAiDiagnosticFileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private readonly object _sync = new();

    public SafeAiDiagnosticFileLoggerProvider(string filePath)
    {
        _filePath = filePath;
    }

    public ILogger CreateLogger(string categoryName) => new SafeAiDiagnosticLogger(this);

    public void Dispose()
    {
    }

    private void Write(string? message)
    {
        var isFailure = message?.Contains("AI_FAILURE_CODE=", StringComparison.Ordinal) == true;
        var isToolAudit = message?.Contains("AI_AUDIT_EVENT=TOOL_EXECUTION", StringComparison.Ordinal) == true;
        if (string.IsNullOrWhiteSpace(message) || (!isFailure && !isToolAudit))
        {
            return;
        }

        var code = ReadField(message, "AI_FAILURE_CODE", "[A-Z0-9_]+");
        var phase = ReadField(message, "AI_PHASE", "[A-Za-z0-9_]+");
        var subphase = ReadField(message, "AI_SUBPHASE", "[A-Z0-9_]+");
        var correlationId = ReadField(message, "AI_CORRELATION_ID", "[0-9a-fA-F-]{36}");
        var toolExecuted = ReadField(message, "TOOL_EXECUTED", "YES|NO");
        var duration = ReadField(message, "AI_DURATION_MS", "[0-9]+");
        var toolName = ReadField(message, "AI_TOOL_NAME", "[A-Za-z0-9_:-]+");

        if (code == "UNKNOWN" || phase == "UNKNOWN")
        {
            return;
        }

        var line = string.Join(
            " ",
            $"UTC={DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)}",
            $"AI_FAILURE_CODE={code}",
            $"AI_PHASE={phase}",
            $"AI_SUBPHASE={subphase}",
            $"AI_CORRELATION_ID={correlationId}",
            $"TOOL_EXECUTED={toolExecuted}",
            $"AI_DURATION_MS={duration}",
            $"AI_TOOL_NAME={toolName}");

        if (isToolAudit)
        {
            var executionState = ReadField(message, "AI_EXECUTION_STATE", "[A-Z_]+");
            var policyOutcome = ReadField(message, "AI_POLICY_OUTCOME", "[A-Z_]+");
            var role = ReadField(message, "AI_ROLE", "[a-z]+|UNKNOWN");
            var branchScopeMatch = ReadField(message, "AI_BRANCH_SCOPE_MATCH", "YES|NO");
            var readOnly = ReadField(message, "AI_READ_ONLY", "[A-Z]+");
            var errorCategory = ReadField(message, "AI_ERROR_CATEGORY", "[A-Z_]+");
            // Keep the audit fields separated from the base AI_TOOL_NAME field.
            // Without this boundary, downstream allow-listed parsers can read a
            // malformed concatenated tool name and cannot prove tool identity.
            line += " " + string.Join(
                " ",
                $"AI_AUDIT_EVENT=TOOL_EXECUTION",
                $"AI_EXECUTION_STATE={executionState}",
                $"AI_POLICY_OUTCOME={policyOutcome}",
                $"AI_ROLE={role}",
                $"AI_BRANCH_SCOPE_MATCH={branchScopeMatch}",
                $"AI_READ_ONLY={readOnly}",
                $"AI_ERROR_CATEGORY={errorCategory}");
        }

        try
        {
            lock (_sync)
            {
                var directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.AppendAllText(_filePath, line + Environment.NewLine, Encoding.UTF8);
            }
        }
        catch (IOException)
        {
            // Diagnostics must never change the outcome of an AI request.
        }
        catch (UnauthorizedAccessException)
        {
            // Diagnostics must never change the outcome of an AI request.
        }
    }

    private static string ReadField(string message, string fieldName, string valuePattern)
    {
        var match = Regex.Match(
            message,
            $@"(?:^|\s){Regex.Escape(fieldName)}=(?<value>{valuePattern})(?=\s|$)",
            RegexOptions.CultureInvariant);

        return match.Success ? match.Groups["value"].Value : "UNKNOWN";
    }

    private sealed class SafeAiDiagnosticLogger : ILogger
    {
        private readonly SafeAiDiagnosticFileLoggerProvider _provider;

        public SafeAiDiagnosticLogger(SafeAiDiagnosticFileLoggerProvider provider)
        {
            _provider = provider;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                _provider.Write(formatter(state, exception));
            }
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }
}
