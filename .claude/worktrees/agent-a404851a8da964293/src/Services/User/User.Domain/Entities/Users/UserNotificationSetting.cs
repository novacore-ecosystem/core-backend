namespace NovaCore.User.Domain.Entities.Users;

/// <summary>
/// Owned 1:1 extension of User holding per-channel and per-category notification toggles.
/// Order/Security default to enabled since those are transactional, not marketing; the rest
/// default to disabled, requiring explicit opt-in.
/// </summary>
public sealed class UserNotificationSetting : BaseEntity, IAuditable, ITenantEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;
    public bool EmailEnabled { get; private set; } = true;
    public bool SmsEnabled { get; private set; }
    public bool PushEnabled { get; private set; } = true;
    public bool SignalREnabled { get; private set; } = true;
    public bool MarketingEnabled { get; private set; }
    public bool OrderEnabled { get; private set; } = true;
    public bool PromotionEnabled { get; private set; }
    public bool SecurityEnabled { get; private set; } = true;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private UserNotificationSetting() { }

    public static UserNotificationSetting Create(
        Guid userId,
        bool emailEnabled = true,
        bool smsEnabled = false,
        bool pushEnabled = true,
        bool signalREnabled = true,
        bool marketingEnabled = false,
        bool orderEnabled = true,
        bool promotionEnabled = false,
        bool securityEnabled = true)
    {
        return new UserNotificationSetting
        {
            UserId = userId,
            EmailEnabled = emailEnabled,
            SmsEnabled = smsEnabled,
            PushEnabled = pushEnabled,
            SignalREnabled = signalREnabled,
            MarketingEnabled = marketingEnabled,
            OrderEnabled = orderEnabled,
            PromotionEnabled = promotionEnabled,
            SecurityEnabled = securityEnabled,
        };
    }

    internal void UpdateDetails(
        bool emailEnabled,
        bool smsEnabled,
        bool pushEnabled,
        bool signalREnabled,
        bool marketingEnabled,
        bool orderEnabled,
        bool promotionEnabled,
        bool securityEnabled)
    {
        EmailEnabled = emailEnabled;
        SmsEnabled = smsEnabled;
        PushEnabled = pushEnabled;
        SignalREnabled = signalREnabled;
        MarketingEnabled = marketingEnabled;
        OrderEnabled = orderEnabled;
        PromotionEnabled = promotionEnabled;
        SecurityEnabled = securityEnabled;
    }
}
