using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Tools;
using RestaurantPOS.AI.Security;
using RestaurantPOS.AI.Diagnostics;
using System.Linq;
using RestaurantPOS.Application.Common.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantPOS.AI.Services
{
    public class AiOrchestrator : IAiOrchestrator
    {
        private const string AuthContextPhase = "AUTH_CONTEXT";
        private const string BusinessContextPhase = "BUSINESS_CONTEXT";
        private const string PromptBuildPhase = "PROMPT_BUILD";
        private const string ToolRegistryPhase = "TOOL_REGISTRY";
        private const string ProviderRequestPhase = "AI_REQUEST";
        private const string ToolExecutionPhase = "TOOL_EXECUTION";
        private const string ResponseProcessingPhase = "RESPONSE_PROCESSING";
        private static readonly IReadOnlySet<string> GuestToolAllowlist =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "customer_get_menu"
            };

        private readonly IAiProvider _aiProvider;
        private readonly AiContextService _contextService;
        private readonly AiToolRegistry _toolRegistry;
        private readonly AiPermissionService _permissionService;
        private readonly PromptBuilder _promptBuilder;
        private readonly ILogger<AiOrchestrator>? _logger;
        private readonly IAiToolExecutionAuditSink? _toolAudit;

        [ActivatorUtilitiesConstructor]
        public AiOrchestrator(
            IAiProvider aiProvider,
            AiContextService contextService,
            AiToolRegistry toolRegistry,
            AiPermissionService permissionService,
            PromptBuilder promptBuilder,
            ILogger<AiOrchestrator>? logger = null,
            IAiToolExecutionAuditSink? toolAudit = null)
        {
            _aiProvider = aiProvider;
            _contextService = contextService;
            _toolRegistry = toolRegistry;
            _permissionService = permissionService;
            _promptBuilder = promptBuilder;
            _logger = logger;
            _toolAudit = toolAudit;
        }

        public Task<AiResponse> ProcessAsync(
            AiRequest request,
            AiUserContext userContext,
            CancellationToken cancellationToken = default) =>
            ProcessInternalAsync(request, userContext, readOnlyMode: false, cancellationToken);

        public Task<AiResponse> ProcessReadOnlyAsync(
            AiRequest request,
            AiUserContext userContext,
            CancellationToken cancellationToken = default) =>
            ProcessInternalAsync(request, userContext, readOnlyMode: true, cancellationToken);

        public Task<AiResponse> ProcessGuestReadOnlyAsync(
            AiRequest request,
            CancellationToken cancellationToken = default) =>
            ProcessInternalAsync(
                request,
                new AiUserContext
                {
                    Role = RoleNames.Customer,
                    IsGuest = true,
                    UserName = "Khách vãng lai"
                },
                readOnlyMode: true,
                cancellationToken: cancellationToken,
                allowedToolNames: GuestToolAllowlist);

        private async Task<AiResponse> ProcessInternalAsync(
            AiRequest request,
            AiUserContext userContext,
            bool readOnlyMode,
            CancellationToken cancellationToken,
            IReadOnlySet<string>? allowedToolNames = null)
        {
            var correlationId = Guid.NewGuid();
            var stopwatch = Stopwatch.StartNew();
            var currentPhase = AuthContextPhase;

            try
            {
                if (!RoleNames.IsKnownRole(userContext.Role))
                {
                    LogSafeFailure(AiFailureCodes.ToolPermissionDenied, currentPhase, correlationId, elapsedMilliseconds: stopwatch.ElapsedMilliseconds);
                    return new AiResponse { Success = false, Error = "Bạn không có quyền sử dụng AI Assistant." };
                }

                if (RoleNames.IsBranchScopedStaffRole(userContext.Role) && !userContext.BranchId.HasValue)
                {
                    currentPhase = BusinessContextPhase;
                    LogSafeFailure(AiFailureCodes.OrchestrationFailed, currentPhase, correlationId, elapsedMilliseconds: stopwatch.ElapsedMilliseconds);
                    return new AiResponse { Success = false, Error = "Không xác định được chi nhánh của tài khoản." };
                }

                // 1. Xây dựng Ngữ cảnh & Lệnh hệ thống (Prompt) theo Role
                currentPhase = BusinessContextPhase;
                var contextData = await _contextService.GetContextAsync(userContext);
                currentPhase = PromptBuildPhase;
                var systemInstruction = _promptBuilder.BuildSystemInstruction(userContext, contextData);

                // 2. Lấy danh sách Tool được phép cho Role này
                currentPhase = ToolRegistryPhase;
                var toolsDef = _toolRegistry.GetToolsDefinition(userContext.Role, readOnlyMode, allowedToolNames);

                // 3. Chuẩn bị request cho Mock AI (Lượt 1) bao gồm lịch sử hội thoại
                var contentsList = new System.Collections.Generic.List<object>();
                if (request.History != null && request.History.Count > 0)
                {
                    foreach (var h in request.History.Where(h => !string.IsNullOrWhiteSpace(h.Content)))
                    {
                        // Client history is untrusted context. Never let a client-supplied
                        // role impersonate a trusted model turn.
                        contentsList.Add(new
                        {
                            role = "user",
                            parts = new object[]
                            {
                                new { text = $"Untrusted conversation history:\n{h.Content}" }
                            }
                        });
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

                // 4. Lượt hội thoại với Mock AI (Hỗ trợ Sequential Tool Calling)
                int iterations = 0;
                const int MAX_ITERATIONS = 5;
                var currentContents = new System.Collections.Generic.List<object>(contentsList);
                while (iterations < MAX_ITERATIONS)
                {
                    var providerRequest = new
                    {
                        contents = currentContents.ToArray(),
                        system_instruction = new { parts = new object[] { new { text = systemInstruction } } },
                        tools = toolsDef.Length > 0 ? toolsDef : null,
                        generationConfig = new { temperature = 0.2, maxOutputTokens = 1024 }
                    };

                    currentPhase = ProviderRequestPhase;
                    var providerResponse = await _aiProvider.GenerateContentAsync(providerRequest, cancellationToken);

                    currentPhase = ResponseProcessingPhase;
                    using var doc = JsonDocument.Parse(providerResponse);
                    if (!doc.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                    {
                        LogSafeFailure(AiFailureCodes.InvalidResponse, "ai_response", correlationId);
                        return new AiResponse { Success = false, Error = "AI không trả về kết quả hợp lệ." };
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
                            // Preserve the complete function call, including its id and any
                            // model metadata required to correlate the function response.
                            modelParts.Add(new { functionCall = call.Clone() });
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
                            var args = call.TryGetProperty("args", out var arguments) ? arguments.Clone() : default;

                            currentPhase = ToolRegistryPhase;
                            var tool = _toolRegistry.GetTool(functionName);
                            _toolAudit?.Record(
                                correlationId,
                                functionName,
                                userContext,
                                tool?.RiskLevel,
                                "SELECTED",
                                "SELECTED",
                                readOnlyMode,
                                stopwatch.ElapsedMilliseconds,
                                tool == null ? "TOOL_NOT_FOUND" : "NONE");
                            AiToolResult toolResult;

                            if (tool == null)
                            {
                                _toolAudit?.Record(correlationId, functionName, userContext, null, "DENIED", "DENIED_TOOL_NOT_FOUND", readOnlyMode, stopwatch.ElapsedMilliseconds, "TOOL_NOT_FOUND");
                                toolResult = AiToolResult.CreateError($"Tool '{functionName}' không tồn tại.");
                            }
                            else if (allowedToolNames is not null && !allowedToolNames.Contains(tool.Name))
                            {
                                _toolAudit?.Record(correlationId, tool.Name, userContext, tool.RiskLevel, "DENIED", "DENIED_GUEST_ALLOWLIST", readOnlyMode, stopwatch.ElapsedMilliseconds, "GUEST_TOOL_BLOCKED");
                                toolResult = AiToolResult.CreateError("Tool này không được phép cho khách vãng lai.");
                            }
                            else if (readOnlyMode && tool.RiskLevel != ToolRiskLevel.Read)
                            {
                                // Defense in depth: the read-only registry filter is not the only gate.
                                _toolAudit?.Record(correlationId, tool.Name, userContext, tool.RiskLevel, "DENIED", "DENIED_READ_ONLY", readOnlyMode, stopwatch.ElapsedMilliseconds, "READ_ONLY_BLOCKED");
                                LogSafeFailure(AiFailureCodes.ReadOnlyPolicyBlocked, ToolExecutionPhase, correlationId, toolName: functionName, elapsedMilliseconds: stopwatch.ElapsedMilliseconds);
                                toolResult = AiToolResult.CreateError("Tool này không được phép trong chế độ chỉ đọc.");
                            }
                            else if (!await _permissionService.CanExecuteAsync(tool, userContext))
                            {
                                _toolAudit?.Record(correlationId, tool.Name, userContext, tool.RiskLevel, "DENIED", "DENIED_PERMISSION", readOnlyMode, stopwatch.ElapsedMilliseconds, "PERMISSION_DENIED");
                                LogSafeFailure(AiFailureCodes.ToolPermissionDenied, ToolExecutionPhase, correlationId, toolName: functionName, elapsedMilliseconds: stopwatch.ElapsedMilliseconds);
                                toolResult = AiToolResult.CreateError("Bạn không có quyền thực hiện thao tác này.");
                            }
                            else
                            {
                                _toolAudit?.Record(correlationId, tool.Name, userContext, tool.RiskLevel, "AUTHORIZED", "AUTHORIZED", readOnlyMode, stopwatch.ElapsedMilliseconds, "NONE");
                                currentPhase = ToolExecutionPhase;
                                try
                                {
                                    toolResult = await tool.ExecuteAsync(args, userContext);
                                }
                                catch
                                {
                                    _toolAudit?.Record(correlationId, tool.Name, userContext, tool.RiskLevel, "FAILED", "FAILED", readOnlyMode, stopwatch.ElapsedMilliseconds, "TOOL_EXCEPTION");
                                    throw;
                                }
                            }

                            if (toolResult.InternalException != null)
                            {
                                _toolAudit?.Record(correlationId, tool?.Name ?? functionName, userContext, tool?.RiskLevel, "FAILED", "FAILED", readOnlyMode, stopwatch.ElapsedMilliseconds, "TOOL_EXCEPTION");
                                LogSafeFailure(AiFailureCodes.ToolExecutionFailed, ToolExecutionPhase, correlationId, toolName: functionName, elapsedMilliseconds: stopwatch.ElapsedMilliseconds);
                            }
                            else if (tool != null && toolResult.Success)
                            {
                                _toolAudit?.Record(correlationId, tool.Name, userContext, tool.RiskLevel, "EXECUTED", "EXECUTED", readOnlyMode, stopwatch.ElapsedMilliseconds, "NONE");
                            }
                            else if (tool != null)
                            {
                                _toolAudit?.Record(correlationId, tool.Name, userContext, tool.RiskLevel, "FAILED", "FAILED", readOnlyMode, stopwatch.ElapsedMilliseconds, "TOOL_RESULT_ERROR");
                            }

                            currentPhase = ResponseProcessingPhase;
                            // Stop immediately at the server-trusted confirmation boundary.
                            // The model must not be able to self-confirm in the same turn.
                            if (toolResult.Confirmation != null)
                            {
                                return new AiResponse
                                {
                                    Success = true,
                                    Message = toolResult.Data,
                                    Confirmation = toolResult.Confirmation
                                };
                            }

                            var functionResponse = new Dictionary<string, object?>
                            {
                                ["name"] = functionName,
                                ["response"] = new { content = toolResult.Success ? toolResult.Data : toolResult.Error }
                            };

                            if (call.TryGetProperty("id", out var callId)
                                && callId.ValueKind == JsonValueKind.String
                                && !string.IsNullOrWhiteSpace(callId.GetString()))
                            {
                                functionResponse["id"] = callId.GetString();
                            }

                            functionResponses.Add(new { functionResponse });
                        }
                    }

                    // The Mock AI contract expects a function response as a user turn. Sending role=function
                    // causes the follow-up Generate Content request to be rejected after a tool call.
                    currentContents.Add(new { role = "user", parts = functionResponses.ToArray() });
                    iterations++;
                }

                LogSafeFailure(AiFailureCodes.OrchestrationFailed, "tool_iteration_limit", correlationId);
                return new AiResponse { Success = false, Error = "Đã vượt quá giới hạn lượt gọi Tool (Sequential Tool Calling)." };
            }
            catch (JsonException)
            {
                LogSafeFailure(AiFailureCodes.InvalidResponse, currentPhase, correlationId, elapsedMilliseconds: stopwatch.ElapsedMilliseconds);
                return new AiResponse
                {
                    Success = false,
                    Error = "AI request could not be processed."
                };
            }
            catch (Exception)
            {
                LogSafeFailure(AiFailureCodes.OrchestrationFailed, currentPhase, correlationId, elapsedMilliseconds: stopwatch.ElapsedMilliseconds);
                return new AiResponse
                {
                    Success = false,
                    Error = "AI request could not be processed."
                };
            }
        }

        private void LogSafeFailure(
            string code,
            string phase,
            Guid correlationId,
            long? elapsedMilliseconds = null,
            string? toolName = null,
            string? failureSubphase = null)
        {
            _logger?.LogWarning(
                "AI_FAILURE_CODE={FailureCode} AI_PHASE={Phase} AI_SUBPHASE={Subphase} AI_CORRELATION_ID={CorrelationId} TOOL_EXECUTED={ToolExecuted} AI_DURATION_MS={DurationMilliseconds} AI_TOOL_NAME={ToolName}",
                code,
                phase,
                failureSubphase ?? "UNKNOWN",
                correlationId,
                toolName == null ? "NO" : "YES",
                elapsedMilliseconds,
                toolName);
        }
    }
}
