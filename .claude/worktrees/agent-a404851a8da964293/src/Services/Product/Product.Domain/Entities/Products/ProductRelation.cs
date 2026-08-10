namespace NovaCore.Product.Domain.Entities.Products;

/// <summary>
/// Directed link from one Product to another (related, accessory, alternative, replacement,
/// cross-sell, upsell, frequently-bought-together). Neither side owns the other; this is a
/// standalone linking entity between two independent aggregate roots.
/// </summary>
public sealed class ProductRelation : BaseEntity<long>, ITenantEntity
{
    public Guid SourceProductId { get; private set; }
    public Product SourceProduct { get; private set; } = default!;
    public Guid TargetProductId { get; private set; }
    public Product TargetProduct { get; private set; } = default!;
    public RelationType RelationType { get; private set; }
    public int Priority { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ProductRelation() { }

    public static ProductRelation Create(
        Guid sourceProductId,
        Guid targetProductId,
        RelationType relationType,
        int priority = 0)
    {
        if (sourceProductId == targetProductId)
            throw ExceptionFactory.InvalidState("A product cannot be related to itself.");

        return new ProductRelation
        {
            SourceProductId = sourceProductId,
            TargetProductId = targetProductId,
            RelationType = relationType,
            Priority = priority,
        };
    }

    public void ChangePriority(int priority)
    {
        Priority = priority;
    }

    public void ChangeType(RelationType relationType)
    {
        RelationType = relationType;
    }
}
