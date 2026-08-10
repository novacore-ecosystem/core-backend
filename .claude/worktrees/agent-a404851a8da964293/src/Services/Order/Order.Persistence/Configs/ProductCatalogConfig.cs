using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Order.Persistence.Configs;

public sealed class ProductCatalogConfig : IEntityTypeConfiguration<ProductCatalog>
{
    public void Configure(EntityTypeBuilder<ProductCatalog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.VariationId)
            .IsRequired();

        builder.Property(x => x.ProductName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.VariationName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Sku)
            .HasConversion(x => x.Value, x => Sku.Create(x))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnType("numeric(18,2)");

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(x => new { x.ProductId, x.VariationId }).IsUnique();
    }
}
