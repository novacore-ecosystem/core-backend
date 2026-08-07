namespace NovaCore.Promotion.Domain.Entities.ProductSets;

/// <summary>A member product/variant of a ProductSet - Quantity reuses the shared BuildingBlock Value Object, same as Order.Domain's OrderItem.</summary>
public sealed class ProductSetItem : BaseEntity<Guid>, IAuditable
{
    public Guid ProductSetId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? VariantId { get; private set; }
    public Quantity Quantity { get; private set; } = default!;

    public ProductSet ProductSet { get; private set; } = default!;

    private ProductSetItem() { }

    /// <summary>Only ProductSet may construct a ProductSetItem - see ProductSet.AddItem.</summary>
    internal static ProductSetItem Create(Guid productSetId, Guid productId, Quantity quantity, Guid? variantId)
    {
        return new ProductSetItem
        {
            Id = Guid.CreateVersion7(),
            ProductSetId = productSetId,
            ProductId = productId,
            VariantId = variantId,
            Quantity = quantity,
        };
    }

    public void UpdateQuantity(Quantity quantity)
    {
        Quantity = quantity;
    }
}
