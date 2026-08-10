using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Product.Domain.Entities.Categories;

public sealed class ProductCategoryTranslation : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public ProductCategory Category { get; private set; } = default!;
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ProductCategoryTranslation() { }

    internal static ProductCategoryTranslation Create(
        Guid id,
        LanguageCode languageCode,
        string name,
        string description)
    {
        ValidateName(name);

        return new ProductCategoryTranslation
        {
            Id = id,
            LanguageCode = languageCode,
            Name = name,
            Description = description,
        };
    }

    internal void UpdateDetails(
        string name,
        string description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Category name cannot be empty.");
    }
}
