using NovaCore.Auth.Domain.Entities.Accounts;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class ExternalIdentityConfig : IEntityTypeConfiguration<ExternalIdentity>
{
    public void Configure(EntityTypeBuilder<ExternalIdentity> builder)
    {
        // Table
        builder.ToTable("external_identities");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AccountId)
            .IsRequired();

        builder.Property(x => x.Provider)
            .IsRequired()
            .HasConversion<short>();

        builder.Property(x => x.ProviderUserId)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.LinkedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Account)
            .WithMany(a => a.ExternalIdentities)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        // A given provider account can only ever be linked to one Account.
        builder.HasIndex(x => new { x.Provider, x.ProviderUserId })
            .IsUnique();
        // One link per provider per Account.
        builder.HasIndex(x => new { x.AccountId, x.Provider })
            .IsUnique();

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
