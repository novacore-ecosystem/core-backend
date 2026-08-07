using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Shipping.Persistence.Configs;

public sealed class DeliveryConfig : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<Delivery> builder)
    {
        // Table
        builder.ToTable("deliveries");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TransportationId).IsRequired();
        builder.Property(x => x.ReceiverName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();
        builder.Property(x => x.AttemptCount).IsRequired();
        builder.Property(x => x.FailureReason).HasMaxLength(500);
        builder.Property(x => x.CodCollected).IsRequired();
        builder.Property(x => x.ReceiverPhone)
            .HasConversion(x => x.Value, x => PhoneNumber.Create(x))
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(x => x.CodAmount)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.OwnsShippingAddress(x => x.Address, "address", required: true);

        builder.ConfigureCommonFields();

        // Indexes
        builder.HasIndex(x => x.TransportationId).IsUnique();
        builder.HasIndex(x => new { x.Status, x.CreatedAt });
    }
}
