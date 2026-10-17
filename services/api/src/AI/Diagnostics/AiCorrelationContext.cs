namespace RestaurantPOS.AI.Diagnostics;

/// <summary>Request-scoped correlation identifier shared by the AI controller and orchestrator.</summary>
public sealed class AiCorrelationContext
{
    public Guid CorrelationId { get; private set; }

    public Guid Begin()
    {
        CorrelationId = Guid.NewGuid();
        return CorrelationId;
    }
}
