using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Order.Persistence.Configs;

public sealed class OrderTagDefinitionConfig : IEntityTypeConfiguration<OrderTagDefinition>
{
    public void Configure(EntityTypeBuilder<OrderTagDefinition> builder)
    {
        // Table
        builder.ToTable("order_tag_definitions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Color).HasMaxLength(20);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.IsSystem).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

        // Indexes
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.IsActive);
    }
}
