using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Order.Persistence.Configs;

public sealed class OrderTaxConfig : IEntityTypeConfiguration<OrderTax>
{
    public void Configure(EntityTypeBuilder<OrderTax> builder)
    {
        // Table
        builder.ToTable("order_taxes");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId).IsRequired();
        builder.Property(x => x.TaxType).HasConversion<int>().IsRequired();
        builder.Property(x => x.TaxRate).HasColumnType("numeric(9,4)");
        builder.Property(x => x.TaxName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CountryCode).HasMaxLength(2).IsRequired();

        builder.Property(x => x.TaxAmount)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnType("numeric(18,2)");

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

        // Relationships
        // Shadow reference to Order, same reasoning as OrderItem/OrderDiscount - no
        // OrderTax->Order navigation exists on the domain side. See OrderConfig for the
        // Order.Taxes side of this relationship.
        builder.HasOne<OrderEntity>()
            .WithMany(o => o.Taxes)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.OrderId);
    }
}
