using NovaCore.Auth.Domain.Entities.Accounts;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class MfaMethodConfig : IEntityTypeConfiguration<MfaMethod>
{
    public void Configure(EntityTypeBuilder<MfaMethod> builder)
    {
        // Table
        builder.ToTable("mfa_methods");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AccountId)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<short>();

        builder.Property(x => x.SecretEncrypted)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.IsVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsPrimary)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(x => x.Account)
            .WithMany(a => a.MfaMethods)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.BackupCodes)
            .WithOne(c => c.MfaMethod)
            .HasForeignKey(c => c.MfaMethodId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => new { x.AccountId, x.IsPrimary });

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
