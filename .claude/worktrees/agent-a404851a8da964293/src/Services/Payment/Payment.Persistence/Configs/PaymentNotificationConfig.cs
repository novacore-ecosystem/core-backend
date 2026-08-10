using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class PaymentNotificationConfig : IEntityTypeConfiguration<PaymentNotification>
{
    public void Configure(EntityTypeBuilder<PaymentNotification> builder)
    {
        // Table
        builder.ToTable("payment_notifications");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Channel).HasConversion<short>().IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();

        builder.ConfigureCommonFields();

        // Indexes
        builder.HasIndex(x => x.PaymentId);
        builder.HasIndex(x => x.Status);
    }
}
