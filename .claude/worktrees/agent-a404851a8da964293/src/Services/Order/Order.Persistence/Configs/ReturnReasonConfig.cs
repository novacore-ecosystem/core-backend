using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Order.Persistence.Configs;

public sealed class ReturnReasonConfig : IEntityTypeConfiguration<ReturnReason>
{
    public void Configure(EntityTypeBuilder<ReturnReason> builder)
    {
        // Table
        builder.ToTable("return_reasons");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

        // Indexes
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.IsActive);
    }
}
