namespace MockAiEmbeddingSpike;

public sealed record IntentExample(string Id, string IntentId, string Text, string? ToolName, string ActorScope);

public static class IntentCatalog
{
    public static IReadOnlyList<IntentExample> Examples { get; } = new[]
    {
        new IntentExample("CAT-REV-01", "REVENUE_QUERY", "Doanh thu của chi nhánh hôm nay là bao nhiêu?", "get_revenue", "owner"),
        new IntentExample("CAT-REV-02", "REVENUE_QUERY", "Cho tôi xem tổng tiền bán được trong ngày.", "get_revenue", "owner"),
        new IntentExample("CAT-REV-03", "REVENUE_QUERY", "Hôm nay cửa hàng thu về bao nhiêu tiền?", "get_revenue", "owner"),
        new IntentExample("CAT-CMP-01", "REVENUE_COMPARISON", "So sánh doanh thu hôm nay với hôm qua.", "get_revenue_comparison", "owner"),
        new IntentExample("CAT-CMP-02", "REVENUE_COMPARISON", "Doanh số ngày này khác ngày trước thế nào?", "get_revenue_comparison", "owner"),
        new IntentExample("CAT-CMP-03", "REVENUE_COMPARISON", "Đặt doanh thu hai ngày cạnh nhau cho tôi.", "get_revenue_comparison", "owner"),
        new IntentExample("CAT-CMP-04", "REVENUE_COMPARISON", "Đối chiếu doanh số giữa hai mốc thời gian.", "get_revenue_comparison", "owner"),
        new IntentExample("CAT-RANK-01", "PRODUCT_RANKING", "Món nào bán được nhiều nhất trong tuần?", "get_best_sellers", "owner"),
        new IntentExample("CAT-RANK-02", "PRODUCT_RANKING", "Xếp hạng các món theo số lượng bán.", "get_best_sellers", "owner"),
        new IntentExample("CAT-RANK-03", "PRODUCT_RANKING", "Thống kê các sản phẩm có số suất tiêu thụ cao nhất tháng này.", "get_best_sellers", "owner"),
        new IntentExample("CAT-RANK-04", "PRODUCT_RANKING", "Liệt kê top món theo lượng bán.", "get_best_sellers", "owner"),
        new IntentExample("CAT-SUM-01", "BUSINESS_SUMMARY", "Tóm tắt tình hình kinh doanh của tháng này.", "get_business_summary", "owner"),
        new IntentExample("CAT-SUM-02", "BUSINESS_SUMMARY", "Cho tôi bản tổng quan hoạt động cửa hàng.", "get_business_summary", "owner"),
        new IntentExample("CAT-SUM-03", "BUSINESS_SUMMARY", "Tình hình bán hàng gần đây ra sao?", "get_business_summary", "owner"),
        new IntentExample("CAT-MENU-01", "MENU_QUERY", "Thực đơn hiện có của chi nhánh gồm những món nào?", "customer_get_menu", "customer|guest"),
        new IntentExample("CAT-MENU-02", "MENU_QUERY", "Món ăn và giá đang được phục vụ là gì?", "customer_get_menu", "customer|guest"),
        new IntentExample("CAT-MENU-03", "MENU_QUERY", "Tôi muốn xem menu của quán.", "customer_get_menu", "customer|guest"),
        new IntentExample("CAT-MENU-04", "MENU_QUERY", "Các món đang phục vụ hôm nay gồm những gì?", "customer_get_menu", "customer|guest"),
        new IntentExample("CAT-MENU-05", "MENU_QUERY", "Xin bảng thực đơn kèm đơn giá từng món của nhà hàng.", "customer_get_menu", "customer|guest"),
        new IntentExample("CAT-MENU-06", "MENU_QUERY", "Quán có bán những món gì, giá tiền mỗi phần thế nào?", "customer_get_menu", "customer|guest"),
        new IntentExample("CAT-ORDER-01", "MY_ORDERS", "Tôi muốn xem các đơn hàng của mình.", "get_my_orders", "customer"),
        new IntentExample("CAT-ORDER-02", "MY_ORDERS", "Đơn gần đây của tài khoản này đang thế nào?", "get_my_orders", "customer"),
        new IntentExample("CAT-SHIFT-01", "MY_SHIFT", "Ca làm hiện tại của tôi là ca nào?", "get_my_shift", "employee"),
        new IntentExample("CAT-SHIFT-02", "MY_SHIFT", "Cho tôi biết lịch ca của tôi hôm nay.", "get_my_shift", "employee"),
        new IntentExample("CAT-UNSUP-01", "UNSUPPORTED_INTENT", "Kiểm tra tồn kho nguyên liệu giúp tôi.", null, "owner|customer|guest"),
        new IntentExample("CAT-UNSUP-02", "UNSUPPORTED_INTENT", "Tôi muốn đặt lại mật khẩu bằng chat.", null, "owner|customer|guest"),
        new IntentExample("CAT-UNSUP-03", "UNSUPPORTED_INTENT", "Tra cứu ưu đãi tích lũy của một khách khác.", null, "owner|customer|guest"),
        new IntentExample("CAT-UNSUP-04", "UNSUPPORTED_INTENT", "Số dư điểm tích lũy trong chương trình khách hàng thân thiết.", null, "owner|customer|guest"),
        new IntentExample("CAT-UNSUP-05", "UNSUPPORTED_INTENT", "Điểm thưởng hội viên còn lại trong chương trình tích điểm.", null, "owner|customer|guest"),
        new IntentExample("CAT-UNSUP-06", "UNSUPPORTED_INTENT", "Tra cứu số điểm thưởng hiện có của khách hàng.", null, "owner|customer|guest"),
        new IntentExample("CAT-WRITE-01", "WRITE_INTENT_UNSUPPORTED", "Hãy đổi giá món phở thành năm mươi nghìn.", null, "owner"),
        new IntentExample("CAT-WRITE-02", "WRITE_INTENT_UNSUPPORTED", "Xóa đơn hàng này giúp tôi.", null, "owner|customer"),
        new IntentExample("CAT-AMB-01", "AMBIGUOUS_MULTI_INTENT", "Xem doanh thu và cho biết món bán chạy nhất.", null, "owner"),
        new IntentExample("CAT-AMB-02", "AMBIGUOUS_MULTI_INTENT", "Mở menu rồi so sánh doanh số hôm qua.", null, "owner|customer|guest")
    };

