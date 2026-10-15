using System.Threading;
using System.Threading.Tasks;

namespace RestaurantPOS.AI.Services;

/// <summary>
/// Local AI transport used by the orchestrator. The current implementation is
/// deterministic Mock AI; the interface keeps the request boundary injectable
/// for contract and security tests without selecting an external provider.
/// </summary>
public interface IAiProvider
{
    string Name { get; }

    bool IsConfigured { get; }

    Task<string> GenerateContentAsync(object requestBody, CancellationToken cancellationToken = default);
}
