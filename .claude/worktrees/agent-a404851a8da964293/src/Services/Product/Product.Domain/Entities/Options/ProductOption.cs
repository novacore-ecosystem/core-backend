using ProductEntity = NovaCore.Product.Domain.Entities.Products.Product;

namespace NovaCore.Product.Domain.Entities.Options;

/// <summary>
/// Owned child of Product - the usage of a shared <see cref="ProductOptionDefinition"/> (e.g.
/// "Color") within this Product. Defines no data of its own; the option's identity, name, and
/// translations live entirely on the definition. Its <see cref="Values"/> collection selects
/// which of the definition's ValueDefinitions this Product makes available.
/// </summary>
public sealed class ProductOption : BaseEntity<Guid>, ITenantEntity
{
    public Guid ProductId { get; private set; }
    public ProductEntity Product { get; private set; } = default!;
    public Guid ProductOptionDefinitionId { get; private set; }
    public ProductOptionDefinition OptionDefinition { get; private set; } = default!;
    public int DisplayOrder { get; private set; }
    public CatalogStatus Status { get; private set; } = CatalogStatus.Active;

    public ICollection<ProductOptionValue> Values { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ProductOption() { }

    public static ProductOption Create(
        Guid productId,
        Guid productOptionDefinitionId,
        int displayOrder = 0,
        CatalogStatus status = CatalogStatus.Active)
    {
        return new ProductOption
        {
            Id = Guid.CreateVersion7(),
            ProductId = productId,
            ProductOptionDefinitionId = productOptionDefinitionId,
            DisplayOrder = displayOrder,
            Status = status,
        };
    }

    // ============================================================================
    // Values
    // Manages which of the OptionDefinition's ValueDefinitions this Product makes
    // available (e.g. only "Red"/"Blue" out of the full "Color" catalog):
    // selection, removal, and display-order sequencing.
    // ============================================================================

    #region Values

    public ProductOptionValue SelectValue(Guid valueDefinitionId)
    {
        var existing = Values.FirstOrDefault(v => v.ProductOptionValueDefinitionId == valueDefinitionId);
        if (existing is not null)
            return existing;

        var displayOrder = Values.Count == 0 ? 0 : Values.Max(v => v.DisplayOrder) + 1;
        var value = ProductOptionValue.Create(Id, valueDefinitionId, displayOrder);
        Values.Add(value);

        return value;
    }

    public void DeselectValue(Guid valueId)
    {
        var value = Values.FirstOrDefault(v => v.Id == valueId);
        if (value is null)
            return;

        Values.Remove(value);
    }

    public void ReorderValues(IEnumerable<Guid> orderedValueIds)
    {
        var idsInOrder = orderedValueIds.ToArray();

        for (var i = 0; i < idsInOrder.Length; i++)
        {
            var value = Values.FirstOrDefault(v => v.Id == idsInOrder[i])
                ?? throw ExceptionFactory.EntityNotFound<ProductOptionValue>(idsInOrder[i]);
            value.ChangeDisplayOrder(i);
        }
    }

    #endregion

    // ============================================================================
    // Details & lifecycle
    // Display ordering and Active/Inactive status transitions for this
    // Product's usage of the option.
    // ============================================================================

    #region Details & lifecycle

    public void ChangeDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }

    public void Activate()
    {
        Status = CatalogStatus.Active;
    }

    public void Deactivate()
    {
        Status = CatalogStatus.Inactive;
    }

    #endregion
}
