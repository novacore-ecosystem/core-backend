using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class PaymentAuditConfig : IEntityTypeConfiguration<PaymentAudit>
{
    public void Configure(EntityTypeBuilder<PaymentAudit> builder)
    {
        // Table
        builder.ToTable("payment_audits");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PaymentId).IsRequired();
        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Snapshot).HasColumnType("jsonb");
        builder.Property(x => x.OccurredAt).IsRequired();

        builder.ConfigureAuditFields();

        // Indexes
        builder.HasIndex(x => x.PaymentId);
        builder.HasIndex(x => x.OccurredAt);
    }
}
