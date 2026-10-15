using System.Globalization;
using System.Text;

namespace RestaurantPOS.AI.Services;

internal sealed record MockAiIntent(
    string Code,
    string? ToolName,
    IReadOnlyDictionary<string, object?> Arguments,
    string Response);

/// <summary>
/// Explicit, deterministic Vietnamese intent rules for Development Mock mode.
/// This is not an LLM and does not infer unsupported capabilities.
/// </summary>
internal static class MockAiIntentRecognizer
{
    public static bool IsPersonalOrderRequest(string message) =>
        IsMyOrders(MockAiTextNormalizer.Normalize(message));

    public static MockAiIntent Recognize(string message, IReadOnlyCollection<string> previousMessages)
    {
        var normalized = MockAiTextNormalizer.Normalize(message);
        var context = MockAiTextNormalizer.Normalize(string.Join(" ", previousMessages));

        if (MockAiTextNormalizer.ContainsAny(normalized, "gioi thieu", "tro ly ai", "ban co the", "kha nang", "ho tro gi"))
        {
            return Text("INTRODUCTION", "Tôi là trợ lý DOAN POS ở chế độ Mock theo luật định sẵn. Tôi không phải mô hình ngôn ngữ và chỉ xử lý các truy vấn đã được lập trình.");
        }

        if (MockAiTextNormalizer.ContainsAny(normalized, "ton kho", "hang ton", "nguyen lieu ton", "inventory"))
        {
            return Text("INVENTORY_UNSUPPORTED", "Mock AI hiện chưa có tool đọc tồn kho được xác minh trong hệ thống.");
        }

        // Mutations are rejected before read-intent rules. This prevents a
        // request such as "cập nhật trạng thái đơn" from being interpreted as
        // a harmless order-status lookup.
        if (IsWriteMutation(normalized))
        {
            return Text("WRITE_INTENT_UNSUPPORTED", "Chế độ Mock chỉ hỗ trợ truy vấn đọc và không thực hiện thay đổi giá hoặc thao tác ghi; yêu cầu này hiện chưa được hỗ trợ.");
        }

        if (!IsRevenueComparison(normalized) && IsMultiIntent(normalized))
        {
            return Text("AMBIGUOUS_MULTI_INTENT", "Tôi nhận thấy bạn đang hỏi nhiều nội dung cùng lúc. Vui lòng gửi từng yêu cầu riêng để tránh chọn sai dữ liệu.");
        }

        var period = ResolvePeriod(normalized);

        if (IsRevenueComparison(normalized))
        {
            var comparison = ResolveComparison(normalized);
            return comparison is null
                ? Text("CLARIFICATION_REQUIRED", "Bạn muốn so sánh doanh thu hôm nay với hôm qua, tuần này với tuần trước hay tháng này với tháng trước?")
                : Tool("REVENUE_COMPARISON", "get_revenue_comparison", new Dictionary<string, object?>
                {
                    ["currentPeriod"] = comparison.Value.Current,
                    ["comparisonPeriod"] = comparison.Value.Previous
                });
        }

        if (IsProductRanking(normalized))
        {
            var sellerPeriod = ResolveBestSellerPeriod(normalized);
            return sellerPeriod is null
                ? Text("CLARIFICATION_REQUIRED", "Bạn muốn xem món bán chạy trong hôm nay, hôm qua, tuần này hay tháng này?")
                : Tool("PRODUCT_RANKING", "get_best_sellers", new Dictionary<string, object?>
                {
                    ["period"] = sellerPeriod,
                    ["limit"] = 5
                });
        }

        // Customer-owned order queries must be evaluated before the staff
        // order-status rules. The registry remains the role boundary: only a
        // customer receives get_my_orders in its declared tool set.
        if (IsMyOrders(normalized))
        {
            return Tool("MY_ORDERS", "get_my_orders", new Dictionary<string, object?>());
        }

        if (IsOrderStatus(normalized))
        {
            var arguments = new Dictionary<string, object?>();
            var status = ResolveOrderStatus(normalized);
            if (status is not null)
            {
                arguments["status"] = status;
            }

            AddDateRange(arguments, period);
            return Tool("ORDER_STATUS", "get_order_list", arguments);
        }

        if (IsOrderCount(normalized))
        {
            return period is null
                ? Text("CLARIFICATION_REQUIRED", "Bạn muốn xem số đơn của hôm nay, hôm qua, tuần này hay tháng này?")
                : Tool("ORDER_COUNT", "get_business_summary", new Dictionary<string, object?> { ["period"] = period });
        }

        if (IsRevenue(normalized, context))
        {
            var revenue = ResolveRevenuePeriod(normalized);
            if (revenue is not null)
            {
                return revenue == "yesterday"
                    ? Tool("REVENUE_QUERY", "get_business_summary", new Dictionary<string, object?> { ["period"] = revenue })
                    : Tool("REVENUE_QUERY", "get_revenue", new Dictionary<string, object?> { ["period"] = revenue });
            }

            return Text("CLARIFICATION_REQUIRED", "Bạn muốn xem doanh thu của hôm nay, hôm qua, tháng này hay cả năm? Vui lòng nêu rõ khoảng thời gian.");
        }

        if (IsBusinessSummary(normalized))
        {
            return period is null
                ? Text("CLARIFICATION_REQUIRED", "Bạn muốn xem tóm tắt kinh doanh trong khoảng thời gian nào?")
                : Tool("BUSINESS_SUMMARY", "get_business_summary", new Dictionary<string, object?> { ["period"] = period });
        }

        if (MockAiTextNormalizer.ContainsAny(
                normalized,
                "tinh trang ban",
                "trang thai ban",
                "ban nao dang co khach",
                "ban nao con trong",
                "ban nao dang dung",
                "ban nao da dat",
                "so do ban",
                "ban trong",
                "ban con trong",
                "ban co khach",
                "ban dang dung",
                "ban da dat",
                "ban kha dung",
                "table status",
                "available table"))
        {
            return Tool("TABLE_STATUS", "employee_get_table_summary", new Dictionary<string, object?>());
        }

        if (MockAiTextNormalizer.ContainsAny(
                normalized,
                "thuc don",
                "menu",
                "mon an",
                "co mon gi",
                "quan co mon gi",
                "mon gi",
                "danh sach mon",
                "do uong",
                "nuoc uong",
                "san pham",
                "gia mon",
                "bao gia",
                "danh muc"))
        {
            var category = ExtractCategory(normalized);
            return Tool("MENU_QUERY", "customer_get_menu", category is null
                ? new Dictionary<string, object?>()
                : new Dictionary<string, object?> { ["category"] = category });
        }

        if (MockAiTextNormalizer.ContainsAny(
                normalized,
                "ca lam",
                "lich lam",
                "lich lam viec",
                "ca hom nay",
                "ca cua toi",
                "lich lam cua toi",
                "toi lam ca nao",
                "toi co ca hom nay"))
        {
            return Tool("MY_SHIFT", "get_my_shift", new Dictionary<string, object?>());
        }

        return Text("UNSUPPORTED_INTENT", "Chế độ Mock chỉ hỗ trợ các truy vấn đã được lập trình; yêu cầu này hiện chưa được hỗ trợ. Vui lòng hỏi về doanh thu, tóm tắt kinh doanh, so sánh doanh thu, số đơn, món bán chạy, trạng thái đơn, tình trạng bàn, thực đơn hoặc ca làm việc.");
    }

