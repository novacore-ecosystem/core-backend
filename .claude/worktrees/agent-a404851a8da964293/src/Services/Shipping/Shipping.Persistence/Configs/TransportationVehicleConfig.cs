using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Shipping.Persistence.Configs;

public sealed class TransportationVehicleConfig : IEntityTypeConfiguration<TransportationVehicle>
{
    public void Configure(EntityTypeBuilder<TransportationVehicle> builder)
    {
        // Table
        builder.ToTable("transportation_vehicles");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProviderId).IsRequired();
        builder.Property(x => x.PlateNumber).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Model).HasMaxLength(100);
        builder.Property(x => x.CapacityKg).HasColumnType("numeric(10,3)").IsRequired();
        builder.Property(x => x.CapacityM3).HasColumnType("numeric(10,3)");
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();

        builder.ConfigureCommonFields();

        // Indexes
        builder.HasIndex(x => x.PlateNumber).IsUnique();
        builder.HasIndex(x => new { x.ProviderId, x.Status });
    }
}
