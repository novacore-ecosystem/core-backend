using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.User.Persistence.Configs;

public sealed class UserPrivacySettingConfig : IEntityTypeConfiguration<UserPrivacySetting>
{
    public void Configure(EntityTypeBuilder<UserPrivacySetting> builder)
    {
        // Table
        builder.ToTable("user_privacy_settings");

        // Properties
        // Shared primary key (1:1 with User) - no surrogate Id, exactly one settings row per user.
        builder.HasKey(x => x.UserId);

        builder.Property(x => x.ShowBirthday)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.ShowEmail)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.ShowPhoneNumber)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.AllowTracking)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.AllowRecommendation)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.AllowPersonalizedAds)
            .IsRequired()
            .HasDefaultValue(false);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
