namespace NovaCore.Shipping.Application.Abstractions.Persistence.TransportationCostRules;

public interface ITransportationCostRuleWriteService
{

    Task<TransportationCostRule> CreateAsync(
        string code,
        string name,
        CostRuleType ruleType,
        Money baseAmount,
        Money unitAmount,
        DateTime effectiveFrom,
        Guid? providerId = null,
        Money? minAmount = null,
        Money? maxAmount = null,
        DateTime? effectiveTo = null,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
