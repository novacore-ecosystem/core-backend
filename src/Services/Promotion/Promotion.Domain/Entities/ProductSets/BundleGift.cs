namespace NovaCore.Promotion.Domain.Entities.ProductSets;

/// <summary>A free-gift product attached to a ProductBundle - Quantity reuses the shared BuildingBlock Value Object. Not navigated from ProductBundle, so construction is public.</summary>
public sealed class BundleGift : BaseEntity<Guid>, IAuditable
{
    public Guid BundleId { get; private set; }
    public Guid ProductId { get; private set; }
    public Quantity Quantity { get; private set; } = default!;

    public ProductBundle Bundle { get; private set; } = default!;

    private BundleGift() { }

    public static BundleGift Create(Guid bundleId, Guid productId, Quantity quantity)
    {
        return new BundleGift
        {
            Id = Guid.CreateVersion7(),
            BundleId = bundleId,
            ProductId = productId,
            Quantity = quantity,
        };
    }
}
