using RestaurantPOS.Domain.Entities;

namespace RestaurantPOS.Application.Common.Security;

public sealed record QrTokenBackfillAssignment(Guid TableId, string Token);

public static class QrTokenBackfillPreparation
{
    // Generation is deliberately separate from persistence so the diagnostic can conditionally update only NULL rows.
    public static IReadOnlyList<QrTokenBackfillAssignment> CreateAssignments(IEnumerable<RestaurantTable> tables) =>
        tables
            .Where(table => table.QrToken is null)
            .Select(table => new QrTokenBackfillAssignment(table.Id, QrTokenGenerator.Generate()))
            .ToList();
}
