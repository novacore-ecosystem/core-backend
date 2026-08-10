using NovaCore.Shipping.Application.Abstractions.Persistence.TransportationCostRules;
using NovaCore.Shipping.Persistence.Contexts.TransportationCostRules.Repositories;

namespace NovaCore.Shipping.Persistence.Contexts.TransportationCostRules.Read;

public sealed class TransportationCostRuleReadService(ITransportationCostRuleRepository repo) : ITransportationCostRuleReadService
{
    public async Task<TransportationCostRule?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await repo.GetByIdAsync(id, ct);

    public async Task<TransportationCostRule?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await repo.GetByCodeAsync(code, ct);
}
