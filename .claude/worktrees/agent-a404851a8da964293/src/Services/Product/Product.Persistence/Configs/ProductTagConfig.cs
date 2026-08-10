using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductTagConfig : IEntityTypeConfiguration<ProductTag>
{
    public void Configure(EntityTypeBuilder<ProductTag> builder)
    {
        // Table
        builder.ToTable("product_tags");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Slug)
            .HasConversion(x => x.Value, x => Slug.Create(x))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Color)
            .HasMaxLength(20);

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.Note)
            .HasMaxLength(1000)
            .IsRequired(false)
            .HasDefaultValue(string.Empty);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(CatalogStatus.Active);

        // Relationships
        builder.HasMany(x => x.Translations)
            .WithOne(t => t.ProductTag)
            .HasForeignKey(t => t.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.Slug)
            .IsUnique();
        builder.HasIndex(x => x.Status);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
