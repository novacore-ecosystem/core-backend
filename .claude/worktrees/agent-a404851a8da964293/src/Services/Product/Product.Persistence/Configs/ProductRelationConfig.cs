using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductRelationConfig : IEntityTypeConfiguration<ProductRelation>
{
    public void Configure(EntityTypeBuilder<ProductRelation> builder)
    {
        // Table
        builder.ToTable("product_relations");

        // Properties
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.SourceProductId)
            .IsRequired();

        builder.Property(x => x.TargetProductId)
            .IsRequired();

        builder.Property(x => x.RelationType)
            .IsRequired()
            .HasConversion<short>();

        builder.Property(x => x.Priority)
            .IsRequired()
            .HasDefaultValue(0);

        // Relationships
        builder.HasOne(x => x.SourceProduct)
            .WithMany()
            .HasForeignKey(x => x.SourceProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.TargetProduct)
            .WithMany()
            .HasForeignKey(x => x.TargetProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => new { x.SourceProductId, x.TargetProductId, x.RelationType })
            .IsUnique();
        builder.HasIndex(x => x.TargetProductId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
