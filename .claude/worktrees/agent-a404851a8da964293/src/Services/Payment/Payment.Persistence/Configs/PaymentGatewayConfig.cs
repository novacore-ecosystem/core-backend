using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class PaymentGatewayConfig : IEntityTypeConfiguration<PaymentGateway>
{
    public void Configure(EntityTypeBuilder<PaymentGateway> builder)
    {
        // Table
        builder.ToTable("payment_gateways");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.GatewayType).HasConversion<short>().IsRequired();
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasMany(x => x.Configurations)
            .WithOne()
            .HasForeignKey(c => c.GatewayId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.StatusMappings)
            .WithOne()
            .HasForeignKey(m => m.GatewayId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.Code).IsUnique();

        // Reference catalog rows (Stripe/PayPal/VNPay/MoMo) are seeded at startup by
        // PaymentSeeder - see Storage/Seeders/PaymentSeeder.cs.
    }
}
