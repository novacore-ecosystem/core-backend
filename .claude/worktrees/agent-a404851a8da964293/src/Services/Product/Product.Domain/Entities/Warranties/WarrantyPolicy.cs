namespace NovaCore.Product.Domain.Entities.Warranties;

/// <summary>
/// Independent, reusable catalog lookup.
/// Variants reference a policy via Variant.WarrantyPolicyId instead of duplicating warranty terms.
/// </summary>
public sealed class WarrantyPolicy : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public WarrantyType WarrantyType { get; private set; }
    public Duration Duration { get; private set; } = null!;
    public CatalogStatus Status { get; private set; } = CatalogStatus.Active;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private WarrantyPolicy() { }

    public static WarrantyPolicy Create(
        Guid id,
        string name,
        string description,
        WarrantyType warrantyType,
        Duration duration,
        CatalogStatus status = CatalogStatus.Active)
    {
        ValidateName(name);

        return new WarrantyPolicy
        {
            Id = id,
            Name = name,
            Description = description,
            WarrantyType = warrantyType,
            Duration = duration,
            Status = status,
        };
    }

    public void UpdateDetails(string name, string description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    public void ChangeDuration(Duration duration)
    {
        Duration = duration;
    }

    public void Activate()
    {
        Status = CatalogStatus.Active;
    }

    public void Deactivate()
    {
        Status = CatalogStatus.Inactive;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Warranty policy name cannot be empty.");
    }
}
