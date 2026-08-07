using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class BundleGiftConfig : IEntityTypeConfiguration<BundleGift>
{
    public void Configure(EntityTypeBuilder<BundleGift> builder)
    {
        // Table
        builder.ToTable("bundle_gifts");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity)
            .HasConversion(x => x.Value, x => Quantity.Create(x))
            .HasColumnName("quantity")
            .IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Bundle)
            .WithMany(x => x.Gifts)
            .HasForeignKey(x => x.BundleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.BundleId);
    }
}
