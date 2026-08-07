using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Shipping.Persistence.Configs;

public sealed class TransportationPersonConfig : IEntityTypeConfiguration<TransportationPerson>
{
    public void Configure(EntityTypeBuilder<TransportationPerson> builder)
    {
        // Table
        builder.ToTable("transportation_people");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProviderId).IsRequired();
        builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.LicenseNumber).HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();
        builder.Property(x => x.JoinedAt).IsRequired();
        builder.Property(x => x.PhoneNumber)
            .HasConversion(x => x.Value, x => PhoneNumber.Create(x))
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(x => x.Email)
            .HasConversion(x => x!.Value, x => Email.Create(x))
            .HasMaxLength(256);

        builder.ConfigureCommonFields();

        // Indexes
        builder.HasIndex(x => x.ProviderId);
        builder.HasIndex(x => new { x.ProviderId, x.Status });
        builder.HasIndex(x => x.UserId);
    }
}
