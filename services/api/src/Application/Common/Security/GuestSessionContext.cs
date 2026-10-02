using System.Security.Claims;

namespace RestaurantPOS.Application.Common.Security;

/// <summary>
/// Trusted table context carried only by a QR bootstrap JWT.
/// </summary>
public sealed record GuestSessionContext(Guid TableId, Guid BranchId)
{
    public const string SessionTypeClaim = "customerSessionType";
    public const string TableIdClaim = "tableId";
    public const string BranchIdClaim = "branchId";

    public static bool TryCreate(ClaimsPrincipal user, out GuestSessionContext? context)
    {
        context = null;
        if (!user.IsInRole("customer") ||
            !string.Equals(user.FindFirst(SessionTypeClaim)?.Value, "guest", StringComparison.OrdinalIgnoreCase) ||
            !Guid.TryParse(user.FindFirst(TableIdClaim)?.Value, out var tableId) || tableId == Guid.Empty ||
            !Guid.TryParse(user.FindFirst(BranchIdClaim)?.Value, out var branchId) || branchId == Guid.Empty)
        {
            return false;
        }

        context = new GuestSessionContext(tableId, branchId);
        return true;
    }

    public static bool IsGuest(ClaimsPrincipal user) =>
        user.IsInRole("customer") &&
        string.Equals(user.FindFirst(SessionTypeClaim)?.Value, "guest", StringComparison.OrdinalIgnoreCase);
}
