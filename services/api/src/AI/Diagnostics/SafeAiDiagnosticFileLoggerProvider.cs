using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace RestaurantPOS.AI.Diagnostics;

/// <summary>Development/Test-only allow-listed AI UAT logger.</summary>
public sealed class SafeAiDiagnosticFileLoggerProvider : ILoggerProvider
{
    private static readonly string[] Events = { "AI_HTTP_REQUEST", "AI_E5_DECISION", "AI_E5_ACCESS_DENIED", "AI_EMBEDDING_PILOT_DECISION", "AI_AUDIT_EVENT=TOOL_EXECUTION", "AI_HTTP_RESPONSE", "AI_FAILURE_CODE=" };
    private readonly string _filePath;
    private readonly long _maxBytes;
    private readonly int _retainedFiles;
    private readonly object _sync = new();

    public SafeAiDiagnosticFileLoggerProvider(string filePath, long maxBytes = 5 * 1024 * 1024, int retainedFiles = 3)
    {
        _filePath = filePath;
        _maxBytes = Math.Clamp(maxBytes, 64 * 1024, 100 * 1024 * 1024);
        _retainedFiles = Math.Clamp(retainedFiles, 1, 20);
    }

    public ILogger CreateLogger(string categoryName) => new SafeAiDiagnosticLogger(this);
    public void Dispose() { }