    private static bool IsRevenue(string message, string context) =>
        message.Contains("doanh thu", StringComparison.Ordinal)
        || message.Contains("doanh so", StringComparison.Ordinal)
        || message.Contains("tien ban hang", StringComparison.Ordinal)
        || message.Contains("bao nhieu tien", StringComparison.Ordinal)
        || message.Contains("revenue", StringComparison.Ordinal)
        || ((context.Contains("doanh thu", StringComparison.Ordinal)
                || context.Contains("revenue", StringComparison.Ordinal))
            && MockAiTextNormalizer.ContainsAny(message, "hom qua", "hom nay", "thang", "nam", "today", "yesterday"));

    private static bool IsBusinessSummary(string message) =>
        MockAiTextNormalizer.ContainsAny(
            message,
            "tom tat kinh doanh",
            "tong quan kinh doanh",
            "tinh hinh kinh doanh",
            "tinh hinh buon ban",
            "kpi kinh doanh",
            "bao cao kinh doanh",
            "bao cao tong hop",
            "business summary",
            "business overview");

    private static bool IsRevenueComparison(string message) =>
        MockAiTextNormalizer.ContainsAny(
            message,
            "so sanh doanh thu",
            "doi chieu doanh thu",
            "chenh lech doanh thu",
            "tang truong doanh thu",
            "doanh thu so voi",
            "doanh thu so",
            "doanh thu hon",
            "doanh thu kem",
            "revenue compared",
            "revenue comparison")
        || (message.Contains("doanh thu", StringComparison.Ordinal)
            && MockAiTextNormalizer.ContainsAny(message, "so voi", "voi", "hon", "kem"));

    private static bool IsOrderCount(string message) =>
        MockAiTextNormalizer.ContainsAny(
            message,
            "so don",
            "tong so don",
            "bao nhieu don",
            "may don",
            "so luong don",
            "don hang hom nay co bao nhieu",
            "how many orders",
            "order count");

