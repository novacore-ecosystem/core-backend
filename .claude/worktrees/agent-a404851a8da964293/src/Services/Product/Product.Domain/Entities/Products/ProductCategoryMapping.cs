using NovaCore.Product.Domain.Entities.Categories;

namespace NovaCore.Product.Domain.Entities.Products;

public sealed class ProductCategoryMapping : BaseEntity, ITenantEntity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = default!;
    public Guid CategoryId { get; private set; }
    public ProductCategory Category { get; private set; } = default!;
    public int DisplayOrder { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ProductCategoryMapping() { }

    internal static ProductCategoryMapping Create(Guid productId, Guid categoryId, int displayOrder = 0)
    {
        return new ProductCategoryMapping
        {
            ProductId = productId,
            CategoryId = categoryId,
            DisplayOrder = displayOrder,
        };
    }

    public void UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }
}
