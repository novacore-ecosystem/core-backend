using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class PaymentMethodConfig : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        // Table
        builder.ToTable("payment_methods");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.MethodType).HasConversion<short>().IsRequired();
        builder.Property(x => x.IconUrl).HasMaxLength(500);
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        builder.ConfigureCommonFields();

        // Indexes
        builder.HasIndex(x => x.Code).IsUnique();

        // Reference catalog rows (Visa/MasterCard/PayPal/VNPay/MoMo) are seeded at startup by
        // PaymentSeeder, not via EF HasData - see Storage/Seeders/PaymentSeeder.cs, matching the
        // Product/Auth/User/Inventory services' own seeding convention.
    }
}
