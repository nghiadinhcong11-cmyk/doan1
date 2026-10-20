using RestaurantPOS.AI.Services;

var zone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
var utcNow = new DateTime(2026, 10, 18, 5, 0, 0, DateTimeKind.Utc); // Sunday, 12:00 VN
var range = AdminOrderPeriodResolver.Resolve("last_week", utcNow, zone);

var fakeOrders = new[]
{
    new DateTime(2026, 10, 10, 16, 59, 59, DateTimeKind.Utc), // before start
    new DateTime(2026, 10, 10, 17, 0, 0, DateTimeKind.Utc),   // start included
    new DateTime(2026, 10, 17, 16, 59, 59, DateTimeKind.Utc), // end-1 included
    new DateTime(2026, 10, 17, 17, 0, 0, DateTimeKind.Utc),   // end excluded
    new DateTime(2026, 10, 18, 0, 0, 0, DateTimeKind.Utc)     // current week excluded
};

var selected = fakeOrders.Where(createdAt => createdAt >= range.StartUtc && createdAt < range.EndUtc).ToArray();
if (selected.Length != 2 || selected[0] != fakeOrders[1] || selected[1] != fakeOrders[2])
    throw new InvalidOperationException("last_week did not select the expected [start, end) UTC boundaries.");

Console.WriteLine($"PASS last_week [{range.StartUtc:O}, {range.EndUtc:O}) selected={selected.Length}");
