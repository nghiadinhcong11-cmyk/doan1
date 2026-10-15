namespace RestaurantPOS.AI.Models;

public static class AiFailureCodes
{
    public const string InvalidResponse = "AI_INVALID_RESPONSE";
    public const string ToolPermissionDenied = "TOOL_PERMISSION_DENIED";
    public const string ToolExecutionFailed = "TOOL_EXECUTION_FAILED";
    public const string ToolResponseSerializationFailed = "TOOL_RESPONSE_SERIALIZATION_FAILED";
    public const string ReadOnlyPolicyBlocked = "READ_ONLY_POLICY_BLOCKED";
    public const string OrchestrationFailed = "ORCHESTRATION_FAILED";
}
