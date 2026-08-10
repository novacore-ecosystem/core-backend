using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Shipping.Persistence.Configs;

public sealed class PackageConfig : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        // Table
        builder.ToTable("packages");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ShipmentId).IsRequired();
        builder.Property(x => x.PackageCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PackageType).HasConversion<short>().IsRequired();
        builder.Property(x => x.WeightKg).HasColumnType("numeric(10,3)").IsRequired();

        builder.OwnsPackageDimensions(x => x.Dimensions, "dimensions");

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(i => i.PackageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ShipmentId);
        builder.HasIndex(x => new { x.ShipmentId, x.PackageCode }).IsUnique();
    }
}
