namespace RestaurantPOS.Application.Common.Security;

public static class QrTokenBackfillTargetGuard
{
    public const string AuthorizedDevProjectRef = "qfkgjxwbshjgsxsvkpkp";

    public static bool IsAuthorized(string? explicitProjectRef, string? host, string? username) =>
        string.Equals(explicitProjectRef, AuthorizedDevProjectRef, StringComparison.Ordinal) &&
        $"{host}|{username}".Contains(AuthorizedDevProjectRef, StringComparison.OrdinalIgnoreCase);
}
