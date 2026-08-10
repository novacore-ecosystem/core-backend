using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductCollectionConfig : IEntityTypeConfiguration<ProductCollection>
{
    public void Configure(EntityTypeBuilder<ProductCollection> builder)
    {
        // Table
        builder.ToTable("product_collections");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.Slug)
            .HasConversion(x => x.Value, x => Slug.Create(x))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.CollectionType)
            .IsRequired()
            .HasConversion<short>();

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(CatalogStatus.Active);

        // Relationships
        builder.HasMany(x => x.Translations)
            .WithOne(t => t.Collection)
            .HasForeignKey(t => t.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.Slug)
            .IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CollectionType);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
