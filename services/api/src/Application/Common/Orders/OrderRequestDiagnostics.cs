using System;

namespace RestaurantPOS.Application.Common.Orders
{
    /// <summary>
    /// Safe metadata shared by the GET /api/Order diagnostic checkpoints.
    /// This class intentionally excludes request values and exception messages.
    /// </summary>
    public static class OrderRequestDiagnostics
    {
        public const string CorrelationItemKey = "Order.Get.CorrelationId";
        public const string PhaseItemKey = "Order.Get.Phase";

        public static string CreateCorrelationId()
            => Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();

        public static string GetSafeExceptionType(Exception exception)
        {
            for (var current = exception; current is not null; current = current.InnerException)
            {
                var typeName = current.GetType().Name;
                if (typeName is "ArgumentException"
                    or "DbUpdateException"
                    or "FormatException"
                    or "InvalidCastException"
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
}
