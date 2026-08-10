namespace NovaCore.Product.Domain.Entities.Products;

/// <summary>
/// Owned child of Variant, referenced back via Variant.ProductShippingId - describes the physical
/// shipping characteristics of a variant. Never calculates shipping fees; that is a Shipping
/// service concern.
/// </summary>
public sealed class ProductShipping : BaseEntity<Guid>, ITenantEntity
{
    public ShippingClass ShippingClass { get; private set; }
    public Weight? Weight { get; private set; }
    public Dimension? Dimension { get; private set; }
    public bool RequiresShipping { get; private set; } = true;
    public bool IsFragile { get; private set; }
    public bool IsHazardous { get; private set; }
    public bool AllowBackOrder { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ProductShipping() { }

    public static ProductShipping Create(
        ShippingClass shippingClass,
        Weight? weight = null,
        Dimension? dimension = null,
        bool requiresShipping = true,
        bool isFragile = false,
        bool isHazardous = false,
        bool allowBackOrder = false)
    {
        return new ProductShipping
        {
            Id = Guid.CreateVersion7(),
            ShippingClass = shippingClass,
            Weight = weight,
            Dimension = dimension,
            RequiresShipping = requiresShipping,
            IsFragile = isFragile,
            IsHazardous = isHazardous,
            AllowBackOrder = allowBackOrder,
        };
    }

    public void UpdatePhysicalAttributes(Weight? weight, Dimension? dimension)
    {
        Weight = weight;
        Dimension = dimension;
    }

    public void ChangeShippingClass(ShippingClass shippingClass)
    {
        ShippingClass = shippingClass;
    }

    public void MarkAsFragile()
    {
        IsFragile = true;
    }

    public void UnmarkAsFragile()
    {
        IsFragile = false;
    }

    public void MarkAsHazardous()
    {
        IsHazardous = true;
    }

    public void UnmarkAsHazardous()
    {
        IsHazardous = false;
    }

    public void AllowBackOrders()
    {
        AllowBackOrder = true;
    }

    public void DisallowBackOrders()
    {
        AllowBackOrder = false;
    }

    public void RequireShipping()
    {
        RequiresShipping = true;
    }

    public void ExemptFromShipping()
    {
        RequiresShipping = false;
    }
}
