using NovaCore.Product.Domain.Entities.Specifications;

namespace NovaCore.Product.Domain.Entities.Products;

/// <summary>
/// Owned child of Product - the value a product supplies for one system-managed
/// <see cref="SpecificationDefinition"/> (e.g. Definition "Screen Size" + Value "6.9\""). The
/// field's name, data type, and unit are owned entirely by the definition; sellers only provide
/// the value here, never a custom name.
/// </summary>
public sealed class ProductSpecification : BaseEntity<Guid>, ITenantEntity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = default!;
    public Guid SpecificationDefinitionId { get; private set; }
    public SpecificationDefinition SpecificationDefinition { get; private set; } = default!;
    public string Value { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ProductSpecification() { }

    internal static ProductSpecification Create(
        Guid productId,
        Guid specificationDefinitionId,
        string value,
        int displayOrder)
    {
        ValidateValue(value);

        return new ProductSpecification
        {
            Id = Guid.CreateVersion7(),
            ProductId = productId,
            SpecificationDefinitionId = specificationDefinitionId,
            Value = value,
            DisplayOrder = displayOrder,
        };
    }

    public void UpdateValue(string value)
    {
        ValidateValue(value);

        Value = value;
    }

    public void UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }

    public static bool IsValidValue(string? value) => !string.IsNullOrWhiteSpace(value);

    private static void ValidateValue(string value)
    {
        if (!IsValidValue(value))
            throw ExceptionFactory.RequiredField("Specification value cannot be empty.");
    }
}
