using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class DistributionItemConfig : IEntityTypeConfiguration<DistributionItem>
{
    public void Configure(EntityTypeBuilder<DistributionItem> builder)
    {
        // Table
        builder.ToTable("distribution_items");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RewardType).HasConversion<short>().IsRequired();
        builder.Property(x => x.Status).HasMaxLength(50);

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Batch)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.BatchId)
            .OnDelete(DeleteBehavior.Cascade);

        // Executions is configured from DistributionExecutionConfig's side (single source per relationship).

        // Indexes
        builder.HasIndex(x => x.BatchId);
        builder.HasIndex(x => x.UserId);
    }
}
