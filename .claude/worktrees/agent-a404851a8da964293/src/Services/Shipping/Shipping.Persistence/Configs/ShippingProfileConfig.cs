using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Shipping.Persistence.Configs;

public sealed class ShippingProfileConfig : IEntityTypeConfiguration<ShippingProfile>
{
    public void Configure(EntityTypeBuilder<ShippingProfile> builder)
    {
        // Table
        builder.ToTable("shipping_profiles");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Label).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ContactName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.IsDefault).IsRequired();
        builder.Property(x => x.VerificationStatus).HasConversion<short>().IsRequired();
        builder.Property(x => x.ContactPhone)
            .HasConversion(x => x.Value, x => PhoneNumber.Create(x))
            .HasMaxLength(30)
            .IsRequired();

        builder.OwnsShippingAddress(x => x.Address, "address", required: true);

        builder.ConfigureCommonFields();

        // Indexes
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.IsDefault });
        builder.HasIndex(x => x.VerifiedAddressId);
    }
}
