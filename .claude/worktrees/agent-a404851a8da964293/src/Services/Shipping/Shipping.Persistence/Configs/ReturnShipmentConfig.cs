using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Shipping.Persistence.Configs;

public sealed class ReturnShipmentConfig : IEntityTypeConfiguration<ReturnShipment>
{
    public void Configure(EntityTypeBuilder<ReturnShipment> builder)
    {
        // Table
        builder.ToTable("return_shipments");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OriginalShipmentId).IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.RequestedAt).IsRequired();
        builder.Property(x => x.RejectionReason).HasMaxLength(500);

        builder.ConfigureCommonFields();

        // Indexes
        builder.HasIndex(x => x.OriginalShipmentId);
        builder.HasIndex(x => x.ReturnedShipmentId);
        builder.HasIndex(x => new { x.Status, x.RequestedAt });
    }
}
