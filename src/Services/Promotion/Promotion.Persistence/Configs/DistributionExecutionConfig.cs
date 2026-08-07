using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class DistributionExecutionConfig : IEntityTypeConfiguration<DistributionExecution>
{
    public void Configure(EntityTypeBuilder<DistributionExecution> builder)
    {
        // Table
        builder.ToTable("distribution_executions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ExecutionKey).HasMaxLength(200).IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Item)
            .WithMany(x => x.Executions)
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Retries is configured from DistributionRetryConfig's side (single source per relationship).

        // Indexes
        builder.HasIndex(x => x.ExecutionKey).IsUnique();
        builder.HasIndex(x => x.ItemId);
    }
}
