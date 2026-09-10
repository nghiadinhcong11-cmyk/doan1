using System;
using System.Text.Json;
using System.Threading.Tasks;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Tools;
using System.Linq;

namespace RestaurantPOS.AI.Services
{
    public class AiOrchestrator : IAiOrchestrator
    {
        private readonly IGeminiService _geminiService;
        private readonly AiContextService _contextService;
        private readonly AiToolRegistry _toolRegistry;
        private readonly AiPermissionService _permissionService;
        private readonly PromptBuilder _promptBuilder;

        public AiOrchestrator(
            IGeminiService geminiService,
            AiContextService contextService,
            AiToolRegistry toolRegistry,
            AiPermissionService permissionService,
            PromptBuilder promptBuilder)
        {
            _geminiService = geminiService;
            _contextService = contextService;
            _toolRegistry = toolRegistry;
            _permissionService = permissionService;
            _promptBuilder = promptBuilder;
        }

        public async Task<AiResponse> ProcessAsync(AiRequest request, AiUserContext userContext)
        {
            try
            {
                // 1. Xây dựng Ngữ cảnh & Lệnh hệ thống (Prompt) theo Role
                var contextData = await _contextService.GetContextAsync(userContext);
                var systemInstruction = _promptBuilder.BuildSystemInstruction(userContext, contextData);

                // 2. Lấy danh sách Tool được phép cho Role này
                var toolsDef = _toolRegistry.GetGeminiToolsDefinition(userContext.Role);

                // 3. Chuẩn bị Request gửi lên Gemini (Lượt 1) bao gồm lịch sử hội thoại
                var contentsList = new System.Collections.Generic.List<object>();
                if (request.History != null && request.History.Count > 0)
                {
                    foreach (var h in request.History.Where(h => !string.IsNullOrWhiteSpace(h.Content)))
                    {
                        var role = (h.Role?.ToLower() == "model" || h.Role?.ToLower() == "assistant") ? "model" : "user";
                        contentsList.Add(new { role = role, parts = new object[] { new { text = h.Content } } });
                    }
                }
                contentsList.Add(new { role = "user", parts = new object[] { new { text = request.Message } } });

                var requestBody = new
                {
                    contents = contentsList.ToArray(),
                    system_instruction = new { parts = new object[] { new { text = systemInstruction } } },
                    tools = toolsDef.Length > 0 ? toolsDef : null,
                    generationConfig = new { temperature = 0.2, maxOutputTokens = 1024 }
                };

                // 4. Gọi Gemini API
                var jsonResponse = await _geminiService.GenerateContentAsync(requestBody);
                using var doc = JsonDocument.Parse(jsonResponse);

                if (!doc.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                {
                    return new AiResponse { Success = false, Error = "Gemini không trả về kết quả hợp lệ." };
                }

                var content = candidates[0].GetProperty("content");
                var parts = content.GetProperty("parts");

                // 5. Kiểm tra AI có yêu cầu gọi Tool không
                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("functionCall", out var call))
                    {
                        var functionName = call.GetProperty("name").GetString() ?? "";
                        var args = call.TryGetProperty("args", out var arguments) ? arguments : default;

                        // BẢO MẬT BACKEND: Tìm tool và kiểm tra quyền thực thi thực tế
                        var tool = _toolRegistry.GetTool(functionName);
                        if (tool == null)
                            return new AiResponse { Success = false, Error = $"Tool '{functionName}' không tồn tại trong hệ thống." };

                        if (!await _permissionService.CanExecuteAsync(tool, userContext))
                        {
                            return new AiResponse { Success = false, Message = "Rất tiếc, bạn không có quyền thực hiện thao tác này." };
                        }

                        // 6. Thực thi Tool tại Backend
                        var toolResult = await tool.ExecuteAsync(args, userContext);

                        // 7. Gửi kết quả Tool quay lại Gemini để nhận câu trả lời cuối cùng (Lượt 2)
                        var secondContentsList = new System.Collections.Generic.List<object>(contentsList);
                        secondContentsList.Add(new { role = "model", parts = new object[] { new { functionCall = new { name = functionName, args = args } } } });
                        secondContentsList.Add(new { role = "function", parts = new object[] { new { functionResponse = new { name = functionName, response = new { content = toolResult.Success ? toolResult.Data : toolResult.Error } } } } });

                        var secondRequestBody = new
                        {
                            contents = secondContentsList.ToArray(),
                            system_instruction = new { parts = new object[] { new { text = systemInstruction } } },
                            tools = toolsDef,
                            generationConfig = new { temperature = 0.2 }
                        };

                        var finalJsonResponse = await _geminiService.GenerateContentAsync(secondRequestBody);
                        using var finalDoc = JsonDocument.Parse(finalJsonResponse);
                        var finalText = finalDoc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

                        return new AiResponse { Success = true, Message = finalText ?? "" };
                    }
                }

                // 8. Nếu AI trả lời bằng văn bản thông thường (không gọi tool)
                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var text))
                    {
                        return new AiResponse { Success = true, Message = text.GetString() ?? "" };
                    }
                }

                return new AiResponse { Success = false, Error = "AI không trả về text hoặc functionCall." };
            }
            catch (Exception ex)
            {
                return new AiResponse { Success = false, Error = ex.Message };
            }
        }
    }
}
