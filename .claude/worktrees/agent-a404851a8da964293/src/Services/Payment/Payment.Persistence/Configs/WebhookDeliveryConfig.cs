using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class WebhookDeliveryConfig : IEntityTypeConfiguration<WebhookDelivery>
{
    public void Configure(EntityTypeBuilder<WebhookDelivery> builder)
    {
        // Table
        builder.ToTable("webhook_deliveries");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TargetUrl).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Payload).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();
        builder.Property(x => x.AttemptCount).IsRequired().HasDefaultValue(0);

        builder.ConfigureCommonFields();

        // Indexes
        builder.HasIndex(x => x.Status);
    }
}
