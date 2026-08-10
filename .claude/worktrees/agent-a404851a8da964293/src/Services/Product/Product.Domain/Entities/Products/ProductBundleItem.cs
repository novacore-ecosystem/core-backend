using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Product.Domain.Entities.Products;

/// <summary>
/// Owned child of Product - one child Variant and its quantity that make up a Bundle-type Variant.
/// </summary>
public sealed class ProductBundleItem : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public ProductVariant BundleVariant { get; private set; } = default!;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = default!;
    public Guid VariantId { get; private set; }
    public ProductVariant Variant { get; private set; } = default!;
    public Quantity Quantity { get; private set; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ProductBundleItem() { }

    internal static ProductBundleItem Create(
        Guid bundleProductId,
        Guid productId,
        Guid variantId,
        Quantity quantity)
    {
        return new ProductBundleItem
        {
            Id = bundleProductId,
            ProductId = productId,
            VariantId = variantId,
            Quantity = quantity,
        };
    }
}
