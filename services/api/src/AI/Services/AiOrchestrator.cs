using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using RestaurantPOS.AI.Diagnostics;
using RestaurantPOS.AI.Models;
using RestaurantPOS.AI.Security;
using RestaurantPOS.AI.Tools;
using RestaurantPOS.Application.Common.Security;

namespace RestaurantPOS.AI.Services;

// Chat has one execution path: an enabled E5 pilot may select a registered, authorized tool.
public sealed class AiOrchestrator : IAiOrchestrator
{
    private readonly AiToolRegistry _registry;
    private readonly AiPermissionService _permission;
    private readonly AdminEmbeddingPilot _adminPilot;
    private readonly CustomerGuestEmbeddingPilot _customerGuestPilot;
    private readonly IAiToolExecutionAuditSink? _audit;
    private readonly AiCorrelationContext? _correlation;
    private readonly ILogger<AiOrchestrator>? _logger;

    public AiOrchestrator(AiToolRegistry registry, AiPermissionService permission,
        AdminEmbeddingPilot adminPilot, CustomerGuestEmbeddingPilot customerGuestPilot,
        ILogger<AiOrchestrator>? logger = null, IAiToolExecutionAuditSink? audit = null,
        AiCorrelationContext? correlation = null)
    {
        _registry = registry;
        _permission = permission;
        _adminPilot = adminPilot;
        _customerGuestPilot = customerGuestPilot;
        _logger = logger;
        _audit = audit;
        _correlation = correlation;
    }

    public Task<AiResponse> ProcessAsync(AiRequest request, AiUserContext context, CancellationToken token = default) =>
        ProcessInternalAsync(request, context, token);

    public Task<AiResponse> ProcessReadOnlyAsync(AiRequest request, AiUserContext context, CancellationToken token = default) =>
        ProcessInternalAsync(request, context, token);

    public Task<AiResponse> ProcessGuestReadOnlyAsync(AiRequest request, CancellationToken token = default) =>
        Task.FromResult(new AiResponse { Success = false, Error = "Verified QR session is required." });

    public Task<AiResponse> ProcessGuestReadOnlyAsync(AiRequest request, AiUserContext context, CancellationToken token = default) =>
        ProcessInternalAsync(request, context, token);

