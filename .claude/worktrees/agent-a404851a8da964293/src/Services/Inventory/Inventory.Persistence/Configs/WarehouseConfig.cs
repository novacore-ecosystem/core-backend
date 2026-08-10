using NovaCore.Inventory.Domain.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Inventory.Persistence.Configs;

public sealed class WarehouseConfig : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("warehouses");
        builder.HasKey(x => x.Id);

        // Fields
        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(1000)
            .IsRequired(false)
            .HasDefaultValue(string.Empty);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<short>();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(WarehouseStatus.Active);

        builder.Property(x => x.Priority)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.TimeZone)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("UTC");

        // TODO: Configure owned types properly with JSON serialization
        // For now, ignore them to allow initial migration
        builder.Ignore(x => x.Address);
        builder.Ignore(x => x.Location);
        builder.Ignore(x => x.Contact);
        builder.Ignore(x => x.Capacity);

        builder.Property(x => x.SupportsReceiving)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.SupportsShipping)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.SupportsReservation)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.SupportsTransfer)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.SupportsPicking)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.SupportsReturns)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.AllowNegativeStock)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x.ToJson(),
                x => WarehouseMetadata.FromJson<WarehouseMetadata>(x))
            .HasColumnType("jsonb")
            .IsRequired();

        // Relationships
        builder.HasMany(x => x.Zones)
            .WithOne(z => z.Warehouse)
            .HasForeignKey(z => z.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.Code)
            .IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Type);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
