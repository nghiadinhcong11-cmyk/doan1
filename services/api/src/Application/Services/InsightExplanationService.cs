using System;
using System.Text.Json;
using System.Threading.Tasks;
using RestaurantPOS.AI.Services;
using RestaurantPOS.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace RestaurantPOS.Application.Services;

public class InsightExplanationService : IInsightExplanationService
{
    private readonly IGeminiService _geminiService;
    private readonly ILogger<InsightExplanationService> _logger;

    public InsightExplanationService(IGeminiService geminiService, ILogger<InsightExplanationService> logger)
    {
        _geminiService = geminiService;
        _logger = logger;
    }

    public async Task ExplainInsightAsync(BusinessInsight insight)
    {
        var prompt = $@"
Bạn là chuyên gia phân tích tài chính nhà hàng. Hãy giải thích cảnh báo kinh doanh sau đây:

Loại cảnh báo: {insight.Type}
Mức độ: {insight.Severity}
Tóm tắt: {insight.Summary}
Dữ liệu bằng chứng (Evidence): {insight.EvidenceJson}

YÊU CẦU:
1. Giải thích lý do tại sao cảnh báo này xuất hiện dựa TRÊN DỮ LIỆU BẰNG CHỨNG.
2. Đưa ra 3 khuyến nghị hành động cụ thể, ngắn gọn, mang tính tư vấn.
3. Tuân thủ nguyên tắc: FACT (Dữ liệu thực tế), INFERENCE (Nhận định logic), RECOMMENDATION (Khuyến nghị).
4. KHÔNG tự bịa số liệu mới. Sử dụng đúng các con số trong bằng chứng.
5. KHÔNG tự suy đoán nguyên nhân (ví dụ: do món ăn không ngon, do đối thủ) trừ khi có dữ liệu trong bằng chứng.
6. Lợi nhuận và giá vốn là số liệu ƯỚC TÍNH (Estimated).
7. Nếu có chi phí ""Nguyên liệu"", hãy lưu ý về khả năng double-counting nếu người dùng nhập cả chi phí mua hàng vào Expense.

Trả về kết quả dưới dạng JSON với cấu trúc sau:
{{
  ""explanation"": ""Nội dung giải thích (phải bao gồm FACT và INFERENCE)"",
  ""recommendation"": ""Các khuyến nghị (dạng danh sách hoặc đoạn văn ngắn)""
}}
";

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                responseMimeType = "application/json"
            }
        };

        try
        {
            var jsonResponse = await _geminiService.GenerateContentAsync(requestBody);
            using var doc = JsonDocument.Parse(jsonResponse);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (!string.IsNullOrEmpty(text))
            {
                // Gemini sometimes returns markdown code blocks even with responseMimeType
                var cleanJson = text;
                if (text.StartsWith("```json"))
                {
                    cleanJson = text.Substring(7, text.Length - 10).Trim();
                }
                else if (text.StartsWith("```"))
                {
                    cleanJson = text.Substring(3, text.Length - 6).Trim();
                }

                using var resultDoc = JsonDocument.Parse(cleanJson);
                insight.AiExplanation = resultDoc.RootElement.GetProperty("explanation").GetString();
                insight.AiRecommendation = resultDoc.RootElement.GetProperty("recommendation").GetString();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error explaining insight {InsightId} with Gemini.", insight.Id);
            // We intentionally leave AiExplanation and AiRecommendation null if AI fails.
        }
    }
}
