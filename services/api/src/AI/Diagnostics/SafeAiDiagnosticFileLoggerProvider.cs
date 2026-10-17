using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace RestaurantPOS.AI.Diagnostics;

/// <summary>Development/Test-only allow-listed AI UAT logger.</summary>
public sealed class SafeAiDiagnosticFileLoggerProvider : ILoggerProvider
{
    private static readonly string[] Events = { "AI_HTTP_REQUEST", "AI_EMBEDDING_PILOT_DECISION", "AI_AUDIT_EVENT=TOOL_EXECUTION", "AI_HTTP_RESPONSE", "AI_FAILURE_CODE=" };
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
        foreach (var field in new[] { "AI_HTTP_REQUEST", "AI_HTTP_RESPONSE", "AI_AUDIT_EVENT", "AI_FAILURE_CODE", "AI_PHASE", "AI_SUBPHASE", "AI_CORRELATION_ID", "AI_DECISION", "AI_INTENT", "AI_TOOL_NAME", "AI_PERIOD", "AI_REASON", "AI_ROUTE", "AI_ROLE", "AI_FEATURE_FLAG", "AI_ENVIRONMENT", "AI_HTTP_STATUS", "AI_ERROR_CODE", "AI_EXECUTION_STATE", "AI_POLICY_OUTCOME", "AI_BRANCH_SCOPE_MATCH", "AI_READ_ONLY", "AI_DURATION_MS", "TOOL_EXECUTED", "AI_ERROR_CATEGORY" })
        {
            var pattern = field == "AI_ROUTE" ? @"[A-Za-z0-9_./:-]+" : field == "AI_REASON" || field == "AI_ERROR_CODE" ? @"[A-Za-z0-9_.:-]+" : field == "AI_CORRELATION_ID" ? @"[0-9a-fA-F-]{36}" : @"[A-Za-z0-9_./:-]+";
            var match = Regex.Match(message, $@"(?:^|\s){Regex.Escape(field)}=(?<value>{pattern})(?=\s|$)", RegexOptions.CultureInvariant);
            if (match.Success) fields[field] = match.Groups["value"].Value;
        }
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
