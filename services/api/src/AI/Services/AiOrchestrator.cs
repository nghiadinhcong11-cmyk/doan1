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

                // 4. Lượt hội thoại với Gemini (Hỗ trợ Sequential Tool Calling)
                int iterations = 0;
                const int MAX_ITERATIONS = 5;
                var currentContents = new System.Collections.Generic.List<object>(contentsList);

                while (iterations < MAX_ITERATIONS)
                {
                    var response = await _geminiService.GenerateContentAsync(new
                    {
                        contents = currentContents.ToArray(),
                        system_instruction = new { parts = new object[] { new { text = systemInstruction } } },
                        tools = toolsDef.Length > 0 ? toolsDef : null,
                        generationConfig = new { temperature = 0.2, maxOutputTokens = 1024 }
                    });

                    using var doc = JsonDocument.Parse(response);
                    if (!doc.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                    {
                        return new AiResponse { Success = false, Error = "Gemini không trả về kết quả hợp lệ." };
                    }

                    var candidate = candidates[0];
                    var content = candidate.GetProperty("content");
                    var parts = content.GetProperty("parts");

                    // Lưu phản hồi của model vào history cho lượt tiếp theo
                    var modelParts = new System.Collections.Generic.List<object>();
                    bool hasFunctionCall = false;

                    foreach (var part in parts.EnumerateArray())
                    {
                        if (part.TryGetProperty("functionCall", out var call))
                        {
                            hasFunctionCall = true;
                            var functionName = call.GetProperty("name").GetString() ?? "";
                            var args = call.TryGetProperty("args", out var arguments) ? arguments : default;
                            modelParts.Add(new { functionCall = new { name = functionName, args = args } });
                        }
                        else if (part.TryGetProperty("text", out var text))
                        {
                            modelParts.Add(new { text = text.GetString() });
                        }
                    }

                    currentContents.Add(new { role = "model", parts = modelParts.ToArray() });

                    if (!hasFunctionCall)
                    {
                        // Nếu không có function call, trả về kết quả text cuối cùng
                        string? finalText = null;
                        foreach (var part in parts.EnumerateArray())
                        {
                            if (part.TryGetProperty("text", out var text))
                            {
                                finalText = text.GetString();
                                break;
                            }
                        }
                        return new AiResponse { Success = true, Message = finalText ?? "" };
                    }

                    // Xử lý các function call trong lượt này
                    var functionResponses = new System.Collections.Generic.List<object>();
                    foreach (var part in parts.EnumerateArray())
                    {
                        if (part.TryGetProperty("functionCall", out var call))
                        {
                            var functionName = call.GetProperty("name").GetString() ?? "";
                            var args = call.TryGetProperty("args", out var arguments) ? arguments : default;

                            var tool = _toolRegistry.GetTool(functionName);
                            AiToolResult toolResult;

                            if (tool == null)
                            {
                                toolResult = AiToolResult.CreateError($"Tool '{functionName}' không tồn tại.");
                            }
                            else if (!await _permissionService.CanExecuteAsync(tool, userContext))
                            {
                                toolResult = AiToolResult.CreateError("Bạn không có quyền thực hiện thao tác này.");
                            }
                            else
                            {
                                toolResult = await tool.ExecuteAsync(args, userContext);
                            }

                            functionResponses.Add(new
                            {
                                functionResponse = new
                                {
                                    name = functionName,
                                    response = new { content = toolResult.Success ? toolResult.Data : toolResult.Error }
                                }
                            });
                        }
                    }

                    currentContents.Add(new { role = "function", parts = functionResponses.ToArray() });
                    iterations++;
                }

                return new AiResponse { Success = false, Error = "Đã vượt quá giới hạn lượt gọi Tool (Sequential Tool Calling)." };
            }
            catch (Exception ex)
            {
                return new AiResponse { Success = false, Error = ex.Message };
            }
        }
    }
}
