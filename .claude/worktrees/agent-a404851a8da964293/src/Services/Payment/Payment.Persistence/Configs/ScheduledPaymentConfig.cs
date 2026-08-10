using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class ScheduledPaymentConfig : IEntityTypeConfiguration<ScheduledPayment>
{
    public void Configure(EntityTypeBuilder<ScheduledPayment> builder)
    {
        // Table
        builder.ToTable("scheduled_payments");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReferenceType).HasConversion<short>().IsRequired();
        builder.Property(x => x.ReferenceId).IsRequired();
        builder.Property(x => x.Frequency).HasConversion<short>().IsRequired();
        builder.Property(x => x.NextRunAt).IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();

        builder.OwnsMoney(x => x.Amount, "amount");

        builder.ConfigureCommonFields();

        // Indexes
        builder.HasIndex(x => new { x.ReferenceType, x.ReferenceId });
        builder.HasIndex(x => new { x.Status, x.NextRunAt });
    }
}
