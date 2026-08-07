namespace NovaCore.Promotion.Domain.Entities.Gifts;

/// <summary>Per-warehouse available stock for a GiftItem - Quantity reuses the shared BuildingBlock Value Object. Not navigated from GiftProgram, so construction is public. No stock deduction logic lives here.</summary>
public sealed class GiftInventory : BaseEntity<Guid>, IAuditable
{
    public Guid GiftItemId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Quantity AvailableQuantity { get; private set; } = default!;

    public GiftItem GiftItem { get; private set; } = default!;

    private GiftInventory() { }

    public static GiftInventory Create(Guid giftItemId, Guid warehouseId, Quantity availableQuantity)
    {
        return new GiftInventory
        {
            Id = Guid.CreateVersion7(),
            GiftItemId = giftItemId,
            WarehouseId = warehouseId,
            AvailableQuantity = availableQuantity,
        };
    }

    public void UpdateAvailableQuantity(Quantity availableQuantity)
    {
        AvailableQuantity = availableQuantity;
    }
}
