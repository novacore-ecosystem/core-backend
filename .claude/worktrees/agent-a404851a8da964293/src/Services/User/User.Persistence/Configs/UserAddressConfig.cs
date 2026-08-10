using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.User.Persistence.Configs;

public sealed class UserAddressConfig : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        // Table
        builder.ToTable("user_addresses");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Label)
            .HasMaxLength(100)
            .IsRequired();

        builder.OwnsOne(x => x.Receiver, receiver =>
        {
            receiver.Property(r => r.FullName)
                .HasColumnName("receiver_full_name")
                .HasMaxLength(200)
                .IsRequired();

            receiver.Property(r => r.PhoneNumber)
                .HasColumnName("receiver_phone_number")
                .HasMaxLength(20)
                .IsRequired();

            receiver.Property(r => r.Company)
                .HasColumnName("receiver_company")
                .HasMaxLength(200);
        });

        builder.Navigation(x => x.Receiver).IsRequired();

        builder.OwnsOne(x => x.Address, address =>
        {
            address.Property(a => a.Country)
                .HasColumnName("country")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(a => a.Province)
                .HasColumnName("province")
                .HasMaxLength(100);

            address.Property(a => a.District)
                .HasColumnName("district")
                .HasMaxLength(100);

            address.Property(a => a.Ward)
                .HasColumnName("ward")
                .HasMaxLength(100);

            address.Property(a => a.Street)
                .HasColumnName("street")
                .HasMaxLength(300)
                .IsRequired();

            address.Property(a => a.PostalCode)
                .HasColumnName("postal_code")
                .HasMaxLength(20);
        });

        builder.Navigation(x => x.Address).IsRequired();

        builder.OwnsOne(x => x.GeoLocation, geo =>
        {
            geo.Property(g => g.Latitude)
                .HasColumnName("latitude")
                .HasColumnType("numeric(9,6)");

            geo.Property(g => g.Longitude)
                .HasColumnName("longitude")
                .HasColumnType("numeric(9,6)");
        });

        builder.Property(x => x.Building)
            .HasMaxLength(100);

        builder.Property(x => x.Apartment)
            .HasMaxLength(100);

        builder.Property(x => x.Floor)
            .HasMaxLength(50);

        builder.Property(x => x.DeliveryInstruction)
            .HasMaxLength(500);

        builder.Property(x => x.AddressType)
            .IsRequired()
            .HasConversion<short>();

        builder.Property(x => x.IsDefaultShipping)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsDefaultBilling)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsVerified)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(x => x.User)
            .WithMany(u => u.Addresses)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.IsDefaultShipping });
        builder.HasIndex(x => new { x.UserId, x.IsDefaultBilling });

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
