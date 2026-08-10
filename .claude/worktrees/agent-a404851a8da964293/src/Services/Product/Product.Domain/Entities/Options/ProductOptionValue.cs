namespace NovaCore.Product.Domain.Entities.Options;

/// <summary>
/// Owned child of ProductOption - selects one of the OptionDefinition's ValueDefinitions (e.g.
/// "Red") as available for this Product. Never defines its own value data; that lives on
/// <see cref="ProductOptionValueDefinition"/>.
/// </summary>
public sealed class ProductOptionValue : BaseEntity<Guid>, ITenantEntity
{
    public Guid ProductOptionId { get; private set; }
    public ProductOption ProductOption { get; private set; } = default!;
    public Guid ProductOptionValueDefinitionId { get; private set; }
    public ProductOptionValueDefinition ValueDefinition { get; private set; } = default!;
    public int DisplayOrder { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ProductOptionValue() { }

    internal static ProductOptionValue Create(
        Guid productOptionId,
        Guid productOptionValueDefinitionId,
        int displayOrder)
    {
        return new ProductOptionValue
        {
            Id = Guid.CreateVersion7(),
            ProductOptionId = productOptionId,
            ProductOptionValueDefinitionId = productOptionValueDefinitionId,
            DisplayOrder = displayOrder,
        };
    }

    internal void ChangeDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }
}
