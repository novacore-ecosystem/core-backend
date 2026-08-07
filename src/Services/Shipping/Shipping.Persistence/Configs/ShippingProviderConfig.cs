using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Shipping.Persistence.Configs;

public sealed class ShippingProviderConfig : IEntityTypeConfiguration<ShippingProvider>
{
    public void Configure(EntityTypeBuilder<ShippingProvider> builder)
    {
        // Table
        builder.ToTable("shipping_providers");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ProviderType).HasConversion<short>().IsRequired();
        builder.Property(x => x.IsActive).IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        // Profile is a shared-PK 1:1 child - configured from its own side, see
        // ShippingProviderProfileConfig.

        // Indexes
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => new { x.ProviderType, x.IsActive });
    }
}
