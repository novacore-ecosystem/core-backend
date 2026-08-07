namespace NovaCore.Shipping.Domain.Entities.Transportations;

/// <summary>
/// A reusable pricing rule a Transportation's cost can be derived from. Standalone aggregate
/// root: it outlives any individual trip and is *referenced* by Transportation.CostRuleId, never
/// owned by it. Rule evaluation itself is not implemented in this foundation phase - the rule is
/// stored, and whichever later phase computes cost reads it.
/// </summary>
public sealed class TransportationCostRule : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public CostRuleType RuleType { get; private set; }

    /// <summary>Null means the rule applies to every provider.</summary>
    public Guid? ProviderId { get; private set; }
    public Money BaseAmount { get; private set; } = default!;

    /// <summary>Amount per unit (per km for PerKilometer, per trip for PerTrip) - zero for Fixed/Manual rules.</summary>
    public Money UnitAmount { get; private set; } = default!;
    public Money? MinAmount { get; private set; }
    public Money? MaxAmount { get; private set; }
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }
    public bool IsActive { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private TransportationCostRule() { }

    public static TransportationCostRule Create(
        string code,
        string name,
        CostRuleType ruleType,
        Money baseAmount,
        Money unitAmount,
        DateTime effectiveFrom,
        Guid? providerId = null,
        Money? minAmount = null,
        Money? maxAmount = null,
        DateTime? effectiveTo = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw ExceptionFactory.RequiredField("Cost rule code cannot be empty.");

        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Cost rule name cannot be empty.");

        if (effectiveTo is not null && effectiveTo <= effectiveFrom)
            throw ExceptionFactory.InvalidRange("Cost rule EffectiveTo must be after EffectiveFrom.");

        if (minAmount is not null && maxAmount is not null && maxAmount.Value < minAmount.Value)
            throw ExceptionFactory.InvalidRange("Cost rule MaxAmount cannot be lower than MinAmount.");

        return new TransportationCostRule
        {
            Id = Guid.CreateVersion7(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            RuleType = ruleType,
            ProviderId = providerId,
            BaseAmount = baseAmount,
            UnitAmount = unitAmount,
            MinAmount = minAmount,
            MaxAmount = maxAmount,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            IsActive = true,
        };
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void Reschedule(DateTime effectiveFrom, DateTime? effectiveTo)
    {
        if (effectiveTo is not null && effectiveTo <= effectiveFrom)
            throw ExceptionFactory.InvalidRange("Cost rule EffectiveTo must be after EffectiveFrom.");

        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
    }
}
