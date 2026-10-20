using System.Globalization;
using System.Text.Json;

namespace RestaurantPOS.AI.Services;

/// <summary>Deterministic presentation layer for Admin pilot read-only tool results.</summary>
public static class AdminAiResponseFormatter
{
    public static bool TryFormat(string intent, string toolName, string data, out string formatted, out string? error)
    {
        formatted = data;
        error = null;

        if (string.IsNullOrWhiteSpace(data) || !LooksLikeJson(data))
            return true;

        try
        {
            using var document = JsonDocument.Parse(data);
            var root = document.RootElement;
            formatted = intent switch
            {
                "BUSINESS_SUMMARY" => FormatSummary(root),
                "REVENUE_COMPARISON" => FormatComparison(root),
                "PRODUCT_RANKING" => FormatRanking(root),
                "ORDER_COUNT" => FormatOrderCount(root),
                "ORDER_STATUS" => FormatOrderStatus(root),
                "REVENUE_QUERY" => FormatRevenue(root),
                _ => data
            };
            return true;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException)
        {
            error = $"Admin tool result could not be formatted for {intent}/{toolName}.";
            formatted = string.Empty;
            return false;
        }
    }

    private static string FormatSummary(JsonElement root)
    {
        var period = GetString(root, "Period") ?? "kỳ đã chọn";
        var kpis = root.GetProperty("KPIs");
        return string.Join("\n", new[]
        {
            $"Tổng quan kinh doanh ({period})",
            $"- Doanh thu: {Money(kpis.GetProperty("Revenue"))}",
            $"- Số đơn: {kpis.GetProperty("OrderCount").GetInt32():N0}",
            $"- Giá trị đơn trung bình: {Money(kpis.GetProperty("AverageOrderValue"))}",
            $"- Chi phí: {Money(kpis.GetProperty("TotalExpenses"))}",
            $"- Giá vốn ước tính: {Money(kpis.GetProperty("EstimatedCOGS"))}",
            $"- Lợi nhuận ước tính: {Money(kpis.GetProperty("EstimatedNetProfit"))}",
            "Lưu ý: Giá vốn và lợi nhuận là số liệu ước tính theo dữ liệu hiện tại."
        });
    }

    private static string FormatRevenue(JsonElement root)
    {
        var period = GetString(root, "Period") ?? "kỳ đã chọn";
        var revenue = Money(root.GetProperty("Revenue"));
        var lines = new List<string> { $"Doanh thu ({period}): {revenue}" };
        if (root.TryGetProperty("Branches", out var branches) && branches.ValueKind == JsonValueKind.Array && branches.GetArrayLength() > 0)
        {
            lines.Add("Theo cơ sở:");
            foreach (var branch in branches.EnumerateArray())
            {
                var name = GetString(branch, "Name") ?? "Không rõ cơ sở";
                var branchRevenue = branch.TryGetProperty("Revenue", out var value) ? Money(value) : "0 VNĐ";
                var orderCount = branch.TryGetProperty("OrderCount", out var count) ? count.GetInt32().ToString("N0") : "0";
                lines.Add($"- {name}: {branchRevenue} ({orderCount} đơn)");
            }
        }
        return string.Join("\n", lines);
    }

    private static string FormatComparison(JsonElement root)
    {
        var current = root.GetProperty("Current");
        var comparison = root.GetProperty("Comparison");
        var analysis = root.GetProperty("Analysis");
        var percent = analysis.GetProperty("Percentage");
        var percentageText = percent.ValueKind == JsonValueKind.Null
            ? "không xác định (doanh thu kỳ so sánh bằng 0)"
            : $"{percent.GetDecimal():N2}%";
        var direction = GetString(analysis, "Direction") switch
        {
            "up" => "tăng",
            "down" => "giảm",
            _ => "không đổi"
        };
        return string.Join("\n", new[]
        {
            "So sánh doanh thu",
            $"- Kỳ hiện tại ({GetString(current, "Period")}): {Money(current.GetProperty("Revenue"))}",
            $"- Kỳ so sánh ({GetString(comparison, "Period")}): {Money(comparison.GetProperty("Revenue"))}",
            $"- Chênh lệch: {Money(analysis.GetProperty("Difference"))}",
            $"- Tỷ lệ: {percentageText}",
            $"- Xu hướng: {direction}"
        });
    }

    private static string FormatRanking(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Array)
            throw new InvalidOperationException("Ranking result must be an array.");
        var lines = new List<string> { "Món bán chạy:" };
        var index = 1;
        foreach (var item in root.EnumerateArray())
        {
            var name = GetString(item, "ProductName") ?? "Không rõ tên món";
            var quantity = item.TryGetProperty("TotalQuantity", out var quantityProperty) ? quantityProperty.GetInt32().ToString("N0") : "0";
            var line = $"{index++}. {name} — {quantity} món";
            if (item.TryGetProperty("TotalRevenue", out var revenue)) line += $", doanh thu {Money(revenue)}";
            lines.Add(line);
        }
        return lines.Count == 1 ? "Không có dữ liệu bán hàng trong kỳ đã chọn." : string.Join("\n", lines);
    }

    private static string FormatOrderCount(JsonElement root)
    {
        var kpis = root.GetProperty("KPIs");
        var period = GetString(root, "Period") ?? "kỳ đã chọn";
        return $"Tổng số đơn ({period}): {kpis.GetProperty("OrderCount").GetInt32():N0} đơn.";
    }

    private static string FormatOrderStatus(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Array)
            throw new InvalidOperationException("Order status result must be an array.");
        var count = root.GetArrayLength();
        if (count == 0) return "Không tìm thấy đơn hàng phù hợp với trạng thái và kỳ đã chọn.";
        var lines = new List<string> { $"Danh sách đơn hàng ({count:N0}):" };
        foreach (var order in root.EnumerateArray())
        {
            var invoice = GetString(order, "InvoiceCode") ?? "Không có mã đơn";
            var status = GetString(order, "Status") ?? "Không rõ trạng thái";
            var created = FormatCreatedAt(order);
            lines.Add($"- {invoice} — {status}{(string.IsNullOrWhiteSpace(created) ? string.Empty : $" — {created}")}");
        }
        return string.Join("\n", lines);
    }

    private static string Money(JsonElement value)
    {
        var amount = value.ValueKind == JsonValueKind.Number ? value.GetDecimal() : decimal.Parse(value.GetString() ?? "0", CultureInfo.InvariantCulture);
        return $"{amount:N0} VNĐ";
    }

    private static string? GetString(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind != JsonValueKind.Null ? value.ToString() : null;

    private static string FormatCreatedAt(JsonElement order)
    {
        var raw = GetString(order, "CreatedAt");
        if (!DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
            return raw ?? string.Empty;

        var vietnamZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        return TimeZoneInfo.ConvertTime(parsed, vietnamZone).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
    }

    private static bool LooksLikeJson(string value)
    {
        var trimmed = value.TrimStart();
        return trimmed.StartsWith("{", StringComparison.Ordinal) || trimmed.StartsWith("[", StringComparison.Ordinal);
    }
}
