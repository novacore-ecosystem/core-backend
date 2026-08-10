using NovaCore.BuildingBlock.Domain.Metadata;
using NovaCore.User.Domain.Metadata;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.User.Persistence.Configs;

public sealed class UserConfig : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        // Table
        builder.ToTable("users");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Username)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.DisplayName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(UserStatus.PendingVerification);

        builder.Property(x => x.UserType)
            .IsRequired()
            .HasConversion<short>();

        builder.Property(x => x.LastSeenAt);

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x.ToJson(),
                x => MetadataBase.FromJson<UserMetadata>(x))
            .HasColumnType("jsonb")
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Avatar)
            .WithOne(a => a.User)
            .HasForeignKey<UserAvatar>(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<UserProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Setting)
            .WithOne(s => s.User)
            .HasForeignKey<UserSetting>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SecuritySetting)
            .WithOne(s => s.User)
            .HasForeignKey<UserSecuritySetting>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.PrivacySetting)
            .WithOne(s => s.User)
            .HasForeignKey<UserPrivacySetting>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.NotificationSetting)
            .WithOne(s => s.User)
            .HasForeignKey<UserNotificationSetting>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Preference)
            .WithOne(p => p.User)
            .HasForeignKey<UserPreference>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ActivitySummary)
            .WithOne(a => a.User)
            .HasForeignKey<UserActivitySummary>(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.PermissionSnapshot)
            .WithOne(p => p.User)
            .HasForeignKey<UserPermissionSnapshot>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Addresses)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Contacts)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.PaymentMethods)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Verifications)
            .WithOne(v => v.User)
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.RoleAssignments)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.TagMappings)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.Username)
            .IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.UserType);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
