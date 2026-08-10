using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class PayoutConfig : IEntityTypeConfiguration<Payout>
{
    public void Configure(EntityTypeBuilder<Payout> builder)
    {
        // Table
        builder.ToTable("payouts");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PayeeReferenceId).IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();

        builder.OwnsMoney(x => x.Amount, "amount");

        builder.ConfigureCommonFields();

        // Indexes
        builder.HasIndex(x => x.PayeeReferenceId);
        builder.HasIndex(x => x.Status);
    }
}
