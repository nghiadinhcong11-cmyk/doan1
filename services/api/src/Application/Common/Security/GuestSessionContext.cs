using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace RestaurantPOS.Application.Common.Security;

/// <summary>
/// Trusted branch/table context carried by a server-issued QR or direct-guest JWT.
/// </summary>
public sealed record GuestSessionContext(Guid TableId, Guid BranchId, string? SessionId = null, string SessionType = "guest")
{
    public const string QrSessionType = "guest";
    public const string DirectSessionType = "directGuest";
    public const string SessionTypeClaim = "customerSessionType";
    public const string TableIdClaim = "tableId";
    public const string BranchIdClaim = "branchId";
    public const string SessionIdClaim = JwtRegisteredClaimNames.Jti;

    public static bool TryCreate(ClaimsPrincipal user, out GuestSessionContext? context)
    {
        context = null;
        var sessionType = user.FindFirst(SessionTypeClaim)?.Value;
        var isQrSession = string.Equals(sessionType, QrSessionType, StringComparison.OrdinalIgnoreCase);
        var isDirectSession = string.Equals(sessionType, DirectSessionType, StringComparison.OrdinalIgnoreCase);
        var hasTable = Guid.TryParse(user.FindFirst(TableIdClaim)?.Value, out var tableId) && tableId != Guid.Empty;
        if (!user.IsInRole("customer") ||
            (!isQrSession && !isDirectSession) ||
            (isQrSession && !hasTable) ||
            !Guid.TryParse(user.FindFirst(BranchIdClaim)?.Value, out var branchId) || branchId == Guid.Empty)
        {
            return false;
        }

        // The session identifier is optional for backwards compatibility with existing
        // guest tokens; when present it is read only from validated server claims.
        var sessionId = user.FindFirst(SessionIdClaim)?.Value
            ?? user.FindFirst(ClaimTypes.Sid)?.Value;
        context = new GuestSessionContext(hasTable ? tableId : Guid.Empty, branchId, sessionId, sessionType!);
        return true;
    }

    public static bool IsGuest(ClaimsPrincipal user) =>
        user is not null && user.IsInRole("customer") &&
        (string.Equals(user.FindFirst(SessionTypeClaim)?.Value, QrSessionType, StringComparison.OrdinalIgnoreCase) ||
         string.Equals(user.FindFirst(SessionTypeClaim)?.Value, DirectSessionType, StringComparison.OrdinalIgnoreCase));

    public static bool IsDirectGuest(ClaimsPrincipal user) =>
        user is not null && user.IsInRole("customer") &&
        string.Equals(user.FindFirst(SessionTypeClaim)?.Value, DirectSessionType, StringComparison.OrdinalIgnoreCase);
}
