using NovaCore.Product.Domain.Entities.Collections;

namespace NovaCore.Product.Domain.Entities.Products;

/// <summary>
/// Explicit many-to-many join entity between Product and ProductCollection - Product and
/// ProductCollection are independent aggregate roots, so this row (not a raw id collection) is
/// how Product references a collection without holding an object reference to another root.
/// Owned by Product (no independent identity, no repository of its own).
/// </summary>
public sealed class ProductCollectionMapping : BaseEntity, ITenantEntity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = default!;
    public Guid CollectionId { get; private set; }
    public ProductCollection Collection { get; private set; } = default!;

    public int DisplayOrder { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ProductCollectionMapping() { }

    internal static ProductCollectionMapping Create(Guid productId, Guid collectionId, int displayOrder = 0)
    {
        return new ProductCollectionMapping
        {
            ProductId = productId,
            CollectionId = collectionId,
            DisplayOrder = displayOrder,
        };
    }

    public void UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }
}
