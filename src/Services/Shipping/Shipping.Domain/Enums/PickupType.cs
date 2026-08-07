namespace NovaCore.Shipping.Domain.Enums;

/// <summary>Where goods are collected from at the start of a Shipment.</summary>
public enum PickupType
{
    Warehouse = 1,
    Merchant = 2,
    Customer = 3,
}
