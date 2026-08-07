using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class GiftUsageConfig : IEntityTypeConfiguration<GiftUsage>
{
    public void Configure(EntityTypeBuilder<GiftUsage> builder)
    {
        // Table
        builder.ToTable("gift_usages");

        // Properties
        builder.HasKey(x => x.Id);

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.GiftItem)
            .WithMany(x => x.Usages)
            .HasForeignKey(x => x.GiftItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.GiftItemId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.OrderId);
    }
}
