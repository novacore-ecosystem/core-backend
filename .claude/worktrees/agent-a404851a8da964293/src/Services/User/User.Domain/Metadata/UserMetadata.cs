using NovaCore.BuildingBlock.Domain.Metadata;

namespace NovaCore.User.Domain.Metadata;

/// <summary>Extensible, strongly-typed metadata bag embedded directly on User (not a separate
/// table) - a JSONB-backed dictionary for integration/analytics data that doesn't warrant its
/// own column, following the same MetadataBase pattern as Product.Domain's ProductMetadata.</summary>
public sealed class UserMetadata : MetadataBase
{
    [Metadata("referral_source")]
    public string? ReferralSource
    {
        get => Get<string>();
        set => Set(value);
    }

    [Metadata("external_crm_id")]
    public string? ExternalCrmId
    {
        get => Get<string>();
        set => Set(value);
    }

    [Metadata("onboarding_completed", DefaultValue = false)]
    public bool OnboardingCompleted
    {
        get => Get<bool>();
        set => Set(value);
    }
}
