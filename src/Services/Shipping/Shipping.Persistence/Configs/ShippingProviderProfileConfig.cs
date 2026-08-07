using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Shipping.Persistence.Configs;

public sealed class ShippingProviderProfileConfig : IEntityTypeConfiguration<ShippingProviderProfile>
{
    public void Configure(EntityTypeBuilder<ShippingProviderProfile> builder)
    {
        // Table
        builder.ToTable("shipping_provider_profiles");

        // Properties
        // Shared-PK 1:1 with ShippingProvider - the child's key IS the provider id.
        builder.HasKey(x => x.ProviderId);

        builder.Property(x => x.ProviderId).ValueGeneratedNever();
        builder.Property(x => x.ContactName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ServiceAreas).HasMaxLength(2000);
        builder.Property(x => x.Note).HasMaxLength(1000);
        builder.Property(x => x.ContactPhone)
            .HasConversion(x => x.Value, x => PhoneNumber.Create(x))
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(x => x.ContactEmail)
            .HasConversion(x => x!.Value, x => Email.Create(x))
            .HasMaxLength(256);

        builder.OwnsShippingAddress(x => x.OfficeAddress, "office_address", required: false);

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne<ShippingProvider>()
            .WithOne(p => p.Profile)
            .HasForeignKey<ShippingProviderProfile>(x => x.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
