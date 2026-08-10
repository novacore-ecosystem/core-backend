using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class GatewayConfigurationConfig : IEntityTypeConfiguration<GatewayConfiguration>
{
    public void Configure(EntityTypeBuilder<GatewayConfiguration> builder)
    {
        // Table
        builder.ToTable("gateway_configurations");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Environment).HasConversion<short>().IsRequired();
        builder.Property(x => x.ApiKeyRef).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SecretRef).HasMaxLength(200).IsRequired();
        builder.Property(x => x.WebhookSecretRef).HasMaxLength(200);
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        builder.ConfigureAuditFields();

        // Indexes
        builder.HasIndex(x => new { x.GatewayId, x.Environment }).IsUnique();
    }
}
