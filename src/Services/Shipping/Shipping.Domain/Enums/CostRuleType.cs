namespace NovaCore.Shipping.Domain.Enums;

/// <summary>How a TransportationCostRule computes its amount.</summary>
public enum CostRuleType
{
    PerKilometer = 1,
    PerTrip = 2,
    Fixed = 3,
    Manual = 4,
}
