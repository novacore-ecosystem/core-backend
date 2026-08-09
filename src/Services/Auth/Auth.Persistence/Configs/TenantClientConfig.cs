using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NovaCore.Auth.Domain.Entities.TenantClients;
using NovaCore.Auth.Domain.Enums;
using NovaCore.Auth.Domain.ValueObjects;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class TenantClientConfig : IEntityTypeConfiguration<TenantClient>
{
    public void Configure(EntityTypeBuilder<TenantClient> builder)
    {
        // Table
        builder.ToTable("tenant_clients");

        // Properties
        builder.HasKey(x => x.Id);

        // TenantClient deliberately does not implement ITenantEntity (see class doc comment) -
        // TenantId is mapped/indexed by hand here instead of by the Entity Convention, and there
        // is no query filter, since PublicKey lookup must work before any tenant context exists.
        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.PublicKey)
            .HasConversion(
                x => x.Value,
                x => ClientPublicKey.Create(x))
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(TenantClientStatus.Active);

        builder.Property(x => x.RevokedReason)
            .HasConversion<short?>();

        // Relationships
        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        // Primary future lookup is PublicKey -> TenantClient -> TenantId (pre-authentication), so
        // this unique index is the one that matters most; TenantId and (TenantId, Status) support
        // the admin-facing "list this tenant's clients"/"list its active clients" access patterns.
        builder.HasIndex(x => x.PublicKey)
            .IsUnique();
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.TenantId, x.Status });

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
