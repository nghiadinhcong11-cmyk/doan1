namespace RestaurantPOS.AI.Services;

/// <summary>
/// Resolves Admin order-list periods as half-open UTC ranges. Order.CreatedAt
/// is persisted in UTC, while natural-language periods are business dates in
/// Vietnam time.
/// </summary>
public static class AdminOrderPeriodResolver
{
    public static (DateTime StartUtc, DateTime EndUtc) Resolve(string period, DateTime utcNow, TimeZoneInfo businessZone)
    {
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc), businessZone);
        var startLocal = period switch
        {
            "yesterday" => nowLocal.Date.AddDays(-1),
            "this_week" => nowLocal.Date.AddDays(-(int)nowLocal.DayOfWeek),
            "last_week" => nowLocal.Date.AddDays(-(int)nowLocal.DayOfWeek - 7),
            "this_month" => new DateTime(nowLocal.Year, nowLocal.Month, 1),
            "last_month" => new DateTime(nowLocal.Year, nowLocal.Month, 1).AddMonths(-1),
            _ => nowLocal.Date
        };
        var endLocal = period switch
        {
            "this_week" => nowLocal.Date.AddDays(1),
            "last_week" => nowLocal.Date.AddDays(-(int)nowLocal.DayOfWeek),
            "this_month" => startLocal.AddMonths(1),
            "last_month" => new DateTime(nowLocal.Year, nowLocal.Month, 1),
            _ => startLocal.AddDays(1)
        };

        return (
            TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(startLocal, DateTimeKind.Unspecified), businessZone),
            TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(endLocal, DateTimeKind.Unspecified), businessZone));
    }
}
