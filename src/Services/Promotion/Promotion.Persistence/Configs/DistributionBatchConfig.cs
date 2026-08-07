using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class DistributionBatchConfig : IEntityTypeConfiguration<DistributionBatch>
{
    public void Configure(EntityTypeBuilder<DistributionBatch> builder)
    {
        // Table
        builder.ToTable("distribution_batches");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BatchNo).IsRequired();
        builder.Property(x => x.TotalItems).IsRequired();
        builder.Property(x => x.ProcessedItems).IsRequired();
        builder.Property(x => x.FailedItems).IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Job)
            .WithMany(x => x.Batches)
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        // Items is configured from DistributionItemConfig's side (single source per relationship).

        // Indexes
        builder.HasIndex(x => new { x.JobId, x.BatchNo }).IsUnique();
    }
}
