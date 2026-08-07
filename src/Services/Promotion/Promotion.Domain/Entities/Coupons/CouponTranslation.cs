namespace NovaCore.Promotion.Domain.Entities.Coupons;

/// <summary>Per-language override of a Coupon's Name/Description. Identity is CouponId + LanguageCode (Phase 3.1 correction) - no surrogate Id.</summary>
public sealed class CouponTranslation : BaseEntity, IAuditable, ITenantEntity
{
    public Guid CouponId { get; private set; }
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public Coupon Coupon { get; private set; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private CouponTranslation() { }

    /// <summary>Only Coupon may construct a CouponTranslation - see Coupon.Translate.</summary>
    internal static CouponTranslation Create(Guid couponId, LanguageCode languageCode, string name, string? description)
    {
        ValidateName(name);

        return new CouponTranslation
        {
            CouponId = couponId,
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
            throw ExceptionFactory.RequiredField("Translated coupon name cannot be empty.");
    }
}
