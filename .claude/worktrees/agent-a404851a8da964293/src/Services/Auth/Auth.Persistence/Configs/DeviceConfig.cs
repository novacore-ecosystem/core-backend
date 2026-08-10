using NovaCore.Auth.Domain.Entities.Accounts;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class DeviceConfig : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        // Table
        builder.ToTable("devices");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AccountId)
            .IsRequired();

        builder.Property(x => x.Fingerprint)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.DeviceName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.DeviceType)
            .IsRequired()
            .HasConversion<short>();

        builder.Property(x => x.Platform)
            .HasMaxLength(100);

        builder.Property(x => x.IsTrusted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.LastSeenAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Account)
            .WithMany(a => a.Devices)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.AccountId, x.Fingerprint })
            .IsUnique();

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
