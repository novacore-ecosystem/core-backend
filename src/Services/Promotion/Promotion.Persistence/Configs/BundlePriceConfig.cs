using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class BundlePriceConfig : IEntityTypeConfiguration<BundlePrice>
{
    public void Configure(EntityTypeBuilder<BundlePrice> builder)
    {
        // Table
        builder.ToTable("bundle_prices");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Currency)
            .HasConversion(x => x.Value, x => Currency.Create(x))
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnName("price")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Bundle)
            .WithMany(x => x.Prices)
            .HasForeignKey(x => x.BundleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.BundleId, x.Currency }).IsUnique();
    }
}
