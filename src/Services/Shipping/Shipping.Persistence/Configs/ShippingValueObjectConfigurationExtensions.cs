using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Shipping.Persistence.Configs;

/// <summary>
/// Shared OwnsOne mappings for the multi-field Value Objects this service reuses across many
/// entities (ShippingAddress, GeoCoordinate, PackageDimensions) - avoids repeating the same
/// column-set six times. Single-scalar VOs (Money, PhoneNumber, ShipmentNumber, ...) need no
/// helper: they map with a plain HasConversion at the call site.
/// </summary>
public static class ShippingValueObjectConfigurationExtensions
{
    public static void OwnsShippingAddress<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, ShippingAddress?>> propertyExpression,
        string columnPrefix,
        bool required)
        where TEntity : class
    {
        builder.OwnsOne(propertyExpression, address =>
        {
            address.Property(a => a.Country)
                .HasColumnName($"{columnPrefix}_country")
                .HasMaxLength(100)
                .IsRequired(required);
            address.Property(a => a.Province)
                .HasColumnName($"{columnPrefix}_province")
                .HasMaxLength(100);
            address.Property(a => a.District)
                .HasColumnName($"{columnPrefix}_district")
                .HasMaxLength(100);
            address.Property(a => a.Ward)
                .HasColumnName($"{columnPrefix}_ward")
                .HasMaxLength(100);
            address.Property(a => a.Street)
                .HasColumnName($"{columnPrefix}_street")
                .HasMaxLength(300)
                .IsRequired(required);
            address.Property(a => a.PostalCode)
                .HasColumnName($"{columnPrefix}_postal_code")
                .HasMaxLength(20);
        });

        if (required)
            builder.Navigation(propertyExpression).IsRequired();
    }

    public static void OwnsGeoCoordinate<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, GeoCoordinate?>> propertyExpression,
        string columnPrefix)
        where TEntity : class
    {
        builder.OwnsOne(propertyExpression, coordinate =>
        {
            coordinate.Property(c => c.Latitude)
                .HasColumnName($"{columnPrefix}_latitude")
                .HasColumnType("numeric(9,6)");
            coordinate.Property(c => c.Longitude)
                .HasColumnName($"{columnPrefix}_longitude")
                .HasColumnType("numeric(9,6)");
        });
    }

    public static void OwnsPackageDimensions<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, PackageDimensions?>> propertyExpression,
        string columnPrefix)
        where TEntity : class
    {
        builder.OwnsOne(propertyExpression, dimensions =>
        {
            dimensions.Property(d => d.LengthCm)
                .HasColumnName($"{columnPrefix}_length_cm")
                .HasColumnType("numeric(10,2)");
            dimensions.Property(d => d.WidthCm)
                .HasColumnName($"{columnPrefix}_width_cm")
                .HasColumnType("numeric(10,2)");
            dimensions.Property(d => d.HeightCm)
                .HasColumnName($"{columnPrefix}_height_cm")
                .HasColumnType("numeric(10,2)");

            // Derived from the three stored columns - never a column of its own.
            dimensions.Ignore(d => d.VolumeCm3);
        });
    }
}
