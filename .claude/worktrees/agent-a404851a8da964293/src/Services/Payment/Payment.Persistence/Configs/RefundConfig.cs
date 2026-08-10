using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class RefundConfig : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        // Table
        builder.ToTable("refunds");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PaymentId).IsRequired();
        builder.Property(x => x.ReferenceType).HasConversion<short>().IsRequired();
        builder.Property(x => x.ReferenceId).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();

        builder.OwnsMoney(x => x.Amount, "amount");

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne<PaymentEntity>()
            .WithMany()
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Attempts)
            .WithOne()
            .HasForeignKey(a => a.RefundId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.PaymentId);
        builder.HasIndex(x => new { x.ReferenceType, x.ReferenceId });
        builder.HasIndex(x => new { x.Status, x.CreatedAt });
    }
}