    private static bool IsProductRanking(string message) =>
        MockAiTextNormalizer.ContainsAny(
            message,
            "mon ban chay",
            "ban chay nhat",
            "mon nao ban nhieu",
            "mon nao chay nhat",
            "goi nhieu nhat",
            "top mon",
            "top ban chay",
            "xep hang mon",
            "best seller",
            "best selling");

    private static bool IsPriceMutation(string message) =>
        message.Contains("gia mon", StringComparison.Ordinal)
        && MockAiTextNormalizer.ContainsAny(message, "thay doi", "doi gia", "cap nhat", "sua gia", "tang gia", "giam gia");

    private static bool IsWriteMutation(string message) =>
        IsPriceMutation(message)
        || (message.Contains("cap nhat", StringComparison.Ordinal)
            && message.Contains("don", StringComparison.Ordinal)
            && message.Contains("trang thai", StringComparison.Ordinal))
        || MockAiTextNormalizer.ContainsAny(
            message,
            "cap nhat trang thai",
            "thay doi trang thai",
            "sua trang thai",
            "huy don",
            "xoa mon",
            "xoa khoi",
            "them mon",
            "tao don",
            "dat mon",
            "order mon",
            "order mon nay",
            "dat ban",
            "book table",
            "giu ban",
            "huy dat ban",
            "tao dat ban",
            "sua don",
            "doi don",
            "them mon vao don",
            "xoa mon khoi don",
            "xoa don",
            "thanh toan",
            "xac nhan thanh toan",
            "thu tien",
            "hoan tien",
            "refund",
            "cap nhat gia",
            "thay doi gia",
            "doi gia",
            "sua gia",
            "tang gia",
            "giam gia",
            "doi trang thai don");

    private static bool IsMyOrders(string message) =>
        MockAiTextNormalizer.ContainsAny(
            message,
            "don hang cua toi",
            "don cua toi",
            "xem don toi da dat",
            "don toi da dat",
            "don cua toi dau",
            "trang thai don cua toi",
            "toi da dat nhung gi",
            "toi da dat gi",
            "my orders",
            "my order");

    private static bool IsOrderStatus(string message) =>
        MockAiTextNormalizer.ContainsAny(
            message,
            "trang thai don",
            "trang thai cac don",
            "don dang xu ly",
            "don cho xu ly",
            "don dang cho",
            "don pending",
            "don hoan thanh",
            "don da hoan tat",
            "don hoan tat",
            "don da xong",
            "don completed",
            "don da huy",
            "don huy",
            "don cancelled",
            "don nao dang",
            "cac order pending",
            "don hom nay",
            "cho xem danh sach don",
            "order status");

    private static bool IsMultiIntent(string message)
    {
        var count = 0;
        if (IsRevenue(message, string.Empty)) count++;
        if (IsBusinessSummary(message)) count++;
        if (IsProductRanking(message)) count++;
        if (IsMyOrders(message)) count++;
        if (IsOrderCount(message)) count++;
        if (!IsMyOrders(message) && IsOrderStatus(message)) count++;
        if (MockAiTextNormalizer.ContainsAny(message, "tinh trang ban", "trang thai ban", "so do ban", "ban nao dang co khach", "ban nao con trong", "ban nao dang dung", "ban nao da dat", "ban trong", "ban co khach", "ban kha dung", "table status", "available table")) count++;
        if (MockAiTextNormalizer.ContainsAny(message, "thuc don", "menu", "mon an", "co mon gi", "quan co mon gi", "mon gi", "danh sach mon", "do uong", "nuoc uong", "san pham", "gia mon", "bao gia")) count++;
        if (MockAiTextNormalizer.ContainsAny(message, "ca lam", "lich lam", "lich lam viec", "ca hom nay", "ca cua toi", "lich lam cua toi", "toi lam ca nao", "toi co ca hom nay")) count++;
        if (MockAiTextNormalizer.ContainsAny(message, "ca lam", "lich lam", "lich lam viec", "ca cua toi", "lich lam cua toi", "toi lam ca nao", "toi co ca hom nay")
            && message.Contains("don hang", StringComparison.Ordinal)) count++;
        return count > 1;
    }

    private static string? ResolveRevenuePeriod(string message) =>
        message.Contains("hom qua", StringComparison.Ordinal) || message.Contains("yesterday", StringComparison.Ordinal) ? "yesterday" :
        message.Contains("thang", StringComparison.Ordinal) || message.Contains("month", StringComparison.Ordinal) ? "month" :
        message.Contains("nam", StringComparison.Ordinal) || message.Contains("year", StringComparison.Ordinal) ? "year" :
        message.Contains("hom nay", StringComparison.Ordinal) || message.Contains("today", StringComparison.Ordinal) ? "today" : null;

