using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class PaymentAttemptConfig : IEntityTypeConfiguration<PaymentAttempt>
{
    public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
    {
        // Table
        builder.ToTable("payment_attempts");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AttemptNumber).IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();
        builder.Property(x => x.GatewayTransactionId).HasMaxLength(200);
        builder.Property(x => x.GatewayOrderId).HasMaxLength(200);
        builder.Property(x => x.RequestPayload).HasColumnType("jsonb");
        builder.Property(x => x.ResponsePayload).HasColumnType("jsonb");
        builder.Property(x => x.ResponseCode).HasMaxLength(50);
        builder.Property(x => x.FailureCode).HasMaxLength(50);
        builder.Property(x => x.FailureReason).HasMaxLength(500);
        builder.Property(x => x.RedirectUrl).HasMaxLength(2000);

        builder.ConfigureAuditFields();

        // Indexes
        builder.HasIndex(x => x.PaymentId);
        builder.HasIndex(x => x.GatewayTransactionId);
        builder.HasIndex(x => new { x.PaymentId, x.AttemptNumber }).IsUnique();
    }
}
