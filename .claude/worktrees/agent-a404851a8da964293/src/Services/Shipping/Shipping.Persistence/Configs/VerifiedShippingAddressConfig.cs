using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Shipping.Persistence.Configs;

public sealed class VerifiedShippingAddressConfig : IEntityTypeConfiguration<VerifiedShippingAddress>
{
    public void Configure(EntityTypeBuilder<VerifiedShippingAddress> builder)
    {
        // Table
        builder.ToTable("verified_shipping_addresses");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();
        builder.Property(x => x.RejectionReason).HasMaxLength(500);
        builder.Property(x => x.SuccessfulDeliveryCount).IsRequired();

        builder.OwnsShippingAddress(x => x.Address, "address", required: true);
        builder.OwnsGeoCoordinate(x => x.Coordinate, "coordinate");

        builder.ConfigureCommonFields();

        // Indexes
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.Status });
    }
}
