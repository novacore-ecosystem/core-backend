using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class DistributionRetryConfig : IEntityTypeConfiguration<DistributionRetry>
{
    public void Configure(EntityTypeBuilder<DistributionRetry> builder)
    {
        // Table
        builder.ToTable("distribution_retries");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RetryCount).IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Execution)
            .WithMany(x => x.Retries)
            .HasForeignKey(x => x.ExecutionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ExecutionId);
    }
}
