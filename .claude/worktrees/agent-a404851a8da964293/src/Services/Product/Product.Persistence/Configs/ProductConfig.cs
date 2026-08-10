using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NovaCore.Product.Domain.Metadata;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductConfig : IEntityTypeConfiguration<ProductEntity>
{
    public void Configure(EntityTypeBuilder<ProductEntity> builder)
    {
        // Table
        builder.ToTable("products");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasConversion(x => x.Value, x => Slug.Create(x))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ShortDescription)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(x => x.ProductType)
            .IsRequired()
            .HasConversion<short>();

        builder.Property(x => x.ProductStatus)
            .IsRequired()
            .HasConversion<byte>()
            .HasDefaultValue(ProductStatus.Draft);

        builder.Property(x => x.IsFeatured)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsVisible)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x.ToJson(),
                x => MetadataBase.FromJson<ProductMetadata>(x))
            .HasColumnType("jsonb")
            .IsRequired();

        // SeoId mirrors ProductSeo.Id (shared primary key, see the Seo relationship below) -
        // mapped as a plain column since the actual FK constraint lives on ProductSeo's side.
        builder.Property(x => x.SeoId);

        // Relationships
        builder.HasOne(x => x.Brand)
            .WithMany()
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<ProductCategory>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Seo)
            .WithOne(s => s.Product)
            .HasForeignKey<ProductSeo>(s => s.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Options)
            .WithOne(o => o.Product)
            .HasForeignKey(o => o.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Specifications)
            .WithOne(s => s.Product)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Media)
            .WithOne(m => m.Product)
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Translations)
            .WithOne(t => t.Product)
            .HasForeignKey(t => t.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.CategoryMappings)
            .WithOne(m => m.Product)
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.TagMappings)
            .WithOne(m => m.Product)
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.CollectionMappings)
            .WithOne(m => m.Product)
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.Slug)
            .IsUnique();
        builder.HasIndex(x => x.ProductStatus);
        builder.HasIndex(x => x.BrandId);
        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.DisplayOrder);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
