using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.User.Persistence.Configs;

public sealed class UserSecuritySettingConfig : IEntityTypeConfiguration<UserSecuritySetting>
{
    public void Configure(EntityTypeBuilder<UserSecuritySetting> builder)
    {
        // Table
        builder.ToTable("user_security_settings");

        // Properties
        // Shared primary key (1:1 with User) - no surrogate Id, exactly one settings row per user.
        builder.HasKey(x => x.UserId);

        builder.Property(x => x.TwoFactorEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.RequirePasswordRotation)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.AllowRememberDevice)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.TrustedDevicesOnly)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.RecoveryEmail)
            .HasMaxLength(256);

        builder.Property(x => x.RecoveryPhone)
            .HasMaxLength(20);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