    // Preserve the original catalog for provenance; these replacements make the
    // capability boundary explicit instead of letting conversational filler dominate.
    public static IReadOnlyList<IntentExample> TargetedRevision { get; } = Examples.Select(example => example.Id switch
    {
        "CAT-SUM-03" => example with { Text = "Tổng hợp doanh thu, số đơn và giá trị đơn trung bình trong một báo cáo KPI." },
        "CAT-SHIFT-02" => example with { Text = "Lịch phân ca làm việc hôm nay của nhân viên gồm giờ bắt đầu và kết thúc." },
        "CAT-CMP-03" => example with { Text = "Chênh lệch doanh thu giữa kỳ hiện tại và kỳ trước là bao nhiêu?" },
        _ => example
    }).Concat(new[]
    {
        new IntentExample("CAT-REV-R2-01", "REVENUE_QUERY", "Doanh số bán hàng phát sinh trong ngày là bao nhiêu tiền?", "get_revenue", "owner"),
        new IntentExample("CAT-REV-R2-02", "REVENUE_QUERY", "Tổng doanh thu cộng gộp các cửa hàng trong hệ thống.", "get_revenue", "owner"),
        new IntentExample("CAT-CMP-R2-01", "REVENUE_COMPARISON", "Tiền thu về kỳ này tăng hay giảm so với kỳ trước?", "get_revenue_comparison", "owner")
    }).ToArray();

    // A single diagnostic intervention: keep the full baseline competition and add
    // conversational/lookup examples only for the three intents under investigation.
    public static IReadOnlyList<IntentExample> WithThreeIntentExamples { get; } = Examples.Concat(new[]
    {
        new IntentExample("CAT-REV-EXP-01", "REVENUE_QUERY", "Báo giúp mình tổng doanh số bán hàng hôm nay.", "get_revenue", "owner"),
        new IntentExample("CAT-REV-EXP-02", "REVENUE_QUERY", "Mình cần tra số tiền bán hàng của quán.", "get_revenue", "owner"),
        new IntentExample("CAT-MENU-EXP-01", "MENU_QUERY", "Cho em xem các lựa chọn đồ ăn cùng giá tiền từng phần.", "customer_get_menu", "customer|guest"),
        new IntentExample("CAT-MENU-EXP-02", "MENU_QUERY", "Nhà hàng có những món gì để tôi lựa chọn?", "customer_get_menu", "customer|guest"),
        new IntentExample("CAT-ORDER-EXP-01", "MY_ORDERS", "Cho mình tra lại lịch sử mua hàng tại nhà hàng.", "get_my_orders", "customer"),
        new IntentExample("CAT-ORDER-EXP-02", "MY_ORDERS", "Trước giờ tôi đặt các đơn gì ở quán, cho xem lại nhé.", "get_my_orders", "customer")
    }).ToArray();
}
