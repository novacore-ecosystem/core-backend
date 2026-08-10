using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class PaymentSessionConfig : IEntityTypeConfiguration<PaymentSession>
{
    public void Configure(EntityTypeBuilder<PaymentSession> builder)
    {
        // Table
        builder.ToTable("payment_sessions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PaymentIntentId).IsRequired();
        builder.Property(x => x.RedirectUrl).HasMaxLength(2000);
        builder.Property(x => x.ReturnUrl).HasMaxLength(2000);
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne<PaymentIntent>()
            .WithMany()
            .HasForeignKey(x => x.PaymentIntentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.PaymentIntentId);
    }
}
