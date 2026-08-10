using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class PaymentConfig : IEntityTypeConfiguration<PaymentEntity>
{
    public void Configure(EntityTypeBuilder<PaymentEntity> builder)
    {
        // Table
        builder.ToTable("payments");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReferenceType).HasConversion<short>().IsRequired();
        builder.Property(x => x.ReferenceId).IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();
        builder.Property(x => x.GatewayId).IsRequired();
        builder.Property(x => x.IdempotencyKey).HasMaxLength(200);
        builder.Property(x => x.FailureCode).HasMaxLength(50);
        builder.Property(x => x.FailureReason).HasMaxLength(500);
        builder.Property(x => x.Metadata).HasColumnType("jsonb");

        builder.OwnsMoney(x => x.Amount, "amount");

        builder.ConfigureCommonFields();

        // Relationships
        // PaymentItem/PaymentAttempt have no independent identity outside their Payment, but are
        // normal related entities (own table, own PK, FK back to Payment) - not owned types, same
        // reasoning as Order.Items after Order's Owned->Relational refactor. Every read path that
        // needs them Includes explicitly.
        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(i => i.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Attempts)
            .WithOne()
            .HasForeignKey(a => a.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.ReferenceType, x.ReferenceId });
        builder.HasIndex(x => x.PaymentIntentId);
        builder.HasIndex(x => x.GatewayId);
        builder.HasIndex(x => x.IdempotencyKey).IsUnique();
        builder.HasIndex(x => new { x.Status, x.CreatedAt });
    }
}
