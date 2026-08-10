using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class WebhookEventConfig : IEntityTypeConfiguration<WebhookEvent>
{
    public void Configure(EntityTypeBuilder<WebhookEvent> builder)
    {
        // Table
        builder.ToTable("webhook_events");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.GatewayId).IsRequired();
        builder.Property(x => x.EventType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Payload).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.Signature).HasMaxLength(500);
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();
        builder.Property(x => x.RetryCount).IsRequired().HasDefaultValue(0);

        builder.ConfigureCommonFields();

        // Indexes
        builder.HasIndex(x => x.GatewayId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAt);
    }
}
