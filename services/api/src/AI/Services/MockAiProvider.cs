using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RestaurantPOS.AI.Services;

/// <summary>
/// Deterministic Development-only provider. It recognizes a small allowlist of
/// Vietnamese intents and emits the existing function-calling response contract;
/// it does not reason autonomously or call an external model.
/// </summary>
public sealed class MockAiProvider : IAiProvider
{
    private readonly IHostEnvironment _environment;
    private readonly ILogger<MockAiProvider> _logger;

    public MockAiProvider(IHostEnvironment environment, ILogger<MockAiProvider> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public string Name => "Mock";

    public bool IsConfigured =>
        string.Equals(_environment.EnvironmentName, Environments.Development, StringComparison.OrdinalIgnoreCase);

    public Task<string> GenerateContentAsync(object requestBody, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(requestBody));
        var root = document.RootElement;
        var history = ReadContentTexts(root).ToList();

        if (RequestsStructuredInsight(root))
        {
            return Task.FromResult(TextResponse(JsonSerializer.Serialize(new
            {
                explanation = "FACT: Đây là diễn giải được tạo từ dữ liệu bằng chứng được cung cấp. INFERENCE: Mock AI không suy đoán thêm ngoài dữ liệu đó.",
                recommendation = "RECOMMENDATION: Kiểm tra dữ liệu bằng chứng và theo dõi chỉ số trong kỳ tiếp theo."
            })));
        }

        if (TryReadToolResult(root, out var toolName, out var toolResult))
        {
            LogDiagnostic("TOOL_RESULT", toolName, toolExecuted: true);
            return Task.FromResult(TextResponse(FormatToolResult(toolName, toolResult)));
        }

        var message = history.LastOrDefault() ?? string.Empty;
        var previousMessages = history.Take(Math.Max(0, history.Count - 1)).ToArray();
        var intent = MockAiIntentRecognizer.Recognize(message, previousMessages);

        if (intent.ToolName is null)
        {
            LogDiagnostic(intent.Code, "NONE", toolExecuted: false);
            return Task.FromResult(TextResponse(intent.Response));
        }

        if (HasTool(root, intent.ToolName))
        {
            LogDiagnostic(intent.Code, intent.ToolName, toolExecuted: false);
            return Task.FromResult(FunctionCallResponse(intent.ToolName, intent.Arguments));
        }

        LogDiagnostic("TOOL_UNAVAILABLE_FOR_ROLE", intent.ToolName, toolExecuted: false);
        return Task.FromResult(TextResponse(
            "Truy vấn này không được cấp cho vai trò hiện tại hoặc chưa có tool đọc tương ứng."));
    }

    private static bool HasTool(JsonElement root, string toolName) =>
        root.TryGetProperty("tools", out var tools) && tools.ValueKind == JsonValueKind.Array
        && tools.EnumerateArray().Any(tool =>
            tool.TryGetProperty("function_declarations", out var declarations)
            && declarations.ValueKind == JsonValueKind.Array
            && declarations.EnumerateArray().Any(declaration =>
                declaration.TryGetProperty("name", out var name)
                && string.Equals(name.GetString(), toolName, StringComparison.OrdinalIgnoreCase)));

    private static bool RequestsStructuredInsight(JsonElement root) =>
        root.TryGetProperty("generationConfig", out var generationConfig)
        && generationConfig.ValueKind == JsonValueKind.Object
        && generationConfig.TryGetProperty("responseMimeType", out var responseMimeType)
        && string.Equals(responseMimeType.GetString(), "application/json", StringComparison.OrdinalIgnoreCase);

