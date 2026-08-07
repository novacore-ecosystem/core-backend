namespace NovaCore.Promotion.Domain.Entities.ProductSets;

/// <summary>Per-language override of a ProductSet's Name/Description. Identity is ProductSetId + LanguageCode (Phase 3.1 correction) - no surrogate Id.</summary>
public sealed class ProductSetTranslation : BaseEntity, IAuditable, ITenantEntity
{
    public Guid ProductSetId { get; private set; }
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public ProductSet ProductSet { get; private set; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ProductSetTranslation() { }

    /// <summary>Only ProductSet may construct a ProductSetTranslation - see ProductSet.Translate.</summary>
    internal static ProductSetTranslation Create(Guid productSetId, LanguageCode languageCode, string name, string? description)
    {
        ValidateName(name);

        return new ProductSetTranslation
        {
            ProductSetId = productSetId,
            LanguageCode = languageCode,
            Name = name,
            Description = description,
        };
    }

    internal void UpdateDetails(string name, string? description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Translated product set name cannot be empty.");
    }
}
