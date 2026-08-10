namespace NovaCore.Product.Domain.Entities.Options;

/// <summary>
/// Owned child of ProductOptionDefinition - a reusable selectable value (e.g. "Red" for the
/// "Color" dimension) shared across every Product that opts into the option via
/// <see cref="ProductOption"/>.
/// </summary>
public sealed class ProductOptionValueDefinition : BaseEntity<Guid>, IAuditable
{
    public Guid ProductOptionDefinitionId { get; private set; }
    public ProductOptionDefinition ProductOptionDefinition { get; private set; } = default!;
    public string Code { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public CatalogStatus Status { get; private set; } = CatalogStatus.Active;
    public ProductOptionValueDefinitionMetadata Metadata { get; private set; } = new();

    public ICollection<ProductOptionValueDefinitionTranslation> Translations { get; private set; } = [];

    private ProductOptionValueDefinition() { }

    internal static ProductOptionValueDefinition Create(
        Guid id,
        Guid productOptionDefinitionId,
        string code,
        int displayOrder,
        CatalogStatus status = CatalogStatus.Active,
        ProductOptionValueDefinitionMetadata? metadata = null)
    {
        ValidateCode(code);

        return new ProductOptionValueDefinition
        {
            Id = id,
            ProductOptionDefinitionId = productOptionDefinitionId,
            Code = code,
            DisplayOrder = displayOrder,
            Status = status,
            Metadata = metadata ?? new ProductOptionValueDefinitionMetadata(),
        };
    }

    internal void ChangeDisplayOrder(int displayOrder)
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

    public static bool IsValidCode(string? code) => !string.IsNullOrWhiteSpace(code);

    private static void ValidateCode(string code)
    {
        if (!IsValidCode(code))
            throw ExceptionFactory.RequiredField("Option value definition code cannot be empty.");
    }
}
