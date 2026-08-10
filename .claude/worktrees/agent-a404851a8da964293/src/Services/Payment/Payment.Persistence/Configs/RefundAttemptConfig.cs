using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class RefundAttemptConfig : IEntityTypeConfiguration<RefundAttempt>
{
    public void Configure(EntityTypeBuilder<RefundAttempt> builder)
    {
        // Table
        builder.ToTable("refund_attempts");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AttemptNumber).IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();
        builder.Property(x => x.GatewayRefundId).HasMaxLength(200);
        builder.Property(x => x.ResponsePayload).HasColumnType("jsonb");
        builder.Property(x => x.FailureCode).HasMaxLength(50);
        builder.Property(x => x.FailureReason).HasMaxLength(500);

        builder.ConfigureAuditFields();

        // Indexes
        builder.HasIndex(x => x.RefundId);
        builder.HasIndex(x => new { x.RefundId, x.AttemptNumber }).IsUnique();
    }
}
