using NovaCore.Shipping.Application.Abstractions.Persistence.TransportationCostRules;
using NovaCore.Shipping.Persistence.Contexts.TransportationCostRules.Repositories;

namespace NovaCore.Shipping.Persistence.Contexts.TransportationCostRules.Write;

public sealed class TransportationCostRuleWriteService(ITransportationCostRuleRepository repo) : ITransportationCostRuleWriteService
{
    public async Task<TransportationCostRule> CreateAsync(
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
        CancellationToken ct = default)
    {
        var rule = TransportationCostRule.Create(
            code, name, ruleType, baseAmount, unitAmount, effectiveFrom, providerId, minAmount, maxAmount, effectiveTo);

        await repo.AddAsync(rule, ct);

        return rule;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        => await repo.DeleteByIdAsync(id, ct);
}
