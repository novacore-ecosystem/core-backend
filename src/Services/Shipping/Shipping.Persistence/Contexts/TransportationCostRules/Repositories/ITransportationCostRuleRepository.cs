using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Shipping.Persistence.Contexts.TransportationCostRules.Repositories;

public interface ITransportationCostRuleRepository : IRepository<TransportationCostRule, Guid>
{
    Task<TransportationCostRule?> GetByCodeAsync(string code, CancellationToken ct = default);
}
