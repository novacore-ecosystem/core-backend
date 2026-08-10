using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class WarrantyPolicyConfig : IEntityTypeConfiguration<WarrantyPolicy>
{
    public void Configure(EntityTypeBuilder<WarrantyPolicy> builder)
    {
        // Table
        builder.ToTable("warranty_policies");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.WarrantyType)
            .IsRequired()
            .HasConversion<short>();

        builder.OwnsOne(x => x.Duration, duration =>
        {
            duration.Property(d => d.Value)
                .HasColumnName("duration_value")
                .IsRequired();

            duration.Property(d => d.Unit)
                .HasColumnName("duration_unit")
                .HasConversion<short>()
                .IsRequired();
        });

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(CatalogStatus.Active);

        // Indexes
        builder.HasIndex(x => x.Status);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
