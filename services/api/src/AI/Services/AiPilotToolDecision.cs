using System.Text.Json;

namespace RestaurantPOS.AI.Services;

/// <summary>Shared execution envelope for a read-only semantic pilot.</summary>
public sealed record AiPilotToolDecision(
    string Decision,
    string Reason,
    string? Intent,
    string? ToolName,
    JsonElement Arguments,
    string Period,
    double TopScore,
    double Margin);
