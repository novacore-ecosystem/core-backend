using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class PaymentIntentConfig : IEntityTypeConfiguration<PaymentIntent>
{
    public void Configure(EntityTypeBuilder<PaymentIntent> builder)
    {
        // Table
        builder.ToTable("payment_intents");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReferenceType).HasConversion<short>().IsRequired();
        builder.Property(x => x.ReferenceId).IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();
        builder.Property(x => x.ClientSecret).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Metadata).HasColumnType("jsonb");

        builder.OwnsMoney(x => x.RequestedAmount, "requested_amount");

        // Relationships
        // Payments is a read-only navigation, populated only when explicitly Included - Payment
        // owns its own lifecycle and is created independently of PaymentIntent's write path.
        builder.HasMany(x => x.Payments)
            .WithOne()
            .HasForeignKey(p => p.PaymentIntentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ConfigureCommonFields();

        // Indexes
        builder.HasIndex(x => new { x.ReferenceType, x.ReferenceId });
        builder.HasIndex(x => x.ClientSecret).IsUnique();
        builder.HasIndex(x => x.CreatedAt);
    }
}
