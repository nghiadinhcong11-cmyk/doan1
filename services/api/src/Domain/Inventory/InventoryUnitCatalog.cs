namespace RestaurantPOS.Domain.Inventory;

/// <summary>
/// Controlled units for inventory master data. Receipt and issue lines must
/// derive their unit from InventoryItem rather than accepting free text.
/// </summary>
public static class InventoryUnitCatalog
{
    public const string Kilogram = "kg";
    public const string Gram = "g";
    public const string Litre = "litre";
    public const string Millilitre = "ml";
    public const string Piece = "piece";
    public const string Box = "box";
    public const string Bottle = "bottle";
    public const string Pack = "pack";

    private static readonly IReadOnlySet<string> AllowedUnits = new HashSet<string>(StringComparer.Ordinal)
    {
        Kilogram, Gram, Litre, Millilitre, Piece, Box, Bottle, Pack
    };

    private static readonly IReadOnlySet<string> DiscreteUnits = new HashSet<string>(StringComparer.Ordinal)
    {
        Piece, Box, Bottle, Pack
    };

    public static string Normalize(string? unitCode) => unitCode?.Trim().ToLowerInvariant() ?? string.Empty;

    public static bool IsValid(string? unitCode) => AllowedUnits.Contains(Normalize(unitCode));

    public static bool IsDiscrete(string? unitCode) => DiscreteUnits.Contains(Normalize(unitCode));
}

public static class InventoryQuantityRules
{
    public static bool IsValidPositive(string? unitCode, decimal quantity)
    {
        if (!InventoryUnitCatalog.IsValid(unitCode) || quantity <= 0)
            return false;

        return !InventoryUnitCatalog.IsDiscrete(unitCode) || decimal.Truncate(quantity) == quantity;
    }

    public static bool IsValidNonNegative(decimal quantity) => quantity >= 0;
}
