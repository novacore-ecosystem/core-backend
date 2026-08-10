using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class PaymentFeeConfig : IEntityTypeConfiguration<PaymentFee>
{
    public void Configure(EntityTypeBuilder<PaymentFee> builder)
    {
        // Table
        builder.ToTable("payment_fees");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FeeType).HasConversion<short>().IsRequired();
        builder.Property(x => x.Description).HasMaxLength(300);

        builder.OwnsMoney(x => x.Amount, "amount");

        builder.ConfigureAuditFields();

        // Indexes
        builder.HasIndex(x => x.PaymentId);
        builder.HasIndex(x => x.SettlementId);
    }
}
