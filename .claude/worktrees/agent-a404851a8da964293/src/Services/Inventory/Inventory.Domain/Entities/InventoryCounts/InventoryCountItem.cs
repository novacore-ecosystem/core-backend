using NovaCore.Inventory.Domain.Entities.Inventories;

namespace NovaCore.Inventory.Domain.Entities.InventoryCounts;

public sealed class InventoryCountItem : BaseEntity<long>, IAuditable, ITenantEntity
{
    public Guid InventoryCountId { get; private set; }
    public InventoryCount InventoryCount { get; private set; } = default!;
    public Guid InventoryId { get; private set; }
    public InventoryStock Inventory { get; private set; } = default!;
    public Guid VariantId { get; private set; }
    public Quantity ExpectedQuantity { get; private set; } = default!;
    public Quantity? ActualQuantity { get; private set; }
    public int DifferenceQuantity { get; private set; }
    public string Note { get; private set; } = string.Empty;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private InventoryCountItem() { }

    /// <summary>Only InventoryCount may construct/mutate its Items - see InventoryCount.AddItem.</summary>
    internal static InventoryCountItem Create(Guid inventoryCountId, Guid inventoryId, Guid productVariantId, int expectedQuantity)
    {
        return new InventoryCountItem
        {
            InventoryCountId = inventoryCountId,
            InventoryId = inventoryId,
            VariantId = productVariantId,
            ExpectedQuantity = Quantity.Create(expectedQuantity),
        };
    }

    /// <summary>Only InventoryCount may mutate its Items - see InventoryCount.RecordCount.</summary>
    internal void RecordActual(int actualQuantity)
    {
        ActualQuantity = Quantity.Create(actualQuantity);
        DifferenceQuantity = actualQuantity - ExpectedQuantity.Value;
    }
}
