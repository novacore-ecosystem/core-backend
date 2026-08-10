using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class GatewayStatusMappingConfig : IEntityTypeConfiguration<GatewayStatusMapping>
{
    public void Configure(EntityTypeBuilder<GatewayStatusMapping> builder)
    {
        // Table
        builder.ToTable("gateway_status_mappings");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.GatewayStatusCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.MappedStatus).HasConversion<short>().IsRequired();
        builder.Property(x => x.Description).HasMaxLength(300);

        builder.ConfigureAuditFields();

        // Indexes
        builder.HasIndex(x => new { x.GatewayId, x.GatewayStatusCode }).IsUnique();
    }
}
