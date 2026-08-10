using NovaCore.Product.Domain.Entities.Products;

namespace NovaCore.Product.Domain.Entities.Options;

/// <summary>
/// Explicit many-to-many join entity between Variant and ProductOptionValue - the combination of
/// option values that identifies a specific Variant (e.g. "Red" + "Large"). Composite key,
/// no independent identity, owned by Variant.
/// </summary>
public sealed class VariantOptionValue : BaseEntity, ITenantEntity
{
    public Guid VariantId { get; private set; }
    public ProductVariant Variant { get; private set; } = default!;
    public Guid ProductOptionValueId { get; private set; }
    public ProductOptionValue ProductOptionValue { get; private set; } = default!;
    public int DisplayOrder { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private VariantOptionValue() { }

    internal static VariantOptionValue Create(Guid variantId, Guid productOptionValueId, int displayOrder)
    {
        return new VariantOptionValue
        {
            VariantId = variantId,
            ProductOptionValueId = productOptionValueId,
            DisplayOrder = displayOrder,
        };
    }
}
