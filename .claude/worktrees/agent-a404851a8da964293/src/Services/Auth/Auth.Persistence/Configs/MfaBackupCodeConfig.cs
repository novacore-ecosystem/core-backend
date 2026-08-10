using NovaCore.Auth.Domain.Entities.Accounts;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class MfaBackupCodeConfig : IEntityTypeConfiguration<MfaBackupCode>
{
    public void Configure(EntityTypeBuilder<MfaBackupCode> builder)
    {
        // Table
        builder.ToTable("mfa_backup_codes");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.MfaMethodId)
            .IsRequired();

        builder.Property(x => x.CodeHash)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.IsUsed)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(x => x.MfaMethod)
            .WithMany(m => m.BackupCodes)
            .HasForeignKey(x => x.MfaMethodId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.MfaMethodId);

        // Audit & Concurrency
        // Unlike a pure append-only log, two concurrent login attempts could race to consume the
        // same code - keep the concurrency token so that race is caught, not silently double-spent.
        builder.ConfigureCommonFields();
    }
}
