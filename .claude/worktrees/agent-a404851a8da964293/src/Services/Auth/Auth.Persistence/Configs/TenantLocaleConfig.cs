using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.BuildingBlock.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class TenantLocaleConfig : IEntityTypeConfiguration<TenantLocale>
{
    public void Configure(EntityTypeBuilder<TenantLocale> builder)
    {
        // Table
        builder.ToTable("tenant_locales");

        // Properties
        // Own surrogate Id - unlike Role/Product/ScopeTranslation, a null LanguageCode (the
        // fallback resource) cannot participate in a composite primary key, so TenantLocale
        // keeps a real generated Id plus a separate TenantId foreign key.
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.LanguageCode)
            .HasConversion(
                x => x == null ? null : x.Value,
                x => x == null ? null : LanguageCode.Create(x))
            .HasMaxLength(10);

        builder.Property(x => x.ConfigurationJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.DictionaryJson)
            .HasColumnType("jsonb")
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Tenant)
            .WithMany(t => t.Locales)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.TenantId, x.LanguageCode })
            .IsUnique();

        // Only one fallback (null LanguageCode) resource per tenant - a plain unique index
        // on (TenantId, LanguageCode) does not enforce this, since Postgres treats every NULL
        // as distinct from every other NULL.
        builder.HasIndex(x => x.TenantId)
            .IsUnique()
            .HasFilter("language_code IS NULL")
            .HasDatabaseName("ix_tenant_locales_tenant_id_fallback");

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