    public async Task<AiResponse> ConfirmProductPriceAsync(string token, AiUserContext userContext, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token) || userContext.Role != RoleNames.Admin || userContext.UserId == Guid.Empty)
            return new AiResponse { Success = false, Error = "Xác nhận không hợp lệ hoặc không được phép." };

        var tool = _registry.GetTool("update_product_price");
        if (tool is not IProductPriceConfirmationTool confirmationTool ||
            !await _permission.CanExecuteAsync(tool, userContext))
            return new AiResponse { Success = false, Error = "AI tool is not authorized." };

        cancellationToken.ThrowIfCancellationRequested();
        var result = await confirmationTool.ConfirmAsync(token, userContext);
        return result.Success
            ? new AiResponse { Success = true, Message = result.Data }
            : new AiResponse { Success = false, Error = result.Error ?? "Không thể cập nhật giá sản phẩm." };
    }

    public async Task<AiResponse> ConfirmCatalogCreationAsync(string token, string type, AiUserContext userContext, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token) || userContext.Role != RoleNames.Admin || userContext.UserId == Guid.Empty)
            return new AiResponse { Success = false, Error = "Xác nhận không hợp lệ hoặc không được phép." };
        var toolName = type.Equals("topping-create", StringComparison.OrdinalIgnoreCase) ? "create_topping" : "create_product";
        var tool = _registry.GetTool(toolName);
        if (tool is not ICatalogCreationConfirmationTool confirmationTool || !await _permission.CanExecuteAsync(tool, userContext))
            return new AiResponse { Success = false, Error = "AI tool is not authorized." };
        cancellationToken.ThrowIfCancellationRequested();
        var result = await confirmationTool.ConfirmAsync(token, userContext);
        return result.Success ? new AiResponse { Success = true, Message = result.Data } : new AiResponse { Success = false, Error = result.Error ?? "Không thể tạo dữ liệu catalog." };
    }

    public async Task<AiResponse> ConfirmProductNameAsync(string token, AiUserContext userContext, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token) || userContext.Role != RoleNames.Admin || userContext.UserId == Guid.Empty)
            return new AiResponse { Success = false, Error = "Xác nhận không hợp lệ hoặc không được phép." };
        var tool = _registry.GetTool("update_product_name");
        if (tool is not IProductNameConfirmationTool confirmationTool || !await _permission.CanExecuteAsync(tool, userContext))
            return new AiResponse { Success = false, Error = "AI tool is not authorized." };
        cancellationToken.ThrowIfCancellationRequested();
        var result = await confirmationTool.ConfirmAsync(token, userContext);
        return result.Success ? new AiResponse { Success = true, Message = result.Data } : new AiResponse { Success = false, Error = result.Error ?? "Không thể đổi tên sản phẩm." };
    }

    private async Task<AiResponse> ProcessInternalAsync(AiRequest request, AiUserContext context, CancellationToken token)
    {
        var correlationId = _correlation?.CorrelationId ?? Guid.Empty;
        if (correlationId == Guid.Empty) correlationId = Guid.NewGuid();
        var stopwatch = Stopwatch.StartNew();
        var phase = "AUTH_CONTEXT";
        try
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Message) ||
                (!context.IsGuest && context.Role != RoleNames.Admin && context.Role != RoleNames.Customer) ||
                (context.IsGuest && (context.Role != RoleNames.Customer || !context.HasVerifiedGuestSession || !context.BranchId.HasValue)) ||
                (!context.IsGuest && context.Role == RoleNames.Customer && (!context.CustomerId.HasValue || context.CustomerId == Guid.Empty)))
                return new AiResponse { Success = false, Error = "AI access is not authorized for this identity." };

            // Social messages do not represent a business intent and must not be
            // sent through the embedding/tool classifier. Otherwise a greeting
            // can be misclassified as an unsupported admin request.
            if (TryGetSocialResponse(request.Message, context, out var socialResponse))
                return new AiResponse { Success = true, Message = socialResponse };

            phase = "EMBEDDING_DECISION";
            if (context.Role == RoleNames.Admin)
            {
                if (!_adminPilot.IsEnabled)
                    return new AiResponse { Success = false, Error = "Admin AI is disabled." };
                var decision = await _adminPilot.DecideAsync(request.Message, context, token);
                _logger?.LogInformation("AI_E5_DECISION CorrelationId={CorrelationId} Role=admin Decision={Decision} Reason={Reason} Intent={Intent} Tool={Tool}",
                    correlationId, decision.Decision, decision.Reason, decision.AcceptedIntent, decision.ProposedTool);
                if (decision.Decision == "CLARIFY")
                {
                    if (decision.Boundary == "TEMPORAL_SCOPE")
                        return new AiResponse { Success = true, Message = "Bạn muốn xem kỳ nào: hôm nay, tuần này, tháng này hay năm nay?" };
                    if (decision.Boundary == "COMPARISON_SCOPE")
                        return new AiResponse { Success = true, Message = "Bạn muốn so sánh khoảng thời gian nào, ví dụ hôm nay với hôm qua hoặc tháng này với tháng trước?" };
                    if (decision.Boundary == "ORDER_STATUS_SCOPE")
                        return new AiResponse { Success = true, Message = "Bạn muốn xem đơn đang xử lý, đã hoàn thành hay đã hủy trong khoảng thời gian nào?" };
                    if (decision.Boundary == "CATALOG_CREATION_SCOPE")
                        return new AiResponse { Success = true, Message = "Vui lòng nhập đủ thông tin: tên, giá, loại và nhóm. Ví dụ: Tạo món Bạc xỉu giá 30000, loại đồ uống, nhóm Cà phê." };
                    if (decision.Boundary == "NAME_UPDATE_SCOPE")
                        return new AiResponse { Success = true, Message = "Vui lòng nhập tên món hiện tại và tên mới, ví dụ: Đổi món Bạc xỉu TEST thành Bạc xỉu." };
                    if (decision.Boundary == "PRICE_UPDATE_SCOPE")
                        return new AiResponse { Success = true, Message = "Vui lòng nhập tên món và giá mới, ví dụ: Đổi giá món Phở thành 50000." };
                    var message = decision.Boundary == "BRANCH_SELECTION"
                        ? "Vui lòng nói rõ muốn xem cơ sở hiện tại hay toàn bộ các cơ sở. Việc chọn một cơ sở theo tên chưa được hỗ trợ trong câu hỏi AI."
                        : "Vui lòng làm rõ kỳ thời gian, trạng thái đơn hoặc nội dung cần xem.";
                    return new AiResponse { Success = true, Message = message };
                }
                if (decision.Decision != "ACCEPT")
                    return new AiResponse { Success = true, Message = "Yêu cầu nằm ngoài phạm vi AI Admin." };
                return await ExecuteAsync(new AiPilotToolDecision(decision.Decision, decision.Reason,
                    decision.AcceptedIntent, decision.ProposedTool, decision.Arguments, decision.Period,
                    decision.TopScore, decision.Margin), context, correlationId, stopwatch, true, token);
            }

            if (!_customerGuestPilot.IsEnabled)
                return new AiResponse { Success = false, Error = "Customer/Guest AI is disabled." };
            var customerDecision = await _customerGuestPilot.DecideAsync(request.Message, context, token);
            _logger?.LogInformation("AI_E5_DECISION CorrelationId={CorrelationId} Role={Role} Decision={Decision} Reason={Reason} Intent={Intent} Tool={Tool}",
                correlationId, context.IsGuest ? "guest" : "customer", customerDecision.Decision,
                customerDecision.Reason, customerDecision.Intent, customerDecision.ProposedTool);
            if (customerDecision.Decision == "CLARIFY")
                return new AiResponse { Success = true, Message = "Vui lòng nói rõ hơn yêu cầu về thực đơn hoặc đơn hàng của bạn." };
            if (customerDecision.Decision == "REJECT")
                return new AiResponse { Success = true, Message = context.IsGuest
                    ? "Phiên khách chỉ được phép xem thực đơn của chi nhánh hiện tại."
                    : "Bạn không có quyền truy cập thông tin này." };
            if (customerDecision.Decision != "ACCEPT")
                return new AiResponse { Success = true, Message = "Yêu cầu này chưa được hỗ trợ trong trợ lý Customer/Guest." };
            return await ExecuteAsync(new AiPilotToolDecision(customerDecision.Decision, customerDecision.Reason,
                customerDecision.Intent, customerDecision.ProposedTool, customerDecision.Arguments,
                "unknown", customerDecision.TopScore, customerDecision.Margin), context, correlationId,
                stopwatch, false, token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            return new AiResponse { Success = false, Error = "AI request was cancelled." };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "AI_E5_FAILURE CorrelationId={CorrelationId} Phase={Phase} ToolExecuted=NO", correlationId, phase);
            return new AiResponse { Success = false, Error = "Semantic AI is unavailable; request was not executed." };
        }
    }

    private static bool TryGetSocialResponse(string text, AiUserContext context, out string response)
    {
        var normalized = FoldForSocial(text).Trim();
        if (Regex.IsMatch(normalized, "^(?:(?:xin )?chao(?: ban)?|hello(?: there)?|hi(?: there)?|hey|alo|good morning|good afternoon|good evening)[.!?]*$", RegexOptions.CultureInvariant))
        {
            response = string.Equals(context.Role, RoleNames.Admin, StringComparison.OrdinalIgnoreCase)
                ? "Chào bạn! Mình là trợ lý AI quản trị. Mình có thể hỗ trợ doanh thu, tổng quan kinh doanh, đơn hàng, món bán chạy và một số thao tác quản trị."
                : "Chào bạn! Mình có thể hỗ trợ xem thực đơn hoặc kiểm tra các đơn hàng của bạn.";
            return true;
        }

        if (Regex.IsMatch(normalized, "^(?:ban khoe khong|ban co khoe khong|khoe khong)[.!?]*$", RegexOptions.CultureInvariant))
        {
            response = "Mình vẫn hoạt động tốt và sẵn sàng hỗ trợ bạn. Bạn muốn xem thông tin gì?";
            return true;
        }

        if (Regex.IsMatch(normalized, "^(?:cam on|cam on ban|thanks|thank you)[.!?]*$", RegexOptions.CultureInvariant))
        {
            response = "Không có gì. Khi cần, bạn cứ gửi yêu cầu cho mình nhé.";
            return true;
        }

        if (Regex.IsMatch(normalized, "^(?:tam biet|hen gap lai|bye)[.!?]*$", RegexOptions.CultureInvariant))
        {
            response = "Tạm biệt bạn. Mình luôn sẵn sàng hỗ trợ khi bạn quay lại.";
            return true;
        }

        response = string.Empty;
        return false;
    }

    private static string FoldForSocial(string text)
    {
        var mapped = text.Replace('\u0111', 'd').Replace('\u0110', 'D');
        var decomposed = mapped.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                builder.Append(character);
        }
        return builder.ToString().ToLowerInvariant();
    }

    //dieu phoi va kiem soat quyen thuc thi
    private async Task<AiResponse> ExecuteAsync(AiPilotToolDecision decision, AiUserContext context,
        Guid correlationId, Stopwatch stopwatch, bool admin, CancellationToken token)
    {
        if (decision.ToolName is null || decision.Intent is null)
            return new AiResponse { Success = false, Error = "Semantic pilot returned an invalid decision." };
        var tool = _registry.GetTool(decision.ToolName);
        var readOnly = tool?.RiskLevel == ToolRiskLevel.Read;
        _audit?.Record(correlationId, decision.ToolName, context, tool?.RiskLevel, "SELECTED", "SELECTED",
            readOnly, stopwatch.ElapsedMilliseconds, tool is null ? "TOOL_NOT_FOUND" : "NONE", decision.Period);
        if (tool is null ||
            !await _permission.CanExecuteAsync(tool, context))
        {
            _audit?.Record(correlationId, decision.ToolName, context, tool?.RiskLevel, "DENIED", "DENIED_PERMISSION",
                readOnly, stopwatch.ElapsedMilliseconds, "PERMISSION_DENIED", decision.Period);
            return new AiResponse { Success = false, Error = "AI tool is not authorized." };
        }
        token.ThrowIfCancellationRequested();
        _audit?.Record(correlationId, tool.Name, context, tool.RiskLevel, "AUTHORIZED", "AUTHORIZED",
            readOnly, stopwatch.ElapsedMilliseconds, "NONE", decision.Period);
        var result = await tool.ExecuteAsync(decision.Arguments, context);
        _audit?.Record(correlationId, tool.Name, context, tool.RiskLevel, result.Success ? "EXECUTED" : "FAILED",
            result.Success ? "EXECUTED" : "FAILED", readOnly, stopwatch.ElapsedMilliseconds,
            result.Success ? "NONE" : "TOOL_RESULT_ERROR", decision.Period);
        if (result.Confirmation is not null)
            return new AiResponse { Success = true, Message = result.Data, Confirmation = result.Confirmation };
        if (!result.Success) return new AiResponse { Success = false, Error = result.Error ?? "AI tool failed." };
        bool formatted;
        string message;
        string? error;
        if (admin)
            formatted = AdminAiResponseFormatter.TryFormat(decision.Intent, tool.Name, result.Data, out message, out error);
        else
            formatted = CustomerGuestAiResponseFormatter.TryFormat(decision.Intent, tool.Name, result.Data, out message, out error);
        return formatted
            ? new AiResponse { Success = true, Message = message }
            : new AiResponse { Success = false, Error = error ?? "AI result could not be formatted." };
    }
}
