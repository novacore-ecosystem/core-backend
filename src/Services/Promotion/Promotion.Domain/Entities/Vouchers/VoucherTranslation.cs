namespace NovaCore.Promotion.Domain.Entities.Vouchers;

/// <summary>Per-language override of a Voucher's Name/Description. Identity is VoucherId + LanguageCode (Phase 3.1 correction) - no surrogate Id.</summary>
public sealed class VoucherTranslation : BaseEntity, IAuditable, ITenantEntity
{
    public Guid VoucherId { get; private set; }
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public Voucher Voucher { get; private set; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private VoucherTranslation() { }

    /// <summary>Only Voucher may construct a VoucherTranslation - see Voucher.Translate.</summary>
    internal static VoucherTranslation Create(Guid voucherId, LanguageCode languageCode, string name, string? description)
    {
        ValidateName(name);

        return new VoucherTranslation
        {
            VoucherId = voucherId,
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
            throw ExceptionFactory.RequiredField("Translated voucher name cannot be empty.");
    }
}
