using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductCategoryConfig : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        // Table
        builder.ToTable("product_categories");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasConversion(x => x.Value, x => CategoryCode.Create(x))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Note)
            .HasMaxLength(1000)
            .IsRequired(false)
            .HasDefaultValue(string.Empty);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<byte>()
            .HasDefaultValue(ProductCategoryStatus.Active);

        // Relationships
        // Self-referencing hierarchy - no Domain-level Parent/Children navigation (see
        // ProductCategory.ChangeParent remarks), so this is a shadow-navigation FK only.
        builder.HasOne<ProductCategory>()
            .WithMany()
            .HasForeignKey(x => x.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Translation)
            .WithOne(t => t.Category)
            .HasForeignKey(t => t.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.Code)
            .IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ParentCategoryId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
