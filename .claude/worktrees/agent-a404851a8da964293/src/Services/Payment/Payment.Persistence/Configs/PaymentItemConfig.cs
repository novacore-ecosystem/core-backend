using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class PaymentItemConfig : IEntityTypeConfiguration<PaymentItem>
{
    public void Configure(EntityTypeBuilder<PaymentItem> builder)
    {
        // Table
        builder.ToTable("payment_items");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ItemType).HasConversion<short>().IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Quantity).IsRequired();

        builder.OwnsMoney(x => x.Amount, "amount");

        builder.ConfigureAuditFields();

        // Indexes
        builder.HasIndex(x => x.PaymentId);
    }
}
