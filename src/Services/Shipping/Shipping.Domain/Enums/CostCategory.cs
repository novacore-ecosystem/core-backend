namespace NovaCore.Shipping.Domain.Enums;

/// <summary>Cost bucket for a single TransportationCost line, so a trip's total can be broken down and reconciled per category.</summary>
public enum CostCategory
{
    ShippingFee = 1,
    Fuel = 2,
    Toll = 3,
    Parking = 4,
    Insurance = 5,
    Loading = 6,
    ManualAdjustment = 7,
    Other = 99,
}