    private static IEnumerable<string> ReadContentTexts(JsonElement root)
    {
        if (!root.TryGetProperty("contents", out var contents) || contents.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        foreach (var content in contents.EnumerateArray())
        {
            if (!content.TryGetProperty("parts", out var parts) || parts.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
                {
                    yield return text.GetString() ?? string.Empty;
                }
            }
        }
    }

    private static bool TryReadToolResult(JsonElement root, out string toolName, out string content)
    {
        toolName = string.Empty;
        content = string.Empty;
        if (!root.TryGetProperty("contents", out var contents) || contents.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var turn in contents.EnumerateArray().Reverse())
        {
            if (!turn.TryGetProperty("parts", out var parts) || parts.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var part in parts.EnumerateArray())
            {
                if (!part.TryGetProperty("functionResponse", out var response))
                {
                    continue;
                }

                toolName = response.TryGetProperty("name", out var name) ? name.GetString() ?? string.Empty : string.Empty;
                if (response.TryGetProperty("response", out var responseBody)
                    && responseBody.TryGetProperty("content", out var value))
                {
                    content = value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.GetRawText();
                }

                return true;
            }
        }

        return false;
    }

    private void LogDiagnostic(string intentCode, string toolName, bool toolExecuted) =>
        _logger.LogInformation(
            "AI_PROVIDER_MODE=MOCK AI_INTENT_CODE={IntentCode} AI_TOOL_NAME={ToolName} TOOL_EXECUTED={ToolExecuted} AI_FAILURE_CODE=NONE",
            intentCode,
            toolName,
            toolExecuted ? "YES" : "NO");

    private static string TextResponse(string text) =>
        JsonSerializer.Serialize(new { candidates = new[] { new { content = new { parts = new[] { new { text } } } } } });

    private static string FunctionCallResponse(string name, object arguments) =>
        JsonSerializer.Serialize(new
        {
            candidates = new[]
            {
                new
                {
                    content = new
                    {
                        parts = new[]
                        {
                            new { functionCall = new { id = "mock-call-1", name, args = arguments } }
                        }
                    }
                }
            }
        });

    private static string FormatToolResult(string toolName, string content)
    {
        if (toolName is "get_revenue" or "employee_get_table_summary" or "customer_get_menu" or "get_my_shift")
        {
            return content;
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            return toolName switch
            {
                "get_business_summary" => FormatBusinessSummary(document.RootElement),
                "get_revenue_comparison" => FormatRevenueComparison(document.RootElement),
                "get_best_sellers" => FormatBestSellers(document.RootElement),
                "get_order_list" => FormatOrderList(document.RootElement),
                _ => content
            };
        }
        catch (JsonException)
        {
            return content;
        }
    }

    private static string FormatBusinessSummary(JsonElement root)
    {
        var period = ReadString(root, "Period") ?? "khoảng thời gian đã chọn";
        var kpis = root.TryGetProperty("KPIs", out var kpi) ? kpi : default;
        return $"Tóm tắt kinh doanh ({period}): doanh thu {FormatNumber(kpis, "Revenue")} VNĐ, " +
               $"số đơn {FormatNumber(kpis, "OrderCount")}, AOV {FormatNumber(kpis, "AverageOrderValue")} VNĐ.";
    }

    private static string FormatRevenueComparison(JsonElement root)
    {
        var current = root.TryGetProperty("Current", out var currentValue) ? currentValue : default;
        var comparison = root.TryGetProperty("Comparison", out var comparisonValue) ? comparisonValue : default;
        var analysis = root.TryGetProperty("Analysis", out var analysisValue) ? analysisValue : default;
        var currentPeriod = ReadString(current, "Period") ?? "kỳ hiện tại";
        var comparisonPeriod = ReadString(comparison, "Period") ?? "kỳ so sánh";
        var direction = ReadString(analysis, "Direction") switch
        {
            "up" => "tăng",
            "down" => "giảm",
            _ => "không đổi"
        };
        return $"So sánh doanh thu: {currentPeriod} {FormatNumber(current, "Revenue")} VNĐ, " +
               $"{comparisonPeriod} {FormatNumber(comparison, "Revenue")} VNĐ; kết quả {direction}.";
    }

    private static string FormatBestSellers(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
        {
            return "Không có dữ liệu món bán chạy trong khoảng thời gian đã chọn.";
        }

        var items = root.EnumerateArray()
            .Select(item => $"- {ReadString(item, "ProductName") ?? "Món không xác định"}: {FormatNumber(item, "TotalQuantity")}")
            .ToArray();
        return "Các món bán chạy:\n" + string.Join("\n", items);
    }

    private static string FormatOrderList(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
        {
            return "Không tìm thấy đơn hàng nào phù hợp.";
        }

        var items = root.EnumerateArray().Select(item =>
            $"- Đơn {ReadString(item, "InvoiceCode") ?? "không xác định"}: {ReadString(item, "Status") ?? "không rõ trạng thái"}"
        ).ToArray();
        return "Danh sách đơn hàng:\n" + string.Join("\n", items);
    }

    private static string? ReadString(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static string FormatNumber(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out var property))
        {
            return "không xác định";
        }

        return property.ValueKind switch
        {
            JsonValueKind.Number when property.TryGetDecimal(out var decimalValue) => decimalValue.ToString("N0", new System.Globalization.CultureInfo("vi-VN")),
            JsonValueKind.Number when property.TryGetInt32(out var intValue) => intValue.ToString("N0", new System.Globalization.CultureInfo("vi-VN")),
            _ => "không xác định"
        };
    }

}
