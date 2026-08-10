using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class BillingProfileConfig : IEntityTypeConfiguration<BillingProfile>
{
    public void Configure(EntityTypeBuilder<BillingProfile> builder)
    {
        // Table
        builder.ToTable("billing_profiles");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OwnerReferenceId).IsRequired();
        builder.Property(x => x.LegalName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TaxId).HasMaxLength(50);
        builder.Property(x => x.Email).HasMaxLength(320);
        builder.Property(x => x.Phone).HasMaxLength(20);

        builder.OwnsOne(x => x.Address, address =>
        {
            address.Property(a => a.Country).HasColumnName("address_country").HasMaxLength(100).IsRequired();
            address.Property(a => a.State).HasColumnName("address_state").HasMaxLength(100);
            address.Property(a => a.City).HasColumnName("address_city").HasMaxLength(100);
            address.Property(a => a.Street).HasColumnName("address_street").HasMaxLength(300).IsRequired();
            address.Property(a => a.PostalCode).HasColumnName("address_postal_code").HasMaxLength(20);
        });

        builder.ConfigureCommonFields();

        // Indexes
        builder.HasIndex(x => x.OwnerReferenceId);
    }
}
