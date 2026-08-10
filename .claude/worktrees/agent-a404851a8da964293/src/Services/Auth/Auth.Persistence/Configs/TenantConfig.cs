using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.Auth.Domain.Metadata;
using NovaCore.Auth.Domain.ValueObjects;
using NovaCore.BuildingBlock.Domain.Metadata;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class TenantConfig : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        // Table
        builder.ToTable("tenants");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasConversion(
                x => x.Value,
                x => TenantCode.Create(x))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.LogoUrl)
            .HasMaxLength(500);

        builder.Property(x => x.FaviconUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Version)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x.ToJson(),
                x => MetadataBase.FromJson<TenantMetadata>(x))
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Relationships
        builder.HasMany(x => x.Locales)
            .WithOne(l => l.Tenant)
            .HasForeignKey(l => l.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.Code)
            .IsUnique();
        builder.HasIndex(x => x.IsActive);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
