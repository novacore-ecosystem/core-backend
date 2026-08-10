using NovaCore.BuildingBlock.Persistence.Ef.DbContext;
using NovaCore.BuildingBlock.Persistence.Ef.Inbox;
using NovaCore.BuildingBlock.Persistence.Ef.Outbox;

namespace NovaCore.User.Persistence.Engine;

public sealed class UserDbContext(DbContextOptions<UserDbContext> options)
    : DbContextBase(options),
    IOutboxDbContext,
    IInboxDbContext
{
    public DbSet<UserEntity> Users { get; set; } = null!;
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;
    public DbSet<UserAvatar> UserAvatars { get; set; } = null!;
    public DbSet<UserAddress> UserAddresses { get; set; } = null!;
    public DbSet<UserContact> UserContacts { get; set; } = null!;
    public DbSet<UserSetting> UserSettings { get; set; } = null!;
    public DbSet<UserSecuritySetting> UserSecuritySettings { get; set; } = null!;
    public DbSet<UserPrivacySetting> UserPrivacySettings { get; set; } = null!;
    public DbSet<UserNotificationSetting> UserNotificationSettings { get; set; } = null!;
    public DbSet<UserPreference> UserPreferences { get; set; } = null!;
    public DbSet<UserActivitySummary> UserActivitySummaries { get; set; } = null!;
    public DbSet<UserPaymentMethod> UserPaymentMethods { get; set; } = null!;
    public DbSet<UserVerification> UserVerifications { get; set; } = null!;
    public DbSet<UserRoleAssignment> UserRoleAssignments { get; set; } = null!;
    public DbSet<UserPermissionSnapshot> UserPermissionSnapshots { get; set; } = null!;
    public DbSet<UserTagMapping> UserTagMappings { get; set; } = null!;

    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<UserRoleTranslation> UserRoleTranslations { get; set; } = null!;

    public DbSet<UserTag> UserTags { get; set; } = null!;
    public DbSet<UserTagTranslation> UserTagTranslations { get; set; } = null!;

    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;
    public DbSet<InboxRetryHistory> InboxRetryHistories { get; set; } = null!;
}
