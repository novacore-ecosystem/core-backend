using NovaCore.Shipping.Persistence.Engine;

namespace NovaCore.Shipping.Persistence.Contexts.TransportationCostRules.Repositories;

public sealed class TransportationCostRuleRepo(ShippingDbContext dbContext)
    : ShippingBaseRepository<TransportationCostRule, Guid>(dbContext), ITransportationCostRuleRepository
{
    public async Task<TransportationCostRule?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await GetAsync(x => x.Code, code.Trim().ToUpperInvariant(), ct);
}
