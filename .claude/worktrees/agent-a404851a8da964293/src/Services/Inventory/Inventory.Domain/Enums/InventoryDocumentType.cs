namespace NovaCore.Inventory.Domain.Enums;

public enum InventoryDocumentType : short
{
    Receipt = 1,
    Issue = 2,
    Transfer = 3,
    Adjustment = 4,
    Return = 5,
    CycleCount = 6,
    Production = 7,
    Consumption = 8,
}
