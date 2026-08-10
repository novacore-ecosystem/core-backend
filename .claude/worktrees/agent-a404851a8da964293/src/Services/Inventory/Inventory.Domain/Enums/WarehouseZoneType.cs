namespace NovaCore.Inventory.Domain.Enums;

public enum WarehouseZoneType : short
{
    Receiving = 1,
    Storage = 2,
    Picking = 3,
    Packing = 4,
    Shipping = 5,
    Return = 6,
    Damaged = 7,
    QualityControl = 8,
    ColdStorage = 9,
    Hazardous = 10,
    Transit = 11,
}