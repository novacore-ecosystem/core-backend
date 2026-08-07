using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Shipping.Persistence.Configs;

public sealed class CarrierIntegrationConfig : IEntityTypeConfiguration<CarrierIntegration>
{
    public void Configure(EntityTypeBuilder<CarrierIntegration> builder)
    {
        // Table
        builder.ToTable("carrier_integrations");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ShippingProviderId).IsRequired();
        builder.Property(x => x.IntegrationCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.BaseUrl).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();
        builder.Property(x => x.LastError).HasMaxLength(1000);

        // Opaque secret-store key references, never the secrets themselves - see
        // CarrierIntegration's own remarks and Payment's GatewayConfiguration precedent.
        builder.Property(x => x.ApiKeyRef).HasMaxLength(200);
        builder.Property(x => x.SecretRef).HasMaxLength(200);
        builder.Property(x => x.WebhookSecretRef).HasMaxLength(200);

        builder.ConfigureCommonFields();

        // Indexes
        builder.HasIndex(x => x.ShippingProviderId).IsUnique();
        builder.HasIndex(x => x.IntegrationCode).IsUnique();
    }
}
