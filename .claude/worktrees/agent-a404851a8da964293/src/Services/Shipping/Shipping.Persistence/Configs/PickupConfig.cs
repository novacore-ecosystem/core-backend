using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Shipping.Persistence.Configs;

public sealed class PickupConfig : IEntityTypeConfiguration<Pickup>
{
    public void Configure(EntityTypeBuilder<Pickup> builder)
    {
        // Table
        builder.ToTable("pickups");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ShipmentId).IsRequired();
        builder.Property(x => x.PickupType).HasConversion<short>().IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();
        builder.Property(x => x.ContactName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ScheduledAt).IsRequired();
        builder.Property(x => x.FailureReason).HasMaxLength(500);
        builder.Property(x => x.Note).HasMaxLength(1000);
        builder.Property(x => x.ContactPhone)
            .HasConversion(x => x.Value, x => PhoneNumber.Create(x))
            .HasMaxLength(30)
            .IsRequired();

        builder.OwnsShippingAddress(x => x.Address, "address", required: true);

        builder.ConfigureCommonFields();

        // Indexes
        builder.HasIndex(x => x.ShipmentId);
        builder.HasIndex(x => new { x.Status, x.ScheduledAt });
    }
}
