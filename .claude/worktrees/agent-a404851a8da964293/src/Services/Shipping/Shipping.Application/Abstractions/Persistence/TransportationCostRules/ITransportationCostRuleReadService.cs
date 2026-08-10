namespace NovaCore.Shipping.Application.Abstractions.Persistence.TransportationCostRules;

public interface ITransportationCostRuleReadService
{
    Task<TransportationCostRule?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<TransportationCostRule?> GetByCodeAsync(string code, CancellationToken ct = default);
}
