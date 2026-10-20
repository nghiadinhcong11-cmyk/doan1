namespace RestaurantPOS.AI.Services;

public static class CustomerGuestAiResponseFormatter
{
    public static bool TryFormat(string? intent, string? toolName, string? data, out string formatted, out string? error)
    {
        formatted = string.Empty;
        error = null;
        if (string.IsNullOrWhiteSpace(data))
        {
            formatted = string.Equals(toolName, "customer_get_menu", StringComparison.OrdinalIgnoreCase)
                ? "Hiện tại thực đơn không có món phù hợp."
                : "Không có dữ liệu phù hợp.";
            return true;
        }

        if (string.Equals(toolName, "customer_get_menu", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(toolName, "get_my_orders", StringComparison.OrdinalIgnoreCase))
        {
            // The tools already produce a deterministic, server-authoritative
            // Vietnamese representation. Do not reconstruct prices or order data here.
            formatted = data.Trim();
            return true;
        }

        error = "Customer/Guest semantic pilot returned an unsupported response.";
        return false;
    }
}
