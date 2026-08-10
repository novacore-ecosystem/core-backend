using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class PaymentEventLogConfig : IEntityTypeConfiguration<PaymentEventLog>
{
    public void Configure(EntityTypeBuilder<PaymentEventLog> builder)
    {
        // Table
        builder.ToTable("payment_event_logs");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReferenceType).HasConversion<short>().IsRequired();
        builder.Property(x => x.ReferenceId).IsRequired();
        builder.Property(x => x.EventType).HasConversion<short>().IsRequired();
        builder.Property(x => x.Details).HasColumnType("jsonb");
        builder.Property(x => x.OccurredAt).IsRequired();

        // Append-only - CreatedAt only, no UpdatedAt tracking needed for a write-once row.
        builder.ConfigureAuditFields();

        // Indexes
        builder.HasIndex(x => new { x.ReferenceType, x.ReferenceId });
        builder.HasIndex(x => x.PaymentId);
        builder.HasIndex(x => x.OccurredAt);
    }
}
