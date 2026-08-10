using NovaCore.BuildingBlock.Domain.Metadata;

namespace NovaCore.Product.Domain.Metadata;

/// <summary>Extensible, strongly-typed metadata specific to a single Variant.</summary>
public sealed class VariantMetadata : MetadataBase
{
    [Metadata("gift_wrap_eligible", DefaultValue = false)]
    public bool GiftWrapEligible
    {
        get => Get<bool>();
        set => Set(value);
    }

    [Metadata("warranty_months")]
    public int? WarrantyMonths
    {
        get => Get<int?>();
        set => Set(value);
    }

    [Metadata("care_instructions")]
    public string? CareInstructions
    {
        get => Get<string>();
        set => Set(value);
    }
}
