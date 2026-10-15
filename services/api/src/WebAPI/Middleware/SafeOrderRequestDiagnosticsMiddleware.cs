using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RestaurantPOS.Application.Common.Orders;

namespace RestaurantPOS.WebAPI.Middleware
{
    /// <summary>
    /// Records sanitized failures for GET /api/Order without suppressing or
    /// rewriting the original exception or response.
    /// </summary>
    public sealed class SafeOrderRequestDiagnosticsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SafeOrderRequestDiagnosticsMiddleware> _logger;

        public SafeOrderRequestDiagnosticsMiddleware(
            RequestDelegate next,
            ILogger<SafeOrderRequestDiagnosticsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!HttpMethods.IsGet(context.Request.Method)
                || !context.Request.Path.Equals("/api/Order", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            var correlationId = OrderRequestDiagnostics.CreateCorrelationId();
            var started = Stopwatch.GetTimestamp();
            context.Items[OrderRequestDiagnostics.CorrelationItemKey] = correlationId;
            context.Items[OrderRequestDiagnostics.PhaseItemKey] = "CONTROLLER_ENTRY";
            context.Response.Headers["X-Order-Diagnostic-Id"] = correlationId;

            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    "ORDER_GET_DIAGNOSTIC CODE=ORDER_GET_UNHANDLED_EXCEPTION PHASE={Phase} EXCEPTION_TYPE={ExceptionType} CORRELATION_ID={CorrelationId} HTTP_STATUS=500 DURATION_MS={DurationMs}",
                    GetPhase(context),
                    OrderRequestDiagnostics.GetSafeExceptionType(exception),
                    correlationId,
                    GetDurationMilliseconds(started));
                throw;
            }

            if (context.Response.StatusCode >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogWarning(
                    "ORDER_GET_DIAGNOSTIC CODE=ORDER_GET_HTTP_500 PHASE={Phase} EXCEPTION_TYPE=NOT_OBSERVED CORRELATION_ID={CorrelationId} HTTP_STATUS={HttpStatus} DURATION_MS={DurationMs}",
                    GetPhase(context),
                    correlationId,
                    context.Response.StatusCode,
                    GetDurationMilliseconds(started));
            }
        }

        private static string GetPhase(HttpContext context)
            => context.Items.TryGetValue(OrderRequestDiagnostics.PhaseItemKey, out var phase)
                && phase is string value
                && !string.IsNullOrWhiteSpace(value)
                ? value
                : "UNKNOWN";

        private static long GetDurationMilliseconds(long started)
            => (long)Stopwatch.GetElapsedTime(started).TotalMilliseconds;
    }
}
