using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class SessionConfig : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        // Table
        builder.ToTable("sessions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AccountId)
            .IsRequired();

        builder.Property(x => x.IpAddress)
            .HasConversion(x => x.Value, x => IpAddress.Create(x))
            .HasMaxLength(45)
            .IsRequired();

        builder.Property(x => x.UserAgent)
            .HasMaxLength(500);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(SessionStatus.Active);

        builder.Property(x => x.StartedAt)
            .IsRequired();

        builder.Property(x => x.LastActivityAt)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.RevokedReason)
            .HasConversion<short?>();

        // Relationships
        builder.HasOne(x => x.Account)
            .WithMany(a => a.Sessions)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Device)
            .WithMany()
            .HasForeignKey(x => x.DeviceId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => x.DeviceId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ExpiresAt);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
