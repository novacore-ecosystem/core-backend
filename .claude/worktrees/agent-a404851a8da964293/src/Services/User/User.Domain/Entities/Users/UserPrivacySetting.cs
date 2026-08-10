namespace NovaCore.User.Domain.Entities.Users;

/// <summary>
/// Owned 1:1 extension of User holding profile-visibility and data-usage consent flags. All
/// default to disabled - visibility and data usage require explicit opt-in, not opt-out.
/// </summary>
public sealed class UserPrivacySetting : BaseEntity, IAuditable, ITenantEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;
    public bool ShowBirthday { get; private set; }
    public bool ShowEmail { get; private set; }
    public bool ShowPhoneNumber { get; private set; }
    public bool AllowTracking { get; private set; }
    public bool AllowRecommendation { get; private set; }
    public bool AllowPersonalizedAds { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private UserPrivacySetting() { }

    public static UserPrivacySetting Create(
        Guid userId,
        bool showBirthday = false,
        bool showEmail = false,
        bool showPhoneNumber = false,
        bool allowTracking = false,
        bool allowRecommendation = false,
        bool allowPersonalizedAds = false)
    {
        return new UserPrivacySetting
        {
            UserId = userId,
            ShowBirthday = showBirthday,
            ShowEmail = showEmail,
            ShowPhoneNumber = showPhoneNumber,
            AllowTracking = allowTracking,
            AllowRecommendation = allowRecommendation,
            AllowPersonalizedAds = allowPersonalizedAds,
        };
    }

    internal void UpdateDetails(
        bool showBirthday,
        bool showEmail,
        bool showPhoneNumber,
        bool allowTracking,
        bool allowRecommendation,
        bool allowPersonalizedAds)
    {
        ShowBirthday = showBirthday;
        ShowEmail = showEmail;
        ShowPhoneNumber = showPhoneNumber;
        AllowTracking = allowTracking;
        AllowRecommendation = allowRecommendation;
        AllowPersonalizedAds = allowPersonalizedAds;
    }
}
