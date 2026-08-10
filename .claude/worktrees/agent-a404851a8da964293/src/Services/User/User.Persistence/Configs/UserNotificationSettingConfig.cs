using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.User.Persistence.Configs;

public sealed class UserNotificationSettingConfig : IEntityTypeConfiguration<UserNotificationSetting>
{
    public void Configure(EntityTypeBuilder<UserNotificationSetting> builder)
    {
        // Table
        builder.ToTable("user_notification_settings");

        // Properties
        // Shared primary key (1:1 with User) - no surrogate Id, exactly one settings row per user.
        builder.HasKey(x => x.UserId);

        builder.Property(x => x.EmailEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.SmsEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.PushEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.SignalREnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.MarketingEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.OrderEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.PromotionEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.SecurityEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
