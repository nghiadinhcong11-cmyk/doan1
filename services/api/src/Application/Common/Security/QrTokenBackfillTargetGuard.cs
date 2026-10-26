namespace RestaurantPOS.Application.Common.Security;

public static class QrTokenBackfillTargetGuard
{
    public const string AuthorizedDevProjectRef = "lizpcvayxcuwjgrxsvfe";

    public static bool IsAuthorized(string? explicitProjectRef, string? host, string? username)
    {
        // The historical backfill target is retired. Keeping this guard closed
        // prevents every old fixture/diagnostic caller from opening that project.
        return false;
    }
}
