using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace RestaurantPOS.WebAPI.Middleware;

/// <summary>
/// Emits only bounded metadata for intermittent GET /api/Auth/me failures.
/// It does not inspect or log claims, tokens, identities, or response bodies.
/// </summary>
public sealed class SafeAuthMeDiagnosticsMiddleware
{
    private const string CorrelationHeader = "X-Auth-Me-Diagnostic-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<SafeAuthMeDiagnosticsMiddleware> _logger;

    public SafeAuthMeDiagnosticsMiddleware(
        RequestDelegate next,
        ILogger<SafeAuthMeDiagnosticsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!HttpMethods.IsGet(context.Request.Method)
            || !context.Request.Path.Equals("/api/Auth/me", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var correlationId = CreateCorrelationId();
        var started = Stopwatch.GetTimestamp();
        context.Response.Headers[CorrelationHeader] = correlationId;

        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                "AUTH_ME_DIAGNOSTIC CODE=AUTH_ME_UNHANDLED_EXCEPTION PHASE=AUTH_ME_PIPELINE EXCEPTION_TYPE={ExceptionType} CORRELATION_ID={CorrelationId} HTTP_STATUS=500 DURATION_MS={DurationMs}",
                GetSafeExceptionType(exception),
                correlationId,
                GetDurationMilliseconds(started));
            throw;
        }

        if (context.Response.StatusCode >= StatusCodes.Status500InternalServerError)
        {
            var handledException = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;
            _logger.LogWarning(
                "AUTH_ME_DIAGNOSTIC CODE=AUTH_ME_HTTP_500 PHASE=AUTH_ME_RESPONSE EXCEPTION_TYPE={ExceptionType} CORRELATION_ID={CorrelationId} HTTP_STATUS={HttpStatus} DURATION_MS={DurationMs}",
                handledException is null ? "NOT_OBSERVED" : GetSafeExceptionType(handledException),
                correlationId,
                context.Response.StatusCode,
                GetDurationMilliseconds(started));
        }
    }

    private static string CreateCorrelationId()
        => Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();

    private static long GetDurationMilliseconds(long started)
        => (long)Stopwatch.GetElapsedTime(started).TotalMilliseconds;

    private static string GetSafeExceptionType(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            var typeName = current.GetType().Name;
            if (typeName is "AuthenticationException"
                or "ArgumentException"
                or "CryptographicException"
                or "FormatException"
                or "InvalidOperationException"
                or "IOException"
                or "JsonException"
                or "NpgsqlException"
                or "PostgresException"
                or "TaskCanceledException"
                or "TimeoutException")
            {
                return typeName;
            }
        }

        return "UnknownException";
    }
}