    private void Write(string? message)
    {
        if (string.IsNullOrWhiteSpace(message) || !Events.Any(message.Contains)) return;
        var eventName = message.Contains("AI_AUDIT_EVENT=TOOL_EXECUTION", StringComparison.Ordinal) ? "TOOL_EXECUTION" : Events.FirstOrDefault(message.Contains)?.TrimEnd('=') ?? "AI_EVENT";
        var fields = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["UTC"] = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            ["AI_EVENT"] = eventName
        };
        foreach (var field in new[] { "AI_HTTP_REQUEST", "AI_HTTP_RESPONSE", "AI_E5_DECISION", "AI_E5_ACCESS_DENIED", "AI_EMBEDDING_PILOT_DECISION", "AI_AUDIT_EVENT", "AI_FAILURE_CODE", "AI_PHASE", "AI_SUBPHASE", "AI_CORRELATION_ID", "AI_CONTEXT_RESOLUTION", "AI_DECISION", "AI_INTENT", "AI_PREDICTED_INTENT", "AI_SECOND_INTENT", "AI_TOOL_NAME", "AI_PERIOD", "AI_REASON", "AI_ROUTE", "AI_ROLE", "AI_FEATURE_FLAG", "AI_ENVIRONMENT", "AI_HTTP_STATUS", "AI_ERROR_CODE", "AI_EXECUTION_STATE", "AI_POLICY_OUTCOME", "AI_BRANCH_SCOPE_MATCH", "AI_READ_ONLY", "AI_DURATION_MS", "AI_ERROR_CATEGORY", "AI_SECOND_SCORE", "AI_TOP_SCORE", "AI_MARGIN", "AI_OOD_SCORE", "AI_OOD_GAP", "AI_SCORE_THRESHOLD", "AI_MARGIN_THRESHOLD", "AI_OOD_SCORE_THRESHOLD", "AI_OOD_GAP_THRESHOLD", "AI_BOUNDARY", "AI_TOOL_EXECUTED", "TOOL_EXECUTED", "ContextResolution", "TopScore", "SecondScore", "Margin", "OodScore", "OodGap", "ScoreThreshold", "MarginThreshold", "OodScoreThreshold", "OodGapThreshold", "Boundary" })
        {
            var pattern = field == "AI_ROUTE" ? @"[A-Za-z0-9_./:-]+" : field == "AI_REASON" || field == "AI_ERROR_CODE" ? @"[A-Za-z0-9_.:-]+" : field == "AI_CORRELATION_ID" ? @"[0-9a-fA-F-]{36}" : @"[A-Za-z0-9_./:-]+";
            var match = Regex.Match(message, $@"(?:^|\s){Regex.Escape(field)}=(?<value>{pattern})(?=\s|$)", RegexOptions.CultureInvariant);
            if (match.Success) fields[field] = match.Groups["value"].Value;
        }
        AddAlias(fields, message, "AI_CORRELATION_ID", "CorrelationId", "[0-9a-fA-F-]{36}");
        AddAlias(fields, message, "AI_CONTEXT_RESOLUTION", "ContextResolution", "[A-Za-z0-9_./:-]+");
        AddAlias(fields, message, "AI_DECISION", "Decision", "[A-Za-z0-9_./:-]+");
        AddAlias(fields, message, "AI_REASON", "Reason", "[A-Za-z0-9_./:-]+");
        AddAlias(fields, message, "AI_INTENT", "Intent", "[A-Za-z0-9_./:-]+");
        AddAlias(fields, message, "AI_TOOL_NAME", "Tool", "[A-Za-z0-9_./:-]+");
        AddAlias(fields, message, "AI_PREDICTED_INTENT", "PredictedIntent", "[A-Za-z0-9_./:-]+");
        AddAlias(fields, message, "AI_SECOND_INTENT", "SecondIntent", "[A-Za-z0-9_./:-]+");
        AddAlias(fields, message, "AI_SECOND_SCORE", "SecondScore", "[A-Za-z0-9_./:-]+");
        AddAlias(fields, message, "AI_TOP_SCORE", "TopScore", "[A-Za-z0-9_./:-]+");
        AddAlias(fields, message, "AI_MARGIN", "Margin", "[A-Za-z0-9_./:-]+");
        AddAlias(fields, message, "AI_OOD_SCORE", "OodScore", "[A-Za-z0-9_./:-]+");
        AddAlias(fields, message, "AI_OOD_GAP", "OodGap", "[A-Za-z0-9_./:-]+");
        AddAlias(fields, message, "AI_SCORE_THRESHOLD", "ScoreThreshold", "[A-Za-z0-9_./:-]+");
        AddAlias(fields, message, "AI_MARGIN_THRESHOLD", "MarginThreshold", "[A-Za-z0-9_./:-]+");
        AddAlias(fields, message, "AI_OOD_SCORE_THRESHOLD", "OodScoreThreshold", "[A-Za-z0-9_./:-]+");
        AddAlias(fields, message, "AI_OOD_GAP_THRESHOLD", "OodGapThreshold", "[A-Za-z0-9_./:-]+");
        AddAlias(fields, message, "AI_BOUNDARY", "Boundary", "[A-Za-z0-9_./:-]+");
        var line = string.Join(" ", fields.Select(pair => $"{pair.Key}={pair.Value}"));
        try
        {
            lock (_sync)
            {
                var directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
                RotateIfNeeded(line.Length + Environment.NewLine.Length);
                File.AppendAllText(_filePath, line + Environment.NewLine, Encoding.UTF8);
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    private static void AddAlias(Dictionary<string, string> fields, string message, string outputName, string inputName, string pattern)
    {
        if (fields.ContainsKey(outputName)) return;
        var match = Regex.Match(message, $@"(?:^|\s){Regex.Escape(inputName)}=(?<value>{pattern})(?=\s|$)", RegexOptions.CultureInvariant);
        if (match.Success) fields[outputName] = match.Groups["value"].Value;
    }

    private void RotateIfNeeded(long nextBytes)
    {
        if (!File.Exists(_filePath) || new FileInfo(_filePath).Length + nextBytes <= _maxBytes) return;
        for (var index = _retainedFiles - 1; index >= 1; index--)
        {
            var source = $"{_filePath}.{index}";
            var target = $"{_filePath}.{index + 1}";
            if (File.Exists(target)) File.Delete(target);
            if (File.Exists(source)) File.Move(source, target);
        }
        var first = $"{_filePath}.1";
        if (File.Exists(first)) File.Delete(first);
        File.Move(_filePath, first);
    }

    private sealed class SafeAiDiagnosticLogger : ILogger
    {
        private readonly SafeAiDiagnosticFileLoggerProvider _provider;
        public SafeAiDiagnosticLogger(SafeAiDiagnosticFileLoggerProvider provider) => _provider = provider;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { if (IsEnabled(logLevel)) _provider.Write(formatter(state, exception)); }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}
