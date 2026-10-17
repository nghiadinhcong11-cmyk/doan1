using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MockAiEmbeddingSpike;

public static class PolicyUnitChecks
{
    public static Task<int> RunAsync()
    {
        var checks = new (string Name, string Actual, string Expected)[]
        {
            ("negation_plain_ascii", IsNegated(Fold("khong xem doanh thu hom nay")).ToString(), true.ToString()),
            ("negation_diacritic", IsNegated(Fold("Đừng cho tôi xem doanh thu hôm nay")).ToString(), true.ToString()),
            ("negation_middle_clause", IsNegated(Fold("Doanh thu hôm nay, không cần báo cáo KPI")).ToString(), true.ToString()),
            ("positive_request", IsNegated(Fold("Cho tôi xem doanh thu hôm nay")).ToString(), false.ToString()),
            ("condition_statement_safe", IsNegated(Fold("Cửa hàng không có doanh thu hôm nay")).ToString(), true.ToString()),
            ("single_tool_multi_field", Composition("Tổng hợp doanh thu và số đơn hôm nay"), "SINGLE_TOOL_MULTI_FIELD"),
            ("multi_tool_compound", Composition("Cho tôi doanh thu hôm nay và thực đơn hôm nay"), "MULTI_TOOL_COMPOUND"),
            ("ambiguous_subject", Composition("Cho tôi xem tình hình hôm nay"), "AMBIGUOUS"),
            ("ood_opening_hours", IsOutOfScope(Fold("Nhà hàng mở cửa đến mấy giờ?")).ToString(), true.ToString()),
            ("ood_loyalty", IsOutOfScope(Fold("Khách còn bao nhiêu điểm thành viên?")).ToString(), true.ToString()),
            ("ood_order_status", IsOutOfScope(Fold("Cập nhật trạng thái đơn hàng")).ToString(), true.ToString())
        };
        var failed = 0;
        foreach (var check in checks)
        {
            var pass = check.Actual == check.Expected;
            if (!pass) failed++;
            Console.WriteLine($"POLICY|{check.Name}|expected={check.Expected}|actual={check.Actual}|{(pass ? "PASS" : "FAIL")}");
        }
        return Task.FromResult(failed == 0 ? 0 : 1);
    }

    private static string Fold(string text)
    {
        var mapped = text.Replace('\u0111', 'd').Replace('\u0110', 'D');
        return new string(mapped.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray()).ToLowerInvariant();
    }

    private static bool IsNegated(string text) => Regex.IsMatch(text, @"(^|\s)(khong|dung)(?=\s|$|[,;.!?])");

    private static string Composition(string text)
    {
        var t = Fold(text);
        var revenue = Regex.IsMatch(t, @"doanh thu|doanh so|ban hang|thu duoc|thu ve|kiem");
        var orders = Regex.IsMatch(t, @"so don|don hang|luong don");
        var menu = Regex.IsMatch(t, @"thuc don|menu|danh sach mon|mon an|gia mon");
        var clauses = Regex.Split(t, @"\s*(?:,|;|\bva\b|\broi\b)\s*").Count(x => !string.IsNullOrWhiteSpace(x));
        if (menu && revenue && clauses > 1) return "MULTI_TOOL_COMPOUND";
        if (revenue && orders) return "SINGLE_TOOL_MULTI_FIELD";
        if (Regex.IsMatch(t, @"\b(tinh hinh|hoat dong)\b") && !revenue && !orders) return "AMBIGUOUS";
        return "SINGLE_INTENT";
    }

    private static bool IsOutOfScope(string text) => Regex.IsMatch(text, @"thuc don|menu|mo cua|gio phuc vu|phuc vu den|diem thanh vien|loyalty|ca lam|lich ca|ton kho|nguyen lieu|cap nhat trang thai|trang thai don|xep hang mon|mon ban chay|don dang cho|don cho xu ly");
}