    private static string? ResolvePeriod(string message) =>
        message.Contains("hom qua", StringComparison.Ordinal) || message.Contains("yesterday", StringComparison.Ordinal) ? "yesterday" :
        message.Contains("tuan truoc", StringComparison.Ordinal) || message.Contains("last week", StringComparison.Ordinal) ? "last_week" :
        message.Contains("thang truoc", StringComparison.Ordinal) || message.Contains("last month", StringComparison.Ordinal) ? "last_month" :
        message.Contains("tuan nay", StringComparison.Ordinal) || message.Contains("this week", StringComparison.Ordinal) ? "this_week" :
        message.Contains("thang nay", StringComparison.Ordinal) || message.Contains("this month", StringComparison.Ordinal) ? "this_month" :
        message.Contains("hom nay", StringComparison.Ordinal) || message.Contains("today", StringComparison.Ordinal) ? "today" : null;

    private static string? ResolveBestSellerPeriod(string message) =>
        message.Contains("hom qua", StringComparison.Ordinal) || message.Contains("yesterday", StringComparison.Ordinal) ? "yesterday" :
        message.Contains("tuan", StringComparison.Ordinal) || message.Contains("week", StringComparison.Ordinal) ? "week" :
        message.Contains("thang", StringComparison.Ordinal) || message.Contains("month", StringComparison.Ordinal) ? "month" :
        message.Contains("hom nay", StringComparison.Ordinal) || message.Contains("today", StringComparison.Ordinal) ? "today" : null;

    private static (string Current, string Previous)? ResolveComparison(string message) =>
        MockAiTextNormalizer.ContainsAny(message, "hom nay", "today") && MockAiTextNormalizer.ContainsAny(message, "hom qua", "yesterday") ? ("today", "yesterday") :
        MockAiTextNormalizer.ContainsAny(message, "tuan nay", "this week") && MockAiTextNormalizer.ContainsAny(message, "tuan truoc", "last week") ? ("this_week", "last_week") :
        MockAiTextNormalizer.ContainsAny(message, "thang nay", "this month") && MockAiTextNormalizer.ContainsAny(message, "thang truoc", "last month") ? ("this_month", "last_month") : null;

    private static string? ResolveOrderStatus(string message) =>
        MockAiTextNormalizer.ContainsAny(message, "dang xu ly", "cho xu ly", "dang cho", "pending") ? "Đang xử lý" :
        MockAiTextNormalizer.ContainsAny(message, "hoan thanh", "hoan tat", "da xong", "completed") ? "Hoàn thành" :
        MockAiTextNormalizer.ContainsAny(message, "da huy", "don huy", "cancelled") ? "Đã hủy" : null;

    private static void AddDateRange(Dictionary<string, object?> arguments, string? period)
    {
        if (period is not ("today" or "yesterday"))
        {
            return;
        }

        var timeZone = FindVietnamTimeZone();
        var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone).Date;
        var date = period == "today" ? today : today.AddDays(-1);
        arguments["fromDate"] = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        arguments["toDate"] = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    private static TimeZoneInfo FindVietnamTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
    }

    private static string? ExtractCategory(string message)
    {
        foreach (var marker in new[] { "danh muc ", "loai ", "nhom ", "category " })
        {
            var index = message.IndexOf(marker, StringComparison.Ordinal);
            if (index >= 0)
            {
                var value = message[(index + marker.Length)..].Trim();
                return value.Length == 0 ? null : value;
            }
        }

        return MockAiTextNormalizer.ContainsAny(message, "do uong", "nuoc uong") ? "do uong" :
            MockAiTextNormalizer.ContainsAny(message, "khai vi") ? "khai vi" :
            MockAiTextNormalizer.ContainsAny(message, "mon chinh") ? "mon chinh" :
            MockAiTextNormalizer.ContainsAny(message, "trang mieng") ? "trang mieng" : null;
    }

    private static MockAiIntent Tool(string code, string toolName, IReadOnlyDictionary<string, object?> arguments) =>
        new(code, toolName, arguments, string.Empty);

    private static MockAiIntent Text(string code, string response) =>
        new(code, null, new Dictionary<string, object?>(), response);
}

internal static class MockAiTextNormalizer
{
    public static string Normalize(string value)
    {
        var decomposed = value.Replace('đ', 'd').Replace('Đ', 'D').Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            // FormD separates Vietnamese tone/diacritic marks from the base
            // letter. Remove those marks instead of turning them into spaces;
            // otherwise "hôm" becomes "h o m" and valid rules never match.
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(character) || char.IsWhiteSpace(character))
            {
                builder.Append(character);
            }
            else
            {
                builder.Append(' ');
            }
        }

        return string.Join(' ', builder.ToString().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant();
    }

    public static bool ContainsAny(string value, params string[] candidates) =>
        candidates.Any(candidate => value.Contains(candidate, StringComparison.Ordinal));
}
